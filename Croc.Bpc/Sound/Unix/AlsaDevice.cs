using System; 

using System.Runtime.InteropServices; 

 

 

namespace Croc.Bpc.Sound.Unix 

{ 

    internal class AlsaDevice : AudioDevice 

    { 

        private IntPtr _handle; 

 

 

        public AlsaDevice(int latency, int afterPlayDelay) 

            : base(latency, afterPlayDelay) 

        { 

            int err = snd_pcm_open(ref _handle, "default", 0, 0); 

            if (err < 0) 

                throw new Exception("Error of openning PCM. Error number: " + err); 

        } 

 

 

        public override bool SetFormat(AudioFormat format, int channels, int rate) 

        { 

            return snd_pcm_set_params(_handle, (int)format, 3, channels, rate, 1, Latency) == 0; 

        } 

 

 

        public override int PlaySample(byte[] buffer, int num_frames) 

        { 

            return snd_pcm_writei(_handle, buffer, num_frames); 

        } 

 

 

        public override void Wait() 

        { 

            snd_pcm_drain(_handle); 

        } 

 

 

        public override void Close() 

        { 

            snd_pcm_close(_handle); 

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

        static extern int snd_pcm_open(ref IntPtr handle, string pcm_name, int stream, int mode); 

 

 

        [DllImport("libasound.so.2")] 

        static extern int snd_pcm_close(IntPtr handle); 

 

 

        [DllImport("libasound.so.2")] 

        static extern int snd_pcm_drain(IntPtr handle); 

 

 

        [DllImport("libasound.so.2")] 

        static extern int snd_pcm_writei(IntPtr handle, byte[] buf, int size); 

 

 

        [DllImport("libasound.so.2")] 

        static extern int snd_pcm_set_params( 

            IntPtr handle, int format, int access, int channels, int rate, int soft_resample, int latency); 

 

 

        #endregion 

    } 

}


