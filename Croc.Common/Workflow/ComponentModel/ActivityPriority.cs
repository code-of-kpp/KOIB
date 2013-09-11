using Croc.Core; 
namespace Croc.Workflow.ComponentModel 
{ 
    public class ActivityPriority 
    { 
        public static ActivityPriority Highest = new ActivityPriority { _value = 1 }; 
        public static ActivityPriority Lowest = new ActivityPriority { _value = 10 }; 
        public static ActivityPriority Default = new ActivityPriority { _value = 5 }; 
        private int _value; 
        public int Value 
        { 
            get 
            { 
                return _value; 
            } 
        } 
        private ActivityPriority() 
        { 
        } 
        public ActivityPriority(int value) 
        { 
            CodeContract.Requires(Highest.Value <= value && value <= Lowest.Value); 
            _value = value; 
        } 
        public static bool operator <(ActivityPriority p1, ActivityPriority p2) 
        { 
            return p1._value > p2._value; 
        } 
        public static bool operator <=(ActivityPriority p1, ActivityPriority p2) 
        { 
            return p1._value >= p2._value; 
        } 
        public static bool operator >(ActivityPriority p1, ActivityPriority p2) 
        { 
            return p1._value < p2._value; 
        } 
        public static bool operator >=(ActivityPriority p1, ActivityPriority p2) 
        { 
            return p1._value <= p2._value; 
        } 
        public static bool operator ==(ActivityPriority p1, ActivityPriority p2) 
        { 
            return p1._value == p2._value; 
        } 
        public static bool operator !=(ActivityPriority p1, ActivityPriority p2) 
        { 
            return p1._value != p2._value; 
        } 
        public override int GetHashCode() 
        { 
            return _value; 
        } 
        public override bool Equals(object obj) 
        { 
            return obj is ActivityPriority 
                       ? _value == ((ActivityPriority)obj)._value 
                       : false; 
        } 
    } 
}
