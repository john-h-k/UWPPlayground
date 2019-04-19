using System.Runtime.InteropServices;
using TerraFX.Interop;

namespace UWPPlayground.Common.d3dx12
{
    public static unsafe class Functions
    {
        public static byte D3D12GetFormatPlaneCount(
            [In] ID3D12Device* pDevice,
        DXGI_FORMAT Format
        )
        {
            D3D12_FEATURE_DATA_FORMAT_INFO formatInfo = new D3D12_FEATURE_DATA_FORMAT_INFO { Format = Format };
            if (TerraFX.Interop.Windows.FAILED(pDevice->CheckFeatureSupport(D3D12_FEATURE.D3D12_FEATURE_FORMAT_INFO, &formatInfo, (uint)sizeof(D3D12_FEATURE_DATA_FORMAT_INFO))))
            {
                return 0;
            }
            return formatInfo.PlaneCount;
        }

        public static uint D3D12CalcSubresource(uint MipSlice, uint ArraySlice, uint PlaneSlice, uint MipLevels, uint ArraySize)
        {
            return MipSlice + ArraySlice * MipLevels + PlaneSlice * MipLevels * ArraySize;
        }
    }
}