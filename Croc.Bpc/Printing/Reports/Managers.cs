using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Core; 

using Croc.Bpc.Election; 

using Croc.Bpc.FileSystem; 

using Croc.Bpc.Common.Interfaces; 

 

 

namespace Croc.Bpc.Printing.Reports 

{ 

    /// <summary> 

    /// Класс для доступа к менеджерам 

    /// </summary> 

    /// <remarks>FIX: необходимость в этом классе возникла из-за того, что при загрузке шаблонов, 

    /// когда они десериализуются, то никак не добраться до нужных данных, кроме как через статич. 

    /// св-ва. Надеюсь, это временно решение и рефакторинг не за горами</remarks> 

    internal static class Managers 

    { 

        private static IPrintingManager _printingManager; 

        /// <summary> 

        /// Менеджер печати 

        /// </summary> 

        public static IPrintingManager PrintingManager 

        { 

            get 

            { 

                if (_printingManager == null) 

                    _printingManager = CoreApplication.Instance.GetSubsystemOrThrow<IPrintingManager>(); 

 

 

                return _printingManager; 

            } 

        } 

 

 

        private static IElectionManager _electionManager; 

        /// <summary> 

        /// Менеджер выборов 

        /// </summary> 

        public static IElectionManager ElectionManager 

        { 

            get 

            { 

                if (_electionManager == null) 

                    _electionManager = CoreApplication.Instance.GetSubsystemOrThrow<IElectionManager>(); 

 

 

                return _electionManager; 


            } 

        } 

 

 

        private static IFileSystemManager _fsManager; 

        /// <summary> 

        /// Менеджер файлов 

        /// </summary> 

        public static IFileSystemManager FileSystemManager 

        { 

            get 

            { 

                if (_fsManager == null) 

                    _fsManager = CoreApplication.Instance.GetSubsystemOrThrow<IFileSystemManager>(); 

 

 

                return _fsManager; 

            } 

        } 

 

 

        private static IScannersInfo _scannersInfo; 

        /// <summary> 

        /// Интерфейс для получения информации о сканерах 

        /// </summary> 

        public static IScannersInfo ScannersInfo 

        { 

            get 

            { 

                if (_scannersInfo == null) 

                    _scannersInfo = CoreApplication.Instance.FindSubsystemImplementsInterfaceOrThrow<IScannersInfo>(); 

 

 

                return _scannersInfo; 

            } 

        } 

    } 

 

 

}


