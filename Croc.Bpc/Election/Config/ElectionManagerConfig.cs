using Croc.Core.Configuration; 
using System.Configuration; 
namespace Croc.Bpc.Election.Config 
{ 
    public class ElectionManagerConfig : SubsystemConfig 
    { 
        [ConfigurationProperty("dataDirectories", IsRequired = true)] 
        public DataDirectoriesConfig DataDirectories 
        { 
            get 
            { 
                return (DataDirectoriesConfig)this["dataDirectories"]; 
            } 
            set 
            { 
                this["dataDirectories"] = value; 
            } 
        } 
        [ConfigurationProperty("sourceDataFileSearch", IsRequired = true)] 
        public SourceDataFileSearchConfig SourceDataFileSearch 
        { 
            get 
            { 
                return (SourceDataFileSearchConfig)this["sourceDataFileSearch"]; 
            } 
            set 
            { 
                this["sourceDataFileSearch"] = value; 
            } 
        } 
        [ConfigurationProperty("defaultVotingModeTimes", IsDefaultCollection = false, IsRequired = true)] 
        [ConfigurationCollection(typeof(VotingModeTimeConfig), AddItemName = "votingModeTime")] 
        public VotingModeTimeConfigCollection DefaultVotingModeTimes 
        { 
            get 
            { 
                return (VotingModeTimeConfigCollection)base["defaultVotingModeTimes"]; 
            } 
        } 
        [ConfigurationProperty("executeCheckExpressions", IsRequired = true)] 
        public ValueConfig<bool> ExecuteCheckExpressions 
        { 
            get 
            { 
                return (ValueConfig<bool>)this["executeCheckExpressions"]; 
            } 
            set 
            { 
                this["executeCheckExpressions"] = value; 
            } 
        } 
        [ConfigurationProperty("canRestoreCandidateCanseledInSd", IsRequired = true)] 
        public ValueConfig<bool> СanRestoreCandidateCanseledInSd 
        { 
            get 
            { 
                return (ValueConfig<bool>)this["canRestoreCandidateCanseledInSd"]; 
            } 
            set 
            { 
                this["canRestoreCandidateCanseledInSd"] = value; 
            } 
        } 
        [ConfigurationProperty("electionDayDuration", IsRequired = true)] 
        [IntValueConfigValidator(MinValue = 1)] 
        public ValueConfig<int> ElectionDayDuration 
        { 
            get 
            { 
                return (ValueConfig<int>)this["electionDayDuration"]; 
            } 
            set 
            { 
                this["electionDayDuration"] = value; 
            } 
        } 
    } 
}
