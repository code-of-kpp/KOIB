using System; 

 

 

namespace Croc.Workflow.ComponentModel.Compiler 

{ 

    /// <summary> 

    /// Cпециальный фиктивный вычислитель значения параметра, который является признаком того, что  

    /// не все необходимый данные вычислителя, которые нужны для вычисления значения, вычислены 

    /// </summary> 

    internal class UnevaluatedActivityParameterEvaluator : ActivityParameterEvaluator 

    { 

        public string PropertyName 

        { 

            get; 

            private set; 

        } 

 

 

        public new UnevaluatedActivity PropertyOwner 

        { 

            get; 

            private set; 

        } 

 

 

        internal UnevaluatedActivityParameterEvaluator(string propertyName, UnevaluatedActivity propertyOwner) 

            : base(ActivityParameterValueType.ReferenceToProperty) 

        { 

            CodeContract.Requires(!string.IsNullOrEmpty(propertyName)); 

            CodeContract.Requires(propertyOwner != null); 

 

 

            PropertyName = propertyName; 

            PropertyOwner = propertyOwner; 

        } 

    } 

}


