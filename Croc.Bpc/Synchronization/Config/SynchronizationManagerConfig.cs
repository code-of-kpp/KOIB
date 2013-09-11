using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Core.Configuration; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Synchronization.Config 

{ 

	/// <summary> 

	/// Конфиг-элемент менеджера синхронизации 

	/// </summary> 

	public class SynchronizationManagerConfig : SubsystemConfig 

	{ 

        /// <summary> 

        /// Настройки вызова удаленного сканера 

        /// </summary> 

        [ConfigurationProperty("remoteScannerCallProperties", IsRequired = true)] 

        public RemoteScannerCallPropertiesConfig RemoteScannerCallProperties 

        { 

            get 

            { 

                return (RemoteScannerCallPropertiesConfig)this["remoteScannerCallProperties"]; 

            } 

            set 

            { 

                this["remoteScannerCallProperties"] = value; 

            } 

        } 

	} 

}


