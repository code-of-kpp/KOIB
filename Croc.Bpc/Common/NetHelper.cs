using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Net; 

 

 

namespace Croc.Bpc.Common 

{ 

    /// <summary> 

    /// Класс для работы с сетью 

    /// </summary> 

    public static class NetHelper 

    { 

        /// <summary> 

        /// Список локальных IP-адресов 

        /// </summary> 

        private static List<IPAddress> _localhostAddressList; 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        static NetHelper() 

        { 

            _localhostAddressList = new List<IPAddress>(Dns.GetHostEntry(Dns.GetHostName()).AddressList); 

        } 

 

 

        /// <summary> 

        /// Возвращает локальный IP-адрес 

        /// </summary> 

        /// <returns></returns> 

        public static string GetLocalIPAddress() 

        { 

            try 

            { 

                return _localhostAddressList.First( 

                    address => !address.IsIPv6LinkLocal 

#if DEBUG 

                    // если Отладка, то дополнительно фильтруем по IP, но только если кол-во адресов в списке >= 2 

                    && (_localhostAddressList.Count < 2 || address.ToString().StartsWith("192.168.")) 

#endif 

                    ).ToString(); 

            } 

            catch (Exception ex) 

            { 

                throw new Exception("Не удалось определить локальный IP-адрес", ex); 

            } 

        } 

 


 
        /// <summary> 

        /// Проверяет, что заданный IP-адрес - это локальный адрес 

        /// </summary> 

        /// <param name="ipAddress"></param> 

        /// <returns>true - локальный, false - не локальный</returns> 

        public static bool IsLocalIPAddress(string testIPAddress) 

        { 

            var testIP = IPAddress.Parse(testIPAddress); 

            return _localhostAddressList.Contains(testIP); 

        } 

    } 

}


