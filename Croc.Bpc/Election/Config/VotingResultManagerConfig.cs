using System.Configuration; 
using Croc.Core.Configuration; 
namespace Croc.Bpc.Election.Config 
{ 
    public class VotingResultManagerConfig : SubsystemConfig 
    { 
        [ConfigurationProperty("addBadBlankToCounterValue", IsRequired = true)] 
        public EnabledConfig AddBadBlankToCounterValue 
        { 
            get 
            { 
                return (EnabledConfig)this["addBadBlankToCounterValue"]; 
            } 
            set 
            { 
                this["addBadBlankToCounterValue"] = value; 
            } 
        } 
        [ConfigurationProperty("needSourceDataForSaveResults", IsRequired = true)] 
        public EnabledConfig NeedSourceDataForSaveResults 
        { 
            get 
            { 
                return (EnabledConfig)this["needSourceDataForSaveResults"]; 
            } 
            set 
            { 
                this["needSourceDataForSaveResults"] = value; 
            } 
        } 
        [ConfigurationProperty("packResults", IsRequired = false)] 
        public EnabledConfig PackResults 
        { 
            get 
            { 
                return (EnabledConfig)this["packResults"]; 
            } 
            set 
            { 
                this["packResults"] = value; 
            } 
        } 
        [ConfigurationProperty("includeElectionNumberToResultsFileName", IsRequired = true)] 
        public EnabledConfig IncludeElectionNumberToResultsFileName 
        { 
            get 
            { 
                return (EnabledConfig)this["includeElectionNumberToResultsFileName"]; 
            } 
            set 
            { 
                this["includeElectionNumberToResultsFileName"] = value; 
            } 
        } 
        [ConfigurationProperty("resultsReserveCopyCount", IsRequired = false)] 
        [IntValueConfigValidator(MinValue = 0, MaxValue = 10)] 
        public ValueConfig<int> ResultsReserveCopyCount 
        { 
            get 
            { 
                return (ValueConfig<int>)this["resultsReserveCopyCount"]; 
            } 
            set 
            { 
                this["resultsReserveCopyCount"] = value; 
            } 
        } 
    } 
}
