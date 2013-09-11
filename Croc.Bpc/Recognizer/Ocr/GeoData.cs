using System; 

using System.Runtime.InteropServices; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Геометрия бюллетеня 

    /// </summary> 

    [StructLayout(LayoutKind.Sequential)] 

    public struct GeoData 

    { 

        /// <summary> 

        /// Маскимальное колво квадратов в структуре GeoData 

        /// </summary> 

        public const int MAX_SQUARES = 127; 

 

 

        /// <summary> 

        /// Результат проверки 

        ///  0 - OK 

        /// -1 - не найден или не определен верхний маркер 

        /// -2 - не найден или не определен нижний маркер 

        /// -3 - несовпадение маркеров 

        /// -4 - недопустимый код маркера 

        /// -5 - не найдена левая граница бюллетеня 

        /// -6 - не найдена базовая линия 

        /// -7 - неверное положение базовой линии 

        /// -8 - не найдены квадраты 

        /// </summary> 

        public int result; 

        /// <summary> 

        /// Верхний маркер (-1 не определен) 

        /// </summary> 

        public int topMarker; 

        /// <summary> 

        /// Верхний маркер - характеристика цвета 

        /// </summary> 

        public int topMarkerColor; 

        /// <summary> 

        /// Верхний маркер (-1 не определен) 

        /// </summary> 

        public int bottomMarker; 

        /// <summary> 

        /// Верхний маркер - характеристика цвета 

        /// </summary> 

        public int bottomMarkerColor; 

        /// <summary> 

        /// Качество печати маркеров (1 - достаточное, 0 - недостаточное) 

        /// </summary> 


        public int markerQuality; 

        /// <summary> 

        /// Отклонение по вертикали базовой линии 

        /// </summary> 

        public int baseLineSkew; 

        /// <summary> 

        /// Равномерность черного цвета базовой линии 

        /// </summary> 

        public int baseLineColor; 

        /// <summary> 

        /// Качество печати базовых линий (1 - достаточное, 0 - не достаточное) 

        /// </summary> 

        public int baseLineQuality; 

        /// <summary> 

        /// квадраты - признаки найден (1) / не найден (0) 

        /// </summary> 

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_SQUARES + 1, ArraySubType = UnmanagedType.I4)] 

        public int[] squares; 

        /// <summary> 

        /// отклонения по вертикали (в мм) 

        /// </summary> 

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_SQUARES + 1, ArraySubType = UnmanagedType.I4)] 

        public int[] squaresSkewV; 

        /// <summary> 

        /// по горизонтали (в мм) 

        /// </summary> 

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_SQUARES + 1, ArraySubType = UnmanagedType.I4)] 

        public int[] squaresSkewH; 

        /// <summary> 

        /// отклонения по размеру (в мм)  

        /// </summary> 

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_SQUARES + 1, ArraySubType = UnmanagedType.I4)] 

        public int[] squaresSize; 

        /// <summary> 

        /// равновмерность цвета  

        /// </summary> 

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_SQUARES + 1, ArraySubType = UnmanagedType.I4)] 

        public int[] squaresColor; 

        /// <summary> 

        /// качество печати   

        /// </summary> 

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_SQUARES + 1, ArraySubType = UnmanagedType.I4)] 

        public int[] squaresQuality; 

        /// <summary> 

        /// общая характеристика равномерности цвета  

        /// </summary> 

        public int ColorQuality; 

    }; 

}


