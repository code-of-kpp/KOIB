using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities 
{ 
    public static class BpcNextActivityKeys_VotingTime 
    { 
        public static NextActivityKey ElectionDayHasNotCome = new NextActivityKey("ElectionDayHasNotCome"); 
        public static NextActivityKey ElectionDayNow = new NextActivityKey("ElectionDayNow"); 
        public static NextActivityKey ElectionDayPassed = new NextActivityKey("ElectionDayPassed"); 
        public static NextActivityKey VotingTimeNow = new NextActivityKey("VotingTimeNow"); 
        public static NextActivityKey SomeTimeToVotingStart = new NextActivityKey("SomeTimeToVotingStart"); 
        public static NextActivityKey NotVotingTime = new NextActivityKey("NotVotingTime"); 
    } 
}
