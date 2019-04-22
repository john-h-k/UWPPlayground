using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop;

namespace UWPPlayground.Common
{
    using HRESULT = System.Int32;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class DirectXHelper
    {
        private static readonly Dictionary<uint, string> _hresults = new Dictionary<uint, string>
        {
            [0x00000000] = "S_OK - Operation successful",
            [0x80004004] = "E_ABORT - Operation aborted",
            [0x80070005] = "E_ACCESSDENIED - General access denied error",
            [0x80004005] = "E_FAIL - Unspecified failure",
            [0x80070006] = "E_HANDLE - Handle that is not valid",
            [0x80070057] = "E_INVALIDARG - One or more arguments are not valid",
            [0x80004002] = "E_NOINTERFACE - No such interface supported",
            [0x80004001] = "E_NOTIMPL - Not implemented",
            [0x8007000E] = "E_OUTOFMEMORY - Failed to allocate necessary memory",
            [0x80004003] = "E_POINTER - Pointer that is not valid",
            [0x8000FFFF] = "E_UNEXPECTED - Unexpected failure"
        };

        public static unsafe float* CornflowerBlue;

        static unsafe DirectXHelper()
        {
            CornflowerBlue = (float*) Marshal.AllocHGlobal(sizeof(float) * 4);
            CornflowerBlue[0] = 0.392156899f;
            CornflowerBlue[1] = 0.584313750f;
            CornflowerBlue[2] = 0.929411829f;
            CornflowerBlue[3] = 1.000000000f;
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

        public static unsafe void NameObject<T>(ComPtr<T> d3Dect, string name) where T : unmanaged
        {
#if DEBUG
            var mem = (void*)Marshal.AllocHGlobal(sizeof(char) * name.Length);
            name.AsSpan().CopyTo(new Span<char>(mem, name.Length));
            ((ID3D12Object*)d3Dect.Get())->SetName((char*)mem);
#endif
        }

        public static unsafe void NameObject<T>(ComPtrField<T> d3Dect, string name) where T : unmanaged
        {
#if DEBUG
            var mem = (void*)Marshal.AllocHGlobal(sizeof(char) * name.Length);
            name.AsSpan().CopyTo(new Span<char>(mem, name.Length));
            ((ID3D12Object*)d3Dect.Get())->SetName((char*)mem);
#endif
        }

        [DebuggerHidden]
        public static void ThrowWin32Exception(HRESULT hr)
        {
            if (_hresults.TryGetValue((uint)hr, out string value))
            {
                throw new COMException($"Unknown exception occured with HRESULT {hr:X8} - \"" +
                                       $"{value}\"", hr);
            }
            else if (Errors.ErrorMap.TryGetValue(hr, out value))
            {
                throw new COMException($"Unknown exception occured with HRESULT {hr:X8} - \"" +
                                       $"{value}\"", hr);
            }
            else 
            {
                throw new COMException($"Unknown exception occured with HRESULT {hr:X8}", hr);
            }
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

        public static readonly unsafe float* pCornflowerBlue;
    }
}