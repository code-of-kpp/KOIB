using System; 

using System.Configuration; 

using Croc.Core.Configuration; 

 

 

namespace Croc.Bpc.Election.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент директорий с данными 

    /// </summary> 

    public class DataDirectoriesConfig : ConfigurationElement 

    { 

        /// <summary> 

        /// Имя директории, в которой будет выполняться поиск файла с ИД 

        /// </summary> 

        [ConfigurationProperty("sourceDataDirName", IsRequired = true)] 

        public string SourceDataDirName 

        { 

            get 

            { 

                return (string)this["sourceDataDirName"]; 

            } 

            set 

            { 

                this["sourceDataDirName"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Имя директории для сохранения результатов голосования 

        /// </summary> 

        [ConfigurationProperty("votingResultDirName", IsRequired = true)] 

        public string VotingResultDirName 

        { 

            get 

            { 

                return (string)this["votingResultDirName"]; 

            } 

            set 

            { 

                this["votingResultDirName"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Возможные пути корневой директории 

        /// </summary> 

        [ConfigurationProperty("root", IsDefaultCollection = false, IsRequired = true)] 


        [ConfigurationCollection(typeof(ValueConfig<string>), AddItemName = "path")] 

		public PathConfigCollection RootPaths 

        { 

            get 

            { 

				return (PathConfigCollection)base["root"]; 

            } 

        } 

    } 

}


