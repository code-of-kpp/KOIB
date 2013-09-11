using System; 

 

 

namespace Croc.Bpc.Scanner.Config 

{ 

    /// <summary> 

    /// Допустимые смещения бланка 

    /// </summary> 

    public class BlankOffset 

    { 

        /// <summary> 

        /// Хеш-код 

        /// </summary> 

        private int _hashCode; 

 

 

        /// <summary> 

        /// ширина бланка 

        /// </summary> 

        public int Width 

        { 

            get; 

            private set; 

        } 

        /// <summary> 

        /// максимально допустимое смещение относительно левого края 

        /// </summary> 

        public int MaxShift 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="width">ширина бланка</param> 

        /// <param name="maxShift">максимально допустимое смещение относительно левого края</param> 

        public BlankOffset(int width, int maxShift) 

        { 

            Width = width; 

            MaxShift = maxShift; 

 

 

            _hashCode = (Width * 1000 + MaxShift).GetHashCode(); 

        } 

 

 

        public override bool Equals(object obj) 


        { 

            var other = (BlankOffset)obj; 

            return this.Width == other.Width && this.MaxShift == other.MaxShift; 

        } 

 

 

        public override int GetHashCode() 

        { 

            return _hashCode; 

        } 

    } 

}


