using System;
using System.Diagnostics;
using System.IO;
using Windows.ApplicationModel;
using static TerraFX.Interop.Windows;
using static TerraFX.Utilities.ExceptionUtilities;

namespace TerraFX.Interop
{
    public static unsafe class DX
    {
        public static void ThrowIfFailed(string methodName, int hr)
        {
            if (FAILED(hr))
            {
                // Set a breakpoint on this line to catch Win32 API errors.
                ThrowExternalException(methodName, hr);
            }
        }

        public static void ThrowIfFailed(int hr)
        {
            if (FAILED(hr))
            {
                // Set a breakpoint on this line to catch Win32 API errors.
                ThrowExternalException("External exception occured", hr);
            }
        }

        public static string GetAssetsPath()
        {
            var package = Package.Current;
            return Path.GetDirectoryName(package.InstalledLocation.Path);
        }

        public static string GetAssetFullPath(string assetName)
        {
            var assetsPath = GetAssetsPath();
            return Path.Combine(assetsPath, assetName);
        }

        public static byte[] ReadDataFromFile(string filename)
        {
            byte[] data;

            using (var fileReader = File.OpenRead(filename))
            {
                var endOfFile = fileReader.Length;

                if (endOfFile > int.MaxValue)
                {
                    ThrowIOException();
                }

                var size = (int)endOfFile;
                data = new byte[size];

                fileReader.Read(data, 0, size);
            }

            return data;
        }

        // Converts a length in device-independent pixels (DIPs) to a length in physical pixels.
        public static float ConvertDipsToPixels(float dips, float dpi)
        {
            const float dipsPerInch = 96.0f;
            return MathF.Floor(dips * dpi / dipsPerInch + 0.5f); // Round to nearest integer.
        }

        // Assign a name to the object to aid with debugging.

        [Conditional("DEBUG")]
        public static void NameD3D12Object(ID3D12CommandQueue* pObject, string name)
        {
            fixed (char* pName = name)
            {
                pObject->SetName(pName);
            }
        }

        [Conditional("DEBUG")]
        public static void NameD3D12Object(ID3D12DescriptorHeap* pObject, string name)
        {
            fixed (char* pName = name)
            {
                pObject->SetName(pName);
            }
        }


        [Conditional("DEBUG")]
        public static void NameD3D12Object(ID3D12Fence* pObject, string name)
        {
            fixed (char* pName = name)
            {
                pObject->SetName(pName);
            }
        }

        [Conditional("DEBUG")]
        public static void NameD3D12Object(ID3D12GraphicsCommandList* pObject, string name)
        {
            fixed (char* pName = name)
            {
                pObject->SetName(pName);
            }
        }

        [Conditional("DEBUG")]
        public static void NameD3D12Object(ID3D12Resource* pObject, string name)
        {
            fixed (char* pName = name)
            {
                pObject->SetName(pName);
            }
        }

        [Conditional("DEBUG")]
        public static void NameD3D12Object(ID3D12RootSignature* pObject, string name)
        {
            fixed (char* pName = name)
            {
                pObject->SetName(pName);
            }
        }
    }
}
