using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Workflow.ComponentModel 

{ 

    /// <summary> 

    /// Приоритет действия 

    /// </summary> 

    /// <remarks> 

    /// Приоритет, значение которого меньше, считается более высоким 

    /// </remarks> 

    public class ActivityPriority 

    { 

        /// <summary> 

        /// Самый высокий приоритет 

        /// </summary> 

        public static ActivityPriority Highest = new ActivityPriority() { _value = 1 }; 

        /// <summary> 

        /// Самый низкий приоритет 

        /// </summary> 

        public static ActivityPriority Lowest = new ActivityPriority() { _value = 10 }; 

        /// <summary> 

        /// Приоритет по умолчанию 

        /// </summary> 

        public static ActivityPriority Default = new ActivityPriority() { _value = 5 }; 

 

 

        private int _value; 

        /// <summary> 

        /// Значение приоритета 

        /// </summary> 

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

            var other = obj as ActivityPriority; 


            return other != null && _value == other._value; 

        } 

    } 

}


