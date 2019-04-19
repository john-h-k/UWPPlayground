using System;

namespace UWPPlayground.Common
{
    public static class ThrowHelper
    {
        public static void GenericThrow(Exception e)
            => throw e;

        public static void ThrowDisposed(string name = null, Exception inner = null)
            => throw new ObjectDisposedException(name, inner);

        public static void ThrowInvalidOp(string message = null, Exception inner = null)
            => throw new InvalidOperationException(message, inner);

        public static void ThrowOutOfMemory(string message = null, Exception inner = null)
            => throw new OutOfMemoryException(message, inner);

        #region ARG EXCEPTIONS

        public static void ThrowArgNull(string name = null, Exception inner = null)
            => throw new ArgumentNullException(name, inner);

        public static void ThrowArgOutOfRange(string message = null, Exception inner = null)
            => throw new ArgumentOutOfRangeException(message, inner);

        public static void ThrowArgEx(string message = null, Exception inner = null)
            => throw new ArgumentException(message, inner);

        #endregion

        // TODO
    }
}