using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.SystemMenu 
{ 
    public class DssLevelSettingsActivity : BpcCompositeActivity 
    { 
        private short _left; 
        public short CurrentLeftLevel 
        { 
            get 
            { 
                return _scannerManager.DssLeftLevel; 
            } 
        } 
        public short CurrentRightLevel 
        { 
            get 
            { 
                return _scannerManager.DssRightLevel; 
            } 
        } 
        public NextActivityKey SaveLeft( 
           WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return short.TryParse(CommonActivity.LastReadedValue, out _left) 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey ApplyNewConfig( 
           WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            short right; 
            if (!short.TryParse(CommonActivity.LastReadedValue, out right)) 
                return BpcNextActivityKeys.No; 
            _scannerManager.SetDoubleSheetSensorLevel(_left, right); 
            return BpcNextActivityKeys.Yes; 
        } 
    } 
}
