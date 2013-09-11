using System; 
using System.Linq; 
using System.Xml.Serialization; 
using System.Text; 
using Croc.Bpc.Printing; 
namespace Croc.Bpc.Voting 
{ 
    [Serializable] 
    public class ProtocolTemplate 
    { 
        [XmlArray("Lines")] 
        public Line[] Lines 
        { 
            get 
            { 
                if (_linesInternal != null) 
                    foreach (Line oLine in _linesInternal) 
                        oLine.SetProtocol(this); 
                return _linesInternal; 
            } 
            set 
            { 
                _linesInternal = value; 
                if (_linesInternal != null) 
                    foreach (Line oLine in _linesInternal) 
                        oLine.SetProtocol(this); 
            } 
        } 
        private Line[] _linesInternal = new Line[0]; // начально заполняем пустым массивом 
        [XmlArray("ExtraChecks")] 
        public Line[] ExtraChecks 
        { 
            get 
            { 
                foreach (Line oLine in _extraChecksInternal) 
                    oLine.SetProtocol(this); 
                return _extraChecksInternal; 
            } 
            set 
            { 
                _extraChecksInternal = value; 
                foreach (Line oLine in _extraChecksInternal) 
                    oLine.SetProtocol(this); 
            } 
        } 
        private Line[] _extraChecksInternal = new Line[0]; //Начально заполняем пустым массивом 
        [XmlArray("Checks")] 
        [XmlArrayItem("Check")] 
        public CheckExpression[] Checks; 
        [XmlAttribute("name")] 
        public string Name; 
        [XmlAttribute("textValueWidth")] 
        public int TextValueWidth; 
        [XmlAttribute("numberWidth")] 
        public int NumberWidth; 
        [XmlAttribute("nameWidth")] 
        public int NameWidth; 
        [XmlAttribute("valueWidth")] 
        public int ValueWidth; 
        [XmlAttribute("disabledString")] 
        public string DisabledString; 
        [XmlAttribute("font")] 
        public FontType FontType; 
        [XmlAttribute("font-size")] 
        public int FontSize = -1; 
        [XmlArray("Texts")] 
        [XmlArrayItem("Text")] 
        public ProtocolText[] Texts = new ProtocolText[0]; 
        [XmlIgnore] 
        public Election ElectionLink 
        { 
            get 
            { return _electionLink; } 
        } 
        public void SetElection(Election election) 
        { 
            _electionLink = election; 
        } 
        private Election _electionLink; 
        public ProtocolText GetProtocolTemplate(bool final) 
        { 
            if (Texts != null) 
            { 
                foreach (ProtocolText currentText in Texts) 
                { 
                    if (currentText.Final == final) 
                    { 
                        return currentText; 
                    } 
                } 
            } 
            return null; 
        } 
        public int LatestLineNumber 
        { 
            get 
            { 
                if (Lines != null && Lines.Length > 0) 
                { 
                    return Lines[Lines.Length - 1].Num; 
                } 
                return 0; 
            } 
        } 
        public int GetLatestLineNumber(bool final) 
        { 
            if (Lines == null || Lines.Length <= 0) 
            { 
                return 0; 
            } 


            ProtocolText template = GetProtocolTemplate(final); 
            if (template == null) 
            { 
                return LatestLineNumber; 
            } 


            if (template.VoteLines.Any(oVoteLine => oVoteLine.Type == VoteLineType.Line)) 
            { 
                return LatestLineNumber; 
            } 
            return 0; 
        } 
        public override string ToString() 
        { 
            var sText = new StringBuilder(); 
            sText.Append('['); 
            sText.Append("Lines=["); 
            foreach (Line line in Lines) 
            { 
                sText.Append(line); 
                sText.Append(';'); 
            } 
            sText.Append("];ExtraChecks=["); 
            foreach (Line line in ExtraChecks) 
            { 
                sText.Append(line); 
                sText.Append(';'); 
            } 
            sText.Append("];Checks=["); 
            foreach (CheckExpression exp in Checks) 
            { 
                sText.Append(exp); 
                sText.Append(';'); 
            } 
            sText.Append("];Name="); 
            sText.Append(Name); 
            sText.Append(";TextValueWidth="); 
            sText.Append(TextValueWidth); 
            sText.Append(";NumberWidth="); 
            sText.Append(NumberWidth); 
            sText.Append(";NameWidth="); 
            sText.Append(NameWidth); 
            sText.Append(";ValueWidth="); 
            sText.Append(ValueWidth); 
            sText.Append(";DisabledString="); 
            sText.Append(DisabledString); 
            sText.Append(';'); 
            if (Texts != null) 
            { 
                sText.Append("Texts=["); 
                foreach (ProtocolText oText in Texts) 
                { 
                    sText.Append(oText); 
                    sText.Append(';'); 
                } 
                sText.Append("];"); 
            } 


            sText.Append(']'); 
            return sText.ToString(); 
        } 
    } 
}
