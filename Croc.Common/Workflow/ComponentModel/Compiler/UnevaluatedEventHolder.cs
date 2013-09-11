using Croc.Core; 
namespace Croc.Workflow.ComponentModel.Compiler 
{ 
    internal class UnevaluatedEventHolder : EventHolder 
    { 
        public new string EventName 
        { 
            get; 
            private set; 
        } 
        public new UnevaluatedActivity EventOwner 
        { 
            get; 
            private set; 
        } 
        internal UnevaluatedEventHolder(string eventName, UnevaluatedActivity eventOwner) 
        { 
            CodeContract.Requires(!string.IsNullOrEmpty(eventName)); 
            CodeContract.Requires(eventOwner != null); 
            EventName = eventName; 
            EventOwner = eventOwner; 
        } 
    } 
}
