using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Workflow.ComponentModel; 

 

 

namespace Croc.Bpc.Workflow.Activities 

{ 

    public static class BpcNextActivityKeys 

    { 

        public static NextActivityKey Yes = new NextActivityKey("Yes"); 

        public static NextActivityKey No = new NextActivityKey("No"); 

        public static NextActivityKey YesAndNo = new NextActivityKey("YesAndNo"); 

        public static NextActivityKey Back = new NextActivityKey("Back"); 

        public static NextActivityKey Help = new NextActivityKey("Help"); 

    } 

}


