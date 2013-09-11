using System; 

 

 

namespace Croc.Bpc.Scanner 

{ 

    /// <summary> 

    /// Коды логических причин реверса 

    /// </summary> 

    public enum LogicalReverseReason : int 

    { 

        /// <summary> 

        /// Принятие листа запрещено 

        /// </summary> 

        SheetReceivingForbidden = 100, 

        /// <summary> 

        /// Недопустимый номер бюллетеня 

        /// </summary> 

        InvalidBlankNumber = 101, 

        /// <summary> 

        /// Бюллетень не имеет текущего режима голосования 

        /// </summary> 

        BlankHasNoCurrentVoteRegime = 102, 

    } 

}


