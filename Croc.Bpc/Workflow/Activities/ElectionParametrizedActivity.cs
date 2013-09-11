using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Bpc.Workflow.Activities 

{ 

    /// <summary> 

    /// Действие, у которого есть параметр "Выборы" 

    /// </summary> 

    /// <remarks>на самом деле, в качестве параметра передается идентификатор выборов</remarks> 

    [Serializable] 

    public class ElectionParametrizedActivity : BpcCompositeActivity 

    { 

        private int _electionIndex; 

        /// <summary> 

        /// Индекс выборов 

        /// </summary> 

        public int ElectionIndex 

        { 

            get 

            { 

                return _electionIndex; 

            } 

            set 

            { 

                _electionIndex = value; 

                _election = _electionManager.SourceData.Elections[_electionIndex]; 

                _blankId = _electionManager.SourceData.GetBlankIdByElectionNumber(_election.ElectionId); 

            } 

        } 

        private Election.Voting.Election _election; 

        /// <summary> 

        /// Выборы 

        /// </summary> 

        protected Election.Voting.Election Election 

        { 

            get 

            { 

                return _election; 

            } 

        } 

        private string _blankId; 

        /// <summary> 

        /// ИД бланка, который соответствует выборам 

        /// </summary> 

        public string BlankId 

        { 

            get 


            { 

                return _blankId; 

            } 

        } 

        /// <summary> 

        /// Номер выборов 

        /// </summary> 

        public int ElectionNumber 

        { 

            get 

            { 

                return _electionIndex + 1; 

            } 

        } 

    } 

}


