using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.OperatorMenu 
{ 
    public class VolumeSettingsActivity : BpcCompositeActivity 
    { 
        public short CurrentVolume 
        { 
            get 
            { 
                return _soundManager.GetVolume(); 
            } 
        } 
        public NextActivityKey SetNewVolume( 
           WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            short volume; 
            if (!short.TryParse(CommonActivity.LastReadedValue, out volume)) 
                return BpcNextActivityKeys.No; 
            _soundManager.SetVolume(volume > 100 ? (short) 100 : volume); 
            return BpcNextActivityKeys.Yes; 
        } 
    } 
}
