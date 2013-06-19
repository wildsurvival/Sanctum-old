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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Sanctum;
using Sanctum.Communication;

namespace Server
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class StateAttribute : Attribute
    {
        public ClientState State
        {
            get { return clientState; }
            set { clientState = value; }
        }


        private ClientState clientState;
        public StateAttribute(ClientState State)
        {
            this.State = State;
        }
    }

    public static class PacketHandler
    {
        private static Handler[,] Handlers = InitializeHandlers();

        private static Handler[,] InitializeHandlers()
        {
            Handlers = new Handler[256, 256];

            foreach (Type Type in Assembly.GetCallingAssembly().GetTypes())
            {
                if (Type.Namespace == typeof(PacketHandler).Namespace + ".Packets")
                {
                    PacketFamily Family = (PacketFamily)Enum.Parse(typeof(PacketFamily), Type.Name);

                    foreach (MethodInfo Method in Type.GetMethods())
                    {
                        if (Method.IsStatic && Method.IsPublic)
                        {
                            PacketAction Action = (PacketAction)Enum.Parse(typeof(PacketAction), Method.Name);

                            Register(Family, Action, (Action<Client, Packet>)Delegate.CreateDelegate(typeof(Action<Client, Packet>), Method));
                        }
                    }
                }
            }

            return Handlers;
        }

        public static void Register(PacketFamily Family, PacketAction Action, Action<Client, Packet> Delegate)
        {
            if (Exists(Family, Action))
            {
                Output.Warning("Overriding previously registered packet: {0}_{1}", Family.ToString(), Action.ToString());
            }

            Handlers[(int)Family, (int)Action] = new Handler(Family, Action, Delegate);
        }

        public static void Handle(Client Client, Packet Packet)
        {
            //if (!Packet.IsValid)
            //{
            //    Output.Warn(string.Format("Packet checksum [{0}] not valid : should be {1}", ASCIIEncoding.ASCII.GetString(Packet.WrittenChecksum), ASCIIEncoding.ASCII.GetString(Packet.ValidChecksum)));
            //    return;
            //}

            PacketFamily Family = Packet.Family;
            PacketAction Action = Packet.Action;

            if (Exists(Family, Action))
            {
                Handler Handler = Handlers[(int)Family, (int)Action];
                StateAttribute[] StateAttributes = (StateAttribute[])Handler.Delegate.GetInvocationList()[0].Method.GetCustomAttributes(typeof(StateAttribute), false);

                bool StateMet = false;
                foreach (StateAttribute State in StateAttributes)
                {
                    if (Client.State == State.State)
                    {
                        StateMet = true;
                    }
                }

                if (StateMet)
                {
                    lock (Client)
                    {
                        Handler.Delegate.Invoke(Client, Packet);
                    }
                }
            }
            else
            {
                Output.Warning("Unhandled packet: {0}_{1} not registered", Family.ToString(), Action.ToString());
            }
        }

        public static bool Exists(PacketFamily Family, PacketAction Action)
        {
            if ((Handlers[(int)Family, (int)Action] != null))
            {
                return true;
            }

            return false;
        }

        public static int Count()
        {
            int Amount = 0;

            for (int Family = 0; Family <= 255; Family++)
            {
                for (int Action = 0; Action <= 255; Action++)
                {
                    if (Exists((PacketFamily)Family, (PacketAction)Action))
                    {
                        Amount += 1;
                    }
                }
            }

            return Amount;
        }

        private class Handler
        {
            public PacketFamily Family;
            public PacketAction Action;
            public ClientState State;
            public Action<Client, Packet> Delegate;

            public Handler(PacketFamily Family, PacketAction Action, Action<Client, Packet> Delegate)
            {
                this.Action = Action;
                this.Family = Family;
                this.Delegate = Delegate;
            }
        }

    }
}
