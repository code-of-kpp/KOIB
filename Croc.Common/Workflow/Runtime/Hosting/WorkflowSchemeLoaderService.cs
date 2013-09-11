using System.Collections.Generic; 
using System.Xml; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Workflow.Runtime.Hosting 
{ 
    public abstract class WorkflowSchemeLoaderService : WorkflowRuntimeService 
    { 
        protected internal abstract WorkflowScheme CreateInstance( 
            string workflowSchemeUri, IEnumerable<KeyValuePair<string, XmlReader>> customXmlSchemas); 
    } 
}
