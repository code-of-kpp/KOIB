using System; 
namespace Croc.Bpc.Workflow.Activities.OperatorMenu 
{ 
    public class ViewDateActivity : BpcCompositeActivity 
    { 
        public DateTime CurrentDate 
        { 
            get  
            { 
                return DateTime.Now; 
            } 
        } 
    } 
}
