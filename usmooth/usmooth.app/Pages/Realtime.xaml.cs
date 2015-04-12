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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace usmooth.app.Pages
{
    public class MeshObject
    {
        public string Name { get; set; }
        public string MeshName { get; set; }
        public int VertCnt { get; set; }
        public int MatCnt { get; set; }
        public float Size { get; set; }
    }

    public class MaterialObject
    {
        public string Name { get; set; }
        public string ShaderName { get; set; }
        public int RefCnt { get; set; }
    }

    public class TextureObject
    {
        public string Name { get; set; }
        public string PixelSize { get; set; }
        public int RefCnt { get; set; }
        public string MemSize { get; set; }
    }

    /// <summary>
    /// Interaction logic for Realtime.xaml
    /// </summary>
    public partial class Realtime : UserControl
    {
        public Realtime()
        {
            InitializeComponent();

            MeshGrid.DataContext = GetMeshCollection();
            MaterialGrid.DataContext = GetMaterialCollection();
            TextureGrid.DataContext = GetTextureCollection();
        }

        private ObservableCollection<MeshObject> GetMeshCollection()
        {
            var meshes = new ObservableCollection<MeshObject>();
            meshes.Add(new MeshObject { Name = "Orlando", MeshName = "Gee", VertCnt = 100, MatCnt = 2, Size = 3.25f });
            meshes.Add(new MeshObject { Name = "Orlando", MeshName = "Gee", VertCnt = 50, MatCnt = 2, Size = 3.25f });
            meshes.Add(new MeshObject { Name = "Orlando", MeshName = "Gee", VertCnt = 20, MatCnt = 2, Size = 3.25f });
            return meshes;
        }

        private ObservableCollection<MaterialObject> GetMaterialCollection()
        {
            var materials = new ObservableCollection<MaterialObject>();
            materials.Add(new MaterialObject { Name = "Orlando", ShaderName = "Gee", RefCnt = 2 });
            return materials;
        }

        private ObservableCollection<TextureObject> GetTextureCollection()
        {
            var textures = new ObservableCollection<TextureObject>();
            textures.Add(new TextureObject { Name = "Orlando", PixelSize = "512x512", RefCnt = 2, MemSize = "356KB" });
            return textures;
        }
    }
}
