using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Game.Utility
{
    public static class EnumUtility
    {
        public static bool HasFlagUnsafe<TEnum>(TEnum lhs, TEnum rhs) where TEnum :
#if CSHARP_7_3_OR_NEWER
            unmanaged, Enum
#else
            struct
#endif
        {

            unsafe
            {
#if CSHARP_7_3_OR_NEWER
                switch (sizeof(TEnum))
                {
                    case 1:
                        return (*(byte*)(&lhs) & *(byte*)(&rhs)) > 0;
                    case 2:
                        return (*(ushort*)(&lhs) & *(ushort*)(&rhs)) > 0;
                    case 4:
                        return (*(uint*)(&lhs) & *(uint*)(&rhs)) > 0;
                    case 8:
                        return (*(ulong*)(&lhs) & *(ulong*)(&rhs)) > 0;
                    default:
                        throw new Exception("Size does not match a known Enum backing type.");
                }

#else
                switch (UnsafeUtility.SizeOf<TEnum>())
                {
                    case 1:
                        {
                            byte valLhs = 0;
                            UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
                            byte valRhs = 0;
                            UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
                            return (valLhs & valRhs) > 0;
                        }
                    case 2:
                        {
                            ushort valLhs = 0;
                            UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
                            ushort valRhs = 0;
                            UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
                            return (valLhs & valRhs) > 0;
                        }
                    case 4:
                        {
                            uint valLhs = 0;
                            UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
                            uint valRhs = 0;
                            UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
                            return (valLhs & valRhs) > 0;
                        }
                    case 8:
                        {
                            ulong valLhs = 0;
                            UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
                            ulong valRhs = 0;
                            UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
                            return (valLhs & valRhs) > 0;
                        }
                    default:
                        throw new Exception("Size does not match a known Enum backing type.");
                }
#endif
            }
        }

        public static void HasFlagUnsafe<TEnum>(TEnum lhs, TEnum rhs, out bool res) where TEnum :
            unmanaged, Enum
        {
            unsafe
            {
                switch (sizeof(TEnum))
                {
                    case 1:
                        res = (*(byte*)(&lhs) & *(byte*)(&rhs)) > 0;
                        return;
                    case 2:
                        res = (*(ushort*)(&lhs) & *(ushort*)(&rhs)) > 0;
                        return;
                    case 4:
                        res = (*(uint*)(&lhs) & *(uint*)(&rhs)) > 0;
                        return;
                    case 8:
                        res = (*(ulong*)(&lhs) & *(ulong*)(&rhs)) > 0;
                        return;
                }
            }
            res = false;
        }


#if CSHARP_7_3_OR_NEWER
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl((MethodImplOptions)256)]
#endif
        public static TEnum AddFlag<TEnum>(this TEnum lhs, TEnum rhs) where TEnum :
#if CSHARP_7_3_OR_NEWER
                unmanaged, Enum
#else
                struct
#endif
        {

            unsafe
            {
#if CSHARP_7_3_OR_NEWER
                switch (sizeof(TEnum))
                {
                    case 1:
                        {
                            var r = *(byte*)(&lhs) | *(byte*)(&rhs);
                            return *(TEnum*)&r;
                        }
                    case 2:
                        {
                            var r = *(ushort*)(&lhs) | *(ushort*)(&rhs);
                            return *(TEnum*)&r;
                        }
                    case 4:
                        {
                            var r = *(uint*)(&lhs) | *(uint*)(&rhs);
                            return *(TEnum*)&r;
                        }
                    case 8:
                        {
                            var r = *(ulong*)(&lhs) | *(ulong*)(&rhs);
                            return *(TEnum*)&r;
                        }
                    default:
                        throw new Exception("Size does not match a known Enum backing type.");
                }

#else
                 
            switch (UnsafeUtility.SizeOf<TEnum>())
            {
                case 1:
                    {
                        byte valLhs = 0;
                        UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
                        byte valRhs = 0;
                        UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
                        var result = (valLhs | valRhs);
                        void * r = &result;
                        TEnum o;
                        UnsafeUtility.CopyPtrToStructure(r, out o);
                        return o;
                    }
                case 2:
                    {
                        ushort valLhs = 0;
                        UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
                        ushort valRhs = 0;
                        UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
                        var result = (valLhs | valRhs);
                        void* r = &result;
                        TEnum o;
                        UnsafeUtility.CopyPtrToStructure(r, out o);
                        return o;
                    }
                case 4:
                    {
                        uint valLhs = 0;
                        UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
                        uint valRhs = 0;
                        UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
                        var result = (valLhs | valRhs);
                        void* r = &result;
                        TEnum o;
                        UnsafeUtility.CopyPtrToStructure(r, out o);
                        return o;
                    }
                case 8:
                    {
                        ulong valLhs = 0;
                        UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
                        ulong valRhs = 0;
                        UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
                        var result = (valLhs | valRhs);
                        void* r = &result;
                        TEnum o;
                        UnsafeUtility.CopyPtrToStructure(r, out o);
                        return o;
                    }
                default:
                    throw new Exception("Size does not match a known Enum backing type.");
            }
#endif
            }
        }


#if CSHARP_7_3_OR_NEWER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl((MethodImplOptions)256)]
#endif
        public static TEnum RemoveFlag<TEnum>(this TEnum lhs, TEnum rhs) where TEnum :
#if CSHARP_7_3_OR_NEWER
                unmanaged, Enum
#else
                struct
#endif
        {

            unsafe
            {
#if CSHARP_7_3_OR_NEWER
                switch (sizeof(TEnum))
                {
                    case 1:
                        {
                            var r = *(byte*)(&lhs) & ~*(byte*)(&rhs);
                            return *(TEnum*)&r;
                        }
                    case 2:
                        {
                            var r = *(ushort*)(&lhs) & ~*(ushort*)(&rhs);
                            return *(TEnum*)&r;
                        }
                    case 4:
                        {
                            var r = *(uint*)(&lhs) & ~*(uint*)(&rhs);
                            return *(TEnum*)&r;
                        }
                    case 8:
                        {
                            var r = *(ulong*)(&lhs) & ~*(ulong*)(&rhs);
                            return *(TEnum*)&r;
                        }
                    default:
                        throw new Exception("Size does not match a known Enum backing type.");
                }

#else
                 
            switch (UnsafeUtility.SizeOf<TEnum>())
            {
                case 1:
                    {
                        byte valLhs = 0;
                        UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
                        byte valRhs = 0;
                        UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
                        var result = (valLhs & ~valRhs);
                        void * r = &result;
                        TEnum o;
                        UnsafeUtility.CopyPtrToStructure(r, out o);
                        return o;
                    }
                case 2:
                    {
                        ushort valLhs = 0;
                        UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
                        ushort valRhs = 0;
                        UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
                        var result = (valLhs & ~valRhs);
                        void* r = &result;
                        TEnum o;
                        UnsafeUtility.CopyPtrToStructure(r, out o);
                        return o;
                    }
                case 4:
                    {
                        uint valLhs = 0;
                        UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
                        uint valRhs = 0;
                        UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
                        var result = (valLhs & ~valRhs);
                        void* r = &result;
                        TEnum o;
                        UnsafeUtility.CopyPtrToStructure(r, out o);
                        return o;
                    }
                case 8:
                    {
                        ulong valLhs = 0;
                        UnsafeUtility.CopyStructureToPtr(ref lhs, &valLhs);
                        ulong valRhs = 0;
                        UnsafeUtility.CopyStructureToPtr(ref rhs, &valRhs);
                        var result = (valLhs & ~valRhs);
                        void* r = &result;
                        TEnum o;
                        UnsafeUtility.CopyPtrToStructure(r, out o);
                        return o;
                    }
                default:
                    throw new Exception("Size does not match a known Enum backing type.");
            }
#endif
            }

        }

#if CSHARP_7_3_OR_NEWER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetFlag<TEnum>(ref this TEnum lhs, TEnum rhs) where TEnum : unmanaged, Enum
        {

            unsafe
            {
                fixed (TEnum* lhs1 = &lhs)
                {
                    switch (sizeof(TEnum))
                    {
                        case 1:
                            {
                                var r = *(byte*)(lhs1) | *(byte*)(&rhs);
                                *lhs1 = *(TEnum*)&r;
                                return;
                            }
                        case 2:
                            {
                                var r = *(ushort*)(lhs1) | *(ushort*)(&rhs);
                                *lhs1 = *(TEnum*)&r;
                                return;
                            }
                        case 4:
                            {
                                var r = *(uint*)(lhs1) | *(uint*)(&rhs);
                                *lhs1 = *(TEnum*)&r;
                                return;
                            }
                        case 8:
                            {
                                var r = *(ulong*)(lhs1) | *(ulong*)(&rhs);
                                *lhs1 = *(TEnum*)&r;
                                return;
                            }
                        default:
                            throw new Exception("Size does not match a known Enum backing type.");
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearFlag<TEnum>(this ref TEnum lhs, TEnum rhs) where TEnum : unmanaged, Enum
        {

            unsafe
            {
                fixed (TEnum* lhs1 = &lhs)
                {
                    switch (sizeof(TEnum))
                    {
                        case 1:
                            {
                                var r = *(byte*)(lhs1) & ~*(byte*)(&rhs);
                                *lhs1 = *(TEnum*)&r;
                                return;
                            }
                        case 2:
                            {
                                var r = *(ushort*)(lhs1) & ~*(ushort*)(&rhs);
                                *lhs1 = *(TEnum*)&r;
                                return;
                            }
                        case 4:
                            {
                                var r = *(uint*)(lhs1) & ~*(uint*)(&rhs);
                                *lhs1 = *(TEnum*)&r;
                                return;
                            }
                        case 8:
                            {
                                var r = *(ulong*)(lhs1) & ~*(ulong*)(&rhs);
                                *lhs1 = *(TEnum*)&r;
                                return;
                            }
                        default:
                            throw new Exception("Size does not match a known Enum backing type.");
                    }
                }
            }
        }
#endif
    }
}
