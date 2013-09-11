namespace Croc.Bpc.Scanner 
{ 
    public enum ScannerError : short 
    { 
        PaperTooLong = 1, 
        CantPaperUp = 2, 
        PaperTooShort = 3, 
        WrongPaperFormat = 4, 
        CantRevercePaper = 5, 
        WrongPaperOffset = 6, 
        DirtOnZeroSide = 7, 
        DirtOnFirstSide = 8, 
        DoublePaper = 9, 
        DoublePaperSensorFail = 10, 
        PaperEscape = 11, 
        SystemError = 12, 
        ZeroCisFail = 13, 
        FirstCisFail = 14, 
        InvalidReverse = 15, 
        PowerFailure = 16, 
        LongDelay = 17, 
        JoinedDrop = 18, 
    } 
}
