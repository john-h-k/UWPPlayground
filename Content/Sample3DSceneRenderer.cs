using System;
using System.Diagnostics;
using TerraFX.Interop;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using UWPPlayground.Common;
using UWPPlayground.Common.d3dx12;
using static UWPPlayground.Common.DirectXHelper;
using static TerraFX.Interop.D3D12;
using static TerraFX.Interop.D3D12_ROOT_SIGNATURE_FLAGS;
using Size = Windows.Foundation.Size;
using D3D12_RECT = TerraFX.Interop.RECT;
using static TerraFX.Interop.D3D_PRIMITIVE_TOPOLOGY;
using static TerraFX.Interop.D3D_ROOT_SIGNATURE_VERSION;
using static TerraFX.Interop.D3D12_CLEAR_FLAGS;
using static TerraFX.Interop.D3D12_DESCRIPTOR_RANGE_TYPE;
using static TerraFX.Interop.D3D12_RESOURCE_STATES;
using static TerraFX.Interop.D3D12_SHADER_VISIBILITY;
using static TerraFX.Interop.Windows;
using static TerraFX.Utilities.ExceptionUtilities;

namespace UWPPlayground.Content
{
    public sealed partial class Sample3DSceneRenderer : IDisposable
    {
        private static readonly string AngleKey = "Angle";
        private static readonly string TrackingKey = "Tracking";
        private bool _disposed;

        public static readonly unsafe uint AlignedConstantBufferSize
            = ((uint)sizeof(ModelViewProjectionConstantBuffer) + 255U) & ~255U;

        private readonly DeviceResources _deviceResources;

        private ComPtrField<ID3D12GraphicsCommandList> _commandList;
        private ComPtrField<ID3D12RootSignature> _rootSignature;
        private ComPtrField<ID3D12PipelineState> _pipelineState;
        private ComPtrField<ID3D12DescriptorHeap> _cbvHeap;
        private ComPtrField<ID3D12Resource> _vertexBuffer;
        private ComPtrField<ID3D12Resource> _indexBuffer;
        private ComPtrField<ID3D12Resource> _constantBuffer;
        private ModelViewProjectionConstantBuffer _constantBufferData;
        private unsafe byte* _mappedConstantBuffer;
        private uint _cbvDescriptorSize;
        private D3D12_RECT _scissorRect;
        private ComPtrField<ID3DBlob> _vertexShader;
        private ComPtrField<ID3DBlob> _pixelShader;
        private D3D12_VERTEX_BUFFER_VIEW _vertexBufferView;
        private D3D12_INDEX_BUFFER_VIEW _indexBufferView;

        private bool _loadingComplete  = false;
        private readonly float _radiansPerSecond;
        private bool _tracking;
        private bool _rotating;
        private float _rotationY;

        public Sample3DSceneRenderer(DeviceResources deviceResources)
        {
            LoadState();
            _constantBufferData = default;
            _radiansPerSecond = 0.785398163F;
            _deviceResources = deviceResources;

            CreateDeviceDependentResources();
            CreateWindowSizeDependentResources();
        }

        public static void ThrowIfFailed(string methodName, int hr)
        {
            if (FAILED(hr))
            {
                ThrowExternalException(methodName, hr);
            }
        }

        public static void ThrowIfFailed(int hr)
        {
            if (FAILED(hr))
            {
                ThrowExternalException("", hr);
            }
        }

        public async void CreateDeviceDependentResources()
        {
            CreateDeviceDependentResourcesInternal();
            Task pipelineTask = CreatePipelineState(ReadVertexShader(), ReadPixelShader());
            try
            {
                await CreateRendererAssets(pipelineTask);
            }
            catch (Exception e)
            {
                Debug.WriteLine("FATAL ERROR");
                Debug.WriteLine(e);
                Environment.Exit(0x2);
            }
        }

        public unsafe void CreateDeviceDependentResourcesInternal()
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
                ThrowIfFailed(d3dDevice->CreateRootSignature(0, pSignature.Get()->GetBufferPointer(), pSignature.Get()->GetBufferSize(), &iid, (void**)&rootSignature));
                _rootSignature = rootSignature;
                NameObject(_rootSignature, nameof(_rootSignature));
            }

            
        }

        public unsafe void CreateDeviceDependentResources221()
        {
            ID3D12Device* d3dDevice = _deviceResources.D3DDevice;

            {
                D3D12_DESCRIPTOR_RANGE range = CD3DX12_DESCRIPTOR_RANGE.Create(
                    D3D12_DESCRIPTOR_RANGE_TYPE_CBV, 1, 0);

                CD3DX12_ROOT_PARAMETER.InitAsDescriptorTable(out D3D12_ROOT_PARAMETER parameter, 1, &range,
                    D3D12_SHADER_VISIBILITY_VERTEX);

                D3D12_ROOT_SIGNATURE_FLAGS rootSignatureFlags =
                    D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT |
                    D3D12_ROOT_SIGNATURE_FLAG_DENY_DOMAIN_SHADER_ROOT_ACCESS |
                    D3D12_ROOT_SIGNATURE_FLAG_DENY_GEOMETRY_SHADER_ROOT_ACCESS |
                    D3D12_ROOT_SIGNATURE_FLAG_DENY_HULL_SHADER_ROOT_ACCESS |
                    D3D12_ROOT_SIGNATURE_FLAG_DENY_PIXEL_SHADER_ROOT_ACCESS;

                CD3DX12_ROOT_SIGNATURE_DESC.Init(out D3D12_ROOT_SIGNATURE_DESC descRootSignature, 1, &parameter, 0, null, rootSignatureFlags);

                using ComPtr<ID3DBlob> pSignature = default;
                using ComPtr<ID3DBlob> pError = default;

                ThrowIfFailed(D3D12SerializeRootSignature(
                    &descRootSignature,
                    D3D_ROOT_SIGNATURE_VERSION_1,
                    pSignature.GetAddressOf(),
                    pError.GetAddressOf()));

                ID3D12RootSignature* p = _rootSignature.Get();
                {
                    Guid iid = IID_ID3D12RootSignature;
                    ThrowIfFailed(d3dDevice->CreateRootSignature(
                        0,
                        pSignature.Get()->GetBufferPointer(),
                        pSignature.Get()->GetBufferSize(),
                        &iid,
                        (void**)&p
                    ));

                    _rootSignature = p;

                    NameObject(_rootSignature, nameof(_rootSignature));
                }
            }
        }



        public void CreateWindowSizeDependentResources()
        {
            Size outputSize = _deviceResources.OutputSize;
            float aspectRatio = (float)outputSize.Width / (float)outputSize.Height;
            float fovAngleY = 70.0F * (float)Math.PI / 180.0F;

            D3D12_VIEWPORT viewport = _deviceResources.ScreenViewport;

            _scissorRect = new D3D12_RECT
            {
                left = 0,
                top = 0,
                right = (int)viewport.Width,
                bottom = (int)viewport.Height
            };

            if (aspectRatio < 1)
            {
                fovAngleY *= 2;
            }

            Matrix4x4 perspectiveMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                fovAngleY,
                aspectRatio,
                0.01F,
                100.0F
                );

            Matrix4x4 orientation = _deviceResources.OrientationTransform3D;
            _constantBufferData.projection = Matrix4x4.Transpose(perspectiveMatrix * orientation);

            _constantBufferData.view = Matrix4x4.Transpose(Matrix4x4.CreateLookAt(Eye, At, Up));
        }
        private static readonly Vector3 Eye = new Vector3(0.0F, 0.7F, 1.5F);
        private static readonly Vector3 At = new Vector3(0.0F, -0.1F, 0.0F);
        private static readonly Vector3 Up = new Vector3(0.0F, 1.0F, 0.0F);

        public unsafe void Update(ref StepTimer timer)
        {
            if (_loadingComplete)
            {
                if (!_tracking && _rotating)
                {
                    float angle = (float)timer.ElapsedSeconds * _radiansPerSecond;
                    _rotationY += angle;
                    Rotate(_rotationY);
                }

                byte* destination = _mappedConstantBuffer
                                    + (_deviceResources.CurrentFrameIndex * AlignedConstantBufferSize);

                fixed (void* p = &_constantBufferData)
                {

                    Unsafe.CopyBlockUnaligned(destination,
                        p,
                        (uint) sizeof(ModelViewProjectionConstantBuffer));
                }
            }
        }

        public unsafe bool Render()
        {
            if (!_loadingComplete)
                return false;

            ThrowIfFailed(_deviceResources.CommandAllocator->Reset());

            ThrowIfFailed(_commandList.Get()->Reset(_deviceResources.CommandAllocator, _pipelineState.Get()));

            {
                _commandList.Get()->SetGraphicsRootSignature(_rootSignature.Get());
                const uint ppHeapsCount = 1;
                ID3D12DescriptorHeap** ppHeaps = stackalloc ID3D12DescriptorHeap*[(int)ppHeapsCount]
                {
                    _cbvHeap.Get()
                };

                _commandList.Get()->SetDescriptorHeaps(ppHeapsCount, ppHeaps);

                D3D12_GPU_DESCRIPTOR_HANDLE gpuHandle;
                _cbvHeap.Get()->GetGPUDescriptorHandleForHeapStart(&gpuHandle);
                gpuHandle.ptr += _deviceResources.CurrentFrameIndex * _cbvDescriptorSize;
                _commandList.Get()->SetGraphicsRootDescriptorTable(0, gpuHandle);

                D3D12_VIEWPORT viewport = _deviceResources.ScreenViewport;
                _commandList.Get()->RSSetViewports(1, &viewport);
                D3D12_RECT rect = _scissorRect;
                _commandList.Get()->RSSetScissorRects(1, &rect);

                D3D12_RESOURCE_BARRIER renderTargetResourceBarrier =
                    CD3DX12_RESOURCE_BARRIER.Transition(
                        _deviceResources.RenderTarget,
                        D3D12_RESOURCE_STATE_PRESENT,
                        D3D12_RESOURCE_STATE_RENDER_TARGET
                    );
                _commandList.Get()->ResourceBarrier(1, &renderTargetResourceBarrier);

                D3D12_CPU_DESCRIPTOR_HANDLE renderTargetView = _deviceResources.GetRenderTargetView();
                D3D12_CPU_DESCRIPTOR_HANDLE depthStencilView = _deviceResources.GetDepthStencilView();

                _commandList.Get()->ClearRenderTargetView(renderTargetView, CornflowerBlue, 0, null);
                _commandList.Get()->ClearDepthStencilView(depthStencilView, D3D12_CLEAR_FLAG_DEPTH,
                1, 0, 0,
                null);

                _commandList.Get()->OMSetRenderTargets(1, &renderTargetView, FALSE, &depthStencilView);

                _commandList.Get()->IASetPrimitiveTopology(D3D10_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

                D3D12_VERTEX_BUFFER_VIEW vertexBufferView = _vertexBufferView;
                D3D12_INDEX_BUFFER_VIEW indexBufferView = _indexBufferView;
                _commandList.Get()->IASetVertexBuffers(0, 1, &vertexBufferView);
                _commandList.Get()->IASetIndexBuffer(&indexBufferView);

                _commandList.Get()->DrawIndexedInstanced(36, 1, 0, 0, 0);

                D3D12_RESOURCE_BARRIER presentResourceBarrier =
                        CD3DX12_RESOURCE_BARRIER.Transition(
                            _deviceResources.RenderTarget,
                            D3D12_RESOURCE_STATE_RENDER_TARGET,
                            D3D12_RESOURCE_STATE_PRESENT);

                _commandList.Get()->ResourceBarrier(1, &presentResourceBarrier);
            }

            ThrowIfFailed(_commandList.Get()->Close());

            const uint ppCommandListsCount = 1;
            ID3D12CommandList** ppCommandLists = stackalloc ID3D12CommandList*[(int)ppCommandListsCount]
            {
                (ID3D12CommandList*)_commandList.Get()
            };

            _deviceResources.CommandQueue->ExecuteCommandLists(ppCommandListsCount, ppCommandLists);

            return true;
        }

        public void SaveState()
        {
            IPropertySet state = ApplicationData.Current.LocalSettings.Values;

            if (state.ContainsKey(AngleKey))
            {
                state.Remove(AngleKey);
            }
            if (state.ContainsKey(TrackingKey))
            {
                state.Remove(TrackingKey);
            }

            state.Add(AngleKey, PropertyValue.CreateSingle(_rotationY));
            state.Add(TrackingKey, PropertyValue.CreateBoolean(_tracking));
        }

        public void StartTracking()
        {
            _tracking = true;
        }

        public void TrackingUpdate(float positionX)
        {
            if (_tracking)
            {
                float radians = (float)(Math.PI * 2 * positionX / _deviceResources.OutputSize.Width);
                Rotate(radians);
            }
        }

        public void StopTracking()
        {
            _tracking = false;
        }

        public bool IsTracking() => _tracking;

        public void ToggleTracking() => _tracking = !_tracking;

        private void LoadState()
        {
            IPropertySet state = ApplicationData.Current.LocalSettings.Values;

            if (state.ContainsKey(AngleKey))
            {
                //_rotationY = ((IPropertyValue)state[AngleKey]).GetSingle();
            }

            if (state.ContainsKey(TrackingKey))
            {
                //_tracking = ((IPropertyValue)state[TrackingKey]).GetBoolean();
                //state.Remove(TrackingKey);
            }
        }

        private void Rotate(float angle)
        {
            _constantBufferData.model = Matrix4x4.Transpose(Matrix4x4.CreateRotationY(angle));
        }

        private unsafe void ReleaseUnmanagedResources()
        {
            if (_disposed)
                return;

            _commandList.Dispose();
            _rootSignature.Dispose();
            _pipelineState.Dispose();
            _cbvHeap.Dispose();
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _constantBuffer.Dispose();
            _vertexShader.Dispose();
            _pixelShader.Dispose();
            _constantBuffer.Get()->Unmap(0, null);
            _mappedConstantBuffer = null;

            _disposed = true;
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                _deviceResources?.Dispose();
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Sample3DSceneRenderer()
        {
            Dispose(false);
        }
    }
}