using System;
using System.IO;
using System.Numerics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TerraFX.Interop;
using UWPPlayground.Common;
using UWPPlayground.Common.d3dx12;
using static TerraFX.Interop.DXGI_FORMAT;
using static TerraFX.Interop.D3D12_INPUT_CLASSIFICATION;
using static UWPPlayground.Common.d3dx12.CD3DX12_DEFAULT;
using static UWPPlayground.Common.DirectXHelper;
using static TerraFX.Interop.D3D12;
using UWPPlayground.Content;

namespace UWPPlayground.Content
{
    public partial class Sample3DSceneRenderer
    {
        // Safe context used for async
        private static partial class Sample3DSceneRendererHelper
        {
            public static async Task ReadVertexShaderAsync(UnmanagedSpan<byte> result) =>
                (await File.ReadAllBytesAsync("SampleVertexShader.cso"))
                .AsSpan().CopyTo(result);

            public static async Task ReadPixelShaderAsync(UnmanagedSpan<byte> result) =>
                (await File.ReadAllBytesAsync("SamplePixelShader.cso"))
                .AsSpan().CopyTo(result);

            static unsafe Sample3DSceneRendererHelper()
            {
                pAsciiColorString = (sbyte*)Marshal.AllocHGlobal(AsciiColorString.Length);
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<sbyte, byte>(ref AsciiColorString[0]),
                    ref *(byte*)pAsciiColorString, (uint)AsciiColorString.Length);

                pAsciiPositionString = (sbyte*)Marshal.AllocHGlobal(AsciiPositionString.Length);
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<sbyte, byte>(ref AsciiPositionString[0]),
                    ref *(byte*)pAsciiPositionString, (uint)AsciiPositionString.Length);

                pInputLayoutDesc =
                    (D3D12_INPUT_ELEMENT_DESC*)Marshal.AllocHGlobal(
                        InputLayout.Length * sizeof(D3D12_INPUT_ELEMENT_DESC));

                Unsafe.CopyBlockUnaligned(ref Unsafe.As<D3D12_INPUT_ELEMENT_DESC, byte>(ref InputLayout[0]),
                    ref *(byte*)pInputLayoutDesc, (uint)InputLayout.Length * (uint)sizeof(D3D12_INPUT_ELEMENT_DESC));
            }

            private static readonly sbyte[] AsciiColorString =
            {
                (sbyte) 'C', (sbyte) 'O', (sbyte) 'L', (sbyte) 'O', (sbyte) 'R'
            };

            private static readonly sbyte[] AsciiPositionString =
            {
                (sbyte) 'P', (sbyte) 'O', (sbyte) 'S', (sbyte) 'I', (sbyte) 'T',
                (sbyte) 'I', (sbyte) 'O', (sbyte) 'N'
            };

            private static readonly unsafe sbyte* pAsciiColorString;
            private static readonly unsafe sbyte* pAsciiPositionString;

            private static readonly unsafe D3D12_INPUT_ELEMENT_DESC[] InputLayout =
            {
                new D3D12_INPUT_ELEMENT_DESC
                {
                    SemanticName = pAsciiPositionString, SemanticIndex = 0,
                    Format = DXGI_FORMAT_R32G32B32_FLOAT, InputSlot = 0,
                    AlignedByteOffset = 0, InputSlotClass = D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA,
                    InstanceDataStepRate = 0
                },
                new D3D12_INPUT_ELEMENT_DESC
                {
                    SemanticName = pAsciiColorString, SemanticIndex = 0,
                    Format = DXGI_FORMAT_R32G32B32_FLOAT, InputSlot = 0,
                    AlignedByteOffset = 12, InputSlotClass = D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA,
                    InstanceDataStepRate = 0
                },
            };

            private static readonly unsafe D3D12_INPUT_ELEMENT_DESC* pInputLayoutDesc;

            private static unsafe void CreatePipelineDescAndPipelineState(Sample3DSceneRenderer obj)
            {
                D3D12_GRAPHICS_PIPELINE_STATE_DESC state;
                state.InputLayout = new D3D12_INPUT_LAYOUT_DESC
                { pInputElementDescs = pInputLayoutDesc, NumElements = (uint)InputLayout.Length };
                state.pRootSignature = obj._rootSignature;
                state.VS = CD3DX12_SHADER_BYTECODE.Create(obj._vertexShader.AsPointer(),
                    (nuint)obj._vertexShader.Length);

                state.PS = CD3DX12_SHADER_BYTECODE.Create(obj._vertexShader.AsPointer(),
                    (nuint)obj._vertexShader.Length);

                state.RasterizerState = CD3DX12_RASTERIZER_DESC.Create(D3D12_DEFAULT);
                state.BlendState = CD3DX12_BLEND_DESC.Create(D3D12_DEFAULT);
                state.DepthStencilState = CD3DX12_DEPTH_STENCIL_DESC.Create(D3D12_DEFAULT);
                state.SampleMask = uint.MaxValue;
                state.PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE.D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
                state.NumRenderTargets = 1;
                state.RTVFormats = default; // TODO redundant init - any solution?
                state.RTVFormats[0] = obj._deviceResources.GetBackBufferFormat();
                state.DSVFormat = obj._deviceResources.GetDepthBufferFormat();
                state.SampleDesc.Count = 1;

                fixed (ID3D12PipelineState** p = &obj._pipelineState)
                {
                    Guid iid = IID_ID3D12PipelineState;
                    ThrowIfFailed(
                        obj._deviceResources.GetD3DDevice()->CreateGraphicsPipelineState(
                        &state,
                        &iid,
                        (void**)p)
                        );
                }

                obj._vertexShader.Dispose();
                obj._pixelShader.Dispose();
            }

            public static async Task CreatePipelineStateAsync(Sample3DSceneRenderer obj, Task vertexRead,
                Task pixelRead)
            {
                await Task.WhenAll(vertexRead, pixelRead);
                CreatePipelineDescAndPipelineState(obj);
            }

            public static async Task CreatePipelineStateAsync(Sample3DSceneRenderer obj, Task pipelineCreation)
            {
                await pipelineCreation;
                CreatePipelineDescAndPipelineState(obj);
            }

            public static unsafe void CreateAssets(Sample3DSceneRenderer obj)
            {
                ID3D12Device* d3dDevice = obj._deviceResources.GetD3DDevice();

                Guid iid;
                fixed (ID3D12GraphicsCommandList** p = &obj._commandList)
                {
                    iid = IID_ID3D12GraphicsCommandList;
                    ThrowIfFailed(d3dDevice->CreateCommandList(
                        0,
                        D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_DIRECT,
                        obj._deviceResources.GetCommandAllocator(),
                        obj._pipelineState,
                        &iid,
                        (void**)p));
                    NameObject(obj._commandList, nameof(obj._commandList));
                }

                // Cube vertices. Each vertex has a position and a color.
                const uint VertexPositionColorCount = 8;
                VertexPositionColor* cubeVertices = stackalloc VertexPositionColor[(int)VertexPositionColorCount]
                {
                    new VertexPositionColor { pos = new Vector3(-0.5f, -0.5f, -0.5f), color = new Vector3(0.0f, 0.0f, 0.0f) },
                    new VertexPositionColor { pos = new Vector3(-0.5f, -0.5f, 0.5f), color = new Vector3(0.0f, 0.0f, 1.0f) },
                    new VertexPositionColor { pos = new Vector3(-0.5f, 0.5f, -0.5f), color = new Vector3(0.0f, 1.0f, 0.0f) },
                    new VertexPositionColor { pos = new Vector3(-0.5f, 0.5f, 0.5f), color = new Vector3(0.0f, 1.0f, 1.0f) },
                    new VertexPositionColor { pos = new Vector3(0.5f, -0.5f, -0.5f), color = new Vector3(1.0f, 0.0f, 0.0f) },
                    new VertexPositionColor { pos = new Vector3(0.5f, -0.5f, 0.5f), color = new Vector3(1.0f, 0.0f, 1.0f) },
                    new VertexPositionColor { pos = new Vector3(0.5f, 0.5f, -0.5f), color = new Vector3(1.0f, 1.0f, 0.0f) },
                    new VertexPositionColor { pos = new Vector3(0.5f, 0.5f, 0.5f), color = new Vector3(1.0f, 1.0f, 1.0f) }
                };

                uint vertexBufferSize = (uint)sizeof(VertexPositionColor) * VertexPositionColorCount;

                ID3D12Resource* vertexBufferUpload = default;

                D3D12_HEAP_PROPERTIES defaultHeapProperties =
                    CD3DX12_HEAP_PROPERTIES.Create(D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT);

                D3D12_RESOURCE_DESC vertexBufferDesc = CD3DX12_RESOURCE_DESC.Buffer(vertexBufferSize);

                fixed (ID3D12Resource** p = &obj._vertexBuffer)
                {
                    iid = IID_ID3D12Resource;
                    ThrowIfFailed(d3dDevice->CreateCommittedResource(
                        &defaultHeapProperties,
                        D3D12_HEAP_FLAGS.D3D12_HEAP_FLAG_NONE,
                        &vertexBufferDesc,
                        D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COPY_DEST,
                        null,
                        &iid,
                        (void**)p));
                }

                iid = IID_ID3D12Resource;
                D3D12_HEAP_PROPERTIES uploadHeapProperties =
                    CD3DX12_HEAP_PROPERTIES.Create(D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_UPLOAD);

                ThrowIfFailed(d3dDevice->CreateCommittedResource(
                    &uploadHeapProperties,
                    D3D12_HEAP_FLAGS.D3D12_HEAP_FLAG_NONE,
                    &vertexBufferDesc,
                    D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_GENERIC_READ,
                    null,
                    &iid,
                    (void**)&vertexBufferUpload));

                NameObject(obj._vertexBuffer, nameof(obj._vertexBuffer));

                {
                    D3D12_SUBRESOURCE_DATA vertexData;
                    vertexData.pData = (byte*)cubeVertices;
                    vertexData.RowPitch = (nint)vertexBufferSize;
                    vertexData.SlicePitch = vertexData.RowPitch;

                    obj.UpdateSubresources(
                        obj._commandList,
                        obj._vertexBuffer,
                        vertexBufferUpload,
                        0, 0, 1,
                        &vertexData);

                    D3D12_RESOURCE_BARRIER vertexBufferResourceBarrier =
                        CD3DX12_RESOURCE_BARRIER.Transition(obj._vertexBuffer,
                            D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COPY_DEST,
                            D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER);

                    obj._commandList->ResourceBarrier(1, &vertexBufferResourceBarrier);
                }

                const int cubeIndicesCount = 36;
                ushort* cubeIndices = stackalloc ushort[cubeIndicesCount]
                {
                    0,
                    2,
                    1, // -x
                    1,
                    2,
                    3,

                    4,
                    5,
                    6, // +x
                    5,
                    7,
                    6,

                    0,
                    1,
                    5, // -y
                    0,
                    5,
                    4,

                    2,
                    6,
                    7, // +y
                    2,
                    7,
                    3,

                    0,
                    4,
                    6, // -z
                    0,
                    6,
                    2,

                    1,
                    3,
                    7, // +z
                    1,
                    7,
                    5,
                };

                uint indexBufferSize = cubeIndicesCount;

                ID3D12Resource* indexBufferUpload;

                D3D12_RESOURCE_DESC indexBufferDesc = CD3DX12_RESOURCE_DESC.Buffer(indexBufferSize);

                fixed (ID3D12Resource** p = &obj._indexBuffer)
                {
                    iid = IID_ID3D12Resource;
                    ThrowIfFailed(d3dDevice->CreateCommittedResource(
                        &defaultHeapProperties,
                        D3D12_HEAP_FLAGS.D3D12_HEAP_FLAG_NONE,
                        &indexBufferDesc,
                        D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COPY_DEST,
                        null,
                        &iid,
                        (void**)p));

                }

                iid = IID_ID3D12Resource;
                ThrowIfFailed(d3dDevice->CreateCommittedResource(
                    &uploadHeapProperties,
                    D3D12_HEAP_FLAGS.D3D12_HEAP_FLAG_NONE,
                    &indexBufferDesc,
                    D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_GENERIC_READ,
                    null,
                    &iid,
                    (void**)&indexBufferUpload));

                NameObject(obj._indexBuffer, nameof(obj._indexBuffer));

                {
                    D3D12_SUBRESOURCE_DATA indexData;
                    indexData.pData = (byte*)cubeIndices;
                    indexData.RowPitch = (nint)indexBufferSize;
                    indexData.SlicePitch = indexData.RowPitch;

                    obj.UpdateSubresources(
                        obj._commandList,
                        obj._indexBuffer,
                        indexBufferUpload,
                        0, 0, 1,
                        &indexData);

                    D3D12_RESOURCE_BARRIER indexBufferResourceBarrier =
                        CD3DX12_RESOURCE_BARRIER.Transition(obj._indexBuffer,
                            D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COPY_DEST,
                            D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_INDEX_BUFFER);

                    obj._commandList->ResourceBarrier(1, &indexBufferResourceBarrier);
                }

                {
                    D3D12_DESCRIPTOR_HEAP_DESC heapDesc;
                    heapDesc.NumDescriptors = DeviceResources.FrameCount;
                    heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
                    heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAGS.D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;

                    fixed (ID3D12DescriptorHeap** p = &obj._cbvHeap)
                    {
                        iid = IID_ID3D12DescriptorHeap;
                        ThrowIfFailed(d3dDevice->CreateDescriptorHeap(&heapDesc, &iid, (void**)p));
                        NameObject(obj._cbvHeap, nameof(obj._cbvHeap));
                    }
                }

                D3D12_RESOURCE_DESC constantBufferDesc = CD3DX12_RESOURCE_DESC.Buffer(
                    DeviceResources.FrameCount * Sample3DSceneRenderer.AlignedConstantBufferSize);

                fixed (ID3D12Resource** p = &obj._constantBuffer)
                {
                    iid = IID_ID3D12Resource;
                    ThrowIfFailed(d3dDevice->CreateCommittedResource(
                        &uploadHeapProperties,
                        D3D12_HEAP_FLAGS.D3D12_HEAP_FLAG_NONE,
                        &constantBufferDesc,
                        D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_GENERIC_READ,
                        null,
                        &iid,
                        (void**)p));

                    NameObject(obj._constantBuffer, nameof(obj._constantBuffer));
                }


            }
        }
    }
}