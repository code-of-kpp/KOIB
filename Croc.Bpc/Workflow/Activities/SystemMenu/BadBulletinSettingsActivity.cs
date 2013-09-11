using System; 
using System.Linq; 
using Croc.Bpc.Voting; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.SystemMenu 
{ 
    public class BadBulletinSettingsActivity : BpcCompositeActivity 
    { 
        private BlankMarking _badMarking; 
        public int BadBulletinMarking 
        { 
            get 
            { 
                return (int)_recognitionManager.GetBlankMarking(BlankType.Bad); 
            } 
        } 
        public int BadStampBulletinMarking 
        { 
            get 
            { 
                return (int)_recognitionManager.GetBlankMarking(BlankType.BadStamp); 
            } 
        } 
        public int MaxMarkingValue 
        { 
            get  
            {  
                var values = (short[])Enum.GetValues(typeof (BlankMarking)); 
                return values.Max(); 
            } 
        } 
        public NextActivityKey SaveBadMarking( 
           WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return TryParseBlankMarking(CommonActivity.LastReadedValue, out _badMarking) 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey SaveBadStampMarkingApplyNewConfig( 
           WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            BlankMarking badStampMarking; 
            if (!TryParseBlankMarking(CommonActivity.LastReadedValue, out badStampMarking)) 
                return BpcNextActivityKeys.No; 
            _recognitionManager.SetBlankMarking(BlankType.Bad, _badMarking); 
            _recognitionManager.SetBlankMarking(BlankType.BadStamp, badStampMarking); 
            return BpcNextActivityKeys.Yes; 
        } 
        private static bool TryParseBlankMarking(string numericValue, out BlankMarking marking) 
        { 
            try 
            { 
                var markingShort = short.Parse(numericValue); 
                var values = (short[])Enum.GetValues(typeof (BlankMarking)); 
                foreach (var value in values.Where(value => markingShort == value)) 
                { 
                    marking = (BlankMarking) value; 
                    return true; 
                }    
            } 
            catch 
            { 
            } 
            marking = BlankMarking.DropWithoutMark; 
            return false; 
        } 
    } 
}
