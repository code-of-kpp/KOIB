using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.SystemMenu 
{ 
    public class MarkerSettingsActivity : BpcCompositeActivity 
    { 
        protected override void Initialize(WorkflowExecutionContext context) 
        { 
            base.Initialize(context); 
            _scannerManager.GetMarkerParameters( 
                out _on, out _off, out _markingTime, out _rollbackTime, out _downTime); 
        } 
        #region Параметры маркера и методы сохранения их нового значения 
        private short _on; 
        public short On { get { return _on; } } 
        private short _off; 
        public short Off { get { return _off; } } 
        private short _markingTime; 
        public short MarkingTime { get { return _markingTime; } } 
        private short _rollbackTime; 
        public short RollbackTime { get { return _rollbackTime; } } 
        private short _downTime; 
        public short DownTime { get { return _downTime; } } 
        public NextActivityKey SaveOn( 
           WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            short.TryParse(CommonActivity.LastReadedValue, out _on); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey SaveOff( 
           WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            short.TryParse(CommonActivity.LastReadedValue, out _off); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey SaveMarkingTime( 
           WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            short.TryParse(CommonActivity.LastReadedValue, out _markingTime); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey SaveRollbackTime( 
           WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            short.TryParse(CommonActivity.LastReadedValue, out _rollbackTime); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey SaveDownTime( 
           WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            short.TryParse(CommonActivity.LastReadedValue, out _downTime); 
            return context.DefaultNextActivityKey; 
        } 
        #endregion 
        public NextActivityKey SetNewMarkerSettings( 
           WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _scannerManager.SetMarkerParameters(_on, _off, _markingTime, _rollbackTime, _downTime); 
            return context.DefaultNextActivityKey; 
        } 
    } 
}
