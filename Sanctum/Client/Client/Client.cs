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
using System.Threading;
using System.Threading.Tasks;
using Sanctum;
using Sanctum.Communication;

namespace Client
{
    class Client
    {
        #region "Delegates"

        public delegate void PacketRecievedEvent(Packet Packet);
        public event PacketRecievedEvent PacketRecieved;

        public delegate void ConnectedEvent(IPAddress Host, int Port);
        public event ConnectedEvent Connected;

        public delegate void DisconnectedEvent(IPAddress Host, int Port);
        public event DisconnectedEvent Disconnected;

        #endregion

        public ClientState State = ClientState.Uninitialized;

        private Socket listener;

        public Client()
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start(IPAddress Host, int Port)
        {
            listener.BeginConnect(new IPEndPoint(Host, Port), new AsyncCallback(ConnectCallback), null);
        }

        public void Stop()
        {
            listener.BeginDisconnect(true, new AsyncCallback(DisconnectCallback), null);
        }

        #region "Callbacks"

        private void ConnectCallback(IAsyncResult ar)
        {
            listener.EndConnect(ar);

            StateObject State = new StateObject();
            State.WorkSocket = listener;

            IPEndPoint RemoteEndpoint = listener.RemoteEndPoint as IPEndPoint;
            Connected(RemoteEndpoint.Address, RemoteEndpoint.Port);

            listener.BeginReceive(State.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(RecieveCallback), State);
        }

        private void DisconnectCallback(IAsyncResult ar)
        {
            IPEndPoint RemoteEndpoint = listener.RemoteEndPoint as IPEndPoint;
            Disconnected(RemoteEndpoint.Address, RemoteEndpoint.Port);

            listener.EndDisconnect(ar);
        }

        private void RecieveCallback(IAsyncResult ar)
        {
            StateObject State = (StateObject)ar.AsyncState;

            SocketError ErrorCode;
            int BytesRead = listener.EndReceive(ar, out ErrorCode);

            if (BytesRead == 0)
            {
                listener.BeginReceive(State.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(RecieveCallback), State);
                return;
            }

            if (State.ResponseSize == 0) // Starting a new packet because there was previous data was sent
            {
                int Length = State.Buffer[0] << State.Buffer[1];

                State.ResponseSize = Length;
                State.Response.AddRange(State.Buffer.ToList<byte>().GetRange(2, State.Buffer.Length - 2));
            }
            else // Theres still some data and we are adding onto the packet
            {
                State.Response.AddRange(State.Buffer);
            }

            if (State.ResponseSize == State.Response.Count) // Packet was finished
            {
                Packet Response = new Packet(State.Response.ToArray<byte>());
                PacketRecieved(Response);

                State.ResponseSize = 0;
                State.Response.Clear();
            }

            listener.BeginReceive(State.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(RecieveCallback), State);
        }

        private void SendCallback(IAsyncResult ar)
        {
            SocketError ErrorCode;
            int BytesSent = listener.EndSend(ar, out ErrorCode);
        }

        #endregion

    }
}
