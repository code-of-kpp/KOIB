using System; 

using Croc.Workflow.ComponentModel; 

 

 

namespace Croc.Workflow.Runtime 

{ 

    /// <summary> 

    /// Встроенные функции, которые предоставляет исполняющая среда 

    /// </summary> 

    internal static class WorkflowBuiltinFunctions 

    { 

        /// <summary> 

        /// Получить вычислитель значения параметра действия для значения, которое 

        /// является именем встроенной функции 

        /// </summary> 

        /// <param name="functionName"></param> 

        /// <returns></returns> 

        internal static ActivityParameterEvaluator GetEvaluatorForBuiltinFunction(string functionName) 

        { 

            if (string.IsNullOrEmpty(functionName)) 

                throw new ArgumentNullException("Не задано имя встроенной функции"); 

 

 

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

 

 

        /// <summary> 

        /// Получить действие выхода из составного действия по выражению, описывающему это выходное действие 

        /// </summary> 

        /// <param name="returnActivityExpression"></param> 

        /// <returns></returns> 

        internal static ReturnActivity GetReturnActivity(string returnActivityExpression) 

        { 

            if (string.IsNullOrEmpty(returnActivityExpression)) 

                throw new ArgumentNullException("Не задано имя встроенной функции"); 

 

 


            if (!returnActivityExpression.StartsWith("Return(") || !returnActivityExpression.EndsWith(")")) 

                throw new ArgumentException("Некорректное имя встроенной функции: @@" + returnActivityExpression); 

 

 

            // отсечем "Return(" с начала и ")" с конца 

            returnActivityExpression = returnActivityExpression.Substring(7, returnActivityExpression.Length - 8); 

 

 

            return new ReturnActivity(new NextActivityKey(returnActivityExpression)); 

        } 

    } 

}


