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

namespace Exomia.Network
{
    /// <summary>
    ///     CommandID
    /// </summary>
    public static class CommandID
    {
        #region Variables

        /// <summary>
        ///     RESPONSE_ID
        /// </summary>
        public const uint RESPONSE = 16383;

        /// <summary>
        ///     CLIENTINFO_ID
        /// </summary>
        public const uint CLIENTINFO = 16382;

        /// <summary>
        ///     CONNECT_ID
        /// </summary>
        public const uint CONNECT = 16381;

        /// <summary>
        ///     DISCONNECT_ID
        /// </summary>
        public const uint DISCONNECT = 16380;

        /// <summary>
        ///     PING_ID
        /// </summary>
        public const uint PING = 16379;

        #endregion
    }
}