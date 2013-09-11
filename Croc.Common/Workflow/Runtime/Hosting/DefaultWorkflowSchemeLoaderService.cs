using System; 

using System.Collections.Generic; 

using Croc.Workflow.ComponentModel; 

using Croc.Workflow.ComponentModel.Compiler; 

using System.Xml; 

 

 

namespace Croc.Workflow.Runtime.Hosting 

{ 

    /// <summary> 

    /// Загрузчик схемы потока работ по умолчанию 

    /// </summary> 

    public class DefaultWorkflowSchemeLoaderService : WorkflowSchemeLoaderService 

    { 

        public DefaultWorkflowSchemeLoaderService() 

        { 

        } 

 

 

        /// <summary> 

        ///  

        /// </summary> 

        /// <returns></returns> 

        /// <exception cref="WorkflowSchemeParsingException">ошибка разбора схемы потока работ</exception> 

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


