using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Configuration; 

using Croc.Bpc.Election.Voting; 

 

 

namespace Croc.Bpc.Election.Config 

{ 

    /// <summary> 

    /// Коллекция времен начала режимов голосования 

    /// </summary> 

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

 

 

        public new VotingModeTimeConfig this[VotingMode mode] 

        { 

            get 

            { 

                return (VotingModeTimeConfig)BaseGet(mode); 

            } 

        } 

    } 

}


