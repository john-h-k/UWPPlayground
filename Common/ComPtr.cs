using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop;

namespace UWPPlayground.Common
{
    public unsafe ref struct ComPtr<T> where T : unmanaged
    {
        private IntPtr _ptr;

        public ComPtr(T* ptr)
        {
            _ptr = (IntPtr)ptr;
        }

        public ComPtr(IntPtr ptr)
        {
            _ptr = ptr;
        }

        public static implicit operator ComPtr<T>(T* value) => new ComPtr<T>(value);
        public static implicit operator ComPtr<T>(ComPtrField<T> value) => new ComPtr<T>(value.Detach());
        public static implicit operator ComPtr<IUnknown>(ComPtr<T> value) => new ComPtr<IUnknown>((IUnknown*)value._ptr);
        public static implicit operator IUnknown* (ComPtr<T> value) => (IUnknown*)value._ptr;

        public static bool operator ==(ComPtr<T> left, ComPtr<T> right) => left.Get() == right.Get();
        public static bool operator !=(ComPtr<T> left, ComPtr<T> right) => !(left == right);

        public T* Detach()
        {
            IntPtr temp = _ptr;
            _ptr = (IntPtr)null;
            return (T*)temp;
        }

        public T* Get() => (T*)_ptr;

        public T** GetAddressOf() => (T**)Unsafe.AsPointer(ref _ptr);
        public T** ReleaseGetAddressOf()
        {
            if (_ptr != IntPtr.Zero)
            {
                ((IUnknown*)_ptr)->Release();

            }
            
            return GetAddressOf();
        }

        public void Dispose()
        {
            IntPtr temp = _ptr;

            if (temp != (IntPtr)null)
            {
                _ptr = (IntPtr)null;

                ((IUnknown*)temp)->Release();
            }
        }
        public void Release() => Dispose();
    }
}