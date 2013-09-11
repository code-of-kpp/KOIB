using System; 
using System.Threading; 
using Croc.Core.Extensions; 
using Croc.Core.Utils.Threading; 
namespace Croc.Bpc.Sound 
{ 
    public class SilentPlayer : ISoundPlayer 
    { 
        #region ISoundPlayer Members 
        public string FileExt 
        { 
            get { return string.Empty; } 
        } 
        public void Play(string soundFilePath) 
        { 
            ThreadUtils.StartBackgroundThread( 
                () => 
                    { 
                        Thread.Sleep(100); 
                        PlayingStopped.RaiseEvent(this); 
                    }); 
        } 
        public void Stop() 
        { 
        } 
        public event EventHandler PlayingStopped; 
        #endregion 
        #region IDisposable Members 
        public void Dispose() 
        { 
        } 
        #endregion 
    } 
}
