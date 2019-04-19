using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TerraFX.Interop;

namespace UWPPlayground.Common.d3dx12
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public struct CD3DX12_DESCRIPTOR_RANGE
    {
        public static D3D12_DESCRIPTOR_RANGE Create(in D3D12_DESCRIPTOR_RANGE o)
        {
            return new D3D12_DESCRIPTOR_RANGE
            {
                RangeType = o.RangeType,
                NumDescriptors = o.NumDescriptors,
                BaseShaderRegister = o.BaseShaderRegister,
                RegisterSpace = o.RegisterSpace,
                OffsetInDescriptorsFromTableStart = o.OffsetInDescriptorsFromTableStart
            };
        }

        public static D3D12_DESCRIPTOR_RANGE Create(
            D3D12_DESCRIPTOR_RANGE_TYPE rangeType,
            uint numDescriptors,
            uint baseShaderRegister,
            uint registerSpace = 0,
            uint offsetInDescriptorsFromTableStart =
             /* D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND */ uint.MaxValue)
        {
            return new D3D12_DESCRIPTOR_RANGE
            {
                RangeType = rangeType,
                NumDescriptors = numDescriptors,
                BaseShaderRegister = baseShaderRegister,
                RegisterSpace = registerSpace,
                OffsetInDescriptorsFromTableStart = offsetInDescriptorsFromTableStart
            };
        }
    }
}
