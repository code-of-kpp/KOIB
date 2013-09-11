using System; 
namespace Croc.Bpc.Utils 
{ 
    public class ProcessOutputProcessorState 
    { 
        public string Line { get; private set; } 
        public int LineNumber { get; private set; } 
        public ProcessOutputProcessorState(string line, int lineNumber) 
        { 
            Line = line; 
            LineNumber = lineNumber; 
        } 
    } 
}
