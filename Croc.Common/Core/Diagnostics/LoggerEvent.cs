using System; 

using System.Diagnostics; 

 

 

namespace Croc.Core.Diagnostics 

{ 

	/// <summary> 

	/// ???????? ??????? ??? ??????? ILogger 

	/// </summary> 

	public class LoggerEvent 

	{ 

        public const string MESSAGE_PROPERTY = "Message"; 

        public const string EXCEPTION_PROPERTY = "Exception"; 

        public const string TIMESTAMP_PROPERTY = "Timestamp"; 

        public const string THREAD_ID = "ThreadId"; 

        public const string METHODNAME_PROPERTY = "MethodName"; 

	    public const string LOGGERNAME_PROPERTY = "Logger"; 

        public const string PARAMETERS_PROPERTY = "Params"; 

        public const string EVENTTYPE_PROPERTY = "EventType"; 

 

 

		/// <summary> 

		/// ???????? ??????? 

		/// </summary> 

		public TraceEventType EventType { get; set; } 

 

 

		/// <summary> 

		/// ?????? ???????. ???? ???-????????. 

		/// </summary> 

        public EventProperties Properties { get; set; } 

 

 

        /// <summary> 

        /// ?????????? ?? ?????? ??????? 

        /// </summary> 

        public object this[string index] 

	    { 

	        get 

	        { 

                if (Properties.ContainsKey(index)) 

                { 

                    return Properties[index]; 

                } 

                else 

                { 

                    if (index == EVENTTYPE_PROPERTY) 

                    { 

                        return EventType; 

                    } 


                    else 

                    { 

                        return string.Empty; 

                    } 

                } 

	        } 

            set 

            { 

                if (index != EVENTTYPE_PROPERTY) 

                { 

                    Properties[index] = value; 

                } 

                else 

                { 

                    EventType = (TraceEventType)value; 

                } 

            } 

	    } 

 

 

        /// <summary> 

        /// ?????????? ????????????? 

        /// </summary> 

        public Guid Id { get; private set; } 

 

 

        /// <summary> 

        /// ??????????? 

        /// </summary> 

        public LoggerEvent() 

        { 

            Id = Guid.NewGuid(); 

            Properties = new EventProperties(); 

            LoggingUtils.FillCommonContextProperies(Properties); 

        } 

	} 

}


