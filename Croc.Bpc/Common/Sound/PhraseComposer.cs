using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using System.Text.RegularExpressions; 
using Croc.Bpc.RegExpressions; 
using Croc.Core; 
namespace Croc.Bpc.Sound 
{ 
    public class PhraseComposer 
    { 
        private readonly ISoundManager _soundManager; 
        private enum ParameterModifier 
        { 
            None, 
            d, 
            delim, 
            hhmm, 
            hhimmi, 
            ddMMyyyy, 
        } 
        public object[] Parameters 
        { 
            get; 
            set; 
        } 
        public string[] Sounds 
        { 
            get; 
            set; 
        } 
        public string TextFormat 
        { 
            get; 
            set; 
        } 
        public string PhraseFormat 
        { 
            get; set; 
        } 
        public PhraseComposer(ISoundManager soundManager) 
        { 
            CodeContract.Requires(soundManager != null); 
            _soundManager = soundManager; 
        } 
        #region ComposeText 
        public string ComposeText() 
        { 
            try 
            { 
                var sb = new StringBuilder(); 
                int curReadIndex = 0; 
                int len = TextFormat.Length; 
                while (curReadIndex < len) 
                {                     
                    var paramStartIndex = TextFormat.IndexOf("{p", curReadIndex); 
                    if (paramStartIndex < 0) 
                    { 
                        sb.Append(TextFormat.Substring(curReadIndex)); 
                        break; 
                    } 
                    sb.Append(TextFormat.Substring(curReadIndex, paramStartIndex - curReadIndex)); 
                    curReadIndex = paramStartIndex + 1; 
                    var paramEndIndex = TextFormat.IndexOf("}", curReadIndex); 
                    if (paramEndIndex < 0) 
                        throw new Exception("Не найдена закрывающая скобка '}' параметра"); 
                    var arg = TextFormat.Substring(curReadIndex, paramEndIndex - paramStartIndex - 1); 
                    AppendTextFromParameters(arg, sb); 
                    curReadIndex = paramEndIndex + 1; 
                } 
                return sb.ToString(); 
            } 
            catch (Exception ex) 
            { 
                throw new Exception(string.Format("Ошибка разбора Формат-строки текста '{0}': {1}", 
                    TextFormat, ex.Message)); 
            } 
        } 
        private void AppendTextFromParameters(string arg, StringBuilder sb) 
        { 
            try 
            { 
                List<KeyValuePair<object, Type>> paramValues; 
                ParameterModifier paramModifier; 
                string paramModifierAtt; 
                ParseParameterArgument(arg, out paramValues, out paramModifier, out paramModifierAtt); 
                foreach (var paramValue in paramValues) 
                { 
                    var typeName = paramValue.Value.Name; 
                    switch (typeName) 
                    { 
                        case "String": 
                            sb.Append(paramValue.Key); 
                            break; 
                        case "Int32": 
                            AppendTextForIntValue((int)paramValue.Key, paramModifier, paramModifierAtt, sb); 
                            break; 
                        case "DateTime": 
                            AppendTextForDateTimeValue((DateTime)paramValue.Key, paramModifier, paramModifierAtt, sb); 
                            break; 
                        case "TimeSpan": 
                            AppendTextForTimeSpanValue((TimeSpan)paramValue.Key, paramModifier, paramModifierAtt, sb); 
                            break; 
                        default: 
                            throw new Exception(string.Format("Неизвестный тип параметра '{0}'", typeName)); 
                    } 
                } 
                if (paramModifier == ParameterModifier.delim && paramValues.Count > 0) 
                    sb.Length -= paramModifierAtt.Length; 
            } 
            catch (Exception ex) 
            { 
                throw new Exception(string.Format( 
                    "Для агрумента '{0}' не удалось получить текст: {1}", arg, ex.Message)); 
            } 
        } 
        private void AppendTextForIntValue( 
            int value, ParameterModifier modifier, string modifierAtt, StringBuilder sb) 
        { 
            switch (modifier) 
            { 
                case ParameterModifier.None: 
                    sb.Append(value); 
                    break; 
                case ParameterModifier.d: 
                    sb.Append(_soundManager.GetTextForDays(value)); 
                    break; 
                case ParameterModifier.delim: 
                    sb.Append(value); 
                    sb.Append(modifierAtt); 
                    break; 
                default: 
                    throw new Exception("Неподдерживаемый модификатор: " + modifier); 
            } 
        } 
        private void AppendTextForDateTimeValue( 
            DateTime value, ParameterModifier modifier, string modifierAtt, StringBuilder sb) 
        { 
            switch (modifier) 
            { 
                case ParameterModifier.None: 
                    sb.Append(value.ToString("HH:mm dd.MM.yyyy")); 
                    break; 
                case ParameterModifier.hhmm: 
                    sb.Append(value.ToString("HH:mm")); 
                    break; 
                case ParameterModifier.ddMMyyyy: 
                    sb.AppendFormat(value.ToString("dd.MM.yyyy")); 
                    break; 
                default: 
                    throw new Exception("Неподдерживаемый модификатор: " + modifier); 
            } 
        } 
        private void AppendTextForTimeSpanValue( 
            TimeSpan value, ParameterModifier modifier, string modifierAtt, StringBuilder sb) 
        { 
            switch (modifier) 
            { 
                case ParameterModifier.None: 
                case ParameterModifier.hhmm: 
                    sb.Append(string.Format("{0:00}:{1:00}", value.Hours, value.Minutes)); 
                    break; 
                default: 
                    throw new Exception("Неподдерживаемый модификатор: " + modifier); 
            } 
        } 
        #endregion 
        #region ComposePhrase 
        public string[] ComposePhrase() 
        { 
            return ComposePhrase(null); 
        } 
        public string[] ComposePhrase(string[] additionalSounds) 
        { 
            try 
            { 
                var resultSounds = new List<string>(); 
                var args = PhraseFormat.Substring(0, PhraseFormat.Length - 1).Replace("{", "").Split('}'); 
                foreach (var arg in args) 
                { 
                    var firstChar = arg[0]; 
                    switch (firstChar) 
                    { 
                        case 's': // это ссылка на звук. файл 
                            if (arg.Substring(1) == "*") 
                            { 
                                resultSounds.AddRange(Sounds.Select(i => _soundManager.SoundsDirPath + i)); 
                            } 
                            else 
                            { 
                                resultSounds.Add(_soundManager.SoundsDirPath + GetSoundFromSounds(arg)); 
                            } 
                            break; 
                        case 'p': // это ссылка на параметр 
                            AddSoundFilePathsFromParameters(arg, resultSounds); 
                            break; 
                        default: 
                            throw new Exception(string.Format("Неизвестный префикс аргумента '{0}'", arg)); 
                    } 
                } 
                if (additionalSounds != null) 
                    resultSounds.AddRange(additionalSounds.Select(i => _soundManager.SoundsDirPath + i)); 
                return resultSounds.ToArray(); 
            } 
            catch (Exception ex) 
            { 
                throw new Exception(string.Format("Ошибка разбора Формат-строки фразы '{0}': {1}", 
                    PhraseFormat, ex.Message)); 
            } 
        } 
        private string GetSoundFromSounds(string arg) 
        { 
            try 
            { 
                var index = Convert.ToInt32(arg.Substring(1)); 
                return Sounds[index]; 
            } 
            catch (Exception ex) 
            { 
                throw new Exception(string.Format( 
                    "Для агрумента '{0}' не удалось найти путь к звуковому файлу: {1}", arg, ex.Message)); 
            } 
        } 
        private void AddSoundFilePathsFromParameters(string arg, List<string> sounds) 
        { 
            try 
            { 
                List<KeyValuePair<object, Type>> paramValues; 
                ParameterModifier paramModifier; 
                string paramModifierAtt; 
                ParseParameterArgument(arg, out paramValues, out paramModifier, out paramModifierAtt); 
                foreach (var paramValue in paramValues) 
                { 
                    var typeName = paramValue.Value.Name; 
                    switch (typeName) 
                    { 
                        case "String": 
                            AddSoundForStringValue((String)paramValue.Key, paramModifier, paramModifierAtt, sounds); 
                            break; 
                        case "Int32": 
                            AddSoundForIntValue((Int32)paramValue.Key, paramModifier, paramModifierAtt, sounds); 
                            break; 
                        case "DateTime": 
                            AddSoundForDateTimeValue((DateTime)paramValue.Key, paramModifier, paramModifierAtt, sounds); 
                            break; 
                        case "TimeSpan": 
                            AddSoundForTimeSpanValue((TimeSpan)paramValue.Key, paramModifier, paramModifierAtt, sounds); 
                            break; 
                        default: 
                            throw new Exception(string.Format("Неизвестный тип параметра '{0}'", typeName)); 
                    } 
                } 
            } 
            catch (Exception ex) 
            { 
                throw new Exception(string.Format( 
                    "Для агрумента '{0}' не удалось найти путь к звуковому файлу: {1}", arg, ex.Message)); 
            } 
        } 
        private void AddSoundForIntValue( 
            int value, ParameterModifier modifier, string modifierAtt, List<string> sounds) 
        { 
            switch (modifier) 
            { 
                case ParameterModifier.None: 
                    sounds.AddRange(_soundManager.GetSoundForNumber(value, false, NumberDeclension.None)); 
                    break; 
                case ParameterModifier.d: 
                    sounds.AddRange(_soundManager.GetSoundForDays(value)); 
                    break; 
                default: 
                    throw new Exception("Неподдерживаемый модификатор: " + modifier); 
            } 
        } 
        private void AddSoundForDateTimeValue( 
            DateTime value, ParameterModifier modifier, string modifierAtt, List<string> sounds) 
        { 
            switch (modifier) 
            { 
                case ParameterModifier.None: 
                    sounds.AddRange(_soundManager.GetSoundForHours(value.Hour)); 
                    sounds.AddRange(_soundManager.GetSoundForMinutes(value.Minute)); 
                    sounds.AddRange(_soundManager.GetSoundForDayInMonth(value.Day)); 
                    sounds.Add(_soundManager.GetSoundForMonth(value.Month)); 
                    sounds.AddRange(_soundManager.GetSoundForYear(value.Year)); 
                    break; 
                case ParameterModifier.hhmm: 
                    sounds.AddRange(_soundManager.GetSoundForHours(value.Hour)); 
                    sounds.AddRange(_soundManager.GetSoundForMinutes(value.Minute)); 
                    break; 
                case ParameterModifier.hhimmi: 
                    sounds.AddRange(_soundManager.GetSoundForHours(value.Hour, false, NumberDeclension.N_ti)); 
                    sounds.AddRange(_soundManager.GetSoundForMinutes(value.Minute, true, NumberDeclension.N_ti)); 
                    break; 
                case ParameterModifier.ddMMyyyy: 
                    sounds.AddRange(_soundManager.GetSoundForDayInMonth(value.Day)); 
                    sounds.Add(_soundManager.GetSoundForMonth(value.Month)); 
                    sounds.AddRange(_soundManager.GetSoundForYear(value.Year)); 
                    break; 
                default: 
                    throw new Exception("Неподдерживаемый модификатор: " + modifier); 
            } 
        } 
        private void AddSoundForTimeSpanValue( 
            TimeSpan value, ParameterModifier modifier, string modifierAtt, List<string> sounds) 
        { 
            switch (modifier) 
            { 
                case ParameterModifier.None: 
                    sounds.AddRange(_soundManager.GetSoundForHours(value.Hours)); 
                    sounds.AddRange(_soundManager.GetSoundForMinutes(value.Minutes)); 
                    break; 
                case ParameterModifier.hhimmi: 
                    sounds.AddRange(_soundManager.GetSoundForHours(value.Hours, false, NumberDeclension.N_ti)); 
                    sounds.AddRange(_soundManager.GetSoundForMinutes(value.Minutes, true, NumberDeclension.N_ti)); 
                    break; 
                case ParameterModifier.hhmm: 
                    if (value.Hours > 0) 
                        sounds.AddRange(_soundManager.GetSoundForHours(value.Hours)); 
                    sounds.AddRange(_soundManager.GetSoundForMinutes(value.Minutes)); 
                    break; 
                default: 
                    throw new Exception("Неподдерживаемый модификатор: " + modifier); 
            } 
        } 
        private void AddSoundForStringValue( 
            String value, ParameterModifier modifier, string modifierAtt, List<string> sounds) 
        { 
            if (string.IsNullOrEmpty(value)) 
                return; 
            sounds.Add(value.Length == 1 
                           ? _soundManager.GetSoundForLetter(value[0]) 
                           : _soundManager.SoundsDirPath + value); 
        } 
        #endregion 
        private void ParseParameterArgument( 
            string arg, 
            out List<KeyValuePair<object, Type>> paramValues,  
            out ParameterModifier paramModifier, 
            out string paramModifierAtt) 
        { 
            const string INDEX_ALL = "IndexAll"; 
            const string INDEX_FROM = "IndexFrom"; 
            const string INDEX_TO = "IndexTo"; 
            const string MODIFIER = "Modifier"; 
            const string MODIFIER_ATT = "ModifierAtt"; 
            var regex = new ParseParameter(); 
            var match = regex.Match(arg); 
            if (!match.Success) 
                throw new Exception("Неправильный синтаксис параметра"); 
            paramValues = new List<KeyValuePair<object,Type>>(); 
            if (match.Groups[INDEX_ALL].Success) 
            { 
                for (int i = 0; i < Parameters.Length; i++) 
                    paramValues.Add(GetTypedParamValueAndType(i)); 
            } 
            else 
            { 
                var paramIndexFrom = Convert.ToInt32(match.Groups[INDEX_FROM].Value); 
                var paramIndexTo = paramIndexFrom; 
                if (match.Groups[INDEX_TO].Success) 
                { 
                    var paramIndexToValue = match.Groups[INDEX_TO].Value; 
                    paramIndexTo =  
                        string.CompareOrdinal(paramIndexToValue, "*") == 0 
                        ? Parameters.Length - 1  
                        : Convert.ToInt32(paramIndexToValue); 
                } 
                if (paramIndexFrom > paramIndexTo || paramIndexTo >= Parameters.Length) 
                    throw new Exception("Некорректный диапазон индексов параметров"); 
                for (int i = paramIndexFrom; i <= paramIndexTo; i++) 
                    paramValues.Add(GetTypedParamValueAndType(i));     
            } 
            var modifierGroup = match.Groups[MODIFIER]; 
            if (modifierGroup.Success) 
            { 
                paramModifier = (ParameterModifier)Enum.Parse(typeof(ParameterModifier), modifierGroup.Value); 
                var modifierAttGroup = match.Groups[MODIFIER_ATT]; 
                if (modifierAttGroup.Success) 
                    paramModifierAtt = modifierAttGroup.Value; 
                else 
                    paramModifierAtt = null; 
            } 
            else 
            { 
                paramModifier = ParameterModifier.None; 
                paramModifierAtt = null; 
            } 
        } 
        private KeyValuePair<object, Type> GetTypedParamValueAndType(int paramIndex) 
        { 
            var paramValue = Parameters[paramIndex]; 
            var paramValueType = paramValue.GetType(); 
            if (paramValueType == typeof(string)) 
            { 
                int parseRes; 
                if (int.TryParse((string)paramValue, out parseRes)) 
                { 
                    paramValue = parseRes; 
                    paramValueType = typeof(Int32); 
                } 
            } 
            return new KeyValuePair<object, Type>(paramValue, paramValueType); 
        } 
    } 
}
