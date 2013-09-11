using System; 
using System.Diagnostics; 
using System.Text.RegularExpressions; 
using System.Threading; 
using Croc.Bpc.Config; 
using Croc.Bpc.Diagnostics; 
using Croc.Core; 
namespace Croc.Bpc.Utils 
{ 
    public static class ProcessHelper 
    { 
        public const int PROCESS_START_FAILED = -8888; 
        public const int PROCESS_EXECUTION_FAILED = -9999; 
        public delegate bool ProcessOutputProcessor(ProcessOutputProcessorState state); 
        public static int StartProcessAndWaitForFinished( 
            string command, string parameters, ProcessOutputProcessor stdout, ProcessOutputProcessor stderr) 
        { 
            try 
            { 
                var startInfo = new ProcessStartInfo(command, parameters ?? String.Empty) 
                { 
                    CreateNoWindow = true, 
                    UseShellExecute = false, 
                    RedirectStandardOutput = true, 
                    RedirectStandardError = true 
                }; 
                using (var process = Process.Start(startInfo)) 
                { 
                    return WaitForProcessFinished(process, stdout, stderr); 
                } 
            } 
            catch (Exception ex) 
            { 
                CoreApplication.Instance.Logger.LogError(Message.Common_ProcessStartFailed, command, parameters, ex.Message); 
                return PROCESS_START_FAILED; 
            } 
        } 
        public static int WaitForProcessFinished( 
            Process process, ProcessOutputProcessor stdout, ProcessOutputProcessor stderr) 
        { 
            int exitCode; 
            try 
            { 
                CoreApplication.Instance.Logger.LogVerbose(Message.Common_ProcessStartInfo,  
                    process.Id, process.StartInfo.FileName, process.StartInfo.Arguments); 
                process.WaitForExit(); 
                var stopFlag = false; 
                var index = 0; 
                while (process.StandardOutput.Peek() >= 0) 
                { 
                    var line = process.StandardOutput.ReadLine(); 
                    CoreApplication.Instance.Logger.LogVerbose(Message.Common_ProcessStdOutDump, 
                        process.Id, line); 
                    if (stdout != null && !stopFlag) 
                    { 
                        stopFlag = stdout(new ProcessOutputProcessorState(line, ++index)); 
                    } 
                } 
                stopFlag = false; 
                index = 0; 
                while (process.StandardError.Peek() >= 0) 
                { 
                    var line = process.StandardError.ReadLine(); 
                    CoreApplication.Instance.Logger.LogVerbose(Message.Common_ProcessStdErrDump, 
                        process.Id, line); 
                    if (stderr != null && !stopFlag) 
                    { 
                        stopFlag = stderr(new ProcessOutputProcessorState(line, ++index)); 
                    } 
                } 
                exitCode = process.ExitCode; 
                CoreApplication.Instance.Logger.LogVerbose(Message.Common_ProcessExitCode, 
                    process.Id, exitCode); 
                process.Close(); 
            } 
            catch (Exception ex) 
            { 
                CoreApplication.Instance.Logger.LogError(Message.Common_ProcessExecutionFailed, 
                    process.StartInfo.FileName, process.StartInfo.Arguments, ex.Message); 
                exitCode = PROCESS_EXECUTION_FAILED; 
            } 
            return exitCode; 
        } 
        public static Process GetProcessWithSameName() 
        { 
            var currentProcessName = Process.GetCurrentProcess().GetProcessName(); 
            foreach (var process in Process.GetProcessesByName(currentProcessName)) 
                if (currentProcessName == process.GetProcessName()) 
                    return process; 
            return null; 
        } 
        public static string GetProcessName(this Process process) 
        { 
            if (process.MainModule == null) 
                return string.Empty; 
            var moduleName = process.MainModule.ModuleName; 
            var name = Regex.Replace(moduleName, ".exe", ""); 
#if DEBUG 
            name = Regex.Replace(name, ".vshost", ""); 
#endif 
            return name; 
        } 
        public static void ExecCommand(CommandConfig cmd) 
        { 
            if (cmd == null || string.IsNullOrEmpty(cmd.Command)) 
                return; 
            var res = StartProcessAndWaitForFinished(cmd.Command, cmd.Params, null, null); 
            CoreApplication.Instance.Logger.LogVerbose(Message.Common_ExecCommandResult, cmd.Command, cmd.Params, res); 
            if (cmd.SleepInterval != 0) 
            { 
                Thread.Sleep(cmd.SleepInterval); 
            } 
        } 
    } 
}
