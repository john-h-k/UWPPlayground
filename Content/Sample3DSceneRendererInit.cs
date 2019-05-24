using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TerraFX.Interop;
using UWPPlayground.Common;
using UWPPlayground.Common.d3dx12;
using static TerraFX.Interop.DXGI_FORMAT;
using static TerraFX.Interop.D3D12_DESCRIPTOR_RANGE_TYPE;
using static TerraFX.Interop.D3D_ROOT_SIGNATURE_VERSION;
using static TerraFX.Interop.D3D12_SHADER_VISIBILITY;
using static TerraFX.Interop.D3D12_ROOT_SIGNATURE_FLAGS;
using static TerraFX.Interop.D3D12_INPUT_CLASSIFICATION;
using static UWPPlayground.Common.d3dx12.CD3DX12_DEFAULT;
using static TerraFX.Interop.D3D12;
using static TerraFX.Interop.D3DCompiler;
using static TerraFX.Interop.D3D12_COMMAND_LIST_TYPE;
using static TerraFX.Interop.D3D12_DESCRIPTOR_HEAP_FLAGS;
using static TerraFX.Interop.D3D12_DESCRIPTOR_HEAP_TYPE;
using static TerraFX.Interop.D3D12_HEAP_FLAGS;
using static TerraFX.Interop.D3D12_HEAP_TYPE;
using static TerraFX.Interop.D3D12_PRIMITIVE_TOPOLOGY_TYPE;
using static TerraFX.Interop.D3D12_RESOURCE_STATES;
using static TerraFX.Interop.DX;
using static UWPPlayground.Common.DeviceResources;
using D3D12_GPU_VIRTUAL_ADDRESS = System.UInt64;


namespace UWPPlayground.Content
{
    public partial class Sample3DSceneRenderer
    {
        private unsafe void CreateDeviceDependentResourcesInternal()
        {
            ID3D12Device* d3dDevice = _deviceResources.D3DDevice;

            {
                D3D12_DESCRIPTOR_RANGE range =
                    CD3DX12_DESCRIPTOR_RANGE.Create(D3D12_DESCRIPTOR_RANGE_TYPE_CBV, 1, 0);
                CD3DX12_ROOT_PARAMETER.InitAsDescriptorTable(out D3D12_ROOT_PARAMETER parameter, 1, &range, D3D12_SHADER_VISIBILITY_VERTEX);

                D3D12_ROOT_SIGNATURE_FLAGS rootSignatureFlags =
                    D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT | // Only the input assembler stage needs access to the constant buffer.
                    D3D12_ROOT_SIGNATURE_FLAG_DENY_DOMAIN_SHADER_ROOT_ACCESS |
                    D3D12_ROOT_SIGNATURE_FLAG_DENY_GEOMETRY_SHADER_ROOT_ACCESS |
                    D3D12_ROOT_SIGNATURE_FLAG_DENY_HULL_SHADER_ROOT_ACCESS |
                    D3D12_ROOT_SIGNATURE_FLAG_DENY_PIXEL_SHADER_ROOT_ACCESS;

                CD3DX12_ROOT_SIGNATURE_DESC.Init(out D3D12_ROOT_SIGNATURE_DESC descRootSignature, 1, &parameter, 0, null, rootSignatureFlags);

                var pSignature = new ComPtr<ID3DBlob>();
                var pError = new ComPtr<ID3DBlob>();

                ThrowIfFailed(D3D12SerializeRootSignature(&descRootSignature, D3D_ROOT_SIGNATURE_VERSION_1, pSignature.GetAddressOf(), pError.GetAddressOf()));

                Guid iid = IID_ID3D12RootSignature;
                ID3D12RootSignature* rootSignature;
                ThrowIfFailed(d3dDevice->CreateRootSignature(0, pSignature.Ptr->GetBufferPointer(), pSignature.Ptr->GetBufferSize(), &iid, (void**)&rootSignature));
                _rootSignature = rootSignature;
                NameD3D12Object(_rootSignature.Ptr, nameof(_rootSignature));
            }


        }

        #region Helper

        private static unsafe void CopyBytesToBlob(out ID3DBlob* blob, UIntPtr size, byte[] bytes)
        {
            Span<byte> span = CreateBlob(out blob, size);
            bytes.CopyTo(span);
        }

        private static unsafe Span<byte> CreateBlob(out ID3DBlob* ppBlob, UIntPtr size)
        {
#if DEBUG
            ppBlob = null;
#endif
            ID3DBlob* p;

            ThrowIfFailed(D3DCreateBlob(size, &p));

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
            state.pRootSignature = _rootSignature.Ptr;
            state.VS = CD3DX12_SHADER_BYTECODE.Create(_vertexShader.Ptr->GetBufferPointer(),
                _vertexShader.Ptr->GetBufferSize());

            state.PS = CD3DX12_SHADER_BYTECODE.Create(_pixelShader.Ptr->GetBufferPointer(),
                _pixelShader.Ptr->GetBufferSize());

            state.RasterizerState = CD3DX12_RASTERIZER_DESC.Create(D3D12_DEFAULT);
            state.BlendState = CD3DX12_BLEND_DESC.Create(D3D12_DEFAULT);
            state.DepthStencilState = CD3DX12_DEPTH_STENCIL_DESC.Create(D3D12_DEFAULT);
            state.SampleMask = uint.MaxValue;
            state.PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
            state.NumRenderTargets = 1;
            state.RTVFormats = default; // TODO redundant init - any solution?
            state.RTVFormats[0] = _deviceResources.BackBufferFormat;
            state.DSVFormat = _deviceResources.DepthBufferFormat;
            state.SampleDesc.Count = 1;

            {
                Guid iid = IID_ID3D12PipelineState;
                ID3D12PipelineState* pipelineState;
                ThrowIfFailed(
                    _deviceResources.D3DDevice->CreateGraphicsPipelineState(
                        &state,
                        &iid,
                        (void**)&pipelineState)
                );
                _pipelineState = pipelineState;
            }
        }

        private async Task CreatePipelineState(Task vertexShaderTask, Task pixelShaderTask)
        {
            await vertexShaderTask;
            await pixelShaderTask;

            CreatePipelineDescAndPipelineState();
        }

        private async Task CreateRendererAssets(Task pipelineTask)
        {
            await pipelineTask;
            CreateAssets();
            _loadingComplete = true;
        }

        private unsafe void CreateAssets()
        {
            ID3D12Device* d3dDevice = _deviceResources.D3DDevice;

            Guid iid;

            {
                iid = IID_ID3D12GraphicsCommandList;
                ID3D12GraphicsCommandList* commandList;
                ThrowIfFailed(d3dDevice->CreateCommandList(
                    0,
                    D3D12_COMMAND_LIST_TYPE_DIRECT,
                    _deviceResources.CommandAllocator,
                    _pipelineState.Ptr,
                    &iid,
                    (void**)&commandList));

                _commandList = commandList;
                NameD3D12Object(_commandList.Ptr, nameof(_commandList));
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
            Debug.Assert(sizeof(VertexPositionColor) == Marshal.SizeOf<VertexPositionColor>());
            uint vertexBufferSize = (uint)sizeof(VertexPositionColor) * vertexPositionColorCount;

            using ComPtr<ID3D12Resource> vertexBufferUpload = default;

            D3D12_HEAP_PROPERTIES defaultHeapProperties =
                CD3DX12_HEAP_PROPERTIES.Create(D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT);

            D3D12_RESOURCE_DESC vertexBufferDesc = CD3DX12_RESOURCE_DESC.Buffer(vertexBufferSize);

            {
                iid = IID_ID3D12Resource;
                ID3D12Resource* vertexBuffer;
                ThrowIfFailed(d3dDevice->CreateCommittedResource(
                    &defaultHeapProperties,
                    D3D12_HEAP_FLAG_NONE,
                    &vertexBufferDesc,
                    D3D12_RESOURCE_STATE_COPY_DEST,
                    null,
                    &iid,
                    (void**)&vertexBuffer));

                _vertexBuffer = vertexBuffer;
            }

            iid = IID_ID3D12Resource;
            D3D12_HEAP_PROPERTIES uploadHeapProperties =
                CD3DX12_HEAP_PROPERTIES.Create(D3D12_HEAP_TYPE_UPLOAD);

            ThrowIfFailed(d3dDevice->CreateCommittedResource(
                &uploadHeapProperties,
                D3D12_HEAP_FLAG_NONE,
                &vertexBufferDesc,
                D3D12_RESOURCE_STATE_GENERIC_READ,
                null,
                &iid,
                (void**)vertexBufferUpload.GetAddressOf()));

            NameD3D12Object(_vertexBuffer.Ptr, nameof(_vertexBuffer));

            {
                D3D12_SUBRESOURCE_DATA vertexData;
                vertexData.pData = (byte*)cubeVertices;
                vertexData.RowPitch = (IntPtr)vertexBufferSize;
                vertexData.SlicePitch = vertexData.RowPitch;

                Functions.UpdateSubresources(
                    _commandList.Ptr,
                    _vertexBuffer.Ptr,
                    vertexBufferUpload.Ptr,
                    0, 0, 1,
                    &vertexData);

                D3D12_RESOURCE_BARRIER vertexBufferResourceBarrier =
                    CD3DX12_RESOURCE_BARRIER.Transition(_vertexBuffer.Ptr,
                        D3D12_RESOURCE_STATE_COPY_DEST,
                        D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER);

                _commandList.Ptr->ResourceBarrier(1, &vertexBufferResourceBarrier);
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
            const uint indexBufferSize = sizeof(ushort) * cubeIndicesCount;

            using var indexBufferUpload = new ComPtr<ID3D12Resource>();

            D3D12_RESOURCE_DESC indexBufferDesc = CD3DX12_RESOURCE_DESC.Buffer(indexBufferSize);

            {
                iid = IID_ID3D12Resource;
                ID3D12Resource* indexBuffer;
                ThrowIfFailed(d3dDevice->CreateCommittedResource(
                    &defaultHeapProperties,
                    D3D12_HEAP_FLAG_NONE,
                    &indexBufferDesc,
                    D3D12_RESOURCE_STATE_COPY_DEST,
                    null,
                    &iid,
                    (void**)&indexBuffer));
                _indexBuffer = indexBuffer;
            }

            iid = IID_ID3D12Resource;
            ThrowIfFailed(d3dDevice->CreateCommittedResource(
                &uploadHeapProperties,
                D3D12_HEAP_FLAG_NONE,
                &indexBufferDesc,
                D3D12_RESOURCE_STATE_GENERIC_READ,
                null,
                &iid,
                (void**)indexBufferUpload.GetAddressOf()));

            NameD3D12Object(_indexBuffer.Ptr, nameof(_indexBuffer));

            {
                D3D12_SUBRESOURCE_DATA indexData;
                indexData.pData = (byte*)cubeIndices;
                indexData.RowPitch = (IntPtr)indexBufferSize;
                indexData.SlicePitch = indexData.RowPitch;

                Functions.UpdateSubresources(
                    _commandList.Ptr,
                    _indexBuffer.Ptr,
                    indexBufferUpload.Ptr,
                    0, 0, 1,
                    &indexData);

                D3D12_RESOURCE_BARRIER indexBufferResourceBarrier =
                    CD3DX12_RESOURCE_BARRIER.Transition(_indexBuffer.Ptr,
                        D3D12_RESOURCE_STATE_COPY_DEST,
                        D3D12_RESOURCE_STATE_INDEX_BUFFER);

                _commandList.Ptr->ResourceBarrier(1, &indexBufferResourceBarrier);
            }

            {
                D3D12_DESCRIPTOR_HEAP_DESC heapDesc;
                heapDesc.NumDescriptors = FrameCount;
                heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
                heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;

                {
                    ID3D12DescriptorHeap* cbvHeap;
                    iid = IID_ID3D12DescriptorHeap;
                    ThrowIfFailed(d3dDevice->CreateDescriptorHeap(&heapDesc, &iid, (void**)&cbvHeap));
                    _cbvHeap = cbvHeap;
                    NameD3D12Object(_cbvHeap.Ptr, nameof(_cbvHeap));
                }
            }

            D3D12_RESOURCE_DESC constantBufferDesc = CD3DX12_RESOURCE_DESC.Buffer(
                DeviceResources.FrameCount * AlignedConstantBufferSize);

            {
                iid = IID_ID3D12Resource;
                ID3D12Resource* constantBuffer;
                ThrowIfFailed(d3dDevice->CreateCommittedResource(
                    &uploadHeapProperties,
                    D3D12_HEAP_FLAG_NONE,
                    &constantBufferDesc,
                    D3D12_RESOURCE_STATE_GENERIC_READ,
                    null,
                    &iid,
                    (void**)&constantBuffer));
                _constantBuffer = constantBuffer;

                NameD3D12Object(_constantBuffer.Ptr, nameof(_constantBuffer));
            }

            D3D12_GPU_VIRTUAL_ADDRESS cbvGpuAddress = _constantBuffer.Ptr->GetGPUVirtualAddress();
            D3D12_CPU_DESCRIPTOR_HANDLE cbvCpuHandle;
            _cbvHeap.Ptr->GetCPUDescriptorHandleForHeapStart(&cbvCpuHandle);
            _cbvDescriptorSize =
                d3dDevice->GetDescriptorHandleIncrementSize(
                    D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);

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
                ThrowIfFailed(_constantBuffer.Ptr->Map(0, &readRange, (void**)p));
                Unsafe.InitBlockUnaligned(_mappedConstantBuffer, 0, DeviceResources.FrameCount * AlignedConstantBufferSize);
            }

            ThrowIfFailed(_commandList.Ptr->Close());
            const int ppCommandListCount = 1;
            ID3D12CommandList** ppCommandLists = stackalloc ID3D12CommandList*[ppCommandListCount]
            {
                (ID3D12CommandList*)_commandList.Ptr
            };

            _deviceResources.CommandQueue->ExecuteCommandLists(ppCommandListCount, ppCommandLists);

            _vertexBufferView.BufferLocation = _vertexBuffer.Ptr->GetGPUVirtualAddress();
            _vertexBufferView.StrideInBytes = (uint)sizeof(VertexPositionColor);
            _vertexBufferView.SizeInBytes = vertexBufferSize;

            _indexBufferView.BufferLocation = _indexBuffer.Ptr->GetGPUVirtualAddress();
            _indexBufferView.SizeInBytes = indexBufferSize;
            _indexBufferView.Format = DXGI_FORMAT_R16_UINT;

            _deviceResources.WaitForGpu();
        }
        #endregion
    }
}
