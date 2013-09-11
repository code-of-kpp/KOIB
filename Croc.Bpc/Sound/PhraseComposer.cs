using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Text.RegularExpressions; 

 

 

namespace Croc.Bpc.Sound 

{ 

    /// <summary> 

    /// Составитель фраз для воспроизведения (звука) и вывода (текста) 

    /// </summary> 

    public class PhraseComposer 

    { 

        /// <summary> 

        /// менеджер звуков 

        /// </summary> 

        private ISoundManager _soundManager; 

 

 

        /// <summary> 

        /// Модификаторы параметра 

        /// </summary> 

        private enum ParameterModifier 

        { 

            /// <summary> 

            /// Модификатор отсутствует 

            /// </summary> 

            None, 

            /// <summary> 

            /// Кол-во дней (например, фраза "123-и дня", текст "123"). 

            /// Для параметров типа Int32 

            /// </summary> 

            d, 

            /// <summary> 

            /// Разделитель, который используется при перечислении списка параметров. 

            /// Для параметров типа Int32 

            /// </summary> 

            delim, 

            /// <summary> 

            /// Время: часы и минуты (например, фраза "17-цать часов 32-е минуты", текст "17:32"). 

            /// Для параметров типа DateTime и TimeSpan 

            /// </summary> 

            hhmm, 

            /// <summary> 

            /// Дата: день, месяц, год (например, фраза "25-ое марта 2010-ого года", текст "25.03.2010"). 

            /// Для параметров типа DateTime 

            /// </summary> 

            ddMMyyyy, 

        } 


 
 

        /// <summary> 

        /// Параметры фразы 

        /// </summary> 

        /// <remarks> 

        /// Поддерживаемые типы параметров:  

        ///     Int32 

        ///     DateTime 

        ///     TimeSpan 

        ///     String 

        /// Для адресации к значению параметра в формат-строках (TextFormat и PhraseFormat)  

        /// нужно использовать синтаксис: 

        /// {pN} - где N может иметь формат: 

        ///     Для доступа к одному параметру: 

        ///         - число: индекс параметра в массиве Parameters 

        ///     Для доступа к списку параметров: 

        ///         - символ '*' - список из всех параметров 

        ///         - "i-j" - список, начиная с параметра с индексом i и заканчивая параметром с индексом j  

        ///             (i должно быть больше или равно 0 и меньше j) 

        ///         - "i-*" - список, начиная с параметра с индексом i и заканчивая последним параметром 

        ///             (i должно быть больше или равно 0) 

        /// {pN:модификатор[атрибут модификатора]} - аналогично {pN}, но в модификаторе и его опциональном атрибуте  

        ///     указывается, как интерпретировать значение параметра (см. ParameterModifier) 

        /// </remarks> 

        public object[] Parameters 

        { 

            get; 

            set; 

        } 

 

 

        /// <summary> 

        /// Пути к звуковым файлам, которые будут использоваться во фразе 

        /// </summary> 

        /// <remarks> 

        /// Для адресации к звук. файлам в формат-строке фразы (PhraseFormat)  

        /// нужно использовать синтаксис: 

        /// {sN}    - звук. файл с индексом N в массиве Sounds 

        /// {s*}    - последовательность всех путей к звук. файлов из Sounds 

        /// </remarks> 

        public string[] Sounds 

        { 

            get; 

            set; 

        } 

 

 

        /// <summary> 

        /// Формат-строка текста 


        /// </summary> 

        /// <remarks> 

        /// В формат-строке текста можно ссылаться на параметры, например, "Московское время: {p0:hhmm}" 

        /// </remarks> 

        public string TextFormat 

        { 

            get; 

            set; 

        } 

 

 

        /// <summary> 

        /// Формат-строка фразы 

        /// </summary> 

        /// <remarks> 

        /// В формат-строке фразы можно ссылаться на параметры и звук. файлы,  

        /// например, "{s0}{p0}{s1}{p1}{s2}" 

        /// </remarks> 

        public string PhraseFormat 

        { 

            get; 

            set; 

        } 

 

 

        /// <summary> 

        /// Констурктор 

        /// </summary> 

        /// <param name="soundManager">менеджер звуков</param> 

        public PhraseComposer(ISoundManager soundManager) 

        { 

            CodeContract.Requires(soundManager != null); 

 

 

            _soundManager = soundManager; 

        } 

 

 

        #region ComposeText 

 

 

        /// <summary> 

        /// Собрать текст 

        /// </summary> 

        /// <returns>собранный текст</returns> 

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

                    else 

                    { 

                        sb.Append(TextFormat.Substring(curReadIndex, paramStartIndex - curReadIndex)); 

                        curReadIndex = paramStartIndex + 1; 

 

 

                        // читаем параметр 

                        var paramEndIndex = TextFormat.IndexOf("}", curReadIndex); 

                        if (paramEndIndex < 0) 

                            throw new Exception("Не найдена закрывающая скобка '}' параметра"); 

 

 

                        var arg = TextFormat.Substring(curReadIndex, paramEndIndex - paramStartIndex - 1); 

                        AppendTextFromParameters(arg, sb); 

 

 

                        curReadIndex = paramEndIndex + 1; 

                    } 

                } 

 

 

                return sb.ToString(); 

            } 

            catch (Exception ex) 

            { 

                throw new Exception(string.Format("Ошибка разбора Формат-строки текста '{0}': {1}", 

                    TextFormat, ex.Message)); 

            } 

        } 

 

 

        /// <summary> 

        /// Добавляет текст, который находит по аргументу в параметрах 

        /// </summary> 

        /// <param name="arg"></param> 

        /// <param name="sb"></param> 


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


 
 

        /// <summary> 

        /// Возвращает текст для значения типа Int32 

        /// </summary> 

        /// <param name="value"></param> 

        /// <param name="modifier"></param> 

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

 

 

                // TODO: длинный вариант записи кол-ва дней 

                //case ParameterModifier.days: 

                //    sb.Append(_soundManager.GetTextForDays(value)); 

                //    break; 

 

 

                case ParameterModifier.delim: 

                    sb.Append(value); 

                    sb.Append(modifierAtt); 

                    break; 

 

 

                default: 

                    throw new Exception("Неподдерживаемый модификатор: " + modifier); 

            } 

        } 

 

 

        /// <summary> 

        /// Возвращает текст для значения типа DateTime 

        /// </summary> 

        /// <param name="value"></param> 

        /// <param name="modifier"></param> 

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

 

 

                // TODO: длинный формат записи времени 

                //case ParameterModifier.hour_min: 

                //sb.AppendFormat("{0} {1}", 

                //    _soundManager.GetTextForHours(value.Hour), _soundManager.GetTextForMinutes(value.Minute)); 

 

 

                case ParameterModifier.ddMMyyyy: 

                    sb.AppendFormat(value.ToString("dd.MM.yyyy")); 

                    break; 

 

 

                default: 

                    throw new Exception("Неподдерживаемый модификатор: " + modifier); 

            } 

        } 

 

 

        /// <summary> 

        /// Возвращает текст для значения типа TimeSpan 

        /// </summary> 

        /// <param name="value"></param> 

        /// <param name="modifier"></param> 

        private void AppendTextForTimeSpanValue( 

            TimeSpan value, ParameterModifier modifier, string modifierAtt, StringBuilder sb) 

        { 

            switch (modifier) 

            { 

                case ParameterModifier.None: 

                case ParameterModifier.hhmm: 

                    sb.Append(string.Format("{0:00}:{1:00}", value.Hours, value.Minutes)); 

                    break; 

 

 

                // TODO: длинная запись времени 

                //case ParameterModifier.hhmm: 

                //    sb.AppendFormat("{0} {1}", 

                //        _soundManager.GetTextForHours(value.Hours), _soundManager.GetTextForMinutes(value.Minutes)); 

                //    break; 

 

 


                default: 

                    throw new Exception("Неподдерживаемый модификатор: " + modifier); 

            } 

        } 

 

 

        #endregion 

 

 

        #region ComposePhrase 

 

 

        /// <summary> 

        /// Собрать фразу для воспроизведения 

        /// </summary> 

        /// <returns> 

        /// массив звук. файлов, при последовательном воспроизведении которых получится собранная фраза 

        /// </returns> 

        public string[] ComposePhrase() 

        { 

            return ComposePhrase(null); 

        } 

 

 

        /// <summary> 

        /// Собрать фразу для воспроизведения 

        /// </summary> 

        /// <param name="additionalSounds">массив ключей звук. файлов, которые нужно добавить в конец</param> 

        /// <returns> 

        /// массив звук. файлов, при последовательном воспроизведении которых получится собранная фраза 

        /// </returns> 

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

 

 

        /// <summary> 

        /// Находит по аргументу путь к звук. файлу в массиве путей 

        /// </summary> 

        /// <param name="arg"></param> 

        /// <returns></returns> 

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

 

 

        /// <summary> 

        /// Находит по аргументу пути к звук. файлам, которые воспроизводят  

        /// значение Параметра, соответствующего аргументу, и добавляет их в коллекцию звук. файлов 

        /// </summary> 

        /// <param name="arg"></param> 

        /// <param name="sounds"></param> 

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

 

 

        /// <summary> 

        /// Возвращает пути к звук. файлам для воспроизведения значения типа Int32 

        /// </summary> 

        /// <param name="value"></param> 

        /// <param name="modifier"></param> 

        /// <param name="modifierAtt"></param> 

        /// <param name="sounds"></param> 

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

 

 

        /// <summary> 

        /// Возвращает пути к звук. файлам для воспроизведения значения типа DateTime 

        /// </summary> 

        /// <param name="value"></param> 

        /// <param name="modifier"></param> 

        /// <param name="modifierAtt"></param> 

        /// <param name="sounds"></param> 

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

 

 

                case ParameterModifier.ddMMyyyy: 

                    sounds.AddRange(_soundManager.GetSoundForDayInMonth(value.Day)); 

                    sounds.Add(_soundManager.GetSoundForMonth(value.Month)); 

                    sounds.AddRange(_soundManager.GetSoundForYear(value.Year)); 

                    break; 

 

 

                default: 

                    throw new Exception("Неподдерживаемый модификатор: " + modifier); 

            } 

        } 

 

 

        /// <summary> 

        /// Возвращает пути к звук. файлам для воспроизведения значения типа TimeSpan 

        /// </summary> 

        /// <param name="value"></param> 

        /// <param name="modifier"></param> 

        /// <param name="modifierAtt"></param> 

        /// <param name="sounds"></param> 

        private void AddSoundForTimeSpanValue( 

            TimeSpan value, ParameterModifier modifier, string modifierAtt, List<string> sounds) 

        { 

            switch (modifier) 

            { 

                case ParameterModifier.None: 

                    sounds.AddRange(_soundManager.GetSoundForHours(value.Hours)); 

                    sounds.AddRange(_soundManager.GetSoundForMinutes(value.Minutes)); 

                    break; 

 

 

                case ParameterModifier.hhmm: 

                    sounds.AddRange(_soundManager.GetSoundForHours(value.Hours)); 

                    sounds.AddRange(_soundManager.GetSoundForMinutes(value.Minutes)); 

                    break; 

 

 

                default: 


                    throw new Exception("Неподдерживаемый модификатор: " + modifier); 

            } 

        } 

 

 

        /// <summary> 

        /// Возвращает пути к звук. файлам для воспроизведения значения типа String 

        /// </summary> 

        /// <remarks>умеет работать только со строками, состоящими из 1 буквы</remarks> 

        /// <param name="value"></param> 

        /// <param name="modifier"></param> 

        /// <param name="modifierAtt"></param> 

        /// <param name="sounds"></param> 

        private void AddSoundForStringValue( 

            String value, ParameterModifier modifier, string modifierAtt, List<string> sounds) 

        { 

            if (string.IsNullOrEmpty(value) || value.Length != 1) 

                return; 

 

 

            sounds.Add(_soundManager.GetSoundForLetter(value[0])); 

        } 

 

 

        #endregion 

 

 

        /// <summary> 

        /// Разбирает строку-аргумент параметра 

        /// </summary> 

        /// <param name="arg">строка-аргумент параметра</param> 

        /// <param name="paramValues">список значений параметров и их типов</param> 

        /// <param name="paramValueType">тип параметра</param> 

        /// <param name="paramModifier">модификатор параметра</param> 

        /// <param name="paramModifierAtt">атрибут модификатора параметра</param> 

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

            const string PATTERN = @"^p((?<" + INDEX_ALL + @">\*)|(?<" + INDEX_FROM + @">\d+)(-(?<" +  

                INDEX_TO + @">\*|\d+))?)(:(?<" + MODIFIER + @">\w+)(\[(?<" + MODIFIER_ATT + @">.+)\])?)?$"; 

 

 


            var match = Regex.Match(arg, PATTERN); 

            if (!match.Success) 

                throw new Exception("Неправильный синтаксис параметра"); 

 

 

            // получим список параметров 

            paramValues = new List<KeyValuePair<object,Type>>(); 

 

 

            // если нужно взять все параметры 

            if (match.Groups[INDEX_ALL].Success) 

            { 

                for (int i = 0; i < Parameters.Length; i++) 

                    paramValues.Add(GetTypedParamValueAndType(i)); 

            } 

            else 

            { 

                var paramIndexFrom = Convert.ToInt32(match.Groups[INDEX_FROM].Value); 

                var paramIndexTo = paramIndexFrom; 

 

 

                // если индекс ПО задан 

                if (match.Groups[INDEX_TO].Success) 

                { 

                    var paramIndexToValue = match.Groups[INDEX_TO].Value; 

                    paramIndexTo =  

                        // если индекс ПО - это как бы индекс последнего элемента 

                        string.CompareOrdinal(paramIndexToValue, "*") == 0 

                        ? Parameters.Length - 1  

                        : Convert.ToInt32(paramIndexToValue); 

                } 

 

 

                if (paramIndexFrom > paramIndexTo || paramIndexTo >= Parameters.Length) 

                    throw new Exception("Некорректный диапазон индексов параметров"); 

 

 

                for (int i = paramIndexFrom; i <= paramIndexTo; i++) 

                    paramValues.Add(GetTypedParamValueAndType(i));     

            } 

 

 

            // получим модификатор и его атрибут 

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

 

 

        /// <summary> 

        /// Получить типизированное значение параметра с заданным индексом и его тип 

        /// </summary> 

        /// <param name="paramIndex"></param> 

        /// <returns>пара {типизированное значение параметра, тип значения параметра}</returns> 

        private KeyValuePair<object, Type> GetTypedParamValueAndType(int paramIndex) 

        { 

            var paramValue = Parameters[paramIndex]; 

            var paramValueType = paramValue.GetType(); 

 

 

            // если тип параметра Строка 

            if (paramValueType == typeof(string)) 

            { 

                // то возможно ее удастся преобразовать в один из поддерживаемых типов 

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


