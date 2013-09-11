using System; 
using System.Collections.Generic; 
using System.Diagnostics; 
using System.Threading; 
using Croc.Core.Diagnostics; 
using Croc.Core.Configuration; 
namespace Croc.Core 
{ 
    public interface ICoreApplication : ILoggerContainer 
    { 
        string Name { get; } 
        #region Конфигурация 
        ApplicationConfig Config { get; } 
        bool ApplyNewConfig(ApplicationConfig newConfig, bool force); 
        event EventHandler<ConfigUpdatedEventArgs> ConfigUpdated; 
        #endregion 
        #region Логирование 
        TraceLevel TraceLevel { get; } 
        string LogFileFolder { get; } 
        ILogger CreateLogger(string loggerName, TraceLevel traceLevel); 
        bool LoggerEnabled { get; set; } 
        #endregion 
        #region Подсистемы 
        T FindSubsystemImplementsInterface<T>(); 
        T FindSubsystemImplementsInterfaceOrThrow<T>(); 
        IEnumerable<T> FindAllSubsystemsImplementsInterface<T>(); 
        T GetSubsystemOrThrow<T>() where T : ISubsystem; 
        T GetSubsystemOrThrow<T>(string errorMsg) where T : ISubsystem; 
        IEnumerable<ISubsystem> Subsystems { get; } 
        ISubsystem GetSubsystem(String sName); 
        T GetSubsystem<T>(String sName) where T : ISubsystem; 
        T GetSubsystem<T>() where T : ISubsystem; 
        List<KeyValuePair<String, T>> GetSubsystems<T>() where T : ISubsystem; 
        #endregion 
        #region Версия приложения 
        Version ApplicationVersion { get; } 
        #endregion 
        #region Завершение работы 
        WaitHandle ExitEvent { get; } 
        event EventHandler<ApplicationExitEventArgs> Exited; 
        void WaitForExit(); 
        void Exit(ApplicationExitType exitType); 
        #endregion 
    } 
}
