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

            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_FrameData_Mesh, NetHandle_FrameData_Mesh);
            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_FrameData_Material, NetHandle_FrameData_Material);
            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_FrameData_Texture, NetHandle_FrameData_Texture);
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

            NetRequest_FrameData();
        }
        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
        }

        private void MeshGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var mesh = DataGridUtil.GetSelectedObject<MeshObject>(MeshGrid);
            if (mesh == null)
                return;
            ClearAllSelectionsAndHighlightedObjects();

            DataGridUtil.MarkAsHighlighted(MeshGrid, mesh);
            var matLst = HighlightMaterialByMesh(mesh);
            foreach (var mat in matLst)
                HighlightTextureByMaterial(mat);
        }

        private void MaterialGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var mat = DataGridUtil.GetSelectedObject<MaterialObject>(MaterialGrid);
            if (mat == null)
                return;
            ClearAllSelectionsAndHighlightedObjects();

            DataGridUtil.MarkAsHighlighted(MaterialGrid, mat);
            HighlightMeshByMaterial(mat);
            HighlightTextureByMaterial(mat);
        }

        private void TextureGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var texture = DataGridUtil.GetSelectedObject<TextureObject>(TextureGrid);
            if (texture == null)
                return;
            ClearAllSelectionsAndHighlightedObjects();

            DataGridUtil.MarkAsHighlighted(TextureGrid, texture);
            var matLst = HighlightMaterialByTexture(texture);
            foreach (var mat in matLst)
                HighlightMeshByMaterial(mat);
        }

        private void bt_refresh_Click(object sender, RoutedEventArgs e)
        {
            NetRequest_FrameData();
        }

        private void bt_locate_Click(object sender, RoutedEventArgs e)
        {
            var mesh = DataGridUtil.GetSelectedObject<MeshObject>(MeshGrid);
            NetRequest_FlyToMesh(mesh);
        }
    }
}
