using System; 

using System.Configuration; 

using Croc.Core.Configuration; 

 

 

namespace Croc.Bpc.Recognizer.Config 

{ 

    public class DigitalMarkerConfig : ConfigurationElement 

    { 

        [ConfigurationProperty("width", IsRequired = true)] 

        public MinMaxConfig<int> Width 

        { 

            get 

            { 

                return (MinMaxConfig<int>)this["width"]; 

            } 

            set 

            { 

                this["width"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("height", IsRequired = true)] 

        public MinMaxConfig<int> Height 

        { 

            get 

            { 

                return (MinMaxConfig<int>)this["height"]; 

            } 

            set 

            { 

                this["height"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("rio", IsRequired = true)] 

        public MinMaxConfig<double> Rio 

        { 

            get 

            { 

                return (MinMaxConfig<double>)this["rio"]; 

            } 

            set 

            { 

                this["rio"] = value; 

            } 

        } 

    } 


}


