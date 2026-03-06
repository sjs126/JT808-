namespace JT808.Extensions
{
    /// <summary>
    /// BCD (Binary Coded Decimal) encoding extensions
    /// </summary>
    public static class JT808BcdExtension
    {
        /// <summary>
        /// Convert a BCD byte array to a string of digits.
        /// Each byte encodes two decimal digits: high nibble first.
        /// </summary>
        public static string ToHexString(byte[] bcdBytes)
        {
            return BitConverter.ToString(bcdBytes).Replace("-", string.Empty);
        }

        /// <summary>
        /// Encode a numeric string (up to 2*byteLength digits) as BCD bytes.
        /// Pads with leading zeros if necessary.
        /// </summary>
        public static byte[] EncodeBcd(string number, int byteLength)
        {
            // Pad to exact digit count
            string padded = number.PadLeft(byteLength * 2, '0');
            if (padded.Length > byteLength * 2)
                padded = padded.Substring(padded.Length - byteLength * 2);

            byte[] result = new byte[byteLength];
            for (int i = 0; i < byteLength; i++)
            {
                int high = padded[i * 2] - '0';
                int low = padded[i * 2 + 1] - '0';
                result[i] = (byte)((high << 4) | low);
            }
            return result;
        }

        /// <summary>
        /// Decode a BCD byte array to a numeric string.
        /// </summary>
        public static string DecodeBcd(byte[] bcdBytes)
        {
            var sb = new System.Text.StringBuilder(bcdBytes.Length * 2);
            foreach (byte b in bcdBytes)
            {
                sb.Append((b >> 4) & 0x0F);
                sb.Append(b & 0x0F);
            }
            return sb.ToString();
        }
    }
}
