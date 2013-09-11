using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Printing.Config 

{ 

	/// <summary> 

	/// конфиг - элемент принтера 

	/// </summary> 

	public class PrinterConfig : ConfigurationElement 

	{ 

		/// <summary> 

		/// Наименование принтера 

		/// </summary> 

		[ConfigurationProperty("name", IsRequired = true)] 

		public string Name 

		{ 

			get 

			{ 

				return (string)this["name"]; 

			} 

			set 

			{ 

				this["name"] = value; 

			} 

		} 

 

 

		/// <summary> 

		/// Регулярное выражение для поиска принтера в Unix 

		/// </summary> 

		[ConfigurationProperty("searchRegExp", IsRequired = true)] 

		public string SearchRegExp 

		{ 

			get 

			{ 

				return (string)this["searchRegExp"]; 

			} 

			set 

			{ 

				this["searchRegExp"] = value; 

			} 

		} 

	} 

}


