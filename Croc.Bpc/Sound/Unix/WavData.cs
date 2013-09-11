using System; 
using System.IO; 
using System.Text; 
using System.Threading; 
using Croc.Core.Diagnostics; 
using Croc.Core.Utils.Threading; 
namespace Croc.Bpc.Sound.Unix 
{ 
    internal class WavData : AudioData 
    { 
        private const int CHUNK_HEADER_SIZE = 8; 
        private readonly Stream _stream; 
        private readonly ushort _channels; 
        private readonly ushort _frameDivider; 
        private readonly int _sampleRate; 
        private readonly int _dataLen; 
        private readonly AudioFormat _format; 
        private readonly AutoResetEvent _stopEvent; 
        public override int Channels 
        { 
            get { return _channels; } 
        } 
        public override int Rate 
        { 
            get { return _sampleRate; } 
        } 
        public override AudioFormat Format 
        { 
            get { return _format; } 
        } 
        private static string ReadChunkHeader(Stream stream, out int chunkSize) 
        { 
            var buffer = new byte[CHUNK_HEADER_SIZE]; 
            if (stream.Read(buffer, 0, CHUNK_HEADER_SIZE) != CHUNK_HEADER_SIZE) 
                throw new Exception("incorrect format (error of read chunk header)"); 
            chunkSize = GetInt(buffer, 4); 
            return Encoding.ASCII.GetString(buffer, 0, 4); 
        } 
        private static int GetInt(byte[] buffer, int startIndex) 
        { 
            if (BitConverter.IsLittleEndian) 
            { 
                return (buffer[startIndex] | 
                    (buffer[startIndex + 1] << 8) | 
                    (buffer[startIndex + 2] << 16) | 
                    (buffer[startIndex + 3] << 24)); 
            } 
            return BitConverter.ToInt32(buffer, startIndex); 
        } 
        private static ushort GetShort(byte[] buffer, int startIndex) 
        { 
            if (BitConverter.IsLittleEndian) 
            { 
                return (ushort)(buffer[startIndex] | (buffer[startIndex + 1] << 8)); 
            } 
            return BitConverter.ToUInt16(buffer, startIndex); 
        } 
        public WavData(ILogger logger, Stream data, AutoResetEvent stopEvent) 
        { 
            _logger = logger; 
            _stopEvent = stopEvent; 
            _stream = data; 
            var buffer = new byte[40]; 
            var readBytes = _stream.Read(buffer, 0, 12); 
            if (readBytes != 12 || 
                buffer[0] != 'R' || buffer[1] != 'I' || buffer[2] != 'F' || buffer[3] != 'F' || 
                buffer[8] != 'W' || buffer[9] != 'A' || buffer[10] != 'V' || buffer[11] != 'E') 
                throw new Exception("incorrect RIFF chunk descriptor"); 
            int chunkSize; 
            if (string.CompareOrdinal(ReadChunkHeader(_stream, out chunkSize), "fmt ") != 0) 
                throw new Exception("incorrect format (fmt)"); 
            readBytes = _stream.Read(buffer, 0, chunkSize); 
            if (readBytes != chunkSize) 
                throw new Exception("incorrect format (Subchunk1)"); 
            if (GetShort(buffer, 0) != 1) // compression 
                throw new Exception("incorrect format (not PCM)"); 
            _channels = GetShort(buffer, 2); 
            _sampleRate = GetInt(buffer, 4); 
            /* 
                        int avg_bytes = GetInt(buffer, 8); 
                        ushort block_align = GetShort(buffer, 12); 
            */ 
            var signBits = GetShort(buffer, 14); 
            var chunkName = ReadChunkHeader(_stream, out chunkSize); 
            if (string.CompareOrdinal(chunkName, "fact") == 0) 
            { 
                _stream.Seek(chunkSize, SeekOrigin.Current); 
                chunkName = ReadChunkHeader(_stream, out chunkSize); 
            } 
            if (string.CompareOrdinal(chunkName, "data") != 0) 
                throw new Exception("incorrect format (data)"); 
            _dataLen = chunkSize; 
            switch (signBits) 
            { 
                case 8: 
                    _frameDivider = 1; 
                    _format = AudioFormat.U8; 
                    break; 
                case 16: 
                    _frameDivider = 2; 
                    _format = AudioFormat.S16_LE; 
                    break; 
                default: 
                    throw new Exception("bits per sample"); 
            } 
        } 
        protected const int BUFFER_SIZE = 1024; 
        private static readonly byte[] s_sampleBuffer = new byte[BUFFER_SIZE]; 
        public override void Play(AudioDevice dev) 
        { 
            ThreadUtils.StartBackgroundThread(WaitStopEvent); 
            var count = _dataLen; 
            _stream.Position = 100; 
            while (!IsStopped && count > 0) 
            { 
                var read = _stream.Read(s_sampleBuffer, 0, Math.Min(BUFFER_SIZE, count)); 
                if (read <= 0) 
                    break; 
                count -= read; 
                dev.PlaySample(s_sampleBuffer, read / _frameDivider); 
            } 
            if (IsStopped) 
                dev.StopPlaying(); 
            else 
                dev.WaitForPlayingFinished(); 
            dev.Close(); 
            PlayingFinishedEvent.Set(); 
        } 
        private void WaitStopEvent() 
        { 
            var index = WaitHandle.WaitAny(new WaitHandle[] { PlayingFinishedEvent, _stopEvent }); 
            if (index == 0) 
                return; 
            IsStopped = true; 
        } 
    } 
}
