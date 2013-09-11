using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Workflow.ComponentModel; 

 

 

namespace Croc.Bpc.Workflow.Activities.Cancelation 

{ 

    public static class BpcNextActivityKeys_Cancelation 

    { 

        public static NextActivityKey CanceledLocally = new NextActivityKey("CanceledLocally"); 

        public static NextActivityKey CanceledInSD = new NextActivityKey("CanceledInSD"); 

        public static NextActivityKey NotCanceled = new NextActivityKey("NotCanceled"); 

    } 

}


