using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Bpc.Common 

{ 

	/// <summary> 

	/// Информация о сканере 

	/// </summary> 

	public class ScannerInfo 

	{ 

		/// <summary> 

		/// Серийник сканера 

		/// </summary> 

		public string Serial 

		{ 

			get; 

			private set; 

		} 

 

 

		/// <summary> 

		/// ИП сканера 

		/// </summary> 

		public string IP 

		{ 

			get; 

			private set; 

		} 

 

 

		/// <summary> 

		/// Конструктор 

		/// </summary> 

		/// <param name="serial">серийник</param> 

		/// <param name="ip">ип</param> 

		public ScannerInfo(string serial, string ip) 

		{ 

			Serial = serial; 

			IP = ip; 

		} 

 

 

		/// <summary> 

		/// Переопределленный ToString 

		/// </summary> 

		/// <returns>серийник сканера</returns> 

		public override string ToString() 


		{ 

			return Serial; 

		} 

 

 

		/// <summary> 

		/// Сравнение объектов с текущим 

		/// </summary> 

		/// <param name="obj">сравниваемый объект</param> 

		/// <returns>результат сравнения</returns> 

		public override bool Equals(object obj) 

		{ 

			// If parameter is null return false. 

			if (obj == null) 

			{ 

				return false; 

			} 

 

 

			// If parameter cannot be cast to ScannerInfo return false. 

			ScannerInfo scanner = obj as ScannerInfo; 

			if (scanner == null) 

			{ 

				return false; 

			} 

 

 

			// сравним серийные номера 

			return Serial.Equals(scanner.Serial); 

		} 

 

 

		/// <summary> 

		/// Получение хеша объекта 

		/// </summary> 

		/// <returns></returns> 

		public override int GetHashCode() 

		{ 

			return Serial.GetHashCode(); 

		} 

	} 

}


