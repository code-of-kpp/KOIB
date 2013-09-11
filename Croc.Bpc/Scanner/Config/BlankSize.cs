using System; 
namespace Croc.Bpc.Scanner.Config 
{ 
    public class BlankSize 
    { 
        private int _hashCode; 
        public int Width 
        { 
            get; 
            private set; 
        } 
        public int Height 
        { 
            get; 
            private set; 
        } 
        public int Delta 
        { 
            get; 
            private set; 
        } 
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
