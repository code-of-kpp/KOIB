using System; 
using System.Runtime.InteropServices; 
namespace Croc.Bpc.Sound.Unix 
{ 
    internal class AlsaDevice : AudioDevice 
    { 
        private IntPtr _handle; 
        public AlsaDevice(int latency) 
            : base(latency) 
        { 
            var res = snd_pcm_open(ref _handle, "default", 0, 0); 
            if (res < 0) 
                throw new Exception("Error of openning PCM. Error number: " + res); 
        } 
        public override bool SetFormat(AudioFormat format, int channels, int rate) 
        { 
            return snd_pcm_set_params(_handle, (int)format, 3, channels, rate, 1, Latency) == 0; 
        } 
        public override int PlaySample(byte[] buffer, int numFrames) 
        { 
            const int EINTR = 4; 
            while (true) 
            { 
                var res = snd_pcm_writei(_handle, buffer, numFrames); 
                if (res == -EINTR) 
                    return 0; 
                if (res >= 0) 
                    return res; 
                res = snd_pcm_recover(_handle, res, 1); 
                if (res < 0) 
                    return res; 
            } 
        } 
        public override void WaitForPlayingFinished() 
        { 
            var res = snd_pcm_drain(_handle); 
        } 
        public override void StopPlaying() 
        { 
            var res = snd_pcm_drop(_handle); 
        } 
        public override void Close() 
        { 
            var res = snd_pcm_close(_handle); 
            _handle = IntPtr.Zero; 
        } 
        public override void Dispose() 
        { 
            if (_handle != IntPtr.Zero) 
            { 
                snd_pcm_close(_handle); 
                _handle = IntPtr.Zero; 
            } 
            GC.SuppressFinalize(this); 
        } 
        #region extern libasound methods 
        [DllImport("libasound.so.2")] 
        private static extern int snd_pcm_open(ref IntPtr handle, string pcmName, int stream, int mode); 
        [DllImport("libasound.so.2")] 
        private static extern int snd_pcm_close(IntPtr handle); 
        [DllImport("libasound.so.2")] 
        private static extern int snd_pcm_drain(IntPtr handle); 
        [DllImport("libasound.so.2")] 
        private static extern int snd_pcm_drop(IntPtr handle); 
        [DllImport("libasound.so.2")] 
        private static extern int snd_pcm_writei(IntPtr handle, byte[] buf, int size); 
        [DllImport("libasound.so.2")] 
        private static extern int snd_pcm_recover(IntPtr handle, int err, int silent); 
        [DllImport("libasound.so.2")] 
        private static extern int snd_pcm_set_params( 
            IntPtr handle, int format, int access, int channels, int rate, int softResample, int latency); 


        #endregion 
    } 
}
