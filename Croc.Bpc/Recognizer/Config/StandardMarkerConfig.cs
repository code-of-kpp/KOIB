using System; 

using System.Configuration; 

using Croc.Core.Configuration; 

 

 

namespace Croc.Bpc.Recognizer.Config 

{ 

    public class StandardMarkerConfig : ConfigurationElement 

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

 

 

        [ConfigurationProperty("zone", IsRequired = true)] 

        public ValueConfig<int> Zone 

        { 

            get 

            { 

                return (ValueConfig<int>)this["zone"]; 

            } 

            set 

            { 

                this["zone"] = value; 

            } 

        } 

    } 


}


