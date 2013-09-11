using System; 
using System.Runtime; 
using System.Threading; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Voting; 
using Croc.Core; 
using Croc.Core.Diagnostics; 
namespace Croc.Bpc.Scanner 
{ 
    public class SheetProcessingSession 
    { 
        private static readonly object s_sync = new object(); 
        private ILogger _logger; 
        public int Id 
        { 
            get; 
            private set; 
        } 
        private volatile bool _closed; 
        public bool Closed 
        { 
            get { return _closed; } 
        } 
        private readonly ManualResetEvent _closedEvent = new ManualResetEvent(false); 
        public bool ReceivingAllowed = true; 
        public VotingResult VotingResult; 
        public readonly ManualResetEvent ErrorSpecified = new ManualResetEvent(false); 
        private SheetProcessingError _error; 
        public SheetProcessingError Error 
        { 
            get 
            { 
                return _error; 
            } 
            set 
            { 
                CodeContract.Requires(value != null); 
                if (_error != null && _error.Code == value.Code) 
                { 
                    value.IsRepeated = true; 
                } 
                _error = value; 
                ErrorSpecified.Set(); 
            } 
        } 
        public SheetType SheetType = SheetType.Undefined; 
        public DropResult DropResult = DropResult.Timeout; 
        public static SheetProcessingSession GetClosedSheetProcessingSession(ILogger logger) 
        { 
            var session = new SheetProcessingSession 
                       { 
                           Id = GenerateId(), 
                           _logger = logger, 
                           _closed = true 
                       }; 
            session._closedEvent.Set(); 
            return session; 
        } 
        private static int GenerateId() 
        { 
            return Math.Abs(Guid.NewGuid().GetHashCode()); 
        } 
        public void Open() 
        { 
            lock (s_sync) 
            { 
                Id = GenerateId(); 
                _closed = false; 
                _closedEvent.Reset(); 
                ReceivingAllowed = true; 
                VotingResult = null; 
                _error = null; 
                ErrorSpecified.Reset(); 
                DropResult = DropResult.Timeout; 


                GCSettings.LatencyMode = GCLatencyMode.LowLatency; 
            } 
        } 
        public void Reset() 
        { 
            lock (s_sync) 
            { 
                Id = GenerateId(); 
                _closed = true; 
                _closedEvent.Set(); 
                ReceivingAllowed = true; 
                VotingResult = null; 
                _error = null; 
                ErrorSpecified.Reset(); 
                DropResult = DropResult.Timeout; 
            } 
        } 
        public void Close() 
        { 
            lock (s_sync) 
            { 
                GCSettings.LatencyMode = GCLatencyMode.Batch; 
                GC.Collect(); 
                _closed = true; 
                _closedEvent.Set(); 
            } 
        } 
        public bool WaitForClose(int timeout) 
        { 
            lock (s_sync) 
            { 
                if (_closed) 
                    return true; 
            } 
            _logger.LogVerbose(Message.ScannerManagerWaitForCloseSheetProcessingSession, Id); 
            return _closedEvent.WaitOne(timeout); 
        } 
    } 
}
