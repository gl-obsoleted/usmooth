using FirstFloor.ModernUI.Windows.Controls;
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
        private void SetNetHandlers()
        {
            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_FrameData_Mesh, NetHandle_FrameData_Mesh);
            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_FrameData_Material, NetHandle_FrameData_Material);
            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_FrameData_Texture, NetHandle_FrameData_Texture);

            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_Editor_SelectionChanged, NetHandle_Editor_SelectionChanged);
        }

        private void NetRequest_FrameData()
        {
            ClearAllSelectionsAndHighlightedObjects();

            UsCmd cmd = new UsCmd();
            cmd.WriteNetCmd(eNetCmd.CL_RequestFrameData);
            NetManager.Instance.Send(cmd);
        }

        private void NetRequest_FlyToMesh(MeshObject mesh)
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

        int _meshExpectedCount = 0;
        ObservableCollection<MeshObject> _meshes;
        private bool NetHandle_FrameData_Mesh(eNetCmd cmd, UsCmd c)
        {
            short subCmd = c.ReadInt16();
            switch ((eSubCmd_TransmitStage)subCmd)
            {
                case eSubCmd_TransmitStage.DataBegin:
                    {
                        MeshGrid.Dispatcher.Invoke(new Action(() =>
                        {
                            if (_meshes == null)
                            {
                                _meshes = new ObservableCollection<MeshObject>();
                            }
                            else
                            {
                                _meshes.Clear();
                            }
                            _meshExpectedCount = c.ReadInt32();
                            UsLogging.Printf("eNetCmd.NetHandle_FrameData_Mesh [b](DataBegin)[/b] ({0}).", _meshExpectedCount);
                        }));
                    }
                    break;

                case eSubCmd_TransmitStage.DataSlice:
                    {
                        int count = c.ReadInt32();
                        UsLogging.Printf("eNetCmd.NetHandle_FrameData_Mesh [b](DataSlice)[/b] ({0}).", count);
                        if (count > 0)
                        {
                            MeshGrid.Dispatcher.Invoke(new Action(() =>
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    var m = new MeshObject();
                                    m.InstID = c.ReadInt32();
                                    m.Name = c.ReadString();
                                    m.VertCnt = c.ReadInt32();
                                    m.MatCnt = c.ReadInt32();
                                    m.Size = (float)c.ReadFloat();
                                    _meshes.Add(m);
                                }
                            }));
                        }
                    }
                    break;

                case eSubCmd_TransmitStage.DataEnd:
                    {
                        UsLogging.Printf("eNetCmd.NetHandle_FrameData_Mesh [b](DataEnd)[/b] (expected: {0} actual: {1}).", _meshExpectedCount, _meshes.Count);
                        if (_meshExpectedCount != _meshes.Count)
                        {
                            UsLogging.Printf(LogWndOpt.Bold, "The actually received mesh count is mismatched with expected.");
                        }

                        MeshGrid.Dispatcher.Invoke(new Action(() =>
                        {
                            title_mesh.Text = string.Format("Meshes ({0})", _meshes.Count);
                            MeshGrid.DataContext = _meshes;
                        }));
                    }
                    break;
                default:
                    break;
            }
            return true;
        }
        private bool NetHandle_FrameData_Material(eNetCmd cmd, UsCmd c)
        {
            UsLogging.Printf("eNetCmd.Handle_FrameData_Material received ({0}).", c.Buffer.Length);

            var materials = new ObservableCollection<MaterialObject>();
            int count = c.ReadInt32();
            if (count > 0)
            {
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
            }

            MaterialGrid.Dispatcher.Invoke(new Action(() =>
            {
                title_material.Text = string.Format("Materials ({0})", materials.Count);
                MaterialGrid.DataContext = materials;
            }));
            return true;
        }
        private bool NetHandle_FrameData_Texture(eNetCmd cmd, UsCmd c)
        {
            UsLogging.Printf("eNetCmd.Handle_FrameData_Texture received ({0}).", c.Buffer.Length);

            var textures = new ObservableCollection<TextureObject>();
            int count = c.ReadInt32();
            if (count > 0)
            {
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
            }

            TextureGrid.Dispatcher.Invoke(new Action(() =>
            {
                title_texture.Text = string.Format("Textures ({0})", textures.Count);
                TextureGrid.DataContext = textures;
            }));
            return true;
        }

        List<int> _instances = new List<int>();

        private bool NetHandle_Editor_SelectionChanged(eNetCmd cmd, UsCmd c)
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
