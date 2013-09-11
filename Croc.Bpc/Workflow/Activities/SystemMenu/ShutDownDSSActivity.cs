using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Workflow.ComponentModel; 

 

 

namespace Croc.Bpc.Workflow.Activities.SystemMenu 

{ 

	public class ShutDownDSSActivity : BpcCompositeActivity 

	{ 

		/// <summary> 

		/// Включен ли датчик двойного листа 

		/// </summary> 

		/// <param name="context"></param> 

		/// <param name="parameters"></param> 

		/// <returns></returns> 

		public NextActivityKey IsDSSEnabled( 

			WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

		{ 

			return _scannerManager.DoubleSheetSensorEnabled 

				? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

		} 

 

 

		/// <summary> 

		/// Запрещает/Разрешает ДДЛ 

		/// </summary> 

		/// <param name="context"></param> 

		/// <param name="parameters"></param> 

		/// <returns></returns> 

		public NextActivityKey SwitchDSSEnabled( 

			WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

		{ 

			// поменяем разрешенность ДЛЛ 

			_scannerManager.DoubleSheetSensorEnabled = !_scannerManager.DoubleSheetSensorEnabled; 

 

 

			return BpcNextActivityKeys.Yes; 

		} 

	} 

}


