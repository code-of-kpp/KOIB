using System; 
using System.Configuration; 
namespace Croc.Core.Utils 
{ 
    public static class ConfigurationUtils 
    { 
        public static System.Configuration.Configuration OpenConfigurationFromFile(string configFilePath) 
        { 
            var fileMap = new ConfigurationFileMap(configFilePath); 
            return ConfigurationManager.OpenMappedMachineConfiguration(fileMap); 
        } 
        public static T GetSection<T>(string configFilePath, string sectionName)  
            where T : ConfigurationSection 
        { 
            var config = OpenConfigurationFromFile(configFilePath); 
            var section = (T)config.GetSection(sectionName); 
            if (section == null) 
                throw new Exception("Секция не найдена: " + sectionName); 
            return section; 
        } 
    } 
}
