using System.Threading; 
using Croc.Core.Diagnostics; 
namespace Croc.Bpc.Sound.Unix 
{ 
    internal abstract class AudioData 
    { 
        protected ILogger _logger; 


        public static ManualResetEvent PlayingFinishedEvent = new ManualResetEvent(false); 
        protected AudioData() 
        { 
            PlayingFinishedEvent.Reset(); 
        } 
        public abstract int Channels 
        { 
            get; 
        } 
        public abstract int Rate 
        { 
            get; 
        } 
        public abstract AudioFormat Format 
        { 
            get; 
        } 
        public bool IsStopped  
        {  
            get;  
            protected set;  
        } 
        public virtual void Setup(AudioDevice dev) 
        { 
            dev.SetFormat(Format, Channels, Rate); 
        } 
        public abstract void Play(AudioDevice dev); 
    } 
}
