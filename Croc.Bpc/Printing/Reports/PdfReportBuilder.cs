using System; 
using System.Collections.Generic; 
using System.Data; 
using System.IO; 
using System.Linq; 
using System.Text; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.FileSystem; 
using Croc.Bpc.Printing.Reports.Templates; 
using Croc.Bpc.Utils; 
using Croc.Core.Utils.IO; 
using iTextSharp.text; 
using iTextSharp.text.pdf; 
using Table = iTextSharp.text.Table; 
namespace Croc.Bpc.Printing.Reports 
{ 
    public class PdfReportBuilder 
    { 
        private int _totalPagesCount; 
        private int _currentPageNumber; 
        private Font _font; 
        private const float DBL_LEADING_FONT = 1.2f; 
        private const string FONT_CODEPAGE = "CP1251"; 
        private const string FONT_FILE_EXTENSION = ".ttf"; 
        private int _defaultFontSize; 
        private Document _pdfDocument; 
        public bool PageNumbered = true; 
        public bool TableDotted; 
        public FontType TemplateFont; 
        public int FontSize 
        { 
            get { return _defaultFontSize; } 
            set { _defaultFontSize = value; } 
        } 
        public int[] Margins; 
        public Dictionary<PageSection, Lines> Headers = new Dictionary<PageSection, Lines>(); 
        public DataSet Data; 
        public bool ClaspFooter; 
        private static int Border 
        { 
            get 
            { 
                var reportConfig = Managers.PrintingManager.ReportConfig; 
                return reportConfig.DebugMode.Value ? Rectangle.BOX : Rectangle.NO_BORDER; 
            } 
        } 
        public PrinterJob Build(ReportType reportType, int copies) 
        { 
            var reportConfig = Managers.PrintingManager.ReportConfig; 
            var logger = Managers.PrintingManager.Logger; 
            logger.LogVerbose(Message.Common_DebugCall); 
            _totalPagesCount = 0; 
            _currentPageNumber = 0; 
            var fileName = FileUtils.GetUniqueName( 
                Managers.FileSystemManager.GetDataDirectoryPath(FileType.Report), 
                String.Format("{0}.{1:yyyyMMdd}.", "PrintJob", DateTime.Now), 
                "pdf", 
                6); 
            var filePath = Path.Combine(Managers.FileSystemManager.GetDataDirectoryPath(FileType.Report), fileName); 
            using (var stream = new MemoryStream()) 
            { 
                try 
                { 
                    _pdfDocument = new Document(PageSize.A4); 
                    _pdfDocument.ClaspFooter = ClaspFooter; 
                    PdfWriter writer = PdfWriter.GetInstance(_pdfDocument, stream); 
                    writer.PageEvent = new PdfEventHelper(this); 
                    _pdfDocument.SetMargins(Margins[0], Margins[1], Margins[2], Margins[3]); 
                    var fontFileName = Path.Combine(reportConfig.Font.Path, 
                                                    TemplateFont.ToString().ToLower() + FONT_FILE_EXTENSION); 
                    BaseFont baseFont = BaseFont.CreateFont(fontFileName, FONT_CODEPAGE, true); 
                    _font = new Font(baseFont, _defaultFontSize, Font.NORMAL); 
                    if (Headers.ContainsKey(PageSection.PageHeader) && Headers[PageSection.PageHeader].Count > 0) 
                    { 
                        logger.LogVerbose(Message.PrintingPdfBuilderStartEvent, "Создание PageHeader"); 
                        var head = new HeaderFooter(MakeParagraph(Headers[PageSection.PageHeader]), false); 
                        head.Border = Border; 
                        _pdfDocument.Header = head; 
                        logger.LogVerbose(Message.PrintingPdfBuilderEndEvent, "Создание PageHeader"); 
                    } 
                    if (Headers.ContainsKey(PageSection.PageFooter) && Headers[PageSection.PageFooter].Count > 0) 
                    { 
                        logger.LogVerbose(Message.PrintingPdfBuilderStartEvent, "Создание PageFooter"); 
                        Paragraph footParagraph = MakeParagraph(Headers[PageSection.PageFooter]); 
                        if (PageNumbered) 
                        { 
                            footParagraph.Add(new Phrase(Chunk.NEWLINE)); 
                            footParagraph.Add(new Phrase(Chunk.NEWLINE)); 
                        } 
                        var foot = new HeaderFooter(footParagraph, false); 
                        foot.Border = Border; 
                        _pdfDocument.Footer = foot; 
                        logger.LogVerbose(Message.PrintingPdfBuilderEndEvent, "Создание PageFooter"); 
                    } 
                    try 
                    { 
                        _pdfDocument.Open(); 
                    } 
                    catch (Exception ex) 
                    { 
                        throw new Exception("Ошибка формирования PDF", ex); 
                    } 
                    if (Headers.ContainsKey(PageSection.Header)) 
                    { 
                        logger.LogVerbose(Message.PrintingPdfBuilderStartEvent, "Создание Header"); 
                        _pdfDocument.Add(MakeParagraph(Headers[PageSection.Header])); 
                        logger.LogVerbose(Message.PrintingPdfBuilderEndEvent, "Создание Header"); 
                    } 
                    if (Data != null) 
                    { 
                        int dataTablesCount = Data.Tables.Cast<DataTable>().Count(table => !table.TableName.StartsWith("C")); 
                        for (int tableIndex = 0; tableIndex < dataTablesCount; tableIndex++) 
                        { 
                            float tableFactor = (_pdfDocument.Right - _pdfDocument.Left) / 100; 
                            string tableName = tableIndex.ToString(); 
                            logger.LogVerbose(Message.PrintingPdfBuilderStartEvent, "Создание Таблицы " + tableName); 
                            if (Data.Tables.Contains(tableName) && Data.Tables[tableName] != null) 
                            { 
                                var currentTable = Data.Tables[tableName]; 
                                int serviceColumnCount = 
                                    currentTable.Columns.Cast<DataColumn>().Count( 
                                        c => c.ColumnName.StartsWith(ServiceTableColumns.SERVICE_COLUMN_PREFIX)); 
                                int dataColumns = currentTable.Columns.Count - serviceColumnCount; 
                                if (currentTable.Rows.Count > 0) 
                                { 
                                    Table tbl = MakeTable(dataColumns, currentTable.Rows.Count); 
                                    foreach (DataRow row in currentTable.Rows) 
                                    { 
                                        if (((ServiceMode)row[ServiceTableColumns.ServiceMode] & 
                                             ServiceMode.ResetPageCounter) > 0) 
                                        { 
                                            _currentPageNumber = 0; 
                                        } 
                                        TableDotted = (bool)row[ServiceTableColumns.IsTableDotted]; 
                                        if (((ServiceMode)row[ServiceTableColumns.ServiceMode] & 
                                             ServiceMode.PageBreak) > 0) 
                                        { 
                                            if (row == currentTable.Rows.Cast<DataRow>().Last() && 
                                                !Data.Tables.Contains((tableIndex + 1).ToString())) 
                                                continue; 
                                            _pdfDocument.Add(tbl); 
                                            _pdfDocument.NewPage(); 
                                            tbl = MakeTable(dataColumns, currentTable.Rows.Count); 
                                        } 
                                        var colLines = new string[dataColumns]; 
                                        var colWidths = new float[dataColumns]; 
                                        var colAligns = new LineAlign?[dataColumns]; 
                                        int fontSize = _defaultFontSize; 
                                        bool bold = false; 
                                        bool italic = false; 
                                        LineAlign lineAlign = LineAlign.Left; 
                                        for (int columnIndex = 0; columnIndex < currentTable.Columns.Count; columnIndex++) 
                                        { 
                                            switch (currentTable.Columns[columnIndex].ColumnName) 
                                            { 
                                                case ServiceTableColumns.FontSize: 
                                                    fontSize = (int)(row.ItemArray[columnIndex]); 
                                                    break; 
                                                case ServiceTableColumns.IsBold: 
                                                    bold = (bool)(row.ItemArray[columnIndex]); 
                                                    break; 
                                                case ServiceTableColumns.IsItalic: 
                                                    italic = (bool)(row.ItemArray[columnIndex]); 
                                                    break; 
                                                case ServiceTableColumns.Align: 
                                                    lineAlign = (LineAlign)(row.ItemArray[columnIndex]); 
                                                    break; 
                                                default: 
                                                    if (!currentTable.Columns[columnIndex] 
                                                            .ColumnName.StartsWith(ServiceTableColumns.SERVICE_COLUMN_PREFIX)) 
                                                    { 
                                                        colLines[columnIndex] = row.ItemArray[columnIndex].ToString(); 
                                                        if (Data.Tables["C" + tableIndex] != null) 
                                                        { 
                                                            DataRow[] columnProps = Data.Tables["C" + tableIndex] 
                                                                .Select(ServiceTableColumns.Name + " = '" 
                                                                        + currentTable.Columns[columnIndex].ColumnName + "'"); 
                                                            if (columnProps.Length > 0) 
                                                            { 
                                                                colWidths[columnIndex] = (int)columnProps[0][ServiceTableColumns.Width]; 
                                                                if (columnProps[0][ServiceTableColumns.Align] != DBNull.Value) 
                                                                { 
                                                                    colAligns[columnIndex] = (LineAlign)columnProps[0][ServiceTableColumns.Align]; 
                                                                } 
                                                            } 
                                                        } 
                                                    } 
                                                    break; 
                                            } 
                                        } 
                                        Font tempFont = GetFont(fontSize, bold, italic, baseFont); 
                                        var cellLeading = (float)Math.Round(tempFont.Size * DBL_LEADING_FONT); 
                                        tbl.Widths = colWidths; 
                                        for (int columnIndex = 0; columnIndex < dataColumns; columnIndex++) 
                                        { 
                                            float cellWidth = tableFactor * colWidths[columnIndex]; 
                                            colLines[columnIndex] = TextAlign( 
                                                colLines[columnIndex], tempFont, colAligns[columnIndex] ?? lineAlign, 
                                                cellWidth, (row != currentTable.Rows.Cast<DataRow>().First() 
                                                            && TableDotted) 
                                                            ? '.' 
                                                            : ' '); 
                                            var cell = 
                                                new Cell(new Phrase(cellLeading, colLines[columnIndex], tempFont)) 
                                                    { 
                                                        Border = Border, 
                                                        Leading = cellLeading, 
                                                    }; 
                                            tbl.AddCell(cell); 
                                        } 
                                    } 
                                    logger.LogVerbose(Message.PrintingPdfBuilderEndEvent, "Создание Таблицы " + tableName); 
                                    logger.LogVerbose(Message.PrintingPdfBuilderStartEvent, "Добавление Таблицы " + tableName); 
                                    _pdfDocument.Add(tbl); 
                                    logger.LogVerbose(Message.PrintingPdfBuilderEndEvent, "Добавление Таблицы " + tableName); 
                                } 
                            } 
                        } 
                    } 
                    if (Headers.ContainsKey(PageSection.Footer) && Headers[PageSection.Footer].Count > 0) 
                    { 
                        logger.LogVerbose(Message.PrintingPdfBuilderStartEvent, "Создание Footer"); 
                        _pdfDocument.Add(new Phrase(Chunk.NEWLINE)); 
                        _pdfDocument.Add(MakeParagraph(Headers[PageSection.Footer])); 
                        logger.LogVerbose(Message.PrintingPdfBuilderEndEvent, "Создание Footer"); 
                    } 
                    _pdfDocument.Close(); 
                    var size = (stream.ToArray().Length / FileUtils.BYTES_IN_KB) + 1; 
                    if (!Managers.FileSystemManager.ReserveDiskSpace(filePath, size)) 
                    { 
                        throw new ApplicationException("Недостаточно места на диске для сохранения отчета"); 
                    } 
                    File.WriteAllBytes(filePath, stream.ToArray()); 
                    SystemHelper.SyncFileSystem(); 
                    logger.LogVerbose(Message.Common_DebugReturn); 
                    return new PrinterJob(reportType, filePath, _totalPagesCount, copies); 
                } 
                catch (Exception ex) 
                { 
                    Managers.PrintingManager.Logger.LogError(Message.PrintingPdfBuildFailed, ex); 
                } 
            } 
            return null; 
        } 
        private Paragraph MakeParagraph(Lines lines) 
        { 
            var logger = Managers.PrintingManager.Logger; 
            logger.LogVerbose(Message.Common_DebugCall); 
            var paragraph = new Paragraph(); 
            paragraph.KeepTogether = true; 
            foreach (IReportElement t in lines) 
            { 
                if (!t.IsPrintable) continue; 
                if (paragraph.Chunks.Count > 0) 
                { 
                    paragraph.Add(new Phrase(Chunk.NEWLINE)); 
                } 
                var line = (ReportLine)t; 
                if (line.FirstLine.Trim().Length > 0) 
                { 
                    Font tempFont = GetFont(line, _font.BaseFont); 
                    paragraph.Add(new Phrase( 
                                    (float)Math.Round(tempFont.Size * DBL_LEADING_FONT), 
                                    TextAlign(line.FirstLine, tempFont, line.Align, _pdfDocument.Right - _pdfDocument.Left, ' '), 
                                    tempFont)); 
                } 
                else 
                { 
                    paragraph.Add(new Phrase(Chunk.NEWLINE)); 
                } 
            } 
            paragraph.SpacingAfter = 0.0f; 
            paragraph.SpacingBefore = 0.0f; 
            paragraph.Leading = (float)Math.Round(_font.Size * DBL_LEADING_FONT); 
            logger.LogVerbose(Message.Common_DebugReturn); 
            return paragraph; 
        } 
        private static Table MakeTable(int dataColums, int rows) 
        { 
            var tbl = new Table(dataColums, rows); 
            tbl.WidthPercentage = 100; 
            tbl.Cellpadding = 0; 
            tbl.Border = Border; 
            tbl.CellsFitPage = true; 
            tbl.SpaceInsideCell = 0; 
            tbl.Spacing = 0; 
            tbl.SpaceInsideCell = 0; 
            return tbl; 
        } 
        private static string TextAlign(string text, Font font, LineAlign lineAlign, float areaWidth, char fillChar) 
        { 
            var alignedText = new StringBuilder(); 
            float fillCharWidth = font.BaseFont.GetWidthPoint(fillChar, font.Size); 
            float lineWidth = fillChar != ' ' ? 
                font.BaseFont.GetWidthPoint(text, font.Size) : 0; 
            if (lineAlign != LineAlign.Left) 
            { 
                float fillAreaWidth = areaWidth; 
                if (lineWidth == 0) 
                { 
                    lineWidth = font.BaseFont.GetWidthPoint(text, font.Size); 
                } 
                if (lineAlign == LineAlign.Center) 
                { 
                    fillAreaWidth = (fillAreaWidth + lineWidth) / 2; 
                } 
                if (lineWidth < fillAreaWidth) 
                { 
                    int fillCharCount = (int)((fillAreaWidth - lineWidth) / fillCharWidth); 
                    if (fillCharCount > 0) 
                    { 
                        alignedText.Append(fillChar, fillCharCount); 
                    } 
                    lineWidth += fillAreaWidth - lineWidth; 
                } 
                alignedText.Append(text); 
            } 
            if (lineWidth > 0 && fillChar != ' ') 
            { 
                if (alignedText.Length == 0) 
                { 
                    alignedText.Append(text); 
                } 
                if (lineWidth < areaWidth) 
                { 
                    int fillCharCount = (int)((areaWidth - lineWidth) / fillCharWidth); 
                    if (fillCharCount > 0) 
                    { 
                        alignedText.Append(fillChar, fillCharCount); 
                    } 
                } 
            } 
            return lineWidth > 0 ? alignedText.ToString() : text; 
        } 
        private Font GetFont(ReportLine oRepLine, BaseFont oBaseFont) 
        { 
            return GetFont(oRepLine.FontSize(_defaultFontSize), oRepLine.Bold, oRepLine.Italic, oBaseFont); 
        } 
        private Font GetFont(int fontSize, bool bold, bool italic, BaseFont baseFont) 
        { 
            fontSize = (fontSize > 0) ? fontSize : _defaultFontSize; 
            int fontStyle; 
            if (bold && italic) 
                fontStyle = Font.BOLDITALIC; 
            else if (bold) 
                fontStyle = Font.BOLD; 
            else if (italic) 
                fontStyle = Font.ITALIC; 
            else 
                fontStyle = Font.NORMAL; 
            return new Font(baseFont, fontSize, fontStyle); 
        } 
        internal class PdfEventHelper : IPdfPageEvent 
        { 
            private PdfReportBuilder _reportPrint; 
            public PdfEventHelper(PdfReportBuilder reportPrint) 
            { 
                _reportPrint = reportPrint; 
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
                if (_reportPrint.PageNumbered) 
                { 
                    _reportPrint._currentPageNumber++; 
                    int pageNumber = _reportPrint._currentPageNumber; 
                    PdfContentByte contentByte = writer.DirectContent; 
                    contentByte.BeginText(); 
                    contentByte.SetFontAndSize(_reportPrint._font.BaseFont, _reportPrint._font.Size); 
                    contentByte.SetTextMatrix(document.Left, document.Bottom); 
                    contentByte.ShowText( 
                        TextAlign("Лист " + pageNumber, _reportPrint._font, LineAlign.Center, document.Right - document.Left, ' ')); 
                    contentByte.EndText(); 
                } 
                _reportPrint._totalPagesCount++; 
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
