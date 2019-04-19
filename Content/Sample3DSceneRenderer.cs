using System;
using System.Collections.Generic;
using System.IO;
using TerraFX.Interop;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using UWPPlayground.Common;
using UWPPlayground.Common.d3dx12;
using static UWPPlayground.Common.DirectXHelper;
using static TerraFX.Interop.D3DCommon;
using static TerraFX.Interop.D3D12;
using static TerraFX.Interop.D3D12_ROOT_SIGNATURE_FLAGS;

namespace UWPPlayground.Content
{
    public partial class Sample3DSceneRenderer : IDisposable
    {
        private static partial class Sample3DSceneRendererHelper
        {
        }

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        protected virtual unsafe void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                // Managed disposal (COM Ptr in future? TODO?)
            }

            _constantBuffer->Unmap(0, null);
            _mappedConstantBuffer = null;
        }

        ~Sample3DSceneRenderer()
        {
            Dispose(false);
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
            }

            var vertexShaderLen = (IntPtr)new FileInfo("SampleVertexShader.cs").Length;
            IntPtr vertexShaderMem = Marshal.AllocHGlobal(vertexShaderLen);
            _vertexShader = new UnmanagedSpan<byte>((void*)vertexShaderMem, (int)vertexShaderLen, 
                () => Marshal.FreeHGlobal(vertexShaderMem) // TODO better alternative?
                );

            var pixelShaderLen = (IntPtr)new FileInfo("SamplePixelShader.cs").Length;
            IntPtr pixelShaderMem = Marshal.AllocHGlobal(vertexShaderLen);
            _vertexShader = new UnmanagedSpan<byte>((void*)pixelShaderMem, (int)pixelShaderLen,
                () => Marshal.FreeHGlobal(pixelShaderMem) // TODO better alternative?
                );
        }

        public void CreateWindowSizeDependentResources()
        {
        }

        public void Update(ref StepTimer timer)
        {
        }

        public bool Render()
        {
            throw null;
        }

        public void SaveState()
        {
        }

        public void StartTracking()
        {
        }

        public void TrackingUpdate(float positionX)
        {
        }

        public void StopTracking()
        {
        }

        public bool IsTracking() => _tracking;

        public void ToggleTracking() => _tracking = !_tracking;

        private void LoadState()
        {
            IPropertySet state = ApplicationData.Current.LocalSettings.Values;

            if (state.ContainsKey(AngleKey))
            {
                _rotationY = ((IPropertyValue)state[AngleKey]).GetSingle();
            }

            if (state.ContainsKey(TrackingKey))
            {
                _tracking = ((IPropertyValue)state[TrackingKey]).GetBoolean();
                state.Remove(TrackingKey);
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
        private RECT _scissorRect;
        private UnmanagedSpan<byte> _vertexShader;
        private UnmanagedSpan<byte> _pixelShader;
        private D3D12_VERTEX_BUFFER_VIEW _vertexBufferView;
        private D3D12_INDEX_BUFFER_VIEW _indexBufferView;

        private bool _loadingComplete;
        private float _radiansPerSecond;
        private bool _tracking;
        private bool _rotating;
        private float _rotationY;
    }
}