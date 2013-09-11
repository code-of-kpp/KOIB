using System; 
using System.Globalization; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.SystemMenu 
{ 
    public class DateTimeSettingsActivity : BpcCompositeActivity 
    { 
        private DateTime _date; 
        private DateTime _time; 
        public NextActivityKey CheckNewDate( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var dateValid = DateTime.TryParseExact( 
                CommonActivity.LastReadedValue, 
                "ddMMyyyy", 
                null, 
                DateTimeStyles.None, 
                out _date); 
            if (_date.Year < 2010) 
                dateValid = false; 
            return dateValid ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey CheckNewTime( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return DateTime.TryParseExact( 
                CommonActivity.LastReadedValue, 
                "HHmm", 
                null, 
                DateTimeStyles.None, 
                out _time) 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey SetDateTime( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var utcDateTime = _date.AddHours(_time.Hour).AddMinutes(_time.Minute).ToUniversalTime(); 
            _syncManager.SetSystemTime(utcDateTime); 
            return context.DefaultNextActivityKey; 
        } 
    } 
}
