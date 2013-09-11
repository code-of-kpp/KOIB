using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using Croc.Core.Configuration; 
using System.Configuration; 
namespace Croc.Bpc.Synchronization.Config 
{ 
    public class SynchronizationManagerConfig : SubsystemConfig 
    { 
        [ConfigurationProperty("remoteScannerCallProperties", IsRequired = true)] 
        public RemoteScannerCallPropertiesConfig RemoteScannerCallProperties 
        { 
            get 
            { 
                return (RemoteScannerCallPropertiesConfig)this["remoteScannerCallProperties"]; 
            } 
            set 
            { 
                this["remoteScannerCallProperties"] = value; 
            } 
        } 
        [ConfigurationProperty("pingPeriod", IsRequired = true)] 
        public ValueConfig<int> PingPeriod 
        { 
            get 
            { 
                return (ValueConfig<int>)this["pingPeriod"]; 
            } 
            set 
            { 
                this["pingPeriod"] = value; 
            } 
        } 
    } 
}
