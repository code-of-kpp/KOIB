using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Bpc.Workflow.Activities.OperatorMenu 

{ 

	/// <summary> 

	/// Показать номер версии 

	/// </summary> 

	public class ViewVersionActivity : BpcCompositeActivity 

	{ 

		/// <summary> 

		/// Версия приложения 

		/// </summary> 

		public string Version 

		{ 

			get  

			{ 

				return Core.CoreApplication.Instance.ApplicationVersion.ToString(); 

			} 

		} 

	} 

}


