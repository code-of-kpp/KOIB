using System; 
namespace Croc.Bpc.Scanner.Config 
{ 
    public class BlankOffset 
    { 
        private int _hashCode; 
        public int Width 
        { 
            get; 
            private set; 
        } 
        public int MaxShift 
        { 
            get; 
            private set; 
        } 
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
