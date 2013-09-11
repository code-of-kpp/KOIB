using System; 
using System.Configuration; 
using System.Xml; 
namespace Croc.Core.Configuration 
{ 
    public class SubsystemConfigCollection : ConfigurationElementCollection 
    { 
        XmlReader _reader; 
        protected override ConfigurationElement CreateNewElement() 
        { 
            string subsystemName = null; 
            string subsystemTypeName = null; 
            string traceLevelName = null; 
            string logFileFolder = null; 
            var separateLog = false; 
            var disposeOrder = SubsystemConfig.DISPOSE_ORDER_UNDEFINED; 
            for (var go = _reader.MoveToFirstAttribute(); go; go = _reader.MoveToNextAttribute()) 
            { 
                switch (_reader.Name) 
                { 
                    case "name": 
                        subsystemName = _reader.Value; 
                        break; 
                    case "type": 
                        subsystemTypeName = _reader.Value; 
                        break; 
                    case "traceLevel": 
                        traceLevelName = _reader.Value; 
                        break; 
                    case "logFileFolder": 
                        logFileFolder = _reader.Value; 
                        break; 
                    case "separateLog": 
                        separateLog = bool.Parse(_reader.Value); 
                        break; 
                    case "disposeOrder": 
                        disposeOrder = int.Parse(_reader.Value); 
                        break; 
                    default: 
                        throw new ConfigurationErrorsException("Неожиданный атрибут: " + _reader.Name, _reader); 
                } 
            } 
            if (string.IsNullOrEmpty(subsystemTypeName)) 
                throw new ConfigurationErrorsException("Не задано имя типа класса подсистемы в атрибуте type", _reader); 
            Type subsystemType; 
            try 
            { 
                subsystemType = Type.GetType(subsystemTypeName, true); 
            } 
            catch (Exception ex) 
            { 
                throw new ConfigurationErrorsException("Ошибка получения типа подсистемы: " + subsystemTypeName, 
                    ex, _reader); 
            } 
            if (string.IsNullOrEmpty(subsystemName)) 
                subsystemName = subsystemType.Name; 
            var atts = subsystemType.GetCustomAttributes(typeof(SubsystemConfigurationElementTypeAttribute), true); 
            SubsystemConfig configElem; 
            if (atts.Length == 0) 
            { 
                configElem = new SubsystemConfig(); 
            } 
            else 
            { 
                var att = (SubsystemConfigurationElementTypeAttribute)atts[0]; 


                try 
                { 
                    configElem = (SubsystemConfig)Activator.CreateInstance(att.Type); 
                } 
                catch (Exception ex) 
                { 
                    throw new ConfigurationErrorsException("Ошибка создания конфигурационного элемента из типа: " 
                        + att.Type.FullName, ex, _reader); 
                } 
            } 
            configElem.SubsystemName = subsystemName; 
            configElem.SubsystemTypeName = subsystemTypeName; 
            configElem.TraceLevelName = traceLevelName; 
            configElem.LogFileFolder = logFileFolder; 
            configElem.SeparateLog = separateLog; 
            configElem.DisposeOrder = disposeOrder; 
            return configElem; 
        } 
        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey) 
        { 
            _reader = reader;             
            base.DeserializeElement(_reader, serializeCollectionKey); 
        } 
        protected override object GetElementKey(ConfigurationElement element) 
        { 
            return ((SubsystemConfig)element).SubsystemName; 
        } 
        public new SubsystemConfig this[string name] 
        { 
            get 
            { 
                return (SubsystemConfig)BaseGet(name); 
            } 
        } 
    } 
}
