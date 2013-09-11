using System.Configuration; 
namespace Croc.Bpc.Scanner.Config 
{ 
    public class BlankPaperTypeConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("blankMarker", IsRequired = true)] 
        public int BlankMarker 
        { 
            get 
            { 
                return (int)this["blankMarker"]; 
            } 
            set 
            { 
                this["blankMarker"] = value; 
            } 
        } 
        [ConfigurationProperty("paperType", IsRequired = true)] 
        public PaperType PaperType 
        { 
            get 
            { 
                return (PaperType)this["paperType"]; 
            } 
            set 
            { 
                this["paperType"] = value; 
            } 
        } 
    } 
}
