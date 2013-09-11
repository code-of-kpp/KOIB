using System; 

using System.Collections.Generic; 

using Croc.Core.Utils.Collections; 

using System.Runtime.Serialization; 

 

 

namespace Croc.Workflow.ComponentModel 

{ 

    /// <summary> 

    /// Словарь параметров действия 

    /// </summary> 

    [Serializable] 

    public sealed class ActivityParameterDictionary : ByNameAccessDictionary<ActivityParameter>, 

        IEnumerable<ActivityParameter> 

    { 

        public ActivityParameterDictionary() 

            : base() 

        { 

        } 

 

 

        public ActivityParameterDictionary(SerializationInfo info, StreamingContext context) 

            : base(info, context) 

        { 

        } 

 

 

        #region IEnumerable<ActivityParameter> Members 

 

 

        public new IEnumerator<ActivityParameter> GetEnumerator() 

        { 

            return this.Values.GetEnumerator(); 

        } 

 

 

        #endregion 

 

 

        /// <summary> 

        /// Проверяет, что параметр с заданным именем есть в коллекции. 

        /// Если параметр не будет обнаружен, то генерится исключение 

        /// </summary> 

        /// <param name="paramName"></param> 

        public void CheckParameter(string paramName) 

        { 

            if (!Contains(paramName)) 

                throw new Exception(string.Format("Параметр {0} не найден", paramName)); 

        } 

 


 
        /// <summary> 

        /// Находит параметр с заданным именем и возвращает его значение 

        /// или генерит исключение, если параметр не найден 

        /// </summary> 

        /// <typeparam name="T"></typeparam> 

        /// <param name="paramName"></param> 

        /// <returns></returns> 

        public T GetParamValueOrThrow<T>(string paramName) 

        { 

            CheckParameter(paramName); 

            return this[paramName].GetValue<T>(); 

        } 

 

 

        /// <summary> 

        /// Находит параметр с заданным именем и возвращает его значение 

        /// или возвращает default(T), если параметр не найден 

        /// </summary> 

        /// <typeparam name="T"></typeparam> 

        /// <param name="paramName"></param> 

        /// <returns></returns> 

        public T GetParamValue<T>(string paramName) 

        { 

            return GetParamValue<T>(paramName, default(T)); 

        } 

 

 

        /// <summary> 

        /// Находит параметр с заданным именем и возвращает его значение 

        /// или возвращает значение по умолчанию, если параметр не найден 

        /// </summary> 

        /// <typeparam name="T"></typeparam> 

        /// <param name="paramName"></param> 

        /// <param name="defaultValue"></param> 

        /// <returns></returns> 

        public T GetParamValue<T>(string paramName, T defaultValue) 

        { 

            if (!Contains(paramName)) 

                return defaultValue; 

 

 

            return this[paramName].GetValue<T>(); 

        } 

 

 

        /// <summary> 

        /// Находит параметр с заданным именем и возвращает перечисление,  

        /// которое состоит из элементов массива, которым является значение параметра. 

        /// Но если значение параметра не массив, то возвращает перечисление из одного элемента - значения параметра. 


        /// Если параметр не найден, то генерит исключение. 

        /// </summary> 

        /// <typeparam name="T"></typeparam> 

        /// <param name="paramName"></param> 

        /// <returns></returns> 

        public IEnumerable<T> GetParamValueAsEnumerableOrThrow<T>(string paramName) 

        { 

            CheckParameter(paramName); 

            return this[paramName].GetParamValueAsEnumerable<T>(); 

        } 

 

 

        /// <summary> 

        /// Находит параметр с заданным именем и возвращает перечисление,  

        /// которое состоит из элементов массива, которым является значение параметра. 

        /// Но если значение параметра не массив, то возвращает перечисление из одного элемента - значения параметра. 

        /// Если параметр не найден, то возвращает значение по умолчанию 

        /// </summary> 

        /// <typeparam name="T"></typeparam> 

        /// <param name="parameters"></param> 

        /// <param name="paramName"></param> 

        /// <returns></returns> 

        public IEnumerable<T> GetParamValueAsEnumerable<T>(string paramName, IEnumerable<T> defaultValue) 

        { 

            if (!Contains(paramName)) 

                return defaultValue; 

 

 

            return this[paramName].GetParamValueAsEnumerable<T>(); 

        } 

 

 

        /// <summary> 

        /// Получить значение параметра в виде массива или сгенерить исключение, если параметр не найден 

        /// </summary> 

        /// <param name="paramName"></param> 

        /// <returns></returns> 

        public object[] GetParamValueAsArrayOrThrow(string paramName) 

        { 

            CheckParameter(paramName); 

            return this[paramName].GetParamValueAsArray(); 

        } 

 

 

        /// <summary> 

        /// Получить значение параметра в виде массива или получить пустой массив, если параметр не найден 

        /// </summary> 

        /// <param name="paramName"></param> 

        /// <returns></returns> 

        public object[] GetParamValueAsArray(string paramName) 


        { 

            return GetParamValueAsArray(paramName, new object[] { }); 

        } 

 

 

        /// <summary> 

        /// Получить значение параметра в виде массива или получить массив по умолчанию, если параметр не найден 

        /// </summary> 

        /// <param name="paramName"></param> 

        /// <param name="defaultValue"></param> 

        /// <returns></returns> 

        public object[] GetParamValueAsArray(string paramName, object[] defaultValue) 

        { 

            if (!Contains(paramName)) 

                return defaultValue; 

 

 

            return this[paramName].GetParamValueAsArray(); 

        } 

 

 

        /// <summary> 

        /// Получить значение параметра в виде массива элементов заданного типа 

        /// или сгенерить исключение, если параметр не найден 

        /// </summary> 

        /// <param name="paramName"></param> 

        /// <returns></returns> 

        public T[] GetParamValueAsArrayOrThrow<T>(string paramName) 

        { 

            CheckParameter(paramName); 

            return this[paramName].GetParamValueAsArray<T>(); 

        } 

 

 

        /// <summary> 

        /// Получить значение параметра в виде массива элементов заданного типа 

        /// или получить пустой массив, если параметр не найден 

        /// </summary> 

        /// <param name="paramName"></param> 

        /// <returns></returns> 

        public T[] GetParamValueAsArray<T>(string paramName) 

        { 

            return GetParamValueAsArray<T>(paramName, new T[] { }); 

        } 

 

 

        /// <summary> 

        /// Получить значение параметра в виде массива элементов заданного типа 

        /// или получить массив по умолчанию, если параметр не найден 

        /// </summary> 


        /// <param name="paramName"></param> 

        /// <param name="defaultValue"></param> 

        /// <returns></returns> 

        public T[] GetParamValueAsArray<T>(string paramName, T[] defaultValue) 

        { 

            if (!Contains(paramName)) 

                return defaultValue; 

 

 

            return this[paramName].GetParamValueAsArray<T>(); 

        } 

    } 

}


