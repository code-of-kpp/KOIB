using System; 

using System.Collections.Generic; 

 

 

namespace Croc.Bpc.Common.Interfaces 

{ 

    /// <summary> 

    /// Интерфейс для получения информации обо всех сканерах 

    /// </summary> 

    public interface IScannersInfo 

    { 

        /// <summary> 

        /// Серийный номер локального сканера 

        /// </summary> 

        string LocalScannerSerialNumber { get; } 

 

 

        /// <summary> 

        /// Получить информацию о сканерах 

        /// </summary> 

        /// <returns></returns> 

        List<ScannerInfo> GetScannerInfos(); 

    } 

}


