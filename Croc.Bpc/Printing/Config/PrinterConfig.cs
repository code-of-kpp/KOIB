using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using System.Configuration; 
namespace Croc.Bpc.Printing.Config 
{ 
    public class PrinterConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("name", IsRequired = true)] 
        public string Name 
        { 
            get 
            { 
                return (string)this["name"]; 
            } 
            set 
            { 
                this["name"] = value; 
            } 
        } 
        [ConfigurationProperty("searchRegExp", IsRequired = true)] 
        public string SearchRegExp 
        { 
            get 
            { 
                return (string)this["searchRegExp"]; 
            } 
            set 
            { 
                this["searchRegExp"] = value; 
            } 
        } 
    } 
}
