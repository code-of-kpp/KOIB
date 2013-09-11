using System; 

 

 

namespace Croc.Bpc.Scanner 

{ 

    /// <summary> 

    /// Аргументы событий о сканере 

    /// </summary> 

    public class ScannerEventArgs : EventArgs 

    { 

        /// <summary> 

        /// Серийный номер сканера 

        /// </summary> 

        public string SerialNumber 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// IP-адрес сканера 

        /// </summary> 

        public string IPAddress 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        internal ScannerEventArgs(string serialNumber, string ipAddress) 

        { 

            SerialNumber = serialNumber; 

            IPAddress = ipAddress; 

        } 

    } 

}


