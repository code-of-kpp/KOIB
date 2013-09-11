using System; 

using System.Configuration; 

using Croc.Bpc.Recognizer.Ocr; 

 

 

namespace Croc.Bpc.Recognizer.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент печати 

    /// </summary> 

    public class StampConfig : ConfigurationElement 

    { 

        [ConfigurationProperty("testLevel", IsRequired = true)] 

        public StampTestLevel TestLevel 

        { 

            get 

            { 

                return (StampTestLevel)this["testLevel"]; 

            } 

            set 

            { 

                this["testLevel"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("verticalSize", IsRequired = true)] 

        public int VerticalSize 

        { 

            get 

            { 

                return (int)this["verticalSize"]; 

            } 

            set 

            { 

                this["verticalSize"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("frameWidth", IsRequired = true)] 

        public int FrameWidth 

        { 

            get 

            { 

                return (int)this["frameWidth"]; 

            } 

            set 

            { 

                this["frameWidth"] = value; 


            } 

        } 

 

 

        [ConfigurationProperty("lowThr", IsRequired = true)] 

        public int LowThr 

        { 

            get 

            { 

                return (int)this["lowThr"]; 

            } 

            set 

            { 

                this["lowThr"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("digital", IsRequired = true)] 

        public DigitalStampConfig Digital 

        { 

            get 

            { 

                return (DigitalStampConfig)this["digital"]; 

            } 

            set 

            { 

                this["digital"] = value; 

            } 

        } 

    } 

}


