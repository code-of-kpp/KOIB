using System; 

using System.CodeDom.Compiler; 

using System.Reflection; 

using System.Text; 

using Croc.Core.Utils; 

using System.IO; 

 

 

namespace Croc.Bpc.Common 

{ 

	/// <summary> 

	/// Класс помошник в создании сборки для вычисления выражений 

	/// для КС и выражений автоматически вычисляемых строк протокола 

	/// </summary> 

	public static class DynamicAssemblyHelper 

	{ 

		/// <summary> 

		///		Максимально допустимая длина строки 

		/// </summary> 

		public const int MAX_STRING_LENGTH = 2045;	 

 

 

		/// <summary> 

		/// Компилирует код на C# в сборку в памяти 

		/// </summary> 

		/// <param name="sSource">Исходный текст сборки</param> 

		/// <param name="referencedAssemblies">Сборки приложения, которые нужно подтянуть</param> 

		public static Assembly Compile(string sSource, string[] referencedAssemblies) 

		{ 

			// настроим компилятор 

			Microsoft.CSharp.CSharpCodeProvider comp = new Microsoft.CSharp.CSharpCodeProvider(); 

			CompilerParameters cp = new CompilerParameters(); 

 

 

			// необходимо отделить префикс "file:///" от фактического пути 

			if (!PlatformDetector.IsUnix) 

			{ 

				// для виндовс отрезаем все 3 слэша 

				cp.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "")); 

			} 

			else 

			{ 

				// для юникс отрезаем только 2 слэша 

				cp.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().CodeBase.Replace("file://", "")); 

			} 

			cp.GenerateExecutable = false; 

			cp.GenerateInMemory = true; 

 

 

			// добавим необходимые сборки 


			foreach (var assembly in referencedAssemblies) 

			{ 

 

 

				cp.ReferencedAssemblies.Add(Path.Combine( 

					AppDomain.CurrentDomain.BaseDirectory 

					, assembly + ".dll")); 

			} 

 

 

			// попытаемся откомпилировать 

			CompilerResults cr = comp.CompileAssemblyFromSource(cp, sSource); 

 

 

			// проверим на ошибки 

			if (cr.Errors.HasErrors) 

			{ 

				// строка с описанием ошибки 

				StringBuilder sError = new StringBuilder(); 

				sError.Append("Error Compiling Expression: "); 

 

 

				// дописываю тексты всех ошибок компилятора 

				foreach (CompilerError err in cr.Errors) 

					sError.AppendFormat("{0}\n", err.ErrorText); 

 

 

				throw new ApplicationException("Ошибка компиляции выражения: " + sError.ToString() + "\n" + sSource); 

			} 

 

 

			return cr.CompiledAssembly; 

		} 

 

 

		/// <summary> 

		///		Разбивает строку на несколько, если ее длина превышает 2046 символов.  

		///		Для разбиения используется символ переноса строки. 

		///		Используется для динамически компилируемых сборок, в которых  

		///		длина строки не может превышать 2 Kb (2046 символов) 

		/// </summary> 

		/// <param name="sIn">Входная строка</param> 

		/// <returns> 

		///		string - Возвращает входную строку, разбитую на несколько символами переноса строки. 

		/// </returns> 

		public static string SplitStringByLength(string sIn) 

		{ 

			StringBuilder sRes = new StringBuilder();		// результат 

 

 


			// если длина больше 2046, то надо разбить на строки 

			// пока не закончилась строка 

			int nLastPos = 0;	// последняя позиция 

			for (int nCurPos = MAX_STRING_LENGTH; sIn.Length > nCurPos; nCurPos += MAX_STRING_LENGTH) 

			{ 

				int nFind = sIn.LastIndexOf(" ", nCurPos - 1); 

				if (-1 < nFind) 

				{ 

					sRes.Append(sIn.Substring(nLastPos, nFind - nLastPos) + Environment.NewLine); 

					nLastPos = nFind; 

					nCurPos = nFind + 1; 

				} 

			} 

			sRes.Append(sIn.Substring(nLastPos)); 

 

 

			return sRes.ToString(); 

		} 

	} 

}


