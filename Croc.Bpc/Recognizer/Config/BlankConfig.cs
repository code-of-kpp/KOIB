using System.Configuration; 
using Croc.Bpc.Scanner; 
using Croc.Bpc.Voting; 
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
        [ConfigurationProperty("sheetType", IsRequired = false, DefaultValue = SheetType.Undefined)] 
        public SheetType SheetType 
        { 
            get 
            { 
                return (SheetType)this["sheetType"]; 
            } 
            set 
            { 
                this["sheetType"] = value; 
            } 
        } 
        [ConfigurationProperty("marking", IsRequired = true)] 
        public BlankMarking Marking 
        { 
            get 
            { 
                return (BlankMarking)this["marking"]; 
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
