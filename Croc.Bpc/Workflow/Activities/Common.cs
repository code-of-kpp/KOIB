using System; 
using System.Collections.Generic; 
using System.Threading; 
using Croc.Bpc.Sound; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Keyboard; 
using Croc.Bpc.Synchronization; 
using Croc.Bpc.Voting; 
using Croc.Core; 
using Croc.Core.Extensions; 
using Croc.Core.Utils.Threading; 
using Croc.Workflow.ComponentModel; 
using System.Text; 
namespace Croc.Bpc.Workflow.Activities 
{ 
    [Serializable] 
    public class CommonActivity : BpcCompositeActivity 
    { 
        #region Событие "Начинается вывод информации" 
        public event EventHandler<InfoOutputEventArgs> InfoOutputStarting; 
        private void RaiseOutputStarting(InfoType infoType) 
        { 
            InfoOutputStarting.RaiseEvent(this, new InfoOutputEventArgs(infoType)); 
        } 
        #endregion 
        #region Константы 
        public const string SYNCHRONIZATION_INDICATOR_TEXT = "Синхронизация..."; 
        #endregion 
        #region Свойства 
        public DateTime DateTimeNow 
        { 
            get 
            { 
                return DateTime.Now; 
            } 
        } 
        public DateTime LocalDateTimeNow 
        { 
            get 
            { 
                return _electionManager.SourceData.LocalTimeNow; 
            } 
        } 
        #endregion 
        #region Общие методы 
        public NextActivityKey IsMasterScanner( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _syncManager.ScannerRole == ScannerRole.Master 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey SetIndicator( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            RaiseOutputStarting(InfoType.Information); 
            var composer = CreatePhraseComposer(parameters, true); 
            var text = composer.ComposeText(); 
            _scannerManager.SetIndicator(text); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey NeedPortableVoting( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _electionManager.SourceData.VotingModeExists(VotingMode.Portable) 
                ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey SyncWorkflowState( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _workflowManager.SyncState(); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey IsSourceDataLoaded 
            (WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _electionManager.HasSourceData() 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey HasSourceDataChanged( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _electionManager.HasSourceDataChanged 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        #endregion 
        #region Воспроизведение фраз 
        #region Воспроизведение фраз и считывание значение 
        public NextActivityKey SayAndReadValue( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var okEvent = parameters.GetParamValue("OK", (WaitHandle)KeyPressedWaitHandle.YesPressed); 
            var cancelEvent = parameters.GetParamValue("Cancel", (WaitHandle)KeyPressedWaitHandle.NoPressed); 
            return SayAndReadValue(context, parameters, okEvent, cancelEvent); 
        } 
        public NextActivityKey SayAndReadValueCancelIsBack( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var okEvent = parameters.GetParamValue("OK", (WaitHandle)KeyPressedWaitHandle.YesPressed); 
            var cancelEvent = parameters.GetParamValue("Cancel", (WaitHandle)KeyPressedWaitHandle.GoBackPressed); 
            return SayAndReadValue(context, parameters, okEvent, cancelEvent); 
        } 
        public static string LastReadedValue; 
        private NextActivityKey SayAndReadValue( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters, 
            WaitHandle okEvent, WaitHandle cancelEvent) 
        { 
            RaiseOutputStarting(InfoType.Question); 
            var silent = NeedSilent(parameters); 
            var composer = CreatePhraseComposer(parameters, silent); 
            var text = composer.ComposeText(); 
            LastReadedValue = parameters.GetParamValue<object>("Value") == null 
                                  ? null 
                                  : parameters.GetParamValue<long>("Value").ToString(); 
            var passwordChar = parameters.GetParamValue("PasswordChar", ReadValueContext.NULL_PASSWORD_CHAR); 
            var readValueMode = parameters.GetParamValue("ReadValueMode", ReadValueMode.CutLeadingZero); 
            var helpComposer = CreateHelpPhraseComposer(parameters); 
            var helpSounds = helpComposer.ComposePhrase(); 
            var readValueContext = new ReadValueContext( 
                context, _scannerManager.IndicatorLength, text, LastReadedValue, passwordChar, readValueMode); 
            var readValueThread = new Thread(ReadValueMethod); 
            var required = parameters.GetParamValue("Required", false); 
            try 
            { 
                readValueThread.Start(readValueContext); 
                var sounds = silent ? new string[] { } : composer.ComposePhrase(); 
                var repeatTimeout = GetRepeatTimeout(parameters); 
                var startTimeout = GetStartTimeout(parameters); 


                var nextActivityKeyResolver = GetNextActivityKeyResolver(parameters); 
                while (true) 
                { 
                    int occurredEventIndex; 
                    var result = SayAndWaitEvents( 
                        context, "", sounds, null, helpSounds, 
                        silent, false, new[] { okEvent, cancelEvent }, 
                        startTimeout, repeatTimeout, nextActivityKeyResolver, out occurredEventIndex); 
                    if (occurredEventIndex == 0) 
                    { 
                        if (required && string.IsNullOrEmpty(readValueContext.Value)) 
                            continue; 
                        LastReadedValue = readValueContext.Value; 
                    } 
                    return result; 
                } 
            } 
            finally 
            { 
                readValueContext.StopReadingEvent.Set(); 
                readValueThread.Join(TimeSpan.FromSeconds(1)); 
            } 
        } 
        private void ReadValueMethod(object state) 
        { 
            const int BLINK_TIMEOUT = 500; 
            var readValueContext = (ReadValueContext) state; 
            var underscoreDisplayed = true; 
            _logger.LogInfo(Message.WorkflowText, readValueContext.TextWithUnderscores); 
            try 
            { 
                while (true) 
                { 
                    var text = underscoreDisplayed 
                                   ? readValueContext.TextWithUnderscores 
                                   : readValueContext.TextWithoutUnderscores; 
                    _scannerManager.SetIndicator(text); 
                    KeyPressedWaitHandle keyWaitHandle; 
                    int timeout; 
                    if (readValueContext.TextWithUnderscores.IndexOf('_') != -1) 
                    { 
                        if (string.CompareOrdinal( 
                            readValueContext.TextWithUnderscores, readValueContext.OriginalText) == 0) 
                        { 
                            keyWaitHandle = KeyPressedWaitHandle.DigitPressed; 
                            timeout = BLINK_TIMEOUT; 
                        } 
                        else 
                        { 
                            keyWaitHandle = KeyPressedWaitHandle.DigitOrDeletePressed; 
                            timeout = BLINK_TIMEOUT; 
                        } 
                    } 
                    else 
                    { 
                        keyWaitHandle = KeyPressedWaitHandle.DeletePressed; 
                        timeout = Timeout.Infinite; 
                    } 
                    keyWaitHandle.Reset(); 
                    var index = readValueContext.Context.WaitAny( 
                        new WaitHandle[] {readValueContext.StopReadingEvent, keyWaitHandle}, timeout); 
                    if (index == WaitHandle.WaitTimeout) 
                    { 
                        underscoreDisplayed = !underscoreDisplayed; 
                        continue; 
                    } 
                    if (index == 0) 
                        return; 
                    var keyArgs = keyWaitHandle.PressedKeyArgs; 
                    var oldValue = readValueContext.Value; 


                    switch (keyArgs.Type) 
                    { 
                        case KeyType.Digit: 
                            try 
                            { 
                                if (readValueContext.ReadValueMode == ReadValueMode.CutLeadingZero) 
                                    readValueContext.Value = 
                                        !string.IsNullOrEmpty(oldValue) 
                                            ? (int.Parse(oldValue)*10 + keyArgs.Value).ToString() 
                                            : keyArgs.Value.ToString(); 
                                else 
                                    readValueContext.Value += keyArgs.Value; 
                            } 
                            catch (OverflowException) 
                            { 
                            } 
                            break; 
                        case KeyType.Delete: 
                            readValueContext.Value = 
                                string.IsNullOrEmpty(oldValue) || oldValue.Length == 1 
                                    ? null 
                                    : oldValue.Substring(0, oldValue.Length - 1); 
                            break; 
                    } 
                    _logger.LogInfo(Message.WorkflowText, readValueContext.TextWithUnderscores); 
                } 
            } 
            catch (ActivityExecutionInterruptException) 
            { 
                return; 
            } 
        } 
        private enum ReadValueMode 
        { 
            CutLeadingZero, 
            WithLeadingZero 
        } 
        private class ReadValueContext 
        { 
            public readonly ManualResetEvent StopReadingEvent = new ManualResetEvent(false); 
            public readonly WorkflowExecutionContext Context; 
            public readonly ReadValueMode ReadValueMode; 
            public readonly string OriginalText; 
            public string TextWithUnderscores 
            { 
                get; 
                private set; 
            } 
            public string TextWithoutUnderscores 
            { 
                get; 
                private set; 
            } 
            private volatile string _value; 
            public string Value 
            { 
                get 
                { 
                    return _value; 
                } 
                set 
                { 
                    _value = value; 
                    var sbWithUnderscores = new StringBuilder(OriginalText.Length); 
                    var sbWithoutUnderscores = new StringBuilder(OriginalText.Length); 
                    var valueCharArr = string.IsNullOrEmpty(_value) ? new char[0] : _value.ToCharArray(); 
                    var digitIndex = -1; 
                    foreach (var ch in OriginalText) 
                    { 
                        if (ch == '_') 
                        { 
                            if (++digitIndex < valueCharArr.Length) 
                            { 
                                var digitCh = _passwordChar == NULL_PASSWORD_CHAR 
                                                  ? valueCharArr[digitIndex] 
                                                  : _passwordChar; 
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
            public const char NULL_PASSWORD_CHAR = (char)0; 
            private readonly char _passwordChar; 
            public ReadValueContext( 
                WorkflowExecutionContext context, 
                int indicatorLength, 
                string text, 
                string value, 
                char passwordChar, 
                ReadValueMode readingMode) 
            { 
                CodeContract.Requires(context != null); 
                Context = context; 
                CheckText(indicatorLength, text); 
                OriginalText = text; 
                Value = value; 
                _passwordChar = passwordChar; 
                ReadValueMode = readingMode; 
            } 
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
        public NextActivityKey SayPhrase(WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            RaiseOutputStarting(InfoType.Information); 
            var silent = NeedSilent(parameters); 
            var composer = CreatePhraseComposer(parameters, silent); 
            var text = composer.ComposeText(); 
            if (text.Length > 0) 
            { 
                _logger.LogInfo(Message.WorkflowText, text); 
                _scannerManager.SetIndicator(text); 
            } 
            if (silent) 
                return context.DefaultNextActivityKey; 
            var playSoundFinishedEvent = new AutoResetEvent(false); 
            var sounds = composer.ComposePhrase(); 
            _soundManager.PlaySounds(sounds, (sender, e) => playSoundFinishedEvent.Set()); 
            try 
            { 
                KeyPressedWaitHandle.YesOrNoPressed.Reset(); 
                var index = context.WaitAny( 
                    new WaitHandle[] {KeyPressedWaitHandle.YesOrNoPressed, playSoundFinishedEvent}); 
                if (index == 0) 
                { 
                    _soundManager.StopPlaying(); 
                    _logger.LogVerbose(Message.WorkflowSoundPlayingStoppedByYesOrNoPressed); 
                } 
            } 
            catch (ActivityExecutionInterruptException) 
            { 
                _soundManager.StopPlaying(); 
                _logger.LogVerbose(Message.WorkflowSoundPlayingStoppedByActivityExecutionInterrupt); 
                throw; 
            } 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey SayAndWaitEvents( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var waitedEvents = GetWaitedEvents(parameters); 
            var nextActivityKeyResolver = GetNextActivityKeyResolver(parameters); 
            return SayAndWaitEvents(context, parameters, waitedEvents, nextActivityKeyResolver); 
        } 
        public NextActivityKey SayAndWaitYes( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return SayAndWaitEvents(context, parameters, 
                new[] { KeyPressedWaitHandle.YesPressed }, DefaultNextActivityKeyResolver); 
        } 
        public NextActivityKey SayAndWaitNo( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return SayAndWaitEvents(context, parameters, 
                new[] { KeyPressedWaitHandle.NoPressed }, DefaultNextActivityKeyResolver); 
        } 
        public NextActivityKey SayAndWaitYesOrNo( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return SayAndWaitEvents(context, parameters, 
                new[] { KeyPressedWaitHandle.YesOrNoPressed }, DefaultNextActivityKeyResolver); 
        } 
        public NextActivityKey SayAndWaitYesAndNoAtOnce( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            SayAndWaitEvents(context, parameters, 
                new[] { KeyPressedWaitHandle.YesAndNoAtOncePressed }, DefaultNextActivityKeyResolver); 
            return BpcNextActivityKeys.YesAndNo; 
        } 
        public NextActivityKey SayAndWaitYesOrNoOrBack( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return SayAndWaitEvents(context, parameters, 
                new[] { KeyPressedWaitHandle.YesOrNoOrBackPressed }, DefaultNextActivityKeyResolver); 
        } 
        public NextActivityKey SayAndWaitHelp( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return SayAndWaitEvents(context, parameters, 
                new[] { KeyPressedWaitHandle.HelpPressed }, DefaultNextActivityKeyResolver); 
        } 
        public NextActivityKey SayAndSleep( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            AddInfiniteRepeatTimeoutParameter(parameters); 
            return SayAndWaitEvents(context, parameters, new WaitHandle[] { }, DefaultNextActivityKeyResolver); 
        } 
        public NextActivityKey SayAndWaitTimeout( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var timeout = parameters.GetParamValueOrThrow<int>("Timeout"); 
            var timeoutEvent = new TimeoutWaitHandle(timeout); 
            AddInfiniteRepeatTimeoutParameter(parameters); 


            return SayAndWaitEvents( 
                context, parameters, new WaitHandle[] { timeoutEvent }, DefaultNextActivityKeyResolver); 
        } 
        public NextActivityKey WaitEvents( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var waitedEvents = GetWaitedEvents(parameters); 
            var nextActivityKeyResolver = GetNextActivityKeyResolver(parameters); 
            var composer = CreatePhraseComposer(parameters, true); 
            var text = composer.ComposeText(); 
            int occurredEventIndex; 
            return SayAndWaitEvents( 
                context, 
                text, null, null, null, 
                true, false, 
                waitedEvents, 
                0, Timeout.Infinite, 
                nextActivityKeyResolver, out occurredEventIndex); 
        } 


        #endregion 
        #region Private-методы, реализующие воспроизведение фраз и ожидание событий 
        #region Вычисление ключа след. действия 
        public delegate NextActivityKey NextActivityKeyResolver( 
            WaitHandle occurredEvent, WorkflowExecutionContext context); 
        private static NextActivityKeyResolver GetNextActivityKeyResolver(ActivityParameterDictionary parameters) 
        { 
            return parameters.GetParamValue("NextActivityKeyResolver", 
                new NextActivityKeyResolver(DefaultNextActivityKeyResolver)); 
        } 
        private static NextActivityKey DefaultNextActivityKeyResolver( 
            WaitHandle occurredEvent, WorkflowExecutionContext context) 
        { 
            var keyWh = occurredEvent as KeyPressedWaitHandle; 
            if (keyWh != null) 
            { 
                if (keyWh.Equals(KeyPressedWaitHandle.YesOrNoOrBackPressed) || 
                    keyWh.Equals(KeyPressedWaitHandle.YesOrNoPressed)) 
                { 
                    var pressedKeyArgs = keyWh.PressedKeyArgs; 
                    if (KeyPressedWaitHandle.YesPressed.WaitDescriptor.IsMatch(pressedKeyArgs)) 
                        return BpcNextActivityKeys.Yes; 
                    if (KeyPressedWaitHandle.NoPressed.WaitDescriptor.IsMatch(pressedKeyArgs)) 
                        return BpcNextActivityKeys.No; 
                    if (KeyPressedWaitHandle.GoBackPressed.WaitDescriptor.IsMatch(pressedKeyArgs)) 
                        return BpcNextActivityKeys.Back; 
                } 
                if (keyWh.Equals(KeyPressedWaitHandle.YesPressed)) 
                    return BpcNextActivityKeys.Yes; 
                if (keyWh.Equals(KeyPressedWaitHandle.NoPressed)) 
                    return BpcNextActivityKeys.No; 
                if (keyWh.Equals(KeyPressedWaitHandle.GoBackPressed)) 
                    return BpcNextActivityKeys.Back; 
                if (keyWh.Equals(KeyPressedWaitHandle.HelpPressed)) 
                    return BpcNextActivityKeys.Help; 
                if (keyWh.Equals(KeyPressedWaitHandle.YesAndNoAtOncePressed)) 
                    return BpcNextActivityKeys.YesAndNo; 
                if (keyWh.Equals(KeyPressedWaitHandle.HelpAndNoAtOncePressed)) 
                    return BpcNextActivityKeys.HelpAndNo; 
            } 
            return context.DefaultNextActivityKey; 
        } 
        #endregion 
        #region Получение параметров воспроизведения фразы 
        private static WaitHandle[] GetWaitedEvents(ActivityParameterDictionary parameters) 
        { 
            return parameters.GetParamValueAsArray<WaitHandle>("WaitedEvents"); 
        } 
        private bool NeedSilent(ActivityParameterDictionary parameters) 
        { 
            var silentOnSlave = parameters.GetParamValue("SilentOnSlave", false); 
            var silent = parameters.GetParamValue("Silent", false); 
            return 
                silentOnSlave && _syncManager.ScannerRole == ScannerRole.Slave 
                || silent; 
        } 
        private bool NeedIgnoreButtons(ActivityParameterDictionary parameters) 
        { 
            var ignoreButtonsOnSlave = parameters.GetParamValue("IgnoreButtonsOnSlave", false); 
            return ignoreButtonsOnSlave && _syncManager.ScannerRole == ScannerRole.Slave; 
        } 
        private const int DEFAULT_REPEAT_TIMEOUT = 30000; 
        private static int GetRepeatTimeout(ActivityParameterDictionary parameters) 
        { 
            return GetTimeout(parameters, "RepeatTimeout", DEFAULT_REPEAT_TIMEOUT); 
        } 
        private static int GetStartTimeout(ActivityParameterDictionary parameters) 
        { 
            return GetTimeout(parameters, "StartTimeout", 0); 
        } 
        private static int GetTimeout( 
            ActivityParameterDictionary parameters, string parameterName, int defaultValue) 
        { 
            var paramValue = parameters.GetParamValue<string>(parameterName); 
            if (string.IsNullOrEmpty(paramValue)) 
                return defaultValue; 
            if (paramValue == "Infinite") 
                return Timeout.Infinite; 
            try 
            { 
                var timeout = Convert.ToInt32(TimeSpan.Parse(paramValue).TotalMilliseconds); 
                if (timeout > 0) 
                    return timeout; 
                throw new ApplicationException(string.Format("Параметр '{0}' должен быть больше 0", parameterName)); 
            } 
            catch (Exception ex) 
            { 
                throw new Exception( 
                    string.Format("Некорректное значение параметра '{0}': {1}", parameterName, paramValue), ex); 
            } 
        } 
        private static void AddInfiniteRepeatTimeoutParameter(ActivityParameterDictionary parameters) 
        { 
            parameters.Add(new ActivityParameter("RepeatTimeout", "Infinite")); 
        } 
        #endregion 
        #region Создание составителя фразы 
        private PhraseComposer CreatePhraseComposer( 
            ActivityParameterDictionary parameters, bool forComposeTextOnly) 
        { 
            var composer = new PhraseComposer(_soundManager) 
            { 
                TextFormat = parameters.GetParamValue("TextFormat", string.Empty), 
                Parameters = parameters.GetParamValueAsArray("Parameters") 
            }; 
            if (!forComposeTextOnly) 
            { 
                composer.PhraseFormat = parameters.GetParamValue("PhraseFormat", "{s*}"); 
                composer.Sounds = parameters.GetParamValueAsArray<string>("Sounds"); 
            } 
            return composer; 
        } 
        private PhraseComposer CreateHelpPhraseComposer(ActivityParameterDictionary parameters) 
        { 
            var composer = new PhraseComposer(_soundManager) 
            { 
                TextFormat = parameters.GetParamValue("HelpTextFormat", string.Empty), 
                Parameters = parameters.GetParamValueAsArray("HelpParameters"), 
                PhraseFormat = parameters.GetParamValue("HelpPhraseFormat", "{s*}"), 
                Sounds = parameters.GetParamValueAsArray("HelpSounds", new string[0]) 
            }; 
            return composer; 
        } 
        #endregion 
        #region Реализация воспроизведения фразы и ожидания событий 
        private NextActivityKey SayAndWaitEvents( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters, 
            ICollection<WaitHandle> waitedEvents, 
            NextActivityKeyResolver nextActivityKeyResolver) 
        { 
            var silent = NeedSilent(parameters); 
            var composer = CreatePhraseComposer(parameters, silent); 
            var text = composer.ComposeText(); 
            var sounds = silent ? new string[] { } : composer.ComposePhrase(); 
            var helpComposer = CreateHelpPhraseComposer(parameters); 
            var helpText = helpComposer.ComposeText(); 
            var helpSounds = helpComposer.ComposePhrase(); 
            var ignoreButtons = NeedIgnoreButtons(parameters); 
            var startTimeout = GetStartTimeout(parameters); 
            var repeatTimeout = GetRepeatTimeout(parameters); 
            int occurredEventIndex; 
            return SayAndWaitEvents( 
                context, 
                text, sounds, helpText, helpSounds, 
                silent, ignoreButtons, 
                waitedEvents, startTimeout, repeatTimeout, 
                nextActivityKeyResolver, out occurredEventIndex); 
        } 
        private NextActivityKey SayAndWaitEvents( 
            WorkflowExecutionContext context, 
            string text, string[] sounds, string helpText, string[] helpSounds, 
            bool silent, bool ignoreButtons, 
            ICollection<WaitHandle> waitedEvents, int startTimeout, int repeatTimeout, 
            NextActivityKeyResolver nextActivityKeyResolver, out int occurredEventIndex) 
        { 
            var filteredWaitedEvents = new List<WaitHandle>(waitedEvents.Count + 1); 
            var needWaitKeyPressed = false; 
            var needWaitHelpPressed = false; 
            foreach (var currentEvent in waitedEvents) 
            { 
                var keyWh = currentEvent as KeyPressedWaitHandle; 
                if (keyWh != null) 
                { 
                    needWaitKeyPressed = true; 
                    if (keyWh.Equals(KeyPressedWaitHandle.HelpPressed)) 
                    { 
                        needWaitHelpPressed = true; 
                    } 
                    else if (ignoreButtons) 
                    { 
                        continue; 
                    } 
                } 
                filteredWaitedEvents.Add(currentEvent); 
            } 
            RaiseOutputStarting(needWaitKeyPressed ? InfoType.Question : InfoType.Information); 
            var lastWaitedEventsIndex = filteredWaitedEvents.Count - 1; 
            var playSoundFinishedEvent = new ManualResetEvent(false); 
            filteredWaitedEvents.Add(playSoundFinishedEvent); 
            var waitedEventsDuringPlaying = filteredWaitedEvents.ToArray(); 
            if (!needWaitHelpPressed) 
            { 
                filteredWaitedEvents[filteredWaitedEvents.Count - 1] = KeyPressedWaitHandle.HelpPressed; 
            } 
            else 
            { 
                filteredWaitedEvents.RemoveAt(filteredWaitedEvents.Count - 1); 
            } 
            var waitedEventsAfterPlaying = filteredWaitedEvents.ToArray(); 
            var helpButtonPressed = false; 
            if (startTimeout > 0 || startTimeout == Timeout.Infinite) 
            { 
                ResetEvents(waitedEventsAfterPlaying); 
                occurredEventIndex = context.WaitAny(waitedEventsAfterPlaying, startTimeout); 
                if (0 <= occurredEventIndex && occurredEventIndex <= lastWaitedEventsIndex) 
                    return nextActivityKeyResolver(waitedEventsAfterPlaying[occurredEventIndex], context); 
                helpButtonPressed = (occurredEventIndex == waitedEventsAfterPlaying.Length - 1); 
            } 
            while (true) 
            { 
                if (helpButtonPressed && helpSounds != null && helpSounds.Length > 0) 
                    occurredEventIndex = SetIndicatorAndPlayPhrase( 
                        context, waitedEventsDuringPlaying, playSoundFinishedEvent, 
                        helpText ?? text, helpSounds, false); 
                else 
                    occurredEventIndex = SetIndicatorAndPlayPhrase( 
                        context, waitedEventsDuringPlaying, playSoundFinishedEvent, 
                        text, sounds, silent); 
                if (occurredEventIndex >= 0) 
                    return nextActivityKeyResolver(waitedEventsDuringPlaying[occurredEventIndex], context); 
                ResetEvents(waitedEventsAfterPlaying); 
                occurredEventIndex = context.WaitAny(waitedEventsAfterPlaying, repeatTimeout); 
                if (0 <= occurredEventIndex && occurredEventIndex <= lastWaitedEventsIndex) 
                    return nextActivityKeyResolver(waitedEventsAfterPlaying[occurredEventIndex], context); 
                helpButtonPressed = (occurredEventIndex == waitedEventsAfterPlaying.Length - 1); 
            } 
        } 
        private int SetIndicatorAndPlayPhrase( 
            IWaitController waitController, 
            WaitHandle[] waitedEventsDuringPlaying, 
            EventWaitHandle playSoundFinishedEvent, 
            string text, string[] sounds, bool silent) 
        { 
            if (!string.IsNullOrEmpty(text)) 
            { 
                _logger.LogInfo(Message.WorkflowText, text); 
                _scannerManager.SetIndicator(text); 
            } 
            var occurredEventIndex = -1; 
            if (!silent) 
            { 
                ResetEvents(waitedEventsDuringPlaying); 
                _soundManager.PlaySounds(sounds, (sender, e) => playSoundFinishedEvent.Set()); 
                try 
                { 
                    occurredEventIndex = waitController.WaitAny(waitedEventsDuringPlaying); 
                    if (occurredEventIndex <= waitedEventsDuringPlaying.Length - 2) 
                    { 
                        _soundManager.StopPlaying(); 
                        return occurredEventIndex; 
                    } 
                    occurredEventIndex = -1; 
                } 
                catch (ActivityExecutionInterruptException) 
                { 
                    _soundManager.StopPlaying(); 
                    _logger.LogVerbose(Message.WorkflowSoundPlayingStoppedByActivityExecutionInterrupt); 
                    throw; 
                } 
            } 
            return occurredEventIndex; 
        } 
        private static void ResetEvents(IEnumerable<WaitHandle> events) 
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
        #endregion 
        #endregion 
        #endregion 
        #region Сообщение об ошибках 
        public NextActivityKey ResetErrorCounters( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _workflowManager.ResetErrorCounters(); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey ReportError(WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var errorId = parameters.GetParamValueOrThrow<string>("ErrorId"); 
            var maxRestartCount = parameters.GetParamValue("MaxRestartCount", 5); 
            var secondMaxRestartCount = parameters.GetParamValue("SecondMaxRestartCount", 5); 
            RaiseOutputStarting(InfoType.Warning); 
            var newErrorCount = _workflowManager.IncreaseErrorCounter(errorId); 
            if (newErrorCount <= maxRestartCount) 
            { 
                return SayPhraseAndExit(context, parameters, "ExitType"); 
            } 
            if (newErrorCount <= secondMaxRestartCount + maxRestartCount) 
            { 
                return SayPhraseAndExit(context, parameters, "SecondExitType"); 
            } 
            var composer = CreatePhraseComposer(parameters, false); 
            var text = composer.ComposeText(); 
            var sounds = composer.ComposePhrase(new[] { "contact_technical_support" }); 
            var helpComposer = CreateHelpPhraseComposer(parameters); 
            var helpText = helpComposer.ComposeText(); 
            var helpSounds = helpComposer.ComposePhrase();  


            int occurredEventIndex; 
            return SayAndWaitEvents( 
                context, 
                text, sounds, helpText, helpSounds, 
                false, false, 
                new WaitHandle[] {}, 0, Timeout.Infinite, 
                null, out occurredEventIndex); 
        } 
        private NextActivityKey SayPhraseAndExit( 
            WorkflowExecutionContext context,  
            ActivityParameterDictionary parameters,  
            string exitTypeParamName) 
        { 
            SayPhrase(context, parameters); 
            var exitType = parameters.GetParamValue(exitTypeParamName, ApplicationExitType.RestartApplication); 
            CoreApplication.Instance.Exit(exitType); 
            context.Sleep(Timeout.Infinite); 
            return context.DefaultNextActivityKey; 
        } 
        #endregion 
        #region Перезапуск 
        public NextActivityKey RestartApplication( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            CoreApplication.Instance.Exit(ApplicationExitType.RestartApplication); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey RestartOperationSystem( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            CoreApplication.Instance.Exit(ApplicationExitType.RebootOperationSystem); 
            return context.DefaultNextActivityKey; 
        } 
        #endregion 
    } 
}
