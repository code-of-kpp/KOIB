using System; 

using System.Collections.Generic; 

using System.Text; 

using System.Runtime.InteropServices; 

using System.IO; 

using Croc.Core.Extensions; 

using Croc.Bpc.Common; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Драйвер распознавалки (OCR - Optical Character Recognizer) 

    /// </summary> 

    public class Ocr : IOcr 

    { 

        /// <summary> 

        /// Количество цифр в номере печати 

        /// </summary> 

        public const int STAMP_DIGIT_COUNT = 4; 

        /// <summary> 

        /// количество запусков функции RunRecognize. 

        /// </summary> 

        private static int		g_lRunRecCount; 

 

 

        /// <summary> 

        /// Размер области печати по вертикали 

        /// </summary> 

        private	int		m_nStampVSize; 

        /// <summary> 

        /// Признак необходимости поиска нижней линии 

        /// </summary> 

        private	bool	m_bSeekBottom; 

        /// <summary> 

        /// Буфера для хранения изображений 

        /// </summary> 

        private			BufferHeader  m_bhSide0, m_bhSide1; 

        /// <summary> 

        /// Флаг того, что распознавание начато 

        /// </summary> 

        private	bool	m_bStarted; 

        /// <summary> 

        /// Разрешения сканера по X и Y по обеим сторонам 

        /// </summary> 

        private float	m_dpiX0, m_dpiX1, m_dpiY0, m_dpiY1; 

        /// <summary> 

        /// Обработчик событий (запросов) распознавалки 

        /// </summary> 

        private IOcrEventHandler	m_pEvents; 


        /// <summary> 

        /// Путь к каталогу распознавалки 

        /// </summary> 

        private string		m_bstrPath2RecognitionData; 

        /// <summary> 

        /// Размер массива для хранения номеров комиссий 

        /// </summary> 

        private const int     m_StampSize = 10; 

        /// <summary> 

        /// Массив номеров комиссий для проверки печати 

        /// </summary> 

        private int[]			m_alStamp = new int[m_StampSize]; 

        /// <summary> 

        /// Режим проверки печати 

        /// </summary> 

        private StampTestLevel	m_sStampTestLevel; 

        /// <summary> 

        /// Результаты распознавания 

        /// </summary> 

        private List<PollResult> m_pollResults; 

        /// <summary> 

        /// Номер бюллетеня 

        /// </summary> 

        private int			m_nBulletinNumber = -1; // по умолчанию не определен 

        /// <summary> 

        /// Признак использования механизма отсева одной слабой лишней метки 

        /// </summary> 

        private bool			m_bIgnoreOneExtraCheck; 

        /// <summary> 

        /// искать ли единственный недостающий квадрат 

        /// </summary> 

        private bool			m_bLookForLostSquare;  

        /// <summary> 

        /// Размеры в линиях последних полученных порций изображения 

        /// </summary> 

        private int[]			m_iLastLines = new int[2]; 

        /// <summary> 

        /// Максимальная ширина линии печати 

        /// </summary> 

        private int			m_lStampMaxLineWidth; 

        /// <summary> 

        /// Минимальная ширина линии печати 

        /// </summary> 

        private int			m_lStampMinLineWidth; 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        public Ocr() 


        { 

            // Для совместимости с старым COM-объектом 

            // так как распознавалка у нас лежит на уровень выше 

            string sCurrentDir = Directory.GetCurrentDirectory(); 

            Directory.SetCurrentDirectory(".."); 

            m_pEvents = null; 

            LookForLostSquare = false; 

 

 

            g_lRunRecCount = 0;	// инициализирую количество вызовов функции RunRecognize 

 

 

            Initialize(); 

            Directory.SetCurrentDirectory(sCurrentDir); 

        } 

 

 

        /// <summary> 

        /// Инициализация компоненты 

        /// </summary> 

        /// <returns>всегда true</returns> 

        bool Initialize() 

        { 

            m_pollResults = new List<PollResult>(); 

            m_bStarted = false; 

            m_bIgnoreOneExtraCheck = false; 

            m_iLastLines[0] = m_iLastLines[1] = 0; 

            m_bSeekBottom = false; 

            m_nStampVSize = 380; 

            return true; 

        } 

 

 

        // Implement IDisposable. 

        // Do not make this method virtual. 

        // A derived class should not be able to override this method. 

        public void Dispose() 

        { 

            Dispose(true); 

            // This object will be cleaned up by the Dispose method. 

            // Therefore, you should call GC.SupressFinalize to 

            // take this object off the finalization queue  

            // and prevent finalization code for this object 

            // from executing a second time. 

            GC.SuppressFinalize(this); 

        } 

 

 

        // Track whether Dispose has been called. 

        private bool disposed = false; 


 
 

        // Dispose(bool disposing) executes in two distinct scenarios. 

        // If disposing equals true, the method has been called directly 

        // or indirectly by a user's code. Managed and unmanaged resources 

        // can be disposed. 

        // If disposing equals false, the method has been called by the  

        // runtime from inside the finalizer and you should not reference  

        // other objects. Only unmanaged resources can be disposed. 

        private void Dispose(bool disposing) 

        { 

            // Check to see if Dispose has already been called. 

            if(!this.disposed) 

            { 

                // If disposing equals true, dispose all managed  

                // and unmanaged resources. 

                if(disposing) 

                { 

                    // Dispose managed resources. 

                } 

 

 

                // Call the appropriate methods to clean up  

                // unmanaged resources here. 

                // If disposing is false,  

                // only the following code is executed. 

				try  

				{ 

					// убираем обрабочика сообщений, чтобы не падала распознавалка 

					Ocr.OCR_Initialize(null); 

					m_pEvents = null; 

					Ocr.CloseRecognition(); 

				} 

				catch  

				{ 

					// на случай падения распознавалки 

				} 

            } 

            disposed = true;          

        } 

 

 

        /// <summary> 

        /// Деструктор 

        /// </summary> 

        ~Ocr() 

        { 

            // Do not re-create Dispose clean-up code here. 

            // Calling Dispose(false) is optimal in terms of 

            // readability and maintainability. 


            Dispose(false); 

        } 

 

 

        /// <summary> 

        /// Получить номер комиссии 

        /// </summary> 

        /// <param name="nIndex">Номер номера в массиве номеров</param> 

        /// <returns>Номер комиссии, -1 в случае ошибки</returns> 

        int	GetStamp(int nIndex) 

        {  

            if(0 > nIndex || m_StampSize <= nIndex)  

            { 

                SetError(ErrorCode.IllegalUse, "GetStamp: выход за пределы массива"); 

                return -1; 

            } 

            return m_alStamp[nIndex];  

        } 

 

 

        /// <summary> 

        /// Вернуть количество номеров комиссии 

        /// </summary> 

        /// <returns>количество номеров</returns> 

        int	GetStampCount()	 

        {  

            int i; 

            for(i = 0; i < m_StampSize; i++)  

            { 

                if(!(0 < m_alStamp[i])) 

                { 

                    break; 

                } 

            } 

 

 

            return i; 

        } 

 

 

        /// <summary> 

        /// Callback объявлен здесь, чтобы предотвратить его убивание сборщиком мусора 

        /// </summary> 

        OcrCallBack myCallBack = null; 

 

 

        /// <summary> 

        /// Инициализация компоненты. Необходимо вызывать сразу после создания 

        /// </summary> 

        public void Init() 


        { 

            myCallBack = new OcrCallBack(OcrCallBackHandler); 

            Ocr.OCR_Initialize(myCallBack); 

        } 

 

 

        /// <summary> 

        /// установить внешний event-интерфейс 

        /// </summary> 

        /// <param name="pEvent">Обработчик</param> 

        public void SetEventsHandler(IOcrEventHandler pEvent) 

        { 

            m_pEvents = pEvent; 

        } 

 

 

        /// <summary> 

        /// Уровень проверки печати 

        /// </summary> 

        public StampTestLevel StampTestLevel  

        { 

            get 

            { 

                return m_sStampTestLevel; 

            } 

            set 

            { 

                m_sStampTestLevel = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Уровень проверки небюллетеня 

        /// </summary> 

        public InlineLevel InlineRecognitionLevel  

        { 

            get 

            { 

                return (InlineLevel)Ocr.GetFeatureOnlineTest(); 

            } 

            set  

            { 

                Ocr.SetFeatureOnlineTest((int)value); 

            } 

        } 

 

 

        /// <summary> 

        /// Максимально допустимое значение перекоса 


        /// </summary> 

        public int MaxOnlineSkew  

        { 

            get  

            { 

                return Ocr.GetMaxOnlineSkew(); 

            } 

            set 

            { 

                Ocr.SetMaxOnlineSkew(value); 

            }  

        } 

 

 

        /// <summary> 

        /// Разрешение сканера по X 

        /// </summary> 

        public float DpiX0  

        { 

            get  

            { 

                return m_dpiX0; 

            } 

            set 

            { 

                m_dpiX0 = value; 

            }  

        } 

 

 

        /// <summary> 

        /// Разрешение сканера по X 

        /// </summary> 

        public float DpiX1  

        { 

            get  

            { 

                return m_dpiX1; 

            } 

            set 

            { 

                m_dpiX1 = value; 

            }  

        } 

 

 

        /// <summary> 

        /// Разрешение сканера по Y 

        /// </summary> 

        public float DpiY0  


        { 

            get  

            { 

                return m_dpiY0; 

            } 

            set 

            { 

                m_dpiY0 = value; 

            }  

        } 

 

 

        /// <summary> 

        /// Разрешение сканера по Y 

        /// </summary> 

        public float DpiY1  

        { 

            get  

            { 

                return m_dpiY1; 

            } 

            set 

            { 

                m_dpiY1 = value; 

            }  

        } 

 

 

        /// <summary> 

        /// Установить разрешение сканера 

        /// </summary> 

        /// <param name="x0">значение по оси Х для стороны 0</param> 

        /// <param name="y0">значение по оси Y для стороны 0</param> 

        /// <param name="x1">значение по оси Х для стороны 1</param> 

        /// <param name="y1">значение по оси Y для стороны 1</param> 

        public void SetDpi(float x0, float y0, float x1, float y1) 

        { 

            m_dpiX0 = x0; 

            m_dpiY0 = y0; 

            m_dpiX1 = x1; 

            m_dpiY1 = y1; 

        } 

 

 

        /// <summary> 

        /// Номер бюллетеня (от 0), полученный по результату распознавания маркера 

        /// </summary> 

        public int BulletinNumber  

        { 

            get  


            { 

                return this.m_nBulletinNumber; 

            } 

        } 

 

 

        /// <summary> 

        /// Результат распознавания печати 

        /// </summary> 

        public StampResult StampResult  

        { 

            get  

            { 

                return (StampResult)Ocr.IsStampOK(); 

            } 

        } 

 

 

        /// <summary> 

        /// Результаты распознавания 

        /// </summary> 

        public List<PollResult> Results  

        { 

            get  

            { 

                return m_pollResults; 

            } 

        } 

 

 

        /// <summary> 

        /// [DEPRECATED] Cнимает квадрат с гoлoсoвания 

        /// </summary> 

        /// <param name="bulletin">номер бюллетеня</param> 

        /// <param name="election">номер выборов</param> 

        /// <param name="square">номер квадрата в модели</param> 

        public void OCR_ExcludeSquare(int bulletin, int election, int square) 

        { 

            try  

            { 

                if (Ocr.ExcludeSquare(bulletin, election, square) < 0)  

                { 

                    SetError(ErrorCode.IllegalUse, "OCR_ExcludeSquare: Неверные параметры для Ocr.ExcludeSquare: " + bulletin + ", " + election + ", " + square); 

                    throw new OcrException("OCR_ExcludeSquare: Неверные параметры для Ocr.ExcludeSquare: " + bulletin + ", " + election + ", " + square); 

                } 

                if (Ocr.LinkChangedModel() < 0)  

                { 

                    SetError(ErrorCode.LinkModelError, "OCR_ExcludeSquare: Не удалось перекомпилировать модель функцией Ocr.LinkChangedModel"); 

                    throw new OcrException("OCR_ExcludeSquare: Не удалось перекомпилировать модель функцией Ocr.LinkChangedModel"); 

                } 


            } 

            catch (Exception ex) 

            {  

                SetError(ErrorCode.UnexpectedError, "OCR_ExcludeSquare: " + ex.ToString()); 

                throw; 

            } 

        } 

 

 

        /// <summary> 

        /// [DEPRECATED] Восстанавливает учатие квадрата в выборах 

        /// </summary> 

        /// <param name="bulletin">номер бюллетеня</param> 

        /// <param name="election">номер выборов</param> 

        /// <param name="square">номер квадрата в модели</param> 

        public void OCR_RestoreSquare(int bulletin, int election, int square) 

        { 

            try  

            { 

                if (Ocr.EnableSquare(bulletin, election, square) < 0)  

                { 

                    SetError(ErrorCode.IllegalUse, "OCR_RestoreSquare: Неверные параметры для Ocr.EnableSquare: " + bulletin + ", " + election + ", " + square); 

                    throw new OcrException("OCR_RestoreSquare: Неверные параметры для Ocr.EnableSquare: " + bulletin + ", " + election + ", " + square); 

                } 

                if (Ocr.LinkChangedModel() < 0)  

                { 

                    SetError(ErrorCode.LinkModelError, "OCR_RestoreSquare: Не удалось перекомпилировать модель функцией Ocr.LinkChangedModel"); 

                    throw new OcrException("OCR_RestoreSquare: Не удалось перекомпилировать модель функцией Ocr.LinkChangedModel"); 

                } 

            } 

            catch (Exception ex) 

            {  

                SetError(ErrorCode.UnexpectedError, "OCR_RestoreSquare: " + ex.ToString()); 

                throw; 

            } 

        } 

 

 

        /// <summary> 

        /// [DEPRECATED] возвращает состояние флага участия квадрата в выборах 

        /// </summary> 

        /// <param name="bulletin">номер бюллетеня</param> 

        /// <param name="election">номер выборов</param> 

        /// <param name="square">номер квадрата в модели</param> 

        /// <param name="retVal">признак участия в выборах</param> 

        /// <returns>код выполнения операции (меньше 0 сигнализирует от ошибке)</returns> 

        public int OCR_IsSquareValid(int bulletin, int election, int square, out int retVal) 

        { 

            int ret = 0; 

 


 
            try  

            { 

                ret = Ocr.IsSquareValid(bulletin, election, square); 

                if (ret == -1)  

                { 

                    SetError(ErrorCode.IllegalUse, "OCR_IsSquareValid: Неверные параметры для Ocr.IsSquareValid: " + bulletin + ", " + election + ", " + square); 

                    throw new OcrException("OCR_IsSquareValid: Неверные параметры для Ocr.IsSquareValid: " + bulletin + ", " + election + ", " + square); 

                } 

 

 

                retVal = ret; 

            } 

            catch (Exception ex) 

            {  

                SetError(ErrorCode.UnexpectedError, "OCR_IsSquareValid: " + ex.ToString()); 

                retVal = 0; 

                return 0; 

            } 

 

 

            return ret; 

        } 

 

 

        /// <summary> 

        /// Инициализация процесса распознавания бюллетеня 

        /// </summary> 

        /// <param name="pdImage0">изображение для анализа 1я сторона</param> 

        /// <param name="pdImage1">изображение для анализа 2я сторона</param> 

        /// <param name="nLineWidth0">длина строки стороны 1</param> 

        /// <param name="nLineWidth1">длина строки стороны 2</param> 

        /// <param name="nNumOfLines0">количество строк первой стороны</param> 

        /// <param name="nNumOfLines1">количество строк второй стороны</param> 

        public void RunRecognize(MemoryBlock pdImage0, MemoryBlock pdImage1, int nLineWidth0, int nLineWidth1, int nNumOfLines0, int nNumOfLines1) 

        { 

            g_lRunRecCount++;		// увеличиваю количество вызовов функции 

 

 

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

 

 

            m_bhSide0.all_bin_buf = IntPtr.Zero; 

            m_bhSide0.bin_buf = buffer0; 

            m_bhSide0.bin_size = (uint)(nNumOfLines0 * nLineWidth0); 

            m_bhSide0.ByteImageWidth = nLineWidth0; 

            m_bhSide0.ImageWidth = nLineWidth0*8; 

            m_bhSide0.ActualScanNumber = nNumOfLines0; 

 

 

            m_bhSide0.tone_buf = IntPtr.Zero; 

            m_bhSide0.bin_width = 0; 

            m_bhSide0.size = 0; 

            m_bhSide0.tone_size = 0; 

            m_bhSide0.g_p0.len = 0; 

            m_bhSide0.g_p0.start = 0; 

            m_bhSide0.g_p1.len = 0; 

            m_bhSide0.g_p1.start = 0; 

            m_bhSide0.g_p2.len = 0; 

            m_bhSide0.g_p2.start = 0; 

            m_bhSide0.flag = 0; 

            m_bhSide0.ScanCounter = 0; 

            m_bhSide0.OutFlag = 0; 

            m_bhSide0.LineCount = 0; 

            m_bhSide0.xl = 0; 

            m_bhSide0.yl = 0; 

            m_bhSide0.xr = 0; 

            m_bhSide0.yr = 0; 

 

 

            m_bhSide1.bin_size = (uint)(nNumOfLines1 * nLineWidth1); 

            m_bhSide1.bin_buf = buffer1; 

            m_bhSide1.ImageWidth = nLineWidth1*8; 

            m_bhSide1.ByteImageWidth = nLineWidth1; 

            m_bhSide1.ActualScanNumber = nNumOfLines1; 

 

 

            m_bhSide1.tone_buf = IntPtr.Zero; 

            m_bhSide1.bin_width = 0; 

            m_bhSide1.size = 0; 

            m_bhSide1.tone_size = 0; 

            m_bhSide1.g_p0.len = 0; 

            m_bhSide1.g_p0.start = 0; 

            m_bhSide1.g_p1.len = 0; 

            m_bhSide1.g_p1.start = 0; 


            m_bhSide1.g_p2.len = 0; 

            m_bhSide1.g_p2.start = 0; 

            m_bhSide1.flag = 0; 

            m_bhSide1.ScanCounter = 0; 

            m_bhSide1.OutFlag = 0; 

            m_bhSide1.LineCount = 0; 

            m_bhSide1.xl = 0; 

            m_bhSide1.yl = 0; 

            m_bhSide1.xr = 0; 

            m_bhSide1.yr = 0; 

 

 

            // Чистим результаты предыдущего распознавания 

            m_pollResults.Clear(); 

 

 

            // Подготавливаем распознование 

            try  

            { 

                if (m_bStarted)  

                { 

                    Ocr.EndRecognition(0,0,0); 

                } 

                else  

                { 

                    m_bStarted = true; 

                } 

 

 

                // сбрасываем номер бюллетеня 

                m_nBulletinNumber = -1; 

 

 

                // запускаем распознавание 

                int	res = Ocr.StartRecognition(ref m_bhSide0, ref m_bhSide1); 

                if (res < 0) 

                { 

                    SetError(ErrorCode.StartRecognitionFailed, "Не удалось начать распознавание: " + res); 

                    throw new OcrException("Не удалось начать распознавание: " + res); 

                } 

 

 

                Ocr.StartOnLineTesting(ref m_bhSide0, ref m_bhSide1, Ocr.GetFrameDist_mm()); 

            } 

            catch (Exception ex) 

            { 

                SetError(ErrorCode.UnexpectedError, "RunRecognize: " + ex.ToString()); 

                throw; 

            } 

        } 


 
 

        /// <summary> 

        /// Завершение процесса распознавания 

        /// </summary> 

        /// <param name="mtMarkerType">Тип маркера</param> 

        /// <returns>Результат распознавания</returns> 

        public int EndRecognize(MarkerType mtMarkerType) 

        { 

            try  

            { 

                m_bStarted = false; 

                return Ocr.EndRecognition(mtMarkerType, m_iLastLines[0], m_iLastLines[1]); 

            } 

            catch (Exception ex) 

            {  

                SetError(ErrorCode.UnexpectedError, "EndRecognize: " + ex.ToString()); 

                throw; 

            } 

        } 

 

 

        /// <summary> 

        /// Завершение процесса распознавания: проверка геометрии бюллетеня 

        /// </summary> 

        /// <returns>Результат распознавания</returns> 

        public int TestBallot(ref GeoData GeoData) 

        { 

            try  

            { 

                m_bStarted = false; 

                return Ocr.TestBallot(m_iLastLines[0], m_iLastLines[1], ref GeoData); 

            } 

            catch (Exception ex) 

            {  

                SetError(ErrorCode.UnexpectedError, "TestBallot: " + ex.ToString()); 

                throw; 

            } 

        } 

 

 

        /// <summary> 

        /// компилирует модель 

        /// </summary> 

        public void LinkModel() 

        { 

            try  

            { 

                if (Ocr.DefaultModelSave() < 0) 

                { 


                    SetError(ErrorCode.LinkModelError, "LinkModel: не удалось скомпилировать модель"); 

                    throw new OcrException("LinkModel: не удалось скомпилировать модель"); 

                } 

            } 

            catch (Exception ex) 

            {  

                SetError(ErrorCode.UnexpectedError, "LinkModel: " + ex.ToString()); 

                throw; 

            } 

        } 

 

 

        /// <summary> 

        /// Получить порцию изображения 

        /// </summary> 

        /// <param name="count">Количество строк</param> 

        /// <returns> 

        /// -1  - это не бюллетень  

        ///  0  - решение пока не принято  

        ///  1  - признан бюллетенем 

        /// </returns> 

        public int NextBuffer(int count) 

        { 

            try  

            { 

                int res = Ocr.NextBufferInternal(count); 

                m_iLastLines[0] = count; 

                m_iLastLines[1] = count; 

 

 

                if (res < 0) 

                { 

                    // do nothing 

                } 

 

 

                return res; 

            } 

            catch (Exception ex) 

            {  

                SetError(ErrorCode.UnexpectedError, "NextBuffer: " + ex.ToString()); 

                throw; 

            } 

        } 

 

 

        /// <summary> 

        /// Определить в online номер маркера 

        /// </summary> 

        /// <param name="mtMarkerType">Тип маркера</param> 


        /// <returns>номер маркера</returns> 

        public int GetOnlineMarker(MarkerType mtMarkerType) 

        { 

            try  

            { 

                int res = 0; 

 

 

                switch( mtMarkerType )  

                { 

                    case MarkerType.Standard: 

                        // штрих-маркер 

                        res = Ocr.OnlineDefStandartMarker(); 

                        break; 

                    case MarkerType.Digital: 

                        // цифровой маркер 

                        res = Ocr.OnlineDefCharMarker(); 

                        break; 

                    default: 

                        SetError(ErrorCode.IllegalUse, "GetOnlineMarker: неверный тип маркера: " + mtMarkerType); 

                        // неверные параметры функции 

                        res = -1; 

                        break; 

                } 

 

 

                return res; 

            } 

            catch (Exception ex) 

            {  

                SetError(ErrorCode.UnexpectedError, "GetOnlineMarker: " + ex.ToString()); 

                throw; 

            } 

        } 

 

 

        /// <summary> 

        /// Инициализация модуля разпознавания. Вызывается при вхoде в режим выбoрoв 

        /// </summary> 

        public void InitRecognize() 

        { 

            try  

            { 

                Ocr.CloseRecognition(); 

 

 

                int	res = Ocr.InitRecognition(); 

 

 

                if (res < 0) 


                { 

                    SetError(ErrorCode.StartRecognitionFailed, "InitRecognize: Ошибка при инициализации распознавания: " + res); 

                    throw new OcrException("Ошибка при инициализации распознавания: " + res); 

                } 

            } 

            catch (Exception ex) 

            {  

                SetError(ErrorCode.UnexpectedError, "InitRecognize: " + ex.ToString()); 

                throw; 

            } 

        } 

 

 

        /// <summary> 

        /// Добавляет номер комиссии в массив 

        /// </summary> 

        /// <param name="nStamp">Номер комиссии</param> 

        public void AddStamp(int nStamp) 

        { 

            int nIndex = GetStampCount(); 

 

 

            if(m_StampSize > nIndex)  

            { 

                m_alStamp[nIndex] = nStamp; 

            } 

            else 

            { 

                SetError(ErrorCode.IllegalUse, "AddStamp: Превышен лимит. Не больше " + m_StampSize + " номеров"); 

                throw new OcrException("AddStamp: Превышен лимит. Не больше " + m_StampSize + " номеров"); 

            } 

        } 

 

 

        /// <summary> 

        /// Очистка массива номеров комиссий 

        /// </summary> 

        public void ClearStamps() 

        { 

            // очищу массив номеров 

            for(int i = 0; i < m_StampSize; i++)  

            { 

                m_alStamp[i] = 0; 

            } 

        } 

 

 

        /// <summary> 

        /// Путь к файлу с моделью 

        /// </summary> 


        public string ModelFilePath  

        { 

            get; 

            set; 

        } 

 

 

        /// <summary> 

        /// Полный путь к данным и модулю распознавания цифр. Должен заканчиваться на слэш 

        /// </summary> 

        public string Path2RecognitionData  

        { 

            get  

            { 

                return m_bstrPath2RecognitionData; 

            } 

            set 

            { 

                m_bstrPath2RecognitionData = value; 

            }  

        } 

 

 

        /// <summary> 

        /// Минимальная ширина маркера в пикселях 

        /// </summary> 

        public int MinMarkerWid  

        { 

            get  

            { 

                return Ocr.GetMinMarkerWid(); 

            } 

            set 

            { 

                Ocr.SetMinMarkerWid(value); 

            }  

        } 

 

 

        /// <summary> 

        /// Максимальная ширина маркера в пикселях 

        /// </summary> 

        public int MaxMarkerWid  

        { 

            get  

            { 

                return Ocr.GetMaxMarkerWid(); 

            } 

            set 

            { 


                Ocr.SetMaxMarkerWid(value); 

            }  

        } 

 

 

        /// <summary> 

        /// Минимальная высота маркера в пикселях 

        /// </summary> 

        public int MinMarkerHgh  

        { 

            get  

            { 

                return Ocr.GetMinMarkerHgh(); 

            } 

            set 

            { 

                Ocr.SetMinMarkerHgh(value); 

            }  

        } 

 

 

        /// <summary> 

        /// Максимальная высота маркера в пикселях 

        /// </summary> 

        public int MaxMarkerHgh  

        { 

            get  

            { 

                return Ocr.GetMaxMarkerHgh(); 

            } 

            set 

            { 

                Ocr.SetMaxMarkerHgh(value); 

            }  

        } 

 

 

        /// <summary> 

        /// Минимальное отношение высота/ширина для маркера 

        /// </summary> 

        public double MinMarkerRio  

        { 

            get  

            { 

                return Ocr.GetMaxMarkerRio(); 

            } 

            set 

            { 

                Ocr.SetMaxMarkerRio(value); 

            }  


        } 

 

 

        /// <summary> 

        /// Максимальное отношение высота/ширина для маркера 

        /// </summary> 

        public double MaxMarkerRio  

        { 

            get  

            { 

                return Ocr.GetMaxMarkerRio(); 

            } 

            set 

            { 

                Ocr.SetMaxMarkerRio(value); 

            }  

        } 

 

 

        /// <summary> 

        /// Начало зоны контроля пустого листа (не бюллетеня) в пикселях 

        /// </summary> 

        public int BlankTestStart  

        { 

            get  

            { 

                return Ocr.GetBlankTestStart(); 

            } 

            set 

            { 

                Ocr.SetBlankTestStart(value); 

            }  

        } 

 

 

        /// <summary> 

        /// Конец зоны контроля пустого листа (не бюллетеня) в пикселях 

        /// </summary> 

        public int BlankTestStop  

        { 

            get  

            { 

                return Ocr.GetBlankTestStop(); 

            } 

            set 

            { 

                Ocr.SetBlankTestStop(value); 

            }  

        } 

 


 
        /// <summary> 

        /// Конец зоны контроля пустого листа (не бюллетеня) в пикселях 

        /// </summary> 

        public int MinCheckArea  

        { 

            get  

            { 

                return Ocr.GetMinCheckArea(); 

            } 

            set 

            { 

                Ocr.SetMinCheckArea(value); 

            }  

        } 

 

 

        /// <summary> 

        /// Минимальная площадь засчитываемой печати в пикселях 

        /// </summary> 

        public int StampLowThr  

        { 

            get  

            { 

                return Ocr.GetStampOK_Low_Threshold_Area(); 

            } 

            set 

            { 

                Ocr.SetStampOK_Low_Threshold_Area(value); 

            }  

        } 

 

 

        // Экспортируемые функции для изменения параметров печати. 

        // Могут быть вызваны после InitRecognition в любой момент перед 

        // распознаванием печати. 

 

 

        /// <summary> 

        /// Средний горизонтальный размер цифры номера печати в пикселях 

        /// </summary> 

        public int StampDigitXsize  

        { 

            get  

            { 

                return Ocr.GetStampDigitXsize(); 

            } 

            set 

            { 

                Ocr.SetStampDigitXsize(value); 


            }  

        } 

 

 

        /// <summary> 

        /// Средний вертикальный размер цифры номера печати в пикселях 

        /// </summary> 

        public int StampDigitYsize  

        { 

            get  

            { 

                return Ocr.GetStampDigitYsize(); 

            } 

            set 

            { 

                Ocr.SetStampDigitYsize(value); 

            }  

        } 

 

 

        /// <summary> 

        /// Минимальная ширина линии цифры номера печати в пикселях 

        /// </summary> 

        public int StampDigitMinLineWidth  

        { 

            get  

            { 

                return m_lStampMinLineWidth; 

            } 

            set 

            { 

                m_lStampMinLineWidth = value; 

            }  

        } 

 

 

        /// <summary> 

        /// Максмальная ширина линии цифры номера печати в пикселях 

        /// </summary> 

        public int StampDigitMaxLineWidth  

        { 

            get  

            { 

                return m_lStampMaxLineWidth; 

            } 

            set 

            { 

                m_lStampMaxLineWidth = value; 

            }  

        } 


 
 

        /// <summary> 

        /// Средний размер белого промежутка между цифрами номера печати в пикселях 

        /// </summary> 

        public int StampDigitGap  

        { 

            get  

            { 

                return Ocr.GetStampDigitGap(); 

            } 

            set 

            { 

                Ocr.SetStampDigitGap(value); 

            }  

        } 

 

 

        /// <summary> 

        /// Среднее расстояние в пикселях от центра цифр печати до нижней линии рамки печати 

        /// </summary> 

        public int StampDigitDistBotLine  

        { 

            get  

            { 

                return Ocr.GetStampDigitDistBotLine(); 

            } 

            set 

            { 

                Ocr.SetStampDigitDistBotLine(value); 

            }  

        } 

 

 

        /// <summary> 

        /// Среднее расстояние в пикселях от центра левой цифры печати до средины левой линии рамки 

        /// </summary> 

        public int StampDigitDistLftLine  

        { 

            get  

            { 

                return Ocr.GetStampDigitDistLftLine(); 

            } 

            set 

            { 

                Ocr.SetStampDigitDistLftLine(value); 

            }  

        } 

 

 


        /// <summary> 

        /// Среднее расстояние в пикселях от центра правой цифры печати до средины правой линии рамки 

        /// </summary> 

        public int StampDigitDistRghLine  

        { 

            get  

            { 

                return Ocr.GetStampDigitDistRghLine(); 

            } 

            set 

            { 

                Ocr.SetStampDigitDistRghLine(value); 

            }  

        } 

 

 

        /// <summary> 

        /// Среднее расстояние в пикселях от центра левой линии рамки печати до центра правой 

        /// </summary> 

        public int StampFrameWidth  

        { 

            get  

            { 

                return Ocr.GetStampFrameWidth(); 

            } 

            set 

            { 

                Ocr.SetStampFrameWidth(value); 

            }  

        } 

 

 

        /// <summary> 

        /// Включение/отключение механизма отсева одной слабой лишней метки 

        /// </summary> 

        public bool CutWeakCheck  

        { 

            get  

            { 

                return m_bIgnoreOneExtraCheck; 

            } 

            set 

            { 

                m_bIgnoreOneExtraCheck = value; 

                Ocr.SetIgnoreOneExtraCheck(m_bIgnoreOneExtraCheck.ToInt()); 

            }  

        } 

 

 

        /// <summary> 


        /// проверка наличия нижней линии каждой правой секции бюллетня 

        /// </summary> 

        public bool SeekBottomRightLine  

        { 

            get  

            { 

                return m_bSeekBottom; 

            } 

            set 

            { 

                if( value != m_bSeekBottom)	 

                { 

                    m_bSeekBottom = value; 

                    if(m_bSeekBottom)  

                    { 

                        Ocr.EnableSectBotLineTest(1); 

                    }  

                    else 

                    { 

                        Ocr.EnableSectBotLineTest(0); 

                    } 

                } 

            }  

        } 

 

 

        /// <summary> 

        /// вертикальный размер анализируемой зоны печати 

        /// </summary> 

        public int StampVSize  

        { 

            get  

            { 

                return m_nStampVSize; 

            } 

            set 

            { 

                m_nStampVSize = value; 

                Ocr.SetStampZoneVertSize(value); 

            }  

        } 

 

 

        /// <summary> 

        /// Искать ли потерянный квадрат 

        /// </summary> 

        public bool LookForLostSquare  

        { 

            get  

            { 


                return m_bLookForLostSquare; 

            } 

            set 

            { 

                m_bLookForLostSquare = value; 

 

 

                if (m_bLookForLostSquare)  

                { 

                    Ocr.SetLook4LostSquare(1); 

                } 

                else  

                { 

                    Ocr.SetLook4LostSquare(0); 

                } 

            }  

        } 

 

 

        /// <summary> 

        /// Количество запусков распознавания 

        /// </summary> 

        public int RunRecCount  

        { 

            get  

            { 

                return g_lRunRecCount; 

            } 

        } 

 

 

        /// <summary> 

        /// Включает логгирование распознавания в указанный файл 

        /// </summary> 

        /// <param name="sLogFileName">Имя файла</param> 

        public void EnableLogging(string sLogFileName) 

        { 

            // Включаю логгирование распознавания 

            Ocr.EnableLoggingInternal(sLogFileName); 

        } 

 

 

        /// <summary> 

        /// Минимальная ширина маркера в пикселях 

        /// </summary> 

        public int MinStandartMarkerWid  

        { 

            get  

            { 

                return Ocr.GetMinStandartMarkerWid(); 


            } 

            set 

            { 

                Ocr.SetMinStandartMarkerWid(value); 

            }  

        } 

 

 

        /// <summary> 

        /// Максимальная ширина маркера в пикселях 

        /// </summary> 

        public int MaxStandartMarkerWid  

        { 

            get  

            { 

                return Ocr.GetMaxStandartMarkerWid(); 

            } 

            set 

            { 

                Ocr.SetMaxStandartMarkerWid(value); 

            }  

        } 

 

 

        /// <summary> 

        /// Минимальная высота маркера в пикселях 

        /// </summary> 

        public int MinStandartMarkerHgh  

        { 

            get  

            { 

                return Ocr.GetMinStandartMarkerHgh(); 

            } 

            set 

            { 

                Ocr.SetMinStandartMarkerHgh(value); 

            }  

        } 

 

 

        /// <summary> 

        /// Максимальная высота маркера в пикселях 

        /// </summary> 

        public int MaxStandartMarkerHgh  

        { 

            get  

            { 

                return Ocr.GetMaxStandartMarkerHgh(); 

            } 

            set 


            { 

                Ocr.SetMaxStandartMarkerHgh(value); 

            }  

        } 

 

 

        /// <summary> 

        /// Зона поиска маркера в пикселях 

        /// </summary> 

        public int StandartMarkerZone  

        { 

            get  

            { 

                return Ocr.GetStandartMarkerZone(); 

            } 

            set 

            { 

                Ocr.SetStandartMarkerZone(value); 

            }  

        } 

 

 

		/// <summary> 

		/// Смещение в буфере для первой линейки 

		/// </summary> 

		public int OffsetFirstRule 

		{ 

			get  

			{ 

				return Ocr.GetOffsetFirstRule(); 

			} 

			set 

			{ 

				Ocr.SetOffsetFirstRule(value); 

			}  

		} 

 

 

		/// <summary> 

        /// Обработчик обратных вызовов из распознавалки 

        /// </summary> 

        /// <param name="code">Код запроса</param> 

        /// <param name="data">Данные</param> 

        /// <param name="size">Размер данных</param> 

        /// <returns>Результат выполнения</returns> 

        int OcrCallBackHandler(OcrCallBackType cbType, IntPtr data, int size) 

        { 

			try 

			{ 

				DebugOut("CALLBACK: " + cbType + " [" + data.ToInt32().ToString("X") + ", " + size + "]"); 


				switch (cbType) 

				{ 

                    case OcrCallBackType.ModelSave: 

						{ 

							// data - бинарное представление модели 

							// size - размер данных 

							if (size > 0 && data != IntPtr.Zero) 

							{ 

								try  

								{ 

									if( File.Exists(ModelFilePath) ) 

                                        File.Delete(ModelFilePath); 

 

 

									byte[] modelData = new byte[size + 1]; 

									Marshal.Copy(data, modelData, 0, size); 

                                    FileStream fs = File.Create(ModelFilePath); 

									fs.Write(modelData, 0, size); 

									fs.Flush(); 

									fs.Close(); 

									// Успешное выполнение 

									return 1; 

								}  

								catch (Exception ex) 

								{ 

									SetError(ErrorCode.UnexpectedError, "CALLBACK: " + cbType + ": " + ex.ToString()); 

									// непредвиденная ошибка 

									return -2; 

								} 

							} 

							else 

							{ 

								SetError(ErrorCode.IllegalUse, "CALLBACK: " + cbType + ": неверные или неожиданные параметры [" + data.ToInt32().ToString("X") + ", " + size + "]"); 

								// неверные параметры 

								return -3; 

							} 

						} 

					case OcrCallBackType.GetModelFileSize: 

						{ 

							try  

							{ 

								FileInfo fi = new FileInfo(ModelFilePath); 

								return (int)fi.Length; 

							}  

							catch (Exception ex) 

							{ 

								SetError(ErrorCode.UnexpectedError, "CALLBACK: " + cbType + ": " + ex.ToString()); 

								// непредвиденная ошибка 

								return -2; 

							} 


						} 

 

 

					case OcrCallBackType.ModelRestore: 

						{ 

							// data - буфер для модели 

							// size - размер бинарного представления 

 

 

							// признак успешного завершения 

							int    retval = 1; 

 

 

                            FileInfo fi = new FileInfo(ModelFilePath); 

							if (fi.Length == size) 

							{ 

								byte[] modelData = new byte[size + 1]; 

                                FileStream fs = File.OpenRead(ModelFilePath); 

								fs.Read(modelData, 0, size); 

								fs.Close(); 

 

 

								Marshal.Copy(modelData, 0, data, size); 

							} 

							else 

							{ 

								SetError(ErrorCode.IllegalUse, "CALLBACK: " + cbType + ": длина файла не соответствует ожидаемой"); 

								// ошибка в параметрах 

								retval = -1; 

							} 

 

 

							return retval; 

						} 

 

 

					case OcrCallBackType.DataSave: 

						break; 

					case OcrCallBackType.DataRestore: 

						break; 

 

 

					case OcrCallBackType.GetStamp: 

						// size - номер номера комиссии в массиве 

						return GetStamp(size); 

					case OcrCallBackType.GetStampCount: 

						// получение количества печатей 

						return GetStampCount(); 

 

 


					case OcrCallBackType.GetStampMinLineWidth:			 

						// Получение данных о минимальной ширине линий печати  

						return GetStampMinLineWidth(); 

					case OcrCallBackType.GetStampMaxLineWidth:			 

						// Получение данных о максимальной ширине  

						return GetStampMaxLineWidth(); 

 

 

					case OcrCallBackType.GetStampTestLevel: 

						// Получение уровня контроля печаит 

						return GetStampTestLevel(); 

 

 

					case OcrCallBackType.GetSideResolution: 

						{ 

							// data - структура-описатель разрешения 

							// size - размер структуры 

							if (data == IntPtr.Zero)  

							{ 

								SetError(ErrorCode.IllegalUse, "CALLBACK: " + cbType + ": неверные или неожиданные параметры [" + data.ToInt32().ToString("X") + ", " + size + "]"); 

								// ошибка в параметрах 

								return -1; 

							} 

 

 

							Resolution s = (Resolution)Marshal.PtrToStructure(data, typeof(Resolution)); 

 

 

							if (s.side == 0) 

							{ 

								s.x = m_dpiX0; 

								s.y = m_dpiY0; 

								Marshal.StructureToPtr(s, data, true); 

							} 

							else if (s.side == 1) 

							{ 

								s.x = m_dpiX1; 

								s.y = m_dpiY1; 

								Marshal.StructureToPtr(s, data, true); 

							} 

							else  

							{ 

								SetError(ErrorCode.IllegalUse, "CALLBACK: " + cbType + ": неверные или неожиданные параметры [" + data.ToInt32().ToString("X") + ", " + size + "]"); 

								// ошибка в параметрах (неверно задана сторона) 

								return -1; 

							} 

						} 

 

 

						break; 


 
 

					case OcrCallBackType.PutBulletinNumber: 

						// size - номер бюллетеня 

						PutBulletinNumber(size); 

						break; 

 

 

					case OcrCallBackType.PutResults: 

						{ 

							// data - структура-описатель результатов распознавания 

							// size - размер структуры 

                            OcrResult s = (OcrResult)Marshal.PtrToStructure(data, typeof(OcrResult)); 

 

 

							DebugOut("s.IsValid = " + s.IsValid + ", s.numChecked = " + s.numChecked + ", s.PollNum = " + s.PollNum); 

							if (! PutResult(s))  

							{ 

								SetError(ErrorCode.UnexpectedError, "CALLBACK: " + cbType + ": не удалось сохранить результат распознавания"); 

								// ошибка при сохранении результатов 

								return -1; 

							} 

						} 

						break; 

 

 

					case OcrCallBackType.GetPath2Data:  

						{ 

							// data - буфер под имя каталога 

							if(m_bstrPath2RecognitionData == null || m_bstrPath2RecognitionData.Length == 0) 

							{ 

								data = IntPtr.Zero; 

							} 

							else  

							{ 

								Marshal.Copy(m_bstrPath2RecognitionData.ToCharArray(), 0, data, m_bstrPath2RecognitionData.Length); 

							} 

						} 

						break; 

 

 

					case OcrCallBackType.GetDigitSquares: 

						// не поддерживаем 

						return 0; 

 

 

					case OcrCallBackType.UnloadDigitOcrResult: 

						// не поддерживаем 

						return -1; 

 


 
					case OcrCallBackType.GetGrayRectBuffSize:		 

						//data - ALRECT 

						//size - сторона бюллетеня 

						if (data != IntPtr.Zero && (size == 0 || size == 1)) 

						{ 

                            AlRect r = (AlRect)Marshal.PtrToStructure(data, typeof(AlRect)); 

							int	nSize = 0; 

							if (r.y < 0 || r.h < 0 || r.x < 0 || r.w < 0) 

							{ 

								SetError(ErrorCode.IllegalUse, "CALLBACK: " + cbType + ": Запрошен недопустимый размер буфера полутона: x=" + r.x + ", y=" + r.y + ", w=" + r.w + ", h=" + r.h); 

								// ошибка в параметрах 

								return -1; 

							} 

							// Расчитываем величину буфера 

							nSize = r.h*r.w;	// количество пиксел 

 

 

							DebugOut("Вычислен размер полутонового буфера: size=" + nSize); 

 

 

							return nSize; 

						} 

						else 

						{ 

							SetError(ErrorCode.IllegalUse, "CALLBACK: " + cbType + ": неверные или неожиданные параметры [" + data.ToInt32().ToString("X") + ", " + size + "]"); 

							// ошибка в параметрах 

							return -1; 

						} 

 

 

					case OcrCallBackType.GetGrayRectImage:	 

						//data - ALSUBF (описатель полутона) 

						//size - сторона бюллетеня 

						if (data != IntPtr.Zero && (size == 0 || size == 1)) 

						{ 

                            AlSubf s = (AlSubf)Marshal.PtrToStructure(data, typeof(AlSubf)); 

 

 

							if (m_pEvents != null) 

							{ 

                                MemoryBlock piMem = new MemoryBlock(s.@base); 

								try  

								{ 

									long nSize = m_pEvents.GetHalfToneBuffer(this, (short)size, s.x, s.y, s.ys, s.xs, piMem); 

									if (nSize == -1)  

									{ 

										DebugOut("GetGrayRectImage: Unable to get image"); 

										// ошибка в параметрах 

										return -1; 


									} 

								}  

								catch (Exception ex) 

								{ 

									SetError(ErrorCode.UnexpectedError, "CALLBACK: " + cbType + ": " + ex); 

									// неожиданная ошибка 

									return -1; 

								} 

							}  

							else 

							{ 

								SetError(ErrorCode.IllegalUse, "CALLBACK: " + cbType + ": не установлен обработчик события GetHalfToneBuffer"); 

								// отсутствует обработчик 

								return -1; 

							} 

							s.width = s.xs; 

							// успешно выполнено 

							return 1; 

						} 

						else  

						{ 

							// неверные параметры 

							return -1; 

						} 

 

 

					case OcrCallBackType.GetBinThreshold:		 

						// size - сторона бюллетеня 

						if (size == 0 || size == 1) 

						{ 

							try  

							{ 

								if (m_pEvents != null) 

								{ 

									int nThr = m_pEvents.GetBinaryThreshold(this, (short)size); 

									return nThr; 

								}  

								else 

								{ 

									SetError(ErrorCode.IllegalUse, "CALLBACK: " + cbType + ": не установлен обработчик события GetBinaryThreshold"); 

									// отсутствует обработчик 

									return -1; 

								} 

							}  

							catch (Exception ex) 

							{ 

								SetError(ErrorCode.UnexpectedError, "CALLBACK: " + cbType + ": " + ex.ToString()); 

								// неожиданная ошибка 

								return -1; 

							} 


						} 

						else  

						{ 

							SetError(ErrorCode.IllegalUse, "CALLBACK: " + cbType + ": неверные или неожиданные параметры [" + data.ToInt32().ToString("X") + ", " + size + "]"); 

							// неверные параметры 

							return -1; 

						} 

 

 

					case OcrCallBackType.ReportProgress: 

						// отладочное сообщение 

						// size - стадия распознавания  

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

 

 

            // возвратим код успешного выполнения 

            return 1; 

        } 

 

 

        /// <summary> 

        /// Максимальная ширина линии печати 

        /// </summary> 

        /// <returns></returns> 

        int GetStampMaxLineWidth() 

        { 

            return m_lStampMaxLineWidth; 

        } 

 

 

        /// <summary> 

        /// Минимальная ширина линии печати 


        /// </summary> 

        /// <returns></returns> 

        int GetStampMinLineWidth() 

        { 

            return m_lStampMinLineWidth; 

        } 

 

 

        /// <summary> 

        /// Уровень анализа печати 

        /// </summary> 

        /// <returns>Уровень анализа печати</returns> 

        int	GetStampTestLevel() 

        { 

            return (int)m_sStampTestLevel; 

        } 

 

 

        /// <summary> 

        /// Установка номера бюллетеня (внутренняя функция) 

        /// </summary> 

        /// <param name="nBN">номер бюллетеня</param> 

        void PutBulletinNumber(int nBN) 

        { 

            // Чистим результаты предыдущего распознавания 

            m_pollResults.Clear(); 

 

 

            m_nBulletinNumber = nBN; 

        } 

 

 

        /// <summary> 

        /// Запись результатов распознавания бюллетеня во внутреннее состояние класса 

        /// </summary> 

        /// <param name="results">Результаты от распознавлки</param> 

        /// <returns>Признак успешного анализа результатов</returns> 

        private bool PutResult(OcrResult results) 

        { 

            PollResult pr = new PollResult(); 

            pr.PollNumber = results.PollNum; 

            pr.IsValid = (results.IsValid > 0); 

 

 

            int[] sqData= new int[results.numChecked]; 

            Marshal.Copy(results.sqData, sqData, 0, results.numChecked); 

            for (int i=0; i < results.numChecked; i++) 

            { 

                pr.Add(sqData[i]); 

            } 


 
 

            m_pollResults.Add(pr); 

 

 

            return true; 

        } 

 

 

        /// <summary> 

        /// Установить ошибку распознавания 

        /// </summary> 

        /// <param name="code">Код ошибки</param> 

        /// <param name="message">Описание</param> 

        void SetError(ErrorCode code, string message) 

        { 

            if (m_pEvents != null) 

            { 

                m_pEvents.Error(this, (int)code, message); 

            } 

        } 

 

 

        /// <summary> 

        /// Отладочное сообщение от распознавалки 

        /// </summary> 

        /// <param name="message">Сообщение</param> 

        void DebugOut(string message) 

        { 

            if (m_pEvents != null) 

            { 

                m_pEvents.AppendToLog(this, message); 

            } 

        } 

 

 

        /// <summary> 

        /// Возвращает текстовое описание результата проверки геометрии 

        /// </summary> 

        /// <param name="geoData">Результат проверки</param> 

        /// <param name="nSquareCount">Колво квадратов</param> 

        /// <returns>Описание</returns> 

        public static string GetGeoResultMessage(GeoData geoData, long nSquareCount) 

        { 

            // если возвращено, что все хорошо, то нужен доп анализ 

            if ((GeoResult)geoData.result == GeoResult.OK) 

            { 

                // общее максимальное отклонение 

                long nMaxSquareSkew = 0; 

                // максимальное и минимальное отклонение квадратов по горизонтали 


                long nMaxSquareSkewH = 0, nMinSquareSkewH = Int32.MaxValue; 

                // максимальное и минимальное отклонение квадратов по размерам 

                long nMaxSquareSizeSkew = 0, nMinSquareSizeSkew = Int32.MaxValue; 

                // макс и мин значения интенсивности цвета 

                int nMinColor = Int32.MaxValue, nMaxColor = 0; 

 

 

                nMinColor = nMaxColor = geoData.topMarkerColor; 

                nMinColor = Math.Min(nMinColor, geoData.bottomMarkerColor); 

                nMaxColor = Math.Max(nMaxColor, geoData.bottomMarkerColor); 

 

 

                nMinColor = Math.Min(nMinColor, geoData.baseLineColor); 

                nMaxColor = Math.Max(nMaxColor, geoData.baseLineColor); 

 

 

                // по всем найденным квадратам 

                for (int i = 0; i < nSquareCount; i++) 

                { 

                    if (geoData.squares[i] == 1) 

                    { 

                        nMaxSquareSkew = Math.Max(nMaxSquareSkew, geoData.squaresSkewV[i]); 

                        nMaxSquareSkew = Math.Max(nMaxSquareSkew, geoData.squaresSkewV[i]); 

                        nMaxSquareSkewH = Math.Max(nMaxSquareSkewH, geoData.squaresSkewH[i]); 

                        nMinSquareSkewH = Math.Min(nMinSquareSkewH, geoData.squaresSkewH[i]); 

                        nMaxSquareSizeSkew = Math.Max(nMaxSquareSizeSkew, geoData.squaresSize[i]); 

                        nMinSquareSizeSkew = Math.Min(nMinSquareSizeSkew, geoData.squaresSize[i]); 

                        nMinColor = Math.Min(nMinColor, geoData.squaresColor[i]); 

                        nMaxColor = Math.Max(nMaxColor, geoData.squaresColor[i]); 

                    } 

                } 

 

 

                if (nMinSquareSizeSkew == Int32.MaxValue) 

                { 

                    // коррекция на случай, если не искали квадраты 

                    nMinSquareSizeSkew = nMaxSquareSizeSkew; 

                } 

 

 

                // допускается общее отклонение квадратов не более трех мм 

                if (nMaxSquareSkew > 3 || 

                    // не допускается разброс в отклонении квадратов по горизонтали больше 2х мм 

                    nMaxSquareSkewH != nMinSquareSkewH && nMaxSquareSkewH > 2 || 

                    // нет отклонений по размерам квадратов 

                    nMaxSquareSizeSkew != 0) 

                { 

                    return "Некорректное расположение квадратов"; 

                } 

 


 
                if (nMaxColor - nMinColor > 20 && nMaxColor > nMinColor * 2) 

                { 

                    return "Нарушена равномерность черного цвета"; 

                } 

 

 

                if (nMaxColor > 64) 

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

 

 

        /// <summary> 

        /// Бросает exception с кодом последней ошибки 

        /// </summary> 

        public static void ThrowLastError() 

        { 

            // Максимальная длина буфера с ошибкой 

            const int MAX_ERROR_MESSAGE_LENGTH = 1000; 

 

 

            // Описание ошибки 


            StringBuilder sDescr = new StringBuilder(MAX_ERROR_MESSAGE_LENGTH); 

 

 

            // Код возврата GetErrorDesc 

            int nRes = Ocr.GetErrorDesc(sDescr); 

            if (0 != nRes) 

                throw new Exception(String.Format("Ошибка Ocr. Описание получить не удалось: {0}", nRes)); 

            else 

                // Передаю ошибку наверх 

                throw new Exception(sDescr.ToString()); 

        } 

 

 

        /// <summary> 

        /// Полутоновая проверка печати 

        /// </summary> 

        /// <param name="sStampNumber">Наиболее вероятный номер</param> 

        /// <param name="sAlternatives">Альтернативы цифр по позициям</param> 

        /// <returns>Результат распознавания</returns> 

        public static StampResult IsStampOKGray(ref string sStampNumber, ref string[] sAlternatives) 

        { 

            // буфера под хранение альтернатив для 4х позиций номера печати 

            // распознавалка возвращает гарантированно не более трех альтернатив 

            byte[][] digits = new byte[Ocr.STAMP_DIGIT_COUNT][]; 

            for (int i = 0; i < Ocr.STAMP_DIGIT_COUNT; i++) 

            { 

                digits[i] = new byte[10]; 

            } 

 

 

            // полутоновое распознавание 

            var stampResult = (StampResult)Ocr.IsStampOKGray(digits[0], digits[1], digits[2], digits[3]); 

            // формируем альтернативы 

            for (int i = 0; i < Ocr.STAMP_DIGIT_COUNT; i++) 

            { 

                sAlternatives[i] = ""; 

                for (int j = 0; digits[i][j] != 0; j++) 

                { 

                    sAlternatives[i] += (char)digits[i][j]; 

                } 

                if (sAlternatives[i].Length == 0) 

                { 

                    sAlternatives[i] += "X"; 

                } 

                // наиболее вероятный номер печати 

                sStampNumber += sAlternatives[i][0]; 

            } 

 

 

            return stampResult; 


        } 

 

 

        #region DllImport-ы из Ocr.dll 

 

 

        /// <summary> 

        /// Функция инициализации модели бюллетеня 

        /// </summary> 

        /// <param name="iBulCount">Количество бюллетеней</param> 

        /// <param name="iBulCodeType"> 

        /// Тип цифрового маркера: 

        /// 0 - не цифровой 

        /// 1 - одна цифра 

        /// 2 - две цифры 

        /// </param> 

        /// <returns></returns> 

        /// <remarks> 

        /// Соотв НЕ МОЖЕМ одновременно распознавать: 

        /// а) цифровой и штрих маркер 

        /// б) одноцифровой и двухцифровой маркеры 

        /// </remarks> 

        [DllImport("Xib.dll")] 

        internal extern static int InitModel(int iBulCount, int iBulCodeType); 

 

 

        /// <summary> 

        /// Функция создания модели бюллетеня 

        /// </summary> 

        /// <param name="sData">Текстовое описание модели</param> 

        /// <param name="iNum">Номер бюллетеня по порядку</param> 

        /// <param name="iCode">Номер маркера</param> 

        /// <returns>1 - в случае успешной компиляции модели</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int createBallotModel(byte[] sData, int iNum, int iCode); 

 

 

        /// <summary> 

        /// Функция установки данных голосования 

        /// </summary> 

        /// <param name="iBallot">Номер маркера</param> 

        /// <param name="iPoll">Номер секции</param> 

        /// <param name="pData">Данные о голосах</param> 

        /// <returns>1 - в случае успеха</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetPollData(int iBallot, int iPoll, ref PollData pData); 

 

 

        /// <summary> 

        /// Обнуление параметров проверки печати 


        /// </summary> 

        [DllImport("Xib.dll")] 

        internal extern static int SetDefaultStampGeometry(); 

 

 

        /// <summary> 

        /// Получить описание последней ошибки 

        /// </summary> 

        /// <param name="sError">Описание ошибки</param> 

        /// <returns>1 - в случае успеха</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetErrorDesc(StringBuilder sError); 

 

 

        /// <summary> 

        /// Сбрасывает ошибку 

        /// </summary> 

        [DllImport("Xib.dll")] 

        internal extern static int ClearError(); 

 

 

        /// <summary> 

        /// Сохраняет модель в виде текста 

        /// </summary> 

        /// <param name="sText">Буфер под текст модели</param> 

        /// <param name="nSize">Размер буфера в байтах</param> 

        /// <returns>1 - в случае успеха</returns> 

        [DllImport("Xib.dll", CharSet = CharSet.Ansi)] 

        internal extern static int SaveAsText(StringBuilder sText, int nSize); 

 

 

        /// <summary> 

        /// Уровень online-распознавания бюллетеня 

        /// </summary> 

        /// <returns>Уровень</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetFeatureOnlineTest(); 

 

 

        /// <summary> 

        /// Установить уровень onlline-распознавания бюллетеня 

        /// </summary> 

        /// <param name="val">Уровень</param> 

        /// <returns>1 - в случае успеха</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetFeatureOnlineTest(int val); 

 

 

        /// <summary> 

        /// Инициализация распознавалки 


        /// </summary> 

        /// <param name="cbFunc">Функция обратного вызова для взаимодействия с распознавалкой</param> 

        /// <returns>предыдущая функция обратного вызова</returns> 

        [DllImport("Xib.dll")] 

        internal extern static OcrCallBack OCR_Initialize(OcrCallBack callback); 

 

 

        /// <summary> 

        /// Включает отладочный протокол распознавалки 

        /// </summary> 

        /// <param name="FileName">Файл, в который записывается протокол</param> 

        [DllImport("Xib.dll", EntryPoint = "EnableLogging")] 

        internal extern static void EnableLoggingInternal(string FileName); 

 

 

        /// <summary> 

        /// Завершение процесса распознавания 

        /// </summary> 

        /// <param name="DoIt">Тип маркера</param> 

        /// <param name="y0">Кол во строк по первой стороне</param> 

        /// <param name="y1">Кол во строк по второй стороне</param> 

        /// <returns>результат распознавания</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int EndRecognition(MarkerType DoIt, int y0, int y1); 

 

 

        /// <summary> 

        /// Функция проверки геометрии бюллетеня 

        /// </summary> 

        /// <param name="y0">Кол во строк по первой стороне</param> 

        /// <param name="y1">Кол во строк по второй стороне</param> 

        /// <param name="pData">Структура с результатами проверки</param> 

        /// <returns>результат проверки геометрии</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int TestBallot(int y0, int y1, ref GeoData pData); 

 

 

        /// <summary> 

        /// Установить начало зоны контроля пустого листа (не бюллетеня) в пикселях 

        /// </summary> 

        /// <param name="val">Начало зоны контроля пустого листа (не бюллетеня) в пикселях</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetBlankTestStart(int val); 

 

 

        /// <summary> 

        /// Начало зоны контроля пустого листа (не бюллетеня) в пикселях 

        /// </summary> 

        /// <returns>Начало зоны контроля пустого листа (не бюллетеня) в пикселях</returns> 


        [DllImport("Xib.dll")] 

        internal extern static int GetBlankTestStart(); 

 

 

        /// <summary> 

        /// Установить конец зоны контроля пустого листа (не бюллетеня) в пикселях 

        /// </summary> 

        /// <param name="val">Конец зоны контроля пустого листа (не бюллетеня) в пикселях</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetBlankTestStop(int val); 

 

 

        /// <summary> 

        /// Конец зоны контроля пустого листа (не бюллетеня) в пикселях 

        /// </summary> 

        /// <returns>Конец зоны контроля пустого листа (не бюллетеня) в пикселях</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetBlankTestStop(); 

 

 

        /// <summary> 

        /// Включение/отключение механизма отсева одной слабой лишней метки 

        /// </summary> 

        /// <param name="val">1 - вкл/ 0 -выкл</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetIgnoreOneExtraCheck(int val); 

 

 

        /// <summary> 

        /// Искать ли потерянный квадрат 

        /// </summary> 

        /// <param name="val">1 - вкл/ 0 -выкл</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetLook4LostSquare(int val); 

 

 

        /// <summary> 

        /// Установить макс высоту маркера в пикселях 

        /// </summary> 

        /// <param name="val">Максимальная высота маркера в пикселях</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetMaxMarkerHgh(int val); 

 

 

        /// <summary> 

        /// Максимальная высота маркера в пикселях 


        /// </summary> 

        /// <returns>Максимальная высота маркера в пикселях</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetMaxMarkerHgh(); 

 

 

        /// <summary> 

        /// Минимальное отношение высота/ширина для маркера 

        /// </summary> 

        /// <param name="val">Минимальное отношение высота/ширина для маркера</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static double SetMaxMarkerRio(double val); 

 

 

        /// <summary> 

        /// Минимальное отношение высота/ширина для маркера 

        /// </summary> 

        /// <returns>Минимальное отношение высота/ширина для маркера</returns> 

        [DllImport("Xib.dll")] 

        internal extern static double GetMaxMarkerRio(); 

 

 

        /// <summary> 

        /// Максимальная ширина маркера в пикселях 

        /// </summary> 

        /// <param name="val">Максимальная ширина маркера в пикселях</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetMaxMarkerWid(int val); 

 

 

        /// <summary> 

        /// Максимальная ширина маркера в пикселях 

        /// </summary> 

        /// <returns>Максимальная ширина маркера в пикселях</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetMaxMarkerWid(); 

 

 

        /// <summary> 

        /// Максимально допустимое значение перекоса 

        /// </summary> 

        /// <param name="val">Максимально допустимое значение перекоса</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetMaxOnlineSkew(int val); 

 

 

        /// <summary> 


        /// Максимально допустимое значение перекоса 

        /// </summary> 

        /// <returns>Максимально допустимое значение перекоса</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetMaxOnlineSkew(); 

 

 

        /// <summary> 

        /// Максимальная высота маркера в пикселях 

        /// </summary> 

        /// <param name="val">Максимальная высота маркера в пикселях</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetMaxStandartMarkerHgh(int val); 

 

 

        /// <summary> 

        /// Максимальная высота маркера в пикселях 

        /// </summary> 

        /// <returns>Максимальная высота маркера в пикселях</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetMaxStandartMarkerHgh(); 

 

 

        /// <summary> 

        /// Максимальная ширина маркера в пикселя 

        /// </summary> 

        /// <param name="val">Максимальная ширина маркера в пикселя</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetMaxStandartMarkerWid(int val); 

 

 

        /// <summary> 

        /// Максимальная ширина маркера в пикселя 

        /// </summary> 

        /// <returns>Максимальная ширина маркера в пикселя</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetMaxStandartMarkerWid(); 

 

 

        /// <summary> 

        /// Конец зоны контроля пустого листа (не бюллетеня) в пикселях 

        /// </summary> 

        /// <param name="val">Конец зоны контроля пустого листа (не бюллетеня) в пикселях</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetMinCheckArea(int val); 

 

 


        /// <summary> 

        /// Конец зоны контроля пустого листа (не бюллетеня) в пикселях 

        /// </summary> 

        /// <returns>Конец зоны контроля пустого листа (не бюллетеня) в пикселях</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetMinCheckArea(); 

 

 

        /// <summary> 

        /// Минимальная высота маркера в пикселях 

        /// </summary> 

        /// <param name="val">Минимальная высота маркера в пикселях</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetMinMarkerHgh(int val); 

 

 

        /// <summary> 

        /// Минимальная высота маркера в пикселях 

        /// </summary> 

        /// <returns>Минимальная высота маркера в пикселях</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetMinMarkerHgh(); 

 

 

        /// <summary> 

        /// Минимальное отношение высота/ширина для маркера 

        /// </summary> 

        /// <param name="val">Минимальное отношение высота/ширина для маркера</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static double SetMinMarkerRio(double val); 

 

 

        /// <summary> 

        /// Минимальное отношение высота/ширина для маркера 

        /// </summary> 

        /// <returns>Минимальное отношение высота/ширина для маркера</returns> 

        [DllImport("Xib.dll")] 

        internal extern static double GetMinMarkerRio(); 

 

 

        /// <summary> 

        /// Минимальная ширина маркера в пикселях 

        /// </summary> 

        /// <param name="val">Минимальная ширина маркера в пикселях</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetMinMarkerWid(int val); 

 


 
        /// <summary> 

        /// Минимальная ширина маркера в пикселях 

        /// </summary> 

        /// <returns>Минимальная ширина маркера в пикселях</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetMinMarkerWid(); 

 

 

        /// <summary> 

        /// Минимальная высота маркера в пикселях 

        /// </summary> 

        /// <param name="val">Минимальная высота маркера в пикселях</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetMinStandartMarkerHgh(int val); 

 

 

        /// <summary> 

        /// Минимальная высота маркера в пикселях 

        /// </summary> 

        /// <returns>Минимальная высота маркера в пикселях</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetMinStandartMarkerHgh(); 

 

 

        /// <summary> 

        /// Минимальная ширина маркера в пикселях 

        /// </summary> 

        /// <param name="val">Минимальная ширина маркера в пикселях</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetMinStandartMarkerWid(int val); 

 

 

        /// <summary> 

        /// Минимальная ширина маркера в пикселях 

        /// </summary> 

        /// <returns>Минимальная ширина маркера в пикселях</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetMinStandartMarkerWid(); 

 

 

        /// <summary> 

        /// проверка наличия нижней линии каждой правой секции бюллетня 

        /// </summary> 

        /// <param name="val">проверка наличия нижней линии каждой правой секции бюллетня</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int EnableSectBotLineTest(int val); 


 
 

        /// <summary> 

        /// Смещение в буфере для первой линейки 

        /// </summary> 

        /// <returns>Смещение в буфере для первой линейки</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetOffsetFirstRule(); 

 

 

        /// <summary> 

        /// Смещение в буфере для первой линейки 

        /// </summary> 

        /// <param name="val">Смещение в буфере для первой линейки</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetOffsetFirstRule(int val); 

 

 

        /// <summary> 

        /// Среднее расстояние в пикселях от центра цифр печати до нижней линии рамки печати 

        /// </summary> 

        /// <param name="val">Среднее расстояние в пикселях от центра цифр печати до нижней линии рамки печати</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetStampDigitDistBotLine(int val); 

 

 

        /// <summary> 

        /// Среднее расстояние в пикселях от центра цифр печати до нижней линии рамки печати 

        /// </summary> 

        /// <returns>Среднее расстояние в пикселях от центра цифр печати до нижней линии рамки печати</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetStampDigitDistBotLine(); 

 

 

        /// <summary> 

        /// Среднее расстояние в пикселях от центра левой цифры печати до средины левой линии рамки 

        /// </summary> 

        /// <param name="val">Среднее расстояние в пикселях от центра левой цифры печати до средины левой линии рамки</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetStampDigitDistLftLine(int val); 

 

 

        /// <summary> 

        /// Среднее расстояние в пикселях от центра левой цифры печати до средины левой линии рамки 

        /// </summary> 

        /// <returns>Среднее расстояние в пикселях от центра левой цифры печати до средины левой линии рамки</returns> 

        [DllImport("Xib.dll")] 


        internal extern static int GetStampDigitDistLftLine(); 

 

 

        /// <summary> 

        /// Среднее расстояние в пикселях от центра правой цифры печати до средины правой линии рамки 

        /// </summary> 

        /// <param name="val">Среднее расстояние в пикселях от центра правой цифры печати до средины правой линии рамки</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetStampDigitDistRghLine(int val); 

 

 

        /// <summary> 

        /// Среднее расстояние в пикселях от центра правой цифры печати до средины правой линии рамки 

        /// </summary> 

        /// <returns>Среднее расстояние в пикселях от центра правой цифры печати до средины правой линии рамки</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetStampDigitDistRghLine(); 

 

 

        /// <summary> 

        /// Средний размер белого промежутка между цифрами номера печати в пикселях 

        /// </summary> 

        /// <param name="val">Средний размер белого промежутка между цифрами номера печати в пикселях</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetStampDigitGap(int val); 

 

 

        /// <summary> 

        /// Средний размер белого промежутка между цифрами номера печати в пикселях 

        /// </summary> 

        /// <returns>Средний размер белого промежутка между цифрами номера печати в пикселях</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetStampDigitGap(); 

 

 

        /// <summary> 

        /// Средний горизонтальный размер цифры номера печати в пикселях 

        /// </summary> 

        /// <param name="val">Средний горизонтальный размер цифры номера печати в пикселях</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetStampDigitXsize(int val); 

 

 

        /// <summary> 

        /// Средний горизонтальный размер цифры номера печати в пикселях 

        /// </summary> 

        /// <returns>Средний горизонтальный размер цифры номера печати в пикселях</returns> 


        [DllImport("Xib.dll")] 

        internal extern static int GetStampDigitXsize(); 

 

 

        /// <summary> 

        /// Средний вертикальный размер цифры номера печати в пикселях 

        /// </summary> 

        /// <param name="val">Средний вертикальный размер цифры номера печати в пикселях</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetStampDigitYsize(int val); 

 

 

        /// <summary> 

        /// Средний вертикальный размер цифры номера печати в пикселях 

        /// </summary> 

        /// <returns>Средний вертикальный размер цифры номера печати в пикселях</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetStampDigitYsize(); 

 

 

        /// <summary> 

        /// Среднее расстояние в пикселях от центра левой линии рамки печати до центра правой 

        /// </summary> 

        /// <param name="val">Среднее расстояние в пикселях от центра левой линии рамки печати до центра правой</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetStampFrameWidth(int val); 

 

 

        /// <summary> 

        /// Среднее расстояние в пикселях от центра левой линии рамки печати до центра правой 

        /// </summary> 

        /// <returns>Среднее расстояние в пикселях от центра левой линии рамки печати до центра правой</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetStampFrameWidth(); 

 

 

        /// <summary> 

        /// Минимальная площадь засчитываемой печати в пикселях 

        /// </summary> 

        /// <param name="val">Минимальная площадь засчитываемой печати в пикселях</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetStampOK_Low_Threshold_Area(int val); 

 

 

        /// <summary> 

        /// Минимальная площадь засчитываемой печати в пикселях 

        /// </summary> 


        /// <returns>Минимальная площадь засчитываемой печати в пикселях</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetStampOK_Low_Threshold_Area(); 

 

 

        /// <summary> 

        /// Бинарное распознавние печати 

        /// </summary> 

        /// <returns>результат распознавания</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int IsStampOK(); 

 

 

        /// <summary> 

        /// Полутоновое распознавние печати 

        /// </summary> 

        /// <param name="digit1">альтернативы 1й позиции</param> 

        /// <param name="digit2">альтернативы 2й позиции</param> 

        /// <param name="digit3">альтернативы 3й позиции</param> 

        /// <param name="digit4">альтернативы 4й позиции</param> 

        /// <returns>результат распознавания</returns> 

        [DllImport("Xib.dll", CharSet = CharSet.Ansi)] 

        internal extern static int IsStampOKGray(byte[] digit1, byte[] digit2, byte[] digit3, byte[] digit4); 

 

 

        /// <summary> 

        /// вертикальный размер анализируемой зоны печати 

        /// </summary> 

        /// <param name="val">вертикальный размер анализируемой зоны печати</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetStampZoneVertSize(int val); 

 

 

        /// <summary> 

        /// Зона поиска маркера в пикселях 

        /// </summary> 

        /// <param name="val">Зона поиска маркера в пикселях</param> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int SetStandartMarkerZone(int val); 

 

 

        /// <summary> 

        /// Зона поиска маркера в пикселях 

        /// </summary> 

        /// <returns>Зона поиска маркера в пикселях</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetStandartMarkerZone(); 

 


 
        /// <summary> 

        /// [DEPRECATED] Снять квадрат с выборов 

        /// </summary> 

        /// <param name="ballot">Номер бюллетеня</param> 

        /// <param name="poll">Номер секции</param> 

        /// <param name="n">Номер квадрата</param> 

        /// <returns>1 - ок</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int ExcludeSquare(int ballot, int poll, int n); 

 

 

        /// <summary> 

        /// [DEPRECATED] Восстановить квадрат 

        /// </summary> 

        /// <param name="ballot">Номер бюллетеня</param> 

        /// <param name="poll">Номер секции</param> 

        /// <param name="n">Номер квадрата</param> 

        /// <returns>1 - ок</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int EnableSquare(int ballot, int poll, int n); 

 

 

        /// <summary> 

        /// [DEPRECATED] Проверить, не снят ли квадрат 

        /// </summary> 

        /// <param name="ballot">Номер бюллетеня</param> 

        /// <param name="poll">Номер секции</param> 

        /// <param name="n">Номер квадрата</param> 

        /// <returns>1 - ок, 0 - снят</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int IsSquareValid(int ballot, int poll, int n); 

 

 

        /// <summary> 

        /// Скомпилировать текстовую модель бюллетеня 

        /// </summary> 

        /// <returns>1 - ок</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int LinkChangedModel(); 

 

 

        /// <summary> 

        /// Начать распознавание 

        /// </summary> 

        /// <param name="pSide0">Параметры первой стороны</param> 

        /// <param name="pSide1">Параметры второй стороны</param> 

        /// <returns>меньше нуля - ошибка</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int StartRecognition(ref BufferHeader pSide0, ref BufferHeader pSide1); 


 
 

        /// <summary> 

        /// Начать online распознавание 

        /// </summary> 

        /// <param name="pSide0">Параметры первой стороны</param> 

        /// <param name="pSide1">Параметры второй стороны</param> 

        /// <param name="FrameLineDist_mm">Какой-то внутренний параметр распознавалки</param> 

        [DllImport("Xib.dll")] 

        internal extern static void StartOnLineTesting(ref BufferHeader pSide0, ref BufferHeader pSide1, int FrameLineDist_mm); 

 

 

        /// <summary> 

        /// Получить параметр для функции StartOnLineTesting 

        /// </summary> 

        /// <returns>Значение параметра</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int GetFrameDist_mm(); 

 

 

        /// <summary> 

        /// Сохранить модель 

        /// </summary> 

        /// <returns></returns> 

        [DllImport("Xib.dll")] 

        internal extern static int DefaultModelSave(); 

 

 

        /// <summary> 

        /// Обработать следующий буфер 

        /// </summary> 

        /// <param name="count">Количество строк</param> 

        /// <returns> 

        /// -1  - это не бюллетень  

        ///  0  - решение пока не принято  

        ///  1  - признан бюллетенем 

        /// </returns> 

        [DllImport("Xib.dll", EntryPoint = "NextBuffer")] 

        internal extern static int NextBufferInternal(int count); 

 

 

        /// <summary> 

        /// Определить номер цифрового маркера 

        /// </summary> 

        /// <returns>Номер маркера</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int OnlineDefStandartMarker(); 

 

 

        /// <summary> 


        /// Определить номер шрих-маркера 

        /// </summary> 

        /// <returns>Номер маркера</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int OnlineDefCharMarker(); 

 

 

        /// <summary> 

        /// Завершение работы распознавалки - освобождение ресурсов 

        /// </summary> 

        /// <returns>отрицательное число - код ошибки</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int CloseRecognition(); 

 

 

        /// <summary> 

        /// Инициализация распознавалки 

        /// </summary> 

        /// <returns>отрицательное число - код ошибки</returns> 

        [DllImport("Xib.dll")] 

        internal extern static int InitRecognition(); 

 

 

        #endregion 

    } 

}


