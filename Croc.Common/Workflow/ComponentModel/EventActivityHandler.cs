using System; 
using Croc.Core; 
using Croc.Workflow.Runtime; 
namespace Croc.Workflow.ComponentModel 
{ 
    [Serializable] 
    internal class EventActivityHandler 
    { 
        [NonSerialized] 
        private WorkflowInstance _workflowInstance; 
        public EventHandler Method 
        { 
            get; 
            private set; 
        } 
        public Activity SyncActivity 
        { 
            get; 
            private set; 
        } 
        public bool ContainsActivities 
        { 
            get 
            { 
                return SyncActivity != null; 
            } 
        } 
        public EventActivityHandler(WorkflowInstance workflowInstance) 
        { 
            CodeContract.Requires(workflowInstance != null); 
            _workflowInstance = workflowInstance; 
            Method = new EventHandler(OnEvent); 
        } 
        internal void SetWorkflowInstance(WorkflowInstance workflowInstance) 
        { 
            CodeContract.Requires(workflowInstance != null); 
            _workflowInstance = workflowInstance; 
        } 
        public void AddActivity(Activity handlerActivity, EventHandlingType handlingType) 
        { 
            switch (handlingType) 
            { 
                case EventHandlingType.Sync: 
                    SyncActivity = handlerActivity; 
                    break; 
                case EventHandlingType.Async: 
                    throw new NotImplementedException(); 
            } 
        } 
        public void RemoveActivity(Activity handlerActivity) 
        { 
            if (SyncActivity == handlerActivity) 
                SyncActivity = null; 
        } 
        private void OnEvent(object sender, EventArgs e) 
        { 
            if (SyncActivity != null) 
                _workflowInstance.GoToActivity(SyncActivity); 
        } 
    } 
}
