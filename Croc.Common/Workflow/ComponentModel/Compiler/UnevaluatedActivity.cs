namespace Croc.Workflow.ComponentModel.Compiler 
{ 
    internal class UnevaluatedActivity : Activity 
    { 
        public string ActivityName 
        { 
            get; 
            private set; 
        } 
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
