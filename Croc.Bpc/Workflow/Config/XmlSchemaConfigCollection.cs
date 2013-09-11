using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Configuration; 

using System.Xml; 

 

 

namespace Croc.Bpc.Workflow.Config 

{ 

    /// <summary> 

    /// Коллекция ошибок 

    /// </summary> 

    public class XmlSchemaConfigCollection : ConfigurationElementCollection 

    { 

        public List<KeyValuePair<string, XmlReader>> ToList() 

        { 

            var list = new List<KeyValuePair<string, XmlReader>>(); 

            foreach (XmlSchemaConfig item in this) 

                list.Add(new KeyValuePair<string, XmlReader>( 

                    item.TargetNamespace, XmlReader.Create(item.Uri))); 

 

 

            return list; 

        } 

 

 

        protected override ConfigurationElement CreateNewElement() 

        { 

            return new XmlSchemaConfig(); 

        } 

 

 

        protected override Object GetElementKey(ConfigurationElement element) 

        { 

            return ((XmlSchemaConfig)element).TargetNamespace; 

        } 

 

 

        public XmlSchemaConfig this[int index] 

        { 

            get 

            { 

                return (XmlSchemaConfig)BaseGet(index); 

            } 

            set 

            { 

                if (BaseGet(index) != null) 

                { 

                    BaseRemoveAt(index); 


                } 

                BaseAdd(index, value); 

            } 

        } 

 

 

        public int IndexOf(XmlSchemaConfig conf) 

        { 

            return BaseIndexOf(conf); 

        } 

 

 

        public void Add(XmlSchemaConfig conf) 

        { 

            BaseAdd(conf); 

        } 

        protected override void BaseAdd(ConfigurationElement element) 

        { 

            BaseAdd(element, false); 

        } 

 

 

        public void Remove(XmlSchemaConfig conf) 

        { 

            if (BaseIndexOf(conf) >= 0) 

                BaseRemove(conf.TargetNamespace); 

        } 

 

 

        public void RemoveAt(int index) 

        { 

            BaseRemoveAt(index); 

        } 

 

 

        public void Remove(string key) 

        { 

            BaseRemove(key); 

        } 

 

 

        public void Clear() 

        { 

            BaseClear(); 

        } 

    } 

}


