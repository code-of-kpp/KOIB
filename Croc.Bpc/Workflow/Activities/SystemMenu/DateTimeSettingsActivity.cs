using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Workflow.ComponentModel; 

 

 

namespace Croc.Bpc.Workflow.Activities.SystemMenu 

{ 

	public class DateTimeSettingsActivity : BpcCompositeActivity 

	{ 

		/// <summary> 

		/// введенная дата 

		/// </summary> 

		private DateTime _date; 

 

 

		/// <summary> 

		/// введенное время 

		/// </summary> 

		private DateTime _time; 

 

 

		/// <summary> 

		/// Проверка введенной даты 

		/// </summary> 

		/// <param name="context"></param> 

		/// <param name="parameters"></param> 

		/// <returns></returns> 

		public NextActivityKey CheckNewDate( 

			WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

		{ 

			// попробуем конвертировать дату из введенных значений 

			if (DateTime.TryParseExact(CommonActivity.LastReadedValue 

					, "ddMMyyyy" 

					, null 

					, System.Globalization.DateTimeStyles.None 

					, out _date)) 

				return BpcNextActivityKeys.Yes; 

 

 

			return BpcNextActivityKeys.No; 

		} 

 

 

		/// <summary> 

		/// Проверка введенного времени 

		/// </summary> 

		/// <param name="context"></param> 

		/// <param name="parameters"></param> 


		/// <returns></returns> 

		public NextActivityKey CheckNewTime( 

			WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

		{ 

			// попробуем конвертировать дату из введенных значений 

			if (DateTime.TryParseExact(CommonActivity.LastReadedValue 

					, "HHmm" 

					, null 

					, System.Globalization.DateTimeStyles.None 

					, out _time)) 

				return BpcNextActivityKeys.Yes; 

 

 

			return BpcNextActivityKeys.No; 

		} 

 

 

		/// <summary> 

		/// Установка времени на сканер 

		/// </summary> 

		/// <param name="context"></param> 

		/// <param name="parameters"></param> 

		/// <returns></returns> 

		public NextActivityKey SetDateTime( 

			WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

		{ 

			// сформируем дату из дня и времени 

			var utcDateTime = _date.AddHours(_time.Hour).AddMinutes(_time.Minute).ToUniversalTime(); 

 

 

			// установим время 

            _syncManager.SetSystemTime(utcDateTime); 

 

 

			return BpcNextActivityKeys.Yes; 

		} 

	} 

}


