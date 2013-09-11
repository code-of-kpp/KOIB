using System; 

using System.Configuration; 

using Croc.Core.Configuration; 

using Croc.Bpc.Recognizer.Ocr; 

 

 

namespace Croc.Bpc.Recognizer.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент online-распознавания 

    /// </summary> 

    /// <remarks>inline - это не очепятка, а так было в исходном коде</remarks> 

    public class InlineRecognizeConfig : EnabledConfig 

    { 

        [ConfigurationProperty("level", IsRequired = true)] 

        public InlineLevel Level 

        { 

            get 

            { 

                return (InlineLevel)this["level"]; 

            } 

            set 

            { 

                this["level"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("startAtLine", IsRequired = true)] 

        public int StartAtLine 

        { 

            get 

            { 

                return (int)this["startAtLine"]; 

            } 

            set 

            { 

                this["startAtLine"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("stopAtLine", IsRequired = true)] 

        public int StopAtLine 

        { 

            get 

            { 

                return (int)this["stopAtLine"]; 

            } 

            set 


            { 

                this["stopAtLine"] = value; 

            } 

        } 

    } 

}


