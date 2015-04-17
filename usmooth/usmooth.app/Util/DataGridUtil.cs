using System;
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
