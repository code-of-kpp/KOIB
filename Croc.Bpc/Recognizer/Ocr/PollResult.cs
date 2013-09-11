using System; 

using System.Collections.Generic; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Результат голосования 

    /// </summary> 

    public class PollResult 

    { 

        /// <summary> 

        /// Квадраты 

        /// </summary> 

        protected List<int> _squares = new List<int>(); 

        /// <summary> 

        /// Номер бюллетеня 

        /// </summary> 

        public long PollNumber 

        { 

            get; 

            set; 

        } 

        /// <summary> 

        /// Признак действительности 

        /// </summary> 

        public bool IsValid 

        { 

            get; 

            set; 

        } 

        /// <summary> 

        /// Количество квадратов 

        /// </summary> 

        public int Count 

        { 

            get 

            { 

                return _squares.Count; 

            } 

        } 

        /// <summary> 

        /// Получить отметку в квадрате 

        /// </summary> 

        /// <param name="index">индекс квадрата</param> 

        /// <returns></returns> 

        public int this[int index] 

        { 

            get 

            { 


                return _squares[index]; 

            } 

        } 

        /// <summary> 

        /// Добавить результат распознавания квадрата 

        /// </summary> 

        /// <param name="check">результат распознавания квадрата</param> 

        public void Add(int check) 

        { 

            _squares.Add(check); 

        } 

    } 

}


