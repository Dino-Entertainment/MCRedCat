﻿/*
    Copyright 2015 MCGalaxy
        
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    http://www.opensource.org/licenses/ecl2.php
    http://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;

namespace MCGalaxy {
    /// <summary> Utility methods for reading/writing big endian integers, and fixed length strings. </summary>
    public static class NetUtils {

        /// <summary> Number of bytes a string occupies in a packet.</summary>
        public const int StringSize = 64;        
        
        /// <summary> Reads a 16 bit signed integer, big endian form. </summary>
        public static short ReadI16(byte[] array, int offset) {
            return (short)(array[offset] << 8 | array[offset + 1]);
        }
        
        /// <summary> Reads a 16 bit unsigned integer, big endian form. </summary>
        public static ushort ReadU16(byte[] array, int offset) {
            return (ushort)(array[offset] << 8 | array[offset + 1]);
        }

        /// <summary> Reads a 32 bit signed integer, big endian form. </summary>
        public static int ReadI32(byte[] array, int offset) {
            return array[offset] << 24 | array[offset + 1] << 16
                | array[offset + 2] << 8 | array[offset + 3];
        }
        

        /// <summary> Writes a 16 bit signed integer, big endian form. </summary>
        public static void WriteI16(short value, byte[] array, int index) {
            array[index++] = (byte)(value >> 8);
            array[index++] = (byte)(value);
        }

        /// <summary> Writes a 16 bit unsigned integer, big endian form. </summary>
        public static void WriteU16(ushort value, byte[] array, int index) {
            array[index++] = (byte)(value >> 8);
            array[index++] = (byte)(value);
        }

        /// <summary> Writes a 32 bit signed integer, big endian form. </summary>
        public static void WriteI32(int value, byte[] array, int index) {
            array[index++] = (byte)(value >> 24);
            array[index++] = (byte)(value >> 16);
            array[index++] = (byte)(value >> 8);
            array[index++] = (byte)(value);
        }
        
        /// <summary> Writes three (X, Y, Z) either 16 or 32 bit signed integers, big endian form. </summary>
        /// <returns> Number of bytes written. </returns>
        internal static int WritePos(Position pos, byte[] arr, int offset, bool extPos) {
            if (!extPos) {
                WriteI16((short)pos.X, arr, offset + 0);
                WriteI16((short)pos.Y, arr, offset + 2);
                WriteI16((short)pos.Z, arr, offset + 4);
            } else {
                WriteI32((int)pos.X, arr, offset + 0);
                WriteI32((int)pos.Y, arr, offset + 4);
                WriteI32((int)pos.Z, arr, offset + 8);
            }
            return extPos ? 12 : 6;
        }


        /// <summary> Reads a string of unicode characters. (input is 64 bytes). </summary>
        /// <remarks> String length may be less than 64, as string is trimmed of trailing spaces and nuls. </remarks>
        public unsafe static string ReadString(byte[] data, int offset) {
            int length = 0;
            char* characters = stackalloc char[StringSize];
            for (int i = StringSize - 1; i >= 0; i--) {
                byte code = data[i + offset];
                if( length == 0 && !(code == 0x00 || code == 0x20))
                    length = i + 1;
                characters[i] = ((char)code).Cp437ToUnicode();
            }
            return new String(characters, 0, length);
        }
        
        /// <summary> Writes a string of unicode characters. (output is 64 bytes). </summary>
        /// <remarks> Unicode characters that are unable to be mapped into code page 437 are converted to '?'. </remarks>
        /// <remarks> If 'hasCP437' is false, characters that cannot be mapped to ASCII are converted to '?'. </remarks>
        public static void Write(string str, byte[] array, int offset, bool hasCP437) {
            if (hasCP437) WriteCP437(str, array, offset);
            else WriteAscii(str, array, offset);
        }
        
        static void WriteAscii(string str, byte[] array, int offset) {
            int count = Math.Min(str.Length, StringSize);
            for (int i = 0; i < count; i++) {
                char raw = str[i].UnicodeToCp437();
                array[offset + i] = raw >= '\u0080' ? (byte)'?' : (byte)raw;
            }
            for (int i = count; i < StringSize; i++)
                array[offset + i] = (byte)' ';
        }

        static void WriteCP437(string str, byte[] array, int offset) {
            int count = Math.Min(str.Length, StringSize);
            for (int i = 0; i < count; i++)
                array[offset + i] = (byte)str[i].UnicodeToCp437();
            for (int i = count; i < StringSize; i++)
                array[offset + i] = (byte)' ';
        }
    }
}
