using System.Configuration; 
using System.Diagnostics; 
namespace Croc.Core.Diagnostics.Default 
{ 
    public sealed class SystemTraceWriter : IEventWriter 
    { 
        public SystemTraceWriter() 
        { 
            Trace.AutoFlush = true; 
        } 
        public void Write(string uniqueLogId, string message) 
        { 
            Trace.WriteLine(message); 
            Trace.Flush(); 
        } 
        public void Init(NameValueConfigurationCollection props) 
        { 
        } 
    } 
}
