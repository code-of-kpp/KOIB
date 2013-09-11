using System.IO; 
using System.Text; 
using System.Xml; 
namespace Croc.Core.Utils.Xml 
{ 
    public class PrettyPrintXmlWriter : XmlTextWriter 
    { 
        public PrettyPrintXmlWriter(Stream stream) 
            : base(stream, Encoding.Unicode) 
        { 
            Formatting = Formatting.Indented; 
            Indentation = 4; 
            QuoteChar = '\''; 
        } 
        public string ToFormatString() 
        { 
            Flush(); 
            BaseStream.Position = 0; 
            using (var streamReader = new StreamReader(BaseStream)) 
            { 
                var resXml = streamReader.ReadToEnd(); 
                return resXml; 
            } 
        } 
    } 
}
