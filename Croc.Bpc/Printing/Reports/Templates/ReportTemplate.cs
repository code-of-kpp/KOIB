using System; 
using System.Collections.Generic; 
using System.Collections.Specialized; 
using System.Data; 
using System.IO; 
using System.Xml; 
using System.Xml.Serialization; 
using Croc.Bpc.Diagnostics; 
using Croc.Core.Diagnostics; 
namespace Croc.Bpc.Printing.Reports.Templates 
{ 
    [Serializable, XmlType("ReportTemplate", Namespace = "http://localhost/Schemas/SIB2003/ReportTemplate")] 
    public class ReportTemplate 
    { 
        private static readonly ReportTemplateParser s_parser = new ReportTemplateParser(); 
        [XmlArray("Parameters")] 
        [XmlArrayItem("Parameter", typeof(string))] 
        public string[] Parameters; 
        [XmlArrayItem("Line", typeof(LineClause))] 
        [XmlArrayItem("For", typeof(ForClause))] 
        [XmlArrayItem("If", typeof(IfClause))] 
        [XmlArrayItem("SetCurrentRow", typeof(SetCurrentRow))] 
        public BasePlainElement[] Header = new BasePlainElement[0]; 
        [XmlArrayItem("Line", typeof(LineClause))] 
        [XmlArrayItem("For", typeof(ForClause))] 
        [XmlArrayItem("If", typeof(IfClause))] 
        [XmlArrayItem("SetCurrentRow", typeof(SetCurrentRow))] 
        public BasePlainElement[] PageHeader = new BasePlainElement[0]; 
        [XmlArrayItem("Line", typeof(LineClause))] 
        [XmlArrayItem("For", typeof(ForClause))] 
        [XmlArrayItem("If", typeof(IfClause))] 
        [XmlArrayItem("SetCurrentRow", typeof(SetCurrentRow))] 
        public BasePlainElement[] PageFooter = new BasePlainElement[0]; 
        [XmlArrayItem("Line", typeof(LineClause))] 
        [XmlArrayItem("For", typeof(ForClause))] 
        [XmlArrayItem("If", typeof(IfClause))] 
        [XmlArrayItem("SetCurrentRow", typeof(SetCurrentRow))] 
        public BasePlainElement[] Footer = new BasePlainElement[0]; 
        [XmlArrayItem("Table", typeof(Table))] 
        [XmlArrayItem("TableFor", typeof(TableFor))] 
        public BaseTableHolder[] Body; 
        [XmlAttribute("name")] 
        public string Name; 
        [XmlAttribute("claspFooter")] 
        public bool ClaspFooter = true; 
        [XmlAttribute("pageNumbered")] 
        public bool PageNumbered = true; 
        [XmlAttribute("font")] 
        public FontType Font = FontType.ArialNarrow; 
        [XmlAttribute("font-size")] 
        public int FontSize = 9; 
        [XmlIgnore] 
        public int[] Margins 
        { 
            get 
            { 
                var reportConfig = Managers.PrintingManager.ReportConfig; 
                if (LeftMargin < 0) 
                    LeftMargin = reportConfig.Margin.Left; 
                if (RightMargin < 0) 
                    RightMargin = reportConfig.Margin.Right; 
                if (TopMargin < 0) 
                    TopMargin = reportConfig.Margin.Top; 
                if (BottomMargin < 0) 
                    BottomMargin = reportConfig.Margin.Bottom; 
                return new[] {LeftMargin, RightMargin, TopMargin, BottomMargin}; 
            } 
        } 
        [XmlAttribute("margin-left")] 
        public int LeftMargin = -1; 
        [XmlAttribute("margin-top")] 
        public int TopMargin = -1; 
        [XmlAttribute("margin-right")] 
        public int RightMargin = -1; 
        [XmlAttribute("margin-bottom")] 
        public int BottomMargin = -1; 
        public static ReportTemplate LoadTemplate(ReportType reportType, ILogger logger) 
        { 
            logger.LogVerbose(Message.Common_DebugCall); 
            const string REPORTTEMPLATE_FILEPATH_FORMAT = "./Data/Templates/{0}.xml"; 
            const string REPORTTEMPLATE_SCHEMA_FILEPATH = "./Data/Schemas/ReportTemplate.xsd"; 
            const string REPORTTEMPLATE_SCHEMA_URL = "http://localhost/Schemas/SIB2003/ReportTemplate"; 
            var templatePath = new FileInfo(string.Format(REPORTTEMPLATE_FILEPATH_FORMAT, reportType)); 
            if (!templatePath.Exists) 
            { 
                logger.LogError(Message.PrintingReportTemplateNotFound, reportType); 
                throw new Exception("Не найден шаблон: " + reportType); 
            } 
            try 
            { 
                var doc = new XmlDocument(); 
                doc.Load(templatePath.FullName); 
                var xmlSettings = new XmlReaderSettings(); 
                xmlSettings.Schemas.Add(REPORTTEMPLATE_SCHEMA_URL, REPORTTEMPLATE_SCHEMA_FILEPATH); 
                xmlSettings.ValidationEventHandler += 
                    (sender, args) => logger.LogError( 
                        Message.PrintingReportTemplateValidationError, 
                        args.Exception, 
                        reportType); 
                var xmlTextReader = new XmlTextReader(doc.InnerXml, XmlNodeType.Document, null); 
                var xmlReader = XmlReader.Create(xmlTextReader, xmlSettings); 
                var oSerializer = new XmlSerializer(typeof(ReportTemplate), REPORTTEMPLATE_SCHEMA_URL); 
                logger.LogVerbose(Message.Common_DebugReturn); 
                return (ReportTemplate)oSerializer.Deserialize(xmlReader); 
            } 
            catch (Exception ex) 
            { 
                logger.LogError(Message.PrintingLoadReportTemplateFailed, ex, reportType); 
                throw; 
            } 
        } 
        public void LoadParameters(ListDictionary reportParameters) 
        { 
            if (Parameters != null) 
            { 
                foreach (string parameter in Parameters) 
                { 
                    if (reportParameters.Contains(parameter)) 
                    { 
                        s_parser.AddParameter(parameter, reportParameters[parameter]); 
                    } 
                } 
            } 
        } 
        public DataSet PrepareTable() 
        { 
            if (Body == null) 
            { 
                return null; 
            } 
            var logger = Managers.PrintingManager.Logger; 
            logger.LogVerbose(Message.Common_DebugCall); 
            var ds = new DataSet(); 
            var bodyTables = new List<Table>(); 
            foreach (var baseTableHolder in Body) 
            { 
                bodyTables.AddRange(baseTableHolder.GetTables(s_parser)); 
            } 
            int tableNumber = 0; 
            foreach (var bodyTable in bodyTables) 
            { 
                DataTable data = ds.Tables.Add(tableNumber.ToString()); 
                DataTable props = ds.Tables.Add("C" + tableNumber); 
                props.Columns.Add(ServiceTableColumns.Name, typeof(string)); 
                props.Columns.Add(ServiceTableColumns.Width, typeof(int)); 
                props.Columns.Add(ServiceTableColumns.FontSize, typeof(string)); 
                props.Columns.Add(ServiceTableColumns.IsBold, typeof(bool)); 
                props.Columns.Add(ServiceTableColumns.IsItalic, typeof(bool)); 
                props.Columns.Add(ServiceTableColumns.Align, typeof(LineAlign)); 
                int dataColumnCount = 0; // количество столбцов данных 
                int overallWidth = 0; // общая ширина 
                int zeroedCount = 0; // количество столбцов с неуказанной шириной 
                int widthCorrection = 0; // флаг (и размер) необходимости коррекции ширин если превысили 100% 
                int zeroedWidth = 0; // размер, рассчитанный для столбцов с неуказанной шириной 
                foreach (ColDefinition prop in bodyTable.Columns) 
                { 
                    int colCount = 1; 
                    if (prop.Count != null) 
                    { 
                        if (!Int32.TryParse(s_parser.GetVariable(prop.Count).ToString(), out colCount)) 
                        { 
                            colCount = 1; 
                        } 
                    } 
                    if (prop.Width > 0) 
                    { 
                        overallWidth += (prop.Width * colCount); 
                    } 
                    else 
                    { 
                        zeroedCount += 1 * colCount; 
                    } 
                } 
                if (zeroedCount > 0) 
                { 
                    zeroedWidth = (int)((100.0 - overallWidth) / zeroedCount); 
                } 
                if (overallWidth > 100) 
                { 
                    widthCorrection = (int)(100.0 / bodyTable.Columns.Length); 
                } 
                foreach (ColDefinition prop in bodyTable.Columns) 
                { 
                    int colCount = 1; 
                    if (prop.Count != null) 
                    { 
                        if (!Int32.TryParse(s_parser.GetVariable(prop.Count).ToString(), out colCount)) 
                        { 
                            colCount = 1; 
                        } 
                    } 
                    for (int i = 0; i < colCount; i++) 
                    { 
                        DataRow dr = props.NewRow(); 
                        dr[ServiceTableColumns.Name] = "S" + dataColumnCount; 
                        if (widthCorrection > 0) 
                        { 
                            dr[ServiceTableColumns.Width] = widthCorrection; 
                        } 
                        else 
                        { 
                            if (prop.Width > 0) 
                            { 
                                dr[ServiceTableColumns.Width] = prop.Width; 
                            } 
                            else 
                            { 
                                dr[ServiceTableColumns.Width] = zeroedWidth; 
                            } 
                        } 
                        dr[ServiceTableColumns.FontSize] = prop.FontSize; 
                        dr[ServiceTableColumns.IsBold] = prop.IsBold; 
                        dr[ServiceTableColumns.IsItalic] = prop.IsItalic; 
                        if(prop.Align.HasValue) 
                        { 
                            dr[ServiceTableColumns.Align] = prop.Align; 
                        } 
                        else 
                        { 
                            dr[ServiceTableColumns.Align] = DBNull.Value; 
                        } 
                        props.Rows.Add(dr); 
                        data.Columns.Add("S" + dataColumnCount, typeof(string)); 
                        dataColumnCount++; 
                    } 
                } 
                data.Columns.Add(ServiceTableColumns.FontSize, typeof(int)); 
                data.Columns.Add(ServiceTableColumns.IsBold, typeof(bool)); 
                data.Columns.Add(ServiceTableColumns.IsItalic, typeof(bool)); 
                data.Columns.Add(ServiceTableColumns.ServiceMode, typeof(ServiceMode)); 
                data.Columns.Add(ServiceTableColumns.IsTableDotted, typeof(bool)); 
                data.Columns.Add(ServiceTableColumns.Align, typeof(LineAlign)); 
                Lines lines = bodyTable.Lines; 
                foreach (IReportElement line in lines) 
                { 
                    if (line.IsPrintable) 
                    { 
                        var repLine = (ReportLine)line; 
                        DataRow dr = data.NewRow(); 
                        for (int index = 0; index < dataColumnCount; index++) 
                        { 
                            dr["S" + index] = repLine.Lines.Length > index ? repLine.Lines[index] : " "; 
                        } 
                        dr[ServiceTableColumns.FontSize] = repLine.FontSize(FontSize); 
                        dr[ServiceTableColumns.IsBold] = repLine.Bold; 
                        dr[ServiceTableColumns.IsItalic] = repLine.Italic; 
                        dr[ServiceTableColumns.ServiceMode] = repLine.Mode; 
                        var lineDotted = repLine.IsLineDotted < 0 
                                             ? bodyTable.IsDotted 
                                             : Convert.ToBoolean(repLine.IsLineDotted); 
                        dr[ServiceTableColumns.IsTableDotted] = lineDotted; 
                        dr[ServiceTableColumns.Align] = repLine.Align; 
                        data.Rows.Add(dr); 
                    } 
                } 
                tableNumber++; 
            } 
            logger.LogVerbose(Message.Common_DebugReturn); 
            return ds; 
        } 
        public static Lines ConstructHeader(BasePlainElement[] headerBody) 
        { 
            var logger = Managers.PrintingManager.Logger; 
            logger.LogVerbose(Message.Common_DebugCall); 
            var header = new Lines(); 
            if(headerBody != null) 
            { 
                foreach (BasePlainElement element in headerBody) 
                { 
                    header.AddRange(element.ConstructContent(s_parser)); 
                } 
                int rowIndex = 0; 
                foreach (IReportElement t in header) 
                { 
                    rowIndex++; 
                    if (t.IsPrintable) 
                    { 
                        ((ReportLine) t).TransformLine( 
                            str => str.Replace(ReportTemplateParser.MACRO_CURRENT_ROW, rowIndex.ToString())); 
                    } 
                    else 
                    { 
                        if(t is ServiceLine) 
                        { 
                            rowIndex = ((ServiceLine) t).CurrentRow; 
                        } 
                    } 
                } 
            } 
            logger.LogVerbose(Message.Common_DebugReturn); 
            return header; 
        } 
    } 
}
