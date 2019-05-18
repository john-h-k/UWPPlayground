using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop;
using static TerraFX.Interop.Windows;
using HRESULT = System.Int32;
using static TerraFX.Interop.D3D12_RESOURCE_DIMENSION;

namespace UWPPlayground.Common.d3dx12
{
    public static unsafe class Functions
    {
        public static byte D3D12GetFormatPlaneCount(
            [In] ID3D12Device* pDevice,
            DXGI_FORMAT Format
        )
        {
            D3D12_FEATURE_DATA_FORMAT_INFO formatInfo = new D3D12_FEATURE_DATA_FORMAT_INFO {Format = Format};
            if (FAILED(pDevice->CheckFeatureSupport(D3D12_FEATURE.D3D12_FEATURE_FORMAT_INFO,
                &formatInfo, (uint) sizeof(D3D12_FEATURE_DATA_FORMAT_INFO))))
            {
                return 0;
            }

            return formatInfo.PlaneCount;
        }

        public static uint D3D12CalcSubresource(uint MipSlice, uint ArraySlice, uint PlaneSlice, uint MipLevels,
            uint ArraySize)
        {
            return MipSlice + ArraySlice * MipLevels + PlaneSlice * MipLevels * ArraySize;
        }

        public static ulong UpdateSubresources(
            [In] ID3D12GraphicsCommandList* pCmdList,
            [In] ID3D12Resource* pDestinationResource,
            [In] ID3D12Resource* pIntermediate,
            ulong IntermediateOffset,
            [In] uint FirstSubresource,
            [In] uint NumSubresources,
            [In] D3D12_SUBRESOURCE_DATA* pSrcData)
        {
            ulong RequiredSize = 0;
            ulong MemToAlloc = (ulong) (sizeof(D3D12_PLACED_SUBRESOURCE_FOOTPRINT) + sizeof(uint) + sizeof(ulong)) *
                               NumSubresources;

            if (MemToAlloc > (ulong)(void*)-1) // new
            {
                return 0;
            }

            void* pMem = UnsafeNativeMethods.HeapAlloc(UnsafeNativeMethods.GetProcessHeap(), 0, (IntPtr)MemToAlloc);

            if (pMem == null)
            {
                return 0;
            }

            D3D12_PLACED_SUBRESOURCE_FOOTPRINT* pLayouts = (D3D12_PLACED_SUBRESOURCE_FOOTPRINT*) pMem;
            ulong* pRowSizesInBytes = (ulong*) (pLayouts + NumSubresources);
            uint* pNumRows = (uint*) (pRowSizesInBytes + NumSubresources);

            D3D12_RESOURCE_DESC Desc;
            pDestinationResource->GetDesc(&Desc);
            ID3D12Device* pDevice;
            Guid iid = D3D12.IID_ID3D12Device;
            pDestinationResource->GetDevice(&iid, (void**) &pDevice);
            pDevice->GetCopyableFootprints(&Desc, FirstSubresource, NumSubresources, IntermediateOffset, pLayouts,
                pNumRows, pRowSizesInBytes, &RequiredSize);
            pDevice->Release();

            ulong Result = UpdateSubresources(pCmdList, pDestinationResource, pIntermediate, FirstSubresource,
                NumSubresources, RequiredSize, pLayouts, pNumRows, pRowSizesInBytes, pSrcData);
            UnsafeNativeMethods.HeapFree(UnsafeNativeMethods.GetProcessHeap(), 0, pMem);
            return Result;
        }

        public static ulong UpdateSubresources(
            uint MaxSubresources,
            [In] ID3D12GraphicsCommandList* pCmdList,
            [In] ID3D12Resource* pDestinationResource,
            [In] ID3D12Resource* pIntermediate,
            ulong IntermediateOffset,
            [In] uint FirstSubresource,
            [In] uint NumSubresources,
            [In] D3D12_SUBRESOURCE_DATA* pSrcData)
        {
            ulong RequiredSize = 0;
            D3D12_PLACED_SUBRESOURCE_FOOTPRINT* Layouts
                = stackalloc D3D12_PLACED_SUBRESOURCE_FOOTPRINT[(int) MaxSubresources];
            uint* NumRows = stackalloc uint[(int) MaxSubresources];
            ulong* RowSizesInBytes = stackalloc ulong[(int) MaxSubresources];

            D3D12_RESOURCE_DESC Desc;
            pDestinationResource->GetDesc(&Desc);
            ID3D12Device* pDevice;
            Guid iid = D3D12.IID_ID3D12Device;
            pDestinationResource->GetDevice(&iid, (void**) &pDevice);
            pDevice->GetCopyableFootprints(&Desc, FirstSubresource, NumSubresources, IntermediateOffset, Layouts,
                NumRows, RowSizesInBytes, &RequiredSize);
            pDevice->Release();

            return UpdateSubresources(pCmdList, pDestinationResource, pIntermediate, FirstSubresource, NumSubresources,
                RequiredSize, Layouts, NumRows, RowSizesInBytes, pSrcData);
        }

        public static ulong UpdateSubresources(
            [In] ID3D12GraphicsCommandList* pCmdList,
            [In] ID3D12Resource* pDestinationResource,
            [In] ID3D12Resource* pIntermediate,
            [In] uint FirstSubresource,
            [In] uint NumSubresources,
            ulong RequiredSize,
            [In] D3D12_PLACED_SUBRESOURCE_FOOTPRINT* pLayouts,
            [In] uint* pNumRows,
            [In] ulong* pRowSizesInBytes,
            [In] D3D12_SUBRESOURCE_DATA* pSrcData)
        {
            // Minor validation
            D3D12_RESOURCE_DESC IntermediateDesc;
            pIntermediate->GetDesc(&IntermediateDesc);

            D3D12_RESOURCE_DESC DestinationDesc;
            pDestinationResource->GetDesc(&DestinationDesc);

            if (IntermediateDesc.Dimension != D3D12_RESOURCE_DIMENSION_BUFFER ||
                IntermediateDesc.Width < RequiredSize + pLayouts[0].Offset ||
                RequiredSize > (ulong)(void*)(-1) ||
                (DestinationDesc.Dimension == D3D12_RESOURCE_DIMENSION_BUFFER &&
                 (FirstSubresource != 0 || NumSubresources != 1)))
            {
                return 0;
            }

            byte* pData;
            HRESULT hr = pIntermediate->Map(0, null, (void**) &pData);
            if (FAILED(hr))
            {
                return 0;
            }

            for (uint i = 0; i < NumSubresources; ++i)
            {
                /*
                   void *pData;
                   SIZE_T RowPitch;
                   SIZE_T SlicePitch;
                 */
                if (pRowSizesInBytes[i] > (ulong)(void*)-1) return 0;
                D3D12_MEMCPY_DEST DestData = new D3D12_MEMCPY_DEST
                {
                    pData = pData + pLayouts[i].Offset,
                    RowPitch = (UIntPtr)pLayouts[i].Footprint.RowPitch,
                    SlicePitch = (UIntPtr)(pLayouts[i].Footprint.RowPitch * pNumRows[i])
                };

                MemcpySubresource(&DestData, &pSrcData[i], (UIntPtr) pRowSizesInBytes[i], pNumRows[i],
                    pLayouts[i].Footprint.Depth);
            }

            pIntermediate->Unmap(0, null);

            if (DestinationDesc.Dimension == D3D12_RESOURCE_DIMENSION_BUFFER)
            {
                D3D12_BOX SrcBox = CD3DX12_BOX.Create((int) pLayouts[0].Offset,
                    (int) (pLayouts[0].Offset + pLayouts[0].Footprint.Width));
                pCmdList->CopyBufferRegion(
                    pDestinationResource, 0, pIntermediate, pLayouts[0].Offset, pLayouts[0].Footprint.Width);
            }
            else
            {
                for (uint i = 0; i < NumSubresources; ++i)
                {
                    D3D12_TEXTURE_COPY_LOCATION Dst =
                        CD3DX12_TEXTURE_COPY_LOCATION.Create(pDestinationResource, i + FirstSubresource);
                    D3D12_TEXTURE_COPY_LOCATION Src = CD3DX12_TEXTURE_COPY_LOCATION.Create(pIntermediate, pLayouts[i]);
                    pCmdList->CopyTextureRegion(&Dst, 0, 0, 0, &Src, null);
                }
            }

            return RequiredSize;
        }

        public static void MemcpySubresource(
            [In] D3D12_MEMCPY_DEST* pDest,
            [In] D3D12_SUBRESOURCE_DATA* pSrc,
            UIntPtr RowSizeInBytes,
            uint NumRows,
            uint NumSlices)
        {
            for (uint z = 0; z < NumSlices; ++z)
            {
                byte* pDestSlice = (byte*) (pDest->pData) + (ulong)pDest->SlicePitch * z;
                byte* pSrcSlice = (byte*) (pSrc->pData) + (ulong)pSrc->SlicePitch * z;
                for (uint y = 0; y < NumRows; ++y)
                {
                    Buffer.MemoryCopy(source: pDestSlice + (ulong)pDest->RowPitch * y,
                        pSrcSlice + (ulong)pSrc->RowPitch * y,
                        (ulong)RowSizeInBytes, (ulong)RowSizeInBytes);
                }
            }
        }

        private static class UnsafeNativeMethods
        {
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            public enum HeapAllocFlags : uint
            {
                HEAP_GENERATE_EXCEPTIONS = 0x00000004,
                HEAP_NO_SERIALIZE = 0x00000001,
                HEAP_ZERO_MEMORY = 0x00000008
            }

            [SuppressMessage("ReSharper", "InconsistentNaming")]
            public enum HeapFreeFlags : uint
            {
                HEAP_NO_SERIALIZE = HeapAllocFlags.HEAP_NO_SERIALIZE
            }

            [DllImport("kernel32.dll", 
                EntryPoint = "HeapAlloc", 
                CallingConvention = CallingConvention.Winapi,
                SetLastError = true, 
                PreserveSig = true)]
            public static extern void* HeapAlloc(IntPtr hHeap, HeapAllocFlags dwFlags, IntPtr dwBytes);

            [DllImport("kernel32.dll",
                EntryPoint = "HeapFree",
                CallingConvention = CallingConvention.Winapi,
                SetLastError = true,
                PreserveSig = true)]
            public static extern uint HeapFree(
                IntPtr hHeap,
                HeapFreeFlags dwFlags,
                void* lpMem
            );

            [DllImport("kernel32.dll", EntryPoint = "GetProcessHeap", CallingConvention = CallingConvention.Winapi,
                SetLastError = true, PreserveSig = true)]
            public static extern IntPtr GetProcessHeap();
        }
    }
}