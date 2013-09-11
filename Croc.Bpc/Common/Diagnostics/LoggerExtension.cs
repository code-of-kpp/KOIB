using System; 
using System.Diagnostics; 
using System.Linq; 
using System.Reflection; 
using Croc.Core.Diagnostics; 
using Croc.Core.Diagnostics.Default; 
namespace Croc.Bpc.Diagnostics 
{ 
    public static class BpcLoggerExtensions 
    { 
        public const string MESSAGEID_PROPERTY = "MessageId"; 
        private static readonly LoggerEvent s_dummyVerboseEvent = 
            new LoggerEvent { EventType = TraceEventType.Verbose }; 
        private static void InternalLog( 
            this ILogger logger, TraceEventType traceEventType, Message message, EventProperties properties) 
        { 
            properties[MESSAGEID_PROPERTY] = (int)message; 
#if DEBUG 
            properties[LoggerEvent.MESSAGE_PROPERTY] = GetMessageBody(message); 
            properties[LoggerEvent.METHODNAME_PROPERTY] = 
                LoggerExtensions.GetCallerMethodName(typeof(BpcLoggerExtensions)); 
#else 
            properties[LoggerEvent.MESSAGE_PROPERTY] = string.Empty; 
#endif 
            logger.Log(traceEventType, properties); 
        } 
        public static string GetMessageBody(Message message) 
        { 
            var messageBody = message.ToString(); 
            var atts = typeof(Message).GetField( 
                messageBody, 
                BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public) 
                .GetCustomAttributes(true); 
            foreach (var att in atts.OfType<MessageParametersAttribute>()) 
                return att.Body; 
            return messageBody; 
        } 
        public static void LogVerbose(this ILogger logger, Message message, Func<object[]> getArgsFunc) 
        { 
            if (logger.IsAcceptedByEventType(s_dummyVerboseEvent)) 
            { 
                logger.InternalLog(TraceEventType.Verbose, message, 
                    new EventProperties {  
                    { LoggerEvent.PARAMETERS_PROPERTY, getArgsFunc() } 
                }); 
            } 
        } 
        public static void LogVerbose(this ILogger logger, Message message, params object[] args) 
        { 
            if (logger.IsAcceptedByEventType(s_dummyVerboseEvent)) 
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
        public static void LogWarning(this ILogger logger, Message message, Exception ex, params object[] args) 
        { 
            logger.InternalLog(TraceEventType.Warning, message, 
                new EventProperties {  
                    { LoggerEvent.EXCEPTION_PROPERTY, ex }, 
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
        public static void LogError(this ILogger logger, Message message, Exception ex, params object[] args) 
        { 
            logger.InternalLog(TraceEventType.Error, message, 
                new EventProperties {  
                    { LoggerEvent.EXCEPTION_PROPERTY, ex }, 
                    { LoggerEvent.PARAMETERS_PROPERTY, args } 
                }); 
        } 
    } 
}
