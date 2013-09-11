using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Bpc.Synchronization 

{ 

    /// <summary> 

    /// Результат выполнения синхронизации 

    /// </summary> 

    public enum SynchronizationResult 

    { 

        /// <summary> 

        /// Синхронизация успешно выполнена 

        /// </summary> 

        Succeeded, 

        /// <summary> 

        /// Синхронизация не была включена 

        /// </summary> 

        SynchronizationNotEnabled, 

        /// <summary> 

        /// Во время синхронизации произошла ошибка 

        /// </summary> 

        Failed, 

        /// <summary> 

        /// Во время синхронизации связь с удаленным сканером была потеряна 

        /// </summary> 

        RemoteScannerDisconnected, 

        /// <summary> 

        /// Во время синхронизации ее (синхронизацию) отключили 

        /// </summary> 

        SynchronizationDisabled, 

    } 

}


