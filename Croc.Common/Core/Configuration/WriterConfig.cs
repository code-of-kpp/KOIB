using System.Configuration; 

 

 

namespace Croc.Core.Configuration 

{ 

    /// <summary> 

    /// Конфигурация писателя в журнал 

    /// </summary> 

    public class WriterConfig : ConfigurableClassConfig 

    { 

        /// <summary> 

        /// Альтернативный форматтер событий диагностики 

        /// </summary> 

        [ConfigurationProperty("formatter", IsRequired = false)] 

        public FormatterConfig EventFormatter 

        { 

            get 

            { 

                return (FormatterConfig)this["formatter"]; 

            } 

        } 

 

 

        /// <summary> 

        /// Фильтры (срабатывают непосредственно перед записью) 

        /// </summary> 

        [ConfigurationProperty("filters", IsDefaultCollection = false, IsRequired = false)] 

        [ConfigurationCollection(typeof(FilterConfig), AddItemName = "filter")] 

        public FilterConfigCollection EventFilters 

        { 

            get { return (FilterConfigCollection)base["filters"]; } 

        } 

    } 

}


