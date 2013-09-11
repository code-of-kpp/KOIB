using System; 
using System.Collections.Generic; 
using System.Linq; 
using Croc.Core.Configuration; 
namespace Croc.Core.Diagnostics 
{ 
    public static class EventDispatcher 
    { 
        private static readonly List<EventWriterTriplet> s_eventWriterTriplets = new List<EventWriterTriplet>(); 
        public static string GroupByField { get; private set; } 
        public static bool GroupByFieldDefined { get; private set; } 
        public static List<IEventFilter> EventFilters = new List<IEventFilter>(); 
        public static void Init(DiagnosticsConfig config) 
        { 
            if (config == null) 
                return; 
            if (config.EventFilters != null) 
            { 
                foreach (FilterConfig filterConfig in config.EventFilters) 
                { 
                    var filter = ConstructObject(filterConfig.TypeName) as IEventFilter; 
                    if (filter != null) 
                    { 
                        filter.Init(filterConfig.Props); 
                        EventFilters.Add(filter); 
                    } 
                } 
            } 
            if (config.Writers == null) 
                return; 
            GroupByField = config.GroupBy.Trim(); 
            GroupByFieldDefined = !string.IsNullOrEmpty(GroupByField); 
            foreach (WriterConfig writer in config.Writers) 
            { 
                var eventWriterTriplet = new EventWriterTriplet 
                                             { 
                                                 Writer = ConstructObject(writer.TypeName) as IEventWriter 
                                             }; 
                if (eventWriterTriplet.Writer == null) 
                    continue; 
                eventWriterTriplet.Writer.Init(writer.Props); 
                eventWriterTriplet.Formatter = ConstructObject(writer.EventFormatter.TypeName) as IEventFormatter; 
                if (eventWriterTriplet.Formatter == null) 
                { 
                    eventWriterTriplet.Formatter = new Default.EventFormatter(); 
                } 
                else 
                { 
                    eventWriterTriplet.Raw = writer.EventFormatter.Raw; 
                } 
                eventWriterTriplet.Formatter.Init(writer.EventFormatter.Props); 
                foreach (FilterConfig filterConfig in writer.EventFilters) 
                { 
                    var filter = ConstructObject(filterConfig.TypeName) as IEventWriterFilter; 
                    if (filter != null) 
                    { 
                        filter.Init(filterConfig.Props); 
                        eventWriterTriplet.Filters.Add(filter); 
                    } 
                } 
                s_eventWriterTriplets.Add(eventWriterTriplet); 
            } 
        } 
        private static object ConstructObject(string typeName) 
        { 
            if (!string.IsNullOrEmpty(typeName)) 
            { 
                var type = Type.GetType(typeName, false); 
                if (type != null) 
                { 
                    try 
                    { 
                        return Activator.CreateInstance(type); 
                    } 
                    catch 
                    { 
                        return null; 
                    } 
                } 
            } 
            return null; 
        } 
        public static void Dispatch(LoggerEvent loggerEvent) 
        { 
            foreach (var item in s_eventWriterTriplets) 
            { 
                var triplet = item; 
                if (!triplet.Raw && loggerEvent.Properties.ContainsKey(LoggerEvent.PARAMETERS_PROPERTY)) 
                { 
                    loggerEvent[LoggerEvent.MESSAGE_PROPERTY] = 
                        string.Format((string) loggerEvent[LoggerEvent.MESSAGE_PROPERTY], 
                                      (object[]) loggerEvent[LoggerEvent.PARAMETERS_PROPERTY]); 
                    loggerEvent.Properties.Remove(LoggerEvent.PARAMETERS_PROPERTY); 
                } 
                var message = triplet.Formatter.Format(loggerEvent); 
                try 
                { 
                    var accepted = triplet.Filters.All(filter => filter.Accepted(triplet, loggerEvent, message)); 
                    if (!accepted) 
                        continue; 
                    triplet.Writer.Write(GetUniqueId(loggerEvent), message); 
                } 
                catch (Exception ex) 
                { 
                    throw new Exception("Ошибка при записи сообщения:\n" + message, ex); 
                } 
            } 
        } 
        public static string GetUniqueId(LoggerEvent loggerEvent) 
        { 
            string uniqueId = null; 
            if (GroupByFieldDefined && loggerEvent.Properties.ContainsKey(GroupByField)) 
            { 
                uniqueId = (string)loggerEvent[GroupByField]; 
            } 
            if (string.IsNullOrEmpty(uniqueId)) 
            { 
                uniqueId = loggerEvent.EventType.ToString(); 
            } 
            return uniqueId; 
        } 
    } 
}
