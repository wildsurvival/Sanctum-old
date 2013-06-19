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
    public class StateObject
    {
        public Socket WorkSocket = null;
        public const int BufferSize = 2048;
        public byte[] Buffer = new byte[BufferSize];
        public List<byte> Response = new List<byte>();
        public int ResponseSize = 0;
    }

    public class Client
    {
        public StateObject SocketState;

        public int ID;
        public ClientState State = ClientState.Uninitialized;

        public Client()
        {
            SocketState = new StateObject();
        }

        #region "Callbacks"

        private void SendCallback(IAsyncResult ar)
        {
            Client Client = (Client)ar.AsyncState;

            SocketError ErrorCode;
            int BytesSent = Client.SocketState.WorkSocket.EndSend(ar, out ErrorCode);
        }

        #endregion

        public void Send(Packet Packet)
        {
            Socket ClientSocket = SocketState.WorkSocket;

            if (ClientSocket == null)
                return;

            List<byte> Data = Packet.Data;
            byte[] Length = new byte[2] { (byte)(Data.Count >> 8), (byte)Data.Count }; //Short length (so we can have more than just a 255 sized packet)
            Data.InsertRange(0, Length);

            ClientSocket.BeginSend(Data.ToArray<byte>(), 0, Data.Count, 0, new AsyncCallback(SendCallback), this);
        }
    }
}
