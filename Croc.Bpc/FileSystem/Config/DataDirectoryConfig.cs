using System; 

using System.Configuration; 

using Croc.Core.Configuration; 

 

 

namespace Croc.Bpc.FileSystem.Config 

{ 

	public class DataDirectoryConfig : ConfigurationElement 

	{ 

		/// <summary> 

		/// Тип файлов хранящихся в папке 

		/// </summary> 

		[ConfigurationProperty("fileType", IsRequired = true)] 

		public string FileTypeStr 

		{ 

			get 

			{ 

				return (string)this["fileType"]; 

			} 

			set 

			{ 

				this["fileType"] = value; 

			} 

		} 

 

 

		/// <summary> 

		/// Путь к папке 

		/// </summary> 

		[ConfigurationProperty("path", IsRequired = true)] 

		public string Path 

		{ 

			get 

			{ 

				return (string)this["path"]; 

			} 

			set 

			{ 

				this["path"] = value; 

			} 

		} 

 

 

		/// <summary> 

		/// Нужно ли архивировать папку 

		/// </summary> 

		[ConfigurationProperty("needToArchive", IsRequired = true)] 

		public bool NeedToArchive 

		{ 

			get 


			{ 

				return (bool)this["needToArchive"]; 

			} 

			set 

			{ 

				this["needToArchive"] = value; 

			} 

		} 

 

 

		/// <summary> 

		/// Тип файлов, хранящихся в этой директории  

		/// </summary> 

		public FileType FileType 

		{ 

			get 

			{ 

				if(Enum.IsDefined(typeof(FileType), FileTypeStr)) 

					return (FileType)Enum.Parse(typeof(FileType), FileTypeStr, true); 

 

 

				// если такого типа файлов нет вернем ошибку  

				throw new ArgumentException(String.Format("Тип файлов {0} не содержится в перечислении FileType", FileTypeStr)); 

			} 

		} 

	} 

}


