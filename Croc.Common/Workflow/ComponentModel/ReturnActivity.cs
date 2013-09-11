using System; 
namespace Croc.Workflow.ComponentModel 
{ 
    [Serializable] 
    public class ReturnActivity : Activity 
    { 
        private const string NAME_FORMAT_STRING = "@@Return({0})"; 
        public NextActivityKey Result 
        { 
            get; 
            private set; 
        } 
        public ReturnActivity(NextActivityKey result) 
        { 
            Result = result; 
            Name = string.Format(NAME_FORMAT_STRING, result); 
            Tracking = false; 
        } 
    } 
}
