using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Configuration; 

 

 

namespace Croc.Core.Configuration 

{ 

    /// <summary> 

    /// Коллекция конфиг-элементов типа ValueConfig 

    /// </summary> 

    public class ValueConfigCollection<T> : ConfigurationElementCollection 

    { 

        protected override ConfigurationElement CreateNewElement() 

        { 

            return new ValueConfig<T>(); 

        } 

 

 

        protected override Object GetElementKey(ConfigurationElement element) 

        { 

            return ((ValueConfig<T>)element).Value; 

        } 

 

 

        /// <summary> 

        /// Получить список значений конфиг-элементов, которые входят в коллекцию 

        /// </summary> 

        /// <returns></returns> 

        public List<T> ToList() 

        { 

            var res = new List<T>(this.Count); 

 

 

            foreach (ValueConfig<T> item in this) 

                res.Add(item.Value); 

 

 

            return res; 

        } 

    } 

}


