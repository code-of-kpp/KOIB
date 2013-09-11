using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Core 

{ 

	/// <summary> 

	/// Аргументы события изменения конфигурации 

	/// </summary> 

	public class ConfigUpdatedEventArgs : EventArgs 

	{ 

		/// <summary> 

		/// Имя подсистемы, с измененным параметром 

		/// </summary> 

		public string SubsystemName 

		{ 

			get; 

			private set; 

		} 

 

 

		/// <summary> 

		/// Имя измененного параметра 

		/// </summary> 

		public string UpdatedParameterName 

		{ 

			get; 

			private set; 

		} 

 

 

		/// <summary> 

		/// Старое значение параметра 

		/// </summary> 

		public object OldValue 

		{ 

			get; 

			private set; 

		} 

 

 

		/// <summary> 

		/// Новое значение параметра 

		/// </summary> 

		public object NewValue 

		{ 

			get; 

			private set; 


		} 

 

 

		/// <summary> 

		/// Конструктор 

		/// </summary> 

		/// <param name="subsystemName">имя подсистемы с измененным параметром</param> 

		/// <param name="updatedParameterName">имя измененного параметра</param> 

		/// <param name="oldValue">старое значение</param> 

		/// <param name="newValue">новое значение</param> 

		public ConfigUpdatedEventArgs(string subsystemName, string updatedParameterName, object oldValue, object newValue) 

		{ 

			SubsystemName = subsystemName; 

			UpdatedParameterName = updatedParameterName; 

			OldValue = oldValue; 

			NewValue = newValue; 

		} 

	} 

}


