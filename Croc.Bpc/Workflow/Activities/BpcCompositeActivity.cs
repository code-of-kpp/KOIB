using System; 
using System.Threading; 
using Croc.Bpc.Configuration; 
using Croc.Bpc.FileSystem; 
using Croc.Bpc.Keyboard; 
using Croc.Bpc.Printing; 
using Croc.Bpc.Recognizer; 
using Croc.Bpc.Scanner; 
using Croc.Bpc.Sound; 
using Croc.Bpc.Synchronization; 
using Croc.Bpc.Voting; 
using Croc.Core; 
using Croc.Core.Diagnostics; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities 
{ 
    [Serializable] 
    public abstract class BpcCompositeActivity : CompositeActivity 
    { 
        public int Order 
        { 
            get; 
            set; 
        } 
        [NonSerialized] 
        protected ILogger _logger; 
        [NonSerialized] 
        protected IWorkflowManager _workflowManager; 
        [NonSerialized] 
        protected IScannerManager _scannerManager; 
        [NonSerialized] 
        protected IElectionManager _electionManager; 
        [NonSerialized] 
        protected IVotingResultManager _votingResultManager; 
        [NonSerialized] 
        protected ISynchronizationManager _syncManager; 
        [NonSerialized] 
        protected ISoundManager _soundManager; 
        [NonSerialized] 
        protected IKeyboardManager _keyboard; 
        [NonSerialized] 
        protected IConfigurationManager _configManager; 
        [NonSerialized] 
        protected IFileSystemManager _fileSystemManager; 
        [NonSerialized] 
        protected IPrintingManager _printingManager; 
        [NonSerialized] 
        protected IRecognitionManager _recognitionManager; 
        protected override void Initialize(WorkflowExecutionContext context) 
        { 
            base.Initialize(context); 
            var app = CoreApplication.Instance; 
            _workflowManager = app.GetSubsystemOrThrow<IWorkflowManager>(); 
            _scannerManager = app.GetSubsystemOrThrow<IScannerManager>(); 
            _electionManager = app.GetSubsystemOrThrow<IElectionManager>(); 
            _votingResultManager = app.GetSubsystemOrThrow<IVotingResultManager>(); 
            _syncManager = app.GetSubsystemOrThrow<ISynchronizationManager>(); 
            _soundManager = app.GetSubsystemOrThrow<ISoundManager>(); 
            _keyboard = app.GetSubsystemOrThrow<UnionKeyboard>(); 
            _configManager = app.GetSubsystemOrThrow<IConfigurationManager>(); 
            _fileSystemManager = app.GetSubsystemOrThrow<IFileSystemManager>(); 
            _printingManager = app.GetSubsystemOrThrow<IPrintingManager>(); 
            _recognitionManager = app.GetSubsystemOrThrow<IRecognitionManager>(); 
            _logger = _workflowManager.Logger; 
        } 
        #region Св-ва для получения событий нажатия различных комбинаций кнопок 
        public WaitHandle YesPressed 
        { 
            get { return KeyPressedWaitHandle.YesPressed; } 
        } 
        public WaitHandle NoPressed 
        { 
            get { return KeyPressedWaitHandle.NoPressed; } 
        } 
        public WaitHandle GoBackPressed 
        { 
            get { return KeyPressedWaitHandle.GoBackPressed; } 
        } 
        public WaitHandle HelpPressed 
        { 
            get { return KeyPressedWaitHandle.HelpPressed; } 
        } 
        public WaitHandle YesAndNoPressed 
        { 
            get { return KeyPressedWaitHandle.YesAndNoAtOncePressed; } 
        } 
        public WaitHandle HelpAndNoPressed 
        { 
            get { return KeyPressedWaitHandle.HelpAndNoAtOncePressed; } 
        } 
        #endregion 
        #region Вспомогательные методы 
        protected bool IsIsElectionDayOrExtra(ElectionDayСomming edc) 
        { 
            return edc == ElectionDayСomming.ItsElectionDay || edc == ElectionDayСomming.ItsExtraElectionDay; 
        } 
        #endregion 
    } 
}
