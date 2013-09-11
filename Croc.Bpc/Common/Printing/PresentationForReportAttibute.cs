using System; 
namespace Croc.Bpc.Printing 
{ 
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class PresentationForReportAttribute : Attribute 
    { 
        public string DisplayName 
        { 
            get; set; 
        } 
        public PresentationForReportAttribute(string displayName) 
        { 
            DisplayName = displayName; 
        } 
    } 
}
