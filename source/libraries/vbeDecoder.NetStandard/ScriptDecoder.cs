using System;
using System.Collections.Generic;
using System.IO;

namespace vbeDecoder
{
    /// <summary>
    /// Decode a Visual Basic Encoded script (VBE) / JScript Encoded (JSE).
    /// </summary>
    public static class ScriptDecoder
    {
        #region Constants
        // VBE / Visual Basic Encoded script signatures
        private const string VBE_SIG_START = "#@~^";
        private const string VBE_SIG_START_ALT = "==";
        private const string VBE_SIG_END = "==^#~@";
        private const string VBE_ERROR_MISSING_SIG = "Missing VBE signature.";


        // Other constants
        private const int VBE_PERM_IDX_SIZE = 0x40;
        private const int VBE_PERM_TRIPLET_SIZE = 0x80;
        #endregion

        #region Globals
        private static readonly string[] _permTripletTokens = new string[VBE_PERM_TRIPLET_SIZE]
        {
            "\x00\x00\x00","\x01\x01\x01","\x02\x02\x02","\x03\x03\x03","\x04\x04\x04","\x05\x05\x05","\x06\x06\x06","\x07\x07\x07",
            "\x08\x08\x08","\x57\x6E\x7B","\x4A\x4C\x41","\x0B\x0B\x0B","\x0C\x0C\x0C","\x4A\x4C\x41","\x0E\x0E\x0E","\x0F\x0F\x0F",
            "\x10\x10\x10","\x11\x11\x11","\x12\x12\x12","\x13\x13\x13","\x14\x14\x14","\x15\x15\x15","\x16\x16\x16","\x17\x17\x17",
            "\x18\x18\x18","\x19\x19\x19","\x1A\x1A\x1A","\x1B\x1B\x1B","\x1C\x1C\x1C","\x1D\x1D\x1D","\x1E\x1E\x1E","\x1F\x1F\x1F",
            "\x2E\x2D\x32","\x47\x75\x30","\x7A\x52\x21","\x56\x60\x29","\x42\x71\x5B","\x6A\x5E\x38","\x2F\x49\x33","\x26\x5C\x3D",
            "\x49\x62\x58","\x41\x7D\x3A","\x34\x29\x35","\x32\x36\x65","\x5B\x20\x39","\x76\x7C\x5C","\x72\x7A\x56","\x43\x7F\x73",
            "\x38\x6B\x66","\x39\x63\x4E","\x70\x33\x45","\x45\x2B\x6B","\x68\x68\x62","\x71\x51\x59","\x4F\x66\x78","\x09\x76\x5E",
            "\x62\x31\x7D","\x44\x64\x4A","\x23\x54\x6D","\x75\x43\x71","\x4A\x4C\x41","\x7E\x3A\x60","\x4A\x4C\x41","\x5E\x7E\x53",
            "\x40\x4C\x40","\x77\x45\x42","\x4A\x2C\x27","\x61\x2A\x48","\x5D\x74\x72","\x22\x27\x75","\x4B\x37\x31","\x6F\x44\x37",
            "\x4E\x79\x4D","\x3B\x59\x52","\x4C\x2F\x22","\x50\x6F\x54","\x67\x26\x6A","\x2A\x72\x47","\x7D\x6A\x64","\x74\x39\x2D",
            "\x54\x7B\x20","\x2B\x3F\x7F","\x2D\x38\x2E","\x2C\x77\x4C","\x30\x67\x5D","\x6E\x53\x7E","\x6B\x47\x6C","\x66\x34\x6F",
            "\x35\x78\x79","\x25\x5D\x74","\x21\x30\x43","\x64\x23\x26","\x4D\x5A\x76","\x52\x5B\x25","\x63\x6C\x24","\x3F\x48\x2B",
            "\x7B\x55\x28","\x78\x70\x23","\x29\x69\x41","\x28\x2E\x34","\x73\x4C\x09","\x59\x21\x2A","\x33\x24\x44","\x7F\x4E\x3F",
            "\x6D\x50\x77","\x55\x09\x3B","\x53\x56\x55","\x7C\x73\x69","\x3A\x35\x61","\x5F\x61\x63","\x65\x4B\x50","\x46\x58\x67",
            "\x58\x3B\x51","\x31\x57\x49","\x69\x22\x4F","\x6C\x6D\x46","\x5A\x4D\x68","\x48\x25\x7C","\x27\x28\x36","\x5C\x46\x70",
            "\x3D\x4A\x6E","\x24\x32\x7A","\x79\x41\x2F","\x37\x3D\x5F","\x60\x5F\x4B","\x51\x4F\x5A","\x20\x42\x2C","\x36\x65\x57"
        };

        private static readonly int[] _permIdx = new int[VBE_PERM_IDX_SIZE] {
            0, 1, 2, 0, 1, 2, 1, 2, 2, 1, 2, 1, 0, 2, 1, 2,
            0, 2, 1, 2, 0, 0, 1, 2, 2, 1, 0, 2, 1, 2, 2, 1,
            0, 0, 2, 1, 2, 1, 2, 0, 2, 0, 0, 1, 2, 0, 2, 1,
            0, 2, 1, 2, 0, 0, 1, 2, 2, 0, 0, 1, 2, 0, 2, 1
        };
        #endregion

        #region Public Methods
        /// <summary>
        /// Decode a Visual Basic Encoded script (VBE) file.
        /// </summary>
        /// <param name="path">Path to the encoded Visual Basic script file.</param>
        /// <returns>The decoded Visual Basic script (VBS).</returns>
        public static string DecodeFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException($"File \"{path}\" not found.", path);

            string vbe = File.ReadAllText(path);
            return DecodeScript(vbe);
        }

        /// <summary>
        /// Decode a Visual Basic Encoded script (VBE) from a Stream.
        /// </summary>
        /// <param name="scriptStream">Encoded Visual Basic script stream.</param>
        /// <returns>The decoded Visual Basic script (VBS).</returns>
        public static string DecodeStream(Stream scriptStream)
        {
            if (scriptStream == null)
                throw new ArgumentNullException(nameof(scriptStream));

            if (scriptStream.CanSeek)
                scriptStream.Seek(0, SeekOrigin.Begin);

            if (scriptStream.CanRead)
            {
                StreamReader streamReader = new StreamReader(scriptStream);
                string scriptContent = streamReader.ReadToEnd();
                return DecodeScript(scriptContent);
            }

            throw new Exception("Can't read the stream.");
        }

        /// <summary>
        /// Decode a Visual Basic Encoded script (VBE).
        /// </summary>
        /// <param name="encodedScript">Encoded Visual Basic script.</param>
        /// <returns>The decoded Visual Basic script (VBS).</returns>
        public static string DecodeScript(string encodedScript)
        {
            if (string.IsNullOrWhiteSpace(encodedScript))
                throw new ArgumentNullException(encodedScript);

            string vbe = Unwrap(encodedScript);
            return DecodeTokens(vbe);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Unwrap a Visual Basic Encoded script (VBE) from it's VBE layer.
        /// </summary>
        /// <param name="wrappedScript"></param>
        /// <returns>The unwrapped VBE tokens.</returns>
        private static string Unwrap(string wrappedScript)
        {
            if (string.IsNullOrWhiteSpace(wrappedScript))
                throw new ArgumentNullException(wrappedScript);

            var iTagBeginPos = wrappedScript.IndexOf(VBE_SIG_START);

            if (iTagBeginPos >= 0)
            {
                var iEndTagBegin = wrappedScript.IndexOf(VBE_SIG_START_ALT, iTagBeginPos);


                if (iEndTagBegin >= 0)
                {
                    var startPosition = iEndTagBegin + VBE_SIG_START_ALT.Length;
                    wrappedScript = wrappedScript.Substring(startPosition);

                    var iTagEnd = wrappedScript.IndexOf(VBE_SIG_END, 0);

                    if (iTagEnd > 0)
                        wrappedScript = wrappedScript.Substring(0, iTagEnd - VBE_SIG_END.Length);

                    return wrappedScript;
                }
            }

            throw new NotSupportedException(VBE_ERROR_MISSING_SIG);
        }

        private static string DecodeTokens(string encodedScript)
        {
            if (string.IsNullOrWhiteSpace(encodedScript))
                return default;

            char[] script = Unescape(encodedScript);

            var index = -1;
            var pos = 0;

            string result;

            foreach (char c in script)
            {
                
                if (c < VBE_PERM_TRIPLET_SIZE)
                    index++;

                if ((c == 9 || (c > 31 && c < 128)) && c != 60 && c != 62 && c != 64)
                    script[pos] = (_permTripletTokens[c][_permIdx[index % VBE_PERM_IDX_SIZE]]);
                else
                    script[pos] = c;
                

                pos++;
            }

            return new string(script);
        }

        private static char[] Unescape(string encodedScript)
        {
            if (string.IsNullOrWhiteSpace(encodedScript))
                return default;

            return encodedScript
                .Replace("@*", ">")
                .Replace("@!", "<")
                .Replace("@$", "@")
                .Replace("@&", "\xA")
                .Replace("@#", "\xD")
                .ToCharArray();
        }
        #endregion
    }
}
