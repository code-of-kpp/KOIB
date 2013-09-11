using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities 
{ 
    public static class BpcNextActivityKeys_RecognitionResult 
    { 
        public static NextActivityKey ValidBulletin = new NextActivityKey("ValidBulletin"); 
        public static NextActivityKey NoMarksBulletin = new NextActivityKey("NoMarksBulletin"); 
        public static NextActivityKey TooManyMarksBulletin = new NextActivityKey("TooManyMarksBulletin"); 
        public static NextActivityKey BadBulletin = new NextActivityKey("BadBulletin"); 
        public static NextActivityKey BulletinReversed = new NextActivityKey("BulletinReversed"); 
        public static NextActivityKey BulletinReceivingForbidden = new NextActivityKey("BulletinReceivingForbidden"); 
        public static NextActivityKey Error = new NextActivityKey("Error"); 
        public static NextActivityKey ProbablyDropped = new NextActivityKey("ProbablyDropped"); 
    } 
}
