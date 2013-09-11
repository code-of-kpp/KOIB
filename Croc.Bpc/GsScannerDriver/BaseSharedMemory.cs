using System; 
using System.Runtime.InteropServices; 
using Mono.Unix.Native; 
namespace Croc.Bpc.GsScannerDriver 
{ 
    public abstract class BaseSharedMemory 
    { 
        public const int DotsPerByteBinary = 8; 
        public const int DotsPerByteHalftone = 1; 
        protected const int LINES = 16; 
        public const int MaxSheetFormats = 20; 
        public const int MaxSheetOffset = 20; 
        protected const int SHMKEY = 0x10; 
        protected const int IPC_CREAT = 01000; 
        protected const int IPC_EXCL = 02000; 
        protected const int IPC_RMID = 0; 
        protected const int SHM_PERMISSION = 0600; 
        protected const string NoSharedMemory = "Не обнаружен буфер драйвера, размер {0} ({1}): {2} ({3})"; 
        [DllImport("libc")] 
        protected static extern int shmget( long/*(key_t)*/ key, int size, int shmflg ); 
        [DllImport("libc")] 
        protected static extern int shmat( int shmid, int/*(void*)*/ shmaddr, int shmflg ); 
        [DllImport("libc")] 
        protected static extern int shmdt( int/*(void*)*/shmaddr ); 
        [DllImport("libc")] 
        protected static extern int shmctl(int shmid, int cmd, int /*(shmid_ds*)*/ buf); 
        [StructLayout(LayoutKind.Sequential)] 
        protected struct    SListFormatsEqualSize 
        { 
            public uint    amountFormats; 
            [MarshalAs (UnmanagedType.ByValArray, SizeConst= MaxSheetFormats )] 
            public uint[]    width; 
            [MarshalAs (UnmanagedType.ByValArray, SizeConst= MaxSheetFormats )] 
            public uint[]    heightMin; 
            [MarshalAs (UnmanagedType.ByValArray, SizeConst= MaxSheetFormats )] 
            public uint[]    heightMax; 
        } 
        [StructLayout(LayoutKind.Sequential)] 
        protected    struct    SListFormats 
        { 
            public uint    amountFormats; 
            public SFormats[]    formats; 
        } 
        [StructLayout(LayoutKind.Sequential)] 
        protected    struct    SFormats 
        { 
            public uint    width; 
            public uint    heightMin; 
            public uint    heightMax; 
        } 
        [StructLayout(LayoutKind.Sequential)] 
        protected    struct    SListShiftsEqualSize 
        { 
            public uint    amountShifts; 
            [MarshalAs (UnmanagedType.ByValArray, SizeConst= MaxSheetOffset )] 
            public uint[]    width; 
            [MarshalAs (UnmanagedType.ByValArray, SizeConst= MaxSheetOffset )] 
            public uint[]    shift; 
        } 
        [StructLayout(LayoutKind.Sequential)] 
        protected    struct SShifts 
        { 
            public uint    width; 
            public uint    shift; 
        } 
        [StructLayout(LayoutKind.Sequential)] 
        protected    struct    SListShifts 
        { 
            public uint        amountShifts; 
            public SShifts[]    shitfs; 
        } 
        [StructLayout(LayoutKind.Sequential)] 
        protected    struct    SProps 
        { 
            public uint        command; 
            public SIB2003.ManufProps    prop; 
        } 
        protected int ptr; 
        protected int shmid; 
        public IntPtr[] BinBuffer { get; protected set; } 
        public IntPtr[] HalftoneBuffer { get; protected set; } 
        public IntPtr Buffer 
        { 
            get 
            { 
                return new IntPtr( ptr ); 
            } 
        } 
        public int BufferSize { get; private set; } 
        protected Type ShmType; 
        protected BaseSharedMemory(bool allowCreate, Type type, bool oneSide) 
        { 
            HalftoneBuffer = new IntPtr[2]; 
            BinBuffer = new IntPtr[2]; 
            ShmType = type; 
            BufferSize = Marshal.SizeOf(ShmType); 
            shmid = shmget(SHMKEY, 0, 0); 
            Errno errno = Stdlib.GetLastError(); 
            if (shmid == -1) 
            { 
                if (allowCreate) 
                { 
                    shmid = shmget(SHMKEY, BufferSize, SHM_PERMISSION | IPC_CREAT | IPC_EXCL); 
                    errno = Stdlib.GetLastError(); 
                    if (shmid == -1) 
                    { 
                        throw new Exception(string.Format(NoSharedMemory, BufferSize, "shmget-new", errno, Stdlib.strerror(errno))); 
                    } 
                } 
                else 
                { 
                    throw new Exception(string.Format(NoSharedMemory, BufferSize, "shmget", errno, Stdlib.strerror(errno))); 
                } 
            } 
            ptr = shmat(shmid, 0, 0); 
            errno = Stdlib.GetLastError(); 
            if (ptr == -1) 
            { 
                throw new Exception(string.Format(NoSharedMemory, BufferSize, "shmat", errno, Stdlib.strerror(errno))); 
            } 
            BinBuffer[0] = GetDataPointer("oneSideBin"); 
            HalftoneBuffer[0] = GetDataPointer("oneSide"); 
            if(oneSide) 
            { 
                BinBuffer[1] = BinBuffer[0]; 
                HalftoneBuffer[1] = HalftoneBuffer[0]; 
            } 
            else 
            { 
                BinBuffer[1] = GetDataPointer("twoSideBin"); 
                HalftoneBuffer[1] = GetDataPointer("twoSide"); 
            } 
        } 
        public void Close() 
        { 
            shmdt( ptr ); 
        } 
        protected IntPtr GetDataPointer(string fieldName) 
        { 
            return new IntPtr(ptr + Marshal.OffsetOf(ShmType, fieldName).ToInt32()); 
        } 
    } 
}
