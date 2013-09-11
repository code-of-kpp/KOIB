using System; 
using System.Reflection; 
using Croc.Core; 
using Croc.Core.Extensions; 
namespace Croc.Workflow.ComponentModel 
{ 
    [Serializable] 
    internal class MethodCaller 
    { 
        public readonly MethodInfo Method; 
        public readonly object MethodOwner; 
        public MethodCaller(Type methodDelegateType, string methodName, object methodOwner) 
        { 
            CodeContract.Requires(methodDelegateType != null); 
            CodeContract.Requires(!string.IsNullOrEmpty(methodName)); 
            CodeContract.Requires(methodOwner != null); 
            Method = methodDelegateType.FindMethod(methodName, methodOwner); 
            MethodOwner = methodOwner; 
        } 
        public object Call(object[] parameters) 
        { 
            return Method.Invoke(MethodOwner, parameters); 
        } 
    } 
}
