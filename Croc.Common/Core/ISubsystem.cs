using System; 
using System.Diagnostics; 
using Croc.Core.Diagnostics; 
using Croc.Core.Configuration; 
namespace Croc.Core 
{ 
    public interface ISubsystem : ILoggerContainer, IDisposable 
    { 
        ICoreApplication Application { get; set; } 
        void Init(SubsystemConfig config); 
        void ApplyNewConfig(SubsystemConfig newConfig); 
        event EventHandler<ConfigUpdatedEventArgs> ConfigUpdated; 
        string Name { get; } 
        #region Логирование 
        TraceLevel TraceLevel { get; } 
        string LogFileFolder { get; } 
        bool SeparateLog 
        { 
            get; 
            set; 
        } 
        void DisposeLogger(); 
        #endregion 
        int DisposeOrder { get; } 
    } 
}
