using Croc.Core; 
namespace Croc.Bpc.Configuration 
{ 
    public interface IConfigurationManager : ISubsystem 
    { 
        bool WorkingConfigLoaded { get; } 
        bool LoadWorkingConfig(); 
        bool LoadPartialConfig(ref string partialConfigXml); 
        void ResetWorkingConfig(); 
        bool ApplyConfig(); 
    } 
}
