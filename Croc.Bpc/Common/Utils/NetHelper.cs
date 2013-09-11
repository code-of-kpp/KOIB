using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Net; 
namespace Croc.Bpc.Utils 
{ 
    public static class NetHelper 
    { 
        private static readonly List<IPAddress> s_localhostAddressList; 
        static NetHelper() 
        { 
            s_localhostAddressList = new List<IPAddress>(Dns.GetHostEntry(Dns.GetHostName()).AddressList); 
        } 
        public static string GetLocalIpAddress() 
        { 
            try 
            { 
                return s_localhostAddressList.First( 
                    address => !address.IsIPv6LinkLocal 
#if DEBUG 
                    && (s_localhostAddressList.Count < 2 || address.ToString().StartsWith("192.168.91")) 
#endif 
                    ).ToString(); 
            } 
            catch (Exception ex) 
            { 
                throw new Exception("Не удалось определить локальный IP-адрес", ex); 
            } 
        } 
        public static bool IsLocalIPAddress(string testIPAddress) 
        { 
            var testIP = IPAddress.Parse(testIPAddress); 
            return s_localhostAddressList.Contains(testIP); 
        } 
    } 
}
