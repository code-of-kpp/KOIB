using Croc.Workflow.ComponentModel; 

 

 

namespace Croc.Bpc.Workflow.Activities.SystemMenu 

{ 

    public class ResetSoftActivity : BpcCompositeActivity 

	{ 

        public NextActivityKey ResetSoft( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

		{ 

			// сбросим ПО 

			_syncManager.ResetSoft(); 

 

 

			// в принципе приложение завершит работу ранее, но вернуть что-то надо  

			return BpcNextActivityKeys.Yes; 

		} 

	} 

}


