using System; 
using System.Collections.Generic; 
using System.Configuration; 
using System.Diagnostics; 
using System.Linq; 
using System.Reflection; 
using System.Threading; 
using Croc.Core.Configuration; 
using Croc.Core.Diagnostics; 
using Croc.Core.Diagnostics.Default; 
using Croc.Core.Extensions; 
using Croc.Core.Utils; 
using Croc.Core.Utils.Threading; 
namespace Croc.Core 
{ 
    public class CoreApplication : ICoreApplication 
    { 
        public static ICoreApplication Instance 
        { 
            get; 
            private set; 
        } 
        public string Name 
        { 
            get; 
            private set; 
        } 
        #region Инициализация 
        public CoreApplication() 
        { 
            Instance = this; 
            try 
            { 
                var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None); 
                Config = (ApplicationConfig)exeConfig.GetSection(ApplicationConfig.SECTION_NAME); 
                if (Config == null) 
                    throw new Exception("Секция не найдена: " + ApplicationConfig.SECTION_NAME); 
            } 
            catch (Exception ex) 
            { 
                throw new ConfigurationErrorsException("Ошибка получения конфигурации приложения", ex); 
            } 
            Name = string.IsNullOrEmpty(Config.Name) ? Guid.NewGuid().ToString() : Config.Name; 
            InitLogger(); 
            CreateSubsystems(); 
        } 
        private void InitLogger() 
        { 
            LogFileFolder = string.IsNullOrEmpty(Config.LogFileFolder) 
                ? Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) 
                : Config.LogFileFolder; 
            TraceLevel = GetTraceLevelByName(Config.TraceLevelName, TraceLevel.Error); 
            EventDispatcher.Init(Config.DiagnosticsConfig); 
            FileWriter.Init(LogFileFolder); 
            _logger = (Logger)CreateLogger(Name, TraceLevel); 
        } 
        private void CreateSubsystems() 
        { 
            if (Config.Subsystems.Count == 0) 
                throw new ConfigurationErrorsException("Список подсистем пуст"); 
            foreach (SubsystemConfig subsystemConfig in Config.Subsystems) 
            { 
                Subsystem subsystem; 
                var subsystemName = subsystemConfig.SubsystemName; 
                try 
                { 
                    var type = Type.GetType(subsystemConfig.SubsystemTypeName, true); 
                    subsystem = (Subsystem)Activator.CreateInstance(type); 
                    subsystem.Name = subsystemName; 
                    AddSubsystem(subsystemName, subsystem); 
                    subsystem.TraceLevel = GetTraceLevelByName(subsystemConfig.TraceLevelName, TraceLevel); 
                    subsystem.LogFileFolder = string.IsNullOrEmpty(subsystemConfig.LogFileFolder) 
                        ? LogFileFolder 
                        : LogFileFolder + "/" + subsystemConfig.LogFileFolder; 
                    subsystem.SeparateLog = subsystemConfig.SeparateLog; 
                    subsystem.DisposeOrder = subsystemConfig.DisposeOrder; 
                    subsystem.ConfigUpdated += SubsystemConfigUpdated; 
                    Logger.LogVerbose("Создана подсистема {0}", subsystemName); 
                } 
                catch (Exception ex) 
                { 
                    Logger.LogException("Ошибка создания подсистемы {0}: {1}", ex, subsystemName, ex.Message); 
                    throw new Exception("Ошибка создания подсистемы " + subsystemName, ex); 
                } 
            } 
            foreach (SubsystemConfig subsystemConfig in Config.Subsystems) 
            { 
                var subsystemName = subsystemConfig.SubsystemName; 
                try 
                { 
                    var subsystem = GetSubsystem(subsystemName); 
                    subsystem.Init(subsystemConfig); 
                    Logger.LogVerbose("Выполнена инициализация подсистемы {0}", subsystemName); 
                } 
                catch (Exception ex) 
                { 
                    Logger.LogException("Ошибка инициализации подсистемы {0}: {1}", ex, subsystemName, ex.Message); 
                    throw new Exception("Ошибка инициализации подсистемы " + subsystemName, ex); 
                } 
            } 
        } 
        private static TraceLevel GetTraceLevelByName(string traceLevelName, TraceLevel defaultTraceLevel) 
        { 
            if (string.IsNullOrEmpty(traceLevelName)) 
                return defaultTraceLevel; 
            try 
            { 
                return (TraceLevel)Enum.Parse(typeof(TraceLevel), traceLevelName); 
            } 
            catch 
            { 
                throw new Exception(string.Format("Некорректно задан уровень трассировки: '{0}'", traceLevelName)); 
            } 
        } 
        #endregion 
        #region Конфигурация 
        private void SubsystemConfigUpdated(object sender, ConfigUpdatedEventArgs e) 
        { 
            var subsystem = (ISubsystem)sender; 
            subsystem.ApplyNewConfig(Config.Subsystems[subsystem.Name]); 
            ConfigUpdated.RaiseEvent(this, e); 
        } 
        public ApplicationConfig Config 
        { 
            get; 
            private set; 
        } 
        public bool ApplyNewConfig(ApplicationConfig newConfig, bool force) 
        { 
            CodeContract.Requires(newConfig != null); 
            if (!force && Config.Equals(newConfig)) 
                return false; 
            Logger.LogVerbose("Применение новой конфигурации..."); 
            Config = newConfig; 
            foreach (SubsystemConfig subsystemConfig in Config.Subsystems) 
            { 
                var subsystemName = subsystemConfig.SubsystemName; 
                try 
                { 
                    var subsystem = GetSubsystem(subsystemName); 
                    subsystem.ApplyNewConfig(subsystemConfig); 
                    Logger.LogVerbose("Выполнена переинициализация подсистемы {0}", subsystemName); 
                } 
                catch (Exception ex) 
                { 
                    Logger.LogException("Ошибка переинициализации подсистемы {0}: {1}", ex, subsystemName, ex.Message); 
                    throw new Exception("Ошибка переинициализации подсистемы " + subsystemName, ex); 
                } 
            } 
            return true; 
        } 
        public event EventHandler<ConfigUpdatedEventArgs> ConfigUpdated; 
        #endregion 
        #region Логирование 
        public TraceLevel TraceLevel 
        { 
            get; 
            private set; 
        } 
        public string LogFileFolder 
        { 
            get; 
            private set; 
        } 
        Logger _logger; 
        public ILogger Logger 
        { 
            get { return _logger; } 
        } 
        public ILogger CreateLogger(string loggerName, TraceLevel traceLevel) 
        { 
            var logger = new Logger(loggerName, new TraceLevelFilter(traceLevel), _logger, _loggerEnabled); 
            foreach (var filter in EventDispatcher.EventFilters) 
            { 
                logger.AddFilter(filter); 
            } 
            return logger; 
        } 
        private readonly ManualResetEvent _loggerEnabled = new ManualResetEvent(true); 
        public bool LoggerEnabled 
        { 
            get 
            { 
                return _loggerEnabled.WaitOne(0); 
            } 
            set 
            { 
                if (value) 
                    _loggerEnabled.Set(); 
                else 
                    _loggerEnabled.Reset(); 
            } 
        } 
        #endregion 
        #region Подсистемы 
        private readonly List<KeyValuePair<String, ISubsystem>> _subsystems = 
            new List<KeyValuePair<string, ISubsystem>>(); 
        public T FindSubsystemImplementsInterface<T>() 
        { 
            return (T)_subsystems.FirstOrDefault(i => i.Value is T).Value; 
        } 
        public T FindSubsystemImplementsInterfaceOrThrow<T>() 
        { 
            var res = FindSubsystemImplementsInterface<T>(); 
            if (res == null) 
                throw new ArgumentException("Приложение не содержит подсистемы, реализующей интерфейс " + typeof(T).FullName); 
            return res; 
        } 
        public IEnumerable<T> FindAllSubsystemsImplementsInterface<T>() 
        { 
            return _subsystems.Where(i => i.Value is T).Select(i => (T) i.Value); 
        } 
        public T GetSubsystemOrThrow<T>() where T : ISubsystem 
        { 
            return GetSubsystemOrThrow<T>("Приложение не содержит подсистемы " + typeof(T).FullName); 
        } 
        public T GetSubsystemOrThrow<T>(string errorMsg) where T : ISubsystem 
        { 
            var subsystem = GetSubsystem<T>(); 
            if (subsystem == null) 
                throw new ArgumentException(errorMsg); 
            return subsystem; 
        } 
        public IEnumerable<ISubsystem> Subsystems 
        { 
            get { return _subsystems.Select(i => i.Value); } 
        } 
        public void AddSubsystem(String name, ISubsystem subsystem) 
        { 
            CodeContract.Requires(subsystem != null); 
            foreach (var item in _subsystems) 
                if (!String.IsNullOrEmpty(name) && item.Key == name) 
                    throw new ArgumentException( 
                        "Приложение уже содержит подсистему с наименованием " + name); 
            _subsystems.Add(new KeyValuePair<String, ISubsystem>(name, subsystem)); 
            subsystem.Application = this; 
        } 
        public ISubsystem GetSubsystem(String name) 
        { 
            CodeContract.Requires(!string.IsNullOrEmpty(name)); 
            foreach (var item in _subsystems) 
                if (item.Key == name) 
                    return item.Value; 
            return null; 
        } 
        public T GetSubsystem<T>(String name) where T : ISubsystem 
        { 
            CodeContract.Requires(!string.IsNullOrEmpty(name)); 
            foreach (var item in _subsystems) 
                if (item.Key == name) 
                { 
                    if (!(item.Value is T)) 
                        throw new ArgumentException( 
                            "Запрошенная подсистема '" + name + "' не реализует требуемый интерфейс " +  
                            typeof(T).FullName); 
                    return (T)item.Value; 
                } 
            return default(T); 
        } 
        public T GetSubsystem<T>() where T : ISubsystem 
        { 
            var foundSubsystems = GetSubsystems<T>(); 
            if (foundSubsystems.Count > 1) 
                throw new InvalidOperationException( 
                    string.Format("Найдено более одной подсистемы типа {0}", typeof(T).Name)); 
            if (foundSubsystems.Count == 0) 
                return default(T); 
            return foundSubsystems[0].Value; 
        } 
        public List<KeyValuePair<String, T>> GetSubsystems<T>() where T : ISubsystem 
        { 
            var subsystemsReq = new List<KeyValuePair<string, T>>(); 
            foreach (var item in _subsystems) 
            { 
                if (item.Value is T) 
                    subsystemsReq.Add(new KeyValuePair<String, T>(item.Key, (T)item.Value)); 
            } 
            return subsystemsReq; 
        } 
        #endregion 
        #region Версия приложения 
        public Version ApplicationVersion 
        { 
            get 
            { 
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) 
                { 
                    if (assembly.EntryPoint == null) 
                        continue; 
                    if (assembly.EntryPoint.Name == "Main" && !assembly.GetName().Name.StartsWith("vshost")) 
                        return assembly.GetName().Version; 
                } 
                return Assembly.GetExecutingAssembly().GetName().Version; 
            } 
        } 
        #endregion 
        #region Завершение работы 
        private static readonly object s_exitSync = new object(); 
        private Thread _exitThread; 
        protected ManualResetEvent _exitEvent = new ManualResetEvent(false); 
        public WaitHandle ExitEvent 
        { 
            get 
            { 
                return _exitEvent; 
            } 
        } 
        public event EventHandler<ApplicationExitEventArgs> Exited; 
        public void WaitForExit() 
        { 
            _exitEvent.WaitOne(); 
        } 
        public void Exit(ApplicationExitType exitType) 
        { 
            lock (s_exitSync) 
            { 
                if (_exitThread == null) 
                { 
                    _exitThread = new Thread(ExitThread); 
                    _exitThread.Start(exitType); 
                } 
                WaitForExit(); 
                Thread.Sleep(3000); 
            } 
        } 
        private void ExitThread(object state) 
        { 
            Logger.LogVerbose("Завершение работы приложения..."); 
            var sortedList = new SortedList<int, List<ISubsystem>>(_subsystems.Count); 
            foreach (var pair in _subsystems) 
            { 
                var subsystem = pair.Value; 
                if (sortedList.ContainsKey(subsystem.DisposeOrder)) 
                { 
                    sortedList[subsystem.DisposeOrder].Add(subsystem); 
                } 
                else 
                { 
                    sortedList.Add(subsystem.DisposeOrder, new List<ISubsystem> {subsystem}); 
                } 
            } 
            foreach (var pair in sortedList) 
                foreach (var subsystem in pair.Value) 
                { 
                    Logger.LogVerbose(string.Format("Останов подсистемы '{0}'...", subsystem.Name)); 
                    Disposer.DisposeObject(subsystem); 
                } 
            Thread.Sleep(1000); 
            foreach (var pair in sortedList) 
                foreach (var subsystem in pair.Value) 
                { 
                    Logger.LogVerbose(string.Format("Останов журналирования подсистемы '{0}'...", subsystem.Name)); 
                    subsystem.DisposeLogger(); 
                } 
            var exitType = state == null ? "не задан" : ((ApplicationExitType) state).ToString(); 
            Logger.LogInfo("Завершение работы приложения (тип выхода {0})", exitType); 
            Thread.Sleep(1000); 
            Disposer.DisposeObject(Logger); 
            FileWriter.Close(); 
            _exitEvent.Set(); 
            Thread.Sleep(1000); 
            if (state == null) 
                return; 
            Exited.RaiseEvent(this, new ApplicationExitEventArgs((ApplicationExitType)state)); 
            var exitCode = (int) state; 
            LoggingUtils.LogToConsole("exit with code: {0}", exitCode); 
            Environment.Exit(exitCode); 
        } 
        #endregion 
    } 
}
