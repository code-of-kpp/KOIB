namespace Croc.Bpc.Diagnostics 
{ 
    public enum Message 
    { 
        #region Общесистемные сообщения 
        [MessageParameters("Необработанное исключение: {0}")] 
        Common_UnhandledException = 1, 
        [MessageParameters("Произошла внутренняя ошибка! Дальнейшая работа программы невозможна. Обратитесь в службу технической поддержки.")] 
        Common_CriticalException, 
        [MessageParameters("{0}")] 
        Common_Exception, 
        [MessageParameters("{0}")] 
        Common_Information, 
        [MessageParameters("{0}")] 
        Common_Debug, 
        [MessageParameters("LOCK try enter: {0}")] 
        Common_LockTryEnter, 
        [MessageParameters("LOCK done: {0}")] 
        Common_LockDone, 
        [MessageParameters("LOCK exit: {0}")] 
        Common_LockExit, 
        [MessageParameters("Версия приложения: {0}")] 
        Common_ApplicationVersion, 
        [MessageParameters("Версия приложения: {0} (DEBUG)")] 
        Common_ApplicationVersionDebug, 
        [MessageParameters("Имя компьютера: {0}")] 
        Common_MachineName, 
        [MessageParameters("IP-адрес: {0}")] 
        Common_IpAddress, 
        [MessageParameters("Тихий (быстрый) запуск")] 
        Common_QuietStart, 
        [MessageParameters("Получен критичный Unix-сигнал: {0}")] 
        Common_UnixCriticalSignalReceived, 
        [MessageParameters("Получен Unix сигнал: {0}")] 
        Common_UnixSignalReceived, 
        [MessageParameters("[{0},STA] {1} {2}")] 
        Common_ProcessStartInfo, 
        [MessageParameters("{0} {1}: process start failed: {2}")] 
        Common_ProcessStartFailed, 
        [MessageParameters("{0} {1}: process execution failed: {2}")] 
        Common_ProcessExecutionFailed, 
        [MessageParameters("[{0},OUT] {1}")] 
        Common_ProcessStdOutDump, 
        [MessageParameters("[{0},ERR] {1}")] 
        Common_ProcessStdErrDump, 
        [MessageParameters("[{0},EXI] {1}")] 
        Common_ProcessExitCode, 
        [MessageParameters("Результат выполнения команды '{0} {1}' = {2}")] 
        Common_ExecCommandResult, 
        [MessageParameters("call")] 
        Common_DebugCall, 
        [MessageParameters("return")] 
        Common_DebugReturn, 
        [MessageParameters("Вызываем событие {0}")] 
        Common_SetEvent, 
        [MessageParameters("Ждем события {0}")] 
        Common_WaitEvent, 
        [MessageParameters("Ошибка в задании формата строки: {0}")] 
        Common_SingleLineFormatterFormatError, 
        [MessageParameters("Запись в протокол {0} прекращена из-за недостатка свободного места (доступно {1:N} MiB, требуется {2:N} MiB)")] 
        Common_AvailableSpaceFilterNotEnoughSpaceError, 
        #endregion 
        __End_Common = 1000, 
        #region Сообщения менеджера сканеров 
        #region Исключения 
        [MessageParameters("Ошибка при отправке первого сообщения")] 
        ScannerManagerSendingFirstMessageError, 
        [MessageParameters("Ошибка при отправке сообщения: {0}")] 
        ScannerManagerSendingMessageError, 
        [MessageParameters("Ошибка при получении сообщения от удаленного сканера")] 
        ScannerManagerPollMessageError, 
        [MessageParameters("Ошибка в методе потока рассылки широковещательных уведомлений")] 
        ScannerManagerBroadcastError, 
        [MessageParameters("Ошибка подключения к сканеру")] 
        ScannerManagerConnectingScannerError, 
        [MessageParameters("Ошибка при обработке ошибки от сканера")] 
        ScannerManagerAlertingErrorError, 
        [MessageParameters("Ошибка при обработке поступления нового листа")] 
        ScannerManagerNewSheetError, 
        [MessageParameters("Ошибка при обработке следующего буфера листа")] 
        ScannerManagerNextBufferError, 
        [MessageParameters("Ошибка при обработке события сброса листа")] 
        ScannerManagerSheetDropError, 
        [MessageParameters("Ошибка при обработке события готовности листа")] 
        ScannerManagerSheetIsReadyError, 
        [MessageParameters("Ошибка при обработке события готовности к приему листа")] 
        ScannerManagerReadyToScanError, 
        [MessageParameters("Ошибка диагностики оборудования")] 
        ScannerManagerDiagnosticError, 
        [MessageParameters("Ошибка при выполнении команды реверса")] 
        ScannerManagerReverseError, 
        [MessageParameters("Ошибка при {0} мотора: motorNumber = {1}; direction = {2}; step = {3}")] 
        ScannerManagerMotorError, 
        [MessageParameters("Ошибка в рабочем потоке сканера")] 
        ScannerManagerScannerWorkThreadError, 
        [MessageParameters("Ошибка потоке отправки сообщений в сканер")] 
        ScannerManagerScannerSendEventsThreadError, 
        #endregion 
        [MessageParameters("Отправлено сообщение о присутствии сканера в сети")] 
        ScannerBrodcastMessageSended, 
        [MessageParameters("Рассылка широковещательных уведомлений о присутствие данного сканера остановлена")] 
        ScannerBrodcastMessagingStopped, 
        [MessageParameters("Выполняется реверс...")] 
        ScannerExecutingRevers, 
        [MessageParameters("Получен буфер {0}")] 
        ScannerManagerBufferIsReady, 
        [MessageParameters("Отладочное сообщение: {0} ({1} байт)")] 
        ScannerManagerDebugMessage, 
        [MessageParameters("Ошибка от сканера {0}: {1}")] 
        ScannerManagerError, 
        [MessageParameters("NextBuffer({0})")] 
        ScannerManagerNextBufferCall, 
        [MessageParameters("Для бланка {0} установлена плотность {1}")] 
        ScannerManagerDensitySet, 
        [MessageParameters("Считано строк больше, чем размер буфера ({0})")] 
        ScannerManagerSheetIsReadyTooLarge, 
        [MessageParameters("Не удалось подключиться к сканеру [попытка №{0}]")] 
        ScannerManagerCantConnect, 
        [MessageParameters("Лист обработан с ошибкой: {0}")] 
        ScannerManagerSheetProcessedWithError, 
        [MessageParameters("Лист обработан: результат = {0}; тип сброса = {1}")] 
        ScannerManagerSheetProcessed, 
        [MessageParameters("Сбрасываем лист с кодом {0}")] 
        ScannerManagerDropSheet, 
        [MessageParameters("Ошибка при сбросе листа с кодом {0}")] 
        ScannerManagerDropSheetFailed, 
        [MessageParameters("Команда на реверс (код причины = {0})")] 
        ScannerManagerRevers, 
        [MessageParameters("Сканер отклонил команду реверса")] 
        ScannerManagerReversRejected, 
        [MessageParameters("Лист реверсирован")] 
        ScannerManagerReversSuccessfull, 
        [MessageParameters("Запрошен полутон: Side={0}; X={1}; Y={2}; W={3}; H={4}; ID={5}")] 
        ScannerManagerGetHalftone, 
        [MessageParameters("{0} {1} буфер стороны {2} сохранен в файл {3}")] 
        ScannerManagerBufferSaved, 
        [MessageParameters("{0} {1} буфер стороны {2} не удалось сохранить в файл {3}: {4}")] 
        ScannerManagerBufferSaveError, 
        [MessageParameters("Попытка установить режим ламп, равный текущему режиму: {0}")] 
        ScannerManagerTrySetEqualsLampsRegime, 
        [MessageParameters("Установлен новый режим ламп: текущий = {0}; новый = {1}")] 
        ScannerManagerSetLampsRegime, 
        [MessageParameters("Запоздалая попытка восстановить прежний режим ламп: текущий = {0}; прежний = {1}")] 
        ScannerManagerLateTryRestoreLampsRegime, 
        [MessageParameters("Восстановлен прежний режим ламп: текущий = {0}; новый = {1}")] 
        ScannerManagerRestoreLampsRegime, 
        [MessageParameters("Сигнализируем об ошибке (код={0})")] 
        ScannerManagerAlertError, 
        [MessageParameters("Подключение к сканеру {0} установлено: {1}")] 
        ScannerManagerConnected, 
        [MessageParameters("Рабочий поток сканера прерван")] 
        ScannerManagerScannerWorkThreadAborted, 
        [MessageParameters("Поток отправки сообщений в сканер прерван")] 
        ScannerManagerScannerSendEventsThreadAborted, 
        [MessageParameters("Найден Gs2Manager {0} ({1}, {2}, {3}, {4}, {5})")] 
        ScannerManagerDetectedHardware, 
        [MessageParameters("Новый лист")] 
        ScannerManagerNewSheet, 
        [MessageParameters("Команда сброса выполнена:  marking={0}, result={1}")] 
        ScannerManagerSheetDroped, 
        [MessageParameters("Результат команды сброса скорректирован: {0}")] 
        ScannerManagerSheetDropedResultAdjusted, 
        [MessageParameters("Лист отсканирован: кол-во строк = {0}, тип листа = {1}")] 
        ScannerManagerSheetIsReady, 
        [MessageParameters("Готов к приему")] 
        ScannerManagerReadyToScanning, 
        [MessageParameters("Сбой питания: min = {0}, max = {1}, avg = {2}")] 
        ScannerManagerPowerFailure, 
        [MessageParameters("Параметры питания: min = {0}, max = {1}, avg = {2}")] 
        ScannerManagerPowerStatistics, 
        [MessageParameters("Попытка установить текст остановленному прокрутчику: [{0}]")] 
        ScannerManagerTryToSetTextToRollWhenStopped, 
        [MessageParameters("Установлен текст прокрутчика: [{0}]")] 
        ScannerManagerSetTextToRoll, 
        [MessageParameters("Открываем новую сессию обработки бюллетеня...")] 
        ScannerManagerOpenNewSheetProcessingSession, 
        [MessageParameters("Открыли новую сессию обработки бюллетеня: Id = {0}")] 
        ScannerManagerOpenNewSheetProcessingSessionDone, 
        [MessageParameters("Неизвестный код ошибки при закрытии сессии обработки бюллетеня: Id = {0}, errorCode = {1}, driverError = {2}")] 
        ScannerManagerUnknownErrorOnClosingSheetProcessingSession, 
        [MessageParameters("Попытка закрыть уже закрытую сессию обработки бюллетеня с ошибкой: Id = {0}, errorCode = {1}, driverError = {2}")] 
        ScannerManagerTryToCloseAlreadyClosedSheetProcessingSessionWithError, 
        [MessageParameters("Закрываем сессию обработки бюллетеня с ошибкой: Id = {0}, errorCode = {1}, driverError = {2}")] 
        ScannerManagerCloseSheetProcessingSessionWithError, 
        [MessageParameters("Закрываем сессию обработки бюллетеня: Id = {0}")] 
        ScannerManagerCloseSheetProcessingSession, 
        [MessageParameters("Попытка закрыть уже закрытую сессию обработки бюллетеня: Id = {0}")] 
        ScannerManagerTryToCloseAlreadyClosedSheetProcessingSession, 
        [MessageParameters("Ожидаем закрытия сессии обработки бюллетеня: Id = {0}...")] 
        ScannerManagerWaitForCloseSheetProcessingSession, 
        [MessageParameters("Прокрутчик текста остановлен: текущий текст = '{0}'; текст после запуска = '{1}'")] 
        RollTextMachineStopped, 
        [MessageParameters("Прокрутчик текста запущен: текущий текст = '{0}'")] 
        RollTextMachineStarted, 
        #endregion 
        __End_ScannerManager = 2 * __End_Common, 
        #region Сообщения OCR 
        #region Исключения 
        [MessageParameters("Ошибка при сбросе состояния распознавалки")] 
        RecognizerResetError, 
        [MessageParameters("Ошибка распознавания бюллетеня")] 
        RecognizerBulletinError, 
        [MessageParameters("Ошибка при сохранении изображения")] 
        RecognizerSaveImageError, 
        [MessageParameters("Ошибка при сохранении копии лога OCR")] 
        RecognizerSaveOrcLogCopyError, 
        [MessageParameters("Ошибка при инициализации распознавалки")] 
        RecognizerInitError, 
        [MessageParameters("Ошибка при завершении распознавания бюллетеня")] 
        RecognizerEndBulletinRecognitionError, 
        [MessageParameters("Ошибка при выключении лонирования OCR")] 
        RecognizerStopOcrLoggingError, 
        [MessageParameters("Ошибка при добавлении номера печати вышестоящей комиссии")] 
        RecognizerSuperiorStampError, 
        [MessageParameters("Ошибка при удалении лога OCR")] 
        RecognizerOcrLogDeletingError, 
        [MessageParameters("Ошибка получения имени файла для сохранения изображения")] 
        RecognizerGetImageFileNameError, 
        [MessageParameters("Ошибка получения буфера")] 
        RecognizerGetBufferError, 
        [MessageParameters("Ошибка записи в журнал сканирования")] 
        RecognizerLogError, 
        #endregion 
        [MessageParameters("Модель создана")] 
        RecognizerModelCreated, 
        [MessageParameters("Call OCR.RunRecognize")] 
        RecognizerOCRRunRecognizeCall, 
        [MessageParameters("OCR.RunRecognize return")] 
        RecognizerOCRRunRecognizeReturn, 
        [MessageParameters("Lines = {0}")] 
        RecognizerLinesInfo, 
        [MessageParameters("NextBuffer...")] 
        RecognizerBeforeNextBufferCall, 
        [MessageParameters("EndRecognize...")] 
        RecognizerBeforeEndRecognizeCall, 
        [MessageParameters("EndRecognize done")] 
        RecognizerEndRecognizeReturn, 
        [MessageParameters("Результат распознавания сохранен")] 
        RecognizerRecognitionResultSaved, 
        [MessageParameters("Маркируем лист методом: {0}")] 
        RecognizerMarkSheet, 
        [MessageParameters("Бюллетень распознан")] 
        RecognizerBulletinRecognized, 
        [MessageParameters("Бюллетень не имеет текущего режима голосования")] 
        RecognizerWrongModeForBulletin, 
        [MessageParameters("Сохранение изображения...")] 
        RecognizerSavingImage, 
        [MessageParameters("DpiXTop = {0}, DpiYTop = {1}, DpiXBottom = {2}, DpiYBottom = {3}")] 
        RecognizerSetDpi, 
        [MessageParameters("ProcessingNextBuffer({0})")] 
        RecognizerNextBufferCall, 
        [MessageParameters("Call OCR.NextBuffer({0})...")] 
        RecognizerOcrNextBufferCall, 
        [MessageParameters("OCR.NextBuffer({0}) = {1}")] 
        RecognizerOcrNextBufferReturn, 
        [MessageParameters("GetOnlineMarker...")] 
        RecognizerOnlineBlankIndexCall, 
        [MessageParameters("GetOnlineMarker: {0}")] 
        RecognizerOnlineBlankIndex, 
        [MessageParameters("Маркер не может быть распознан в режиме Online: {0}")] 
        RecognizerOnlineFailed, 
        [MessageParameters("Определен индекс бюллетеня в режиме Online: {0}")] 
        RecognizerOnlineSuccess, 
        [MessageParameters("Бюллетень распознан в режиме Online")] 
        RecognizerOnlineBulletinValid, 
        [MessageParameters("Ошибка при сохранении результата распознавания")] 
        RecognizerSaveResultError, 
        [MessageParameters("Ошибка добавления результата голосования")] 
        RecognizerAddVotingResultError, 
        [MessageParameters("Halftone stamp: {0} [{1}]")] 
        RecognizerHalftoneStamp, 
        [MessageParameters("Binary stamp: {0}")] 
        RecognizerBinaryStamp, 
        [MessageParameters("Число бюл. = {0}, {1} [{2}], №{3}, {4}, {5}, Отметки: {6}")] 
        RecognitionResult, 
        [MessageParameters("{0}")] 
        RecognizerLog, 
        [MessageParameters("{0} бюллетень. Режим {1}")] 
        RecognizerBulletinResult, 
        [MessageParameters("НУФ: {0}")] 
        RecognizerNuf, 
        [MessageParameters("Номер печати: {0}. Альтернативы: {1}, {2}, {3}, {4}")] 
        RecognizerStampNumber, 
        [MessageParameters("Проверка альтернатив для номера печати '{0}'")] 
        RecognizerCheckAlternatives, 
        [MessageParameters("Call SaveLastImage({0}, {1})")] 
        RecognizerSaveImageCall, 
        [MessageParameters("Сохранение изображения: нет свободного места на {0} (доступно {1}, требуется {2})")] 
        RecognizerSaveImageNotEnoughFreeSpace, 
        [MessageParameters("Сохранение изображения выполнено за {0} мсек")] 
        RecognizerSaveImageTiming, 
        [MessageParameters("Порог бинаризации для стороны {0} = {1}")] 
        RecognizerBinarizationThreshold, 
        [MessageParameters("OCR ошибка: [{0}] {1}")] 
        RecognizerOcrError, 
        [MessageParameters("OCR: {0}")] 
        RecognizerOcrDebug, 
        [MessageParameters("Результат сброса листа: {0}")] 
        RecognizerSheetDroped, 
        #endregion 
        __End_Recognizer = 3 * __End_Common, 
        #region Сообщения звуковой подсистемы 
        [MessageParameters("Ошибка при воспроизведении {0}")] 
        SoundPlayError, 
        [MessageParameters("Не найден звуковой файл {0}")] 
        SoundFileNotFound, 
        [MessageParameters("Начинаем воспроизведение {0}")] 
        SoundSpeexStartPlay, 
        [MessageParameters("Ошибка при получении громкости")] 
        SoundGetVolumeFailed, 
        [MessageParameters("Установлена громкость: {0}%")] 
        SoundSetVolume, 
        #endregion 
        __End_Sound = 4 * __End_Common, 
        #region Сообщения менеджера результатов голосования 
        [MessageParameters("Очищены данные о голосах для тестового режима")] 
        VotingResult_ClearTestData, 
        [MessageParameters("Попытка установить некорректное значение голосов с ключом {0}: старое значение = {1}, новое значение = {2}; счетчик = {3}")] 
        VotingResult_TryToSetIncorrectVotesValue, 
        [MessageParameters("Установлено значение голосов с ключом {0}: старое значение = {1}, новое значение = {2}; счетчик = {3}")] 
        VotingResult_SetVotesValue, 
        [MessageParameters("Новое состояния отклонено, т.к. оно = null")] 
        VotingResult_NewStateRejectedBecauseIsNull, 
        [MessageParameters("Новое состояния принято в результате слияния")] 
        VotingResult_NewStateAcceptedByMerge, 
        [MessageParameters("Новое состояния принято (без слияния)")] 
        VotingResult_NewStateAccepted, 
        [MessageParameters("Ошибка принятия нового состояния менеджера выборов")] 
        VotingResult_NewStateAссeptError, 
        [MessageParameters("Ошибка при добавлении результата голосования")] 
        VotingResult_AddVotingResultError, 
        [MessageParameters("Попытка прерывания потока добавления результата голосования")] 
        VotingResult_AddVotingResultAbort, 
        [MessageParameters("Поиск пути к файлу для сохранения протокола с результатами голосования...")] 
        VotingResult_FindFilePathToSaveVotingResultProtocol, 
        [MessageParameters("Поиск пути к файлу для сохранения протокола с результатами голосования завершен: {0}")] 
        VotingResult_FindFilePathToSaveVotingResultProtocolDone, 
        [MessageParameters("Ошибка при поиске пути к файлу для сохранения протокола с результатами голосования")] 
        VotingResult_FindFilePathToSaveVotingResultProtocolFailed, 
        [MessageParameters("Файл с результатами голосования успешно сохранен в локальной директории: {0}")] 
        VotingResult_SaveVotingResultToLocalDirSucceeded, 
        [MessageParameters("Ошибка при сохранении файла с результатами голосования в локальной директории: {0}")] 
        VotingResult_SaveVotingResultToLocalDirFailed, 
        [MessageParameters("Ошибка при сохранении результатов голосования на flash-диск")] 
        VotingResult_SaveVotingResultToFlashFailed, 
        [MessageParameters("Ошибка при сохранении резервных копий результатов голосования на flash-диск")] 
        VotingResult_SaveVotingResultReserveCopiesToFlashFailed, 
        #endregion 
        __End_VotingResult = 5 * __End_Common, 
        #region Сообщения менеджера выборов 
        [MessageParameters("Ошибка поиска файла с ИД в директории {0}: {1} [попытка №{2}]")] 
        Election_FindSourceDataError, 
        [MessageParameters("ИД загружены")] 
        Election_SourceDataLoaded, 
        [MessageParameters("Восстановление ИД...")] 
        Election_SourceDataRepairing, 
        [MessageParameters("Проверка ИД...")] 
        Election_SourceDataVerifying, 
        [MessageParameters("Проверка создания модели...")] 
        Election_CheckCreateModel, 
        [MessageParameters("ИД из файла {0} некорректные: {1}")] 
        Election_SourceDataIncorrect, 
        [MessageParameters("ИД успешно загружены из файла: {0}")] 
        Election_SourceDataSuccessfullyLoadedFromFile, 
        [MessageParameters("Ошибка загрузки ИД из файла {0}")] 
        Election_SourceDataLoadFromFileFailed, 
        [MessageParameters("Поиск файла ИД в папке: {0}")] 
        Election_SearchSourceDataInDir, 
        [MessageParameters("Проверка файла ИД: {0}")] 
        Election_CheckSourceDataFile, 
        [MessageParameters("Найден файл ИД: {0}")] 
        Election_SourceDataFileFound, 
        [MessageParameters("Ошибка при установки ИД")] 
        Election_SetSourceDataFailed, 
        [MessageParameters("Установлены ИД: УИК={0}; дата голосования={1}; время стационарного голосования=[{2}-{3}]")] 
        Election_SetSourceDataSucceeded, 
        [MessageParameters("Новое состояния отклонено, т.к. оно = null")] 
        Election_NewStateRejectedBecauseIsNull, 
        [MessageParameters("Новое состояния отклонено, т.к. новые ИД = null")] 
        Election_NewStateRejectedBecauseNewSourceDataIsNull, 
        [MessageParameters("Новое состояния принято")] 
        Election_NewStateAccepted, 
        [MessageParameters("Ошибка принятия нового состояния")] 
        Election_NewStateAссeptError, 
        [MessageParameters("Контрольные соотношения не выполнены: {0}")] 
        Election_ChecksFailed, 
        #endregion 
        __End_Election = 6 * __End_Common, 
        #region Сообщения синхронизации 
        [MessageParameters("Ошибка открытия канала для подключения удаленных сканеров")] 
        SyncOpenInteractionChannelError, 
        [MessageParameters("Ошибка при подключении к удаленному сканеру")] 
        SyncConnectRemoteScannerError, 
        [MessageParameters("Ошибка при обработке события потери связи с удаленным сканером")] 
        SyncRemoteScannerDisconnectedError, 
        [MessageParameters("Удаленный сканер подключился")] 
        SyncRemoteScannerConnected, 
        [MessageParameters("Удаленный сканер отключился")] 
        SyncRemoteScannerDisconnected, 
        [MessageParameters("Удаленный сканер ожидает инициализации")] 
        SyncRemoteScannerWaitForInitialization, 
        [MessageParameters("Удаленный сканер вышел из меню")] 
        SyncRemoteScannerExitFromMenu, 
        [MessageParameters("Извещаем о подключении удаленного сканера")] 
        SyncRemoteScannerConnectedEventRaise, 
        [MessageParameters("Извещаем об отключении удаленного сканера")] 
        SyncRemoteScannerDisconnectedEventRaise, 
        [MessageParameters("Извещаем о том, что удаленный сканер ждет инициализации")] 
        SyncRemoteScannerWaitForInitializationEventRaise, 
        [MessageParameters("Извещаем о том, что удаленный сканер вышел из меню")] 
        SyncRemoteScannerExitFromMenuEventRaise, 
        [MessageParameters("Игнорируем сообщение удаленного сканера о том, что он подключен")] 
        SyncIgnoreRemoteScannerConnectedEvent, 
        [MessageParameters("Пробуем установить соединение с удаленным сканером")] 
        SyncTryOpenConnection, 
        [MessageParameters("Удаленный сканер запретил подключение")] 
        SyncRemoteConnectionNotAllow, 
        [MessageParameters("Получаем коннектор, Uri: {0}")] 
        SyncGetRemoteConnector, 
        [MessageParameters("Подключение к удаленному сканеру установлено: SerialNumber = {0}; IP = {1}")] 
        SyncRemoteConnectionSuccess, 
        [MessageParameters("Запущен поток отслеживания наличия связи с удаленным сканером")] 
        SyncMonitorConnectionStarted, 
        [MessageParameters("Проверка связи включена")] 
        SyncMonitorConnectionEnabled, 
        [MessageParameters("Проверка связи вЫключена")] 
        SyncMonitorConnectionDisabled, 
        [MessageParameters("Проверка связи...")] 
        SyncExecMonitorConnection, 
        [MessageParameters("Подключение к удаленному сканеру потеряно: SerialNumber = {0}; IP = {1}")] 
        SyncRemoteConnectionLoss, 
        [MessageParameters("Начинаем синхронизацию")] 
        SyncStartSynchronization, 
        [MessageParameters("Ждем завершения текущей синхронизации...")] 
        SyncWaitForCurrentSynchronizationFinished, 
        [MessageParameters("Синхронизация выключена или 2-ого сканера нет")] 
        SyncStopedOrNoSecondScanner, 
        [MessageParameters("Отправляем на синхронизацию элементы состояния: {0}")] 
        SyncStateItemsForSync, 
        [MessageParameters("Изменен элемент состояния '{0}': old = '{1}'; new = '{2}'")] 
        SyncStateItemUpdated, 
        [MessageParameters("Синхронизация включена")] 
        SyncEnabled, 
        [MessageParameters("Синхронизация вЫключена")] 
        SyncDisabled, 
        [MessageParameters("Игнорируем запрос на синхронизацию: синхронизация выключена")] 
        SyncIgnoreSyncRequestByDisabled, 
        [MessageParameters("Начинаем обработку запроса на синхронизацию")] 
        SyncStartRequestExecuting, 
        [MessageParameters("Синхронизация состояния подсистемы: {0}")] 
        SyncSubsystemState, 
        [MessageParameters("Результат принятия нового состояния подсистемой {0}: {1}")] 
        SyncSubsystemStateResult, 
        [MessageParameters("Синхронизация не требуется")] 
        SyncNotNeeded, 
        [MessageParameters("Запрос на синхронизацию обработан")] 
        SyncRequestProcessed, 
        [MessageParameters("Синхронизация завершена: {0}")] 
        SyncComplete, 
        [MessageParameters("Не удалось подключиться к удаленному сканеру: {0}")] 
        SyncCannotConnectToRemoteScanner, 
        [MessageParameters("Канал для подключения удаленных сканеров открыт (localIPAddress = {0})")] 
        SyncChannelOpened, 
        [MessageParameters("Канал для подключения удаленных сканеров закрыт")] 
        SyncChannelClosed, 
        [MessageParameters("Установлена роль сканера: {0}")] 
        SyncScannerRoleSet, 
        [MessageParameters("Запрос на подключение от сканера SerialNumber={0}, IPAddress={1} отклонен")] 
        SyncConnectRejected, 
        [MessageParameters("Запрос на подключение от сканера SerialNumber={0}, IPAddress={1} принят")] 
        SyncConnectAccepted, 
        [MessageParameters("Вызов удаленного метода '{0}' завершился ошибкой: {1}")] 
        SyncCallRemoteMethodFailed, 
        [MessageParameters("Пробуем вызвать удаленный метод '{0}' еще раз, попытка №{1}")] 
        SyncTryCallRemoteMethodAgain, 
        [MessageParameters("Связь с удаленным сканером потеряна")] 
        SyncDisconnected, 
        [MessageParameters("Связь с удаленным сканером потеряна (уже знаем, что связи нет)")] 
        SyncDisconnectedAlreadyKnown, 
        [MessageParameters("Состояние загружено")] 
        SyncStateLoaded, 
        [MessageParameters("Не удалось загрузить состояние")] 
        SyncStateLoadFailed, 
        [MessageParameters("Ошибка загрузки состояния")] 
        SyncStateLoadError, 
        [MessageParameters("Ошибка сохранения состояния")] 
        SyncStateSaveError, 
        [MessageParameters("Не удалось сохранить состояние")] 
        SyncStateSaveFailed, 
        [MessageParameters("Ошибка сохранения бекапа текущего состояния")] 
        SyncBackupCurrentStateFailed, 
        [MessageParameters("Состояние сброшено в начальное")] 
        SyncResetStateSucceeded, 
        [MessageParameters("Ошибка при сбросе состояния в начальное")] 
        SyncStateResetFailed, 
        [MessageParameters("Ошибка при синхронизации с удаленным сканером")] 
        SyncSynchronizationError, 
        [MessageParameters("Ошибка восстановления состояния подсистемы: {0}")] 
        SyncSubsystemRestoreStateFailed, 
        [MessageParameters("Установлено новое время: {0:dd.MM.yyyy HH:mm:ss} UTC")] 
        SyncSetSystemTime, 
        [MessageParameters("Удаленный сканер вызвал метод '{0}', параметры: {1}")] 
        SyncRemoteScannerCall, 
        [MessageParameters("Файл состояния не найден, загружаю начальное состояние")] 
        SyncInitialState, 
        [MessageParameters("Состояние сохранено")] 
        SyncStateSaved, 
        [MessageParameters("Синхронизация состояния отклонена, так как идет сканирование")] 
        SyncStateRejectedByScanning, 
        [MessageParameters("Сброс состояния, причина = '{0}'")] 
        SyncResetState, 
        [MessageParameters("Начинается печать на локальном сканере")] 
        SyncPrintReportStartingOnLocal, 
        [MessageParameters("Печать на локальном сканере окончена")] 
        SyncPrintReportFinishedOnLocal, 
        [MessageParameters("Начинается печать на удаленном сканере")] 
        SyncPrintReportStartingOnRemote, 
        [MessageParameters("Печать на удаленном сканере окончена")] 
        SyncPrintReportFinishedOnRemote, 
        [MessageParameters("Запускаем процесс переподнятия сети...")] 
        SyncIfrestartStarting, 
        [MessageParameters("Процесс переподнятия сети завершен: res = {0}")] 
        SyncIfrestartDone, 
        [MessageParameters("Сброс ПО: причина={0}; удаленный инициатор={1}; перезапуск={2}")] 
        SyncResetSoft, 
        #endregion 
        __End_Sync = 7 * __End_Common, 
        #region Сообщения конфигурации 
        [MessageParameters("Ошибка удаления рабочего конфиг-файла")] 
        ConfigDeleteWorkingError, 
        [MessageParameters("Ошибка применения конфигурации")] 
        ConfigApplyError, 
        [MessageParameters("Ошибка загрузки рабочей конфигурации")] 
        ConfigLoadWorkingError, 
        [MessageParameters("Ошибка поиска файла с частной конфигурации по пути: {0}")] 
        ConfigFindPartialError, 
        [MessageParameters("Ошибка чтения файла частной конфигурации")] 
        ConfigReadPartialError, 
        [MessageParameters("Ошибка загрузки частной конфигурации")] 
        ConfigLoadPartialError, 
        [MessageParameters("Изменен параметр '{0}' подсистемы '{1}': старое значение [{2}]; новое значение [{3}]")] 
        ConfigSubsystemConfigUpdated, 
        #endregion 
        __End_Config = 8 * __End_Common, 
        #region Сообщения workflow 
        [MessageParameters("Работа потока завершилась")] 
        WorkflowThreadStopped, 
        [MessageParameters("Работа потока завершилась. Результат: {0}")] 
        WorkflowThreadStoppedWithResult, 
        [MessageParameters("Работа потока прервана: {0}")] 
        WorkflowThreadTerminated, 
        [MessageParameters("{0}")] 
        WorkflowText, 
        [MessageParameters("Воспроизведение фразы остановлено нажатием кнопки ДА или НЕТ")] 
        WorkflowSoundPlayingStoppedByYesOrNoPressed, 
        [MessageParameters("Воспроизведение фразы остановлено в результате переключения выполнения на другое действие")] 
        WorkflowSoundPlayingStoppedByActivityExecutionInterrupt, 
        [MessageParameters("Не можем определить наличие конфликта")] 
        WorkflowCannotDetectConflict, 
        [MessageParameters("Главный сканер обнаружил конфликт сканеров")] 
        WorkflowMasterDetectConflict, 
        [MessageParameters("Главный сканер не обнаружил конфликта сканеров")] 
        WorkflowMasterDetectNoConflict, 
        [MessageParameters("Обнаружен конфликт")] 
        WorkflowHasConflict, 
        [MessageParameters("Не можем определить, были ли введены доп. сведения на главном сканере")] 
        WorkflowCannotDetectAddInfoEnteredOnMaster, 
        [MessageParameters("Доп. сведения введены для выборов Id={0}")] 
        WorkflowAddInfoEnteredForElection, 
        [MessageParameters("Выборы с Id={0}, который получен с главного сканера, не найдены")] 
        WorkflowElectionWithIdFromMasterNotFound, 
        [MessageParameters("Ввод доп. сведений завершен")] 
        WorkflowAddInfoEnteringFinished, 
        [MessageParameters("Неизвестный тип бланка: {0}")] 
        WorkflowUnknownBlankType, 
        [MessageParameters("---> {0}")] 
        WorkflowActivityExecutionStarting, 
        [MessageParameters("<--- {0}")] 
        WorkflowActivityExecutionFinished, 
        [MessageParameters("Ошибка при сохранении счетчика ошибок")] 
        WorkflowErrorCounterSaveError, 
        [MessageParameters("Не удалось сохранить счетчик ошибок")] 
        WorkflowErrorCounterSaveFailed, 
        [MessageParameters("Не удалось загрузить счетчик ошибок")] 
        WorkflowErrorCounterLoadFailed, 
        [MessageParameters("Ошибка при загрузке счетчика ошибок")] 
        WorkflowErrorCounterLoadError, 
        [MessageParameters("Запущен контроль начала голосования: Период={0}, TrainingMinTime={1}, RealMinTime={2}")] 
        WorkflowControlThreadStarted, 
        [MessageParameters("Контроль начала голосования: голосование уже идет")] 
        WorkflowControlThreadVotingAlreadyGoes, 
        [MessageParameters("Контроль начала голосования: голосование скоро начнется!")] 
        WorkflowControlThreadVotingWillSoonStart, 
        [MessageParameters("До начала голосования осталось не более 10 мин")] 
        WorkflowNotMoreThen10MinToVotingStart, 
        [MessageParameters("До окончания голосования осталось не более 10 мин")] 
        WorkflowNotMoreThen10MinToVotingEnd, 
        [MessageParameters("Причина реверса не определена: DropResult = {0}; VotingResult = {1}")] 
        WorkflowReverseReasonUndefined, 
        [MessageParameters("Удаленный сканер принял роль {0}")] 
        WorkflowRemoteScannerTakeRole, 
        [MessageParameters("Версии ПО различаются: данный сканер = {0}; удаленный сканер = {1}")] 
        WorkflowApplicationVersionsDiffer, 
        [MessageParameters("Ждем решение главного сканера о наличии конфликта...")] 
        WorkflowWaitForMasterDecision, 
        [MessageParameters("Главный сканер ждет, когда удаленный определит свою роль...")] 
        WorkflowMasterWaitForRemoteScannerRoleDefined, 
        [MessageParameters("Подчиненный сканер ждет, когда удаленный определит свою роль...")] 
        WorkflowSlaveWaitForRemoteScannerRoleDefined, 
        [MessageParameters("Начало инициализации: ждем удаленный сканер...")] 
        WorkflowInitStartWaitForRemoteScanner, 
        [MessageParameters("Начало инициализации: сообщаем удаленному сканеру, что мы его ждем")] 
        WorkflowInitStartNoticeRemoteScannerAboutWait, 
        [MessageParameters("Ждем получение частного конфига с главного сканера...")] 
        WorkflowWaitForPartialConfigFromMaster, 
        [MessageParameters("Ждем завершения ввода доп. сведений на главном сканере...")] 
        WorkflowWaitForAddInfoEnteredOnMaster, 
        [MessageParameters("Ждем синхронизации с главным сканером...")] 
        WorkflowWaitForSynchronizationWithMaster, 
        [MessageParameters("Новое состояние '{0}' отклонено, т.к. его порядковый номер меньше, чем у текущего состояния '{1}'")] 
        WorkflowNewStateRejectedBecauseOfOrderLess, 
        [MessageParameters("Новое состояние '{0}' отклонено, т.к. текущее состояние '{1}' Главного сканера с таким же порядковым номером имеет приоритет")] 
        WorkflowNewStateRejectedBecauseOfMasterStateHasPriority, 
        [MessageParameters("Принято новое состояние '{0}' (текущее = '{1}')")] 
        WorkflowNewStateAccepted, 
        [MessageParameters("Проверка конфликта: сбрасываем состояние на подчиненном")] 
        WorkflowResetStateOnSlaveScanner, 
        [MessageParameters("Запускаем синхронизацию...")] 
        WorkflowStartSynchronization, 
        [MessageParameters("Переход к действию: {0}")] 
        WorkflowStateChanged, 
        [MessageParameters("Нажатие клавиш {0}: {1} - {2}")] 
        WorkflowUserKeyPressed, 
        [MessageParameters("Сброс ПО на подчиненном сканере в результате обнаружения конфликта")] 
        WorkflowResetSoftOnSlaveBecauseConflictDetected, 
        [MessageParameters("Сброс ПО на главном сканере в результате обнаружения конфликта")] 
        WorkflowResetSoftOnMasterBecauseConflictDetected, 
        #endregion 
        __End_Workflow = 9 * __End_Common, 
        #region Сообщения менеджера файловой системы 
        [MessageParameters("Ошибка при записи в файл '{0}'")] 
        FileSystemWriteToFileError, 
        [MessageParameters("Ошибка при перемещении директории '{0}' в архив")] 
        FileSystemMoveToArchiveError, 
        [MessageParameters("Не удалось удалить архивную директорию")] 
        FileSystemDeleteArchiveDirectoryError, 
        [MessageParameters("Не удалось удалить файл {0}")] 
        FileSystemDeleteFileError, 
        [MessageParameters("Не удалось выполнить архивацию директории {0}")] 
        FileSystemArchiveFolderError, 
        [MessageParameters("Ошибка при определении размера свободного места в '{0}'")] 
        FileSystemCheckFreeSpaceError, 
        [MessageParameters("Ошибка при синхронизации файловой системы")] 
        FileSystemSyncError, 
        [MessageParameters("Свободное место на диске {0} ({1}) = {2} байт")] 
        FileSystemDiscSpace, 
        [MessageParameters("Ошибка при выполнении безопасной сериализации {0} на стадии {1}")] 
        FileSystemSafeSerializationFailed, 
        [MessageParameters("Файл {0} имеет нулевой размер")] 
        FileSystemFileHasZeroSize, 
        [MessageParameters("При удалении файла {0} произошла ошибка")] 
        FileSystemFileDeleteError, 
        [MessageParameters("Ошибка при выполнении безопасной десериализации из файла {0} на стадии {1}")] 
        FileSystemSafeDeserializationFailed, 
        #endregion 
        __End_FileSystem = 10 * __End_Common, 
        #region Сообщения менеджера клавиатуры 
        [MessageParameters("Тип клавиатуры: {0} ({1})")] 
        KeyboardType, 
        [MessageParameters("Нажата клавиша: code = {0}; type = {1}; value = {2}; time = {3}")] 
        KeyboardKeyPressed, 
        [MessageParameters("Создан драйвер клавиатуры ({0})")] 
        KeyboardDriverCreated, 
        [MessageParameters("Ошибка создания драйвера клавиатуры ({0}): {1}")] 
        KeyboardDriverCreationFailed, 
        [MessageParameters("Ошибка в методе обработки нажатий на клавиши: {0}")] 
        KeyboardDriverWorkMethodFailed, 
        #endregion 
        __End_Keyboard = 11 * __End_Common, 
        #region Сообщения менеджера печати 
        [MessageParameters("Ошибка загрузки шаблона отчета: {0}")] 
        PrintingLoadReportTemplateFailed, 
        [MessageParameters("Не найден шаблон отчета: {0}")] 
        PrintingReportTemplateNotFound, 
        [MessageParameters("Ошибка валидации шаблона отчета: {0}")] 
        PrintingReportTemplateValidationError, 
        [MessageParameters("Ошибка формирования PDF-отчета")] 
        PrintingPdfBuildFailed, 
        [MessageParameters("Ошибка при формировании заголовков протокола (сформированы по умолчанию)")] 
        PrintingReportHeadersBuildFailed, 
        [MessageParameters("Ошибка при формировании содержания протокола (сформированы по умолчанию)")] 
        PrintingReportBodyBuildFailed, 
        [MessageParameters("Запущена печать {0} страницы файла {1}, копий: {2}")] 
        PrintingStartPagePrint, 
        [MessageParameters("Ошибка при печати документа {0}")] 
        PrintingError, 
        [MessageParameters("Найден принтер: '{0}'")] 
        PrintingFindPrinter, 
        [MessageParameters("Строка результата поиска принтеров: {0}")] 
        PrintingBackendLine, 
        [MessageParameters("Ошибка при разрешении очереди принтера")] 
        PrintingEnablingError, 
        [MessageParameters("Начало выполнения: {0}")] 
        PrintingPdfBuilderStartEvent, 
        [MessageParameters("Окончание выполнения: {0}")] 
        PrintingPdfBuilderEndEvent, 
        [MessageParameters("Формирование отчета: {0}")] 
        PrintingCreateReport, 
        [MessageParameters("Печать отчета: {0}")] 
        PrintingPrintReport, 
        #endregion 
        __End_Printing = 12 * __End_Common, 
        #region Сообщения DummyScannerDriver 
        [MessageParameters("Начинаем эмуляцию сканирования")] 
        DummyScannerStartScanning, 
        [MessageParameters("Читаем следующий буфер")] 
        DummyScannerReadNextBuffer, 
        [MessageParameters("Чтение буферов завершено")] 
        DummyScannerReadBuffersReturn, 
        [MessageParameters("Эмуляция сканирования завершена")] 
        DummyScannerEndScanning, 
        [MessageParameters("Лист реверсирован")] 
        DummyScannerSheetReversed, 
        [MessageParameters("SheetIssue = {0}")] 
        DummyScannerSheetIssue, 
        [MessageParameters("motor {0}, direction {1}: {2}")] 
        DummyScannerMotorCall, 
        [MessageParameters("{0}")] 
        DummyScannerIndicator, 
        [MessageParameters("Сообщение на время сканирования = '{0}'")] 
        DummyScannerSetScanningIndicatorMessage, 
        [MessageParameters("Red {0}")] 
        DummyScannerRedLamp, 
        [MessageParameters("Green {0}")] 
        DummyScannerGreenLamp, 
        [MessageParameters("LocalIP = {0}")] 
        DummyScannerLocalIp, 
        [MessageParameters("Версия эмулятора сканера не задана/задана некорректно. Установлена версия по умолчанию: V2010")] 
        DummyScannerWrongVersion, 
        [MessageParameters("Ошибка во время эмуляции сканирования")] 
        DummyScannerEmulationError, 
        [MessageParameters("ScanningEnabled {0}")] 
        DummyScannerScanningEnabled, 
        [MessageParameters("DoubleSheetSensorEnabled {0}")] 
        DummyScannerDoubleSheetSensorEnabled, 
        #endregion 
        __End_DummyScannerDriver = 13 * __End_Common, 
        #region Сообщения жизни КОИБ 
        [MessageParameters("Включение")] 
        ApplicationStart, 
        [MessageParameters("Выключение ({0})")] 
        ApplicationEnd, 
        [MessageParameters("'{0}' --> '{1}'. Бюллетеней {2}")] 
        VotingModeChange, 
        [MessageParameters("Архив {0}")] 
        ArchiveFolder, 
        #endregion 
        __End_MainLog = 14 * __End_Common, 
    } 
}
