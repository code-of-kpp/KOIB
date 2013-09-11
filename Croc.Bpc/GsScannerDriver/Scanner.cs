using System; 
using System.Collections; 
using System.Collections.Specialized; 
using System.IO; 
using System.Linq; 
using System.Net; 
using System.Net.Sockets; 
using System.Runtime.InteropServices; 
using System.Text; 
using System.Threading; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Scanner; 
using Croc.Bpc.Utils; 
using Croc.Bpc.Utils.Images; 
using Croc.Bpc.Utils.Images.Tiff; 
using Croc.Bpc.Voting; 
using Croc.Core.Diagnostics; 
using Croc.Core.Extensions; 
namespace Croc.Bpc.GsScannerDriver 
{ 
    public class Scanner : IScanner 
    { 
        private static readonly HardwareConfiguration[] s_ports = 
        { 
            new HardwareConfiguration(20051, 20052, 20053, SIB2003.SharedMemory.MaxLines,  
                SIB2003.SharedMemory.DotsOneLine, ScannerVersion.V2003, HardwareOptions.Default), 
            new HardwareConfiguration(20041, 20042, 20043, SIB2005.SharedMemory.MaxLines,  
                SIB2005.SharedMemory.DotsOneLine, ScannerVersion.V2005, HardwareOptions.Default), 
            new HardwareConfiguration(20061, 20062, 20063, SIB2009.SharedMemory.MaxLines,  
                SIB2009.SharedMemory.DotsOneLine, ScannerVersion.V2009, HardwareOptions.Default), 
            new HardwareConfiguration(20071, 20072, 20073, SIB2010.SharedMemory.MaxLines,  
                SIB2010.SharedMemory.DotsOneLine, ScannerVersion.V2010,  
                HardwareOptions.OneSide | HardwareOptions.EnhancedDrop | HardwareOptions.RightToLeftHalftone), 
            new HardwareConfiguration(20074, 20075, 20076, SIB2010.SharedMemory.MaxLines,  
                SIB2010.SharedMemory.DotsOneLine, ScannerVersion.V2010,  
                HardwareOptions.OneSide | HardwareOptions.EnhancedDrop), 
        }; 
        public static int MaxPortVariant = s_ports.Length; 
        private const long LOCAL_HOST = 0x0100007F; 
        private const int TIMEOUT = 1000; 
        private const int MAX_SHEET_FORMATS = 20; 
        private readonly ILogger _logger; 
        private readonly HardwareConfiguration _currentConfiguration; 
        private ScannerSocket _scannerSocket; 
        private ScannerProps _scannerProps; 
        private byte[] _whiteCoeff; 
        private readonly MemoryBlock _whiteCoeffU = new MemoryBlock(); 
        private IManufProps _manufProps; 
        private BaseSharedMemory _sh; 
        private Thread _workThread; 
        private readonly HybridDictionary _waitEvents; 
        private static readonly object s_waitEventsSync = new object(); 
        private readonly HybridDictionary _recevedEvents; 
        private static readonly object s_recevedEventsSync = new object(); 
        private bool _scannerBusy; 
        private bool _sheetScanning; 
        private int _linesCount; 
        private int _treshold0; 
        private int _treshold1; 
        private const int NUMBER_OF_SIDES = 2; 
        private readonly short[] _x; 
        private readonly short[] _y; 
        public const int DOTS_PER_BYTE_BINARY = 8; 
        public const int DOTS_PER_BYTE_HALFTONE = 1; 
        private readonly ValidPageOffset[] _validPageOffsets; 
        private readonly ValidPageLength[] _validPageLengths; 
        private IScannerEventHandler _eventHandler; 
        private bool _validLength; 
        private IPAddress _ipAddress; 
        private IPAddress _ipBroadCastAddress; 
        private const string IP_ADDRESS_SIGNATURE = "inet addr:"; 
        private const string IP_BROAD_CAST_SIGNATURE = "Bcast:"; 
        private const string IP_ADDRESS_DETECTOR = "ifconfig"; 
        private readonly int _status; 
        private readonly Queue _events; 
        private static readonly object s_eventsSync = new object(); 
        private Thread _sendEventsThread; 
        private readonly ManualResetEvent _mre; 
        #region IDisposable members 
        ~Scanner() 
        { 
            Dispose(false); 
        } 
        public void Dispose() 
        { 
            Dispose(true); 
        } 
        private void Dispose(bool disposing) 
        { 
            lock (this) 
            { 
                if (disposing) 
                { 
                    GC.SuppressFinalize(this); 
                } 
                if (_sendEventsThread != null) 
                { 
                    _sendEventsThread.SafeAbort(); 
                    _sendEventsThread = null; 
                } 
                if (_workThread != null) 
                { 
                    _workThread.SafeAbort(); 
                    _workThread = null; 
                } 
                if (_scannerSocket != null) 
                { 
                    ScanningEnabled = false; 
                    _scannerSocket.Close(); 
                    _scannerSocket = null; 
                } 
                if (_sh != null) 
                { 
                    _sh.Close(); 
                    _sh = null; 
                } 
            } 
        } 
        #endregion 
        public Scanner(ILogger logger) 
        { 
            _logger = logger; 
            _waitEvents = new HybridDictionary(); 
            _recevedEvents = new HybridDictionary(); 
            _events = new Queue(); 
            _mre = new ManualResetEvent(false); 
            _validPageLengths = new ValidPageLength[MAX_SHEET_FORMATS]; 
            _validPageOffsets = new ValidPageOffset[MAX_SHEET_FORMATS]; 
            _sheetScanning = false; 
            _scannerBusy = false; 
            foreach (HardwareConfiguration t in s_ports) 
            { 
                _currentConfiguration = t; 
                Socket udpSocketSend = null; 
                Socket udpSocketReceive = null; 
                Socket tcpSocket = null; 
                try 
                { 
                    udpSocketSend = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Unspecified); 
                    udpSocketReceive = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Unspecified); 
                    udpSocketReceive.Bind(new IPEndPoint(System.Net.IPAddress.Any, _currentConfiguration.UdpPortReceive)); 
                    byte[] data = { 1, 0, 0, 0 }; 
                    var um = new UserMessage(Command.umConnect, data); 
                    EndPoint ep = new IPEndPoint(LOCAL_HOST, _currentConfiguration.UdpPortSend); 
                    udpSocketSend.SendTo(um.Buffer, ep); 
                    if (!udpSocketReceive.Poll(TIMEOUT, SelectMode.SelectRead)) 
                    { 
                        continue; 
                    } 
                    data = new byte[1024]; 
                    udpSocketReceive.ReceiveFrom(data, ref ep); 
                    um = new UserMessage(data); 
                    if (um.Command != Command.umConnect) 
                    { 
                        throw new Exception("ScannerWantNotConnect"); 
                    } 
                    var cc = new UmConnectConfirmation(um.Data); 
                    if (cc.Answer != 1) 
                    { 
                        throw new Exception("ScannerWantNotConnect"); 
                    } 
                    _status = cc.Status; 
                    tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Unspecified); 
                    tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1); 
                    ep = new IPEndPoint(System.Net.IPAddress.Any, _currentConfiguration.TcpPport); 
                    tcpSocket.Bind(ep); 
                    tcpSocket.Listen(1); 
                    ScannerSocket.SetBuffers(tcpSocket); 
                    _scannerSocket = new ScannerSocket(tcpSocket.Accept()); 
                    udpSocketSend.Close(); 
                    udpSocketReceive.Close(); 
                    tcpSocket.Close(); 
                    _workThread = new Thread(Work); 
                    _workThread.Start(); 
                    _sendEventsThread = new Thread(SendEvents); 
                    _sendEventsThread.Start(); 
                    um = SendAndWaitAnswer(Command.umGetVersion); 
                    DriverVersion = (new Versions(um.Data)).Driver; 
                    switch (_currentConfiguration.ScannerVersion) 
                    { 
                        case ScannerVersion.V2003: 
                            _sh = new SIB2003.SharedMemory(); 
                            break; 
                        case ScannerVersion.V2005: 
                            _sh = new SIB2005.SharedMemory(); 
                            break; 
                        case ScannerVersion.V2009: 
                            _sh = new SIB2009.SharedMemory(); 
                            break; 
                        case ScannerVersion.V2010: 
                            _sh = new SIB2010.SharedMemory(); 
                            break; 
                    } 
                    _x = new short[NUMBER_OF_SIDES]; 
                    _x[0] = _x[1] = 0; 
                    _y = new short[NUMBER_OF_SIDES]; 
                    _y[0] = _y[1] = 0; 
                    ScanningEnabled = false; 
                    ReloadProperties(); 
                    ReloadManufProps(); 
                    ReloadWhiteCoeffs(); 
                    logger.LogInfo(Message.ScannerManagerDetectedHardware, 
                                   _currentConfiguration.ScannerVersion, _currentConfiguration.MaxLines, _currentConfiguration.DotsOneLine, 
                                   _currentConfiguration.DotsOneSide, _currentConfiguration.SizeofBinaryBuffer, _currentConfiguration.SizeofHalftoneBuffer); 
                    break; 
                } 
                finally 
                { 
                    if (udpSocketSend != null) 
                        udpSocketSend.Close(); 
                    if (udpSocketReceive != null) 
                        udpSocketReceive.Close(); 
                    if (tcpSocket != null) 
                        tcpSocket.Close(); 
                } 
            } 
            if (_scannerSocket == null) 
            { 
                throw new Exception("не дождались соединения со сканером"); 
            } 
        } 
        public void ReloadWhiteCoeffs() 
        { 
            var um = SendAndWaitAnswer(Command.umGetWhiteCoef); 
            _whiteCoeff = new byte[_currentConfiguration.DotsOneLine]; 
            if ((_currentConfiguration.Options & HardwareOptions.OneSide) > 0) 
            { 
                Array.Copy(um.Data, 0, _whiteCoeff, 0, _currentConfiguration.DotsOneSide); 
                Array.Copy(um.Data, 0, _whiteCoeff, _currentConfiguration.DotsOneSide, _currentConfiguration.DotsOneSide); 
            } 
            else 
            { 
                Array.Copy(um.Data, _whiteCoeff, _currentConfiguration.DotsOneLine); 
            } 
            _whiteCoeffU.Alloc(_currentConfiguration.DotsOneLine); 
            Marshal.Copy(_whiteCoeff, 0, _whiteCoeffU.ToPointer(), _currentConfiguration.DotsOneLine); 
        } 
        public void ReloadProperties() 
        { 
            var um = SendAndWaitAnswer(Command.umGetProps); 
            _scannerProps = new ScannerProps(um.Data); 
        } 
        public void ReloadManufProps() 
        { 
            var um = SendAndWaitAnswer(Command.umGetManufProps); 
            switch (_currentConfiguration.ScannerVersion) 
            { 
                case ScannerVersion.V2003: 
                case ScannerVersion.V2005: 
                case ScannerVersion.V2009: 
                    _manufProps = new SIB2003.ManufProps(um.Data); 
                    break; 
                case ScannerVersion.V2010: 
                    _manufProps = new SIB2010.ManufProps(um.Data); 
                    break; 
            } 
        } 
        public int Status 
        { 
            get 
            { 
                return _status; 
            } 
        } 
        public DropResult Drop(BlankMarking marking) 
        { 
            _sheetScanning = false; 
            var data = new[] { (byte)((byte)marking & 0xFF), (byte)((byte)marking >> 8) }; 
            var um = SendAndWaitAnswer(Command.umDrop, data, Command.umSheetDroped, TIMEOUT * 5, true); 
            if (um == null) 
            { 
                _eventHandler.SheetDroped(this, marking, DropResult.Timeout); 
                return DropResult.Timeout; 
            } 
            var dropResult = DropResult.Dropped; 
            if ((_currentConfiguration.Options & HardwareOptions.EnhancedDrop) > 0) 
            { 
                var result = (short)(um.Data[0] | um.Data[1] << 8); 
                if (result == 1) 
                { 
                    dropResult = DropResult.Reversed; 
                } 
            } 
            _eventHandler.SheetDroped(this, marking, dropResult); 
            return dropResult; 
        } 
        [DllImport("xib.dll")] 
        static unsafe extern void applyWhiteCoeffs(byte* source, byte* destination, byte* whiteCoeffs, int width, int height); 
        [DllImport("xib.dll")] 
        static unsafe extern void applyWhiteCoeffsDirectly(byte* side1, byte* side2, byte* whiteCoeffs, int width, int height); 
        public unsafe void GetHalftoneBuffer( 
            ScannedSide side, short x, short y, short w, short h, MemoryBlock iMemory, out short id) 
        { 
            if (side == ScannedSide.Undefined) 
                throw new ArgumentException("Сторона не определена"); 
            var sideIndex = (int)side; 
            var ptr = _sh.HalftoneBuffer[sideIndex]; 
            if (side == ScannedSide.Top) 
            { 
                ptr = HalfToneBuffer0.ToPointer(); 
            } 
            else if (side == ScannedSide.Bottom) 
            { 
                ptr = HalfToneBuffer1.ToPointer(); 
            } 
            var sourceBuffer = (byte*)ptr.ToPointer(); 
            x -= _x[sideIndex]; 
            y -= _y[sideIndex]; 
            if (x < 0) 
            { 
                throw new Exception("недопустимое значение левой границы"); 
            } 
            if (y < 0) 
            { 
                throw new Exception("недопустимое значение верхней границы"); 
            } 
            if (h < 0) 
            { 
                throw new Exception("отрицательная высота"); 
            } 
            if (w < 0) 
            { 
                throw new Exception("отрицательная ширина"); 
            } 
            if (x + w > _currentConfiguration.DotsOneSide || y + h > _currentConfiguration.MaxLines) 
            { 
                throw new Exception("запрашиваемое изображение выходит за границы отсканированной области"); 
            } 
            if (x % DOTS_PER_BYTE_HALFTONE != 0) 
            { 
                throw new Exception("начальная координата не попадает на границу байта!"); 
            } 
            if (w % DOTS_PER_BYTE_HALFTONE != 0) 
            { 
                throw new Exception("ширина запрошенной области содерит не целое число байт!"); 
            } 
            ptr = iMemory.ToPointer(); 
            if (ptr == IntPtr.Zero) 
            { 
                throw new Exception("нет буфера для записи изображения!"); 
            } 
            var destination = (byte*)ptr.ToPointer(); 
            var lengthLine = _currentConfiguration.DotsOneSide / DOTS_PER_BYTE_HALFTONE; 
            var lengthCopy = w / DOTS_PER_BYTE_HALFTONE; 
            var shift = (side == 0 ? 0 : _currentConfiguration.DotsOneSide); 
            if (x == y && y == 0 && lengthCopy == lengthLine) 
            { 
                if (!WhiteCoeffApplyed[sideIndex]) 
                { 
                    var wcPtr = _whiteCoeffU.ToPointer(); 
                    if ((_currentConfiguration.Options & HardwareOptions.RightToLeftHalftone) > 0) 
                    { 
                        if (!_halftoneFlipped[sideIndex]) 
                        { 
                            FlipBufferVertically(sourceBuffer, lengthCopy, h); 
                            _halftoneFlipped[sideIndex] = true; 
                        } 
                        applyWhiteCoeffs(sourceBuffer, destination, (byte*)wcPtr.ToPointer() + shift, lengthCopy, h); 
                    } 
                    else 
                    { 
                        applyWhiteCoeffs(sourceBuffer, destination, (byte*)wcPtr.ToPointer() + shift, lengthCopy, h); 
                    } 
                } 
                else 
                { 
                } 
            } 
            else 
            { 
                int wcX = x; 
                var wcInc = 1; 
                if (!_halftoneFlipped[sideIndex] && 
                    (_currentConfiguration.Options & HardwareOptions.RightToLeftHalftone) > 0) 
                { 
                    wcX = x + w - 1; 
                    wcInc = -1; 
                    x = (short)(_currentConfiguration.DotsOneSide - x - w); 
                } 
                var startLine = sourceBuffer + // начало буфера 
                                y * lengthLine + // число байт в первых y-строках 
                                x / BaseSharedMemory.DotsPerByteHalftone; // число байт в первых x-точках 
                var p = destination; 
                for (var i = 0; i < h; i++) 
                { 
                    var source = startLine; 
                    for (int j = 0, wcJ = wcX; j < lengthCopy; j++, wcJ += wcInc) 
                    { 
                        *p++ = whiteCoeffTable.whiteCoeff[(*source++ << 8) + _whiteCoeff[shift + wcJ]]; 
                    } 
                    startLine += lengthLine; 
                } 
                if (!_halftoneFlipped[sideIndex] && 
                    (_currentConfiguration.Options & HardwareOptions.RightToLeftHalftone) > 0) 
                { 
                    FlipBufferVertically(destination, w, h); 
                } 
            } 
            id = 0; 
        } 
        private const int INDICATOR_LENGTH = 16; 
        public int IndicatorLength 
        { 
            get 
            { 
                return INDICATOR_LENGTH; 
            } 
        } 
        public void SetIndicator(string str) 
        { 
            str = str.Replace('№', 'N'); 
            _scannerSocket.SendCommand(Command.umIndicatorS, Encode(str)); 
        } 
        private static readonly Encoding s_enc1251 = Encoding.GetEncoding(1251); 
        private static byte[] Encode(string str) 
        { 
            return s_enc1251.GetBytes(str); 
        } 
        public int MotorCount 
        { 
            get 
            { 
                return (_currentConfiguration.Options & HardwareOptions.OneSide) > 0 ? 1 : 2; 
            } 
        } 
        public void Motor(short number, bool enable, int dir, int step) 
        { 
            var onoff = (short)(enable ? 1 : 0); 
            if ((_currentConfiguration.Options & HardwareOptions.OneSide) > 0) 
            { 
                byte[] data = { 0, 0, 0, 0 }; 
                if (onoff == 1) 
                { 
                    switch (dir) 
                    { 
                        case 0: 
                            data = new byte[] { 1, 0, 0, 0 }; 
                            break; 
                        case 1: 
                            data = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }; 
                            break; 
                    } 
                } 
                _scannerSocket.SendCommand(Command.umMotor, data); 
            } 
            else 
            { 
                MotorData md; 
                if (number == 1) 
                { 
                    md = new MotorData(number, onoff, (short)dir, (short)step); 
                    _scannerSocket.SendCommand(Command.umMotor, md.Data); 
                } 
                else if (number == 2) 
                { 
                    md = new MotorData(number, onoff, (short)(1 - dir), (short)step); 
                    _scannerSocket.SendCommand(Command.umMotor, md.Data); 
                } 
                else if (number == 3) 
                { 
                    md = new MotorData(1, onoff, (short)dir, (short)step); 
                    _scannerSocket.SendCommand(Command.umMotor, md.Data); 
                    md = new MotorData(2, onoff, (short)(1 - dir), (short)step); 
                    _scannerSocket.SendCommand(Command.umMotor, md.Data); 
                } 
            } 
        } 
        public int PageOffset_AddItem(int width, int maxOffset) 
        { 
            int i; 
            for (i = 0; i < _validPageOffsets.Length; i++) 
            { 
                if (_validPageOffsets[i] == null) 
                { 
                    _validPageOffsets[i] = new ValidPageOffset(width, maxOffset); 
                    break; 
                } 
            } 
            SetCurrentPageOffsetTable(); 
            return i; 
        } 
        private void SetCurrentPageOffsetTable() 
        { 
            var ms = new MemoryStream(); 
            var bw = new BinaryWriter(ms); 
            var els = _validPageOffsets.Count(t => t != null); 
            bw.Write(els); 
            foreach (var t in _validPageOffsets) 
            { 
                if (t == null) 
                    continue; 
                bw.Write((int)(t.Width * _scannerProps.DpiX1 / sm2inch)); 
                bw.Write((int)(t.MaxLength * _scannerProps.DpiY1 / sm2inch)); 
            } 
            bw.Flush(); 
            var data = ms.ToArray(); 
            bw.Close(); 
            _scannerSocket.SendCommand(Command.umSetPageOffset, data); 
        } 
        public void PageOffset_ClearAll() 
        { 
            for (int i = 0; i < _validPageOffsets.Length; i++) 
            { 
                _validPageOffsets[i] = null; 
            } 
            SetCurrentPageOffsetTable(); 
        } 
        public void PageOffset_ClearItem(int itemId) 
        { 
            _validPageOffsets[itemId] = null; 
            SetCurrentPageOffsetTable(); 
        } 
        public ReverseCommandResult Reverse() 
        { 
            UserMessage um = SendAndWaitAnswer(Command.umRevers, true); 
            if (um == null || um.Data.Length == 0 || um.Data[0] != 1) 
            { 
                _sheetScanning = false; 
                _scannerBusy = false; 
                return ReverseCommandResult.Impossible; 
            } 
            _sheetScanning = false; 
            _scannerBusy = false; 
            return ReverseCommandResult.Accepted; 
        } 
        public void SetEventsHandler(IScannerEventHandler pEvent) 
        { 
            _eventHandler = pEvent; 
        } 
        public void SetWorkZone(ScannedSide side, short x, short y) 
        { 
            if (side == ScannedSide.Undefined) 
                throw new ArgumentException("Сторона не определена"); 
            var sideIndex = (int)side; 
            _x[sideIndex] = x; 
            _y[sideIndex] = y; 
        } 
        public void TestMarker(short sheetIssue) 
        { 
            var data = new byte[4]; 
            data[0] = (byte)(sheetIssue & 0xFF); 
            data[1] = (byte)(sheetIssue >> 8); 
            _scannerSocket.SendCommand(Command.umTestMarker, data); 
        } 
        private void SetEmptyFormatsTable() 
        { 
            _scannerSocket.SendCommand(Command.umSetFormats, new byte[] { 0, 0, 0, 0 }); 
        } 
        private void SetCurrentFormatsTable() 
        { 
            var ms = new MemoryStream(); 
            var bw = new BinaryWriter(ms); 
            var els = _validPageLengths.Count(t => t != null); 
            bw.Write(els); 
            foreach (var t in _validPageLengths) 
            { 
                if (t == null) continue; 
                bw.Write((int)(t.Width * _scannerProps.DpiX1 / sm2inch)); 
                bw.Write((int)(t.MinLength * _scannerProps.DpiY1 / sm2inch)); 
                bw.Write((int)(t.MaxLength * _scannerProps.DpiY1 / sm2inch)); 
            } 
            bw.Flush(); 
            var data = ms.ToArray(); 
            bw.Close(); 
            _scannerSocket.SendCommand(Command.umSetFormats, data); 
        } 
        public int ValidLength_AddItem(int width, int minLength, int maxLength) 
        { 
            int i; 
            for (i = 0; i < _validPageLengths.Length; i++) 
            { 
                if (_validPageLengths[i] == null) 
                { 
                    _validPageLengths[i] = new ValidPageLength(width, minLength, maxLength); 
                    break; 
                } 
            } 
            SetCurrentFormatsTable(); 
            return i; 
        } 
        public void ValidLength_ClearAll() 
        { 
            for (var i = 0; i < _validPageLengths.Length; i++) 
            { 
                _validPageLengths[i] = null; 
            } 
            SetCurrentFormatsTable(); 
        } 
        public void ValidLength_ClearItem(int itemId) 
        { 
            _validPageLengths[itemId] = null; 
            SetCurrentFormatsTable(); 
        } 
        private void SetProperties(PropName propName, short val) 
        { 
            var data = new byte[4]; 
            data[0] = (byte)((short)propName & 0xFF); 
            data[1] = (byte)((short)propName >> 8); 
            data[2] = (byte)(val & 0xFF); 
            data[3] = (byte)(val >> 8); 
            _scannerSocket.SendCommand(Command.umSetProps, data); 
        } 
        public short BinaryThresholdTop 
        { 
            get 
            { 
                return _scannerProps.BinaryThreshold0; 
            } 
            set 
            { 
                _scannerProps.BinaryThreshold0 = value; 
                SetProperties(PropName.BinaryThreshold0, value); 
            } 
        } 
        public short BinaryThresholdBottom 
        { 
            get 
            { 
                return _scannerProps.BinaryThreshold1; 
            } 
            set 
            { 
                _scannerProps.BinaryThreshold1 = value; 
                SetProperties(PropName.BinaryThreshold1, value); 
            } 
        } 
        public short CurrentBinaryThresholdTop 
        { 
            get 
            { 
                return (short)_treshold0; 
            } 
        } 
        public short CurrentBinaryThresholdBottom 
        { 
            get 
            { 
                return (short)_treshold1; 
            } 
        } 
        public short DpiXTop 
        { 
            get 
            { 
                return _scannerProps.DpiX0; 
            } 
            set 
            { 
                _scannerProps.DpiX0 = value; 
                SetProperties(PropName.DpiX0, value); 
            } 
        } 
        public short DpiXBottom 
        { 
            get 
            { 
                return _scannerProps.DpiX1; 
            } 
            set 
            { 
                _scannerProps.DpiX1 = value; 
                SetProperties(PropName.DpiX1, value); 
            } 
        } 
        public short DpiYTop 
        { 
            get 
            { 
                return _scannerProps.DpiY0; 
            } 
            set 
            { 
                _scannerProps.DpiY0 = value; 
                SetProperties(PropName.DpiY0, value); 
            } 
        } 
        public short DpiYBottom 
        { 
            get 
            { 
                return _scannerProps.DpiY1; 
            } 
            set 
            { 
                _scannerProps.DpiY1 = value; 
                SetProperties(PropName.DpiY1, value); 
            } 
        } 
        public const string ETHERNET_CONTROLLER = " eth0"; 
        private const double sm2inch = 25.4; 
        public void FillIpAddresses() 
        { 
            lock (this) 
            { 
                ProcessHelper.StartProcessAndWaitForFinished(IP_ADDRESS_DETECTOR, ETHERNET_CONTROLLER, 
                    delegate(ProcessOutputProcessorState state) 
                    { 
                        int i = state.Line.IndexOf(IP_ADDRESS_SIGNATURE); 
                        if (i != -1) 
                        { 
                            i += IP_ADDRESS_SIGNATURE.Length; 
                            int j = state.Line.IndexOf(' ', i); 
                            if (j != -1) 
                            { 
                                _ipAddress = System.Net.IPAddress.Parse(state.Line.Substring(i, j - i)); 
                            } 
                        } 
                        i = state.Line.IndexOf(IP_BROAD_CAST_SIGNATURE); 
                        if (i != -1) 
                        { 
                            i += IP_BROAD_CAST_SIGNATURE.Length; 
                            int j = state.Line.IndexOf(' ', i); 
                            if (j != -1) 
                            { 
                                _ipBroadCastAddress = System.Net.IPAddress.Parse(state.Line.Substring(i, j - i)); 
                            } 
                        } 
                        return false; 
                    }, null 
                ); 
            } 
        } 
        public IPAddress BroadcastIPAdress 
        { 
            get 
            { 
                if (_ipBroadCastAddress == null) 
                { 
                    FillIpAddresses(); 
                } 
                return _ipBroadCastAddress; 
            } 
        } 
        public IPAddress OwnIPAdress 
        { 
            get 
            { 
                if (_ipAddress == null) 
                { 
                    FillIpAddresses(); 
                } 
                return _ipAddress; 
            } 
        } 
        public string IPAddress 
        { 
            get 
            { 
                return OwnIPAdress.ToString(); 
            } 
        } 
        public void EnableLamps(bool enable) 
        { 
            _scannerProps.Lamps = enable 
                                      ? Lamps.BothOn 
                                      : Lamps.BothOff; 
            SetProperties(PropName.Lamps, (short) _scannerProps.Lamps); 
        } 
        public bool Marker 
        { 
            get 
            { 
                return _scannerProps.Marker == GsScannerDriver.Marker.On; 
            } 
            set 
            { 
                _scannerProps.Marker = value 
                    ? GsScannerDriver.Marker.On 
                    : GsScannerDriver.Marker.Off; 
                SetProperties(PropName.Marker, (short)_scannerProps.Marker); 
            } 
        } 
        public bool MarkerWork 
        { 
            get 
            { 
                return _scannerProps.MarkerWork == 1; 
            } 
            set 
            { 
                _scannerProps.MarkerWork = (short)(value ? 1 : 0); 
                SetProperties(PropName.MarkerWork, _scannerProps.MarkerWork); 
            } 
        } 
        public short MaxSheetLength 
        { 
            get 
            { 
                return _scannerProps.MaxSheetLength; 
            } 
            set 
            { 
                _scannerProps.MaxSheetLength = value; 
                SetProperties(PropName.MaxSheetLength, value); 
            } 
        } 
        public short MinSheetLength 
        { 
            get 
            { 
                return _scannerProps.MinSheetLength; 
            } 
            set 
            { 
                _scannerProps.MinSheetLength = value; 
                SetProperties(PropName.MinSheetLength, value); 
            } 
        } 
        public string SerialNumber 
        { 
            get 
            { 
                return _manufProps.SerialNumber.ToString(); 
            } 
        } 
        public bool SheetScanning 
        { 
            get 
            { 
                return _sheetScanning; 
            } 
        } 
        public bool TuningEnabled 
        { 
            get 
            { 
                return _scannerProps.TuningMode == TuningMode.On; 
            } 
            set 
            { 
                _scannerProps.TuningMode = (value ? TuningMode.On : TuningMode.Off); 
                SetProperties(PropName.TuningMode, (short)_scannerProps.TuningMode); 
            } 
        } 
        public bool LengthValidationEnabled 
        { 
            get 
            { 
                return _validLength; 
            } 
            set 
            { 
                _validLength = value; 
                if (_validLength) 
                { 
                    SetCurrentFormatsTable(); 
                } 
                else 
                { 
                    SetEmptyFormatsTable(); 
                } 
            } 
        } 
        public int WhiteCoeff 
        { 
            get 
            { 
                return (int)_scannerProps.WhiteCoeff; 
            } 
            set 
            { 
                _scannerProps.WhiteCoeff = (WhiteCoeff)value; 
                SetProperties(PropName.WhiteCoeff, (short)value); 
            } 
        } 
        private readonly bool[] _whiteCoeffApplyed = new bool[2]; 
        private bool[] WhiteCoeffApplyed 
        { 
            get { return _whiteCoeffApplyed; } 
        } 
        public MemoryBlock WorkBufferTop 
        { 
            get 
            { 
                var b = new IntPtr( 
                    _sh.BinBuffer[0].ToInt32() +  
                    (_y[0] + _manufProps.ShiftLines) * _currentConfiguration.DotsOneSide / DOTS_PER_BYTE_BINARY); 
                return new MemoryBlock(b); 
            } 
        } 
        public MemoryBlock WorkBufferBottom 
        { 
            get 
            { 
                var b = new IntPtr( 
                    _sh.BinBuffer[1].ToInt32() + _y[1] * _currentConfiguration.DotsOneSide / DOTS_PER_BYTE_BINARY); 
                return new MemoryBlock(b); 
            } 
        } 
        public MemoryBlock HalfToneBuffer0 
        { 
            get 
            { 
                var b = new IntPtr( 
                    _sh.HalftoneBuffer[0].ToInt32() +  
                    (_y[0] + _manufProps.ShiftLines) * _currentConfiguration.DotsOneSide / DOTS_PER_BYTE_HALFTONE); 
                return new MemoryBlock(b); 
            } 
        } 
        public MemoryBlock HalfToneBuffer1 
        { 
            get 
            { 
                var b = new IntPtr( 
                    _sh.HalftoneBuffer[1].ToInt32() +  
                    _y[1] * _currentConfiguration.DotsOneSide / DOTS_PER_BYTE_HALFTONE); 
                return new MemoryBlock(b); 
            } 
        } 
        public bool ScanningEnabled 
        { 
            get 
            { 
                return _scannerProps.WorkMode == WorkMode.Work; 
            } 
            set 
            { 
                _scannerProps.WorkMode = value ? WorkMode.Work : WorkMode.Debug; 
                SetProperties(PropName.WorkMode, (short)_scannerProps.WorkMode); 
            } 
        } 
        public int WorkZoneH 
        { 
            get 
            { 
                return _currentConfiguration.MaxLines; 
            } 
        } 
        public int WorkZoneW 
        { 
            get 
            { 
                return _currentConfiguration.DotsOneSide; 
            } 
        } 
        public void ScanningIndicatorMessage(string str) 
        { 
            _scannerSocket.SendCommand(Command.umScanIndicator, Encode(str)); 
        } 
        public bool DirtDetectionEnabled 
        { 
            get 
            { 
                return _scannerProps.DirtDetection == DirtDetection.On; 
            } 
            set 
            { 
                _scannerProps.DirtDetection = (value ? DirtDetection.On : DirtDetection.Off); 
                SetProperties(PropName.DirtDetection, (short)_scannerProps.DirtDetection); 
            } 
        } 
        public long GetBufferSize(ImageType imageType, BufferSize bufferSize) 
        { 
            return _currentConfiguration.DotsOneSide * 
                   (bufferSize == BufferSize.Scanned ? _linesCount : _currentConfiguration.MaxLines) / 
                   (imageType == ImageType.Binary ? DOTS_PER_BYTE_BINARY : DOTS_PER_BYTE_HALFTONE); 
        } 
        public unsafe bool SaveBuffer(string fileName, ImageType imageType, ScannedSide side, BufferSize bufferSize) 
        { 
            try 
            { 
                if (/*WhiteCoeff > 0 && */imageType == ImageType.Halftone) 
                { 
                    IntPtr wcPtr = _whiteCoeffU.ToPointer(); 
                    int shift = (side == 0 ? 0 : _currentConfiguration.DotsOneSide); 
                    if ((_currentConfiguration.Options & HardwareOptions.RightToLeftHalftone) > 0) 
                    { 
                        if ((_currentConfiguration.Options & HardwareOptions.OneSide) == 0) 
                        { 
                            if ((side == ScannedSide.Top || side == ScannedSide.Undefined) && 
                                !_halftoneFlipped[(int)ScannedSide.Top]) 
                            { 
                                IntPtr p0 = HalfToneBuffer0.ToPointer(); 
                                FlipBufferVertically((byte*)p0.ToPointer(), _currentConfiguration.DotsOneSide, _linesCount); 
                                _halftoneFlipped[(int)ScannedSide.Top] = true; 
                            } 
                        } 
                        if ((side == ScannedSide.Bottom || side == ScannedSide.Undefined) && 
                            !_halftoneFlipped[(int)ScannedSide.Bottom]) 
                        { 
                            IntPtr p1 = HalfToneBuffer1.ToPointer(); 
                            FlipBufferVertically((byte*)p1.ToPointer(), _currentConfiguration.DotsOneSide, _linesCount); 
                            _halftoneFlipped[(int)ScannedSide.Bottom] = true; 
                        } 
                    } 
                    if ((_currentConfiguration.Options & HardwareOptions.OneSide) == 0) 
                    { 
                        if ((side == ScannedSide.Top || side == ScannedSide.Undefined) && 
                            !WhiteCoeffApplyed[(int)ScannedSide.Top]) 
                        { 
                            IntPtr p0 = HalfToneBuffer0.ToPointer(); 
                            applyWhiteCoeffs( 
                                (byte*)p0.ToPointer(), (byte*)p0.ToPointer(),  
                                (byte*)wcPtr.ToPointer() + shift,  
                                _currentConfiguration.DotsOneSide, _linesCount); 
                            WhiteCoeffApplyed[(int)ScannedSide.Top] = true; 
                        } 
                    } 
                    if ((side == ScannedSide.Bottom || side == ScannedSide.Undefined) && 
                        !WhiteCoeffApplyed[(int)ScannedSide.Bottom]) 
                    { 
                        IntPtr p1 = HalfToneBuffer1.ToPointer(); 
                        applyWhiteCoeffs( 
                            (byte*)p1.ToPointer(), (byte*)p1.ToPointer(),  
                            (byte*)wcPtr.ToPointer() + shift,  
                            _currentConfiguration.DotsOneSide, _linesCount); 
                        WhiteCoeffApplyed[(int)ScannedSide.Bottom] = true; 
                    } 
                } 
                if (side == ScannedSide.Top || side == ScannedSide.Undefined) 
                { 
                    TiffImageHelper.SaveToFile(fileName + "_0.tif", imageType, 
                        imageType == ImageType.Binary ? WorkBufferTop : HalfToneBuffer0, 
                        _currentConfiguration.DotsOneSide, 
                        bufferSize == BufferSize.Scanned ? _linesCount : _currentConfiguration.MaxLines); 
                } 
                if (side == ScannedSide.Bottom || side == ScannedSide.Undefined) 
                { 
                    TiffImageHelper.SaveToFile(fileName + "_1.tif", imageType, 
                        imageType == ImageType.Binary ? WorkBufferBottom : HalfToneBuffer1, 
                        _currentConfiguration.DotsOneSide, 
                        bufferSize == BufferSize.Scanned ? _linesCount : _currentConfiguration.MaxLines); 
                } 
                return true; 
            } 
            catch 
            { 
                return false; 
            } 
        } 
        private readonly bool[] _halftoneFlipped = new [] { false, false }; 
        private static unsafe void FlipBufferVertically(byte* buffer, int width, int height) 
        { 
            var half = width / 2; 
            var offset = 0; 
            for (var i = 0; i < height; i++) 
            { 
                for (int srcIdx = offset, dstIdx = offset + width - 1; 
                    srcIdx < offset + half; 
                    srcIdx++, dstIdx--) 
                { 
                    var a = buffer[srcIdx]; 
                    buffer[srcIdx] = buffer[dstIdx]; 
                    buffer[dstIdx] = a; 
                } 
                offset += width; 
            } 
        } 
        public bool Green 
        { 
            get 
            { 
                return (_scannerProps.Lamps & Lamps.GreenOn) != 0; 
            } 
            set 
            { 
                Lamps lamps; 
                var propsValue = _scannerProps.Lamps; 
                if (value) 
                { 
                    lamps = Lamps.GreenOn; 
                    propsValue |= Lamps.GreenOn; 
                } 
                else 
                { 
                    lamps = Lamps.GreenOff; 
                    propsValue &= ~Lamps.GreenOn; 
                } 
                _scannerProps.Lamps = propsValue; 
                SetProperties(PropName.Lamps, (short)lamps); 
            } 
        } 
        public bool Red 
        { 
            get 
            { 
                return (_scannerProps.Lamps & Lamps.RedOn) != 0; 
            } 
            set 
            { 
                Lamps lamps; 
                var propsValue = _scannerProps.Lamps; 
                if (value) 
                { 
                    lamps = Lamps.RedOn; 
                    propsValue |= Lamps.RedOn; 
                } 
                else 
                { 
                    lamps = Lamps.RedOff; 
                    propsValue &= ~Lamps.RedOn; 
                } 
                _scannerProps.Lamps = propsValue; 
                SetProperties(PropName.Lamps, (short)lamps); 
            } 
        } 
        public void RestoreNormalState() 
        { 
            _scannerSocket.SendCommand(Command.umRestoreLamps); 
            _sheetScanning = false; 
            _scannerBusy = false; 
        } 
        public bool DoubleSheetSensorEnabled 
        { 
            get 
            { 
                return _scannerProps.DoubleSheet == DoubleSheet.On; 
            } 
            set 
            { 
                _scannerProps.DoubleSheet = (value ? DoubleSheet.On : DoubleSheet.Off); 
                SetProperties(PropName.DoubleSheet, (short)_scannerProps.DoubleSheet); 
            } 
        } 
        public short DoubleSheetSensorCurrentValue 
        { 
            get 
            { 
                return (short)_scannerProps.DoubleSheet; 
            } 
        } 
        public void GetDoubleSheetSensorLevel(out short l, out short r) 
        { 
            l = _scannerProps.DoubleSheetLevelL; 
            r = _scannerProps.DoubleSheetLevelR; 
        } 
        public void GetDoubleSheetSensorCurrentValue(out short l, out short r) 
        { 
            l = _scannerProps.DoubleSheetLevelL; 
            r = _scannerProps.DoubleSheetLevelR; 
        } 
        public void SetDoubleSheetSensorLevel(short l, short r) 
        { 
            _scannerProps.DoubleSheetLevelL = l; 
            SetProperties(PropName.DoubleSheetLevelL, l); 
            _scannerProps.DoubleSheetLevelR = r; 
            SetProperties(PropName.DoubleSheetLevelR, r); 
        } 
        public void SetDoubleSheetSensorCurrentSheetLevel(short nLeftSensorLevel, short nRightSensorLevel) 
        { 
            var data = new byte[4]; 
            data[0] = (byte)(nLeftSensorLevel & 0xFF); 
            data[1] = (byte)(nLeftSensorLevel >> 8); 
            data[2] = (byte)(nRightSensorLevel & 0xFF); 
            data[3] = (byte)(nRightSensorLevel >> 8); 
            _scannerSocket.SendCommand(Command.umSetDoubleSheetSensorCurrentLevel, data); 
        } 
        public void CheckDoubleSheetSensor(out bool leftWork, out bool rightWork) 
        { 
            var um = SendAndWaitAnswer(Command.umCheckDoubleSheetSensor); 
            var br = new BinaryReader(new MemoryStream(um.Data)); 
            leftWork = (br.ReadInt32() == 0); 
            rightWork = (br.ReadInt32() == 0); 
        } 
        public bool ScannerBusy 
        { 
            get 
            { 
                return _scannerBusy; 
            } 
        } 
        public int ExpectedLength 
        { 
            set 
            { 
                value = (int)(value * _scannerProps.DpiY1 / sm2inch); 
                var data = new byte[4]; 
                data[0] = (byte)(value & 0xFF); 
                data[1] = (byte)(value >> 8); 
                _scannerSocket.SendCommand(Command.umSetLength, data); 
            } 
        } 
        public void GetMarkerParameters( 
            out short on, 
            out short off, 
            out short markingTime, 
            out short rollbackTime, 
            out short downTime) 
        { 
            on = _manufProps.On; 
            off = _manufProps.Off; 
            markingTime = _manufProps.MarkingTime; 
            rollbackTime = _manufProps.RollbackTime; 
            downTime = _manufProps.DownTime; 
        } 
        public void SetMarkerParameters( 
            short on, 
            short off, 
            short markingTime, 
            short rollbackTime, 
            short downTime) 
        { 
            _manufProps.On = on; 
            _manufProps.Off = off; 
            _manufProps.MarkingTime = markingTime; 
            _manufProps.RollbackTime = rollbackTime; 
            _manufProps.DownTime = downTime; 
            var data = _manufProps.ToByteArray(); 
            _scannerSocket.SendCommand(Command.umSetManufProps, data); 
        } 
        private void SendEvents() 
        { 
            while (true) 
            { 
                try 
                { 
                    DoEvent(); 
                } 
                catch (ThreadAbortException) 
                { 
                    _logger.LogInfo(Message.ScannerManagerScannerSendEventsThreadAborted); 
                    return; 
                } 
                catch (Exception ex) 
                { 
                    _logger.LogError(Message.ScannerManagerScannerSendEventsThreadError, ex); 
                } 
            } 
        } 
        private void DoEvent() 
        { 
            _mre.WaitOne(TIMEOUT, false); 
            if (_events.Count != 0) 
            { 
                UserMessage um; 
                lock (s_eventsSync) 
                { 
                    um = (UserMessage)_events.Dequeue(); 
                } 
                switch (um.Command) 
                { 
                    case Command.umReadyToScanning: 
                        { 
                            _scannerBusy = false; 
                            _sheetScanning = false; 
                            if (_eventHandler != null) 
                            { 
                                _eventHandler.ReadyToScanning(this); 
                            } 
                            break; 
                        } 
                    case Command.umPageIn: 
                        { 
                            _linesCount = 0; 
                            _scannerBusy = true; 
                            _sheetScanning = true; 
                            WhiteCoeffApplyed[0] = false; 
                            WhiteCoeffApplyed[1] = false; 
                            _halftoneFlipped[0] = _halftoneFlipped[1] = false; 
                            if (_eventHandler != null) 
                            { 
                                _eventHandler.NewSheet(this); 
                            } 
                            break; 
                        } 
                    case Command.umError: 
                        { 
                            var err = (ScannerError)(um.Data[0] | um.Data[1] << 8); 
                            if (err == ScannerError.DirtOnZeroSide || err == ScannerError.DirtOnFirstSide || err == ScannerError.DoublePaperSensorFail) 
                            { 
                                _sheetScanning = false; 
                            } 
                            if (_eventHandler != null) 
                            { 
                                _eventHandler.Error(this, err); 
                            } 
                            break; 
                        } 
                    case Command.umScanBinStrip: 
                        { 
                            int stripSize = um.Data[0] | um.Data[1] << 8 | um.Data[2] << 16 | um.Data[3] << 24; 
                            _treshold0 = um.Data[4] | um.Data[5] << 8 | um.Data[6] << 16 | um.Data[7] << 24; 
                            _treshold1 = um.Data[8] | um.Data[9] << 8 | um.Data[10] << 16 | um.Data[11] << 24; 
                            _linesCount += stripSize; 
                            if (_eventHandler != null) 
                            { 
                                _eventHandler.NextBuffer(this, (short)_linesCount); 
                            } 
                            break; 
                        } 
                    case Command.umScanStop: 
                        { 
                            var sheetType = (SheetType)(um.Data[0] | um.Data[1] << 8 | um.Data[2] << 16 | um.Data[3] << 24); 
                            _linesCount = um.Data[4] | um.Data[5] << 8 | um.Data[6] << 16 | um.Data[7] << 24; 
                            if (_eventHandler != null) 
                            { 
                                _eventHandler.SheetIsReady(this, (short)_linesCount, sheetType); 
                            } 
                            break; 
                        } 
                    case Command.umVoltStat: 
                        { 
                            var min = (uint)(um.Data[0] | um.Data[1] << 8 | um.Data[2] << 16 | um.Data[3] << 24); 
                            var max = (uint)(um.Data[4] | um.Data[5] << 8 | um.Data[6] << 16 | um.Data[7] << 24); 
                            var avg = (uint)(um.Data[8] | um.Data[9] << 8 | um.Data[10] << 16 | um.Data[11] << 24); 
                            var powerstate = (uint)(um.Data[12] | um.Data[13] << 8 | um.Data[14] << 16 | um.Data[15] << 24); 
                            if (_eventHandler != null) 
                            { 
                                _eventHandler.PowerStatistics(this, powerstate != 0, min, max, avg); 
                            } 
                            break; 
                        } 
                } 
            } 
            lock (s_eventsSync) 
            { 
                if (_events.Count == 0) 
                { 
                    _mre.Reset(); 
                } 
                else 
                { 
                    _mre.Set(); 
                } 
            } 
        } 
        private void Work() 
        { 
            UserMessage um; 
            while (true) 
            { 
                try 
                { 
                    um = _scannerSocket.GetCommand(TIMEOUT); 
                    switch (um.Command) 
                    { 
                        case Command.umReadyToScanning: 
                        case Command.umPageIn: 
                        case Command.umError: 
                        case Command.umScanBinStrip: 
                        case Command.umScanStop: 
                        case Command.umVoltStat: 
                            { 
                                lock (s_eventsSync) 
                                { 
                                    _events.Enqueue(um); 
                                    _mre.Set(); 
                                } 
                                break; 
                            } 
                        case Command.umGetVersion: 
                        case Command.umGetProps: 
                        case Command.umGetManufProps: 
                        case Command.umRevers: 
                        case Command.umCheckDoubleSheetSensor: 
                        case Command.umGetWhiteCoef: 
                            { 
                                ReleaseWaiting(um); 
                                break; 
                            } 
                        case Command.umSheetDroped: 
                            { 
                                ReleaseWaiting(um); 
                                _scannerBusy = false; 
                                break; 
                            } 
                        default: 
                            break; 
                    } 
                } 
                catch (ThreadAbortException) 
                { 
                    _logger.LogInfo(Message.ScannerManagerScannerWorkThreadAborted); 
                    return; 
                } 
                catch (Exception ex) 
                { 
                    _logger.LogError(Message.ScannerManagerScannerWorkThreadError, ex); 
                } 
            } 
        } 
        private UserMessage SendAndWaitAnswer(Command command) 
        { 
            return SendAndWaitAnswer(command, null, command, TIMEOUT, false); 
        } 
        private UserMessage SendAndWaitAnswer(Command command, bool ignoreTimeout) 
        { 
            return SendAndWaitAnswer(command, null, command, TIMEOUT, ignoreTimeout); 
        } 
        private UserMessage SendAndWaitAnswer( 
            Command sendCommand, byte[] sendData, Command waitCommand, int timeout, bool ignoreTimeout) 
        { 
            UserMessage um = null; 
            ManualResetEvent mre = null; 
            lock (s_waitEventsSync) 
            { 
                if (!_waitEvents.Contains(waitCommand)) 
                { 
                    mre = new ManualResetEvent(false); 
                    _waitEvents.Add(waitCommand, mre); 
                } 
            } 
            _scannerSocket.SendCommand(sendCommand, sendData); 
            if (mre != null) 
            { 
                if (mre.WaitOne(timeout, false)) 
                { 
                    lock (s_recevedEventsSync) 
                    { 
                        um = (UserMessage)_recevedEvents[waitCommand]; 
                        _recevedEvents.Remove(waitCommand); 
                    } 
                } 
            } 
            if (um == null && !ignoreTimeout) 
            { 
                throw new Exception("Нет ответа " + waitCommand); 
            } 
            return um; 
        } 
        private void ReleaseWaiting(UserMessage um) 
        { 
            lock (s_waitEventsSync) 
            { 
                lock (s_recevedEventsSync) 
                { 
                    _recevedEvents[um.Command] = um; 
                } 
                if (_waitEvents.Contains(um.Command)) 
                { 
                    var mre = (ManualResetEvent)_waitEvents[um.Command]; 
                    _waitEvents.Remove(um.Command); 
                    mre.Set(); 
                } 
            } 
        } 
        public int DriverVersion 
        { 
            get; 
            private set; 
        } 
        public ScannerVersion Version 
        { 
            get 
            { 
                return _currentConfiguration.ScannerVersion; 
            } 
        } 
        #region Native types 
        class ValidPageLength 
        { 
            public readonly int Width; 
            public readonly int MinLength; 
            public readonly int MaxLength; 
            public ValidPageLength(int width, int minLength, int maxLength) 
            { 
                Width = width; 
                MinLength = minLength; 
                MaxLength = maxLength; 
            } 
        } 
        class ValidPageOffset 
        { 
            public readonly int Width; 
            public readonly int MaxLength; 
            public ValidPageOffset(int width, int maxLength) 
            { 
                Width = width; 
                MaxLength = maxLength; 
            } 
        } 
        struct UmConnectConfirmation 
        { 
            public readonly int Answer; 
            public readonly int Status; 
            public UmConnectConfirmation(byte[] data) 
            { 
                Answer = 
                    data[0] 
                    | data[1] << 8 
                    | data[2] << 16 
                    | data[3] << 24; 
                Status = 
                    data[4] 
                    | data[5] << 8 
                    | data[6] << 16 
                    | data[7] << 24; 
            } 
        } 
        struct Versions 
        { 
            public readonly int Driver; 
            public readonly int Manager; 
            public Versions(byte[] data) 
            { 
                Driver = 
                    data[0] 
                    | data[1] << 8 
                    | data[2] << 16 
                    | data[3] << 24; 
                Manager = 
                    data[4] 
                    | data[5] << 8 
                    | data[6] << 16 
                    | data[7] << 24; 
            } 
        } 
        [Flags] 
        private enum HardwareOptions 
        { 
            Default = 0, 
            OneSide = 0x1, 
            RightToLeftHalftone = 0x2, 
            EnhancedDrop = 0x4, 
        } 
        private class HardwareConfiguration 
        { 
            public readonly int UdpPortReceive; 
            public readonly int UdpPortSend; 
            public readonly int TcpPport; 
            public readonly ScannerVersion ScannerVersion; 
            public readonly int MaxLines; 
            public readonly int DotsOneLine; 
            public readonly int DotsOneSide; 
            public readonly int SizeofHalftoneBuffer; 
            public readonly int SizeofBinaryBuffer; 
            public readonly HardwareOptions Options = HardwareOptions.Default; 
            public HardwareConfiguration(int udpPortReceive, int udpPortSend, int tcpPport, int maxLines, int dotsOneLine, ScannerVersion scannerVersion, HardwareOptions
ptions) 
            { 
                UdpPortReceive = udpPortReceive; 
                UdpPortSend = udpPortSend; 
                TcpPport = tcpPport; 
                MaxLines = maxLines; 
                DotsOneLine = dotsOneLine; 
                DotsOneSide = DotsOneLine / 2; 
                SizeofHalftoneBuffer = DotsOneSide * maxLines / BaseSharedMemory.DotsPerByteHalftone; 
                SizeofBinaryBuffer = DotsOneSide * maxLines / BaseSharedMemory.DotsPerByteBinary; 
                ScannerVersion = scannerVersion; 
                Options = options; 
            } 
        } 
        #endregion 
    } 
}
