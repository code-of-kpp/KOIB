using System; 
namespace Croc.Bpc.Sound.Unix 
{ 
    internal abstract class AudioDevice : IDisposable 
    { 
        public readonly int Latency; 
        public static AudioDevice CreateAlsaDevice(int latency) 
        { 
            return new AlsaDevice(latency); 
        } 
        protected AudioDevice(int latency) 
        { 
            Latency = latency; 
        } 
        public abstract bool SetFormat(AudioFormat format, int channels, int rate); 
        public abstract int PlaySample(byte[] buffer, int numFrames); 
        public abstract void WaitForPlayingFinished(); 
        public abstract void StopPlaying(); 
        public abstract void Close(); 
        #region IDisposable Members 
        public virtual void Dispose() 
        { 
        } 
        #endregion 
    } 
}
