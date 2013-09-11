using System; 
using System.Diagnostics; 
using System.Text; 
using System.Threading; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.FileSystem; 
using Croc.Bpc.Keyboard; 
using Croc.Bpc.Utils; 
using Croc.Bpc.Voting; 
using Croc.Bpc.Workflow; 
using Croc.Core; 
using Croc.Core.Diagnostics; 
using Croc.Core.Utils; 
using Mono.Unix.Native; 
namespace Croc.Bpc 
{ 
    public class BpcApplication : CoreApplication 
    { 
        private readonly ILogger _mainApplicationLogger = new FileAppendLogger(Environment.CurrentDirectory, "main");  
        public void Run() 
        { 
            var args = new CommandLineArguments(); 
            if (PlatformDetector.IsUnix) 
                SubscribeToUnixSignals(); 
            LogStartInfo(); 
            SubscribeToMainBpcEvents(); 
            InitExitCommandsTracking(); 
            var quietStart = (args["quiet"] != null); 
            if (quietStart) 
                Logger.LogInfo(Message.Common_QuietStart); 
            StartWorkflow(quietStart); 
            WaitForExit(); 
            SystemHelper.SyncFileSystem(); 
        } 
        private void LogStartInfo() 
        { 
            Logger.LogInfo( 
#if DEBUG 
                Message.Common_ApplicationVersionDebug, 
#else 
                Message.Common_ApplicationVersion, 
#endif 
                ApplicationVersion); 
            Logger.LogInfo(Message.Common_MachineName, Environment.MachineName); 
            Logger.LogInfo(Message.Common_IpAddress, NetHelper.GetLocalIpAddress()); 
            _mainApplicationLogger.LogInfo(Message.ApplicationStart); 
        } 
        private void SubscribeToMainBpcEvents() 
        { 
            Exited += (sender, e) => 
                          { 
                              _mainApplicationLogger.LogInfo(Message.ApplicationEnd, e.ExitType); 
                              Disposer.DisposeObject(_mainApplicationLogger); 
                          }; 
            GetSubsystemOrThrow<IFileSystemManager>().FilesArchived += 
                (sender, e) => _mainApplicationLogger.LogInfo(Message.ArchiveFolder, e.ArchiveName); 
            GetSubsystemOrThrow<IElectionManager>().VotingModeChanged += 
                (sender, e) => _mainApplicationLogger.LogInfo( 
                    Message.VotingModeChange, e.OldMode, e.NewMode, e.BulletinCount); 
        } 
        private void InitExitCommandsTracking() 
        { 
            var keyboard = (IKeyboardManager)GetSubsystemOrThrow<UnionKeyboard>(); 
            keyboard.KeyPressed += (sender, e) => 
            { 
                switch (e.Type) 
                { 
                    case KeyType.Quit: 
                        Exit(ApplicationExitType.Exit); 
                        break; 
                    case KeyType.PowerOff: 
                        Exit(ApplicationExitType.PowerOff); 
                        break; 
                } 
            }; 
        } 
        private void StartWorkflow(bool quietStart) 
        { 
            var workflowManager = GetSubsystemOrThrow<IWorkflowManager>(); 
            workflowManager.WorkflowTerminated += (sender, e) => Exit(ApplicationExitType.RestartApplication); 
            workflowManager.StartWorkflow(quietStart); 
        } 
        #region Обработка Unix-сигналов 
        public void SubscribeToUnixSignals() 
        { 
            var handler = new SignalHandler(UnixSignalHandler); 
#pragma warning disable 612,618 
            Stdlib.signal(Signum.SIGTERM, handler); 
            Stdlib.signal(Signum.SIGINT, handler); 
            Stdlib.signal(Signum.SIGHUP, handler); 
            Stdlib.signal(Signum.SIGKILL, handler); 
            Stdlib.signal(Signum.SIGTSTP, handler); 
            Stdlib.signal(Signum.SIGSEGV, handler); 
            Stdlib.signal(Signum.SIGFPE, handler); 
            Stdlib.signal(Signum.SIGABRT, handler); 
            Stdlib.signal(Signum.SIGILL, handler); 
            Stdlib.signal(Signum.SIGSTOP, handler); 
            Stdlib.signal(Signum.SIGQUIT, handler); 
#pragma warning restore 612,618 
        } 
        private void UnixSignalHandler(int signal) 
        { 
            var receivedSignal = (Signum)signal; 
            switch (receivedSignal) 
            { 
                case Signum.SIGSEGV: 
                case Signum.SIGFPE: 
                case Signum.SIGABRT: // он же SIGIOT 
                case Signum.SIGILL: 
                    LoggingUtils.LogToConsole("Received critical unix signal: " + receivedSignal); 
                    Logger.LogError(Message.Common_UnixCriticalSignalReceived, receivedSignal); 
                    Exit(ApplicationExitType.RestartApplication); 
                    break; 
                default: 
                    Logger.LogInfo(Message.Common_UnixSignalReceived, receivedSignal); 
                    break; 
            } 
        } 
        #endregion 
    } 
}
