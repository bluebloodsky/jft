using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JFT
{
    public partial class FrmWrnCheck : Form
    {
        public FrmWrnCheck(DataTable dt)
        {
            InitializeComponent();

            dgv.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.Click;
            dgv.OptionsBehavior.Editable = true;
            dgv.OptionsMenu.EnableColumnMenu = false;
            dgv.OptionsMenu.EnableGroupPanelMenu = false;
            dgv.OptionsSelection.MultiSelect = false;
            foreach (KeyValuePair<string, string> kvp in GlobalCache.CacheData.dic_check_instr_columns)
            {
                DevExpress.XtraGrid.Columns.GridColumn grid_column = new DevExpress.XtraGrid.Columns.GridColumn();
                grid_column.Caption = kvp.Value;
                grid_column.FieldName = kvp.Key;
                grid_column.Visible = true;
                grid_column.VisibleIndex = dgv.Columns.Count;
                dgv.Columns.Add(grid_column);
            }
            gc.DataSource = dt;
        }

        private void btn_export_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "待检测仪器记录表";
            sfd.Filter = "Excel2003工作簿(*.xls)|.xls";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                gc.ExportToXls(sfd.FileName);
                DataTable dt = gc.DataSource as DataTable;
                Db.DbHelper.UpdateInstrState(dt);
            }
            this.DialogResult = DialogResult.OK;
        }
    }
}
