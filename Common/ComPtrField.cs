using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop;

namespace UWPPlayground.Common
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct ComPtrField<T> where T : unmanaged
    {
        [FieldOffset(0)]
        private IntPtr _intPtr;
        [FieldOffset(0)]
        private T* _ptr;

        public ComPtrField(T* ptr)
        {
            _intPtr = default;
            _ptr = ptr;
        }

        public static implicit operator ComPtrField<T>(void* value) => new ComPtrField<T>((T*)value);

        public static implicit operator T* (ComPtrField<T> value) => value._ptr;

        public static implicit operator ComPtrField<T>(ComPtr<T> value) => new ComPtrField<T>(value.Detach());

        public static implicit operator ComPtrField<IUnknown>(ComPtrField<T> value) => new ComPtrField<IUnknown>((IUnknown*)value._ptr);

        public static implicit operator IUnknown* (ComPtrField<T> value) => (IUnknown*)value._ptr;

        public static bool operator ==(ComPtrField<T> left, ComPtrField<T> right) => left.Get() == right.Get();

        public static bool operator !=(ComPtrField<T> left, ComPtrField<T> right) => !(left == right);

        public T* Detach()
        {
            T* temp = _ptr;
            _ptr = null;
            return temp;
        }

        public T* Get() => _ptr;
        public ref T* GetPinnableReference()
        {
            fixed (T** p = &_ptr)
            {
                return ref *p;
            }
        }

        public ref T* ReleaseGetPinnableReference()
        {
            ((IUnknown*)_ptr)->Release();
            return ref GetPinnableReference();
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