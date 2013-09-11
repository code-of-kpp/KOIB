using System; 
using System.Xml.Serialization; 
namespace Croc.Bpc.Voting 
{ 
    [Serializable, XmlType("ParentComittee")] 
    public class ParentComittee 
    { 
        [XmlAttribute("parentid")] 
        public string ParentID; 
        [XmlAttribute("name")] 
        public string Name; 
        [XmlAttribute("e-mail")] 
        public string EMail; 
        public override string ToString() 
        { 
            return "[m_sParentID=" + ParentID + ",m_sName=" + Name + ",m_sEMail=" + EMail + "]"; 
        } 
    } 
}
