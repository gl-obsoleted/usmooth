using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace usmooth.app.Pages
{
    public partial class Realtime
    {
        private void HighlightMeshByMaterial(MaterialObject material)
        {
            foreach (var item in MeshGrid.Items)
            {
                MeshObject mo = item as MeshObject;
                if (mo != null && material.RefList.Contains(mo.InstID))
                {
                    DataGridUtil.MarkAsHighlighted(MeshGrid, item);
                }
            }
        }

        private List<MaterialObject> HighlightMaterialByMesh(MeshObject mesh)
        {
            List<MaterialObject> highlighted = new List<MaterialObject>();
            foreach (var item in MaterialGrid.Items)
            {
                MaterialObject mo = item as MaterialObject;
                if (mo != null && mo.RefList.Contains(mesh.InstID))
                {
                    DataGridUtil.MarkAsHighlighted(MaterialGrid, item);
                    highlighted.Add(mo);
                }
            }
            return highlighted;
        }

        private List<MaterialObject> HighlightMaterialByTexture(TextureObject texture)
        {
            List<MaterialObject> highlighted = new List<MaterialObject>();
            foreach (var item in MaterialGrid.Items)
            {
                MaterialObject mo = item as MaterialObject;
                if (mo != null && texture.RefList.Contains(mo.InstID))
                {
                    DataGridUtil.MarkAsHighlighted(MaterialGrid, item);
                    highlighted.Add(mo);
                }
            }
            return highlighted;
        }

        private void HighlightTextureByMaterial(MaterialObject material)
        {
            foreach (var item in TextureGrid.Items)
            {
                TextureObject to = item as TextureObject;
                if (to != null && to.RefList.Contains(material.InstID))
                {
                    DataGridUtil.MarkAsHighlighted(TextureGrid, item);
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
