using System.Text; 
using Croc.Bpc.Recognizer.Ocr; 
using System.IO; 
using Croc.Bpc.Voting; 
namespace Croc.Bpc.Recognizer 
{ 
    public class Model 
    { 
        private MarkerType _markerType; 
        public Model(MarkerType markerType) 
        { 
            _markerType = markerType; 
        } 
        private byte[] ConvertModelText(string modelText) 
        { 
            var temp = new StringBuilder(modelText); 
            temp = temp.Replace("\r\n", "\n"); 
            while(temp[0] == '\n' && temp.Length > 0) 
            { 
                temp.Remove(0, 1); 
            } 
            var ba = new byte[temp.Length + 1]; 
            for (int i = 0; i < temp.Length; i++) 
            { 
                if (temp[i] == '\n') 
                { 
                    ba[i] = 0; 
                } 
                else 
                { 
                    ba[i] = (byte)temp[i]; 
                } 
            } 
            ba[temp.Length] = 0; 
            return ba; 
        } 
        private void CreateBulletinModel(int bulletinNum, SourceData sourceData) 
        { 
            byte[] data = ConvertModelText(sourceData.Blanks[bulletinNum].Model); 
            int buletinCode = sourceData.Blanks[bulletinNum].Marker; 
            if (Ocr.Ocr.createBallotModel(data, bulletinNum, buletinCode) != 1)  
                Ocr.Ocr.ThrowLastError(); 
            for (int i = 0; i < sourceData.Blanks[bulletinNum].Sections.Length; i++) 
            { 
                var election = sourceData.GetElectionByNum(sourceData.Blanks[bulletinNum].Sections[i]); 
                var pd = new PollData(); 
                int candCount = election.Candidates.Length; 
                int maxMarks = election.MaxMarks; 
                if (candCount != 0) 
                { 
                    pd.polltype = (maxMarks <= 1) ? (int)PollType.Single : (int)PollType.Multi; 
                    pd.totalNum = candCount; 
                    pd.MinValid = 1; 
                    pd.MaxValid = maxMarks; 
                } 
                else 
                { 
                    pd.polltype = (int)PollType.Referendum; 
                } 
                if (Ocr.Ocr.SetPollData(bulletinNum, i, ref pd) != 1)  
                    Ocr.Ocr.ThrowLastError(); 
            } 
        } 
        public void Create(SourceData sourceData) 
        { 
            Ocr.Ocr.ClearError(); 
            if (MarkerType.Digital == _markerType) 
            { 
                int markerType = 1; // установим одноцифровой тип маркера 
                foreach (Blank blank in sourceData.Blanks) 
                { 
                    if (blank.Marker >= 10) 
                    { 
                        markerType = 2; 
                        break; 
                    } 
                } 
                Ocr.Ocr.InitModel(sourceData.Blanks.Length, markerType); 
            } 
            else 
            { 
                Ocr.Ocr.InitModel(sourceData.Blanks.Length, 0); 
            } 
            for (int i = 0; i < sourceData.Blanks.Length; i++) 
            { 
                CreateBulletinModel(i, sourceData); 
            } 
            Ocr.Ocr.SetDefaultStampGeometry(); 
            if (Ocr.Ocr.DefaultModelSave() < 0)  
                Ocr.Ocr.ThrowLastError(); 
        } 
        public void SaveAsText(string fileName) 
        { 
            const int MAX_MODEL_TEXT_SIZE = 256*1024; 
            var modelText = new StringBuilder(MAX_MODEL_TEXT_SIZE); 
            Ocr.Ocr.SaveAsText(modelText, MAX_MODEL_TEXT_SIZE); 
            modelText.Replace("\n", "\r\n"); 
            if (File.Exists(fileName)) 
            { 
                var att = File.GetAttributes(fileName); 
                if (FileAttributes.ReadOnly == (att & FileAttributes.ReadOnly)) 
                    File.SetAttributes(fileName, att ^ FileAttributes.ReadOnly); 
            } 
            var file = new FileInfo(fileName); 
            using (FileStream writer = file.OpenWrite()) 
            { 
                using (var streamWriter = new StreamWriter(writer)) 
                { 
                    streamWriter.Write(modelText.ToString()); 
                    streamWriter.Flush(); 
                } 
            } 
        } 
    } 
}
