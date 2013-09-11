using System; 
using Croc.Core.Configuration; 
using System.Configuration; 
namespace Croc.Bpc.Scanner.Config 
{ 
    public class ErrorConfig : EnabledConfig 
    { 
        [ConfigurationProperty("code", IsRequired = true)] 
        public int Code 
        { 
            get 
            { 
                return (int)this["code"]; 
            } 
            set 
            { 
                this["code"] = value; 
            } 
        } 
        [ConfigurationProperty("isReverse", IsRequired = false, DefaultValue = true)] 
        public bool IsReverse 
        { 
            get 
            { 
                return (bool)this["isReverse"]; 
            } 
            set 
            { 
                this["isReverse"] = value; 
            } 
        } 
        [ConfigurationProperty("description", IsRequired = false)] 
        public string Description 
        { 
            get 
            { 
                return (string)this["description"]; 
            } 
            set 
            { 
                this["description"] = value; 
            } 
        } 
    } 
}
