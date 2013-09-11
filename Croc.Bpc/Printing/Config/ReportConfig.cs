using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using System.Configuration; 
using Croc.Core.Configuration; 
namespace Croc.Bpc.Printing.Config 
{ 
    public class ReportConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("font", IsRequired = true)] 
        public FontConfig Font 
        { 
            get 
            { 
                return (FontConfig)this["font"]; 
            } 
            set 
            { 
                this["font"] = value; 
            } 
        } 
        [ConfigurationProperty("margin", IsRequired = true)] 
        public MarginConfig Margin 
        { 
            get 
            { 
                return (MarginConfig)this["margin"]; 
            } 
            set 
            { 
                this["margin"] = value; 
            } 
        } 
        [ConfigurationProperty("debugMode", IsRequired = false)] 
        public ValueConfig<bool> DebugMode 
        { 
            get 
            { 
                return (ValueConfig<bool>)this["debugMode"]; 
            } 
            set 
            { 
                this["debugMode"] = value; 
            } 
        } 
    } 
}
