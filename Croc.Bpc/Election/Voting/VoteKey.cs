using System; 

using System.Text; 

using System.Collections; 

using System.Collections.Generic; 

 

 

namespace Croc.Bpc.Election.Voting 

{     

    /// <summary> 

    /// ????, ???????????? ???-?? ??????? 

    /// </summary> 

    [Serializable] 

    public class VoteKey 

    { 

        /// <summary> 

        /// ???????? ????? ??????? 

        /// </summary> 

        private int _scannerSerialNumber = 0; 

        private bool _scannerSerialNumberHasValue = false; 

 

 

		/// <summary> 

		/// ?????? ??????????? 

		/// </summary> 

		public VoteKey() 

		{ } 

 

 

		/// <summary> 

		/// ??????????? ?? ???????? 

		/// </summary> 

		/// <param name="type">??? ??????</param> 

		/// <param name="mode">????? ???????????</param> 

		/// <param name="scannerSerial">???????? ???????</param> 

		/// <param name="candidate">?? ?????????</param> 

		/// <param name="election">?? ???????</param> 

		public VoteKey(BlankType? type, 

			VotingMode? mode, 

			int? scannerSerial, 

			string candidate, 

			string election, 

			string blankId) 

		{ 

			BlankType = type; 

			VotingMode = mode; 

			ScannerSerialNumber = scannerSerial; 

			CandidateId = candidate; 

			ElectionNum = election; 

			BlankId = blankId; 

		} 


 
 

        public int? ScannerSerialNumber 

        { 

            get 

            { 

                return _scannerSerialNumberHasValue ? (int?)_scannerSerialNumber : null; 

            } 

            set 

            { 

                if (value.HasValue) 

                { 

                    _scannerSerialNumber = value.Value; 

                    _scannerSerialNumberHasValue = true; 

                } 

                else 

                    _scannerSerialNumberHasValue = false; 

            } 

        } 

 

 

        /// <summary> 

        ///	????? ??????????? 

        /// </summary> 

        private VotingMode? _votingMode; 

        private bool _votingModeHasValue = false; 

 

 

        public VotingMode? VotingMode 

        { 

            get 

            { 

                return _votingModeHasValue ? (VotingMode?)_votingMode : null; 

            } 

            set 

            { 

                if (value.HasValue) 

                { 

                    _votingMode = value.Value; 

                    _votingModeHasValue = true; 

                } 

                else 

                    _votingModeHasValue = false; 

            } 

        } 

 

 

        /// <summary> 

        ///	????????????? ????????? 

        /// </summary> 


        public string CandidateId { get; set; } 

 

 

        /// <summary> 

        ///	????????????? ??????? 

        /// </summary> 

        public string ElectionNum { get; set; } 

 

 

        /// <summary> 

        ///	????????????? ?????? 

        /// </summary> 

        public string BlankId { get; set; } 

 

 

        /// <summary> 

        ///	????????? ????????? 

        /// </summary> 

        private BlankType? _blankType; 

        private bool _blankTypeHasValue = false; 

 

 

        public BlankType? BlankType 

        { 

            get 

            { 

                return _blankTypeHasValue ? (BlankType?)_blankType : null; 

            } 

            set 

            { 

                if (value.HasValue) 

                { 

                    _blankType = value.Value; 

                    _blankTypeHasValue = true; 

                } 

                else 

                    _blankTypeHasValue = false; 

            } 

        } 

 

 

        /// <summary> 

        ///		?????????????? ?????. ??? ?? ?????????? ????????? ???????????????. 

        ///		??? ?????????? ?????? ??????????????? ???????? 

        /// </summary> 

        /// <param name="obj">??????</param> 

        /// <returns> 

        ///		bool - true - ???? ???????? ???????????? ? false ? ????????? ?????? 

        /// </returns> 

        public override bool Equals(object obj) 


        { 

            var other = obj as VoteKey; 

 

 

            if (other == null || 

                other.BlankType.HasValue && other.BlankType.Value != BlankType || 

                other.CandidateId != null && other.CandidateId.CompareTo(CandidateId) != 0 || 

                other.ElectionNum != null && other.ElectionNum.CompareTo(ElectionNum) != 0 || 

                other.BlankId != null && other.BlankId.CompareTo(BlankId) != 0 || 

                other.ScannerSerialNumber.HasValue && other.ScannerSerialNumber != ScannerSerialNumber || 

                other.VotingMode.HasValue && other.VotingMode.Value != VotingMode) 

                return false; 

 

 

            return true; 

        } 

 

 

        /// <summary> 

        /// ????????? ???-???? 

        /// </summary> 

        /// <returns>???-???</returns> 

        public override int GetHashCode() 

        { 

            int nResult = 0; 

 

 

            // ??? ?????? 

            if (BlankType.HasValue) 

            { 

                nResult += (int)BlankType.Value; 

            } 

            // ????? ??????????? 

            if (VotingMode.HasValue) 

            { 

                nResult += ((int)VotingMode.Value) << 2; 

            } 

            // ????? ??????? 

            if (ScannerSerialNumber.HasValue) 

            { 

                nResult += (ScannerSerialNumber.Value % 16) << 5; 

            } 

            // ?????? ????????? 

            if (CandidateId != null) 

            { 

                nResult ^= CandidateId.GetHashCode(); 

            } 

            // ????? ????????? 

            if (BlankId != null) 

            { 


                nResult ^= BlankId.GetHashCode(); 

            } 

            // ????????????? ??????? 

            if (ElectionNum != null) 

            { 

                nResult ^= ElectionNum.GetHashCode(); 

            } 

            // ?????????? ??????? ???-??? 

            return nResult; 

        } 

 

 

        /// <summary> 

        ///		????????? ???????????? ???????? ????? 

        /// </summary> 

        /// <param name="mask">?????</param> 

        /// <returns> 

        ///		bool - ?????????? true ???? ??????? ?????? ????????????? 

        ///		?????????? ????? ? false, ? ????????? ?????? 

        /// </returns> 

        public bool CheckMask(VoteKey mask) 

        { 

            if (mask.BlankType.HasValue) 

            { 

                // ???? ?? ?????? ??? ?????? - ?????? ??? ?????? ????? ?? ????????? 

                if (!BlankType.HasValue) 

                    return false; 

 

 

                // ???? ?? ??????????? ????? All (?.?. ??????? ??? ?????????) 

                if (mask.BlankType != Voting.BlankType.All) 

                { 

                    // ????????????? ???????? ??????????? ??? NotValid ? AllButBad, ?????????? ???????????????? 

                    if (mask.BlankType.Value == Voting.BlankType.AllButBad) 

                    { 

                        if (BlankType.Value == Voting.BlankType.Bad) 

                            return false; 

                    } 

                    else 

                    { 

                        if (mask.BlankType.Value == Voting.BlankType.NotValid) 

                        { 

                            if (BlankType.Value == Voting.BlankType.Valid || 

                                BlankType.Value == Voting.BlankType.Bad) 

                                return false; 

                        } 

                        else if (mask.BlankType.Value != BlankType.Value) 

                            return false; 

                    } 

                } 


            } 

 

 

            if (mask.CandidateId != null && mask.CandidateId != CandidateId || 

                mask.ElectionNum != null && mask.ElectionNum != ElectionNum || 

                mask.BlankId != null && mask.BlankId != BlankId || 

                mask.ScannerSerialNumber.HasValue && mask.ScannerSerialNumber.Value != ScannerSerialNumber || 

                mask.VotingMode.HasValue && mask.VotingMode.Value != VotingMode) 

                return false; 

 

 

            return true; 

        } 

 

 

        /// <summary> 

        /// ???? ????????? ????????????? ?????  

        /// </summary> 

        public override string ToString() 

        { 

            StringBuilder sRes = new StringBuilder("{"); 

 

 

            if (BlankType.HasValue) 

                sRes.AppendFormat("BT = {0}; ", BlankType.Value); 

 

 

            if (CandidateId != null) 

                sRes.AppendFormat("CI = {0}; ", CandidateId); 

 

 

            if (ElectionNum != null) 

                sRes.AppendFormat("EL = {0}; ", ElectionNum); 

 

 

            if (BlankId != null) 

                sRes.AppendFormat("BI = {0}; ", BlankId); 

 

 

            if (ScannerSerialNumber.HasValue) 

                sRes.AppendFormat("SN = {0}; ", ScannerSerialNumber.Value); 

 

 

            if (VotingMode.HasValue) 

                sRes.AppendFormat("VM = {0}; ", VotingMode.Value); 

 

 

            sRes.Append("}"); 

 

 


            return sRes.ToString(); 

        } 

    } 

}


