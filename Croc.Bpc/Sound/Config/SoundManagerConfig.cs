using System; 

using System.Configuration; 

using Croc.Core.Configuration; 

 

 

namespace Croc.Bpc.Sound.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент менеджера потока работ 

    /// </summary> 

    public class SoundManagerConfig : SubsystemConfig 

    { 

        /// <summary> 

        /// Конфиг плеера 

        /// </summary> 

        [ConfigurationProperty("player", IsRequired = true)] 

        public PlayerConfig Player 

        { 

            get 

            { 

                return (PlayerConfig)this["player"]; 

            } 

            set 

            { 

                this["player"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Команды 

        /// </summary> 

        [ConfigurationProperty("commands", IsRequired = false)] 

        public CommandsConfig Commands 

        { 

            get 

            { 

                return (CommandsConfig)this["commands"]; 

            } 

            set 

            { 

                this["commands"] = value; 

            } 

        } 

    } 

}


