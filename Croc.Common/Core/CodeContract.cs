using System; 
namespace Croc.Core 
{ 
    public static class CodeContract 
    { 
        public static void Requires(bool condition) 
        { 
            if (!condition) 
                throw new ArgumentException("Нарушение контракта!"); 
        } 
    } 
}
