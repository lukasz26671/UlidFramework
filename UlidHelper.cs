using System;
using System.Runtime.InteropServices;

namespace UlidFramework
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct UlidInteropStruct
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] data;
        
        public static implicit operator UlidInteropStruct(Ulid ulid) => new UlidInteropStruct()
        {
            data = ulid.Bytes
        };
        public static implicit operator Ulid(UlidInteropStruct ulidInterop) => new Ulid(ulidInterop.data);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct UlidParseStruct
    {
        public bool success;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] data;
    }

    internal static class UlidHelper
    {
        private const string DllName = "UlidInterop.dll";
        [DllImport(DllName, EntryPoint = "NewUlid", CallingConvention = CallingConvention.Cdecl)]
        public static extern void NewUlid(ref UlidInteropStruct str);

        [DllImport(DllName, EntryPoint = "NewUlidRng", CallingConvention = CallingConvention.Cdecl)]
        public static extern void NewUlid(ref UlidInteropStruct str, int rng);

        [DllImport(DllName, EntryPoint = "CompareUlid", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CompareUlid(ref UlidInteropStruct ulid1, ref UlidInteropStruct ulid2);

        [DllImport(DllName, EntryPoint = "TryParse", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool TryParse([MarshalAs(UnmanagedType.LPStr)] string str, ref UlidParseStruct @struct);

        [DllImport(DllName, EntryPoint = "ExtractTime", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint ExtractTimeEpoch(ref UlidInteropStruct ulid);

        [DllImport(DllName, EntryPoint = "ToString", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ToString(ref UlidInteropStruct ulid);

        public static string ToStringSafe(ref Ulid ulid)
        {
            var @struct = (UlidInteropStruct)ulid;
            IntPtr strPtr = ToString(ref @struct);
            return Marshal.PtrToStringAnsi(strPtr);
        }

        public static DateTime ExtractDateTimeUtc(ref Ulid ulid)
        {
            var @struct = (UlidInteropStruct)ulid;
            uint timestamp = ExtractTimeEpoch(ref @struct);

            DateTime dateTime = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(timestamp) - DateTimeOffset.Now.Offset;

            return dateTime;
        }

        public static DateTime ExtractDateTime(ref Ulid ulid)
        {
            var @struct = (UlidInteropStruct)ulid;
            uint timestamp = ExtractTimeEpoch(ref @struct);

            DateTime dateTime = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(timestamp);

            return dateTime;
        }

        public static bool TryParse(string v, out Ulid ulid)
        {
            ulid = new Ulid(new byte[Ulid.UlidLength]);

            if (string.IsNullOrWhiteSpace(v) || v.Length != 26)
            {
                return false;
            }

            try
            {
                // 6 bytes timestamp (48 bits)
                ulid.Bytes[0] = (byte)((Dec[v[0]] << 5) | Dec[v[1]]);
                ulid.Bytes[1] = (byte)((Dec[v[2]] << 3) | (Dec[v[3]] >> 2));
                ulid.Bytes[2] = (byte)((Dec[v[3]] << 6) | (Dec[v[4]] << 1) | (Dec[v[5]] >> 4));
                ulid.Bytes[3] = (byte)((Dec[v[5]] << 4) | (Dec[v[6]] >> 1));
                ulid.Bytes[4] = (byte)((Dec[v[6]] << 7) | (Dec[v[7]] << 2) | (Dec[v[8]] >> 3));
                ulid.Bytes[5] = (byte)((Dec[v[8]] << 5) | Dec[v[9]]);

                //10 bytes of entropy (80 bits)
                ulid.Bytes[6] = (byte)((Dec[v[10]] << 3) | (Dec[v[11]] >> 2));
                ulid.Bytes[7] = (byte)((Dec[v[11]] << 6) | (Dec[v[12]] << 1) | (Dec[v[13]] >> 4));
                ulid.Bytes[8] = (byte)((Dec[v[13]] << 4) | (Dec[v[14]] >> 1));
                ulid.Bytes[9] = (byte)((Dec[v[14]] << 7) | (Dec[v[15]] << 2) | (Dec[v[16]] >> 3));
                ulid.Bytes[10] = (byte)((Dec[v[16]] << 5) | Dec[v[17]]);
                ulid.Bytes[11] = (byte)((Dec[v[18]] << 3) | Dec[v[19]] >> 2);
                ulid.Bytes[12] = (byte)((Dec[v[19]] << 6) | (Dec[v[20]] << 1) | (Dec[v[21]] >> 4));
                ulid.Bytes[13] = (byte)((Dec[v[21]] << 4) | (Dec[v[22]] >> 1));
                ulid.Bytes[14] = (byte)((Dec[v[22]] << 7) | (Dec[v[23]] << 2) | (Dec[v[24]] >> 3));
                ulid.Bytes[15] = (byte)((Dec[v[24]] << 5) | Dec[v[25]]);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static readonly byte[] Dec = new byte [256]
        {
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x01,
            0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E,
            0x0F, 0x10, 0x11, 0xFF, 0x12, 0x13, 0xFF, 0x14, 0x15, 0xFF,
            0x16, 0x17, 0x18, 0x19, 0x1A, 0xFF, 0x1B, 0x1C, 0x1D, 0x1E,
            0x1F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x0A, 0x0B, 0x0C,
            0x0D, 0x0E, 0x0F, 0x10, 0x11, 0xFF, 0x12, 0x13, 0xFF, 0x14,
            0x15, 0xFF, 0x16, 0x17, 0x18, 0x19, 0x1A, 0xFF, 0x1B, 0x1C,
            0x1D, 0x1E, 0x1F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        };
    }
}