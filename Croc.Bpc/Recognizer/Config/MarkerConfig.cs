using System; 

using System.Configuration; 

using Croc.Bpc.Recognizer.Ocr; 

 

 

namespace Croc.Bpc.Recognizer.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент маркера 

    /// </summary> 

    public class MarkerConfig : ConfigurationElement 

    { 

        [ConfigurationProperty("type", IsRequired = true)] 

        public MarkerType Type 

        { 

            get 

            { 

                return (MarkerType)this["type"]; 

            } 

            set 

            { 

                this["type"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("standard", IsRequired = true)] 

        public StandardMarkerConfig Standard 

        { 

            get 

            { 

                return (StandardMarkerConfig)this["standard"]; 

            } 

            set 

            { 

                this["standard"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("digital", IsRequired = true)] 

        public DigitalMarkerConfig Digital 

        { 

            get 

            { 

                return (DigitalMarkerConfig)this["digital"]; 

            } 

            set 

            { 

                this["digital"] = value; 


            } 

        } 

    } 

}


