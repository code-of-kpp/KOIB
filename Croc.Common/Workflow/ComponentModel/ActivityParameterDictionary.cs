using System; 
using System.Collections.Generic; 
using System.Runtime.Serialization; 
using Croc.Core.Utils.Collections; 
namespace Croc.Workflow.ComponentModel 
{ 
    [Serializable] 
    public sealed class ActivityParameterDictionary : ByNameAccessDictionary<ActivityParameter>, 
        IEnumerable<ActivityParameter> 
    { 
        public ActivityParameterDictionary() 
        { 
        } 
        public ActivityParameterDictionary(SerializationInfo info, StreamingContext context) 
            : base(info, context) 
        { 
        } 
        #region IEnumerable<ActivityParameter> Members 
        public new IEnumerator<ActivityParameter> GetEnumerator() 
        { 
            return Values.GetEnumerator(); 
        } 
        #endregion 
        public void CheckParameter(string paramName) 
        { 
            if (!Contains(paramName)) 
                throw new ArgumentException(string.Format("Параметр {0} не найден", paramName)); 
        } 
        public T GetParamValueOrThrow<T>(string paramName) 
        { 
            CheckParameter(paramName); 
            return this[paramName].GetValue<T>(); 
        } 
        public T GetParamValue<T>(string paramName) 
        { 
            return GetParamValue(paramName, default(T)); 
        } 
        public T GetParamValue<T>(string paramName, T defaultValue) 
        { 
            if (!Contains(paramName)) 
                return defaultValue; 
            return this[paramName].GetValue<T>(); 
        } 
        public IEnumerable<T> GetParamValueAsEnumerableOrThrow<T>(string paramName) 
        { 
            CheckParameter(paramName); 
            return this[paramName].GetParamValueAsEnumerable<T>(); 
        } 
        public IEnumerable<T> GetParamValueAsEnumerable<T>(string paramName, IEnumerable<T> defaultValue) 
        { 
            if (!Contains(paramName)) 
                return defaultValue; 
            return this[paramName].GetParamValueAsEnumerable<T>(); 
        } 
        public object[] GetParamValueAsArrayOrThrow(string paramName) 
        { 
            CheckParameter(paramName); 
            return this[paramName].GetParamValueAsArray(); 
        } 
        public object[] GetParamValueAsArray(string paramName) 
        { 
            return GetParamValueAsArray(paramName, new object[] { }); 
        } 
        public object[] GetParamValueAsArray(string paramName, object[] defaultValue) 
        { 
            if (!Contains(paramName)) 
                return defaultValue; 
            return this[paramName].GetParamValueAsArray(); 
        } 
        public T[] GetParamValueAsArrayOrThrow<T>(string paramName) 
        { 
            CheckParameter(paramName); 
            return this[paramName].GetParamValueAsArray<T>(); 
        } 
        public T[] GetParamValueAsArray<T>(string paramName) 
        { 
            return GetParamValueAsArray(paramName, new T[] { }); 
        } 
        public T[] GetParamValueAsArray<T>(string paramName, T[] defaultValue) 
        { 
            if (!Contains(paramName)) 
                return defaultValue; 
            return this[paramName].GetParamValueAsArray<T>(); 
        } 
    } 
}
