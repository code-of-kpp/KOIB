using System; 

using System.Reflection; 

 

 

namespace Croc.Workflow.ComponentModel 

{ 

    /// <summary> 

    /// Типы значения параметра 

    /// </summary> 

    internal enum ActivityParameterValueType 

    { 

        /// <summary> 

        /// чистое значение 

        /// </summary> 

        PlainValue, 

        /// <summary> 

        /// ссылка на св-во 

        /// </summary> 

        ReferenceToProperty, 

        /// <summary> 

        /// Массив 

        /// </summary> 

        Array 

    } 

 

 

    /// <summary> 

    /// Вычислятель значения параметра действия 

    /// </summary> 

    [Serializable] 

    internal class ActivityParameterEvaluator 

    { 

        /// <summary> 

        /// Тип значения параметра 

        /// </summary> 

        public ActivityParameterValueType ValueType 

        { 

            get; 

            private set; 

        } 

        /// <summary> 

        /// Значение в чистом виде (когда вычисление не требуется) 

        /// </summary> 

        public object PlainValue 

        { 

            get; 

            private set; 

        } 

        /// <summary> 

        /// Массив вычислителей значений  


        /// </summary> 

        public ActivityParameterEvaluator[] EvaluatorArray 

        { 

            get; 

            private set; 

        } 

        /// <summary> 

        /// Св-во, которое нужно "дернуть" для получения значения 

        /// </summary> 

        public PropertyInfo Property 

        { 

            get; 

            private set; 

        } 

        /// <summary> 

        /// Владелец св-ва, которое нужно "дернуть" для получения значения 

        /// </summary> 

        public object PropertyOwner 

        { 

            get; 

            private set; 

        } 

 

 

 

 

        public ActivityParameterEvaluator(ActivityParameterValueType valueType) 

        { 

            ValueType = valueType; 

        } 

 

 

        public ActivityParameterEvaluator(object plainValue) 

        { 

            PlainValue = plainValue; 

            ValueType = ActivityParameterValueType.PlainValue; 

        } 

 

 

        public ActivityParameterEvaluator(ActivityParameterEvaluator[] evaluatorArray) 

        { 

            EvaluatorArray = evaluatorArray; 

            ValueType = ActivityParameterValueType.Array; 

        } 

 

 

        public ActivityParameterEvaluator(PropertyInfo property, object propertyOwner) 

        { 

            CodeContract.Requires(property != null); 

            CodeContract.Requires(propertyOwner != null); 


 
 

            Property = property; 

            PropertyOwner = propertyOwner; 

            ValueType = ActivityParameterValueType.ReferenceToProperty; 

        } 

 

 

        /// <summary> 

        /// Получить значение 

        /// </summary> 

        /// <returns></returns> 

        public object GetValue() 

        { 

            switch (ValueType) 

            { 

                case ActivityParameterValueType.PlainValue: 

                    return PlainValue; 

 

 

                case ActivityParameterValueType.ReferenceToProperty: 

                    return Property.GetValue(PropertyOwner, null); 

 

 

                case ActivityParameterValueType.Array: 

                    var valueArray = new object[EvaluatorArray.Length]; 

                    for (int i = 0; i < valueArray.Length; i++) 

                        valueArray[i] = EvaluatorArray[i].GetValue(); 

 

 

                    return valueArray; 

 

 

                default: 

                    throw new Exception("Неизвестный тип вычисления значения параметра"); 

            } 

        } 

    } 

}


