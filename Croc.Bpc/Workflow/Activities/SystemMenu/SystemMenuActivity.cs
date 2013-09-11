using System; 

using System.Collections.Generic; 

using System.Text; 

using Croc.Bpc.Election.Voting; 

using Croc.Bpc.Scanner; 

using Croc.Workflow.ComponentModel; 

using Croc.Core.Extensions; 

using Croc.Bpc.Common.Diagnostics; 

 

 

namespace Croc.Bpc.Workflow.Activities.SystemMenu 

{ 

    /// <summary> 

    /// Системное меню 

    /// </summary> 

    [Serializable] 

    public class SystemMenuActivity : BpcCompositeActivity 

    { 

		/// <summary> 

		/// Проверка введенного пароля 

		/// </summary> 

		/// <param name="context"></param> 

		/// <param name="parameters"></param> 

		/// <returns></returns> 

		public NextActivityKey AcceptPassword( 

			WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

		{ 

			// совпадает ли пароль с последним введенным значением 

			return GetTodayPassword() == int.Parse(CommonActivity.LastReadedValue) 

				? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

		} 

 

 

		/// <summary> 

		/// Получить пароль на сегодняшний день 

		/// </summary> 

		/// <returns></returns> 

		private int GetTodayPassword() 

		{ 

			// версия приложения 

			var version = Core.CoreApplication.Instance.ApplicationVersion; 

 

 

			// вычислим пароль(версия % 10000 + день * 3 + месяц * 2) 

			return ((version.Revision % 10000) + DateTime.Today.Day * 3 + DateTime.Today.Month * 2) % 10000; 

		} 

    } 

}


