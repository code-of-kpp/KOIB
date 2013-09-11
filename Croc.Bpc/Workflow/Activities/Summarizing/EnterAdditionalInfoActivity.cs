using System; 
using System.Collections.Specialized; 
using Croc.Workflow.ComponentModel; 
using Croc.Bpc.Voting; 
namespace Croc.Bpc.Workflow.Activities.Summarizing 
{ 
    [Serializable] 
    public class EnterAdditionalInfoActivity : ElectionParametrizedActivity 
    { 
        [NonSerialized] 
        protected Line _currentLine; 
        public int CurrentLineIndex 
        { 
            get; 
            protected set; 
        } 
        public int? CurrentLineValue 
        { 
            get 
            { 
                return _currentLine.Value; 
            } 
        } 
        public int CurrentLineNumber 
        { 
            get 
            { 
                return _currentLine.Num; 
            } 
        } 
        public string CurrentLineAlphaNumber 
        { 
            get 
            { 
                return _currentLine.AdditionalNum; 
            } 
        } 
        public ListDictionary ElectionProtocolParameters 
        { 
            get 
            { 
                var protocolParams = new ListDictionary 
                                         { 
                                             {"Election", Election}, 
                                             {"withResults", true} 
                                         }; 
                return protocolParams; 
            } 
        } 
        protected override void Initialize(WorkflowExecutionContext context) 
        { 
            base.Initialize(context); 
            InitLineIterator(context); 
        } 
        #region Итератор строк протокола 
        private void InitLineIterator(WorkflowExecutionContext context) 
        { 
            if (!context.Restoring) 
            { 
                CurrentLineIndex = -1; 
            } 
            else if (0 <= CurrentLineIndex && CurrentLineIndex < Election.Protocol.Lines.Length) 
            { 
                _currentLine = Election.Protocol.Lines[CurrentLineIndex]; 
            } 
        } 
        public NextActivityKey MoveNextLine( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (++CurrentLineIndex < Election.Protocol.Lines.Length) 
            { 
                _currentLine = Election.Protocol.Lines[CurrentLineIndex]; 
                return BpcNextActivityKeys.Yes; 
            } 
            return BpcNextActivityKeys.No; 
        } 
        public NextActivityKey MovePreviousLine( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (CurrentLineIndex > 0) 
                CurrentLineIndex -= 2; 
            else 
                CurrentLineIndex -= 1; 
            return MoveNextLine(context, parameters); 
        } 
        public NextActivityKey ResetLineEnumerator( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            CurrentLineIndex = -1; 
            return context.DefaultNextActivityKey; 
        } 
        #endregion 
        public NextActivityKey CompileChecksAndAutoLineAssembly( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (ElectionIndex == 0) 
            { 
                _scannerManager.SetIndicator("Подготовка..."); 
                _electionManager.SourceData.BindAutoLinesAndChecksCountMethods(); 
            } 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey IsAutoCalculatedLine( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _currentLine.IsAutoCalculated ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey IsAutoCalculatedAndNotFirstLine( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _currentLine.IsAutoCalculated && CurrentLineIndex > 0 
                ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey CalculateLineValue( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _currentLine.CalculateValue(); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey AcceptEnteredLineValue( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _currentLine.Value = int.Parse(CommonActivity.LastReadedValue); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey CheckControlRelations( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (!_electionManager.NeedExecuteCheckExpressions) 
                return BpcNextActivityKeys.Yes; 
            return Election.IsControlRelationsSatisfied() ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 
        } 
    } 
}
