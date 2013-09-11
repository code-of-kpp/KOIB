using System; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Recognizer.Config 

{ 

    public class DebugImageSavingConfig : ConfigurationElement 

    { 

        [ConfigurationProperty("presave", IsRequired = true)] 

        public bool Presave 

        { 

            get 

            { 

                return (bool)this["presave"]; 

            } 

            set 

            { 

                this["presave"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("reverse", IsRequired = true)] 

        public bool Reverse 

        { 

            get 

            { 

                return (bool)this["reverse"]; 

            } 

            set 

            { 

                this["reverse"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("nextBuffer", IsRequired = true)] 

        public bool NextBuffer 

        { 

            get 

            { 

                return (bool)this["nextBuffer"]; 

            } 

            set 

            { 

                this["nextBuffer"] = value; 

            } 

        } 

 

 


		[ConfigurationProperty("driverReverse", IsRequired = true)] 

		public bool DriverReverse 

		{ 

			get 

			{ 

                return (bool)this["driverReverse"]; 

			} 

			set 

			{ 

                this["driverReverse"] = value; 

			} 

		} 

 

 

		[ConfigurationProperty("squares", IsRequired = true)] 

		public bool Squares 

		{ 

			get 

			{ 

				return (bool)this["squares"]; 

			} 

			set 

			{ 

				this["squares"] = value; 

			} 

		} 

	} 

}


