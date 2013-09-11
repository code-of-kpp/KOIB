using System; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Sound.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент плеера 

    /// </summary> 

    public class PlayerConfig : ConfigurationElement 

    { 

        /// <summary> 

        /// Формат плеера 

        /// </summary> 

        [ConfigurationProperty("format", IsRequired = true)] 

        public string FormatString 

        { 

            get 

            { 

                return (string)this["format"]; 

            } 

            set 

            { 

                this["format"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Формат плеера  

        /// </summary> 

        public SoundPlayerType Format 

        { 

            get 

            { 

                return (SoundPlayerType)Enum.Parse(typeof(SoundPlayerType), FormatString, true); 

            } 

        } 

 

 

        /// <summary> 

        /// Размер буфера устройства в микросекундах 

        /// </summary> 

        [ConfigurationProperty("deviceLatency", IsRequired = true)] 

        public int DeviceLatency 

        { 

            get 

            { 

                return (int)this["deviceLatency"]; 

            } 


            set 

            { 

                this["deviceLatency"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Задержка после воспроизведения фразы в мсек (точнее, после того, как все данные переданы в устройство) 

        /// </summary> 

        [ConfigurationProperty("afterPlayDelay", IsRequired = true)] 

        public int AfterPlayDelay 

        { 

            get 

            { 

                return (int)this["afterPlayDelay"]; 

            } 

            set 

            { 

                this["afterPlayDelay"] = value; 

            } 

        } 

    } 

}


