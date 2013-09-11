using System; 
using System.Collections.Generic; 
namespace Croc.Bpc.Recognizer.Ocr 
{ 
    public class PollResult 
    { 
        protected List<int> _squares = new List<int>(); 
        public long PollNumber 
        { 
            get; 
            set; 
        } 
        public bool IsValid 
        { 
            get; 
            set; 
        } 
        public int Count 
        { 
            get 
            { 
                return _squares.Count; 
            } 
        } 
        public int this[int index] 
        { 
            get 
            { 
                return _squares[index]; 
            } 
        } 
        public void Add(int check) 
        { 
            _squares.Add(check); 
        } 
    } 
}
