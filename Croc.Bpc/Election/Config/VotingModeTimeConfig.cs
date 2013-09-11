using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Configuration; 

using Croc.Bpc.Election.Voting; 

 

 

namespace Croc.Bpc.Election.Config 

{ 

    /// <summary> 

	/// Конфиг-элемент времени начала режима голосования 

	/// </summary> 

	public class VotingModeTimeConfig : ConfigurationElement 

	{ 

		/// <summary> 

		/// Режим 

		/// </summary> 

		[ConfigurationProperty("mode", IsRequired = true)] 

        public VotingMode Mode 

		{ 

			get 

			{ 

                return (VotingMode)this["mode"]; 

			} 

			set 

			{ 

                this["mode"] = value; 

			} 

		} 

 

 

		/// <summary> 

		/// Время начала режима 

		/// </summary> 

        [ConfigurationProperty("time", IsRequired = false)] 

        public TimeSpan Time 

		{ 

			get 

			{ 

                return (TimeSpan)this["time"]; 

			} 

			set 

			{ 

                this["time"] = value; 

			} 

		} 

	} 

}


