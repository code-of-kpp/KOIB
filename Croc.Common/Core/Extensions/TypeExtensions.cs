using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Reflection; 

 

 

namespace Croc.Core.Extensions 

{ 

    /// <summary> 

    /// Расширения для Type 

    /// </summary> 

    public static class TypeExtensions 

    { 

        /// <summary> 

        /// Находит метод у объекта methodOwner именем methodName и сигнатурой, 

        /// которая определена делегатом delegateType 

        /// </summary> 

        /// <param name="methodName"></param> 

        /// <param name="methodOwner"></param> 

        /// <returns></returns> 

        public static MethodInfo FindMethod(this Type methodDelegateType, string methodName, object methodOwner) 

        { 

            if (!methodDelegateType.IsInheritedFromType(typeof(Delegate))) 

                throw new ArgumentException("Тип не является делегатом"); 

 

 

            var delegateParameters = TypeExtensions.GetMethodParametersTypes(methodDelegateType, "Invoke"); 

            var delegateReturnType = TypeExtensions.GetMethodReturnType(methodDelegateType, "Invoke"); 

 

 

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

 

 

            // необходимо еще раз проверить типы параметров, т.к. type.GetMethod мог найти метод, 

            // у кот. параметры имеют не точно заданные типы, а типы, от которых унаследованы заданные  

            var methodParams = methodInfo.GetParameters(); 

            foreach (var methodParam in methodParams) 

            { 


                if (!delegateParameters.Contains(methodParam.ParameterType)) 

                    throw new Exception(string.Format( 

                        "Типы параметров метода {0} не соответствуют требуемым", methodName)); 

            } 

 

 

            return methodInfo; 

        } 

 

 

        /// <summary> 

        /// Возвращает тип return-значения метода у типа 

        /// </summary> 

        /// <param name="type"></param> 

        /// <param name="methodName"></param> 

        /// <returns></returns> 

        public static Type GetMethodReturnType(this Type type, string methodName) 

        { 

            return type.GetMethod(methodName).ReturnType; 

        } 

 

 

        /// <summary> 

        /// Возвращает массив типов параметров метода у типа 

        /// </summary> 

        /// <param name="type"></param> 

        /// <param name="methodName"></param> 

        /// <returns></returns> 

        public static Type[] GetMethodParametersTypes(this Type type, string methodName) 

        { 

            var types = new List<Type>(); 

 

 

            var parameters = type.GetMethod(methodName).GetParameters(); 

            foreach (var param in parameters) 

            { 

                types.Add(param.ParameterType); 

            } 

 

 

            return types.ToArray(); 

        } 

 

 

        /// <summary> 

        /// Проверяет, является ли тип унаследованным от заданного типа 

        /// </summary> 

        /// <param name="testType"></param> 

        /// <param name="baseType"></param> 

        /// <returns></returns> 


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

 

 

        /// <summary> 

        /// Проверяет, реализует ли тип заданный интерфейс 

        /// </summary> 

        /// <param name="testType"></param> 

        /// <param name="baseType"></param> 

        /// <returns></returns> 

        public static bool IsImplementInterface(this Type testType, Type interfaceType) 

        { 

            Type[] interfaces = testType.GetInterfaces(); 

            foreach (var type in interfaces) 

            { 

                if (type == interfaceType) 

                    return true; 

            } 

 

 

            return false; 

        } 

 

 

        /// <summary> 

        /// Проверяет, что тип можно привести к заданному 

        /// </summary> 

        /// <param name="testType"></param> 

        /// <param name="castType"></param> 

        /// <returns></returns> 

        public static bool CanCastToType(this Type testType, Type castType) 

        { 

            return testType == castType || 

                testType.IsInheritedFromType(castType) || 

                testType.IsImplementInterface(castType); 

        } 

 

 

        /// <summary> 

        /// Возвращает описание св-ва с заданным именем в типе и проверяет, что у св-ва доступны заданные аксессоры 


        /// </summary> 

        /// <param name="propertyName"></param> 

        /// <param name="needGetAccessor"></param> 

        /// <param name="needSetAccessor"></param> 

        /// <returns></returns> 

        public static PropertyInfo GetProperty( 

            this Type type, string propertyName, bool needGetAccessor, bool needSetAccessor) 

        { 

            // проверим, что для данного параметра есть соотв. св-во в классе составного действия 

            var propInfo = type.GetProperty(propertyName); 

            if (propInfo == null) 

                throw new Exception(string.Format("Тип {0} не содержит свойства public {1}", type.Name, propertyName)); 

 

 

            // проверим, что св-во имеет нужные аксессоры 

            bool foundGetAccessor = false; 

            bool foundSetAccessor = false; 

            foreach (var accessor in propInfo.GetAccessors()) 

            { 

                if (accessor.Name.StartsWith("get_")) 

                    foundGetAccessor = true; 

                else if (accessor.Name.StartsWith("set_")) 

                    foundSetAccessor = true; 

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


