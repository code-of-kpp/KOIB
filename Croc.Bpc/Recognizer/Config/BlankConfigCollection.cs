using System; 
using System.Configuration; 
using Croc.Bpc.Scanner; 
using Croc.Bpc.Voting; 
namespace Croc.Bpc.Recognizer.Config 
{ 
    public class BlankConfigCollection : ConfigurationElementCollection 
    { 
        protected override ConfigurationElement CreateNewElement() 
        { 
            return new BlankConfig(); 
        } 
        protected override Object GetElementKey(ConfigurationElement element) 
        { 
            var bc = (BlankConfig) element; 
            return GetKey(bc.Type, bc.SheetType); 
        } 
        private static object GetKey(BlankType blankType, SheetType sheetType) 
        { 
            return ((int)blankType) * 1000 + (int)sheetType; 
        } 
        public BlankConfig Get(BlankType blankType) 
        { 
            return Get(blankType, SheetType.Undefined); 
        } 
        public BlankConfig Get(BlankType blankType, SheetType sheetType) 
        { 
            return  
                (BlankConfig) BaseGet(GetKey(blankType, sheetType)) 
                ?? 
                (BlankConfig) BaseGet(GetKey(blankType, SheetType.Undefined)); 
        } 
        public int IndexOf(BlankConfig conf) 
        { 
            return BaseIndexOf(conf); 
        } 
        public void Add(BlankConfig conf) 
        { 
            BaseAdd(conf); 
        } 
        protected override void BaseAdd(ConfigurationElement element) 
        { 
            BaseAdd(element, false); 
        } 
        public void Remove(BlankConfig conf) 
        { 
            if (BaseIndexOf(conf) >= 0) 
                BaseRemove(conf.Type); 
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
