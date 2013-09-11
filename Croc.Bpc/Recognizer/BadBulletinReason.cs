using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Bpc.Election.Voting 

{ 

    /// <summary> 

    /// Обобщенные причины НУФ для вывода на ЖКИ 

    /// </summary> 

    public enum BadBulletinReason 

    { 

        /// <summary> 

        /// Нуф: маркер 

        /// </summary> 

        Marker, 

 

 

        /// <summary> 

        /// Нуф: линии 

        /// </summary> 

        Lines, 

 

 

        /// <summary> 

        /// Нуф: квадраты 

        /// </summary> 

        Squares, 

 

 

        /// <summary> 

        /// Нуф: печать 

        /// </summary> 

        Stamp, 

 

 

        /// <summary> 

        /// НУФ: точки 

        /// </summary> 

        Refp 

    } 

}


