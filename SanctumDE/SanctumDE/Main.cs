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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sanctum;
using Sanctum.Data;
using Microsoft.VisualBasic;
using PropertyGridEx;

namespace SanctumDE
{
    public partial class Main : Form
    {
        public ResourceFile file = new ResourceFile();

        public Main()
        {
            InitializeComponent();
        }

        private void Open(object sender, EventArgs e)
        {
            OpenFileDialog Dialog = new OpenFileDialog();
            Dialog.Multiselect = false;
            Dialog.Filter = "Sanctum Data File (*.sd)|*.sd";

            if (Dialog.ShowDialog() == DialogResult.OK)
            {
                LoadFile(Dialog.FileName);
            }
        }

        private void Save(object sender, EventArgs e)
        {
            SaveFileDialog Dialog = new SaveFileDialog();
            Dialog.Filter = "Sanctum Data File (*.sd)|*.sd";

            if (Dialog.ShowDialog() == DialogResult.OK)
            {
                this.file.Save(Dialog.FileName);
            }
        }

        public void LoadFile(string file)
        {
            try
            {
                this.file.Load(file);
            }
            catch (Exception)
            {
                
                MessageBox.Show("This file is either corrupted or is not a valid resource file");
                return;
            }

            foreach (KeyValuePair<string, Resource> Pair in this.file.Resources)
            {
                TreeNode node = new TreeNode() { Name = Pair.Key, Text = Pair.Key, Tag = Pair.Value };
                Resource resource= Pair.Value;

                if (resource.Value != null)
                {
                    if (resource.Value.GetType() == typeof(Bitmap))
                    {
                        node.ImageIndex = 2;
                        node.SelectedImageIndex = 2;
                    }
                    else if (resource.Value.GetType() == typeof(string))
                    {
                        node.ImageIndex = 0;
                        node.SelectedImageIndex = 0;
                    }
                }

                DataList.Nodes[0].Nodes.Add(node);
                DataList.Nodes[0].Expand();
            }
        }

        private void AddImage(object sender, EventArgs e)
        {
            OpenFileDialog Dialog = new OpenFileDialog();
            Dialog.Multiselect = false;

            if (Dialog.ShowDialog() == DialogResult.OK)
            {
                AddImageData(Dialog.FileName);
            }
        }

        private void AddImageData(string file)
        {
            string input = Interaction.InputBox("What would you like to call this asset?", "Asset title").ToLower();

            if (input != "")
            {
                if (DataList.Nodes[0].Nodes.ContainsKey(input))
                {
                    AddImageData(file);
                }
                else
                {
                    Image image = Image.FromFile(file);

                    Resource resource = new Resource() { Value = image };

                    this.file.Resources.Add(input, resource);
                    DataList.Nodes[0].Nodes.Add(new TreeNode() { Name = input, Text = input, Tag = resource, ImageIndex = 2 });
                    DataList.Nodes[0].Expand();
                }
            }
        }

        private void Clear(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            textBox1.Text = "";
            textBox1.Visible = false;
            this.file.Resources.Clear();
            DataList.Nodes[0].Nodes.Clear();
        }

        private void SelectChanged(object sender, TreeViewEventArgs e)
        {
            pictureBox1.Image = null;
            textBox1.Text = "";
            textBox1.Visible = false;
            saveChangeToolStripMenuItem.Enabled = false;

            if (DataList.SelectedNode != null)
            {
                TreeNode Node = DataList.SelectedNode;
                if (Node.Tag != null)
                {
                    Resource Resource = (Resource)Node.Tag;

                    if (Resource.Value != null)
                    {
                        if (Resource.Value.GetType() == typeof(Bitmap))
                        {
                            pictureBox1.Image = (Image)Resource.Value;
                        }
                        else if (Resource.Value.GetType() == typeof(string))
                        {
                            textBox1.Text = (string)Resource.Value;
                            textBox1.Visible = true;
                            saveChangeToolStripMenuItem.Enabled = true;
                        }
                    }

                    dataGridView1.Rows.Clear();

                    foreach(KeyValuePair<string, string> Pair in Resource.Metadata)
                    {
                        dataGridView1.Rows.Add(Pair.Key, Pair.Value);
                    }
                }
            }
        }

        private void AddString(object sender, EventArgs e)
        {
            AddStringData();
        }

        private void AddStringData()
        {
            string input = Interaction.InputBox("What would you like to call this asset?", "Asset title").ToLower();

            if (input != "")
            {
                if (DataList.Nodes[0].Nodes.ContainsKey(input))
                {
                    AddStringData();
                }
                else
                {
                    string value = Interaction.InputBox("What do you want this asset to contain?", "Asset value");

                    Resource resource = new Resource() { Value = value };

                    this.file.Resources.Add(input, resource);
                    DataList.Nodes[0].Nodes.Add(new TreeNode() { Name = input, Text = input, Tag = resource, ImageIndex = 0 });
                    DataList.Nodes[0].Expand();
                }
            }
        }

        private void SaveChanges(object sender, EventArgs e)
        {
            if (saveChangeToolStripMenuItem.Enabled == true)
            {
                if (DataList.SelectedNode != null)
                {
                    TreeNode Node = DataList.SelectedNode;
                    if (Node.Tag != null)
                    {
                        if (Node.Tag.GetType() == typeof(Bitmap))
                        {
                            
                        }
                        else if (Node.Tag.GetType() == typeof(string))
                        {
                            Node.Tag = textBox1.Text;

                            this.file.Resources[Node.Name].Value = textBox1.Text;
                        }
                    }
                }
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void SaveMetadata(object sender, EventArgs e)
        {
            if (DataList.SelectedNode != null)
            {
                TreeNode Node = DataList.SelectedNode;
                if (Node.Tag != null)
                {
                    Resource resource = (Resource)Node.Tag;
                    Dictionary<string, string> clone = new Dictionary<string, string>(resource.Metadata);
                    resource.Metadata.Clear();

                    foreach(DataGridViewRow Row in dataGridView1.Rows)
                    {
                        string Key = (string)Row.Cells[0].Value;
                        string Value = (string)Row.Cells[1].Value;

                        if (Key == null) continue;
                        if (resource.Metadata.Keys.Contains<string>(Key))
                        {
                            resource.Metadata = clone;
                            MessageBox.Show(string.Format("The key, {0}, has more than one record!", Key));
                            return;
                        }

                        resource.Metadata.Add(Key, Value);
                    }
                }
                else
                {
                    MessageBox.Show("You havn't selected a resource!");
                }
            }
        }

        private void DeleteSelected(object sender, EventArgs e)
        {
            if (DataList.SelectedNode != null)
            {
                TreeNode Node = DataList.SelectedNode;
                Node.Remove();
                this.file.Resources.Remove(Node.Name);
            }
        }

        private void SaveMetadataEnter(object sender, EventArgs e) { pictureBox2.BackColor = Color.Silver; }
        private void SaveMetadataLeave(object sender, EventArgs e) { pictureBox2.BackColor = Color.Transparent; }
    }
}
