using System; 

using System.Runtime; 

using Croc.Bpc.Common; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Bpc.Keyboard; 

using Croc.Bpc.Workflow; 

using Croc.Core; 

using Croc.Core.Utils; 

using Mono.Unix.Native; 

 

 

namespace Croc.Bpc 

{ 

    /// <summary> 

    /// Приложение "Bulletin Processing Complex"-а 

    /// </summary> 

    public class BpcApplication : CoreApplication 

    { 

        /// <summary> 

        /// Запуск приложения 

        /// </summary> 

        public void Run() 

        { 

            // если это UNIX 

            if (PlatformDetector.IsUnix) 

                // то подпишемся на unix-сигналы 

                SubscribeToUnixSignals(); 

 

 

            // логируем начальную информацию 

            LogStartInfo(); 

 

 

            // начинаем отслеживать команды на выход 

            InitExitCommandsTracking(); 

 

 

            // запускаем поток работ 

            GetSubsystemOrThrow<IWorkflowManager>().StartWorkflow(); 

 

 

            // ждем завершения работы 

            WaitForExit(); 

        } 

 

 

        /// <summary> 

        /// Логирование начальной информации 

        /// </summary> 

        private void LogStartInfo() 


        { 

            Logger.LogInfo(Message.ApplicationVersion, ApplicationVersion); 

            Logger.LogInfo(Message.MachineName, Environment.MachineName); 

            Logger.LogInfo(Message.IPAddress, NetHelper.GetLocalIPAddress()); 

        } 

 

 

        /// <summary> 

        /// Инициализируем отслеживание команд завершения работы приложения 

        /// </summary> 

        private void InitExitCommandsTracking() 

        { 

            var keyboard = (IKeyboard)GetSubsystemOrThrow<UnionKeyboard>(); 

            keyboard.KeyPressed += (sender, e) => 

            { 

                switch (e.Type) 

                { 

                    case KeyType.Quit: 

                        // Завершить работу приложения 

                        Exit(ApplicationExitType.Exit); 

                        break; 

 

 

                    case KeyType.PowerOff: 

                        // выключаем сканер 

                        Exit(ApplicationExitType.PowerOff); 

                        break; 

                } 

            }; 

        } 

 

 

        #region Обработка Unix-сигналов 

 

 

        /// <summary> 

        /// Подписка на события (unix сигналы) 

        /// </summary> 

        public void SubscribeToUnixSignals() 

        { 

            var handler = new SignalHandler(UnixSignalHandler); 

 

 

            // не смотря на то, что метод Stdlib.signal помечен как obsolete, 

            // только он обеспечивает нормальную обработку сигналов (в частности "kill -9"), 

            // а если делать через UnixSignal.WaitAll, то обработать сигнал не успеваем 

 

 

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

        } 

 

 

        /// <summary> 

        /// Обработка unix-сигналов 

        /// </summary> 

        /// <param name="signal">сигнал</param> 

        private void UnixSignalHandler(int signal) 

        { 

            // протоколируем все сигналы 

            var receivedSignal = (Signum)signal; 

            Logger.LogInfo(Message.UnixSignalReceived, receivedSignal); 

 

 

            // для некотрых сигналов выполняем перезапуск приложения 

            switch (receivedSignal) 

            { 

                case Signum.SIGSEGV: 

                case Signum.SIGFPE: 

                case Signum.SIGABRT: 

                case Signum.SIGILL: 

                    Exit(ApplicationExitType.RestartApplication); 

                    break; 

            } 

        } 

 

 

        #endregion 

    } 

}


