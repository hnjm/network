﻿#region License

// Copyright (c) 2018-2020, exomia
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

#endregion

#define TCP

using System;
using System.Text;
using Exomia.Network;
#if TCP
using Exomia.Network.TCP;

#else
using Exomia.Network.UDP;
#endif

namespace Example.Server
{
    class Program
    {
        private static void Main()
        {
            using Server server = new Server();

            server.ClientConnected += (server1, client) =>
            {
                Console.WriteLine("Client connected: " + client.IPAddress);
                SendRequestAndWaitForResponse(server1, client);
            };
            server.ClientDisconnected += (server1, client, reason) =>
            {
                Console.WriteLine(reason + " Client disconnected: " + client.IPAddress);
            };

            Console.WriteLine(
                server.Run(3000, b => b.ReceiveBufferSize = 64 * 1024)
                    ? "Server started..."
                    : "Server failed to start!");

            Console.WriteLine("press any key to exit...");
            Console.ReadKey();
        }

        private static async void SendRequestAndWaitForResponse(IServer<ServerClient> server, ServerClient client)
        {
            byte[] request = Encoding.UTF8.GetBytes("Hello ");
            Response<string> response = await server.SendToR(
                client, 1, request, 0, request.Length, DeserializePacketToString);
            Console.WriteLine("GOT({1}): {0}", response.Result, response.SendError);

            byte[] requestResponse = Encoding.UTF8.GetBytes($"Current server time is: {DateTime.UtcNow}");
            server.SendTo(client, response.ID, requestResponse, 0, requestResponse.Length, true);
        }

        private static string DeserializePacketToString(in Packet packet)
        {
            return Encoding.UTF8.GetString(packet.Buffer, packet.Offset, packet.Length);
        }
    }

#if TCP
    class Server : TcpServerEapBase<ServerClient>
#else
    class Server : UdpServerEapBase<ServerClient>
#endif
    {
        public Server(ushort expectedMaxPayloadSize = 512)
            : base(expectedMaxPayloadSize) { }

        protected override bool CreateServerClient(out ServerClient serverClient)
        {
            serverClient = new ServerClient();
            return true;
        }
    }

#if TCP
    class ServerClient : TcpServerClientBase
#else
    class ServerClient : UdpServerClientBase
#endif
    { }
}