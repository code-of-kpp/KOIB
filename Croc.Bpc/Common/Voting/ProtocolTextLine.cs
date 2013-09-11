using System; 
using System.Xml.Serialization; 
using Croc.Bpc.Printing; 
namespace Croc.Bpc.Voting 
{ 
    [Serializable, XmlType("ProtocolLine")] 
    public class ProtocolTextLine 
    { 
        [XmlAttribute("section")] 
        public PageSection Section; 
        [XmlAttribute("align")] 
        public LineAlign Align; 
        [XmlAttribute("bold")] 
        public bool Bold 
        { 
            get; 
            set; 
        } 
        [XmlAttribute("italic")] 
        public bool Italic 
        { 
            get; 
            set; 
        } 
        private int _fontSize = 8; 
        [XmlAttribute("fontSize")] 
        public int FontSize 
        { 
            get {return _fontSize;} 
            set {_fontSize = value;} 
        } 
        [XmlText] 
        public string Text; 
        public override string ToString() 
        { 
            return    "[Section=" + Section + ";" + 
                    "Align=" + Align + ";" + 
                    "Bold=" + Bold + ";" + 
                    "Italic=" + Italic + ";" + 
                    "FontSize=" + FontSize + ";" + 
                    "Text=" + Text + "]"; 
        } 
    } 
}
