using System; 
using System.Collections; 
using System.Collections.Generic; 
using System.Reflection; 
using System.Text; 
using System.Text.RegularExpressions; 
using System.Xml.Serialization; 
using Croc.Bpc.RegExpressions; 
using Croc.Core; 
using Croc.Core.Utils; 
using Croc.Core.Utils.Text; 
using Croc.Bpc.Voting; 
namespace Croc.Bpc.Printing.Reports.Templates 
{ 
    public class ReportTemplateParser 
    { 
        public const string MACRO_CURRENT_ROW = "##Row##"; 
        private Dictionary<string, object> _parameters = new Dictionary<string, object>(); 
        public void AddParameter(string name, object value) 
        { 
            _parameters["@" + name] = value; 
        } 
        public bool Check(string expression, bool invert) 
        { 
            if (expression == null) 
            { 
                return true; 
            } 
            object obj = GetVariable(expression); 
            if (obj == null) 
            { 
                return invert ? true : false; 
            } 
            if (obj is bool) 
            { 
                return invert ? !((bool)obj) : (bool)obj; 
            } 
            if (obj is IEnumerable) 
            { 
                return invert ? 
                    !(obj as IEnumerable).GetEnumerator().MoveNext() : 
                    (obj as IEnumerable).GetEnumerator().MoveNext(); 
            } 
            try 
            { 
                return invert ? 
                    !(Convert.ToInt32(obj) > 0) : 
                    Convert.ToInt32(obj) > 0; 
            } 
            catch 
            { 
            } 
            return invert ? 
                !(obj.ToString().Length > 0) : 
                obj.ToString().Length > 0; 
        } 
        private readonly Regex _reVariable = new ReportTemplateVariableRegex(); 
        private readonly Regex _reFmtVariable = new ReportTemplateVariableInFormatStrRegex(); 
        public string Format(string fmtLine) 
        { 
            if (fmtLine != null) 
            { 
                Match matchComponents = _reVariable.Match(fmtLine); 
                while (matchComponents.Success) 
                { 
                    string[] components = matchComponents.Groups[1].Value.Split('|'); 
                    string replaceStr = String.Empty; 
                    object obj = GetVariable(components[0]); 
                    if (obj is IEnumerable && !(obj is string)) 
                    { 
                        StringBuilder list = new StringBuilder(); 
                        foreach (object o in obj as IEnumerable) 
                        { 
                            if (0 < list.Length) list.Append(", "); 
                            if (components.Length > 2) 
                            { 
                                list.Append(FormatObject(GetValue(new[] { components[2] }, o, 0), components)); 
                            } 
                            else 
                            { 
                                list.Append(FormatObject(o, components)); 
                            } 
                        } 
                        replaceStr = list.ToString(); 
                    } 
                    else 
                    { 
                        replaceStr = FormatObject(obj, components); 
                    } 
                    fmtLine = fmtLine.Replace(matchComponents.Groups[0].Value, replaceStr); 
                    matchComponents = matchComponents.NextMatch(); 
                } 
            } 
            return fmtLine; 
        } 
        private string FormatObject(object obj, string[] objComponents) 
        { 
            var fmtString = objComponents.Length > 1 ? objComponents[1] : ""; 
            if (obj == null) 
                throw new ArgumentNullException( 
                    "obj", 
                    string.Format("Объект '{0}' не определен. Строка формата = '{1}'", objComponents[0],fmtString)); 
            if (obj.GetType().IsEnum) 
            { 
                foreach (var attribute in obj.GetType().GetField(obj.ToString()).GetCustomAttributes(true)) 
                { 
                    if (attribute is PresentationForReportAttribute) 
                    { 
                        return (attribute as PresentationForReportAttribute).DisplayName; 
                    } 
                } 
                return obj.ToString(); 
            } 
            if (obj is bool) 
            { 
                return (bool)obj ? "Да" : "Нет"; 
            } 
            string objectPresentation; 
            if (fmtString.Length > 0) 
            { 
                var fmtParts = fmtString.Split('#'); 
                if (fmtParts[0].Length > 0) 
                { 
                    var methodInfo = obj.GetType().GetMethod("ToString", new[] { typeof(string) }); 
                    if (methodInfo == null) 
                    { 
                        objectPresentation = obj.ToString(); 
                    } 
                    else 
                    { 
                        if (obj is DateTime && PlatformDetector.IsUnix && fmtParts[0].Contains("MMMM")) 
                        { 
                            objectPresentation = 
                                ((DateTime)obj).ToString(fmtParts[0], new System.Globalization.CultureInfo("ru-RU")); 
                            objectPresentation = DataConvert(objectPresentation); 
                        } 
                        else 
                        { 
                            var components = _reFmtVariable.Match(fmtParts[0]); 
                            while (components.Success) 
                            { 
                                fmtParts[0] = fmtParts[0].Replace( 
                                    components.Groups[0].Value, 
                                    GetVariable(components.Groups[1].Value).ToString()); 
                                components = components.NextMatch(); 
                            } 
                            objectPresentation = methodInfo.Invoke(obj, new object[] { fmtParts[0] }).ToString(); 
                        } 
                    } 
                } 
                else 
                { 
                    objectPresentation = obj.ToString(); 
                } 
                if (fmtParts.Length > 1 && fmtParts[1].Length > 0) 
                { 
                    switch (fmtParts[1][0]) 
                    { 
                        case 'u': 
                            return objectPresentation.ToUpper(); 
                        case 'l': 
                            return objectPresentation.ToLower(); 
                        case 'w': 
                            int value; 
                            if (Int32.TryParse(objectPresentation, out value)) 
                            { 
                                return CustomRusNumber.Str(value, true).Trim(); 
                            } 
                            break; 
                    } 
                } 
            } 
            else 
            { 
                objectPresentation = obj.ToString(); 
            } 
            return objectPresentation; 
        } 
        private string[] GetVariableComponents(string variableName, char splitter) 
        { 
            var components = new List<string>(); 
            do 
            { 
                var ltIndex = variableName.IndexOf("<"); 
                if (ltIndex == -1) 
                { 
                    components.AddRange(variableName.Trim().Split(splitter)); 
                    return components.ToArray(); 
                } 
                var tail = variableName.Substring(ltIndex); 
                components.AddRange(variableName.Substring(0, ltIndex).Split(splitter)); 
                int charCount = 0; 
                int endIndex = 1; 
                foreach (var ch in tail) 
                { 
                    if (ch == '>') 
                        charCount--; 
                    if (ch == '<') 
                        charCount++; 
                    if (charCount == 0) 
                        break; 
                    endIndex++; 
                } 
                components[components.Count - 1] += variableName.Substring(ltIndex, endIndex); 
                variableName = tail.Substring(endIndex); 
            } 
            while (variableName.Length > 0); 
            return components.ToArray(); 
        } 
        public object GetVariable(string variableName) 
        { 
            string[] components = GetVariableComponents(variableName, '.'); 
            try 
            { 
                if (components[0][0] == '@') 
                { 
                    if (_loopCollection.ContainsKey(components[0])) 
                    { 
                        return GetValue(components, _loopCollection[components[0]].enumerator.Current, 1); 
                    } 
                    if (_parameters.ContainsKey(components[0])) 
                    { 
                        return GetValue(components, _parameters[components[0]], 1); 
                    } 
                } 
                switch (components[0]) 
                { 
                    case "CurrentRow": 
                        return MACRO_CURRENT_ROW; 
                    case "TAB": 
                        return "   "; 
                    case "CRLF": 
                        return Environment.NewLine; 
                    case "CurrentDateTime": 
                        return GetValue(components, Managers.ElectionManager.LocalTimeNow, 1); 
                    case "AppVersion": 
                        return GetValue(components, CoreApplication.Instance.ApplicationVersion, 1); 
                    case "ElectionManager": 
                        return GetValue(components, Managers.ElectionManager, 1); 
                    case "VotingResultManager": 
                        return GetValue(components, Managers.VotingResultManager, 1); 
                    case "ScannersInfo": 
                        return GetValue(components, Managers.ScannersInfo, 1); 
                    case "VoteKey": 
                        return GetValue(components, new VoteKey(), 1); 
                    default: 
                        throw new ArgumentException("Не найдена переменная " + components[0]); 
                } 
            } 
            catch (ReportTemplateParserException pex) 
            { 
                throw new ArgumentException("Не удалось получить переменную отчета " + variableName, pex); 
            } 
        } 
        private object GetValue(string[] components, object obj, int index) 
        { 
            try 
            { 
                if (components.Length <= index) 
                { 
                    return obj; 
                } 
                string parametersString = null; 
                string memberName = components[index]; 
                if (components[index].Contains("<")) 
                { 
                    var ltIndex = components[index].IndexOf("<"); 
                    var gtIndex = components[index].LastIndexOf(">"); 
                    parametersString = components[index].Substring(ltIndex + 1, gtIndex - ltIndex - 1); 
                    memberName = components[index].Substring(0, ltIndex); 
                } 
                var memberParams = new string[0]; 
                if (!String.IsNullOrEmpty(parametersString)) 
                    memberParams = GetVariableComponents(parametersString, ';'); 
                if (components[index] == "@Current") 
                { 
                    if (index > 0 && _loopCollection.ContainsKey(components[index - 1])) 
                    { 
                        return _loopCollection[components[index - 1]].current; 
                    } 
                } 
                foreach (MemberInfo member in obj.GetType().GetMembers()) 
                { 
                    switch (member.MemberType) 
                    { 
                        case MemberTypes.Field: 
                        case MemberTypes.Property: 
                            MemberInfo found = null; 
                            if (member.Name == memberName) 
                                found = member; 
                            if (found != null) 
                            { 
                                object child; 
                                if (member.MemberType == MemberTypes.Field) 
                                { 
                                    FieldInfo field = obj.GetType().GetField(member.Name); 
                                    child = field.GetValue(obj); 
                                } 
                                else 
                                { 
                                    PropertyInfo prop = obj.GetType().GetProperty(member.Name); 
                                    child = prop.GetValue(obj, null); 
                                } 
                                if (index != components.Length - 1) 
                                { 
                                    return GetValue(components, child, index + 1); 
                                } 
                                return child; 
                            } 
                            break; 
                        case MemberTypes.Method: 
                            if (member.Name == memberName) 
                            { 
                                try 
                                { 
                                    var method = (MethodInfo)member; 
                                    if (method.GetParameters().Length == memberParams.Length) 
                                    { 
                                        object[] methodParams = GetMethodParameters(method.GetParameters(), memberParams); 
                                        return GetValue(components, method.Invoke(obj, methodParams), index + 1); 
                                    } 
                                } 
                                catch 
                                { 
                                    continue; 
                                } 
                            } 
                            break; 
                        case MemberTypes.Constructor: 
                            if (member.Name.TrimStart('.') == memberName) 
                            { 
                                var ctor = (ConstructorInfo)member; 
                                if (ctor.GetParameters().Length == memberParams.Length) 
                                { 
                                    object[] ctorParams = GetMethodParameters(ctor.GetParameters(), memberParams); 
                                    return GetValue(components, ctor.Invoke(ctorParams), index + 1); 
                                } 
                            } 
                            break; 
                    } 
                } 
                throw new ReportTemplateParserException(ParseExceptionReason.NotFound, components[index], obj.GetType()); 
            } 
            catch (Exception ex) 
            { 
                throw new ReportTemplateParserException(ParseExceptionReason.NotFound, components[index], obj.GetType(), ex); 
            } 
        } 
        private object GetTypedValueFromString(ParameterInfo methodParam, string value) 
        { 
            object paramValue; 
            if (methodParam.ParameterType.IsGenericType) 
                if (methodParam.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>)) 
                { 
                    var genericArg = methodParam.ParameterType.GetGenericArguments()[0]; 
                    paramValue = genericArg.IsEnum 
                        ? Enum.Parse(genericArg, value) : 
                        Convert.ChangeType(value, genericArg); 
                } 
                else 
                    throw new NotImplementedException(); 
            else 
                paramValue = methodParam.ParameterType.IsEnum 
                    ? Enum.Parse(methodParam.ParameterType, value) : 
                    Convert.ChangeType(value, methodParam.ParameterType); 
            return paramValue; 
        } 
        private object[] GetMethodParameters(ParameterInfo[] methodParams, string[] values) 
        { 
            var paramValues = new object[methodParams.Length]; 
            int i = 0; 
            foreach (var paramTypeInfo in methodParams) 
            { 
                if (String.IsNullOrEmpty(values[i])) 
                { 
                    paramValues[i] = null; 
                    i++; 
                    continue; 
                } 
                if (values[i].StartsWith("$")) 
                { 
                    var value = GetVariable(values[i].Substring(1)); 
                    if (value.GetType() != paramTypeInfo.ParameterType) 
                        paramValues[i] = GetTypedValueFromString(paramTypeInfo, value.ToString()); 
                    else 
                        paramValues[i] = value; 
                    i++; 
                    continue; 
                } 
                paramValues[i] = GetTypedValueFromString(paramTypeInfo, values[i]); 
                i++; 
            } 
            return paramValues; 
        } 
        private ArrayList _emptyEnumerator = new ArrayList(); 
        private class LoopContext 
        { 
            public IEnumerator enumerator; 
            public int current; 
            public LoopContext(IEnumerator e) 
            { 
                enumerator = e; 
                current = 1; 
            } 
        } 
        private Dictionary<string, LoopContext> _loopCollection = new Dictionary<string, LoopContext>(); 
        public delegate void ForBody(); 
        public void RunFor(string variable, string collection, ForBody forBody) 
        { 
            if (collection != null && variable != null) 
            { 
                variable = "@" + variable.Trim(); 
                LoopContext context = GetEnumerator(variable, collection.Trim()); 
                if (context.enumerator != null) 
                { 
                    while (context.enumerator.MoveNext()) 
                    { 
                        forBody(); 
                        context.current++; 
                    } 
                    _loopCollection.Remove(variable); 
                } 
            } 
        } 
        private LoopContext GetEnumerator(string variable, string collection) 
        { 
            if (collection == null || variable == null) 
            { 
                return new LoopContext(_emptyEnumerator.GetEnumerator()); 
            } 
            if (_loopCollection.ContainsKey(variable)) 
            { 
                throw new ReportTemplateParserException(ParseExceptionReason.AmbigiousFor, variable, typeof(string)); 
            } 
            object obj = GetVariable(collection.Trim()); 
            if (obj != null && obj is IEnumerable && !(obj is string)) 
            { 
                _loopCollection[variable] = new LoopContext((obj as IEnumerable).GetEnumerator()); 
                return _loopCollection[variable]; 
            } 
            return new LoopContext(_emptyEnumerator.GetEnumerator()); 
        } 
        private static readonly Dictionary<string, string> s_months = new Dictionary<string, string>() 
        { 
            {"январь",   "января" }, 
            {"февраль",  "февраля"}, 
            {"март",     "марта"}, 
            {"апрель",   "апреля"}, 
            {"май",      "мая"}, 
            {"июнь",     "июня"}, 
            {"июль",     "июля"}, 
            {"август",   "августа"}, 
            {"сентябрь", "сентября"}, 
            {"октябрь",  "октября"}, 
            {"ноябрь",   "ноября"}, 
            {"декабрь",  "декабря"}, 
        }; 
        public static string DataConvert(string dateTime) 
        { 
            foreach (var month in s_months) 
            { 
                int index = dateTime.IndexOf(month.Key, StringComparison.InvariantCultureIgnoreCase); 
                if (index >= 0) 
                { 
                    return dateTime.Remove(index, month.Key.Length) 
                        .Insert(index, month.Value); 
                } 
            } 
            return dateTime; 
        } 
    } 
}
