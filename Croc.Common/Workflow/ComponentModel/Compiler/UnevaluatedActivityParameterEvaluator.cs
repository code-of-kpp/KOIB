using Croc.Core; 
namespace Croc.Workflow.ComponentModel.Compiler 
{ 
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
