using System; 

using Croc.Core.Utils.Collections; 

 

 

namespace Croc.Workflow.ComponentModel 

{ 

    /// <summary> 

    /// Схема потока работ 

    /// </summary> 

    /// <remarks> 

    /// Схема - это набор действий потока работ, переходов между ними, информация о вложенности и т.д. 

    /// </remarks> 

    [Serializable] 

    public class WorkflowScheme 

    { 

        /// <summary> 

        /// Ключ следующего действия, к выполнению которого нужно перейти, 

        /// по умолчанию 

        /// </summary> 

        public NextActivityKey DefaultNextActivityKey 

        { 

            get; 

            set; 

        } 

 

 

        /// <summary> 

        /// Имя корневого действия 

        /// </summary> 

        public string RootActivityName 

        { 

            get; 

            set; 

        } 

 

 

        /// <summary> 

        /// Корневое действие 

        /// </summary> 

        public Activity RootActivity 

        { 

            get 

            { 

                return Activities[RootActivityName]; 

            } 

        } 

 

 

        /// <summary> 

        /// Словарь действий: [полное имя действия, действие] 


        /// </summary> 

        public ByNameAccessDictionary<Activity> Activities 

        { 

            get; 

            private set; 

        } 

 

 

        public WorkflowScheme() 

        { 

            Activities = new ByNameAccessDictionary<Activity>(); 

        } 

    } 

}


