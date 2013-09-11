using System; 
using System.Xml.Serialization; 
using System.Text; 
using Croc.Core.Extensions; 
namespace Croc.Bpc.Voting 
{ 
    [Serializable, XmlType("Candidate")] 
    public class Candidate 
    { 
        [XmlAttribute("lastName")] 
        public string LastName; 
        [XmlAttribute("firstName")] 
        public string FirstName; 
        [XmlAttribute("patronymic")] 
        public string Patronymic; 
        [XmlAttribute("registered")] 
        public bool Registered; 
        [XmlAttribute("disabled")] 
        public bool DisabledInSourceData; 
        [XmlAttribute("disabledLocally")] 
        public bool DisabledLocally; 
        [XmlAttribute("num")] 
        public int Number; 
        [XmlAttribute("biography")] 
        public string Biography; 
        [XmlAttribute("selfRegistered")] 
        public bool SelfRegistered; 
        [XmlAttribute("party")] 
        public string Party; 
        [XmlAttribute("id")] 
        public string Id; 
        [XmlAttribute("noneAbove")] 
        public bool NoneAbove; 
        [NonSerialized] 
        [XmlIgnore] 
        public EventHandler DisablingChangedHandler; 
        [XmlIgnore] 
        public bool Disabled 
        { 
            get 
            { 
                return DisabledInSourceData || DisabledLocally; 
            } 
            set 
            { 
                if (value) 
                { 
                    DisabledLocally = true; 
                } 
                else 
                { 
                    DisabledLocally = false; 
                    DisabledInSourceData = false; 
                } 
                DisablingChangedHandler.RaiseEvent(this); 
            } 
        } 
        [XmlIgnore] 
        public Election Election 
        { 
            get; 
            internal set; 
        } 
        public string CandidateDescription(bool showCancel) 
        { 
            var resultString = new StringBuilder(); 
            resultString.Append(Number + ". " + GetInitials(true)); 
            if (showCancel && Disabled) 
            { 
                if (Disabled) 
                    resultString.Append(" (позиция снята)"); 
                else if (DisabledLocally) 
                    resultString.Append(" (позиция снята на УИК)"); 
            } 
            return resultString.ToString(); 
        } 
        public string GetInitials(bool upper) 
        { 
            var resultString = new StringBuilder(); 
            if (!string.IsNullOrEmpty(LastName)) 
                resultString.Append(upper ? LastName.ToUpper() : LastName); 
            if (!string.IsNullOrEmpty(FirstName)) 
                resultString.Append(" " + FirstName); 
            if (!string.IsNullOrEmpty(Patronymic)) 
                resultString.Append(" " + Patronymic); 
            return resultString.ToString(); 
        } 
        public string GetShortInitials() 
        { 
            string candName = LastName ?? String.Empty; 
            if (FirstName != null && FirstName.Trim() != "") 
            { 
                candName += " " + FirstName[0] + "."; 
                if (Patronymic != null && Patronymic.Trim() != "") 
                { 
                    candName += Patronymic[0] + "."; 
                } 
            } 
            return candName; 
        } 
        public override string ToString() 
        { 
            var sText = new StringBuilder(); 
            sText.Append("["); 
            sText.Append("LastName=" + LastName + ";"); 
            sText.Append("FirstName=" + FirstName + ";"); 
            sText.Append("Patronymic=" + Patronymic + ";"); 
            sText.Append("Registered=" + Registered.ToString() + ";"); 
            sText.Append("Disabled=" + Disabled.ToString() + ";"); 
            sText.Append("DisabledLocally=" + DisabledLocally.ToString() + ";"); 
            sText.Append("Num=" + Number.ToString() + ";"); 
            sText.Append("Biography=" + Biography + ";"); 
            sText.Append("SelfRegistered=" + SelfRegistered.ToString() + ";"); 
            sText.Append("Party=" + Party + ";"); 
            sText.Append("ID=" + Id + ";"); 
            sText.Append("NoneAbove=" + NoneAbove.ToString()); 
            sText.Append("]"); 
            return sText.ToString(); 
        } 
    } 
}
