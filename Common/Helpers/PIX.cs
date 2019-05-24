using static DirectX.Detail;

namespace TerraFX.Interop
{
    public static unsafe class PIX
    {
        public static void PIXSetMarker(ID3D12GraphicsCommandList* pCommandList, ulong metadata, string format)
        {
            var size = (uint)((format.Length + 1) * sizeof(char));

            fixed (char* pFormat = format)
            {
                pCommandList->SetMarker(PIX_EVENT_UNICODE_VERSION, pFormat, size);
            }
        }

        public static void PIXSetMarker(ID3D12CommandQueue* pCommandQueue, ulong metadata, string format)
        {
            var size = (uint)((format.Length + 1) * sizeof(char));

            fixed (char* pFormat = format)
            {
                pCommandQueue->SetMarker(PIX_EVENT_UNICODE_VERSION, pFormat, size);
            }
        }

        public static void PIXSetMarker(ID3D12GraphicsCommandList* pCommandList, ulong metadata, string format, params object[] args)
        {
            string buf = string.Format(format, args);
            var count = (uint)((buf.Length + 1) * sizeof(char));

            fixed (char* pBuf = buf)
            {
                pCommandList->SetMarker(PIX_EVENT_UNICODE_VERSION, pBuf, count);
            }
        }

        public static void PIXSetMarker(ID3D12CommandQueue* pCommandQueue, ulong metadata, string format, params object[] args)
        {
            string buf = string.Format(format, args);
            var count = (uint)((buf.Length + 1) * sizeof(char));

            fixed (char* pBuf = buf)
            {
                pCommandQueue->SetMarker(PIX_EVENT_UNICODE_VERSION, pBuf, count);
            }
        }

        public static void PIXBeginEvent(ID3D12GraphicsCommandList* pCommandList, ulong metadata, string format)
        {
            var size = (uint)((format.Length + 1) * sizeof(char));

            fixed (char* pFormat = format)
            {
                pCommandList->BeginEvent(PIX_EVENT_UNICODE_VERSION, pFormat, size);
            }
        }

        public static void PIXBeginEvent(ID3D12CommandQueue* pCommandQueue, ulong metadata, string format)
        {
            var size = (uint)((format.Length + 1) * sizeof(char));

            fixed (char* pFormat = format)
            {
                pCommandQueue->BeginEvent(PIX_EVENT_UNICODE_VERSION, pFormat, size);
            }
        }

        public static void PIXBeginEvent(ID3D12GraphicsCommandList* pCommandList, ulong metadata, string format, params object[] args)
        {
            string buf = string.Format(format, args);
            var count = (uint)((buf.Length + 1) * sizeof(char));

            fixed (char* pBuf = buf)
            {
                pCommandList->BeginEvent(PIX_EVENT_UNICODE_VERSION, pBuf, count);
            }
        }

        public static void PIXBeginEvent(ID3D12CommandQueue* pCommandQueue, ulong metadata, string format, params object[] args)
        {
            string buf = string.Format(format, args);
            var count = (uint)((buf.Length + 1) * sizeof(char));

            fixed (char* pBuf = buf)
            {
                pCommandQueue->BeginEvent(PIX_EVENT_UNICODE_VERSION, pBuf, count);
            }
        }

        public static void PIXEndEvent(ID3D12GraphicsCommandList* pCommandList)
        {
            pCommandList->EndEvent();
        }

        public static void PIXEndEvent(ID3D12CommandQueue* pCommandQueue)
        {
            pCommandQueue->EndEvent();
        }
    }
}
