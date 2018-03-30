﻿#region MIT License

// Copyright (c) 2018 exomia - Daniel Bätz
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System.Runtime.InteropServices;

namespace Exomia.Network
{
    /// <summary>
    ///     PING_STRUCT
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 8)]
    public struct PING_STRUCT
    {
        /// <summary>
        ///     TimeStamp
        /// </summary>
        public long TimeStamp;
    }

    /// <summary>
    ///     CLIENTINFO_STRUCT
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 72)]
    public struct CLIENTINFO_STRUCT
    {
        /// <summary>
        ///     ClientID
        /// </summary>
        public long ClientID;

        /// <summary>
        ///     ClientName (64)
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string ClientName;
    }

    /// <summary>
    ///     UDP_CONNECT_STRUCT
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public struct UDP_CONNECT_STRUCT
    {
        /// <summary>
        ///     Checksum(16)
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Checksum;
    }
}