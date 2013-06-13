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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Resources;
using System.ComponentModel;

namespace Sanctum.Data
{
    [Serializable]
    public class Resource
    {
        public object Value;

        public Dictionary<string, string> Metadata = new Dictionary<string, string>();
    }

    public class ResourceFile
    {
        public Dictionary<string, Resource> Resources;

        public ResourceFile()
        {
            this.Resources = new Dictionary<string, Resource>();
        }

        public void Load(string file)
        {
            Resources.Clear();

            ResourceSet Set = new ResourceSet(file);
            IDictionaryEnumerator Enumerator = Set.GetEnumerator();

            while (Enumerator.MoveNext())
            {
                string Key = (string)Enumerator.Key;
                Resource Value = (Resource)Enumerator.Value;

                Resources.Add(Key, Value);
            }

            Set.Close();
        }

        public void Save(string file)
        {
            ResourceWriter Writer = new ResourceWriter(file);

            foreach(KeyValuePair<string, Resource> Pair in Resources)
            {
                Writer.AddResource(Pair.Key, Pair.Value);
            }

            Writer.Generate();
            Writer.Close();
        }
    }
}
