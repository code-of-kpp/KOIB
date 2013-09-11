using System.Collections.Generic; 
using System.Linq; 
using Croc.Bpc.Scanner; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.SystemMenu 
{ 
    public class BlankPaperTypeSettingsActivity : BpcCompositeActivity 
    { 
        private Dictionary<int, PaperType> _papers; 
        private readonly List<PaperType> _types = new List<PaperType> 
                                                      { 
                                                          PaperType.Normal, 
                                                          PaperType.Thick, 
                                                          PaperType.Thin 
                                                      }; 
        private int _paperTypeIndex; 
        public int CurrentMarkerIndex 
        { 
            get; 
            protected set; 
        } 
        public int CurrentMarker 
        { 
            get 
            { 
                return _papers.Keys.ToArray()[CurrentMarkerIndex]; 
            } 
        } 
        public string CurrentPaperTypeStr 
        { 
            get 
            { 
                switch (_types[_paperTypeIndex]) 
                { 
                    case PaperType.Normal: 
                        return "Обычная"; 
                    case PaperType.Thick: 
                        return "Тонкая"; 
                    case PaperType.Thin: 
                        return "Толстая"; 
                } 


                return _types[_paperTypeIndex].ToString(); 
            } 
        } 
        public NextActivityKey ResetMarkerEnumerator( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            CurrentMarkerIndex = -1; 
            _papers = _scannerManager.BlanksPaperType; 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey ResetPaperTypeEnumerator( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _paperTypeIndex = _types.IndexOf(_papers.Values.ToArray()[CurrentMarkerIndex]); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey MoveNextMarker( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (++CurrentMarkerIndex < _papers.Count) 
            { 
                return BpcNextActivityKeys.Yes; 
            } 
            CurrentMarkerIndex--; 
            return BpcNextActivityKeys.No; 
        } 
        public NextActivityKey MoveNextPaperType( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (++_paperTypeIndex >= _types.Count) 
            { 
                _paperTypeIndex = 0; 
            } 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey MovePreviosMarker( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            CurrentMarkerIndex -= 2; 
            if (CurrentMarkerIndex < -1) 
            { 
                CurrentMarkerIndex = -1; 
            } 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey UpdatePaperType( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _scannerManager.SetBlankPaperType(CurrentMarker, _types[_paperTypeIndex]); 
            _papers[CurrentMarkerIndex] = _types[_paperTypeIndex]; 
            return context.DefaultNextActivityKey; 
        } 
    } 
}
