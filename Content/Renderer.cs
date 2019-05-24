using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop;
using Windows.Storage;
using UWPPlayground.Common;
using UWPPlayground.Common.d3dx12;
using static TerraFX.Interop.D3D_PRIMITIVE_TOPOLOGY;
using static TerraFX.Interop.D3D_ROOT_SIGNATURE_VERSION;
using static TerraFX.Interop.D3D12;
using static TerraFX.Interop.D3D12_CLEAR_FLAGS;
using static TerraFX.Interop.D3D12_COMMAND_LIST_TYPE;
using static TerraFX.Interop.D3D12_DESCRIPTOR_HEAP_FLAGS;
using static TerraFX.Interop.D3D12_DESCRIPTOR_HEAP_TYPE;
using static TerraFX.Interop.D3D12_DESCRIPTOR_RANGE_TYPE;
using static TerraFX.Interop.D3D12_HEAP_FLAGS;
using static TerraFX.Interop.D3D12_HEAP_TYPE;
using static TerraFX.Interop.D3D12_INPUT_CLASSIFICATION;
using static TerraFX.Interop.D3D12_PRIMITIVE_TOPOLOGY_TYPE;
using static TerraFX.Interop.D3D12_RESOURCE_STATES;
using static TerraFX.Interop.D3D12_ROOT_SIGNATURE_FLAGS;
using static TerraFX.Interop.D3D12_SHADER_VISIBILITY;
using static TerraFX.Interop.D3DCompiler;
using static TerraFX.Interop.DX;
using static TerraFX.Interop.DXGI_FORMAT;
using static TerraFX.Interop.PIX;
using static TerraFX.Interop.Windows;


namespace UWPPlayground.Content
{
    public abstract class Renderer
    {
        public abstract void Update(ref StepTimer timer);
        public abstract bool Render();
        public abstract void CreateWindowSizeDependentResources();
        public abstract void SaveState();
    }
}
