using System; 

 

 

namespace Croc.Bpc.Scanner.Config 

{ 

    /// <summary> 

    /// Размер бланка 

    /// </summary> 

    public class BlankSize 

    { 

        /// <summary> 

        /// Хеш-код 

        /// </summary> 

        private int _hashCode; 

 

 

        /// <summary> 

        /// Ширина бланка 

        /// </summary> 

        public int Width 

        { 

            get; 

            private set; 

        } 

        /// <summary> 

        ///	Высота бланка 

        /// </summary> 

        public int Height 

        { 

            get; 

            private set; 

        } 

        /// <summary> 

        ///	Допустимая дельта для расчета минимальной и максимальной длины бланка 

        /// </summary> 

        public int Delta 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

		///	Конструктор 

		/// </summary> 

        /// <param name="width">ширина бланка</param> 

        /// <param name="height">высота бланка</param> 

        /// <param name="delta">Допустимая дельта для расчета минимальной и максимальной длины бланка</param> 

        public BlankSize(int width, int height, int delta) 

		{ 


			Width = width; 

			Height = height; 

			Delta = delta; 

 

 

            _hashCode = (Width * 1000000 + Height * 1000 + Delta).GetHashCode(); 

		} 

 

 

        public override bool Equals(object obj) 

        { 

            var other = (BlankSize)obj; 

            return this.Width == other.Width && this.Height == other.Height && this.Delta == other.Delta; 

        } 

 

 

        public override int GetHashCode() 

        { 

            return _hashCode; 

        } 

    } 

}


