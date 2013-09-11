using System; 
using System.Reflection; 
using System.Text; 
using System.Text.RegularExpressions; 
using System.Xml.Serialization; 
using Croc.Bpc.RegExpressions; 
namespace Croc.Bpc.Voting 
{ 
    [Serializable, XmlType("Check")] 
    public class CheckExpression 
    { 
        [XmlAttribute("expression")] 
        public string InnerExpression; 
        [XmlAttribute("enabled")] 
        public bool Enabled; 
        [XmlAttribute("mild")] 
        public bool Mild; 
        protected ProtocolTemplate _template; 
        [XmlIgnore] 
        public bool Failed 
        { 
            get; 
            set; 
        } 
        [XmlIgnore] 
        public string Expression 
        { 
            get 
            { 
                return InnerExpression + " * номера порядковые"; 
            } 
        } 
        public bool Check(ProtocolTemplate oProtocol) 
        { 
            _template = oProtocol; 
            if (!Enabled) return true; 
            if (_method == null) 
                BindMethod(Assembly.GetExecutingAssembly()); 


            Failed = !(bool)_method.Invoke(null, null); 
            return !Failed; 
        } 
        #region Методы для создания / привязывания метода проверки КС в динамической сборке 
        const string ASSEMBLY_BEGIN = @"public class ExpressionChecker_SUFFIX{static public bool check(){return "; 
        const string ASSEMBLY_END = @";}}"; 
        [NonSerialized] 
        private MethodInfo _method; 
        public string BuildCheckTypeText(ProtocolTemplate protocol) 
        { 
            try 
            { 
                _template = protocol; 
                string fullKey; 
                var preparedExpression = new StringBuilder(InnerExpression); 
                var regExpression = new CheckExpressionLineReferenceRegex(); 
                var matches = regExpression.Matches(InnerExpression); 
                var delta = 0; 
                foreach (Match match in matches) 
                { 
                    string newValue; 
                    switch (match.Value) 
                    { 
                        case "S": 
                        case "[S]": 
                            newValue = String.Format( 
                                "Managers.VotingResultManager.VotingResults.GetTotalVotesCount(\"{0}\")", 
                                protocol.ElectionLink.ElectionId); 
                            break; 
                        case "M": 
                        case "[M]": 
                            newValue = _template.ElectionLink.MaxMarks.ToString(); 
                            break; 
                        case "P": 
                        case "[P]": 
                            newValue = String.Format( 
                                "Managers.VotingResultManager.VotingResults.GetAboveCandidateVotesCount(" 
                                    + "Managers.ElectionManager.SourceData.GetElectionByNum(\"{0}\"))", 
                                protocol.ElectionLink.ElectionId); 
                            break; 
                        default: 
                            fullKey = match.Value; 
                            fullKey = fullKey.TrimStart('['); 
                            fullKey = fullKey.TrimEnd(']'); 
                            int lineNumber = Convert.ToInt32(fullKey); 
                            if (lineNumber < 1 || lineNumber > protocol.Lines.Length) 
                                throw new Exception("В протоколе нет строки с порядковым номером " + lineNumber); 
                            newValue = String.Format( 
                                "Managers.ElectionManager.SourceData.GetElectionByNum(\"{0}\").Protocol.Lines[{1}].Value" 
                                , protocol.ElectionLink.ElectionId 
                                , lineNumber - 1); 
                            break; 
                    } 
                    preparedExpression.Replace(match.Value, newValue, match.Index + delta, match.Length); 
                    delta += newValue.Length - match.Length; 
                } 
                preparedExpression.Replace("=", "@"); 
                preparedExpression.Replace("<@", " " + "<=" + " "); 
                preparedExpression.Replace(">@", " " + ">=" + " "); 
                preparedExpression.Replace("@", " " + "==" + " "); 
                preparedExpression.Replace("<>", " " + "!=" + " "); 
                preparedExpression.Insert(0, ASSEMBLY_BEGIN.Replace( 
                    "_SUFFIX" 
                    , GetExpressionCheckerSuffix())); 
                preparedExpression.Append(ASSEMBLY_END); 
                return preparedExpression.ToString(); 
            } 
            catch (Exception ex) 
            { 
                throw new Exception( 
                    String.Format( 
                        "Ошибка сборки в выражении : {0}:\n{1}" 
                        ,InnerExpression 
                        ,ex.Message) 
                    , ex); 
            } 
        } 
        public void BindMethod(Assembly assembly) 
        { 
            const string CHECK_METHOD = "check";  
            const string CHECKER_TYPE_NAME = "Croc.Bpc.DynamicTypes.ExpressionChecker"; 
            try 
            { 
                string checkTypeName = CHECKER_TYPE_NAME + GetExpressionCheckerSuffix(); 
                Type checkerType = assembly.GetType(checkTypeName, true, false); 
                _method = checkerType.GetMethod(CHECK_METHOD); 
            } 
            catch (Exception ex) 
            { 
                throw new Exception("Ошибка связывания для  выражения:" + InnerExpression + ":\n" + ex.Message, ex); 
            } 
        } 
        private string GetExpressionCheckerSuffix() 
        { 
            return String.Format( 
                "_{0}_{1}", 
                _template.ElectionLink.ElectionId, 
                InnerExpression.GetHashCode().ToString().Replace("-", "_")); 
        } 
        #endregion 
        public string GetExpansion(ProtocolTemplate protocol) 
        { 
            try 
            { 
                string fullKey; 
                var preparedExpression = new StringBuilder(InnerExpression); 
                var regExpression = new CheckExpressionLineReferenceRegex(); 
                var matches = regExpression.Matches(InnerExpression); 
                var delta = 0; 
                foreach (Match match in matches) 
                { 
                    if (match.Value.StartsWith("[")) 
                    { 
                        fullKey = match.Value; 
                        fullKey = fullKey.TrimStart('['); 
                        fullKey = fullKey.TrimEnd(']'); 


                        int lineNumber; 
                        if(!int.TryParse(fullKey, out lineNumber)) 
                            continue; 
                        if (lineNumber < 1 || lineNumber > protocol.Lines.Length) 
                            throw new Exception("В протоколе нет строки с порядковым номером " + lineNumber); 
                        var sNewValue = "[" + protocol.Lines[lineNumber - 1].Num + protocol.Lines[lineNumber - 1].AdditionalNum + "]"; 
                        preparedExpression.Replace(match.Value, sNewValue, match.Index + delta, match.Length); 
                        delta += sNewValue.Length - match.Length; 
                    } 
                } 
                return preparedExpression.ToString(); 
            } 
            catch (Exception ex) 
            { 
                throw new Exception( 
                    String.Format("Ошибка КС \"{0}\". Выборы № {1}:\n{2}" 
                        ,InnerExpression 
                        ,protocol.ElectionLink.ElectionId 
                        ,ex.Message) 
                    ,ex); 
            } 
        } 
        public string GetFullExpression(ProtocolTemplate protocol, VotingResults results) 
        { 
            try 
            { 
                _template = protocol; 
                string fullKey; 
                var preparedExpression = new StringBuilder(InnerExpression); 
                var regExpression = new CheckExpressionLineReferenceRegex(); 
                var matches = regExpression.Matches(InnerExpression); 
                var delta = 0; 
                foreach (Match match in matches) 
                { 
                    string newValue; 
                    switch (match.Value) 
                    { 
                        case "S": 
                        case "[S]": 
                            newValue = results.GetTotalVotesCount(protocol.ElectionLink.ElectionId).ToString(); 
                            break; 
                        case "M": 
                        case "[M]": 
                            newValue = protocol.ElectionLink.MaxMarks.ToString(); 
                            break; 
                        case "P": 
                        case "[P]": 
                            newValue = results.GetAboveCandidateVotesCount(protocol.ElectionLink).ToString(); 
                            break; 
                        default: 
                            fullKey = match.Value; 
                            fullKey = fullKey.TrimStart('['); 
                            fullKey = fullKey.TrimEnd(']'); 
                            int lineNumber = Convert.ToInt32(fullKey); 
                            if (lineNumber < 1 || lineNumber > protocol.Lines.Length) 
                                throw new Exception("В протоколе нет строки с порядковым номером " + lineNumber); 
                            if (protocol.Lines[lineNumber - 1].Value.HasValue) 
                                newValue = protocol.Lines[lineNumber - 1].Value.ToString(); 
                            else 
                                throw new Exception("Не заполнена строка протокола номер " + lineNumber); 
                            break; 
                    } 
                    preparedExpression.Replace(match.Value, newValue, match.Index + delta, match.Length); 
                    delta += newValue.Length - match.Length; 
                } 
                return preparedExpression.ToString(); 
            } 
            catch (Exception ex) 
            { 
                throw new Exception( 
                    String.Format("Ошибка КС \"{0}\". Выборы № {1}:\n{2}" 
                        , InnerExpression 
                        , protocol.ElectionLink.ElectionId 
                        , ex.Message) 
                    , ex); 
            } 
        } 
        public override string ToString() 
        { 
            return "[Expression=" + InnerExpression + 
                    ",Enabled=" + Enabled + 
                    ",Mild=" + Mild + "]"; 
        } 
    } 
}
