namespace Croc.Bpc.Common.Diagnostics 

{ 

    /// <summary> 

    /// События приложения 

    /// </summary> 

    public enum Message 

    { 

        #region Общесистемные сообщения 

        /// <summary> 

        /// Необработанное исключение: {0} 

        /// </summary> 

        [MessageParameters("Необработанное исключение: {0}")] 

        UnhandledException = 1, 

 

 

        /// <summary> 

        /// Произошла внутренняя ошибка! Дальнейшая работа программы невозможна. Обратитесь в службу технической поддержки. 

        /// </summary> 

        [MessageParameters("Произошла внутренняя ошибка! Дальнейшая работа программы невозможна. Обратитесь в службу технической поддержки.")] 

        CriticalException, 

 

 

        /// <summary> 

        /// [Исключение] 

        /// </summary> 

        [MessageParameters("{0}")] 

        Exception, 

 

 

        /// <summary> 

        /// [Информационное сообщение] 

        /// </summary> 

        [MessageParameters("{0}")] 

        Information, 

 

 

        /// <summary> 

        /// [Отладочное сообщение] 

        /// </summary> 

        [MessageParameters("{0}")] 

        Debug, 

 

 

        /// <summary> 

        /// [Отладочное сообщение дополнительной диагностики] 

        /// </summary> 

        [MessageParameters("{0}")] 

        DebugVerbose, 

 

 


        /// <summary> 

        /// LOCK try enter: {0} 

        /// </summary> 

        [MessageParameters("LOCK try enter: {0}")] 

        LockTryEnter, 

 

 

        /// <summary> 

        /// LOCK done: {0} 

        /// </summary> 

        [MessageParameters("LOCK done: {0}")] 

        LockDone, 

 

 

        /// <summary> 

        /// Версия приложения: {0} 

        /// </summary> 

        [MessageParameters("Версия приложения: {0}")] 

        ApplicationVersion, 

 

 

        /// <summary> 

        /// Имя компьютера: {0} 

        /// </summary> 

        [MessageParameters("Имя компьютера: {0}")] 

        MachineName, 

 

 

        /// <summary> 

        /// IP-адрес: {0} 

        /// </summary> 

        [MessageParameters("IP-адрес: {0}")] 

        IPAddress, 

 

 

		/// <summary> 

        /// Получен Unix сигнал: {0} 

		/// </summary> 

		[MessageParameters("Получен Unix сигнал: {0}")] 

		UnixSignalReceived, 

 

 

        /// <summary> 

        /// [{0},STA] {1} {2} 

        /// </summary> 

        [MessageParameters("[{0},STA] {1} {2}")] 

        ProcessStartInfo, 

 

 

        /// <summary> 


        /// [{0},OUT] {1} 

        /// </summary> 

        [MessageParameters("[{0},OUT] {1}")] 

        ProcessStdOutDump, 

 

 

        /// <summary> 

        /// [{0},ERR] {1} 

        /// </summary> 

        [MessageParameters("[{0},ERR] {1}")] 

        ProcessStdErrDump, 

 

 

        /// <summary> 

        /// [{0},EXI] {1} 

        /// </summary> 

        [MessageParameters("[{0},EXI] {1}")] 

        ProcessExitCode, 

 

 

        /// <summary> 

        /// Результат выполнения команды '{0} {1}' = {2} 

        /// </summary> 

        [MessageParameters("Результат выполнения команды '{0} {1}' = {2}")] 

        ExecCommandResult, 

 

 

        #endregion 

 

 

        /// <summary> 

        /// Конец общесистемных сообщений 

        /// </summary> 

        __End_Common = 1000, 

 

 

        #region Сообщения диагностики 

        /// <summary> 

        /// Ошибка в задании формата строки: {0} 

        /// </summary> 

        [MessageParameters("Ошибка в задании формата строки: {0}")] 

        SingleLineFormatterFormatError, 

        /// <summary> 

        /// Запись в протокол {0} прекращена из-за недостатка свободного места (доступно {1:N} MiB, требуется {2:N} MiB) 

        /// </summary> 

        [MessageParameters("Запись в протокол {0} прекращена из-за недостатка свободного места (доступно {1:N} MiB, требуется {2:N} MiB)")] 

        AvailableSpaceFilterNotEnoughSpaceError, 

        #endregion 

 

 


        /// <summary> 

        /// Конец сообщений диагностики 

        /// </summary> 

        __End_Diagnostics = 2 * __End_Common, 

 

 

        #region Сообщения менеджера сканеров 

        /// <summary> 

        /// [Исключение] 

        /// </summary> 

        [MessageParameters("{0}")] 

        ScannerManagerException, 

 

 

        /// <summary> 

        /// Получен буфер {0} 

        /// </summary> 

        [MessageParameters("Получен буфер {0}")] 

        ScannerManagerBufferIsReady, 

 

 

        /// <summary> 

        /// Отладочное сообщение: {0} ({1} байт) 

        /// </summary> 

        [MessageParameters("Отладочное сообщение: {0} ({1} байт)")] 

        ScannerManagerDebugMessage, 

 

 

        /// <summary> 

        /// Ошибка от сканера {0}: {1} 

        /// </summary> 

        [MessageParameters("Ошибка от сканера {0}: {1}")] 

        ScannerManagerError, 

 

 

        /// <summary> 

        /// [CALL] NextBuffer({0}, {1}) 

        /// </summary> 

        [MessageParameters("[CALL] NextBuffer({0}, {1})")] 

        ScannerManagerNextBufferCall, 

 

 

        /// <summary> 

        /// [RET] NextBuffer({0}, {1}) 

        /// </summary> 

        [MessageParameters("[RET] NextBuffer({0}, {1})")] 

        ScannerManagerNextBufferReturn, 

 

 

        /// <summary> 


        /// [CALL] SheetIsReady({0}, {1}) 

        /// </summary> 

        [MessageParameters("[CALL] SheetIsReady({0}, {1})")] 

        ScannerManagerSheetIsReadyCall, 

 

 

        /// <summary> 

        /// [RET] SheetIsReady({0}, {1}) 

        /// </summary> 

        [MessageParameters("[RET] SheetIsReady({0}, {1})")] 

        ScannerManagerSheetIsReadyReturn, 

 

 

        /// <summary> 

        /// Для бланка {0} установлена плотность {1} 

        /// </summary> 

        [MessageParameters("Для бланка {0} установлена плотность {1}")] 

        ScannerManagerDensitySet, 

 

 

        /// <summary> 

        /// Считано строк больше, чем размер буфера ({0}) 

        /// </summary> 

        [MessageParameters("Считано строк больше, чем размер буфера ({0})")] 

        ScannerManagerSheetIsReadyTooLarge, 

 

 

        /// <summary> 

        /// Количество считаных строк на сторонах листа различается: {0} != {1} 

        /// </summary> 

        [MessageParameters("Количество считаных строк на сторонах листа различается: {0} != {1}")] 

        ScannerManagerSheetIsReadyDontMatch, 

 

 

        /// <summary> 

        /// Не удалось подключиться к сканеру [попытка №{0}] 

        /// </summary> 

        [MessageParameters("Не удалось подключиться к сканеру [попытка №{0}]")] 

        ScannerManagerCantConnect, 

 

 

        /// <summary> 

        /// Ошибка при {0} мотора: motorNumber = {1}; direction = {2}; step = {3} 

        /// </summary> 

        [MessageParameters("Ошибка при {0} мотора: motorNumber = {1}; direction = {2}; step = {3}")] 

        ScannerManagerMotorException, 

 

 

        /// <summary> 

        /// Лист обработан. Результат = {0}; ошибка = {1} 


        /// </summary> 

        [MessageParameters("Лист обработан. Результат = {0}; ошибка = {1}")] 

        ScannerManagerSheetProcessed, 

 

 

        /// <summary> 

        /// Лист сброшен с кодом {0} 

        /// </summary> 

        [MessageParameters("Лист сброшен с кодом {0}")] 

        ScannerManagerDropSheet, 

 

 

        /// <summary> 

        /// Команда на реверс (код причины = {0}) 

        /// </summary> 

        [MessageParameters("Команда на реверс (код причины = {0})")] 

        ScannerManagerRevers, 

 

 

        /// <summary> 

        /// Сканер отклонил команду реверса 

        /// </summary> 

        [MessageParameters("Сканер отклонил команду реверса")] 

        ScannerManagerReversRejected, 

 

 

        /// <summary> 

        /// Лист реверсирован 

        /// </summary> 

        [MessageParameters("Лист реверсирован")] 

        ScannerManagerReversSuccessfull, 

 

 

        /// <summary> 

        /// Запрошен полутон: Side={0}; X={1}; Y={2}; W={3}; H={4}; ID={5} 

        /// </summary> 

        [MessageParameters("Запрошен полутон: Side={0}; X={1}; Y={2}; W={3}; H={4}; ID={5}")] 

        ScannerManagerGetHalftone, 

 

 

        /// <summary> 

        /// {0} {1} буфер стороны {2} сохранен в файл {3} 

        /// </summary> 

        [MessageParameters("{0} {1} буфер стороны {2} сохранен в файл {3}")] 

        ScannerManagerBufferSaved, 

 

 

        /// <summary> 

        /// {0} {1} буфер стороны {2} не удалось сохранить в файл {3}: {4} 

        /// </summary> 


        [MessageParameters("{0} {1} буфер стороны {2} не удалось сохранить в файл {3}: {4}")] 

        ScannerManagerBufferSaveError, 

 

 

        /// <summary> 

        /// Режим ламп: {0} 

        /// </summary> 

        [MessageParameters("Режим ламп: {0}")] 

        ScannerManagerLampsRegime, 

 

 

        /// <summary> 

        /// Сигнализируем об ошибке: {0} ({1}) 

        /// </summary> 

        [MessageParameters("Сигнализируем об ошибке: {0} ({1})")] 

        ScannerManagerAlertError, 

 

 

        /// <summary> 

        /// Подключение к сканеру установлено 

        /// </summary> 

        [MessageParameters("Подключение к сканеру {0} установлено: {1}")] 

        ScannerManagerConnected, 

 

 

        /// <summary> 

        /// Рабочий поток сканера прерван 

        /// </summary> 

        [MessageParameters("Рабочий поток сканера прерван")] 

        ScannerManagerScannerWorkThreadAborted, 

 

 

        /// <summary> 

        /// Ошибка в рабочем потоке сканера 

        /// </summary> 

        [MessageParameters("Ошибка в рабочем потоке сканера")] 

        ScannerManagerScannerWorkThreadException, 

 

 

        /// <summary> 

        /// Поток отправки сообщений в сканер прерван 

        /// </summary> 

        [MessageParameters("Поток отправки сообщений в сканер прерван")] 

        ScannerManagerScannerSendEventsThreadAborted, 

 

 

        /// <summary> 

        /// Ошибка потоке отправки сообщений в сканер 

        /// </summary> 

        [MessageParameters("Ошибка потоке отправки сообщений в сканер")] 


        ScannerManagerScannerSendEventsThreadException, 

 

 

        /// <summary> 

        /// Найден Gs2Manager {0} ({1}, {2}, {3}, {4}, {5}) 

        /// </summary> 

        [MessageParameters("Найден Gs2Manager {0} ({1}, {2}, {3}, {4}, {5})")] 

        ScannerManagerDetectedHardware, 

 

 

        /// <summary> 

        /// Новый лист 

        /// </summary> 

        [MessageParameters("Новый лист")] 

        ScannerManagerNewSheet, 

 

 

        /// <summary> 

        /// Команда сброса выполнена: {0} 

        /// </summary> 

        [MessageParameters("Команда сброса выполнена: {0}")] 

        ScannerManagerSheetDroped, 

 

 

        /// <summary> 

        /// Лист отсканирован: {0}, {1} 

        /// </summary> 

        [MessageParameters("Лист отсканирован: {0}, {1}")] 

        ScannerManagerSheetIsReady, 

 

 

        /// <summary> 

        /// Готов к приему 

        /// </summary> 

        [MessageParameters("Готов к приему")] 

        ScannerManagerReadyToScanning, 

        #endregion 

 

 

        /// <summary> 

        /// Конец сообщений диагностики 

        /// </summary> 

        __End_ScannerManager = 3 * __End_Common, 

 

 

        #region Сообщения OCR 

        /// <summary> 

        /// [исключение] 

        /// </summary> 

        [MessageParameters("{0}")] 


        RecognizerException, 

 

 

        /// <summary> 

        /// DpiXTop = {0}, DpiYTop = {1}, DpiXBottom = {2}, DpiYBottom = {3} 

        /// </summary> 

        [MessageParameters("DpiXTop = {0}, DpiYTop = {1}, DpiXBottom = {2}, DpiYBottom = {3}")] 

        RecognizerSetDpi, 

 

 

        /// <summary> 

        /// [CALL] ProcessingNextBuffer({0}, {1}) 

        /// </summary> 

        [MessageParameters("[CALL] ProcessingNextBuffer({0}, {1})")] 

        RecognizerNextBufferCall, 

 

 

        /// <summary> 

        /// [RET] ProcessingNextBuffer({0}, {1}) 

        /// </summary> 

        [MessageParameters("[RET] ProcessingNextBuffer({0}, {1})")] 

        RecognizerNextBufferReturn, 

 

 

        /// <summary> 

        /// [CALL] OCR.NextBuffer({0}) 

        /// </summary> 

        [MessageParameters("[CALL] OCR.NextBuffer({0})")] 

        RecognizerOcrNextBufferCall, 

 

 

        /// <summary> 

        /// [RET] OCR.NextBuffer({0}, {1}) 

        /// </summary> 

        [MessageParameters("[RET] OCR.NextBuffer({0}) = {1}")] 

        RecognizerOcrNextBufferReturn, 

 

 

        /// <summary> 

        /// GetOnlineMarker => {0} 

        /// </summary> 

        [MessageParameters("GetOnlineMarker => {0}")] 

        RecognizerOnlineBlankIndex, 

 

 

        /// <summary> 

        /// Маркер не может быть распознан в режиме Online 

        /// </summary> 

        [MessageParameters("Маркер не может быть распознан в режиме Online: {0}")] 

        RecognizerOnlineFailed, 


 
 

        /// <summary> 

        /// Определен индекс бюллетеня в режиме Online: {0} 

        /// </summary> 

        [MessageParameters("Определен индекс бюллетеня в режиме Online: {0}")] 

        RecognizerOnlineSuccess, 

 

 

        /// <summary> 

        /// Бюллетень распознан в режиме Online 

        /// </summary> 

        [MessageParameters("Бюллетень распознан в режиме Online")] 

        RecognizerOnlineBulletinValid, 

 

 

        /// <summary> 

        /// Ошибка при сохранении результата распознавания 

        /// </summary> 

        [MessageParameters("Ошибка при сохранении результата распознавания")] 

        RecognizerSaveResultError, 

 

 

        /// <summary> 

        /// Ошибка добавления результата голосования 

        /// </summary> 

        [MessageParameters("Ошибка добавления результата голосования")] 

        RecognizerAddVotingResultError, 

 

 

        /// <summary> 

        /// Halftone stamp: {0} [{1}] 

        /// </summary> 

        [MessageParameters("Halftone stamp: {0} [{1}]")] 

        RecognizerHalftoneStamp, 

 

 

        /// <summary> 

        /// Binary stamp: {0} 

        /// </summary> 

        [MessageParameters("Binary stamp: {0}")] 

        RecognizerBinaryStamp, 

 

 

        /// <summary> 

        /// Число бюл. = {0}, {1} [{2}], №{3}, {4}, {5}, Отметки: {6} 

        /// </summary> 

        [MessageParameters("Число бюл. = {0}, {1} [{2}], №{3}, {4}, {5}, Отметки: {6}")] 

        RecognitionResult, 

 


 
        /// <summary> 

        /// Запись строки в протокол распознавания 

        /// </summary> 

        [MessageParameters("{0}")] 

        RecognizerLog, 

 

 

        /// <summary> 

        /// {0} бюллетень. Режим {1} 

        /// </summary> 

        [MessageParameters("{0} бюллетень. Режим {1}")] 

        RecognizerBulletinResult, 

 

 

        /// <summary> 

        /// [Описание НУФ] 

        /// </summary> 

        [MessageParameters("{0}")] 

        RecognizerNuf, 

 

 

        /// <summary> 

        /// Номер печати: {0}. Альтернативы: {1}, {2}, {3}, {4} 

        /// </summary> 

        [MessageParameters("Номер печати: {0}. Альтернативы: {1}, {2}, {3}, {4}")] 

        RecognizerStampNumber, 

 

 

        /// <summary> 

        /// Проверка альтернатив для номера печати '{0}' 

        /// </summary> 

        [MessageParameters("Проверка альтернатив для номера печати '{0}'")] 

        RecognizerCheckAlternatives, 

 

 

        /// <summary> 

        /// [CALL] SaveLastImage({0}, {1}, {2}) 

        /// </summary> 

        [MessageParameters("[CALL] SaveLastImage({0}, {1}, {2})")] 

        RecognizerSaveImageCall, 

 

 

        /// <summary> 

        /// [RET] SaveLastImage({0}, {1}, {2}) 

        /// </summary> 

        [MessageParameters("[RET] SaveLastImage({0}, {1}, {2})")] 

        RecognizerSaveImageReturn, 

 

 


        /// <summary> 

        /// Сохранение изображения: нет свободного места на {0} (доступно {1}, требуется {2}) 

        /// </summary> 

        [MessageParameters("Сохранение изображения: нет свободного места на {0} (доступно {1}, требуется {2})")] 

        RecognizerSaveImageNotEnoughFreeSpace, 

 

 

        /// <summary> 

        /// Сохранение изображения выполнено за {0} мсек 

        /// </summary> 

        [MessageParameters("Сохранение изображения выполнено за {0} мсек")] 

        RecognizerSaveImageTiming, 

 

 

        /// <summary> 

        /// Порог бинаризации для стороны {0} = {1} 

        /// </summary> 

        [MessageParameters("Порог бинаризации для стороны {0} = {1}")] 

        RecognizerBinarizationThreshold, 

 

 

        /// <summary> 

        /// OCR ошибка: [{0}] {1} 

        /// </summary> 

        [MessageParameters("OCR ошибка: [{0}] {1}")] 

        RecognizerOcrError, 

 

 

        /// <summary> 

        /// OCR: {0} 

        /// </summary> 

        [MessageParameters("OCR: {0}")] 

        RecognizerOcrDebug, 

 

 

        /// <summary> 

        /// Результат сброса листа: {0} 

        /// </summary> 

        [MessageParameters("Результат сброса листа: {0}")] 

        RecognizerSheetDroped, 

        #endregion 

 

 

        /// <summary> 

        /// Конец сообщений OCR 

        /// </summary> 

        __End_Recognizer = 4 * __End_Common, 

 

 

        #region Сообщения звуковой подсистемы 


        /// <summary> 

        /// Ошибка при воспроизведении {0} 

        /// </summary> 

        [MessageParameters("Ошибка при воспроизведении {0}")] 

        SoundPlayException, 

 

 

		/// <summary> 

		/// "Не найден звуковой файл {0}" 

		/// </summary> 

		[MessageParameters("Не найден звуковой файл {0}")] 

		SoundFileNotFound, 

        #endregion 

 

 

        /// <summary> 

        /// Конец сообщений звуковой подсистемы 

        /// </summary> 

        __End_Sound = 5 * __End_Common, 

 

 

        #region Сообщения менеджера выборов 

        /// <summary> 

        /// [Исключение] 

        /// </summary> 

        [MessageParameters("{0}")] 

        ElectionException, 

 

 

        /// <summary> 

        /// Ошибка при добавлении результата голосования 

        /// </summary> 

        [MessageParameters("Ошибка при добавлении результата голосования")] 

        ElectionAddVotingResultException, 

 

 

        /// <summary> 

        /// Попытка прерывания потока добавления результата голосования 

        /// </summary> 

        [MessageParameters("Попытка прерывания потока добавления результата голосования")] 

        ElectionAddVotingResultAbort, 

 

 

        /// <summary> 

        /// Ошибка поиска файла с ИД в директории {0}: {1} [попытка №{2}] 

        /// </summary> 

        [MessageParameters("Ошибка поиска файла с ИД в директории {0}: {1} [попытка №{2}]")] 

        ElectionFindSourceDataError, 

 

 


        /// <summary> 

        /// Найден файл с ИД: {0}; УИК: {1} 

        /// </summary> 

        [MessageParameters("Найден файл с ИД: {0}; УИК: {1}")] 

        ElectionSourceDataFound, 

 

 

        /// <summary> 

        /// ИД загружены 

        /// </summary> 

        [MessageParameters("ИД загружены")] 

        ElectionSourceDataLoaded, 

 

 

        /// <summary> 

        /// Ошибка загрузки ИД 

        /// </summary> 

        [MessageParameters("Ошибка загрузки ИД")] 

        ElectionSourceDataLoadException, 

 

 

        /// <summary> 

        /// Поиск файла ИД в папке: {0} 

        /// </summary> 

        [MessageParameters("Поиск файла ИД в папке: {0}")] 

        ElectionSearchSourceDataInDir, 

 

 

        /// <summary> 

        /// Проверка файла ИД: {0} 

        /// </summary> 

        [MessageParameters("Проверка файла ИД: {0}")] 

        ElectionCheckSourceDataFile, 

 

 

        /// <summary> 

        /// Найден файл ИД: {0} 

        /// </summary> 

        [MessageParameters("Найден файл ИД: {0}")] 

        ElectionSourceDataFileFound, 

 

 

        /// <summary> 

        /// Ошибка при поиске пути к файлу для сохранения протокола с результатами голосования 

        /// </summary> 

        [MessageParameters("Ошибка при поиске пути к файлу для сохранения протокола с результатами голосования")] 

        ElectionFindFilePathToSaveVotingResultProtocolFailed, 

 

 

        /// <summary> 


        /// Ошибка при сохранении результатов голосования в локальной директории 

        /// </summary> 

        [MessageParameters("Ошибка при сохранении результатов голосования в локальной директории")] 

        ElectionSaveVotingResultToLocalDirFailed, 

 

 

        /// <summary> 

        /// Ошибка при сохранении результатов голосования на flash-диск 

        /// </summary> 

        [MessageParameters("Ошибка при сохранении результатов голосования на flash-диск")] 

        ElectionSaveVotingResultToFlashFailed, 

 

 

        /// <summary> 

        /// Ошибка при сохранении резервных копий результатов голосования на flash-диск 

        /// </summary> 

        [MessageParameters("Ошибка при сохранении резервных копий результатов голосования на flash-диск")] 

        ElectionSaveVotingResultReserveCopiesToFlashFailed, 

 

 

        #endregion 

 

 

        /// <summary> 

        /// Конец сообщений звуковой подсистемы 

        /// </summary> 

        __End_Election = 6 * __End_Common, 

 

 

        #region Сообщения синхронизации 

        /// <summary> 

        /// [Исключение] 

        /// </summary> 

        [MessageParameters("{0}")] 

        SyncException, 

 

 

        /// <summary> 

        /// Не удалось подключиться к удаленному сканеру: {0} 

        /// </summary> 

        [MessageParameters("Не удалось подключиться к удаленному сканеру: {0}")] 

        SyncCannotConnectToRemoteScanner, 

 

 

        /// <summary> 

        /// Канал для подключения удаленных сканеров открыт (localIPAddress = {0}) 

        /// </summary> 

        [MessageParameters("Канал для подключения удаленных сканеров открыт (localIPAddress = {0})")] 

        SyncChannelOpened, 

 


 
        /// <summary> 

        /// Канал для подключения удаленных сканеров закрыт 

        /// </summary> 

        [MessageParameters("Канал для подключения удаленных сканеров закрыт")] 

        SyncChannelClosed, 

 

 

        /// <summary> 

        /// Установлена роль сканера: {0} 

        /// </summary> 

        [MessageParameters("Установлена роль сканера: {0}")] 

        SyncScannerRoleSet, 

 

 

        /// <summary> 

        /// Запрос на подключение от сканера SerialNumber={0}, IPAddress={1} отклонен 

        /// </summary> 

        [MessageParameters("Запрос на подключение от сканера SerialNumber={0}, IPAddress={1} отклонен")] 

        SyncConnectRejected, 

 

 

        /// <summary> 

        /// Запрос на подключение от сканера SerialNumber={0}, IPAddress={1} принят 

        /// </summary> 

        [MessageParameters("Запрос на подключение от сканера SerialNumber={0}, IPAddress={1} принят")] 

        SyncConnectAccepted, 

 

 

        /// <summary> 

        /// Связь с удаленным сканером потеряна: {0} 

        /// </summary> 

        [MessageParameters("Связь с удаленным сканером потеряна: {0}")] 

        SyncDisconnected, 

 

 

        /// <summary> 

        /// Состояние загружено 

        /// </summary> 

        [MessageParameters("Состояние загружено")] 

        SyncStateLoaded, 

 

 

        /// <summary> 

        /// Ошибка загрузки состояния 

        /// </summary> 

        [MessageParameters("Ошибка загрузки состояния")] 

        SyncStateLoadException, 

 

 


        /// <summary> 

        /// Ошибка сохранения состояния 

        /// </summary> 

        [MessageParameters("Ошибка сохранения состояния")] 

        SyncStateSaveException, 

 

 

        /// <summary> 

        /// Не удалось сохранить состояние 

        /// </summary> 

        [MessageParameters("Не удалось сохранить состояние")] 

        SyncStateSaveFailed, 

 

 

        /// <summary> 

        /// Ошибка архивации состояния 

        /// </summary> 

        [MessageParameters("Ошибка архивации состояния")] 

        SyncStateArchiveException, 

 

 

        /// <summary> 

        /// Состояние сброшено в начальное 

        /// </summary> 

        [MessageParameters("Состояние сброшено в начальное")] 

        SyncStateReset, 

 

 

        /// <summary> 

        /// Ошибка при сбросе состояния в начальное 

        /// </summary> 

        [MessageParameters("Ошибка при сбросе состояния в начальное")] 

        SyncStateResetException, 

 

 

        /// <summary> 

        /// Ошибка при синхронизации с удаленным сканером 

        /// </summary> 

        [MessageParameters("Ошибка при синхронизации с удаленным сканером")] 

        SyncSynchronizationException, 

 

 

        /// <summary> 

        /// Ошибка восстановления состояния подсистемы: {0} 

        /// </summary> 

        [MessageParameters("Ошибка восстановления состояния подсистемы: {0}")] 

        SyncSubsystemRestoreStateFailed, 

 

 

        /// <summary> 


        /// Установлено новое время: {0:dd.MM.yyyy HH:mm:ss} UTC 

        /// </summary> 

        [MessageParameters("Установлено новое время: {0:dd.MM.yyyy HH:mm:ss} UTC")] 

        SyncSetSystemTime, 

 

 

        /// <summary> 

        /// Файл состояния не найден, загружаю начальное состояние 

        /// </summary> 

        [MessageParameters("Файл состояния не найден, загружаю начальное состояние")] 

        SyncInitialState, 

 

 

        /// <summary> 

        /// Файл состояния имеет нулевой размер 

        /// </summary> 

        [MessageParameters("Файл состояния имеет нулевой размер")] 

        SyncStateHasZeroSize, 

 

 

        /// <summary> 

        /// При удалении файла состояния произошла ошибка 

        /// </summary> 

        [MessageParameters("При удалении файла состояния произошла ошибка")] 

        SyncStateDeleteError, 

 

 

        /// <summary> 

        /// Состояние сохранено 

        /// </summary> 

        [MessageParameters("Состояние сохранено")] 

        SyncStateSaved, 

 

 

        /// <summary> 

        /// Синхронизация состояния отклонена, так как идет сканирование 

        /// </summary> 

        [MessageParameters("Синхронизация состояния отклонена, так как идет сканирование")] 

        SyncStateRejectedByScanning, 

        #endregion 

 

 

        /// <summary> 

        /// Конец сообщений синхронизации 

        /// </summary> 

        __End_Sync = 7 * __End_Common, 

 

 

        #region Сообщения конфигурации 

        /// <summary> 


        /// [Исключение] 

        /// </summary> 

        [MessageParameters("{0}")] 

        ConfigException, 

 

 

        /// <summary> 

        /// Ошибка загрузки рабочей конфигурации 

        /// </summary> 

        [MessageParameters("Ошибка загрузки рабочей конфигурации")] 

        ConfigLoadWorkingException, 

 

 

        /// <summary> 

        /// Ошибка поиска файла с частной конфигурации по пути: {0} 

        /// </summary> 

        [MessageParameters("Ошибка поиска файла с частной конфигурации по пути: {0}")] 

        ConfigFindPartialError, 

 

 

        /// <summary> 

        /// Ошибка чтения файла частной конфигурации 

        /// </summary> 

        [MessageParameters("Ошибка чтения файла частной конфигурации")] 

        ConfigReadPartialException, 

 

 

        /// <summary> 

        /// Ошибка загрузки частной конфигурации 

        /// </summary> 

        [MessageParameters("Ошибка загрузки частной конфигурации")] 

        ConfigLoadPartialException, 

        #endregion 

 

 

        /// <summary> 

        /// Конец сообщений конфигурации 

        /// </summary> 

        __End_Config = 8 * __End_Common, 

 

 

        #region Сообщения workflow 

        /// <summary> 

        /// [Исключение] 

        /// </summary> 

        [MessageParameters("{0}")] 

        WorkflowException, 

 

 

        /// <summary> 


        /// Во время работы потока произошло исключение 

        /// </summary> 

        [MessageParameters("Во время работы потока произошло исключение")] 

        WorkflowThreadException, 

 

 

        /// <summary> 

        /// Работа потока завершилась{0} 

        /// </summary> 

        [MessageParameters("Работа потока завершилась{0}")] 

        WorkflowThreadStopped, 

 

 

        /// <summary> 

        /// [Текст состояния] 

        /// </summary> 

        [MessageParameters("{0}")] 

        WorkflowText, 

 

 

        /// <summary> 

        /// Не можем определить наличие конфликта 

        /// </summary> 

        [MessageParameters("Не можем определить наличие конфликта")] 

        WorkflowCannotDetectConflict, 

 

 

        /// <summary> 

        /// Главный сканер обнаружил конфликт сканеров 

        /// </summary> 

        [MessageParameters("Главный сканер обнаружил конфликт сканеров")] 

        WorkflowMasterDetectConflict, 

 

 

        /// <summary> 

        /// Главный сканер не обнаружил конфликта сканеров 

        /// </summary> 

        [MessageParameters("Главный сканер не обнаружил конфликта сканеров")] 

        WorkflowMasterDetectNoConflict, 

 

 

        /// <summary> 

        /// Не можем определить, были ли введены доп. сведения на главном сканере 

        /// </summary> 

        [MessageParameters("Не можем определить, были ли введены доп. сведения на главном сканере")] 

        WorkflowCannotDetectAddInfoEnteredOnMaster, 

 

 

        /// <summary> 

        /// Доп. сведения не были введены на главном сканере 


        /// </summary> 

        [MessageParameters("Доп. сведения не были введены на главном сканере")] 

        WorkflowAddInfoNotEnteredOnMaster, 

 

 

        /// <summary> 

        /// Доп. сведения введены на главном сканере 

        /// </summary> 

        [MessageParameters("Доп. сведения введены на главном сканере")] 

        WorkflowAddInfoEnteredOnMaster, 

 

 

        /// <summary> 

        /// Неизвестный тип бланка: {0} 

        /// </summary> 

        [MessageParameters("Неизвестный тип бланка: {0}")] 

        WorkflowUnknownBlankType, 

 

 

        /// <summary> 

        /// ---> {0} 

        /// </summary> 

        [MessageParameters("---> {0}")] 

        WorkflowActivityExecutionStarting, 

 

 

        /// <summary> 

        /// <--- {0} 

        /// </summary> 

        [MessageParameters("<--- {0}")] 

        WorkflowActivityExecutionFinished, 

 

 

		/// <summary> 

		/// Ошибка при сериализации счетчика ошибок 

		/// </summary> 

		[MessageParameters("Ошибка при сериализации счетчика ошибок")] 

		WorkflowErrorCounterSerializationException, 

 

 

        /// <summary> 

        /// Не удалось сохранить файл счетчика ошибок 

        /// </summary> 

        [MessageParameters("Не удалось сохранить файл счетчика ошибок")] 

        WorkflowErrorCounterSerializationFailed, 

 

 

        /// <summary> 

        /// Ошибка при десериализации счетчика ошибок 

        /// </summary> 


        [MessageParameters("Ошибка при десериализации счетчика ошибок")] 

        WorkflowErrorCounterDeserializationError, 

 

 

        /// <summary> 

        /// Файл счетчика ошибок имеет нулевой размер 

        /// </summary> 

        [MessageParameters("Файл счетчика ошибок имеет нулевой размер")] 

        WorkflowErrorCounterHasZeroSize, 

 

 

        /// <summary> 

        /// При удалении файла счетчика ошибок произошла ошибка 

        /// </summary> 

        [MessageParameters("При удалении файла счетчика ошибок произошла ошибка")] 

        WorkflowErrorCounterDeleteError, 

 

 

        /// <summary> 

        /// Запущен поток контроля времени начала голосования 

        /// </summary> 

        [MessageParameters("Запущен поток контроля времени начала голосования")] 

        WorkflowControlThreadStarted, 

 

 

        /// <summary> 

        /// Поток контроля времени начала голосования: пора закончить тренировку! 

        /// </summary> 

        [MessageParameters("Поток контроля времени начала голосования: пора закончить тренировку!")] 

        WorkflowControlThreadNeedFinishTraining, 

 

 

        #endregion 

 

 

        /// <summary> 

        /// Конец сообщений workflow 

        /// </summary> 

        __End_Workflow = 9 * __End_Common, 

 

 

        #region Сообщения менеджера файловой системы 

 

 

        /// <summary> 

        /// Ошибка при записи в файл '{0}' 

        /// </summary> 

        [MessageParameters("Ошибка при записи в файл '{0}'")] 

        FileSystemWriteToFileError, 

 


 
        /// <summary> 

        /// Ошибка при определении размера свободного места в '{0}' 

        /// </summary> 

        [MessageParameters("Ошибка при определении размера свободного места в '{0}'")] 

        FileSystemCheckFreeSpaceError, 

 

 

        /// <summary> 

        /// Ошибка при синхронизации файловой системы 

        /// </summary> 

        [MessageParameters("Ошибка при синхронизации файловой системы")] 

        FileSystemSyncError, 

 

 

        /// <summary> 

        /// Свободное место на диске {0} ({1}) = {2} байт 

        /// </summary> 

        [MessageParameters("Свободное место на диске {0} ({1}) = {2} байт")] 

        FileSystemDiscSpace, 

 

 

        /// <summary> 

        /// Ошибка при выполнении безопасной сериализации {0} на стадии {1} 

        /// </summary> 

        [MessageParameters("Ошибка при выполнении безопасной сериализации {0} на стадии {1}")] 

        FileSystemSafeSerializationFailed, 

 

 

        #endregion 

 

 

        /// <summary> 

        /// Конец сообщений менеджера файловой системы 

        /// </summary> 

        __End_FileSystem = 10 * __End_Common, 

 

 

        #region Сообщения менеджера клавиатуры 

 

 

        /// <summary> 

        /// Тип клавиатуры: {0} ({1}) 

        /// </summary> 

        [MessageParameters("Тип клавиатуры: {0} ({1})")] 

        KeyboardType, 

 

 

        /// <summary> 

        /// Нажата клавиша: code = {0}; type = {1}; value = {2}; time = {3} 


        /// </summary> 

        [MessageParameters("Нажата клавиша: code = {0}; type = {1}; value = {2}; time = {3}")] 

        KeyboardKeyPressed, 

 

 

        #endregion 

 

 

        /// <summary> 

        /// Конец сообщений менеджера клавиатуры 

        /// </summary> 

        __End_Keyboard = 11 * __End_Common, 

 

 

        #region Сообщения менеджера печати 

 

 

        /// <summary> 

        /// Ошибка загрузки шаблона отчета: {0} 

        /// </summary> 

        [MessageParameters("Ошибка загрузки шаблона отчета: {0}")] 

        PrintingLoadReportTemplateFailed, 

 

 

        /// <summary> 

        /// Не найден шаблон отчета: {0} 

        /// </summary> 

        [MessageParameters("Не найден шаблон отчета: {0}")] 

        PrintingReportTemplateNotFound, 

 

 

        /// <summary> 

        /// Ошибка валидации шаблона отчета: {0} 

        /// </summary> 

        [MessageParameters("Ошибка валидации шаблона отчета: {0}")] 

        PrintingReportTemplateValidationError, 

 

 

        /// <summary> 

        /// Ошибка формирования PDF-отчета 

        /// </summary> 

        [MessageParameters("Ошибка формирования PDF-отчета")] 

        PrintingPdfBuildFailed, 

 

 

        /// <summary> 

        /// Ошибка при формировании заголовков протокола (сформированы по умолчанию) 

        /// </summary> 

        [MessageParameters("Ошибка при формировании заголовков протокола (сформированы по умолчанию)")] 

        PrintingReportHeadersBuildFailed, 


 
 

        /// <summary> 

        /// Ошибка при формировании содержания протокола (сформированы по умолчанию) 

        /// </summary> 

        [MessageParameters("Ошибка при формировании содержания протокола (сформированы по умолчанию)")] 

        PrintingReportBodyBuildFailed, 

 

 

		/// <summary> 

		/// "Запуцена печать {0} страницы файла {1}" 

		/// </summary> 

		[MessageParameters("Запущена печать {0} страницы файла {1}")] 

		PrintingStartPagePrint, 

 

 

		/// <summary> 

		/// "Ошибка при печати документа" 

		/// </summary> 

		[MessageParameters("Ошибка при печати документа {0}")] 

		PrintingException, 

 

 

		/// <summary> 

		/// "Найден принтер {0}" 

		/// </summary> 

		[MessageParameters("Найден принтер {0}")] 

		PrintingFindPrinter, 

 

 

		/// <summary> 

		/// "Строка результата поиска принтеров: {0}" 

		/// </summary> 

		[MessageParameters("Строка результата поиска принтеров: {0}")] 

		PrintingBackendLine, 

 

 

		/// <summary> 

		/// "Ошибка при разрешении очереди принтера" 

		/// </summary> 

		[MessageParameters("Ошибка при разрешении очереди принтера")] 

		PrinterEnablingError, 

 

 

        #endregion 

 

 

        /// <summary> 

        /// Конец сообщений менеджера клавиатуры 

        /// </summary> 


        __End_Printing = 12 * __End_Common, 

 

 

        #region Сообщения о действиях пользователя 

 

 

        /// <summary> 

        /// {стате состояние}: {текущее действие} - {клавиша} 

        /// </summary> 

        [MessageParameters("Нажатие клавиш {0}: {1} - {2}")] 

        UserKeyPressed, 

 

 

        /// <summary> 

        /// Переход в состояние {0} 

        /// </summary> 

        [MessageParameters("Переход в состояние {0}")] 

        WorkflowStateChanged, 

 

 

		/// <summary> 

		/// Изменен параметр {0} подсистемы {1}, старое значение {2} новое {3}  

		/// </summary> 

		[MessageParameters("Изменен параметр {0} подсистемы {1}, старое значение {2} новое {3}")] 

		ParameterUpdated, 

 

 

        #endregion 

 

 

        /// <summary> 

        /// Конец сообщений менеджера клавиатуры 

        /// </summary> 

        __End_UserAction = 13 * __End_Common, 

    } 

}


