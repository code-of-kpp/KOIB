using System; 
using System.Configuration; 
using Croc.Bpc.Config; 
namespace Croc.Bpc.Printing.Config 
{ 
    public class CommandsConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("beforePrinting", IsRequired = false)] 
        public CommandConfig BeforePrinting 
        { 
            get 
            { 
                return (CommandConfig)this["beforePrinting"]; 
            } 
            set 
            { 
                this["beforePrinting"] = value; 
            } 
        } 
        [ConfigurationProperty("afterPrinting", IsRequired = false)] 
        public CommandConfig AfterPrinting 
        { 
            get 
            { 
                return (CommandConfig)this["afterPrinting"]; 
            } 
            set 
            { 
                this["afterPrinting"] = value; 
            } 
        } 
    } 
}
