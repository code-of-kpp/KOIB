using System; 
using System.Linq; 
using Croc.Core.Extensions; 
using System.Reflection; 
using Croc.Core.Utils.Collections; 
using Croc.Workflow.ComponentModel.Compiler; 
namespace Croc.Workflow.ComponentModel 
{ 
    [Serializable] 
    public class CompositeActivity : Activity 
    { 
        public ByNameAccessDictionary<Activity> Activities 
        { 
            get; 
            private set; 
        } 
        public string StartActivity 
        { 
            get; 
            set; 
        } 
        public CompositeActivity() 
        { 
            Activities = new ByNameAccessDictionary<Activity>(); 
            ExecutionMethodCaller = new ActivityExecutionMethodCaller("ExecuteNestedActivity", this); 
        } 
        protected override void Initialize(WorkflowExecutionContext context) 
        { 
            base.Initialize(context); 
            StartActivity = null; 
        } 
        public Activity GetChildActivity(string localChildActivityName) 
        { 
            return Activities[WorkflowSchemeParser.CreateFullActivityName(localChildActivityName, this)]; 
        } 
        public T GetChildActivity<T>(string localChildActivityName) where T : Activity 
        { 
            return (T)GetChildActivity(localChildActivityName); 
        } 
        private void InitProperties(WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var type = GetType(); 
            foreach (var param in parameters.Values) 
            { 
                PropertyInfo propInfo; 
                try 
                { 
                    propInfo = type.GetProperty(param.Name, true, true); 
                } 
                catch (Exception ex) 
                { 
                    throw new ActivityExecutionException( 
                        string.Format("Ошибка получения информации о св-ве {0}", param.Name), ex, this, context); 
                } 
                object propValue; 
                try 
                { 
                    propValue = param.GetValue(); 
                } 
                catch (Exception ex) 
                { 
                    throw new ActivityExecutionException( 
                        string.Format("Ошибка получения значения для св-ва {0}", param.Name), ex, this, context); 
                } 
                try 
                { 
                    if (propValue != null) 
                    { 
                        try 
                        { 
                            propValue = propInfo.PropertyType.ConvertToType(propValue); 
                        } 
                        catch (Exception ex) 
                        { 
                            throw new InvalidCastException( 
                                string.Format("Ошибка приведения значения '{0}' к типу {1}", 
                                              propValue, propInfo.PropertyType.Name), ex); 
                        } 
                    } 
                    propInfo.SetValue(this, propValue, null); 
                } 
                catch (Exception ex) 
                { 
                    throw new ActivityExecutionException( 
                        string.Format("Ошибка установки значения для св-ва {0}", param.Name), ex, this, context); 
                } 
            } 
        } 
        private Activity GetStartActivity(WorkflowExecutionContext context) 
        { 
            if (context.Restoring) 
                return Activities[context.GetActivityNameToRestore()]; 
            if (StartActivity == null) 
                return Activities.Values.First(); 
            var startActivityFullName = WorkflowSchemeParser.CreateFullActivityName(StartActivity, Name); 
            if (!Activities.ContainsKey(startActivityFullName)) 
                throw new ActivityExecutionException( 
                    "Начальное действие не найдено: " + startActivityFullName, this, context); 
            return Activities[startActivityFullName]; 
        } 
        private static Activity GetNextActivity(Activity currentExecutingActivity, NextActivityKey nextActivityKey) 
        { 
            var nextActivities = currentExecutingActivity.NextActivities; 
            if (nextActivities.ContainsKey(nextActivityKey)) 
                return nextActivities[nextActivityKey]; 
            if (nextActivities.ContainsKey(NextActivityKey.DefaultNextActivityKey)) 
                return nextActivities[NextActivityKey.DefaultNextActivityKey]; 
            return currentExecutingActivity.FollowingActivity; 
        } 
        internal NextActivityKey ExecuteNestedActivity( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            InitProperties(context, parameters); 
            var nextActivityKey = context.DefaultNextActivityKey; 
            var currentExecutingActivity = GetStartActivity(context); 
            while (currentExecutingActivity != null) 
            { 
                var returnActivity = currentExecutingActivity as ReturnActivity; 
                if (returnActivity != null) 
                    return returnActivity.Result; 
                try 
                { 
                    nextActivityKey = currentExecutingActivity.Execute(context); 
                } 
                catch (ActivityExecutionInterruptException ex) 
                { 
                    currentExecutingActivity = context.GetToggledActivity(ex); 
                    continue; 
                } 
                catch (Exception ex) 
                { 
                    throw new ActivityExecutionException( 
                        "Ошибка выполнения действия", ex, currentExecutingActivity, context); 
                } 
                currentExecutingActivity = GetNextActivity(currentExecutingActivity, nextActivityKey); 
                if (currentExecutingActivity == null) 
                    return context.DefaultNextActivityKey; 
            } 
            return nextActivityKey; 
        } 
    } 
}
