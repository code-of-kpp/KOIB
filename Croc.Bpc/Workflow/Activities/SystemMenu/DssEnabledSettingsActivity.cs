using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.SystemMenu 
{ 
    public class DssEnabledSettingsActivity : BpcCompositeActivity 
    { 
        public NextActivityKey IsDssEnabled( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _scannerManager.DoubleSheetSensorEnabled 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey SwitchDssEnabled( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _scannerManager.DoubleSheetSensorEnabled = !_scannerManager.DoubleSheetSensorEnabled; 
            return context.DefaultNextActivityKey; 
        } 
    } 
}
