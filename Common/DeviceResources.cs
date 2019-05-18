using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Core;
using TerraFX.Interop;
using UWPPlayground.Common.d3dx12;
using static TerraFX.Interop.Windows;

using HRESULT = System.Int32;

namespace UWPPlayground.Common
{
    public static class DisplayMetrics
    {
        // High resolution displays can require a lot of GPU and battery power to render.
        // High resolution phones, for example, may suffer from poor battery life if
        // games attempt to render at 60 frames per second at full fidelity.
        // The decision to render at full fidelity across all platforms and form factors
        // should be deliberate.
        public const bool SupportHighResolutions = false;

        // The default thresholds that define a "high resolution" display. If the thresholds
        // are exceeded and SupportHighResolutions is false, the dimensions will be scaled
        // by 50%.
        public const float DpiThreshold = 192.0f;       // 200% of standard desktop display.
        public const float WidthThreshold = 1920.0f;    // 1080p width.
        public const float HeightThreshold = 1080.0f;	// 1080p height.
    };

    // Constants used to calculate screen rotations.
    public static class ScreenRotation
    {
        // 0-degree Z-rotation
        public static readonly Matrix4x4 Rotation0 = new Matrix4x4(
            1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f
        );

        // 90-degree Z-rotation
        public static readonly Matrix4x4 Rotation90 = new Matrix4x4(
            0.0f, 1.0f, 0.0f, 0.0f,
            -1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f
        );

        // 180-degree Z-rotation
        public static readonly Matrix4x4 Rotation180 = new Matrix4x4(
            -1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, -1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f
        );

        // 270-degree Z-rotation
        public static readonly Matrix4x4 Rotation270 = new Matrix4x4(
            0.0f, -1.0f, 0.0f, 0.0f,
            1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f
        );
    };

    public sealed unsafe class DeviceResources : IDisposable
    {
        #region Fields

        public const int FrameCount = 3;

        private int _currentFrame;
        // Direct3D objects.
        private ComPtrField<ID3D12Device> _d3dDevice;
        private ComPtrField<IDXGIFactory4> _dxgiFactory;
        private ComPtrField<IDXGISwapChain3> _swapChain;
        private RenderTargets_e__FixedBuffer _renderTargets;
        private ComPtrField<ID3D12Resource> _depthStencil;
        private ComPtrField<ID3D12DescriptorHeap> _rtvHeap;
        private ComPtrField<ID3D12DescriptorHeap> _dsvHeap;
        private ComPtrField<ID3D12CommandQueue> _commandQueue;
        private CommandAllocators_e__FixedBuffer _commandAllocators;
        private DXGI_FORMAT _backBufferFormat;
        private DXGI_FORMAT _depthBufferFormat;
        private D3D12_VIEWPORT _screenViewport;
        private uint _rtvDescriptorSize;
        private bool _deviceRemoved;

        // CPU/GPU Synchronization.
        private ComPtrField<ID3D12Fence> _fence;
        private FenceValues_e__FixedBuffer _fenceValues;
        private IntPtr _fenceEvent;

        // Cached reference to the Window.
        private CoreWindow _window;

        // Cached device properties.
        private Size _d3dRenderTargetSize;
        private Size _outputSize;
        private Size _logicalSize;
        private DisplayOrientations _nativeOrientation;
        private DisplayOrientations _currentOrientation;
        private float _dpi;

        // This is the DPI that will be reported back to the app. It takes into account whether the app supports high resolution screens or not.
        private float _effectiveDpi;

        // Transforms used for display orientation.
        private Matrix4x4 _orientationTransform3D;

        #endregion

        #region Structs
        private unsafe struct CommandAllocators_e__FixedBuffer
        {
            #region Fields
#pragma warning disable CS0649
            public ID3D12CommandAllocator* e0;

            public ID3D12CommandAllocator* e1;

            public ID3D12CommandAllocator* e2;
#pragma warning restore CS0649
            #endregion

            #region Properties
            public ID3D12CommandAllocator* this[int index]
            {
                get
                {
                    fixed (ID3D12CommandAllocator** e = &e0)
                    {
                        return e[index];
                    }
                }

                set
                {
                    fixed (ID3D12CommandAllocator** e = &e0)
                    {
                        e[index] = value;
                    }
                }
            }
            #endregion
        }

        private unsafe struct FenceValues_e__FixedBuffer
        {
            #region Fields
#pragma warning disable CS0649
            public ulong e0;

            public ulong e1;

            public ulong e2;
#pragma warning restore CS0649
            #endregion

            #region Properties
            public ulong this[int index]
            {
                get
                {
                    fixed (ulong* e = &e0)
                    {
                        return e[index];
                    }
                }

                set
                {
                    fixed (ulong* e = &e0)
                    {
                        e[index] = value;
                    }
                }
            }
            #endregion
        }

        private unsafe struct RenderTargets_e__FixedBuffer
        {
            #region Fields
#pragma warning disable CS0649
            public ID3D12Resource* e0;

            public ID3D12Resource* e1;

            public ID3D12Resource* e2;
#pragma warning restore CS0649
            #endregion

            #region Properties
            public ID3D12Resource* this[int index]
            {
                get
                {
                    fixed (ID3D12Resource** e = &e0)
                    {
                        return e[index];
                    }
                }

                set
                {
                    fixed (ID3D12Resource** e = &e0)
                    {
                        e[index] = value;
                    }
                }
            }
            #endregion
        }
        #endregion

        public DeviceResources(DXGI_FORMAT backBufferFormat = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,
            DXGI_FORMAT depthBufferFormat = DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT)
        {
            _backBufferFormat = backBufferFormat;
            _depthBufferFormat = depthBufferFormat;
            _nativeOrientation = DisplayOrientations.None;
            _currentOrientation = DisplayOrientations.None;
            _dpi = -1.0f;
            _effectiveDpi = _dpi;

            CreateDeviceIndependentResources();
            CreateDeviceResources();
        }

        private void CreateDeviceResources()
        {
#if DEBUG
            Guid iid;
            {
                using ComPtr<ID3D12Debug> debugController = null;

                iid = D3D12.IID_ID3D12Debug;
                if (SUCCEEDED(D3D12.D3D12GetDebugInterface(&iid, (void**)debugController.GetAddressOf())))
                {
                    debugController.Get()->EnableDebugLayer();
                }
            }
#endif
            iid = DXGI.IID_IDXGIFactory4;
            IDXGIFactory4* dxgiFactory;
            DirectXHelper.ThrowIfFailed(DXGI.CreateDXGIFactory1(&iid, (void**)&dxgiFactory));
            _dxgiFactory = dxgiFactory;

            using ComPtr<IDXGIAdapter1> adapter = null;
            GetHardwareAdapter(adapter.ReleaseGetAddressOf());

            ID3D12Device* d3dDevice;
            iid = D3D12.IID_ID3D12Device;
            HRESULT hr;
            {
                hr = D3D12.D3D12CreateDevice(
                   adapter,
                   D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_0,
                   &iid,
                   (void**)&d3dDevice
               );

#if DEBUG
                if (FAILED(hr))
                {
                    using ComPtr<IDXGIAdapter> warpAdapter = null;

                    iid = DXGI.IID_IDXGIAdapter;
                    DirectXHelper.ThrowIfFailed(_dxgiFactory.Get()->EnumWarpAdapter(&iid, (void**)warpAdapter.GetAddressOf()));

                    iid = D3D12.IID_ID3D12Device1;
                    hr = D3D12.D3D12CreateDevice(
                        warpAdapter,
                        D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_0,
                        &iid,
                        (void**)&d3dDevice
                    );
                }
#endif
            }
            _d3dDevice = d3dDevice;

            DirectXHelper.ThrowIfFailed(hr);

            D3D12_COMMAND_QUEUE_DESC queueDesc;
            queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAGS.D3D12_COMMAND_QUEUE_FLAG_NONE;
            queueDesc.Type = D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_DIRECT;

            ID3D12CommandQueue* commandQueue;
            {
                iid = D3D12.IID_ID3D12CommandQueue;
                DirectXHelper.ThrowIfFailed(_d3dDevice.Get()->CreateCommandQueue(&queueDesc, &iid, (void**)&commandQueue));
            }
            _commandQueue = commandQueue;
            DirectXHelper.NameObject(_commandQueue, nameof(_commandQueue));

            D3D12_DESCRIPTOR_HEAP_DESC rtvHeapDesc;
            rtvHeapDesc.NumDescriptors = FrameCount;
            rtvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_RTV;
            rtvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAGS.D3D12_DESCRIPTOR_HEAP_FLAG_NONE;

            ID3D12DescriptorHeap* rtvHeap;
            {
                iid = D3D12.IID_ID3D12DescriptorHeap;
                DirectXHelper.ThrowIfFailed(_d3dDevice.Get()->CreateDescriptorHeap(&rtvHeapDesc, &iid, (void**)&rtvHeap));
            }
            _rtvHeap = rtvHeap;
            DirectXHelper.NameObject(_rtvHeap, nameof(_rtvHeap));

            _rtvDescriptorSize =
                _d3dDevice.Get()->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE
                    .D3D12_DESCRIPTOR_HEAP_TYPE_RTV);



            D3D12_DESCRIPTOR_HEAP_DESC dsvHeapDesc;
            dsvHeapDesc.NumDescriptors = 1;
            dsvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_DSV;
            dsvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAGS.D3D12_DESCRIPTOR_HEAP_FLAG_NONE;

            ID3D12DescriptorHeap* dsvHeap;
            {
                iid = D3D12.IID_ID3D12DescriptorHeap;
                DirectXHelper.ThrowIfFailed(_d3dDevice.Get()->CreateDescriptorHeap(&dsvHeapDesc, &iid, (void**)&dsvHeap));
            }

            _dsvHeap = dsvHeap;
            DirectXHelper.NameObject(_dsvHeap, nameof(_dsvHeap));

            fixed (CommandAllocators_e__FixedBuffer* pBuffer = &_commandAllocators)
            { 
                var p = (ID3D12CommandAllocator**)pBuffer;
                iid = D3D12.IID_ID3D12CommandAllocator;
                for (var n = 0; n < FrameCount; n++)
                {

                    DirectXHelper.ThrowIfFailed(_d3dDevice.Get()->CreateCommandAllocator(
                        D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_DIRECT,
                        &iid,
                        (void**)(p + n)));
                }
            }

            ID3D12Fence* fence;
            {
                iid = D3D12.IID_ID3D12Fence;
                DirectXHelper.ThrowIfFailed(
                    _d3dDevice.Get()->CreateFence(_fenceValues[_currentFrame],
                        D3D12_FENCE_FLAGS.D3D12_FENCE_FLAG_NONE,
                        &iid,
                        (void**)&fence));
                _fenceValues[_currentFrame]++;
            }
            _fence = fence;
            DirectXHelper.NameObject(_fence, nameof(_fence));

            _fenceEvent = Kernel32.CreateEvent(null, FALSE, FALSE, null);
            if (_fenceEvent == IntPtr.Zero)
            {
                DirectXHelper.ThrowIfFailed(Marshal.GetLastWin32Error());
            }
        }

        private void GetHardwareAdapter(IDXGIAdapter1** ppAdapter)
        {
            using ComPtr<IDXGIAdapter1> adapter = null;
            *ppAdapter = null;

            for (uint adapterIndex = 0;
                DXGI_ERROR_NOT_FOUND != _dxgiFactory.Get()->EnumAdapters1(adapterIndex, adapter.GetAddressOf());
                adapterIndex++)
            {
                DXGI_ADAPTER_DESC1 desc;
                adapter.Get()->GetDesc1(&desc);

                if (((DXGI_ADAPTER_FLAG)desc.Flags & DXGI_ADAPTER_FLAG.DXGI_ADAPTER_FLAG_SOFTWARE) != 0)
                {
                    continue;
                }

                Guid iid = D3D12.IID_ID3D12Device;
                if (SUCCEEDED(D3D12.D3D12CreateDevice(adapter, D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_0, &iid, null)))
                {
                    break;
                }
            }

            *ppAdapter = adapter.Detach();
        }

        private void CreateDeviceIndependentResources()
        {

        }

        public void CreateWindowSizeDependentResources()
        {
            WaitForGpu();

            for (int n = 0; n < FrameCount; n++)
            {
                _renderTargets[n] = null;
                _fenceValues[n] = _fenceValues[_currentFrame];
            }

            UpdateRenderTargetSize();

            DXGI_MODE_ROTATION displayRotation = ComputeDisplayRotation();

            bool swapDimensions =
                displayRotation == DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_ROTATE90
                || displayRotation == DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_ROTATE270;

            _d3dRenderTargetSize.Width = swapDimensions ? _outputSize.Height : _outputSize.Width;
            _d3dRenderTargetSize.Height = swapDimensions ? _outputSize.Width : _outputSize.Height;

            uint backBufferWidth = (uint)Math.Round(_d3dRenderTargetSize.Width, MidpointRounding.AwayFromZero);
            uint backBufferHeight = (uint)Math.Round(_d3dRenderTargetSize.Height, MidpointRounding.AwayFromZero);

            if (_swapChain != null)
            {
                HRESULT hr = _swapChain.Get()->ResizeBuffers(FrameCount, backBufferWidth, backBufferHeight, _backBufferFormat,
                    0);

                if (hr == DXGI_ERROR_DEVICE_REMOVED || hr == DXGI_ERROR_DEVICE_RESET)
                {
                    _deviceRemoved = true;

                    return;
                }
                else
                {
                    DirectXHelper.ThrowIfFailed(hr);
                }
            }
            else
            {
                DXGI_SCALING scaling = DisplayMetrics.SupportHighResolutions
                    ? DXGI_SCALING.DXGI_SCALING_NONE
                    : DXGI_SCALING.DXGI_SCALING_STRETCH;

                DXGI_SWAP_CHAIN_DESC1 swapChainDesc;

                swapChainDesc.Width = backBufferWidth;
                swapChainDesc.Height = backBufferHeight;
                swapChainDesc.Format = _backBufferFormat;
                swapChainDesc.Stereo = 0;
                swapChainDesc.SampleDesc.Count = 1;
                swapChainDesc.SampleDesc.Quality = 0;
                swapChainDesc.BufferUsage = DXGI.DXGI_USAGE_RENDER_TARGET_OUTPUT;
                swapChainDesc.BufferCount = FrameCount;
                swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT.DXGI_SWAP_EFFECT_FLIP_DISCARD;
                swapChainDesc.Flags = 0;
                swapChainDesc.Scaling = scaling;
                swapChainDesc.AlphaMode = DXGI_ALPHA_MODE.DXGI_ALPHA_MODE_IGNORE;

                {
                    using ComPtr<IDXGISwapChain1> swapChain = null;
                    IntPtr pWindow = Marshal.GetIUnknownForObject(_window);
                    DirectXHelper.ThrowIfFailed(
                        _dxgiFactory.Get()->CreateSwapChainForCoreWindow(
                            _commandQueue,
                            (IUnknown*)pWindow,
                            &swapChainDesc,
                            null,
                            swapChain.GetAddressOf()));

                    IDXGISwapChain3* swapChain3;
                    Guid iid = DXGI.IID_IDXGISwapChain3;
                    DirectXHelper.ThrowIfFailed(swapChain.Get()->QueryInterface(&iid, (void**)&swapChain3));
                    _swapChain = swapChain3;

                }
            }

            switch (displayRotation)
            {
                case DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_IDENTITY:
                    _orientationTransform3D = ScreenRotation.Rotation0;
                    break;

                case DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_ROTATE90:
                    _orientationTransform3D = ScreenRotation.Rotation270;
                    break;

                case DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_ROTATE180:
                    _orientationTransform3D = ScreenRotation.Rotation180;
                    break;

                case DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_ROTATE270:
                    _orientationTransform3D = ScreenRotation.Rotation90;
                    break;
            }

            DirectXHelper.ThrowIfFailed(_swapChain.Get()->SetRotation(displayRotation));

            {
                _currentFrame = (int)_swapChain.Get()->GetCurrentBackBufferIndex();
                D3D12_CPU_DESCRIPTOR_HANDLE rtvDescriptor;
                _rtvHeap.Get()->GetCPUDescriptorHandleForHeapStart(&rtvDescriptor);

                fixed (void* pBuffer = & _renderTargets)
                {
                    var p = (ID3D12Resource**)pBuffer;

                    Guid iid = D3D12.IID_ID3D12Resource;
                    for (var n = 0; n < FrameCount; n++)
                    {
                        DirectXHelper.ThrowIfFailed(_swapChain.Get()->GetBuffer((uint)n, &iid, (void**)&p[n]));
                        _d3dDevice.Get()->CreateRenderTargetView(
                            _renderTargets[n],
                            null,
                            rtvDescriptor);

                        rtvDescriptor.Offset((int)_rtvDescriptorSize);

                        DirectXHelper.NameObject(_renderTargets[n], $"{nameof(_renderTargets)}[{n}]"); // _renderTargets[n]

                    }
                }
            }

            {
                D3D12_HEAP_PROPERTIES depthHeapProperties = CD3DX12_HEAP_PROPERTIES.Create(D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT);

                D3D12_RESOURCE_DESC depthResourceDesc =
                    CD3DX12_RESOURCE_DESC.Tex2D(_depthBufferFormat, backBufferWidth, backBufferHeight, 1, 1);

                depthResourceDesc.Flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL;

                D3D12_CLEAR_VALUE depthOptimizedClearValue = CD3DX12_CLEAR_VALUE.Create(_depthBufferFormat, 1, 0);

                fixed (ID3D12Resource** p = _depthStencil)
                {
                    Guid iid = D3D12.IID_ID3D12Resource;
                    DirectXHelper.ThrowIfFailed(_d3dDevice.Get()->CreateCommittedResource(
                        &depthHeapProperties,
                        D3D12_HEAP_FLAGS.D3D12_HEAP_FLAG_NONE,
                        &depthResourceDesc,
                        D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_DEPTH_WRITE,
                        &depthOptimizedClearValue,
                        &iid,
                        (void**)p
                    ));

                    DirectXHelper.NameObject(_depthStencil, nameof(_depthStencil));

                    D3D12_DEPTH_STENCIL_VIEW_DESC dsvDesc = default;
                    dsvDesc.Format = _depthBufferFormat;
                    dsvDesc.ViewDimension = D3D12_DSV_DIMENSION.D3D12_DSV_DIMENSION_TEXTURE2D;
                    dsvDesc.Flags = D3D12_DSV_FLAGS.D3D12_DSV_FLAG_NONE;
                    D3D12_CPU_DESCRIPTOR_HANDLE handle;
                    _dsvHeap.Get()->GetCPUDescriptorHandleForHeapStart(&handle);
                    _d3dDevice.Get()->CreateDepthStencilView(_depthStencil.Get(), &dsvDesc, handle);

                }
            }

            // 0.0f, 0.0f, m_d3dRenderTargetSize.Width, m_d3dRenderTargetSize.Height, 0.0f, 1.0f
            _screenViewport = new D3D12_VIEWPORT
            {
                TopLeftX = 0,
                TopLeftY = 0,
                Width = (float)_d3dRenderTargetSize.Width,
                Height = (float)_d3dRenderTargetSize.Height,
                MinDepth = 0,
                MaxDepth = 1
            };
        }

        private DXGI_MODE_ROTATION ComputeDisplayRotation()
        {
            DXGI_MODE_ROTATION rotation = DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_UNSPECIFIED;

            switch (_nativeOrientation)
            {
                case DisplayOrientations.Landscape:
                    switch (_currentOrientation)
                    {
                        case DisplayOrientations.Landscape:
                            rotation = DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_IDENTITY;
                            break;

                        case DisplayOrientations.Portrait:
                            rotation = DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_ROTATE270;
                            break;

                        case DisplayOrientations.LandscapeFlipped:
                            rotation = DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_ROTATE180;
                            break;

                        case DisplayOrientations.PortraitFlipped:
                            rotation = DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_ROTATE90;
                            break;
                    }
                    break;

                case DisplayOrientations.Portrait:
                    switch (_currentOrientation)
                    {
                        case DisplayOrientations.Landscape:
                            rotation = DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_ROTATE90;
                            break;

                        case DisplayOrientations.Portrait:
                            rotation = DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_IDENTITY;
                            break;

                        case DisplayOrientations.LandscapeFlipped:
                            rotation = DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_ROTATE270;
                            break;

                        case DisplayOrientations.PortraitFlipped:
                            rotation = DXGI_MODE_ROTATION.DXGI_MODE_ROTATION_ROTATE180;
                            break;
                    }
                    break;
            }
            return rotation;
        }

        private void UpdateRenderTargetSize()
        {
            _effectiveDpi = _dpi;

            if (!DisplayMetrics.SupportHighResolutions && _dpi > DisplayMetrics.DpiThreshold)
            {
                float width = DirectXHelper.ConvertDipsToPixels((float)_logicalSize.Width, _dpi);
                float height = DirectXHelper.ConvertDipsToPixels((float)_logicalSize.Height, _dpi);

                if (MathF.Max(width, height) > DisplayMetrics.WidthThreshold
                    && MathF.Min(width, height) > DisplayMetrics.HeightThreshold)
                {
                    _effectiveDpi /= 2;
                }
            }

            _outputSize.Width = DirectXHelper.ConvertDipsToPixels((float)_logicalSize.Width, _effectiveDpi);
            _outputSize.Height = DirectXHelper.ConvertDipsToPixels((float)_logicalSize.Height, _effectiveDpi);

            _outputSize.Width = Math.Max(_outputSize.Width, 1);
            _outputSize.Height = Math.Max(_outputSize.Height, 1);
        }

        public void SetWindow(CoreWindow window)
        {
            DisplayInformation currentDisplayInformation = DisplayInformation.GetForCurrentView();

            _window = window;
            _logicalSize = new Size(window.Bounds.Width, window.Bounds.Height);
            _nativeOrientation = currentDisplayInformation.NativeOrientation;
            _currentOrientation = currentDisplayInformation.CurrentOrientation;
            _dpi = currentDisplayInformation.LogicalDpi;

            CreateWindowSizeDependentResources();
        }

        public void SetLogicalSize(Size logicalSize)
        {
            if (_logicalSize != logicalSize)
            {
                _logicalSize = logicalSize;
                CreateWindowSizeDependentResources();
            }
        }

        public void SetCurrentOrientation(DisplayOrientations currentOrientation)
        {
            if (_currentOrientation != currentOrientation)
            {
                _currentOrientation = currentOrientation;
                CreateWindowSizeDependentResources();
            }
        }

        public void SetDpi(float dpi)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_dpi != dpi)
            {
                _dpi = dpi;

                _logicalSize = new Size(_window.Bounds.Width, _window.Bounds.Height);

                CreateWindowSizeDependentResources();
            }
        }

        public void ValidateDevice()
        {
            DXGI_ADAPTER_DESC previousDesc;
            {
                using ComPtr<IDXGIAdapter1> previousDefaultAdapter = null;
                DirectXHelper.ThrowIfFailed(_dxgiFactory.Get()->EnumAdapters1(0, previousDefaultAdapter.GetAddressOf()));
                DirectXHelper.ThrowIfFailed(previousDefaultAdapter.Get()->GetDesc(&previousDesc));
            }

            {
                using ComPtr<IDXGIFactory4> currrentDxgiFactory = null;
                using ComPtr<IDXGIAdapter1> currentDefaultAdapter = null;

                Guid iid = DXGI.IID_IDXGIFactory4;
                DirectXHelper.ThrowIfFailed(DXGI.CreateDXGIFactory1(&iid, (void**)currrentDxgiFactory.GetAddressOf()));


                DirectXHelper.ThrowIfFailed(currrentDxgiFactory.Get()->EnumAdapters1(0, currentDefaultAdapter.GetAddressOf()));

                DXGI_ADAPTER_DESC currentDesc;
                DirectXHelper.ThrowIfFailed(currentDefaultAdapter.Get()->GetDesc(&currentDesc));

                if (previousDesc.AdapterLuid.LowPart != currentDesc.AdapterLuid.LowPart ||
                    previousDesc.AdapterLuid.HighPart != currentDesc.AdapterLuid.HighPart ||
                    FAILED(_d3dDevice.Get()->GetDeviceRemovedReason()))
                {
                    _deviceRemoved = true;
                }
            }
        }

        public void Present()
        {
            HRESULT hr = _swapChain.Get()->Present(1, 0);

            if (hr == DXGI_ERROR_DEVICE_REMOVED || hr == DXGI_ERROR_DEVICE_RESET)
            {
                _deviceRemoved = true;
            }
            else
            {
                DirectXHelper.ThrowIfFailed(hr);
                MoveToNextFrame();
            }
        }

        public void WaitForGpu()
        {
            DirectXHelper.ThrowIfFailed(_commandQueue.Get()->Signal(_fence.Get(), _fenceValues[_currentFrame]));

            DirectXHelper.ThrowIfFailed(_fence.Get()->SetEventOnCompletion(_fenceValues[_currentFrame], _fenceEvent));

            // TODO - proper call is 'WaitForSingleObjectEx(_fenceEvent, INFINITE, FALSE)'
            Kernel32.WaitForSingleObject(_fenceEvent, INFINITE);

            _fenceValues[_currentFrame]++;
        }

        private void MoveToNextFrame()
        {
            ulong currentFenceValue = _fenceValues[_currentFrame];

            DirectXHelper.ThrowIfFailed(_commandQueue.Get()->Signal(_fence.Get(), currentFenceValue));

            _currentFrame = (int)_swapChain.Get()->GetCurrentBackBufferIndex();

            ulong completedValue = _fence.Get()->GetCompletedValue();
            if (completedValue < _fenceValues[_currentFrame])
            {
                DirectXHelper.ThrowIfFailed(_fence.Get()->SetEventOnCompletion(_fenceValues[_currentFrame], _fenceEvent));

                // TODO - proper call is 'WaitForSingleObjectEx(_fenceEvent, INFINITE, FALSE)'
                Kernel32.WaitForSingleObject(_fenceEvent, INFINITE);
            }

            _fenceValues[_currentFrame] = currentFenceValue + 1;
        }

        // The size of the render target, in pixels.
        public Size OutputSize => _outputSize;

        // The size of the render target, in dips.
        public Size LogicalSize => _logicalSize;

        public float Dpi => _effectiveDpi;
        public bool IsDeviceRemoved => _deviceRemoved;

        // D3D Accessors.
        public ID3D12Device* D3DDevice => _d3dDevice.Get();
        public IDXGISwapChain3* SwapChain => _swapChain.Get();
        public ID3D12Resource* RenderTarget => _renderTargets[_currentFrame];
        public ID3D12Resource* DepthStencil => _depthStencil.Get();
        public ID3D12CommandQueue* CommandQueue => _commandQueue.Get();
        public ID3D12CommandAllocator* CommandAllocator => _commandAllocators[_currentFrame];
        public DXGI_FORMAT BackBufferFormat => _backBufferFormat;
        public DXGI_FORMAT DepthBufferFormat => _depthBufferFormat;
        public D3D12_VIEWPORT ScreenViewport => _screenViewport;
        public Matrix4x4 OrientationTransform3D => _orientationTransform3D;
        public uint CurrentFrameIndex => (uint)_currentFrame;
        public CoreWindow Window => _window;

        public D3D12_CPU_DESCRIPTOR_HANDLE GetRenderTargetView()
        {
            D3D12_CPU_DESCRIPTOR_HANDLE handle;
            _rtvHeap.Get()->GetCPUDescriptorHandleForHeapStart(&handle);
            return CD3DX12_CPU_DESCRIPTOR_HANDLE.Create(handle, _currentFrame, _rtvDescriptorSize);
        }

        public D3D12_CPU_DESCRIPTOR_HANDLE GetDepthStencilView()
        {
            D3D12_CPU_DESCRIPTOR_HANDLE handle;
            _dsvHeap.Get()->GetCPUDescriptorHandleForHeapStart(&handle);
            return handle;
        }

        private void ReleaseUnmanagedResources()
        {
            _d3dDevice.Dispose();
            _dxgiFactory.Dispose();
            _swapChain.Dispose();
            _depthStencil.Dispose();
            _rtvHeap.Dispose();
            _dsvHeap.Dispose();
            _commandQueue.Dispose();

            // CPU/GPU Synchronization.
            _fence.Dispose();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~DeviceResources()
        {
            ReleaseUnmanagedResources();
        }
    }
}