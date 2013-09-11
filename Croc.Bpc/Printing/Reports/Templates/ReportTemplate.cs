using System; 

using System.Collections; 

using System.Collections.Generic; 

using System.Collections.Specialized; 

using System.Data; 

using System.Diagnostics; 

using System.IO; 

using System.Text; 

using System.Xml; 

using System.Xml.Schema; 

using System.Xml.Serialization; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Core.Diagnostics; 

 

 

namespace Croc.Bpc.Printing.Reports.Templates 

{ 

    /// <summary> 

    /// ?????? ?????? ???? 

    /// </summary> 

    [Serializable, XmlType("ReportTemplate", Namespace = "http://localhost/Schemas/SIB2003/ReportTemplate")] 

    public class ReportTemplate 

    { 

        /// <summary> 

        /// ?????? ????????? 

        /// </summary> 

        ReportTemplateParser parser = new ReportTemplateParser(); 

 

 

        /// <summary> 

        /// ??????????? ??????? ????????? 

        /// </summary> 

        [XmlArray("Parameters")] 

        [XmlArrayItem("Parameter", typeof(string))] 

        public string[] Parameters; 

 

 

        /// <summary> 

        /// ????? ????????? ?????? 

        /// </summary> 

        [XmlArrayItem("Line", typeof(LineClause))] 

        [XmlArrayItem("For", typeof(ForClause))] 

        [XmlArrayItem("If", typeof(IfClause))] 

        [XmlArrayItem("SetCurrentRow", typeof(SetCurrentRow))] 

        public BasePlainElement[] Header = new BasePlainElement[0]; 

        /// <summary> 

        /// ???????????? ????????? ?????? 

        /// </summary> 

        [XmlArrayItem("Line", typeof(LineClause))] 

        [XmlArrayItem("For", typeof(ForClause))] 


        [XmlArrayItem("If", typeof(IfClause))] 

        [XmlArrayItem("SetCurrentRow", typeof(SetCurrentRow))] 

        public BasePlainElement[] PageHeader = new BasePlainElement[0]; 

        /// <summary> 

        /// ???????????? ?????? ?????? 

        /// </summary> 

        [XmlArrayItem("Line", typeof(LineClause))] 

        [XmlArrayItem("For", typeof(ForClause))] 

        [XmlArrayItem("If", typeof(IfClause))] 

        [XmlArrayItem("SetCurrentRow", typeof(SetCurrentRow))] 

        public BasePlainElement[] PageFooter = new BasePlainElement[0]; 

        /// <summary> 

        /// ????? ?????? ?????? 

        /// </summary> 

        [XmlArrayItem("Line", typeof(LineClause))] 

        [XmlArrayItem("For", typeof(ForClause))] 

        [XmlArrayItem("If", typeof(IfClause))] 

        [XmlArrayItem("SetCurrentRow", typeof(SetCurrentRow))] 

        public BasePlainElement[] Footer = new BasePlainElement[0]; 

 

 

        /// <summary> 

        /// ??????? ?????? 

        /// </summary> 

        [XmlElement("Table")] 

        public Table Table; 

 

 

        /// <summary> 

        /// ??? ?????? 

        /// </summary> 

        [XmlAttribute("name")] 

        public string Name; 

 

 

        /// <summary> 

        /// ??????? "??????? ?????? ? ?????? ??????" 

        /// </summary> 

        [XmlAttribute("claspFooter")] 

        public bool ClaspFooter; 

 

 

        /// <summary> 

        /// ??????? ????????? ??????? 

        /// </summary> 

        [XmlAttribute("pageNumbered")] 

        public bool PageNumbered; 

 

 

        /// <summary> 


        /// ???????? ??????? ?????? 

        /// TODO: ????????, ???????? ????? ?????????? ????? 

        /// </summary> 

        /// <param name="reportType">??? ??????</param> 

        /// <returns>?????? ??????</returns> 

        public static ReportTemplate LoadTemplate(ReportType reportType, ILogger logger) 

        { 

            const string REPORTTEMPLATE_FILEPATH_FORMAT = "./Data/Templates/{0}.xml"; 

            const string REPORTTEMPLATE_SCHEMA_FILEPATH = "./Data/Schemas/ReportTemplate.xsd"; 

            const string REPORTTEMPLATE_SCHEMA_URL = "http://localhost/Schemas/SIB2003/ReportTemplate"; 

 

 

            FileInfo templatePath = new FileInfo(string.Format(REPORTTEMPLATE_FILEPATH_FORMAT, reportType)); 

            if (!templatePath.Exists) 

            { 

                logger.LogError(Message.PrintingReportTemplateNotFound, reportType); 

                throw new Exception("?? ?????? ??????: " + reportType); 

            } 

 

 

            try 

            { 

                // ???????? ????? 

                XmlDocument doc = new XmlDocument(); 

                doc.Load(templatePath.FullName); 

 

 

                // ???????? ????????????? ?? ??????? ?? ?? ??? ??????????? 

                XmlTextReader xmlReader = new XmlTextReader(doc.InnerXml, XmlNodeType.Document, null); 

                XmlValidatingReader xmlValidReader = new XmlValidatingReader(xmlReader); 

                // ??????? ????? 

                XmlSchemaCollection xmlSchemas = new XmlSchemaCollection(); 

                xmlSchemas.Add(REPORTTEMPLATE_SCHEMA_URL, REPORTTEMPLATE_SCHEMA_FILEPATH); 

                xmlValidReader.Schemas.Add(xmlSchemas); 

 

 

                // ???????????? 

                XmlSerializer oSerializer = new XmlSerializer(typeof(ReportTemplate), REPORTTEMPLATE_SCHEMA_URL); 

 

 

                // ????????? ?????????? 

                xmlValidReader.ValidationEventHandler += new ValidationEventHandler( 

                    (sender, args) => 

                    { 

                        logger.LogException(Message.PrintingReportTemplateValidationError, args.Exception, reportType); 

                    } 

                ); 

 

 

                return (ReportTemplate)oSerializer.Deserialize(xmlValidReader); ; 


            } 

            catch (Exception ex) 

            { 

                logger.LogException(Message.PrintingLoadReportTemplateFailed, ex, reportType); 

                throw; 

            } 

        } 

 

 

        /// <summary> 

        /// ????????????? ??????? ?????????? ?????? 

        /// </summary> 

        /// <param name="reportParameters">????????? ??????</param> 

        public void LoadParameters(ListDictionary reportParameters) 

        { 

            if (Parameters != null) 

            { 

                foreach (string parameter in Parameters) 

                { 

                    if (reportParameters.Contains(parameter)) 

                    { 

                        parser.AddParameter(parameter, reportParameters[parameter]); 

                    } 

                } 

            } 

        } 

 

 

        /// <summary> 

        /// ????????? ?????????? ??????? ?????? 

        /// </summary> 

        /// <returns></returns> 

        public DataSet PrepareTable() 

        { 

            if (Table != null && Table.Columns != null && Table.Columns.Length > 0) 

            { 

                DataSet ds = new DataSet(); 

 

 

                // ??????? ?????? 

                DataTable data = ds.Tables.Add("0"); 

 

 

                // ??????? ????? ???????? 

                DataTable props = ds.Tables.Add("C0"); 

                props.Columns.Add(ServiceTableColumns.Name, typeof (string)); 

                props.Columns.Add(ServiceTableColumns.Width, typeof (int)); 

                props.Columns.Add(ServiceTableColumns.FontSize, typeof (int)); 

                props.Columns.Add(ServiceTableColumns.IsBold, typeof (bool)); 

                props.Columns.Add(ServiceTableColumns.IsItalic, typeof (bool)); 


                // TODO: ???????????? ??? ???????? 

 

 

                int dataColumnCount = 0; // ?????????? ???????? ?????? 

                int overallWidth = 0; // ????? ?????? 

                int zeroedCount = 0; // ?????????? ???????? ? ??????????? ??????? 

                int widthCorrection = 0; // ???? (? ??????) ????????????? ????????? ????? ???? ????????? 100% 

                int zeroedWidth = 0; // ??????, ???????????? ??? ???????? ? ??????????? ??????? 

 

 

                // ????????? ????? ?????? ????????? ????? ? ????? ??????? ????? 

                foreach (ColDefinition prop in Table.Columns) 

                { 

                    int colCount = 1; 

 

 

                    if(prop.Count != null) 

                    { 

                        if (!Int32.TryParse(parser.getVariable(prop.Count).ToString(), out colCount)) 

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

 

 

                if(zeroedCount > 0) 

                { 

                    // ???????? ?????? ??????? ? ??????????? ??????? 

                    zeroedWidth = (100 - overallWidth) / zeroedCount; 

                } 

 

 

                // ???? ????????? ?????? 100, ?? ???????????? ??? ??????? 

                if(overallWidth > 100) 

                { 

                    widthCorrection = 100 / Table.Columns.Length; 

                } 

 

 


                foreach (ColDefinition prop in Table.Columns) 

                { 

                    int colCount = 1; 

 

 

                    // ???? ??????? ????????? ??? ?????????? ?????????? ???????? 

                    if (prop.Count != null) 

                    { 

                        if (!Int32.TryParse(parser.getVariable(prop.Count).ToString(), out colCount)) 

                        { 

                            colCount = 1; 

                        } 

                    } 

 

 

                    for (int i = 0; i < colCount; i++) 

                    { 

                        DataRow dr = props.NewRow(); 

                        // TODO: ??? ???????? ?????????????? ?????? Width 

                        dr[ServiceTableColumns.Name] = "S" + dataColumnCount; 

                        if (widthCorrection > 0) 

                        { 

                            // ????? ?????? ????? ?????? 100% 

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

                        props.Rows.Add(dr); 

                        data.Columns.Add("S" + dataColumnCount, typeof (string)); 

                        dataColumnCount++; 

                    } 

                } 

 

 

                // ????????? ??????? 

                data.Columns.Add(ServiceTableColumns.FontSize, typeof(int)); 

                data.Columns.Add(ServiceTableColumns.IsBold, typeof(bool)); 

                data.Columns.Add(ServiceTableColumns.IsItalic, typeof(bool)); 


                data.Columns.Add(ServiceTableColumns.NewPage, typeof(bool)); 

                // TODO: ????????????, ?????????? ??????? ??? ????? 

 

 

                // ???????? ?????? 

                Lines lines = ConstructHeader(Table.Body); 

 

 

                foreach (IReportElement line in lines) 

                { 

                    if (line.IsPrintable) 

                    { 

                        DataRow dr = data.NewRow(); 

                        // TODO: ???????????? 

                        // ???????? ??????? ?????? 

                        for (int index = 0; index < dataColumnCount; index++) 

                        { 

                            // TODO: ????? ???????? ? ????? ????? ??????? 

                            dr["S" + index] = (line as ReportLine).Lines.Length > index ? (line as ReportLine).Lines[index] : " "; 

                        } 

                        dr[ServiceTableColumns.FontSize] = (line as ReportLine).FontSize; 

                        dr[ServiceTableColumns.IsBold] = (line as ReportLine).Bold; 

                        dr[ServiceTableColumns.IsItalic] = (line as ReportLine).Italic; 

                        dr[ServiceTableColumns.NewPage] = (line as ReportLine).NewPage; 

                        data.Rows.Add(dr); 

                    } 

                } 

 

 

                return ds; 

            } 

            else 

            { 

                return null; 

            } 

        } 

 

 

        /// <summary> 

        /// ???????????? ???? ????????? 

        /// </summary> 

        /// <param name="headerBody"></param> 

        /// <returns></returns> 

        public Lines ConstructHeader(BasePlainElement[] headerBody) 

        { 

            Lines header = new Lines(); 

 

 

            if(headerBody != null) 

            { 


                // ???????????? ????????? 

                foreach (BasePlainElement element in headerBody) 

                { 

                    header.AddRange(element.ConstructContent(parser)); 

                } 

 

 

                // ????????? ??????????? ??????? ????? 

                int rowIndex = 0; 

                for (int i = 0; i < header.Count; i++) 

                { 

                    rowIndex++; 

                    if (header[i].IsPrintable) 

                    { 

                        ((ReportLine)header[i]).TransformLine(str => str.Replace(ReportTemplateParser.MACRO_CURRENT_ROW, rowIndex.ToString())); 

                    } 

                    else 

                    { 

                        if(header[i] is ServiceLine) 

                        { 

                            rowIndex = (header[i] as ServiceLine).CurrentRow; 

                        } 

                    } 

                } 

            } 

 

 

            return header; 

        } 

    } 

}


