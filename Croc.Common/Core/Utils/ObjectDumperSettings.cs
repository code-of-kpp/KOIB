using System; 
using System.Collections.Generic; 
namespace Croc.Core.Utils 
{ 
    public class ObjectDumperSettings 
    { 
        public static ObjectDumperSettings Default = new ObjectDumperSettings(); 


        private IEnumerable<String> m_propsToIgnore; 
        public IEnumerable<String> PropsToIgnore 
        { 
            get { return m_propsToIgnore ?? new String[0]; } 
            set { m_propsToIgnore = value; } 
        } 
        public Boolean DoNotUseToStringMethod; 
        public Int32 MaxDepth = 3; 
        public Int32 MaxProps = 10; 
        public Int32 MaxEnumerableItems = 100; 
        public string EnumerableDelimiter = ","; 
    } 
}
