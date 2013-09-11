using System; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Scanner.Config 

{ 

    /// <summary> 

    /// Коллекция ошибок 

    /// </summary> 

    public class ErrorConfigCollection : ConfigurationElementCollection 

    { 

        protected override ConfigurationElement CreateNewElement() 

        { 

            return new ErrorConfig(); 

        } 

 

 

        protected override Object GetElementKey(ConfigurationElement element) 

        { 

            return ((ErrorConfig)element).Code; 

        } 

 

 

        public ErrorConfig this[int index] 

        { 

            get 

            { 

                return (ErrorConfig)BaseGet(index); 

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

 

 

        public ErrorConfig GetErrorByCode(int code) 

        { 

            return (ErrorConfig)BaseGet((object)code); 

        } 

 

 

        public int IndexOf(ErrorConfig conf) 

        { 

            return BaseIndexOf(conf); 

        } 


 
 

        public void Add(ErrorConfig conf) 

        { 

            BaseAdd(conf); 

        } 

        protected override void BaseAdd(ConfigurationElement element) 

        { 

            BaseAdd(element, false); 

        } 

 

 

        public void Remove(ErrorConfig conf) 

        { 

            if (BaseIndexOf(conf) >= 0) 

                BaseRemove(conf.Code); 

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


