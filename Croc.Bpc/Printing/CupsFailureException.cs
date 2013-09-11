using System; 
namespace Croc.Bpc.Printing 
{ 
    public class CupsFailureException : Exception 
    { 
        public CupsFailureException(string message) 
            : base(message) 
        {} 
        public CupsFailureException(string message, Exception innerEx) 
            : base(message, innerEx) 
        { } 
    } 
}
