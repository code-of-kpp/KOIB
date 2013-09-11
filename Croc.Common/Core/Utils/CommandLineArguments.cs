using System; 
using System.Collections.Specialized; 
using System.Text.RegularExpressions; 
namespace Croc.Core.Utils 
{ 
    public class CommandLineArguments : StringDictionary 
    { 
        public CommandLineArguments() 
        { 
            var re = new Regex("(?:-{1,2}|/)([^=:]+)(?:=|:)?(.*)", RegexOptions.Multiline | RegexOptions.IgnoreCase); 
            foreach (var arg in Environment.GetCommandLineArgs()) 
            { 
                var match = re.Match(arg); 
                if (match.Success) 
                { 
                    if (ContainsKey(match.Groups[1].Value)) 
                    { 
                        this[match.Groups[1].Value] = match.Groups[2].Value; 
                    } 
                    else 
                    { 
                        Add(match.Groups[1].Value, match.Groups[2].Value); 
                    } 
                } 
                else 
                { 
                    if (!ContainsKey(arg)) 
                    { 
                        Add(arg, ""); 
                    } 
                } 
            } 
        } 
    } 
}
