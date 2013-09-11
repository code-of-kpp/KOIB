using Croc.Core.Configuration; 
using System.Configuration; 
namespace Croc.Bpc.Recognizer.Config 
{ 
    public class RecognitionManagerConfig : SubsystemConfig 
    { 
        [ConfigurationProperty("GCCollect", IsRequired = true)] 
        public EnabledConfig GCCollect 
        { 
            get 
            { 
                return (EnabledConfig)this["GCCollect"]; 
            } 
            set 
            { 
                this["GCCollect"] = value; 
            } 
        } 
        [ConfigurationProperty("MinFreeSpaceForImageKb", IsRequired = true)] 
        public ValueConfig<int> MinFreeSpaceForImageKb 
        { 
            get 
            { 
                return (ValueConfig<int>)this["MinFreeSpaceForImageKb"]; 
            } 
            set 
            { 
                this["MinFreeSpaceForImageKb"] = value; 
            } 
        } 
        [ConfigurationProperty("superiorStamp", IsRequired = true)] 
        public EnabledConfig SuperiorStamp 
        { 
            get 
            { 
                return (EnabledConfig)this["superiorStamp"]; 
            } 
            set 
            { 
                this["superiorStamp"] = value; 
            } 
        } 
        [ConfigurationProperty("debugImageSaving", IsRequired = true)] 
        public DebugImageSavingConfig DebugImageSaving 
        { 
            get 
            { 
                return (DebugImageSavingConfig)this["debugImageSaving"]; 
            } 
            set 
            { 
                this["debugImageSaving"] = value; 
            } 
        } 
        [ConfigurationProperty("blankProcessing", IsDefaultCollection = false, IsRequired = true)] 
        [ConfigurationCollection(typeof(BlankConfig), AddItemName = "blank")] 
        public BlankConfigCollection Blanks 
        { 
            get 
            { 
                return (BlankConfigCollection)base["blankProcessing"]; 
            } 
        } 
        [ConfigurationProperty("ocr", IsRequired = true)] 
        public OcrConfig Ocr 
        { 
            get 
            { 
                return (OcrConfig)this["ocr"]; 
            } 
            set 
            { 
                this["ocr"] = value; 
            } 
        } 
    } 
}
