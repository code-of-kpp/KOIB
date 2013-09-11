using System.Configuration; 

 

 

namespace Croc.Core.Configuration 

{ 

    /// <summary> 

    /// Конфигурация диагностики 

    /// </summary> 

    public class DiagnosticsConfig : ConfigurationElement 

    { 

        /// <summary> 

        /// Группировка собщений (указывается имя свойства, по которому группируем) 

        /// </summary> 

        [ConfigurationProperty("groupBy", IsRequired = false)] 

        public string GroupBy 

        { 

            get 

            { 

                return (string)this["groupBy"]; 

            } 

            set 

            { 

                this["groupBy"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Дополнительные фильтры (фильтр по категориям устанавливается по умолчанию) 

        /// </summary> 

        [ConfigurationProperty("eventFilters", IsDefaultCollection = false, IsRequired = false)] 

        [ConfigurationCollection(typeof(FilterConfig), AddItemName = "filter")] 

        public FilterConfigCollection EventFilters 

        { 

            get { return (FilterConfigCollection)base["eventFilters"]; } 

        } 

 

 

        /// <summary> 

        /// Писатели 

        /// </summary> 

        [ConfigurationProperty("writers", IsDefaultCollection = false, IsRequired = true)] 

        [ConfigurationCollection(typeof(WriterConfig), AddItemName = "writer")] 

        public WriterConfigCollection Writers 

        { 

            get { return (WriterConfigCollection)base["writers"]; } 

        } 

    } 

}


