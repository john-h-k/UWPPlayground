using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using TerraFX.Interop;

namespace UWPPlayground.Common.d3dx12
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static unsafe class CD3DX12_CPU_DESCRIPTOR_HANDLE
    {
        public static D3D12_CPU_DESCRIPTOR_HANDLE Create(in D3D12_CPU_DESCRIPTOR_HANDLE o)
        {
            return new D3D12_CPU_DESCRIPTOR_HANDLE { ptr = o.ptr };
        }

        public static D3D12_CPU_DESCRIPTOR_HANDLE Create(in D3D12_CPU_DESCRIPTOR_HANDLE other, int offsetScaledByIncrementSize)
        {
            return new D3D12_CPU_DESCRIPTOR_HANDLE { ptr = (UIntPtr)((ulong)other.ptr + (ulong)offsetScaledByIncrementSize) };
        }

        public static D3D12_CPU_DESCRIPTOR_HANDLE Create(in D3D12_CPU_DESCRIPTOR_HANDLE other, int offsetInDescriptors, uint descriptorIncrementSize)
        {
            return new D3D12_CPU_DESCRIPTOR_HANDLE { ptr = (UIntPtr)((ulong)other.ptr + (ulong)offsetInDescriptors * descriptorIncrementSize) };
        }

        public static void Offset(this D3D12_CPU_DESCRIPTOR_HANDLE obj, int offsetInDescriptors, uint descriptorIncrementSize)
        {
            obj.ptr = (UIntPtr)((ulong)obj.ptr + (ulong)offsetInDescriptors * descriptorIncrementSize);
        }

        public static void Offset(this D3D12_CPU_DESCRIPTOR_HANDLE obj, int offsetScaledByIncrementSize)
        {
            obj.ptr = (UIntPtr)((ulong)obj.ptr + (ulong)offsetScaledByIncrementSize);
        }

        public static D3D12_CPU_DESCRIPTOR_HANDLE InitOffsetted(in D3D12_CPU_DESCRIPTOR_HANDLE @base, int offsetScaledByIncrementSize)
        {
            InitOffsetted(out D3D12_CPU_DESCRIPTOR_HANDLE d3dType, @base, offsetScaledByIncrementSize);
            return d3dType;
        }

        public static D3D12_CPU_DESCRIPTOR_HANDLE InitOffsetted(in D3D12_CPU_DESCRIPTOR_HANDLE @base, int offsetInDescriptors, uint descriptorIncrementSize)
        {
            InitOffsetted(out D3D12_CPU_DESCRIPTOR_HANDLE d3dType, @base, offsetInDescriptors, descriptorIncrementSize);
            return d3dType;
        }

        public static void InitOffsetted(out D3D12_CPU_DESCRIPTOR_HANDLE handle, in D3D12_CPU_DESCRIPTOR_HANDLE @base, int offsetScaledByIncrementSize)
        {
            handle.ptr = (UIntPtr)((ulong)@base.ptr + (ulong)offsetScaledByIncrementSize);
        }

        public static void InitOffsetted(out D3D12_CPU_DESCRIPTOR_HANDLE handle, in D3D12_CPU_DESCRIPTOR_HANDLE @base, int offsetInDescriptors, uint descriptorIncrementSize)
        {
            handle.ptr =(UIntPtr)((ulong)@base.ptr + (ulong)offsetInDescriptors * descriptorIncrementSize);
        }
    }
}