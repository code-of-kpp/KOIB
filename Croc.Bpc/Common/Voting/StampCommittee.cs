using System; 
using System.Xml.Serialization; 
namespace Croc.Bpc.Voting 
{ 
    [Serializable, XmlType("StampCommittee")] 
    public class StampCommittee 
    { 
        [XmlAttribute("num")] 
        public int Num; 
        public override string ToString() 
        { 
            return Num.ToString(); 
        } 
    } 
}
