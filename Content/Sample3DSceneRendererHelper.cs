using System;
using System.Diagnostics;
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

using D3D12_GPU_VIRTUAL_ADDRESS = System.UInt64;

namespace UWPPlayground.Content
{
    public partial class Sample3DSceneRenderer
    {
        static unsafe Sample3DSceneRenderer()
        {
            pAsciiColorString = (sbyte*)Marshal.AllocHGlobal(AsciiColorString.Length);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<sbyte, byte>(ref AsciiColorString[0]),
                ref *(byte*)pAsciiColorString, (uint)AsciiColorString.Length);

            pAsciiPositionString = (sbyte*)Marshal.AllocHGlobal(AsciiPositionString.Length);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<sbyte, byte>(ref AsciiPositionString[0]),
                ref *(byte*)pAsciiPositionString, (uint)AsciiPositionString.Length);
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

        public unsafe void ReadVertexShader()
        {
            const string fileName = "SampleVertexShader.cso";

            UIntPtr size = (UIntPtr)new FileInfo(fileName).Length;
            Span<byte> span = CreateBlob(out _vertexShader, size);
            byte[] shader = File.ReadAllBytes(fileName);
            shader.CopyTo(span);
        }

        public unsafe void ReadPixelShader()
        {
            const string fileName = "SamplePixelShader.cso";

            UIntPtr size = (UIntPtr)new FileInfo(fileName).Length;
            Span<byte> span = CreateBlob(out _pixelShader, size);
            byte[] shader = File.ReadAllBytes(fileName);
            shader.CopyTo(span);
        }

        private static unsafe Span<byte> CreateBlob(out ID3DBlob* ppBlob, UIntPtr size)
        {
#if DEBUG
            ppBlob = null;
#endif
            ID3DBlob* p;

            ThrowIfFailed(D3DCompiler.D3DCreateBlob(size, &p));

            ppBlob = p;

            return new Span<byte>(ppBlob->GetBufferPointer(), (int)ppBlob->GetBufferSize());
        }

        private unsafe void CreatePipelineDescAndPipelineState()
        {
            sbyte* pColor = stackalloc sbyte[]
             {
                (sbyte)'C',
                (sbyte)'O',
                (sbyte)'L',
                (sbyte)'O',
                (sbyte)'R',
                (sbyte)'\0'
            };

            sbyte* pPosition = stackalloc sbyte[]
            {
                (sbyte)'P',
                (sbyte)'O',
                (sbyte)'S',
                (sbyte)'I',
                (sbyte)'T',
                (sbyte)'I',
                (sbyte)'O',
                (sbyte)'N',
                (sbyte)'\0'
            };

            D3D12_INPUT_ELEMENT_DESC* pInputLayout = stackalloc D3D12_INPUT_ELEMENT_DESC[]
                {
                new D3D12_INPUT_ELEMENT_DESC
                {
                    SemanticName = pPosition,
                    SemanticIndex = 0,
                    Format = DXGI_FORMAT_R32G32B32_FLOAT,
                    InputSlot = 0,
                    AlignedByteOffset = 0,
                    InputSlotClass = D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA,
                    InstanceDataStepRate = 0
                },
                new D3D12_INPUT_ELEMENT_DESC
                {
                    SemanticName = pColor,
                    SemanticIndex = 0,
                    Format = DXGI_FORMAT_R32G32B32_FLOAT,
                    InputSlot = 0,
                    AlignedByteOffset = 12,
                    InputSlotClass = D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA,
                    InstanceDataStepRate = 0
                }
            };

            D3D12_GRAPHICS_PIPELINE_STATE_DESC state;
            state.InputLayout = new D3D12_INPUT_LAYOUT_DESC
            {
                pInputElementDescs = pInputLayout,
                NumElements = 2
            };
            state.pRootSignature = _rootSignature;
            state.VS = CD3DX12_SHADER_BYTECODE.Create(_vertexShader->GetBufferPointer(),
                _vertexShader->GetBufferSize());

            state.PS = CD3DX12_SHADER_BYTECODE.Create(_pixelShader->GetBufferPointer(),
                _pixelShader->GetBufferSize());

            state.RasterizerState = CD3DX12_RASTERIZER_DESC.Create(D3D12_DEFAULT);
            state.BlendState = CD3DX12_BLEND_DESC.Create(D3D12_DEFAULT);
            state.DepthStencilState = CD3DX12_DEPTH_STENCIL_DESC.Create(D3D12_DEFAULT);
            state.SampleMask = uint.MaxValue;
            state.PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE.D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
            state.NumRenderTargets = 1;
            state.RTVFormats = default; // TODO redundant init - any solution?
            state.RTVFormats[0] = _deviceResources.GetBackBufferFormat();
            state.DSVFormat = _deviceResources.GetDepthBufferFormat();
            state.SampleDesc.Count = 1;

            fixed (ID3D12PipelineState** p = &_pipelineState)
            {
                Guid iid = IID_ID3D12PipelineState;
                ThrowIfFailed(
                    _deviceResources.GetD3DDevice()->CreateGraphicsPipelineState(
                        &state,
                        &iid,
                        (void**)p)
                );
            }
        }

        public void CreatePipelineState()
        {
            CreatePipelineDescAndPipelineState();
        }

        public void CreateRendererAssets()
        {
            CreateAssets();
            _loadingComplete = true;
        }

        private unsafe void CreateAssets()
        {
            ID3D12Device* d3dDevice = _deviceResources.GetD3DDevice();

            Guid iid;
            fixed (ID3D12GraphicsCommandList** p = &_commandList)
            {
                iid = IID_ID3D12GraphicsCommandList;
                ThrowIfFailed(d3dDevice->CreateCommandList(
                    0,
                    D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_DIRECT,
                    _deviceResources.GetCommandAllocator(),
                    _pipelineState,
                    &iid,
                    (void**)p));

                NameObject(_commandList, nameof(_commandList));
            }

            // Cube vertices. Each vertex has a position and a color.
            const uint vertexPositionColorCount = 8;
            VertexPositionColor* cubeVertices = stackalloc VertexPositionColor[(int)vertexPositionColorCount]
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
            uint cubeVerticesSize = (uint)sizeof(VertexPositionColor) * vertexPositionColorCount;

            uint vertexBufferSize = (uint)sizeof(VertexPositionColor) * vertexPositionColorCount;

            ID3D12Resource* vertexBufferUpload = default;

            D3D12_HEAP_PROPERTIES defaultHeapProperties =
                CD3DX12_HEAP_PROPERTIES.Create(D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT);

            D3D12_RESOURCE_DESC vertexBufferDesc = CD3DX12_RESOURCE_DESC.Buffer(vertexBufferSize);

            fixed (ID3D12Resource** p = &_vertexBuffer)
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

            NameObject(_vertexBuffer, nameof(_vertexBuffer));

            {
                D3D12_SUBRESOURCE_DATA vertexData;
                vertexData.pData = (byte*)cubeVertices;
                vertexData.RowPitch = (IntPtr)vertexBufferSize;
                vertexData.SlicePitch = vertexData.RowPitch;

                Functions.UpdateSubresources(
                    _commandList,
                    _vertexBuffer,
                    vertexBufferUpload,
                    0, 0, 1,
                    &vertexData);

                D3D12_RESOURCE_BARRIER vertexBufferResourceBarrier =
                    CD3DX12_RESOURCE_BARRIER.Transition(_vertexBuffer,
                        D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COPY_DEST,
                        D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER);

                _commandList->ResourceBarrier(1, &vertexBufferResourceBarrier);
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
            uint cubeIndicesSize = sizeof(ushort) * cubeIndicesCount;
            uint indexBufferSize = cubeIndicesCount;

            ID3D12Resource* indexBufferUpload;

            D3D12_RESOURCE_DESC indexBufferDesc = CD3DX12_RESOURCE_DESC.Buffer(indexBufferSize);

            fixed (ID3D12Resource** p = &_indexBuffer)
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

            NameObject(_indexBuffer, nameof(_indexBuffer));

            {
                D3D12_SUBRESOURCE_DATA indexData;
                indexData.pData = (byte*)cubeIndices;
                indexData.RowPitch = (IntPtr)indexBufferSize;
                indexData.SlicePitch = indexData.RowPitch;

                Functions.UpdateSubresources(
                    _commandList,
                    _indexBuffer,
                    indexBufferUpload,
                    0, 0, 1,
                    &indexData);

                D3D12_RESOURCE_BARRIER indexBufferResourceBarrier =
                    CD3DX12_RESOURCE_BARRIER.Transition(_indexBuffer,
                        D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COPY_DEST,
                        D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_INDEX_BUFFER);

                _commandList->ResourceBarrier(1, &indexBufferResourceBarrier);
            }

            {
                D3D12_DESCRIPTOR_HEAP_DESC heapDesc;
                heapDesc.NumDescriptors = DeviceResources.FrameCount;
                heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
                heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAGS.D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;

                fixed (ID3D12DescriptorHeap** p = &_cbvHeap)
                {
                    iid = IID_ID3D12DescriptorHeap;
                    ThrowIfFailed(d3dDevice->CreateDescriptorHeap(&heapDesc, &iid, (void**)p));
                    NameObject(_cbvHeap, nameof(_cbvHeap));
                }
            }

            D3D12_RESOURCE_DESC constantBufferDesc = CD3DX12_RESOURCE_DESC.Buffer(
                DeviceResources.FrameCount * AlignedConstantBufferSize);

            fixed (ID3D12Resource** p = &_constantBuffer)
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

                NameObject(_constantBuffer, nameof(_constantBuffer));
            }

            D3D12_GPU_VIRTUAL_ADDRESS cbvGpuAddress = _constantBuffer->GetGPUVirtualAddress();
            D3D12_CPU_DESCRIPTOR_HANDLE cbvCpuHandle;
            _cbvHeap->GetCPUDescriptorHandleForHeapStart(&cbvCpuHandle);
            _cbvDescriptorSize =
                d3dDevice->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE
                    .D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);

            for (var i = 0; i < DeviceResources.FrameCount; i++)
            {
                D3D12_CONSTANT_BUFFER_VIEW_DESC desc;
                desc.BufferLocation = cbvGpuAddress;
                desc.SizeInBytes = AlignedConstantBufferSize;
                d3dDevice->CreateConstantBufferView(&desc, cbvCpuHandle);
                cbvGpuAddress += desc.SizeInBytes;
                cbvCpuHandle.Offset((int)_cbvDescriptorSize);
            }

            D3D12_RANGE readRange = CD3DX12_RANGE.Create((UIntPtr)0, (UIntPtr)0);

            fixed (byte** p = &_mappedConstantBuffer)
            {
                ThrowIfFailed(_constantBuffer->Map(0, &readRange, (void**)p));
                Unsafe.InitBlockUnaligned(*p, 0, DeviceResources.FrameCount * AlignedConstantBufferSize);
            }

            ThrowIfFailed(_commandList->Close());
            const int ppCommandListCount = 1;
            ID3D12CommandList** ppCommandLists = stackalloc ID3D12CommandList*[ppCommandListCount]
            {
                (ID3D12CommandList*)_commandList
            };
            _deviceResources.GetCommandQueue()->ExecuteCommandLists(ppCommandListCount, ppCommandLists);

            _vertexBufferView.BufferLocation = _vertexBuffer->GetGPUVirtualAddress();
            _vertexBufferView.SizeInBytes = (uint)sizeof(VertexPositionColor);
            _vertexBufferView.SizeInBytes = cubeVerticesSize;

            _indexBufferView.BufferLocation = _indexBuffer->GetGPUVirtualAddress();
            _indexBufferView.SizeInBytes = cubeIndicesSize;
            _indexBufferView.Format = DXGI_FORMAT_R16_UINT;

            _deviceResources.WaitForGpu();
        }
    }
}
