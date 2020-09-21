﻿#region License

// Copyright (c) 2018-2020, exomia
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

#endregion

namespace Exomia.Network.Lib
{
    sealed class ClientEventEntry
    {
        internal readonly DeserializePacketHandler<object?> _deserialize;
        private readonly  Event<DataReceivedHandler>        _dataReceived;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ClientEventEntry" /> class.
        /// </summary>
        /// <param name="deserialize"> The deserialize. </param>
        public ClientEventEntry(DeserializePacketHandler<object?> deserialize)
        {
            _dataReceived = new Event<DataReceivedHandler>();
            _deserialize  = deserialize;
        }

        /// <summary>
        ///     Adds callback.
        /// </summary>
        /// <param name="callback"> The callback to remove. </param>
        public void Add(DataReceivedHandler callback)
        {
            _dataReceived.Add(callback);
        }

        /// <summary>
        ///     Removes the given callback.
        /// </summary>
        /// <param name="callback"> The callback to remove. </param>
        public void Remove(DataReceivedHandler callback)
        {
            _dataReceived.Remove(callback);
        }

        /// <summary>
        ///     Raises.
        /// </summary>
        /// <param name="client">     The client. </param>
        /// <param name="data">       The data. </param>
        /// <param name="responseID"> Identifier for the response. </param>
        public void Raise(IClient client, object data, uint responseID)
        {
            for (int i = _dataReceived.Count - 1; i >= 0; --i)
            {
                if (!_dataReceived[i].Invoke(client, data, responseID))
                {
                    _dataReceived.Remove(i);
                }
            }
        }
    }
}