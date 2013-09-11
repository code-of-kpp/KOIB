using System; 

 

 

namespace Croc.Bpc.Scanner 

{ 

    /// <summary> 

    /// Режим работы ламп сканера (зеленой и красной) 

    /// </summary> 

    public enum ScannerLampsRegime 

    { 

        /// <summary> 

        /// Обе лампы выключены 

        /// </summary> 

        BothOff, 

        /// <summary> 

        /// Зеленая лампа горит 

        /// </summary> 

        GreenOn, 

        /// <summary> 

        /// Зеленая лампа мигает 

        /// </summary> 

        GreenBlinking, 

        /// <summary> 

        /// Предупреждение об ошибке: 

        /// обе лампы поочередно мигают в течение некоторого времени, потом принимают исходное положение 

        /// </summary> 

        Alerting, 

        /// <summary> 

        /// Сканирование: зеленая выключена, красную - не трогаем, т.к. драйвер сканера сам ее включает 

        /// </summary> 

        Scanning 

    } 

}


