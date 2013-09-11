using System; 
using System.Xml.Serialization; 
namespace Croc.Bpc.Voting 
{ 
    [Serializable, XmlType("ModeTime")] 
    public class ModeTime 
    { 
        [XmlAttribute] 
        public VotingMode mode; 
        [XmlAttribute] 
        public int hour; 
        [XmlAttribute] 
        public int minute; 
        public override string ToString() 
        { 
            return    "[mode=" + mode.ToString() +  
                    ",hour=" + hour.ToString() +  
                    ",minute=" + minute.ToString() + "]"; 
        } 
        public static ModeTime MaxValue 
        { 
            get 
            { 
                return new ModeTime() 
                { 
                    hour = int.MaxValue, 
                    minute = int.MaxValue 
                }; 
            } 
        } 
        public int TotalMinutes 
        { 
            get 
            { 
                return hour * 60 + minute; 
            } 
        } 
    } 
}
