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

namespace Exomia.Network.Lib
{
    sealed class ClientEventEntry
    {
        internal readonly DeserializePacket<object> _deserialize;
        private readonly Event<DataReceivedHandler> _dataReceived;

        public ClientEventEntry(DeserializePacket<object> deserialize)
        {
            _dataReceived = new Event<DataReceivedHandler>();
            _deserialize = deserialize;
        }

        public void Add(DataReceivedHandler callback)
        {
            _dataReceived.Add(callback);
        }

        public void Remove(DataReceivedHandler callback)
        {
            _dataReceived.Remove(callback);
        }

        public void Raise(IClient client, object result)
        {
            for (int i = _dataReceived.Count - 1; i >= 0; --i)
            {
                if (!_dataReceived[i].Invoke(client, result))
                {
                    _dataReceived.Remove(i);
                }
            }
        }
    }
}