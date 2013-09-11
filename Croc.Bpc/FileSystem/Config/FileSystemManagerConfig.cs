using System.Configuration; 

using Croc.Core.Configuration; 

 

 

namespace Croc.Bpc.FileSystem.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент менеджера файловой системы 

    /// </summary> 

    public class FileSystemManagerConfig : SubsystemConfig 

    { 

		/// <summary> 

		/// Директории с данными по типам файлов 

		/// </summary> 

		[ConfigurationProperty("dataDirectories", IsDefaultCollection = false, IsRequired = true)] 

		[ConfigurationCollection(typeof(DataDirectoryConfigCollection), AddItemName = "dataDirectory")] 

		public DataDirectoryConfigCollection DataDirectories 

		{ 

			get 

			{ 

				return (DataDirectoryConfigCollection)base["dataDirectories"]; 

			} 

		} 

 

 

		/// <summary> 

		/// Путь к папке с архивами предыдущих выборов 

		/// </summary> 

		[ConfigurationProperty("archivesFolderPath", IsRequired = true)] 

		public ValueConfig<string> ArchivesFolderPath 

		{ 

			get 

			{ 

				return (ValueConfig<string>)base["archivesFolderPath"]; 

			} 

		} 

 

 

		/// <summary> 

		/// Минимальное место, которое необходимо освободить при чистке диска 

		/// </summary> 

        [ConfigurationProperty("minDiskSpaceToFreeMb", IsRequired = true)] 

        public ValueConfig<int> MinDiskSpaceToFreeMb 

		{ 

			get 

			{ 

				return (ValueConfig<int>)base["minDiskSpaceToFreeMb"]; 

			} 

		} 

 


 
		/// <summary> 

		/// Порядок удаления файлов при освобождении места 

		/// </summary> 

		[ConfigurationProperty("cleanOrder", IsRequired = true)] 

		public ValueConfig<string> CleanOrder 

		{ 

			get 

			{ 

				return (ValueConfig<string>)base["cleanOrder"]; 

			} 

		} 

    } 

}


