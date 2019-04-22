using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop;

namespace UWPPlayground.Common
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe ref struct ComPtr<T> where T : unmanaged
    {
        [FieldOffset(0)]
        private IntPtr _intPtr;
        [FieldOffset(0)]
        private T* _ptr;

        public ComPtr(T* ptr)
        {
            _intPtr = default;
            _ptr = ptr;
        }

        public static implicit operator ComPtr<T>(T* value) => new ComPtr<T>(value);

        public static implicit operator T*(ComPtr<T> value) => value._ptr;

        public static implicit operator ComPtr<T>(ComPtrField<T> value) => new ComPtr<T>(value.Detach());

        public static implicit operator ComPtr<IUnknown>(ComPtr<T> value) => new ComPtr<IUnknown>((IUnknown*)value._ptr);

        public static implicit operator IUnknown* (ComPtr<T> value) => (IUnknown*)value._ptr;

        public static bool operator ==(ComPtr<T> left, ComPtr<T> right) => left.Get() == right.Get();

        public static bool operator !=(ComPtr<T> left, ComPtr<T> right) => !(left == right);

        public T* Detach()
        {
            T* temp = _ptr;
            _ptr = null;
            return temp;
        }

        public T* Get() => _ptr;
        public T** GetAddressOf() => (T**)Unsafe.AsPointer(ref _intPtr);
        public T** ReleaseGetAddressOf()
        {
            ((IUnknown*)_ptr)->Release();
            return GetAddressOf();
        }

        public void Release() => Dispose();

        public void Dispose()
        {
            T* temp = _ptr;

            if (temp != null)
            {
                _ptr = null;

                ((IUnknown*)temp)->Release();
            }
        }
    }
}