using System; 

using System.Collections.Generic; 

using Croc.Core; 

using Croc.Bpc.Common; 

using Croc.Bpc.Common.Images; 

 

 

namespace Croc.Bpc.Scanner 

{ 

    /// <summary> 

    /// Интерфейс менеджера сканера 

    /// </summary> 

    public interface IScannerManager : ISubsystem 

    { 

        #region Подключение к сканеру и диагностика 

 

 

        /// <summary> 

        /// Подключен ли сканер 

        /// </summary> 

        bool ScannerConnected { get; } 

 

 

        /// <summary> 

        /// Установить соединение со сканером 

        /// </summary> 

        /// <param name="maxTryCount">максимальное кол-во попыток установить подключение</param> 

        /// <param name="delay">задержка между попытками</param> 

        /// <returns> 

        /// true - соединение установлено, false - не удалось установить соединение 

        /// </returns> 

        bool EstablishConnectionToScanner(int maxTryCount, TimeSpan delay); 

 

 

        /// <summary> 

        /// Событие "Удаленный сканер подключился" 

        /// </summary> 

        event EventHandler<ScannerEventArgs> RemoteScannerConnected; 

 

 

        /// <summary> 

        /// Выполнить диагностику сканера 

        /// </summary> 

        /// <returns>список ошибок, обнаруженных в результате диагностики</returns> 

        List<ScannerDiagnosticsError> PerformDiagnostics(); 

 

 

        /// <summary> 

        /// Работает ли датчик двойного листа 

        /// </summary> 


        bool IsDoubleSheetSensorWork { get; } 

 

 

        #endregion 

 

 

        #region Основные атрибуты 

 

 

        /// <summary> 

        /// Серийный номер сканера 

        /// </summary> 

        string SerialNumber { get; } 

 

 

        /// <summary> 

        /// Целочисленный серийный номер сканера 

        /// </summary> 

        int IntSerialNumber { get; } 

 

 

        /// <summary> 

        /// Версия драйвера сканера 

        /// </summary> 

        int DriverVersion { get; } 

 

 

        /// <summary> 

        /// Версия драйвера сканера 

        /// </summary> 

        ScannerVersion Version { get; } 

 

 

        /// <summary> 

        /// IP адрес сканера 

        /// </summary> 

        string IPAddress { get; } 

 

 

        #endregion 

 

 

        #region Параметры сканера 

 

 

        /// <summary> 

        /// Разрешение сканера по оси X на верхней стороне 

        /// </summary> 

        short DpiXTop { get; } 

 


 
        /// <summary> 

        /// Разрешение сканера по оси Y на верхней стороне 

        /// </summary> 

        short DpiYTop { get; } 

 

 

        /// <summary> 

        /// Разрешение сканера по оси X на нижней стороне 

        /// </summary> 

        short DpiXBottom { get; } 

 

 

        /// <summary> 

        /// Разрешение сканера по оси Y на нижней стороне 

        /// </summary> 

        short DpiYBottom { get; } 

 

 

        /// <summary> 

        /// Порог бинаризации верхней стороны (с учетом коррекции) 

        /// </summary> 

        short BinarizationThresholdTop { get; } 

 

 

        /// <summary> 

        /// Порог бинаризации нижней стороны (с учетом коррекции) 

        /// </summary> 

        short BinarizationThresholdBottom { get; } 

 

 

		/// <summary> 

		/// Включен ли датчик двойного листа 

		/// </summary> 

		bool DoubleSheetSensorEnabled {get; set;} 

 

 

		/// <summary> 

		/// Перезагрузка параметров сканера 

		/// </summary> 

		void ReLoadParameters(); 

 

 

        #endregion 

 

 

        #region Сессия обработки листа 

 

 

        /// <summary> 


        /// Событие "Поступил новый лист" 

        /// </summary> 

        event EventHandler<SheetEventArgs> NewSheetReceived; 

 

 

        /// <summary> 

        /// Событие "Лист обработан" 

        /// </summary> 

        /// <remarks>событие созникает как в случае удачной обработке листа (лист сброшшен в урну), 

        /// так и в случае неудачной (лист реверсирован)</remarks> 

        event EventHandler<SheetEventArgs> SheetProcessed; 

 

 

        /// <summary> 

        /// Текущая сессия обработки листа 

        /// </summary> 

        SheetProcessingSession SheetProcessingSession { get; } 

 

 

        #endregion 

 

 

        #region Управление процессом сканирования 

 

 

        /// <summary> 

        /// Занят ли в данный момент сканер 

        /// </summary> 

        /// <remarks>сканер занят, когда в нем находится лист (не путать со сканированием листа, т.к. лист после того,  

        /// как сканирование завершено, еще некоторое время "висит" в сканере)</remarks> 

        bool IsBusy { get; } 

 

 

        /// <summary> 

        /// Занят ли в данный момент сканер сканированием листа 

        /// </summary> 

        bool IsSheetScanning { get; } 

 

 

        /// <summary> 

        /// Запустить сканирование 

        /// </summary> 

        /// <returns>запущено ли сканирование</returns> 

        bool StartScanning(); 

 

 

        /// <summary> 

        /// Остановить сканирование 

        /// </summary> 

        /// <returns>остановлено ли сканирование</returns> 


        bool StopScanning(); 

 

 

        /// <summary> 

        /// Восстановить сканирование на сканере после того, как случилась ошибка 

        /// </summary> 

        /// <remarks>если сканирование не было начато, то ничего не делает,  

        /// иначе последовательно вызывается StopScanning -> StartScanning</remarks> 

        /// <returns>true - сканирование перезапущено, false - перезапускать сканирование не нужно</returns> 

        bool RestoreScanningAfterError(); 

 

 

        /// <summary> 

        /// Сбросить лист 

        /// </summary> 

        /// <param name="markingCode">код метода маркировки листа  

        /// (по сути - это кол-во проколов, которые нужно сделать в листе)</param> 

        /// <returns>результат выполнения сброса листа</returns> 

        DropResult DropSheet(short markingCode); 

 

 

        #endregion 

 

 

        #region Реверс листа 

 

 

        /// <summary> 

        /// Реверсировать лист 

        /// </summary> 

        /// <param name="reasonCode">Код причины реверса</param> 

        void ReverseSheet(int reasonCode); 

 

 

        /// <summary> 

        /// Убедиться, что реверс листа выполнен 

        /// </summary> 

        /// <remarks>Метод проверяет, была ли ранее команда на реверс листа, и если команда была, 

        /// но реверс так и не был выполнен, то команда реверса посылается повторно</remarks> 

        /// <returns>true - реверс выполняется или уже выполнен, false - реверсировать лист не нужно</returns> 

        bool EnsureSheetReversed(); 

 

 

        #endregion 

 

 

        #region Cканирование и его результаты 

 

 

        /// <summary> 


        ///	Кол-во строк отсканированных в последний раз 

        /// </summary> 

        int ScannedLinesCountLast { get; } 

 

 

        /// <summary> 

        /// Рабочий буфер верхней стороны 

        /// </summary> 

        MemoryBlock WorkBufferTop { get; } 

 

 

        /// <summary> 

        /// Рабочий буфер нижней стороны 

        /// </summary> 

        MemoryBlock WorkBufferBottom { get; } 

 

 

        /// <summary> 

        /// Сканировать полутон в указанный буфер 

		/// </summary> 

        /// <remarks>xCoord и width должны быть кратны 2 и в сумме не превышать 2688</remarks> 

        /// <param name="side">строна</param> 

		/// <param name="xCoord">координата X</param> 

		/// <param name="yCoord">координата Y</param> 

		/// <param name="width">ширина</param> 

		/// <param name="height">высота</param> 

		/// <param name="image">память, в которую нужно записать запрашиваемый полутон</param> 

		/// <returns>true - дождались события готовности буфера; false - не дождались</returns> 

        bool GetHalftoneBuffer(ScannedSide side, short xCoord, short yCoord, short width, short height, MemoryBlock image); 

 

 

        /// <summary> 

        /// Сохраняет изображение буфера в файл 

        /// </summary> 

        /// <param name="filePath">путь к файлу, в который нужно сохранить изображение</param> 

        /// <param name="imageType">тип сохраняемого изображения</param> 

        /// <param name="side">сторона, которую нужно сохранить. Если Undefined - значит обе</param> 

        /// <param name="bufferSize">размер сохраняемого буфера</param> 

        /// <returns>true - Сохранение прошло успешно; false - Ошибка сохранения</returns> 

        void SaveBuffer(string filePath, ImageType imageType, ScannedSide side, BufferSize bufferSize); 

 

 

        /// <summary> 

        /// Размер буфера изображения 

        /// </summary> 

        /// <param name="imageType">тип сохраняемого изображения</param> 

        /// <param name="bufferSize">размер сохраняемого буфера</param> 

        /// <returns>Размер буфера изображения</returns> 

        long GetBufferSize(ImageType imageType, BufferSize bufferSize); 

 


 
        #endregion 

 

 

        #region Управление индикатором 

 

 

        /// <summary> 

        /// Длина индикатора сканера 

        /// </summary> 

        /// <returns></returns> 

        int IndicatorLength { get; } 

 

 

        /// <summary> 

        /// Установить текст на индикаторе 

        /// </summary> 

        /// <param name="text"></param> 

        void SetIndicator(string text); 

 

 

        #endregion 

 

 

        #region Управление лампами 

 

 

        /// <summary> 

        /// Установить режим работы ламп 

        /// </summary> 

        /// <param name="regime"></param> 

        void SetLampsRegime(ScannerLampsRegime regime); 

 

 

        #endregion 

    } 

}


