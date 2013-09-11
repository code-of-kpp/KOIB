using System.Configuration; 
namespace Croc.Core.Configuration 
{ 
    public class WriterConfig : ConfigurableClassConfig 
    { 
        [ConfigurationProperty("formatter", IsRequired = false)] 
        public FormatterConfig EventFormatter 
        { 
            get 
            { 
                return (FormatterConfig)this["formatter"]; 
            } 
        } 
        [ConfigurationProperty("filters", IsDefaultCollection = false, IsRequired = false)] 
        [ConfigurationCollection(typeof(FilterConfig), AddItemName = "filter")] 
        public FilterConfigCollection EventFilters 
        { 
            get { return (FilterConfigCollection)base["filters"]; } 
        } 
    } 
}
