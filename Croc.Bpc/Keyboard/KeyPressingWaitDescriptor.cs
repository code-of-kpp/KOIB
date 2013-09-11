using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Bpc.Keyboard 

{ 

    /// <summary> 

    /// Дескриптор того, нажатия каких клавиш ожидается 

    /// </summary> 

    public class KeyPressingWaitDescriptor 

    { 

        /// <summary> 

        /// Клавиши, нажатие которых ожидается 

        /// </summary> 

        public readonly KeyType[] Keys; 

        /// <summary> 

        /// Признак того, что ожидается нажатие всех клавиш 

        /// </summary> 

        public bool AllKeysPressed; 

 

 

        /// <summary> 

        /// Конструктор: ожидание нажатия одной конкретной клавиши 

        /// </summary> 

        /// <param name="key"></param> 

        public KeyPressingWaitDescriptor(KeyType key) 

            : this(new[] { key }, false) 

        { 

        } 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="keys">Клавиши, нажатие которых ожидается</param> 

        /// <param name="allKeysPressed">Признак того, что ожидается нажатие всех клавиш</param> 

        public KeyPressingWaitDescriptor(KeyType[] keys, bool allKeysPressed) 

        { 

            CodeContract.Requires(keys != null); 

            CodeContract.Requires(keys.Length > 0); 

 

 

            Keys = keys; 

            AllKeysPressed = allKeysPressed; 

        } 

 

 

        #region Проверка события нажатия кнопки на соответствие ожидаемому событию 


 
 

        /// <summary> 

        /// Список клавиш, которые совпали с ожидаемыми 

        /// </summary> 

        /// <remarks> 

        /// Используется, когда allKeysPressed = true 

        /// </remarks> 

        private List<KeyType> _matchedKeys = new List<KeyType>(); 

        /// <summary> 

        /// Время, когда последний раз была нажата ожидаемая клавиша 

        /// </summary> 

        private DateTime _lastKeyPressedTime = DateTime.MaxValue; 

        /// <summary> 

        /// Максимальный временной промежуток, который может пройти между нажатиями клавиш, 

        /// одновременное нажатие которых ожидается 

        /// </summary> 

        private static TimeSpan s_maxKeysPressingInterval = TimeSpan.FromMilliseconds(500); 

 

 

        /// <summary> 

        /// Сбрасывает информацию о совпавших ожидаемых клавишах 

        /// </summary> 

        private void ResetMatchedKeys() 

        { 

            _matchedKeys.Clear(); 

            _lastKeyPressedTime = DateTime.MaxValue; 

        } 

 

 

        /// <summary> 

        /// Проверяет, соответствует ли заданное UI-событие описанию 

        /// </summary> 

        /// <param name="uiEvent"></param> 

        /// <returns></returns> 

        public bool IsMatch(KeyEventArgs keyEventArgs) 

        { 

            // если аргументы не заданы или это клавиша, нажатие которой не ожидаем 

            if (keyEventArgs == null || !Keys.Contains(keyEventArgs.Type)) 

            { 

                // если ждем одновременного нажатия клавиш 

                if (AllKeysPressed) 

                    ResetMatchedKeys(); 

 

 

                return false; 

            } 

 

 

            if (!AllKeysPressed) 


                return Keys.Contains(keyEventArgs.Type); 

 

 

            // если нужно ждать одновременное нажатие всех клавиш 

            var now = DateTime.Now; 

 

 

            // если нажатая клавиша одна из тех, которые ждем 

            if (Keys.Contains(keyEventArgs.Type)) 

            { 

                // если с момента, когда была нажата предыдущая ожидаемая клавиша,  

                // прошло не более максимального временного промежутка 

                if (now - _lastKeyPressedTime < s_maxKeysPressingInterval) 

                { 

                    // добавим клавишу в список совпавших 

                    if (!_matchedKeys.Contains(keyEventArgs.Type)) 

                        _matchedKeys.Add(keyEventArgs.Type); 

 

 

                    // обновим время последнего нажатия клавиши 

                    _lastKeyPressedTime = now; 

 

 

                    // если совпали уже все ожидаемые клавиши 

                    if (_matchedKeys.Count == Keys.Length) 

                        // то ОК 

                        return true; 

                } 

                else 

                { 

                    // сбросим список совпавших клавиш 

                    ResetMatchedKeys(); 

 

 

                    // добавим клавишу в список совпавших 

                    _matchedKeys.Add(keyEventArgs.Type); 

 

 

                    // обновим время последнего нажатия клавиши 

                    _lastKeyPressedTime = now; 

                } 

            } 

            else 

                // считаем, что одновременного нажатия пока не случилось 

                ResetMatchedKeys(); 

 

 

            return false; 

        } 

 


 
        #endregion 

 

 

        /// <summary> 

        /// Возвращает строковое представление 

        /// </summary> 

        /// <returns></returns> 

        public override string ToString() 

        { 

            if (Keys == null || Keys.Length == 0) 

                return ""; 

 

 

            var sb = new StringBuilder(); 

            foreach (var key in Keys) 

            { 

                sb.Append(key); 

 

 

                if (AllKeysPressed) 

                    sb.Append(" и "); 

                else 

                    sb.Append(" или "); 

            } 

 

 

            if (sb.Length > 0) 

                sb.Length -= AllKeysPressed ? 3 : 5; 

 

 

            return sb.ToString(); 

        } 

    } 

}


