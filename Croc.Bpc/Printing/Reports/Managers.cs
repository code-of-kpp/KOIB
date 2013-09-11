using Croc.Bpc.FileSystem; 
using Croc.Bpc.Scanner; 
using Croc.Bpc.Voting; 
using Croc.Core; 
namespace Croc.Bpc.Printing.Reports 
{ 
    internal static class Managers 
    { 
        private static PrintingManager s_printingManager; 
        public static PrintingManager PrintingManager 
        { 
            get 
            { 
                if (s_printingManager == null) 
                    s_printingManager = CoreApplication.Instance.GetSubsystemOrThrow<PrintingManager>(); 
                return s_printingManager; 
            } 
        } 
        private static IElectionManager s_electionManager; 
        public static IElectionManager ElectionManager 
        { 
            get 
            { 
                if (s_electionManager == null) 
                    s_electionManager = CoreApplication.Instance.GetSubsystemOrThrow<IElectionManager>(); 
                return s_electionManager; 
            } 
        } 
        private static IVotingResultManager s_votingResultManager; 
        public static IVotingResultManager VotingResultManager 
        { 
            get 
            { 
                if (s_votingResultManager == null) 
                    s_votingResultManager = CoreApplication.Instance.GetSubsystemOrThrow<IVotingResultManager>(); 
                return s_votingResultManager; 
            } 
        } 
        private static IFileSystemManager s_fileSystemManager; 
        public static IFileSystemManager FileSystemManager 
        { 
            get 
            { 
                if (s_fileSystemManager == null) 
                    s_fileSystemManager = CoreApplication.Instance.GetSubsystemOrThrow<IFileSystemManager>(); 
                return s_fileSystemManager; 
            } 
        } 
        private static IScannersInfo s_scannersInfo; 
        public static IScannersInfo ScannersInfo 
        { 
            get 
            { 
                if (s_scannersInfo == null) 
                    s_scannersInfo = CoreApplication.Instance.FindSubsystemImplementsInterfaceOrThrow<IScannersInfo>(); 
                return s_scannersInfo; 
            } 
        } 
    } 
}
