using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using System.Configuration; 
namespace Croc.Bpc.Election.Config 
{ 
    public class PathConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("rootPath", IsRequired = true)] 
        public string RootPath 
        { 
            get 
            { 
                return (string)this["rootPath"]; 
            } 
            set 
            { 
                this["rootPath"] = value; 
            } 
        } 
        [ConfigurationProperty("wildcard", IsRequired = false)] 
        public string Wildcard 
        { 
            get 
            { 
                return (string)this["wildcard"]; 
            } 
            set 
            { 
                this["wildcard"] = value; 
            } 
        } 
    } 
}
