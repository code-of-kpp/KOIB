using System.Xml.Serialization; 
namespace Croc.Bpc.Voting 
{ 
    [XmlType("Mode")] 
    public enum VotingMode 
    { 
        None = 0, 
        Ahead, 
        Test, 
        Main, 
        Portable, 
        Results, 
    } 
}
