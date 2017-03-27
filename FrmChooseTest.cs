using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JFT.GlobalCache;
using DevExpress.XtraTreeList.Nodes;
using JFT.Entity;
using System.Text.RegularExpressions;

namespace JFT
{
    public partial class FrmChooseTest : Form
    {
        EntProject pro;
        public FrmChooseTest(EntProject pro)
        {
            InitializeComponent();
            this.pro = pro;
            if (pro == null)
            {
                this.pro = new EntProject();
            }
        }

        private void FrmChooseTest_Load(object sender, EventArgs e)
        {
            this.lk_device_type.Properties.ValueMember = "Key";
            this.lk_device_type.Properties.DisplayMember = "Value";
            this.lk_device_type.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Value", 10, "请选择"));
            this.lk_device_type.Properties.DataSource = CacheData.dic_group_field[(int)GlobalCache.Groups.DevType];
            this.lk_device_type.EditValue = pro.device_type;

            this.tl_test_item.OptionsBehavior.Editable = true;
            this.tl_test_item.OptionsMenu.EnableColumnMenu = false;
            this.tl_test_item.KeyFieldName = "id";
            this.tl_test_item.ParentFieldName = "parentid";
            this.tl_test_item.OptionsView.ShowCheckBoxes = true;
            this.tl_test_item.OptionsBehavior.AllowIndeterminateCheckState = false;

            this.tl_test_item_choose.OptionsBehavior.Editable = true;
            this.tl_test_item_choose.OptionsMenu.EnableColumnMenu = false;
            this.tl_test_item_choose.KeyFieldName = "id";
            this.tl_test_item_choose.ParentFieldName = "parentid";
            this.tl_test_item_choose.OptionsView.ShowCheckBoxes = false;

            onLoadItems();

            this.tl_instrument.OptionsBehavior.Editable = false;
            this.tl_instrument.OptionsMenu.EnableColumnMenu = false;
            this.tl_instrument.KeyFieldName = "id";
            this.tl_instrument.ParentFieldName = "parentid";
            this.tl_instrument.OptionsView.ShowCheckBoxes = true;
            this.tl_instrument.OptionsBehavior.AllowIndeterminateCheckState = false;
            onLoadInstruments();

            tb_pro_num.Text = pro.pro_num;
            tb_pro_test_money.Text = pro.test_money;
        }
        private void lk_device_type_EditValueChanged(object sender, EventArgs e)
        {
            pro.device_type = lk_device_type.EditValue.ToString();
            onLoadItems();
        }
        private void onLoadItems()
        {
            DataSet ds = Db.DbHelper.GetProTestItem(pro.pro_code, pro.device_type);
            this.tl_test_item.DataSource = ds.Tables[0];
            this.tl_test_item.ExpandAll();

            this.tl_test_item_choose.DataSource = ds.Tables[1];
            this.tl_test_item_choose.ExpandAll();

            foreach (TreeListNode node in tl_test_item.Nodes)
            {
                setNodeCheckState(node);
            }

        }
        private void setNodeCheckState(TreeListNode node)
        {
            DataRowView drv = tl_test_item.GetDataRecordByNode(node) as DataRowView;
            if (Boolean.Parse(drv["tag"].ToString()))
                node.CheckState = CheckState.Checked;
            if (node.HasChildren)
            {
                foreach (TreeListNode childNode in node.Nodes)
                {
                    setNodeCheckState(childNode);
                }

            }
        }
        private void onLoadInstruments()
        {
            DataTable dt = Db.DbHelper.GetItemInstruments(pro.pro_code);
            this.tl_instrument.DataSource = dt;
            this.tl_instrument.ExpandAll();
            foreach (TreeListNode node in tl_instrument.Nodes)
            {
                DataRowView drv = tl_instrument.GetDataRecordByNode(node) as DataRowView;
                if (Boolean.Parse(drv["tag"].ToString()))
                    node.CheckState = CheckState.Checked;
                if (node.HasChildren)
                {
                    node.CheckState = CheckState.Checked;
                    foreach (TreeListNode childNode in node.Nodes)
                    {
                        DataRowView child_drv = tl_instrument.GetDataRecordByNode(childNode) as DataRowView;
                        if (Boolean.Parse(child_drv["tag"].ToString()))
                            childNode.CheckState = CheckState.Checked;
                        else
                            node.CheckState = CheckState.Unchecked;
                    }
                }
            }
        }

        /// <summary>
        /// 选择某一节点时,该节点的子节点全部选择  取消某一节点时,该节点的子节点全部取消选择
        /// </summary>
        /// <param name="node"></param>
        /// <param name="state"></param>
        private void SetCheckedChildNodes(TreeListNode node, CheckState check)
        {
            for (int i = 0; i < node.Nodes.Count; i++)
            {
                node.Nodes[i].CheckState = check;
                SetCheckedChildNodes(node.Nodes[i], check);
            }
        }

        /// <summary>
        /// 某节点的子节点有一个选择时,该节点选择   某节点的子节点全部不选择时,该节点不选择
        /// </summary>
        /// <param name="node"></param>
        /// <param name="check"></param>
        private void SetCheckedParentNodes(TreeListNode node, CheckState check)
        {
            if (node.ParentNode != null)
            {

                CheckState parentCheckState = node.ParentNode.CheckState;
                CheckState nodeCheckState;
                for (int i = 0; i < node.ParentNode.Nodes.Count; i++)
                {
                    nodeCheckState = (CheckState)node.ParentNode.Nodes[i].CheckState;
                    if (!check.Equals(nodeCheckState))//只要任意一个与其选中状态不一样即父节点状态选中
                    {
                        parentCheckState = CheckState.Checked;
                        break;
                    }
                    parentCheckState = check;//否则（该节点的兄弟节点选中状态都相同），则父节点选中状态为该节点的选中状态
                }

                node.ParentNode.CheckState = parentCheckState;
                SetCheckedParentNodes(node.ParentNode, check);//遍历上级节点
            }
        }

        private void deleteChooseNode(string str_id)
        {
            DataTable dt_choose = tl_test_item_choose.DataSource as DataTable;
            DataTable dt_choose_new = dt_choose.Clone();
            foreach (DataRow dr in dt_choose.Rows)
            {
                if (dr["id"].ToString() != str_id
                    && dr["parentid"].ToString() != str_id.ToString())
                {
                    DataRow dr_new = dt_choose_new.NewRow();
                    dr_new.ItemArray = dr.ItemArray;
                    dt_choose_new.Rows.Add(dr_new);
                }
            }
            tl_test_item_choose.DataSource = dt_choose_new;
            this.tl_test_item_choose.ExpandAll();
            tl_test_item_choose.FocusedNode = tl_test_item_choose.GetNodeByVisibleIndex(dt_choose_new.Rows.Count - 1);
        }

        private void insertChooseNode(int pos , TreeListNode node)
        {
            DataTable dt_choose = tl_test_item_choose.DataSource as DataTable;
            DataRow dr_new = dt_choose.NewRow();

            DataRowView drv = tl_test_item.GetDataRecordByNode(node) as DataRowView;

            dr_new.ItemArray = drv.Row.ItemArray;
            dt_choose.Rows.InsertAt(dr_new, pos);
            tl_test_item_choose.ExpandAll();
            tl_test_item_choose.FocusedNode = tl_test_item_choose.GetNodeByVisibleIndex(pos);
        }

        private void calTotMoney()
        {
            int total_money = 0;
            foreach (TreeListNode node in tl_test_item_choose.Nodes)
            {
                DataRowView child_drv = tl_test_item_choose.GetDataRecordByNode(node) as DataRowView;
                total_money += MyMethod.obj2int(child_drv["real_unit_price"]);
            }
            total_money *= MyMethod.obj2int(tb_pro_num.Text);
            tb_pro_test_money.Text = total_money.ToString();
        }
        private void tl_test_item_AfterCheckNode(object sender, DevExpress.XtraTreeList.NodeEventArgs e)
        {
            if (e.Node.Level == 0)
            {
                SetCheckedChildNodes(e.Node, e.Node.CheckState);
                SetCheckedParentNodes(e.Node, e.Node.CheckState);
                DataTable dt_choose = tl_test_item_choose.DataSource as DataTable;
                dt_choose.Clear();
                if (e.Node.Checked)
                {
                    int pos = 0;
                    foreach (TreeListNode childNode in e.Node.Nodes)
                    {
                        insertChooseNode(pos++, childNode);
                        foreach (TreeListNode grandChildNode in childNode.Nodes)
                        {
                            insertChooseNode(pos++, grandChildNode);
                        }
                    }                
                }              
            }
            else
            {
                bool pre_check = e.Node.ParentNode.Checked;
                SetCheckedChildNodes(e.Node, e.Node.CheckState);
                SetCheckedParentNodes(e.Node, e.Node.CheckState);

                if (e.Node.Checked)
                {
                    TreeListNode node = tl_test_item_choose.FocusedNode;
                    if (node != null && node.HasChildren)
                    {
                        node = node.LastNode;
                    }
                    int pos = tl_test_item_choose.GetVisibleIndexByNode(node) + 1;

                    if (e.Node.Level == 2 && !pre_check && e.Node.ParentNode.Checked)
                    {
                        insertChooseNode(pos++, e.Node.ParentNode);
                    }

                    insertChooseNode(pos++, e.Node);
                    if (e.Node.HasChildren)
                    {
                        foreach (TreeListNode childNode in e.Node.Nodes)
                        {
                            insertChooseNode(pos++, childNode);
                        }
                    }
                }
                else
                {
                    if (e.Node.Level == 2 && e.Node.ParentNode.CheckState == CheckState.Unchecked)
                    {
                        DataRowView drv = tl_test_item.GetDataRecordByNode(e.Node.ParentNode) as DataRowView;
                        deleteChooseNode(drv["id"].ToString());
                    }
                    else
                    {
                        DataRowView drv = tl_test_item.GetDataRecordByNode(e.Node) as DataRowView;
                        deleteChooseNode(drv["id"].ToString());
                    }
                }
            }
            calTotMoney();
          
        }
        
        private void btn_item_submit_Click(object sender, EventArgs e)
        {
            bool flg_hasType = false; //试验是否分小类
            int childCount = 0; //子类个数
            int grandChildCount = 0; //小项个数 
            string str_items = "";

            string ts_std_eg = "";
            string[] cols = { "pro_type_item_id", "real_unit_price", "item_result", "item_eval", "reserve1", "reserve2", "reserve3" };
            DataTable dt_insert = new DataTable();
            foreach (string col in cols)
                dt_insert.Columns.Add(col);
            foreach (TreeListNode node in tl_test_item_choose.Nodes)
            {               
                {
                    if (childCount == 0)
                    {
                        str_items += node.GetDisplayText(0);
                    }
                    else if (childCount < 4)
                    {
                        str_items += "、" + node.GetDisplayText(0);
                    }
                    childCount++;
                    DataRowView child_drv = tl_test_item_choose.GetDataRecordByNode(node) as DataRowView;
                    DataRow dr = dt_insert.NewRow();
                    foreach (string col in cols)
                        dr[col] = child_drv[col].ToString();
                    dt_insert.Rows.Add(dr);
                    if (ts_std_eg == "")
                        ts_std_eg = child_drv["ts_std"].ToString().Trim();
                }
                if (node.HasChildren)
                {
                    flg_hasType = true;
                    foreach (TreeListNode childNode in node.Nodes)
                    {                        
                        grandChildCount++;
                        DataRowView child_drv = tl_test_item_choose.GetDataRecordByNode(childNode) as DataRowView;
                        DataRow dr = dt_insert.NewRow();
                        foreach (string col in cols)
                            dr[col] = child_drv[col].ToString();
                        dt_insert.Rows.Add(dr);
                        if (ts_std_eg == "")
                            ts_std_eg = child_drv["ts_std"].ToString().Trim();
                    }
                }
            }

            Regex rgx = new Regex(@"^(\S+\s+\S+)\s+");
            if (rgx.IsMatch(ts_std_eg))
            {
                ts_std_eg = rgx.Match(ts_std_eg).Groups[1].Value;
            }
            if (flg_hasType)
            {
                pro.test_result = string.Format(@"根据{0}等标准的要求，对{1}送检的{2}型{3}进行了{4}等{5}类{6}个项目的试验和测量。试验结果见报告第七项《试验项目和结果》。"
                    , ts_std_eg , pro.client, pro.pro_type, pro.pro_name, str_items, childCount, grandChildCount);
            }
            else
            {
                pro.test_result = string.Format(@"根据{0}等标准的要求，对{1}送检的{2}型{3}进行了{4}等{5}个项目的试验和测量。试验结果见报告第七项《试验项目和结果》。"
                    , ts_std_eg , pro.client, pro.pro_type, pro.pro_name, str_items, childCount);

            }

            pro.device_type = lk_device_type.EditValue.ToString();
            pro.test_money = tb_pro_test_money.Text;
            pro.pro_num = tb_pro_num.Text;
            Db.DbHelper.UpdatePro(pro, false);

            if (Db.DbHelper.ReUpdateProTestItem(pro.pro_code, dt_insert))
            {
                onLoadInstruments();
                DevExpress.XtraEditors.XtraMessageBox.Show("修改成功", "提示");
            }
            else
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("修改失败", "注意");
            }

        }

        private void btn_instr_submit_Click(object sender, EventArgs e)
        {
            HashSet<string> lst_instr_id = new HashSet<string>();
            DataTable dt = new DataTable();
            dt.Columns.Add("link_id");
            dt.Columns.Add("reserve1");
            foreach (TreeListNode node in tl_instrument.Nodes)
            {
                DataRow dr = dt.NewRow();
                DataRowView drv = tl_instrument.GetDataRecordByNode(node) as DataRowView;
                dr["link_id"] = drv["link_id"];
                HashSet<string> l_lst_instr_id = new HashSet<string>();
                foreach (TreeListNode childNode in node.Nodes)
                {
                    if (childNode.CheckState == CheckState.Checked)
                    {
                        DataRowView child_drv = tl_instrument.GetDataRecordByNode(childNode) as DataRowView;
                        l_lst_instr_id.Add(child_drv["instr_id"].ToString());
                        lst_instr_id.Add(child_drv["instr_id"].ToString());
                    }
                }
                dr["reserve1"] = String.Join(",", l_lst_instr_id.ToArray());
                dt.Rows.Add(dr);
            }

            pro.instruments = String.Join(",", lst_instr_id.ToArray());

            if (Db.DbHelper.UpdatePro(pro,false) 
                && Db.DbHelper.OnlyUpdateTable("link_pro_item_tbl", dt, "link_id"))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("修改成功", "提示");
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("修改失败", "注意");
            }
        }

        private void btn_item_cancel_Click(object sender, EventArgs e)
        {
            onLoadItems();
        }

        private void btn_instr_cancel_Click(object sender, EventArgs e)
        {
            onLoadInstruments();
        }

        private void tl_instrument_AfterCheckNode(object sender, DevExpress.XtraTreeList.NodeEventArgs e)
        {
            if (e.Node.Checked&&e.Node.Level == 1)
            {
                DataRowView child_drv = tl_instrument.GetDataRecordByNode(e.Node) as DataRowView;
                DataTable dt = Db.DbHelper.GetExpiredInstr(child_drv["instr_id"].ToString());
                if (dt != null && dt.Rows.Count > 0)
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show(String.Format("{0}已过期", child_drv["item_name"].ToString()), "提示");
                }
            }           
        }

        private void tl_test_item_choose_CellValueChanged(object sender, DevExpress.XtraTreeList.CellValueChangedEventArgs e)
        {
            calTotMoney();
        }

    }
}
