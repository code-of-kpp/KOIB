using System; 
using Croc.Core; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.SystemMenu 
{ 
    [Serializable] 
    public class SystemMenuActivity : BpcCompositeActivity 
    { 
        public NextActivityKey CheckWorkingConfigLoaded( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _configManager.WorkingConfigLoaded 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey AcceptPassword( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var passwordCorrect = (GetTodayPassword() == int.Parse(CommonActivity.LastReadedValue)); 
#if DEBUG 
            return BpcNextActivityKeys.Yes; 
#else 
            return passwordCorrect 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
#endif 
        } 
        private static int GetTodayPassword() 
        { 
            var today = DateTime.Today; 
            return (CoreApplication.Instance.ApplicationVersion.Revision + today.Day*3 + today.Month*2)%10000; 
        } 
    } 
}
