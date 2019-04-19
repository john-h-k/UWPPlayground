using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UWPPlayground.Common
{
    public readonly unsafe struct UnmanagedSpan<T> : IEnumerable<T>, IDisposable
        where T : unmanaged
    {
        private readonly T* _pointer;
        private readonly int _length;
        private readonly Action _disposer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnmanagedSpan(void* pointer, int length, Action disposer = null)
        {
            if (pointer == null)
            {
                this = default;
                return;
            }

            ThrowNonPositive(length);
            _pointer = (T*) pointer;
            _length = length;
            _disposer = disposer;
            // DO NOT use '() => {};' or 'delegate { }' as the default
            // This requires an allocation every time, and setting to null is far cheaper
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnmanagedSpan(T* pointer, int length, Action disposer = null)
        {
            if (pointer == null)
            {
                this = default;
                return;
            }

            ThrowNonPositive(length);
            _pointer = pointer;
            _length = length * sizeof(T);
            _disposer = disposer;
        }
        

        public Span<T> AsSpan() => new Span<T>(_pointer, _length);

        public bool IsAligned => new UIntPtr(_pointer).ToUInt64() % (ulong)IntPtr.Size == 0;

        private static void ThrowNonPositive(int length)
        {
            if (length < 1)
                ThrowHelper.ThrowArgOutOfRange("Length less than 1");
        }

        public int Length => _length;
        public bool IsEmpty => _pointer == null || _length is 0;
        public static UnmanagedSpan<T> Empty => default;

        [IndexerName("Item")]
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index >= _length)
                    ThrowHelper.ThrowArgOutOfRange();

                return ref _pointer[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureNotNullAndNotEmpty()
        {
            if (_pointer == null || _length == 0)
                ThrowHelper.ThrowInvalidOp(
                    nameof(UnmanagedSpan<T>) + " is either null or empty");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            // Not calling 'Fill' with 0 to prevent branching and check of size
            EnsureNotNullAndNotEmpty();
            Unsafe.InitBlockUnaligned(ref Unsafe.AsRef<byte>(_pointer), 0, (uint)_length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(T value)
        {
            EnsureNotNullAndNotEmpty();
            if (sizeof(T) is 1)
            {
                T temp = value; // copying prevents taking address of arg, which would affect perf of 
                                // not 'InitBlock' below
                Unsafe.InitBlockUnaligned(ref Unsafe.AsRef<byte>(_pointer), Unsafe.As<T, byte>(ref temp), (uint)_length);
            }
            else
            {
                for (var i = 0; i < _length; i++)
                {
                    this[i] = value;
                    // TODO fast impl
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(UnmanagedSpan<T> dest)
        {
            EnsureNotNullAndNotEmpty();
            Unsafe.CopyBlockUnaligned(dest._pointer, _pointer, (uint)_length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<T> dest)
        {
            fixed (T* tPtr = dest)
            {
                EnsureNotNullAndNotEmpty();
                Unsafe.CopyBlockUnaligned(tPtr, _pointer, (uint)_length);
            }
        }

        public ref T GetPinnableReference() => ref Unsafe.AsRef<T>(_pointer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetEnumerator()
            => new Enumerator(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj is UnmanagedSpan<T> span)
            {
                return Equals(span);
            }

            return false;
        }

        public T[] ToArray()
        {
            var array = new T[_length];

            fixed (void* ptr = array)
            {
                Unsafe.CopyBlockUnaligned(ptr, _pointer, (uint)_length);
            }

            return array;
        }

        public bool TryCopyTo(UnmanagedSpan<T> destination)
        {
            try
            {
                EnsureNotNullAndNotEmpty();
                destination.EnsureNotNullAndNotEmpty();
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            throw new NotImplementedException();
        }

        public T* AsPointer() => _pointer;

        public UnmanagedSpan<T> Slice(int start)
           => new UnmanagedSpan<T>(_pointer + start, _length - start);

        public UnmanagedSpan<T> Slice(int start, int length)
            => new UnmanagedSpan<T>(_pointer + start, length);

        public override string ToString() =>
            default(T) is char
                ? new string((char*)_pointer, 0, _length)
                : GetType().ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => (unchecked((int)(long)_pointer) * 397) ^ _length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(UnmanagedSpan<T> other)
            => this._pointer == other._pointer && this._length == other._length;
        // Explicitly doesn't compare the disposer

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(UnmanagedSpan<T> left, UnmanagedSpan<T> right)
            => left._pointer == right._pointer && left._length == right._length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(UnmanagedSpan<T> left, UnmanagedSpan<T> right)
            => !(left == right);

        public void Dispose()
        {
            _disposer?.Invoke();
        }

        private struct Enumerator : IEnumerator<T>
        {
            private readonly UnmanagedSpan<T> _span;
            private int _offset;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(UnmanagedSpan<T> span)
            {
                _offset = 0;
                _span = span;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (_offset <= _span._length)
                {
                    return false;
                }
                _offset++;
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => _offset = 0;

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _span._pointer[_offset];
            }

            // Get the boxing perf hit anyway, so no aggressiveinlining
            object IEnumerator.Current => Current;

            public void Dispose()
            {

            }
        }
    }

    public static class SpanExtensions
    {
        public static unsafe void CopyTo<T>(this Span<T> source, UnmanagedSpan<T> dest)
            where T : unmanaged
        {
            if (source.Length > dest.Length)
                ThrowHelper.ThrowArgOutOfRange();
            fixed (T* sourcePtr = source)
            fixed (T* destPtr = &dest.GetPinnableReference())
            {
                Unsafe.CopyBlockUnaligned(destPtr, sourcePtr, (uint)source.Length);
            }
        }
    }
}