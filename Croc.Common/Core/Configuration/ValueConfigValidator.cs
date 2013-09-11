using System; 
using System.Configuration; 
namespace Croc.Core.Configuration 
{ 
    public class IntValueConfigValidator : ConfigurationValidatorBase 
    { 
        public int MinValue { get; set; } 
        public int MaxValue { get; set; } 
        public override bool CanValidate(Type type) 
        { 
            return type.Equals(typeof(ValueConfig<int>)); 
        } 
        public override void Validate(object value) 
        { 
            var val = ((ValueConfig<int>)value).Value; 
            if (MinValue > val || val > MaxValue) 
                throw new ArgumentException( 
                    string.Format("Значение должно быть в диапазоне [{0}, {1}]", MinValue, MaxValue)); 
        } 
    } 
    [AttributeUsage(AttributeTargets.Property)] 
    public class IntValueConfigValidatorAttribute : ConfigurationValidatorAttribute 
    { 
        public int MinValue { get; set; } 
        public int MaxValue { get; set; } 
        public IntValueConfigValidatorAttribute() 
        { 
            MinValue = int.MinValue; 
            MaxValue = int.MaxValue; 
        } 
        public override ConfigurationValidatorBase ValidatorInstance 
        { 
            get 
            { 
                return new IntValueConfigValidator 
                           { 
                               MinValue = MinValue, 
                               MaxValue = MaxValue 
                           }; 
            } 
        } 
    } 
}
