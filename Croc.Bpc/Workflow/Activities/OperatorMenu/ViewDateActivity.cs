using System; 

 

 

namespace Croc.Bpc.Workflow.Activities.OperatorMenu 

{ 

	/// <summary> 

	/// Показать текущую дату 

	/// </summary> 

	public class ViewDateActivity : BpcCompositeActivity 

	{ 

		/// <summary> 

		/// Текущая дата 

		/// </summary> 

		public DateTime CurrentDate 

		{ 

			get  

			{ 

				return DateTime.Now; 

			} 

		} 

	} 

}


