using System; 

using System.Collections.Generic; 

using Croc.Bpc.Common; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Интерфейс драйвера распознавалки 

    /// </summary> 

    public interface IOcr : IDisposable 

    { 

        /// <summary> 

        /// Инициализация компоненты. Необходимо вызывать сразу после создания 

        /// </summary> 

        void Init(); 

        /// <summary> 

        /// установить обработчик событий 

        /// </summary> 

        /// <param name="eventHandler">Обработчик событий</param> 

        void SetEventsHandler(IOcrEventHandler eventHandler); 

        /// <summary> 

        /// Уровень проверки печати 

        /// </summary> 

        StampTestLevel StampTestLevel { get; set; } 

        /// <summary> 

        /// Уровень проверки небюллетеня 

        /// </summary> 

        InlineLevel InlineRecognitionLevel { get; set; } 

        /// <summary> 

        /// Максимально допустимое значение перекоса 

        /// </summary> 

        int MaxOnlineSkew { get; set; } 

        /// <summary> 

        /// Разрешение сканера по X 

        /// </summary> 

        float DpiX0 { get; set; } 

        /// <summary> 

        /// Разрешение сканера по X 

        /// </summary> 

        float DpiX1 { get; set; } 

        /// <summary> 

        /// Разрешение сканера по Y 

        /// </summary> 

        float DpiY0 { get; set; } 

        /// <summary> 

        /// Разрешение сканера по Y 

        /// </summary> 

        float DpiY1 { get; set; } 

        /// <summary> 


        /// Установить разрешение сканера 

        /// </summary> 

        /// <param name="x0">значение по оси Х для стороны 0</param> 

        /// <param name="y0">значение по оси Y для стороны 0</param> 

        /// <param name="x1">значение по оси Х для стороны 1</param> 

        /// <param name="y1">значение по оси Y для стороны 1</param> 

        void SetDpi(float x0, float y0, float x1, float y1); 

        /// <summary> 

        /// Номер бюллетеня (от 0), полученный по результату распознавания маркера 

        /// </summary> 

        int BulletinNumber { get; } 

        /// <summary> 

        /// Результат распознавания печати 

        /// </summary> 

        StampResult StampResult { get; } 

        /// <summary> 

        /// Результаты распознавания 

        /// </summary> 

        List<PollResult> Results { get; } 

        /// <summary> 

        /// [DEPRECATED] Cнимает квадрат с гoлoсoвания 

        /// </summary> 

        /// <param name="bulletin">Номер бюллетеня</param> 

        /// <param name="election">Номер выборов</param> 

        /// <param name="square">Номер квадрата</param> 

        void OCR_ExcludeSquare(int bulletin, int election, int square); 

        /// <summary> 

        /// [DEPRECATED] Восстанавливает учатие квадрата в выборах 

        /// </summary> 

        /// <param name="bulletin">Номер бюллетеня</param> 

        /// <param name="election">Номер выборов</param> 

        /// <param name="square">Номер квадрата</param> 

        void OCR_RestoreSquare(int bulletin, int election, int square); 

        /// <summary> 

        /// [DEPRECATED] возвращает состояние флага участия квадрата в выборах 

        /// </summary> 

        /// <param name="bulletin">Номер бюллетеня</param> 

        /// <param name="election">Номер выборов</param> 

        /// <param name="square">Номер квадрата</param> 

        /// <param name="retVal">Признак участия</param> 

        /// <returns>Результат выполнения</returns> 

        int OCR_IsSquareValid(int bulletin, int election, int square, out int retVal); 

        /// <summary> 

        /// Инициализация процесса распознавания бюллетеня 

        /// </summary> 

        /// <param name="pdImage0">Сторона 1</param> 

        /// <param name="pdImage1">Сторона 2</param> 

        /// <param name="nLineWidth0">Ширина 1ой стороны</param> 

        /// <param name="nLineWidth1">Ширина 2ой стороны</param> 

        /// <param name="nNumOfLines0">Колво линий 1ой стороны</param> 


        /// <param name="nNumOfLines1">Колво линий 2ой стороны</param> 

        void RunRecognize(MemoryBlock pdImage0, MemoryBlock pdImage1, int nLineWidth0, int nLineWidth1, int nNumOfLines0, int nNumOfLines1); 

        /// <summary> 

        /// Завершение процесса распознавания 

        /// </summary> 

        /// <param name="markerType">Тип маркера</param> 

        /// <returns>Результат распознавания</returns> 

        int EndRecognize(MarkerType markerType); 

        /// <summary> 

        /// Завершение процесса распознавания: проверка геометрии бюллетеня 

        /// </summary> 

        /// <returns>Результат распознавания</returns> 

        int TestBallot(ref GeoData geoData); 

        /// <summary> 

        /// Скомпилировать модель 

        /// </summary> 

        void LinkModel(); 

        /// <summary> 

        /// Получить порцию изображения 

        /// </summary> 

        /// <param name="count">Колво строк</param> 

        /// <returns> 

        /// -1  - это не бюллетень  

        ///  0  - решение пока не принято  

        ///  1  - признан бюллетенем 

        /// </returns> 

        int NextBuffer(int count); 

        /// <summary> 

        /// Определить в online номер маркера 

        /// </summary> 

        /// <param name="markerType">Тип маркера</param> 

        /// <returns>Номер маркера</returns> 

        int GetOnlineMarker(MarkerType markerType); 

        /// <summary> 

        /// Инициализация модуля разпознавания. Вызывается при вхoде в режим выбoрoв 

        /// </summary> 

        void InitRecognize(); 

        /// <summary> 

        /// Добавляет номер комиссии в массив 

        /// </summary> 

        /// <param name="nStamp">Номер комиссии</param> 

        void AddStamp(int nStamp); 

        /// <summary> 

        /// Очистка массива номеров комиссий 

        /// </summary> 

        void ClearStamps(); 

        /// <summary> 

        /// Путь к файлу с моделью 

        /// </summary> 

        string ModelFilePath { get; set; } 


        /// <summary> 

        /// Полный путь к данным и модулю распознавания цифр. Должен заканчиваться на слэш 

        /// </summary> 

        string Path2RecognitionData { get; set; } 

        /// <summary> 

        /// Минимальная ширина маркера в пикселях 

        /// </summary> 

        int MinMarkerWid { get; set; } 

        /// <summary> 

        /// Максимальная ширина маркера в пикселях 

        /// </summary> 

        int MaxMarkerWid { get; set; } 

        /// <summary> 

        /// Минимальная высота маркера в пикселях 

        /// </summary> 

        int MinMarkerHgh { get; set; } 

        /// <summary> 

        /// Максимальная высота маркера в пикселях 

        /// </summary> 

        int MaxMarkerHgh { get; set; } 

        /// <summary> 

        /// Минимальное отношение высота/ширина для маркера 

        /// </summary> 

        double MinMarkerRio { get; set; } 

        /// <summary> 

        /// Максимальное отношение высота/ширина для маркера 

        /// </summary> 

        double MaxMarkerRio { get; set; } 

        /// <summary> 

        /// Начало зоны контроля пустого листа (не бюллетеня) в пикселях 

        /// </summary> 

        int BlankTestStart { get; set; } 

        /// <summary> 

        /// Начало зоны контроля пустого листа (не бюллетеня) в пикселях 

        /// </summary> 

        int BlankTestStop { get; set; } 

        /// <summary> 

        /// Конец зоны контроля пустого листа (не бюллетеня) в пикселях 

        /// </summary> 

        int MinCheckArea { get; set; } 

        /// <summary> 

        /// Минимальная площадь засчитываемой печати в пикселях 

        /// </summary> 

        int StampLowThr { get; set; } 

 

 

        // Экспортируемые функции для изменения параметров печати. 

        // Могут быть вызваны после InitRecognition в любой момент перед 

        // распознаванием печати. 

 


 
        /// <summary> 

        /// Средний горизонтальный размер цифры номера печати в пикселях 

        /// </summary> 

        int StampDigitXsize { get; set; } 

        /// <summary> 

        /// Средний вертикальный размер цифры номера печати в пикселях 

        /// </summary> 

        int StampDigitYsize { get; set; } 

        /// <summary> 

        /// Минимальная ширина линии цифры номера печати в пикселях 

        /// </summary> 

        int StampDigitMinLineWidth { get; set; } 

        /// <summary> 

        /// Максмальная ширина линии цифры номера печати в пикселях 

        /// </summary> 

        int StampDigitMaxLineWidth { get; set; } 

        /// <summary> 

        /// Средний размер белого промежутка между цифрами номера печати в пикселях 

        /// </summary> 

        int StampDigitGap { get; set; } 

        /// <summary> 

        /// Среднее расстояние в пикселях от центра цифр печати до нижней линии рамки печати 

        /// </summary> 

        int StampDigitDistBotLine { get; set; } 

        /// <summary> 

        /// Среднее расстояние в пикселях от центра левой цифры печати до средины левой линии рамки 

        /// </summary> 

        int StampDigitDistLftLine { get; set; } 

        /// <summary> 

        /// Среднее расстояние в пикселях от центра правой цифры печати до средины правой линии рамки 

        /// </summary> 

        int StampDigitDistRghLine { get; set; } 

        /// <summary> 

        /// Среднее расстояние в пикселях от центра левой линии рамки печати до центра правой 

        /// </summary> 

        int StampFrameWidth { get; set; } 

        /// <summary> 

        /// Включение/отключение механизма отсева одной слабой лишней метки 

        /// </summary> 

        bool CutWeakCheck { get; set; } 

        /// <summary> 

        /// проверка наличия нижней линии каждой правой секции бюллетня 

        /// </summary> 

        bool SeekBottomRightLine { get; set; } 

        /// <summary> 

        /// вертикальный размер анализируемой зоны печати 

        /// </summary> 

        int StampVSize { get; set; } 

        /// <summary> 


        /// Искать ли потерянный квадрат 

        /// </summary> 

        bool LookForLostSquare { get; set; } 

        /// <summary> 

        /// Количество запусков распознавания 

        /// </summary> 

        int RunRecCount { get; } 

        /// <summary> 

        /// Включает логгирование распознавания в указанный файл 

        /// </summary> 

        /// <param name="sLogFileName">Имя файла</param> 

        void EnableLogging(string sLogFileName); 

        /// <summary> 

        /// Минимальная ширина маркера в пикселях 

        /// </summary> 

        int MinStandartMarkerWid { get; set; } 

        /// <summary> 

        /// Максимальная ширина маркера в пикселях 

        /// </summary> 

        int MaxStandartMarkerWid { get; set; } 

        /// <summary> 

        /// Минимальная высота маркера в пикселях 

        /// </summary> 

        int MinStandartMarkerHgh { get; set; } 

        /// <summary> 

        /// Максимальная высота маркера в пикселях 

        /// </summary> 

        int MaxStandartMarkerHgh { get; set; } 

        /// <summary> 

        /// Зона поиска маркера в пикселях 

        /// </summary> 

        int StandartMarkerZone { get; set; } 

        /// <summary> 

        /// Смещение в буфере для первой линейки 

        /// </summary> 

        int OffsetFirstRule { get; set; } 

    } 

}


