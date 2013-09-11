using System; 
using Croc.Core.Extensions; 
namespace Croc.Core.Configuration 
{ 
    public sealed class SubsystemConfigurationElementTypeAttribute : Attribute 
    { 
        public Type Type 
        { 
            get; 
            private set; 
        } 
        public SubsystemConfigurationElementTypeAttribute(Type type) 
        { 
            CodeContract.Requires(type != null); 
            if (!type.IsInheritedFromType(typeof(SubsystemConfig))) 
                throw new ArgumentException("Тип должен быть унаследован от SubsystemConfig", "type"); 
            Type = type; 
        } 
    } 
}
