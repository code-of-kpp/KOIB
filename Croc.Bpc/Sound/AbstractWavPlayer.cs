using System; 

using Croc.Bpc.Common.Diagnostics; 

using System.Threading; 

using System.IO; 

using Croc.Core.Utils; 

using System.Media; 

using Croc.Bpc.Sound.Unix; 

using Croc.Core.Diagnostics; 

using Croc.Core.Extensions; 

 

 

namespace Croc.Bpc.Sound 

{ 

    /// <summary> 

    /// Проигрыватель wav-файлов 

    /// </summary> 

    /// <remarks>В зависимости от платформы (Windows/Unix) воспроизведение выполняется разными движками</remarks> 

    public abstract class AbstractWavPlayer : ISoundPlayer 

    { 

		/// <summary> 

		/// Путь к пустому звуковому файлу 

		/// </summary> 

		private const string EMPTY_SOUND_FILE_PATH = "empty"; 

 

 

        /// <summary> 

        /// Объект для синхронизации воспроизведения звук. файлов 

        /// </summary> 

        private static object _playSync = new object(); 

        /// <summary> 

        /// Логгер 

        /// </summary> 

        protected ILogger _logger; 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="latency">Размер буфера устройства в микросекундах</param> 

        /// <param name="afterPlayDelay">Задержка после воспроизведения</param> 

        public AbstractWavPlayer(ILogger logger, int latency, int afterPlayDelay) 

        { 

            CodeContract.Requires(logger != null); 

            CodeContract.Requires(latency > 0); 

            CodeContract.Requires(afterPlayDelay >= 0); 

 

 

            _logger = logger; 

            _latency = latency; 

            _afterPlayDelay = afterPlayDelay; 


 
 

            if (!PlatformDetector.IsUnix) 

                _winPlayer = new SoundPlayer(); 

        } 

 

 

        /// <summary> 

        /// Расширение файла 

        /// </summary> 

        public abstract string FileExt { get; } 

 

 

        /// <summary> 

        /// Загружает файл в поток  

        /// </summary> 

        /// <param name="filePath">Путь к файлу</param> 

        /// <returns>Поток в виде WAV файла</returns> 

        protected abstract Stream InternalReadFile(string filePath); 

 

 

        /// <summary> 

        /// Загружает файл в поток  

        /// </summary> 

        /// <param name="filePath">Путь к файлу</param> 

        /// <returns>Поток в виде WAV файла</returns> 

        private Stream ReadFile(string filePath) 

        { 

            // проверим, что файл существует 

			if (!File.Exists(filePath)) 

			{ 

				// логируем сообщение о том, что файл не найден 

				_logger.LogError(Message.SoundFileNotFound, filePath); 

#if DEBUG 

				throw new Exception("Файл не найден: " + filePath); 

#else 

				// заменим файл на пустой 

				filePath = filePath.Replace( 

					Path.GetFileNameWithoutExtension(filePath), 

					EMPTY_SOUND_FILE_PATH); 

#endif 

			} 

 

 

            try 

            { 

                return InternalReadFile(filePath); 

            } 

            catch (ThreadAbortException ex) 

            { 


                throw ex; 

            } 

            catch (Exception ex) 

            { 

                throw new Exception("Ошибка при декодировании файла: " + filePath, ex); 

            } 

        } 

 

 

        #region ISoundPlayer Members 

 

 

        public void Play(string soundFilePath) 

        { 

            CodeContract.Requires(!string.IsNullOrEmpty(soundFilePath)); 

 

 

            if (!soundFilePath.EndsWith(FileExt)) 

                soundFilePath += FileExt; 

 

 

            if (PlatformDetector.IsUnix) 

                UnixPlay(soundFilePath); 

            else 

                WinPlay(soundFilePath); 

        } 

 

 

        public void Stop() 

        { 

            if (PlatformDetector.IsUnix) 

                UnixStop(); 

            else 

                WinStop(); 

        } 

 

 

        public event EventHandler PlayingStopped; 

 

 

        #endregion 

 

 

        #region Реализация для Windows 

 

 

        private SoundPlayer _winPlayer; 

        private Thread _currentWinPlayThread; 

 

 


        /// <summary> 

        /// Воспроизвести звук. файл 

        /// </summary> 

        /// <param name="soundFilePath"></param> 

        private void WinPlay(string soundFilePath) 

        { 

            lock (_playSync) 

            { 

                if (_currentWinPlayThread != null && 

                    _currentWinPlayThread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId) 

                { 

                    _currentWinPlayThread.Abort(); 

                    _currentWinPlayThread.Join(250); 

                    _currentWinPlayThread = null; 

                } 

 

 

                _currentWinPlayThread = new Thread(new ParameterizedThreadStart(WinPlayThread)); 

                _currentWinPlayThread.Start(soundFilePath); 

            } 

        } 

 

 

        /// <summary> 

        /// Метод потока, в котором воспроизводим звук 

        /// </summary> 

        /// <param name="state"></param> 

        private void WinPlayThread(object state) 

        { 

            try 

            { 

                // загрузим звук. файл в память 

                using (var stream = ReadFile((string)state)) 

                { 

                    _winPlayer.Stream = stream; 

 

 

                    // вычислим время, которое потребуется на воспроизведение файла по формуле: 

                    // time = ((total size - header size) / (sample rate * (bit rate / 8))) / number of channels 

                    // полученное значение *1000, чтобы получить мсек, и +100 мсек для запаса 

                    var time = TimeSpan.FromMilliseconds((_winPlayer.Stream.Length - 44.0) / (44100 * (16 / 8)) / 1 * 1000 + 100); 

 

 

                    _winPlayer.Play(); 

 

 

                    // ждем, пока файл проиграет или не остановят 

                    Thread.Sleep(time); 

                } 

 


 
                // говорим, что воспроизведение закончено 

                PlayingStopped.RaiseEvent(this); 

            } 

            catch (ThreadAbortException) 

            { 

            } 

            catch (Exception ex) 

            { 

                _logger.LogException(Message.SoundPlayException, ex, (string)state); 

            } 

        } 

 

 

        /// <summary> 

        /// Остановить воспроизведение 

        /// </summary> 

        private void WinStop() 

        { 

            Thread forAbort; 

            lock (_playSync) 

            { 

                forAbort = _currentWinPlayThread; 

                _currentWinPlayThread = null; 

                _winPlayer.Stop(); 

 

 

                if (_winPlayer.Stream != null) 

                    _winPlayer.Stream.Close(); 

            } 

 

 

            // это сделано потому, что при возникновении ошибки, когда звук. файл не найден, 

            // вызов Abort() приводит к прерыванию данного треда, в результате _playSync остается  

            // заблокированной и выполнение приложения зависает в блокировке при попытке  

            // воспроизвести фразу "Обратитесь в службу поддержки" 

            if (forAbort != null) 

            { 

                forAbort.Abort(); 

                forAbort.Join(250); 

            } 

        } 

 

 

        #endregion 

 

 

        #region Реализация для Unix 

 

 


        private int _latency; 

        private int _afterPlayDelay; 

        /// <summary> 

        /// Wav-данные для воспроизведения 

        /// </summary> 

        private WavData _unixWavData; 

        /// <summary> 

        /// Событие остановки воспроизведения 

        /// </summary> 

        private AutoResetEvent _unixPlayStopEvent = new AutoResetEvent(false); 

 

 

 

 

        private void UnixPlay(string soundFilePath) 

        { 

            ThreadPool.QueueUserWorkItem(new WaitCallback(UnixPlayThread), soundFilePath); 

        } 

 

 

        private void UnixPlayThread(object state) 

        { 

            Stream stream = null; 

            AudioDevice dev = null; 

            try 

            { 

                // подготавливаем данные для воспроизведения 

                lock (_playSync) 

                { 

                    // и одновременно останавливаем текущее воспроизведение, если оно есть 

                    UnixStop(); 

 

 

                    // создаем устройство 

                    dev = AudioDevice.CreateAlsaDevice(_latency, _afterPlayDelay); 

 

 

                    _unixPlayStopEvent.Reset(); 

                    stream = ReadFile((string)state); 

                    _unixWavData = new WavData(_logger, stream, _unixPlayStopEvent); 

                } 

 

 

                // запускаем воспроизведение 

                _unixWavData.Setup(dev); 

                _unixWavData.Play(dev); 

 

 

//                _logger.LogVerbose(Message.DebugVerbose, string.Format("playing done (interrupted = {0})", 

  //                  _unixWavData.IsStopped)); 


            } 

            catch (ThreadAbortException) 

            { 

            } 

            catch (Exception ex) 

            { 

                _logger.LogException(Message.SoundPlayException, ex, (string)state); 

            } 

            finally 

            { 

                if (dev != null) 

                { 

                    dev.Dispose(); 

                } 

 

 

                if (stream != null) 

                    stream.Close(); 

 

 

                // если прерывания не было (или ничего не известно о прерывании) 

                if (_unixWavData == null || !_unixWavData.IsStopped) 

                    // то известим, что закончили 

                    PlayingStopped.RaiseEvent(this); 

            } 

        } 

 

 

        private void UnixStop() 

        { 

            lock (_playSync) 

            { 

                if (_unixWavData != null) 

                { 

                    // сообщаем, что надо остановиться 

                    _logger.LogVerbose(Message.DebugVerbose, "set stop event"); 

                    _unixPlayStopEvent.Set(); 

 

 

                    // ждем, когда остановится 

                    _logger.LogVerbose(Message.DebugVerbose, "wait for stop"); 

                    _unixWavData.PlayingFinishedEvent.WaitOne(); 

                } 

            } 

        } 

 

 

        #endregion 

 

 


        #region IDisposable Members 

 

 

        public void Dispose() 

        { 

            if (!PlatformDetector.IsUnix && _winPlayer != null) 

            { 

                if (_winPlayer.Stream != null) 

                    _winPlayer.Stream.Close(); 

 

 

                _winPlayer.Dispose(); 

            } 

 

 

            GC.SuppressFinalize(this); 

        } 

 

 

        #endregion 

    } 

}


