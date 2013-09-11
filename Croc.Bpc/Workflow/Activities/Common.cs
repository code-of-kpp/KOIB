using System; 

using System.Collections.Generic; 

using System.Threading; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Bpc.Sound; 

using Croc.Bpc.Synchronization; 

using Croc.Core; 

using Croc.Core.Utils.Threading; 

using Croc.Workflow.ComponentModel; 

using Croc.Bpc.Keyboard; 

using System.Text; 

 

 

namespace Croc.Bpc.Workflow.Activities 

{ 

    [Serializable] 

    public class CommonActivity : BpcCompositeActivity 

    { 

        #region Константы 

 

 

        /// <summary> 

        /// Текст для индикатора сканера, который отображает что идет синхронизация 

        /// </summary> 

        public const string SYNCHRONIZATION_INDICATOR_TEXT = "Синхронизация..."; 

 

 

        #endregion 

 

 

        #region Свойства 

 

 

        /// <summary> 

        /// Текущая дата и время 

        /// </summary> 

        public DateTime DateTimeNow 

        { 

            get 

            { 

                return DateTime.Now; 

            } 

        } 

 

 

        /// <summary> 

        /// Текущая локальная дата и время 

        /// </summary> 

        public DateTime LocalDateTimeNow 

        { 


            get 

            { 

                return _electionManager.SourceData.LocalTimeNow; 

            } 

        } 

 

 

        #endregion 

 

 

        #region Режимы голосования 

 

 

        /// <summary> 

        /// Задан ли для выборов переносной режим 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        public NextActivityKey NeedPortableVoting( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _electionManager.SourceData.VotingModeExists(Croc.Bpc.Election.Voting.VotingMode.Portable) 

                ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

 

 

 

 

        #endregion 

 

 

        #region Работа со сканером 

 

 

        /// <summary> 

        /// Является ли данный сканер Главным? 

        /// </summary> 

        public NextActivityKey IsMasterScanner( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _syncManager.ScannerRole == ScannerRole.Master ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// Установить текст на индикаторе сканера 

        /// </summary> 

        /// <remarks> 

        /// Параметры: 

        ///     TextFormat      - формат текста 


        ///     Parameters      - параметры 

        /// </remarks> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey SetIndicator( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            var composer = new PhraseComposer(_soundManager); 

            composer.TextFormat = parameters.GetParamValue<string>("TextFormat"); 

            composer.Parameters = parameters.GetParamValueAsArray("Parameters"); 

            var text = composer.ComposeText(); 

 

 

            // установим текст на индикаторе 

            _scannerManager.SetIndicator(text); 

 

 

            return context.DefaultNextActivityKey; 

        } 

 

 

        #endregion 

 

 

        #region Воспроизведение фраз 

 

 

        #region Воспроизведение фраз и считывание значение 

 

 

        /// <summary> 

        /// Воспроизводит фразу и считывает значение, которое вводит пользователь 

        /// </summary> 

        /// <remarks> 

        /// Параметры: 

        ///     Value       - исходное значение типа long (или другого, который может быть приведен к long),  

        ///                     по умолчанию не задано (равно null); 

        ///                     значение вводится путем нажатия цифровых клавиш клавиатуры и 

        ///                     клавиши Del для удаления ранее введенного значения 

        ///     OK          - событие, при возникновении которого завершается считывание значения,  

        ///                     при этом значение становится равным тому, что ввел пользователь; 

        ///                     по умолчанию = нажатие кнопки ДА 

        ///     Cancel      - событие, при возникновении которого завершается считывание значения,  

        ///                     при этом значение остается не измененым; по умолчанию = нажатие кнопки НЕТ 

        ///     Required    - обязательно ли нужно вводить значение. Если = Да, то событие OK игнорируется, 

        ///                     если значение на момент возникновения события не введено. По умолчанию = Нет. 

        ///     NextActivityKeyResolver - метод, который вычисляет ключ след. действия на основании  

        ///                     события, завершившего считывание значение (OK или Cancel) 

        ///     TextFormat  - аналогично действию SayPhrase, но есть доп. требование: 


        ///                     1) строка не должна быть длиннее длины индикатора (16 символов) 

        ///                     2) строка должна содержать символы '_', каждый из которых будет соответствовать  

        ///                         вводимой цифре. Заполнение '_' цифрами осуществляется слева направо,  

        ///                         стирание - наоборот. Символы '_' - мигают. 

        ///     PasswordChar - символ, который нужно использовать для маскировки вводимых символов.  

        ///                     По умолчанию не задан (нет маскировки) 

        ///     PhraseFormat, Parameters, Sounds, SilentOnSlave, Silent - аналогично действию SayPhrase 

        ///     RepeatTimeout - аналогично SayAndWaitEvents 

        ///      

        ///     При нажатии на кнопку Помощь фраза повторяется 

        /// </remarks> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey SayAndReadValue( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            var okEvent = parameters.GetParamValue("OK", (WaitHandle)KeyPressedWaitHandle.YesPressed); 

            var cancelEvent = parameters.GetParamValue("Cancel", (WaitHandle)KeyPressedWaitHandle.NoPressed); 

 

 

            return SayAndReadValue(context, parameters, okEvent, cancelEvent); 

        } 

 

 

        /// <summary> 

        /// Воспроизводит фразу и считывает значение, событие "Отмена" = нажатие кнопки "Возврат" 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey SayAndReadValueCancelIsBack( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            var okEvent = parameters.GetParamValue("OK", (WaitHandle)KeyPressedWaitHandle.YesPressed); 

            var cancelEvent = parameters.GetParamValue("Cancel", (WaitHandle)KeyPressedWaitHandle.GoBackPressed); 

 

 

            return SayAndReadValue(context, parameters, okEvent, cancelEvent); 

        } 

 

 

        /// <summary> 

        /// Последнее считанное значение 

        /// </summary> 

        /// <remarks>если в момент начала считывания было задано инициализирующее значение, 

        /// то оно записывается в эту переменную</remarks> 

        public static string LastReadedValue; 

 

 


        /// <summary> 

        /// Воспроизведение фразы и считывание значения 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <param name="okEvent"></param> 

        /// <param name="cancelEvent"></param> 

        /// <returns></returns> 

        private NextActivityKey SayAndReadValue( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters, 

            WaitHandle okEvent, WaitHandle cancelEvent) 

        { 

            var composer = CreatePhraseComposer(parameters); 

            var text = composer.ComposeText(); 

            LastReadedValue = parameters.GetParamValue<object>("Value") == null 

                ? null : parameters.GetParamValue<long>("Value").ToString(); 

            var passwordChar = parameters.GetParamValue("PasswordChar", ReadValueContext.NULL_PASSWORD_CHAR); 

			var readValueMode = parameters.GetParamValue<ReadValueMode>("ReadValueMode", ReadValueMode.CutLeadingZero); 

 

 

            // контекст считывания значения 

            var readValueContext = new ReadValueContext( 

                context, _scannerManager.IndicatorLength, text, LastReadedValue, passwordChar, readValueMode); 

 

 

            // поток считывания значения 

            var readValueThread = new Thread(ReadValueMethod); 

 

 

            var required = parameters.GetParamValue("Required", false); 

            var result = context.DefaultNextActivityKey; 

            int occurredEventIndex = -1; 

 

 

            try 

            { 

                // запускаем поток считывания значения 

                readValueThread.Start(readValueContext); 

 

 

                // запускаем воспроизведение фразы и ожидание событий OK или Cancel 

                var silent = NeedSilent(parameters); 

                var sounds = silent ? new string[] { } : composer.ComposePhrase(); 

                var timeout = GetTimeout(parameters); 

 

 

                var nextActivityKeyResolver = GetNextActivityKeyResolver(parameters); 

 

 

                while (true) 


                { 

                    result = SayAndWaitEvents( 

                        context, "", sounds, silent, false, new[] { okEvent, cancelEvent }, 

                        true, timeout, nextActivityKeyResolver, out occurredEventIndex); 

 

 

                    // если произошло событие OK (оно первое в массиве) 

                    if (occurredEventIndex == 0) 

                    { 

                        // если значение обязательно для ввода и оно не введено 

                        if (required && string.IsNullOrEmpty(readValueContext.Value)) 

                            // то повторяем фразу, предлагающую ввести значение 

                            continue; 

 

 

                        // изменяем последнее считанное значение 

                        LastReadedValue = readValueContext.Value; 

                    } 

 

 

                    return result; 

                } 

            } 

            finally 

            { 

                // останавливаем считывание значения 

                readValueContext.StopReadingEvent.Set(); 

            } 

        } 

 

 

        /// <summary> 

        /// Метод потока считывания значения и отображения текста на индикаторе 

        /// </summary> 

        private void ReadValueMethod(object state) 

        { 

            // период мигания символов '_' (0.5 сек) 

            const int BLINK_TIMEOUT = 500; 

 

 

            var readValueContext = (ReadValueContext)state; 

            // признак того, что отображаем символы '_' 

            var underscoreDisplayed = true; 

 

 

            _logger.LogInfo(Message.WorkflowText, readValueContext.TextWithUnderscores); 

 

 

            try 

            { 


                while (true) 

                { 

                    // установим текст на индикаторе 

                    var text = underscoreDisplayed 

                        ? readValueContext.TextWithUnderscores : readValueContext.TextWithoutUnderscores; 

 

 

                    _scannerManager.SetIndicator(text); 

 

 

                    KeyPressedWaitHandle keyWaitHandle; 

                    int timeout; 

 

 

                    // еще не все поля для цифр заполнены 

                    if (readValueContext.TextWithUnderscores.IndexOf('_') != -1) 

                    { 

                        // если вообще ни одно поле для цифр не заполнено 

                        if (string.CompareOrdinal(readValueContext.TextWithUnderscores, readValueContext.OriginalText) == 0) 

                        { 

                            // будем ждать нажатия цифр, а если не нажимают, то будем мигать 

                            keyWaitHandle = KeyPressedWaitHandle.DigitPressed; 

                            timeout = BLINK_TIMEOUT; 

                        } 

                        else 

                        { 

                            // будем ждать нажатия цифр и клавиши стирания, а если не нажимают, то будем мигать 

                            keyWaitHandle = KeyPressedWaitHandle.DigitOrDeletePressed; 

                            timeout = BLINK_TIMEOUT; 

                        } 

                    } 

                    else 

                    { 

                        // будем ждать нажатия только клавиши стирания и не будем мигать 

                        keyWaitHandle = KeyPressedWaitHandle.DeletePressed; 

                        timeout = Timeout.Infinite; 

                    } 

 

 

                    // начинаем ждать остановки считывания или ввода цифр 

                    keyWaitHandle.Reset(); 

                    int index = readValueContext.WEContext.WaitAny( 

                        new WaitHandle[] { readValueContext.StopReadingEvent, keyWaitHandle }, timeout); 

 

 

                    // если это таймаут 

                    if (index == WaitHandle.WaitTimeout) 

                    { 

                        // инвертируем режим отображения символов '_' 

                        underscoreDisplayed = !underscoreDisplayed; 


                        continue; 

                    } 

 

 

                    // если это событие остановки считывания (первое в массиве) 

                    if (index == 0) 

                        return; 

 

 

                    // иначе - нажали цифру или клавишу стирания => изменяем текст и значение 

                    var keyArgs = keyWaitHandle.PressedKeyArgs; 

 

 

                    var oldValue = readValueContext.Value; 

                    // если нажали цифровую клавишу 

                    if (keyArgs.Type == KeyType.Digit) 

                    { 

                        // пробуем увеличить значение 

                        try 

                        { 

							// если нужно обрезать лидирующие 0 

							if (readValueContext.ReadValueMode == ReadValueMode.CutLeadingZero) 

								readValueContext.Value = 

									!string.IsNullOrEmpty(oldValue) ? 

										(int.Parse(oldValue) * 10 + keyArgs.Value).ToString() 

										: keyArgs.Value.ToString(); 

							else 

								readValueContext.Value += keyArgs.Value; 

						} 

                        catch (OverflowException) { } 

                    } 

                    // иначе - нажали Delete 

                    else 

                    { 

                        // пробуем уменьшить значение 

						readValueContext.Value = 

							string.IsNullOrEmpty(oldValue) || oldValue.Length == 1 

							? null : oldValue.Substring(0, oldValue.Length - 1); 

                    } 

 

 

                    // запишем в лог новую строку, которая отобразится на индикаторе 

                    _logger.LogInfo(Message.WorkflowText, readValueContext.TextWithUnderscores); 

                } 

            } 

            catch 

            { 

                // если исключение было сгенерировано с целью прерывания выполнения потока работ 

                if (readValueContext.WEContext.IsExecutionInterrupting()) 

                    // то все ОК - выходим 


                    return; 

 

 

                throw; 

            } 

        } 

 

 

        /// <summary> 

        /// Контекст считывания значения 

        /// </summary> 

        private class ReadValueContext 

        { 

            /// <summary> 

            /// Событие "Остановить считывание значения" 

            /// </summary> 

            public readonly ManualResetEvent StopReadingEvent = new ManualResetEvent(false); 

            /// <summary> 

            /// Контекст выполнения потока работ 

            /// </summary> 

            public readonly WorkflowExecutionContext WEContext; 

			/// <summary> 

			/// Режим считывания значения с клавиатуры 

			/// </summary> 

			public readonly ReadValueMode ReadValueMode; 

 

 

			/// <summary> 

            /// Исходный текст для отображения на индикаторе 

            /// </summary> 

            public readonly string OriginalText; 

            /// <summary> 

            /// Текст для отображения на индикаторе, который включает символы '_' 

            /// </summary> 

            public string TextWithUnderscores 

            { 

                get; 

                private set; 

            } 

            /// <summary> 

            /// Текст для отображения на индикаторе, в котором символы '_' заменены на пробелы 

            /// </summary> 

            public string TextWithoutUnderscores 

            { 

                get; 

                private set; 

            } 

 

 

            private string _value; 


            /// <summary> 

            /// Считанное значение 

            /// </summary> 

            public string Value 

            { 

                get 

                { 

                    return _value; 

                } 

                set 

                { 

                    _value = value; 

 

 

                    // обновим тексты для индикатора 

                    var sbWithUnderscores = new StringBuilder(OriginalText.Length); 

                    var sbWithoutUnderscores = new StringBuilder(OriginalText.Length); 

					var valueCharArr = string.IsNullOrEmpty(_value) ? new char[0] : _value.ToCharArray(); 

                    int digitIndex = -1; 

 

 

                    foreach (var ch in OriginalText) 

                    { 

                        if (ch == '_') 

                        { 

                            if (++digitIndex < valueCharArr.Length) 

                            { 

                                var digitCh = PasswordChar == NULL_PASSWORD_CHAR  

                                    ? valueCharArr[digitIndex] : PasswordChar; 

 

 

                                sbWithUnderscores.Append(digitCh); 

                                sbWithoutUnderscores.Append(digitCh); 

                            } 

                            else 

                            { 

                                sbWithUnderscores.Append('_'); 

                                sbWithoutUnderscores.Append(' '); 

                            } 

                        } 

                        else 

                        { 

                            sbWithUnderscores.Append(ch); 

                            sbWithoutUnderscores.Append(ch); 

                        } 

                    } 

 

 

                    TextWithUnderscores = sbWithUnderscores.ToString(); 

                    TextWithoutUnderscores = sbWithoutUnderscores.ToString(); 


                } 

            } 

 

 

            /// <summary> 

            /// Значение символа для маскировки, когда оно не задано, т.е. когда маскировка не используется 

            /// </summary> 

            public const char NULL_PASSWORD_CHAR = (char)0; 

            /// <summary> 

            /// Символ для маскировки при вводе пароля 

            /// </summary> 

            public readonly char PasswordChar; 

 

 

            /// <summary> 

            /// Конструктор 

            /// </summary> 

            /// <param name="weContext"></param> 

            /// <param name="indicatorLength"></param> 

            /// <param name="text"></param> 

            /// <param name="value"></param> 

            /// <param name="passwordChar"></param> 

            public ReadValueContext( 

                WorkflowExecutionContext weContext 

				, int indicatorLength 

				, string text 

				, string value 

				, char passwordChar 

				, ReadValueMode readingMode) 

            { 

                CodeContract.Requires(weContext != null); 

                WEContext = weContext; 

 

 

                // проверим и установим текст 

                CheckText(indicatorLength, text); 

                OriginalText = text; 

 

 

                // устанавливаем значение именно через обращение к св-ву, т.к. важно, чтобы при этом текст обновился 

                Value = value; 

 

 

                PasswordChar = passwordChar; 

				ReadValueMode = readingMode; 

            } 

 

 

            /// <summary> 

            /// Проверяет текст, который будет отображаться во время считывания значения 


            /// </summary> 

            /// <remarks> 

            /// 1) текст не должна быть длиннее длины индикатора 

            /// 2) текст должен содержать символы '_' 

            /// </remarks> 

            /// <param name="indicatorLength"></param> 

            /// <param name="text"></param> 

            private static void CheckText(int indicatorLength, string text) 

            { 

                if (string.IsNullOrEmpty(text)) 

                    throw new Exception("Текст для отображения на индикаторе должен быть задан"); 

 

 

                if (!text.Contains("_")) 

                    throw new Exception("Текст для отображения на индикаторе должен содержать символы '_'"); 

 

 

                if (text.Length > indicatorLength) 

                    throw new Exception(string.Format( 

                        "Длина текста для отображения на индикаторе не должна быть больше {0} символов", 

                        indicatorLength)); 

            } 

        } 

 

 

        #endregion 

 

 

        #region Воспроизведение фраз и ожидание событий 

 

 

        /// <summary> 

        /// Воспроизводит фразу и выводит текст на индикатор 

        /// </summary> 

        /// <remarks> 

        /// Параметры: 

        ///     TextFormat      - формат текста (если не задан, то на индикатор ничего не выводится) 

        ///     PhraseFormat    - формат фразы 

        ///     Parameters      - параметры 

        ///     Sounds          - список звук. файлов (обязательный) 

        ///     SilentOnSlave   - признак того, что не нужно воспроизводить фразу на подчиненном сканере  

        ///                         (по умолчанию = false, т.е. фраза будет воспроизводиться) 

        ///     Silent          - признак того, что фразу вообще не нужно воспроизводить 

        ///      

        /// Если во время воспроизведения была нажата кнопка ДА или НЕТ, то воспроизведение прерывается 

        /// </remarks> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey SayPhrase(WorkflowExecutionContext context, ActivityParameterDictionary parameters) 


        { 

            var composer = CreatePhraseComposer(parameters); 

            var text = composer.ComposeText(); 

 

 

            // если есть, что выводить на индикатор 

            if (text.Length > 0) 

            { 

                _logger.LogInfo(Message.WorkflowText, text); 

                // установим текст на индикаторе 

                _scannerManager.SetIndicator(text); 

            } 

 

 

            // если нужно молчать 

            if (NeedSilent(parameters)) 

                // то выходим 

                return context.DefaultNextActivityKey; 

 

 

            // воспроизведем фразу 

            var playSoundFinishedEvent = new AutoResetEvent(false); 

            var sounds = composer.ComposePhrase(); 

 

 

            _soundManager.PlaySounds(sounds, new EventHandler((sender, e) => { playSoundFinishedEvent.Set(); })); 

 

 

            try 

            { 

                // начинаем ждать нажатия кнопки ДА или НЕТ или завершения воспроизведения фразы 

                KeyPressedWaitHandle.YesOrNoPressed.Reset(); 

                var index = context.WaitAny( 

                    new WaitHandle[] { KeyPressedWaitHandle.YesOrNoPressed, playSoundFinishedEvent }); 

 

 

                // если нажали кнопку 

                if (index == 0) 

                    // прерываем воспроизведение 

                    _soundManager.StopPlaying(); 

            } 

            catch (ActivityExecutionInterruptException ex) 

            { 

                // прерываем воспроизведение 

                _soundManager.StopPlaying(); 

 

 

                throw ex; 

            } 

 


 
            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Воспроизводит фразу и ожидает любое из заданных событий 

        /// </summary> 

        /// <remarks> 

        /// Параметры: 

        ///     TextFormat, PhraseFormat, Parameters, Sounds, SilentOnSlave, Silent - аналогично SayPhrase 

        ///     IgnoreButtonsOnSlave    - признак того, что нужно игнорировать нажатия кнопок на подчиненном  

        ///                                 сканере (по умолчанию = false, т.е. нажатия будут обрабатываться) 

        ///     SayFirstTime            - воспроизводить ли фразу первый раз (true/false). По умолчанию = true.  

        ///                                 Если = false, то фраза воспроизводится только по истечение  

        ///                                 таймаута RepeatTimeout или при нажатии на кнопку Помощь 

        ///     RepeatTimeout           - таймаут (формат "d | [d.]hh:mm[:ss[.ff]] | Infinite"), по истечение  

        ///                                 которого (отчет начитается сразу после завершения воспроизведения фразы)  

        ///                                 фраза начинает воспроизводиться заново, если ожидаемое событие не произошло 

        ///          

        ///     WaitedEvents            - ожидаемое событие 

        ///     NextActivityKeyResolver  - метод, который вычисляет ключ след. действия 

        ///      

        /// Если до завершения воспроизведения фразы случилось ожидаемое событие, то воспроизведение прерывается. 

        /// Если во время ожидания была нажата кнопка ПОМОЩЬ, то воспроизведение фразы повторяется 

        /// </remarks> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey SayAndWaitEvents( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            var waitedEvents = parameters.GetParamValueAsArray<WaitHandle>("WaitedEvents"); 

            var nextActivityKeyResolver = GetNextActivityKeyResolver(parameters); 

 

 

            return SayAndWaitEvents(context, parameters, waitedEvents, nextActivityKeyResolver); 

        } 

 

 

        /// <summary> 

        /// Воспроизводит фразу и ожидает нажатия кнопки Да 

        /// </summary> 

        public NextActivityKey SayAndWaitYes( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return SayAndWaitEvents(context, parameters, 

                new[] { KeyPressedWaitHandle.YesPressed }, DefaultNextActivityKeyResolver); 

        } 

 


 
        /// <summary> 

        /// Воспроизводит фразу и ожидает нажатия кнопки Нет 

        /// </summary> 

        public NextActivityKey SayAndWaitNo( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return SayAndWaitEvents(context, parameters, 

                new[] { KeyPressedWaitHandle.NoPressed }, DefaultNextActivityKeyResolver); 

        } 

 

 

        /// <summary> 

        /// Воспроизводит фразу и ожидает нажатия кнопки Да или Нет 

        /// </summary> 

        public NextActivityKey SayAndWaitYesOrNo( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return SayAndWaitEvents(context, parameters, 

                new[] { KeyPressedWaitHandle.YesOrNoPressed }, DefaultNextActivityKeyResolver); 

        } 

 

 

        /// <summary> 

        /// Воспроизводит фразу и ожидает нажатия кнопки Да и Нет одновременно 

        /// </summary> 

        public NextActivityKey SayAndWaitYesAndNoAtOnce( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            SayAndWaitEvents(context, parameters, 

                new[] { KeyPressedWaitHandle.YesAndNoAtOncePressed }, DefaultNextActivityKeyResolver); 

 

 

            // т.к. SayAndWaitEvents всегда возвращает ключ по последней нажатой клавиши, 

            // то вернем ключ YesAndNo 

            return BpcNextActivityKeys.YesAndNo; 

        } 

 

 

        /// <summary> 

        /// Воспроизводит фразу и ожидает нажатия кнопки Да или Нет или Возврат 

        /// </summary> 

        public NextActivityKey SayAndWaitYesOrNoOrBack( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return SayAndWaitEvents(context, parameters, 

                new[] { KeyPressedWaitHandle.YesOrNoOrBackPressed }, DefaultNextActivityKeyResolver); 

        } 

 

 


        /// <summary> 

        /// Воспроизводит фразу и ожидает нажатия кнопки Помощь 

        /// </summary> 

        public NextActivityKey SayAndWaitHelp( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return SayAndWaitEvents(context, parameters, 

                new[] { KeyPressedWaitHandle.HelpPressed }, DefaultNextActivityKeyResolver); 

        } 

 

 

        /// <summary> 

        /// Воспроизводит фразу и засыпает, т.е. остается бесконечно висеть в данном действие 

        /// </summary> 

        public NextActivityKey SayAndSleep( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            parameters.Add(new ActivityParameter("RepeatTimeout", "Infinite")); 

            return SayAndWaitEvents(context, parameters, new WaitHandle[] { }, DefaultNextActivityKeyResolver); 

        } 

 

 

		/// <summary> 

		/// Воспроизведение фразы и ожидание таймаута 

		/// </summary> 

		/// <param name="context"></param> 

		/// <param name="parameters"></param> 

		/// <returns></returns> 

		public NextActivityKey SayAndWaitTimeout( 

			WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

		{ 

			var timeout = parameters.GetParamValueOrThrow<int>("Timeout"); 

			parameters.Add(new ActivityParameter("RepeatTimeout", "Infinite")); 

 

 

			// создадим событие по ожиданию таймаута 

			var timeoutEvent = new TimeoutWaitHandle(timeout); 

			return SayAndWaitEvents 

				(context, parameters, new WaitHandle[1] { timeoutEvent }, DefaultNextActivityKeyResolver); 

		} 

 

 

        #endregion 

 

 

        #region Private-методы, реализующие воспроизведение фраз и ожидание событий 

 

 

        /// <summary> 

        /// Значение параметра RepeatTimeout по умолчанию = 30 сек. 


        /// </summary> 

        private const int DEFAULT_REPEAT_TIMEOUT = 30000; 

 

 

        #region Вычисление ключа след. действия 

 

 

        /// <summary> 

        /// Делегат метода для вычисления ключа след. действия по полученному событию 

        /// </summary> 

        /// <param name="occurredEvent">полученное событие</param> 

        /// <returns></returns> 

        public delegate NextActivityKey NextActivityKeyResolver( 

            WaitHandle occurredEvent, WorkflowExecutionContext context); 

 

 

        /// <summary> 

        /// Возвращает метод для вычисления ключа след. действия на основании значения параметра NextActivityKeyResolver 

        /// </summary> 

        /// <param name="parameters"></param> 

        /// <returns>метод для вычисления ключа след. действия, заданный в параметре NextActivityKeyResolver, 

        /// или метод по умолчанию, если значение параметра NextActivityKeyResolver не задано</returns> 

        private NextActivityKeyResolver GetNextActivityKeyResolver(ActivityParameterDictionary parameters) 

        { 

            return parameters.GetParamValue("NextActivityKeyResolver", 

                new NextActivityKeyResolver(DefaultNextActivityKeyResolver)); 

        } 

 

 

        /// <summary> 

        /// Метод для вычисления ключа след. действия, который используется по умолчанию 

        /// </summary> 

        /// <returns> 

        /// Если событие - это нажатие кнопки, то ключ след. действия соответствует этой кнопки, 

        /// иначе ключ = Yes 

        /// </returns> 

        private static NextActivityKey DefaultNextActivityKeyResolver( 

            WaitHandle occurredEvent, WorkflowExecutionContext context) 

        { 

            var keyWH = occurredEvent as KeyPressedWaitHandle; 

            if (keyWH != null) 

            { 

                var pressedKeyArgs = keyWH.PressedKeyArgs; 

 

 

                if (KeyPressedWaitHandle.YesPressed.WaitDescriptor.IsMatch(pressedKeyArgs)) 

                    return BpcNextActivityKeys.Yes; 

 

 

                if (KeyPressedWaitHandle.NoPressed.WaitDescriptor.IsMatch(pressedKeyArgs)) 


                    return BpcNextActivityKeys.No; 

 

 

                if (KeyPressedWaitHandle.GoBackPressed.WaitDescriptor.IsMatch(pressedKeyArgs)) 

                    return BpcNextActivityKeys.Back; 

 

 

                if (KeyPressedWaitHandle.HelpPressed.WaitDescriptor.IsMatch(pressedKeyArgs)) 

                    return BpcNextActivityKeys.Help; 

            } 

 

 

            return context.DefaultNextActivityKey; 

        } 

 

 

        #endregion 

 

 

        /// <summary> 

        /// Нужно ли молчать? 

        /// </summary> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        private bool NeedSilent(ActivityParameterDictionary parameters) 

        { 

            var silentOnSlave = parameters.GetParamValue("SilentOnSlave", false); 

            var silent = parameters.GetParamValue("Silent", false); 

 

 

            // молчать нужно, если  

            return 

                // SilentOnSlave = true и это подчиненный сканер  

                silentOnSlave && _syncManager.ScannerRole == ScannerRole.Slave 

                // или молчать нужно всегда (Silent = true) 

                || silent; 

        } 

 

 

        /// <summary> 

        /// Нужно ли игнорировать нажатия кнопок? 

        /// </summary> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        private bool NeedIgnoreButtons(ActivityParameterDictionary parameters) 

        { 

            var ignoreButtonsOnSlave = parameters.GetParamValue("IgnoreButtonsOnSlave", false); 

 

 

            // игнорировать нажатия кнопок нужно, если параметр IgnoreButtonsOnSlave = true и это подчиненный сканер 


            return ignoreButtonsOnSlave && _syncManager.ScannerRole == ScannerRole.Slave; 

        } 

 

 

        /// <summary> 

		/// Выводит текст на индикатор и ожидает заданные события 

		/// </summary> 

        /// <remarks> 

        /// Параметры: 

        ///     TextFormat, Parameters  - аналогично SayPhrase 

        ///     WaitedEvents            - ожидаемое событие 

        ///     NextActivityKeyResolver  - метод, который вычисляет ключ след. действия 

        /// </remarks> 

		/// <param name="context">Контекст воркфлоу</param> 

		/// <param name="parameters">Параметры активности</param> 

		/// <returns>ключ следующего действия</returns> 

		private NextActivityKey WaitEvents(WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

		{ 

			// ожидаемые события 

			var waitedEvents = parameters.GetParamValueAsArray<WaitHandle>("WaitedEvents"); 

            var nextActivityKeyResolver = GetNextActivityKeyResolver(parameters); 

 

 

			// текст индикатора 

			var composer = new PhraseComposer(_soundManager); 

			composer.TextFormat = parameters.GetParamValue<string>("TextFormat"); 

			composer.Parameters = parameters.GetParamValueAsArray("Parameters"); 

			var text = composer.ComposeText(); 

            int occurredEventIndex; 

 

 

			// ждем событий, звуки не воспроизводим, ждем бесконечно 

			return SayAndWaitEvents( 

                context,  

                text, null,  

                true, false,  

                waitedEvents, 

                true, Timeout.Infinite, 

                nextActivityKeyResolver, out occurredEventIndex); 

		} 

 

 

        /// <summary> 

        /// Воспроизвести фразу и ожидать заданных событий 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <param name="waitedEvent"></param> 

        /// <param name="nextActivityKeyResolver"></param> 

        /// <returns></returns> 


        private NextActivityKey SayAndWaitEvents( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters, 

            WaitHandle[] waitedEvents, 

            NextActivityKeyResolver nextActivityKeyResolver) 

        { 

            var composer = CreatePhraseComposer(parameters); 

            var text = composer.ComposeText(); 

            var silent = NeedSilent(parameters); 

            var sounds = silent ? new string[] { } : composer.ComposePhrase(); 

            var ignoreButtons = NeedIgnoreButtons(parameters); 

            var sayFirstTime = parameters.GetParamValue("SayFirstTime", true); 

            int timeout = GetTimeout(parameters); 

            int occurredEventIndex; 

 

 

            return SayAndWaitEvents( 

                context, 

                text, sounds, silent, ignoreButtons, 

                waitedEvents, sayFirstTime, timeout, 

                nextActivityKeyResolver, out occurredEventIndex); 

        } 

 

 

        /// <summary> 

        /// Возвращает таймаут, заданный в параметре RepeatTimeout 

        /// </summary> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        private int GetTimeout(ActivityParameterDictionary parameters) 

        { 

            var paramValue = parameters.GetParamValue<string>("RepeatTimeout"); 

 

 

            if (string.IsNullOrEmpty(paramValue)) 

                return DEFAULT_REPEAT_TIMEOUT; 

 

 

            if (paramValue == "Infinite") 

                return Timeout.Infinite; 

 

 

            try 

            { 

                return Convert.ToInt32(TimeSpan.Parse(paramValue).TotalMilliseconds); 

            } 

            catch (Exception ex) 

            { 

                throw new Exception("Некорректное значение параметра 'RepeatTimeout': " + paramValue, ex); 

            } 

        } 


 
 

        /// <summary> 

        /// Воспроизвести фразу и ожидать заданных событий 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="text"></param> 

        /// <param name="sounds"></param> 

        /// <param name="silent"></param> 

        /// <param name="ignoreButtons"> 

        /// нужно ли игнорировать события типа KeyPressingWaitDescriptor из коллекции waitedEvents 

        /// </param> 

        /// <param name="waitedEvents"></param> 

        /// <param name="sayFirstTime">нужно ли воспроизвести фразу в первый раз</param> 

        /// <param name="repeatTimeout"> 

        /// таймаут, по истечение которого фраза воспроизведется еще раз автоматически 

        /// </param> 

        /// <param name="nextActivityKeyResolver"></param> 

        /// <returns></returns> 

        private NextActivityKey SayAndWaitEvents( 

            WorkflowExecutionContext context, 

            string text, string[] sounds, bool silent, bool ignoreButtons, 

            WaitHandle[] waitedEvents, bool sayFirstTime, int repeatTimeout, 

            NextActivityKeyResolver nextActivityKeyResolver, out int occurredEventIndex) 

        { 

            // отфильтруем события, которых нужно ждать 

            var filteredWaitedEvents = new List<WaitHandle>(waitedEvents.Length + 1); 

            foreach (var currentEvent in waitedEvents) 

            { 

                // если нужно игнорировать нажатия кнопок и 

                // текущее событие - это событие по ожиданию нажатия кнопок 

                if (ignoreButtons && currentEvent is KeyPressedWaitHandle) 

                    // то пропускаем его 

                    continue; 

 

 

                filteredWaitedEvents.Add(currentEvent); 

            } 

            // получим индекс последнего события 

            int lastWaitedEventsIndex = filteredWaitedEvents.Count - 1; 

 

 

            // воспроизведение фразы и ожидание событий 

 

 

            // получим массив событий для ожидания во время воспроизведения фразы 

            var playSoundFinishedEvent = new ManualResetEvent(false); 

            filteredWaitedEvents.Add(playSoundFinishedEvent); 

            var waitedEventsDuringPlaying = filteredWaitedEvents.ToArray(); 

 


 
            // получим массив событий для ожидания после воспроизведения фразы 

            filteredWaitedEvents[filteredWaitedEvents.Count - 1] = KeyPressedWaitHandle.HelpPressed; 

            var waitedEventsAfterPlaying = filteredWaitedEvents.ToArray(); 

 

 

            // воспроизводим фразу в цикле 

            while (true) 

            { 

                // если нужно воспроизвести фразу в первый раз (и в очередной тоже) 

                if (sayFirstTime) 

                { 

                    // если есть, что выводить на индикатор 

                    if (text.Length > 0) 

                    { 

                        _logger.LogInfo(Message.WorkflowText, text); 

                        // установим текст на индикаторе 

                        _scannerManager.SetIndicator(text); 

                    } 

 

 

                    // если не нужно молчать 

                    if (!silent) 

                    { 

                        // сбрасываем события 

                        ResetEvents(waitedEventsDuringPlaying); 

 

 

                        // запускаем воспроизведение фразы 

                        _soundManager.PlaySounds(sounds, 

                            new EventHandler((sender, e) => { playSoundFinishedEvent.Set(); })); 

 

 

                        try 

                        { 

                            // начинаем ждать события (в том числе, событие о завершении воспроизведения) 

                            occurredEventIndex = context.WaitAny(waitedEventsDuringPlaying); 

 

 

                            // если случилось одно из ожидаемых событий 

                            if (occurredEventIndex <= lastWaitedEventsIndex) 

                            { 

                                // прерываем воспроизведение 

                                _soundManager.StopPlaying(); 

                                // возвращаем соотв. ключ след. действия 

                                return nextActivityKeyResolver( 

                                    waitedEventsDuringPlaying[occurredEventIndex], context); 

                            } 

                            // воспроизведение фразы закончилось 

                        } 


                        catch (ActivityExecutionInterruptException ex) 

                        { 

                            // прерываем воспроизведение 

                            _soundManager.StopPlaying(); 

 

 

                            throw ex; 

                        } 

                    } 

                } 

                else 

                    // выставим в true, чтобы на след. итерации в любом случае воспроизвести фразу 

                    sayFirstTime = true; 

 

 

                // ждем события (в том числе, нажатие кнопки Помощь) в течение заданного таймаута 

                ResetEvents(waitedEventsAfterPlaying); 

                occurredEventIndex = context.WaitAny(waitedEventsAfterPlaying, repeatTimeout); 

 

 

                // если случилось одно из ожидаемых событий 

                if (occurredEventIndex <= lastWaitedEventsIndex) 

                    // возвращаем соотв. ключ след. действия 

                    return nextActivityKeyResolver( 

                        waitedEventsDuringPlaying[occurredEventIndex], context); 

 

 

                // иначе: нажали кнопку Помощь или это timeout => идем на след. итерацию 

            } 

        } 

 

 

        /// <summary> 

        /// Сбразывает события 

        /// </summary> 

        /// <param name="events"></param> 

        private void ResetEvents(WaitHandle[] events) 

        { 

            foreach (var ev in events) 

            { 

				if (ev is EventWaitHandle) 

					((EventWaitHandle)ev).Reset(); 

				else if (ev is TimeoutWaitHandle) 

					((TimeoutWaitHandle)ev).Reset(); 

				else if (ev is EventWaitHandleEx) 

					((EventWaitHandleEx)ev).Reset(); 

            } 

        } 

 

 


        /// <summary> 

        /// Создает составителя фразы, инициализируя его неободимыми параметрами,  

        /// значения которых ищет в параметрах действия 

        /// </summary> 

        /// <remarks> 

        /// Выполняет поиск параметров: 

        /// TextFormat 

        /// PhraseFormat 

        /// Sounds          - список путей к звук. файлам; элементы списка должны быть разделены символом ';' 

        /// Parameters 

        /// </remarks> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        private PhraseComposer CreatePhraseComposer(ActivityParameterDictionary parameters) 

        { 

            var composer = new PhraseComposer(_soundManager); 

 

 

            composer.TextFormat = parameters.GetParamValue("TextFormat", string.Empty); 

            composer.PhraseFormat = parameters.GetParamValue("PhraseFormat", "{s*}"); 

            composer.Sounds = parameters.GetParamValueAsArray<string>("Sounds"); 

            composer.Parameters = parameters.GetParamValueAsArray("Parameters"); 

 

 

            return composer; 

        } 

 

 

        #endregion 

 

 

        #endregion 

 

 

        #region Сообщение об ошибках 

 

 

 

 

 

 

        /// <summary> 

        /// Сбросить счетчики ошибок 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey ResetErrorCounters( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 


            // сбросим счетчики ошибок 

            _workflowManager.ResetErrorCounters(); 

 

 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Воспроизводит фразу-сообщение об ошибке 

        /// </summary> 

        /// <remarks> 

        /// При выполнении действия:  

        /// - воспроизводится фраза-сообщение об ошибке 

        /// - увеличивается счетчик перезапусков на 1 для данной ошибки 

        /// - если счетчик меньше или равен MaxRestartCount, то выполняется перезапуск, иначе,  

        ///     дополнительно воспроизводится фраза «Обратитесь в службу технической поддержки», 

        ///     после чего выполнение зацикливается в данном действие навсегда 

        /// Параметры: 

        ///     TextFormat, PhraseFormat, Parameters, Sounds - аналогично SayPhrase 

        ///     ErrorId         - идентификатор ошибки 

		///     MaxRestartCount - максимальное кол-во перезапусков (по умолчанию = 5) с типом выхода ExitType 

		///     SecondMaxRestartCount - максимальное кол-во перезапусков (по умолчанию = 5) с типом выхода SecondMaxRestartCount 

        ///     ExitType        - тип выхода: Exit/RestartApplication/RebootOperationSystem  

        ///                         (по умолчанию = RestartApplication) 

		///     SecondExitType  - второй тип выхода, если текущий счетчик ошибок между MaxRestartCount SecondMaxRestartCount 

		///							используется SecondExitType(по умолчанию = RestartApplication) 

        /// </remarks> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey ReportError(WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            var errorId = parameters.GetParamValueOrThrow<string>("ErrorId"); 

            var maxRestartCount = parameters.GetParamValue("MaxRestartCount", 5); 

			var secondMaxRestartCount = parameters.GetParamValue("SecondMaxRestartCount", 5); 

 

 

            // увеличим счетчик ошибки 

            var newErrorCount = _workflowManager.IncreaseErrorCounter(errorId); 

 

 

            // если счетчик данной ошибки еще не превысил первого максимального значения 

            if (newErrorCount <= maxRestartCount) 

            { 

				return SayPhraseAndExit(context, parameters, "ExitType"); 

            } 

			// если счетчик еще не превысил второго максимального значения 

            else if (newErrorCount <= secondMaxRestartCount + maxRestartCount) 

			{ 


				return SayPhraseAndExit(context, parameters, "SecondExitType"); 

			} 

			else 

			{ 

				// то дополняем сообщение фразой "Обратитесь в службу технической поддержки" 

				var composer = CreatePhraseComposer(parameters); 

				var text = composer.ComposeText(); 

				var sounds = composer.ComposePhrase(new[] { "contact_technical_support" }); 

                int occurredEventIndex; 

 

 

				// говорим сообщение и бесконечно ждем 

				return SayAndWaitEvents( 

					context, 

					text, sounds, false, false, 

					new WaitHandle[] { }, true, Timeout.Infinite, 

                    null, out occurredEventIndex); 

			} 

        } 

 

 

		/// <summary> 

		/// Сообщает об ошибки и выходит из приложения 

		/// </summary> 

		/// <param name="context">контекст выполнения</param> 

		/// <param name="parameters">парвметры</param> 

        /// <param name="exitTypeParamName">имя параметра с типом выхода</param> 

		/// <returns></returns> 

		private NextActivityKey SayPhraseAndExit( 

            WorkflowExecutionContext context,  

            ActivityParameterDictionary parameters,  

            string exitTypeParamName) 

		{ 

			// говорим сообщение об ошибке 

			SayPhrase(context, parameters); 

 

 

			// выходим 

            var exitType = parameters.GetParamValue(exitTypeParamName, ApplicationExitType.RestartApplication); 

			CoreApplication.Instance.Exit(exitType); 

 

 

			context.Sleep(Timeout.Infinite); 

			return context.DefaultNextActivityKey; 

		} 

 

 

        #endregion 

 

 


        #region Перезапуск 

 

 

        /// <summary> 

        /// Перезапуск приложения 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey RestartApplication( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            CoreApplication.Instance.Exit(ApplicationExitType.RestartApplication); 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Перезагрузка операционной системы 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey RestartOperationSystem( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            CoreApplication.Instance.Exit(ApplicationExitType.RebootOperationSystem); 

            return context.DefaultNextActivityKey; 

        } 

 

 

        #endregion 

    } 

}


