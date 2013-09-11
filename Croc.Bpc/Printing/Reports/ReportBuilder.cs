using System; 
using System.Collections.Generic; 
using System.Collections.Specialized; 
using System.Text; 
using System.Text.RegularExpressions; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Printing.Reports.Templates; 
using Croc.Bpc.RegExpressions; 
using Croc.Core; 
using Croc.Core.Utils; 
using Croc.Core.Utils.Text; 
using Croc.Bpc.Voting; 
namespace Croc.Bpc.Printing.Reports 
{ 
    public class ReportBuilder 
    { 
        #region Константы 
        #region Параметры формирования отчетов 
        internal const string PRN_REPORT_NAME = "reportName"; 
        internal const string PRN_TEST_PROTOCOL = "test"; 
        internal const string PRN_PRINT_RESULTS = "withResults"; 
        internal const string PRN_ELECTION = "Election"; 
        #endregion 
        #region Макро переменные шаблонов отчета 
        internal const string MACRO_ELECTION_NAME = "{ElectionName}"; 
        internal const string MACRO_VOTING_DATE = "{VotingDate}"; 
        internal const string MACRO_CURRENT_DATE = "{CurrentDate}"; 
        internal const string MACRO_UIK = "{UIK}"; 
        internal const string MACRO_PROTOCOL_NAME = "{ProtocolName}"; 
        #endregion 
        #region Форматы даты и времени 
        internal const string ELECTION_DATE_FORMAT = "d MMMM yyyy года"; 
        internal const string CURRENT_DATETIME_FORMAT = "dd.MM.yyyy HH:mm:ss"; 
        #endregion 
        #endregion 
        private readonly IPrintingManager _printingManager; 
        private readonly IElectionManager _electionManager; 
        private readonly IVotingResultManager _votingResultManager; 
        private struct ReportDescription 
        { 
            public readonly ReportDelegate Builder; 
            public readonly PrologEpilogDelegate Prolog; 
            public readonly PrologEpilogDelegate Epilog; 
            public ReportDescription(ReportDelegate builder) 
            { 
                Builder = builder; 
                Prolog = delegate(ListDictionary parameters) { }; 
                Epilog = delegate(ListDictionary parameters) { }; 
            } 
            public ReportDescription(ReportDelegate builder, PrologEpilogDelegate prolog, PrologEpilogDelegate epilog) 
            { 
                Builder = builder; 
                Prolog = prolog; 
                Epilog = epilog; 
            } 
        } 
        private delegate void PrologEpilogDelegate(ListDictionary reportParameters); 
        private delegate PrinterJob ReportDelegate( 
            PdfReportBuilder pdfBuilder, 
            ListDictionary reportParameters, 
            int copies, 
            PrologEpilogDelegate prolog, PrologEpilogDelegate epilog); 
        private ReportDescription[] _reportMap; 
        public ReportBuilder( 
            IPrintingManager printingManager, 
            IElectionManager electionManager, 
            IVotingResultManager votingResultManager) 
        { 
            CodeContract.Requires(printingManager != null); 
            CodeContract.Requires(electionManager != null); 
            CodeContract.Requires(votingResultManager != null); 
            _printingManager = printingManager; 
            _electionManager = electionManager; 
            _votingResultManager = votingResultManager; 
            InitReportMap(); 
        } 
        private void InitReportMap() 
        { 
            var reportTypes = Enum.GetValues(typeof(ReportType)); 
            _reportMap = new ReportDescription[reportTypes.Length]; 
            foreach (var reportType in reportTypes) 
                _reportMap[(int)reportType] = new ReportDescription(GenericPrintThread); 
            _reportMap[(int)ReportType.FailedControlRelations] = new ReportDescription(GenericPrintThread, 
                reportParameters => _electionManager.SourceData. 
                    BindAutoLinesAndChecksCountMethods((Election)reportParameters[PRN_ELECTION]), 
                reportParameters => { }); 
            _reportMap[(int)ReportType.ElectionProtocol] = new ReportDescription(GenericPrintThread, 
                reportParameters => _electionManager.SourceData. 
                    BindAutoLinesAndChecksCountMethods((Election)reportParameters[PRN_ELECTION]), 
                reportParameters => { }); 
        } 
        public PrinterJob BuildReport(ReportType reportType, ListDictionary parameters, int copies) 
        { 
            var pdfBuilder = new PdfReportBuilder(); 
            var reportParameters = new ListDictionary(); 
            reportParameters[PRN_REPORT_NAME] = reportType; 
            foreach (string key in parameters.Keys) 
            { 
                reportParameters[key] = parameters[key]; 
            } 
            var reportDesc = _reportMap[(int)reportType]; 
            return reportDesc.Builder(pdfBuilder, reportParameters, copies, reportDesc.Prolog, reportDesc.Epilog); 
        } 
        private static readonly object s_buildReportSync = new object(); 
        private PrinterJob GenericPrintThread( 
            PdfReportBuilder report, 
            ListDictionary reportParameters, 
            int copies, 
            PrologEpilogDelegate prolog, 
            PrologEpilogDelegate epilog) 
        { 
            lock (s_buildReportSync) 
            { 
                prolog(reportParameters); 
                PrepareReport((ReportType)reportParameters[PRN_REPORT_NAME], report, reportParameters); 
                var job = report.Build((ReportType)reportParameters[PRN_REPORT_NAME], copies); 
                epilog(reportParameters); 
                return job; 
            } 
        } 
        private void PrepareReport(ReportType reportType, PdfReportBuilder pdfBuilder, ListDictionary reportParameters) 
        { 
            var template = ReportTemplate.LoadTemplate(reportType, _printingManager.Logger); 
            if (reportType == ReportType.ElectionProtocol || reportType == ReportType.PreliminaryElectionProtocol) 
                ApplySourceDataTemplate(reportType, reportParameters, template); 
            template.LoadParameters(reportParameters); 
            pdfBuilder.Headers[PageSection.Header] = ReportTemplate.ConstructHeader(template.Header); 
            pdfBuilder.Headers[PageSection.PageHeader] = ReportTemplate.ConstructHeader(template.PageHeader); 
            pdfBuilder.Headers[PageSection.Footer] = ReportTemplate.ConstructHeader(template.Footer); 
            pdfBuilder.Headers[PageSection.PageFooter] = ReportTemplate.ConstructHeader(template.PageFooter); 
            pdfBuilder.Data = template.PrepareTable(); 
            pdfBuilder.TemplateFont = template.Font; 
            pdfBuilder.FontSize = template.FontSize; 
            pdfBuilder.Margins = template.Margins; 
            pdfBuilder.ClaspFooter = template.ClaspFooter; 
            pdfBuilder.PageNumbered = template.PageNumbered; 
        } 
        #region Применение шаблона из ИД 
        private void ApplySourceDataTemplate(ReportType reportType, ListDictionary reportParameters, ReportTemplate template) 
        { 
            int fromElection = -1; 
            if (reportParameters.Contains(PRN_ELECTION)) 
            { 
                fromElection = _electionManager.SourceData.GetElectionIndex( 
                    (Election)reportParameters[PRN_ELECTION]); 
            } 
            var toElection = fromElection != -1 ? 
                fromElection + 1 : _electionManager.SourceData.Elections.Length; 
            fromElection = fromElection == -1 ? 0 : fromElection; 
            var tables = new List<BaseTableHolder>(); 
            for (int i = fromElection; i < toElection; i++) 
            { 
                var election = _electionManager.SourceData.Elections[i]; 
                Table headerTable = null; 
                Table footerTable = null; 
                Table bodyTable = null; 
                if (election.Protocol.FontType != FontType.Default) 
                    template.Font = election.Protocol.FontType; 
                if (election.Protocol.FontSize > 0) 
                    template.FontSize = election.Protocol.FontSize; 
                ProtocolText protocolTemplate = election.Protocol.GetProtocolTemplate( 
                    reportType == ReportType.ElectionProtocol); 
                if (protocolTemplate == null) 
                    return; 
                if (protocolTemplate.ProtocolLines.Length > 0) 
                { 
                    var headers = new Dictionary<PageSection, List<BasePlainElement>>(); 
                    foreach (PageSection section in Enum.GetValues(typeof(PageSection))) 
                    { 
                        headers[section] = new List<BasePlainElement>(); 
                    } 
                    try 
                    { 
                        ApplyStandartTemplates(protocolTemplate, election, headers); 
                        template.PageHeader = new BasePlainElement[headers[PageSection.PageHeader].Count]; 
                        headers[PageSection.PageHeader].CopyTo(template.PageHeader); 
                        if (toElection - fromElection > 1) 
                        { 
                            template.Header = new BasePlainElement[0]; 
                            headerTable = new Table 
                            { 
                                Columns = new[] 
                                    { 
                                        new ColDefinition 
                                            { 
                                                Width = 100,  
                                                Align = null, 
                                            }, 
                                    }, 
                                Body = headers[PageSection.Header].ToArray(), 
                            }; 
                            template.Footer = new BasePlainElement[0]; 
                            headers[PageSection.Footer].Add( 
                                new LineClause 
                                { 
                                    Columns = new[] { new LinePart { Text = "" } }, 
                                    NewPage = true, 
                                    ResetPageNumber = true 
                                }); 
                            footerTable = new Table 
                            { 
                                Columns = new[] 
                                    { 
                                        new ColDefinition 
                                            { 
                                                Width = 100,  
                                                Align = null, 
                                            }, 
                                    }, 
                                Body = headers[PageSection.Footer].ToArray(), 
                            }; 
                        } 
                        else 
                        { 
                            template.Header = new BasePlainElement[headers[PageSection.Header].Count]; 
                            headers[PageSection.Header].CopyTo(template.Header); 
                            template.Footer = new BasePlainElement[headers[PageSection.Footer].Count]; 
                            headers[PageSection.Footer].CopyTo(template.Footer); 
                        } 
                        template.PageFooter = new BasePlainElement[headers[PageSection.PageFooter].Count]; 
                        headers[PageSection.PageFooter].CopyTo(template.PageFooter); 
                    } 
                    catch (Exception ex) 
                    { 
                        _printingManager.Logger.LogError(Message.PrintingReportHeadersBuildFailed, ex); 
                    } 
                } 
                if (protocolTemplate.VoteLines != null && protocolTemplate.VoteLines.Length > 0) 
                { 
                    try 
                    { 
                        bodyTable = new Table 
                        { 
                            Columns = new[] 
                            { 
                                new ColDefinition {Width = election.Protocol.NumberWidth}, 
                                new ColDefinition {Width = election.Protocol.NameWidth}, 
                                new ColDefinition {Width = election.Protocol.ValueWidth}, 
                                new ColDefinition {Width = election.Protocol.TextValueWidth}, 
                            } 
                        }; 
                        var tableEntry = CreateProtocolBodyTable( 
                            protocolTemplate, 
                            election, 
                            reportType == ReportType.ElectionProtocol, 
                            (bool)reportParameters[PRN_PRINT_RESULTS]); 
                        bodyTable.Body = tableEntry.ToArray(); 
                    } 
                    catch (Exception ex) 
                    { 
                        _printingManager.Logger.LogError(Message.PrintingReportBodyBuildFailed, ex); 
                    } 
                } 
                if (headerTable != null) 
                { 
                    tables.Add(headerTable); 
                } 
                if (bodyTable != null) 
                { 
                    tables.Add(bodyTable); 
                } 
                if (footerTable != null) 
                { 
                    tables.Add(footerTable); 
                } 
            } 
            template.Body = tables.ToArray(); 
        } 
        private List<BasePlainElement> CreateProtocolBodyTable( 
            ProtocolText protocolTemplate, 
            Election election, 
            bool final, 
            bool printResults) 
        { 
            int lineNumber = election.Protocol.GetLatestLineNumber(final); 
            string disabled = election.Protocol.DisabledString; 
            bool showDisabled = !string.IsNullOrEmpty(disabled); 
            bool delimiter = false; 
            var table = new List<BasePlainElement>(); 
            foreach (var voteLine in protocolTemplate.VoteLines) 
            { 
                var mask = new VoteKey { ElectionNum = election.ElectionId }; 
                switch (voteLine.Type) 
                { 
                    case VoteLineType.Vote: 
                        foreach (Candidate currentCand in election.Candidates) 
                        { 
                            if ((currentCand.Id == voteLine.ID) && (!currentCand.Disabled || showDisabled)) 
                            { 
                                lineNumber++; 
                                mask.CandidateId = currentCand.Id; 
                                int votesCount = _votingResultManager.VotingResults.VotesCount(mask); 
                                table.Add(new LineClause( 
                                    new[]  
                                    { 
                                        lineNumber.ToString(), 
                                        currentCand.GetInitials(!currentCand.NoneAbove), 
                                        currentCand.Disabled ? disabled : (printResults ? votesCount.ToString() : "0"), 
                                        currentCand.Disabled ? "" : (printResults 
                                        ? "(" + CustomRusNumber.Str(votesCount, true).Trim() + ")" 
                                        : CustomRusNumber.Str(0, true).Trim()) 
                                    }, 
                                    voteLine.FontSize, voteLine.Bold, voteLine.Italic, delimiter)); 
                                delimiter = false; 
                                break; 
                            } 
                        } 
                        break; 
                    case VoteLineType.Line: 
                        if (string.CompareOrdinal(voteLine.ID, VoteTextLine.TOTAL_RECEIVED_VOTETEXTLINE_ID) == 0) 
                        { 
                            var value = _votingResultManager.VotingResults.VotesCount( 
                                new VoteKey( 
                                    BlankType.AllButBad, 
                                    null, null, null, null, 
                                    _electionManager.SourceData.GetBlankIdByElectionNumber(election.ElectionId))); 
                            var text = string.IsNullOrEmpty(voteLine.Text) 
                                           ? VoteTextLine.TOTAL_RECEIVED_VOTETEXTLINE_DEFAULT_TEXT 
                                           : voteLine.Text; 
                            table.Add(new LineClause( 
                                          new[] 
                                              { 
                                                  "", 
                                                  text, 
                                                  printResults ? value.ToString() : "0", 
                                                  printResults 
                                                      ? "(" + CustomRusNumber.Str(value, true).Trim() + ")" 
                                                      : CustomRusNumber.Str(0, true).Trim() 
                                              }, 
                                          voteLine.FontSize, voteLine.Bold, voteLine.Italic, delimiter)); 
                        } 
                        else 
                        { 
                            foreach (Line currentLine in election.Protocol.Lines) 
                            { 
                                if (currentLine.Id == voteLine.ID) 
                                { 
                                    int value = final && currentLine.Value.HasValue ? currentLine.Value.Value : 0; 
                                    table.Add(new LineClause( 
                                                  new[] 
                                                      { 
                                                          currentLine.Num + currentLine.AdditionalNum, 
                                                          currentLine.Name, 
                                                          printResults ? value.ToString() : "0", 
                                                          printResults 
                                                              ? "(" + CustomRusNumber.Str(value, true).Trim() + ")" 
                                                              : CustomRusNumber.Str(0, true).Trim() 
                                                      }, 
                                                  voteLine.FontSize, voteLine.Bold, voteLine.Italic, delimiter)); 
                                    break; 
                                } 
                            } 
                        } 
                        delimiter = false; 
                        break; 
                    case VoteLineType.Delimiter: 
                        delimiter = true; 
                        break; 
                } 
            } 
            return table; 
        } 
        private void ApplyStandartTemplates( 
            ProtocolText protocolTemplate, 
            Election election, 
            Dictionary<PageSection, List<BasePlainElement>> headers) 
        { 
            foreach (ProtocolTextLine textLine in protocolTemplate.ProtocolLines) 
            { 
                var text = new StringBuilder(textLine.Text); 
                ApplyStandartTemplatesToLine(election, text); 
                if (textLine.Text != null && textLine.Text.IndexOf(MACRO_PROTOCOL_NAME) != -1) 
                { 
                    var lines = election.Protocol.Name.Split('\n'); 
                    if (lines.Length > 1) 
                    { 
                        foreach (var line in lines) 
                            AddLineToList(headers[textLine.Section], textLine, line); 
                        continue; 
                    } 
                    text.Replace(MACRO_PROTOCOL_NAME, election.Protocol.Name); 
                } 
                AddLineToList(headers[textLine.Section], textLine, text.ToString()); 
            } 
        } 
        private void ApplyStandartTemplatesToLine(Election election, StringBuilder text) 
        { 
            text.Replace(MACRO_ELECTION_NAME, election.Name); 
            if (PlatformDetector.IsUnix) 
            { 
                text.Replace(MACRO_VOTING_DATE, ReportTemplateParser.DataConvert( 
                    _electionManager.SourceData.ElectionDate.ToString(ELECTION_DATE_FORMAT, 
                                                                      new System.Globalization.CultureInfo("ru-RU")))); 
                text.Replace(MACRO_CURRENT_DATE, ReportTemplateParser.DataConvert( 
                    DateTime.Now.ToString(CURRENT_DATETIME_FORMAT, new System.Globalization.CultureInfo("ru-RU")))); 
            } 
            else 
            { 
                text.Replace(MACRO_VOTING_DATE, 
                    _electionManager.SourceData.ElectionDate.ToString(ELECTION_DATE_FORMAT)); 
                text.Replace(MACRO_CURRENT_DATE, DateTime.Now.ToString(CURRENT_DATETIME_FORMAT)); 
            } 
            text.Replace(MACRO_UIK, _electionManager.SourceData.Uik.ToString()); 
            var exp = new UikMemberNamesRegex(); 
            foreach (Match match in exp.Matches(text.ToString())) 
            { 
                var altValue = match.Value.Split(':')[1].Trim('}'); 
                string name = null; 
                if (match.Value.Contains(CommitteeMemberType.ChairmanAssistant.ToString())) 
                { 
                    name = _electionManager.SourceData.GetCommitteeMemberInitialByType(CommitteeMemberType.ChairmanAssistant); 
                } 
                else if (match.Value.Contains(CommitteeMemberType.Chairman.ToString())) 
                { 
                    name = 
                        _electionManager.SourceData.GetCommitteeMemberInitialByType(CommitteeMemberType.Chairman); 
                } 
                else if (match.Value.Contains(CommitteeMemberType.Secretary.ToString())) 
                { 
                    name = _electionManager.SourceData.GetCommitteeMemberInitialByType(CommitteeMemberType.Secretary); 
                } 
                text.Replace(match.Value, string.IsNullOrEmpty(name) ? altValue : name); 
            } 
        } 
        private static void AddLineToList(List<BasePlainElement> list, ProtocolTextLine textLine, string lineText) 
        { 
            list.Add( 
                new LineClause(lineText.Trim(), textLine.Align, textLine.FontSize, textLine.Bold, textLine.Italic)); 
        } 
        #endregion 
    } 
}
