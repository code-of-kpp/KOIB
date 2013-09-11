using System; 
namespace Croc.Core 
{ 
    public class ConfigUpdatedEventArgs : EventArgs 
    { 
        public string SubsystemName 
        { 
            get; 
            private set; 
        } 
        public string UpdatedParameterName 
        { 
            get; 
            private set; 
        } 
        public object OldValue 
        { 
            get; 
            private set; 
        } 
        public object NewValue 
        { 
            get; 
            private set; 
        } 
        public ConfigUpdatedEventArgs(string subsystemName, string updatedParameterName, object oldValue, object newValue) 
        { 
            SubsystemName = subsystemName; 
            UpdatedParameterName = updatedParameterName; 
            OldValue = oldValue; 
            NewValue = newValue; 
        } 
    } 
}
