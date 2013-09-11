using System.Collections.Generic; 
using System.Configuration; 
using Croc.Core.Configuration; 
namespace Croc.Bpc.Scanner.Config 
{ 
    public class AlertsConfig : EnabledConfig 
    { 
        private readonly Dictionary<int, int> _errorOccursCountDict = new Dictionary<int, int>(); 
        private static readonly object s_errorOccursCountDictSync = new object(); 
        public bool NeedAlertAboutError(ErrorConfig error) 
        { 
            if (!error.Enabled) 
                return false; 
            lock (s_errorOccursCountDictSync) 
            { 
                if (_errorOccursCountDict.ContainsKey(error.Code)) 
                    _errorOccursCountDict[error.Code] += 1; 
                else 
                    _errorOccursCountDict[error.Code] = 1; 
                return _errorOccursCountDict[error.Code] >= Limit; 
            } 
        } 
        public void ResetErrorCounters() 
        { 
            lock (s_errorOccursCountDictSync) 
            { 
                _errorOccursCountDict.Clear(); 
            } 
        } 
        public ErrorConfig GetError(int errorCode) 
        { 
            return Errors.GetErrorByCode(errorCode); 
        } 
        [ConfigurationProperty("limit", IsRequired = true)] 
        public int Limit 
        { 
            get 
            { 
                return (int)this["limit"]; 
            } 
            set 
            { 
                this["limit"] = value; 
            } 
        } 
        [ConfigurationProperty("errors", IsDefaultCollection = false, IsRequired = true)] 
        [ConfigurationCollection(typeof(ErrorConfig), AddItemName = "error")] 
        public ErrorConfigCollection Errors 
        { 
            get 
            { 
                return (ErrorConfigCollection)base["errors"]; 
            } 
        } 
    } 
}
