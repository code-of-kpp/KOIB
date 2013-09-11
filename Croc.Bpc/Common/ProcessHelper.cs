using System; 

using System.Diagnostics; 

using System.Text.RegularExpressions; 

using System.Threading; 

using Croc.Bpc.Common.Config; 

using Croc.Core; 

using Croc.Bpc.Common.Diagnostics; 

 

 

namespace Croc.Bpc.Common 

{ 

    /// <summary> 

    /// Класс для работы с процессами 

    /// </summary> 

    public static class ProcessHelper 

    { 

        /// <summary> 

        /// Запуск процесса завершился прерыванием 

        /// </summary> 

        public const int PROCESS_START_FAILED = -8888; 

 

 

        /// <summary> 

        /// Обработчик потока вывода процесса 

        /// </summary> 

        /// <param name="line"></param> 

        /// <returns>true - остановить обработку</returns> 

        public delegate bool ProcessOutputProcessor(string line); 

 

 

        /// <summary> 

        /// Запустить внешний процесс и ждать его завершения 

        /// </summary> 

        /// <param name="command">Наименование команды</param> 

        /// <param name="parameters">Строка параметров</param> 

        /// <param name="stdout">Обработчик стандартного вывода процесса</param> 

        /// <param name="stderr">Обработчик вывода ошибок процесса</param> 

        /// <returns>Код завершения внешнего процесса</returns> 

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

                    return StartProcessAndWaitForFinished(process, stdout, stderr); 

                } 

            } 

            catch 

            { 

                return PROCESS_START_FAILED; 

            } 

        } 

 

 

        /// <summary> 

        /// Запустить внешний процесс и ждать его завершения 

        /// </summary> 

        /// <param name="process">Процесс</param> 

        /// <param name="stdout">Обработчик стандартного вывода процесса</param> 

        /// <param name="stderr">Обработчик вывода ошибок процесса</param> 

        /// <returns>Код завершения внешнего процесса</returns> 

        public static int StartProcessAndWaitForFinished( 

            Process process, ProcessOutputProcessor stdout, ProcessOutputProcessor stderr) 

        { 

            int exitCode = 0; 

 

 

            try 

            { 

                CoreApplication.Instance.Logger.LogVerbose(Message.ProcessStartInfo,  

                    process.Id, process.StartInfo.FileName, process.StartInfo.Arguments); 

                process.WaitForExit(); 

 

 

                // читаем стандартный вывод 

                bool stopFlag = false; 

                while (process.StandardOutput.Peek() >= 0) 

                { 

                    string line = process.StandardOutput.ReadLine(); 

                    CoreApplication.Instance.Logger.LogVerbose(Message.ProcessStdOutDump, 

                        process.Id, line); 

                    if (stdout != null && !stopFlag) 

                    { 

                        stopFlag = stdout(line); 

                    } 

                } 

 

 

                // читаем стандартный поток ошибок 

                stopFlag = false; 


                while (process.StandardError.Peek() >= 0) 

                { 

                    string line = process.StandardError.ReadLine(); 

                    CoreApplication.Instance.Logger.LogVerbose(Message.ProcessStdErrDump, 

                        process.Id, line); 

                    if (stderr != null && !stopFlag) 

                    { 

                        stopFlag = stderr(line); 

                    } 

                } 

 

 

                exitCode = process.ExitCode; 

                CoreApplication.Instance.Logger.LogVerbose(Message.ProcessExitCode, 

                    process.Id, exitCode); 

                process.Close(); 

            } 

            catch 

            { 

                exitCode = PROCESS_START_FAILED; 

            } 

 

 

            return exitCode; 

        } 

 

 

        /// <summary> 

        /// Возвращает другой процесс с таким же именем, как данный 

        /// </summary> 

        /// <returns>если другого такого процесса нет, то вернет null</returns> 

        public static Process GetProcessWithSameName() 

        { 

            var currentProcessName = Process.GetCurrentProcess().GetProcessName(); 

 

 

            foreach (Process process in Process.GetProcessesByName(currentProcessName)) 

                if (currentProcessName == process.GetProcessName()) 

                    return process; 

 

 

            return null; 

        } 

 

 

        /// <summary> 

        /// Получить имя процесса 

        /// </summary> 

        /// <param name="process"></param> 

        /// <returns></returns> 


        public static string GetProcessName(this Process process) 

        { 

            var moduleName = process.MainModule.ModuleName; 

            var name = Regex.Replace(moduleName, ".exe", ""); 

#if DEBUG 

            name = Regex.Replace(name, ".vshost", ""); 

#endif 

            return name; 

        } 

 

 

        /// <summary> 

        /// Выполняет заданную конфиг-команду 

        /// </summary> 

        /// <param name="cmd"></param> 

        public static void ExecCommand(CommandConfig cmd) 

        { 

            if (cmd == null || string.IsNullOrEmpty(cmd.Command)) 

                return; 

 

 

            var res = ProcessHelper.StartProcessAndWaitForFinished(cmd.Command, cmd.Params, null, null); 

            CoreApplication.Instance.Logger.LogVerbose(Message.ExecCommandResult, cmd.Command, cmd.Params, res); 

            if (cmd.SleepInterval != 0) 

            { 

                Thread.Sleep(cmd.SleepInterval); 

            } 

        } 

 

 

    } 

}


