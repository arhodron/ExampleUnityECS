using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using System.Runtime.CompilerServices;

namespace Plugins.Collections
{
    [StructLayout(LayoutKind.Sequential)]
    //[NativeContainer]
    public unsafe struct UnsafeArray<T> : IDisposable where T : unmanaged
    {
        [NativeDisableUnsafePtrRestriction]
        internal void* m_Buffer;
        internal int m_Length;
        internal Allocator m_AllocatorLabel;

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Buffer != null;
        }

        public UnsafeArray(int lenght, Allocator allocator)//, params T[] initialItems
        {
            m_Length = lenght;
            m_AllocatorLabel = allocator;

            int totalSize = UnsafeUtility.SizeOf<T>() * m_Length;
            m_Buffer = UnsafeUtility.MallocTracked(totalSize, UnsafeUtility.AlignOf<T>(), m_AllocatorLabel, 1);

            var handle = GCHandle.Alloc(new T[lenght], GCHandleType.Pinned);
            try
            {
                UnsafeUtility.MemCpy(m_Buffer, handle.AddrOfPinnedObject().ToPointer(), totalSize);
            }
            finally
            {
                handle.Free();
            }
        }

        public int Length
        {
            get
            {
                return m_Length;
            }
        }

        public T this[int index]
        {
            get
            {
                return UnsafeUtility.ReadArrayElement<T>(m_Buffer, index);
            }

            set
            {
                UnsafeUtility.WriteArrayElement(m_Buffer, index, value);
            }
        }
        public NativeArray<T> AsArray()
        {
            var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(m_Buffer, m_Length, Allocator.None);
            return array;
        }

        public void Dispose()
        {
            UnsafeUtility.FreeTracked(m_Buffer, m_AllocatorLabel);
            m_Buffer = null;
            m_Length = 0;
        }
    }
}