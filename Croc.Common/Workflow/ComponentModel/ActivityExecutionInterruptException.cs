using System; 

 

 

namespace Croc.Workflow.ComponentModel 

{ 

    /// <summary> 

    /// Исключение, возникающее при остановке или прерывании выполнения действия 

    /// </summary> 

    public sealed class ActivityExecutionInterruptException : ActivityExecutionException 

    { 

        public ActivityExecutionInterruptException(Activity activity) 

            : base("Прерывание выполнения действия", activity, null) 

        { 

        } 

 

 

        public ActivityExecutionInterruptException(string message, Activity activity) 

            : base(message, activity, null) 

        { 

        } 

    } 

}


