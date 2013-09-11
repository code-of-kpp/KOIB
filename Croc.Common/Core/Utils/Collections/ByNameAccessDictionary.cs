using System; 
using System.Collections.Generic; 
using System.Runtime.Serialization; 
namespace Croc.Core.Utils.Collections 
{ 
    public interface INamed 
    { 
        string Name { get; } 
    } 
    [Serializable] 
    public class ByNameAccessDictionary<TValue> : Dictionary<string, TValue> 
        where TValue : INamed 
    { 
        public ByNameAccessDictionary() : base() 
        { 
        } 
        public ByNameAccessDictionary(SerializationInfo info, StreamingContext context) 
            : base(info, context) 
        { 
        } 
        public void Add(TValue item) 
        { 
            base.Add(item.Name, item); 
        } 
        public bool Remove(TValue item) 
        { 
            return base.Remove(item.Name); 
        } 
        public bool Contains(TValue item) 
        { 
            return this.ContainsKey(item.Name); 
        } 
        public bool Contains(string itemName) 
        { 
            return this.ContainsKey(itemName); 
        } 
    } 
}
