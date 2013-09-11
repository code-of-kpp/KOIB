using System; 
namespace Croc.Bpc.GsScannerDriver 
{ 
    struct comlen 
    { 
        public comlen(byte[] data) 
        { 
            CommandID = 
                data[0] 
                | data[1] << 8 
                | data[2] << 16 
                | data[3] << 24; 
            datalen = 
                data[4] 
                | data[5] << 8 
                | data[6] << 16 
                | data[7] << 24; 
        } 
        public byte[] data() 
        { 
            byte[] buf = new byte[8]; 
            int i = CommandID; 
            buf[0] = (byte)(i & 0xFF); 
            buf[1] = (byte)((i >> 8) & 0xFF); 
            buf[2] = (byte)((i >> 16) & 0xFF); 
            buf[3] = (byte)(i >> 24); 
            i = datalen; 
            buf[4] = (byte)(i & 0xFF); 
            buf[5] = (byte)((i >> 8) & 0xFF); 
            buf[6] = (byte)((i >> 16) & 0xFF); 
            buf[7] = (byte)(i >> 24); 
            return buf; 
        } 
        public int CommandID; 
        public int datalen; 
    } 
    public enum Command 
    { 
        None = 0x0000, 
        umConnect = 0x1000, 
        umDisconnect = 0x1001, 
        umMotor = 0x1002, 
        umGetProps = 0x1003, 
        umSetProps = 0x1004, 
        umDrop = 0x1005, 
        umGetManufProps = 0x1006, 
        umSetManufProps = 0x1007, 
        umGetWhiteCoef = 0x1008, 
        umSetWhiteCoef = 0x1009, 
        umPing = 0x100A, 
        umScanGrayArea = 0x100B, 
        umScanBinArea = 0x100C, 
        umIndicator = 0x100D, 
        umIndicatorS = 0x100E, 
        umSetFormats = 0x100F, 
        umSetPageOffset = 0x1010, 
        umRevers = 0x1011, 
        umTestMarker = 0x1012, 
        umScanIndicator = 0x1013, 
        umGetVersion = 0x1014, 
        umSaveHalftone = 0x1015, 
        umSaveBinary = 0x1016, 
        umRestoreLamps = 0x1017, 
        umSetWorkZone = 0x1018, 
        umGetDoubleSheetSensorLastScanValue = 0x1019, 
        umGetSoundVolume = 0x101A, 
        umSetSoundVolume = 0x101B, 
        umSetDoubleSheetSensorCurrentLevel = 0x101C, 
        umCheckDoubleSheetSensor = 0x101D, 
        umSetLength = 0x1023, 
        msEvent = 0x8000, 
        umWantToConnect = msEvent | 1, 
        umDebugLine = msEvent | 2, 
        umConnected = msEvent | 3, 
        umPageIn = msEvent | 6, 
        umScanDopStrip = msEvent | 7, 
        umScanBinStrip = msEvent | 8, 
        umError = msEvent | 9, 
        umSheetDroped = msEvent | 10, 
        umDebugData = msEvent | 11, 
        umReadyToScanning = msEvent | 12, 
        umVoltStat = msEvent | 13, 
        umScanStop = msEvent | 14, 
    } 
    public class UserMessage 
    { 
        byte[] data; 
        Command command; 
        public Command Command 
        { 
            get { return command; } 
        } 
        public byte[] Data 
        { 
            get { return data; } 
        } 
        public UserMessage(Command command, byte[] data) 
        { 
            this.command = command; 
            this.data = data; 
        } 
        public UserMessage(byte[] recivedData) 
        { 
            command = (Command) 
                ( 
                recivedData[0] 
                | recivedData[1] << 8 
                | recivedData[2] << 16 
                | recivedData[3] << 24); 
            int dataLen = 
                recivedData[4] 
                | recivedData[5] << 8 
                | recivedData[6] << 16 
                | recivedData[7] << 24; 
            data = new byte[dataLen]; 
            Array.Copy(recivedData, 8, data, 0, dataLen); 
        } 
        public byte[] Buffer 
        { 
            get 
            { 
                byte[] buf = new byte[data.Length + 8]; 
                int i = unchecked((int)Command); 
                buf[0] = (byte)(i & 0xFF); 
                buf[1] = (byte)((i >> 8) & 0xFF); 
                buf[2] = (byte)((i >> 16) & 0xFF); 
                buf[3] = (byte)(i >> 24); 
                i = data.Length; 
                buf[4] = (byte)(i & 0xFF); 
                buf[5] = (byte)((i >> 8) & 0xFF); 
                buf[6] = (byte)((i >> 16) & 0xFF); 
                buf[7] = (byte)(i >> 24); 
                Array.Copy(data, 0, buf, 8, data.Length); 
                return buf; 
            } 
        } 
    } 
}
