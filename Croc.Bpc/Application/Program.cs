using System; 
using Croc.Bpc.Diagnostics; 
using Croc.Core; 
using Croc.Core.Diagnostics; 
namespace Croc.Bpc 
{ 
    class Program 
    { 
        [MTAThread] 
        static void Main(string[] args) 
        { 
            try 
            { 
                var app = new BpcApplication(); 
                app.Run(); 
            } 
            catch(Exception ex) 
            { 
                try 
                { 
                    LoggingUtils.LogToConsole("Unhandled exception: " + ex); 
                    CoreApplication.Instance.Logger.LogError(Message.Common_UnhandledException, ex, ex.Message); 
#if DEBUG 
                    Console.Read(); 
#else 
                    System.Threading.Thread.Sleep(1000); 
#endif 
                } 
                catch (Exception ex2) 
                { 
                    LoggingUtils.LogToConsole(ex2.ToString()); 
                } 
            } 
        } 
    } 
}
