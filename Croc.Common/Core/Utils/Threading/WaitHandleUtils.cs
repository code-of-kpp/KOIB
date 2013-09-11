using System.Threading; 
using Croc.Core.Extensions; 
namespace Croc.Core.Utils.Threading 
{ 
    public static class WaitHandleUtils 
    { 
        public static int WaitOneOrAllOthers(WaitHandle one, WaitHandle[] others) 
        { 
            CodeContract.Requires(one != null); 
            CodeContract.Requires(others != null && others.Length > 1); 
            var occurredEventIndex = WaitHandle.WaitTimeout; 
            var eventSignaled = new ManualResetEvent(false); 
            var waitOneThread = ThreadUtils.StartBackgroundThread( 
                () => 
                    { 
                        try 
                        { 
                            one.WaitOne(); 
                            occurredEventIndex = 0;    
                        } 
                        finally 
                        { 
                            eventSignaled.Set(); 
                        } 
                    }); 
            var waitOthersThread = ThreadUtils.StartBackgroundThread( 
                () => 
                    { 
                        try 
                        { 
                            WaitHandle.WaitAll(others); 
                            occurredEventIndex = 1; 
                        } 
                        finally 
                        { 
                            eventSignaled.Set(); 
                        } 
                    }); 
            eventSignaled.WaitOne(); 
            waitOneThread.SafeAbort(); 
            waitOthersThread.SafeAbort(); 


            return occurredEventIndex; 
        } 
        public static int WaitOneOrTwoOrAllOthers(WaitHandle one, WaitHandle two, WaitHandle[] others) 
        { 
            CodeContract.Requires(one != null); 
            CodeContract.Requires(two != null); 
            CodeContract.Requires(others != null && others.Length > 1); 
            var occurredEventIndex = WaitHandle.WaitTimeout; 
            var eventSignaled = new ManualResetEvent(false); 
            var waitOneThread = ThreadUtils.StartBackgroundThread( 
                () => 
                { 
                    try 
                    { 
                        one.WaitOne(); 
                        occurredEventIndex = 0; 
                    } 
                    finally 
                    { 
                        eventSignaled.Set(); 
                    } 
                }); 
            var waitTwoThread = ThreadUtils.StartBackgroundThread( 
                () => 
                { 
                    try 
                    { 
                        two.WaitOne(); 
                        occurredEventIndex = 1; 
                    } 
                    finally 
                    { 
                        eventSignaled.Set(); 
                    } 
                }); 
            var waitOthersThread = ThreadUtils.StartBackgroundThread( 
                () => 
                { 
                    try 
                    { 
                        WaitHandle.WaitAll(others); 
                        occurredEventIndex = 2; 
                    } 
                    finally 
                    { 
                        eventSignaled.Set(); 
                    } 
                }); 
            eventSignaled.WaitOne(); 
            waitOneThread.SafeAbort(); 
            waitTwoThread.SafeAbort(); 
            waitOthersThread.SafeAbort(); 
            return occurredEventIndex; 
        } 
    } 
}
