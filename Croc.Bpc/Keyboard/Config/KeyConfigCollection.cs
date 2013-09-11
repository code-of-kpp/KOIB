using System; 
using System.Configuration; 
namespace Croc.Bpc.Keyboard.Config 
{ 
    public class KeyConfigCollection : ConfigurationElementCollection 
    { 
        protected override ConfigurationElement CreateNewElement() 
        { 
            return new KeyConfig(); 
        } 
        protected override Object GetElementKey(ConfigurationElement element) 
        { 
            return ((KeyConfig)element).ScanCode; 
        } 
    } 
}
