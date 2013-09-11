namespace Croc.Bpc.GsScannerDriver 
{ 
    internal interface IManufProps 
    { 
        byte[] ToByteArray(); 
        int SerialNumber { get; } 
        short ShiftLines { get; } 
        #region Параметры маркера 
        short On { get; set; } 
        short Off { get; set; } 
        short MarkingTime { get; set; } 
        short RollbackTime { get; set; } 
        short DownTime { get; set; } 
        #endregion 
    } 
}
