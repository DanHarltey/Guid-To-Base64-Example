using System;
using System.Buffers;
using System.Buffers.Text;
using System.Runtime.InteropServices;
using System.Text;

namespace EfficientGuids
{
    public static class GuidExtensions
    {
        private const byte ForwardSlashByte = (byte)'/';
        private const byte DashByte = (byte)'-';
        private const byte PlusByte = (byte)'+';
        private const byte UnderscoreByte = (byte)'_';

        public static string EncodeBase64String(this Guid guid)
        {
            Span<byte> guidBytes = stackalloc byte[16];
            Span<byte> encodedBytes = stackalloc byte[24];

            MemoryMarshal.TryWrite(guidBytes, ref guid); // write bytes from the Guid
            Base64.EncodeToUtf8(guidBytes, encodedBytes, out _, out _);

            // replace any characters which are not URL safe
            for (var i = 0; i < 22; i++)
            {
                if (encodedBytes[i] == ForwardSlashByte)
                    encodedBytes[i] = DashByte;

                if (encodedBytes[i] == PlusByte)
                    encodedBytes[i] = UnderscoreByte;
            }

            // skip the last two bytes as these will be '==' padding
            var final = Encoding.UTF8.GetString(encodedBytes.Slice(0, 22));

            return final;
        }

        private const char Underscore = '_';
        private const char Dash = '-';

        public static string EncodeBase64StringImproved(this Guid guid)
        {
            Span<byte> guidBytes = stackalloc byte[16];
            Span<byte> encodedBytes = stackalloc byte[24];

            MemoryMarshal.TryWrite(guidBytes, ref guid); // write bytes from the Guid
            Base64.EncodeToUtf8(guidBytes, encodedBytes, out _, out _);

            Span<char> chars = stackalloc char[22];

            // replace any characters which are not URL safe
            // skip the final two bytes as these will be '==' padding we don't need
            for (var i = 0; i < 22; i++)
            {
                switch (encodedBytes[i])
                {
                    case ForwardSlashByte:
                        chars[i] = Dash;
                        break;
                    case PlusByte:
                        chars[i] = Underscore;
                        break;
                    default:
                        chars[i] = (char)encodedBytes[i];
                        break;
                }
            }

            var final = new string(chars);

            return final;
        }

        // The cached delegate is needed to avoid the compiler allocating a new one every time.
        private static readonly SpanAction<char, Guid> WriteBase64GuidDelegate = WriteBase64Guid;

        public static string ToBase64(this Guid guid)
        {
            string final = string.Create(22, guid, WriteBase64GuidDelegate);
            return final;
        }

        private static void WriteBase64Guid(Span<char> chars, Guid toWrite)
        {
            // custom Url safe Base64 format, last two chars changed
            const string Base64Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-";

            Span<byte> guidBytes = stackalloc byte[16];

            // write bytes from the Guid
            MemoryMarshal.TryWrite(guidBytes, ref toWrite);

            int byteCount = 0;

            // Base64 encoding algorithm, it turns 3 bytes into 4 chars
            for (var i = 0; i < 20; i += 4)
            {
                chars[i] = Base64Chars[guidBytes[byteCount] >> 2];
                chars[i + 1] = Base64Chars[((guidBytes[byteCount] & 0x03) << 4) | (guidBytes[byteCount + 1] >> 4)];
                chars[i + 2] = Base64Chars[((guidBytes[byteCount + 1] & 0x0f) << 2) | (guidBytes[byteCount + 2] >> 6)];
                chars[i + 3] = Base64Chars[guidBytes[byteCount + 2] & 0x3f];

                byteCount += 3;
            }

            // last two bytes to encode. Add no Base64 padding "=="
            chars[20] = Base64Chars[guidBytes[byteCount] >> 2];
            chars[21] = Base64Chars[((guidBytes[byteCount] & 0x03) << 4)];
        }

        // The cached delegate is needed to avoid the compiler allocating a new one every time.
        private static readonly SpanAction<char, Guid> WriteBase64GuidUnrolledDelegate = WriteBase64GuidUnrolled;

        public static string ToBase64Unrolled(this Guid guid)
        {
            string final = string.Create(22, guid, WriteBase64GuidUnrolledDelegate);
            return final;
        }

        private static void WriteBase64GuidUnrolled(Span<char> chars, Guid toWrite)
        {
            // custom Url safe Base64 format, last two chars changed
            const string Base64Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-";

            Span<byte> guidBytes = stackalloc byte[16];

            // write bytes from the Guid
            MemoryMarshal.TryWrite(guidBytes, ref toWrite);

            /*
             * Base64 encoding algorithm, it turns 3 bytes into 4 chars.
             * This goes from last to first, so the compiler can remove bounds checking from the Span's
             * This is could be done in a loop, but it is Unrolled for performance
             */

            // last two bytes to encode. Add no Base64 padding "=="
            chars[21] = Base64Chars[(guidBytes[15] & 0x03) << 4];
            chars[20] = Base64Chars[guidBytes[15] >> 2];

            chars[19] = Base64Chars[guidBytes[14] & 0x3f];
            chars[18] = Base64Chars[((guidBytes[13] & 0x0f) << 2) | (guidBytes[14] >> 6)];
            chars[17] = Base64Chars[((guidBytes[12] & 0x03) << 4) | (guidBytes[13] >> 4)];
            chars[16] = Base64Chars[guidBytes[12] >> 2];

            chars[15] = Base64Chars[guidBytes[11] & 0x3f];
            chars[14] = Base64Chars[((guidBytes[10] & 0x0f) << 2) | (guidBytes[11] >> 6)];
            chars[13] = Base64Chars[((guidBytes[9] & 0x03) << 4) | (guidBytes[10] >> 4)];
            chars[12] = Base64Chars[guidBytes[9] >> 2];

            chars[11] = Base64Chars[guidBytes[8] & 0x3f];
            chars[10] = Base64Chars[((guidBytes[7] & 0x0f) << 2) | (guidBytes[8] >> 6)];
            chars[09] = Base64Chars[((guidBytes[6] & 0x03) << 4) | (guidBytes[7] >> 4)];
            chars[08] = Base64Chars[guidBytes[6] >> 2];

            chars[07] = Base64Chars[guidBytes[5] & 0x3f];
            chars[06] = Base64Chars[((guidBytes[4] & 0x0f) << 2) | (guidBytes[5] >> 6)];
            chars[05] = Base64Chars[((guidBytes[3] & 0x03) << 4) | (guidBytes[4] >> 4)];
            chars[04] = Base64Chars[guidBytes[3] >> 2];

            chars[03] = Base64Chars[guidBytes[2] & 0x3f];
            chars[02] = Base64Chars[((guidBytes[1] & 0x0f) << 2) | (guidBytes[2] >> 6)];
            chars[01] = Base64Chars[((guidBytes[0] & 0x03) << 4) | (guidBytes[1] >> 4)];
            chars[00] = Base64Chars[guidBytes[0] >> 2];
        }
    }
}
