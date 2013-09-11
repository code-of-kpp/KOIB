using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Bpc.Printing 

{ 

	/// <summary> 

	/// Задача для принтера 

	/// </summary> 

	public class PrinterJob 

	{ 

		/// <summary> 

		/// Конструктор 

		/// </summary> 

		/// <param name="filePath"></param> 

		/// <param name="pageCount"></param> 

		public PrinterJob(string filePath, int pageCount) 

		{ 

			FilePath = filePath; 

			PageCont = pageCount; 

		} 

 

 

		/// <summary> 

		/// Путь к файлу, который необходимо распечатать 

		/// </summary> 

		public string FilePath 

		{ 

			get; 

			private set; 

		} 

 

 

		/// <summary> 

		/// Количество страниц в файле 

		/// </summary> 

		public int PageCont 

		{ 

			get; 

			private set; 

		} 

	} 

}


