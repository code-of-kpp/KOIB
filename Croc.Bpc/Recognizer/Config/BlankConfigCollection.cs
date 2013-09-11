using System; 

using System.Configuration; 

using Croc.Bpc.Election.Voting; 

 

 

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

            return ((BlankConfig)element).Type; 

        } 

 

 

        public BlankConfig this[int index] 

        { 

            get 

            { 

                return (BlankConfig)BaseGet(index); 

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

 

 

        public BlankConfig this[BlankType blankType] 

        { 

            get 

            { 

                return (BlankConfig)BaseGet(blankType); 

            } 

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


