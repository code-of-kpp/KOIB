using System.Configuration; 
namespace Croc.Bpc.Printing.Config 
{ 
    public class FontConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("path", IsRequired = true)] 
        public string Path 
        { 
            get 
            { 
                return (string)this["path"]; 
            } 
            set 
            { 
                this["path"] = value; 
            } 
        } 
    } 
}
