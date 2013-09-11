using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Configuration; 

using Croc.Core.Extensions; 

 

 

namespace Croc.Core.Configuration 

{ 

    /// <summary> 

    /// Атрибут описывает тип конфигурационного элемента 

    /// </summary> 

    public sealed class SubsystemConfigurationElementTypeAttribute : Attribute 

    { 

        /// <summary> 

        /// Тип конфигурационного элемента 

        /// </summary> 

        public Type Type 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="type">тип конфиг-элемента подсистемы. Должен быть унаследован от SubsystemConfig</param> 

        public SubsystemConfigurationElementTypeAttribute(Type type) 

        { 

            CodeContract.Requires(type != null); 

 

 

            if (!type.IsInheritedFromType(typeof(SubsystemConfig))) 

                throw new ArgumentException("Тип должен быть унаследован от SubsystemConfig", "type"); 

 

 

            Type = type; 

        } 

    } 

}


