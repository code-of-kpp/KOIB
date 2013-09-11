using System; 

 

 

namespace Croc.Bpc.Common.Diagnostics 

{ 

    /// <summary> 

    /// Атрибут диагоностического сообщения 

    /// </summary> 

    [AttributeUsage(AttributeTargets.Field)] 

    public sealed class MessageParametersAttribute : Attribute  

    { 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="body">Текст сообщения</param> 

        public MessageParametersAttribute(string body)  

        { 

            Body = body; 

        } 

 

 

        /// <summary> 

        /// Название клавиши 

        /// </summary> 

        public string Body { get; set; } 

    } 

}


