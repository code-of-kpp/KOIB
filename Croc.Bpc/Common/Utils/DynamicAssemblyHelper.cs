using System; 
using System.CodeDom.Compiler; 
using System.IO; 
using System.Reflection; 
using System.Text; 
using Croc.Core.Utils; 
namespace Croc.Bpc.Utils 
{ 
    public static class DynamicAssemblyHelper 
    { 
        public const int MAX_STRING_LENGTH = 2045;     
        public static Assembly Compile(string sSource, string[] referencedAssemblies) 
        { 
            Microsoft.CSharp.CSharpCodeProvider comp = new Microsoft.CSharp.CSharpCodeProvider(); 
            CompilerParameters cp = new CompilerParameters(); 
            if (!PlatformDetector.IsUnix) 
            { 
                cp.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "")); 
            } 
            else 
            { 
                cp.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().CodeBase.Replace("file://", "")); 
            } 
            cp.GenerateExecutable = false; 
            cp.GenerateInMemory = true; 
            foreach (var assembly in referencedAssemblies) 
            { 


                cp.ReferencedAssemblies.Add(Path.Combine( 
                    AppDomain.CurrentDomain.BaseDirectory 
                    , assembly + ".dll")); 
            } 
            CompilerResults cr = comp.CompileAssemblyFromSource(cp, sSource); 
            if (cr.Errors.HasErrors) 
            { 
                StringBuilder sError = new StringBuilder(); 
                sError.Append("Error Compiling Expression: "); 
                foreach (CompilerError err in cr.Errors) 
                    sError.AppendFormat("{0}\n", err.ErrorText); 
                throw new ApplicationException("Ошибка компиляции выражения: " + sError.ToString() + "\n" + sSource); 
            } 
            return cr.CompiledAssembly; 
        } 
        public static string SplitStringByLength(string sIn) 
        { 
            StringBuilder sRes = new StringBuilder(sIn.Length + 256);        // результат 
            int nLastPos = 0;    // последняя позиция 
            for (int nCurPos = MAX_STRING_LENGTH; sIn.Length > nCurPos; nCurPos += MAX_STRING_LENGTH) 
            { 
                int nFind = sIn.LastIndexOf(" ", nCurPos - 1); 
                if (-1 < nFind) 
                { 
                    sRes.Append(sIn.Substring(nLastPos, nFind - nLastPos) + Environment.NewLine); 
                    nLastPos = nFind; 
                    nCurPos = nFind + 1; 
                } 
            } 
            sRes.Append(sIn.Substring(nLastPos)); 
            return sRes.ToString(); 
        } 
    } 
}
