using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop;

namespace UWPPlayground.Common
{
    using HRESULT = System.Int32;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class DirectXHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfFailed(HRESULT hr)
        {
            if (TerraFX.Interop.Windows.FAILED(hr))
                ThrowWin32Exception(hr);
        }

        public static unsafe void NameObject(void* d3DObject, string name)
        {
#if DEBUG
            var mem = (void*)Marshal.AllocHGlobal(sizeof(char) * name.Length);
            name.AsSpan().CopyTo(new Span<char>(mem, name.Length));
            ((ID3D12Object*)d3DObject)->SetName((char*)mem);
#endif
        }

        public static void ThrowWin32Exception(HRESULT hr)
        {
            throw new Win32Exception(hr);
        }

        public static void ThrowWin32Exception(string message)
        {
            throw new Win32Exception(message);
        }

        public static void ThrowWin32Exception(string message, HRESULT hr)
        {
            throw new Win32Exception(hr, message);
        }

        public static float ConvertDipsToPixels(float dips, float dpi)
        {
            const float dipsPerInch = 96;
            return MathF.Floor(dips * dpi / dipsPerInch + 0.5F);
        }
    }
}