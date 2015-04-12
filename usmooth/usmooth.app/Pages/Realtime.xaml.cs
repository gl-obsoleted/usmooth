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
    }

    public class TextureObject
    {
        public int InstID { get; set; }
        public string Name { get; set; }
        public string PixelSize { get; set; }
        public int RefCnt { get; set; }
        public string MemSize { get; set; }
    }

    /// <summary>
    /// Interaction logic for Realtime.xaml
    /// </summary>
    public partial class Realtime : UserControl, IContent
    {
        public Realtime()
        {
            InitializeComponent();

            //MeshGrid.DataContext = GetMeshCollection();
            //MaterialGrid.DataContext = GetMaterialCollection();
            //TextureGrid.DataContext = GetTextureCollection();

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
                    for (int k = 0; k < m.RefCnt; k++)
                        c.ReadInt32();

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
                    for (int k = 0; k < m.RefCnt; k++)
                        c.ReadInt32();

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

            UsCmd cmd = new UsCmd();
            cmd.WriteNetCmd(eNetCmd.CL_RequestFrameData);
            NetManager.Instance.Send(cmd);
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
    }
}
