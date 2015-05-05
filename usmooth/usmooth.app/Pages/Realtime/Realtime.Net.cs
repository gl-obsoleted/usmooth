/*!lic_info

The MIT License (MIT)

Copyright (c) 2015 SeaSunOpenSource

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/

﻿using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using usmooth.common;

namespace usmooth.app.Pages
{
    public partial class Realtime
    {
        public void SetNetHandlers()
        {
            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_FrameData_Material, NetHandle_FrameData_Material);
            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_FrameData_Texture, NetHandle_FrameData_Texture);

            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_FrameDataV2, NetHandle_FrameDataV2);
            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_FrameDataV2_Meshes, NetHandle_FrameDataV2_Meshes);
            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_FrameDataV2_Names, NetHandle_FrameDataV2_Names);
            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_FrameDataEnd, NetHandle_FrameDataEnd);

            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_Editor_SelectionChanged, NetHandle_Editor_SelectionChanged);
        }

        public void NetRequest_FrameData()
        {
            ClearAllSelectionsAndHighlightedObjects();

            UsCmd cmd = new UsCmd();
            cmd.WriteNetCmd(eNetCmd.CL_RequestFrameData);
            NetManager.Instance.Send(cmd);
        }

        public void NetRequest_FlyToMesh(MeshObject mesh)
        {
            if (mesh == null)
            {
                ModernDialog.ShowMessage("请先选中一个 Mesh", "定位", MessageBoxButton.OK);
                return;
            }

            UsCmd cmd = new UsCmd();
            cmd.WriteNetCmd(eNetCmd.CL_FlyToObject);
            cmd.WriteInt32(mesh.InstID);
            NetManager.Instance.Send(cmd);
        }

        public bool NetHandle_FrameData_Material(eNetCmd cmd, UsCmd c)
        {
            UsLogging.Printf("eNetCmd.Handle_FrameData_Material received ({0}).", c.Buffer.Length);

            var materials = new ObservableCollection<MaterialObject>();
            int count = c.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var m = new MaterialObject();
                m.InstID = c.ReadInt32();
                m.Name = c.ReadString();
                m.ShaderName = c.ReadString();
                m.RefCnt = c.ReadInt32();

                m.RefList = new List<int>();
                for (int k = 0; k < m.RefCnt; k++)
                {
                    int owner = c.ReadInt32();
                    m.RefList.Add(owner);
                }

                materials.Add(m);
            }

            MaterialGrid.Dispatcher.Invoke(new Action(() =>
            {
                title_material.Text = string.Format("Materials ({0})", materials.Count);
                MaterialGrid.DataContext = materials;
            }));
            return true;
        }
        public bool NetHandle_FrameData_Texture(eNetCmd cmd, UsCmd c)
        {
            UsLogging.Printf("eNetCmd.Handle_FrameData_Texture received ({0}).", c.Buffer.Length);

            var textures = new ObservableCollection<TextureObject>();
            int count = c.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var m = new TextureObject();
                m.InstID = c.ReadInt32();
                m.Name = c.ReadString();
                m.PixelSize = c.ReadString();
                m.MemSize = c.ReadString();
                m.RefCnt = c.ReadInt32();
                m.RefList = new List<int>();
                for (int k = 0; k < m.RefCnt; k++)
                {
                    int owner = c.ReadInt32();
                    m.RefList.Add(owner);
                }

                textures.Add(m);
            }

            TextureGrid.Dispatcher.Invoke(new Action(() =>
            {
                title_texture.Text = string.Format("Textures ({0})", textures.Count);
                TextureGrid.DataContext = textures;
            }));
            return true;
        }

        public bool NetHandle_FrameDataV2(eNetCmd cmd, UsCmd c)
        {
            int var = c.ReadInt32();
            float f1 = c.ReadFloat();
            f1 = c.ReadFloat();
            f1 = c.ReadFloat(); 
            var meshList = UsCmdUtil.ReadIntList(c);
            var materialList = UsCmdUtil.ReadIntList(c);
            var textureList = UsCmdUtil.ReadIntList(c);

            MeshGrid.Dispatcher.Invoke(new Action(() =>
            {
                if (MeshGrid.DataContext == null)
                {
                    MeshGrid.DataContext = new ObservableCollection<MeshObject>();
                }
                else
                {
                    ((ObservableCollection<MeshObject>)(MeshGrid.DataContext)).Clear();
                }
            }));

            {
                UsCmd req = new UsCmd();
                req.WriteNetCmd(eNetCmd.CL_FrameV2_RequestMeshes);
                UsCmdUtil.WriteIntList(req, meshList);
                NetManager.Instance.Send(req);
                UsLogging.Printf("eNetCmd.NetHandle_FrameDataV2 [b]({0} meshes expected)[/b].", meshList.Count);
            }

            {
                UsCmd req = new UsCmd();
                req.WriteNetCmd(eNetCmd.CL_FrameV2_RequestNames);
                UsCmdUtil.WriteIntList(req, meshList);
                NetManager.Instance.Send(req);
                UsLogging.Printf("eNetCmd.NetHandle_FrameDataV2 [b]({0} names expected)[/b].", meshList.Count);
            }

            return true;
        }

        public bool NetHandle_FrameDataV2_Meshes(eNetCmd cmd, UsCmd c)
        {
            MeshGrid.Dispatcher.Invoke(new Action(() =>
            {
                int count = c.ReadInt32();
                UsLogging.Printf("eNetCmd.NetHandle_FrameDataV2_Meshes [b]({0} got)[/b].", count);
                for (int i = 0; i < count; i++)
                {
                    var m = new MeshObject();
                    m.Visible = true;
                    m.InstID = c.ReadInt32();
                    m.VertCnt = c.ReadInt32();
                    m.TriCnt = c.ReadInt32();
                    m.MatCnt = c.ReadInt32();
                    m.Size = c.ReadFloat();
                    ((ObservableCollection<MeshObject>)(MeshGrid.DataContext)).Add(m);
                }
            }));

            return true;
        }

        public bool NetHandle_FrameDataV2_Names(eNetCmd cmd, UsCmd c)
        {
            MeshGrid.Dispatcher.Invoke(new Action(() =>
            {
                int count = c.ReadInt32();
                UsLogging.Printf("eNetCmd.NetHandle_FrameDataV2_Names [b]({0} got)[/b].", count);
                for (int i = 0; i < count; i++)
                {
                    int instID = c.ReadInt32();
                    string instName = c.ReadString();
                    foreach (var item in MeshGrid.Items)
                    {
                        MeshObject mo = item as MeshObject;
                        if (mo != null && mo.InstID == instID)
                        {
                            mo.Name = instName;
                        }
                    }
                }

                MeshGrid.Items.Refresh();
            }));

            return true;
        }

        public bool NetHandle_FrameDataEnd(eNetCmd cmd, UsCmd c)
        {
            return true;
        }

        List<int> _instances = new List<int>();

        public bool NetHandle_Editor_SelectionChanged(eNetCmd cmd, UsCmd c)
        {
            int count = c.ReadInt32();
            UsLogging.Printf("eNetCmd.SV_Editor_SelectionChanged received ({0}, inst count: {1}).", c.Buffer.Length, count);

            _instances.Clear();
            for (int i = 0; i < count; i++)
            {
                int instID = c.ReadInt32();
                _instances.Add(instID);                                              
            }

            MeshGrid.Dispatcher.Invoke(new Action(() =>
            {
                ClearAllSelectionsAndHighlightedObjects();
                var meshes = HighlightMeshes(_instances, Colors.PaleTurquoise);
                foreach (var mesh in meshes)
                {
                    var matLst = HighlightMaterialByMesh(mesh, Colors.PaleTurquoise);
                    foreach (var mat in matLst)
                        HighlightTextureByMaterial(mat, Colors.PaleTurquoise);
                }
            }));

            return true;
        }
    }
}
