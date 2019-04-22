using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop;

namespace UWPPlayground.Common
{
    [Obsolete("WIP, doesn't work very well with C#", false)]
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct ComPtr<T> : IDisposable where T : unmanaged
    {
        [FieldOffset(0)]
        private T* _ptr;

        public ComPtr(T* ptr)
        {
            _ptr = ptr;
        }

        public T* Value => _ptr;

        public T* Get() => Value;
        //public ref T* GetAddressOf() => ref _ptr;

        public void Dispose()
        {
            T* temp = Value;

            if (temp != null)
            {
                _ptr = null;
                ((IUnknown*)temp)->Release();
            }
        }
    }

    public static unsafe class ComPtrExtensions
    {
        public static void Release(void* p)
        {
            if (p != null)
                ((IUnknown*)p)->Release();
        }

        public static ref T* GetAddressOf<T>(ComPtr<T> obj) where T : unmanaged
        {
            throw new NotImplementedException();
        }
    }
}