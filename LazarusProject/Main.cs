﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ME2Save = Gibbed.MassEffect2.FileFormats.SaveFile;
using ME3Save = Gibbed.MassEffect3.FileFormats.SFXSaveGameFile;
using System.IO;
using Gibbed.IO;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;
using System.Runtime.Serialization;
using ME2Vector = Gibbed.MassEffect2.FileFormats.Save.Vector;
using ME3Vector = Gibbed.MassEffect3.FileFormats.Save.Vector;

namespace Pixelmade.Lazarus
{

    public partial class Main : Form
    {
        ME2Save me2Save;
        ME3Save me3Save;
        VertexMapping mapping;
        string me2Path, me3Path;

        bool usingViewer;
        MouseEventArgs previousMouseState, downMouseState;

        void LoadME2Save(Stream file)
        {
            if (file != null) me2Save = ME2Save.Load(file);
        }
        void LoadME3save(Stream file)
        {
            if (file != null)
            {
                if (file.ReadValueU32(Endian.Big) == 0x434F4E20)
                {
                    return;
                }
                file.Seek(-4, SeekOrigin.Current);

                try
                {
                    me3Save = Gibbed.MassEffect3.FileFormats.SFXSaveGameFile.Read(file);
                }
                catch (Exception)
                {
                    return;
                }
            }
        }

        public Main()
        {
            InitializeComponent();

            toolStrip1.Renderer = new MySR();

            me2Path = "C:\\Users\\Pedro Madeira\\Documents\\BioWare\\Mass Effect 2\\Save\\Naomi_12_Sentinel_150212\\";
            me3Path = "C:\\Users\\Pedro Madeira\\Documents\\BioWare\\Mass Effect 3\\Save\\Naomi_12_Sentinel_210312_015c769\\";
        }

        private void lodViewer_MouseDown(object sender, MouseEventArgs e)
        {
            usingViewer = true;
            downMouseState = new MouseEventArgs(e.Button, e.Clicks, e.X, e.Y, e.Delta);
        }
        private void lodViewer_MouseUp(object sender, MouseEventArgs e)
        {
            usingViewer = false;
        }
        private void lodViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (previousMouseState != null && usingViewer)
            {
                if (e.Button == MouseButtons.Left)
                {
                    lodViewerME2.ViewAngle = lodViewerME3.ViewAngle -= (e.X - previousMouseState.X) / 100f;
                    lodViewerME2.ViewAngleVert = lodViewerME3.ViewAngleVert -= (e.Y - previousMouseState.Y) / 100f;
                }
                //else if (e.Button == MouseButtons.Right) lodViewerME2.ViewDistance = lodViewerME3.ViewDistance += (e.Y - previousMouseState.Y);
                else if (e.Button == MouseButtons.Right) lodViewerME2.ViewDistance = lodViewerME3.ViewDistance += (e.Y - previousMouseState.Y) * lodViewerME3.ViewDistance / 100f;
                else
                {
                    lodViewerME2.OffsetView((e.X - previousMouseState.X) / 20f, (e.Y - previousMouseState.Y) / 20f);
                    lodViewerME3.OffsetView((e.X - previousMouseState.X) / 20f, (e.Y - previousMouseState.Y) / 20f);
                }
            }

            previousMouseState = new MouseEventArgs(e.Button, e.Clicks, e.X, e.Y, e.Delta);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            /*
            using (var file = File.Open(me2Path + "AutoSave.pcsav", FileMode.Open))
            {
                LoadME2Save(file);
                file.Close();
            }
            using (var file = File.Open(me3Path + "Save_0001.pcsav", FileMode.Open))
            {
                LoadME3save(file);
                file.Close();
            }
            
            lodViewerME2.SetData(me2Save.PlayerRecord.Appearance.MorphHead.LOD0Vertices);
            lodViewerME3.SetData(me3Save.Player.Appearance.MorphHead.Lod0Vertices);
            */
            try
            {
                mapping = (VertexMapping)LoadObject("lod_mapping", typeof(VertexMapping));
                //lodViewerME2.SetMapData(mapping, false);
                //lodViewerME3.SetMapData(mapping, true);
            }
            catch (FileNotFoundException)
            {
                //CreateMapping();
            }
            comboBoxDisplay.SelectedIndex = 2;
        }

                private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(mapping != null) SaveObject("lod_mapping", mapping);
        }

        private void comboBoxDisplay_SelectedIndexChanged(object sender, EventArgs e)
        {
            string mode = comboBoxDisplay.SelectedItem.ToString();

            if (mode == "Thresholds" || mode == "Mapping")
            {
                if (mode == "Thresholds")
                {
                    labelIndex.Enabled = labelRange.Enabled = false;
                    trackBarIndex.Enabled = trackBarRange.Enabled = false;
                }
                else
                {
                    labelIndex.Enabled = labelRange.Enabled = true;
                    trackBarIndex.Enabled = trackBarRange.Enabled = true;
                    checkBoxIgnore.Enabled = true;
                    checkBoxVerified.Enabled = true;
                }

                trackBarIndexRange_ValueChanged(null, null);

                lodViewerME2.Mode = lodViewerME3.Mode = mode;
            }
            else
            {
                labelIndex.Enabled = labelRange.Enabled = false;
                trackBarIndex.Enabled = trackBarRange.Enabled = false;
                checkBoxIgnore.Enabled = false;
                checkBoxVerified.Enabled = false;

                lodViewerME2.Mode = lodViewerME3.Mode = mode;
            }
        }

        void CreateMapping()
        {
            mapping = new VertexMapping(me2Save.PlayerRecord.Appearance.MorphHead.LOD0Vertices, me3Save.Player.Appearance.MorphHead.Lod0Vertices);

            lodViewerME2.SetMapData(mapping, false);
            lodViewerME3.SetMapData(mapping, true);
        }

        private void trackBarIndexRange_ValueChanged(object sender, EventArgs e)
        {
            if (mapping != null)
            {
                lodViewerME2.SetMapSelection(trackBarIndex.Value, trackBarRange.Value);
                lodViewerME3.SetMapSelection(trackBarIndex.Value, trackBarRange.Value);

                checkBoxIgnore.Checked = mapping.Ignore[trackBarIndex.Value];
                labelVertex.Text = trackBarIndex.Value.ToString();
                checkBoxVerified.Checked = mapping.Verified[trackBarIndex.Value];

                labelMapping.Text = "{";
                foreach (int i in mapping.Mapping[trackBarIndex.Value]) labelMapping.Text += i.ToString() + ", ";
                if (labelMapping.Text.Length > 1) labelMapping.Text = labelMapping.Text.Substring(0, labelMapping.Text.Length - 2);
                labelMapping.Text += "}";

                if (checkBoxTrack.Checked)
                {
                    Vector3 pos = new Vector3(
                        me2Save.PlayerRecord.Appearance.MorphHead.LOD0Vertices[trackBarIndex.Value].X,
                        me2Save.PlayerRecord.Appearance.MorphHead.LOD0Vertices[trackBarIndex.Value].Y,
                        me2Save.PlayerRecord.Appearance.MorphHead.LOD0Vertices[trackBarIndex.Value].Z);
                    lodViewerME2.Focus(pos);
                    lodViewerME3.Focus(pos);
                }
            }
        }

        private void lodViewerME2_MouseClick(object sender, MouseEventArgs e)
        {
            string mode = comboBoxDisplay.SelectedItem.ToString();
            if (mode == "Mapping" && e.Button == MouseButtons.Left && downMouseState.X == e.X && downMouseState.Y == e.Y)
            {
                int n = lodViewerME2.Pick(e.X, e.Y);
                if (n != -1) trackBarIndex.Value = n;
            }
        }
        private void lodViewerME3_MouseClick(object sender, MouseEventArgs e)
        {
            string mode = comboBoxDisplay.SelectedItem.ToString();
            if (mode == "Mapping" && e.Button == MouseButtons.Left)
            {
                int n = lodViewerME3.Pick(e.X, e.Y);

                if (n != -1)
                {
                    if ((ModifierKeys & Keys.Control) != 0) mapping.Mapping[trackBarIndex.Value].Add(n);
                    else if ((ModifierKeys & Keys.Alt) != 0) mapping.Mapping[trackBarIndex.Value].Remove(n);
                    
                    labelMapping.Text = "{";
                    foreach (int i in mapping.Mapping[trackBarIndex.Value]) labelMapping.Text += i.ToString() + ", ";
                    if (labelMapping.Text.Length > 1) labelMapping.Text = labelMapping.Text.Substring(0, labelMapping.Text.Length - 2);
                    labelMapping.Text += "}";

                    lodViewerME2.SetMapSelection(trackBarIndex.Value, trackBarRange.Value);
                    lodViewerME3.SetMapSelection(trackBarIndex.Value, trackBarRange.Value);
                    lodViewerME2.RefreshMapData();
                    lodViewerME3.RefreshMapData();
                }
            }
        }

        Object LoadObject(string filename, Type type)
        {
            FileStream reader = new FileStream(Application.StartupPath + "\\" + filename + ".xml", FileMode.Open);
            DataContractSerializer serializer = new DataContractSerializer(type);
            Object obj = serializer.ReadObject(reader);
            reader.Close();
            return obj;
        }
        void SaveObject(string filename, Object obj)
        {
            FileStream writer = new FileStream(Application.StartupPath + "\\" + filename + ".xml", FileMode.Create);
            DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
            serializer.WriteObject(writer, obj);
            writer.Close();
        }

        private void checkBoxIgnore_CheckedChanged(object sender, EventArgs e)
        {
            if (mapping != null)
            {
                mapping.Ignore[trackBarIndex.Value] = checkBoxIgnore.Checked;
                lodViewerME2.RefreshMapData();
                lodViewerME3.RefreshMapData();
            }
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Add)
            {
                lodViewerME2.IncreasePointSize(me2Save.PlayerRecord.Appearance.MorphHead.LOD0Vertices);
                lodViewerME3.IncreasePointSize(me3Save.Player.Appearance.MorphHead.Lod0Vertices);
                lodViewerME2.SetMapData(mapping, false);
                lodViewerME3.SetMapData(mapping, true);
                lodViewerME2.SetMapSelection(trackBarIndex.Value, trackBarRange.Value);
                lodViewerME3.SetMapSelection(trackBarIndex.Value, trackBarRange.Value);
            }
            else if (e.KeyCode == Keys.Subtract)
            {
                lodViewerME2.DecreasePointSize(me2Save.PlayerRecord.Appearance.MorphHead.LOD0Vertices);
                lodViewerME3.DecreasePointSize(me3Save.Player.Appearance.MorphHead.Lod0Vertices);
                lodViewerME2.SetMapData(mapping, false);
                lodViewerME3.SetMapData(mapping, true);
                lodViewerME2.SetMapSelection(trackBarIndex.Value, trackBarRange.Value);
                lodViewerME3.SetMapSelection(trackBarIndex.Value, trackBarRange.Value);
            }
            else if (e.KeyCode == Keys.C)
            {
                Vector3 pos = new Vector3(
                        me2Save.PlayerRecord.Appearance.MorphHead.LOD0Vertices[trackBarIndex.Value].X,
                        me2Save.PlayerRecord.Appearance.MorphHead.LOD0Vertices[trackBarIndex.Value].Y,
                        me2Save.PlayerRecord.Appearance.MorphHead.LOD0Vertices[trackBarIndex.Value].Z);
                lodViewerME2.Focus(pos);
                lodViewerME3.Focus(pos);
            }
            else if (e.KeyCode == Keys.Space)
            {
                checkBoxVerified.Checked = !checkBoxVerified.Checked;
            }
            else if (e.KeyCode == Keys.R)
            {
                checkBoxIgnore.Checked = !checkBoxIgnore.Checked;
            }
        }

        private void checkBoxVerified_CheckedChanged(object sender, EventArgs e)
        {
            if (mapping != null)
            {
                mapping.Verified[trackBarIndex.Value] = checkBoxVerified.Checked;

                lodViewerME2.RefreshMapData();
                lodViewerME3.RefreshMapData();
            }
        }

        void AdjustLOD()
        {
            List<ME2Vector> lod2 = me2Save.PlayerRecord.Appearance.MorphHead.LOD0Vertices;
            List<ME3Vector> lod3 = me3Save.Player.Appearance.MorphHead.Lod0Vertices;

            for (int i = 0; i < mapping.Mapping.Length; i++)
            {
                if (mapping.Verified[i])
                {
                    ME3Vector vec = new ME3Vector()
                    {
                        X = lod2[i].X,
                        Y = lod2[i].Y,
                        Z = lod2[i].Z
                    };
                    if (mapping.Mapping[i].Count > 1)
                    {
                        Vector3 avr = Vector3.Zero;
                        foreach (int v in mapping.Mapping[i]) avr += new Vector3(lod3[v].X, lod3[v].Y, lod3[v].Z);
                        avr /= mapping.Mapping[i].Count;
                        Vector3 offset = new Vector3(vec.X, vec.Y, vec.Z) - avr;
                        foreach (int v in mapping.Mapping[i])
                        {
                            Vector3 res = new Vector3(lod3[v].X, lod3[v].Y, lod3[v].Z) + offset;

                            lod3[v].X = res.X;
                            lod3[v].Y = res.Y;
                            lod3[v].Z = res.Z;
                        }
                    }
                    else if (mapping.Mapping[i].Count == 1)
                    {
                        foreach (int v in mapping.Mapping[i])
                        {
                            lod3[v] = vec;
                        }
                    }
                }
            }
        }

        void AdjustBones()
        {
            List<Gibbed.MassEffect2.FileFormats.Save.OffsetBone> bones2 = me2Save.PlayerRecord.Appearance.MorphHead.OffsetBones;
            List<Gibbed.MassEffect3.FileFormats.Save.MorphHead.OffsetBone> bones3 = me3Save.Player.Appearance.MorphHead.OffsetBones;

            foreach (Gibbed.MassEffect2.FileFormats.Save.OffsetBone b2 in bones2)
            {
                foreach (Gibbed.MassEffect3.FileFormats.Save.MorphHead.OffsetBone b3 in bones3)
                {
                    if (String.Compare(b2.Name, b3.Name, true) == 0)
                    {
                        b3.Offset.X = b2.Offset.X;
                        b3.Offset.Y = b2.Offset.Y;
                        b3.Offset.Z = b3.Offset.Z;
                        break;
                    }
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog3.FileName != "")
            {
                AdjustLOD();
                if (checkBoxBones.Checked) AdjustBones();

                string savename = Path.GetDirectoryName(openFileDialog3.FileName) + "\\Save_1000.pcsav";
                using (var file = File.Open(savename, FileMode.Create))
                {
                    Gibbed.MassEffect3.FileFormats.SFXSaveGameFile.Write(me3Save, file);
                    file.Close();

                    MessageBox.Show("Saved file to " + savename);
                }
                using (var file = File.Open(openFileDialog3.FileName, FileMode.Open))
                {
                    LoadME3save(file);
                    file.Close();
                }
            }
            return;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.Delete(Application.StartupPath + "\\" + "lod_mapping" + ".xml");

            CreateMapping();
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            Size s = new Size();
            s.Height = toolStripPath2.Size.Height;
            s.Width = 270 + (toolStrip1.Size.Width - 924) / 2;
            toolStripPath2.Size = s;
            toolStripPath3.Size = s;
        }

        private void toolStripOpen2_Click(object sender, EventArgs e)
        {
            openFileDialog2.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\BioWare\\Mass Effect 2\\Save";

            if (openFileDialog2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (var file = openFileDialog2.OpenFile())
                {
                    LoadME2Save(file);
                    file.Close();
                };

                toolStripPath2.Text = openFileDialog2.FileName;
                SetupEditor();
            }
        }

        private void toolStripOpen3_Click(object sender, EventArgs e)
        {
            openFileDialog3.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\BioWare\\Mass Effect 3\\Save";

            if (openFileDialog3.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (var file = openFileDialog3.OpenFile())
                {
                    LoadME3save(file);
                    file.Close();
                };

                toolStripPath3.Text = openFileDialog3.FileName;
                SetupEditor();
            }
        }

        void SetupEditor()
        {
            if (me2Save != null)
            {
                lodViewerME2.SetData(me2Save.PlayerRecord.Appearance.MorphHead.LOD0Vertices);
                if (mapping != null) lodViewerME2.SetMapData(mapping, false);
            }
            if (me3Save != null)
            {
                lodViewerME3.SetData(me3Save.Player.Appearance.MorphHead.Lod0Vertices);
                if (mapping != null) lodViewerME3.SetMapData(mapping, true);
            }

            if (me2Save != null && me3Save != null && mapping == null)
            {
                CreateMapping();
            }
        }
    }
}

public class MySR : ToolStripSystemRenderer
{
    public MySR() { }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        //base.OnRenderToolStripBorder(e);
    }
}