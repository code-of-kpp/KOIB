using System; 

using System.Runtime.InteropServices; 

 

 

namespace Croc.Bpc.Common 

{ 

    /// <summary> 

    /// Блок памяти 

    /// </summary> 

    public class MemoryBlock 

    { 

        /// <summary> 

        /// Внутренний указатель 

        /// </summary> 

        private IntPtr _internalPtr = IntPtr.Zero; 

        /// <summary> 

        /// Признак необходимости очищать память, на которую ссылается указатель 

        /// </summary> 

        private bool _needFree; 

        /// <summary> 

        /// Размер выделенного блока памяти 

        /// </summary> 

        private int _size; 

        /// <summary> 

        /// Размер выделенной памяти 

        /// </summary> 

        public int SizeOf 

        { 

            get 

            { 

                return _size; 

            } 

        } 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        public MemoryBlock() 

        { 

            _internalPtr = IntPtr.Zero; 

            _needFree = true; 

            _size = 0; 

        } 

 

 

        /// <summary> 

        /// Конструктор, который сразу выделяет заданный размер памяти 

        /// </summary> 

        public MemoryBlock(int memorySize) 


            : this() 

        { 

            Alloc(memorySize); 

        } 

 

 

        /// <summary> 

        /// Конструктор стационарного блока 

        /// </summary> 

        /// <param name="pointer">указатель на блок не требующий очистки</param> 

        public MemoryBlock(IntPtr pointer) 

        { 

            _internalPtr = pointer; 

            _needFree = false; 

            _size = 0; 

        } 

 

 

        /// <summary> 

        /// Получить указатель на блок памяти 

        /// </summary> 

        public IntPtr ToPointer() 

        { 

            // просто возвратим сырой указатель 

            return _internalPtr; 

        } 

 

 

        /// <summary> 

        /// Выделить память 

        /// </summary> 

        /// <param name="size">Размер области памяти</param> 

        public IntPtr Alloc(int size) 

        { 

            if (!_needFree) 

                throw new InvalidOperationException("Данная операция неприменима к созданному блоку"); 

 

 

            _internalPtr = Marshal.AllocHGlobal(size); 

            _size = size; 

 

 

            return _internalPtr; 

        } 

 

 

        /// <summary> 

        /// Освободить захваченную память 

        /// </summary> 

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


