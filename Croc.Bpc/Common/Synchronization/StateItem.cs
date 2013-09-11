using System; 
using Croc.Core; 
namespace Croc.Bpc.Synchronization 
{ 
    [Serializable] 
    public class StateItem 
    { 
        public readonly string Name; 
        private object _value; 
        public object Value 
        { 
            get 
            { 
                return _value; 
            } 
            set 
            { 
                _value = value; 
                _synchronized = false; 
            } 
        } 
        private bool _synchronized; 
        public bool Synchronized 
        { 
            get 
            { 
                return _synchronized; 
            } 
            set 
            { 
                _synchronized = value; 
            } 
        } 
        public StateItem(string name) 
        { 
            CodeContract.Requires(!string.IsNullOrEmpty(name)); 
            Name = name; 
        } 
        public StateItem GetSynchronizedEmptyClone() 
        { 
            return new StateItem(Name) { Synchronized = true }; 
        } 
    } 
}
