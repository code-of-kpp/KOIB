using System; 

using System.Text.RegularExpressions; 

using Croc.Bpc.Election.Voting; 

using Croc.Bpc.Recognizer.Ocr; 

 

 

namespace Croc.Bpc.Recognizer 

{ 

    /// <summary> 

    /// Результат распознавания одного бюллетеня 

    /// </summary> 

    public class RecognitionResult 

    { 

        #region Основные данные о результате распознавания 

        /// <summary> 

        /// Целочисленный исходный код результат распознавания 

        /// </summary> 

        public int IntResultCode 

        { 

            get; 

            private set; 

        } 

        /// <summary> 

        /// Код результат распознавания 

        /// </summary> 

        public OcrRecognitionResult ResultCode 

        { 

            get; 

            private set; 

        } 

        /// <summary> 

        ///	Описание результата распознавания 

        /// </summary> 

        public string ResultDescription; 

        /// <summary> 

        ///	Описание результата распознавания без спец. символов (\t,\n,\r) 

        /// </summary> 

        public string ResultDescriptionWithoutCtrlSimbols 

        { 

            get 

            { 

                return Regex.Replace(ResultDescription, "\\s+|\\n|\\r", " "); 

            } 

        } 

        /// <summary> 

        /// Номер бюллетеня (индекс в массиве) 

        /// </summary> 

        public int BulletinNumber; 

        /// <summary> 

        ///	Тип бланка, полученный в результате анализа распознанного бюллетеня 


        /// </summary> 

        public BlankType BlankType = BlankType.NotValid; 

        /// <summary> 

        ///	Описание типа бланка 

        /// </summary> 

        public string BlankTypeDescription 

        { 

            get 

            { 

                switch (BlankType) 

                { 

                    case BlankType.Valid: return "Действительный"; 

                    case BlankType.Bad: return "НУФ"; 

                    default: return "Недействительный"; 

                } 

            } 

        } 

        /// <summary> 

        /// Признак бланка с лишними отметками 

        /// </summary> 

        public bool BulletinWithExtraLabels = false; 

        /// <summary> 

        /// Признак бланка без меток 

        /// </summary> 

        public bool BulletinWithoutLabels = false; 

        /// <summary> 

        ///	Код метода маркировки бюллетеня 

        /// </summary> 

        public short MarkingCode; 

        /// <summary> 

        ///	Тип сохранения изображения 

        /// </summary> 

        public ImageSavingType ImageSavingType; 

        /// <summary> 

        ///	префикс имени файла, в который должно быть сохранено изображение 

        /// </summary> 

        public string ImageFilePrefix; 

 

 

        /// <summary> 

        /// метки 

        /// </summary> 

        public int[][] Marks; 

        /// <summary> 

        /// признаки действительности секций 

        /// </summary> 

        public bool[] SectionsValidity; 

 

 

        #endregion 


 
 

        #region Печать 

        /// <summary> 

        /// Результат распознавания печати 

        /// </summary> 

        public StampResult StampResult; 

        /// <summary> 

        /// Распознанный номер печати 

        /// </summary> 

        public string StampNumber; 

        /// <summary> 

        /// Альтернативные номера печати 

        /// </summary> 

        public string[] StampNumberAlts; 

        /// <summary> 

        /// Признак того, что печать была успешно распознана 

        /// </summary> 

        public bool StampOK; 

        /// <summary> 

        /// Код результата распознавания печати 

        /// </summary> 

        public string StampReasonCode = null; 

        /// <summary> 

        /// Описание распознавания печати 

        /// </summary> 

        public string StampDescription = "не определена"; 

        /// <summary> 

        /// Краткое описание распознавания печати 

        /// </summary> 

        public string StampShortDescription = null; 

        #endregion 

 

 

        #region НУФ 

        /// <summary> 

        /// Пруфик файла для сохранения изображения в случае, если бюллетень НУФ 

        /// </summary> 

        public string BadBulletinFilePrefix = null; 

        /// <summary> 

        /// Код причины НУФа 

        /// </summary> 

        public BadBulletinReason? BadBulletinReason = null; 

        /// <summary> 

        /// Причина НУФ печати 

        /// </summary> 

        public string BadStampReason = String.Empty; 

        /// <summary> 

        /// Описание причины НУФа 

        /// </summary> 


        public string BadBulletinDescription = null; 

        /// <summary> 

        /// Краткое описание причины НУФа 

        /// </summary> 

        public string BadBulletinShortDescription = null; 

 

 

        #endregion 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="recognitionResultCode"></param> 

        public RecognitionResult(int recognitionResultCode) 

        { 

            IntResultCode = recognitionResultCode; 

 

 

            if (IntResultCode >= 0) 

                ResultCode = OcrRecognitionResult.OK; 

            else 

                ResultCode = (OcrRecognitionResult)recognitionResultCode; 

        } 

    } 

}


