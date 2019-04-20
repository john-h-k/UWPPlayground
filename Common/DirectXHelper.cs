using System;
using System.ComponentModel;
using System.Diagnostics;
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
        static unsafe DirectXHelper()
        {
            var mem = (float*)Marshal.AllocHGlobal(CornflowerBlue.Length * sizeof(float));
            fixed (float* p = &CornflowerBlue[0])
            {
                Unsafe.CopyBlockUnaligned(mem, p, (uint)CornflowerBlue.Length * sizeof(float));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public static void ThrowIfFailed(HRESULT hr)
        {
            if (TerraFX.Interop.Windows.FAILED(hr))
                ThrowWin32Exception(hr);
        }

        public static unsafe void NameObject(void* d3Dect, string name)
        {
#if DEBUG
            var mem = (void*)Marshal.AllocHGlobal(sizeof(char) * name.Length);
            name.AsSpan().CopyTo(new Span<char>(mem, name.Length));
            ((ID3D12Object*)d3Dect)->SetName((char*)mem);
#endif
        }

        [DebuggerHidden]
        public static void ThrowWin32Exception(HRESULT hr)
        {
            throw new COMException($"Unknown exception occured with HRESULT {hr:X8}", hr);
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

        public static readonly float[] CornflowerBlue = {
            0.392156899f, 0.584313750f, 0.929411829f, 1.000000000f
        };

        public static readonly unsafe float* pCornflowerBlue;
    }
}