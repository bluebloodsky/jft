using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JFT.Entity;
using JFT.Db;

namespace JFT
{
    public partial class FrmChoose : Form
    {
        public string ProCode { get; set; }
        public FrmChoose()
        {
            InitializeComponent();
            dgv.GroupPanelText = "请选择委托单";
            dgv.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.Click;
            dgv.OptionsBehavior.Editable = false;
            dgv.OptionsMenu.EnableColumnMenu = false;
            dgv.OptionsMenu.EnableGroupPanelMenu = false;

            foreach (KeyValuePair<string, string> kvp in GlobalCache.CacheData.dic_choose_columns)
            {
                DevExpress.XtraGrid.Columns.GridColumn grid_column = new DevExpress.XtraGrid.Columns.GridColumn();
                grid_column.Caption = kvp.Value;
                grid_column.FieldName = kvp.Key;
                grid_column.Visible = true;
                grid_column.VisibleIndex = dgv.Columns.Count;

                grid_column.OptionsFilter.AllowAutoFilter = false;
                grid_column.OptionsFilter.AllowFilter = false;
                grid_column.OptionsFilter.ImmediateUpdateAutoFilter = false;
                dgv.Columns.Add(grid_column);
            }

        }

        private void btn_submit_Click(object sender, EventArgs e)
        {
            int row = dgv.FocusedRowHandle;
            ProCode = dgv.GetRowCellValue(row, dgv.Columns[0]).ToString();
        }

        private void bindData(DataTable dt)
        {
            gridControl1.DataSource = dt;
        }

        public static EntProject getProject(string proCode)
        {
            DataTable dt = DbHelper.GetTableLike("pro_tbl", "pro_code", proCode, string.Join(",", GlobalCache.CacheData.dic_choose_columns.Keys));
            if (null == dt || dt.Rows.Count == 0)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("找不到委托单", "提示");
                return null;
            }
            string pro_code = dt.Rows[0][0].ToString();
            EntProject pro = null;
            if (dt.Rows.Count > 1)
            {
                FrmChoose frm = new FrmChoose();
                frm.bindData(dt);
                frm.StartPosition = FormStartPosition.CenterParent;
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    pro_code = frm.ProCode;
                }
                else
                {
                    return null;
                }
            }
            if ("" != pro_code)
            {
                pro = new EntProject();
                pro.pro_code = pro_code;
            }
            return pro;
        }
    }
}
