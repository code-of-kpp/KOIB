using System; 
using System.Reflection; 
using Croc.Core; 
namespace Croc.Workflow.ComponentModel 
{ 
    public enum ActivityParameterValueType 
    { 
        PlainValue, 
        ReferenceToProperty, 
        Array 
    } 
    [Serializable] 
    public class ActivityParameterEvaluator 
    { 
        public ActivityParameterValueType ValueType 
        { 
            get; 
            private set; 
        } 
        public object PlainValue 
        { 
            get; 
            private set; 
        } 
        public ActivityParameterEvaluator[] EvaluatorArray 
        { 
            get; 
            private set; 
        } 
        public PropertyInfo Property 
        { 
            get; 
            private set; 
        } 
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
