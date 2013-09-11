using System.Configuration; 
namespace Croc.Core.Configuration 
{ 
    public class DiagnosticsConfig : ConfigurationElement 
    { 
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
        [ConfigurationProperty("eventFilters", IsDefaultCollection = false, IsRequired = false)] 
        [ConfigurationCollection(typeof(FilterConfig), AddItemName = "filter")] 
        public FilterConfigCollection EventFilters 
        { 
            get { return (FilterConfigCollection)base["eventFilters"]; } 
        } 
        [ConfigurationProperty("writers", IsDefaultCollection = false, IsRequired = true)] 
        [ConfigurationCollection(typeof(WriterConfig), AddItemName = "writer")] 
        public WriterConfigCollection Writers 
        { 
            get { return (WriterConfigCollection)base["writers"]; } 
        } 
    } 
}
