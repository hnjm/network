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

using System;
using System.Net;
using System.Net.Sockets;
using Exomia.Network.Buffers;
using Exomia.Network.Serialization;

namespace Exomia.Network.TCP
{
    /// <inheritdoc />
    public abstract class TcpClientBase : ClientBase
    {
        #region Variables

        private readonly int _max_data_size = Constants.PACKET_SIZE_MAX;
        private ClientStateObject _state;

        #endregion

        #region Constructors

        /// <inheritdoc />
        protected TcpClientBase()
        {
            _state = new ClientStateObject
            {
                Header = new byte[Constants.HEADER_SIZE],
                Data = new byte[_max_data_size]
            };
        }

        /// <inheritdoc />
        protected TcpClientBase(int maxDataSize)
            : this()
        {
            if (maxDataSize <= 0)
            {
                maxDataSize = Constants.PACKET_SIZE_MAX;
            }
            _max_data_size = maxDataSize;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnConnect(string serverAddress, int port, int timeout, out Socket socket)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                Blocking = false
            };
            try
            {
                IAsyncResult iar = socket.BeginConnect(Dns.GetHostAddresses(serverAddress), port, null, null);
                bool result = iar.AsyncWaitHandle.WaitOne(timeout * 1000, true);
                socket.EndConnect(iar);
                if (result)
                {
                    ReceiveHeaderAsync();
                    return true;
                }
            }
            catch
            {
                /* IGNORE */
            }
            socket = null;
            return false;
        }

        private void ReceiveHeaderAsync()
        {
            try
            {
                _clientSocket.BeginReceive(
                    _state.Header, 0, Constants.HEADER_SIZE, SocketFlags.None, ReceiveHeaderCallback, null);
            }
            catch { OnDisconnected(); }
        }

        private void ReceiveHeaderCallback(IAsyncResult iar)
        {
            try
            {
                if (_clientSocket.EndReceive(iar) <= 0)
                {
                    OnDisconnected();
                    return;
                }
            }
            catch
            {
                OnDisconnected();
                return;
            }

            _state.Header.GetHeader(
                out _state.CommandID, out _state.DataLength, out _state.Response, out _state.Compressed);
            if (_state.DataLength > 0)
            {
                _clientSocket.BeginReceive(
                    _state.Data, 0, _state.DataLength, SocketFlags.None, ClientReceiveDataCallback, null);
                return;
            }

            ReceiveHeaderAsync();
        }

        private void ClientReceiveDataCallback(IAsyncResult iar)
        {
            int length;
            try
            {
                if ((length = _clientSocket.EndReceive(iar)) <= 0)
                {
                    OnDisconnected();
                    return;
                }
            }
            catch
            {
                OnDisconnected();
                return;
            }

            uint commandID = _state.CommandID;
            int dataLength = _state.DataLength;
            uint response = _state.Response;
            uint compressed = _state.Compressed;

            if (length == dataLength)
            {
                uint responseID = 0;
                byte[] data;
                if (compressed != 0)
                {
                    int l;
                    if (response != 0)
                    {
                        responseID = BitConverter.ToUInt32(_state.Data, 0);
                        l = BitConverter.ToInt32(_state.Data, 4);
                        data = ByteArrayPool.Rent(l);

                        int s = LZ4.LZ4Codec.Decode(_state.Data, 8, dataLength - 8, data, 0, l, true);
                        if (s != l) { throw new Exception("LZ4.Decode FAILED!"); }

                    }
                    else
                    {
                        l = BitConverter.ToInt32(_state.Data, 0);
                        data = ByteArrayPool.Rent(l);

                        int s = LZ4.LZ4Codec.Decode(_state.Data, 4, dataLength - 4, data, 0, l, true);
                        if (s != l) { throw new Exception("LZ4.Decode FAILED!"); }
                    }

                    ReceiveHeaderAsync();
                    DeserializeDataAsync(commandID, data, 0, l, responseID);
                    ByteArrayPool.Return(data);
                    return;
                }

                if (response != 0)
                {
                    responseID = BitConverter.ToUInt32(_state.Data, 0);
                    dataLength -= 4;
                    data = ByteArrayPool.Rent(dataLength);
                    Buffer.BlockCopy(_state.Data, 4, data, 0, dataLength);
                }
                else
                {
                    data = ByteArrayPool.Rent(dataLength);
                    Buffer.BlockCopy(_state.Data, 0, data, 0, dataLength);
                }

                ReceiveHeaderAsync();
                Console.WriteLine(dataLength);
                DeserializeDataAsync(commandID, data, 0, dataLength, responseID);
                ByteArrayPool.Return(data);
                return;

            }

            ReceiveHeaderAsync();
        }

        #endregion

        #region Nested

        private struct ClientStateObject
        {
            public byte[] Header;
            public byte[] Data;
            public uint CommandID;
            public int DataLength;
            public uint Response;
            public uint Compressed;
        }

        #endregion
    }
}