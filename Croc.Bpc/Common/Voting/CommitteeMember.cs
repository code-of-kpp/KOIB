using System; 
using System.Xml.Serialization; 
namespace Croc.Bpc.Voting 
{ 
    [Serializable, XmlType("Member")] 
    public class CommitteeMember 
    { 
        [XmlAttribute("lastName")] 
        public string LastName; 
        [XmlAttribute("firstName")] 
        public string FirstName; 
        [XmlAttribute("patronymic")] 
        public string Patronymic; 
        [XmlAttribute("type")] 
        public CommitteeMemberType Type; 
    } 
}
