using System.Configuration; 

 

 

namespace Croc.Core.Configuration 

{ 

    /// <summary> 

    /// Конфигурация писателей в протоколы 

    /// </summary> 

    public class WriterConfigCollection : ConfigurationElementCollection 

    { 

        protected override ConfigurationElement CreateNewElement() 

        { 

            return new WriterConfig(); 

        } 

 

 

        protected override object GetElementKey(ConfigurationElement element) 

        { 

            return ((WriterConfig)element).TypeName; 

        } 

    } 

}


