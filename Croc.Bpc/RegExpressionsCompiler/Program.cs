using System; 
using System.Reflection; 
namespace Croc.Bpc.RegExpressionsCompiler 
{ 
    class Program 
    { 
        static void Main(string[] args) 
        { 
            try 
            { 
                Core.Utils.Text.RegExpressions.RegExpressionsCompiler.Compile( 
                    typeof (RegExpression), 
                    "Croc.Bpc.RegExpressions", 
                    Assembly.GetExecutingAssembly().GetName().Version.ToString()); 
            } 
            catch (Exception ex) 
            { 
                Console.Error.WriteLine("Ошибка компиляции регулярных выражений: " + ex.Message); 
                Environment.Exit(-1); 
            } 
        } 
    } 
}
