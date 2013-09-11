namespace Croc.Bpc.Workflow.Activities.OperatorMenu 
{ 
    public class ViewIpAddressActivity : BpcCompositeActivity 
    { 
        public string IpAddress 
        { 
            get  
            { 
                return _scannerManager.IPAddress; 
            } 
        } 
    } 
}
