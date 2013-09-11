using System; 

 

 

namespace Croc.Bpc.Scanner 

{ 

    /// <summary> 

    /// Уведомление, рассылаемое по широковещательному адресу, о присутствие сканера в сети 

    /// </summary> 

    internal class ScannerBroadcastNotice 

    { 

        /// <summary> 

        /// Размер данных 

        /// </summary> 

        public const int DATA_LENGTH = 8; 

 

 

        /// <summary> 

        /// Серийный номер сканера 

        /// </summary> 

        public readonly string SerialNumber; 

        /// <summary> 

        /// Статус сканера 

        /// </summary> 

        public readonly int Status; 

        /// <summary> 

        /// Бинарное представление данных 

        /// </summary> 

        public readonly byte[] Data; 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="serialNumber">Серийный номер сканера</param> 

        /// <param name="status">Статус сканера</param> 

        public ScannerBroadcastNotice(string serialNumber, int status) 

        { 

            SerialNumber = serialNumber; 

            Status = status; 

 

 

            int sn = int.Parse(SerialNumber); 

 

 

            Data = new byte[DATA_LENGTH]; 

            Data[0] = (byte)(sn & 0xFF); 

            Data[1] = (byte)((sn >> 8) & 0xFF); 

            Data[2] = (byte)((sn >> 16) & 0xFF); 

            Data[3] = (byte)((sn >> 24) & 0xFF); 

            Data[4] = (byte)(Status & 0xFF); 


            Data[5] = (byte)((Status >> 8) & 0xFF); 

            Data[6] = (byte)((Status >> 16) & 0xFF); 

            Data[7] = (byte)((Status >> 24) & 0xFF); 

        } 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="data">Бинарное представление данных</param> 

        public ScannerBroadcastNotice(byte[] data) 

        { 

            Data = data; 

            SerialNumber = (data[0] | data[1] << 8 | data[2] << 16 | data[3] << 24).ToString(); 

            Status = (data[4] | data[5] << 8 | data[6] << 16 | data[7] << 24); 

        } 

    } 

}


