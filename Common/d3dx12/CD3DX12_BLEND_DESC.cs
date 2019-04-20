using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using TerraFX.Interop;
using TerraFX.Utilities;
using static TerraFX.Interop.Windows;
using static TerraFX.Interop.D3D12_BLEND;
using static TerraFX.Interop.D3D12_LOGIC_OP;
using static TerraFX.Interop.D3D12_BLEND_OP;
using static TerraFX.Interop.D3D12;
using static TerraFX.Interop.D3D12_COLOR_WRITE_ENABLE;

namespace UWPPlayground.Common.d3dx12
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public struct CD3DX12_BLEND_DESC
    {
        public static D3D12_BLEND_DESC Create(in D3D12_BLEND_DESC o)
        {
            return o;
        }

        /*
           BOOL BlendEnable;
           BOOL LogicOpEnable;
           D3D12_BLEND SrcBlend;
           D3D12_BLEND DestBlend;
           D3D12_BLEND_OP BlendOp;
           D3D12_BLEND SrcBlendAlpha;
           D3D12_BLEND DestBlendAlpha;
           D3D12_BLEND_OP BlendOpAlpha;
           D3D12_LOGIC_OP LogicOp;
           UINT8 RenderTargetWriteMask;
         */
        private static readonly D3D12_RENDER_TARGET_BLEND_DESC defaultRenderTargetBlendDesc = new D3D12_RENDER_TARGET_BLEND_DESC
        {
            BlendEnable = FALSE,
            LogicOpEnable = FALSE,
            SrcBlend = D3D12_BLEND_ONE,
            DestBlend = D3D12_BLEND_ZERO,
            BlendOp = D3D12_BLEND_OP_ADD,
            SrcBlendAlpha = D3D12_BLEND_ONE,
            DestBlendAlpha = D3D12_BLEND_ZERO,
            BlendOpAlpha = D3D12_BLEND_OP_ADD,
            LogicOp = D3D12_LOGIC_OP_NOOP,
            RenderTargetWriteMask = (byte)D3D12_COLOR_WRITE_ENABLE_ALL
        };

        private static D3D12_RENDER_TARGET_BLEND_DESC def => defaultRenderTargetBlendDesc;

        public static D3D12_BLEND_DESC Create(CD3DX12_DEFAULT _)
        {

            var obj = new D3D12_BLEND_DESC
            {
                AlphaToCoverageEnable = FALSE,
                IndependentBlendEnable = FALSE,
                RenderTarget =
                {
                    e0 = def, e1 = def, e2 = def, e3 = def, e4 = def,
                    e5 = def, e6 = def, e7 = def
                }
            };

            return obj;
        }
    }
}
