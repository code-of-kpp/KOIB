using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using System.Threading; 
using Croc.Core; 
using Croc.Core.Utils.Threading; 
namespace Croc.Bpc.Keyboard 
{ 
    public class KeyPressingWaitDescriptor 
    { 
        public readonly KeyType[] Keys; 
        public readonly bool AllKeysPressed; 
        private static volatile bool s_ignoreKeys; 
        public KeyPressingWaitDescriptor(KeyType key) 
            : this(new[] { key }, false) 
        { 
        } 
        public KeyPressingWaitDescriptor(KeyType[] keys, bool allKeysPressed) 
        { 
            CodeContract.Requires(keys != null && keys.Length > 0); 
            Keys = keys; 
            AllKeysPressed = allKeysPressed; 
        } 
        #region Проверка события нажатия кнопки на соответствие ожидаемому событию 
        private readonly List<KeyType> _matchedKeys = new List<KeyType>(); 
        private DateTime _lastKeyPressedTime = DateTime.MaxValue; 
        private static readonly TimeSpan s_maxKeysPressingInterval = TimeSpan.FromMilliseconds(500); 
        private void ResetMatchedKeys() 
        { 
            _matchedKeys.Clear(); 
            _lastKeyPressedTime = DateTime.MaxValue; 
        } 
        public bool IsMatch(KeyEventArgs keyEventArgs) 
        { 
            if (// игнорируем нажатия клавишь 
                s_ignoreKeys || 
                keyEventArgs == null || 
                !Keys.Contains(keyEventArgs.Type)) 
            { 
                if (AllKeysPressed) 
                    ResetMatchedKeys(); 
                return false; 
            } 
            if (!AllKeysPressed) 
                return true; 
            var now = DateTime.Now; 
            if (now - _lastKeyPressedTime < s_maxKeysPressingInterval) 
            { 
                if (!_matchedKeys.Contains(keyEventArgs.Type)) 
                    _matchedKeys.Add(keyEventArgs.Type); 
                _lastKeyPressedTime = now; 
                if (_matchedKeys.Count == Keys.Length) 
                { 
                    s_ignoreKeys = true; 
                    ThreadUtils.StartBackgroundThread( 
                        () => 
                        { 
                            Thread.Sleep(TimeSpan.FromSeconds(1)); 
                            s_ignoreKeys = false; 
                        }); 
                    return true; 
                } 
            } 
            else 
            { 
                ResetMatchedKeys(); 
                _matchedKeys.Add(keyEventArgs.Type); 
                _lastKeyPressedTime = now; 
            } 
            return false; 
        } 
        #endregion 
        public override string ToString() 
        { 
            if (Keys == null || Keys.Length == 0) 
                return ""; 
            var sb = new StringBuilder(); 
            foreach (var key in Keys) 
            { 
                sb.Append(key); 
                sb.Append(AllKeysPressed ? " и " : " или "); 
            } 
            if (sb.Length > 0) 
                sb.Length -= AllKeysPressed ? 3 : 5; 
            return sb.ToString(); 
        } 
    } 
}
