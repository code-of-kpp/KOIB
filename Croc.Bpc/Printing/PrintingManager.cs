using System; 

using System.Collections.Specialized; 

using System.Diagnostics; 

using System.Text.RegularExpressions; 

using System.Threading; 

using Croc.Bpc.Common; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Bpc.Election; 

using Croc.Bpc.Printing.Config; 

using Croc.Bpc.Printing.Reports; 

using Croc.Bpc.Scanner; 

using Croc.Core; 

using Croc.Core.Configuration; 

using Croc.Core.Utils; 

 

 

namespace Croc.Bpc.Printing 

{ 

    /// <summary> 

    /// Менеджер печати 

    /// </summary> 

    [SubsystemConfigurationElementTypeAttribute(typeof(PrintingManagerConfig))] 

    public class PrintingManager : Subsystem, IPrintingManager 

    { 

        /// <summary> 

        /// Конфиг менеджера 

        /// </summary> 

        private PrintingManagerConfig _config; 

        /// <summary> 

        /// Менеджер выборов 

        /// </summary> 

        private IElectionManager _electionManager; 

		/// <summary> 

		/// Менеджер сканера 

		/// </summary> 

		private IScannerManager _scannerManager; 

 

 

		/// <summary> 

		/// Имя найденного принтера 

		/// </summary> 

		private string _findPrinterName = null; 

 

 

        /// <summary> 

        /// Инициализация 

        /// </summary> 

        /// <param name="config">конфиг</param> 

        public override void Init(SubsystemConfig config) 

        { 


            _config = (PrintingManagerConfig)config; 

 

 

            // получаем ссылки на другие подсистемы 

            _electionManager = Application.GetSubsystemOrThrow<IElectionManager>(); 

			_scannerManager = Application.GetSubsystemOrThrow<IScannerManager>(); 

		} 

 

 

        /// <summary> 

        /// Получение нового конфига 

        /// </summary> 

        /// <param name="newConfig">новый конфиг</param> 

        public override void ApplyNewConfig(SubsystemConfig newConfig) 

        { 

            // инициализируемся с новым конфигом 

            Init(newConfig); 

        } 

 

 

        #region IPrintingManager members 

 

 

        /// <summary> 

        /// Конфиг отчетов 

        /// </summary> 

        public ReportConfig ReportConfig 

        { 

            get 

            { 

                return _config.Report; 

            } 

        } 

 

 

        /// <summary> 

        /// Найти принтер и проверить, что он готов к работе 

        /// </summary> 

        /// <returns>true - принтер найден, false - принтер не найден</returns> 

		public bool FindPrinter() 

		{ 

			// если не Unix всегда true 

			if (!PlatformDetector.IsUnix) 

				return true; 

 

 

			bool isPrinterExists = false; 

			// запустим процесс, который выведет все подключенные через usb принтеры 

			ProcessHelper.StartProcessAndWaitForFinished("/usr/lib/cups/backend/usb", "", 

				delegate(string line) 


				{ 

					Logger.LogInfo(Message.PrintingBackendLine, line); 

 

 

					// проверим, есть ли принтер на данном сканере 

					foreach (PrinterConfig printer in _config.Printers) 

					{ 

						// если, то что вернул /usr/lib/cups/backend/usb совпадает с каким-то принтером 

						if (Regex.Match(line, printer.SearchRegExp).Success) 

						{ 

							Logger.LogInfo(Message.PrintingFindPrinter, printer.Name); 

 

 

							_findPrinterName = printer.Name; 

							isPrinterExists = true; 

							return true; 

						} 

					} 

 

 

					return false; 

				}, 

				null 

				); 

 

 

			// если принтер на нашли 

			if (!isPrinterExists) 

				return false; 

 

 

			// если удалось разрешить принтер 

			if (EnablePrinter(_findPrinterName)) 

				return true; 

 

 

			return false; 

		} 

 

 

        /// <summary> 

        /// Печать отчета 

        /// </summary> 

        /// <param name="reportType">тип отчета</param> 

        /// <returns>true - печать выполнена, false - ошибка печати</returns> 

        public bool PrintReport(ReportType reportType, ListDictionary reportParameters) 

        { 

            try 

            { 

				// сформируем отчет и пошлем его на печать 


                var rb = new ReportBuilder(this, _electionManager); 

 

 

				PrinterJob printerJob; 

				// попробуем построить отчет 

				try 

				{ 

					printerJob = rb.BuildReport(reportType, reportParameters); 

				} 

				catch (Exception ex) 

				{ 

					throw new Exception("Ошибка при построении отчета", ex); 

				} 

 

 

				// непосредственно печать 

				PrintPDF(printerJob); 

 

 

                return true; 

            } 

            catch (Exception ex) 

            { 

				// логирование 

                Logger.LogException(Message.PrintingException, ex, reportType); 

				return false; 

            } 

        } 

 

 

        #endregion 

 

 

        /// <summary> 

        ///	Печатает документ PDF 

        /// </summary> 

        /// <param name="JobFileName">Имя файла с документом в формате PDF</param> 

		private void PrintPDF(PrinterJob printerJob) 

		{ 

			Logger.LogVerbose(Message.DebugVerbose, "call"); 

 

 

			if (PlatformDetector.IsUnix) 

			{ 

                // попробуем освободить память 

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced); 

 

 

                ProcessHelper.ExecCommand(_config.Commands.BeforePrinting); 

 


 
                try 

                { 

                    string LP_ARGUMENTS_FORMAT_STR = 

                        "-o Resolution=300dpi -o Quality=Draft -o PrintQuality=Draft -o ColorModel=Black -n 1 -d {0} " + 

                        (_config.PrintByPage.Value ? "-P {1} " : "") + "{2}"; 

 

 

                    var process = new Process(); 

                    process.StartInfo.FileName = "lp"; 

                    process.StartInfo.CreateNoWindow = true; 

                    process.StartInfo.UseShellExecute = false; 

                    process.StartInfo.EnvironmentVariables["LANG"] = "en"; 

                    process.StartInfo.EnvironmentVariables["LC_CTYPE"] = "en_US.iso885915"; 

 

 

					// выведем на индикатор Печать... 

					_scannerManager.SetIndicator("Печать..."); 

 

 

					// по всем страницам 

                    for (int pageNum = 1; (pageNum <= (_config.PrintByPage.Value ? printerJob.PageCont : 1)); pageNum++) 

                    { 

                        // отправляем по одной странице 

                        process.StartInfo.Arguments = 

                            string.Format(LP_ARGUMENTS_FORMAT_STR, _findPrinterName, pageNum, printerJob.FilePath); 

 

 

                        // сообщим о том, что у нас печатается очередная страница: 

                        Logger.LogInfo(Message.PrintingStartPagePrint, pageNum, printerJob.FilePath); 

 

 

                        try 

                        { 

                            //Запускаем процесс 

                            process.Start(); 

                        } 

                        catch (Exception ex) 

                        { 

                            Logger.LogException(Message.PrintingException, ex, printerJob.FilePath); 

                        } 

                        // будем ждать окончания постановки в очередь на печать 

                        process.WaitForExit(); 

 

 

                        // заснем на 5 сек 

                        Thread.Sleep(5000); 

 

 

                        // будем ждать окончания печати принтера 


                        while (!IsPrinterIdle(_findPrinterName)) 

                        { 

                            // подождем 15 сек, чтобы не мешать печати 

                            Thread.Sleep(15000); 

                        } 

                    } 

                } 

                finally 

                { 

                    ProcessHelper.ExecCommand(_config.Commands.AfterPrinting); 

                } 

			} 

			else 

			{ 

                // для винды печать - это просто открытие сформированного PDF 

                var process = new Process(); 

                process.StartInfo.FileName = printerJob.FilePath; 

                process.StartInfo.CreateNoWindow = true; 

                process.StartInfo.UseShellExecute = true; 

                process.Start(); 

			} 

		} 

 

 

		/// <summary> 

		/// Свободен ли принтер 

		/// </summary> 

		/// <returns></returns> 

		private bool IsPrinterIdle(string printerName) 

		{ 

			//сигнатура которую будем искать  

			const string idleSignature = " is idle."; 

			const string nowPrintingSignature = "now printing"; 

			bool printerIsIdle = false; 

 

 

			// создадим процесс lpstat 

            var startInfo = new ProcessStartInfo("lpstat", "-p " + printerName) 

			{ 

				CreateNoWindow = true, 

				UseShellExecute = false, 

				RedirectStandardOutput = true, 

				RedirectStandardError = true 

			}; 

			// нужно установить из-за русской локализации 

			startInfo.EnvironmentVariables["LC_ALL"] = "C"; 

 

 

			var process = Process.Start(startInfo); 

 


 
			// запустим процесс 

			ProcessHelper.StartProcessAndWaitForFinished(process, 

				delegate(string line) 

				{ 

					// если это не наш принтер 

					if (line.IndexOf(printerName) == -1) 

						return true; 

 

 

					// если есть нужные данные 

					if (line.IndexOf(idleSignature) != -1) 

					{ 

						// все отлично выходим 

						printerIsIdle = true; 

						return true; 

					} 

					if (line.IndexOf(nowPrintingSignature) != -1) 

					{ 

						return true; 

					} 

 

 

					return false; 

				}, null 

			); 

 

 

			return printerIsIdle; 

		} 

 

 

		/// <summary> 

		/// Переводит принтер в состояние online для Unix 

		/// </summary> 

		/// <param name="printerName">имя принтера</param> 

		/// <returns>результат</returns> 

		private bool EnablePrinter(string printerName) 

		{ 

			// попробуем разрешить принтер 

			try 

			{ 

				if(ProcessHelper.StartProcessAndWaitForFinished("cupsenable", printerName, null, null) != 0) 

					return false; 

				if(ProcessHelper.StartProcessAndWaitForFinished("/usr/sbin/accept", printerName, null, null) != 0) 

					return false; 

 

 

				return true; 

			} 


			catch (Exception ex) 

			{ 

				Logger.LogException(Message.PrinterEnablingError, ex); 

				return false; 

			} 

		} 

    } 

}


