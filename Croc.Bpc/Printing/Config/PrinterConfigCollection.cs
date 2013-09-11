using System; 
using System.Configuration; 
namespace Croc.Bpc.Printing.Config 
{ 
    public class PrinterConfigCollection : ConfigurationElementCollection 
    { 
        protected override ConfigurationElement CreateNewElement() 
        { 
            return new PrinterConfig(); 
        } 
        protected override Object GetElementKey(ConfigurationElement element) 
        { 
            return ((PrinterConfig)element).Name; 
        } 
    } 
}
