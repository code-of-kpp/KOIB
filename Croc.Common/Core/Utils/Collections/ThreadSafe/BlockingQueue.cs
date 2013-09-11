using System; 
using System.Collections; 
using System.Collections.Generic; 
using System.Threading; 
namespace Croc.Core.Utils.Collections 
{ 
    public class BlockingQueue<T> : IDisposable 
    { 
        private readonly Queue<T> _queue; 
        private Boolean _open; 
        private Boolean _disposed; 
        private readonly EventWaitHandle _eventEmpty = new ManualResetEvent(true); 
        public BlockingQueue() 
        { 
            _queue = new Queue<T>(); 
            _open = true; 
        } 
        public WaitHandle EmptiedWaitHandle 
        { 
            get { return _eventEmpty; } 
        } 
        public void Dispose() 
        { 
            lock (SyncRoot) 
            { 
                if (!_disposed) 
                { 
                    _open = false; 
                    _disposed = true; 
                    _queue.Clear(); 
                    _eventEmpty.Close(); 
                    Monitor.PulseAll(SyncRoot); // resume any waiting threads 
                } 
            } 
        } 
        private void ThrowIfDisposed() 
        { 
            if (_disposed) 
                throw new ObjectDisposedException(GetType().FullName); 
        } 
        private Object SyncRoot 
        { 
            get { return ((ICollection) _queue).SyncRoot; } 
        } 
        public Int32 Count 
        { 
            get 
            { 
                lock (SyncRoot) 
                    return _queue.Count; 
            } 
        } 
        public void Clear() 
        { 
            lock (SyncRoot) 
            { 
                ThrowIfDisposed(); 
                _queue.Clear(); 
                _eventEmpty.Set(); 
            } 
        } 
        public void Close() 
        { 
            lock (SyncRoot) 
            { 
                if (_disposed) 
                    return; 
                _open = false; 
                Monitor.PulseAll(SyncRoot); // resume any waiting threads 
            } 
        } 
        public T Dequeue() 
        { 
            return Dequeue(Timeout.Infinite); 
        } 
        public T Dequeue(TimeSpan timeout) 
        { 
            return Dequeue(timeout.Milliseconds); 
        } 
        public T Dequeue(Int32 timeoutMilliseconds) 
        { 
            lock (SyncRoot) 
            { 
                ThrowIfDisposed(); 
                while (_open && (_queue.Count == 0)) 
                { 
                    if (!Monitor.Wait(SyncRoot, timeoutMilliseconds)) 
                        throw new InvalidOperationException("Timeout"); 
                } 
                if (_open) 
                { 
                    var value = _queue.Dequeue(); 
                    SignalIfEmptyUnsafe(); 
                    return value; 
                } 
                throw new InvalidOperationException("Queue Closed"); 
            } 
        } 
        private void SignalIfEmptyUnsafe() 
        { 
            if (_queue.Count == 0) 
                _eventEmpty.Set(); 
        } 
        public Boolean TryDequeue(out T value) 
        { 
            return TryDequeue(Timeout.Infinite, out value); 
        } 
        public Boolean TryDequeue(Int32 timeoutMilliseconds, out T value) 
        { 
            value = default(T); 
            lock (SyncRoot) 
            { 
                if (!_open) 
                    return false; 
                while (_open && _queue.Count == 0) 
                { 
                    if (!Monitor.Wait(SyncRoot, timeoutMilliseconds)) 
                        return false; 
                } 
                if (_open) 
                { 
                    value = _queue.Dequeue(); 
                    SignalIfEmptyUnsafe(); 
                    return true; 
                } 
                return false; 
            } 
        } 
        public void Enqueue(T obj) 
        { 
            lock (SyncRoot) 
            { 
                ThrowIfDisposed(); 
                if (!_open) 
                    throw new InvalidOperationException("Помещение объекта в закрытую очередь недопустимо"); 
                _queue.Enqueue(obj); 
                _eventEmpty.Reset(); 
                Monitor.Pulse(SyncRoot); 
            } 
        } 
        public bool TryEnqueue(T obj) 
        { 
            lock (SyncRoot) 
            { 
                if (!_open || _disposed) 
                    return false; 
                _queue.Enqueue(obj); 
                _eventEmpty.Reset(); 
                Monitor.Pulse(SyncRoot); 
                return true; 
            } 
        } 
        public void Open() 
        { 
            lock (SyncRoot) 
            { 
                ThrowIfDisposed(); 
                _open = true; 
            } 
        } 
        public Boolean IsClosed 
        { 
            get { return !_open; } 
        } 
    } 
}
