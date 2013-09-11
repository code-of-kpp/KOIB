using System; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Core; 

using System.Threading; 

 

 

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

                    Console.WriteLine("Unhandled exception: {0}", ex); 

                    CoreApplication.Instance.Logger.LogException(Message.UnhandledException, ex, ex.Message); 

                    // запись в лог асинхронная, поэтому нужно дать ей время 

#if DEBUG 

                    Console.Read(); 

#else 

                    Thread.Sleep(1000); 

#endif 

                } 

                catch (Exception ex2) 

                { 

                    Console.WriteLine(ex2.ToString()); 

                } 

            } 

        } 

    } 

}


