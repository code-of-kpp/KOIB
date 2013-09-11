using System; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Scanner.Config 

{ 

    /// <summary> 

    /// Список конфиг-элементов с параметрами сканеров разных версий 

    /// </summary> 

    public class ScannerParametersConfigCollection : ConfigurationElementCollection 

    { 

        protected override ConfigurationElement CreateNewElement() 

        { 

            return new ScannerParametersConfig(); 

        } 

 

 

        protected override Object GetElementKey(ConfigurationElement element) 

        { 

            return ((ScannerParametersConfig)element).VersionName; 

        } 

 

 

        public ScannerParametersConfig this[int index] 

        { 

            get 

            { 

                return (ScannerParametersConfig)BaseGet(index); 

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

 

 

        new public ScannerParametersConfig this[string versionName] 

        { 

            get 

            { 

                return (ScannerParametersConfig)BaseGet(versionName); 

            } 

        } 

 

 

        public int IndexOf(ScannerParametersConfig conf) 


        { 

            return BaseIndexOf(conf); 

        } 

 

 

        public void Add(ScannerParametersConfig conf) 

        { 

            BaseAdd(conf); 

        } 

        protected override void BaseAdd(ConfigurationElement element) 

        { 

            BaseAdd(element, false); 

        } 

 

 

        public void Remove(ScannerParametersConfig conf) 

        { 

            if (BaseIndexOf(conf) >= 0) 

                BaseRemove(conf.VersionName); 

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


