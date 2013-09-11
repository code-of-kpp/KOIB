using System; 

using System.Text; 

using Croc.Bpc.Election.Voting; 

using Croc.Bpc.Recognizer.Ocr; 

using System.IO; 

 

 

namespace Croc.Bpc.Recognizer 

{ 

    /// <summary> 

    /// Класс управления моделью распознавания 

    /// </summary> 

    public class Model 

    { 

        private MarkerType _markerType; 

 

 

        public Model(MarkerType markerType) 

        { 

            _markerType = markerType; 

        } 

 

 

        /// <summary> 

        /// Преобразование текста модели 

        /// </summary> 

        /// <param name="sModelText">Текст модели</param> 

        private byte[] ConvertModelText(string sModelText) 

        { 

            // Временная строковая переменная для хранения 

            // промежуточного результата 

            StringBuilder sTemp = new StringBuilder(sModelText); 

 

 

            sTemp = sTemp.Replace("\r\n", "\n"); 

 

 

            // модель не может содержать лидирующих переводов строки, поэтому уберем их 

            while(sTemp[0] == '\n' && sTemp.Length > 0) 

            { 

                sTemp.Remove(0, 1); 

            } 

 

 

            byte[] ba = new byte[sTemp.Length + 1]; 

            for (int i = 0; i < sTemp.Length; i++) 

            { 

                if (sTemp[i] == '\n') 

                { 

                    ba[i] = 0; 


                } 

                else 

                { 

                    ba[i] = (byte)sTemp[i]; 

                } 

            } 

            ba[sTemp.Length] = 0; 

 

 

            return ba; 

        } 

 

 

        /// <summary> 

        /// Создаёт модель бюллетеня 

        /// </summary> 

        /// <param name="iBul">Номер бюллетеня</param> 

        /// <param name="oData">Исходные данные</param> 

        private void CreateBulletinModel(int iBul, SourceData oData) 

        { 

            // Текст модели 

            byte[] sData = ConvertModelText(oData.Blanks[iBul].Model); 

 

 

            // Код бюллетеня 

            int iBulCode = oData.Blanks[iBul].Marker; 

 

 

            // Создаю модель бюллетеня 

            if (Ocr.Ocr.createBallotModel(sData, iBul, iBulCode) != 1)  

                Ocr.Ocr.ThrowLastError(); 

 

 

            // По всем выборам на данном бюллетене 

            for (int i = 0; i < oData.Blanks[iBul].Sections.Length; i++) 

            { 

                // Получаю ссылку на выборы 

                var oEl = oData.GetElectionByNum(oData.Blanks[iBul].Sections[i]); 

 

 

                // Данные голосования 

                var pd = new PollData(); 

 

 

                // Число кандидатов 

                int nCandCount = oEl.Candidates.Length; 

 

 

                // Максимальное число отметок 

                int nMaxMarks = oEl.MaxMarks; 


 
 

                // Если есть кандидаты - то это выборы иначе референдум 

                if (nCandCount != 0) 

                { 

                    pd.polltype = (nMaxMarks <= 1) ? (int)PollType.Single : (int)PollType.Multi; 

                    pd.totalNum = nCandCount; 

                    pd.MinValid = 1; 

                    pd.MaxValid = nMaxMarks; 

                } 

                else 

                { 

                    pd.polltype = (int)PollType.Referendum; 

                } 

 

 

                // Передаю данные о выборах в распознавалку 

                if (Ocr.Ocr.SetPollData(iBul, i, ref pd) != 1)  

                    Ocr.Ocr.ThrowLastError(); 

            } 

        } 

 

 

        /// <summary> 

        /// Создаёт бинарную модель 

        /// </summary> 

        /// <param name="oData">Исходные данные</param> 

        public void Create(SourceData oData) 

        { 

            // чищу старые ошибки 

            Ocr.Ocr.ClearError(); 

 

 

            // Инициализирую модель 

            if (MarkerType.Digital == _markerType) 

            { 

                int markerType = 1; // установим одноцифровой тип маркера 

 

 

                foreach (Blank blank in oData.Blanks) 

                { 

                    if (blank.Marker >= 10) 

                    { 

                        // однако если в среди бланков есть маркеры большие 10, то 

                        // установим признак двухцифрового (при этом одноцифровые перестанут 

                        // распознаваться!) 

                        markerType = 2; 

                        break; 

                    } 

                } 


 
 

                Ocr.Ocr.InitModel(oData.Blanks.Length, markerType); 

            } 

            else 

            { 

                Ocr.Ocr.InitModel(oData.Blanks.Length, 0); 

            } 

 

 

            // По всем бланкам 

            for (int i = 0; i < oData.Blanks.Length; i++) 

            { 

                CreateBulletinModel(i, oData); 

            } 

 

 

            Ocr.Ocr.SetDefaultStampGeometry(); 

 

 

            if (Ocr.Ocr.DefaultModelSave() < 0)  

                Ocr.Ocr.ThrowLastError(); 

        } 

 

 

        /// <summary> 

        /// Сохраняет модель в виде текстового файла -  

        /// необходимо для отладки 

        /// </summary> 

        /// <param name="sFileName">Название текстового файла модели</param> 

        public void SaveAsText(string sFileName) 

        { 

            // Максимальный размер текстового буфера модели 

            const int MAX_MODEL_TEXT_SIZE = 256 * 1024; 

 

 

            // Буфер под текст модели 

            StringBuilder sModelText = new StringBuilder(MAX_MODEL_TEXT_SIZE); 

 

 

            // Сохраняю модель в текстовом буфере 

            Ocr.Ocr.SaveAsText(sModelText, MAX_MODEL_TEXT_SIZE); 

            sModelText.Replace("\n", "\r\n"); 

 

 

            // если файл существует и только на чтение, то этот аттрибут надо снять 

            if (File.Exists(sFileName)) 

            { 

                var att = File.GetAttributes(sFileName); 

                if (FileAttributes.ReadOnly == (att & FileAttributes.ReadOnly)) 


                    File.SetAttributes(sFileName, att ^ FileAttributes.ReadOnly); 

            } 

 

 

            FileInfo oFile = new FileInfo(sFileName); 

            FileStream oWriter = oFile.OpenWrite(); 

            StreamWriter oTWr = new StreamWriter(oWriter); 

 

 

            // Сохраняю текст модели в файле 

            oTWr.Write(sModelText.ToString()); 

            oTWr.Flush(); 

            oTWr.Close(); 

            oWriter.Close(); 

        } 

    } 

}


