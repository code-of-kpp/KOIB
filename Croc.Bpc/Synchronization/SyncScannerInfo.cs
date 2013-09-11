using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Bpc.Synchronization 

{ 

    /// <summary> 

    /// Информация о сканере, который участвует в процессе синхронизации с другим сканером 

    /// </summary> 

    internal class SyncScannerInfo 

    { 

        /// <summary> 

        /// Хеш-код 

        /// </summary> 

        private readonly int _hashCode; 

        /// <summary> 

        /// Серийный номер сканера 

        /// </summary> 

        public readonly string SerialNumber; 

        /// <summary> 

        /// IP-адрес сканера 

        /// </summary> 

        public readonly string IPAddress; 

 

 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        public SyncScannerInfo(SyncScannerInfo info) 

            : this(info.SerialNumber, info.IPAddress) 

        { 

        } 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        public SyncScannerInfo(string serialNumber, string ipAddress) 

        { 

            CodeContract.Requires(!string.IsNullOrEmpty(serialNumber)); 

            CodeContract.Requires(!string.IsNullOrEmpty(ipAddress)); 

 

 

            SerialNumber = serialNumber; 

            IPAddress = ipAddress; 

 


 
            _hashCode = string.Format("{0}#{1}", SerialNumber, IPAddress).GetHashCode(); 

        } 

 

 

        public override bool Equals(object obj) 

        { 

            var other = obj as SyncScannerInfo; 

            if (other == null) 

                return false; 

 

 

            return other._hashCode == this._hashCode; 

        } 

 

 

        public override int GetHashCode() 

        { 

            return this._hashCode; 

        } 

    } 

}


