using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid;

namespace JFT
{
    public partial class FrmItemManager : Form
    {
        public FrmItemManager()
        {
            InitializeComponent();
        }

        private void FrmItemManager_Load(object sender, EventArgs e)
        {
            dgv.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.Click;
            dgv.OptionsBehavior.Editable = true;
            dgv.OptionsMenu.EnableColumnMenu = false;
            dgv.OptionsMenu.EnableGroupPanelMenu = false;
            dgv.OptionsSelection.MultiSelect = false;
            foreach (KeyValuePair<string, string> kvp in GlobalCache.CacheData.dic_detail_item_columns)
            {
                DevExpress.XtraGrid.Columns.GridColumn grid_column = new DevExpress.XtraGrid.Columns.GridColumn();
                grid_column.Caption = kvp.Value;
                grid_column.FieldName = kvp.Key;
                grid_column.Visible = true;
                grid_column.VisibleIndex = dgv.Columns.Count;
                dgv.Columns.Add(grid_column);
            }
        }

        private void btn_check_Click(object sender, EventArgs e)
        {
            string key = "item_name";
            string val = tb_instr_name_search.Text.Trim();
            DataTable dt = Db.DbHelper.GetTableLike("test_item_model" , key , val);
            gc.DataSource = dt;
        }

        private void btn_submit_Click(object sender, EventArgs e)
        {
            DataTable dt = gc.DataSource as DataTable;
            if (Db.DbHelper.UpdateTable("test_item_model", dt, "item_id"))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("更新成功", "提示");
            }
            else
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("更新失败", "注意");
            }
        }

        private void dgv_instr_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                GridView view = sender as GridView;
                view.DeleteRow(view.FocusedRowHandle);
            }
            else if (e.KeyCode == Keys.Insert)
            {
                GridView view = sender as GridView;
                view.AddNewRow();
            }
        }
    }
}
