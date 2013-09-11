using System; 
using System.Collections; 
using System.Reflection; 
using System.Text; 
using System.Text.RegularExpressions; 
using System.Xml.Serialization; 
using Croc.Bpc.Printing; 
using Croc.Bpc.RegExpressions; 
using Croc.Core.Extensions; 
namespace Croc.Bpc.Voting 
{ 
    public enum LineType 
    { 
        [PresentationForReport("Не запрашивать пользователя")] 
        DontQueryUser = 0, 
        [PresentationForReport("Для голосования")] 
        Voting = 1, 
        [PresentationForReport("Для бланка")] 
        Blank = 2, 
        [PresentationForReport("Для выборов")] 
        Election = 3 
    } 
    [Serializable, XmlType("Line")] 
    public class Line 
    { 
        [XmlAttribute("expression")] 
        public string Expression; 
        [XmlAttribute("name")] 
        public string Name; 
        [XmlAttribute("num")] 
        public int Num; 
        [XmlAttribute("ID")] 
        public string Id; 
        [XmlAttribute("additionalNum")] 
        public string AdditionalNum = String.Empty; 
        [XmlAttribute("type")] 
        public LineType Type; 
        [XmlIgnore] 
        public bool IsAutoCalculated 
        { 
            get 
            { 
                return Type == LineType.DontQueryUser; 
            } 
        } 
        [XmlIgnore] 
        public string FullKey 
        { 
            get 
            { 
                return Num + AdditionalNum; 
            } 
        } 
        [NonSerialized] 
        private MethodInfo _method; 
        [XmlIgnore] 
        protected string Identifier 
        { 
            get 
            { 
                var result = new StringBuilder(Num.ToString()); 
                for (int i = 0; i < AdditionalNum.Length; i++) 
                { 
                    result.Append("_"); 
                    result.Append(Convert.ToInt32(AdditionalNum[i]).ToString()); 
                } 
                return result.ToString(); 
            } 
        } 
        [XmlIgnore] 
        public Election Election 
        { 
            get; 
            internal set; 
        } 
        #region Методы для генерирования динамической сборки, вычисляющей значения строк 
        public string BuildCheckTypeText(SourceData sourceData) 
        { 
            const string ASSEMBLY_TEXT 
                = @"public class LineExpression_SUFFIX{FUNCTIONS static public int check(){return EXPRESSION;}} "; 
            if (Type != LineType.DontQueryUser) return null; 
            try 
            { 
                var regExpression = new VotingCountReferenceRegex(); 
                var matches = regExpression.Matches(Expression); 
                var preparedExpression = new StringBuilder((matches.Count + 1)*1024); 
                preparedExpression.Append(ASSEMBLY_TEXT); 
                preparedExpression.Replace( 
                    "_SUFFIX", 
                    String.Format("_{0}_{1}_{2}", Num, AdditionalNum, _template.ElectionLink.ElectionId)); 
                preparedExpression.Replace("EXPRESSION", Expression); 
                var functions = new StringBuilder(matches.Count * 1024); 
                int num = 0; 
                foreach (Match match in matches) 
                { 
                    string fullKey = match.Value; 
                    var blankId = sourceData.GetBlankIdByElectionNumber(_template.ElectionLink.ElectionId); 
                    functions.Append(СompileSimpleExpression(blankId, fullKey, ++num)); 
                    preparedExpression.Replace(fullKey, " " + "Expression" + num + "()" + " " + "\n"); 
                } 
                preparedExpression.Replace("FUNCTIONS", functions.ToString()); 
                return preparedExpression.ToString(); 
            } 
            catch (Exception ex) 
            { 
                throw new Exception( 
                    String.Format( 
                        "Ошибка сборки в выражении № {0}{1} \"{2}\". Выборы № {3}:\n{4}" 
                        , Num 
                        , AdditionalNum 
                        , Expression 
                        , _template.ElectionLink.ElectionId 
                        , ex.Message) 
                    , ex); 
            } 
        } 
        public void BindMethod(Assembly assembly) 
        { 
            const string CHECK_METHOD = "check"; 
            const string CHECKER_TYPE_NAME = "Croc.Bpc.DynamicTypes.LineExpression"; 
            if (Type != LineType.DontQueryUser) return; 
            try 
            { 
                string checkTypeName = String.Format( 
                    "{0}_{1}_{2}_{3}", 
                    CHECKER_TYPE_NAME 
                    , Num 
                    , AdditionalNum 
                    , _template.ElectionLink.ElectionId); 
                Type oCheckerType = assembly.GetType(checkTypeName, true, false); 
                _method = oCheckerType.GetMethod(CHECK_METHOD); 
            } 
            catch (Exception ex) 
            { 
                throw new Exception( 
                    String.Format( 
                        "Ошибка связывания для выражения № {0}{1} \"{2}\". Выборы № {3}:\n{4}" 
                        , Num 
                        , AdditionalNum 
                        , Expression 
                        , _template.ElectionLink.ElectionId 
                        , ex.Message) 
                    , ex); 
            } 
        } 
        private string СompileSimpleExpression(string blankElectionId, string expr, int num) 
        { 
            var result = new StringBuilder(1024); 
            result.Append(String.Format("private static int Expression{0}()", num)); 
            result.Append("{"); 
            result.Append("int result;"); 
            result.Append("VoteKey mask = new VoteKey();"); 
            var parseResult = new ParsedExpression(expr); 
            bool allElections = expr.Trim().StartsWith("{@"); 
            string blankId = string.Empty; 
            if (!allElections) 
                blankId = blankElectionId; 
            switch (parseResult.Type) 
            { 
                case ParsedExpression.ExpType.VotesSum: 
                    result.Append(String.Format( 
                        "mask.ElectionId = \"{0}\";",  
                        _template.ElectionLink.ElectionId)); 
                    result.Append("result = Managers.VotingResultManager.VotingResults.VotesCount(mask);"); 
                    break; 
                case ParsedExpression.ExpType.MandateCount: 
                    result.Append("result = " + _template.ElectionLink.MaxMarks + ";"); 
                    break; 
                case ParsedExpression.ExpType.AgainstVotesCount: 
                    if (!_template.ElectionLink.NoneAboveExists) 
                    { 
                        result.Append("result = 0"); 
                    } 
                    else 
                    { 
                        result.Append(String.Format( 
                           "mask.ElectionId = \"{0}\"; mask.CandidateId = \"{1}\";" 
                           , _template.ElectionLink.ElectionId 
                           , _template.ElectionLink.NoneAboveCandidate.Id)); 
                        result.Append("result = Managers.VotingResultManager.VotingResults.VotesCount(mask);"); 
                    } 
                    break; 
                case ParsedExpression.ExpType.RestrictedCount: 
                    if (0 != blankId.CompareTo(string.Empty)) 
                        result.Append(String.Format("mask.BlankId = \"{0}\";", blankId)); 
                    PrepareMask(result, parseResult.Restrictions); 
                    result.Append("result = Managers.VotingResultManager.VotingResults.VotesCount(mask);"); 
                    break; 
                default: 
                    throw new Exception("Неизвестный тип выражения."); 
            } 
            result.Append("return result;"); 
            result.Append("}"); 
            return result.ToString(); 
        } 
        private void PrepareMask(StringBuilder result, Hashtable restr) 
        { 
            if (restr.ContainsKey("Scanner")) 
                result.Append(String.Format( 
                    "mask.ScannerSerialNumber = {0};", 
                    Convert.ToInt32(restr["Scanner"]))); 
            if (restr.ContainsKey("VotingMode")) 
                result.Append(String.Format( 
                    "mask.VotingMode = VotingMode.{0};", 
                    (VotingMode)Enum.Parse(typeof(VotingMode), restr["VotingMode"].ToString()))); 
            if (restr.ContainsKey("BlankType")) 
                result.Append(String.Format( 
                    "mask.BlankType = BlankType.{0};", 
                    (BlankType)Enum.Parse(typeof(BlankType), restr["BlankType"].ToString()))); 
            if (restr.ContainsKey("Election")) 
                result.Append(String.Format( 
                    "mask.ElectionNum = \"{0}\";", 
                    Convert.ToInt32(restr["Election"]))); 
            if (restr.ContainsKey("Candidate")) 
                result.Append(String.Format( 
                    "mask.CandidateId = \"{0}\";", 
                    Convert.ToInt32(restr["Candidate"]))); 
            if (restr.ContainsKey("Blank")) 
                result.Append(String.Format( 
                    "mask.BlankId = \"{0}\";", 
                    Convert.ToInt32(restr["Blank"]))); 
        } 
        #endregion 
        [NonSerialized] 
        [XmlIgnore] 
        public EventHandler ValueChangedHandler; 
        [XmlIgnore] 
        private int _value; 
        [XmlIgnore] 
        private bool _valueDefined; 
        [XmlIgnore] 
        public int? Value 
        { 
            get 
            { 
                if (IsAutoCalculated) 
                { 
                    CalculateValue(); 
                } 
                return _valueDefined ? (int?)_value : null; 
            } 
            set 
            { 
                if (// новое значение задано 
                    value.HasValue &&  
                    (!_valueDefined || _value != value.Value)) 
                { 
                    _value = value.Value; 
                    _valueDefined = true; 
                    ValueChangedHandler.RaiseEvent(this); 
                } 
            } 
        } 
        public void CalculateValue() 
        { 
            if (_method == null) 
            { 
                Election.SourceData.BindAutoLinesAndChecksCountMethods(Election); 
                if (_method == null) 
                    throw new Exception("Не задан метод для автовычисления строки протокола"); 
            } 
            Value = (int)_method.Invoke(null, null); 
        } 
        public class LineValue 
        { 
            public int Value; 
            public DateTime Modified; 
            public LineValue(int nValue) 
            { 
                Value = nValue; 
                Modified = DateTime.Now; 
            } 
            public LineValue(int value, DateTime dtModified) 
            { 
                Value = value; 
                Modified = dtModified; 
            } 
        } 
        public class ParsedExpression 
        { 
            public enum ExpType 
            { 
                VotesSum, 
                MandateCount, 
                AgainstVotesCount, 
                PropertyReference, 
                RestrictedCount 
            } 
            public ParsedExpression(string expression) 
            { 
                if (string.IsNullOrEmpty(expression)) 
                    throw new ArgumentNullException(expression); 
                expression = expression.Trim(); 
                _expression = expression; 
                switch (expression.ToUpper()) 
                { 
                    case "[S]": // сумма голосов 
                        _expType = ExpType.VotesSum; 
                        break; 
                    case "[M]": // количество мандатов 
                        _expType = ExpType.MandateCount; 
                        break; 
                    case "[P]": // против всех 
                        _expType = ExpType.AgainstVotesCount; 
                        break; 
                    default: 
                        if (expression[0] == '{') 
                        { 
                            ParseRestrictions(); 
                            _expType = ExpType.RestrictedCount; 
                        } 
                        else 
                        { 
                            throw new Exception("Неопределен тип выражения " + expression); 
                        } 
                        break; 
                } 
            } 
            private void ParseRestrictions() 
            { 
                var testExp = new LineRestrictionRegex(); 
                var match = testExp.Match(_expression); 
                if (!match.Success) 
                    throw new Exception("Выражение сформировано неправильно:" + _expression); 
                ImportValueOfRestriction(new LineRestrictionScannerRegex(), "Scanner"); 
                ImportValueOfRestriction(new LineRestrictionVotingModeRegex(), "VotingMode"); 
                ImportValueOfRestriction(new LineRestrictionCandidateRegex(), "Candidate"); 
                ImportValueOfRestriction(new LineRestrictionBlankTypeRegex(), "BlankType"); 
                ImportValueOfRestriction(new LineRestrictionElectionRegex(), "Election"); 
                ImportValueOfRestriction(new LineRestrictionBlankRegex(), "Blank"); 
                ImportValueOfRestriction(new LineRestrictionTypeRegex(), "Type"); 
            } 
            private void ImportValueOfRestriction(Regex restrictionPairRegex, string sRestrictionName) 
            { 
                var keyValue = restrictionPairRegex.Match(_expression); 
                if (keyValue.Success) 
                { 
                    var regValue = new LineRestrictionValueRegex(); 
                    var matchValue = regValue.Match(keyValue.Value); 
                    if (!matchValue.Success) 
                        throw new Exception("Ошибка при вычислении значения выражения: " + _expression); 
                    string value; // значение ограничения 
                    try 
                    { 
                        value = matchValue.Value.TrimStart('='); 
                    } 
                    catch (Exception ex) 
                    { 
                        throw new Exception("Ошибка при вычислении значения ограничения: " + _expression, ex); 
                    } 
                    try 
                    { 
                        Restrictions.Add(sRestrictionName, value); 
                    } 
                    catch (Exception ex) 
                    { 
                        throw new Exception("Ошибка при добавлении ограничения в коллекцию:" + _expression, ex); 
                    } 
                } 
            } 
            public ExpType Type 
            { 
                get 
                {  
                    return _expType;  
                } 
            } 
            private ExpType _expType; 
            private string _expression; 
            public Hashtable Restrictions = new Hashtable(); 
        } 
        public ParsedExpression ParseExpression() 
        { 
            return new ParsedExpression(Expression); 
        } 
        [XmlIgnore] 
        public ProtocolTemplate Template 
        { 
            get 
            {  
                return _template;  
            } 
        } 
        protected ProtocolTemplate _template; 
        public void SetProtocol(ProtocolTemplate oProtocol) 
        { 
            _template = oProtocol; 
        } 
        public static string GetLineTypeName(LineType type) 
        { 
            string res; 
            switch (type) 
            { 
                case LineType.Blank: 
                    res = "Для бланка"; 
                    break; 
                case LineType.DontQueryUser: 
                    res = "Не запрашивать пользователя"; 
                    break; 
                case LineType.Election: 
                    res = "Для выборов"; 
                    break; 
                case LineType.Voting: 
                    res = "Для голосования"; 
                    break; 
                default: 
                    res = "Не реализовано"; 
                    break; 
            } 
            return res; 
        } 
        public override string ToString() 
        { 
            var sText = new StringBuilder(); 
            sText.Append("["); 
            sText.Append("Expression=" + Expression + ";"); 
            sText.Append("Name=" + Name + ";"); 
            sText.Append("Num=" + Num + ";"); 
            sText.Append("AdditionalNum=" + AdditionalNum + ";"); 
            sText.Append("Type=" + Type + ";"); 
            sText.Append("]"); 
            return sText.ToString(); 
        } 
    } 
}
