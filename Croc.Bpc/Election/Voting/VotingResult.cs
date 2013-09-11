using System; 

using System.Text; 

 

 

namespace Croc.Bpc.Election.Voting 

{ 

    /// <summary> 

    /// Результат голосования по одному бюллетеню 

    /// </summary> 

    [Serializable] 

    public class VotingResult 

    { 

        /// <summary> 

        /// Пустой результат голосования 

        /// </summary> 

        public static VotingResult Empty = 

            new VotingResult(BlankType.Unknown, -1, null, null, null, new int[][] { }, new bool[] { }); 

 

 

        /// <summary> 

        /// Тип бланка бюллетеня 

        /// </summary> 

        public readonly BlankType BlankType; 

        /// <summary> 

        /// Номер бюллетеня (индекс в массиве бланков в ИД) 

        /// </summary> 

        public readonly int BulletinNumber; 

        /// <summary> 

        /// Номер печати бюллетеня 

        /// </summary> 

        public readonly string StampNumber; 

        /// <summary> 

        /// Код причины НУФа 

        /// </summary> 

        public readonly string BadBulletinReason; 

        /// <summary> 

        /// Описание причины НУФ печати 

        /// </summary> 

        public readonly string BadStampReason; 

        /// <summary> 

        /// Отметки по всем секциям бюллетеня 

        /// </summary> 

        public readonly int[][] SectionsMarks; 

        /// <summary> 

        /// Признаки корректности секций бюллетеня 

        /// </summary> 

        public readonly bool[] SectionsValidity; 

 

 

 


 
        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="blankType"></param> 

        /// <param name="stampNumber"></param> 

        /// <param name="sectionMarks"></param> 

        public VotingResult( 

            BlankType blankType, 

            int bulletinNumber, 

            string stampNumber, 

            string badBulletinReason, 

            string badStampReason, 

            int[][] sectionsMarks, 

            bool[] sectionsValidity) 

        { 

            BlankType = blankType; 

            BulletinNumber = bulletinNumber; 

            StampNumber = stampNumber; 

            BadBulletinReason = badBulletinReason; 

            BadStampReason = badStampReason; 

            SectionsMarks = sectionsMarks; 

            SectionsValidity = sectionsValidity; 

        } 

 

 

        public override string ToString() 

        { 

            var marksSB = new StringBuilder(); 

 

 

            if (SectionsMarks != null) 

                for (int sectionIndex = 0; sectionIndex < SectionsMarks.Length; ++sectionIndex) 

                { 

                    marksSB.Append(sectionIndex + 1); 

                    marksSB.Append(": "); 

 

 

                    if (SectionsMarks[sectionIndex].Length > 0) 

                    { 

                        foreach (var posIndex in SectionsMarks[sectionIndex]) 

                        { 

                            marksSB.Append(posIndex + 1); 

                            marksSB.Append(','); 

                        } 

 

 

                        marksSB.Length -= 1; 

                    } 

                    else 


                    { 

                        marksSB.Append('-'); 

                    } 

 

 

                    marksSB.Append("; "); 

                } 

 

 

            if (marksSB.Length > 0) 

            { 

                marksSB.Length -= 2; 

                marksSB.Insert(0, '['); 

                marksSB.Append(']'); 

            } 

            else 

                marksSB.Append("нет"); 

 

 

            return string.Format("тип бланка={0}, отметки={1}", BlankType, marksSB); 

        } 

    } 

}


