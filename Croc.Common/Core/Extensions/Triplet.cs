namespace Croc.Core.Extensions 
{ 
    public class Triplet<TFirst, TSecond, TThird> 
    { 
        public TFirst First 
        { 
            get; 
            private set; 
        } 
        public TSecond Second 
        { 
            get; 
            private set; 
        } 
        public TThird Third 
        { 
            get; 
            private set; 
        } 
        public Triplet(TFirst first, TSecond second, TThird third) 
        { 
            First = first; 
            Second = second; 
            Third = third; 
        } 
    } 
}
