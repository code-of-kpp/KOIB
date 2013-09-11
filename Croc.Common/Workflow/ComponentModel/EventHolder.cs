using System; 

using System.Reflection; 

 

 

namespace Croc.Workflow.ComponentModel 

{ 

    /// <summary> 

    /// Держатель события. Используется для хранения информации о событии внутри действия 

    /// и для того, чтобы можно было подписываться (отписываться) на данное событие. 

    /// </summary> 

    [Serializable] 

    public class EventHolder 

    { 

        /// <summary> 

        /// Информация о событии 

        /// </summary> 

        public EventInfo Event 

        { 

            get; 

            private set; 

        } 

        /// <summary> 

        /// Владелец события 

        /// </summary> 

        public object EventOwner 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Уникальное имя события 

        /// </summary> 

        public string EventName 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Конструктор, который создает пустой объект 

        /// </summary> 

        internal EventHolder() 

        { 

        } 

 

 

        /// <summary> 


        /// Конструтор 

        /// </summary> 

        /// <param name="ev"></param> 

        /// <param name="eventOwner"></param> 

        public EventHolder(EventInfo ev, object eventOwner) 

        { 

            CodeContract.Requires(ev != null); 

            CodeContract.Requires(eventOwner != null); 

 

 

            Event = ev; 

            EventOwner = eventOwner; 

 

 

            EventName = string.Format("{0}#{1}", eventOwner.GetType().FullName, ev.Name); 

        } 

 

 

        /// <summary> 

        /// Добавить обработчик 

        /// </summary> 

        /// <param name="handler"></param> 

        public void AddEventHandler(Delegate handler) 

        { 

            Event.AddEventHandler(EventOwner, handler); 

        } 

 

 

        /// <summary> 

        /// Удалить обработчик 

        /// </summary> 

        /// <param name="handler"></param> 

        public void RemoveEventHandler(Delegate handler) 

        { 

            Event.RemoveEventHandler(EventOwner, handler); 

        } 

    } 

}


