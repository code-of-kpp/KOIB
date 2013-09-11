using System; 
using System.Collections.Generic; 
using System.Collections.Specialized; 
using System.IO; 
using System.Reflection; 
using System.Text; 
using System.Xml; 
using System.Xml.Schema; 
using Croc.Core; 
using Croc.Core.Diagnostics.Default; 
using Croc.Core.Extensions; 
using Croc.Core.Utils.Collections; 
using Croc.Workflow.Runtime; 
namespace Croc.Workflow.ComponentModel.Compiler 
{ 
    public class WorkflowSchemeParser 
    { 
        #region Константы 
        private const string XML_TARGETNAMESPACE = "http://schemas.croc.ru/Workflow"; 
        private const string ELEM_WORKFLOW = "Workflow"; 
        private const string ELEM_INCLUDE = "Include"; 
        private const string ELEM_COMPOSITEACTIVITY = "CompositeActivity"; 
        private const string ELEM_ACTIVITYPARAMETERSBINDINGS = "ActivityParametersBindings"; 
        private const string ELEM_ACTIVITYPARAMETERSBINDING = "ActivityParametersBinding"; 
        private const string ELEM_ACTIVITY = "Activity"; 
        private const string ELEM_REFERENCEACTIVITY = "ReferenceActivity"; 
        private const string ELEM_SUBSCRIBETOEVENT = "SubscribeToEvent"; 
        private const string ELEM_UNSUBSCRIBEFROMEVENT = "UnsubscribeFromEvent"; 
        private const string ELEM_MONITORENTER = "MonitorEnter"; 
        private const string ELEM_MONITOREXIT = "MonitorExit"; 
        private const string ELEM_PARAMETERS = "Parameters"; 
        private const string ELEM_NEXTACTIVITIES = "NextActivities"; 
        private const string ELEM_PARAMETER = "Parameter"; 
        private const string ELEM_NEXTACTIVITY = "NextActivity"; 
        private const string ELEM_REGION = "Region"; 
        private const string ATT_ROOTACTIVITY = "RootActivity"; 
        private const string ATT_DEFAULTNEXTACTIVITYKEY = "DefaultNextActivityKey"; 
        private const string ATT_ACTIVITYNAME = "ActivityName"; 
        private const string ATT_REF = "Ref"; 
        private const string ATT_NAME = "Name"; 
        private const string ATT_CLASS = "Class"; 
        private const string ATT_TRACKING = "Tracking"; 
        private const string ATT_INITIALIZE = "Initialize"; 
        private const string ATT_UNINITIALIZE = "Uninitialize"; 
        private const string ATT_EXECUTE = "Execute"; 
        private const string ATT_PARAMETERS = "Parameters"; 
        private const string ATT_NEXTACTIVITIES = "NextActivities"; 
        private const string ATT_DEFAULTNEXTACTIVITY = "DefaultNextActivity"; 
        private const string ATT_NEXTACTIVITY = "NextActivity"; 
        private const string ATT_KEY = "Key"; 
        private const string ATT_EVENT = "Event"; 
        private const string ATT_HANDLER = "Handler"; 
        private const string ATT_HANDLINGTYPE = "HandlingType"; 
        private const string ATT_LOCKNAME = "LockName"; 
        private const string ATT_PRIORITY = "Priority"; 
        private const string ATT_COMPOSITEACTIVITYNAME = "CompositeActivityName"; 
        private const char LIST_DELIMITER = ';'; 
        private const char VALUE_ASSIGN_CHAR = '='; 
        private const char PREFIX_DELIMITER = '.'; 
        private const string PREFIX_ROOT = "Root"; 
        private const string PREFIX_REFTOROOT = "Root."; 
        private const int PREFIX_REFTOROOTLEN = 5; 
        internal const string PARAM_STARTACTIVITY = "StartActivity"; 
        #endregion 
        #region Св-ва 
        private readonly List<string> _includeFileUriList = new List<string>(); 
        private WorkflowSchemeParser _parentParser; 
        private bool MainParser 
        { 
            get 
            { 
                return _parentParser == null; 
            } 
        } 
        private XmlReaderEx _reader; 
        public WorkflowScheme Scheme 
        { 
            get; 
            private set; 
        } 
        private readonly ByNameAccessDictionary<ParametersBindingActivity> _parametersBindings =  
            new ByNameAccessDictionary<ParametersBindingActivity>(); 
        private string _workflowSchemaUri; 
        private XmlReaderSettings _xmlReaderSettings; 
        public string FileName 
        { 
            get; 
            private set; 
        } 
        public bool ReadDone 
        { 
            get; 
            private set; 
        } 
        public int LineNumber 
        { 
            get 
            { 
                return _reader.LineNumber; 
            } 
        } 
        public int LinePosition 
        { 
            get 
            { 
                return _reader.LinePosition; 
            } 
        } 
        #endregion 
        public void Parse(string workflowSchemeUri) 
        { 
            Parse(workflowSchemeUri, GetXmlReaderSettings(null)); 
        } 
        public void Parse(string workflowSchemeUri, IEnumerable<KeyValuePair<string, XmlReader>> customXmlSchemas) 
        { 
            Parse(workflowSchemeUri, GetXmlReaderSettings(customXmlSchemas)); 
        } 
        private void Parse(string workflowSchemeUri, XmlReaderSettings settings) 
        { 
            CodeContract.Requires(!string.IsNullOrEmpty(workflowSchemeUri)); 
            _workflowSchemaUri = workflowSchemeUri; 
            _xmlReaderSettings = settings; 
            var fileInfo = new FileInfo(_workflowSchemaUri); 
            FileName = fileInfo.Name; 
            if (!fileInfo.Exists) 
                throw new WorkflowSchemeParserException(string.Format("Файл {0} не найден", FileName)); 
            Scheme = new WorkflowScheme(); 
            ReadDone = false; 
            using (_reader = XmlReaderEx.Create(_workflowSchemaUri, settings)) 
            { 
                ReadScheme(); 
                ReadDone = true; 
                if (!MainParser) 
                    return; 
                EvaluateNextActivities(); 
                ValidateNextActivities(); 
                EvaluateEventHandlerActivities(); 
                EvaluateReferencedActivities(); 
                BindParameters(); 
                EvaluateParameterEvaluators(); 
                EvaluateEventHolders(); 
                CheckParametersNames(); 
                ValidateScheme(); 
                AddExitActivity(); 
            } 
        } 
        private static XmlReaderSettings GetXmlReaderSettings( 
            IEnumerable<KeyValuePair<string, XmlReader>> customXmlSchemas) 
        { 
            var schemas = new XmlSchemaSet(); 
            var schemaXmlReader = XmlReader.Create( 
                Assembly.GetExecutingAssembly().GetManifestResourceStream("Croc.Workflow.Workflow.xsd")); 
            schemas.Add(XML_TARGETNAMESPACE, schemaXmlReader); 
            if (customXmlSchemas != null) 
                foreach (var item in customXmlSchemas) 
                { 
                    try 
                    { 
                        schemas.Add(item.Key, item.Value); 
                    } 
                    catch (Exception ex) 
                    { 
                        throw new WorkflowSchemeParserException( 
                            "Ошибка добавления пользовательской xml-схемы: " + item.Key, ex); 
                    } 
                } 
            var settings = new XmlReaderSettings 
                               { 
                                   ValidationType = ValidationType.Schema, 
                                   Schemas = schemas, 
                                   CloseInput = true, 
                               }; 
            return settings; 
        } 
        private void ReadScheme() 
        { 
            if (!_reader.MoveToFirstElement()) 
                return; 
            if (_reader.Name != ELEM_WORKFLOW) 
                throw new WorkflowSchemeParserException("Корневой элемент должен называться Workflow", this); 
            var atts = ReadAttributes(new[] { ATT_ROOTACTIVITY, ATT_DEFAULTNEXTACTIVITYKEY }); 
            Scheme.RootActivityName = atts[ATT_ROOTACTIVITY]; 
            if (atts[ATT_DEFAULTNEXTACTIVITYKEY] != null) 
                Scheme.DefaultNextActivityKey = new NextActivityKey(atts[ATT_DEFAULTNEXTACTIVITYKEY]); 
            if (MainParser) 
            { 
                if (string.IsNullOrEmpty(Scheme.RootActivityName)) 
                    throw new WorkflowSchemeParserException( 
                        "Корневой элемент Workflow должен содержать имя корневого действия в атрибуте RootActivity", this); 
                if (Scheme.DefaultNextActivityKey == null) 
                    throw new WorkflowSchemeParserException( 
                        "Корневой элемент Workflow должен содержать имя ключа следующего действия по умолчанию " + 
                        "в атрибуте DefaultNextActivityKey", this); 
            } 
            else 
            { 
                Scheme.RootActivityName = _parentParser.Scheme.RootActivityName; 
                if (Scheme.DefaultNextActivityKey == null) 
                    Scheme.DefaultNextActivityKey = _parentParser.Scheme.DefaultNextActivityKey; 
                else if (!Scheme.DefaultNextActivityKey.Equals(_parentParser.Scheme.DefaultNextActivityKey)) 
                { 
                    var rootParser = GetRootParser(); 
                    throw new WorkflowSchemeParserException(string.Format( 
                        "Название ключа следующего действия по умолчанию '{0}' в подключаемом файле {1} " + 
                        "должно совпадать с названием ключа следующего действия по умолчанию '{2}' в основном файле {3}", 
                        Scheme.DefaultNextActivityKey.Name, 
                        FileName, 
                        rootParser.Scheme.DefaultNextActivityKey.Name, 
                        rootParser.FileName), this); 
                } 
            } 
            _reader.DownDepth(); 
            while (_reader.MoveToNextElementOnCurrentDepth()) 
            { 
                switch (_reader.Name) 
                { 
                    case ELEM_INCLUDE: 
                        ReadInclude(); 
                        break; 
                    case ELEM_COMPOSITEACTIVITY: 
                        ReadCompositeActivity(); 
                        break; 
                    case ELEM_ACTIVITYPARAMETERSBINDINGS: 
                        ReadActivityParametersBindings(); 
                        break; 
                    default: 
                        throw new WorkflowSchemeParserException( 
                            string.Format("Неожиданный элемент: {0}", _reader.Name), this); 
                } 
            } 
            _reader.UpDepth(); 
        } 
        private void AddActivityToScheme(Activity activity) 
        { 
            if (Scheme.Activities.ContainsKey(activity.Name)) 
                throw new WorkflowSchemeParserException(string.Format( 
                    "Действие с именем {0} уже объявлено ранее", activity.Name), this); 
            Scheme.Activities.Add(activity); 
        } 
        private WorkflowSchemeParser GetRootParser() 
        { 
            var parser = this; 
            while (parser._parentParser != null) 
                parser = parser._parentParser; 
            return parser; 
        } 
        #region Общие методы парсинга 
        private StringDictionary ReadAttributes(string[] requiredAttNames) 
        { 
            NameValueCollection otherAttributes; 
            var res = ReadAllAttributes(requiredAttNames, out otherAttributes); 
            if (otherAttributes.Count > 0) 
            { 
                var sb = new StringBuilder(); 
                foreach (var name in otherAttributes.Keys) 
                { 
                    sb.Append(name); 
                    sb.Append(','); 
                } 
                sb.Length -= 1; 
                throw new WorkflowSchemeParserException(string.Format("Неожиданные атрибуты: {0}", sb), this); 
            } 
            return res; 
        } 
        private StringDictionary ReadAllAttributes(string[] requiredAttNames, out NameValueCollection otherAttributes) 
        { 
            otherAttributes = new NameValueCollection(); 
            var atts = new StringDictionary(); 
            foreach (var attName in requiredAttNames) 
                atts.Add(attName, null); 
            while (_reader.MoveToNextAttribute()) 
            { 
                var attName = _reader.Name; 
                if (attName.StartsWith("xmlns")) 
                    continue; 
                if (_reader.Prefix.Length > 0) 
                    attName = _reader.Name.Substring(_reader.Prefix.Length + 1); 
                var attValue = _reader.Value; 
                if (atts.ContainsKey(attName)) 
                { 
                    if (!string.IsNullOrEmpty(attValue)) 
                        atts[attName] = attValue; 
                } 
                else 
                    otherAttributes.Add(attName, attValue); 
            } 
            return atts; 
        } 
        private string ReadElementContent() 
        { 
            try 
            { 
                _reader.MoveToContent(); 
                return _reader.ReadElementString(); 
            } 
            catch (Exception ex) 
            { 
                throw new WorkflowSchemeParserException("Ошибка чтения содержимого для элемента", ex, this); 
            } 
        } 
        private delegate void ProcessNameValue(string name, string value); 
        private void ReadNameAndValueString(string nameAndValueString, ProcessNameValue processMethod) 
        { 
            var nameAndValueList = nameAndValueString.Split(LIST_DELIMITER); 
            foreach (var nameAndValue in nameAndValueList) 
            { 
                var index = nameAndValue.IndexOf(VALUE_ASSIGN_CHAR); 
                if (index == -1) 
                    throw new WorkflowSchemeParserException(string.Format( 
                        "Неправильный формат строки: {0}", nameAndValueString), this); 
                var name = nameAndValue.Substring(0, index); 
                if (string.IsNullOrEmpty(name)) 
                    throw new WorkflowSchemeParserException(string.Format( 
                        "Неправильный формат строки. Не задано имя: {0}", nameAndValue), this); 
                var value = nameAndValue.Substring(index + 1); 
                if (string.IsNullOrEmpty(value)) 
                    throw new WorkflowSchemeParserException(string.Format( 
                        "Неправильный формат строки. Не задано значение: {0}", nameAndValue), this); 
                processMethod(name, value); 
            } 
        } 
        #endregion 
        #region Биндинг параметров 
        private void ReadActivityParametersBindings() 
        { 
            var atts = ReadAttributes(new[] { ATT_COMPOSITEACTIVITYNAME }); 
            var compositeActivityName = atts[ATT_COMPOSITEACTIVITYNAME]; 
            if (compositeActivityName == null) 
                throw new WorkflowSchemeParserException( 
                    "Не задано имя составного действия для связывания значений параметров", this); 
            _reader.DownDepth(); 
            while (_reader.MoveToNextElementOnCurrentDepth()) 
            { 
                switch (_reader.Name) 
                { 
                    case ELEM_ACTIVITYPARAMETERSBINDING: 
                        ReadActivityParametersBinding(compositeActivityName); 
                        break; 
                    default: 
                        throw new WorkflowSchemeParserException( 
                            string.Format("Неожиданный элемент: {0}", _reader.Name), this); 
                } 
            } 
            _reader.UpDepth(); 
        } 
        private void ReadActivityParametersBinding(string compositeActivityName) 
        { 
            var atts = ReadAttributes(new[] { ATT_ACTIVITYNAME, ATT_PARAMETERS }); 
            var activityName = atts[ATT_ACTIVITYNAME]; 
            var parametersAttValue = atts[ATT_PARAMETERS]; 
            if (activityName == null) 
                throw new WorkflowSchemeParserException( 
                    "Не задано имя действия для связывания значений параметров", this); 
            activityName = string.CompareOrdinal(activityName, ".") == 0 
                ? compositeActivityName : CreateFullActivityName(activityName, compositeActivityName); 
            ParametersBindingActivity paramsBinding; 
            if (_parametersBindings.ContainsKey(activityName)) 
            { 
                paramsBinding = _parametersBindings[activityName]; 
            } 
            else 
            { 
                paramsBinding = new ParametersBindingActivity {Name = activityName}; 
                _parametersBindings.Add(paramsBinding); 
            } 
            if (parametersAttValue != null) 
                ReadActivityParametersFromAttValue(paramsBinding, parametersAttValue); 
            ReadActivityParameters(paramsBinding); 
        } 
        private void BindParameters() 
        { 
            foreach (var paramsBinding in _parametersBindings.Values) 
            { 
                if (!Scheme.Activities.ContainsKey(paramsBinding.Name)) 
                    throw new WorkflowSchemeParserException(string.Format( 
                        "Ошибка связывания значений параметров. Действие с именем {0} не найдено", 
                        paramsBinding.Name), this); 
                var activity = Scheme.Activities[paramsBinding.Name]; 
                foreach (var param in paramsBinding.Parameters) 
                { 
                    if (!activity.Parameters.Contains(param)) 
                        activity.Parameters.Add(param); 
                } 
            } 
        } 
        #endregion 
        #region Подключение внешних схем 
        private void ReadInclude() 
        { 
            var refUri = ReadAttributes(new[] { ATT_REF })[ATT_REF]; 
            if (refUri == null) 
                throw new WorkflowSchemeParserException("Не задан Uri подключаемого файла с описанием схемы", this); 
            if (IsFileAlreadyIncluded(refUri, this)) 
                return; 
            var parser = new WorkflowSchemeParser {_parentParser = this}; 
            parser.Parse(refUri, _xmlReaderSettings); 
            foreach (var activity in parser.Scheme.Activities.Values) 
            { 
                AddActivityToScheme(activity); 
            } 
            foreach (var paramsBinding in parser._parametersBindings.Values) 
            { 
                if (_parametersBindings.ContainsKey(paramsBinding.Name)) 
                { 
                    var existingParamsBinding = _parametersBindings[paramsBinding.Name]; 
                    foreach (var param in paramsBinding.Parameters) 
                    { 
                        if (existingParamsBinding.Parameters.Contains(param)) 
                            throw new WorkflowSchemeParserException(string.Format( 
                                "В файле {0} (или в подключенных в него файлах) содержится " +  
                                "связывание значения параметра {1} для действия {2}, " +  
                                "которое уже объявлено ранее",  
                                parser.FileName, param.Name, paramsBinding.Name), this); 
                        existingParamsBinding.Parameters.Add(param); 
                    } 
                } 
                else 
                { 
                    _parametersBindings.Add(paramsBinding); 
                } 
            } 
            _includeFileUriList.Add(refUri); 
            foreach (var fileUri in parser._includeFileUriList) 
            { 
                _includeFileUriList.Add(fileUri); 
            } 
        } 
        private bool IsFileAlreadyIncluded(string fileUri, WorkflowSchemeParser startSearchIncludesParser) 
        { 
            if (_workflowSchemaUri.Equals(fileUri)) 
                throw new WorkflowSchemeParserException(string.Format( 
                    "Обнаружена циклическая ссылка при подключении файла {0}", fileUri), startSearchIncludesParser); 
            return _includeFileUriList.Contains(fileUri) || 
                (_parentParser != null && _parentParser.IsFileAlreadyIncluded(fileUri, startSearchIncludesParser)); 
        } 
        #endregion 
        #region Составное действие 
        private void ReadCompositeActivity() 
        { 
            var activity = CreateCompositeActivity(); 
            AddActivityToScheme(activity); 
            Activity lastActivity = null; 
            var regionDiver = new RegionDiver(_reader); 
            _reader.DownDepth(); 
            while ( 
                _reader.MoveToNextElementOnCurrentDepth() || 
                (regionDiver.ExitFromRegion() && _reader.MoveToNextElementOnCurrentDepth())) 
            { 
                Activity currentActivity; 
                switch (_reader.Name) 
                { 
                    case ELEM_ACTIVITY: 
                        currentActivity = ReadActivity(activity); 
                        break; 
                    case ELEM_REFERENCEACTIVITY: 
                        currentActivity = ReadReferenceActivity(activity); 
                        break; 
                    case ELEM_SUBSCRIBETOEVENT: 
                        currentActivity = ReadSubscribeToEventActivity(activity); 
                        break; 
                    case ELEM_UNSUBSCRIBEFROMEVENT: 
                        currentActivity = ReadUnsubscribeFromEventActivity(activity); 
                        break; 
                    case ELEM_MONITORENTER: 
                        currentActivity = ReadMonitorEnterActivity(activity); 
                        break; 
                    case ELEM_MONITOREXIT: 
                        currentActivity = ReadMonitorExitActivity(activity); 
                        break; 
                    case ELEM_REGION: 
                        regionDiver.EnterToRegion(); 
                        continue; 
                    default: 
                        throw new WorkflowSchemeParserException( 
                            string.Format("Неожиданный элемент: {0}", _reader.Name), this); 
                } 
                if (currentActivity != null) 
                { 
                    if (lastActivity != null) 
                        lastActivity.FollowingActivity = currentActivity; 
                    lastActivity = currentActivity; 
                } 
            } 
            regionDiver.ExitFromAllRegions(); 
            _reader.UpDepth(); 
            if (activity.Activities.Count == 0) 
                throw new WorkflowSchemeParserException(string.Format( 
                    "Составное действие {0} должно содержать хотя бы одно дочернее действие", activity.Name), this); 
        } 
        private bool GetTrackingAttValue(StringDictionary atts) 
        { 
            var trackingStr = atts[ATT_TRACKING]; 
            if (trackingStr == null) 
                return true; 
            bool tracking; 
            if (!bool.TryParse(trackingStr, out tracking)) 
                throw new WorkflowSchemeParserException("Некорректно задано значение атрибута Tracking", this); 
            return tracking; 
        } 
        private CompositeActivity CreateCompositeActivity() 
        { 
            CompositeActivity activity; 
            var atts = ReadAttributes(new[] { ATT_NAME, ATT_CLASS, ATT_TRACKING }); 
            var activityName = atts[ATT_NAME]; 
            var activityClassTypeName = atts[ATT_CLASS]; 
            var tracking = GetTrackingAttValue(atts); 
            if (activityName == null) 
                throw new WorkflowSchemeParserException("Не задано имя составного действия", this); 
            if (activityClassTypeName != null) 
            { 
                Type activityType; 
                try 
                { 
                    activityType = Type.GetType(activityClassTypeName, true); 
                } 
                catch (Exception ex) 
                { 
                    throw new WorkflowSchemeParserException(string.Format( 
                        "Ошибка загрузки типа класса действия. Имя типа класса = {0}", activityClassTypeName), 
                        ex, this); 
                } 
                if (activityType == null) 
                    throw new WorkflowSchemeParserException(string.Format( 
                        "Тип класса действия с именем {0} не найден", activityClassTypeName), this); 
                if (!activityType.IsInheritedFromType(typeof(CompositeActivity))) 
                    throw new WorkflowSchemeParserException(string.Format( 
                        "Класс '{0}' должен быть унаследован от CompositeActivity", activityClassTypeName), this); 
                if (activityType.GetConstructor(new Type[] { }) == null) 
                    throw new WorkflowSchemeParserException(string.Format( 
                        "Для класса '{0}' не найден конструктор", activityClassTypeName), this); 
                try 
                { 
                    activity = (CompositeActivity)Activator.CreateInstance(activityType); 
                } 
                catch (Exception ex) 
                { 
                    throw new WorkflowSchemeParserException(string.Format( 
                        "Ошибка создания экземпляра класса действия. Имя типа класса = {0}", activityClassTypeName), 
                        ex, this); 
                } 
            } 
            else 
            { 
                activity = new CompositeActivity(); 
            } 
            activity.Name = activityName; 
            activity.Tracking = tracking; 
            return activity; 
        } 
        private Activity ReadActivity(CompositeActivity parentActivity) 
        { 
            var activity = new Activity(); 
            string activityExecutionMethod; 
            ReadActivityCommon(activity, parentActivity, out activityExecutionMethod); 
            activity.ExecutionMethodCaller = GetActivityExecutionMethodCaller( 
                activityExecutionMethod, parentActivity); 
            return activity; 
        } 
        private void ReadActivityCommon( 
            Activity activity, CompositeActivity parentActivity, out string activityExecutionMethod) 
        { 
            NameValueCollection nextActivitiesAtts; 
            var atts = ReadAllAttributes( 
                new[] {  
                    ATT_NAME,  
                    ATT_INITIALIZE,  
                    ATT_UNINITIALIZE,  
                    ATT_EXECUTE,  
                    ATT_PRIORITY,  
                    ATT_PARAMETERS,  
                    ATT_NEXTACTIVITIES,  
                    ATT_DEFAULTNEXTACTIVITY, 
                    ATT_TRACKING }, 
                out nextActivitiesAtts); 
            var activityName = atts[ATT_NAME]; 
            var activityInitializeMethod = atts[ATT_INITIALIZE]; 
            var activityUninitializeMethod = atts[ATT_UNINITIALIZE]; 
            activityExecutionMethod = atts[ATT_EXECUTE]; 
            var parametersAttValue = atts[ATT_PARAMETERS]; 
            var nextActivitiesAttValue = atts[ATT_NEXTACTIVITIES]; 
            var defaultNextActivityAttValue = atts[ATT_DEFAULTNEXTACTIVITY]; 
            var tracking = GetTrackingAttValue(atts); 
            if (activityName == null) 
                throw new WorkflowSchemeParserException("Не задано имя действия", this); 
            if (activityExecutionMethod == null) 
                throw new WorkflowSchemeParserException("Не задан метод, реализующий логику действия", this); 
            activity.Name = CreateFullActivityName(activityName, parentActivity); 
            if (activityInitializeMethod != null) 
                activity.InitializeMethodCaller =  
                    GetActivityUnInitializeMethodCaller(activityInitializeMethod, parentActivity); 
            if (activityUninitializeMethod != null) 
                activity.UninitializeMethodCaller = 
                    GetActivityUnInitializeMethodCaller(activityUninitializeMethod, parentActivity); 
            SetPriority(activity, atts[ATT_PRIORITY]); 
            activity.Tracking = tracking; 
            AddActivityToScheme(activity); 
            parentActivity.Activities.Add(activity); 
            activity.Parent = parentActivity; 
            if (parametersAttValue != null) 
                ReadActivityParametersFromAttValue(activity, parametersAttValue); 
            if (nextActivitiesAttValue != null) 
            { 
                ReadNameAndValueString( 
                    nextActivitiesAttValue, 
                    (name, value) => AddNextActivity(activity, name, value)); 
            } 
            for (int i = 0; i < nextActivitiesAtts.Count; i++) 
            { 
                var attName = nextActivitiesAtts.GetKey(i); 
                var attValue = nextActivitiesAtts.Get(i); 
                AddNextActivity(activity, attName, attValue); 
            } 
            if (defaultNextActivityAttValue != null) 
            { 
                AddNextActivity(activity, NextActivityKey.DefaultNextActivityKey, defaultNextActivityAttValue); 
            } 
            ReadActivityInnerElements(activity); 
        } 
        private void SetPriority(Activity activity, string priorityAttValue) 
        { 
            try 
            { 
                if (priorityAttValue == null) 
                    return; 
                activity.Priority = new ActivityPriority(int.Parse(priorityAttValue)); 
            } 
            catch (Exception ex) 
            { 
                throw new WorkflowSchemeParserException("Некорректный приоритет: " + priorityAttValue, ex, this); 
            } 
        } 
        private void ReadActivityInnerElements(Activity activity) 
        { 
            _reader.DownDepth(); 
            while (_reader.MoveToNextElementOnCurrentDepth()) 
            { 
                switch (_reader.Name) 
                { 
                    case ELEM_PARAMETERS: 
                        ReadActivityParameters(activity); 
                        break; 
                    case ELEM_NEXTACTIVITIES: 
                        ReadNextActivities(activity); 
                        break; 
                    default: 
                        throw new WorkflowSchemeParserException( 
                            string.Format("Неожиданный элемент: {0}", _reader.Name), this); 
                } 
            } 
            _reader.UpDepth(); 
        } 
        internal static string CreateFullActivityName(string localActivityName, Activity parentActivity) 
        { 
            CodeContract.Requires(parentActivity != null); 
            return CreateFullActivityName(localActivityName, parentActivity.Name); 
        } 
        internal static string CreateFullActivityName(string localActivityName, string parentActivityName) 
        { 
            CodeContract.Requires(!string.IsNullOrEmpty(localActivityName)); 
            CodeContract.Requires(!string.IsNullOrEmpty(parentActivityName)); 
            return string.Format("{0}.{1}", parentActivityName, localActivityName); 
        } 
        private ActivityExecutionMethodCaller GetActivityExecutionMethodCaller( 
            string methodName, CompositeActivity compositeActivity) 
        { 
            if (methodName.StartsWith(PREFIX_REFTOROOT)) 
            { 
                methodName = methodName.Substring(PREFIX_REFTOROOTLEN); 
                return new ActivityExecutionMethodCaller(methodName, Scheme.RootActivity); 
            } 
            return new ActivityExecutionMethodCaller(methodName, compositeActivity); 
        } 
        private ActivityUnInitializeMethodCaller GetActivityUnInitializeMethodCaller( 
            string methodName, CompositeActivity compositeActivity) 
        { 
            if (methodName.StartsWith(PREFIX_REFTOROOT)) 
            { 
                methodName = methodName.Substring(PREFIX_REFTOROOTLEN); 
                return new ActivityUnInitializeMethodCaller(methodName, Scheme.RootActivity); 
            } 
            return new ActivityUnInitializeMethodCaller(methodName, compositeActivity); 
        } 
        #region Параметры действия 
        private void ReadActivityParameters(Activity activity) 
        { 
            _reader.DownDepth(); 
            while (_reader.MoveToNextElementOnCurrentDepth()) 
            { 
                switch (_reader.Name) 
                { 
                    case ELEM_PARAMETER: 
                        ReadActivityParameter(activity); 
                        break; 
                    default: 
                        throw new WorkflowSchemeParserException( 
                            string.Format("Неожиданный элемент: {0}", _reader.Name), this); 
                } 
            } 
            _reader.UpDepth(); 
        } 
        private void ReadActivityParametersFromAttValue(Activity activity, string parametersAttValue) 
        { 
            ReadNameAndValueString( 
                parametersAttValue, 
                (name, value) => 
                    { 
                        var param = new ActivityParameter {Name = name.Trim()}; 
                        if (activity.Parameters.Contains(param)) 
                            throw new WorkflowSchemeParserException(string.Format( 
                                "Параметр с именем {0} уже добавлен ранее", param.Name), this); 
                        activity.Parameters.Add(param); 
                        ReadParameterValue(activity, param, value); 
                    }); 
        } 
        private void ReadActivityParameter(Activity activity) 
        { 
            var param = new ActivityParameter 
                            { 
                                Name = ReadAttributes(new[] {ATT_NAME})[ATT_NAME] 
                            }; 
            if (param.Name == null) 
                throw new WorkflowSchemeParserException("Не задано имя параметра действия", this); 
            if (activity.Parameters.Contains(param)) 
                throw new WorkflowSchemeParserException(string.Format( 
                    "Параметр с именем {0} уже добавлен ранее", param.Name), this); 
            activity.Parameters.Add(param); 
            var paramValue = ReadElementContent(); 
            if (string.IsNullOrEmpty(paramValue)) 
                throw new WorkflowSchemeParserException(string.Format( 
                    "Не задано значение параметра {0}", param.Name), this); 
            ReadParameterValue(activity, param, paramValue); 
        } 
        private void ReadParameterValue(Activity activity, ActivityParameter param, string paramValue) 
        { 
            param.Evaluator = GetParameterEvaluator(activity, param, paramValue); 
        } 
        private ActivityParameterEvaluator GetParameterEvaluator( 
            Activity activity, ActivityParameter param, string paramValue) 
        { 
            if (paramValue.StartsWith("@")) 
            { 
                paramValue = paramValue.Substring(1); 
                if (string.IsNullOrEmpty(paramValue)) 
                    throw new WorkflowSchemeParserException(string.Format( 
                        "Некорректное значение параметра {0}: {1}", param.Name, paramValue), this); 
                if (paramValue.StartsWith("@")) 
                { 
                    paramValue = paramValue.Substring(1); 
                    if (string.IsNullOrEmpty(paramValue)) 
                        throw new WorkflowSchemeParserException(string.Format( 
                            "Некорректное значение параметра {0}: {1}", param.Name, paramValue), this); 
                    try 
                    { 
                        return WorkflowBuiltinFunctions.GetEvaluatorForBuiltinFunction(paramValue); 
                    } 
                    catch (Exception ex) 
                    { 
                        throw new WorkflowSchemeParserException( 
                            "Ошибка получения метода для получения значения параметра", ex, this); 
                    } 
                } 
                int dotIndex = paramValue.IndexOf(PREFIX_DELIMITER); 
                if (dotIndex > 0) 
                { 
                    var strArr = paramValue.Split(PREFIX_DELIMITER); 
                    var prefix = strArr[0]; 
                    var propName = strArr[1]; 
                    if (string.IsNullOrEmpty(propName)) 
                        throw new WorkflowSchemeParserException(string.Format( 
                            "Некорректное значение параметра {0}: {1}", param.Name, paramValue), this); 
                    if (prefix == PREFIX_ROOT) 
                        prefix = Scheme.RootActivityName; 
                    var propOwner = GetReferencedActivity(prefix); 
                    return GetParameterEvaluatorForPropertyReference(propName, propOwner); 
                } 
                if (activity.Parent == null) 
                    throw new WorkflowSchemeParserException(string.Format( 
                        "Для значения-ссылки @{0} параметра {1} не определено родительское действие", 
                        paramValue, param.Name), this); 


                return GetParameterEvaluatorForPropertyReference(paramValue, activity.Parent); 
            } 
            if (paramValue.StartsWith("[") && paramValue.EndsWith("]")) 
            { 
                paramValue = paramValue.Substring(1, paramValue.Length - 2); 
                var array = paramValue.Split(','); 
                var evaluatorArray = new ActivityParameterEvaluator[array.Length]; 
                for (int i = 0; i < array.Length; i++) 
                    evaluatorArray[i] = GetParameterEvaluator(activity, param, array[i]); 
                return new ActivityParameterEvaluator(evaluatorArray); 
            } 
            if (paramValue.StartsWith(@"\@")) 
                paramValue = '@' + paramValue.Substring(2); 
            return new ActivityParameterEvaluator(paramValue); 
        } 
        private ActivityParameterEvaluator GetParameterEvaluatorForPropertyReference( 
            string propertyName, Activity propertyOwner) 
        { 
            var unevalRefActivity = propertyOwner as UnevaluatedActivity; 
            return unevalRefActivity != null 
                       ? new UnevaluatedActivityParameterEvaluator(propertyName, unevalRefActivity) 
                       : CreateActivityParameterEvaluator(propertyName, propertyOwner); 
        } 
        private ActivityParameterEvaluator CreateActivityParameterEvaluator(string propertyName, Activity propertyOwner) 
        { 
            var type = propertyOwner.GetType(); 
            PropertyInfo propInfo; 
            try 
            { 
                propInfo = type.GetProperty(propertyName, true, false); 
            } 
            catch (Exception ex) 
            { 
                throw new WorkflowSchemeParserException(string.Format( 
                    "Ошибка получения св-ва {0}", propertyName), ex, this); 
            } 
            return new ActivityParameterEvaluator(propInfo, propertyOwner); 
        } 
        #endregion 
        #region Следующие действия 
        private void ReadNextActivities(Activity activity) 
        { 
            _reader.DownDepth(); 
            while (_reader.MoveToNextElementOnCurrentDepth()) 
            { 
                switch (_reader.Name) 
                { 
                    case ELEM_NEXTACTIVITY: 
                        ReadNextActivity(activity); 
                        break; 
                    default: 
                        throw new WorkflowSchemeParserException( 
                            string.Format("Неожиданный элемент: {0}", _reader.Name), this); 
                } 
            } 
            _reader.UpDepth(); 
        } 
        private void ReadNextActivity(Activity activity) 
        { 
            var nextActivityKeyName = ReadAttributes(new[] { ATT_KEY })[ATT_KEY]; 
            var nextActivityKeyValue = ReadElementContent(); 
            AddNextActivity(activity, nextActivityKeyName, nextActivityKeyValue); 
        } 
        private void AddNextActivity(Activity activity, string nextActivityKeyName, string nextActivityKeyValue) 
        { 
            if (nextActivityKeyName == null) 
                throw new WorkflowSchemeParserException("Не задан ключ следующего действия", this); 
            var nextActivityKey = new NextActivityKey(nextActivityKeyName); 
            AddNextActivity(activity, nextActivityKey, nextActivityKeyValue); 
        } 
        private void AddNextActivity(Activity activity, NextActivityKey nextActivityKey, string nextActivityKeyValue) 
        { 
            if (activity.NextActivities.ContainsKey(nextActivityKey)) 
                throw new WorkflowSchemeParserException(string.Format( 
                    "Ключ следующего действия {0} уже добавлен ранее", nextActivityKey.Name), this); 
            if (string.IsNullOrEmpty(nextActivityKeyValue)) 
                throw new WorkflowSchemeParserException(string.Format( 
                    "Не задано значение ключа следующего действия {0}", nextActivityKey.Name), this); 
            if (nextActivityKeyValue.StartsWith("@@")) 
            { 
                nextActivityKeyValue = nextActivityKeyValue.Substring(2); 
                if (string.IsNullOrEmpty(nextActivityKeyValue)) 
                    throw new WorkflowSchemeParserException(string.Format( 
                        "Некорректное значение ключа следующего действия {0}: {1}", 
                        nextActivityKey.Name, nextActivityKeyValue), this); 
                ReturnActivity returnActivity; 
                try 
                { 
                    returnActivity = WorkflowBuiltinFunctions.GetReturnActivity(nextActivityKeyValue); 
                } 
                catch (Exception ex) 
                { 
                    throw new WorkflowSchemeParserException(string.Format( 
                        "Ошибка получения встроенной функции по выражению {0}", nextActivityKeyValue), ex, this); 
                } 
                activity.NextActivities.Add(nextActivityKey, returnActivity); 
            } 
            else 
            { 
                var nextActivityFullName = CreateFullActivityName(nextActivityKeyValue, activity.Parent); 
                activity.NextActivities.Add( 
                    nextActivityKey, 
                    Scheme.Activities.ContainsKey(nextActivityFullName) 
                        ? Scheme.Activities[nextActivityFullName] 
                        : new UnevaluatedActivity(nextActivityKeyValue, activity.Parent)); 
            } 
        } 
        #endregion 
        #region Действия-ссылки 
        private ReferenceActivity ReadReferenceActivity(CompositeActivity parentActivity) 
        { 
            var activity = new ReferenceActivity(); 
            string activityExecutionMethod; 
            ReadActivityCommon(activity, parentActivity, out activityExecutionMethod); 
            activity.ActivityForExecute = GetReferencedActivity(activityExecutionMethod); 


            return activity; 
        } 
        private Activity GetReferencedActivity(string referencedActivityFullName) 
        { 
            if (string.IsNullOrEmpty(referencedActivityFullName)) 
                throw new WorkflowSchemeParserException( 
                    "Не задано имя действия, на которое ссылается действие-ссылка", this); 
            if (Scheme.Activities.ContainsKey(referencedActivityFullName)) 
            { 
                return Scheme.Activities[referencedActivityFullName]; 
            } 
            return new UnevaluatedActivity(referencedActivityFullName, null); 
        } 
        #endregion 
        #region Работа с событиями 
        private SubscribeToEventActivity ReadSubscribeToEventActivity(CompositeActivity parentActivity) 
        { 
            var activity = new SubscribeToEventActivity(); 
            var atts = ReadAttributes(new[] { ATT_NAME, ATT_EVENT, ATT_HANDLER, ATT_HANDLINGTYPE, ATT_NEXTACTIVITY }); 
            InitEventHandlerActivity(activity, atts, parentActivity); 
            activity.HandlingType = GetEventHandlingType(atts[ATT_HANDLINGTYPE]); 
            return activity; 
        } 
        private UnsubscribeFromEventActivity ReadUnsubscribeFromEventActivity( 
            CompositeActivity parentActivity) 
        { 
            var activity = new UnsubscribeFromEventActivity(); 
            var atts = ReadAttributes(new[] { ATT_NAME, ATT_EVENT, ATT_HANDLER, ATT_NEXTACTIVITY }); 
            InitEventHandlerActivity(activity, atts, parentActivity); 
            return activity; 
        } 
        private void InitEventHandlerActivity( 
            EventHandlerActivity activity, StringDictionary atts, CompositeActivity parentActivity) 
        { 
            var activityName = atts[ATT_NAME]; 
            var eventAttValue = atts[ATT_EVENT]; 
            var handlerAttValue = atts[ATT_HANDLER]; 
            var nextActivityAttValue = atts[ATT_NEXTACTIVITY]; 
            if (activityName == null) 
                throw new WorkflowSchemeParserException("Не задано имя действия", this); 
            if (eventAttValue == null) 
                throw new WorkflowSchemeParserException("Не задано событие", this); 
            if (handlerAttValue == null) 
                throw new WorkflowSchemeParserException("Не задано действие-обработчик события", this); 
            activity.Name = CreateFullActivityName(activityName, parentActivity); 
            AddActivityToScheme(activity); 
            parentActivity.Activities.Add(activity); 
            activity.Parent = parentActivity; 
            activity.Event = GetEventHolder(activity, eventAttValue); 
            activity.Handler = GetEventHandlerActivity(handlerAttValue, parentActivity); 
            if (!string.IsNullOrEmpty(nextActivityAttValue)) 
                AddNextActivity(activity, Scheme.DefaultNextActivityKey.Name, nextActivityAttValue); 
        } 
        private EventHolder GetEventHolder(Activity activity, string eventAttValue) 
        { 
            if (!eventAttValue.StartsWith("@")) 
                throw new WorkflowSchemeParserException(string.Format( 
                        "Некорректное имя события {0}", eventAttValue), this); 
            eventAttValue = eventAttValue.Substring(1); 
            int dotIndex = eventAttValue.IndexOf(PREFIX_DELIMITER); 
            if (dotIndex > 0) 
            { 
                var strArr = eventAttValue.Split(PREFIX_DELIMITER); 
                var prefix = strArr[0]; 
                var eventName = strArr[1]; 
                if (string.IsNullOrEmpty(eventName)) 
                    throw new WorkflowSchemeParserException(string.Format( 
                        "Некорректное имя события {0}", eventAttValue), this); 
                if (prefix == PREFIX_ROOT) 
                    prefix = Scheme.RootActivityName; 
                var propOwner = GetReferencedActivity(prefix); 
                return GetEventHolderForActivityEvent(eventName, propOwner); 
            } 
            if (activity.Parent == null) 
                throw new WorkflowSchemeParserException(string.Format( 
                    "Событие {0} не определено в родительском действии", eventAttValue), this); 
            return GetEventHolderForActivityEvent(eventAttValue, activity.Parent); 
        } 
        private EventHolder GetEventHolderForActivityEvent(string eventName, Activity eventOwner) 
        { 
            var unevalActivity = eventOwner as UnevaluatedActivity; 
            return unevalActivity != null 
                       ? new UnevaluatedEventHolder(eventName, unevalActivity) 
                       : CreateEventHolder(eventName, eventOwner); 
        } 
        private EventHolder CreateEventHolder(string eventName, Activity eventOwner) 
        { 
            var type = eventOwner.GetType(); 
            EventInfo evInfo; 
            try 
            { 
                evInfo = type.GetEvent(eventName); 
                if (evInfo == null) 
                    throw new Exception(string.Format("Тип {0} не содержит события public {1}", type.Name, eventName)); 
            } 
            catch (Exception ex) 
            { 
                throw new WorkflowSchemeParserException(string.Format( 
                    "Ошибка получения события {0}", eventName), ex, this); 
            } 
            return new EventHolder(evInfo, eventOwner); 
        } 
        private Activity GetEventHandlerActivity(string handlerActivityName, CompositeActivity compositeActivity) 
        { 
            return Scheme.Activities.ContainsKey(handlerActivityName) 
                       ? Scheme.Activities[handlerActivityName] 
                       : new UnevaluatedActivity(handlerActivityName, compositeActivity); 
        } 
        private EventHandlingType GetEventHandlingType(string handlingTypeName) 
        { 
            if (string.IsNullOrEmpty(handlingTypeName)) 
                return EventHandlingType.Sync; 
            try 
            { 
                return (EventHandlingType)Enum.Parse(typeof(EventHandlingType), handlingTypeName); 
            } 
            catch (Exception ex) 
            { 
                throw new WorkflowSchemeParserException( 
                    "Некорректно задан тип обработки события: " + handlingTypeName, ex, this); 
            } 
        } 


        #endregion 
        #region Блокировки 
        private MonitorEnterActivity ReadMonitorEnterActivity(CompositeActivity parentActivity) 
        { 
            var activity = new MonitorEnterActivity(); 
            InitMonitorActivity(activity, parentActivity); 
            return activity; 
        } 
        private MonitorExitActivity ReadMonitorExitActivity(CompositeActivity parentActivity) 
        { 
            var activity = new MonitorExitActivity(); 
            InitMonitorActivity(activity, parentActivity); 
            return activity; 
        } 
        private void InitMonitorActivity(MonitorActivity activity, CompositeActivity parentActivity) 
        { 
            var atts = ReadAttributes(new[] { ATT_NAME, ATT_LOCKNAME, ATT_NEXTACTIVITY }); 
            var activityName = atts[ATT_NAME]; 
            var lockName = atts[ATT_LOCKNAME]; 
            var nextActivityAttValue = atts[ATT_NEXTACTIVITY]; 
            if (activityName == null) 
                throw new WorkflowSchemeParserException("Не задано имя действия", this); 
            if (lockName == null) 
                throw new WorkflowSchemeParserException("Не задано имя блокировки", this); 
            activity.Name = CreateFullActivityName(activityName, parentActivity); 
            activity.LockName = lockName; 
            AddActivityToScheme(activity); 
            parentActivity.Activities.Add(activity); 
            activity.Parent = parentActivity; 
            if (!string.IsNullOrEmpty(nextActivityAttValue)) 
                AddNextActivity(activity, Scheme.DefaultNextActivityKey.Name, nextActivityAttValue); 
        } 
        #endregion 
        #endregion 
        #region Пост-парсинговые вычисления 
        private Activity EvaluateActivity(UnevaluatedActivity unevaluatedActivity, string errorMessagePrefix) 
        { 
            if (Scheme.Activities.ContainsKey(unevaluatedActivity.ActivityName)) 
                return Scheme.Activities[unevaluatedActivity.ActivityName]; 
            if (unevaluatedActivity.ParentActivity != null) 
            { 
                var fullActivityName = CreateFullActivityName( 
                    unevaluatedActivity.ActivityName, unevaluatedActivity.ParentActivity); 
                if (Scheme.Activities.ContainsKey(fullActivityName)) 
                    return Scheme.Activities[fullActivityName]; 
            } 
            throw new WorkflowSchemeParserException(string.Format( 
                "{0}: не найдено действие с именем {1}", errorMessagePrefix, unevaluatedActivity.ActivityName), this); 
        } 
        private void EvaluateNextActivities() 
        { 
            var evaluatedNextActivities = new List<Triplet<Activity, NextActivityKey, Activity>>(); 
            foreach (var activity in Scheme.Activities.Values) 
            { 
                foreach (var entry in activity.NextActivities) 
                { 
                    var nextActivity = entry.Value; 
                    var unevalNextActivity = nextActivity as UnevaluatedActivity; 
                    if (unevalNextActivity != null) 
                        evaluatedNextActivities.Add(new Triplet<Activity, NextActivityKey, Activity>( 
                            activity, entry.Key, 
                            EvaluateActivity( 
                                unevalNextActivity, 
                                string.Format("Вычисление следующего действия для действия {0}", activity.Name)))); 
                } 
            } 
            foreach (var triplet in evaluatedNextActivities) 
                triplet.First.NextActivities[triplet.Second] = triplet.Third; 
        } 
        private void ValidateNextActivities() 
        { 
            foreach (var activity in Scheme.Activities.Values) 
            { 
                if (activity.NextActivities.Count > 1) 
                { 
                    var enumerator = activity.NextActivities.GetEnumerator(); 
                    enumerator.MoveNext(); 
                    var firstName = enumerator.Current.Value.Name; 
                    var allEquals = true; 
                    while (enumerator.MoveNext()) 
                    { 
                        if (string.CompareOrdinal(firstName, enumerator.Current.Value.Name) != 0) 
                        { 
                            allEquals = false; 
                            break; 
                        } 
                    } 
                    if (allEquals) 
                    { 
                        CoreApplication.Instance.Logger.LogWarning( 
                                "Для действия '{0}' все ключи следующих действий имеют одинаковое значение: {1}", 
                                activity.Name, firstName); 
                    } 
                } 
            } 
        } 
        private void EvaluateEventHandlerActivities() 
        { 
            foreach (var activity in Scheme.Activities.Values) 
            { 
                var ehActivity = activity as EventHandlerActivity; 
                if (ehActivity == null) 
                    continue; 
                var unevalHandlerActivity = ehActivity.Handler as UnevaluatedActivity; 
                if (unevalHandlerActivity != null) 
                    ehActivity.Handler = EvaluateActivity( 
                        unevalHandlerActivity, 
                        string.Format("Вычисление действия-обработчика для действия {0}", ehActivity.Name)); 
            } 
        } 
        private void EvaluateReferencedActivities() 
        { 
            foreach (var activity in Scheme.Activities.Values) 
            { 
                var refActivity = activity as ReferenceActivity; 
                if (refActivity == null) 
                    continue; 
                var unevalRefActivity = refActivity.ActivityForExecute as UnevaluatedActivity; 
                if (unevalRefActivity != null) 
                    refActivity.ActivityForExecute = EvaluateActivity( 
                        unevalRefActivity, 
                        string.Format("Вычисление действия-ссылки для действия {0}", refActivity.Name)); 
            } 
        } 
        private void EvaluateParameterEvaluators() 
        { 
            foreach (var activity in Scheme.Activities.Values) 
                foreach (var param in activity.Parameters) 
                    param.Evaluator = EvaluateParameterEvaluator(param.Evaluator); 
        } 
        private ActivityParameterEvaluator EvaluateParameterEvaluator(ActivityParameterEvaluator evaluator) 
        { 
            if (evaluator.ValueType == ActivityParameterValueType.Array) 
            { 
                for (int i = 0; i < evaluator.EvaluatorArray.Length; i++) 
                    evaluator.EvaluatorArray[i] = EvaluateParameterEvaluator(evaluator.EvaluatorArray[i]); 
                return evaluator; 
            } 
            var unevalEvaluator = evaluator as UnevaluatedActivityParameterEvaluator; 
            if (unevalEvaluator == null) 
                return evaluator; 
            return CreateActivityParameterEvaluator( 
                unevalEvaluator.PropertyName,  
                EvaluateActivity( 
                    unevalEvaluator.PropertyOwner, 
                    string.Format("Вычисление действия, содержащего св-во {0}", unevalEvaluator.PropertyName))); 
        } 
        private void EvaluateEventHolders() 
        { 
            foreach (var activity in Scheme.Activities.Values) 
            { 
                var ehActivity = activity as EventHandlerActivity; 
                if (ehActivity == null) 
                    continue; 
                var unevalEventHolder = ehActivity.Event as UnevaluatedEventHolder; 
                if (unevalEventHolder != null) 
                    ehActivity.Event = CreateEventHolder( 
                        unevalEventHolder.EventName, 
                        EvaluateActivity( 
                            unevalEventHolder.EventOwner, 
                            string.Format("Вычисление действия, содержащего событие {0}", unevalEventHolder.EventName))); 
            } 
        } 
        #endregion 
        #region Проверки результата парсинга 
        private void CheckParametersNames() 
        { 
            foreach (var activity in Scheme.Activities.Values) 
                foreach (var param in activity.Parameters) 
                { 
                    if (activity is ReferenceActivity) 
                    { 
                        var refActivity = (ReferenceActivity)activity; 
                        if (refActivity.ActivityForExecute is CompositeActivity) 
                        { 
                            CheckPropertyForParameter(refActivity.ActivityForExecute, param.Name); 
                            if (// это параметр, который задает начальное действие, 
                                string.CompareOrdinal(param.Name, PARAM_STARTACTIVITY) == 0 && 
                                param.Evaluator.ValueType == ActivityParameterValueType.PlainValue) 
                            { 
                                var compositeActivity = (CompositeActivity) refActivity.ActivityForExecute; 
                                var startActivityName = (string) param.GetValue(); 
                                var startActivityFullName = CreateFullActivityName( 
                                    startActivityName, compositeActivity.Name); 
                                if (!compositeActivity.Activities.ContainsKey(startActivityFullName)) 
                                    throw new WorkflowSchemeParserException(string.Format( 
                                        "Действие с именем '{0}' не найдено среди действий составного действия '{1}'", 
                                        startActivityName, compositeActivity.Name), this); 
                            } 
                        } 
                    } 
                    else if (activity is CompositeActivity) 
                    { 
                        var compositeActivity = (CompositeActivity)activity; 
                        CheckPropertyForParameter(compositeActivity, param.Name); 
                    } 
                } 
        } 
        private void CheckPropertyForParameter(Activity propertyOwner, string paramName) 
        { 
            var type = propertyOwner.GetType(); 
            try 
            { 
                type.GetProperty(paramName, true, true); 
            } 
            catch (Exception ex) 
            { 
                throw new WorkflowSchemeParserException(string.Format( 
                    "Для параметра {0} не определено свойство public {0} {{get;set;}} в классе {1} действия {2}", 
                    paramName, type.FullName, propertyOwner.Name), ex, this); 
            } 
        } 
        private void ValidateScheme() 
        { 
            if (string.IsNullOrEmpty(Scheme.RootActivityName)) 
                throw new WorkflowSchemeParserException("Не определено имя корневого действия", this); 
            if (!Scheme.Activities.ContainsKey(Scheme.RootActivityName) || 
                !(Scheme.Activities[Scheme.RootActivityName] is CompositeActivity)) 
                throw new WorkflowSchemeParserException( 
                    string.Format("Не найдено корневое составное действие: {0}", Scheme.RootActivityName), this); 
        } 
        private void AddExitActivity() 
        { 
            var exitActivity = new ReturnActivity(Scheme.DefaultNextActivityKey) 
                                   { 
                                       Name = "ExitActivity", 
                                       Priority = ActivityPriority.Highest 
                                   }; 
            AddActivityToScheme(exitActivity); 
            Scheme.ExitActivity = exitActivity; 
            var rootActivity = (CompositeActivity) Scheme.RootActivity; 
            rootActivity.Activities.Add(exitActivity); 
            exitActivity.Parent = rootActivity; 
        } 
        #endregion 
        #region Native-классы 
        private class RegionDiver 
        { 
            private readonly XmlReaderEx _reader; 
            private int _depth; 
            public RegionDiver(XmlReaderEx reader) 
            { 
                _reader = reader; 
            } 
            public void EnterToRegion() 
            { 
                _reader.DownDepth(); 
                _depth++; 
            } 
            public bool ExitFromRegion() 
            { 
                if (_depth == 0) 
                    return false; 
                _reader.UpDepth(); 
                _depth--; 
                return true; 
            } 
            public void ExitFromAllRegions() 
            { 
                while (ExitFromRegion()) 
                { 
                } 
            } 
        } 
        #endregion 
    } 
}
