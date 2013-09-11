using System; 
using System.Diagnostics; 
using System.IO; 
using System.Threading; 
using Croc.Bpc.Utils; 
using Croc.Bpc.Diagnostics; 
using Croc.Core; 
using Croc.Core.Diagnostics; 
using Croc.Core.Extensions; 
using Croc.Core.Utils.Threading; 
namespace Croc.Bpc.Sound 
{ 
    public sealed class CommandSpeexPlayer : ISoundPlayer 
    { 
        private const string PLAY_COMMAND = "./playspx.sh"; 
        private readonly ILogger _logger; 
        private static readonly object s_playSync = new object(); 
        private Process _playCommand; 
        public CommandSpeexPlayer(ILogger logger) 
        { 
            CodeContract.Requires(logger != null); 
            _logger = logger; 
        } 
        public string FileExt 
        { 
            get { return ".spx"; } 
        } 
        public void Play(string soundFilePath) 
        { 
            CodeContract.Requires(!string.IsNullOrEmpty(soundFilePath)); 
            if (!soundFilePath.EndsWith(FileExt)) 
                soundFilePath += FileExt; 
            var fi = new FileInfo(soundFilePath); 
            ThreadUtils.StartBackgroundThread(PlayThread, fi.FullName); 
        } 
        public void Stop() 
        { 
            lock (s_playSync) 
            { 
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
        private void PlayThread(object state) 
        { 
            try 
            { 
                Stop(); 
                _logger.LogVerbose(Message.SoundSpeexStartPlay, (string)state ?? String.Empty); 
                var startInfo = new ProcessStartInfo(PLAY_COMMAND, (string) state ?? String.Empty) 
                { 
                    CreateNoWindow = true, 
                    UseShellExecute = false, 
                    RedirectStandardOutput = true, 
                    RedirectStandardError = true 
                }; 
                using (_playCommand = Process.Start(startInfo)) 
                { 
                    var res = ProcessHelper.WaitForProcessFinished(_playCommand, null, null); 
                    if (res == ProcessHelper.PROCESS_EXECUTION_FAILED) 
                        throw new Exception("Ошибка выполнения процесса: " + _playCommand); 
                } 
            } 
            catch (ThreadAbortException) 
            { 
                Stop(); 
            } 
            catch (Exception ex) 
            { 
                _logger.LogError(Message.SoundPlayError, ex, (string)state); 
            } 
            finally 
            { 
                if (_playCommand != null) 
                { 
                    PlayingStopped.RaiseEvent(this); 
                } 
                _playCommand = null; 
            } 
        } 
        public event EventHandler PlayingStopped; 
        #region IDisposable Members 
        public void Dispose() 
        { 
        } 
        #endregion 
    } 
}
