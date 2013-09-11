using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Core.Utils.Text 

{ 

    /// <summary> 

    /// Парсер формат-строки вида "тест{ключ_1}техт...техт{ключ_N}техт", т.е.  

    /// почти аналогично как для string.Format, но внутри {} можно указывать любой строковый ключ. 

    /// Для вывода фигурных скобок нужно написать их два раза: '{{', '}}' 

    /// </summary> 

    public static class StringFormatParser 

    { 

        /// <summary> 

        /// Выполняет разбор входной формат-строки. Если разбор выполнен без ошибок, то  

        /// возвращает формат-строку, пригодную для передач в метод string.Format, и  

        /// список ключей, которые были выделены из {}, например: 

        /// format  = "text{key_1}text{key_2}text{key_1}" 

        /// return  = "text{0}text{1}text{0}" 

        /// keys    = {"key_1", "key_2"} 

        ///  

        /// Если при разборе строки выявлена ошибка, то будет сшенерировано исключение с соотв. описанием. 

        /// </summary> 

        /// <param name="format"></param> 

        /// <param name="keys"></param> 

        /// <returns></returns> 

        public static string Parse(string format, out List<string> keys) 

        { 

            try 

            { 

                if (string.IsNullOrEmpty(format)) 

                    throw new Exception("Строка формата не задана"); 

 

 

                keys = new List<string>(); 

 

 

                var sb = new StringBuilder(format.Length); 

                var currentKey = new StringBuilder(10); 

                bool openBraceFound = false; 

                bool closeBraceFound = false; 

                bool prevIsOpenBrace = false; 

                bool prevIsCloseBrace = false; 

 

 

                int symbolNum = 0; 

                foreach (var ch in format) 

                { 


                    symbolNum++; 

 

 

                    if (ch == '{') 

                    { 

						//Если до нее была закрывающая скобка то она одиночная 

						if (prevIsCloseBrace && closeBraceFound) 

							throw new Exception(String.Format("Неожиданная закрывающая скобка '}}' (символ {0})" 

																, symbolNum - 1)); 

                        if (openBraceFound) 

                        { 

                            if (prevIsOpenBrace) 

                            { 

                                sb.Append("{{"); 

                                openBraceFound = false; 

                                prevIsOpenBrace = false; 

                            } 

                            else 

                                throw new Exception( 

                                    string.Format("Неожиданная открывающая скобка '{{' (символ {0})", symbolNum)); 

                        } 

                        else 

                        { 

                            // начинается новый ключ сообщения 

                            currentKey.Length = 0; 

                            openBraceFound = true; 

                            prevIsOpenBrace = true; 

                        } 

                    } 

                    else if (ch == '}') 

                    { 

                        if (closeBraceFound) 

                        { 

                            if (prevIsCloseBrace) 

                            { 

                                sb.Append("}}"); 

                                closeBraceFound = false; 

                                prevIsCloseBrace = false; 

                            } 

                            else 

                                throw new Exception( 

                                    string.Format("Неожиданная закрывающая скобка '}}' (символ {0})", symbolNum)); 

                        } 

                        else 

                        { 

                            closeBraceFound = true; 

                            prevIsCloseBrace = true; 

                        } 

                    } 

                    // обычный символ 


                    else 

                    { 

                        // если ранее была найдена '{' 

                        if (openBraceFound) 

                        { 

                            // если ранее была найдена '}' 

                            if (closeBraceFound) 

                            { 

                                // закончился ключ сообщения '{message}'  

                                if (currentKey.Length == 0) 

                                    throw new Exception(string.Format("Пустой ключ (символ {0})", symbolNum - 1)); 

 

 

                                var msgKey = currentKey.ToString(); 

                                var formatSpec = string.Empty; 

 

 

                                if(msgKey.Contains(":")) 

                                { 

                                    string[] s = msgKey.Split(":".ToCharArray(), 2); 

                                    msgKey = s[0]; 

                                    formatSpec = s[1]; 

                                } 

 

 

                                var msgKeyIndex = keys.IndexOf(msgKey); 

                                if (msgKeyIndex == -1) 

                                { 

                                    keys.Add(msgKey); 

                                    msgKeyIndex = keys.Count - 1; 

                                } 

 

 

                                sb.Append('{'); 

                                sb.Append(msgKeyIndex); 

                                if(formatSpec.Length > 0) 

                                { 

                                    sb.AppendFormat(":{0}", formatSpec); 

                                } 

                                sb.Append('}'); 

 

 

                                openBraceFound = false; 

                                closeBraceFound = false; 

 

 

                                // и идет следующий символ 

                                sb.Append(ch); 

                            } 

                            else 


                                // символ ключа сообщения 

                                currentKey.Append(ch); 

                        } 

                        else if (closeBraceFound) 

                            // встретили одиночную '}' 

                            throw new Exception( 

                                string.Format("Неожиданная закрывающия скобка '}}' (символ {0})", symbolNum - 1)); 

                        else 

                            sb.Append(ch); 

 

 

                        prevIsOpenBrace = false; 

                        prevIsCloseBrace = false; 

                    } 

                } 

 

 

                // если ранее была найдена '{', но не была найдена '}' 

                if (openBraceFound && !closeBraceFound) 

                    throw new Exception("Нет закрывающей скобки '}' в конце строки"); 

 

 

                // если ранее была найдена '}', но не была найдена '{' 

                if (!openBraceFound && closeBraceFound) 

					throw new Exception("Неожиданная закрывающия скобка '}' в конце строки"); 

 

 

                // если ранее была найдена '{' и последний символ - '}' 

                if (openBraceFound && closeBraceFound) 

                { 

                    // закончился ключ сообщения '{message}'  

                    if (currentKey.Length == 0) 

                        throw new Exception(string.Format("Пустой ключ (символ {0})", symbolNum)); 

 

 

                    var msgKey = currentKey.ToString(); 

                    var msgKeyIndex = keys.IndexOf(msgKey); 

                    if (msgKeyIndex == -1) 

                    { 

                        keys.Add(msgKey); 

                        msgKeyIndex = keys.Count - 1; 

                    } 

 

 

                    sb.Append('{'); 

                    sb.Append(msgKeyIndex); 

                    sb.Append('}'); 

                } 

 

 


                return sb.ToString(); 

            } 

            catch (Exception ex) 

            { 

                throw new Exception("Ошибка разбора строки формата: " + ex.Message, ex); 

            } 

        } 

    } 

}


