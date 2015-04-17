using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace usmooth.app.Pages
{
    public partial class Realtime
    {
        private List<MeshObject> HighlightMeshes(List<int> selection, Color c)
        {
            List<MeshObject> highlighted = new List<MeshObject>();
            foreach (var item in MeshGrid.Items)
            {
                MeshObject mo = item as MeshObject;
                if (mo != null && selection.Contains(mo.InstID))
                {
                    DataGridUtil.MarkAsHighlighted(MeshGrid, item, c);
                    highlighted.Add(mo);
                }
            }
            return highlighted;
        }

        private void HighlightMeshByMaterial(MaterialObject material, Color c)
        {
            foreach (var item in MeshGrid.Items)
            {
                MeshObject mo = item as MeshObject;
                if (mo != null && material.RefList.Contains(mo.InstID))
                {
                    DataGridUtil.MarkAsHighlighted(MeshGrid, item, c);
                }
            }
        }

        private List<MaterialObject> HighlightMaterialByMesh(MeshObject mesh, Color c)
        {
            List<MaterialObject> highlighted = new List<MaterialObject>();
            foreach (var item in MaterialGrid.Items)
            {
                MaterialObject mo = item as MaterialObject;
                if (mo != null && mo.RefList.Contains(mesh.InstID))
                {
                    DataGridUtil.MarkAsHighlighted(MaterialGrid, item, c);
                    highlighted.Add(mo);
                }
            }
            return highlighted;
        }

        private List<MaterialObject> HighlightMaterialByTexture(TextureObject texture, Color c)
        {
            List<MaterialObject> highlighted = new List<MaterialObject>();
            foreach (var item in MaterialGrid.Items)
            {
                MaterialObject mo = item as MaterialObject;
                if (mo != null && texture.RefList.Contains(mo.InstID))
                {
                    DataGridUtil.MarkAsHighlighted(MaterialGrid, item, c);
                    highlighted.Add(mo);
                }
            }
            return highlighted;
        }

        private void HighlightTextureByMaterial(MaterialObject material, Color c)
        {
            foreach (var item in TextureGrid.Items)
            {
                TextureObject to = item as TextureObject;
                if (to != null && to.RefList.Contains(material.InstID))
                {
                    DataGridUtil.MarkAsHighlighted(TextureGrid, item, c);
                }
            }
        }

        private void ClearAllSelectionsAndHighlightedObjects()
        {
            MeshGrid.UnselectAllCells();
            MaterialGrid.UnselectAllCells();
            TextureGrid.UnselectAllCells();

            DataGridUtil.ClearAllHighlighted(MeshGrid);
            DataGridUtil.ClearAllHighlighted(MaterialGrid);
            DataGridUtil.ClearAllHighlighted(TextureGrid);
        }
    }
}
