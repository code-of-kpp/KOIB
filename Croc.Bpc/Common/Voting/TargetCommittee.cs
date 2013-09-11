using System; 
using System.Xml.Serialization; 
namespace Croc.Bpc.Voting 
{ 
    [Serializable] 
    public class TargetCommittee 
    { 
        [XmlAttribute("num")] 
        public int Num; 
        [XmlAttribute("name")] 
        public string Name; 
        public override string ToString() 
        { 
            return "[Num=" + Num + ",Name=" + Name + "]"; 
        } 
    } 
}
