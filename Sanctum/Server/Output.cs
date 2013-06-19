﻿/*
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

namespace Server
{
    public static class Output
    {
        public static ConsoleColor MessageColor = ConsoleColor.White;
        public static ConsoleColor WarningColor = ConsoleColor.Yellow;
        public static ConsoleColor ErrorColor = ConsoleColor.Red;

        public static void Message(string message, params object[] args)
        {
            Console.ForegroundColor = MessageColor;
            Console.WriteLine(message, args);
            Console.ForegroundColor = MessageColor;
        }

        public static void Warning(string message, params object[] args)
        {
            Console.ForegroundColor = WarningColor;
            Console.WriteLine(message, args);
            Console.ForegroundColor = MessageColor;
        }

        public static void Error(string message, params object[] args)
        {
            Console.ForegroundColor = ErrorColor;
            Console.WriteLine(message, args);
            Console.ForegroundColor = MessageColor;
        }
    }
}
