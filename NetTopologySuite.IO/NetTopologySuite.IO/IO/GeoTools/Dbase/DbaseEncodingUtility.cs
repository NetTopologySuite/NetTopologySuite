using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NetTopologySuite.IO
{
    internal static class DbaseEncodingUtility
    {
        /// <summary>
        /// The Latin1 Encoding
        /// </summary>
        internal static readonly Encoding Latin1 = Encoding.GetEncoding("iso-8859-1");

        /// <summary>
        /// The default Encoding
        /// </summary>
#if !PCL
        public static Encoding DefaultEncoding { get; set; } = Encoding.Default;
#else
        public static Encoding DefaultEncoding { get; set; } = Encoding.GetEncoding(string.Empty);
#endif
        /// <summary>
        /// Association of language driver id (ldid) to encoding
        /// </summary>
        internal static readonly IDictionary<byte, Encoding> LdidToEncoding;

        /// <summary>
        /// Association of encoding to language driver id (ldid)
        /// </summary>
        internal static readonly IDictionary<Encoding, byte> EncodingToLdid;

        static DbaseEncodingUtility()
        {
            object[][] dbfCodePages =
            {
                new object[] { 0x01 , 437 },  // U.S. MSDOS
                new object[] { 0x02 , 850 },  // International MSDOS
                new object[] { 0x08 , 865 },  // Danish OEM
                new object[] { 0x09 , 437 },  // Dutch OEM
                new object[] { 0x0A , 850 },  // Dutch OEM*
                new object[] { 0x0B , 437 },  // Finnish OEM
                new object[] { 0x0D , 437 },  // French OEM
                new object[] { 0x0E , 850 },  // French OEM*
                new object[] { 0x0F , 437 },  // German OEM
                new object[] { 0x10 , 850 },  // German OEM*
                new object[] { 0x11 , 437 },  // Italian OEM
                new object[] { 0x12 , 850 },  // Italian OEM*
                new object[] { 0x13 , 932 },  // Japanese Shift-JIS
                new object[] { 0x14 , 850 },  // Spanish OEM*
                new object[] { 0x15 , 437 },  // Swedish OEM
                new object[] { 0x16 , 850 },  // Swedish OEM*
                new object[] { 0x17 , 865 },  // Norwegian OEM
                new object[] { 0x18 , 437 },  // Spanish OEM
                new object[] { 0x19 , 437 },  // English OEM (Britain)
                new object[] { 0x1A , 850 },  // English OEM (Britain)*
                new object[] { 0x1B , 437 },  // English OEM (U.S.)
                new object[] { 0x1C , 863 },  // French OEM (Canada)
                new object[] { 0x1D , 850 },  // French OEM*
                new object[] { 0x1F , 852 },  // Czech OEM
                new object[] { 0x22 , 852 },  // Hungarian OEM
                new object[] { 0x23 , 852 },  // Polish OEM
                new object[] { 0x24 , 860 },  // Portuguese OEM
                new object[] { 0x25 , 850 },  // Portuguese OEM*
                new object[] { 0x26 , 866 },  // Russian OEM
                new object[] { 0x37 , 850 },  // English OEM (U.S.)*
                new object[] { 0x40 , 852 },  // Romanian OEM
                new object[] { 0x4D , 936 },  // Chinese GBK (PRC)
                new object[] { 0x4E , 949 },  // Korean (ANSI/OEM)
                new object[] { 0x4F , 950 },  // Chinese Big5 (Taiwan)
                new object[] { 0x50 , 874 },  // Thai (ANSI/OEM)
                new object[] { 0x58 , 1252 }, // Western European ANSI
                new object[] { 0x59 , 1252 }, // Spanish ANSI
                new object[] { 0x64 , 852 },  // Eastern European MSDOS
                new object[] { 0x65 , 866 },  // Russian MSDOS
                new object[] { 0x66 , 865 },  // Nordic MSDOS
                new object[] { 0x67 , 861 },  // Icelandic MSDOS
                new object[] { 0x6A , 737 },  // Greek MSDOS (437G)
                new object[] { 0x6B , 857 },  // Turkish MSDOS
                new object[] { 0x6C , 863 },  // FrenchCanadian MSDOS
                new object[] { 0x78 , 950 },  // Taiwan Big 5
                new object[] { 0x79 , 949 },  // Hangul (Wansung)
                new object[] { 0x7A , 936 },  // PRC GBK
                new object[] { 0x7B , 932 },  // Japanese Shift-JIS
                new object[] { 0x7C , 874 },  // Thai Windows/MSDOS
                new object[] { 0x86 , 737 },  // Greek OEM
                new object[] { 0x87 , 852 },  // Slovenian OEM
                new object[] { 0x88 , 857 },  // Turkish OEM
                new object[] { 0xC8 , 1250 }, // Eastern European Windows
                new object[] { 0xC9 , 1251 }, // Russian Windows
                new object[] { 0xCA , 1254 }, // Turkish Windows
                new object[] { 0xCB , 1253 }, // Greek Windows
                new object[] { 0xCC , 1257 }  // Baltic Windows
            };

            LdidToEncoding = new Dictionary<byte, Encoding>();
            EncodingToLdid = new Dictionary<Encoding, byte>();

            // Register predefined language driver codes with their respective encodings
            RegisterEncodings(dbfCodePages);

            // Add ANSI values 3 and 0x57 as system's default encoding, and 0 which means no encoding.
            AddLdidEncodingPair(0, Encoding.UTF8);
#if !PCL
            AddLdidEncodingPair(0x03, Encoding.Default);
            AddLdidEncodingPair(0x57, Encoding.Default);
#else
            AddLdidEncodingPair(0x03, Encoding.GetEncoding(string.Empty));
            AddLdidEncodingPair(0x57, Encoding.GetEncoding(string.Empty));
#endif
        }
        /*
        private static void AddLdidEncodingPair(byte ldid, int codePage)
        {
            Encoding encToAdd;
            if (!TryGetEncoding("windows-" + codePage, out encToAdd) &&
                !TryGetEncoding(codePage, out encToAdd))
                return;
            AddLdidEncodingPair(ldid, encToAdd);
        }
        */

        private static void AddLdidEncodingPair(byte ldid, Encoding encodingToAdd)
        {
            LdidToEncoding.Add(ldid, encodingToAdd);
            if (!EncodingToLdid.ContainsKey(encodingToAdd))
                EncodingToLdid.Add(encodingToAdd, ldid);
        }

        /*
        /// <summary>
        /// Method to get an encoding base on the <paramref name="codePage"/> number
        /// </summary>
        /// <param name="codePage">The number of the codepage</param>
        /// <param name="encoding">The encoding</param>
        /// <returns><value>true</value> if getting the encoding was successfull, otherwise <value>false</value>.</returns>
        private static bool TryGetEncoding(int codePage, out Encoding encoding)
        {
            encoding = null;
            try
            {
                encoding = Encoding.GetEncoding(codePage);
                return encoding != Encoding.ASCII;
            }
            catch { return false; }
        }

        /// <summary>
        /// Method to get an encoding base on the <paramref name="codePageName"/>
        /// </summary>
        /// <param name="codePageName">The name of the codepage</param>
        /// <param name="encoding">The encoding</param>
        /// <returns><value>true</value> if getting the encoding was successfull, otherwise <value>false</value>.</returns>
        private static bool TryGetEncoding(string codePageName, out Encoding encoding)
        {
            encoding = null;
            try
            {
                encoding = Encoding.GetEncoding(codePageName);
                return true;
            }
            catch { return false; }
        }
        */
#if !PCL
        private static void RegisterEncodings(object[][] ldidCodePagePairs)
        {
            var tmp = new Dictionary<int, EncodingInfo>();
            foreach (EncodingInfo ei in Encoding.GetEncodings())
                tmp.Add(ei.CodePage, ei);

            foreach (var ldidCodePagePair in ldidCodePagePairs)
            {
                EncodingInfo ei;
                if (tmp.TryGetValue((int)ldidCodePagePair[1], out ei))
                {
                    var enc = ei.GetEncoding();
                    AddLdidEncodingPair(Convert.ToByte(ldidCodePagePair[0]), enc);
                }
                else
                {
                    var message = string.Format("Failed to get codepage for language driver {0}", ldidCodePagePair[0]);
                    Debug.WriteLine(message);
                }
            }
        }
#else
        private static void RegisterEncodings(object[][] ldidCodePagePairs)
        {
            // ToDo: For PCL
        }
#endif
    }
}