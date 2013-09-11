using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Workflow.ComponentModel; 

using Croc.Core.Diagnostics; 

 

 

namespace Croc.Bpc.Workflow.Activities 

{ 

    /// <summary> 

    /// Действие-заглушка 

    /// </summary> 

    [Serializable] 

    public class MockActivity : BpcCompositeActivity 

    { 

        public NextActivityKey Mock( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _logger.LogInfo(Message.Debug, parameters.GetParamValue("Text", "-")); 

            context.Sleep(TimeSpan.FromMilliseconds(500)); 

            return context.DefaultNextActivityKey; 

        } 

    } 

}


