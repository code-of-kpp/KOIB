using System; 
namespace Croc.Core.Extensions 
{ 
    public static class EventExtensions 
    { 
        public static void RaiseEvent(this EventHandler ev, object sender) 
        { 
            RaiseEvent(ev, sender, EventArgs.Empty); 
        } 
        public static void RaiseEvent(this EventHandler ev, object sender, EventArgs e) 
        { 
            var handler = ev; 
            if (handler != null) 
                handler(sender, e); 
        } 
        public static void RaiseEvent<TEventArgs>(this EventHandler<TEventArgs> ev, object sender) 
            where TEventArgs : EventArgs 
        { 
            RaiseEvent(ev, sender, default(TEventArgs)); 
        } 
        public static void RaiseEvent<TEventArgs>(this EventHandler<TEventArgs> ev, object sender, TEventArgs e) 
            where TEventArgs : EventArgs 
        { 
            var handler = ev; 
            if (handler != null) 
                handler(sender, e); 
        } 
    } 
}
