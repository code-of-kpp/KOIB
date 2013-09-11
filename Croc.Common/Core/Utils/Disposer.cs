using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
namespace Croc.Core.Utils 
{ 
    public class Disposer 
    { 
        public static void DisposeObject(object obj) 
        { 
            if (obj != null && obj is IDisposable) 
                ((IDisposable)obj).Dispose(); 
        } 
    } 
}
