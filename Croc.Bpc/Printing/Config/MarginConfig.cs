using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Printing.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент отступов 

    /// </summary> 

    public class MarginConfig : ConfigurationElement 

    { 

        /// <summary> 

        /// Отступ от левой границы 

        /// </summary> 

        [ConfigurationProperty("left", IsRequired = true)] 

        public int Left 

        { 

            get 

            { 

                return (int)this["left"]; 

            } 

            set 

            { 

                this["left"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Отступ от правой границы 

        /// </summary> 

        [ConfigurationProperty("right", IsRequired = true)] 

        public int Right 

        { 

            get 

            { 

                return (int)this["right"]; 

            } 

            set 

            { 

                this["right"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Отступ от верхней границы 


        /// </summary> 

        [ConfigurationProperty("top", IsRequired = true)] 

        public int Top 

        { 

            get 

            { 

                return (int)this["top"]; 

            } 

            set 

            { 

                this["top"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Отступ от нижней границы 

        /// </summary> 

        [ConfigurationProperty("bottom", IsRequired = true)] 

        public int Bottom 

        { 

            get 

            { 

                return (int)this["bottom"]; 

            } 

            set 

            { 

                this["bottom"] = value; 

            } 

        } 

    } 

}


