using System; 

using System.Text.RegularExpressions; 

 

 

namespace Croc.Workflow.ComponentModel 

{ 

    /// <summary> 

    /// Ключ для определения следующего действия, которое нужно выполнить 

    /// </summary> 

    [Serializable] 

    public class NextActivityKey 

    { 

        public readonly string Value; 

 

 

        public NextActivityKey(string value) 

        { 

            if (string.IsNullOrEmpty(value)) 

                throw new ArgumentNullException("Не задано значение ключа"); 

 

 

            var regex = new Regex(@"^\w+$"); 

            if (!regex.IsMatch(value)) 

                throw new ArgumentException("Значение ключа может содержать только буквы, цифры и '_': " + value); 

 

 

            Value = value; 

        } 

 

 

        public override bool Equals(object obj) 

        { 

            if (obj == null) 

                return false; 

 

 

            var other = obj as NextActivityKey; 

            if (other == null) 

                return false; 

 

 

            return other.Value.Equals(this.Value); 

        } 

 

 

        public override int GetHashCode() 

        { 

            return Value.GetHashCode(); 

        } 

 


 
        public override string ToString() 

        { 

            return Value; 

        } 

    } 

}


