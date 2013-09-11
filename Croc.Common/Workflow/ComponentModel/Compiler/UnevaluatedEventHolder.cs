using System; 

 

 

namespace Croc.Workflow.ComponentModel.Compiler 

{ 

    /// <summary> 

    /// Специальный фиктивный держатель события. Используется, когда действие-владелец события не может быть 

    /// определено на момент парсинга 

    /// </summary> 

    internal class UnevaluatedEventHolder : EventHolder 

    { 

        /// <summary> 

        /// Имя события 

        /// </summary> 

        public new string EventName 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Действие владелец события 

        /// </summary> 

        public new UnevaluatedActivity EventOwner 

        { 

            get; 

            private set; 

        } 

 

 

        internal UnevaluatedEventHolder(string eventName, UnevaluatedActivity eventOwner) 

        { 

            CodeContract.Requires(!string.IsNullOrEmpty(eventName)); 

            CodeContract.Requires(eventOwner != null); 

 

 

            EventName = eventName; 

            EventOwner = eventOwner; 

        } 

    } 

}


