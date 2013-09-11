namespace Croc.Bpc.Scanner 
{ 
    public class SheetProcessingError 
    { 
        public readonly int Code; 
        public readonly string Description; 
        public readonly bool IsReverseReason; 
        public readonly bool NeedAlert; 
        public bool IsRepeated; 
        public SheetProcessingError( 
            int code, string description, bool isReverseReason, bool needAlert) 
        { 
            Code = code; 
            Description = description; 
            IsReverseReason = isReverseReason; 
            NeedAlert = needAlert; 
        } 
        public override string ToString() 
        { 
            return string.Format("[{0}] {1}", Code, Description); 
        } 
    } 
}
