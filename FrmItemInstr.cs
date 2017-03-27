using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraEditors.Controls;
using JFT.GlobalCache;

namespace JFT
{
    public partial class FrmItemInstr : Form
    {
        public FrmItemInstr()
        {
            InitializeComponent();
        }

        private void FrmItemInstr_Load(object sender, EventArgs e)
        {            
            chkBox.DisplayMember = "Value";
            chkBox.ValueMember = "Key";
            chkBox.CheckOnClick = true;
            this.tl_test_item.OptionsBehavior.Editable = false;
            this.tl_test_item.OptionsMenu.EnableColumnMenu = false;
            this.tl_test_item.KeyFieldName = "id";
            this.tl_test_item.ParentFieldName = "parentid";
            this.tl_test_item.OptionsBehavior.AllowIndeterminateCheckState = true;
            chkBox.DataSource = CacheData.dic_instruments;
            onLoadItems();
        }
        private void onLoadItems()
        {
            string[] cols = { "id", "parentid", "name", "item_id", "instruments" };
            DataTable dt = new DataTable();
            foreach (string col in cols)
            {
                dt.Columns.Add(col);
            }

            DataTable dt_pro_items = Db.DbHelper.GetTableLike("test_item_model" , "item_name" ,"");
            foreach (DataRow dr in dt_pro_items.Rows)
            {
                DataRow newDr = dt.NewRow();
                string id = "1_" + dr["item_id"];
                string str_instruments = dr["instruments"].ToString();
                newDr["id"] = id;
                newDr["parentid"] = "-1";
                newDr["name"] = dr["item_name"];
                newDr["item_id"] = dr["item_id"];              
                newDr["instruments"] = dr["instruments"];
                dt.Rows.Add(newDr);

                if (str_instruments != null && str_instruments != "")
                { 
                    string[] or_lst_instr = str_instruments.Split('/');
                    if (or_lst_instr.Length > 1) //存在多种组合
                    {
                        for (int i = 0; i < or_lst_instr.Length; i++)
                        {
                            DataRow child_newDr = dt.NewRow();
                            string child_id = "2_" + id + "_" + (i + 1).ToString();
                            child_newDr["parentid"] = id;
                            child_newDr["id"] = child_id;
                            child_newDr["name"] = "选择" + (i + 1).ToString();
                            dt.Rows.Add(child_newDr);
                            foreach (string instr in or_lst_instr[i].Split(','))
                            {
                                DataRow grandChild_newDr = dt.NewRow();
                                grandChild_newDr["parentid"] = child_id;
                                grandChild_newDr["id"] = "3_"+ child_id + "_" + instr;
                                grandChild_newDr["name"] = CacheData.dic_instruments[MyMethod.obj2int(instr)];
                                grandChild_newDr["instruments"] = instr;
                                dt.Rows.Add(grandChild_newDr);
                            }
                        }
                    }
                    else
                    {
                        foreach (string instr in or_lst_instr[0].Split(','))
                        {
                            DataRow grandChild_newDr = dt.NewRow();
                            grandChild_newDr["parentid"] = id;
                            grandChild_newDr["id"] = "3_"+ id + "_" + instr;
                            grandChild_newDr["name"] = CacheData.dic_instruments[MyMethod.obj2int(instr)];
                            grandChild_newDr["instruments"] = instr;
                            dt.Rows.Add(grandChild_newDr);
                        }                    
                    }
                }
            }
            tl_test_item.DataSource = dt;
        }

        private void tl_item_instr_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
        {
            loadCheckedItems(e.Node);
        }

        private void loadCheckedItems(TreeListNode node)
        {
            node.ExpandAll();
            for (int i = 0; i < chkBox.ItemCount; i++)
            {
                chkBox.SetItemChecked(i, false);
            }
            foreach (TreeListNode childNode in node.Nodes)
            {
                if (!childNode.HasChildren)
                {
                    DataRowView child_drv = tl_test_item.GetDataRecordByNode(childNode) as DataRowView;
                    string item_id = child_drv["instruments"].ToString();
                    for (int i = 0; i < chkBox.ItemCount; i++)
                    {
                        if (chkBox.GetItemValue(i).ToString() == item_id)
                        {
                            chkBox.SetItemCheckState(i, CheckState.Checked);
                        }
                    }
                }
            }
        
        }
        private void btn_modify_Click(object sender, EventArgs e)
        {
            List<int> lst_existOrder = new List<int>();
            TreeListNode node = tl_test_item.FocusedNode;
            DataRowView drv = tl_test_item.GetDataRecordByNode(node) as DataRowView;

            List<TreeListNode> deleteNodes = new List<TreeListNode>();
         
            foreach (TreeListNode childNode in node.Nodes)
            {
                DataRowView child_drv = tl_test_item.GetDataRecordByNode(childNode) as DataRowView;
                string item_id = child_drv["instruments"].ToString();
                for (int i = 0; i < chkBox.ItemCount; i++)
                {
                    if (chkBox.GetItemValue(i).ToString() == item_id)
                    {
                        if (!chkBox.GetItemChecked(i))//未被选中
                        {
                            deleteNodes.Add(childNode);
                            break;
                        }
                        else
                        {
                            lst_existOrder.Add(i);
                            break;
                        
                        }                      
                    }                  
                }             
            }

            foreach (TreeListNode delNode in deleteNodes)
            {
                tl_test_item.DeleteNode(delNode);
            }
            DataTable dt = tl_test_item.DataSource as DataTable;
            for (int i = 0; i < chkBox.ItemCount; i++)
            {
                if (chkBox.GetItemChecked(i) && lst_existOrder.IndexOf(i) == -1)//被选中但当前不存在
                { 
                    DataRow dr = dt.NewRow();
                    dr["instruments"] = chkBox.GetItemValue(i).ToString();
                    dr["parentid"] = drv["id"];
                    dr["name"] = chkBox.GetItemText(i).ToString();
                    dt.Rows.Add(dr);
                }
            }
        }

        private void btn_submit_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("item_id");
            dt.Columns.Add("instruments");
            foreach (TreeListNode node in tl_test_item.Nodes) //第一层节点为item
            {
                DataRowView drv = tl_test_item.GetDataRecordByNode(node) as DataRowView;
                DataRow dr = dt.NewRow();
                dr["item_id"] = drv["item_id"];
                List<string> lst_instruments = new List<string>();
                Boolean flg_or = false;
                foreach(TreeListNode childNode in node.Nodes) //第二层节点
                {
                    if (childNode.HasChildren) //有多个选择
                    {
                        flg_or = true;
                        lst_instruments.Add(getInstruments(childNode));
                    }
                }
                if (flg_or)
                {
                    dr["instruments"] = string.Join("/", lst_instruments);
                }
                else
                {
                    dr["instruments"] = getInstruments(node);
                }
                dt.Rows.Add(dr);
            }
            if (Db.DbHelper.UpdateTableColumn("test_item_model" ,dt , "item_id"))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("更新成功", "提示");
                onLoadItems();
            }
            else
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("更新失败", "注意");
            }
        }

        private string getInstruments(TreeListNode node)
        {
            List<string> lst_and_instruments = new List<string>();
            foreach (TreeListNode childNode in node.Nodes)
            {
                DataRowView childDrv = tl_test_item.GetDataRecordByNode(childNode) as DataRowView;
                string instruments = childDrv["instruments"].ToString();
                if (instruments != null && instruments != "")
                {
                    lst_and_instruments.Add(instruments);
                }
            }
            return string.Join(",", lst_and_instruments);
        }

        private void btn_addChoose_Click(object sender, EventArgs e)
        {
            TreeListNode node = tl_test_item.FocusedNode;
            if (node.Level != 0)
                return;
            DataRowView drv = tl_test_item.GetDataRecordByNode(node) as DataRowView;

            DataTable dt = tl_test_item.DataSource as DataTable;
            DataRow child_newDr = dt.NewRow();
            child_newDr["parentid"] = drv["id"].ToString();
            child_newDr["id"] = "2_" + drv["id"] + (node.Nodes.Count + 1); ;
            child_newDr["name"] = "选择" + (node.Nodes.Count + 1).ToString();
            dt.Rows.Add(child_newDr);
        }

        private void btn_filter_Click(object sender, EventArgs e)
        {
            Dictionary<int, string> dic_instruments = new Dictionary<int, string>();
            string str_filter = tb_filter.Text;
            foreach (KeyValuePair<int, string> kvp in CacheData.dic_instruments)
            {
                if (kvp.Value.IndexOf(str_filter) != -1)
                {
                    dic_instruments.Add(kvp.Key , kvp.Value);
                }
            }
            chkBox.DataSource = dic_instruments;
            loadCheckedItems(tl_test_item.FocusedNode);
        }
    }
}
