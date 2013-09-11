using System; 

using System.Collections.Generic; 

using System.Data; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Bpc.Election.Voting; 

using Croc.Bpc.FileSystem; 

using Croc.Bpc.Printing.Reports.Templates; 

using Croc.Core.Utils.IO; 

using iTextSharp.text; 

using iTextSharp.text.pdf; 

using Table = iTextSharp.text.Table; 

using Voting = Croc.Bpc.Election.Voting; 

 

 

namespace Croc.Bpc.Printing.Reports 

{ 

    /// <summary> 

    /// ??????????? ?????? ? PDF 

    /// </summary> 

    public class PdfReportBuilder 

    { 

        /// <summary> 

        /// ???????????? ?????????? ??????? 

        /// </summary> 

        private int m_nTotalPagesCount; 

 

 

        /// <summary> 

        ///		??????? ?????? ??????? ??????? 

        /// </summary> 

        public bool PageNumbered = true; 

 

 

        /// <summary> 

        /// ???????? ?? ????? ? ???????? 

        /// </summary> 

        public bool TableDotted = false; 

 

 

        /// <summary> 

        /// ????????? ? ??????? ????????? 

        ///     ????: ??? ?????? 

        ///     ????????: ????? ?????  

        /// </summary> 

        public Dictionary<Voting.Section, Lines> Headers = new Dictionary<Voting.Section, Lines>(); 

 

 

        /// <summary> 

        /// ????? ?????? 

        /// </summary> 


        public DataSet m_data; 

 

 

        /// <summary> 

        /// ????? 

        /// </summary> 

        private Font m_font; 

 

 

        /// <summary> 

        /// ?????? ?????? ???????????? ?????? ??????(?? ????????? ????? 1.5) 

        /// </summary> 

        private const double dblLeading_font = 1.2; 

 

 

        /// <summary> 

        /// ?????? ??????? ? ???????? 

        /// </summary> 

        private const float ftTableWidth = 5; 

        /// <summary> 

        /// ?????????? ?????? ????? (????????) ?????? ? ????? ??????? 

        /// </summary> 

        private const int cEmptyLines = 1; 

 

 

        /// <summary> 

        /// ?????????, ??????? ????? ???????????? ??? ??????? ???????? 

        /// </summary> 

        const string FONT_CODEPAGE = "CP1251"; 

 

 

        /// <summary> 

        /// ?????? ?????? ?? ????????? 

        /// </summary> 

        private int m_nDefaultFontSize; 

 

 

        /// <summary> 

        /// ??????? ???????? ?????? ? ?????? 

        /// </summary> 

        public bool ClaspFooter = false; 

 

 

        /// <summary> 

        /// ??????????? ???????? 

        /// </summary> 

        Document pd = null; 

 

 

        /// <summary> 


        ///	???????????? PDF 

        /// </summary> 

        /// <param name="reportType">??? ??????</param> 

        /// <returns>???? ? ??????????????? pdf-?????</returns> 

        public PrinterJob Build() 

        { 

            var reportConfig = Managers.PrintingManager.ReportConfig; 

 

 

            // ??????? ????? ??????? ???????: 

            m_nTotalPagesCount = 0; 

 

 

            // TODO: ???? ????? ?????????????, ???? ?? ?? ????????? ??????????? ?????????? ??????? PDF,  

            // ??????? ????? ?????????? 

 

 

            // ????????? ????, ? ??????? ????? ?????????? ???????? 

            using (var file = FileUtils.CreateUniqueFileWithDateMark( 

                Managers.FileSystemManager.GetDataDirectoryPath(FileType.Report), 

                "PrintJob", 

                "pdf", 

                6)) 

            { 

                try 

                { 

                    // ??????? ????? PDF-???????? 

                    pd = new Document(PageSize.A4); 

                    pd.ClaspFooter = ClaspFooter; 

 

 

                    // ?????????? PDF-??????? ????????? 

                    PdfWriter oWriter = PdfWriter.GetInstance(pd, file); 

                    oWriter.PageEvent = new PdfEventHelper(this); 

 

 

                    // ????????? ??????? 

                    pd.SetMargins( 

                        reportConfig.Margin.Left, reportConfig.Margin.Right, 

                        reportConfig.Margin.Top + 40.0f, reportConfig.Margin.Bottom); 

 

 

                    // ??????? ????? 

                    BaseFont oBaseFnt = BaseFont.CreateFont(reportConfig.Font.Name, FONT_CODEPAGE, true); 

                    // ?????? ?????? ?? ????????? 

                    m_nDefaultFontSize = reportConfig.Font.Size; 

                    // ??????? 

                    m_font = new Font(oBaseFnt, m_nDefaultFontSize, Font.NORMAL); 

 

 


                    // ?? ???????? ?????????, ????? ???????????? ???????????? ?????????: 

                    if (Headers.ContainsKey(Voting.Section.PageHeader)) 

                    { 

                        HeaderFooter head = new HeaderFooter(MakeParagraph(Headers[Voting.Section.PageHeader]), false); 

                        head.Border = Rectangle.NO_BORDER; //????????? ????? 

                        pd.Header = head; 

                    } 

 

 

                    // ?? ???????? ?????????, ????? ???????????? ???????????? ???????: 

                    if (Headers.ContainsKey(Voting.Section.PageFooter)) 

                    { 

                        Paragraph footParagraph = MakeParagraph(Headers[Voting.Section.PageFooter]); 

 

 

                        // ????????? ???????????? ?????? ? ????????? 

                        if (PageNumbered) 

                        { 

                            footParagraph.Add(new Phrase(Chunk.NEWLINE)); 

                            footParagraph.Add(new Phrase(Chunk.NEWLINE)); 

                        } 

 

 

                        HeaderFooter foot = new HeaderFooter(footParagraph, false); 

                        foot.Border = Rectangle.NO_BORDER; //????????? ????? 

                        pd.Footer = foot; 

                    } 

 

 

                    // ??????? ???????? 

                    pd.Open(); 

 

 

                    // ?????? ??? ????????? ? ???????? 

                    if (Headers.ContainsKey(Voting.Section.Header)) 

                    { 

                        pd.Add(MakeParagraph(Headers[Voting.Section.Header])); 

                    } 

 

 

                    // ?????? ??? ??????? 

                    for (int i = 0; i < (m_data != null ? m_data.Tables.Count : 0); i++) 

                    { 

                        string tableName = i.ToString(); 

                        if (m_data.Tables.Contains(tableName) && m_data.Tables[tableName] != null) 

                        { 

                            // ?????????? ????? ??????? ? ??????? ? ??????? 

                            int nServiceColumnCount = 0; 

                            for (int j = 0; j < m_data.Tables[tableName].Columns.Count; j++) 

                            { 


                                if (m_data.Tables[tableName].Columns[j].ColumnName.StartsWith(ServiceTableColumns.SERVICE_COLUMN_PREFIX)) 

                                { 

                                    nServiceColumnCount++; 

                                } 

                            } 

 

 

                            // ?????????? ???????? ? ??????? 

                            int nDataColumns = m_data.Tables[tableName].Columns.Count - nServiceColumnCount; 

 

 

                            if (m_data.Tables[tableName].Rows.Count > 0) 

                            { 

                                // ????????? ???????: ????????? ??????????: 

                                Table tbl = MakeTable(nDataColumns, m_data.Tables[tableName].Rows.Count); 

 

 

                                // ??????? ??????????????? ??? ?????? ? ??????? 

                                for (int k = 0; k < m_data.Tables[tableName].Rows.Count; k++) 

                                { 

                                    // ???? ?????? ???????? ???????????? 

                                    if ((bool)m_data.Tables[tableName].Rows[k][ServiceTableColumns.NewPage]) 

                                    { 

                                        // ????????? ??????? ? ???????? 

                                        pd.Add(tbl); 

                                        // ????????? ????? ???????? 

                                        pd.NewPage(); 

                                        // ?????? ????? ??????? 

                                        tbl = MakeTable(nDataColumns, m_data.Tables[tableName].Rows.Count); 

                                    } 

 

 

                                    // ??????? ??????? ??? ???????? ????? ? ????? ??????? 

                                    string[] str = new string[nDataColumns]; 

                                    float[] cw = new float[nDataColumns]; 

 

 

                                    // ?????? ?????? ? ?????? 

                                    int nFontSize = 0; 

                                    // ??????? ??????? ?????? 

                                    bool bBold = false; 

                                    // ??????? ?????????? ?????? 

                                    bool bItalic = false; 

 

 

                                    // ??????? ??? ??????? ? ?????? 

                                    for (int j = 0; j < m_data.Tables[tableName].Columns.Count; j++) 

                                    { 

                                        switch (m_data.Tables[tableName].Columns[j].ColumnName) 

                                        { 


                                            case ServiceTableColumns.FontSize: 

                                                nFontSize = (int)(m_data.Tables[tableName].Rows[k].ItemArray[j]); 

                                                break; 

                                            case ServiceTableColumns.IsBold: 

                                                bBold = (bool)(m_data.Tables[tableName].Rows[k].ItemArray[j]); 

                                                break; 

                                            case ServiceTableColumns.IsItalic: 

                                                bItalic = (bool)(m_data.Tables[tableName].Rows[k].ItemArray[j]); 

                                                break; 

                                            default: 

                                                if (!m_data.Tables[tableName].Columns[j].ColumnName.StartsWith(ServiceTableColumns.SERVICE_COLUMN_PREFIX)) 

                                                { 

                                                    // ????????? ???????? ? ??????? 

                                                    str[j] = m_data.Tables[tableName].Rows[k].ItemArray[j].ToString(); 

                                                    if (m_data.Tables["C" + i] != null) 

                                                    { 

                                                        DataRow[] widths = m_data.Tables["C" + i].Select(ServiceTableColumns.Name + " = '" +

m_data.Tables[tableName].Columns[j].ColumnName + "'"); 

                                                        if (widths.Length > 0) 

                                                        { 

                                                            cw[j] = (int)widths[0][ServiceTableColumns.Width]; 

                                                        } 

                                                    } 

                                                } 

                                                break; 

                                        } 

                                    } 

 

 

                                    Font fTempFont = GetFont(nFontSize, bBold, bItalic, oBaseFnt); 

 

 

                                    tbl.Widths = cw; 

                                    for (int j = 0; j < nDataColumns; j++) 

                                    { 

                                        float ftCurentCellWidth = ftTableWidth * cw[j]; 

 

 

                                        if (k != 0 && str[j].Length > 0 && j != nDataColumns - 1 && TableDotted) 

                                        { 

                                            while (oBaseFnt.GetWidthPointKerned(str[j], m_font.Size) < ftCurentCellWidth) 

                                                str[j] += "."; 

                                        } 

 

 

                                        // TODO: ?????? ????? - ???? ??? ?????? ??????, ?? ?????? ?? ???????? 

                                        // TODO: if (str[j] == null || str[j].Trim().Length <= 0) {} 

                                        Cell cell = new Cell(new Phrase((float)Math.Round(fTempFont.Size * dblLeading_font), str[j], fTempFont)); 

                                        cell.Border = Rectangle.NO_BORDER; //??????? ????? 

                                        cell.Leading = (float)(fTempFont.Size * dblLeading_font); 


                                        tbl.AddCell(cell); 

                                    } 

                                } 

                                pd.Add(tbl); 

                            } 

                        } 

                    } 

 

 

                    // ?????? ??? ??????? ? ???????? 

                    if (Headers.ContainsKey(Voting.Section.Footer)) 

                    { 

                        // ??????? ??????? ??? ??????? 

                        for (int l = 0; l < cEmptyLines; l++) 

                        { 

                            pd.Add(new Phrase(Chunk.NEWLINE)); 

                        } 

 

 

                        pd.Add(MakeParagraph(Headers[Voting.Section.Footer])); 

                    } 

 

 

                    // ????????? ???? 

                    pd.Close(); 

 

 

					return new PrinterJob(file.Name, m_nTotalPagesCount); 

                } 

                catch (Exception ex) 

                { 

                    Managers.PrintingManager.Logger.LogException(Message.PrintingPdfBuildFailed, ex); 

                } 

            } 

 

 

            return null; 

        } 

 

 

        /// <summary> 

        /// ????????? ???????? ??? ????????? ??? ??????? 

        /// </summary> 

        /// <param name="lines">????? ?????</param> 

        /// <returns>?????????????? ????????</returns> 

        private Paragraph MakeParagraph(Lines lines) 

        { 

            Paragraph paragraph = new Paragraph(); 

            paragraph.KeepTogether = true; 

            for (int k = 0; k < lines.Count; k++) 


            { 

                if (lines[k].IsPrintable) 

                { 

                    // ????????? ??????? ? ???????????? ?????????: 

                    if (paragraph.Chunks.Count > 0) 

                    { 

                        //???? ??? ?? ?????? ??????????: 

                        paragraph.Add(new Phrase(Chunk.NEWLINE)); 

                    } 

 

 

                    ReportLine line = (ReportLine)lines[k]; 

                    if (line.FirstLine.Trim().Length > 0) 

                    { 

                        // ???????? ????? 

                        Font fTempFont = GetFont(line, m_font.BaseFont); 

 

 

                        //????????? ???????????? 

                        paragraph.Add(new Phrase((float)Math.Round(fTempFont.Size * dblLeading_font), TextAlign(line.FirstLine, fTempFont, line.Align), fTempFont)); 

                    } 

                    else 

                    { 

                        paragraph.Add(new Phrase(Chunk.NEWLINE)); 

                    } 

                } 

            } 

 

 

            paragraph.SpacingAfter = 0.0f; 

            paragraph.SpacingBefore = 0.0f; 

            paragraph.Leading = (float)(m_font.Size * dblLeading_font); 

 

 

            return paragraph; 

        } 

 

 

        /// <summary> 

        /// ??????? ??????? 

        /// </summary> 

        /// <param name="nDataColums">?????????? ????????</param> 

        /// <param name="nRows">?????????? ?????</param> 

        /// <returns>???????</returns> 

        private static Table MakeTable(int nDataColums, int nRows) 

        { 

            Table tbl = new Table(nDataColums, nRows); 

            //???????? ???????: 

            tbl.WidthPercentage = 100.0f; //?? ?????? - 100% 

            tbl.Cellpadding = 0.0f; //?????? - 1%. 


            tbl.Border = Rectangle.NO_BORDER; //??????? ????? 

            tbl.CellsFitPage = true; //?????? ?? ????????? 

            tbl.SpaceInsideCell = 0.0f; 

            tbl.Spacing = 0.0f; 

            tbl.SpaceInsideCell = 0.0f; 

            return tbl; 

        } 

 

 

        /// <summary> 

        /// ????????? ?????? ????????? ?? ???????????? ?? ??????? ???? 

        /// </summary> 

        /// <param name="sText">????? ??? ????????????</param> 

        /// <param name="font">?????</param> 

        /// <param name="lineAlign">????????????</param> 

        /// <returns>?????? ??????? ?????????</returns> 

        private string TextAlign(string sText, Font font, LineAlign lineAlign) 

        { 

            if (lineAlign != LineAlign.Left) 

            { 

                // ????????? ??????? ???????? ??????????? ?????? ?? ??????? ??????? 

                float ftPageAlign = pd.Right - pd.Left - pd.LeftMargin - pd.RightMargin; 

                if (lineAlign == LineAlign.Center) 

                { 

                    ftPageAlign = (ftPageAlign + font.BaseFont.GetWidthPointKerned(sText, font.Size)) / 2; 

                } 

 

 

                while (font.BaseFont.GetWidthPointKerned(sText, font.Size) < ftPageAlign) 

                { 

                    sText = " " + sText; 

                } 

            } 

            return sText; 

        } 

 

 

        /// <summary> 

        /// ?????? ????? ??? ?????? ????????? ?? ???????? ?????? 

        /// </summary> 

        /// <param name="oRepLine">?????? ?????????</param> 

        /// <param name="oBaseFont">??????? ?????</param>		 

        /// <returns>?????</returns> 

        private Font GetFont(ReportLine oRepLine, BaseFont oBaseFont) 

        { 

            return GetFont(oRepLine.FontSize, oRepLine.Bold, oRepLine.Italic, oBaseFont); 

        } 

 

 

        /// <summary> 


        /// ?????? ????? ??? ?????? ????????? ?? ???????? ?????? 

        /// </summary> 

        /// <param name="nFontSize">?????? ??????</param> 

        /// <param name="bBold">??????? ??????? ??????</param>		 

        /// <param name="bItalic">??????? ???????</param> 

        /// <param name="oBaseFont">??????? ?????</param> 

        /// <returns>?????</returns> 

        private Font GetFont(int nFontSize, bool bBold, bool bItalic, BaseFont oBaseFont) 

        { 

            // ?????? ?????? 

            nFontSize = (nFontSize > 0) ? nFontSize : m_nDefaultFontSize; 

 

 

            // ????? ?????? 

            int nFontStyle; 

            if (bBold && bItalic) 

                nFontStyle = Font.BOLDITALIC; 

            else if (bBold && (!bItalic)) 

                nFontStyle = Font.BOLD; 

            else if ((!bBold) && bItalic) 

                nFontStyle = Font.ITALIC; 

            else 

                nFontStyle = Font.NORMAL; 

 

 

            return new Font(oBaseFont, nFontSize, nFontStyle); 

        } 

 

 

        /// <summary> 

        /// ??????????????? ????? ??? ?????? ??????? ??????? 

        /// </summary> 

        internal class PdfEventHelper : IPdfPageEvent 

        { 

            private PdfReportBuilder reportPrint; 

            public PdfEventHelper(PdfReportBuilder aReportPrint) 

            { 

                reportPrint = aReportPrint; 

            } 

 

 

            #region IPdfPageEvent Members 

 

 

            public void OnOpenDocument(PdfWriter writer, Document document) 

            { 

            } 

 

 

            public void OnCloseDocument(PdfWriter writer, Document document) 


            { 

            } 

 

 

            public void OnParagraph(PdfWriter writer, Document document, float paragraphPosition) 

            { 

            } 

 

 

            public void OnEndPage(PdfWriter writer, Document document) 

            { 

                if (reportPrint.PageNumbered) 

                { 

                    // ????????? ????????? ??????? 

                    int nPageNumber = reportPrint.m_nTotalPagesCount + 1; 

                    PdfContentByte cb = writer.DirectContent; 

                    cb.BeginText(); 

                    cb.SetFontAndSize(reportPrint.m_font.BaseFont, reportPrint.m_font.Size); 

                    cb.SetTextMatrix(document.Left, document.Bottom); 

                    cb.ShowText(reportPrint.TextAlign("???? " + nPageNumber, reportPrint.m_font, LineAlign.Center)); 

                    cb.EndText(); 

                } 

                reportPrint.m_nTotalPagesCount++; 

            } 

 

 

            public void OnSection(PdfWriter writer, Document document, float paragraphPosition, int depth, Paragraph title) 

            { 

            } 

 

 

            public void OnSectionEnd(PdfWriter writer, Document document, float paragraphPosition) 

            { 

            } 

 

 

            public void OnParagraphEnd(PdfWriter writer, Document document, float paragraphPosition) 

            { 

            } 

 

 

            public void OnGenericTag(PdfWriter writer, Document document, Rectangle rect, string text) 

            { 

            } 

 

 

            public void OnChapterEnd(PdfWriter writer, Document document, float paragraphPosition) 

            { 

            } 

 


 
            public void OnChapter(PdfWriter writer, Document document, float paragraphPosition, Paragraph title) 

            { 

            } 

 

 

            public void OnStartPage(PdfWriter writer, Document document) 

            { 

            } 

 

 

            #endregion 

        } 

    } 

}


