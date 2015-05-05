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

﻿using System;
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
