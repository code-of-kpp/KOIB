using System; 

using System.Collections.Generic; 

using Croc.Core.Configuration; 

 

 

namespace Croc.Core.Diagnostics 

{ 

    /// <summary> 

    /// Диспетчирезатор событий 

    /// </summary> 

    public static class EventDispatcher 

    { 

        /// <summary> 

        /// Список приеников событий (писателей) 

        /// </summary> 

        private static readonly List<EventWriterTriplet> s_eventWriterTriplets = new List<EventWriterTriplet>(); 

 

 

        /// <summary> 

        /// Поле в свойствах события для группировки 

        /// </summary> 

        public static string GroupByField { get; private set; } 

 

 

        /// <summary> 

        /// Фильтры событий 

        /// </summary> 

        public static List<IEventFilter> EventFilters = new List<IEventFilter>(); 

 

 

        /// <summary> 

        /// Инициализация доступных писателей 

        /// </summary> 

        /// <param name="config">Секция конфигурации</param> 

        public static void Init(DiagnosticsConfig config) 

        { 

            if (config != null) 

            { 

                if(config.EventFilters != null) 

                { 

                    foreach (FilterConfig filterConfig in config.EventFilters) 

                    { 

                        IEventFilter filter = ConstructObject(filterConfig.TypeName) as IEventFilter; 

                        if (filter != null) 

                        { 

                            filter.Init(filterConfig.Props); 

                            EventFilters.Add(filter); 

                        } 

                    } 

                } 


 
 

                if (config.Writers != null) 

                { 

                    GroupByField = config.GroupBy; 

                    foreach (WriterConfig writer in config.Writers) 

                    { 

                        EventWriterTriplet eventWriterTriplet = new EventWriterTriplet(); 

                        eventWriterTriplet.Writer = ConstructObject(writer.TypeName) as IEventWriter; 

                        if (eventWriterTriplet.Writer != null) 

                        { 

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

                                IEventWriterFilter filter = ConstructObject(filterConfig.TypeName) as IEventWriterFilter; 

                                if (filter != null) 

                                { 

                                    filter.Init(filterConfig.Props); 

                                    eventWriterTriplet.Filters.Add(filter); 

                                } 

                            } 

 

 

                            s_eventWriterTriplets.Add(eventWriterTriplet); 

                        } 

                    } 

                } 

            } 

        } 

 

 

        /// <summary> 

        /// Конструирует объект 

        /// </summary> 

        /// <param name="typeName">Имя типа</param> 

        /// <returns>null если не удалось создать</returns> 

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

 

 

        /// <summary> 

        /// Диспетчиризация события 

        /// </summary> 

        /// <param name="loggerEvent">Событие</param> 

        public static void Dispatch(LoggerEvent loggerEvent) 

        { 

            foreach (EventWriterTriplet eventWriterTriplet in s_eventWriterTriplets) 

            { 

                // если нужно отформатировать сообщение и есть параметры для форматирования 

                if (!eventWriterTriplet.Raw && loggerEvent.Properties.ContainsKey(LoggerEvent.PARAMETERS_PROPERTY)) 

                { 

                    loggerEvent[LoggerEvent.MESSAGE_PROPERTY] = 

                        string.Format((string)loggerEvent[LoggerEvent.MESSAGE_PROPERTY], 

                                      (object[])loggerEvent[LoggerEvent.PARAMETERS_PROPERTY]); 

                    loggerEvent.Properties.Remove(LoggerEvent.PARAMETERS_PROPERTY); 

                } 

 

 

                // проверка по фильтрам 

                bool accepted = true; 

                string message = eventWriterTriplet.Formatter.Format(loggerEvent); 

                foreach (IEventWriterFilter eventFilter in eventWriterTriplet.Filters) 

                { 

                    if (!eventFilter.Accepted(eventWriterTriplet, loggerEvent, message)) 

                    { 

                        accepted = false; 

                        break; 

                    } 

                } 

 

 


                if (!accepted) 

                { 

                    continue; 

                } 

 

 

                // вывод в протокол 

                eventWriterTriplet.Writer.Write(GetUniqueId(loggerEvent), message); 

            } 

        } 

 

 

        /// <summary> 

        /// Получить уникальный идентификатор журнала по событию 

        /// </summary> 

        /// <param name="loggerEvent">Событие</param> 

        /// <returns>Уникальный идентификатор журнала</returns> 

        public static string GetUniqueId(LoggerEvent loggerEvent) 

        { 

            string uniqueId = null; 

 

 

            // если указано поле для группировки, то ищем его в свойствах 

            if (!String.IsNullOrEmpty(GroupByField) && GroupByField.Trim().Length > 0) 

            { 

                if(loggerEvent.Properties.ContainsKey(GroupByField)) 

                { 

                    uniqueId = (string)loggerEvent[GroupByField]; 

                } 

            } 

 

 

            // если не смогли найти, то группируем по уровням 

            if(String.IsNullOrEmpty(uniqueId)) 

            { 

                uniqueId = loggerEvent.EventType.ToString(); 

            } 

 

 

            return uniqueId; 

        } 

    } 

}


