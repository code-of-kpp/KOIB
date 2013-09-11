using System; 
using System.Collections.Generic; 
using System.Linq; 
using Croc.Core; 
using Croc.Core.Extensions; 
using Croc.Core.Utils.Collections; 
namespace Croc.Workflow.ComponentModel 
{ 
    [Serializable] 
    public class ActivityParameter : INamed 
    { 
        public string Name 
        { 
            get; 
            set; 
        } 
        public ActivityParameterEvaluator Evaluator 
        { 
            get; 
            internal set; 
        } 
        internal ActivityParameter() 
        { 
        } 
        public ActivityParameter(string name, object plainValue) 
        { 
            CodeContract.Requires(!string.IsNullOrEmpty(name)); 
            CodeContract.Requires(plainValue != null); 
            Name = name; 
            Evaluator = new ActivityParameterEvaluator(plainValue); 
        } 
        public object GetValue() 
        { 
            if (Evaluator == null) 
                return null; 
            return Evaluator.GetValue(); 
        } 
        public T GetValue<T>() 
        { 
            var value = GetValue(); 
            return CastValue<T>(value); 
        } 
        public object[] GetParamValueAsArray() 
        { 
            var value = GetValue(); 
            var type = value.GetType(); 
            return type.IsArray ? (object[])value : new[] { value }; 
        } 
        public IEnumerable<T> GetParamValueAsEnumerable<T>() 
        { 
            var value = GetParamValueAsArray(); 
            return CastArrayValue<T>(value); 
        } 
        public T[] GetParamValueAsArray<T>() 
        { 
            var objectArray = GetParamValueAsArray(); 
            var resArray = new T[objectArray.Length]; 
            for (var i = 0; i < objectArray.Length; i++) 
                resArray[i] = CastValue<T>(objectArray[i]); 
            return resArray; 
        } 
        private IEnumerable<T> CastArrayValue<T>(IEnumerable<object> arrayValue) 
        { 
            return arrayValue.Select(item => CastValue<T>(item)); 
        } 
        private T CastValue<T>(object value) 
        { 
            try 
            { 
                return (T)typeof(T).ConvertToType(value); 
            } 
            catch (Exception ex) 
            { 
                throw new InvalidCastException(string.Format( 
                    "Ошибка приведения значения '{0}' к типу {1} при получении значения параметра {2}", 
                    value, typeof(T).Name, Name), ex); 
            } 
        } 
    } 
}
