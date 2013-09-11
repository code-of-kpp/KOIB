using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Runtime.Serialization; 

 

 

namespace Croc.Core.Utils.Collections 

{ 

    /// <summary> 

    /// Интерфейс для получения имени сущности 

    /// </summary> 

    public interface INamed 

    { 

        /// <summary> 

        /// Имя 

        /// </summary> 

        string Name { get; } 

    } 

 

 

    /// <summary> 

    /// Словарь именованных объектов, т.е. объектов, у кот. есть имя. 

    /// Ключ = имя объекта 

    /// Значение = объект 

    /// </summary> 

    /// <typeparam name="TValue"></typeparam> 

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

 

 

        /// <summary> 

        /// Добавление нового элемента в словарь 

        /// </summary> 

        /// <param name="item"></param> 

        public void Add(TValue item) 

        { 

            base.Add(item.Name, item); 

        } 


 
 

        /// <summary> 

        /// Удаляет элемент из словаря 

        /// </summary> 

        /// <param name="item"></param> 

        /// <returns></returns> 

        public bool Remove(TValue item) 

        { 

            return base.Remove(item.Name); 

        } 

 

 

        /// <summary> 

        /// Проверяет, если ли заданный элемент в словаре 

        /// </summary> 

        /// <param name="item"></param> 

        /// <returns></returns> 

        public bool Contains(TValue item) 

        { 

            return this.ContainsKey(item.Name); 

        } 

 

 

        /// <summary> 

        /// Проверяет, если ли элемент с заданным именем в словаре 

        /// </summary> 

        /// <param name="item"></param> 

        /// <returns></returns> 

        public bool Contains(string itemName) 

        { 

            return this.ContainsKey(itemName); 

        } 

    } 

}


