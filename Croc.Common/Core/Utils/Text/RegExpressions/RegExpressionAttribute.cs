using System; 
using System.Text.RegularExpressions; 
namespace Croc.Core.Utils.Text.RegExpressions 
{ 
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class RegExpressionAttribute : Attribute 
    { 
        public string Pattern { get; set; } 
        public RegexOptions Options { get; set; } 
        public RegExpressionAttribute(string pattern) 
        { 
            if (string.IsNullOrEmpty(pattern)) 
                throw new ArgumentNullException("pattern"); 
            Pattern = pattern; 
        } 
    } 
}
