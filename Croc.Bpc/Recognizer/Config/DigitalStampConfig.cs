using System; 
using System.Configuration; 
namespace Croc.Bpc.Recognizer.Config 
{ 
    public class DigitalStampConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("minLineWidth", IsRequired = true)] 
        public int MinLineWidth 
        { 
            get 
            { 
                return (int)this["minLineWidth"]; 
            } 
            set 
            { 
                this["minLineWidth"] = value; 
            } 
        } 
        [ConfigurationProperty("maxLineWidth", IsRequired = true)] 
        public int MaxLineWidth 
        { 
            get 
            { 
                return (int)this["maxLineWidth"]; 
            } 
            set 
            { 
                this["maxLineWidth"] = value; 
            } 
        } 
        [ConfigurationProperty("xSize", IsRequired = true)] 
        public int XSize 
        { 
            get 
            { 
                return (int)this["xSize"]; 
            } 
            set 
            { 
                this["xSize"] = value; 
            } 
        } 
        [ConfigurationProperty("ySize", IsRequired = true)] 
        public int YSize 
        { 
            get 
            { 
                return (int)this["ySize"]; 
            } 
            set 
            { 
                this["ySize"] = value; 
            } 
        } 
        [ConfigurationProperty("gap", IsRequired = true)] 
        public int Gap 
        { 
            get 
            { 
                return (int)this["gap"]; 
            } 
            set 
            { 
                this["gap"] = value; 
            } 
        } 
        [ConfigurationProperty("distBottom", IsRequired = true)] 
        public int DistBottom 
        { 
            get 
            { 
                return (int)this["distBottom"]; 
            } 
            set 
            { 
                this["distBottom"] = value; 
            } 
        } 
        [ConfigurationProperty("distLeft", IsRequired = true)] 
        public int DistLeft 
        { 
            get 
            { 
                return (int)this["distLeft"]; 
            } 
            set 
            { 
                this["distLeft"] = value; 
            } 
        } 
        [ConfigurationProperty("distRight", IsRequired = true)] 
        public int DistRight 
        { 
            get 
            { 
                return (int)this["distRight"]; 
            } 
            set 
            { 
                this["distRight"] = value; 
            } 
        } 
    } 
}
