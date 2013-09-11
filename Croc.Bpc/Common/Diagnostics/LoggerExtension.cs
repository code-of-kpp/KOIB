using System; 

using System.Diagnostics; 

using Croc.Core.Diagnostics; 

using Croc.Core.Diagnostics.Default; 

 

 

namespace Croc.Bpc.Common.Diagnostics 

{ 

	/// <summary> 

	/// Класс с extension-методами для <see cref="ILogger"/>. 

	/// </summary> 

	public static class BpcLoggerExtensions 

	{ 

        /// <summary> 

        /// Имя идентификатора сообщения 

        /// </summary> 

        public const string MESSAGEID_PROPERTY = "MessageId"; 

 

 

        /// <summary> 

        /// Пустое событие диагностики с важностью "отладка" 

        /// </summary> 

        private static LoggerEvent _dummyVerboseEvent = new LoggerEvent() { EventType = TraceEventType.Verbose }; 

 

 

        private static void InternalLog( 

            this ILogger logger, TraceEventType traceEventType, Message message, EventProperties properties) 

        { 

            properties[LoggerEvent.MESSAGE_PROPERTY] = GetMessageBody(message); 

            properties[MESSAGEID_PROPERTY] = message; 

            properties[LoggerEvent.METHODNAME_PROPERTY] =  

                LoggerExtensions.GetCallerMethodName(typeof(BpcLoggerExtensions)); 

 

 

            logger.Log(traceEventType, properties); 

        } 

 

 

	    public static string GetMessageBody(Message message) 

	    { 

	        string messageBody = message.ToString(); 

            var atts = message.GetType().GetField(messageBody) 

                .GetCustomAttributes(typeof(MessageParametersAttribute), true); 

 

 

            foreach (Attribute att in atts) 

	        { 

	            if (att is MessageParametersAttribute) 

                    messageBody = ((MessageParametersAttribute)att).Body; 

	        } 


 
 

	        return messageBody; 

	    } 

 

 

	    public static void LogVerbose(this ILogger logger, Message message, params object[] args) 

        { 

            if (logger.IsAcceptedByEventType(_dummyVerboseEvent)) 

            { 

                logger.InternalLog(TraceEventType.Verbose, message, 

                    new EventProperties {  

                    { LoggerEvent.PARAMETERS_PROPERTY, args } 

                }); 

            } 

        } 

 

 

        public static void LogInfo(this ILogger logger, Message message, params object[] args) 

        { 

            logger.InternalLog(TraceEventType.Information, message, 

                new EventProperties {  

                    { LoggerEvent.PARAMETERS_PROPERTY, args } 

                }); 

        } 

 

 

        public static void LogWarning(this ILogger logger, Message message, params object[] args) 

        { 

            logger.InternalLog(TraceEventType.Warning, message, 

                new EventProperties {  

                    { LoggerEvent.PARAMETERS_PROPERTY, args } 

                }); 

        } 

 

 

        public static void LogError(this ILogger logger, Message message, params object[] args) 

        { 

            logger.InternalLog(TraceEventType.Error, message, 

                new EventProperties {  

                    { LoggerEvent.PARAMETERS_PROPERTY, args } 

                }); 

        } 

 

 

        public static void LogException(this ILogger logger, Message message, Exception ex, params object[] args) 

        { 

            logger.InternalLog(TraceEventType.Error, message, 

                new EventProperties {  

                    { LoggerEvent.EXCEPTION_PROPERTY, ex }, 


                    { LoggerEvent.PARAMETERS_PROPERTY, args } 

                }); 

        } 

	} 

}


