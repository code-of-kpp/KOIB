using System; 
using System.Configuration; 
using System.Collections.Specialized; 
namespace Croc.Core.Configuration 
{ 
    public class SettingConfigCollection : ConfigurationElementCollection 
    { 
        protected override ConfigurationElement CreateNewElement() 
        { 
            return new SettingConfig(); 
        } 
        protected override Object GetElementKey(ConfigurationElement element) 
        { 
            return ((SettingConfig)element).Key; 
        } 
        public SettingConfig this[int index] 
        { 
            get 
            { 
                return (SettingConfig)BaseGet(index); 
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
        new public SettingConfig this[string name] 
        { 
            get 
            { 
                return (SettingConfig)BaseGet(name); 
            } 
        } 
        public int IndexOf(SettingConfig setting) 
        { 
            return BaseIndexOf(setting); 
        } 
        public void Add(SettingConfig setting) 
        { 
            BaseAdd(setting); 
        } 
        protected override void BaseAdd(ConfigurationElement element) 
        { 
            BaseAdd(element, false); 
        } 
        public void Remove(SettingConfig setting) 
        { 
            if (BaseIndexOf(setting) >= 0) 
                BaseRemove(setting.Key); 
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
        public NameValueCollection ToNameValueCollection() 
        { 
            var res = new NameValueCollection(Count); 
            foreach (SettingConfig item in this) 
                res.Add(item.Key, item.Value); 
            return res; 
        } 
    } 
}
