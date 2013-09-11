using System; 
namespace Croc.Core.Extensions 
{ 
    public static class TimeSpanExtensions 
    { 
        public static TimeSpan RoundMinutes(this TimeSpan time) 
        { 
            return new TimeSpan( 
                time.Days, 
                time.Hours, 
                time.Seconds >= 30 ? time.Minutes + 1 : time.Minutes, 
                0, 
                0); 
        } 
    } 
}
