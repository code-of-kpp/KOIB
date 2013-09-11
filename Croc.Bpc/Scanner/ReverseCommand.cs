using System; 

 

 

namespace Croc.Bpc.Scanner 

{ 

    /// <summary> 

    /// Команда реверса листа в сканере 

    /// </summary> 

    internal class ReverseCommand 

    { 

        /// <summary> 

        /// Сканер, на котором нужно выполнить команду 

        /// </summary> 

        private IScanner _scanner; 

 

 

        /// <summary> 

        /// Код причины реверса 

        /// </summary> 

        public int ReasonCode 

        { 

            get; 

            private set; 

        } 

        /// <summary> 

        /// Выполнена ли команда? 

        /// </summary> 

        public bool Completed 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="scanner">Сканер, на котором нужно выполнить команду</param> 

        /// <param name="reasonCode">Код причины реверса</param> 

        public ReverseCommand(IScanner scanner, int reasonCode) 

        { 

            _scanner = scanner; 

            ReasonCode = reasonCode; 

            Completed = false; 

        } 

 

 

        /// <summary> 

        /// Отправить команду реверса на сканер 

        /// </summary> 


        /// <returns>true - сканер принял команду, false - сканер отклонил команду</returns> 

        public bool SendCommand() 

        {   

            var reverseResult = _scanner.Reverse(); 

            Completed = (reverseResult == ReverseCommandResult.Accepted); 

            return Completed; 

        } 

    } 

}


