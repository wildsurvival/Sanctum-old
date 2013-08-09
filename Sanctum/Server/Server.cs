/*
Sanctum is a free open-source 2D isometric game engine
Copyright (C) 2013  Andrew Choate

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

You can contact the author at a_choate@live.com or at the project website
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanctum;
using Sanctum.Communication;

namespace Server
{
    public class Server
    {
        #region "Delegates"

        public delegate void PacketRecievedEvent(Client Client, Packet Packet);
        public event PacketRecievedEvent PacketRecieved;

        public delegate void ClientConnectedEvent(Client Client);
        public event ClientConnectedEvent ClientConnected;

        #endregion

        private Socket listener;

        private static Random random;

        public static Dictionary<int, Client> Clients;

        public Server()
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            random = new Random();

            Clients = new Dictionary<int, Client>();
        }

        public void Start(int Port)
        {
            listener.Bind(new IPEndPoint(IPAddress.Any, Port));
            listener.Listen(100);

            listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
        }

        public void Stop()
        {
            lock (Clients)
            {
                foreach (KeyValuePair<int, Client> ClientPair in Clients)
                {
                    Client Client = ClientPair.Value;

                    Client.SocketState.WorkSocket.BeginDisconnect(false, new AsyncCallback(DisconnectCallback), Client);
                    Clients.Remove(ClientPair.Key);
                }
            }
        }

        public int GenerateID()
        {
            int Index = random.Next(1, 10000);
            int Start = Index;

            while (Clients.ContainsKey(Index))
            {
                ++Index;

                if (Index > 10000)
                    Index = 1;

                if (Index == Start)
                    throw new Exception("No more client IDs are available");
            }

            return Index;
        }

        #region "Callbacks"

        public void AcceptCallback(IAsyncResult ar)
        {
            Client Client = new Client();
            Client.SocketState.WorkSocket = listener.EndAccept(ar);

            int ID = GenerateID();
            Client.ID = ID;

            Clients.Add(ID, Client);
            ClientConnected(Client);

            Client.SocketState.WorkSocket.BeginReceive(Client.SocketState.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(RecieveCallback), Client);
            listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
        }

        private void DisconnectCallback(IAsyncResult ar)
        {
            Client Client = (Client)ar.AsyncState;
            Socket WorkSocket = Client.SocketState.WorkSocket;

            WorkSocket.EndDisconnect(ar);
        }

        public void RecieveCallback(IAsyncResult ar)
        {
            Client Client = (Client)ar.AsyncState;
            Socket WorkSocket = Client.SocketState.WorkSocket;

            SocketError ErrorCode;
            int BytesRead = WorkSocket.EndReceive(ar, out ErrorCode);

            if (BytesRead == 0) // No data recieved
            {
                    WorkSocket.BeginReceive(Client.SocketState.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(RecieveCallback), Client);
                    return;
            }

            if (Client.SocketState.ResponseSize == 0) // Starting a new packet because there was previous data was sent
            {
                int Length = Client.SocketState.Buffer[0] << Client.SocketState.Buffer[1];

                Client.SocketState.ResponseSize = Length;
                Client.SocketState.Response.AddRange(Client.SocketState.Buffer.ToList<byte>().GetRange(2, Client.SocketState.Buffer.Length - 2));
            }
            else // Theres still some data and we are adding onto the packet
            {
                Client.SocketState.Response.AddRange(Client.SocketState.Buffer);
            }

            if (Client.SocketState.ResponseSize == Client.SocketState.Response.Count) // Packet was finished
            {
                Packet Response = new Packet(Client.SocketState.Response.ToArray<byte>());
                PacketRecieved(Client, Response);

                Client.SocketState.ResponseSize = 0;
                Client.SocketState.Response.Clear();
            }

            WorkSocket.BeginReceive(Client.SocketState.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(RecieveCallback), Client);
        }

        #endregion
    }
}
