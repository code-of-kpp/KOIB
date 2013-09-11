using System; 
using System.Collections.Generic; 
using System.Text; 
using System.Runtime.InteropServices; 
using System.IO; 
using Croc.Bpc.Utils; 
using Croc.Core.Extensions; 
namespace Croc.Bpc.Recognizer.Ocr 
{ 
    public class Ocr : IOcr 
    { 
        public const int STAMP_DIGIT_COUNT = 4; 
        private static int s_runRecCount; 
        private int _stampVSize; 
        private bool _seekBottom; 
        private BufferHeader _side0, _side1; 
        private bool _isRunningNow; 
        private static readonly object s_runSync = new object(); 
        private int _endRecognitionLastResult = -1; 
        private float _dpiX0, _dpiX1, _dpiY0, _dpiY1; 
        private IOcrEventHandler _events; 
        private string _bstrPath2RecognitionData; 
        private const int STAMP_SIZE = 10; 
        private int[] _alStamp = new int[STAMP_SIZE]; 
        private StampTestLevel _stampTestLevel; 
        private List<PollResult> _pollResults; 
        private int _bulletinNumber = -1; // по умолчанию не определен 
        private bool _ignoreOneExtraCheck; 
        private bool _lookForLostSquare; 
        private int[] _lastLines = new int[2]; 
        private int _stampMaxLineWidth; 
        private int _stampMinLineWidth; 
        public Ocr() 
        { 
            string sCurrentDir = Directory.GetCurrentDirectory(); 
            Directory.SetCurrentDirectory(".."); 
            _events = null; 
            LookForLostSquare = false; 


            s_runRecCount = 0;    // инициализирую количество вызовов функции RunRecognize 


            Initialize(); 
            Directory.SetCurrentDirectory(sCurrentDir); 
        } 
        bool Initialize() 
        { 
            _pollResults = new List<PollResult>(); 
            _isRunningNow = false; 
            _endRecognitionLastResult = -1; 
            _ignoreOneExtraCheck = false; 
            _lastLines[0] = _lastLines[1] = 0; 
            _seekBottom = false; 
            _stampVSize = 380; 
            return true; 
        } 
        public void Dispose() 
        { 
            Dispose(true); 
            GC.SuppressFinalize(this); 
        } 
        private bool _disposed; 
        private void Dispose(bool disposing) 
        { 
            if(!_disposed) 
            { 
                if(disposing) 
                { 
                } 
                try  
                { 
                    Ocr.OCR_Initialize(null); 
                    _events = null; 
                    Ocr.CloseRecognition(); 
                } 
                catch  
                { 
                } 
            } 
            _disposed = true;          
        } 
        ~Ocr() 
        { 
            Dispose(false); 
        } 
        int    GetStamp(int index) 
        {  
            if(0 > index || STAMP_SIZE <= index)  
            { 
                SetError(ErrorCode.IllegalUse, "GetStamp: выход за пределы массива"); 
                return -1; 
            } 
            return _alStamp[index];  
        } 
        int    GetStampCount()     
        {  
            int i; 
            for(i = 0; i < STAMP_SIZE; i++)  
            { 
                if(!(0 < _alStamp[i])) 
                { 
                    break; 
                } 
            } 
            return i; 
        } 
        private OcrCallBack _myCallBack; 
        public void Init() 
        { 
            _myCallBack = new OcrCallBack(OcrCallBackHandler); 
            OCR_Initialize(_myCallBack); 
        } 
        public void SetEventsHandler(IOcrEventHandler eventHandler) 
        { 
            _events = eventHandler; 
        } 
        public StampTestLevel StampTestLevel  
        { 
            get 
            { 
                return _stampTestLevel; 
            } 
            set 
            { 
                _stampTestLevel = value; 
            } 
        } 
        public OnlineLevel OnlineRecognitionLevel  
        { 
            get 
            { 
                return (OnlineLevel)GetFeatureOnlineTest(); 
            } 
            set  
            { 
                SetFeatureOnlineTest((int)value); 
            } 
        } 
        public int MaxOnlineSkew  
        { 
            get  
            { 
                return GetMaxOnlineSkew(); 
            } 
            set 
            { 
                SetMaxOnlineSkew(value); 
            }  
        } 
        public float DpiX0  
        { 
            get  
            { 
                return _dpiX0; 
            } 
            set 
            { 
                _dpiX0 = value; 
            }  
        } 
        public float DpiX1  
        { 
            get  
            { 
                return _dpiX1; 
            } 
            set 
            { 
                _dpiX1 = value; 
            }  
        } 
        public float DpiY0  
        { 
            get  
            { 
                return _dpiY0; 
            } 
            set 
            { 
                _dpiY0 = value; 
            }  
        } 
        public float DpiY1  
        { 
            get  
            { 
                return _dpiY1; 
            } 
            set 
            { 
                _dpiY1 = value; 
            }  
        } 
        public void SetDpi(float x0, float y0, float x1, float y1) 
        { 
            _dpiX0 = x0; 
            _dpiY0 = y0; 
            _dpiX1 = x1; 
            _dpiY1 = y1; 
        } 
        public int BulletinNumber  
        { 
            get  
            { 
                return _bulletinNumber; 
            } 
        } 
        public StampResult StampResult  
        { 
            get  
            { 
                return (StampResult)IsStampOK(); 
            } 
        } 
        public List<PollResult> Results  
        { 
            get  
            { 
                return _pollResults; 
            } 
        } 
        public void OCR_ExcludeSquare(int bulletin, int election, int square) 
        { 
            try  
            { 
                if (ExcludeSquare(bulletin, election, square) < 0)  
                { 
                    SetError(ErrorCode.IllegalUse, "OCR_ExcludeSquare: Неверные параметры для Ocr.ExcludeSquare: " 
                        + bulletin + ", " + election + ", " + square); 
                    throw new OcrException("OCR_ExcludeSquare: Неверные параметры для Ocr.ExcludeSquare: " 
                        + bulletin + ", " + election + ", " + square); 
                } 
                if (LinkChangedModel() < 0)  
                { 
                    SetError(ErrorCode.LinkModelError, 
                        "OCR_ExcludeSquare: Не удалось перекомпилировать модель функцией Ocr.LinkChangedModel"); 
                    throw new OcrException("OCR_ExcludeSquare: Не удалось перекомпилировать модель функцией Ocr.LinkChangedModel"); 
                } 
            } 
            catch (Exception ex) 
            {  
                SetError(ErrorCode.UnexpectedError, "OCR_ExcludeSquare: " + ex); 
                throw; 
            } 
        } 
        public void OCR_RestoreSquare(int bulletin, int election, int square) 
        { 
            try  
            { 
                if (EnableSquare(bulletin, election, square) < 0)  
                { 
                    SetError(ErrorCode.IllegalUse, "OCR_RestoreSquare: Неверные параметры для Ocr.EnableSquare: " 
                        + bulletin + ", " + election + ", " + square); 
                    throw new OcrException("OCR_RestoreSquare: Неверные параметры для Ocr.EnableSquare: " 
                        + bulletin + ", " + election + ", " + square); 
                } 
                if (LinkChangedModel() < 0)  
                { 
                    SetError(ErrorCode.LinkModelError, "OCR_RestoreSquare: Не удалось перекомпилировать модель функцией Ocr.LinkChangedModel"); 
                    throw new OcrException("OCR_RestoreSquare: Не удалось перекомпилировать модель функцией Ocr.LinkChangedModel"); 
                } 
            } 
            catch (Exception ex) 
            {  
                SetError(ErrorCode.UnexpectedError, "OCR_RestoreSquare: " + ex); 
                throw; 
            } 
        } 
        public int OCR_IsSquareValid(int bulletin, int election, int square, out int retVal) 
        { 
            int ret; 
            try  
            { 
                ret = IsSquareValid(bulletin, election, square); 
                if (ret == -1)  
                { 
                    SetError(ErrorCode.IllegalUse, "OCR_IsSquareValid: Неверные параметры для Ocr.IsSquareValid: " 
                        + bulletin + ", " + election + ", " + square); 
                    throw new OcrException("OCR_IsSquareValid: Неверные параметры для Ocr.IsSquareValid: " 
                        + bulletin + ", " + election + ", " + square); 
                } 
                retVal = ret; 
            } 
            catch (Exception ex) 
            {  
                SetError(ErrorCode.UnexpectedError, "OCR_IsSquareValid: " + ex); 
                retVal = 0; 
                return 0; 
            } 
            return ret; 
        } 
        public void RunRecognize(MemoryBlock pdImage0, MemoryBlock pdImage1, int nLineWidth0, int nLineWidth1, int nNumOfLines0, int nNumOfLines1) 
        { 
            lock (s_runSync) 
            { 
                s_runRecCount++; // увеличиваю количество вызовов функции 
                if (pdImage0 == null || pdImage1 == null) 
                { 
                    SetError(ErrorCode.IllegalUse, "RunRecognize: переданы нулевые указатели на изображения"); 
                    throw new OcrException("RunRecognize: переданы нулевые указатели на изображения"); 
                } 
                IntPtr buffer0 = pdImage0.ToPointer(); 
                IntPtr buffer1 = pdImage1.ToPointer(); 
                if (buffer0 == IntPtr.Zero || buffer1 == IntPtr.Zero) 
                { 
                    SetError(ErrorCode.UnexpectedError, "RunRecognize: получены нулевые указатели на изображения"); 
                    throw new OcrException("RunRecognize: получены нулевые указатели на изображения"); 
                } 
                _side0.all_bin_buf = IntPtr.Zero; 
                _side0.bin_buf = buffer0; 
                _side0.bin_size = (uint) (nNumOfLines0*nLineWidth0); 
                _side0.ByteImageWidth = nLineWidth0; 
                _side0.ImageWidth = nLineWidth0*8; 
                _side0.ActualScanNumber = nNumOfLines0; 
                _side0.tone_buf = IntPtr.Zero; 
                _side0.bin_width = 0; 
                _side0.size = 0; 
                _side0.tone_size = 0; 
                _side0.g_p0.len = 0; 
                _side0.g_p0.start = 0; 
                _side0.g_p1.len = 0; 
                _side0.g_p1.start = 0; 
                _side0.g_p2.len = 0; 
                _side0.g_p2.start = 0; 
                _side0.flag = 0; 
                _side0.ScanCounter = 0; 
                _side0.OutFlag = 0; 
                _side0.LineCount = 0; 
                _side0.xl = 0; 
                _side0.yl = 0; 
                _side0.xr = 0; 
                _side0.yr = 0; 
                _side1.bin_size = (uint) (nNumOfLines1*nLineWidth1); 
                _side1.bin_buf = buffer1; 
                _side1.ImageWidth = nLineWidth1*8; 
                _side1.ByteImageWidth = nLineWidth1; 
                _side1.ActualScanNumber = nNumOfLines1; 
                _side1.tone_buf = IntPtr.Zero; 
                _side1.bin_width = 0; 
                _side1.size = 0; 
                _side1.tone_size = 0; 
                _side1.g_p0.len = 0; 
                _side1.g_p0.start = 0; 
                _side1.g_p1.len = 0; 
                _side1.g_p1.start = 0; 
                _side1.g_p2.len = 0; 
                _side1.g_p2.start = 0; 
                _side1.flag = 0; 
                _side1.ScanCounter = 0; 
                _side1.OutFlag = 0; 
                _side1.LineCount = 0; 
                _side1.xl = 0; 
                _side1.yl = 0; 
                _side1.xr = 0; 
                _side1.yr = 0; 
                _pollResults.Clear(); 
                try 
                { 
                    if (_isRunningNow) 
                    { 
                        EndRecognition(0, 0, 0); 
                    } 
                    else 
                    { 
                        _isRunningNow = true; 
                    } 
                    _bulletinNumber = -1; 
                    int res = StartRecognition(ref _side0, ref _side1); 
                    if (res < 0) 
                    { 
                        SetError(ErrorCode.StartRecognitionFailed, "Не удалось начать распознавание: " + res); 
                        throw new OcrException("Не удалось начать распознавание: " + res); 
                    } 
                    StartOnLineTesting(ref _side0, ref _side1, GetFrameDist_mm()); 
                } 
                catch (Exception ex) 
                { 
                    SetError(ErrorCode.UnexpectedError, "RunRecognize: " + ex); 
                    throw; 
                } 
            } 
        } 
        public int EndRecognize(MarkerType mtMarkerType) 
        { 
            lock (s_runSync) 
            { 
                try 
                { 
                    if (!_isRunningNow) 
                        return _endRecognitionLastResult; 
                    _isRunningNow = false; 
                    _endRecognitionLastResult = -1; 
                    _endRecognitionLastResult = EndRecognition(mtMarkerType, _lastLines[0], _lastLines[1]); 
                    return _endRecognitionLastResult; 
                } 
                catch (Exception ex) 
                { 
                    SetError(ErrorCode.UnexpectedError, "EndRecognize: " + ex); 
                    throw; 
                } 
            } 
        } 
        public int TestBallot(ref GeoData geoData) 
        { 
            try  
            { 
                _isRunningNow = false; 
                return TestBallot(_lastLines[0], _lastLines[1], ref geoData); 
            } 
            catch (Exception ex) 
            {  
                SetError(ErrorCode.UnexpectedError, string.Format("TestBallot: {0}", ex)); 
                throw; 
            } 
        } 
        public void LinkModel() 
        { 
            try  
            { 
                if (DefaultModelSave() < 0) 
                { 
                    SetError(ErrorCode.LinkModelError, "LinkModel: не удалось скомпилировать модель"); 
                    throw new OcrException("LinkModel: не удалось скомпилировать модель"); 
                } 
            } 
            catch (Exception ex) 
            {  
                SetError(ErrorCode.UnexpectedError, "LinkModel: " + ex); 
                throw; 
            } 
        } 
        public int NextBuffer(int count) 
        { 
            try  
            { 
                int res = NextBufferInternal(count); 
                _lastLines[0] = count; 
                _lastLines[1] = count; 


                if (res < 0) 
                { 
                } 
                return res; 
            } 
            catch (Exception ex) 
            {  
                SetError(ErrorCode.UnexpectedError, "NextBuffer: " + ex); 
                throw; 
            } 
        } 
        public int GetOnlineMarker(MarkerType mtMarkerType) 
        { 
            try  
            { 
                int res; 
                switch( mtMarkerType )  
                { 
                    case MarkerType.Standard: 
                        res = OnlineDefStandartMarker(); 
                        break; 
                    case MarkerType.Digital: 
                        res = OnlineDefCharMarker(); 
                        break; 
                    default: 
                        SetError(ErrorCode.IllegalUse, "GetOnlineMarker: неверный тип маркера: " + mtMarkerType); 
                        res = -1; 
                        break; 
                } 
                return res; 
            } 
            catch (Exception ex) 
            {  
                SetError(ErrorCode.UnexpectedError, "GetOnlineMarker: " + ex); 
                throw; 
            } 
        } 
        public void InitRecognize() 
        { 
            try  
            { 
                CloseRecognition(); 
                int    res = InitRecognition(); 
                if (res < 0) 
                { 
                    SetError(ErrorCode.StartRecognitionFailed, 
                        "InitRecognize: Ошибка при инициализации распознавания: " + res); 
                    throw new OcrException("Ошибка при инициализации распознавания: " + res); 
                } 
            } 
            catch (Exception ex) 
            {  
                SetError(ErrorCode.UnexpectedError, "InitRecognize: " + ex); 
                throw; 
            } 
        } 
        public void AddStamp(int stamp) 
        { 
            int index = GetStampCount(); 
            if(STAMP_SIZE > index)  
            { 
                _alStamp[index] = stamp; 
            } 
            else 
            { 
                SetError(ErrorCode.IllegalUse, "AddStamp: Превышен лимит. Не больше " + STAMP_SIZE + " номеров"); 
                throw new OcrException("AddStamp: Превышен лимит. Не больше " + STAMP_SIZE + " номеров"); 
            } 
        } 
        public void ClearStamps() 
        { 
            for(int i = 0; i < STAMP_SIZE; i++)  
            { 
                _alStamp[i] = 0; 
            } 
        } 
        public string ModelFilePath  
        { 
            get; 
            set; 
        } 
        public string Path2RecognitionData  
        { 
            get  
            { 
                return _bstrPath2RecognitionData; 
            } 
            set 
            { 
                _bstrPath2RecognitionData = value; 
            }  
        } 
        public int MinMarkerWid  
        { 
            get  
            { 
                return GetMinMarkerWid(); 
            } 
            set 
            { 
                SetMinMarkerWid(value); 
            }  
        } 
        public int MaxMarkerWid  
        { 
            get  
            { 
                return GetMaxMarkerWid(); 
            } 
            set 
            { 
                SetMaxMarkerWid(value); 
            }  
        } 
        public int MinMarkerHgh  
        { 
            get  
            { 
                return GetMinMarkerHgh(); 
            } 
            set 
            { 
                SetMinMarkerHgh(value); 
            }  
        } 
        public int MaxMarkerHgh  
        { 
            get  
            { 
                return GetMaxMarkerHgh(); 
            } 
            set 
            { 
                SetMaxMarkerHgh(value); 
            }  
        } 
        public double MinMarkerRio  
        { 
            get  
            { 
                return GetMinMarkerRio(); 
            } 
            set 
            { 
                SetMinMarkerRio(value); 
            }  
        } 
        public double MaxMarkerRio  
        { 
            get  
            { 
                return GetMaxMarkerRio(); 
            } 
            set 
            { 
                SetMaxMarkerRio(value); 
            }  
        } 
        public int BlankTestStart  
        { 
            get  
            { 
                return GetBlankTestStart(); 
            } 
            set 
            { 
                SetBlankTestStart(value); 
            }  
        } 
        public int BlankTestStop  
        { 
            get  
            { 
                return GetBlankTestStop(); 
            } 
            set 
            { 
                SetBlankTestStop(value); 
            }  
        } 
        public int MinCheckArea  
        { 
            get  
            { 
                return GetMinCheckArea(); 
            } 
            set 
            { 
                SetMinCheckArea(value); 
            }  
        } 
        public int StampLowThr  
        { 
            get  
            { 
                return GetStampOK_Low_Threshold_Area(); 
            } 
            set 
            { 
                SetStampOK_Low_Threshold_Area(value); 
            }  
        } 
        public int StampDigitXsize  
        { 
            get  
            { 
                return GetStampDigitXsize(); 
            } 
            set 
            { 
                SetStampDigitXsize(value); 
            }  
        } 
        public int StampDigitYsize  
        { 
            get  
            { 
                return GetStampDigitYsize(); 
            } 
            set 
            { 
                SetStampDigitYsize(value); 
            }  
        } 
        public int StampDigitMinLineWidth  
        { 
            get  
            { 
                return _stampMinLineWidth; 
            } 
            set 
            { 
                _stampMinLineWidth = value; 
            }  
        } 
        public int StampDigitMaxLineWidth  
        { 
            get  
            { 
                return _stampMaxLineWidth; 
            } 
            set 
            { 
                _stampMaxLineWidth = value; 
            }  
        } 
        public int StampDigitGap  
        { 
            get  
            { 
                return GetStampDigitGap(); 
            } 
            set 
            { 
                SetStampDigitGap(value); 
            }  
        } 
        public int StampDigitDistBotLine  
        { 
            get  
            { 
                return GetStampDigitDistBotLine(); 
            } 
            set 
            { 
                SetStampDigitDistBotLine(value); 
            }  
        } 
        public int StampDigitDistLftLine  
        { 
            get  
            { 
                return GetStampDigitDistLftLine(); 
            } 
            set 
            { 
                SetStampDigitDistLftLine(value); 
            }  
        } 
        public int StampDigitDistRghLine  
        { 
            get  
            { 
                return GetStampDigitDistRghLine(); 
            } 
            set 
            { 
                SetStampDigitDistRghLine(value); 
            }  
        } 
        public int StampFrameWidth  
        { 
            get  
            { 
                return GetStampFrameWidth(); 
            } 
            set 
            { 
                SetStampFrameWidth(value); 
            }  
        } 
        public bool CutWeakCheck  
        { 
            get  
            { 
                return _ignoreOneExtraCheck; 
            } 
            set 
            { 
                _ignoreOneExtraCheck = value; 
                SetIgnoreOneExtraCheck(_ignoreOneExtraCheck.ToInt()); 
            }  
        } 
        public bool SeekBottomRightLine  
        { 
            get  
            { 
                return _seekBottom; 
            } 
            set 
            { 
                if( value != _seekBottom) 
                { 
                    _seekBottom = value; 
                    EnableSectBotLineTest(_seekBottom ? 1 : 0); 
                } 
            }  
        } 
        public int StampVSize  
        { 
            get  
            { 
                return _stampVSize; 
            } 
            set 
            { 
                _stampVSize = value; 
                SetStampZoneVertSize(value); 
            }  
        } 
        public bool LookForLostSquare  
        { 
            get  
            { 
                return _lookForLostSquare; 
            } 
            set 
            { 
                _lookForLostSquare = value; 
                SetLook4LostSquare(_lookForLostSquare ? 1 : 0); 
            } 
        } 
        public int RunRecCount  
        { 
            get  
            { 
                return s_runRecCount; 
            } 
        } 
        public void EnableLogging(string sLogFileName) 
        { 
            EnableLoggingInternal(sLogFileName); 
        } 
        public int MinStandartMarkerWid  
        { 
            get  
            { 
                return GetMinStandartMarkerWid(); 
            } 
            set 
            { 
                SetMinStandartMarkerWid(value); 
            }  
        } 
        public int MaxStandartMarkerWid  
        { 
            get  
            { 
                return GetMaxStandartMarkerWid(); 
            } 
            set 
            { 
                SetMaxStandartMarkerWid(value); 
            }  
        } 
        public int MinStandartMarkerHgh  
        { 
            get  
            { 
                return GetMinStandartMarkerHgh(); 
            } 
            set 
            { 
                SetMinStandartMarkerHgh(value); 
            }  
        } 
        public int MaxStandartMarkerHgh  
        { 
            get  
            { 
                return GetMaxStandartMarkerHgh(); 
            } 
            set 
            { 
                SetMaxStandartMarkerHgh(value); 
            }  
        } 
        public int StandartMarkerZone  
        { 
            get  
            { 
                return GetStandartMarkerZone(); 
            } 
            set 
            { 
                SetStandartMarkerZone(value); 
            }  
        } 
        public int OffsetFirstRule 
        { 
            get  
            { 
                return GetOffsetFirstRule(); 
            } 
            set 
            { 
                SetOffsetFirstRule(value); 
            }  
        } 
        int OcrCallBackHandler(OcrCallBackType cbType, IntPtr data, int size) 
        { 
            try 
            { 
                DebugOut("CALLBACK: " + cbType + " [" + data.ToInt32().ToString("X") + ", " + size + "]"); 
                switch (cbType) 
                { 
                    case OcrCallBackType.ModelSave: 
                        { 
                            if (size <= 0 || data == IntPtr.Zero) 
                            { 
                                SetError(ErrorCode.IllegalUse, 
                                         "CALLBACK: " + cbType + 
                                         ": неверные или неожиданные параметры [" 
                                         + data.ToInt32().ToString("X") + ", " + 
                                         size + "]"); 
                                return -3; 
                            } 
                            try 
                            { 
                                if (File.Exists(ModelFilePath)) 
                                    File.Delete(ModelFilePath); 
                                var modelData = new byte[size + 1]; 
                                Marshal.Copy(data, modelData, 0, size); 
                                using (FileStream fs = File.Create(ModelFilePath)) 
                                { 
                                    fs.Write(modelData, 0, size); 
                                    fs.Flush(); 
                                } 
                                return 1; 
                            } 
                            catch (Exception ex) 
                            { 
                                SetError(ErrorCode.UnexpectedError, "CALLBACK: " + cbType + ": " + ex); 
                                return -2; 
                            } 
                        } 
                    case OcrCallBackType.GetModelFileSize: 
                        { 
                            try  
                            { 
                                var fi = new FileInfo(ModelFilePath); 
                                return (int)fi.Length; 
                            }  
                            catch (Exception ex) 
                            { 
                                SetError(ErrorCode.UnexpectedError, "CALLBACK: " + cbType + ": " + ex); 
                                return -2; 
                            } 
                        } 
                    case OcrCallBackType.ModelRestore: 
                        { 
                            int    retval = 1; 
                            var fi = new FileInfo(ModelFilePath); 
                            if (fi.Length == size) 
                            { 
                                var modelData = new byte[size + 1]; 
                                using (FileStream fs = File.OpenRead(ModelFilePath)) 
                                { 
                                    fs.Read(modelData, 0, size); 
                                } 
                                Marshal.Copy(modelData, 0, data, size); 
                            } 
                            else 
                            { 
                                SetError(ErrorCode.IllegalUse, "CALLBACK: " + cbType + ": длина файла не соответствует ожидаемой"); 
                                retval = -1; 
                            } 
                            return retval; 
                        } 
                    case OcrCallBackType.DataSave: 
                        break; 
                    case OcrCallBackType.DataRestore: 
                        break; 
                    case OcrCallBackType.GetStamp: 
                        return GetStamp(size); 
                    case OcrCallBackType.GetStampCount: 
                        return GetStampCount(); 
                    case OcrCallBackType.GetStampMinLineWidth:             
                        return GetStampMinLineWidth(); 
                    case OcrCallBackType.GetStampMaxLineWidth:             
                        return GetStampMaxLineWidth(); 


                    case OcrCallBackType.GetStampTestLevel: 
                        return GetStampTestLevel(); 
                    case OcrCallBackType.GetSideResolution: 
                        { 
                            if (data == IntPtr.Zero)  
                            { 
                                SetError(ErrorCode.IllegalUse, "CALLBACK: " + cbType +  
                                    ": неверные или неожиданные параметры ["  
                                    + data.ToInt32().ToString("X") + ", " + size + "]"); 
                                return -1; 
                            } 
                            var s = (Resolution)Marshal.PtrToStructure(data, typeof(Resolution)); 
                            if (s.side == 0) 
                            { 
                                s.x = _dpiX0; 
                                s.y = _dpiY0; 
                                Marshal.StructureToPtr(s, data, true); 
                            } 
                            else if (s.side == 1) 
                            { 
                                s.x = _dpiX1; 
                                s.y = _dpiY1; 
                                Marshal.StructureToPtr(s, data, true); 
                            } 
                            else  
                            { 
                                SetError(ErrorCode.IllegalUse, "CALLBACK: " + cbType +  
                                    ": неверные или неожиданные параметры ["  
                                    + data.ToInt32().ToString("X") + ", " + size + "]"); 
                                return -1; 
                            } 
                        } 
                        break; 
                    case OcrCallBackType.PutBulletinNumber: 
                        PutBulletinNumber(size); 
                        break; 
                    case OcrCallBackType.PutResults: 
                        { 
                            var s = (OcrResult)Marshal.PtrToStructure(data, typeof(OcrResult)); 
                            DebugOut("s.IsValid = " + s.IsValid + ", s.numChecked = " 
                                + s.numChecked + ", s.PollNum = " + s.PollNum); 
                            if (! PutResult(s))  
                            { 
                                SetError(ErrorCode.UnexpectedError, "CALLBACK: " 
                                    + cbType + ": не удалось сохранить результат распознавания"); 
                                return -1; 
                            } 
                        } 
                        break; 
                    case OcrCallBackType.GetPath2Data:  
                        { 
                            if(string.IsNullOrEmpty(_bstrPath2RecognitionData)) 
                            { 
                                data = IntPtr.Zero; 
                            } 
                            else  
                            { 
                                Marshal.Copy(_bstrPath2RecognitionData.ToCharArray(), 0, data, _bstrPath2RecognitionData.Length); 
                            } 
                        } 
                        break; 
                    case OcrCallBackType.GetDigitSquares: 
                        return 0; 
                    case OcrCallBackType.UnloadDigitOcrResult: 
                        return -1; 
                    case OcrCallBackType.GetGrayRectBuffSize:         
                        if(data == IntPtr.Zero || (size != 0 && size != 1)) 
                        { 
                            SetError(ErrorCode.IllegalUse, "CALLBACK: " + cbType + 
                                ": неверные или неожиданные параметры [" 
                                + data.ToInt32().ToString("X") + ", " + size + "]"); 
                            return -1; 
                        } 
                        var r = (AlRect) Marshal.PtrToStructure(data, typeof (AlRect)); 
                        if (r.y < 0 || r.h < 0 || r.x < 0 || r.w < 0) 
                        { 
                            SetError(ErrorCode.IllegalUse, "CALLBACK: " + cbType + 
                                                           ": Запрошен недопустимый размер буфера полутона: x=" + 
                                                           r.x + ", y=" + r.y + ", w=" + r.w + ", h=" + r.h); 
                            return -1; 
                        } 
                        var bufSize = r.h * r.w; // количество пиксел 
                        DebugOut("Вычислен размер полутонового буфера: size=" + bufSize); 
                        return bufSize; 
                    case OcrCallBackType.GetGrayRectImage:     
                        if (data == IntPtr.Zero || (size != 0 && size != 1)) 
                        { 
                            return -1; 
                        } 
                        var alSubf = (AlSubf) Marshal.PtrToStructure(data, typeof (AlSubf)); 
                        if (_events != null) 
                        { 
                            var piMem = new MemoryBlock(alSubf.@base); 
                            try 
                            { 
                                long nSize = _events.GetHalfToneBuffer(this, (short) size, alSubf.x, 
                                                                       alSubf.y, alSubf.ys, alSubf.xs, piMem); 
                                if (nSize == -1) 
                                { 
                                    DebugOut("GetGrayRectImage: Unable to get image"); 
                                    return -1; 
                                } 
                            } 
                            catch (Exception ex) 
                            { 
                                SetError(ErrorCode.UnexpectedError, "CALLBACK: " + cbType + ": " + ex); 
                                return -1; 
                            } 
                        } 
                        else 
                        { 
                            SetError(ErrorCode.IllegalUse, "CALLBACK: " + cbType + 
                                ": не установлен обработчик события GetHalfToneBuffer"); 
                            return -1; 
                        } 
                        alSubf.width = alSubf.xs; 
                        return 1; 
                    case OcrCallBackType.GetBinThreshold:         
                        if (size == 0 || size == 1) 
                        { 
                            try  
                            { 
                                if (_events != null) 
                                { 
                                    int nThr = _events.GetBinaryThreshold(this, (short)size); 
                                    return nThr; 
                                } 
                                SetError(ErrorCode.IllegalUse, "CALLBACK: " + cbType + 
                                                               ": не установлен обработчик события GetBinaryThreshold"); 
                                return -1; 
                            }  
                            catch (Exception ex) 
                            { 
                                SetError(ErrorCode.UnexpectedError, "CALLBACK: " + cbType + ": " + ex); 
                                return -1; 
                            } 
                        } 
                        SetError(ErrorCode.IllegalUse, "CALLBACK: " + cbType + 
                                                       ": неверные или неожиданные параметры [" + 
                                                       data.ToInt32().ToString("X") + ", " + size + "]"); 
                        return -1; 
                    case OcrCallBackType.ReportProgress: 
                        DebugOut("Progress: " + size); 
                        break; 
                    default: 
                        SetError(ErrorCode.IllegalUse, "CALLBACK: не распознанный код " + cbType); 
                        break; 
                } 
            } 
            catch(Exception ex) 
            { 
                try 
                { 
                    SetError(ErrorCode.UnexpectedError, "CALLBACK: ERROR [" + cbType + ", " + data.ToInt32().ToString("X") + ", " + size + "] => " + ex); 
                } 
                catch(Exception exOnSetError) 
                { 
                    SetError(ErrorCode.UnexpectedError, "CALLBACK: ERROR [" + cbType + "] => " + ex); 
                    SetError(ErrorCode.UnexpectedError, "CALLBACK: SETERROR => " + exOnSetError); 
                } 
            } 
            return 1; 
        } 
        int GetStampMaxLineWidth() 
        { 
            return _stampMaxLineWidth; 
        } 
        int GetStampMinLineWidth() 
        { 
            return _stampMinLineWidth; 
        } 
        int    GetStampTestLevel() 
        { 
            return (int)_stampTestLevel; 
        } 
        void PutBulletinNumber(int bulletinNumber) 
        { 
            _pollResults.Clear(); 
            _bulletinNumber = bulletinNumber; 
        } 
        private bool PutResult(OcrResult results) 
        { 
            try 
            { 
                var pr = new PollResult 
                             { 
                                 PollNumber = results.PollNum, 
                                 IsValid = (results.IsValid > 0) 
                             }; 
                var sqData = new int[results.numChecked]; 
                Marshal.Copy(results.sqData, sqData, 0, results.numChecked); 
                for (int i = 0; i < results.numChecked; i++) 
                { 
                    pr.Add(sqData[i]); 
                } 
                _pollResults.Add(pr); 
                return true; 
            } 
            catch 
            { 
                return false; 
            } 
        } 
        void SetError(ErrorCode code, string message) 
        { 
            if (_events != null) 
            { 
                _events.Error(this, (int)code, message); 
            } 
        } 
        void DebugOut(string message) 
        { 
            if (_events != null) 
            { 
                _events.AppendToLog(this, message); 
            } 
        } 
        public static string GetGeoResultMessage(GeoData geoData, long squareCount) 
        { 
            if ((GeoResult)geoData.result == GeoResult.OK) 
            { 
                long maxSquareSkew = 0; 
                long maxSquareSkewH = 0, minSquareSkewH = Int32.MaxValue; 
                long maxSquareSizeSkew = 0, minSquareSizeSkew = Int32.MaxValue; 
                int minColor, maxColor; 
                minColor = maxColor = geoData.topMarkerColor; 
                minColor = Math.Min(minColor, geoData.bottomMarkerColor); 
                maxColor = Math.Max(maxColor, geoData.bottomMarkerColor); 
                minColor = Math.Min(minColor, geoData.baseLineColor); 
                maxColor = Math.Max(maxColor, geoData.baseLineColor); 
                for (int i = 0; i < squareCount; i++) 
                { 
                    if (geoData.squares[i] == 1) 
                    { 
                        maxSquareSkew = Math.Max(maxSquareSkew, geoData.squaresSkewV[i]); 
                        maxSquareSkew = Math.Max(maxSquareSkew, geoData.squaresSkewV[i]); 
                        maxSquareSkewH = Math.Max(maxSquareSkewH, geoData.squaresSkewH[i]); 
                        minSquareSkewH = Math.Min(minSquareSkewH, geoData.squaresSkewH[i]); 
                        maxSquareSizeSkew = Math.Max(maxSquareSizeSkew, geoData.squaresSize[i]); 
                        minSquareSizeSkew = Math.Min(minSquareSizeSkew, geoData.squaresSize[i]); 
                        minColor = Math.Min(minColor, geoData.squaresColor[i]); 
                        maxColor = Math.Max(maxColor, geoData.squaresColor[i]); 
                    } 
                } 
                if (minSquareSizeSkew == Int32.MaxValue) 
                { 
                    minSquareSizeSkew = maxSquareSizeSkew; 
                } 
                if (maxSquareSkew > 3 || 
                    maxSquareSkewH != minSquareSkewH && maxSquareSkewH > 2 || 
                    maxSquareSizeSkew != 0) 
                { 
                    return "Некорректное расположение квадратов"; 
                } 
                if (maxColor - minColor > 20 && maxColor > minColor * 2) 
                { 
                    return "Нарушена равномерность черного цвета"; 
                } 
                if (maxColor > 64) 
                { 
                    return "Недостаточная интенсивность черного цвета"; 
                } 
            } 
            switch ((GeoResult)geoData.result) 
            { 
                case GeoResult.OK: 
                    return "бюллетень соответствует модели"; 
                case GeoResult.TopMarker: 
                    return "не найдены маркеры"; 
                case GeoResult.BottomMarker: 
                    return "не найден или не определен нижний маркер"; 
                case GeoResult.Markers: 
                    return "несовпадение маркеров"; 
                case GeoResult.BadMarkerNum: 
                    return "недопустимый код маркера"; 
                case GeoResult.LeftSide: 
                    return "не найдена левая граница бюллетеня"; 
                case GeoResult.BaseLine: 
                    return "не найдена базовая линия"; 
                case GeoResult.BadBaseLine: 
                    return "неверное положение базовой линии"; 
                case GeoResult.Squares: 
                    return "не найдены квадраты"; 
                default: 
                    return "неожиданный код возврата"; 
            } 
        } 
        public static void ThrowLastError() 
        { 
            const int MAX_ERROR_MESSAGE_LENGTH = 1000; 
            var sDescr = new StringBuilder(MAX_ERROR_MESSAGE_LENGTH); 
            int nRes = GetErrorDesc(sDescr); 
            if (0 != nRes) 
                throw new Exception(String.Format("Ошибка Ocr. Описание получить не удалось: {0}", nRes)); 


            throw new Exception(sDescr.ToString()); 
        } 
        public static StampResult IsStampOKGray(ref string stampNumber, ref string[] alternatives) 
        { 
            byte[][] digits = new byte[STAMP_DIGIT_COUNT][]; 
            for (int i = 0; i < STAMP_DIGIT_COUNT; i++) 
            { 
                digits[i] = new byte[10]; 
            } 
            var stampResult = (StampResult)IsStampOKGray(digits[0], digits[1], digits[2], digits[3]); 
            for (int i = 0; i < STAMP_DIGIT_COUNT; i++) 
            { 
                alternatives[i] = ""; 
                for (int j = 0; digits[i][j] != 0; j++) 
                { 
                    alternatives[i] += (char)digits[i][j]; 
                } 
                if (alternatives[i].Length == 0) 
                { 
                    alternatives[i] += "X"; 
                } 
                stampNumber += alternatives[i][0]; 
            } 
            return stampResult; 
        } 
        #region DllImport-ы из Ocr.dll 
        [DllImport("Xib.dll")] 
        internal extern static int InitModel(int iBulCount, int iBulCodeType); 
        [DllImport("Xib.dll")] 
        internal extern static int createBallotModel(byte[] sData, int iNum, int iCode); 
        [DllImport("Xib.dll")] 
        internal extern static int SetPollData(int iBallot, int iPoll, ref PollData pData); 
        [DllImport("Xib.dll")] 
        internal extern static int SetDefaultStampGeometry(); 
        [DllImport("Xib.dll")] 
        internal extern static int GetErrorDesc(StringBuilder sError); 
        [DllImport("Xib.dll")] 
        internal extern static int ClearError(); 
        [DllImport("Xib.dll", CharSet = CharSet.Ansi)] 
        internal extern static IntPtr SaveAsText(StringBuilder sText, int nSize); 
        [DllImport("Xib.dll")] 
        internal extern static int GetFeatureOnlineTest(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetFeatureOnlineTest(int val); 
        [DllImport("Xib.dll")] 
        internal extern static OcrCallBack OCR_Initialize(OcrCallBack callback); 
        [DllImport("Xib.dll", EntryPoint = "EnableLogging")] 
        internal extern static void EnableLoggingInternal(string fileName); 
        [DllImport("Xib.dll")] 
        internal extern static int EndRecognition(MarkerType DoIt, int y0, int y1); 
        [DllImport("Xib.dll")] 
        internal extern static int TestBallot(int y0, int y1, ref GeoData pData); 
        [DllImport("Xib.dll")] 
        internal extern static int SetBlankTestStart(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetBlankTestStart(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetBlankTestStop(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetBlankTestStop(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetIgnoreOneExtraCheck(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int SetLook4LostSquare(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int SetMaxMarkerHgh(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetMaxMarkerHgh(); 
        [DllImport("Xib.dll")] 
        internal extern static double SetMaxMarkerRio(double val); 
        [DllImport("Xib.dll")] 
        internal extern static double GetMaxMarkerRio(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetMaxMarkerWid(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetMaxMarkerWid(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetMaxOnlineSkew(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetMaxOnlineSkew(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetMaxStandartMarkerHgh(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetMaxStandartMarkerHgh(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetMaxStandartMarkerWid(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetMaxStandartMarkerWid(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetMinCheckArea(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetMinCheckArea(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetMinMarkerHgh(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetMinMarkerHgh(); 
        [DllImport("Xib.dll")] 
        internal extern static double SetMinMarkerRio(double val); 
        [DllImport("Xib.dll")] 
        internal extern static double GetMinMarkerRio(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetMinMarkerWid(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetMinMarkerWid(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetMinStandartMarkerHgh(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetMinStandartMarkerHgh(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetMinStandartMarkerWid(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetMinStandartMarkerWid(); 
        [DllImport("Xib.dll")] 
        internal extern static int EnableSectBotLineTest(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetOffsetFirstRule(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetOffsetFirstRule(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int SetStampDigitDistBotLine(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetStampDigitDistBotLine(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetStampDigitDistLftLine(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetStampDigitDistLftLine(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetStampDigitDistRghLine(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetStampDigitDistRghLine(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetStampDigitGap(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetStampDigitGap(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetStampDigitXsize(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetStampDigitXsize(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetStampDigitYsize(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetStampDigitYsize(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetStampFrameWidth(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetStampFrameWidth(); 
        [DllImport("Xib.dll")] 
        internal extern static int SetStampOK_Low_Threshold_Area(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetStampOK_Low_Threshold_Area(); 
        [DllImport("Xib.dll")] 
        internal extern static int IsStampOK(); 
        [DllImport("Xib.dll", CharSet = CharSet.Ansi)] 
        internal extern static int IsStampOKGray(byte[] digit1, byte[] digit2, byte[] digit3, byte[] digit4); 
        [DllImport("Xib.dll")] 
        internal extern static int SetStampZoneVertSize(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int SetStandartMarkerZone(int val); 
        [DllImport("Xib.dll")] 
        internal extern static int GetStandartMarkerZone(); 
        [DllImport("Xib.dll")] 
        internal extern static int ExcludeSquare(int ballot, int poll, int n); 
        [DllImport("Xib.dll")] 
        internal extern static int EnableSquare(int ballot, int poll, int n); 
        [DllImport("Xib.dll")] 
        internal extern static int IsSquareValid(int ballot, int poll, int n); 
        [DllImport("Xib.dll")] 
        internal extern static int LinkChangedModel(); 
        [DllImport("Xib.dll")] 
        internal extern static int StartRecognition(ref BufferHeader pSide0, ref BufferHeader pSide1); 
        [DllImport("Xib.dll")] 
        internal extern static void StartOnLineTesting(ref BufferHeader pSide0, ref BufferHeader pSide1, int FrameLineDist_mm); 
        [DllImport("Xib.dll")] 
        internal extern static int GetFrameDist_mm(); 
        [DllImport("Xib.dll")] 
        internal extern static int DefaultModelSave(); 
        [DllImport("Xib.dll", EntryPoint = "NextBuffer")] 
        internal extern static int NextBufferInternal(int count); 
        [DllImport("Xib.dll")] 
        internal extern static int OnlineDefStandartMarker(); 
        [DllImport("Xib.dll")] 
        internal extern static int OnlineDefCharMarker(); 
        [DllImport("Xib.dll")] 
        internal extern static int CloseRecognition(); 
        [DllImport("Xib.dll")] 
        internal extern static int InitRecognition(); 
        #endregion 
    } 
}
