using System; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Общий результат проверки геометрии 

    /// </summary> 

    public enum GeoResult 

    { 

        /// <summary> 

        /// 0 - OK 

        /// </summary> 

        OK = 0, 

        /// <summary> 

        /// -1 - не найден или не определен верхний маркер  

        /// </summary> 

        TopMarker = -1, 

        /// <summary> 

        /// -2 - не найден или не определен нижний маркер  

        /// </summary> 

        BottomMarker = -2, 

        /// <summary> 

        /// -3 - несовпадение маркеров  

        /// </summary> 

        Markers = -3, 

        /// <summary> 

        /// -4 - недопустимый код маркера  

        /// </summary> 

        BadMarkerNum = -4, 

        /// <summary> 

        /// -5 - не найдена левая граница бюллетеня  

        /// </summary> 

        LeftSide = -5, 

        /// <summary> 

        /// -6 - не найдена базовая линия  

        /// </summary> 

        BaseLine = -6, 

        /// <summary> 

        /// -7 - неверное положение базовой линии  

        /// </summary> 

        BadBaseLine = -7, 

        /// <summary> 

        /// -8 - не найдены квадраты  

        /// </summary> 

        Squares = -8, 

    }; 

}


