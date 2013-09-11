using System; 
using System.Collections.Specialized; 
using System.Diagnostics; 
using System.Text.RegularExpressions; 
using System.Threading; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Printing.Config; 
using Croc.Bpc.Printing.Reports; 
using Croc.Bpc.Utils; 
using Croc.Bpc.Voting; 
using Croc.Core; 
using Croc.Core.Configuration; 
using Croc.Core.Extensions; 
using Croc.Core.Utils; 
namespace Croc.Bpc.Printing 
{ 
    [SubsystemConfigurationElementTypeAttribute(typeof(PrintingManagerConfig))] 
    public class PrintingManager : Subsystem, IPrintingManager 
    { 
        #region private Поля 
        private PrintingManagerConfig _config; 
        private IElectionManager _electionManager; 
        private IVotingResultManager _votingResultManager; 
        private string _findPrinterName; 
        #endregion 
        #region Override Subsystem 
        public override void Init(SubsystemConfig config) 
        { 
            _config = (PrintingManagerConfig)config; 
            _electionManager = Application.GetSubsystemOrThrow<IElectionManager>(); 
            _votingResultManager = Application.GetSubsystemOrThrow<IVotingResultManager>(); 
        } 
        public override void ApplyNewConfig(SubsystemConfig newConfig) 
        { 
            Init(newConfig); 
        } 
        #endregion 
        public ReportConfig ReportConfig 
        { 
            get 
            { 
                return _config.Report; 
            } 
        } 
        public bool FindPrinter() 
        { 
            if (!PlatformDetector.IsUnix) 
                return true; 
            string printerName = FindPrinter(null); 
            if (printerName != null && EnablePrinter(printerName)) 
            { 
                _findPrinterName = printerName; 
                return true; 
            } 
            return false; 
        } 
        private string FindPrinter(string name) 
        { 
            string printerName = null; 
            const string FINDPRINTERSCOMMAND = "/usr/lib/cups/backend/usb"; 
            ProcessHelper.StartProcessAndWaitForFinished( 
                FINDPRINTERSCOMMAND, 
                "", 
                delegate(ProcessOutputProcessorState state) 
                { 
                    Logger.LogInfo(Message.PrintingBackendLine, state.Line); 
                    foreach (PrinterConfig printer in _config.Printers) 
                    { 
                        if (Regex.Match(state.Line, printer.SearchRegExp).Success) 
                        { 
                            Logger.LogInfo(Message.PrintingFindPrinter, printer.Name); 
                            if (name == null || name == printer.Name) 
                            { 
                                printerName = printer.Name; 
                                return true; 
                            } 
                        } 
                    } 
                    return false; 
                }, 
                null); 
            return printerName; 
        } 
        private bool EnablePrinter(string printerName) 
        { 
            try 
            { 
                if (ProcessHelper.StartProcessAndWaitForFinished("cupsenable", printerName, null, null) != 0) 
                    return false; 
                if (ProcessHelper.StartProcessAndWaitForFinished("/usr/sbin/accept", printerName, null, null) != 0) 
                    return false; 
                return true; 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.PrintingEnablingError, ex); 
                return false; 
            } 
        } 
        public PrinterJob CreateReport(ReportType reportType, ListDictionary reportParameters, int copies) 
        { 
            Logger.LogInfo(Message.PrintingCreateReport, reportType); 
            try 
            { 
                var rb = new ReportBuilder(this, _electionManager, _votingResultManager); 
                PrinterJob printerJob; 
                try 
                { 
                    printerJob = rb.BuildReport(reportType, reportParameters, copies); 
                } 
                catch (Exception ex) 
                { 
                    throw new Exception("Ошибка при построении отчета", ex); 
                } 
                return printerJob; 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.PrintingError, ex, reportType); 
                return null; 
            } 
        } 
        #region Печать отчета 
        public event EventHandler<PrintReportEventArgs> PrintReportStarting; 
        public event EventHandler<PrintReportEventArgs> PrintReportFinished; 
        public bool PrintReport(PrinterJob printerJob) 
        { 
            Logger.LogInfo(Message.PrintingPrintReport, printerJob.ReportType); 
            var eventArgs = new PrintReportEventArgs(printerJob); 
            try 
            { 
                PrintReportStarting.RaiseEvent(this, eventArgs); 
                ExecutePrinterJob(printerJob); 
                return true; 
            } 
            catch (ThreadAbortException) 
            { 
                return false; 
            } 
            catch (Exception ex) 
            { 
                if (ex is CupsFailureException) 
                { 
                    CancelJobs(_findPrinterName); 
                    EnablePrinter(_findPrinterName); 
                } 
                Logger.LogError(Message.PrintingError, ex, printerJob.FilePath); 
                return false; 
            } 
            finally 
            { 
                PrintReportFinished.RaiseEvent(this, eventArgs); 
            } 
        } 
        private void ExecutePrinterJob(PrinterJob printerJob) 
        { 
            Logger.LogVerbose(Message.Common_DebugCall); 
            const int WAIT_INTERVAL = 5; 
            const int WAIT_END_PRINT = 3 * 60; 
            if (PlatformDetector.IsUnix) 
            { 
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced); 
                ProcessHelper.ExecCommand(_config.Commands.BeforePrinting); 
                try 
                { 
                    const string LP_CMD_ARGS_FORMAT = 
                        "-o Resolution=300dpi -o Quality=Draft -o PrintQuality=Draft -o ColorModel=Black -n {{3}} " + 
                        "-d {{0}} {0} {{2}}"; 
                    var lpCmdArgsFormat = string.Format( 
                        LP_CMD_ARGS_FORMAT, (_config.PrintByPage.Value ? "-P {1} " : "")); 
                    var process = new Process 
                                      { 
                                          StartInfo = 
                                              { 
                                                  FileName = "lp", 
                                                  CreateNoWindow = true, 
                                                  UseShellExecute = false 
                                              } 
                                      }; 
                    process.StartInfo.EnvironmentVariables["LANG"] = "en"; 
                    process.StartInfo.EnvironmentVariables["LC_CTYPE"] = "en_US.iso885915"; 
                    var pageCount = _config.PrintByPage.Value ? printerJob.PageCont : 1; 
                    for (var pageNum = 1; pageNum <= pageCount; ++pageNum) 
                    { 
                        process.StartInfo.Arguments = 
                            string.Format(lpCmdArgsFormat, _findPrinterName, pageNum, printerJob.FilePath, 
                                          printerJob.Copies); 
                        Logger.LogInfo(Message.PrintingStartPagePrint, pageNum, printerJob.FilePath, printerJob.Copies); 
                        try 
                        { 
                            process.Start(); 
                        } 
                        catch (Exception ex) 
                        { 
                            Logger.LogError(Message.PrintingError, ex, printerJob.FilePath); 
                        } 
                        process.WaitForExit(); 
                        Thread.Sleep(WAIT_INTERVAL * 1000); 
                        var waitCounter = 1; 
                        while (!IsPrinterIdle(_findPrinterName)) 
                        { 
                            Thread.Sleep(WAIT_INTERVAL * 1000); 
                            waitCounter++; 
                            if (waitCounter * WAIT_INTERVAL > WAIT_END_PRINT) 
                            { 
                                throw new CupsFailureException("Истек таймаут ожидания выполнения печати " + 
                                                               WAIT_END_PRINT); 
                            } 
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
                Thread.Sleep(TimeSpan.FromSeconds(3)); 
                var process = new Process 
                                  { 
                                      StartInfo = 
                                          { 
                                              FileName = printerJob.FilePath, 
                                              CreateNoWindow = true, 
                                              UseShellExecute = true 
                                          } 
                                  }; 
                process.Start(); 
            } 
        } 
        private bool IsPrinterIdle(string printerName) 
        { 
            const string IDLE_SIGNATURE = " is idle."; 
            var printerIsIdle = false; 
            var output = string.Empty; 
            var startInfo = new ProcessStartInfo("lpstat", "-p " + printerName) 
                                { 
                                    CreateNoWindow = true, 
                                    UseShellExecute = false, 
                                    RedirectStandardOutput = true, 
                                    RedirectStandardError = true 
                                }; 
            startInfo.EnvironmentVariables["LC_ALL"] = "C"; 
            var process = Process.Start(startInfo); 
            ProcessHelper.WaitForProcessFinished( 
                process, 
                delegate(ProcessOutputProcessorState state) 
                { 
                    Logger.LogInfo(Message.Common_Information, string.Format("{0}:<{1}>", state.LineNumber, state.Line)); 
                    if (state.LineNumber == 1) 
                    { 
                        if (state.Line.IndexOf(printerName) == -1) 
                            return true; 
                        if (state.Line.IndexOf(IDLE_SIGNATURE) != -1) 
                        { 
                            printerIsIdle = true; 
                        } 
                    } 
                    if (state.LineNumber > 1) 
                    { 
                        if (state.Line.IndexOf("failed", StringComparison.InvariantCultureIgnoreCase) != -1 || 
                            state.Line.IndexOf("unable", StringComparison.InvariantCultureIgnoreCase) != -1 || 
                            state.Line.IndexOf("error", StringComparison.InvariantCultureIgnoreCase) != -1) 
                        { 
                            output += state.Line; 
                        } 
                    } 
                    return false; 
                }, 
                    null 
                ); 
            if (output.Trim().Length > 0) 
            { 
                throw new CupsFailureException(output); 
            } 
            return printerIsIdle; 
        } 
        private void CancelJobs(string printerName) 
        { 
            try 
            { 
                ProcessHelper.StartProcessAndWaitForFinished("cancel", "-a " + printerName, null, null); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.PrintingEnablingError, ex); 
            } 
        } 
        #endregion 
    } 
}
