using System; 
using Croc.Core; 
namespace Croc.Bpc.Sound 
{ 
    public interface ISoundManager : ISubsystem, IQuietMode 
    { 
        #region Формирование звуковых и текстовых фраз 
        string StubSoundFileName { get; } 
        string SoundsDirPath { get; } 
        #region Причины реверса и НУФ 
        string GetSoundForReverseReason(int reverseReasonCode); 
        string BadModeReverseReasonSound { get; } 
        string InvalidBlankNumberReverseReasonSound { get; } 
        string BadBulletinSound { get; } 
        string LongBadBulletinSound { get; } 
        string StampNotRecognizedSound { get; } 
        string[] GetSoundsForBadBulletinReason(string badBulletinReasonCode, string badStampReasonCode); 
        #endregion 
        #region Числа 
        string[] GetSoundForNumber(int number, bool useFeminine, NumberDeclension declension); 
        #endregion 
        #region Часы 
        string[] GetSoundForHours(int hours); 
        string[] GetSoundForHours(int hours, bool useFeminine, NumberDeclension declension); 
        string GetTextForHours(int hours); 
        #endregion 
        #region Минуты 
        string[] GetSoundForMinutes(int minutes); 
        string[] GetSoundForMinutes(int minutes, bool useFemale, NumberDeclension declension); 
        string GetTextForMinutes(int minutes); 
        #endregion 
        #region Дни 
        string[] GetSoundForDays(int days); 
        string GetTextForDays(int days); 
        #endregion 
        #region Дата (день месяц год) 
        string[] GetSoundForDayInMonth(int dayNumber); 
        string GetSoundForMonth(int monthNumber); 
        string[] GetSoundForYear(int yearNumber); 
        #endregion 
        #region Буквы 
        string GetSoundForLetter(char letter); 
        #endregion 
        #endregion 
        #region Уровень громкости 
        short GetVolume(); 
        void SetVolume(short volume); 
        #endregion 
        #region Воспроизведение фраз 
        void PlaySounds(string[] soundFiles, EventHandler playingFinishedCallback); 
        void StopPlaying(); 
        #endregion 
    } 
}
