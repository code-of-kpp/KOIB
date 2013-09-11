using System; 
using System.Collections.Generic; 
using System.Configuration; 
using System.Linq; 
namespace Croc.Core.Configuration 
{ 
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
        public List<T> ToList() 
        { 
            return new List<T>(from ValueConfig<T> item in this select item.Value); 
        } 
    } 
}
