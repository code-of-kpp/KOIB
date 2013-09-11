using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Workflow.ComponentModel; 

using Croc.Core.Diagnostics; 

using Croc.Bpc.Election.Voting; 

 

 

namespace Croc.Bpc.Workflow.Activities.Summarizing 

{ 

    /// <summary> 

    /// Зачитывание протокола голосования 

    /// </summary> 

    [Serializable] 

    public class SaveVotingResultsActivity : BpcCompositeActivity 

    { 

        /// <summary> 

        /// Поиск пути к файлу для сохранения протокола с результатами голосования 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey FindFilePathToSaveVotingResultProtocol( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _electionManager.FindFilePathToSaveVotingResultProtocol() 

                ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// Сохранение протокола с результатами голосования 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey SaveVotingResultProtocol( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _electionManager.SaveVotingResultProtocol() 

                ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

    } 

}


