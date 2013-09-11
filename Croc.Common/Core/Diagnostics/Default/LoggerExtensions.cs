using System; 

using System.Diagnostics; 

using System.Reflection; 

 

 

namespace Croc.Core.Diagnostics.Default 

{ 

    /// <summary> 

    /// ????? ? extension-???????? ??? <see cref="ILogger"/>. 

    /// </summary> 

    public static class LoggerExtensions 

    { 

        /// <summary> 

        /// ?????? ??????? ??????????? ? ????????? "???????" 

        /// </summary> 

        private static LoggerEvent _dummyVerboseEvent = new LoggerEvent() { EventType = TraceEventType.Verbose }; 

 

 

        /// <summary> 

        /// ?????????? ??????? ? ??????. 

        /// </summary> 

        /// <param name="logger"></param> 

        /// <param name="eventType">???????? ???????</param> 

        /// <param name="properties">?????? ???????</param> 

        public static void Log(this ILogger logger, TraceEventType eventType, 

                               EventProperties properties) 

        { 

            if(properties.ContainsKey(LoggerEvent.PARAMETERS_PROPERTY)) 

            { 

                if(properties[LoggerEvent.PARAMETERS_PROPERTY] == null || 

                   ((object[])properties[LoggerEvent.PARAMETERS_PROPERTY]).Length <= 0) 

                { 

                    properties.Remove(LoggerEvent.PARAMETERS_PROPERTY); 

                } 

            } 

            LoggerEvent loggerEvent = new LoggerEvent(); 

            loggerEvent.EventType = eventType; 

            foreach (var property in properties) 

            { 

                loggerEvent[property.Key] = property.Value; 

            } 

            if (!loggerEvent.Properties.ContainsKey(LoggerEvent.METHODNAME_PROPERTY)) 

            { 

                loggerEvent[LoggerEvent.METHODNAME_PROPERTY] = GetCallerMethodName(typeof(LoggerExtensions)); 

            } 

            logger.Log(loggerEvent); 

        } 

 

 

        public static void LogVerbose(this ILogger logger, string message, params object[] args) 


        { 

            if (logger.IsAcceptedByEventType(_dummyVerboseEvent)) 

            { 

                logger.Log(TraceEventType.Verbose, 

                           new EventProperties {  

                                               { LoggerEvent.MESSAGE_PROPERTY, message }, 

                                               { LoggerEvent.PARAMETERS_PROPERTY, args } 

                                           }); 

            } 

        } 

 

 

        public static void LogInfo(this ILogger logger, string message, params object[] args) 

        { 

            logger.Log(TraceEventType.Information, 

                       new EventProperties {  

                                               { LoggerEvent.MESSAGE_PROPERTY, message }, 

                                               { LoggerEvent.PARAMETERS_PROPERTY, args } 

                                           }); 

        } 

 

 

        public static void LogWarning(this ILogger logger, string message, params object[] args) 

        { 

            logger.Log(TraceEventType.Warning, 

                       new EventProperties {  

                                               { LoggerEvent.MESSAGE_PROPERTY, message }, 

                                               { LoggerEvent.PARAMETERS_PROPERTY, args } 

                                           }); 

        } 

 

 

        public static void LogError(this ILogger logger, string message, params object[] args) 

        { 

            logger.Log(TraceEventType.Error, 

                       new EventProperties {  

                                               { LoggerEvent.MESSAGE_PROPERTY, message }, 

                                               { LoggerEvent.PARAMETERS_PROPERTY, args } 

                                           }); 

        } 

 

 

        public static void LogException(this ILogger logger, string message, Exception ex, params object[] args) 

        { 

            logger.Log(TraceEventType.Error, 

                       new EventProperties {  

                                               { LoggerEvent.MESSAGE_PROPERTY, message },  

                                               { LoggerEvent.EXCEPTION_PROPERTY, ex }, 

                                               { LoggerEvent.PARAMETERS_PROPERTY, args } 

                                           }); 


        } 

 

 

        /// <summary> 

        /// ?????????? ??? ??????, ??????? ????????? ? ????? ?? ??????? ?????? ?????? 

        /// </summary> 

        /// <param name="type">???, ?????? ???????? ??????????</param> 

        /// <returns>?????????.?????????</returns> 

        public static string GetCallerMethodName(Type type) 

        { 

            StackFrame[] frames = new StackTrace().GetFrames(); 

            if (frames != null && frames.Length > 1) 

            { 

                for (int i = 1; i < frames.Length; i++) 

                { 

                    MethodBase method = frames[i].GetMethod(); 

 

 

                    if (method.DeclaringType == type) 

                    { 

                        continue; 

                    } 

 

 

                    return string.Format("{0}.{1}", method.DeclaringType.Name, method.Name); 

                } 

            } 

 

 

            // ?? ????? (?) 

            return ""; 

        } 

    } 

}


