using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JFT.Entity;
using System.Data;
using System.Data.SQLite;

namespace JFT.Db
{
    class DbHelper
    {

        //获取委托单详细信息
        public static void GetProDetailById(string proCode, out EntProject ent)
        {
            string sql_cn = string.Format("select * from pro_tbl where pro_code = '{0}'", proCode);
            DataTable dt_cn = SQLiteHelper.ExecuteQuery(sql_cn);
            if (null == dt_cn || dt_cn.Rows.Count == 0)
            {
                ent = null;
                return;
            }
            ent = new EntProject();
            ent.pro_code = proCode;
            {
                DataRow dr = dt_cn.Rows[0];
                foreach (System.Reflection.PropertyInfo p in ent.GetType().GetProperties())
                {
                    string tempName = p.Name;
                    if (dt_cn.Columns.Contains(tempName))
                    {
                        object val = dr[tempName].ToString().Trim();
                        if (p.PropertyType == System.Type.GetType("System.DateTime"))
                        {
                            val = MyMethod.str2Date(dr[tempName].ToString());
                        }
                        p.SetValue(ent, val, null);

                    }
                }
            }
        }

        //删除委托单
        public static bool DelPro(string proCode)
        {
            try
            {
                List<KeyValuePair<string, SQLiteParameter[]>> lst_sql = new List<KeyValuePair<string, SQLiteParameter[]>>();
                string sql_del_pro = String.Format(@"delete from pro_tbl where pro_code = '{0}'", proCode);
                string sql_del_pro_item = String.Format(@"delete from link_pro_item_tbl where pro_code = '{0}'", proCode);
                lst_sql.Add(
                    new KeyValuePair<string, SQLiteParameter[]>(sql_del_pro, new SQLiteParameter[0]));
                lst_sql.Add(
                     new KeyValuePair<string, SQLiteParameter[]>(sql_del_pro_item, new SQLiteParameter[0]));
                SQLiteHelper.ExecuteNonQueryBatch(lst_sql);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //模糊查找
        public static DataTable GetTableLike(string tableName, string key, string val , string cols = "*")
        {
            try
            {
                string sql = String.Format(@"select {3} from {0} where {1} like '%{2}%'", tableName, key, val , cols);
                DataTable dt = SQLiteHelper.ExecuteQuery(sql);
                return dt;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        //查找已完成委托单
        public static DataTable GetCompPro(DateTime dt_begin, DateTime dt_end)
        {
            try
            {
                string sql = string.Format("select * from pro_tbl where comp_time between '{0}' and '{1}' and test_status='1'", MyMethod.date2str(dt_begin), MyMethod.date2str(dt_end));
                DataTable dt = SQLiteHelper.ExecuteQuery(sql);
                return dt;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        /*
       //获取委托单试验项目信息
       public static DataTable GetCheckItems(string proCode)
       {
           try
           {
               string sql = String.Format(@"select a.* , b.link_id from test_item_model a left join link_pro_item_tbl as b
                              on a.item_id = b.item_id and b.pro_code='{0}'",proCode);
               DataTable dt = SQLiteHelper.ExecuteQuery(sql);
               dt.Columns.Add("checked", System.Type.GetType("System.Boolean"));
               foreach (DataRow dr in dt.Rows)
               {
                   if (dr["link_id"].ToString() != "")
                       dr["checked"] = true;
                   else
                       dr["checked"] = false;
               }
               return dt;
           }
           catch (System.Exception ex)
           {
               return null;
           }

       }

       
       public static void UpdateTestItem(string proCode, DataTable dt)
       {
           bool hasProCode = proCode!=null && proCode!="";
           List<KeyValuePair<string, SQLiteParameter[]>> lst_sql = new List<KeyValuePair<string, SQLiteParameter[]>>();
           foreach (DataRow dr in dt.Rows)
           {
               if (dr.RowState == DataRowState.Added)
               {
                   string sql_insert_itemModel = String.Format(@"insert into test_item_model(
                                                               item_name,
                                                               ts_require,
                                                               ts_std,
                                                               unit_price,
                                                               record_model,
                                                               report_model
                                                              )values(
                                                               '{0}','{1}','{2}','{3}','{4}','{5}'
                                                                  )",
                                                                    dr["item_name"], 
                                                                    dr["ts_require"],
                                                                    dr["ts_std"], 
                                                                    dr["unit_price"],
                                                                    dr["record_model"],
                                                                    dr["report_model"]);
                   KeyValuePair<string, SQLiteParameter[]> pair = new KeyValuePair<string, SQLiteParameter[]>(sql_insert_itemModel, new SQLiteParameter[0]);
                   if (hasProCode && bool.Parse(dr["checked"].ToString())) { 
                    
                   }
                   lst_sql.Add(pair);
               }
               else if (dr.RowState == DataRowState.Modified)
               { 
                
               }
               else if (dr.RowState == DataRowState.Deleted)
               { 
                
               }
           }
           SQLiteHelper.ExecuteNonQueryBatch(lst_sql);
       }
       */
        //获取委托单试验结果信息
        public static DataTable GetTestItemResult(string proCode)
        {
            try
            {
                string sql = String.Format(@"select a.link_id , a.pro_type_item_id, a.real_unit_price , a.item_result , a.item_eval , a.reserve1 , b.test_item_type 
                               , c.item_name , c.ts_require , c.ts_std , c.record_model , c.report_model 
                               from link_pro_item_tbl a 
                               join pro_type_item_tbl b on a.pro_type_item_id = b.pro_type_item_id 
                               join test_item_model c on c.item_id=b.item_id
                               where a.pro_code='{0}'", proCode);
                DataTable dt = SQLiteHelper.ExecuteQuery(sql);
                return dt;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        //获取导出委托单试验结果信息
        public static DataTable GetTestItemResult(string proCode, bool isEmpty)
        {
            try
            {
                string where = "b.test_item_type != ''";
                if (isEmpty)
                {
                    where = "b.test_item_type = ''";
                }
                string sql = String.Format(@"select a.link_id , a.pro_type_item_id, a.item_result , a.item_eval , b.test_item_type 
                               , c.item_name , c.ts_require , c.ts_std , c.record_model , c.report_model ,e.item_name as item_type_name
                               from link_pro_item_tbl a 
                               left join pro_type_item_tbl b on a.pro_type_item_id = b.pro_type_item_id 
                               left join test_item_model c on c.item_id=b.item_id
                               left join pro_type_item_tbl d on b.test_item_type = d.pro_type_item_id 
                               left join test_item_model e on e.item_id=d.item_id
                               where a.pro_code='{0}' and {1}", proCode, where);
                DataTable dt = SQLiteHelper.ExecuteQuery(sql);
                /*
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["test_item_type"].ToString() != "")
                        dr["test_item_type"] = GlobalCache.CacheData.dic_test_item_type[dr["test_item_type"].ToString()];
                }
                */
                return dt;
            }
            catch (System.Exception ex)
            {
                return null;
            }

        }

        //获取试验项目及价格明细
        public static DataTable GetTestItemPrice(string proCode)
        {
            try
            {
                string where = "b.test_item_type = ''";
                string sql = String.Format(@"select a.real_unit_price 
                               , c.item_name 
                               from link_pro_item_tbl a 
                               left join pro_type_item_tbl b on a.pro_type_item_id = b.pro_type_item_id 
                               left join test_item_model c on c.item_id=b.item_id
                               where a.pro_code='{0}' and {1}", proCode, where);
                DataTable dt = SQLiteHelper.ExecuteQuery(sql);
                /*
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["test_item_type"].ToString() != "")
                        dr["test_item_type"] = GlobalCache.CacheData.dic_test_item_type[dr["test_item_type"].ToString()];
                }
                */
                return dt;
            }
            catch (System.Exception ex)
            {
                return null;
            }

        }

        //获取所有试验项目分组关系
        public static DataTable GetItemGroup()
        {
            try
            {
                string sql = String.Format(@"select a.* , b.item_name , c.item_name as test_item_type_name from pro_type_item_tbl a 
                                             join test_item_model b on a.item_id = b.item_id
                                             left join test_item_model c on a.test_item_type = c.item_id ");
                DataTable dt = SQLiteHelper.ExecuteQuery(sql);
                return dt;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        //数据库与table绑定更新
        public static Boolean UpdateTable(string tableName, DataTable dt, string idName)
        {
            try
            {
                List<KeyValuePair<string, SQLiteParameter[]>> lst_sql = new List<KeyValuePair<string, SQLiteParameter[]>>();
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr.RowState == DataRowState.Added)
                    {
                        StringBuilder sql_columns = new StringBuilder();
                        StringBuilder sql_vals = new StringBuilder();
                        Boolean flg_start = true;
                        foreach (DataColumn col in dt.Columns)
                        {
                            if (flg_start)
                            {
                                flg_start = false;
                            }
                            else
                            {
                                sql_columns.Append(",");
                                sql_vals.Append(",");
                            }
                            sql_columns.Append(col.ColumnName);
                            sql_vals.Append("'");

                            string val = dr[col.ColumnName].ToString();
                            if (col.ColumnName.Contains("date"))
                            {
                                val = MyMethod.date2str(MyMethod.str2Date(val));
                            }
                            sql_vals.Append(val);
                            sql_vals.Append("'");
                        }
                        string sql_insert = string.Format("insert into {0}({1}) values({2}) ", tableName, sql_columns, sql_vals);
                        KeyValuePair<string, SQLiteParameter[]> pair = new KeyValuePair<string, SQLiteParameter[]>(sql_insert, new SQLiteParameter[0]);
                        lst_sql.Add(pair);
                    }
                    else if (dr.RowState == DataRowState.Deleted)
                    {
                        string sql_del = string.Format("delete from {0} where {1} = '{2}'", tableName, idName, dr[idName, DataRowVersion.Original]);
                        KeyValuePair<string, SQLiteParameter[]> pair = new KeyValuePair<string, SQLiteParameter[]>(sql_del, new SQLiteParameter[0]);
                        lst_sql.Add(pair);
                    }
                    else
                    {
                        StringBuilder sb_sql = new StringBuilder();
                        Boolean flg_start = true;
                        foreach (DataColumn col in dt.Columns)
                        {

                            if (flg_start)
                            {
                                flg_start = false;
                            }
                            else
                            {
                                sb_sql.Append(",");
                            }
                            string val = dr[col.ColumnName].ToString();
                            if (col.ColumnName.Contains("date"))
                            {
                                val = MyMethod.date2str(MyMethod.str2Date(val));
                            }
                            sb_sql.Append(string.Format("{0} = '{1}'", col.ColumnName, val));
                        }
                        string sql_update = string.Format("update {0} set {1} where {2} = '{3}'", tableName, sb_sql, idName, dr[idName]);
                        KeyValuePair<string, SQLiteParameter[]> pair = new KeyValuePair<string, SQLiteParameter[]>(sql_update, new SQLiteParameter[0]);
                        lst_sql.Add(pair);
                    }
                }
                if (tableName == "instrument_tbl") //有可能对仪器有效时间进行了修改，则更新导出状态为0，下次仍能提前一个月提示
                {
                    string sql_reset = String.Format("update instrument_tbl set reserve1 = 0 where exp_date > '{0}'", MyMethod.date2str(DateTime.Now.AddMonths(2)));
                    KeyValuePair<string, SQLiteParameter[]> pair = new KeyValuePair<string, SQLiteParameter[]>(sql_reset, new SQLiteParameter[0]);
                    lst_sql.Add(pair);
                }
                SQLiteHelper.ExecuteNonQueryBatch(lst_sql);
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        //将dt更新到数据库中
        public static Boolean OnlyUpdateTable(string tableName, DataTable dt, string idName)
        {
            try
            {
                List<KeyValuePair<string, SQLiteParameter[]>> lst_sql = new List<KeyValuePair<string, SQLiteParameter[]>>();
                foreach (DataRow dr in dt.Rows)
                {

                    StringBuilder sb_sql = new StringBuilder();
                    Boolean flg_start = true;
                    foreach (DataColumn col in dt.Columns)
                    {

                        if (flg_start)
                        {
                            flg_start = false;
                        }
                        else
                        {
                            sb_sql.Append(",");
                        }
                        string val = dr[col.ColumnName].ToString();
                        if (col.ColumnName.Contains("date"))
                        {
                            val = MyMethod.date2str(MyMethod.str2Date(val));
                        }
                        sb_sql.Append(string.Format("{0} = '{1}'", col.ColumnName, val));
                    }
                    string sql_update = string.Format("update {0} set {1} where {2} = '{3}'", tableName, sb_sql, idName, dr[idName]);
                    KeyValuePair<string, SQLiteParameter[]> pair = new KeyValuePair<string, SQLiteParameter[]>(sql_update, new SQLiteParameter[0]);
                    lst_sql.Add(pair);
                }
                SQLiteHelper.ExecuteNonQueryBatch(lst_sql);
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        //根据实验仪器id集合查找一起具体信息
        public static DataTable GetInstrument(string instrument_ids)
        {
            try
            {
                string sql = String.Format(@"select * from instrument_tbl where instrument_id in ({0})", instrument_ids);
                DataTable dt = SQLiteHelper.ExecuteQuery(sql);
                return dt;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        public static DataTable GetExpiredInstr(string instr_ids)
        {
            try
            {
                string sql = "";
                if (instr_ids == "")
                {
                    sql = String.Format(@"select * from instrument_tbl where exp_date < '{0}' or (exp_date < '{1}' and reserve1 != 1 and {2} = 25)"
                        , MyMethod.date2str(DateTime.Now), MyMethod.date2str(DateTime.Now.AddMonths(1)), DateTime.Now.Day);
                }
                else
                {
                    sql = String.Format(@"select * from instrument_tbl where instrument_id in ({0}) and exp_date < '{1}'"
          , instr_ids, MyMethod.date2str(DateTime.Now));
                }
                DataTable dt = SQLiteHelper.ExecuteQuery(sql);
                return dt;
            }
            catch (System.Exception ex)
            {
                return null;
            }

        }

        //查找委托单所选试验
        public static DataSet GetProTestItem(string proCode, string devType)
        {
            /*
            string sql = string.Format(@"select a.pro_type_item_id , a.test_item_type ,
                                         b.item_name , b.unit_price , b.def_item_result, b.def_item_eval ,
                                         c.link_id , c.real_unit_price , c.item_result , c.item_eval , c.reserve1 , c.reserve2 , c.reserve3 ,
                                         d.item_name as test_item_type_name 
                                         from pro_type_item_tbl a 
                                         left join test_item_model b on b.item_id = a.item_id
                                         left join test_item_model d on d.item_id = a.test_item_type
                                         left join link_pro_item_tbl c on c.pro_type_item_id=a.pro_type_item_id and c.pro_code='{0}' 
                                         where a.pro_type_id='{1}'", proCode, devType);
             * */
            string sql_items = string.Format(@"select a.test_item_type , a.pro_type_item_id ,
                                         b.item_name , b.ts_std , b.unit_price as real_unit_price , b.def_item_result as item_result , b.def_item_eval as item_eval                     
                                         from pro_type_item_tbl a 
                                         left join test_item_model b on b.item_id = a.item_id
                                         where a.pro_type_id='{0}'", devType);
            DataTable dt_items = SQLiteHelper.ExecuteQuery(sql_items);
            string sql_choose_items = string.Format(@"select  a.test_item_type ,c.pro_type_item_id ,null as item_name ,null as ts_std , c.real_unit_price , c.item_result , c.item_eval , c.reserve1 , c.reserve2 , c.reserve3
                                                       from link_pro_item_tbl c join  pro_type_item_tbl a on c.pro_type_item_id=a.pro_type_item_id
                                                       where c.pro_code='{0}' and a.pro_type_id='{1}'", proCode, devType);
            DataTable dt_choose_items = SQLiteHelper.ExecuteQuery(sql_choose_items);
            /*应保证两个datatable表结构一致以便于赋值*/
            dt_items.Columns.Add("reserve1");
            dt_items.Columns.Add("reserve2");
            dt_items.Columns.Add("reserve3");
            dt_items.Columns.Add("id");
            dt_items.Columns.Add("parentid");
            dt_items.Columns.Add("tag", System.Type.GetType("System.Boolean"));

            dt_choose_items.Columns.Add("id");
            dt_choose_items.Columns.Add("parentid");
            dt_choose_items.Columns.Add("tag", System.Type.GetType("System.Boolean"));

            foreach (DataRow dr in dt_items.Rows)
            {
                string str_pid = dr["test_item_type"].ToString();
                if (str_pid == "")
                {
                    dr["parentid"] = "-1";
                    dr["id"] = "1_" + dr["pro_type_item_id"];
                }
                else
                {
                    dr["parentid"] = "1_" + str_pid;
                    dr["id"] = string.Format("1_{0}_{1}", str_pid, dr["pro_type_item_id"]);
                }

                string str_pro_type_item_id = dr["pro_type_item_id"].ToString();
                bool flg_choose = false;
                foreach (DataRow dr_choose in dt_choose_items.Rows)
                {
                    if (dr_choose["pro_type_item_id"].ToString() == str_pro_type_item_id)
                    {
                        flg_choose = true;
                        dr_choose["item_name"] = dr["item_name"];
                        dr_choose["ts_std"] = dr["ts_std"];
                        dr_choose["parentid"] = dr["parentid"];
                        dr_choose["id"] = dr["id"];
                        break;
                    }
                }
                dr["tag"] = flg_choose;
            }
            {
                DataRow dr = dt_items.NewRow();
                dr["id"] = "-1";
                dr["parentid"] = "-2";
                dr["item_name"] = "试验项目";
                dr["tag"] = false;
                dt_items.Rows.Add(dr);
            }
            DataSet ds = new DataSet();
            ds.Tables.Add(dt_items);
            ds.Tables.Add(dt_choose_items);
            return ds;
           
        }

        //仪器id-名称字典
        public static Dictionary<int, string> GetInstrumentsDic()
        {
            string sql = "select instrument_id,instrument_name,instrument_type,device_code from instrument_tbl";
            DataTable dt = SQLiteHelper.ExecuteQuery(sql);
            Dictionary<int, string> dic = new Dictionary<int, string>();
            foreach (DataRow dr in dt.Rows)
            {
                int key = MyMethod.obj2int(dr["instrument_id"]);
                string val = string.Format("{0}—{1}—{2}", dr["instrument_name"], dr["instrument_type"], dr["device_code"]);
                dic.Add(key, val);
            }
            return dic;
        }

        //获取委托单-试验项--仪器
        public static DataTable GetItemInstruments(string pro_code)
        {
            /*
            string sql = string.Format(@"select a.item_id , a.item_name,a.instruments from test_item_model a 
                                 inner join pro_type_item_tbl b on b.item_id = a.item_id
                                 inner join link_pro_item_tbl c on c.pro_type_item_id = b.pro_type_item_id and c.pro_code='{0}'
                                 group by a.item_id , a.item_name,a.instruments ", pro_code);
            DataTable dt = SQLiteHelper.ExecuteQuery(sql);
            dt.Columns.Add("id");
            dt.Columns.Add("parentid");
            dt.Columns.Add("instr_id");
            dt.Columns.Add("tag", System.Type.GetType("System.Boolean"));
            DataTable dt_tmp = dt.Clone();
            foreach (DataRow dr in dt.Rows)
            {
                string str_id = "1_" + dr["item_id"].ToString();
                dr["id"] = str_id;
                dr["parentid"] = "-1";
                dr["tag"] = false;
                string l_instruments = dr["instruments"].ToString();
                if (l_instruments != "")
                {
                    HashSet<int> lst_instr = new HashSet<int>();
                    Boolean flg_check = true;
                    if (l_instruments.IndexOf("/") == -1) // 不存在多选一
                    {
                        string[] strs = l_instruments.Split(',');
                        foreach (string str in strs)
                        {
                            lst_instr.Add(MyMethod.obj2int(str));
                        }
                    }
                    else
                    {
                        flg_check = false;
                        string[] strs = l_instruments.Split(new char[2] { '/', ',' });
                        foreach (string str in strs)
                        {
                            lst_instr.Add(MyMethod.obj2int(str));
                        }
                    }
                    foreach (int key in lst_instr)
                    {
                        DataRow dr_child = dt_tmp.NewRow();
                        dr_child["instr_id"] = key;
                        dr_child["id"] = str_id + "_" + key;
                        dr_child["parentid"] = str_id;
                        dr_child["item_name"] = GlobalCache.CacheData.dic_instruments[key];
                        dr_child["tag"] = flg_check;
                        if (!flg_check)
                        {
                            if (instruments != null)
                            {
                                string[] may_instrument = instruments.Split(',');
                                if (may_instrument.Contains(key.ToString()))
                                    dr_child["tag"] = true;

                            }
                        }
                        dt_tmp.Rows.Add(dr_child);
                    }
                }
            }

            object[] obj = new object[dt.Columns.Count];
            for (int i = 0; i < dt_tmp.Rows.Count; i++)
            {
                dt_tmp.Rows[i].ItemArray.CopyTo(obj, 0);
                dt.Rows.Add(obj);
             
            }
            return dt;
             * */

            string sql = string.Format(@"select a.link_id , a.reserve1 , c.item_name , c.instruments from link_pro_item_tbl a
                                 inner join pro_type_item_tbl b on b.pro_type_item_id = a.pro_type_item_id
                                 inner join test_item_model c on c.item_id = b.item_id
                                 where a.pro_code='{0}'", pro_code);
            DataTable dt = SQLiteHelper.ExecuteQuery(sql);
            dt.Columns.Add("id");
            dt.Columns.Add("parentid");
            dt.Columns.Add("instr_id");
            dt.Columns.Add("tag", System.Type.GetType("System.Boolean"));
            DataTable dt_tmp = dt.Clone();
            foreach (DataRow dr in dt.Rows)
            {
                string str_id = "1_" + dr["link_id"].ToString();
                dr["id"] = str_id;
                dr["parentid"] = "-1";
                dr["tag"] = false;
                string l_instruments = dr["instruments"].ToString();
                string real_instrs = dr["reserve1"].ToString();
                if (l_instruments != "")
                {
                    HashSet<int> lst_instr = new HashSet<int>();
                    Boolean flg_check = true;
                    if (l_instruments.IndexOf("/") == -1) // 不存在多选一
                    {
                        string[] strs = l_instruments.Split(',');
                        foreach (string str in strs)
                        {
                            lst_instr.Add(MyMethod.obj2int(str));
                        }
                    }
                    else
                    {
                        flg_check = false;
                        string[] strs = l_instruments.Split(new char[2] { '/', ',' });
                        foreach (string str in strs)
                        {
                            lst_instr.Add(MyMethod.obj2int(str));
                        }
                    }
                    foreach (int key in lst_instr)
                    {
                        DataRow dr_child = dt_tmp.NewRow();
                        dr_child["instr_id"] = key;
                        dr_child["id"] = str_id + "_" + key;
                        dr_child["parentid"] = str_id;
                        dr_child["item_name"] = GlobalCache.CacheData.dic_instruments[key];
                        dr_child["tag"] = flg_check;
                        if (!flg_check)
                        {
                            if (real_instrs != "")
                            {
                                string[] may_instrument = real_instrs.Split(',');
                                if (may_instrument.Contains(key.ToString()))
                                    dr_child["tag"] = true;

                            }
                        }
                        dt_tmp.Rows.Add(dr_child);
                    }
                }
            }

            object[] obj = new object[dt.Columns.Count];
            for (int i = 0; i < dt_tmp.Rows.Count; i++)
            {
                dt_tmp.Rows[i].ItemArray.CopyTo(obj, 0);
                dt.Rows.Add(obj);

            }
            return dt;
        }

        //更新委托单-试验项目结果
        public static Boolean UpdateProTestItem(DataTable dt)
        {
            List<KeyValuePair<string, SQLiteParameter[]>> lst_sql = new List<KeyValuePair<string, SQLiteParameter[]>>();
            foreach (DataRow dr in dt.Rows)
            {
                if (dr.RowState == DataRowState.Modified)
                {
                    string sql_insert = String.Format(@"update link_pro_item_tbl set item_result='{1}' , item_eval='{2}' where link_id = '{0}'"
                                        , dr["link_id"].ToString(), dr["item_result"].ToString(), dr["item_eval"].ToString());
                    KeyValuePair<string, SQLiteParameter[]> pair = new KeyValuePair<string, SQLiteParameter[]>(sql_insert, new SQLiteParameter[0]);
                    lst_sql.Add(pair);
                }
            }
            try
            {
                SQLiteHelper.ExecuteNonQueryBatch(lst_sql);
                return true;
            }
            catch
            {
                return false;
            }

        }

        //重新选定委托单-试验项目
        public static Boolean ReUpdateProTestItem(string proCode, DataTable dt)
        {
            List<KeyValuePair<string, SQLiteParameter[]>> lst_sql = new List<KeyValuePair<string, SQLiteParameter[]>>();
            string sql_del = string.Format(@"delete from link_pro_item_tbl where pro_code = '{0}'", proCode);
            KeyValuePair<string, SQLiteParameter[]> del_pair = new KeyValuePair<string, SQLiteParameter[]>(sql_del, new SQLiteParameter[0]);
            lst_sql.Add(del_pair);

            foreach (DataRow dr in dt.Rows)
            {

                string sql_insert = String.Format(@"insert into link_pro_item_tbl(
                                                                pro_code,
                                                                pro_type_item_id,
                                                                real_unit_price,
                                                                item_result,
                                                                item_eval,
                                                                reserve1 , 
                                                                reserve2 ,
                                                                reserve3
                                                               )values(
                                                                '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}'
                                                                   )",
                                                                 proCode,
                                                                 dr["pro_type_item_id"],
                                                                 dr["real_unit_price"],
                                                                 dr["item_result"],
                                                                 dr["item_eval"],
                                                                 dr["reserve1"],
                                                                 dr["reserve2"],
                                                                 dr["reserve3"]);
                KeyValuePair<string, SQLiteParameter[]> insert_pair = new KeyValuePair<string, SQLiteParameter[]>(sql_insert, new SQLiteParameter[0]);

                lst_sql.Add(insert_pair);
            }
            try
            {
                SQLiteHelper.ExecuteNonQueryBatch(lst_sql);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public static Dictionary<string, string> GetLatestProCode()
        {
            string sql = "select report_type , max(pro_code) as pro_code from pro_tbl group by report_type";
            DataTable dt = SQLiteHelper.ExecuteQuery(sql);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (DataRow dr in dt.Rows)
            {
                string key = dr["report_type"].ToString();
                string val = dr["pro_code"].ToString();
                dic.Add(key, val);
            }
            return dic;
        }
        //更新委托单
        public static bool UpdatePro(EntProject ent, Boolean flg_new)
        {
            try
            {
                StringBuilder sb_sql = new StringBuilder();
                string[] cols = {"pro_code", "client" , "client_addr" , 
                                   "factory" , "fac_addr" ,
                                   "linkman" , "tel" ,
                                   "post_code" , "email" ,
                                   "pro_name" , "pro_type" ,
                                   "pro_sn" , "pro_num" ,
                                   "test_type" , "report_type" ,
                                   "report_format" , "report_num" ,
                                   "seal_type" , "plan_comp_time" ,
                                   "test_money" , "instruments" ,
                                   "device_type" , "test_date_start" ,
                                   "test_date_end" , "dev_info" , 
                                   "test_result"  , "contract_code" ,
                                   "in_money"  , "test_status" ,
                                   "report_status"  , "comp_time" ,
                                   "create_time" , "org_name" ,
                                   "pro_status" , "get_date" ,
                                   "come_type" , "comp_do" ,
                                   "attach_file"
                                   };
                if (flg_new)
                {
                    StringBuilder sb_vals = new StringBuilder();
                    sb_sql.Append("insert into pro_tbl (");
                    Boolean flg_start = true;
                    foreach (string col in cols)
                    {
                        if (!flg_start)
                        {
                            sb_sql.Append(",");
                            sb_vals.Append(",");
                        }
                        else
                        {
                            flg_start = false;
                        }
                        sb_sql.Append(col);
                        Object obj = ent.GetValue(col);
                        if (obj != null)
                        {
                            if (obj.GetType().Name == "DateTime")
                            {
                                sb_vals.Append(String.Format("'{0}'", MyMethod.obj2date(ent.GetValue(col))));
                            }
                            else
                            {
                                sb_vals.Append(String.Format("'{0}'", ent.GetValue(col)));
                            }
                        }
                        else
                        {
                            sb_vals.Append("''");
                        }
                    }

                    sb_sql.Append(String.Format(") values ({0})", sb_vals));
                }
                else
                {
                    sb_sql.Append("update pro_tbl set ");
                    Boolean flg_start = true;
                    foreach (string col in cols)
                    {
                        if (!flg_start)
                            sb_sql.Append(",");
                        else
                            flg_start = false;

                        Object obj = ent.GetValue(col);
                        if (obj.GetType().Name == "DateTime")
                        {
                            sb_sql.Append(string.Format("{0} = '{1}'", col, MyMethod.date2str((DateTime)ent.GetValue(col))));
                        }
                        else
                        {
                            sb_sql.Append(string.Format("{0} = '{1}'", col, ent.GetValue(col)));
                        }
                    }
                    sb_sql.Append(string.Format(" where pro_code = '{0}'", ent.pro_code));
                }

                SQLiteHelper.ExecuteNonQuery(sb_sql.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Boolean UpdateItemGroup(DataTable dt)
        {
            List<KeyValuePair<string, SQLiteParameter[]>> lst_sql = new List<KeyValuePair<string, SQLiteParameter[]>>();
            string sql_del = "delete from pro_type_item_tbl";
            KeyValuePair<string, SQLiteParameter[]> del_pair = new KeyValuePair<string, SQLiteParameter[]>(sql_del, new SQLiteParameter[0]);
            lst_sql.Add(del_pair);
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["item_id"].ToString() != "") //为项目
                {
                    string pro_type_item_id = dr["pro_type_item_id"].ToString();
                    string sql_insert = "";
                    if (pro_type_item_id != "")
                    {
                        sql_insert = string.Format(@"insert into pro_type_item_tbl(pro_type_item_id , pro_type_id 
                         , item_id ,test_item_type)values('{0}','{1}','{2}','{3}')", dr["pro_type_item_id"],
                                           dr["pro_type_id"],
                                           dr["item_id"],
                                           dr["test_item_type"]);
                    }
                    else
                    {
                        sql_insert = string.Format(@"insert into pro_type_item_tbl(pro_type_id 
                         , item_id ,test_item_type)values('{0}','{1}','{2}')",
                                          dr["pro_type_id"],
                                          dr["item_id"],
                                          dr["test_item_type"]);
                    }

                    KeyValuePair<string, SQLiteParameter[]> pair = new KeyValuePair<string, SQLiteParameter[]>(sql_insert, new SQLiteParameter[0]);
                    lst_sql.Add(pair);
                }
            }
            try
            {
                SQLiteHelper.ExecuteNonQueryBatch(lst_sql);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Boolean UpdateTableColumn(string tableName, DataTable dt, string idName)
        {
            try
            {
                List<KeyValuePair<string, SQLiteParameter[]>> lst_sql = new List<KeyValuePair<string, SQLiteParameter[]>>();
                foreach (DataRow dr in dt.Rows)
                {
                    StringBuilder sb_sql = new StringBuilder();
                    Boolean flg_start = true;
                    foreach (DataColumn col in dt.Columns)
                    {

                        if (flg_start)
                        {
                            flg_start = false;
                        }
                        else
                        {
                            sb_sql.Append(",");
                        }
                        sb_sql.Append(string.Format("{0} = '{1}'", col.ColumnName, dr[col.ColumnName]));
                    }

                    string sql_update = string.Format("update {0} set {1} where {2} = '{3}'", tableName, sb_sql, idName, dr[idName]);
                    KeyValuePair<string, SQLiteParameter[]> pair = new KeyValuePair<string, SQLiteParameter[]>(sql_update, new SQLiteParameter[0]);
                    lst_sql.Add(pair);
                }
                SQLiteHelper.ExecuteNonQueryBatch(lst_sql);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public static void UpdateInstrState(DataTable dt)
        {
            Boolean flg_start = true;
            StringBuilder sb_instr_ids = new StringBuilder();
            foreach (DataRow dr in dt.Rows)
            {
                if (flg_start)
                {
                    flg_start = false;
                }
                else
                {
                    sb_instr_ids.Append(",");
                }
                sb_instr_ids.Append(dr["instrument_id"].ToString());
            }

            string sql = string.Format("update instrument_tbl set reserve1 = 1 where instrument_id in ({0})", sb_instr_ids);
            SQLiteHelper.ExecuteNonQuery(sql);
        }

        internal static Dictionary<int, Dictionary<string, string>> GeGroupFiled()
        {
            Dictionary<int, Dictionary<string, string>> dic = new Dictionary<int, Dictionary<string, string>>();
            string sql = "select * from group_field_tbl";
            DataTable dt = SQLiteHelper.ExecuteQuery(sql);
            foreach (DataRow dr in dt.Rows)
            { 
                int group_id = MyMethod.obj2int(dr["group_id"]);
                string field_name = dr["field_name"].ToString();
                string field_desc = dr["field_desc"].ToString();
                if (dic.ContainsKey(group_id))
                {
                    dic[group_id][field_name] = field_desc;
                }
                else
                {
                    Dictionary<string, string> dic_field = new Dictionary<string, string>();
                    dic_field[field_name] = field_desc;
                    dic[group_id] = dic_field;
                }
            
            }
            return dic;
        }
    }
}
