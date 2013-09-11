using System; 
using System.IO; 
using System.Runtime.InteropServices; 
namespace Croc.Bpc.GsScannerDriver 
{ 
    public enum WorkMode : short 
    { 
        Work = 1, 
        Debug = 0 
    } 
    public enum DoubleSheet : short 
    { 
        On = 1, 
        Off = 0 
    } 
    public enum Marker : short 
    { 
        On = 1, 
        Off = 0 
    } 
    public enum WhiteCoeff : short 
    { 
        On = 1, 
        Off = 0 
    } 
    public enum TuningMode : short 
    { 
        On = 1, 
        Off = 0 
    } 
    public enum DirtDetection : short 
    { 
        On = 1, 
        Off = 0 
    } 
    [Flags] 
    public enum Lamps : short 
    { 
        RedOn = 1, 
        GreenOn = 2, 
        BothOn = 3, 
        RedOff    = 0x10, 
        GreenOff = 0x20, 
        BothOff = 0x30, 
    } 
    [StructLayout(LayoutKind.Sequential)] 
    public struct ScannerProps 
    { 
        public ScannerProps( byte[] data ) 
        { 
            BinaryReader br = new System.IO.BinaryReader( new MemoryStream( data ) ); 
            WorkMode = (WorkMode)br.ReadInt16(); 
            DpiX0 = br.ReadInt16(); 
            DpiY0 = br.ReadInt16(); 
            DpiX1 = br.ReadInt16(); 
            DpiY1 = br.ReadInt16(); 
            DoubleSheet = (DoubleSheet)br.ReadInt16(); 
            Marker = (Marker)br.ReadInt16(); 
            DoubleSheetLevelL = br.ReadInt16(); 
            WhiteCoeff = (WhiteCoeff)br.ReadInt16(); 
            BinaryThreshold0 = br.ReadInt16(); 
            BinaryThreshold1 = br.ReadInt16(); 
            MinSheetLength = br.ReadInt16(); 
            MaxSheetLength = br.ReadInt16(); 
            DoubleSheetLevelR = br.ReadInt16(); 
            TuningMode = (TuningMode)br.ReadInt16(); 
            MarkerWork = br.ReadInt16(); 
            DirtDetection = (DirtDetection)br.ReadInt16(); 
            OfflineMode = br.ReadInt16(); 
            Lamps = (Lamps)br.ReadInt16(); 
            reserv = new short[21]; 
            for ( int i = 0; i < reserv.Length; i++ ) 
            { 
                reserv[i] = br.ReadInt16(); 
            } 
            br.Close(); 
        } 
        public byte[] Data 
        { 
            get 
            { 
                MemoryStream ms = new MemoryStream( 80 ); 
                BinaryWriter bw = new System.IO.BinaryWriter( ms ); 
                bw.Write((short)WorkMode); 
                bw.Write(DpiX0); 
                bw.Write(DpiY0); 
                bw.Write(DpiX1); 
                bw.Write(DpiY1); 
                bw.Write((short)DoubleSheet); 
                bw.Write((short)Marker); 
                bw.Write(DoubleSheetLevelL); 
                bw.Write((short)WhiteCoeff); 
                bw.Write(BinaryThreshold0); 
                bw.Write(BinaryThreshold1); 
                bw.Write(MinSheetLength); 
                bw.Write(MaxSheetLength); 
                bw.Write(DoubleSheetLevelR); 
                bw.Write((short)TuningMode); 
                bw.Write(MarkerWork); 
                bw.Write((short)DirtDetection); 
                bw.Write(OfflineMode); 
                bw.Write((short)Lamps); 
                for ( int i = 0; i < reserv.Length; i++ ) 
                { 
                    bw.Write(reserv[i]); 
                } 
                bw.Flush(); 
                byte[] data = ms.ToArray(); 
                bw.Close(); 
                return data; 
            } 
        } 
        public WorkMode WorkMode; 
        public short    DpiX0; 
        public short    DpiY0; 
        public short    DpiX1; 
        public short    DpiY1; 
        public DoubleSheet    DoubleSheet; 
        public Marker    Marker; 
        public short    DoubleSheetLevelL; 
        public WhiteCoeff    WhiteCoeff; 
        public short    BinaryThreshold0; 
        public short    BinaryThreshold1; 
        public short    MinSheetLength; 
        public short    MaxSheetLength; 
        public short    DoubleSheetLevelR; 
        public TuningMode    TuningMode; 
        public short    MarkerWork; 
        public DirtDetection    DirtDetection; 
        public short    OfflineMode; 
        public Lamps    Lamps; 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=21)] 
        public short[]    reserv; 
    } 
    public enum PropName : short 
    { 
        WorkMode            = 0x00, 
        DpiX0                = 0x01, 
        DpiY0                = 0x02, 
        DpiX1                = 0x03, 
        DpiY1                = 0x04, 
        DoubleSheet            = 0x05, 
        Marker                = 0x06, 
        DoubleSheetLevelL    = 0x07, 
        WhiteCoeff            = 0x08, 
        BinaryThreshold0    = 0x09, 
        BinaryThreshold1    = 0x0A, 
        MinSheetLength        = 0x0B, 
        MaxSheetLength        = 0x0C, 
        DoubleSheetLevelR    = 0x0D, 
        TuningMode            = 0x0E, 
        MarkerWork            = 0x0F, 
        DirtDetection        = 0x10, 
        OfflineMode            = 0x11, 
        Lamps                = 0x12, 
    } 
    struct MotorData 
    { 
        public MotorData( short number, short onoff, short dir, short step ) 
        { 
            this.number = number; 
            this.onoff = onoff; 
            this.dir = dir; 
            this.step = step; 
        } 
        public MotorData( byte[] data ) 
        { 
            BinaryReader br = new System.IO.BinaryReader( new MemoryStream( data ) ); 
            this.number = br.ReadInt16(); 
            this.onoff = br.ReadInt16(); 
            this.dir = br.ReadInt16(); 
            this.step = br.ReadInt16(); 
            br.Close(); 
        } 
        public byte[] Data 
        { 
            get 
            { 
                MemoryStream ms = new MemoryStream( 8 ); 
                BinaryWriter bw = new System.IO.BinaryWriter( ms ); 
                bw.Write(number); 
                bw.Write(onoff); 
                bw.Write(dir); 
                bw.Write(step); 
                bw.Flush(); 
                byte[] data = ms.ToArray(); 
                bw.Close(); 
                return data; 
            } 
        } 
        short    number; 
        short    onoff; 
        short    dir; 
        short    step; 
    } 
}
