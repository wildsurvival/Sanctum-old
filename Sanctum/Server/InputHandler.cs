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

namespace Server
{
    public static class InputHandler
    {
        private static Dictionary<string, Handler> Handlers = InitializeHandlers();

        private static Dictionary<string, Handler> InitializeHandlers()
        {
            Handlers = new Dictionary<string, Handler>();

            foreach (Type Type in Assembly.GetCallingAssembly().GetTypes())
            {
                if (Type.Namespace == typeof(InputHandler).Namespace + ".Inputs")
                {
                    foreach (MethodInfo Method in Type.GetMethods())
                    {
                        if (Method.IsStatic && Method.IsPublic)
                        {
                            string Keyword = Method.Name.ToLower();

                            Register(Keyword, (Action<string[]>)Delegate.CreateDelegate(typeof(Action<string[]>), Method));
                        }
                    }
                }
            }

            return Handlers;
        }

        public static void Register(string Keyword, Action<string[]> Delegate)
        {
            if (Exists(Keyword))
            {
                Output.Warning("Overriding previously registered input: {0}", Keyword);
            }

            Handlers[Keyword.ToLower()] = new Handler(Keyword.ToLower(), Delegate);
        }

        public static void Handle(string RawString)
        {
            string[] Arguments = RawString.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            if (Arguments[0] == null)
            {
                Output.Warning("Input did not contain any arguments!");
                return;
            }

            string Keyword = Arguments[0];
            Arguments = Arguments.ToList().GetRange(1, Arguments.Length - 1).ToArray();

            Handle(Keyword, Arguments);
        }

        public static void Handle(string Keyword, string[] Arguments)
        {
            if (Exists(Keyword))
            {
                Handler Handler = Handlers[Keyword.ToLower()];

                Handler.Delegate.Invoke(Arguments);
            }
            else
            {
                Output.Warning("Input '{0}' does not exist!", Keyword);
            }
        }

        public static bool Exists(string Keyword)
        {
            if (Handlers.ContainsKey(Keyword.ToLower()))
            {
                return true;
            }

            return false;
        }

        private class Handler
        {
            public string Keyword;
            public Action<string[]> Delegate;

            public Handler(string Keyword, Action<string[]> Delegate)
            {
                this.Keyword = Keyword;
                this.Delegate = Delegate;
            }
        }
    }
}
