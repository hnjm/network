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
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Exomia.Network.Buffers;
using Exomia.Network.Serialization;

namespace Exomia.Network.UDP
{
    /// <inheritdoc />
    public abstract class UdpServerBase<TServerClient> : ServerBase<EndPoint, TServerClient>
        where TServerClient : ServerClientBase<EndPoint>
    {
        #region Variables

        private readonly ServerClientStateObjectPool _pool;

        /// <summary>
        ///     _max_idle_time
        /// </summary>
        protected double _max_Idle_Time;

        #endregion

        #region Constructors

        /// <inheritdoc />
        protected UdpServerBase(int maxClients)
            : this(maxClients, Constants.PACKET_SIZE_MAX, Constants.UDP_IDLE_TIME) { }

        /// <inheritdoc />
        protected UdpServerBase(int maxClients, int maxPacketSize)
            : this(maxClients, maxPacketSize, Constants.UDP_IDLE_TIME) { }

        /// <inheritdoc />
        protected UdpServerBase(int maxClients, double maxIdleTime)
            : this(maxClients, Constants.PACKET_SIZE_MAX, maxIdleTime) { }

        /// <inheritdoc />
        protected UdpServerBase(int maxClients, int maxPacketSize, double maxIdleTime)
            : base(maxPacketSize)
        {
            if (maxIdleTime <= 0) { maxIdleTime = Constants.UDP_IDLE_TIME; }
            _max_Idle_Time = maxIdleTime;

            _pool = new ServerClientStateObjectPool(maxClients, maxPacketSize);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnRun(int port, out Socket listener)
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                listener.Bind(new IPEndPoint(IPAddress.Any, port));

                Listen();
                return true;
            }
            catch
            {
                listener = null;
                return false;
            }
        }

        /// <inheritdoc />
        protected override void BeginSendDataTo(EndPoint arg0, byte[] send, int lenght)
        {
            try
            {
                _listener.BeginSendTo(send, 0, lenght, SocketFlags.None, arg0, SendDataToCallback, send);
            }
            catch
            {
                /* IGNORE */
            }
        }

        private void Listen()
        {
            ServerClientStateObject state = _pool.Rent();
            try
            {
                _listener.BeginReceiveFrom(
                    state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ref state.EndPoint,
                    ClientReceiveDataCallback, state);
            }
            catch
            {
                /* IGNORE */
            }
        }

        private void ClientReceiveDataCallback(IAsyncResult iar)
        {
            ServerClientStateObject state = (ServerClientStateObject)iar.AsyncState;

            int length;
            try
            {
                if ((length = _listener.EndReceiveFrom(iar, ref state.EndPoint)) <= 0)
                {
                    InvokeClientDisconnected(state.EndPoint);
                    return;
                }
            }
            catch
            {
                InvokeClientDisconnected(state.EndPoint);
                return;
            }

            Listen();

            state.Buffer.GetHeader(out uint commandID, out uint type, out int dataLength);

            if (dataLength == length - Constants.HEADER_SIZE)
            {
                byte[] data = ByteArrayPool.Rent(dataLength);
                Buffer.BlockCopy(state.Buffer, Constants.HEADER_SIZE, data, 0, dataLength);
                DeserializeDataAsync(state.EndPoint, commandID, type, data, dataLength);
                ByteArrayPool.Return(data);
            }

            _pool.Return(state);
        }

        private void SendDataToCallback(IAsyncResult iar)
        {
            try
            {
                _listener.EndSendTo(iar);
                byte[] send = (byte[])iar.AsyncState;
                ByteArrayPool.Return(send);
            }
            catch
            {
                /* IGNORE */
            }
        }

        #endregion

        #region Nested

        private sealed class ServerClientStateObject
        {
            #region Variables

            public byte[] Buffer;
            public EndPoint EndPoint;

            #endregion
        }

        private class ServerClientStateObjectPool
        {
            #region Variables

            private readonly ServerClientStateObject[] _buffers;
            private readonly int _max_packetSize;
            private int _index;
            private SpinLock _lock;

            #endregion

            #region Constructors

            public ServerClientStateObjectPool(int maxClients, int maxPacketSize)
            {
                _max_packetSize = maxPacketSize;
                _lock = new SpinLock(Debugger.IsAttached);
                _buffers = new ServerClientStateObject[maxClients + 1];
            }

            #endregion

            #region Methods

            internal ServerClientStateObject Rent()
            {
                ServerClientStateObject buffer = null;
                bool lockTaken = false, allocateBuffer = false;
                try
                {
                    _lock.Enter(ref lockTaken);

                    if (_index < _buffers.Length)
                    {
                        buffer = _buffers[_index];
                        _buffers[_index++] = null;
                        allocateBuffer = buffer == null;
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        _lock.Exit(false);
                    }
                }

                return !allocateBuffer
                    ? buffer
                    : new ServerClientStateObject
                    {
                        Buffer = new byte[_max_packetSize],
                        EndPoint = new IPEndPoint(IPAddress.Any, 0)
                    };
            }

            internal void Return(ServerClientStateObject obj)
            {
                bool lockTaken = false;
                try
                {
                    _lock.Enter(ref lockTaken);

                    if (_index != 0)
                    {
                        obj.EndPoint = new IPEndPoint(IPAddress.Any, 0);
                        _buffers[--_index] = obj;
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        _lock.Exit(false);
                    }
                }
            }

            #endregion
        }

        #endregion
    }
}