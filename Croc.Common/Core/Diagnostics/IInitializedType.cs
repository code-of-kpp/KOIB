using System.Configuration; 
namespace Croc.Core.Diagnostics 
{ 
    public interface IInitializedType 
    { 
        void Init(NameValueConfigurationCollection props); 
    } 
}
