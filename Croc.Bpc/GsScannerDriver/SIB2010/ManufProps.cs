using System; 
using System.Collections.Generic; 
using System.IO; 
using System.Runtime.InteropServices; 
using System.Text; 
namespace Croc.Bpc.GsScannerDriver.SIB2010 
{ 
    [StructLayout(LayoutKind.Sequential)] 
    public struct ManufProps : IManufProps 
    { 
        public int common_serial_number; 
        public short lamps; 
        public short adc_line_b; 
        public short adc_line_w; 
        public short common_bg_bin_th; 
        public short common_dpi_x; 
        public short common_dpi_y; 
        public short ActivePoints; 
        public short Porog; 
        public short speedMotor; 
        public short percentForDirt; 
        public short linesForDirt; 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] 
        public short[] levelList; 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] 
        public short[] levelDoubleList; 
        public short doubleSheetParam; 
        public short shiftLine; 
        public short minVolt, maxVolt; 
        public short maxDelay; 
        public short codOn; 
        public short codOff; 
        public short timeDown; 
        public short typeLCD; 
        public short timeout; 
        public short timeBack; 
        public short timeLine; 
        public ManufProps(byte[] data) 
        { 
            var br = new BinaryReader(new MemoryStream(data)); 
            common_serial_number = br.ReadInt32(); 
            lamps = br.ReadInt16(); 
            adc_line_b = br.ReadInt16(); 
            adc_line_w = br.ReadInt16(); 
            common_bg_bin_th = br.ReadInt16(); 
            common_dpi_x = br.ReadInt16(); 
            common_dpi_y = br.ReadInt16(); 
            ActivePoints = br.ReadInt16(); 
            Porog = br.ReadInt16(); 
            speedMotor = br.ReadInt16(); 
            percentForDirt = br.ReadInt16(); 
            linesForDirt = br.ReadInt16(); 
            levelList = new short[2]; 
            for (int i = 0; i < levelList.Length; i++) 
            { 
                levelList[i] = br.ReadInt16(); 
            } 
            levelDoubleList = new short[2]; 
            for (int i = 0; i < levelDoubleList.Length; i++) 
            { 
                levelDoubleList[i] = br.ReadInt16(); 
            } 
            doubleSheetParam = br.ReadInt16(); 
            shiftLine = br.ReadInt16(); 
            minVolt = br.ReadInt16(); 
            maxVolt = br.ReadInt16(); 
            maxDelay = br.ReadInt16(); 
            codOn = br.ReadInt16(); 
            codOff = br.ReadInt16(); 
            timeDown = br.ReadInt16(); 
            typeLCD = br.ReadInt16(); 
            timeout = br.ReadInt16(); 
            timeBack = br.ReadInt16(); 
            timeLine = br.ReadInt16(); 
        } 
        public byte[] ToByteArray() 
        { 
            using (var stream = new MemoryStream(60)) 
            { 
                var bw = new BinaryWriter(stream); 
                bw.Write(common_serial_number); 
                bw.Write(lamps); 
                bw.Write(adc_line_b); 
                bw.Write(adc_line_w); 
                bw.Write(common_bg_bin_th); 
                bw.Write(common_dpi_x); 
                bw.Write(common_dpi_y); 
                bw.Write(ActivePoints); 
                bw.Write(Porog); 
                bw.Write(speedMotor); 
                bw.Write(percentForDirt); 
                bw.Write(linesForDirt); 
                bw.Write(levelList[0]); 
                bw.Write(levelList[1]); 
                bw.Write(levelDoubleList[0]); 
                bw.Write(levelDoubleList[1]); 
                bw.Write(doubleSheetParam); 
                bw.Write(shiftLine); 
                bw.Write(minVolt); 
                bw.Write(maxVolt); 
                bw.Write(maxDelay); 
                bw.Write(codOn); 
                bw.Write(codOff); 
                bw.Write(timeDown); 
                bw.Write(typeLCD); 
                bw.Write(timeout); 
                bw.Write(timeBack); 
                bw.Write(timeLine); 
                bw.Write(new byte[] {0, 0}); 
                bw.Flush(); 
                return stream.ToArray(); 
            } 
        } 
        public int SerialNumber 
        { 
            get { return common_serial_number; } 
        } 
        public short ShiftLines 
        { 
            get { return 0; } 
        } 
        #region Параметры маркера 
        public short On 
        { 
            get { return codOn; } 
            set { codOn = value; } 
        } 
        public short Off 
        { 
            get { return codOff; } 
            set { codOff = value; } 
        } 
        public short MarkingTime 
        { 
            get { return timeLine; } 
            set { timeLine = value; } 
        } 
        public short RollbackTime 
        { 
            get { return timeBack; } 
            set { timeBack = value; } 
        } 
        public short DownTime 
        { 
            get { return timeDown; } 
            set { timeDown = value; } 
        } 
        #endregion 
    } 
}
