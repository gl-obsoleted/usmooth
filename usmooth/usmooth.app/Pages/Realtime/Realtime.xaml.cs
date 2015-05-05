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

﻿using FirstFloor.ModernUI.Windows;
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
        public bool Visible { get; set; }
        public int InstID { get; set; }
        public string Name { get; set; }
        public int VertCnt { get; set; }
        public int TriCnt { get; set; }
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

            SetNetHandlers();
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

            DataGridUtil.MarkAsHighlighted(MeshGrid, mesh, Colors.Chartreuse);
            var matLst = HighlightMaterialByMesh(mesh, Colors.Chartreuse);
            foreach (var mat in matLst)
                HighlightTextureByMaterial(mat, Colors.Chartreuse);
        }

        private void MaterialGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var mat = DataGridUtil.GetSelectedObject<MaterialObject>(MaterialGrid);
            if (mat == null)
                return;
            ClearAllSelectionsAndHighlightedObjects();

            DataGridUtil.MarkAsHighlighted(MaterialGrid, mat, Colors.Chartreuse);
            HighlightMeshByMaterial(mat, Colors.Chartreuse);
            HighlightTextureByMaterial(mat, Colors.Chartreuse);
        }

        private void TextureGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var texture = DataGridUtil.GetSelectedObject<TextureObject>(TextureGrid);
            if (texture == null)
                return;
            ClearAllSelectionsAndHighlightedObjects();

            DataGridUtil.MarkAsHighlighted(TextureGrid, texture, Colors.Chartreuse);
            var matLst = HighlightMaterialByTexture(texture, Colors.Chartreuse);
            foreach (var mat in matLst)
                HighlightMeshByMaterial(mat, Colors.Chartreuse);
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

        private void MeshGrid_OnVisibleChecked(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            MeshObject mo = MeshGrid.SelectedItem as MeshObject;
            if (mo == null)
                return;

            if (mo.Name == null)    // is in initialization process
                return;

            NetManager.Instance.ExecuteCmd(string.Format("{0} {1}", ((bool)cb.IsChecked ? "showmesh" : "hidemesh"), mo.InstID));
        }
    }
}
