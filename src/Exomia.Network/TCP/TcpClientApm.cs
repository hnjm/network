﻿#region License

// Copyright (c) 2018-2020, exomia
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

#endregion

using System;
using System.Net.Sockets;
using Exomia.Network.Buffers;

namespace Exomia.Network.TCP
{
    /// <summary>
    ///     A TCP/UDP-Client build with the "Asynchronous Programming Model" (APM)
    /// </summary>
    public sealed class TcpClientApm : TcpClientBase
    {
        private readonly byte[] _bufferWrite;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TcpClientApm" /> class.
        /// </summary>
        /// <param name="expectedMaxPayloadSize"> (Optional) Size of the expected maximum payload. </param>
        public TcpClientApm(ushort expectedMaxPayloadSize = Constants.TCP_PAYLOAD_SIZE_MAX)
            : base(expectedMaxPayloadSize)
        {
            _bufferWrite =
                new byte[_bufferRead.Length];
        }

        /// <inheritdoc />
        protected override void OnDispose(bool disposing)
        {
            _circularBuffer.Dispose();
        }

        private void ReceiveAsyncCallback(IAsyncResult iar)
        {
            int bytesTransferred;
            try
            {
                if ((bytesTransferred = _clientSocket!.EndReceive(iar)) <= 0)
                {
                    Disconnect(DisconnectReason.Graceful);
                    return;
                }
            }
            catch (ObjectDisposedException)
            {
                Disconnect(DisconnectReason.Aborted);
                return;
            }
            catch (SocketException)
            {
                Disconnect(DisconnectReason.Error);
                return;
            }
            catch
            {
                Disconnect(DisconnectReason.Unspecified);
                return;
            }

            Receive(_bufferWrite, bytesTransferred);
            ReceiveAsync();
        }

        private void SendDataCallback(IAsyncResult iar)
        {
            try
            {
                if (_clientSocket!.EndSend(iar) <= 0)
                {
                    Disconnect(DisconnectReason.Error);
                }
            }
            catch (ObjectDisposedException) { Disconnect(DisconnectReason.Aborted); }
            catch (SocketException) { Disconnect(DisconnectReason.Error); }
            catch { Disconnect(DisconnectReason.Unspecified); }

            byte[] send = (byte[])iar.AsyncState!;
            ByteArrayPool.Return(send);
        }

        /// <inheritdoc />
        private protected override void ReceiveAsync()
        {
            if ((_state & RECEIVE_FLAG) == RECEIVE_FLAG)
            {
                try
                {
                    _clientSocket!.BeginReceive(
                        _bufferWrite, 0, _bufferWrite.Length, SocketFlags.None, ReceiveAsyncCallback, null);
                }
                catch (ObjectDisposedException) { Disconnect(DisconnectReason.Aborted); }
                catch (SocketException) { Disconnect(DisconnectReason.Error); }
                catch { Disconnect(DisconnectReason.Unspecified); }
            }
        }

        /// <inheritdoc />
        private protected override unsafe SendError BeginSend(in PacketInfo packetInfo)
        {
            int    size;
            byte[] buffer = ByteArrayPool.Rent(_payloadSize + Constants.TCP_HEADER_OFFSET);
            fixed (byte* dst = buffer)
            {
                size = Serialization.Serialization.SerializeTcp(in packetInfo, dst, _encryptionMode);
            }

            try
            {
                _clientSocket!.BeginSend(
                    buffer, 0, size, SocketFlags.None, SendDataCallback, buffer);
                return SendError.None;
            }
            catch (ObjectDisposedException)
            {
                ByteArrayPool.Return(buffer);
                Disconnect(DisconnectReason.Aborted);
                return SendError.Disposed;
            }
            catch (SocketException)
            {
                ByteArrayPool.Return(buffer);
                Disconnect(DisconnectReason.Error);
                return SendError.Socket;
            }
            catch
            {
                ByteArrayPool.Return(buffer);
                Disconnect(DisconnectReason.Unspecified);
                return SendError.Unknown;
            }
        }
    }
}