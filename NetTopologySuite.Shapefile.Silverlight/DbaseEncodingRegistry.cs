// Copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using GisSharpBlog.NetTopologySuite.Shapefile;
using NetTopologySuite.Encodings;

namespace GisSharpBlog.NetTopologySuite.Shapefile
{
    /// <summary>
    /// A registry used to map CLR cultures and encodings to 
    /// dBase language driver id (LDID) encoding specifiers.
    /// </summary>
    /// <remarks>
    /// Lookup values are taken from ESRI's 
    /// <a href="http://downloads.esri.com/support/documentation/pad_/ArcPad_RefGuide_1105.pdf">
    /// ArcPad Reference Guide</a>
    /// </remarks>
    internal static class DbaseLocaleRegistry
    {
        #region Nested types
        /// <summary>
        /// Specifies which code page to use in a <see cref="TextInfo"/>
        /// instance.
        /// </summary>
        enum CodePageChoice
        {
            Custom,
            Oem,
            Ansi,
            Mac
        }

        /// <summary>
        /// Represents a culture and encoding pair.
        /// </summary>
        struct CultureWithEncoding
        {
            private readonly Encoding _encoding;
            public readonly CultureInfo CultureInfo;
            public readonly CodePageChoice CodePageChoice;

            internal CultureWithEncoding(CultureInfo cultureInfo, CodePageChoice codePageChoice)
            {
                CultureInfo = cultureInfo;
                CodePageChoice = codePageChoice;
                _encoding = null;
            }

            internal CultureWithEncoding(CultureInfo cultureInfo, Encoding encoding)
            {
                CultureInfo = cultureInfo;
                _encoding = encoding;
                CodePageChoice = CodePageChoice.Custom;
            }

            public Encoding Encoding
            {
                get
                {
                    if (_encoding != null)
                    {
                        return _encoding;
                    }

                    throw new NotImplementedException();
                    //switch (CodePageChoice)
                    //{
                    //    case CodePageChoice.Ansi:
                    //        return Encoding.GetEncoding(CultureInfo.TextInfo.ANSICodePage);
                    //    case CodePageChoice.Mac:
                    //        return Encoding.GetEncoding(CultureInfo.TextInfo.MacCodePage);
                    //    case CodePageChoice.Oem:
                    //    case CodePageChoice.Custom:
                    //    default:
                    //        return Encoding.GetEncoding(CultureInfo.TextInfo.OEMCodePage);
                    //}
                }
            }
        }
        #endregion

        private static readonly Dictionary<Byte, CultureWithEncoding> _dbaseToEncoding
            = new Dictionary<Byte, CultureWithEncoding>();
        private static readonly Dictionary<KeyValuePair<Int32, Int32>, Byte> _encodingToDbase
            = new Dictionary<KeyValuePair<Int32, Int32>, Byte>();
        private static IEncodingRegistry _encodingRegistry = new EncodingRegistry();

        static DbaseLocaleRegistry()
        {
            setupDbaseToEncodingMap();
            setupEncodingToDbaseMap();
        }

        // Values from the ArcPad reference guide.
        // Found here: http://downloads.esri.com/support/documentation/pad_/ArcPad_RefGuide_1105.pdf
        private static void setupDbaseToEncodingMap()
        {

            _dbaseToEncoding[0x01] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1033), CodePageChoice.Oem); //DOS USA code page 437 
            _dbaseToEncoding[0x02] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1033), _encodingRegistry.GetEncoding(850)); // DOS Multilingual code page 850 
            _dbaseToEncoding[0x03] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1033), CodePageChoice.Ansi); // Windows ANSI code page 1252 
            _dbaseToEncoding[0x04] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1033), CodePageChoice.Mac); // Macintosh US English 
            _dbaseToEncoding[0x08] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1030), _encodingRegistry.GetEncoding(865)); // Danish OEM
            _dbaseToEncoding[0x09] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1043), _encodingRegistry.GetEncoding(437)); // Dutch OEM
            _dbaseToEncoding[0x0A] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1043), CodePageChoice.Oem); // Dutch OEM Secondary codepage
            _dbaseToEncoding[0x0B] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1035), _encodingRegistry.GetEncoding(437)); // Finnish OEM
            _dbaseToEncoding[0x0D] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1036), _encodingRegistry.GetEncoding(437)); // French OEM
            _dbaseToEncoding[0x0E] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1036), CodePageChoice.Oem); // French OEM Secondary codepage
            _dbaseToEncoding[0x0F] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1031), _encodingRegistry.GetEncoding(437)); // German OEM
            _dbaseToEncoding[0x10] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1031), CodePageChoice.Oem); // German OEM Secondary codepage
            _dbaseToEncoding[0x11] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(16), _encodingRegistry.GetEncoding(437)); // Italian OEM
            _dbaseToEncoding[0x12] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(16), CodePageChoice.Oem); // Italian OEM Secondary codepage
            _dbaseToEncoding[0x13] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(17), CodePageChoice.Oem); // Japanese Shift-JIS
            _dbaseToEncoding[0x14] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(10), CodePageChoice.Oem); // Spanish OEM secondary codepage
            _dbaseToEncoding[0x15] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(29), _encodingRegistry.GetEncoding(437)); // Swedish OEM
            _dbaseToEncoding[0x16] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(29), CodePageChoice.Oem); // Swedish OEM secondary codepage
            _dbaseToEncoding[0x17] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(20), _encodingRegistry.GetEncoding(865)); // Norwegian OEM
            _dbaseToEncoding[0x18] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(10), _encodingRegistry.GetEncoding(437)); // Spanish OEM
            _dbaseToEncoding[0x19] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(2057), _encodingRegistry.GetEncoding(437)); // English OEM (Britain)
            _dbaseToEncoding[0x1A] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(2057), CodePageChoice.Oem); // English OEM (Britain) secondary codepage
            _dbaseToEncoding[0x1B] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1033), CodePageChoice.Oem); // English OEM (U.S.)
            _dbaseToEncoding[0x1C] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(3084), _encodingRegistry.GetEncoding(863)); // French OEM (Canada)
            _dbaseToEncoding[0x1D] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(12), CodePageChoice.Oem); // French OEM secondary codepage
            _dbaseToEncoding[0x1F] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(5), CodePageChoice.Oem); // Czech OEM
            _dbaseToEncoding[0x22] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(14), CodePageChoice.Oem); // Hungarian OEM
            _dbaseToEncoding[0x23] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(21), CodePageChoice.Oem); // Polish OEM
            _dbaseToEncoding[0x24] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(22), _encodingRegistry.GetEncoding(860)); // Portuguese OEM
            _dbaseToEncoding[0x25] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(22), CodePageChoice.Oem); // Portuguese OEM secondary codepage
            _dbaseToEncoding[0x26] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(25), CodePageChoice.Oem); // Russian OEM
            _dbaseToEncoding[0x37] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1033), _encodingRegistry.GetEncoding(850)); // English OEM (U.S.) secondary codepage
            _dbaseToEncoding[0x40] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(24), CodePageChoice.Oem); // Romanian OEM
            _dbaseToEncoding[0x4D] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(4), CodePageChoice.Oem); // Chinese GBK (PRC)
            _dbaseToEncoding[0x4E] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1042), CodePageChoice.Oem); // Korean (ANSI/OEM)
            _dbaseToEncoding[0x4F] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1028), CodePageChoice.Oem); // Chinese Big5 (Taiwan)
            _dbaseToEncoding[0x50] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(30), CodePageChoice.Oem); // Thai (ANSI/OEM)
            _dbaseToEncoding[0x57] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1033), CodePageChoice.Ansi); // ANSI
            _dbaseToEncoding[0x58] = new CultureWithEncoding(CultureInfo.InvariantCulture, CodePageChoice.Ansi); // Western European ANSI
            _dbaseToEncoding[0x59] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(10), CodePageChoice.Ansi); // Spanish ANSI
            _dbaseToEncoding[0x64] = new CultureWithEncoding(CultureInfo.InvariantCulture, _encodingRegistry.GetEncoding(852)); // Eastern European MS–DOS
            _dbaseToEncoding[0x65] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(25), CodePageChoice.Oem); // Russian MS–DOS
            _dbaseToEncoding[0x66] = new CultureWithEncoding(CultureInfo.InvariantCulture, _encodingRegistry.GetEncoding(865)); // Nordic MS–DOS
            _dbaseToEncoding[0x67] = new CultureWithEncoding(CultureInfo.InvariantCulture, _encodingRegistry.GetEncoding(861)); // Icelandic MS–DOS
            //_dbaseToEncoding[0x68] = Encoding.GetEncoding(895); // Kamenicky (Czech) MS-DOS 
            //_dbaseToEncoding[0x69] = Encoding.GetEncoding(620); // Mazovia (Polish) MS-DOS 
            _dbaseToEncoding[0x6A] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(8), CodePageChoice.Oem); // Greek MS–DOS (437G)
            _dbaseToEncoding[0x6B] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(31), CodePageChoice.Oem); // Turkish MS–DOS
            _dbaseToEncoding[0x6C] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(3084), _encodingRegistry.GetEncoding(863)); // French–Canadian MS–DOS
            _dbaseToEncoding[0x78] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1028), CodePageChoice.Oem); // Taiwan Big 5
            _dbaseToEncoding[0x79] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1042), CodePageChoice.Oem); // Hangul (Wansung)
            _dbaseToEncoding[0x7A] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(2052), CodePageChoice.Oem); // PRC GBK
            _dbaseToEncoding[0x7B] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(17), CodePageChoice.Oem); // Japanese Shift-JIS
            _dbaseToEncoding[0x7C] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(30), CodePageChoice.Oem); // Thai Windows/MS–DOS
            _dbaseToEncoding[0x7D] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(13), CodePageChoice.Ansi); // Hebrew Windows 
            _dbaseToEncoding[0x7E] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(1), CodePageChoice.Ansi); // Arabic Windows 
            _dbaseToEncoding[0x86] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(8), CodePageChoice.Oem); // Greek OEM
            _dbaseToEncoding[0x87] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(27), CodePageChoice.Oem); // Slovenian OEM
            _dbaseToEncoding[0x88] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(31), CodePageChoice.Oem); // Turkish OEM
            _dbaseToEncoding[0x96] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(25), CodePageChoice.Mac); // Russian Macintosh 
            _dbaseToEncoding[0x97] = new CultureWithEncoding(CultureInfo.InvariantCulture, _encodingRegistry.GetEncoding(10029)); // Eastern European Macintosh 
            _dbaseToEncoding[0x98] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(8), CodePageChoice.Mac); // Greek Macintosh 
            _dbaseToEncoding[0xC8] = new CultureWithEncoding(CultureInfo.InvariantCulture, _encodingRegistry.GetEncoding(1250)); // Eastern European Windows
            _dbaseToEncoding[0xC9] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(25), CodePageChoice.Ansi); // Russian Windows
            _dbaseToEncoding[0xCA] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(31), CodePageChoice.Ansi); // Turkish Windows
            _dbaseToEncoding[0xCB] = new CultureWithEncoding(GlobalizationHelper.GetCultureInfo(8), CodePageChoice.Ansi); // Greek Windows
            _dbaseToEncoding[0xCC] = new CultureWithEncoding(CultureInfo.InvariantCulture, _encodingRegistry.GetEncoding(1257)); // Baltic Windows


        }

        private static void setupEncodingToDbaseMap()
        {
            foreach (KeyValuePair<Byte, CultureWithEncoding> item in _dbaseToEncoding)
            {
                // These encodings are duplicated. When a dBase file is created by
                // the DbaseFile.Create method, these will not be used in the LDID
                // since it would result in an ambiguous lookup.
                if (item.Key == 0x01 || item.Key == 0x02 || item.Key == 0x03 || item.Key == 0x26 ||
                    item.Key == 0x6C || item.Key == 0x78 || item.Key == 0x79 || item.Key == 0x7B ||
                    item.Key == 0x7C || item.Key == 0x86 || item.Key == 0x88)
                {
                    continue;
                }

                Int32 lcid = GlobalizationHelper.GetLCID(item.Value.CultureInfo);
                Int32 codePage = item.Value.Encoding.CodePage();
                _encodingToDbase.Add(new KeyValuePair<Int32, Int32>(lcid, codePage), item.Key);
            }
        }

        /// <summary>
        /// Gets the <see cref="CultureInfo"/> associated with the 
        /// given dBase LDID.
        /// </summary>
        /// <param name="dBaseEncoding">
        /// The language driver id (LDID) to get a CultureInfo for.
        /// </param>
        /// <returns>
        /// A <see cref="CultureInfo"/> which uses the encoding represented by 
        /// <paramref name="dBaseEncoding"/> by default.
        /// </returns>
        public static CultureInfo GetCulture(Byte dBaseEncoding)
        {
            CultureWithEncoding pair;

            return _dbaseToEncoding.TryGetValue(dBaseEncoding, out pair)
                       ? pair.CultureInfo
                       : CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Gets the <see cref="Encoding"/> which matches the 
        /// given dBase LDID.
        /// </summary>
        /// <param name="dBaseEncoding">
        /// The language driver id (LDID) to get the <see cref="Encoding"/> for.
        /// </param>
        /// <returns>
        /// An <see cref="Encoding"/> which corresponds to the the
        /// <paramref name="dBaseEncoding"/> code established by ESRI.
        /// </returns>
        public static Encoding GetEncoding(Byte dBaseEncoding)
        {
            CultureWithEncoding pair;

            return _dbaseToEncoding.TryGetValue(dBaseEncoding, out pair)
                       ? pair.Encoding
                       : EncodingEx.GetACSII();
        }

        /// <summary>
        /// Gets the language driver id (LDID) for a given pair of <see cref="CultureInfo"/>
        /// and <see cref="Encoding"/>.
        /// </summary>
        /// <param name="info">The CultureInfo used to lookup the LDID.</param>
        /// <param name="encoding">The Encoding used to lookup the LDID.</param>
        /// <returns>
        /// A language driver code used to specify the encoding used to write text in the dBase file.
        /// </returns>
        public static Byte GetLanguageDriverCode(CultureInfo info, Encoding encoding)
        {
            Byte driverCode;
            KeyValuePair<Int32, Int32> key = new KeyValuePair<Int32, Int32>(GlobalizationHelper.GetLCID(info), _encodingRegistry.GetCodePage(encoding));

            return _encodingToDbase.TryGetValue(key, out driverCode)
                       ? driverCode
                       : (Byte)0x1;
        }
    }
}
