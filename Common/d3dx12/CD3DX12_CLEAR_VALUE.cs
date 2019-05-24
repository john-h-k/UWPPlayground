using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop;

namespace UWPPlayground.Common.d3dx12
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [StructLayout(LayoutKind.Explicit)]
    public static unsafe class CD3DX12_CLEAR_VALUE
    {
        public static D3D12_CLEAR_VALUE Create(in D3D12_CLEAR_VALUE o)
        {
            return o;
        }

        public static D3D12_CLEAR_VALUE Create(
            DXGI_FORMAT format,
            Vector4 color) //  TODO, original is 'const float color[4]'
        {
            var obj = new D3D12_CLEAR_VALUE { DepthStencil = default, Format = format };

            Unsafe.WriteUnaligned(ref Unsafe.As<float, byte>(ref *obj.Color), color);

            return obj;
        }

        public static D3D12_CLEAR_VALUE Create(
            DXGI_FORMAT format,
            float depth,
            byte stencil)
        {
            D3D12_CLEAR_VALUE obj;

            obj.Format = format;
            obj.DepthStencil = default;
            obj.DepthStencil.Depth = depth;
            obj.DepthStencil.Stencil = stencil;

            return obj;
        }
    };
}