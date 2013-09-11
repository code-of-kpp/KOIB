using System; 
using System.Linq; 
using System.Reflection; 
namespace Croc.Core.Extensions 
{ 
    public static class TypeExtensions 
    { 
        public static MethodInfo FindMethod(this Type methodDelegateType, string methodName, object methodOwner) 
        { 
            if (!methodDelegateType.IsInheritedFromType(typeof(Delegate))) 
                throw new ArgumentException("Тип не является делегатом"); 
            var delegateParameters = methodDelegateType.GetMethodParametersTypes("Invoke"); 
            var delegateReturnType = methodDelegateType.GetMethodReturnType("Invoke"); 
            var type = methodOwner.GetType(); 
            var methodInfo = type.GetMethod( 
                methodName, 
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, 
                null, 
                delegateParameters, 
                null); 
            if (methodInfo == null || methodInfo.ReturnType != delegateReturnType) 
                throw new Exception(string.Format( 
                    "Метод {0} не найден или сигнатура метода не соответствует требуемой", methodName)); 
            var methodParams = methodInfo.GetParameters(); 
            if (methodParams.Any(methodParam => !delegateParameters.Contains(methodParam.ParameterType))) 
            { 
                throw new Exception(string.Format( 
                    "Типы параметров метода {0} не соответствуют требуемым", methodName)); 
            } 
            return methodInfo; 
        } 
        public static Type GetMethodReturnType(this Type type, string methodName) 
        { 
            return type.GetMethod(methodName).ReturnType; 
        } 
        public static Type[] GetMethodParametersTypes(this Type type, string methodName) 
        { 
            var parameters = type.GetMethod(methodName).GetParameters(); 
            return parameters.Select(param => param.ParameterType).ToArray(); 
        } 
        public static bool IsInheritedFromType(this Type testType, Type baseType) 
        { 
            var tmpType = testType; 
            while ((tmpType = tmpType.BaseType) != null) 
            { 
                if (tmpType == baseType) 
                    return true; 
            } 
            return false; 
        } 
        public static bool IsImplementInterface(this Type testType, Type interfaceType) 
        { 
            Type[] interfaces = testType.GetInterfaces(); 
            return interfaces.Any(type => type == interfaceType); 
        } 
        public static bool CanCastToType(this Type testType, Type castType) 
        { 
            return testType == castType || 
                testType.IsInheritedFromType(castType) || 
                testType.IsImplementInterface(castType); 
        } 
        public static object ConvertToType(this Type type, object obj) 
        { 
            if (obj == null) 
                return null; 
            var objType = obj.GetType(); 
            if (objType.CanCastToType(type)) 
                return obj; 
            if (objType == typeof(string)) 
            { 
                if (type.IsEnum) 
                    return Enum.Parse(type, (string)obj); 
                if (typeof(TimeSpan).Equals(type)) 
                    return TimeSpan.Parse((string)obj); 
            } 
            return Convert.ChangeType(obj, type); 
        } 
        public static PropertyInfo GetProperty( 
            this Type type, string propertyName, bool needGetAccessor, bool needSetAccessor) 
        { 
            var propInfo = type.GetProperty(propertyName); 
            if (propInfo == null) 
                throw new Exception(string.Format("Тип {0} не содержит свойства public {1}", type.Name, propertyName)); 
            var foundGetAccessor = false; 
            var foundSetAccessor = false; 
            var accessors = propInfo.GetAccessors(); 
            if (accessors != null) 
            { 
                foreach (var accessor in accessors) 
                { 
                    if (accessor.Name.StartsWith("get_")) 
                        foundGetAccessor = true; 
                    else if (accessor.Name.StartsWith("set_")) 
                        foundSetAccessor = true; 
                } 
            } 
            if (needGetAccessor && !foundGetAccessor) 
                throw new Exception(string.Format( 
                    "Свойства {0} типа {1} не содержит public get аксессора", propertyName, type.Name)); 
            if (needSetAccessor && !foundSetAccessor) 
                throw new Exception(string.Format( 
                    "Свойства {0} типа {1} не содержит public set аксессора", propertyName, type.Name)); 
            return propInfo; 
        } 
    } 
}
