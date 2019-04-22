using System;
using System.Runtime.CompilerServices;
using TerraFX.Interop;

namespace UWPPlayground.Common
{
    public unsafe readonly ref struct ComDisposer
    {
        private readonly Span<IntPtr> _ptr;

        public ComDisposer(void** ptr)
        {
            _ptr = new Span<IntPtr>(ptr, 1);
        }

        public void Dispose()
        {
            ((IUnknown*)_ptr[0])->Release();
        }
    }
}