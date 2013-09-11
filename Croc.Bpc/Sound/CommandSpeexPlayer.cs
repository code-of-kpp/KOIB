using System; 

using System.Diagnostics; 

using System.IO; 

using System.Threading; 

using Croc.Bpc.Common; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Core.Diagnostics; 

using Croc.Core.Extensions; 

 

 

namespace Croc.Bpc.Sound 

{ 

    /// <summary> 

    /// Проигрыватель Speex-файлов с помощью утилиты командной строки 

    /// </summary> 

    public sealed class CommandSpeexPlayer : ISoundPlayer 

    { 

        /// <summary> 

        /// Внешняя команда проигрывания звука, принимает один параметр - имя файла 

        /// </summary> 

        private const string PLAY_COMMAND = "./playspx"; 

 

 

        /// <summary> 

        /// Логгер 

        /// </summary> 

        protected ILogger _logger; 

 

 

        /// <summary> 

        /// Объект для синхронизации воспроизведения звук. файлов 

        /// </summary> 

        private static object _playSync = new object(); 

 

 

        /// <summary> 

        /// Процесс, проигрывающий звук 

        /// </summary> 

        private Process _playCommand; 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        public CommandSpeexPlayer(ILogger logger) 

        { 

            CodeContract.Requires(logger != null); 

 

 

            _logger = logger; 


        } 

 

 

        /// <summary> 

        /// Расширение файла 

        /// </summary> 

        public string FileExt 

        { 

            get { return ".spx"; } 

        } 

 

 

        /// <summary> 

        /// Запустить воспроизведение звукового файла 

        /// </summary> 

        /// <param name="soundFilePath"></param> 

        public void Play(string soundFilePath) 

        { 

            CodeContract.Requires(!string.IsNullOrEmpty(soundFilePath)); 

 

 

            if (!soundFilePath.EndsWith(FileExt)) 

                soundFilePath += FileExt; 

 

 

            FileInfo fi = new FileInfo(soundFilePath); 

 

 

            ThreadPool.QueueUserWorkItem(new WaitCallback(PlayThread), fi.FullName); 

        } 

 

 

        /// <summary> 

        /// Остановить воспроизведение 

        /// </summary> 

        public void Stop() 

        { 

            lock (_playSync) 

            { 

                // если процесс еще "жив" 

                if (_playCommand != null ) 

                { 

                    try 

                    { 

                        if (!_playCommand.HasExited) 

                        { 

                            _playCommand.Kill(); 

                            _playCommand = null; 

                        } 

                    } 


                    catch 

                    { 

                        _playCommand = null; 

                    } 

                } 

            } 

        } 

 

 

        /// <summary> 

        /// Тред проигрывания звука 

        /// </summary> 

        /// <param name="state"></param> 

        private void PlayThread(object state) 

        { 

            try 

            { 

                // останавливаем текущее воспроизведение, если оно есть 

                Stop(); 

 

 

                // запускаем воспроизведение 

                _logger.LogVerbose(Message.DebugVerbose, "start playing " + (string)state ?? String.Empty); 

 

 

                var startInfo = new ProcessStartInfo(PLAY_COMMAND, (string) state ?? String.Empty) 

                { 

                    CreateNoWindow = true, 

                    UseShellExecute = false, 

                    RedirectStandardOutput = true, 

                    RedirectStandardError = true 

                }; 

 

 

                using (_playCommand = Process.Start(startInfo)) 

                { 

                    var res = ProcessHelper.StartProcessAndWaitForFinished(_playCommand, null, null); 

                    if (res == ProcessHelper.PROCESS_START_FAILED) 

                        throw new Exception("Ошибка запуска процесса: " + _playCommand); 

                } 

            } 

            catch (ThreadAbortException) 

            { 

                Stop(); 

            } 

            catch (Exception ex) 

            { 

                _logger.LogException(Message.SoundPlayException, ex, (string)state); 

            } 

            finally 


            { 

                // если прерывания не было (или ничего не известно о прерывании) 

                if (_playCommand != null) 

                { 

                    // то известим, что закончили 

                    PlayingStopped.RaiseEvent(this); 

                } 

 

 

                _playCommand = null; 

            } 

        } 

 

 

        /// <summary> 

        /// Событие "воспроизведение остановлено" 

        /// </summary> 

        public event EventHandler PlayingStopped; 

 

 

        #region IDisposable Members 

 

 

        public void Dispose() 

        { 

            // do nothing  

        } 

 

 

        #endregion 

    } 

}


