using System; 

 

 

namespace Croc.Workflow.ComponentModel.Compiler 

{ 

    /// <summary> 

    /// Cпециальное фиктивное действие, которое является признаком того,  

    /// что найтоящее действие еще не определено  

    /// </summary> 

    internal class UnevaluatedActivity : Activity 

    { 

        /// <summary> 

        /// Имя действия, которое еще не определено 

        /// </summary> 

        /// <remarks>это может быть как локальное имя, так и полное имя</remarks> 

        public string ActivityName 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Родительское составное действие 

        /// </summary> 

        /// <remarks>нужно для того, чтобы получить полное имя действия в случае,  

        /// когда ActivityName - это локальное имя</remarks> 

        public Activity ParentActivity 

        { 

            get; 

            private set; 

        } 

 

 

        public UnevaluatedActivity(string activityName, Activity parentActivity) 

        { 

            ActivityName = activityName; 

            ParentActivity = parentActivity; 

        } 

    } 

}


