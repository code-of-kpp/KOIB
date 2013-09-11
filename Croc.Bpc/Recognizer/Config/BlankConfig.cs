using System; 

using System.Configuration; 

using Croc.Bpc.Election.Voting; 

 

 

namespace Croc.Bpc.Recognizer.Config 

{ 

    public class BlankConfig : ConfigurationElement 

    { 

        [ConfigurationProperty("type", IsRequired = true)] 

        public BlankType Type 

        { 

            get 

            { 

                return (BlankType)this["type"]; 

            } 

            set 

            { 

                this["type"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Код метода маркировки 

        /// </summary> 

        /// <remarks>это кол-во проколов в бюллетене, которые нужно сделать</remarks> 

        [ConfigurationProperty("marking", IsRequired = true)] 

        public short MarkingCode 

        { 

            get 

            { 

                return (short)this["marking"]; 

            } 

            set 

            { 

                this["marking"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("imageSavingType", IsRequired = true)] 

        public ImageSavingType ImageSavingType 

        { 

            get 

            { 

                return (ImageSavingType)this["imageSavingType"]; 

            } 

            set 

            { 


                this["imageSavingType"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("imageFilePrefix", IsRequired = true)] 

        public string ImageFilePrefix 

        { 

            get 

            { 

                return (string)this["imageFilePrefix"]; 

            } 

            set 

            { 

                this["imageFilePrefix"] = value; 

            } 

        } 

    } 

}


