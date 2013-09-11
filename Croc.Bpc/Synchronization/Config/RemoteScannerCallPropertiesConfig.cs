using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using System.Configuration; 
namespace Croc.Bpc.Synchronization.Config 
{ 
    public class RemoteScannerCallPropertiesConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("common", IsRequired = true)] 
        public CallPropertiesConfig Common 
        { 
            get 
            { 
                return (CallPropertiesConfig)this["common"]; 
            } 
            set 
            { 
                this["common"] = value; 
            } 
        } 
        [ConfigurationProperty("synchronization", IsRequired = true)] 
        public CallPropertiesConfig Synchronization 
        { 
            get 
            { 
                return (CallPropertiesConfig)this["synchronization"]; 
            } 
            set 
            { 
                this["synchronization"] = value; 
            } 
        } 
        [ConfigurationProperty("printing", IsRequired = true)] 
        public CallPropertiesConfig Printing 
        { 
            get 
            { 
                return (CallPropertiesConfig)this["printing"]; 
            } 
            set 
            { 
                this["printing"] = value; 
            } 
        } 
    } 
}
