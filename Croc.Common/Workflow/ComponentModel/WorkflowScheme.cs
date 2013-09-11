using System; 
using Croc.Core.Utils.Collections; 
namespace Croc.Workflow.ComponentModel 
{ 
    [Serializable] 
    public class WorkflowScheme 
    { 
        public NextActivityKey DefaultNextActivityKey 
        { 
            get; 
            set; 
        } 
        public string RootActivityName 
        { 
            get; 
            set; 
        } 
        public Activity RootActivity 
        { 
            get 
            { 
                return Activities[RootActivityName]; 
            } 
        } 
        public Activity ExitActivity 
        { 
            get;  
            internal set; 
        } 
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
