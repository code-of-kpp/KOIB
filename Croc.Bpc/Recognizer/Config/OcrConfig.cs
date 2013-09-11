using System; 

using System.Configuration; 

using Croc.Core.Configuration; 

 

 

namespace Croc.Bpc.Recognizer.Config 

{ 

    public class OcrConfig : ConfigurationElement 

    { 

		[ConfigurationProperty("nextBufferMaxCalls", IsRequired=true)] 

		public ValueConfig<int> NextBufferMaxCalls 

		{ 

			get 

			{ 

				return (ValueConfig<int>)this["nextBufferMaxCalls"]; 

			} 

			set 

			{ 

				this["nextBufferMaxCalls"] = value; 

			} 

		} 

 

 

        [ConfigurationProperty("marker", IsRequired = true)] 

        public MarkerConfig Marker 

        { 

            get 

            { 

                return (MarkerConfig)this["marker"]; 

            } 

            set 

            { 

                this["marker"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("inlineRecognize", IsRequired = true)] 

        public InlineRecognizeConfig InlineRecognize 

        { 

            get 

            { 

                return (InlineRecognizeConfig)this["inlineRecognize"]; 

            } 

            set 

            { 

                this["inlineRecognize"] = value; 

            } 

        } 

 


 
        [ConfigurationProperty("minCheckArea", IsRequired = true)] 

        public ValueConfig<int> MinCheckArea 

        { 

            get 

            { 

                return (ValueConfig<int>)this["minCheckArea"]; 

            } 

            set 

            { 

                this["minCheckArea"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("maxOnlineSkew", IsRequired = true)] 

        public ValueConfig<int> MaxOnlineSkew 

        { 

            get 

            { 

                return (ValueConfig<int>)this["maxOnlineSkew"]; 

            } 

            set 

            { 

                this["maxOnlineSkew"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("cutWeakCheck", IsRequired = true)] 

        public EnabledConfig CutWeakCheck 

        { 

            get 

            { 

                return (EnabledConfig)this["cutWeakCheck"]; 

            } 

            set 

            { 

                this["cutWeakCheck"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("lookForLostSquare", IsRequired = true)] 

        public EnabledConfig LookForLostSquare 

        { 

            get 

            { 

                return (EnabledConfig)this["lookForLostSquare"]; 

            } 


            set 

            { 

                this["lookForLostSquare"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("seekBottomRightLine", IsRequired = true)] 

        public EnabledConfig SeekBottomRightLine 

        { 

            get 

            { 

                return (EnabledConfig)this["seekBottomRightLine"]; 

            } 

            set 

            { 

                this["seekBottomRightLine"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("grayAnalysis", IsRequired = true)] 

        public EnabledConfig GrayAnalysis 

        { 

            get 

            { 

                return (EnabledConfig)this["grayAnalysis"]; 

            } 

            set 

            { 

                this["grayAnalysis"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("stamp", IsRequired = true)] 

        public StampConfig Stamp 

        { 

            get 

            { 

                return (StampConfig)this["stamp"]; 

            } 

            set 

            { 

                this["stamp"] = value; 

            } 

        } 

 

 

        /// <summary> 


        /// Включен или выключен 

        /// </summary> 

        [ConfigurationProperty("logging", IsRequired = false)] 

        public EnabledConfig LoggingEnabled 

        { 

            get 

            { 

                return (EnabledConfig)this["logging"]; 

            } 

            set 

            { 

                this["logging"] = value; 

            } 

        } 

    } 

}


