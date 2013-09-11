using System; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Scanner.Config 

{ 

    public class WorkZoneConfig : ConfigurationElement 

    { 

        [ConfigurationProperty("sideTopX", IsRequired = true)] 

        public short SideTopX 

        { 

            get 

            { 

                return (short)this["sideTopX"]; 

            } 

            set 

            { 

                this["sideTopX"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("sideTopY", IsRequired = true)] 

        public short SideTopY 

        { 

            get 

            { 

                return (short)this["sideTopY"]; 

            } 

            set 

            { 

                this["sideTopY"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("sideBottomX", IsRequired = true)] 

        public short SideBottomX 

        { 

            get 

            { 

                return (short)this["sideBottomX"]; 

            } 

            set 

            { 

                this["sideBottomX"] = value; 

            } 

        } 

 

 


        [ConfigurationProperty("sideBottomY", IsRequired = true)] 

        public short SideBottomY 

        { 

            get 

            { 

                return (short)this["sideBottomY"]; 

            } 

            set 

            { 

                this["sideBottomY"] = value; 

            } 

        } 

    } 

}


