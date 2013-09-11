using System; 

using System.Collections.Generic; 

using System.Configuration; 

using Croc.Core.Configuration; 

 

 

namespace Croc.Bpc.Scanner.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент описывающий настройки предупреждений 

    /// </summary> 

    public class AlertsConfig : EnabledConfig 

    { 

        /// <summary> 

        /// Словарь кол-ва появлений ошибок [код ошибки, кол-во появлений] 

        /// </summary> 

        private Dictionary<int, int> _errorOccursCountDict = new Dictionary<int, int>(); 

        /// <summary> 

        /// Объект синхронизации работы со словарем кол-ва ошибок 

        /// </summary> 

        private static object s_errorOccursCountDictSync = new object(); 

 

 

        /// <summary> 

        /// Нужно ли предупреждать об ошибке 

        /// </summary> 

        /// <param name="errorCode">код ошибки</param> 

        /// <returns>true - нужно выдать предупреждение, false - еще рано предупреждать</returns> 

        public bool NeedAlertAboutError(ErrorConfig error) 

        { 

            // если об ошибках данного типа предупреждать вообще не нужно 

            if (!error.Enabled) 

                return false; 

 

 

            lock (s_errorOccursCountDictSync) 

            { 

                if (_errorOccursCountDict.ContainsKey(error.Code)) 

                    _errorOccursCountDict[error.Code] += 1; 

                else 

                    _errorOccursCountDict[error.Code] = 1; 

 

 

                // предупреждаем, только если данная ошибка появилась не менее заданного лимита раз 

                return _errorOccursCountDict[error.Code] >= Limit; 

            } 

        } 

 

 

        /// <summary> 


        /// Сбрасывает внутренние счетчики ошибок 

        /// </summary> 

        public void ResetErrorCounters() 

        { 

            lock (s_errorOccursCountDictSync) 

            { 

                _errorOccursCountDict.Clear(); 

            } 

        } 

 

 

        /// <summary> 

        /// Получить ошибку по коду 

        /// </summary> 

        /// <param name="errorCode"></param> 

        /// <returns></returns> 

        public ErrorConfig GetError(int errorCode) 

        { 

            return Errors.GetErrorByCode(errorCode); 

        } 

 

 

        /// <summary> 

        /// Порог, при достижении которого срабатывает предупреждение 

        /// </summary> 

        /// <remarks>т.е. если limit = 10, значит только на 10-ый раз появления ошибки  

        /// одного и того же типа будет выдано предупреждение</remarks> 

        [ConfigurationProperty("limit", IsRequired = true)] 

        public int Limit 

        { 

            get 

            { 

                return (int)this["limit"]; 

            } 

            set 

            { 

                this["limit"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Список ошибок 

        /// </summary> 

        [ConfigurationProperty("errors", IsDefaultCollection = false, IsRequired = true)] 

        [ConfigurationCollection(typeof(ErrorConfig), AddItemName = "error")] 

        public ErrorConfigCollection Errors 

        { 

            get 

            { 


                return (ErrorConfigCollection)base["errors"]; 

            } 

        } 

    } 

}


