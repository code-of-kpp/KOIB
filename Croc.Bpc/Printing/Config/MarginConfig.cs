using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using System.Configuration; 
namespace Croc.Bpc.Printing.Config 
{ 
    public class MarginConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("left", IsRequired = true)] 
        public int Left 
        { 
            get 
            { 
                return (int)this["left"]; 
            } 
            set 
            { 
                this["left"] = value; 
            } 
        } 
        [ConfigurationProperty("right", IsRequired = true)] 
        public int Right 
        { 
            get 
            { 
                return (int)this["right"]; 
            } 
            set 
            { 
                this["right"] = value; 
            } 
        } 
        [ConfigurationProperty("top", IsRequired = true)] 
        public int Top 
        { 
            get 
            { 
                return (int)this["top"]; 
            } 
            set 
            { 
                this["top"] = value; 
            } 
        } 
        [ConfigurationProperty("bottom", IsRequired = true)] 
        public int Bottom 
        { 
            get 
            { 
                return (int)this["bottom"]; 
            } 
            set 
            { 
                this["bottom"] = value; 
            } 
        } 
    } 
}
