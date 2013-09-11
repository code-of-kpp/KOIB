using System; 

 

 

namespace Croc.Bpc.Scanner 

{ 

    /// <summary> 

    ///	Статусы состояния драйвера сканера 

    /// </summary> 

    public enum ScannerStatus : int 

    { 

        /// <summary> 

        ///	все в порядке 

        /// </summary> 

        OK = 0x0, 

 

 

        /// <summary> 

        ///	неверная версия драйвера 

        /// </summary> 

        BAD_VER = 0x1, 

 

 

        /// <summary> 

        ///	отсутствует или неверный файл конфигурации 

        /// </summary> 

        BAD_CONF = 0x2, 

 

 

        /// <summary> 

        ///	отсутствует или неверный файл коэффициентов яркости 

        /// </summary> 

        BAD_LIGHT = 0x4, 

 

 

        /// <summary> 

        ///	ошибка открытия канала звука 

        /// </summary> 

        BAD_TUNE = 0x8, 

 

 

        /// <summary> 

        /// не работает левый ДДЛ 

        /// </summary> 

        BAD_LEFT_DOUBLE_LIST = 0x10, 

 

 

        /// <summary> 

        /// не работает правый ДДЛ 

        /// </summary> 

        BAD_RIGHT_DOUBLE_LIST = 0x20, 


    } 

}


