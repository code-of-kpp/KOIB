namespace Croc.Bpc.Workflow.Activities.OperatorMenu 
{ 
    public class ViewVersionActivity : BpcCompositeActivity 
    { 
        public string Version 
        { 
            get  
            { 
                return Core.CoreApplication.Instance.ApplicationVersion.ToString(); 
            } 
        } 
    } 
}
