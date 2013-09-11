using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Core.Utils.Threading; 

using Croc.Core; 

using System.Threading; 

 

 

namespace Croc.Bpc.Keyboard 

{ 

	/// <summary> 

	/// Дескриптор ожидания нажатия нужной клавиши нужного количества раз 

	/// </summary> 

	/// <remarks> 

	/// При создании объекта происходит подписывание на событие клавиатуры,  

	/// а отписывание будет выполняться только в Dispose, поэтому использовать  

	/// данный класс рекомендуется с применением конструкции:  

	/// using (var e = new SubscribingEventWaitHandle(...)) { ... } 

	/// или принудительно вызывать метод Dispose 

	/// </remarks> 

	public class RepetableKeyPressWaitHandle : EventWaitHandleEx 

	{ 

		/// <summary> 

		/// Дескриптор ожидаемых нажатий клавиш 

		/// </summary> 

		public readonly KeyPressingWaitDescriptor WaitDescriptor; 

		/// <summary> 

		/// Аргументы нажатой клавиши 

		/// </summary> 

		public KeyEventArgs PressedKeyArgs 

		{ 

			get; 

			private set; 

		} 

		/// <summary> 

		/// Количество раз, которое необходимо нажать 

		/// </summary> 

		public readonly int PressTimes; 

 

 

		/// <summary> 

		/// текущее количество нажатия 

		/// </summary> 

		private volatile int _currentPressCount; 

 

 

		/// <summary> 

		/// Объект синхронизации _currentPressCount 

		/// </summary> 


		private static object s_counterSync = new object(); 

 

 

		/// <summary> 

		/// Минимальное время между нажатьями 

		/// </summary> 

		private readonly TimeSpan maxKeysPressingInterval = TimeSpan.FromMilliseconds(500); 

 

 

		/// <summary> 

		/// Время нажатия последней клавиши 

		/// </summary> 

		private DateTime _lastPressTime = DateTime.MaxValue; 

 

 

		/// <summary> 

		/// Конструктор 

		/// </summary> 

		/// <param name="waitDescriptor">Дескриптор нажатия</param> 

		/// <param name="pressRepeats">Количество повторных нажатий, которое ждем</param> 

		public RepetableKeyPressWaitHandle(KeyPressingWaitDescriptor waitDescriptor, int pressRepeats) 

            : base(false, false) 

        { 

            CodeContract.Requires(waitDescriptor != null); 

			CodeContract.Requires(pressRepeats > 0); 

 

 

			PressTimes = pressRepeats; 

            WaitDescriptor = waitDescriptor; 

            Keyboard.KeyPressed += new EventHandler<KeyEventArgs>(Keyboard_KeyPressed); 

        } 

 

 

        /// <summary> 

        /// Обработка нажатия клавиши 

        /// </summary> 

        /// <param name="sender"></param> 

        /// <param name="e"></param> 

        private void Keyboard_KeyPressed(object sender, KeyEventArgs e) 

        { 

            if (!WaitDescriptor.IsMatch(e)) 

                return; 

 

 

			var time = DateTime.Now; 

 

 

			// если время прошло много обнулим счетчик 

			if (time.TimeOfDay - _lastPressTime.TimeOfDay > maxKeysPressingInterval) 

				_currentPressCount = 0; 


 
 

			_lastPressTime = time; 

			lock (s_counterSync) 

			{ 

				// увеличим колличество нажатий 

				_currentPressCount++; 

			} 

 

 

            PressedKeyArgs = e; 

			if (_currentPressCount == PressTimes) 

			{ 

				// запустим ожидание, если нажали лишку раз то событие не произойдет 

				var thread = new Thread(WaitForSet); 

				thread.IsBackground = true; 

				thread.Start(); 

			} 

		} 

 

 

		/// <summary> 

		/// Ожидает не нажмут ли нашу клавишу еще раз 

		/// </summary> 

		private void WaitForSet() 

		{ 

			Thread.Sleep(maxKeysPressingInterval); 

 

 

			lock(s_counterSync) 

			{ 

				if (_currentPressCount == PressTimes) 

					base.Set(); 

			} 

		} 

 

 

		/// <summary> 

		/// Объект синхронизации 

		/// </summary> 

		private static object s_sync = new object(); 

 

 

		private static IKeyboard _keyboard; 

		/// <summary> 

		/// Менеджер клавиатуры 

		/// </summary> 

		private static IKeyboard Keyboard 

		{ 

			get 


			{ 

				if (_keyboard == null) 

					lock (s_sync) 

						if (_keyboard == null) 

						{ 

							_keyboard = (IKeyboard)CoreApplication.Instance.GetSubsystem<UnionKeyboard>(); 

							if (_keyboard == null) 

								_keyboard = CoreApplication.Instance.GetSubsystemOrThrow<IKeyboard>(); 

						} 

 

 

				return _keyboard; 

			} 

		} 

	} 

}


