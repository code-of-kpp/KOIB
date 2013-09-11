using System; 

using System.Configuration; 

 

 

namespace Croc.Bpc.FileSystem.Config 

{ 

	/// <summary> 

	/// Конфиг элемент директорий с данными 

	/// </summary> 

	public class DataDirectoryConfigCollection : ConfigurationElementCollection 

	{ 

		protected override ConfigurationElement CreateNewElement() 

		{ 

			return new DataDirectoryConfig(); 

		} 

 

 

		protected override Object GetElementKey(ConfigurationElement element) 

		{ 

			return ((DataDirectoryConfig)element).FileTypeStr; 

		} 

	} 

}


