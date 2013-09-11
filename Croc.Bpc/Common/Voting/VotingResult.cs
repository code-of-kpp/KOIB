using System; 
using System.Text; 
namespace Croc.Bpc.Voting 
{ 
    [Serializable] 
    public class VotingResult 
    { 
        public static VotingResult Empty = 
            new VotingResult(BlankType.Unknown, -1, null, null, null, new int[][] { }, new bool[] { }); 
        public readonly BlankType BlankType; 
        public readonly int BulletinNumber; 
        public readonly string StampNumber; 
        public readonly string BadBulletinReason; 
        public readonly string BadStampReason; 
        public readonly int[][] SectionsMarks; 
        public readonly bool[] SectionsValidity; 
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
            var marksSb = new StringBuilder(); 
            if (SectionsMarks != null) 
                for (int sectionIndex = 0; sectionIndex < SectionsMarks.Length; ++sectionIndex) 
                { 
                    marksSb.Append(sectionIndex + 1); 
                    marksSb.Append(": "); 
                    if (SectionsMarks[sectionIndex].Length > 0) 
                    { 
                        foreach (var posIndex in SectionsMarks[sectionIndex]) 
                        { 
                            marksSb.Append(posIndex + 1); 
                            marksSb.Append(','); 
                        } 
                        marksSb.Length -= 1; 
                    } 
                    else 
                    { 
                        marksSb.Append('-'); 
                    } 
                    marksSb.Append("; "); 
                } 
            if (marksSb.Length > 0) 
            { 
                marksSb.Length -= 2; 
                marksSb.Insert(0, '['); 
                marksSb.Append(']'); 
            } 
            else 
                marksSb.Append("нет"); 
            return string.Format("тип бланка={0}, отметки={1}", BlankType, marksSb); 
        } 
    } 
}
