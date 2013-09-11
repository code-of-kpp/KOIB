using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Printing.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент отчетов 

    /// </summary> 

    public class ReportConfig : ConfigurationElement 

    { 

        /// <summary> 

        /// Шрифт 

        /// </summary> 

        [ConfigurationProperty("font", IsRequired = true)] 

        public FontConfig Font 

        { 

            get 

            { 

                return (FontConfig)this["font"]; 

            } 

            set 

            { 

                this["font"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Отступы 

        /// </summary> 

        [ConfigurationProperty("margin", IsRequired = true)] 

        public MarginConfig Margin 

        { 

            get 

            { 

                return (MarginConfig)this["margin"]; 

            } 

            set 

            { 

                this["margin"] = value; 

            } 

        } 

    } 

}


