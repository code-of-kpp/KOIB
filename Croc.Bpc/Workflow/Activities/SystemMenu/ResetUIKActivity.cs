using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Workflow.ComponentModel; 

 

 

namespace Croc.Bpc.Workflow.Activities.SystemMenu 

{ 

	public class ResetUIKActivity : BpcCompositeActivity 

	{ 

		/// <summary> 

		/// Сброс УИК 

		/// </summary> 

		/// <param name="context"></param> 

		/// <param name="parameters"></param> 

		/// <returns></returns> 

		public NextActivityKey ResetUIK 

			(WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

		{ 

			// выключим синхронизацию 

			_syncManager.SynchronizationEnabled = false; 

			// сбросим ПО на удаленном 

			_syncManager.RemoteScanner.ResetSoft(); 

			// сбросим ПО на нашем сканере 

			_syncManager.ResetSoft(); 

 

 

			// в принципе приложение завершит работу ранее, но вернуть что-то надо  

			return BpcNextActivityKeys.Yes; 

		} 

	} 

}


