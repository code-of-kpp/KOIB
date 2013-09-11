using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using Croc.Core.Configuration; 
using System.Configuration; 
namespace Croc.Bpc.Workflow.Config 
{ 
    public class WorkflowManagerConfig : SubsystemConfig 
    { 
        [ConfigurationProperty("workflowScheme", IsRequired = true)] 
        public WorkflowSchemeConfig WorkflowScheme 
        { 
            get 
            { 
                return (WorkflowSchemeConfig)this["workflowScheme"]; 
            } 
            set 
            { 
                this["workflowScheme"] = value; 
            } 
        } 
    } 
    public class WorkflowSchemeConfig : ConfigurationElement 
    { 
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
        [ConfigurationProperty("xmlSchemaSet", IsDefaultCollection = false, IsRequired = true)] 
        [ConfigurationCollection(typeof(XmlSchemaConfig), AddItemName = "xmlSchema")] 
        public XmlSchemaConfigCollection XmlSchemas 
        { 
            get 
            { 
                return (XmlSchemaConfigCollection)base["xmlSchemaSet"]; 
            } 
        } 
    } 
}
