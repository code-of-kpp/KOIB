using System; 

using System.Collections; 

using System.Collections.Generic; 

using System.Configuration; 

using System.Text; 

using Croc.Core; 

using Croc.Core.Diagnostics; 

using Croc.Core.Utils; 

using Croc.Core.Utils.Text; 

 

 

namespace Croc.Bpc.Common.Diagnostics 

{ 

    /// <summary> 

    /// Форматирование событий диагностики в одну строку 

    /// </summary> 

    public class SingleLineFormatter : IEventFormatter 

    { 

        /// <summary> 

        /// Форматная строка для вывода (по умолчанию) 

        /// </summary> 

        private const string DEFAULT_FORMAT =  

            "{" + LoggerEvent.EVENTTYPE_PROPERTY + "}," + 

            "{" + LoggerEvent.TIMESTAMP_PROPERTY + "}," + 

            "{" + LoggerEvent.MESSAGE_PROPERTY + "}," + 

            "{" + LoggerEvent.PARAMETERS_PROPERTY + "}," + 

            "{" + LoggerEvent.EXCEPTION_PROPERTY + "}"; 

 

 

        /// <summary> 

        /// Ключ конфигурации, задающий формат 

        /// </summary> 

        private const string PAR_FORMAT = "format"; 

        /// <summary> 

        /// Ключ конфигурации, задающий разделители при выводе массивов 

        /// </summary> 

        private const string PAR_ARRAY_DELIMITER = "arrayDelimiter"; 

 

 

        /// <summary> 

        /// Исходная строка форматирования 

        /// </summary> 

        private string _userFormat = DEFAULT_FORMAT; 

        /// <summary> 

        /// Форматная строка для вывода 

        /// </summary> 

        private string _format; 

        /// <summary> 

        /// Названия свойств события, которые будут выведены в порядке появления в строке 

        /// </summary> 


        private List<string> _messageNames; 

        /// <summary> 

        /// Настройки дампера объектов (для массивов) 

        /// </summary> 

        private ObjectDumperSettings dumperSetting = new ObjectDumperSettings(); 

 

 

 

 

        public string Format(LoggerEvent loggerEvent) 

        { 

            var args = new object[_messageNames.Count]; 

 

 

            for (int i = 0; i < _messageNames.Count; ++i) 

            { 

                // если не найдем, то значением параметра будет пустая строка 

                args[i] = string.Empty; 

 

 

                if (loggerEvent.Properties.ContainsKey(_messageNames[i])) 

                { 

                    object arg = loggerEvent[_messageNames[i]]; 

 

 

                    if (!(arg is string) && (arg is IEnumerable)) 

                    { 

                        // обработаем специальным образом массивы (кроме строк) 

                        args[i] = ObjectDumper.DumpObject(arg, dumperSetting); 

                    } 

                    else 

                    { 

                        args[i] = arg; 

                    } 

                } 

                else 

                { 

                    switch(_messageNames[i]) 

                    { 

                        case LoggerEvent.EVENTTYPE_PROPERTY: 

                            args[i] = loggerEvent.EventType; 

                            break; 

                    } 

                } 

            } 

            try 

            { 

                return string.Format(_format, args); 

            } 

            catch (Exception ex) 


            { 

                // в случае ошибки сбрасываем формат 

                _format = StringFormatParser.Parse(DEFAULT_FORMAT, out _messageNames); 

                // выводим сообщение в журнал приложения 

                CoreApplication.Instance.Logger.LogException(Message.SingleLineFormatterFormatError, ex, _userFormat); 

                // и выполняем повторное форматирование с форматом по умолчанию 

                return Format(loggerEvent); 

            } 

        } 

 

 

        public void Init(NameValueConfigurationCollection props) 

        { 

            // строка форматирования 

            if (props[PAR_FORMAT] != null && !String.IsNullOrEmpty(props[PAR_FORMAT].Value)) 

            { 

                _userFormat = props[PAR_FORMAT].Value; 

                _format = StringFormatParser.Parse(_userFormat, out _messageNames); 

                _format = _format.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t"); 

            } 

            else 

            { 

                _format = StringFormatParser.Parse(DEFAULT_FORMAT, out _messageNames); 

            } 

 

 

            // разделитель элементов массивов 

            if (props[PAR_ARRAY_DELIMITER] != null && !String.IsNullOrEmpty(props[PAR_ARRAY_DELIMITER].Value)) 

            { 

                dumperSetting.EnumerableDelimiter = props[PAR_ARRAY_DELIMITER].Value; 

            } 

        } 

    } 

}


