using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Configuration; 

using Croc.Core.Configuration; 

 

 

namespace Croc.Bpc.Configuration.Config 

{ 

	/// <summary> 

	/// Конфиг элемент коллекция путей к файлу частичной конфигурации 

	/// </summary> 

	public class PartialConfigPathsCollection : ConfigurationElementCollection 

	{ 

		protected override ConfigurationElement CreateNewElement() 

		{ 

			return new ValueConfig<string>(); 

		} 

 

 

		protected override Object GetElementKey(ConfigurationElement element) 

		{ 

			return ((ValueConfig<string>)element); 

		} 

 

 

		/// <summary> 

		/// Искать ли файл частичного конфига рядом с ИД 

		/// </summary> 

		[ConfigurationProperty("includeSourceDataPaths", IsRequired = true)] 

		public bool IncludeSourceDataPaths 

		{ 

			get 

			{ 

				return (bool)this["includeSourceDataPaths"]; 

			} 

			set 

			{ 

				this["includeSourceDataPaths"] = value; 

			} 

		} 

 

 

		/// <summary> 

		/// Имя файла, который искать 

		/// </summary> 

		[ConfigurationProperty("fileName", IsRequired = true)] 

		public string FileName 

		{ 


			get 

			{ 

				return (string)this["fileName"]; 

			} 

			set 

			{ 

				this["fileName"] = value; 

			} 

		} 

 

 

		/// <summary> 

		/// Привести к списку 

		/// </summary> 

		/// <returns></returns> 

		public List<string> ToList() 

		{ 

			var res = new List<string>(Count); 

 

 

			foreach (ValueConfig<string> item in this) 

				res.Add(item.Value); 

 

 

			return res; 

		} 

	} 

}


