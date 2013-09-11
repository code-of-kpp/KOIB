using System; 
using System.Text.RegularExpressions; 
namespace Croc.Workflow.ComponentModel 
{ 
    [Serializable] 
    public class NextActivityKey 
    { 
        public static readonly NextActivityKey DefaultNextActivityKey =  
            new NextActivityKey { Name = "@@Default" }; 
        public string Name 
        { 
            get; 
            protected set; 
        } 
        private NextActivityKey() 
        { 
        } 
        public NextActivityKey(string name) 
        { 
            if (string.IsNullOrEmpty(name)) 
                throw new ArgumentNullException("name", "Не задано имя ключа"); 
            var regex = new Regex(@"^\w+$"); 
            if (!regex.IsMatch(name)) 
                throw new ArgumentException("Имя ключа может содержать только буквы, цифры и '_': " + name); 
            Name = name; 
        } 
        public override bool Equals(object obj) 
        { 
            if (obj == null) 
                return false; 
            var other = obj as NextActivityKey; 
            if (other == null) 
                return false; 
            return other.Name.Equals(Name); 
        } 
        public override int GetHashCode() 
        { 
            return Name.GetHashCode(); 
        } 
        public override string ToString() 
        { 
            return Name; 
        } 
    } 
}
