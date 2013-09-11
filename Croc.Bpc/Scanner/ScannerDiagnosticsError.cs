using System; 

 

 

namespace Croc.Bpc.Scanner 

{ 

    /// <summary> 

    /// Ошибки, обнаруженные при диагностики сканера 

    /// </summary> 

    public enum ScannerDiagnosticsError 

    { 

        /// <summary> 

        /// Не работает правый датчик двойного листа 

        /// </summary> 

        RightDoubleSheetSensorNotWork, 

        /// <summary> 

        /// Не работает левый датчик двойного листа 

        /// </summary> 

        LeftDoubleSheetSensorNotWork, 

        /// <summary> 

        /// Не работает датчик двойного листа 

        /// </summary> 

        DoubleSheetSensorNotWork, 

        /// <summary> 

        /// Некорректная версия драйвера сканера 

        /// </summary> 

        WrongDriverVersion, 

        /// <summary> 

        /// Некорректный файл конфигурации драйвера 

        /// </summary> 

        WrongDriverConfig, 

        /// <summary> 

        /// Некорректный файл коэффициентов яркости 

        /// </summary> 

        WrongBrightnessCoefFile, 

    } 

}


