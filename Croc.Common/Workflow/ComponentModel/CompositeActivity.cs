using System; 

using System.Linq; 

using Croc.Core.Extensions; 

using System.Reflection; 

using Croc.Core.Utils.Collections; 

using Croc.Workflow.ComponentModel.Compiler; 

 

 

namespace Croc.Workflow.ComponentModel 

{ 

    /// <summary> 

    /// Cоставное действие 

    /// </summary> 

    [Serializable] 

    public class CompositeActivity : Activity 

    { 

        /// <summary> 

        /// Действия, которые входят в данное составное действие 

        /// </summary> 

        public ByNameAccessDictionary<Activity> Activities 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        public CompositeActivity() 

        { 

            Activities = new ByNameAccessDictionary<Activity>(); 

            base.ExecutionMethodCaller = new ActivityExecutionMethodCaller("ExecuteNestedActivity", this); 

        } 

 

 

        /// <summary> 

        /// Возвращает дочернее действие данного составного действия  

        /// по локальному имени дочернего действия 

        /// </summary> 

        /// <param name="localChildActivityName"></param> 

        /// <returns></returns> 

        public Activity GetChildActivity(string localChildActivityName) 

        { 

            return Activities[WorkflowSchemeParser.CreateFullActivityName(localChildActivityName, this)]; 

        } 

 

 

        /// <summary> 

        /// Возвращает типизированное дочернее действие данного составного действия  


        /// по локальному имени дочернего действия 

        /// </summary> 

        /// <typeparam name="T"></typeparam> 

        /// <param name="localChildActivityName"></param> 

        /// <returns></returns> 

        public T GetChildActivity<T>(string localChildActivityName) where T : Activity 

        { 

            return (T)GetChildActivity(localChildActivityName); 

        } 

 

 

        internal NextActivityKey ExecuteNestedActivity( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // инициализируем св-ва экземпляра данного действия значениями, переданными в параметрах 

            InitProperties(context, parameters); 

 

 

            NextActivityKey nextActivityKey = context.DefaultNextActivityKey; 

 

 

            // определим действие, с которого начнем выполнение 

            Activity currentExecutingActivity; 

 

 

            // если это восстановление выполнения 

            if (context.Restoring) 

            { 

                var activityNameToRestore = context.GetActivityNameToRestore(); 

                currentExecutingActivity = Activities[activityNameToRestore]; 

            } 

            else 

            { 

                // начнем выполнение с первого действия 

                currentExecutingActivity = Activities.Values.First(); 

            } 

 

 

            while (currentExecutingActivity != null) 

            { 

                // если текущее действие - это действие выхода из составного действия 

                var returnActivity = currentExecutingActivity as ReturnActivity; 

                if (returnActivity != null) 

                    // то выходим 

                    return returnActivity.Result; 

 

 

                // выполняем действие 

                try 

                { 


                    nextActivityKey = currentExecutingActivity.Execute(context); 

                } 

                // выполнение было прервано 

                catch (ActivityExecutionInterruptException ex) 

                { 

                    // попробуем получить из контекста действие, к выполнению которого нужно перейти, 

                    // если это прерывание произошло в целях переключения выполнения на другое действие 

                    currentExecutingActivity = context.GetToggledActivity(ex); 

                    continue; 

                } 

                catch (Exception ex) 

                { 

                    throw new ActivityExecutionException( 

                        "Ошибка выполнения действия", ex, currentExecutingActivity, context); 

                } 

 

 

                // если след. действие определено 

                if (currentExecutingActivity.NextActivities.ContainsKey(nextActivityKey)) 

                { 

                    // переходим к выполнению этого след. действия 

                    currentExecutingActivity = currentExecutingActivity.NextActivities[nextActivityKey]; 

                } 

                else 

                { 

                    // попробуем взять просто следующее действие, т.е. то, которое идет следом за текущим 

                    if (currentExecutingActivity.FollowingActivity != null) 

                        currentExecutingActivity = currentExecutingActivity.FollowingActivity; 

                    else 

                    { 

                        // видимо, текущее действие - последнее => выходим с результатом по умолчанию 

                        return context.DefaultNextActivityKey; 

                    } 

                } 

            } 

 

 

            return nextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Инициализация св-в данного действия значениями, переданными в параметрах 

        /// </summary> 

        /// <param name="parameters"></param> 

        private void InitProperties(WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            var type = this.GetType(); 

 

 


            foreach (var param in parameters.Values) 

            { 

                // получим св-во  

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

 

 

                // получим значение 

                object propValue = param.GetValue(); 

                try 

                { 

                    propValue = param.GetValue(); 

                } 

                catch (Exception ex) 

                { 

                    throw new ActivityExecutionException( 

                        string.Format("Ошибка получения значения для св-ва {0}", param.Name), ex, this, context); 

                } 

 

 

                // установим значение 

                try 

                { 

                    if (propValue != null) 

                    { 

                        var propValueType = propValue.GetType(); 

 

 

                        // если тип значения нельзя напрямую привести к типу св-ва 

                        if (!propValueType.CanCastToType(propInfo.PropertyType)) 

                        { 

                            // попробуем сделать приведение типа 

                            try 

                            { 

                                if (propInfo.PropertyType.IsEnum && propValueType == typeof(string)) 

                                    propValue = Enum.Parse(propInfo.PropertyType, (string)propValue); 

                                else 

                                    propValue = Convert.ChangeType(propValue, propInfo.PropertyType); 

                            } 

                            catch (Exception ex) 

                            { 

                                throw new InvalidCastException(string.Format("Ошибка приведения значения '{0}' к типу {1}", 


                                    propValue, propInfo.PropertyType.Name), ex); 

                            } 

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

    } 

}


