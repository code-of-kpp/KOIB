using System; 
namespace Croc.Bpc.Diagnostics 
{ 
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class MessageParametersAttribute : Attribute  
    { 
        public MessageParametersAttribute(string body)  
        { 
            Body = body; 
        } 
        public string Body { get; set; } 
    } 
}
