using System; 

using System.Xml.Serialization; 

 

 

namespace Croc.Bpc.Election.Voting 

{ 

    /// <summary> 

    ///	Допустимые ориентации бланка 

    /// </summary> 

    [XmlType("Orientation")] 

    public enum BlankOrientation 

    { 

        /// <summary> 

        ///		Портретная 

        /// </summary> 

        [XmlEnum("P")] 

        Portrait, 

        /// <summary> 

        ///		Альбомная 

        /// </summary> 

        [XmlEnum("L")] 

        Landscape, 

        /// <summary> 

        ///		Портретная и альбомная 

        /// </summary> 

        [XmlEnum("PL")] 

        PortraitAndLandscape 

    } 

}


