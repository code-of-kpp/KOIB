using System; 
using System.Collections.Generic; 
using System.Text; 
namespace Croc.Core.Utils.Collections 
{ 
    [Serializable] 
    public class ListStack<T> : List<T> where T : class 
    { 
        public int TopIndex 
        { 
            get 
            { 
                return Count - 1; 
            } 
        } 
        public T Peek() 
        { 
            if (Count == 0) 
                throw new InvalidOperationException("Стек пуст"); 
            return base[TopIndex]; 
        } 
        public T Pop() 
        { 
            if (Count == 0) 
                throw new InvalidOperationException("Стек пуст"); 
            var top = base[TopIndex]; 
            RemoveAt(TopIndex); 
            return top; 
        } 
        public void Push(T item) 
        { 
            CodeContract.Requires(item != null); 
            Add(item); 
        } 
        public override string ToString() 
        { 
            var sb = new StringBuilder(Count * 20); 
            foreach (var item in this) 
            { 
                sb.Append(item); 
                sb.Append(','); 
            } 
            if (sb.Length > 0) 
                sb.Length -= 1; 
            return sb.ToString(); 
        } 
    } 
}
