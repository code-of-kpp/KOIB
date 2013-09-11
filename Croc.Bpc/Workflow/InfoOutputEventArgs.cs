using System; 
namespace Croc.Bpc.Workflow 
{ 
    public class InfoOutputEventArgs : EventArgs 
    { 
        public InfoType InfoType { get; private set; } 
        public InfoOutputEventArgs(InfoType infoType) 
        { 
            InfoType = infoType; 
        } 
    } 
    public enum InfoType 
    { 
        Information, 
        Question, 
        Warning, 
    } 
}
