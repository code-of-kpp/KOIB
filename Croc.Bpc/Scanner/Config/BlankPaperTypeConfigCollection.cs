using System; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Scanner.Config 

{ 

    public class BlankPaperTypeConfigCollection : ConfigurationElementCollection 

    { 

        protected override ConfigurationElement CreateNewElement() 

        { 

            return new BlankPaperTypeConfig(); 

        } 

 

 

        protected override Object GetElementKey(ConfigurationElement element) 

        { 

            return ((BlankPaperTypeConfig)element).BlankMarker; 

        } 

 

 

        public BlankPaperTypeConfig this[int index] 

        { 

            get 

            { 

                return (BlankPaperTypeConfig)BaseGet(index); 

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

 

 

        /// <summary> 

        /// Получить тип бумаги бланка 

        /// </summary> 

        /// <param name="blankMarker">маркер бланка</param> 

        /// <returns></returns> 

        public PaperType GetPaperTypeByMarker(int blankMarker) 

        { 

            try 

            { 

                var bptConfig = (BlankPaperTypeConfig)BaseGet((object)blankMarker); 

                return bptConfig.PaperType; 

            } 

            catch 


            { 

                return PaperType.None; 

            } 

        } 

 

 

        public int IndexOf(BlankPaperTypeConfig conf) 

        { 

            return BaseIndexOf(conf); 

        } 

 

 

        public void Add(BlankPaperTypeConfig conf) 

        { 

            BaseAdd(conf); 

        } 

        protected override void BaseAdd(ConfigurationElement element) 

        { 

            BaseAdd(element, false); 

        } 

 

 

        public void Remove(BlankPaperTypeConfig conf) 

        { 

            if (BaseIndexOf(conf) >= 0) 

                BaseRemove(conf.BlankMarker); 

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


