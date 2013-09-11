using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
namespace Croc.Core.Utils 
{ 
    public class PlatformDetector 
    { 
        public static bool IsUnix = (Environment.OSVersion.Platform == PlatformID.Unix); 
    } 
}
