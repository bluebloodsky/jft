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

namespace JFT
{
    public partial class FrmItemGroup : Form
    {
        public FrmItemGroup()
        {
            InitializeComponent();
        }

        private void FrmItemGroup_Load(object sender, EventArgs e)
        {
            chkBox.DisplayMember = "item_name";
            chkBox.ValueMember = "item_id";
            chkBox.CheckOnClick = true;

            this.tl_test_item.OptionsBehavior.Editable = false;
            this.tl_test_item.OptionsMenu.EnableColumnMenu = false;
            this.tl_test_item.KeyFieldName = "id";
            this.tl_test_item.ParentFieldName = "parentid";
            this.tl_test_item.OptionsBehavior.AllowIndeterminateCheckState = true;
            onLoadItems();
        }
        private void onLoadItems()
        {

            DataTable dt_items = Db.DbHelper.GetTableLike("test_item_model", "item_name", "");
            chkBox.DataSource = dt_items;

            string[] cols = { "id", "parentid", "name", "pro_type_item_id", "item_id", "pro_type_id", "test_item_type" };
            DataTable dt = new DataTable();
            foreach (string col in cols)
            {
                dt.Columns.Add(col);
            }

            foreach (KeyValuePair<string, string> kvp in GlobalCache.CacheData.dic_group_field[(int)GlobalCache.Groups.DevType])
            {
                DataRow dr = dt.NewRow();
                dr["id"] = "1_" + kvp.Key;
                dr["parentid"] = "-1";
                dr["name"] = kvp.Value;
                dr["pro_type_id"] = kvp.Key;
                dt.Rows.Add(dr);
            }
            HashSet<string> set_item_type = new HashSet<string>();
            DataTable dt_pro_items = Db.DbHelper.GetItemGroup();
            foreach (DataRow dr in dt_pro_items.Rows)
            {
                DataRow newDr = dt.NewRow();
                string str_item_type = dr["test_item_type"].ToString();
                if (str_item_type == "")//没有二级节点
                {
                    newDr["parentid"] = "1_" + dr["pro_type_id"].ToString();
                    newDr["id"] = "2_" + dr["pro_type_item_id"];
                }
                else
                {
                    newDr["parentid"] = "2_" + str_item_type;
                    newDr["id"] = "3_" + dr["pro_type_item_id"];
                    /*
                    if (!set_item_type.Contains(str_item_type))
                    {
                        set_item_type.Add(str_item_type);
                        DataRow newTypeDr = dt.NewRow();
                        newTypeDr["parentid"] = "1_" + dr["pro_type_id"].ToString();
                        newTypeDr["id"] = "2_" + str_item_type;
                        newTypeDr["name"] = dr["test_item_type_name"];
                        newTypeDr["item_id"] = dr["test_item_type"];
                        dt.Rows.Add(newTypeDr);
                    }
                     * */

                }
                newDr["pro_type_item_id"] = dr["pro_type_item_id"];
                newDr["name"] = dr["item_name"];
                newDr["item_id"] = dr["item_id"];
                newDr["pro_type_id"] = dr["pro_type_id"];
                newDr["test_item_type"] = dr["test_item_type"];
                dt.Rows.Add(newDr);
            }
            tl_test_item.DataSource = dt;
        }

        private void tl_test_item_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
        {
            e.Node.ExpandAll();
            DataRowView drv = tl_test_item.GetDataRecordByNode(e.Node) as DataRowView;
            if (e.Node.Level != 0 && (drv["pro_type_item_id"].ToString() == null || drv["pro_type_item_id"].ToString() == "")) //不允许添加子节点
            {
                btn_modify.Enabled = false;
                return;
            }
            btn_modify.Enabled = true;
            try
            {
                for (int i = 0; i < chkBox.ItemCount; i++)
                {                    
                    chkBox.SetItemChecked(i, false);
                }
                foreach (TreeListNode childNode in e.Node.Nodes)
                {
                    DataRowView child_drv = tl_test_item.GetDataRecordByNode(childNode) as DataRowView;
                    string item_id = child_drv["item_id"].ToString();
                    for (int i = 0; i < chkBox.ItemCount; i++)
                    {
                        if (chkBox.GetItemValue(i).ToString() == item_id)
                        {
                            chkBox.SetItemCheckState(i, CheckState.Checked);
                        }
                    }

                }
            }
            catch
            {

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
                string item_id = child_drv["item_id"].ToString();
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
                   
                    dr["item_id"] = chkBox.GetItemValue(i).ToString();
                    dr["parentid"] = drv["id"];
                    dr["name"] = chkBox.GetItemText(i).ToString();
                    dr["id"] = drv["id"].ToString() + dr["item_id"].ToString();
                    dr["pro_type_id"] = drv["pro_type_id"];
                    dr["test_item_type"] = drv["pro_type_item_id"];
                    dt.Rows.Add(dr);
                }
            }
        }

        private void btn_submit_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("pro_type_item_id");
            dt.Columns.Add("pro_type_id");
            dt.Columns.Add("item_id");
            dt.Columns.Add("test_item_type");

            foreach(TreeListNode node in tl_test_item.Nodes)
            {
                addRow(node, ref dt);            
            }
            
            //DataTable dt = tl_test_item.DataSource as DataTable;
            if (Db.DbHelper.UpdateItemGroup(dt))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("更新成功", "提示");
                onLoadItems();                
            }
            else
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("更新失败", "注意");
            }
        }

        private void addRow(TreeListNode node, ref DataTable dt)
        {
            foreach (TreeListNode childNode in node.Nodes)
            {
                DataRowView child_drv = tl_test_item.GetDataRecordByNode(childNode) as DataRowView;
                DataRow dr = dt.NewRow();
                dr["pro_type_item_id"] = child_drv["pro_type_item_id"];
                dr["pro_type_id"] = child_drv["pro_type_id"];
                dr["item_id"] = child_drv["item_id"];
                dr["test_item_type"] = child_drv["test_item_type"];
                dt.Rows.Add(dr);
                if (childNode.HasChildren)
                {
                    addRow(childNode, ref dt);
                }
            }
        }
    }
}
