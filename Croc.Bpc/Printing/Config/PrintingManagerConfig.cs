using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Core.Configuration; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Printing.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент менеджера печати 

    /// </summary> 

    public class PrintingManagerConfig : SubsystemConfig 

    { 

        /// <summary> 

        /// Признак постраничной печати отчета 

        /// </summary> 

        [ConfigurationProperty("printByPage", IsRequired = false)] 

        public ValueConfig<bool> PrintByPage 

        { 

            get 

            { 

                return (ValueConfig<bool>)base["printByPage"]; 

            } 

        } 

 

 

        /// <summary> 

        /// Конфиг формирования отчетов 

        /// </summary> 

        [ConfigurationProperty("report", IsRequired = true)] 

        public ReportConfig Report 

        { 

            get 

            { 

                return (ReportConfig)this["report"]; 

            } 

            set 

            { 

                this["report"] = value; 

            } 

        } 

 

 

		/// <summary> 

		/// Набор возможных принтеров 

		/// </summary> 

		[ConfigurationProperty("printers", IsDefaultCollection = false, IsRequired = true)] 

		[ConfigurationCollection(typeof(PrinterConfigCollection), AddItemName = "printer")] 


		public PrinterConfigCollection Printers 

		{ 

			get 

			{ 

				return (PrinterConfigCollection)base["printers"]; 

			} 

		} 

 

 

        /// <summary> 

        /// Команды 

        /// </summary> 

        [ConfigurationProperty("commands", IsRequired = false)] 

        public CommandsConfig Commands 

        { 

            get 

            { 

                return (CommandsConfig)this["commands"]; 

            } 

            set 

            { 

                this["commands"] = value; 

            } 

        } 

    } 

}


