using System; 

using System.IO; 

using System.Threading; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Core.Diagnostics; 

 

 

namespace Croc.Bpc.Sound.Unix 

{ 

    internal abstract class AudioData 

    { 

        protected ILogger _logger; 

 

 

        public ManualResetEvent PlayingFinishedEvent = new ManualResetEvent(false); 

 

 

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

 

 

        public virtual void Setup(AudioDevice dev) 

        { 

            dev.SetFormat(Format, Channels, Rate); 

        } 

 

 

        public abstract void Play(AudioDevice dev); 

 

 

        private bool stopped = false; 

        public bool IsStopped 

        { 

            get 

            { 

                return stopped; 


            } 

            protected set 

            { 

                stopped = value; 

            } 

        } 

    } 

 

 

    internal class WavData : AudioData 

    { 

        private const int CHUNK_HEADER_SIZE = 8; 

        Stream stream; 

        ushort channels; 

        ushort frame_divider; 

        int sample_rate; 

        int data_len = 0; 

        AudioFormat format; 

        AutoResetEvent _stopEvent; 

 

 

        private static string ReadChunkHeader(Stream stream, out int chunkSize) 

        { 

            byte[] buffer = new byte[CHUNK_HEADER_SIZE]; 

            if (stream.Read(buffer, 0, CHUNK_HEADER_SIZE) != CHUNK_HEADER_SIZE) 

                throw new Exception("incorrect format (error of read chunk header)"); 

 

 

            chunkSize = GetInt(buffer, 4); 

 

 

            return System.Text.ASCIIEncoding.ASCII.GetString(buffer, 0, 4); 

        } 

 

 

        private static int GetInt(byte[] buffer, int startIndex) 

        { 

            if (BitConverter.IsLittleEndian) 

            { 

                return (buffer[startIndex] | 

                    (buffer[startIndex + 1] << 8)  | 

                    (buffer[startIndex + 2] << 16) | 

                    (buffer[startIndex + 3] << 24)); 

            } 

            else 

            { 

                return BitConverter.ToInt32(buffer, startIndex); 

            } 

        } 

 


 
        private static ushort GetShort(byte[] buffer, int startIndex) 

        { 

            if (BitConverter.IsLittleEndian) 

            { 

                return (ushort)(buffer[startIndex] | 

                       (buffer[startIndex + 1] << 8)); 

            } 

            else 

            { 

                return BitConverter.ToUInt16(buffer, startIndex); 

            } 

        } 

 

 

        public WavData(ILogger logger, Stream data, AutoResetEvent stopEvent) 

        { 

            _logger = logger; 

            _stopEvent = stopEvent; 

 

 

            stream = data; 

            byte[] buffer = new byte[40]; 

 

 

            // read "RIFF chunk descriptor"  

            int readBytes = stream.Read(buffer, 0, 12); 

            if (readBytes != 12 || 

                buffer[0] != 'R' || buffer[1] != 'I' || buffer[2] != 'F' || buffer[3] != 'F' || 

                buffer[8] != 'W' || buffer[9] != 'A' || buffer[10] != 'V' || buffer[11] != 'E') 

            { 

                throw new Exception("incorrect RIFF chunk descriptor"); 

            } 

 

 

            // read "FTM sub-chunk" header 

            int chunkSize; 

 

 

            if (ReadChunkHeader(stream, out chunkSize) != "fmt ") 

                throw new Exception("incorrect format (fmt)"); 

 

 

            readBytes = stream.Read(buffer, 0, chunkSize); 

            if (readBytes != chunkSize) 

                throw new Exception("incorrect format (Subchunk1)"); 

 

 

            if (GetShort(buffer, 0) != 1) // compression 

                throw new Exception("incorrect format (not PCM)"); 


 
 

            channels = GetShort(buffer, 2); 

            sample_rate = GetInt(buffer, 4); 

/* 

            int avg_bytes = GetInt(buffer, 8); 

 

 

            ushort block_align = GetShort(buffer, 12); 

*/ 

 

 

            ushort sign_bits = GetShort(buffer, 14); 

 

 

            // read next sub-chunk header 

            string chunkName = ReadChunkHeader(stream, out chunkSize); 

            if (chunkName == "fact") 

            { 

                // skip "fact chunk" 

                //readBytes = stream.Read(buffer, 0, chunkSize); 

                stream.Seek(chunkSize, SeekOrigin.Current); 

                chunkName = ReadChunkHeader(stream, out chunkSize); 

            } 

 

 

            if (chunkName != "data") 

                throw new Exception("incorrect format (data)"); 

 

 

            data_len = chunkSize; 

            switch (sign_bits) 

            { 

                case 8: 

                    frame_divider = 1; 

                    format = AudioFormat.U8; break; 

                case 16: 

                    frame_divider = 2; 

                    format = AudioFormat.S16_LE; break; 

                default: 

                    throw new Exception("bits per sample"); 

            } 

        } 

 

 

        /// <summary> 

        /// ?????? ?????? ??? ??????????????? 

        /// </summary> 

        protected const int BUFFER_SIZE = 1024; 

 


 
        /// <summary> 

        /// ??????????? ????? ??? ??????????????? 

        /// </summary> 

        static byte[] sampleBuffer = new byte[BUFFER_SIZE]; 

 

 

        public override void Play(AudioDevice dev) 

        { 

            ThreadPool.QueueUserWorkItem(new WaitCallback(WaitStopEvent)); 

 

 

            int count = data_len; // ??????? ???????? ??? ?? ???????????????? ???? 

            int read; // ???-?? ???? ? ????????? ?????? ??? ??????????????? 

 

 

            // ??? ?? ? ????? ?????? ?????? ?????? ?????? ??????????, ??????? ??????? ???????? ?? ?????? 

            stream.Position = 100; 

 

 

            while (!IsStopped && count > 0) 

            { 

                read = stream.Read(sampleBuffer, 0, System.Math.Min(BUFFER_SIZE, count)); 

                if (read <= 0) 

                    break; 

 

 

                count -= read; 

 

 

                // FIXME: account for leftover bytes 

                dev.PlaySample(sampleBuffer, read / frame_divider); 

 

 

            } 

 

 

            if (dev.AfterPlayDelay == 0) 

                dev.Wait(); 

            else 

                Thread.Sleep(dev.AfterPlayDelay); 

 

 

            dev.Close(); 

 

 

            PlayingFinishedEvent.Set(); 

        } 

 

 


        private void WaitStopEvent(object state) 

        { 

            var index = WaitHandle.WaitAny(new WaitHandle[] { PlayingFinishedEvent, _stopEvent }); 

 

 

            if (index == 0) 

                return; 

 

 

            IsStopped = true; 

        } 

 

 

        public override int Channels 

        { 

            get { return channels; } 

        } 

        public override int Rate 

        { 

            get { return sample_rate; } 

        } 

        public override AudioFormat Format 

        { 

            get { return format; } 

        } 

    } 

}


