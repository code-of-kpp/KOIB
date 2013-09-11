using Croc.Core.Diagnostics; 
using Croc.Core; 
namespace Croc.Bpc.Scanner 
{ 
    public class BulletinIDLogFilter : IEventFilter 
    { 
        private IScannerManager _scannerManager; 
        private static readonly object s_scannerManagerSync = new object(); 
        public const string BULLETIN_ID_PROPERTY = "BulletinId"; 
        private IScannerManager GetScannerManager() 
        { 
            if (_scannerManager == null) 
                lock (s_scannerManagerSync) 
                { 
                    if (_scannerManager == null) 
                        _scannerManager = CoreApplication.Instance.GetSubsystem<IScannerManager>(); 
                } 
            return _scannerManager; 
        } 
        #region IEventWriterFilter Members 
        public bool Accepted(LoggerEvent loggerEvent) 
        { 
            var scannerManager = GetScannerManager(); 
            if (scannerManager != null && 
                scannerManager.SheetProcessingSession != null && 
                !scannerManager.SheetProcessingSession.Closed) 
            { 
                loggerEvent.Properties[BULLETIN_ID_PROPERTY] = scannerManager.SheetProcessingSession.Id; 
            } 
            return true; 
        } 
        #endregion 
        #region IInitializedType Members 
        public void Init(System.Configuration.NameValueConfigurationCollection props) 
        { 
        } 
        #endregion 
    } 
}
