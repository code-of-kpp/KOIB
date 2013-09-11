using System; 
using System.IO; 
using Croc.Core.Diagnostics; 
using cspeex; 
namespace Croc.Bpc.Sound 
{ 
    public sealed class SpeexPlayer : AbstractWavPlayer 
    { 
        private readonly JSpeexDec _decoder = new JSpeexDec(); 
        public SpeexPlayer(ILogger logger, int latency) 
            : base(logger, latency) 
        { 
            _decoder.setDestFormat(JSpeexDec.FILE_FORMAT_WAVE); 
        } 
        public override string FileExt 
        { 
            get { return ".spx"; } 
        } 
        private Stream GetDecodedStream(Stream stream) 
        { 
            MemoryStream memoryStream = new MemoryStream(); 
            _decoder.decode(new java.io.RandomInputStream(stream), new java.io.RandomOutputStream(memoryStream)); 
            return memoryStream; 
        } 
        protected override Stream InternalReadFile(string filePath) 
        { 
            return GetDecodedStream(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)); 
        } 
    } 
}
