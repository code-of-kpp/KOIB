using System; 

using System.Configuration; 

using Croc.Bpc.Common.Config; 

 

 

namespace Croc.Bpc.Sound.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент списка команд 

    /// </summary> 

    public class CommandsConfig : ConfigurationElement 

    { 

        /// <summary> 

        /// Команда для выполнения перед воспроизведением фразы 

        /// </summary> 

        [ConfigurationProperty("beforePlaying", IsRequired = false)] 

        public CommandConfig BeforePlaying 

        { 

            get 

            { 

                return (CommandConfig)this["beforePlaying"]; 

            } 

            set 

            { 

                this["beforePlaying"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Команда для выполнения после воспроизведения фразы 

        /// </summary> 

        [ConfigurationProperty("afterPlaying", IsRequired = false)] 

        public CommandConfig AfterPlaying 

        { 

            get 

            { 

                return (CommandConfig)this["afterPlaying"]; 

            } 

            set 

            { 

                this["afterPlaying"] = value; 

            } 

        } 

    } 

}


