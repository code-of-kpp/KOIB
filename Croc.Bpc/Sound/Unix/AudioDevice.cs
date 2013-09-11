using System; 

 

 

namespace Croc.Bpc.Sound.Unix 

{ 

    internal class AudioDevice : IDisposable 

    { 

        /// <summary> 

        /// ?????? ?????? ? ????????????? (100000 - ????????????? 0.1 ???) 

        /// </summary> 

        public readonly int Latency; 

        /// <summary> 

        /// ???????? ????? ????????? ??????????????? 

        /// </summary> 

        public readonly int AfterPlayDelay; 

 

 

        /// <summary> 

        /// ??????? ?????????? Alsa 

        /// </summary> 

        /// <param name="latency">?????? ?????? ? ????????????? (100000 - ????????????? 0.1 ???)</param> 

        /// <returns></returns> 

        public static AudioDevice CreateAlsaDevice(int latency, int afterPlayDelay)  

        { 

            return new AlsaDevice(latency, afterPlayDelay); 

		} 

 

 

        protected AudioDevice(int latency, int afterPlayDelay) 

        { 

            Latency = latency; 

            AfterPlayDelay = afterPlayDelay; 

        } 

 

 

        public virtual bool SetFormat(AudioFormat format, int channels, int rate) 

        { 

            return true; 

        } 

 

 

		public virtual int PlaySample (byte[] buffer, int num_frames)  

        { 

			return num_frames; 

		} 

 

 

		public virtual void Wait ()  

        { 

		} 


 
 

        public virtual void Close() 

        { 

        } 

 

 

        #region IDisposable Members 

 

 

        public virtual void Dispose() 

        { 

        } 

 

 

        #endregion 

    } 

} 

 


