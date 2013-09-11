using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Bpc.Synchronization 

{ 

    /// <summary> 

    /// Элемент состояния 

    /// </summary> 

    [Serializable] 

    public class StateItem 

    { 

        /// <summary> 

        /// Имя элемента состояния 

        /// </summary> 

        public readonly string Name; 

 

 

        private object _value; 

        /// <summary> 

        /// Значение 

        /// </summary> 

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

 

 

        private bool _synchronized = false; 

        /// <summary> 

        /// Элемент состояния синхронизирован? 

        /// </summary> 

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

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="name">имя элемента состояния</param> 

        public StateItem(string name) 

        { 

            CodeContract.Requires(!string.IsNullOrEmpty(name)); 

            Name = name; 

        } 

 

 

        /// <summary> 

        /// Возвращает путой клон данного элемента-состояния, у которого 

        /// проставлен признак синхронизированности 

        /// </summary> 

        /// <returns></returns> 

        public StateItem GetSynchronizedEmptyClone() 

        { 

            return new StateItem(Name) { Synchronized = true }; 

        } 

    } 

}


