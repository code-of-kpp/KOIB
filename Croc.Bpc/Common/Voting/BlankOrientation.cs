using System; 
using System.Xml.Serialization; 
namespace Croc.Bpc.Voting 
{ 
    [XmlType("Orientation")] 
    public enum BlankOrientation 
    { 
        [XmlEnum("P")] 
        Portrait, 
        [XmlEnum("L")] 
        Landscape, 
        [XmlEnum("PL")] 
        PortraitAndLandscape 
    } 
}
