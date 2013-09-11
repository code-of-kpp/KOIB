using System; 

using Croc.Core.Configuration; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Scanner.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент с описанием ошибки 

    /// </summary> 

    public class ErrorConfig : EnabledConfig 

    { 

        /// <summary> 

        /// Код ошибки 

        /// </summary> 

        [ConfigurationProperty("code", IsRequired = true)] 

        public int Code 

        { 

            get 

            { 

                return (int)this["code"]; 

            } 

            set 

            { 

                this["code"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Приводит ли данная ошибка к реверсу листа 

        /// </summary> 

        [ConfigurationProperty("isReverse", IsRequired = false, DefaultValue = true)] 

        public bool IsReverse 

        { 

            get 

            { 

                return (bool)this["isReverse"]; 

            } 

            set 

            { 

                this["isReverse"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Описание ошибки 

        /// </summary> 

        [ConfigurationProperty("description", IsRequired = false)] 


        public string Description 

        { 

            get 

            { 

                return (string)this["description"]; 

            } 

            set 

            { 

                this["description"] = value; 

            } 

        } 

    } 

}


