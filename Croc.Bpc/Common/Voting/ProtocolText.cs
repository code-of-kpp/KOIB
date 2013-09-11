using System; 
using System.Text; 
using System.Xml.Serialization; 
namespace Croc.Bpc.Voting 
{ 
    [Serializable, XmlType("Text")] 
    public class ProtocolText 
    { 
        [XmlArray("Table")] 
        [XmlArrayItem("VoteLine")] 
        public VoteTextLine[] VoteLines = new VoteTextLine[0]; 
        [XmlArray("ProtocolLines")] 
        [XmlArrayItem("ProtocolLine")] 
        public ProtocolTextLine[] ProtocolLines = new ProtocolTextLine[0]; 
        private bool _final; 
        [XmlAttribute("final")] 
        public bool Final 
        { 
            get { return _final; } 
            set { _final = value; } 
        } 
        public override string ToString() 
        { 
            StringBuilder sText = new StringBuilder(); 
            sText.Append('['); 
            if (ProtocolLines != null) 
            { 
                sText.Append("ProtocolLines=["); 
                foreach (ProtocolTextLine oLine in ProtocolLines) 
                { 
                    sText.Append(oLine); 
                    sText.Append(';'); 
                } 
                sText.Append(']'); 
            } 
            if (VoteLines != null) 
            { 
                sText.Append("VoteLines=["); 
                foreach (VoteTextLine oVoteLine in VoteLines) 
                { 
                    sText.Append(oVoteLine); 
                    sText.Append(';'); 
                } 
                sText.Append("];"); 
            } 
            sText.Append("m_bFinal="); 
            sText.Append(_final); 
            sText.Append(";]"); 
            return sText.ToString(); 
        } 
    } 
}
