using System; 
namespace Croc.Bpc.Election 
{ 
    public class SourceDataVerifierException : Exception 
    { 
        public SourceDataVerifierException(string message) 
            : base(message) 
        {} 
        public SourceDataVerifierException(string message, Exception innerEx) 
            : base(message, innerEx) 
        { } 
    } 
}
