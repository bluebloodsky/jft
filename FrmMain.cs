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
using System.Text.RegularExpressions;
using System.IO;
using JFT.GlobalCache;

namespace JFT
{
    public partial class FrmMain : Form
    {
        EntProject currentPro;
        Boolean flg_new; //标识委托单为新增还是修改
        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            cmb_testType.Properties.ValueMember = "Key";
            cmb_testType.Properties.DisplayMember = "Value";
            cmb_testType.Properties.DataSource = GlobalCache.CacheData.dic_group_field[(int)GlobalCache.Groups.TestType];

            cmb_reportFormat.Properties.ValueMember = "Key";
            cmb_reportFormat.Properties.DisplayMember = "Value";
            cmb_reportFormat.Properties.DataSource = GlobalCache.CacheData.dic_group_field[(int)GlobalCache.Groups.ReportFormat];

            cmb_sealType.Properties.ValueMember = "Key";
            cmb_sealType.Properties.DisplayMember = "Value";
            cmb_sealType.Properties.DataSource = GlobalCache.CacheData.dic_group_field[(int)GlobalCache.Groups.SealType];

            lk_compDo.Properties.ValueMember = "Key";
            lk_compDo.Properties.DisplayMember = "Value";
            lk_compDo.Properties.DataSource = GlobalCache.CacheData.dic_group_field[(int)GlobalCache.Groups.CompDo];
            lk_compDo.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Value", 10, "请选择"));
            lk_compDo.Properties.NullText = "";

            lk_reportType.Properties.ValueMember = "Key";
            lk_reportType.Properties.DisplayMember = "Value";
            lk_reportType.Properties.DataSource = GlobalCache.CacheData.dic_report_type;
            lk_reportType.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Value", 10, "请选择"));
            lk_reportType.Properties.NullText = "";

            lk_orgName.Properties.ValueMember = "Key";
            lk_orgName.Properties.DisplayMember = "Value";
            lk_orgName.Properties.DataSource = GlobalCache.CacheData.dic_group_field[(int)GlobalCache.Groups.ReportType];
            lk_orgName.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Value", 10, "请选择"));
            lk_orgName.Properties.NullText = "";

            lk_test_status.Properties.ValueMember = "Key";
            lk_test_status.Properties.DisplayMember = "Value";
            lk_test_status.Properties.DataSource = GlobalCache.CacheData.dic_test_status;
            lk_test_status.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Value", 10, "请选择"));
            lk_test_status.Properties.NullText = "";

            lk_report_status.Properties.ValueMember = "Key";
            lk_report_status.Properties.DisplayMember = "Value";
            lk_report_status.Properties.DataSource = GlobalCache.CacheData.dic_report_status;
            lk_report_status.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Value", 10, "请选择"));
            lk_report_status.Properties.NullText = "";

            dp_getDate.Properties.DisplayFormat.FormatString = "yyyy年MM月dd日";
            dp_getDate.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            dp_getDate.Properties.EditFormat.FormatString = "yyyy年MM月dd日";
            dp_getDate.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            dp_getDate.Properties.Mask.EditMask = "yyyy年MM月dd日";

            dp_planCompDate.Properties.DisplayFormat.FormatString = "yyyy年MM月dd日";
            dp_planCompDate.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            dp_planCompDate.Properties.EditFormat.FormatString = "yyyy年MM月dd日";
            dp_planCompDate.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            dp_planCompDate.Properties.Mask.EditMask = "yyyy年MM月dd日";

            dp_testDateEnd.Properties.DisplayFormat.FormatString = "yyyy年MM月dd日";
            dp_testDateEnd.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            dp_testDateEnd.Properties.EditFormat.FormatString = "yyyy年MM月dd日";
            dp_testDateEnd.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            dp_testDateEnd.Properties.Mask.EditMask = "yyyy年MM月dd日";

            dp_testDateStart.Properties.DisplayFormat.FormatString = "yyyy年MM月dd日";
            dp_testDateStart.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            dp_testDateStart.Properties.EditFormat.FormatString = "yyyy年MM月dd日";
            dp_testDateStart.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            dp_testDateStart.Properties.Mask.EditMask = "yyyy年MM月dd日";


            tl_test.OptionsBehavior.Editable = true;
            tl_test.OptionsMenu.EnableColumnMenu = false;
            tl_test.KeyFieldName = "id";
            tl_test.ParentFieldName = "parentid";

            foreach (KeyValuePair<string, string> kvp in GlobalCache.CacheData.dic_test_item_columns_1)
            {
                DevExpress.XtraTreeList.Columns.TreeListColumn grid_column = new DevExpress.XtraTreeList.Columns.TreeListColumn();
                grid_column.Caption = kvp.Value;
                grid_column.FieldName = kvp.Key;
                grid_column.Visible = true;
                grid_column.VisibleIndex = tl_test.Columns.Count;
                grid_column.OptionsFilter.AllowAutoFilter = false;
                grid_column.OptionsFilter.AllowFilter = false;
                grid_column.OptionsFilter.ImmediateUpdateAutoFilter = false;
                tl_test.Columns.Add(grid_column);
            }

            dgv_instr.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.Click;
            dgv_instr.OptionsBehavior.Editable = false;
            dgv_instr.OptionsMenu.EnableColumnMenu = false;
            dgv_instr.OptionsMenu.EnableGroupPanelMenu = false;
            dgv_instr.OptionsSelection.MultiSelect = false;
            foreach (KeyValuePair<string, string> kvp in GlobalCache.CacheData.dic_sel_instr_columns)
            {
                DevExpress.XtraGrid.Columns.GridColumn grid_column = new DevExpress.XtraGrid.Columns.GridColumn();
                grid_column.Caption = kvp.Value;
                grid_column.FieldName = kvp.Key;
                grid_column.Visible = true;
                grid_column.VisibleIndex = dgv_instr.Columns.Count;
                grid_column.OptionsFilter.AllowAutoFilter = false;
                grid_column.OptionsFilter.AllowFilter = false;
                grid_column.OptionsFilter.ImmediateUpdateAutoFilter = false;
                dgv_instr.Columns.Add(grid_column);
            }

            change_group_status(false);
            /*
            DataTable dt_exp_instr = DbHelper.GetExpiredInstr("");
            if (dt_exp_instr != null && dt_exp_instr.Rows.Count > 0)
            {
                FrmWrnCheck frm = new FrmWrnCheck(dt_exp_instr);
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.ShowDialog();
            }
             * */            
        }

        private void btn_pro_submit_Click(object sender, EventArgs e)
        {
            currentPro.report_type = (string)lk_reportType.EditValue;
            currentPro.client = tb_client.Text;
            currentPro.client_addr = tb_clientAddr.Text;
            currentPro.email = tb_email.Text;
            currentPro.fac_addr = tb_facAddr.Text;
            currentPro.factory = tb_factory.Text;
            currentPro.post_code = tb_postCode.Text;
            currentPro.linkman = tb_linkMan.Text;
            currentPro.pro_code = tb_proCode.Text;
            currentPro.pro_num = tb_proNum.Text;
            currentPro.pro_sn = tb_proSn.Text;
            currentPro.pro_type = tb_proType.Text;
            currentPro.report_num = tb_reportNum.Text;
            currentPro.tel = tb_tel.Text;
            currentPro.pro_name = tb_proName.Text;
            currentPro.dev_info = tb_devInfo.Text;

            currentPro.report_format = (string)cmb_reportFormat.EditValue;
            currentPro.seal_type = (string)cmb_sealType.EditValue;
            currentPro.test_type = (string)cmb_testType.EditValue;
            currentPro.plan_comp_time = MyMethod.obj2date(dp_planCompDate.EditValue);
            currentPro.report_type = (string)lk_reportType.EditValue;
            
            currentPro.pro_status = tb_proStatus.Text;
            currentPro.get_date = MyMethod.obj2date(dp_getDate.EditValue);
            currentPro.come_type = tb_comeType.Text;
            currentPro.comp_do = (string)lk_compDo.EditValue;
            currentPro.attach_file = tb_attachFile.Text;
            currentPro.org_name = (string)lk_orgName.EditValue;

            currentPro.contract_code = tb_contractCode.Text;
            currentPro.in_money = tb_inMoney.Text;

            if (DbHelper.UpdatePro(currentPro, flg_new))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("更新成功", "提示");
                if (flg_new)
                {
                    GlobalCache.CacheData.dic_latestProCode[currentPro.report_type] = currentPro.pro_code;
                    flg_new = false;
                }
            }
            else
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("修改失败", "注意");
            }
        }

        private void btn_check_Click(object sender, EventArgs e)
        {
            string proCode = tb_proCode_search.Text.Trim();
            EntProject pro = FrmChoose.getProject(proCode);
            if (null != pro)
            {
                change_group_status(true);
                tb_proCode.ReadOnly = true;
                currentPro = pro; 
                refreshInfos();
                flg_new = false;
            }
        }


        private void btn_add_Click(object sender, EventArgs e)
        {
            currentPro = new EntProject();
            currentPro.report_type = "1";
            currentPro.report_format = "1";
            currentPro.test_type = "5";
            currentPro.seal_type = "1,3,4"; //检测报告
            currentPro.org_name = "2"; 
            currentPro.pro_num = "1";
            currentPro.report_num = "4";
            currentPro.comp_do = "1";

            currentPro.test_status = "0";
            currentPro.report_status = "0";
            currentPro.dev_info = "试品主要由球机、激光雷达、主机箱、后台软件、12V电源适配器组成，外观照片见图1。\r\n视频主要用于输电线路防外破在线监测系统。\r\n试验项目和要求依据标准由委托方确定。试品采用AC220V供电，试验时处于正常工作状态。";

            currentPro.pro_code = getNewProCode(currentPro.report_type);

            currentPro.create_time = DateTime.Now;
            currentPro.get_date = DateTime.Now;

            flg_new = true;
            loadProInfo();
            change_group_status(true);
            tb_proCode.ReadOnly = false;
            gc_instr.DataSource = null;
            tl_test.DataSource = null;

        }

        private string getNewProCode(string report_type)
        {
            string pro_code = "";
            string str_type = "0";
            if (report_type != "1")
            {
                str_type = "1";
            }

            if (GlobalCache.CacheData.dic_latestProCode.ContainsKey(report_type))
            {
                string old_pro_code = GlobalCache.CacheData.dic_latestProCode[report_type];
                Regex rgx = new Regex(@"^CEPRI-EETC06-([\d]{4})-"+ str_type + @"([\d]{3})$" );
                if (rgx.IsMatch(old_pro_code))
                {
                    String rgx_year = rgx.Match(old_pro_code).Groups[1].Value;
                    String rgx_num = rgx.Match(old_pro_code).Groups[2].Value;
                    int num = MyMethod.obj2int(rgx_num);
                    num++;
                    pro_code = String.Format("CEPRI-EETC06-{0}-{1}{2}", rgx_year , str_type , num.ToString("000"));
                }
            }

            if(pro_code == "")
            {
                int year = DateTime.Now.Year;
                pro_code = string.Format("CEPRI-EETC06-{0}-{1}001", year, str_type);
            }
            return pro_code;
        
        }
        private void refreshInfos()
        {
            DbHelper.GetProDetailById(currentPro.pro_code, out currentPro);
            loadProInfo();
            loadTestInfo();
            loadInstruments();
        }

        private void loadProInfo()
        {
            tb_client.Text = currentPro.client;
            tb_clientAddr.Text = currentPro.client_addr;
            tb_email.Text = currentPro.email;
            tb_facAddr.Text = currentPro.fac_addr;
            tb_factory.Text = currentPro.factory;
            tb_postCode.Text = currentPro.post_code;
            tb_linkMan.Text = currentPro.linkman;
            tb_proCode.Text = currentPro.pro_code;
            tb_proNum.Text = currentPro.pro_num;
            tb_proSn.Text = currentPro.pro_sn;
            tb_proType.Text = currentPro.pro_type;
            tb_reportNum.Text = currentPro.report_num;
            tb_tel.Text = currentPro.tel;
            tb_proName.Text = currentPro.pro_name;
            tb_devInfo.Text = currentPro.dev_info;

            cmb_reportFormat.EditValue = currentPro.report_format;
            lk_orgName.EditValue = currentPro.org_name;
            lk_reportType.EditValue = currentPro.report_type;
            cmb_sealType.EditValue = currentPro.seal_type;
            cmb_testType.EditValue = currentPro.test_type;
            lk_compDo.EditValue = currentPro.comp_do;

            cmb_reportFormat.RefreshEditValue();
            lk_orgName.RefreshEditValue();
            lk_reportType.RefreshEditValue();
            cmb_sealType.RefreshEditValue();
            cmb_testType.RefreshEditValue();
            lk_compDo.RefreshEditValue();

            dp_planCompDate.EditValue = currentPro.plan_comp_time;
            dp_testDateStart.EditValue = currentPro.test_date_start;
            dp_testDateEnd.EditValue = currentPro.test_date_end;
            tb_test_result.Text = currentPro.test_result;

            tb_inMoney.Text = currentPro.in_money;
            tb_contractCode.Text = currentPro.contract_code;
            lk_test_status.EditValue = currentPro.test_status;
            lk_report_status.EditValue = currentPro.report_status;
            dp_comp_time.EditValue = currentPro.comp_time;

            dp_getDate.EditValue = currentPro.get_date;
            tb_proStatus.Text = currentPro.pro_status;
            tb_comeType.Text = currentPro.come_type;
            tb_attachFile.Text = currentPro.attach_file;

        }
        private void loadTestInfo()
        {
            DataTable dt = DbHelper.GetTestItemResult(currentPro.pro_code);
            dt.Columns.Add("id");
            dt.Columns.Add("parentid");
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["test_item_type"].ToString() == "") //根节点
                {
                    dr["parentid"] = "-1";
                    dr["id"] = "1_" + dr["pro_type_item_id"];
                }
                else
                {
                    dr["parentid"] = "1_" + dr["test_item_type"];
                    dr["id"] = string.Format("{0}_{01}", dr["parentid"], dr["pro_type_item_id"]);
                }
            }
            tl_test.DataSource = dt;
            tl_test.ExpandAll();
        }

        private void loadInstruments()
        {
            gc_instr.DataSource =  DbHelper.GetInstrument(currentPro.instruments);
        }
        private void btn_test_edit_Click(object sender, EventArgs e)
        {
            if (flg_new)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("请先保存当前项目", "提示");
                return;
            }
            FrmChooseTest frm = new FrmChooseTest(currentPro);
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                refreshInfos();
            }
        }

        private void btn_test_mod_Click(object sender, EventArgs e)
        {
            currentPro.test_date_start = MyMethod.obj2date(dp_testDateStart.EditValue);
            currentPro.test_date_end = MyMethod.obj2date(dp_testDateEnd.EditValue);
            currentPro.test_result = tb_test_result.Text;

            currentPro.test_status = (string)lk_test_status.EditValue;
            currentPro.report_status = (string)lk_report_status.EditValue;
            currentPro.comp_time = MyMethod.obj2date(dp_comp_time.EditValue);

            if (DbHelper.UpdatePro(currentPro,false) &&
            DbHelper.UpdateProTestItem(tl_test.DataSource as DataTable))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("修改成功", "提示");
            }
            else
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("修改失败", "注意");
            }
        }

        private void btn_pro_del_Click(object sender, EventArgs e)
        {
            if(DbHelper.DelPro(currentPro.pro_code))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("删除成功", "提示");
                change_group_status(false);
                CacheData.dic_latestProCode = Db.DbHelper.GetLatestProCode();
            }
            else
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("删除失败", "注意");
            }
        }

        private void change_group_status(Boolean flg)
        {
            group_pro.Enabled = flg;
            group_test.Enabled = flg;
            group_instrument.Enabled = flg;
        }


        private void btn_exportOrder_Click(object sender, EventArgs e)
        {
            if (WordHelper.ExportPro(currentPro))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("导出成功", "提示");
            }
            else
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("导出失败", "注意");
            }
        }

        private void btn_exportReport_Click(object sender, EventArgs e)
        {
            if (WordHelper.ExportTestReport(currentPro))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("导出成功", "提示");
            }
            else
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("导出失败", "注意");
            }
        }

        private void btn_exportRecord_Click(object sender, EventArgs e)
        {
            if (WordHelper.ExportRecordReport(currentPro))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("导出成功", "提示");
            }
            else
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("导出失败", "注意");
            }
        }

        private void menu_month_plan_Click(object sender, EventArgs e)
        {
            DateTime dt_end = new DateTime(DateTime.Now.Year , DateTime.Now.Month , 26);
            DateTime dt_begin = dt_end.AddMonths(-1);
            if (WordHelper.ExportMonthReport(dt_begin, dt_end))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("导出成功", "提示");
            }
            else
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("导出失败", "注意");
            }
        }

        private void menu_year_progress_Click(object sender, EventArgs e)
        {
            FileStream stream = new FileStream(CacheData.base_dir + "\\templets\\model\\progress.xls" , FileMode.Open);
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "xls files(*.xls)|*.xls";
            String year = DateTime.Now.Year.ToString();
            sfd.FileName = String.Format("{0}年实验进度", year);
            sfd.AddExtension = true;
            sfd.RestoreDirectory = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                DataTable dt = DbHelper.GetTableLike("pro_tbl", "create_time", year);
                ExcelNPIOHelper helper = new ExcelNPIOHelper(sfd.FileName);
                helper.ExcelUpdate(stream, dt, year);
                helper.Dispose();
            }
            stream.Close();
        }     
        private void menu_instrManager_Click(object sender, EventArgs e)
        {
            FrmInstrument frm = new FrmInstrument();
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.Show();
        }

        private void menu_itemManager_Click(object sender, EventArgs e)
        {
            FrmItemManager frm = new FrmItemManager();
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.Show();
        }

        private void menu_rel_item_group_Click(object sender, EventArgs e)
        {
            FrmItemGroup frm = new FrmItemGroup();
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.Show();
        }

        private void menu_rel_item_instrument_Click(object sender, EventArgs e)
        {
            FrmItemInstr frm = new FrmItemInstr();
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.Show();
        }

        private void lk_reportType_EditValueChanged(object sender, EventArgs e)
        {
            if (flg_new)
            {
                string report_type = (string)lk_reportType.EditValue;
                tb_proCode.Text = getNewProCode(report_type);
                if(report_type == "1")
                    cmb_sealType.EditValue = "1,3,4"; //检测报告
                else
                    cmb_sealType.EditValue = "5"; //试验报告
                cmb_sealType.RefreshEditValue();
            }
        }
 
    }
}
