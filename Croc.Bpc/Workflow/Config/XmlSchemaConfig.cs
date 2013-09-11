using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using System.Configuration; 
namespace Croc.Bpc.Workflow.Config 
{ 
    public class XmlSchemaConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("targetNamespace", IsRequired = true)] 
        public string TargetNamespace 
        { 
            get 
            { 
                return (string)this["targetNamespace"]; 
            } 
            set 
            { 
                this["targetNamespace"] = value; 
            } 
        } 
        [ConfigurationProperty("uri", IsRequired = true)] 
        public string Uri 
        { 
            get 
            { 
                return (string)this["uri"]; 
            } 
            set 
            { 
                this["uri"] = value; 
            } 
        } 
    } 
}
