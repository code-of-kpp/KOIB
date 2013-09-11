using System; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Workflow.Runtime 
{ 
    internal static class WorkflowBuiltinFunctions 
    { 
        internal static ActivityParameterEvaluator GetEvaluatorForBuiltinFunction(string functionName) 
        { 
            if (string.IsNullOrEmpty(functionName)) 
                throw new ArgumentNullException("functionName", "Не задано имя встроенной функции"); 
            switch (functionName) 
            { 
                case "True": 
                    return new ActivityParameterEvaluator(true); 
                case "False": 
                    return new ActivityParameterEvaluator(false); 
                default: 
                    throw new ArgumentException("Неизвестная встроенная функция @@" + functionName); 
            } 
        } 
        internal static ReturnActivity GetReturnActivity(string returnActivityExpression) 
        { 
            if (string.IsNullOrEmpty(returnActivityExpression)) 
                throw new ArgumentNullException("returnActivityExpression", "Не задано имя встроенной функции"); 
            if (!returnActivityExpression.StartsWith("Return(") || !returnActivityExpression.EndsWith(")")) 
                throw new ArgumentException("Некорректное имя встроенной функции: @@" + returnActivityExpression); 
            returnActivityExpression = returnActivityExpression.Substring(7, returnActivityExpression.Length - 8); 
            return new ReturnActivity(new NextActivityKey(returnActivityExpression)); 
        } 
    } 
}
