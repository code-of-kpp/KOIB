using System.Collections.Specialized; 
namespace Croc.Bpc.Workflow.Activities.SystemMenu 
{ 
    public class PrintSDActivity : BpcCompositeActivity 
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
    } 
}
