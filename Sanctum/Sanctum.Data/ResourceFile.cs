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
