using System; 

using Croc.Bpc.Synchronization; 

using Croc.Workflow.ComponentModel; 

using System.Threading; 

using Croc.Core; 

using Croc.Core.Extensions; 

using Croc.Bpc.Election.Voting; 

using Croc.Bpc.Common.Diagnostics; 

 

 

namespace Croc.Bpc.Workflow.Activities.Initialization 

{ 

    /// <summary> 

    /// Контроль начала голосования 

    /// </summary> 

    [Serializable] 

    public class ControlVotingStartActivity : BpcCompositeActivity 

    { 

        /// <summary> 

        /// Событие "Пора закончить тренировку" 

        /// </summary> 

        public event EventHandler NeedFinishTraining; 

 

 

        /// <summary> 

        /// Сколько времени осталось до начала голосования 

        /// </summary> 

        public TimeSpan VotingStartRemainingTime 

        { 

            get 

            { 

                // получим дату со временем, когда должно начаться голосование 

                var votingStartDateTime = _electionManager.SourceData.ElectionDate + 

                    _electionManager.SourceData.GetVotingModeStartTime(VotingMode.Main); 

 

 

                // вычислим, сколько времени осталось до начала голосования 

                return votingStartDateTime - _electionManager.SourceData.LocalTimeNow; 

            } 

        } 

 

 

        /// <summary> 

        /// Режим выборов - тренировка? 

        /// </summary> 

        public NextActivityKey IsElectionModeTraining( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _electionManager.SourceData.ElectionMode == ElectionMode.Training 

                ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 


        } 

 

 

        /// <summary> 

        /// Запускает поток отслеживания времени начала голосования 

        /// </summary> 

        public NextActivityKey StartControlThread( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // если поток еще не создан 

            if (_controlThread == null) 

            { 

                // получим параметры потока 

                var controlThreadParams = new ControlThreadParameters() 

                { 

                    ControlPeriod = parameters.GetParamValue("ControlPeriod", TimeSpan.FromMinutes(1)), 

                    MinTimeToVotingStart = parameters.GetParamValue("MinTimeToVotingStart", TimeSpan.FromHours(2)) 

                }; 

 

 

                // создадим и запустим поток 

                _controlThread = new Thread(ControlThread) { IsBackground = true }; 

                _controlThread.Start(controlThreadParams); 

                _logger.LogInfo(Message.WorkflowControlThreadStarted); 

            } 

 

 

            return BpcNextActivityKeys.Yes; 

        } 

 

 

        /// <summary> 

        /// Потока отслеживания времени начала голосования 

        /// </summary> 

        private Thread _controlThread; 

 

 

        /// <summary> 

        /// Параметры потока отслеживания времени начала голосования 

        /// </summary> 

        private struct ControlThreadParameters 

        { 

            /// <summary> 

            /// Период, с которым будет выполняться проверка времени 

            /// </summary> 

            public TimeSpan ControlPeriod; 

            /// <summary> 

            /// Минимальное время до начала голосования,  

            /// при достижении которого нужно начать работу в боевом режиме 

            /// </summary> 


            public TimeSpan MinTimeToVotingStart; 

        } 

 

 

        /// <summary> 

        /// Метод потока отслеживания времени начала голосования 

        /// </summary> 

        private void ControlThread(object state) 

        { 

            try 

            { 

                // FIX: нужно некоторое время подождать, т.к. если возбуждать событие "Пора закончить тренировку" 

                // сразу же, то инициализация не успеет завершиться и событие не будет обработано 

                if (CoreApplication.Instance.ExitEvent.WaitOne(TimeSpan.FromSeconds(3))) 

                    return; 

 

 

                var parameters = (ControlThreadParameters)state; 

 

 

                while (true) 

                { 

                    // если ИД заданы 

                    if (_electionManager.SourceData != null) 

                    { 

                        // то проверим, сколько осталось времени до начала голосования 

 

 

                        // если до начала голосования осталось меньше, чем Минимальное время 

                        if (VotingStartRemainingTime - parameters.MinTimeToVotingStart < TimeSpan.Zero) 

                        { 

                            _logger.LogInfo(Message.WorkflowControlThreadNeedFinishTraining); 

 

 

                            // то возбуждаем событие 

                            NeedFinishTraining.RaiseEvent(this); 

 

 

                            // FIX: контрольный сброс УИК. Добавлен потому,  

                            // что иногда не работает переход через поток работ 

 

 

                            // дадим время на воспроизведение фразы 

                            if (CoreApplication.Instance.ExitEvent.WaitOne(TimeSpan.FromSeconds(10))) 

                                return; 

 

 

                            // выключим синхронизацию 

                            _syncManager.SynchronizationEnabled = false; 

                            // сбросим ПО на удаленном 


                            _syncManager.RemoteScanner.ResetSoft(); 

                            // сбросим ПО на нашем сканере 

                            _syncManager.ResetSoft(); 

 

 

                            return; 

                        } 

                    } 

 

 

                    if (CoreApplication.Instance.ExitEvent.WaitOne(parameters.ControlPeriod)) 

                        // сработало событие завершения работы приложения => выходим 

                        return; 

 

 

                    // таймаут => идем на след. итерацию 

                } 

            } 

            finally 

            { 

                _controlThread = null; 

            } 

        } 

    } 

}


