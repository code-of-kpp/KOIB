using System; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Scanner.Config 

{ 

    public class PaperDensityConfig : ConfigurationElement 

    { 

        [ConfigurationProperty("thin", IsRequired = true)] 

        public int Thin 

        { 

            get 

            { 

                return (int)this["thin"]; 

            } 

            set 

            { 

                this["thin"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("normal", IsRequired = true)] 

        public int Normal 

        { 

            get 

            { 

                return (int)this["normal"]; 

            } 

            set 

            { 

                this["normal"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("thick", IsRequired = true)] 

        public int Thick 

        { 

            get 

            { 

                return (int)this["thick"]; 

            } 

            set 

            { 

                this["thick"] = value; 

            } 

        } 

    } 

}


