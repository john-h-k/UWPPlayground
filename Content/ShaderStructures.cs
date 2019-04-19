using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace UWPPlayground.Content
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public struct ModelViewProjectionConstantBuffer
    {
        public Matrix4x4 model;
        public Matrix4x4 view;
        public Matrix4x4 projection;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public struct VertexPositionColor
    {
        public Vector3 pos;
        public Vector3 color;
    }
}