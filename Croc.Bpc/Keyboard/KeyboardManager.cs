using System; 
using System.Collections.Generic; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Keyboard.Config; 
using Croc.Core; 
using Croc.Core.Configuration; 
using Croc.Core.Extensions; 
namespace Croc.Bpc.Keyboard 
{ 
    [SubsystemConfigurationElementTypeAttribute(typeof(KeyboardManagerConfig))] 
    public class KeyboardManager : Subsystem, IKeyboardManager 
    { 
        private KeyboardManagerConfig _config; 
        private Dictionary<int, KeyConfig> _keys; 
        private IKeyboardDriver _driver; 
        private KeyConfig _lastKey = new KeyConfig(); 
        private DateTime _lastKeyTime = DateTime.MinValue; 
        public event EventHandler<KeyEventArgs> KeyPressed; 
        #region Override Subsystem 
        public override void Init(SubsystemConfig config) 
        { 
            _config = (KeyboardManagerConfig) config; 
            _keys = new Dictionary<int, KeyConfig>(_config.Keys.Count); 
            foreach (KeyConfig key in _config.Keys) 
                _keys[key.ScanCode] = key; 
            InitKeyboardDriver(_config.Driver); 
        } 
        public override void ApplyNewConfig(SubsystemConfig newConfig) 
        { 
        } 
        public override void Dispose() 
        { 
            if (_driver != null) 
            { 
                _driver.Dispose(); 
                _driver = null; 
            } 
            base.Dispose(); 
        } 
        #endregion 
        private void InitKeyboardDriver(KeyboardDriverConfig config) 
        { 
            if (string.IsNullOrEmpty(config.Type)) 
                throw new ApplicationException("Не определен тип драйвера клавиатуры"); 
            var driverType = Type.GetType(config.Type); 
            if (driverType == null) 
                throw new ApplicationException("Не найден тип драйвера клавиатуры " + config.Type); 
            if (!driverType.IsImplementInterface(typeof (IKeyboardDriver))) 
                throw new ApplicationException(String.Format( 
                    "Тип драйвера клавиатуры {0} не реализует интерфейс IKeyboardDriver", config.Type)); 
            if (_driver != null) 
            { 
                _driver.Dispose(); 
                _driver = null; 
            } 
            IKeyboardDriver newDriver; 
            try 
            { 
                newDriver = (IKeyboardDriver) Activator.CreateInstance(driverType); 
                newDriver.Init(config, Logger); 
                Logger.LogInfo(Message.KeyboardDriverCreated, config.Type); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogInfo(Message.KeyboardDriverCreationFailed, config.Type, ex); 
                throw; 
            } 
            _driver = newDriver; 
            _driver.KeyPressed += (sender, e) => OnNewDataReady(e.ScanCode, e.TimeStamp); 
            _driver.Start(); 
        } 
        protected void OnNewDataReady(int scanCode, int timeStamp) 
        { 
            var handler = KeyPressed; 
            if (handler == null || _disposed) 
                return; 
            if (_keys.ContainsKey(scanCode)) 
            { 
                var key = _keys[scanCode]; 
                if ((key.Type == KeyType.No || key.Type == KeyType.Yes) 
                    && key.Type == _lastKey.Type 
                    && DateTime.Now - _lastKeyTime < TimeSpan.FromMilliseconds(200)) 
                    return; 
                _lastKey = key; 
                _lastKeyTime = DateTime.Now; 
                Logger.LogInfo(Message.KeyboardKeyPressed, scanCode, key.Type, key.Value, timeStamp); 
                handler(this, new KeyEventArgs(scanCode, key.Type, key.Value, timeStamp)); 
            } 
            else 
            { 
                Logger.LogInfo(Message.KeyboardKeyPressed, scanCode, KeyType.Unknown, 0, timeStamp); 
                handler(this, new KeyEventArgs(scanCode, KeyType.Unknown, 0, timeStamp)); 
            } 
        } 
    } 
}
