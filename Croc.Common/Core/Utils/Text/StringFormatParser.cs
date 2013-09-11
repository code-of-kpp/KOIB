using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
namespace Croc.Core.Utils.Text 
{ 
    public static class StringFormatParser 
    { 
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
                    else 
                    { 
                        if (openBraceFound) 
                        { 
                            if (closeBraceFound) 
                            { 
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
                                sb.Append(ch); 
                            } 
                            else 
                                currentKey.Append(ch); 
                        } 
                        else if (closeBraceFound) 
                            throw new Exception( 
                                string.Format("Неожиданная закрывающия скобка '}}' (символ {0})", symbolNum - 1)); 
                        else 
                            sb.Append(ch); 
                        prevIsOpenBrace = false; 
                        prevIsCloseBrace = false; 
                    } 
                } 
                if (openBraceFound && !closeBraceFound) 
                    throw new Exception("Нет закрывающей скобки '}' в конце строки"); 
                if (!openBraceFound && closeBraceFound) 
                    throw new Exception("Неожиданная закрывающия скобка '}' в конце строки"); 
                if (openBraceFound && closeBraceFound) 
                { 
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
