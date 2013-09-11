using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Workflow.ComponentModel; 

using Croc.Core.Diagnostics; 

using Croc.Bpc.Scanner; 

using Croc.Bpc.Election; 

using Croc.Bpc.Synchronization; 

using Croc.Bpc.Configuration; 

using Croc.Bpc.Sound; 

using Croc.Bpc.FileSystem; 

using Croc.Bpc.Printing; 

using Croc.Core; 

using Croc.Bpc.Keyboard; 

using Croc.Bpc.Recognizer; 

 

 

namespace Croc.Bpc.Workflow.Activities 

{ 

    /// <summary> 

    /// Базовый класс для всех составных действий приложения 

    /// </summary> 

    /// <remarks> 

    /// создан с целью централизованного хранения ссылок на все необходимые подсистемы и логгер 

    /// </remarks> 

    [Serializable] 

    public abstract class BpcCompositeActivity : CompositeActivity 

    { 

        /// <summary> 

        /// Логгер 

        /// </summary> 

        [NonSerialized] 

        protected ILogger _logger; 

        /// <summary> 

        /// Менеджер потока работ 

        /// </summary> 

        [NonSerialized] 

        protected IWorkflowManager _workflowManager; 

        /// <summary> 

        /// Менеджер сканера 

        /// </summary> 

        [NonSerialized] 

        protected IScannerManager _scannerManager; 

        /// <summary> 

        /// Менеджер выборов 

        /// </summary> 

        [NonSerialized] 

        protected IElectionManager _electionManager; 

        /// <summary> 


        /// Менеджер синхронизации сканеров 

        /// </summary> 

        [NonSerialized] 

        protected ISynchronizationManager _syncManager; 

        /// <summary> 

        /// Менеджер звуков 

        /// </summary> 

        [NonSerialized] 

        protected ISoundManager _soundManager; 

        /// <summary> 

        /// Менеджер клавиатуры 

        /// </summary> 

        [NonSerialized] 

        protected IKeyboard _keyboard; 

        /// <summary> 

        /// Менеджер конфигурации 

        /// </summary> 

        [NonSerialized] 

        protected IConfigurationManager _configManager; 

        /// <summary> 

        /// Менеджер файловой системы 

        /// </summary> 

        [NonSerialized] 

        protected IFileSystemManager _fileSystemManager; 

        /// <summary> 

        /// Менеджер печати 

        /// </summary> 

        [NonSerialized] 

        protected IPrintingManager _printingManager; 

		/// <summary> 

		/// Менеджер распознавания 

		/// </summary> 

		[NonSerialized] 

		protected IRecognitionManager _recognitionManager; 

 

 

        /// <summary> 

        /// Инициализация 

        /// </summary> 

        /// <param name="context"></param> 

        protected override void Initialize(WorkflowExecutionContext context) 

        { 

            // получаем ссылки на подсистемы 

            var app = CoreApplication.Instance; 

            _workflowManager = app.GetSubsystemOrThrow<IWorkflowManager>(); 

            _scannerManager = app.GetSubsystemOrThrow<IScannerManager>(); 

            _electionManager = app.GetSubsystemOrThrow<IElectionManager>(); 

            _syncManager = app.GetSubsystemOrThrow<ISynchronizationManager>(); 

            _soundManager = app.GetSubsystemOrThrow<ISoundManager>(); 

            _keyboard = (IKeyboard)app.GetSubsystemOrThrow<UnionKeyboard>(); 


            _configManager = app.GetSubsystemOrThrow<IConfigurationManager>(); 

            _fileSystemManager = app.GetSubsystemOrThrow<IFileSystemManager>(); 

            _printingManager = app.GetSubsystemOrThrow<IPrintingManager>(); 

			_recognitionManager = app.GetSubsystemOrThrow<IRecognitionManager>(); 

 

 

            // используем логгер подсистемы потока работ 

            _logger = _workflowManager.Logger; 

        } 

    } 

}


