using System; 
using System.Collections.Specialized; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.Cancelation 
{ 
    [Serializable] 
    public class CancelationActivity : ElectionEnumeratorActivity 
    { 
        public ListDictionary SourceDataReportParameters 
        { 
            get 
            { 
                return new ListDictionary 
                           { 
                               {"SourceData", _electionManager.SourceData}, 
                               {"UIK", _electionManager.SourceData.Uik} 
                           }; 
            } 
        } 
        public NextActivityKey SaveChanges( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _electionManager.RaiseStateChanged(); 
            return context.DefaultNextActivityKey; 
        } 
    } 
}
