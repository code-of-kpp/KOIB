using System.IO; 
using System.Runtime.InteropServices; 
namespace Croc.Bpc.GsScannerDriver.SIB2003 
{ 
    [StructLayout(LayoutKind.Sequential)] 
    public struct ManufProps : IManufProps 
    { 
        public int common_serial_number; 
        public short r1; 
        public short lamps; 
        public short r2; 
        public short r3; 
        public short r4; 
        public short r5; 
        public short adc_line0_b; 
        public short adc_line0_w; 
        public short r6; 
        public short r7; 
        public short adc_line1_b; 
        public short adc_line1_w; 
        public short marker_on_time; 
        public short marker_on_volt; 
        public short marker_off_time; 
        public short marker_off_volt; 
        public short common_bg_bin_th0; 
        public short common_bg_bin_th1; 
        public short common_dpi_x0; 
        public short common_dpi_x1; 
        public short common_dpi_y0; 
        public short common_dpi_y1; 
        public short ActivePoints; 
        public short Porog; 
        public short shift_lines; 
        public short stepOnDrop; 
        public short mainSpeed; 
        public short stepOnMarker; 
        public short gap0; 
        public short marker_on_long_time; 
        public short marker_off_long_time; 
        public short markerSpeed; 
        public short gap1; 
        public short marker_park_volt; 
        public short percentForDirt; 
        public short linesForDirt; 
        public short marker_hold_volt; 
        public short marker_hold_time; 
        public short doubleSheetLevelL; 
        public short doubleSheetParam; 
        public short doubleSheetLevelR; 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] 
        short[] reserv; 
        public ManufProps(byte[] data) 
        { 
            BinaryReader br = new System.IO.BinaryReader(new MemoryStream(data)); 
            common_serial_number = br.ReadInt32(); 
            r1 = br.ReadInt16(); 
            lamps = br.ReadInt16(); 
            r2 = br.ReadInt16(); 
            r3 = br.ReadInt16(); 
            r4 = br.ReadInt16(); 
            r5 = br.ReadInt16(); 
            adc_line0_b = br.ReadInt16(); 
            adc_line0_w = br.ReadInt16(); 
            r6 = br.ReadInt16(); 
            r7 = br.ReadInt16(); 
            adc_line1_b = br.ReadInt16(); 
            adc_line1_w = br.ReadInt16(); 
            marker_on_time = br.ReadInt16(); 
            marker_on_volt = br.ReadInt16(); 
            marker_off_time = br.ReadInt16(); 
            marker_off_volt = br.ReadInt16(); 
            common_bg_bin_th0 = br.ReadInt16(); 
            common_bg_bin_th1 = br.ReadInt16(); 
            common_dpi_x0 = br.ReadInt16(); 
            common_dpi_x1 = br.ReadInt16(); 
            common_dpi_y0 = br.ReadInt16(); 
            common_dpi_y1 = br.ReadInt16(); 
            ActivePoints = br.ReadInt16(); 
            Porog = br.ReadInt16(); 
            shift_lines = br.ReadInt16(); 
            stepOnDrop = br.ReadInt16(); 
            mainSpeed = br.ReadInt16(); 
            stepOnMarker = br.ReadInt16(); 
            gap0 = br.ReadInt16(); 
            marker_on_long_time = br.ReadInt16(); 
            marker_off_long_time = br.ReadInt16(); 
            markerSpeed = br.ReadInt16(); 
            gap1 = br.ReadInt16(); 
            marker_park_volt = br.ReadInt16(); 
            percentForDirt = br.ReadInt16(); 
            linesForDirt = br.ReadInt16(); 
            marker_hold_volt = br.ReadInt16(); 
            marker_hold_time = br.ReadInt16(); 
            doubleSheetLevelL = br.ReadInt16(); 
            doubleSheetParam = br.ReadInt16(); 
            doubleSheetLevelR = br.ReadInt16(); 
            reserv = new short[5]; 
            for (int i = 0; i < reserv.Length; i++) 
            { 
                reserv[i] = br.ReadInt16(); 
            } 
        } 
        public byte[] ToByteArray() 
        { 
            return new byte[] { }; 
        } 
        public int SerialNumber 
        { 
            get { return common_serial_number; } 
        } 
        public short ShiftLines 
        { 
            get { return shift_lines; } 
        } 
        #region Параметры маркера 
        public short On 
        { 
            get { return 0; } 
            set { } 
        } 
        public short Off 
        { 
            get { return 0; } 
            set { } 
        } 
        public short MarkingTime 
        { 
            get { return 0; } 
            set { } 
        } 
        public short RollbackTime 
        { 
            get { return 0; } 
            set { } 
        } 
        public short DownTime 
        { 
            get { return 0; } 
            set { } 
        } 
        #endregion 
    } 
}
