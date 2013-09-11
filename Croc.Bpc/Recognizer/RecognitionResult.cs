using System; 
using System.Text.RegularExpressions; 
using Croc.Bpc.Recognizer.Ocr; 
using Croc.Bpc.RegExpressions; 
using Croc.Bpc.Voting; 
namespace Croc.Bpc.Recognizer 
{ 
    public class RecognitionResult 
    { 
        #region Основные данные о результате распознавания 
        public int IntResultCode 
        { 
            get; 
            private set; 
        } 
        public OcrRecognitionResult ResultCode 
        { 
            get; 
            private set; 
        } 
        public string ResultDescription; 
        public string ResultDescriptionWithoutCtrlSimbols 
        { 
            get 
            { 
                return new CtrlSimbolsRegex().Replace(ResultDescription, " "); 
            } 
        } 
        public int BulletinNumber; 
        public BlankType BlankType = BlankType.NotValid; 
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
        public bool BulletinWithExtraLabels; 
        public bool BulletinWithoutLabels; 
        public BlankMarking Marking; 
        public ImageSavingType ImageSavingType; 
        public string ImageFilePrefix; 
        public int[][] Marks; 
        public bool[] SectionsValidity; 
        #endregion 
        #region Печать 
        public StampResult StampResult; 
        public string StampNumber; 
        public string[] StampNumberAlts; 
        public bool StampOk; 
        public string StampReasonCode; 
        public string StampDescription = "не определена"; 
        public string StampShortDescription; 
        #endregion 
        #region НУФ 
        public string BadBulletinFilePrefix; 
        public BadBulletinReason? BadBulletinReason; 
        public string BadStampReason = String.Empty; 
        public string BadBulletinDescription; 
        public string BadBulletinShortDescription; 
        #endregion 
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
