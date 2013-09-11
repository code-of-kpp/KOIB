using System; 
using System.IO; 
using Croc.Core.Diagnostics; 
namespace Croc.Bpc.Sound 
{ 
    public sealed class WavPlayer : AbstractWavPlayer 
    { 
        public WavPlayer(ILogger logger, int latency) 
            : base(logger, latency) 
        { 
        } 
        public override string FileExt 
        { 
            get { return ".wav"; } 
        } 
        protected override Stream InternalReadFile(string filePath) 
        { 
            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read); 
        } 
    } 
}
