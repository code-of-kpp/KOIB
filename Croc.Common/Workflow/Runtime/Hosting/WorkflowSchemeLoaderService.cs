using System; 

using System.Collections.Generic; 

using Croc.Workflow.ComponentModel; 

using System.Xml; 

 

 

namespace Croc.Workflow.Runtime.Hosting 

{ 

    /// <summary> 

    /// Базовый сервис загрузки схемы потока работ 

    /// </summary> 

    public abstract class WorkflowSchemeLoaderService : WorkflowRuntimeService 

    { 

        protected WorkflowSchemeLoaderService() 

        { 

        } 

 

 

        /// <summary> 

        /// Создать схему потока работ 

        /// </summary> 

        /// <param name="workflowSchemeUri">путь к файлу со схемой</param> 

        /// <param name="customXmlSchemas">список пользовательских xsd-схем</param> 

        /// <returns></returns> 

        protected internal abstract WorkflowScheme CreateInstance( 

            string workflowSchemeUri, IEnumerable<KeyValuePair<string, XmlReader>> customXmlSchemas); 

    } 

}


