using System; 
using System.Xml.Serialization; 
namespace Croc.Bpc.Voting 
{ 
    public enum VoteLineType 
    { 
        Vote, 
        Line, 
        Delimiter, 
    } 
    [Serializable, XmlType("Vote")] 
    public class VoteTextLine 
    { 
        public const string TOTAL_RECEIVED_VOTETEXTLINE_ID = "0"; 
        public const string TOTAL_RECEIVED_VOTETEXTLINE_DEFAULT_TEXT = "Всего принято бюллетеней"; 
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
        [XmlAttribute("fontSize")] 
        public int FontSize 
        { 
            get; 
            set; 
        } 
        [XmlAttribute("ID")] 
        public string ID; 
        [XmlAttribute("type")] 
        public VoteLineType Type; 
        [XmlText] 
        public string Text; 
        public override string ToString() 
        { 
            return "[m_bBold=" + Bold.ToString() + ";" + 
                    "m_bItalic=" + Italic.ToString() + ";" + 
                    "m_nFontSize=" + FontSize.ToString() + ";" + 
                    "m_sID=" + ID + ";]" + 
                    "Text=" + Text + "]"; 
        } 
    } 
}
