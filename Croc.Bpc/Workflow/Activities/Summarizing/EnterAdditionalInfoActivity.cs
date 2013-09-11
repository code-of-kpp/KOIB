using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Workflow.ComponentModel; 

using Croc.Bpc.FileSystem; 

using Croc.Bpc.Common.Diagnostics; 

using System.IO; 

using Croc.Bpc.Election.Voting; 

using System.Collections.Specialized; 

 

 

namespace Croc.Bpc.Workflow.Activities.Summarizing 

{ 

    /// <summary> 

    /// Ввод дополнительных сведений 

    /// </summary> 

    [Serializable] 

    public class EnterAdditionalInfoActivity : ElectionParametrizedActivity 

    { 

        /// <summary> 

        /// Текущая строка протокола 

        /// </summary> 

        [NonSerialized] 

        protected Line _currentLine; 

        /// <summary> 

        /// Индекс текущей строки протокола 

        /// </summary> 

        public int CurrentLineIndex 

        { 

            get; 

            protected set; 

        } 

        /// <summary> 

        /// Значение текущей строки протокола 

        /// </summary> 

        public int? CurrentLineValue 

        { 

            get 

            { 

                return _currentLine.Value; 

            } 

        } 

        /// <summary> 

        /// Номер текущей строки протокола 

        /// </summary> 

        public int CurrentLineNumber 

        { 

            get 

            { 


                return _currentLine.Num; 

            } 

        } 

        /// <summary> 

        /// Дополнительный буквенный номер текущей строки протокола 

        /// </summary> 

        /// <remarks>или пустой, или состоит из одной буквы от 'а' до 'е'</remarks> 

        public string CurrentLineAlphaNumber 

        { 

            get 

            { 

                return _currentLine.AdditionalNum; 

            } 

        } 

        /// <summary> 

        /// Параметры для отчета "Протокол голосования" 

        /// </summary> 

        public ListDictionary ElectionProtocolParameters 

        { 

            get 

            { 

                var protocolParams = new ListDictionary(); 

                protocolParams.Add("Election", Election); 

				protocolParams.Add("final", true); 

				protocolParams.Add("withResults", true); 

                return protocolParams; 

            } 

        } 

 

 

        /// <summary> 

        /// Инициализация 

        /// </summary> 

        /// <param name="context"></param> 

        protected override void Initialize(WorkflowExecutionContext context) 

        { 

            base.Initialize(context); 

            InitLineIterator(context); 

        } 

 

 

        #region Итератор строк протокола 

 

 

        /// <summary> 

        /// Инициализация итератора строк протокола 

        /// </summary> 

        /// <param name="context"></param> 

        private void InitLineIterator(WorkflowExecutionContext context) 

        { 


            // если это не восстановление 

            if (!context.Restoring) 

            { 

                CurrentLineIndex = -1; 

            } 

            // иначе, если восстановление и индекс текущей строки в пределах массива строк 

            else if (0 <= CurrentLineIndex && CurrentLineIndex < Election.Protocol.Lines.Length) 

            { 

                _currentLine = Election.Protocol.Lines[CurrentLineIndex]; 

            } 

        } 

 

 

        /// <summary> 

        /// Передвигает итератор к след. строке протокола 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey MoveNextLine( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            if (++CurrentLineIndex < Election.Protocol.Lines.Length) 

            { 

                _currentLine = Election.Protocol.Lines[CurrentLineIndex]; 

                return BpcNextActivityKeys.Yes; 

            } 

            else 

                return BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// Передвигает итератор к предыдущей строки протокола 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey MovePreviousLine( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // т.к. ниже индекс будет передвинут на 1 вперед, то 

            // 1) если текущий элемент не первый в списке, то нужно передвинуть индекс на 2 пункта назад 

            // 2) иначе - на 1 пункт назад 

            if (CurrentLineIndex > 0) 

                CurrentLineIndex -= 2; 

            else 

                CurrentLineIndex -= 1; 

 

 


            return MoveNextLine(context, parameters); 

        } 

 

 

        /// <summary> 

        /// Передвигает итератор к строке протокола, за которой следует первая строка 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey ResetLineEnumerator( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            CurrentLineIndex = -1; 

 

 

			// свяжем автовычисляемые строки с методами их вычисления 

			_electionManager.SourceData.BindAutoLinesAndChecksCountMethods(); 

 

 

			return context.DefaultNextActivityKey; 

        } 

 

 

        #endregion 

 

 

        /// <summary> 

        /// Проверяет, должна ли текущая строка протокола вычисляться автоматически 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey IsAutoCalculatedLine( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _currentLine.IsAutoCalculated ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// Проверяет, должна ли текущая строка протокола вычисляться автоматически 

        /// и не является ли она первой 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey IsAutoCalculatedAndNotFirstLine( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 


            return _currentLine.IsAutoCalculated && CurrentLineIndex > 0 

                ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// Вычисляет значение текущей строки протокола 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey CalculateLineValue( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _currentLine.CalculateValue(); 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Принимает введенное пользователем значение строки протокола 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey AcceptEnteredLineValue( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

			_currentLine.Value = int.Parse(CommonActivity.LastReadedValue); 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Проверяет контрольные соотношения 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey CheckControlRelations( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

			// если КС проверять не нужно 

			if (!_electionManager.NeedExecuteCheckExpressions) 

				return BpcNextActivityKeys.Yes; 

 

 

			// проверим соотношения 

            return Election.IsControlRelationsSatisfied() ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 


    } 

}


