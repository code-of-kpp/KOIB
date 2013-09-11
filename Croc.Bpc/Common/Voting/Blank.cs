using System; 
using System.Linq; 
using System.Xml.Serialization; 
using System.Text; 
using System.Collections.Generic; 
namespace Croc.Bpc.Voting 
{ 
    [Serializable, XmlType("Blank")] 
    public class Blank 
    { 
        [XmlElement("model")] 
        public string Model; 
        [XmlArray("Sections")] 
        [XmlArrayItem("int", IsNullable = false)] 
        public string[] Sections = new string[0]; 
        [XmlAttribute("marker")] 
        public int Marker; 
        [XmlAttribute("density")] 
        public int Density; 
        [XmlAttribute("name")] 
        public string Name; 
        [XmlAttribute("width")] 
        public int Width; 
        [XmlAttribute("height")] 
        public int Height; 
        [XmlAttribute("delta")] 
        public int Delta; 
        [XmlAttribute("orientation")] 
        public BlankOrientation Orientation; 
        [XmlAttribute("maxPShift")] 
        public int MaxPortraitShift; 
        [XmlAttribute("maxLShift")] 
        public int MaxLandscapeShift; 
        [XmlAttribute("num")] 
        public string ExternalId; 
        [XmlIgnore] 
        public string Id 
        { 
            get 
            { 
                return Marker.ToString(); 
            } 
        } 
        [XmlIgnore] 
        internal SourceData SourceData 
        { 
            get; 
            set; 
        } 
        [XmlIgnore] 
        public string[] Elections 
        { 
            get 
            { 
                var electionNames = new List<string>(Sections.Length); 
                electionNames.AddRange(Sections.Select(sect => SourceData.GetElectionByNum(sect).Name)); 
                return electionNames.ToArray(); 
            } 
        } 
        public override string ToString() 
        { 
            var sLine = new StringBuilder(); 
            sLine.Append("["); 
            sLine.Append("Model=" + Model + ";"); 
            sLine.Append("Sections=["); 
            foreach(string section in Sections) 
            { 
                sLine.Append(section + ";"); 
            } 
            sLine.Append("],"); 
            sLine.Append("Marker=" + Marker + ";"); 
            sLine.Append("Density=" + Density + ";"); 
            sLine.Append("Name=" + Name + ";"); 
            sLine.Append("Width=" + Width + ";"); 
            sLine.Append("Height=" + Height + ";"); 
            sLine.Append("Delta=" + Delta + ";"); 
            sLine.Append("Orientation=" + Orientation + ";"); 
            sLine.Append("MaxPShift=" + MaxPortraitShift + ";"); 
            sLine.Append("MaxLShift=" + MaxLandscapeShift + ";"); 
            sLine.Append("ID=" + Id); 
            sLine.Append("]"); 
            return sLine.ToString(); 
        } 
    } 
}
