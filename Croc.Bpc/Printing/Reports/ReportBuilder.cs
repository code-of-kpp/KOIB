using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Collections.Specialized; 

using Croc.Bpc.Printing.Reports.Templates; 

using Croc.Bpc.Election.Voting; 

using Croc.Core.Utils; 

using Croc.Core.Utils.Text; 

using Croc.Bpc.Election; 

using Croc.Bpc.Common.Diagnostics; 

 

 

namespace Croc.Bpc.Printing.Reports 

{ 

    /// <summary> 

    /// Генератор отчетов 

    /// </summary> 

    public class ReportBuilder 

    { 

        #region Константы 

 

 

        #region Параметры формирования отчетов 

        /// <summary> 

        /// Наименование отчета (PrintAction) 

        /// </summary> 

        internal const string PRN_REPORT_NAME = "reportName"; 

        /// <summary> 

        /// Задает, какой протокол печатать:  

        /// true - итоговый 

        /// false - предварительные результаты 

        /// </summary> 

        internal const string PRN_FINAL_PROTOCOL = "final"; 

        /// <summary> 

        /// Задает, какой из тестовых протоколов печатать:  

        /// true - данные тестового режима (протокол тестирования) 

        /// false - технологический протокол 

        /// </summary> 

        internal const string PRN_TEST_PROTOCOL = "test"; 

        /// <summary> 

        /// Признак печати результатов в итоговом или предварительном протоколе 

        /// true - выводить результаты голосования 

        /// false - выводить нулю (режим печати шаблона протокола) 

        /// </summary> 

        internal const string PRN_PRINT_RESULTS = "withResults"; 

        /// <summary> 

        /// Выборы, на основе которых печатается протокол 

        /// </summary> 

        internal const string PRN_ELECTION = "Election"; 


        #endregion 

 

 

        #region Макро переменные шаблонов отчета 

        /// <summary> 

        /// Наименование выборов 

        /// </summary> 

        internal const string MACRO_ELECTION_NAME = "{ElectionName}"; 

        /// <summary> 

        /// Дата выборов 

        /// </summary> 

        internal const string MACRO_VOTING_DATE = "{VotingDate}"; 

        /// <summary> 

        /// Дата и время печати (формирования) отчета 

        /// </summary> 

        internal const string MACRO_CURRENT_DATE = "{CurrentDate}"; 

        /// <summary> 

        /// Номер УИК 

        /// </summary> 

        internal const string MACRO_UIK = "{UIK}"; 

        /// <summary> 

        /// Наименование протокола 

        /// </summary> 

        internal const string MACRO_PROTOCOL_NAME = "{ProtocolName}"; 

        #endregion 

 

 

        #region Форматы даты и времени 

        /// <summary> 

        /// Формат вывода даты выборов 

        /// </summary> 

        internal const string ELECTION_DATE_FORMAT = "d MMMM yyyy года"; 

        /// <summary> 

        /// Формат вывода текущей даты 

        /// </summary> 

        internal const string CURRENT_DATETIME_FORMAT = "dd.MM.yyyy HH:mm:ss"; 

 

 

        #endregion 

 

 

        #endregion 

 

 

        /// <summary> 

        /// Менеджер печати 

        /// </summary> 

        private IPrintingManager _printingManager; 

        /// <summary> 

        /// Менеджер выборов 


        /// </summary> 

        private IElectionManager _electionManager; 

 

 

        /// <summary> 

        /// Описание отчета 

        /// </summary> 

        private struct ReportDesc 

        { 

            /// <summary> 

            /// Метод построения отчета 

            /// </summary> 

            public ReportDelegate Builder; 

 

 

            /// <summary> 

            /// Метод, выполняемый перед построением отчета 

            /// </summary> 

            public PrologEpilogDelegate Prolog; 

 

 

            /// <summary> 

            /// Метод, выполняемый после вывода отчета на печать 

            /// </summary> 

            public PrologEpilogDelegate Epilog; 

 

 

            /// <summary> 

            /// Конструктор 

            /// </summary> 

            /// <param name="builder">Метод построения отчета</param> 

            public ReportDesc(ReportDelegate builder) 

            { 

                Builder = builder; 

                Prolog = delegate(ListDictionary parameters) { }; 

                Epilog = delegate(ListDictionary parameters) { }; 

            } 

 

 

            /// <summary> 

            /// Конструктор 

            /// </summary> 

            /// <param name="builder">Метод построения отчета</param> 

            /// <param name="prolog">Метод, выполняемый перед построением отчета</param> 

            /// <param name="epilog">Метод, выполняемый после вывода отчета на печать</param> 

            public ReportDesc(ReportDelegate builder, PrologEpilogDelegate prolog, PrologEpilogDelegate epilog) 

            { 

                Builder = builder; 

                Prolog = prolog; 

                Epilog = epilog; 


            } 

        } 

 

 

        /// <summary> 

        /// Делегат - функция пролога/эпилога к отчету отчета 

        /// </summary> 

        private delegate void PrologEpilogDelegate(ListDictionary reportParameters); 

 

 

        /// <summary> 

        /// Делегат - функция построения отчета 

        /// </summary> 

        private delegate PrinterJob ReportDelegate( 

            PdfReportBuilder pdfBuilder, 

            ListDictionary reportParameters, 

            PrologEpilogDelegate prolog, PrologEpilogDelegate epilog); 

 

 

        /// <summary> 

        /// Функции построения отчетов 

        /// индекс: PrintAction 

        /// </summary> 

        private ReportDesc[] _reportMap; 

 

 

        /// <summary> 

        /// конструктор 

        /// </summary> 

        public ReportBuilder(IPrintingManager printingManager, IElectionManager electionManager) 

        { 

            CodeContract.Requires(printingManager != null); 

            CodeContract.Requires(electionManager != null); 

 

 

            _printingManager = printingManager; 

            _electionManager = electionManager; 

            InitReportMap(); 

        } 

 

 

        /// <summary> 

        /// заполняем массив функций построения отчетов 

        /// </summary> 

        private void InitReportMap() 

        { 

            var reportTypes = Enum.GetValues(typeof(ReportType)); 

            _reportMap = new ReportDesc[reportTypes.Length]; 

 

 


            foreach (var reportType in reportTypes) 

                _reportMap[(int)reportType] = new ReportDesc(GenericPrintThread); 

 

 

            _reportMap[(int)ReportType.SourceData] = new ReportDesc(GenericPrintThread, 

                (reportParameters) => { }, 

                (reportParameters) => { }); 

            _reportMap[(int)ReportType.FailedControlRelations] = new ReportDesc(GenericPrintThread, 

                // свяжем автовычисляемые строки с методами их вычисления 

                (reportParameters) => _electionManager.SourceData. 

                    BindAutoLinesAndChecksCountMethods((Election.Voting.Election)reportParameters[PRN_ELECTION]), 

                (reportParameters) => { }); 

            _reportMap[(int)ReportType.TestResults] = new ReportDesc(GenericPrintThread); 

            _reportMap[(int)ReportType.ElectionProtocol] = new ReportDesc(GenericPrintThread, 

                // свяжем автовычисляемые строки с методами их вычисления 

                (reportParameters) => _electionManager.SourceData. 

                    BindAutoLinesAndChecksCountMethods((Election.Voting.Election)reportParameters[PRN_ELECTION]), 

                (reportParameters) => { }); 

        } 

 

 

        /// <summary> 

        /// Построить указанный отчет 

        /// </summary> 

        /// <param name="reportType">Тип отчета</param> 

        /// <param name="parameters">параметры отчета</param> 

        public PrinterJob BuildReport(ReportType reportType, ListDictionary parameters) 

        { 

            var pdfBuilder = new PdfReportBuilder(); 

 

 

            var reportParameters = new ListDictionary(); 

            reportParameters[PRN_REPORT_NAME] = reportType; 

 

 

			// добавим параметры 

			foreach (string key in parameters.Keys) 

			{ 

				reportParameters[key] = parameters[key]; 

			} 

 

 

            var reportDesc = _reportMap[(int)reportType]; 

            return reportDesc.Builder(pdfBuilder, reportParameters, reportDesc.Prolog, reportDesc.Epilog); 

        } 

 

 

        /// <summary> 

        /// Объект синхронизации для построения отчетов 

        /// </summary> 


        private static object buildReportSync = new object(); 

 

 

        /// <summary> 

        /// Печать отчета 

        /// </summary> 

        private PrinterJob GenericPrintThread( 

            PdfReportBuilder report, ListDictionary reportParameters, PrologEpilogDelegate prolog, PrologEpilogDelegate epilog) 

        { 

            lock (buildReportSync) 

            { 

                prolog(reportParameters); 

                PrepareReport((ReportType)reportParameters[PRN_REPORT_NAME], report, reportParameters); 

				var job = report.Build(); 

                epilog(reportParameters); 

 

 

				return job; 

            } 

        } 

 

 

        /// <summary> 

        /// Подготовка отчета для печати 

        /// </summary> 

        private void PrepareReport(ReportType reportType, PdfReportBuilder pdfBuilder, ListDictionary reportParameters) 

        { 

            var template = ReportTemplate.LoadTemplate(reportType, _printingManager.Logger); 

 

 

            if (reportType == ReportType.ElectionProtocol) 

                ApplySourceDataTemplate(reportParameters, template); 

 

 

            template.LoadParameters(reportParameters); 

 

 

            pdfBuilder.Headers[Section.Header] = template.ConstructHeader(template.Header); 

            pdfBuilder.Headers[Section.PageHeader] = template.ConstructHeader(template.PageHeader); 

            pdfBuilder.Headers[Section.Footer] = template.ConstructHeader(template.Footer); 

            pdfBuilder.Headers[Section.PageFooter] = template.ConstructHeader(template.PageFooter); 

            pdfBuilder.m_data = template.PrepareTable(); 

            if (template.Table != null) 

                pdfBuilder.TableDotted = template.Table.IsDotted; 

            pdfBuilder.ClaspFooter = template.ClaspFooter; 

            pdfBuilder.PageNumbered = template.PageNumbered; 

        } 

 

 

        /// <summary> 


        /// Применяет шаблон протокола, если он есть в ИД 

        /// </summary> 

        /// <param name="reportParameters">Параметры отчета</param> 

        /// <param name="template">Шаблон по умолчанию</param> 

        private void ApplySourceDataTemplate(ListDictionary reportParameters, ReportTemplate template) 

        { 

            var election = (Election.Voting.Election)reportParameters[PRN_ELECTION]; 

 

 

            // коррекция размеров столбцов, считаем, что столбцы идут в следующем порядке 

            // numberWidth; nameWidth; valueWidth; textValueWidth 

            if (template.Table.Columns.Length >= 4) 

            { 

                template.Table.Columns[0].Width = election.Protocol.NumberWidth; 

                template.Table.Columns[1].Width = election.Protocol.NameWidth; 

                template.Table.Columns[2].Width = election.Protocol.ValueWidth; 

                template.Table.Columns[3].Width = election.Protocol.TextValueWidth; 

            } 

 

 

            // добавляю произвольные строки протокола если они есть 

            if (election.Protocol.Texts != null) 

            { 

                // получаем шаблон протокола 

                ProtocolText protocolTemplate = election.Protocol.GetProtocolTemplate( 

                    (bool)reportParameters[PRN_FINAL_PROTOCOL]); 

 

 

                // если шаблон протокола есть 

                if (protocolTemplate != null) 

                { 

                    // если шаблон содержит заголовки 

                    if (protocolTemplate.m_aProtocolLines.Length > 0) 

                    { 

                        var headers = new Dictionary<Section, List<BasePlainElement>>(); 

                        foreach (Section section in Enum.GetValues(typeof(Section))) 

                        { 

                            headers[section] = new List<BasePlainElement>(); 

                        } 

 

 

                        try 

                        { 

                            // перебираем элементы массива 

                            foreach (ProtocolTextLine oTextLine in protocolTemplate.m_aProtocolLines) 

                            { 

                                // текст строки 

                                StringBuilder sText = new StringBuilder(oTextLine.m_sText); 

 

 


                                // стандартные замены 

                                sText.Replace(MACRO_ELECTION_NAME, election.Name); 

                                if (PlatformDetector.IsUnix) 

                                { 

                                    sText.Replace(MACRO_VOTING_DATE, ReportTemplateParser.DataConvert( 

                                        _electionManager.SourceData.ElectionDate.ToString(ELECTION_DATE_FORMAT))); 

                                    sText.Replace(MACRO_CURRENT_DATE, ReportTemplateParser.DataConvert( 

                                        DateTime.Now.ToString(CURRENT_DATETIME_FORMAT))); 

                                } 

                                else 

                                { 

                                    sText.Replace(MACRO_VOTING_DATE, _electionManager.SourceData.ElectionDate.ToString(ELECTION_DATE_FORMAT)); 

                                    sText.Replace(MACRO_CURRENT_DATE, DateTime.Now.ToString(CURRENT_DATETIME_FORMAT)); 

                                } 

                                sText.Replace(MACRO_UIK, _electionManager.UIK.ToString()); 

 

 

                                if (oTextLine.m_sText != null && oTextLine.m_sText.IndexOf(MACRO_PROTOCOL_NAME) != -1) 

                                { 

                                    string[] sLines = election.Protocol.Name.Split('\n'); 

 

 

                                    if (sLines.Length > 1) 

                                    { 

                                        for (int idx = 0; idx < sLines.Length; idx++) 

                                        { 

                                            headers[oTextLine.m_eSection].Add( 

                                                new LineClause( 

                                                    sLines[idx].Trim(),  

                                                    oTextLine.m_eAlign,  

                                                    oTextLine.FontSize,  

                                                    oTextLine.Bold,  

                                                    oTextLine.Italic)); 

                                        } 

                                        continue; 

                                    } 

                                    else 

                                    { 

                                        sText.Replace(MACRO_PROTOCOL_NAME, election.Protocol.Name); 

                                    } 

                                } 

 

 

                                // строки которые мы добавляем 

                                headers[oTextLine.m_eSection].Add( 

                                    new LineClause( 

                                        sText.ToString(),  

                                        oTextLine.m_eAlign,  

                                        oTextLine.FontSize,  

                                        oTextLine.Bold,  


                                        oTextLine.Italic)); 

                            } 

 

 

                            template.PageHeader = new BasePlainElement[headers[Section.PageHeader].Count]; 

                            headers[Section.PageHeader].CopyTo(template.PageHeader); 

                            template.Header = new BasePlainElement[headers[Section.Header].Count]; 

                            headers[Section.Header].CopyTo(template.Header); 

                            template.Footer = new BasePlainElement[headers[Section.Footer].Count]; 

                            headers[Section.Footer].CopyTo(template.Footer); 

                            template.PageFooter = new BasePlainElement[headers[Section.PageFooter].Count]; 

                            headers[Section.PageFooter].CopyTo(template.PageFooter); 

                        } 

                        catch (Exception ex) 

                        { 

                            Managers.PrintingManager.Logger.LogException(Message.PrintingReportHeadersBuildFailed, ex); 

                        } 

                    } 

 

 

                    // если есть строки таблицы протокола 

                    if (protocolTemplate.m_aVoteLines != null && protocolTemplate.m_aVoteLines.Length > 0) 

                    { 

                        try 

                        { 

                            int nLineNumber = election.Protocol.GetLatestLineNumber((bool)reportParameters[PRN_FINAL_PROTOCOL]); 

                            // сформирую строку для снятого кандидата 

                            string sDisabled = election.Protocol.DisabledString; 

 

 

                            // признак необходимости отображения снятых кандидатов 

                            bool bShowDisabled = sDisabled != null && sDisabled != String.Empty; 

 

 

                            List<BasePlainElement> table = new List<BasePlainElement>(); 

 

 

                            // признак переноса строк 

                            bool bDelimiter = false; 

 

 

                            foreach (VoteTextLine oVoteLine in protocolTemplate.m_aVoteLines) 

                            { 

                                // введем фильтрацию 

                                var oMask = new VoteKey(); 

								oMask.ElectionNum = election.ElectionId; 

 

 

                                switch (oVoteLine.Type) 

                                { 


                                    case VoteLineType.Vote: 

                                        // перебираем всех кандидатов 

                                        foreach (Candidate oCurCand in election.Candidates) 

                                        { 

                                            if ((oCurCand.Id == oVoteLine.ID) && (!oCurCand.Disabled || bShowDisabled)) 

                                            { 

                                                nLineNumber++; 

                                                // ограничение: 

                                                oMask.CandidateId = oCurCand.Id; 

                                                // число голосов: 0 - для предварительного протокола 

                                                int nVotesCount = Managers.ElectionManager.VotingResults.VotesCount(oMask); 

                                                table.Add(new LineClause( 

                                                    new string[] { 

                                                        nLineNumber.ToString(), 

                                                        oCurCand.GetFIO(!oCurCand.NoneAbove), 

                                                        oCurCand.Disabled ? sDisabled : ((bool)reportParameters[PRN_PRINT_RESULTS] ? nVotesCount.ToString() : "0"), 

                                                        oCurCand.Disabled ? "" : ((bool)reportParameters[PRN_PRINT_RESULTS]  

                                                        ? "(" + CustomRusNumber.Str(nVotesCount, true).Trim() + ")" 

                                                        : CustomRusNumber.Str(0, true).Trim()) 

                                                    }, 

                                                    oVoteLine.FontSize, oVoteLine.Bold, oVoteLine.Italic, bDelimiter)); 

 

 

                                                bDelimiter = false; 

                                                break; 

                                            } 

                                        } 

                                        break; 

                                    case VoteLineType.Line: 

                                        // перебираем все строки протокола 

                                        foreach (Line oCurLine in election.Protocol.Lines) 

                                        { 

                                            if (oCurLine.Id == oVoteLine.ID) 

                                            { 

                                                // значение  строки - 0, для предварительного протокола 

                                                int nValue = (bool)reportParameters[PRN_FINAL_PROTOCOL] && oCurLine.Value.HasValue 

                                                    ? oCurLine.Value.Value : 0; 

 

 

                                                table.Add(new LineClause( 

                                                    new string[] { 

                                                        oCurLine.Num + oCurLine.AdditionalNum, 

                                                        oCurLine.Name, 

                                                        (bool)reportParameters[PRN_PRINT_RESULTS] ? nValue.ToString() : "0", 

                                                        (bool)reportParameters[PRN_PRINT_RESULTS]  

                                                        ? "(" + CustomRusNumber.Str(nValue, true).Trim() + ")"  

                                                        : CustomRusNumber.Str(0, true).Trim() 

                                                    }, 

                                                    oVoteLine.FontSize, oVoteLine.Bold, oVoteLine.Italic, bDelimiter)); 

 


 
                                                bDelimiter = false; 

                                                break; 

                                            } 

                                        } 

                                        break; 

                                    case VoteLineType.Delimiter: 

                                        bDelimiter = true; 

                                        break; 

                                } 

                            } 

                            template.Table.Body = new BasePlainElement[table.Count]; 

                            table.CopyTo(template.Table.Body); 

                        } 

                        catch (Exception ex) 

                        { 

                            Managers.PrintingManager.Logger.LogException(Message.PrintingReportBodyBuildFailed, ex); 

                        } 

                    } 

                } 

            } 

        } 

    } 

}


