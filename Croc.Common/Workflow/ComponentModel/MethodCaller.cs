using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Reflection; 

using Croc.Core.Extensions; 

 

 

namespace Croc.Workflow.ComponentModel 

{ 

    /// <summary> 

    /// Класс предназначен для вызова метода c заданным именем у заданного объекта 

    /// </summary> 

    [Serializable] 

    internal class MethodCaller 

    { 

        /// <summary> 

        /// Св-во, которое нужно "дернуть" для получения значения 

        /// </summary> 

        public readonly MethodInfo Method; 

        /// <summary> 

        /// Владелец св-ва, которое нужно "дернуть" для получения значения 

        /// </summary> 

        public readonly object MethodOwner; 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="methodDelegateType">тип делегата, который описывает сигнатуру метода</param> 

        /// <param name="methodName">имя метода</param> 

        /// <param name="methodOwner">объект-владелец метода</param> 

        public MethodCaller(Type methodDelegateType, string methodName, object methodOwner) 

        { 

            CodeContract.Requires(methodDelegateType != null); 

            CodeContract.Requires(!string.IsNullOrEmpty(methodName)); 

            CodeContract.Requires(methodOwner != null); 

 

 

            Method = methodDelegateType.FindMethod(methodName, methodOwner); 

            MethodOwner = methodOwner; 

        } 

 

 

        /// <summary> 

        /// Вызвать метод 

        /// </summary> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public object Call(object[] parameters) 


        { 

            return Method.Invoke(MethodOwner, parameters); 

        } 

    } 

}


