﻿using System;
using System.Diagnostics.CodeAnalysis;
using TerraFX.Interop;

namespace UWPPlayground.Common.d3dx12
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class CD3DX12_RANGE
    {
        public static D3D12_RANGE Create(UIntPtr begin, UIntPtr end)
        {
            return new D3D12_RANGE
            {
                Begin = begin,
                End = end
            };
        }
    }
}