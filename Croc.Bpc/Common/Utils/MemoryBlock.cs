using System; 
using System.Runtime.InteropServices; 
namespace Croc.Bpc.Utils 
{ 
    public class MemoryBlock 
    { 
        private IntPtr _internalPtr = IntPtr.Zero; 
        private bool _needFree; 
        private int _size; 
        public int SizeOf 
        { 
            get 
            { 
                return _size; 
            } 
        } 
        public MemoryBlock() 
        { 
            _internalPtr = IntPtr.Zero; 
            _needFree = true; 
            _size = 0; 
        } 
        public MemoryBlock(int memorySize) 
            : this() 
        { 
            Alloc(memorySize); 
        } 
        public MemoryBlock(IntPtr pointer) 
        { 
            _internalPtr = pointer; 
            _needFree = false; 
            _size = 0; 
        } 
        public IntPtr ToPointer() 
        { 
            return _internalPtr; 
        } 
        public IntPtr Alloc(int size) 
        { 
            if (!_needFree) 
                throw new InvalidOperationException("Данная операция неприменима к созданному блоку"); 
            _internalPtr = Marshal.AllocHGlobal(size); 
            _size = size; 
            return _internalPtr; 
        } 
        public void Free() 
        { 
            if (!_needFree) 
                return; 
            Marshal.FreeHGlobal(_internalPtr); 
            _internalPtr = IntPtr.Zero; 
            _size = 0; 
        } 
    } 
}
