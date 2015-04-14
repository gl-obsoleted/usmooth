using FirstFloor.ModernUI.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using usmooth.common;
using FirstFloor.ModernUI.Windows.Navigation;
using FirstFloor.ModernUI.Windows.Controls;
using System.Collections;

namespace usmooth.app.Pages
{
    public class MeshObject
    {
        public int InstID { get; set; }
        public string Name { get; set; }
        public string MeshName { get; set; }
        public int VertCnt { get; set; }
        public int MatCnt { get; set; }
        public float Size { get; set; }
    }

    public class MaterialObject
    {
        public int InstID { get; set; }
        public string Name { get; set; }
        public string ShaderName { get; set; }
        public int RefCnt { get; set; }
        public List<int> RefList { get; set; }
    }

    public class TextureObject
    {
        public int InstID { get; set; }
        public string Name { get; set; }
        public string PixelSize { get; set; }
        public int RefCnt { get; set; }
        public string MemSize { get; set; }
        public List<int> RefList { get; set; }
    }

    /// <summary>
    /// Interaction logic for Realtime.xaml
    /// </summary>
    public partial class Realtime : UserControl, IContent
    {
        public Realtime()
        {
            InitializeComponent();

            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_FrameData_Mesh, Handle_FrameData_Mesh);
            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_FrameData_Material, Handle_FrameData_Material);
            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_FrameData_Texture, Handle_FrameData_Texture);
        }

        private ObservableCollection<MeshObject> GetMeshCollection(UsCmd c)
        {
            var meshes = new ObservableCollection<MeshObject>();

            int count = c.ReadInt32();
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var m = new MeshObject();
                    m.InstID = c.ReadInt32();
                    m.Name = c.ReadString();
                    m.MeshName = c.ReadString();
                    m.VertCnt = c.ReadInt32();
                    m.MatCnt = c.ReadInt32();
                    m.Size = (float)c.ReadInt32();
                    meshes.Add(m);
                }
            }

            return meshes;
        }

        private ObservableCollection<MaterialObject> GetMaterialCollection(UsCmd c)
        {
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
            return materials;
        }

        private ObservableCollection<TextureObject> GetTextureCollection(UsCmd c)
        {
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
            return textures;
        }

        public void OnFragmentNavigation(FragmentNavigationEventArgs e)
        {
        }
        public void OnNavigatedFrom(NavigationEventArgs e)
        {
        }
        public void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!NetManager.Instance.IsConnected)
            {
                if (ModernDialog.ShowMessage("usmooth is [b]offline[/b], connect to a running game first.", "offline", MessageBoxButton.OK) == MessageBoxResult.OK)
                {
                    UsLogging.Printf("trying to enter the '[b]realtime[/b]' page when usmooth is [b]offline[/b], back to the main page.");

                    DefaultLinkNavigator dln = new DefaultLinkNavigator();
                    dln.Navigate(new Uri("/Pages/Home.xaml", UriKind.Relative), this);
                    return;
                }
            }

            RequestServerForLatestData();
        }
        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
        }

        private bool Handle_FrameData_Mesh(eNetCmd cmd, UsCmd c)
        {
            UsLogging.Printf("eNetCmd.Handle_FrameData_Mesh received ({0}).", c.Buffer.Length);
            MeshGrid.Dispatcher.Invoke(new Action(() =>
            {
                MeshGrid.DataContext = GetMeshCollection(c);
            }));
            return true;
        }
        private bool Handle_FrameData_Material(eNetCmd cmd, UsCmd c)
        {
            UsLogging.Printf("eNetCmd.Handle_FrameData_Material received ({0}).", c.Buffer.Length);
            MaterialGrid.Dispatcher.Invoke(new Action(() =>
            {
                MaterialGrid.DataContext = GetMaterialCollection(c);
            }));
            return true;
        }
        private bool Handle_FrameData_Texture(eNetCmd cmd, UsCmd c)
        {
            UsLogging.Printf("eNetCmd.Handle_FrameData_Texture received ({0}).", c.Buffer.Length);
            TextureGrid.Dispatcher.Invoke(new Action(() =>
            {
                TextureGrid.DataContext = GetTextureCollection(c);
            }));
            return true;
        }

        private T GetSelectedObject<T>(DataGrid dataGrid) where T : class
        {
            var selected = dataGrid.SelectedCells;
            if (selected.Count == 0)
                return null;

            return selected[0].Item as T;
        }

        private void HighlightObject(DataGrid dg, object item)
        {
            var row = dg.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
            row.Background = Brushes.Chartreuse;
        }

        private void RemoveHighlightObject(DataGrid dg, object item)
        {
            var row = dg.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
            if (row != null)
                row.Background = dg.RowBackground;
        }

        private void HighlightMeshObjects(List<int> instIDs)
        {
            foreach (var item in MeshGrid.Items)
            {
                MeshObject mo = item as MeshObject;
                if (mo != null && instIDs.Contains(mo.InstID))
                {
                    HighlightObject(MeshGrid, item);
                    m_relevantMeshes.Add(mo);
                }
            }
        }

        private List<MaterialObject> HighlightMaterialObjects(List<int> instIDs)
        {
            List<MaterialObject> highlighted = new List<MaterialObject>();
            foreach (var item in MaterialGrid.Items)
            {
                MaterialObject mo = item as MaterialObject;
                if (mo != null && instIDs.Contains(mo.InstID))
                {
                    HighlightObject(MaterialGrid, item);
                    highlighted.Add(mo);
                    m_relevantMaterials.Add(mo);
                }
            }
            return highlighted;
        }

        private void HighlightTextureObjectsByOwner(int ownerInstID)
        {
            foreach (var item in TextureGrid.Items)
            {
                TextureObject to = item as TextureObject;
                if (to != null && to.RefList.Contains(ownerInstID))
                {
                    HighlightObject(TextureGrid, item);
                    m_relevantTextures.Add(to);
                }
            }
        }

        private void MeshGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var mo = GetSelectedObject<MeshObject>(MeshGrid);
            if (mo == null)
                return;

            UsCmd cmd = new UsCmd();
            cmd.WriteNetCmd(eNetCmd.CL_FlyToObject);
            cmd.WriteInt32(mo.InstID);
            NetManager.Instance.Send(cmd);
        }

        private void ClearAllSelectionsAndHighlightedObjects()
        {
            MeshGrid.UnselectAllCells();
            MaterialGrid.UnselectAllCells();
            TextureGrid.UnselectAllCells();

            foreach (var item in m_relevantMeshes)
                RemoveHighlightObject(MeshGrid, item);
            foreach (var item in m_relevantMaterials)
                RemoveHighlightObject(MaterialGrid, item);
            foreach (var item in m_relevantTextures)
                RemoveHighlightObject(TextureGrid, item);

            m_relevantMeshes.Clear();
            m_relevantMaterials.Clear();
            m_relevantTextures.Clear();
        }

        private void MaterialGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var mat = GetSelectedObject<MaterialObject>(MaterialGrid);
            if (mat == null)
                return;

            ClearAllSelectionsAndHighlightedObjects();

            m_relevantMaterials.Add(mat);
            HighlightObject(MaterialGrid, mat);

            HighlightMeshObjects(mat.RefList);
            HighlightTextureObjectsByOwner(mat.InstID);
        }

        private void TextureGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var texture = GetSelectedObject<TextureObject>(TextureGrid);
            if (texture == null)
                return;

            ClearAllSelectionsAndHighlightedObjects();

            m_relevantTextures.Add(texture);
            HighlightObject(TextureGrid, texture);

            List<MaterialObject> matLst = HighlightMaterialObjects(texture.RefList);

            foreach (var mat in matLst)
            {
                HighlightMeshObjects(mat.RefList);
            }
        }

        private void RequestServerForLatestData()
        {
            ClearAllSelectionsAndHighlightedObjects();

            UsCmd cmd = new UsCmd();
            cmd.WriteNetCmd(eNetCmd.CL_RequestFrameData);
            NetManager.Instance.Send(cmd);
        }

        private void bt_refresh_Click(object sender, RoutedEventArgs e)
        {
            RequestServerForLatestData();
        }

        List<MeshObject> m_relevantMeshes = new List<MeshObject>();
        List<MaterialObject> m_relevantMaterials = new List<MaterialObject>();
        List<TextureObject> m_relevantTextures = new List<TextureObject>();
    }
}
