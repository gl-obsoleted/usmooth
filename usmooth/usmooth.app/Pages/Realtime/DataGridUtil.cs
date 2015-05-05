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
using System.Windows.Controls;
using System.Windows.Media;

namespace usmooth.app
{
    public class DataGridUtil
    {
        public static T GetSelectedObject<T>(DataGrid dataGrid) where T : class
        {
            var selected = dataGrid.SelectedCells;
            if (selected.Count == 0)
                return null;

            return selected[0].Item as T;
        }

        public static void MarkAsHighlighted(DataGrid dg, object item, Color c)
        {
            var row = dg.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
            if (row != null)
                row.Background = new SolidColorBrush(c);
        }

        public static void ClearHighlighted(DataGrid dg, object item)
        {
            var row = dg.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
            if (row != null)
                row.Background = dg.RowBackground;
        }

        public static void ClearAllHighlighted(DataGrid dg)
        {
            foreach (var item in dg.Items)
            {
                ClearHighlighted(dg, item);
            }
        }
    }
}
