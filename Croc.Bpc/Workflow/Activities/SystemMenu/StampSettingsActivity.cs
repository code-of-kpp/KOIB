using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Workflow.ComponentModel; 

 

 

namespace Croc.Bpc.Workflow.Activities.SystemMenu 

{ 

	public class StampSettingsActivity : BpcCompositeActivity 

	{ 

		/// <summary> 

		/// Включен ли контроль печати УИК 

		/// </summary> 

		/// <param name="context"></param> 

		/// <param name="parameters"></param> 

		/// <returns></returns> 

		public NextActivityKey IsStampControlEnabled( 

			WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

		{ 

			return _recognitionManager.StampControlEnabled 

				? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

		} 

 

 

		/// <summary> 

		/// Запрещает/Разрешает контроль печати 

		/// </summary> 

		/// <param name="context"></param> 

		/// <param name="parameters"></param> 

		/// <returns></returns> 

		public NextActivityKey SwitchStampControl( 

			WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

		{ 

			// поменяем контроль печати УИК 

			_recognitionManager.StampControlEnabled = !_recognitionManager.StampControlEnabled; 

 

 

			return BpcNextActivityKeys.Yes; 

		} 

	} 

}


