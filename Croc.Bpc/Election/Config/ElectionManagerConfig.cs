using System; 

using Croc.Core.Configuration; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Election.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент менеджера синхронизации 

    /// </summary> 

    public class ElectionManagerConfig : SubsystemConfig 

    { 

        /// <summary> 

        /// Директории с данными 

        /// </summary> 

        [ConfigurationProperty("dataDirectories", IsRequired = true)] 

        public DataDirectoriesConfig DataDirectories 

        { 

            get 

            { 

                return (DataDirectoriesConfig)this["dataDirectories"]; 

            } 

            set 

            { 

                this["dataDirectories"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Параметры поиска файла с ИД 

        /// </summary> 

        [ConfigurationProperty("sourceDataFileSearch", IsRequired = true)] 

        public SourceDataFileSearchConfig SourceDataFileSearch 

        { 

            get 

            { 

                return (SourceDataFileSearchConfig)this["sourceDataFileSearch"]; 

            } 

            set 

            { 

                this["sourceDataFileSearch"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Времена начала режимов голосования по умолчанию 

        /// </summary> 

        [ConfigurationProperty("defaultVotingModeTimes", IsDefaultCollection = false, IsRequired = true)] 


        [ConfigurationCollection(typeof(VotingModeTimeConfig), AddItemName = "votingModeTime")] 

        public VotingModeTimeConfigCollection DefaultVotingModeTimes 

        { 

            get 

            { 

                return (VotingModeTimeConfigCollection)base["defaultVotingModeTimes"]; 

            } 

        } 

 

 

		/// <summary> 

		/// Нужно ли проверять КС 

		/// </summary> 

		[ConfigurationProperty("needExecuteCheckExpressions", IsRequired = true)] 

		public ValueConfig<bool> NeedExecuteCheckExpressions 

		{ 

			get 

			{ 

				return (ValueConfig<bool>)this["needExecuteCheckExpressions"]; 

			} 

			set 

			{ 

				this["needExecuteCheckExpressions"] = value; 

			} 

		} 

 

 

        /// <summary> 

        /// Нужны ли ИД на флешке при сохранении на нее результатов голосования 

        /// </summary> 

        [ConfigurationProperty("needSourceDataForSaveResults", IsRequired = true)] 

        public ValueConfig<bool> NeedSourceDataForSaveResults 

        { 

            get 

            { 

                return (ValueConfig<bool>)this["needSourceDataForSaveResults"]; 

            } 

            set 

            { 

                this["needSourceDataForSaveResults"] = value; 

            } 

        } 

    } 

}


