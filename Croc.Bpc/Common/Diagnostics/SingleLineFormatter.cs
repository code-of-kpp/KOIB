using System; 
using System.Collections; 
using System.Collections.Generic; 
using System.Configuration; 
using System.IO; 
using Croc.Core; 
using Croc.Core.Diagnostics; 
using Croc.Core.Utils; 
using Croc.Core.Utils.IO; 
using Croc.Core.Utils.Text; 
namespace Croc.Bpc.Diagnostics 
{ 
    public class SingleLineFormatter : IEventFormatter 
    { 
        private const string DEFAULT_FORMAT =  
            "{" + LoggerEvent.EVENTTYPE_PROPERTY + "}," + 
            "{" + LoggerEvent.TIMESTAMP_PROPERTY + "}," + 
            "{" + LoggerEvent.MESSAGE_PROPERTY + "}," + 
            "{" + LoggerEvent.PARAMETERS_PROPERTY + "}," + 
            "{" + LoggerEvent.EXCEPTION_PROPERTY + "}"; 
        private const string PAR_FORMAT = "format"; 
        private const string PAR_ARRAY_DELIMITER = "arrayDelimiter"; 
        private const string PAR_SINGLE_FILE_PROPS = "propsToSaveInSingleFile"; 
        private string _userFormat = DEFAULT_FORMAT; 
        private string _format; 
        private List<string> _messageNames; 
        private readonly List<string> _singleFileProps = new List<string>(); 
        private ObjectDumperSettings dumperSetting = new ObjectDumperSettings(); 
        private string SavePropToFile(string messageType, object prop) 
        { 
            var logFolder = Path.Combine(CoreApplication.Instance.LogFileFolder, messageType); 
            FileUtils.EnsureDirExists(logFolder); 
            var file = FileUtils.GetUniqueName(logFolder, messageType, "log", 6); 
            var fullFilePath = Path.Combine(logFolder, file); 
            File.WriteAllText(fullFilePath, ObjectDumper.DumpObject(prop, dumperSetting)); 
            return Path.GetFileNameWithoutExtension(fullFilePath); 
        } 
        public string Format(LoggerEvent loggerEvent) 
        { 
            var args = new object[_messageNames.Count]; 
            for (int i = 0; i < _messageNames.Count; ++i) 
            { 
                args[i] = string.Empty; 
                if (loggerEvent.Properties.ContainsKey(_messageNames[i])) 
                { 
                    object arg = loggerEvent[_messageNames[i]]; 
                    if (_singleFileProps.Contains(_messageNames[i])) 
                        arg = SavePropToFile(_messageNames[i], arg); 
                    if (!(arg is string) && (arg is IEnumerable)) 
                    { 
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
                _format = StringFormatParser.Parse(DEFAULT_FORMAT, out _messageNames); 
                CoreApplication.Instance.Logger.LogWarning(Message.Common_SingleLineFormatterFormatError, ex, _userFormat); 
                return Format(loggerEvent); 
            } 
        } 
        public void Init(NameValueConfigurationCollection props) 
        { 
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
            if (props[PAR_ARRAY_DELIMITER] != null && !String.IsNullOrEmpty(props[PAR_ARRAY_DELIMITER].Value)) 
            { 
                dumperSetting.EnumerableDelimiter = props[PAR_ARRAY_DELIMITER].Value; 
            } 
            if(props[PAR_SINGLE_FILE_PROPS] != null && !String.IsNullOrEmpty(props[PAR_SINGLE_FILE_PROPS].Value)) 
            { 
                _singleFileProps.Clear(); 
                _singleFileProps.AddRange(props[PAR_SINGLE_FILE_PROPS].Value.Split(';')); 
            } 
        } 
    } 
}
