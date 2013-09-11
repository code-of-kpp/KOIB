using System; 
using System.IO; 
using System.Media; 
using System.Threading; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Sound.Unix; 
using Croc.Core; 
using Croc.Core.Diagnostics; 
using Croc.Core.Extensions; 
using Croc.Core.Utils; 
using Croc.Core.Utils.Threading; 
namespace Croc.Bpc.Sound 
{ 
    public abstract class AbstractWavPlayer : ISoundPlayer 
    { 
        private const string EMPTY_SOUND_FILE_PATH = "empty"; 
        private static readonly object s_playSync = new object(); 
        protected ILogger _logger; 
        protected AbstractWavPlayer(ILogger logger, int latency) 
        { 
            CodeContract.Requires(logger != null); 
            CodeContract.Requires(latency > 0); 


            _logger = logger; 
            _latency = latency; 
            if (!PlatformDetector.IsUnix) 
                _winPlayer = new SoundPlayer(); 
        } 
        public abstract string FileExt { get; } 
        protected abstract Stream InternalReadFile(string filePath); 
        private Stream ReadFile(string filePath) 
        { 
            if (!File.Exists(filePath)) 
            { 
                _logger.LogError(Message.SoundFileNotFound, filePath); 
#if DEBUG 
                throw new Exception("Файл не найден: " + filePath); 
#else 
                filePath = filePath.Replace( 
                    Path.GetFileNameWithoutExtension(filePath), 
                    EMPTY_SOUND_FILE_PATH); 
#endif 
            } 
            try 
            { 
                return InternalReadFile(filePath); 
            } 
            catch (ThreadAbortException) 
            { 
                throw; 
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
        private readonly SoundPlayer _winPlayer; 
        private Thread _currentWinPlayThread; 
        private void WinPlay(string soundFilePath) 
        { 
            lock (s_playSync) 
            { 
                if (_currentWinPlayThread != null && 
                    _currentWinPlayThread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId) 
                { 
                    _currentWinPlayThread.SafeAbort(); 
                    _currentWinPlayThread = null; 
                } 
                _currentWinPlayThread = ThreadUtils.StartBackgroundThread(WinPlayThread, soundFilePath); 
            } 
        } 
        private void WinPlayThread(object state) 
        { 
            try 
            { 
                using (var stream = ReadFile((string)state)) 
                { 
                    _winPlayer.Stream = stream; 
                    var time = TimeSpan.FromMilliseconds( 
                        (_winPlayer.Stream.Length - 44.0) / (24000 * (16 / 8)) / 1 * 1000 + 100); 
                    _winPlayer.Play(); 
                    Thread.Sleep(time); 
                } 
                PlayingStopped.RaiseEvent(this); 
            } 
            catch (ThreadAbortException) 
            { 
            } 
            catch (Exception ex) 
            { 
                _logger.LogError(Message.SoundPlayError, ex, (string)state); 
            } 
        } 
        private void WinStop() 
        { 
            Thread forAbort; 
            lock (s_playSync) 
            { 
                forAbort = _currentWinPlayThread; 
                _currentWinPlayThread = null; 
                _winPlayer.Stop(); 
                if (_winPlayer.Stream != null) 
                    _winPlayer.Stream.Close(); 
            } 
            if (forAbort != null) 
                forAbort.SafeAbort(); 
        } 
        #endregion 
        #region Реализация для Unix 
        private readonly int _latency; 
        private WavData _unixWavData; 
        private readonly AutoResetEvent _unixPlayStopEvent = new AutoResetEvent(false); 
        private void UnixPlay(string soundFilePath) 
        { 
            _unixPlayStopEvent.Reset(); 
            ThreadUtils.StartBackgroundThread(UnixPlayThread, soundFilePath); 
        } 
        private void UnixPlayThread(object state) 
        { 
            if (_unixPlayStopEvent.WaitOne(0)) 
            { 
                return; 
            } 
            Stream stream = null; 
            AudioDevice dev = null; 
            try 
            { 
                lock (s_playSync) 
                { 
                    dev = AudioDevice.CreateAlsaDevice(_latency); 
                    stream = ReadFile((string)state); 
                    _unixWavData = new WavData(_logger, stream, _unixPlayStopEvent); 
                } 
                _unixWavData.Setup(dev); 
                _unixWavData.Play(dev); 
            } 
            catch (ThreadAbortException) 
            { 
            } 
            catch (Exception ex) 
            { 
                _logger.LogError(Message.SoundPlayError, ex, (string)state); 
            } 
            finally 
            { 
                if (dev != null) 
                { 
                    dev.Dispose(); 
                } 
                if (stream != null) 
                { 
                    stream.Close(); 
                } 
                if (_unixWavData == null || !_unixWavData.IsStopped) 
                { 
                    PlayingStopped.RaiseEvent(this); 
                } 
            } 
        } 
        private void UnixStop() 
        { 
            lock (s_playSync) 
            { 
                if (_unixWavData == null) 
                    return; 
            } 
            _unixPlayStopEvent.Set(); 
            AudioData.PlayingFinishedEvent.WaitOne(); 
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
