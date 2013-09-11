using System.Collections.Generic; 
using System.Xml; 
using Croc.Core; 
using Croc.Workflow.ComponentModel; 
using Croc.Workflow.ComponentModel.Compiler; 
namespace Croc.Workflow.Runtime.Hosting 
{ 
    public class DefaultWorkflowSchemeLoaderService : WorkflowSchemeLoaderService 
    { 
        protected internal override WorkflowScheme CreateInstance( 
            string workflowSchemeUri, IEnumerable<KeyValuePair<string, XmlReader>> customXmlSchemas) 
        { 
            CodeContract.Requires(!string.IsNullOrEmpty(workflowSchemeUri)); 
            var parser = new WorkflowSchemeParser(); 
            parser.Parse(workflowSchemeUri, customXmlSchemas); 
            return parser.Scheme; 
        } 
    } 
}
