using System.Text;

namespace JT808
{
    /// <summary>
    /// JT808 library initializer.
    /// Call <see cref="Initialize"/> once at application startup to register required encodings.
    /// </summary>
    public static class JT808Initializer
    {
        private static bool _initialized;

        /// <summary>
        /// Register required code page encodings (GBK/GB2312).
        /// Must be called before encoding/decoding messages that contain Chinese characters.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _initialized = true;
        }
    }
}
