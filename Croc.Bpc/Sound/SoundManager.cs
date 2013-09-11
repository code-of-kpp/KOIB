using System; 
using System.Collections.Generic; 
using Croc.Bpc.Utils; 
using Croc.Core.Configuration; 
using Croc.Core; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Sound.Config; 
using Croc.Core.Extensions; 
using Croc.Core.Utils; 
namespace Croc.Bpc.Sound 
{ 
    [SubsystemConfigurationElementTypeAttribute(typeof(SoundManagerConfig))] 
    public class SoundManager : Subsystem, ISoundManager 
    { 
        public override void Init(SubsystemConfig config) 
        { 
            _config = (SoundManagerConfig)config; 


            InitSoundPlayer(); 
            InitSoundsPaths(); 
        } 
        public override void ApplyNewConfig(SubsystemConfig newConfig) 
        { 
            Init(newConfig); 
        } 
        #region IQuietMode 
        public bool QuietMode { get; set; } 
        #endregion 
        #region ISoundManager Members 
        #region Формирование звуковых и текстовых фраз 
        private const string SOUNDS_DIR_PREFIX = "Sounds"; 
        private const string NUMBERS_SOUNDS_DIR = "Numbers/"; 
        private const string TIMES_SOUNDS_DIR = "Times/"; 
        private const string MONTHS_SOUNDS_DIR = "Months/"; 
        private const string LETTERS_SOUNDS_DIR = "Letters/"; 
        private const string REVERSE_REASONS_SOUNDS_DIR = "ReverseReasons/"; 
        private const string BAD_REASONS_SOUNDS_DIR = "BadReasons/"; 
        private SoundManagerConfig _config; 
        private string _numbersSoundsDirPath; 
        private string _timesSoundsDirPath; 
        private string _monthsSoundsDirPath; 
        private string _lettersSoundsDirPath; 
        private void InitSoundsPaths() 
        { 
            SoundsDirPath = string.Format("{0}{1}/", SOUNDS_DIR_PREFIX, _config.Player.Format); 
            StubSoundFileName = "stub"; 
            _numbersSoundsDirPath = SoundsDirPath + NUMBERS_SOUNDS_DIR; 
            _timesSoundsDirPath = SoundsDirPath + TIMES_SOUNDS_DIR; 
            _monthsSoundsDirPath = SoundsDirPath + MONTHS_SOUNDS_DIR; 
            _lettersSoundsDirPath = SoundsDirPath + LETTERS_SOUNDS_DIR; 
        } 
        public string StubSoundFileName { get; private set; } 
        public string SoundsDirPath 
        { 
            get; 
            private set; 
        } 
        #region Причины реверса и НУФ 
        public string GetSoundForReverseReason(int reverseReasonCode) 
        { 
            return string.Format("{0}reverse_{1}", REVERSE_REASONS_SOUNDS_DIR, reverseReasonCode); 
        } 
        public string BadModeReverseReasonSound 
        { 
            get 
            { 
                return string.Format("{0}reverse_badmode", REVERSE_REASONS_SOUNDS_DIR); 
            } 
        } 
        public string InvalidBlankNumberReverseReasonSound 
        { 
            get 
            { 
                return string.Format("{0}reverse_invalid_blank_number", REVERSE_REASONS_SOUNDS_DIR); 
            } 
        } 
        private const string BAD_BULLETIN_SOUND = "blanktype_bad"; 
        public string BadBulletinSound 
        { 
            get 
            { 
                return BAD_BULLETIN_SOUND; 
            } 
        } 
        private const string LONG_BAD_BULLETIN_SOUND = "blanktype_longbad"; 
        public string LongBadBulletinSound 
        { 
            get 
            { 
                return LONG_BAD_BULLETIN_SOUND; 
            } 
        } 
        public string StampNotRecognizedSound 
        { 
            get 
            { 
                const string STAMP_NOT_RECOGNIZED_SOUND = "stamp-not-recognized"; 
                return BAD_REASONS_SOUNDS_DIR + STAMP_NOT_RECOGNIZED_SOUND; 
            } 
        } 
        public string[] GetSoundsForBadBulletinReason(string badBulletinReasonCode, string badStampReasonCode) 
        { 
            if (string.IsNullOrEmpty(badBulletinReasonCode)) 
                return new[] { BAD_BULLETIN_SOUND }; 
            var badReasonSound = string.IsNullOrEmpty(badStampReasonCode) 
                ? badBulletinReasonCode 
                : string.Format("{0}-{1}", badBulletinReasonCode, badStampReasonCode); 
            return new[] { BAD_BULLETIN_SOUND, BAD_REASONS_SOUNDS_DIR + badReasonSound.ToLower() }; 
        } 
        #endregion 
        #region Числа 
        public string[] GetSoundForNumber(int number, bool useFeminine, NumberDeclension declension) 
        { 
            CodeContract.Requires(-999999 <= number && number <= 999999); 
            var sounds = new List<string>(); 
            if (number < 0) 
            { 
                sounds.Add(_numbersSoundsDirPath + "minus"); 
                number *= -1; 
            } 
            if (0 <= number && number <= 20) 
            { 
                sounds.Add(GetSoundForMonotonousNumber(number, useFeminine, declension)); 
            } 
            else if (number < 100) 
            { 
                sounds.AddRange(GetSoundForNumberFrom21To99(number, useFeminine, declension)); 
            } 
            else if (number < 1000) 
            { 
                sounds.AddRange(GetSoundForNumberFrom100To999(number, useFeminine, declension)); 
            } 
            else if (number < 1000000) 
            { 
                sounds.AddRange(GetSoundForNumberFrom1000To999999(number, useFeminine, declension)); 
            } 
            return sounds.ToArray(); 
        } 
        private IEnumerable<string> GetSoundForNumberFrom1000To999999(int number, bool useFeminine, NumberDeclension declension) 
        { 
            var sounds = new List<string>(); 
            var thousand = number / 1000; 
            var hundred = number % 1000; 
            sounds.AddRange(GetSoundForNumber(thousand, true, hundred == 0 ? declension : NumberDeclension.None)); 
            var soundForThousandWord = string.Format("{0}{1}", _numbersSoundsDirPath, GetThousandKeyWord(thousand)); 
            sounds.Add(soundForThousandWord); 


            if (hundred != 0) 
                sounds.AddRange(GetSoundForNumber(hundred, useFeminine, declension)); 
            return sounds; 
        } 
        private const string THOUSAND_KEY_WORD_TYSYACHA = "1000"; 
        private const string THOUSAND_KEY_WORD_TYSYACHI = "1000chi"; 
        private const string THOUSAND_KEY_WORD_TYSYACH = "1000ch"; 
        private string GetThousandKeyWord(int thousand) 
        { 
            if (thousand > 20) 
                thousand %= 10; 
            if (thousand == 1) 
                return THOUSAND_KEY_WORD_TYSYACHA; 
            if (2 <= thousand && thousand <= 4) 
                return THOUSAND_KEY_WORD_TYSYACHI; 
            return THOUSAND_KEY_WORD_TYSYACH; 
        } 
        private IEnumerable<string> GetSoundForNumberFrom100To999(int number, bool useFeminine, NumberDeclension declension) 
        { 
            var sounds = new List<string>(); 
            var hundred = number / 100 * 100; 
            var tenWithOne = number % 100; 
            sounds.Add(GetSoundForMonotonousNumber(hundred, useFeminine,  
                tenWithOne == 0 ? declension : NumberDeclension.None)); 


            if (tenWithOne != 0) 
            { 
                if (tenWithOne <= 20) 
                    sounds.Add(GetSoundForMonotonousNumber(tenWithOne, useFeminine, declension)); 
                else 
                    sounds.AddRange(GetSoundForNumberFrom21To99(tenWithOne, useFeminine, declension)); 
            } 
            return sounds; 
        } 
        private IEnumerable<string> GetSoundForNumberFrom21To99(int number, bool useFeminine, NumberDeclension declension) 
        { 
            var sounds = new List<string>(); 
            var ten = number / 10 * 10; 
            var one = number % 10; 
            if (ten != 0) 
                sounds.Add(GetSoundForMonotonousNumber( 
                    ten, 
                    useFeminine, 
                    (one == 0 || declension == NumberDeclension.N_ti) ? declension : NumberDeclension.None)); 
            if (one != 0) 
                sounds.Add(GetSoundForMonotonousNumber(one, useFeminine, declension)); 
            return sounds; 
        } 
        private string GetSoundForMonotonousNumber(int number, bool useFeminine, NumberDeclension declension) 
        { 
            if (useFeminine) 
            { 
                switch (declension) 
                { 
                    case NumberDeclension.N_ti: 
                        if (number == 1) 
                            return GetNumberSoundFilePath(declension, "1oi"); 
                        break; 
                    case NumberDeclension.None: 
                        if (number == 1) 
                            return GetNumberSoundFilePath(declension, "1a"); 
                        if (number == 2) 
                            return GetNumberSoundFilePath(declension, "2e"); 
                        break; 
                } 
            } 
            return GetNumberSoundFilePath(declension, number); 
        } 
        private string GetNumberSoundFilePath(NumberDeclension declension, object numberName) 
        { 
            if (declension == NumberDeclension.None) 
                return string.Format("{0}{1}", _numbersSoundsDirPath, numberName); 
            return string.Format("{0}{1}/{2}", _numbersSoundsDirPath, declension, numberName); 
        } 
        #endregion 
        #region Часы 
        public string[] GetSoundForHours(int hours) 
        { 
            return GetSoundForHours(hours, false, NumberDeclension.None); 
        } 
        public string[] GetSoundForHours(int hours, bool useFeminine, NumberDeclension declension) 
        { 
            CodeContract.Requires(0 <= hours && hours <= 23); 
            var sounds = new List<string>(); 
            sounds.AddRange(GetSoundForNumber(hours, useFeminine, declension)); 
            var soundForHourWord = string.Format("{0}{1}", _timesSoundsDirPath, GetHourKeyWord(hours, declension)); 
            sounds.Add(soundForHourWord); 
            return sounds.ToArray(); 
        } 
        public string GetTextForHours(int hours) 
        { 
            CodeContract.Requires(0 <= hours && hours <= 23); 
            var keyWord = GetHourKeyWord(hours, NumberDeclension.None); 
            switch (keyWord) 
            { 
                case HOUR_KEY_WORD_CHAS: 
                    keyWord = "час"; 
                    break; 
                case HOUR_KEY_WORD_CHASA: 
                    keyWord = "часа"; 
                    break; 
                case HOUR_KEY_WORD_CHASOV: 
                    keyWord = "часов"; 
                    break; 
            } 
            return string.Format("{0} {1}", hours, keyWord); 
        } 


        private const string HOUR_KEY_WORD_CHAS = "chas"; 
        private const string HOUR_KEY_WORD_CHASA = "chasa"; 
        private const string HOUR_KEY_WORD_CHAASA = "chaasa"; 
        private const string HOUR_KEY_WORD_CHASOV = "chasov"; 
        private string GetHourKeyWord(int hours, NumberDeclension declension) 
        { 
            switch (declension) 
            { 
                case NumberDeclension.N_ti: 
                    if (hours == 1 || hours == 21) 
                        return HOUR_KEY_WORD_CHAASA; 
                        return HOUR_KEY_WORD_CHASOV; 
                default: 
                        if (hours == 1 || hours == 21) 
                            return HOUR_KEY_WORD_CHAS; 
                        if ((2 <= hours && hours <= 4) || hours >= 22) 
                            return HOUR_KEY_WORD_CHASA; 
                        return HOUR_KEY_WORD_CHASOV; 
            } 
        } 
        #endregion 
        #region Минуты 
        public string[] GetSoundForMinutes(int minutes) 
        { 
            return GetSoundForMinutes(minutes, true, NumberDeclension.None); 
        } 
        public string[] GetSoundForMinutes(int minutes, bool useFemale, NumberDeclension declension) 
        { 
            CodeContract.Requires(0 <= minutes && minutes <= 59); 
            var sounds = new List<string>(); 
            sounds.AddRange(GetSoundForNumber(minutes, useFemale, declension)); 
            sounds.Add(GetSoundForMinuteWord(minutes, declension)); 
            return sounds.ToArray(); 
        } 
        public string GetTextForMinutes(int minutes) 
        { 
            CodeContract.Requires(0 <= minutes && minutes <= 59); 
            var keyWord = GetMinuteKeyWord(minutes, NumberDeclension.None); 
            switch (keyWord) 
            { 
                case MINUTE_KEY_WORD_MINUT: 
                    keyWord = "минут"; 
                    break; 
                case MINUTE_KEY_WORD_MINUTA: 
                    keyWord = "минута"; 
                    break; 
                case MINUTE_KEY_WORD_MINUTY: 
                    keyWord = "минуты"; 
                    break; 
            } 
            return string.Format("{0} {1}", minutes, keyWord); 
        } 
        private string GetSoundForMinuteWord(int minutes, NumberDeclension declension) 
        { 
            return string.Format("{0}{1}", _timesSoundsDirPath, GetMinuteKeyWord(minutes, declension)); 
        } 
        private const string MINUTE_KEY_WORD_MINUTA = "minuta"; 
        private const string MINUTE_KEY_WORD_MINUTY = "minuty"; 
        private const string MINUTE_KEY_WORD_MINUT = "minut"; 
        private string GetMinuteKeyWord(int minutes, NumberDeclension declension) 
        { 
            if (minutes > 20) 
                minutes %= 10; 
            switch (declension) 
            { 
                case NumberDeclension.N_ti: 
                    if (minutes == 1) 
                        return MINUTE_KEY_WORD_MINUTY; 
                    return MINUTE_KEY_WORD_MINUT; 
                default: 
                    if (minutes == 1) 
                        return MINUTE_KEY_WORD_MINUTA; 
                    if (2 <= minutes && minutes <= 4) 
                        return MINUTE_KEY_WORD_MINUTY; 
                    return MINUTE_KEY_WORD_MINUT; 
            } 
        } 
        #endregion 
        #region Дни 
        public string[] GetSoundForDays(int days) 
        { 
            CodeContract.Requires(0 <= days); 
            var sounds = new List<string>(); 
            sounds.AddRange(GetSoundForNumber(days, false, NumberDeclension.None)); 
            var soundForDayWord = string.Format("{0}{1}", _timesSoundsDirPath, GetDayKeyWord(days)); 
            sounds.Add(soundForDayWord); 
            return sounds.ToArray(); 
        } 
        public string GetTextForDays(int days) 
        { 
            CodeContract.Requires(0 <= days); 
            var keyWord = GetDayKeyWord(days); 
            switch (keyWord) 
            { 
                case DAY_KEY_WORD_DEN: 
                    keyWord = "день"; 
                    break; 
                case DAY_KEY_WORD_DNYA: 
                    keyWord = "дня"; 
                    break; 
                case DAY_KEY_WORD_DNEY: 
                    keyWord = "дней"; 
                    break; 
            } 
            return string.Format("{0} {1}", days, keyWord); 
        } 
        private const string DAY_KEY_WORD_DEN = "den"; 
        private const string DAY_KEY_WORD_DNYA = "dnya"; 
        private const string DAY_KEY_WORD_DNEY = "dney"; 
        private string GetDayKeyWord(int days) 
        { 
            days = days % 100; 
            if (11 <= days && days <= 19) 
                return DAY_KEY_WORD_DNEY; 
            var one = days % 10; 
            if (one == 1) 
                return DAY_KEY_WORD_DEN; 
            if (2 <= one && one <= 4) 
                return DAY_KEY_WORD_DNYA; 
            return DAY_KEY_WORD_DNEY; 
        } 
        #endregion 
        #region Дата (день месяц год) 
        public string[] GetSoundForDayInMonth(int dayNumber) 
        { 
            CodeContract.Requires(1 <= dayNumber && dayNumber <= 31); 
            return GetSoundForNumber(dayNumber, false, NumberDeclension.N_oe); 
        } 
        public string GetSoundForMonth(int monthNumber) 
        { 
            CodeContract.Requires(1 <= monthNumber && monthNumber <= 12); 
            return string.Format("{0}{1}_a", _monthsSoundsDirPath, monthNumber); 
        } 
        public string[] GetSoundForYear(int yearNumber) 
        { 
            var sounds = new List<string>(); 
            sounds.AddRange(GetSoundForNumber(yearNumber, false, NumberDeclension.N_ogo)); 
            sounds.Add(string.Format("{0}goda", _timesSoundsDirPath)); 
            return sounds.ToArray(); 
        } 
        #endregion 
        #region Буквы 
        public string GetSoundForLetter(char letter) 
        { 
            return _lettersSoundsDirPath + (int)letter; 
        } 
        #endregion 
        #endregion 
        #region Воспроизведение фраз 
        private ISoundPlayer _soundPlayer; 
        private volatile string[] _soundFiles; 
        private int _currentSoundFileIndex; 
        private volatile EventHandler _playingFinishedCallback; 
        private void InitSoundPlayer() 
        { 
            if (_soundPlayer != null) 
                _soundPlayer.Dispose(); 
            switch (_config.Player.Format) 
            { 
                case SoundPlayerType.Wav: 
                    _soundPlayer = new WavPlayer(Logger, _config.Player.DeviceLatency); 
                    break; 
                case SoundPlayerType.Spx: 
                    _soundPlayer = new SpeexPlayer(Logger, _config.Player.DeviceLatency); 
                    break; 
                case SoundPlayerType.SpxCmd: 
                    _soundPlayer = new CommandSpeexPlayer(Logger); 
                    break; 
                case SoundPlayerType.Silent: 
                    _soundPlayer = new SilentPlayer(); 
                    break; 
                default: 
                    throw new Exception("Неизвестный формат плеера"); 
            } 
            _soundPlayer.PlayingStopped += SoundPlayer_PlayingStopped; 
        } 
        public short GetVolume() 
        { 
            if (!PlatformDetector.IsUnix) 
                return 0; 
            try 
            { 
                string volume = null; 
                ProcessHelper.StartProcessAndWaitForFinished( 
                    "./getvolume.sh", null, 
                    state => 
                        { 
                            volume = state.Line; 
                            return false; 
                        }, 
                    null); 
                return short.Parse(volume); 
            } 
            catch 
            { 
                Logger.LogWarning(Message.SoundGetVolumeFailed); 
                return 0; 
            } 
        } 
        public void SetVolume(short volume) 
        { 
            CodeContract.Requires(0 <= volume && volume <= 100); 
            if (PlatformDetector.IsUnix) 
                ProcessHelper.StartProcessAndWaitForFinished("./setvolume.sh", volume.ToString(), null, null); 
            Logger.LogInfo(Message.SoundSetVolume, volume); 
        } 
        public void PlaySounds(string[] soundFiles, EventHandler playingFinishedCallback) 
        { 
            CodeContract.Requires(soundFiles != null && soundFiles.Length > 0); 
            if (_soundFiles != null) 
                _soundPlayer.Stop(); 
            else if (!QuietMode) 
            { 
                ProcessHelper.ExecCommand(_config.Commands.BeforePlaying); 
            } 
            _soundFiles = soundFiles; 
            _currentSoundFileIndex = 0; 
            _playingFinishedCallback = playingFinishedCallback; 
            if (QuietMode) 
            { 
                _playingFinishedCallback.RaiseEvent(this); 
            } 
            else 
            { 
                _soundPlayer.Play(_soundFiles[0]); 
            } 
        } 
        private void SoundPlayer_PlayingStopped(object sender, EventArgs e) 
        { 
            if (_soundFiles == null) 
                return; 
            if (++_currentSoundFileIndex < _soundFiles.Length) 
            { 
                _soundPlayer.Play(_soundFiles[_currentSoundFileIndex]); 
            } 
            else 
            { 
                _soundFiles = null; 
                ProcessHelper.ExecCommand(_config.Commands.AfterPlaying); 
                _playingFinishedCallback.RaiseEvent(this); 
            } 
        } 
        public void StopPlaying() 
        { 
            _soundPlayer.Stop(); 
            if (_soundFiles != null) 
            {                 
                _soundFiles = null; 
                ProcessHelper.ExecCommand(_config.Commands.AfterPlaying); 
            } 
        } 
        #endregion 
        #endregion 
        #region IDisposable Members 
        public override void Dispose() 
        { 
            base.Dispose(); 
            if (_soundPlayer != null) 
                _soundPlayer.Dispose(); 
            GC.SuppressFinalize(this); 
        } 
        #endregion 
    } 
}
