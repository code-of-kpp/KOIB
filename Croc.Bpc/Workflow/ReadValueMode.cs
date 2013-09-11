using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Bpc.Workflow 

{ 

	/// <summary> 

	/// Режим считывания цифр с клавиатуры 

	/// </summary> 

	public enum ReadValueMode 

	{ 

		/// <summary> 

		/// Обрезать лидирующий 0 

		/// </summary> 

		CutLeadingZero, 

 

 

		/// <summary> 

		/// Оставлять лидирующие 0 

		/// </summary> 

		WithLeadingZero 

	} 

}


