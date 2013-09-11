using System; 
using System.Configuration; 
using Croc.Bpc.Voting; 
namespace Croc.Bpc.Election.Config 
{ 
    public class VotingModeTimeConfigCollection : ConfigurationElementCollection 
    { 
        protected override ConfigurationElement CreateNewElement() 
        { 
            return new VotingModeTimeConfig(); 
        } 
        protected override Object GetElementKey(ConfigurationElement element) 
        { 
            return ((VotingModeTimeConfig)element).Mode; 
        } 
        public VotingModeTimeConfig this[VotingMode mode] 
        { 
            get 
            { 
                return (VotingModeTimeConfig)BaseGet(mode); 
            } 
        } 
        public ModeTime GetModeTime(VotingMode mode) 
        { 
            var modeTimeConfig = this[mode]; 
            var time = (modeTimeConfig == null ? TimeSpan.Zero : modeTimeConfig.Time); 
            return new ModeTime 
                       { 
                           mode = mode, 
                           hour = time.Hours, 
                           minute = time.Minutes 
                       }; 
        } 
    } 
}
