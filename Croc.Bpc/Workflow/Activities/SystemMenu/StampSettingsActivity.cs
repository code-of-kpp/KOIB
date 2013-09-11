using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.SystemMenu 
{ 
    public class StampSettingsActivity : BpcCompositeActivity 
    { 
        public NextActivityKey IsStampControlEnabled( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _recognitionManager.StampControlEnabled 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey SwitchStampControl( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _recognitionManager.StampControlEnabled = !_recognitionManager.StampControlEnabled; 
            return context.DefaultNextActivityKey; 
        } 
    } 
}
