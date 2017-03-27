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
    public partial class FrmInstrument : Form
    {
        public FrmInstrument()
        {
            InitializeComponent();
        }

        private void FrmInstrument_Load(object sender, EventArgs e)
        {
            dgv_instr.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.Click;
            dgv_instr.OptionsBehavior.Editable = true;
            dgv_instr.OptionsMenu.EnableColumnMenu = false;
            dgv_instr.OptionsMenu.EnableGroupPanelMenu = false;
            dgv_instr.OptionsSelection.MultiSelect = false;
            foreach (KeyValuePair<string, string> kvp in GlobalCache.CacheData.dic_detail_instr_columns)
            {
                DevExpress.XtraGrid.Columns.GridColumn grid_column = new DevExpress.XtraGrid.Columns.GridColumn();
                grid_column.Caption = kvp.Value;
                grid_column.FieldName = kvp.Key;
                grid_column.Visible = true;
                grid_column.VisibleIndex = dgv_instr.Columns.Count;
                dgv_instr.Columns.Add(grid_column);
            }
        }

        private void btn_check_Click(object sender, EventArgs e)
        {
            string key = "instrument_name";
            string val = tb_instr_name_search.Text.Trim();
            DataTable dt = Db.DbHelper.GetTableLike("instrument_tbl" , key , val);
            gc_instr.DataSource = dt;
        }

        private void btn_submit_Click(object sender, EventArgs e)
        {
            DataTable dt = gc_instr.DataSource as DataTable;
            if (Db.DbHelper.UpdateTable("instrument_tbl", dt, "instrument_id"))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("更新成功", "提示");
                GlobalCache.CacheData.dic_instruments = Db.DbHelper.GetInstrumentsDic();
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
