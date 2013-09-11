using System; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.SystemMenu 
{ 
    public class PaperSettingsActivity : BpcCompositeActivity 
    { 
        private short _thick; 
        public short CurrentThick 
        { 
            get; private set; 
        } 
        public short CurrentThin 
        {  
            get; private set; 
        } 
        protected override void Initialize(WorkflowExecutionContext context) 
        { 
            base.Initialize(context); 
            short thick, thin; 
            _scannerManager.GetRelativePaperDensity(out thick, out thin); 
            CurrentThick = Math.Abs(thick); 
            CurrentThin = Math.Abs(thin); 
        } 
        public NextActivityKey SaveThick( 
           WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return short.TryParse(CommonActivity.LastReadedValue, out _thick) 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey ApplyNewConfig( 
           WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            short thin; 
            if (!short.TryParse(CommonActivity.LastReadedValue, out thin)) 
                return BpcNextActivityKeys.No; 
            _thick *= -1; 
            _scannerManager.SetRelativePaperDensity(_thick, thin); 
            return BpcNextActivityKeys.Yes; 
        } 
    } 
}
