using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Election.Config 

{ 

	/// <summary> 

	/// Коллекция путей для помска ид 

	/// </summary> 

	public class PathConfigCollection : ConfigurationElementCollection 

	{ 

		protected override ConfigurationElement CreateNewElement() 

		{ 

			return new PathConfig(); 

		} 

 

 

		protected override Object GetElementKey(ConfigurationElement element) 

		{ 

			return ((PathConfig)element); 

		} 

	} 

}


