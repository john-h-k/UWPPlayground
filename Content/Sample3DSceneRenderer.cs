using System;
using TerraFX.Interop;
using System.Numerics;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using UWPPlayground.Common;
using UWPPlayground.Common.d3dx12;
using static UWPPlayground.Common.DirectXHelper;
using static TerraFX.Interop.D3D12;
using static UWPPlayground.Common.ComPtrExtensions;
using static TerraFX.Interop.Windows;
using static TerraFX.Interop.D3D12_ROOT_SIGNATURE_FLAGS;
using Size = Windows.Foundation.Size;
using D3D12_RECT = TerraFX.Interop.RECT;

namespace UWPPlayground.Content
{
    public sealed partial class Sample3DSceneRenderer : IDisposable
    {
        private static readonly string AngleKey = "Angle";
        private static readonly string TrackingKey = "Tracking";
        private bool _disposed;

        public Sample3DSceneRenderer(DeviceResources deviceResources)
        {
            LoadState();
            _constantBufferData = default;
            _radiansPerSecond = 0.785398163F;
            _deviceResources = deviceResources;

            CreateDeviceDependentResources();
            CreateWindowSizeDependentResources();
        }

        public unsafe void CreateDeviceDependentResources()
        {
            ID3D12Device* d3dDevice = _deviceResources.GetD3DDevice();

            {
                D3D12_DESCRIPTOR_RANGE range = CD3DX12_DESCRIPTOR_RANGE.Create(
                    D3D12_DESCRIPTOR_RANGE_TYPE.D3D12_DESCRIPTOR_RANGE_TYPE_CBV, 1, 0);

                CD3DX12_ROOT_PARAMETER.InitAsDescriptorTable(out D3D12_ROOT_PARAMETER parameter, 1, &range,
                    D3D12_SHADER_VISIBILITY.D3D12_SHADER_VISIBILITY_VERTEX);

                D3D12_ROOT_SIGNATURE_FLAGS rootSignatureFlags =
                    D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT |
                    D3D12_ROOT_SIGNATURE_FLAG_DENY_DOMAIN_SHADER_ROOT_ACCESS |
                    D3D12_ROOT_SIGNATURE_FLAG_DENY_GEOMETRY_SHADER_ROOT_ACCESS |
                    D3D12_ROOT_SIGNATURE_FLAG_DENY_HULL_SHADER_ROOT_ACCESS |
                    D3D12_ROOT_SIGNATURE_FLAG_DENY_PIXEL_SHADER_ROOT_ACCESS;

                CD3DX12_ROOT_SIGNATURE_DESC.Init(out D3D12_ROOT_SIGNATURE_DESC descRootSignature, 1, &parameter, 0, null, rootSignatureFlags);

                ID3DBlob* pSignature;
                ID3DBlob* pError;

                ThrowIfFailed(D3D12SerializeRootSignature(
                    &descRootSignature,
                    D3D_ROOT_SIGNATURE_VERSION.D3D_ROOT_SIGNATURE_VERSION_1,
                    &pSignature,
                    &pError));

                fixed (ID3D12RootSignature** p = &_rootSignature)
                {
                    Guid iid = IID_ID3D12RootSignature;
                    ThrowIfFailed(d3dDevice->CreateRootSignature(
                        0,
                        pSignature->GetBufferPointer(),
                        pSignature->GetBufferSize(),
                        &iid,
                        (void**)p
                    ));

                    NameObject(_rootSignature, nameof(_rootSignature));
                }

                Release(pSignature);
                Release(pError);
            }



            ReadPixelShader();
            ReadVertexShader();
            CreatePipelineState();
            CreateRendererAssets();
        }



        public void CreateWindowSizeDependentResources()
        {
            Size outputSize = _deviceResources.GetOutputSize();
            float aspectRatio = (float)outputSize.Width / (float)outputSize.Height;
            float fovAngleY = 70.0F * (float)Math.PI / 180.0F;

            D3D12_VIEWPORT viewport = _deviceResources.GetScreenViewport();

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

            Matrix4x4 orientation = _deviceResources.GetOrientationTransform3D();
            _constantBufferData.projection = Matrix4x4.Transpose(perspectiveMatrix * orientation);

            _constantBufferData.view = Matrix4x4.Transpose(Matrix4x4.CreateLookAt(_eye, _at, _up));
        }
        private static readonly Vector3 _eye = new Vector3(0.0F, 0.7F, 1.5F);
        private static readonly Vector3 _at = new Vector3(0.0F, -0.1F, 0.0F);
        private static readonly Vector3 _up = new Vector3(0.0F, 1.0F, 0.0F);

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
                                    + (_deviceResources.GetCurrentFrameIndex() * AlignedConstantBufferSize);

                Unsafe.CopyBlockUnaligned(ref *destination,
                    ref Unsafe.As<ModelViewProjectionConstantBuffer, byte>(ref _constantBufferData),
                    (uint)sizeof(ModelViewProjectionConstantBuffer));
            }
        }

        public unsafe bool Render()
        {
            if (!_loadingComplete)
                return false;

            ThrowIfFailed(_deviceResources.GetCommandAllocator()->Reset());

            ThrowIfFailed(_commandList->Reset(_deviceResources.GetCommandAllocator(), _pipelineState));

            {
                _commandList->SetGraphicsRootSignature(_rootSignature);
                const uint ppHeapsCount = 1;
                ID3D12DescriptorHeap** ppHeaps = stackalloc ID3D12DescriptorHeap*[(int)ppHeapsCount]
                {
                    _cbvHeap
                };

                _commandList->SetDescriptorHeaps(ppHeapsCount, ppHeaps);

                D3D12_GPU_DESCRIPTOR_HANDLE gpuHandle;
                _cbvHeap->GetGPUDescriptorHandleForHeapStart(&gpuHandle);
                gpuHandle = CD3DX12_GPU_DESCRIPTOR_HANDLE.Create(gpuHandle,
                    (int)_deviceResources.GetCurrentFrameIndex(),
                    _cbvDescriptorSize);
                _commandList->SetGraphicsRootDescriptorTable(0, gpuHandle);

                D3D12_VIEWPORT viewport = _deviceResources.GetScreenViewport();
                _commandList->RSSetViewports(1, &viewport);
                D3D12_RECT rect = _scissorRect;
               _commandList->RSSetScissorRects(1, &rect);

                D3D12_RESOURCE_BARRIER renderTargetResourceBarrier =
                    CD3DX12_RESOURCE_BARRIER.Transition(
                        _deviceResources.GetRenderTarget(),
                        D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PRESENT,
                        D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET
                    );
                _commandList->ResourceBarrier(1, &renderTargetResourceBarrier);

                D3D12_CPU_DESCRIPTOR_HANDLE renderTargetView = _deviceResources.GetRenderTargetView();
                D3D12_CPU_DESCRIPTOR_HANDLE depthStencilView = _deviceResources.GetDepthStencilView();
                _commandList->ClearRenderTargetView(renderTargetView, CornflowerBlue, 0, null);
                //_commandList->ClearDepthStencilView(depthStencilView,
                //D3D12_CLEAR_FLAGS.D3D12_CLEAR_FLAG_DEPTH,
                //1, 0, 0,
                //null);

                // _commandList->OMSetRenderTargets(1, &renderTargetView, FALSE, &depthStencilView);

                //_commandList->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY.D3D10_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

                fixed (D3D12_VERTEX_BUFFER_VIEW* pVertex = &_vertexBufferView)
                fixed (D3D12_INDEX_BUFFER_VIEW* pIndex = &_indexBufferView)
                {
                    //_commandList->IASetVertexBuffers(0, 1, pVertex);
                    //_commandList->IASetIndexBuffer(pIndex);
                }

                //_commandList->DrawIndexedInstanced(36, 1, 0, 0, 0);

                D3D12_RESOURCE_BARRIER presentResourceBarrier =
                    CD3DX12_RESOURCE_BARRIER.Transition(
                        _deviceResources.GetRenderTarget(),
                        D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET,
                        D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PRESENT);
                //_commandList->ResourceBarrier(1, &presentResourceBarrier);
            }

            ThrowIfFailed(_commandList->Close());

            const uint ppCommandListsCount = 1;
            ID3D12CommandList** ppCommandLists = stackalloc ID3D12CommandList*[(int)ppCommandListsCount]
            {
                (ID3D12CommandList*)_commandList
            };

            _deviceResources.GetCommandQueue()->ExecuteCommandLists(ppCommandListsCount, ppCommandLists);

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
                float radians = (float)(Math.PI * 2 * positionX / _deviceResources.GetOutputSize().Width);
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

        public static readonly unsafe uint AlignedConstantBufferSize
            = ((uint)sizeof(ModelViewProjectionConstantBuffer) + 255U) & ~255U;

        private readonly DeviceResources _deviceResources;

        private unsafe ID3D12GraphicsCommandList* _commandList;
        private unsafe ID3D12RootSignature* _rootSignature;
        private unsafe ID3D12PipelineState* _pipelineState;
        private unsafe ID3D12DescriptorHeap* _cbvHeap;
        private unsafe ID3D12Resource* _vertexBuffer;
        private unsafe ID3D12Resource* _indexBuffer;
        private unsafe ID3D12Resource* _constantBuffer;
        private ModelViewProjectionConstantBuffer _constantBufferData;
        private unsafe byte* _mappedConstantBuffer;
        private uint _cbvDescriptorSize;
        private D3D12_RECT _scissorRect;
        private unsafe ID3DBlob* _vertexShader;
        private unsafe ID3DBlob* _pixelShader;
        private D3D12_VERTEX_BUFFER_VIEW _vertexBufferView;
        private D3D12_INDEX_BUFFER_VIEW _indexBufferView;

        private bool _loadingComplete;
        private readonly float _radiansPerSecond;
        private bool _tracking;
        private bool _rotating;
        private float _rotationY;

        private unsafe void ReleaseUnmanagedResources()
        {
            _commandList->Release();
            _rootSignature->Release();
            _pipelineState->Release();
            _cbvHeap->Release();
            _vertexBuffer->Release();
            _indexBuffer->Release();
            _constantBuffer->Release();
            _vertexShader->Release();
            _pixelShader->Release();
            _constantBuffer->Unmap(0, null);
            _mappedConstantBuffer = null;
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