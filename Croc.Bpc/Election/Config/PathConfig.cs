using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Election.Config 

{ 

	/// <summary> 

	/// Конфиг-элемент пути поиска ИД 

	/// </summary> 

	public class PathConfig : ConfigurationElement 

	{ 

		/// <summary> 

		/// Директория 

		/// </summary> 

		[ConfigurationProperty("rootPath", IsRequired = true)] 

		public string RootPath 

		{ 

			get 

			{ 

				return (string)this["rootPath"]; 

			} 

			set 

			{ 

				this["rootPath"] = value; 

			} 

		} 

 

 

		/// <summary> 

		/// Шаблон для поиска нужных директорий 

		/// </summary> 

		[ConfigurationProperty("wildcard", IsRequired = false)] 

		public string Wildcard 

		{ 

			get 

			{ 

				return (string)this["wildcard"]; 

			} 

			set 

			{ 

				this["wildcard"] = value; 

			} 

		} 

	} 

}


