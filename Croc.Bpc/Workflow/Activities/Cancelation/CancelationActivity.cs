using System; 

using System.Collections.Specialized; 

using Croc.Bpc.Election; 

using Croc.Workflow.ComponentModel; 

 

 

namespace Croc.Bpc.Workflow.Activities.Cancelation 

{ 

    /// <summary> 

    /// Снятие кандидатов 

    /// </summary> 

    [Serializable] 

    public class CancelationActivity : ElectionEnumeratorActivity 

    { 

        /// <summary> 

        /// Хеш-метка для того, чтобы по нему определять, были ли ИД изменены или нет 

        /// TODO: нужно чтобы ИД сами определяли, что они были изменены или нет 

        /// </summary> 

        private string _sdHashMark; 

 

 

		/// <summary> 

		/// Параметры печали ИД 

		/// </summary> 

		public ListDictionary SourceDataReportParameters 

		{ 

			get 

			{ 

				var list = new ListDictionary(); 

				// необходимо добавить ИД и УИК, так как на подчиненном сканере их нет 

				// а печатать можно и с него 

				list.Add("SourceData", _electionManager.SourceData); 

				list.Add("UIK", _electionManager.UIK); 

 

 

				return list; 

			} 

		} 

 

 

        /// <summary> 

        /// Инициализация 

        /// </summary> 

        /// <param name="context"></param> 

        protected override void Initialize(WorkflowExecutionContext context) 

        { 

            base.Initialize(context); 

 

 

            // если это не восстановление 


            if (!context.Restoring) 

            { 

                // сформируем хеш-метку для ИД 

                _sdHashMark = SourceDataLoader.ComputeSourceDataHashValue(_electionManager.SourceData); 

            } 

        } 

 

 

        /// <summary> 

        /// Были ли ИД изменены 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey WereChangesMade( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // получим новую хеш-метку для ИД 

            var newHashMark = SourceDataLoader.ComputeSourceDataHashValue(_electionManager.SourceData); 

 

 

            // изменения были, если новая метка не совпадает со старой 

            return (newHashMark != _sdHashMark) ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// Сохранение изменений в состоянии 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey SaveChanges( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _electionManager.RaiseStateChanged(); 

            return context.DefaultNextActivityKey; 

        } 

    } 

}


