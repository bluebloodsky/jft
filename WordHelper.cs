using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Microsoft.Office.Core;
using MsWord = Microsoft.Office.Interop.Word;
using JFT.Entity;
using JFT.GlobalCache;
using JFT.Db;

namespace JFT
{
    public class WordHelper
    {
        public static Boolean ExportRecordReport(EntProject pro)
        {
            string modelName = CacheData.base_dir + "\\templets\\model\\recordReport.doc";
            return ExportReport(pro, "3", modelName);
        }
        public static Boolean ExportRecordReport1(EntProject pro)
        {        
            string model_file_name = "recordReport.doc";
            try
            {
                MsWord.Application wordApp = new MsWord.Application();
                wordApp.Visible = true;
                MsWord.Document wordDoc = wordApp.Documents.Add(CacheData.base_dir + "\\templets\\model\\" + model_file_name);

                foreach (System.Reflection.PropertyInfo p in pro.GetType().GetProperties())
                {
                    try
                    {
                        string name = p.Name;
                        string val = p.GetValue(pro, null).ToString();
                        wordDoc.Bookmarks[name].Range.Text = val;
                    }
                    catch
                    {
                        continue;
                    }
                }
                bool flg_noType = pro.device_type != "4";
                DataTable dt_test_items = DbHelper.GetTestItemResult(pro.pro_code, flg_noType);

                HashSet<string> strs_ts_std = new HashSet<string>(); // 用来作为标准文档
                HashSet<string> strs_report_files = new HashSet<string>(); // 报告附录文档

                foreach (DataRow dr in dt_test_items.Rows)
                {
                    string str_ts_std = dr["ts_std"].ToString().Trim();
                    if (str_ts_std != "")
                    {
                        foreach (string str_ts_std_item in str_ts_std.Split('$'))
                        {
                            strs_ts_std.Add(str_ts_std_item.Trim());
                        }
                    }                   
                }

                string str_std = "";
                foreach (string str_ts_std in strs_ts_std)
                {
                    str_std += str_ts_std + "\r\n";
                }
             
                wordDoc.Bookmarks["test_std"].Range.Text = str_std;

                DataTable dt_report = DbHelper.GetTestItemResult(pro.pro_code);               

                foreach (DataRow dr in dt_report.Rows)
                {
                    string str_report_file = dr["record_model"].ToString().Trim();
                    if (str_report_file != "")
                        strs_report_files.Add(str_report_file);
                }
                foreach (string fileName in strs_report_files)
                {
                    string path = CacheData.base_dir + "\\templets\\record\\" + fileName;
                    try
                    {
                        MsWord.Document openWord = wordApp.Documents.Open(path);
                        openWord.Select();
                        openWord.Sections[1].Range.Copy();

                        MsWord.Paragraph para = wordDoc.Content.Paragraphs.Add();
                        para.Range.InsertBreak(MsWord.WdBreakType.wdSectionBreakNextPage);

                        para.Range.PasteAndFormat(MsWord.WdRecoveryType.wdFormatOriginalFormatting);

                        openWord.Close(true);
                    }
                    catch
                    {
                        continue;
                    }

                }

                DataTable dt_instrument = DbHelper.GetInstrument(pro.instruments);
                MsWord.Range tbl_instr_range = wordDoc.Bookmarks["tbl_instruments"].Range;
                MsWord.Table table = wordDoc.Tables.Add(tbl_instr_range, dt_instrument.Rows.Count + 3, 7);
                table.Rows.Alignment = MsWord.WdRowAlignment.wdAlignRowCenter;
                table.Range.Cells.VerticalAlignment = MsWord.WdCellVerticalAlignment.wdCellAlignVerticalCenter;//居中
                table.Borders.Enable = 1;//这个值可以设置得很大，例如5、13等等            

                table.Columns[1].Width = 29f;
                table.Columns[2].Width = 100f;
                table.Columns[3].Width = 71f;
                table.Columns[4].Width = 78f;
                table.Columns[5].Width = 50f;
                table.Columns[6].Width = 86f;
                table.Columns[7].Width = 65f;

                for (int j = 1; j < table.Columns.Count + 1; j++)
                {
                    table.Cell(3, j).LeftPadding = 0f;
                    table.Cell(3, j).RightPadding = 0f;
                }

                table.Cell(1, 1).Range.Text = "附录3 实验室主要试验设备";
                table.Cell(1, 1).Range.Font.Size = 14;

                table.Cell(1, 6).Range.Text = "仪器设备名称、型号";
                table.Cell(2, 6).Range.Text = "记 录 编 号：";

                table.Cell(3, 1).Range.Text = "序号";

                for (int j = 0; j < CacheData.dic_sel_instr_columns.Count; j++)
                {
                    table.Cell(3, j + 2).Range.Text = CacheData.dic_sel_instr_columns.Values.ElementAt(j);
                }

                for (int i = 0; i < dt_instrument.Rows.Count; i++)
                {
                    table.Cell(i + 4, 1).Range.Text = (i + 1).ToString();
                    for (int j = 0; j < CacheData.dic_sel_instr_columns.Count; j++)
                    {
                        table.Cell(i + 4, j + 2).Range.Text = dt_instrument.Rows[i][CacheData.dic_sel_instr_columns.Keys.ElementAt(j)].ToString();
                    }
                }

                table.Cell(1, 1).Merge(table.Cell(2, 5));
                table.Cell(1, 2).Merge(table.Cell(1, 3));
                table.Cell(2, 2).Merge(table.Cell(2, 3));

                string export_fileName = CacheData.base_dir + "\\export\\原始记录_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".docx";
                wordDoc.SaveAs2(export_fileName);
                //wordDoc.Close(true);
                // wordApp.Quit();
                return true;
            }
            catch
            {
                return false;
            }

        }        

        public static Boolean ExportReport(EntProject pro , string report_num , string modelName)
        {
            try
            {
                DataTable dt = DbHelper.GetTableLike("bookmark_tbl", "bookmark_type", report_num);
                DataTable dt_item_infos = DbHelper.GetTestItemResult(pro.pro_code);
                DataTable dt_instrument = DbHelper.GetInstrument(pro.instruments);

                MsWord.Application wordApp = new MsWord.Application();
                wordApp.Visible = true;
                MsWord.Document wordDoc = wordApp.Documents.Add(modelName);

                foreach (DataRow dr in dt.Rows)
                {
                    string bookmark_name = dr["bookmark_name"].ToString();
                    int bookmark_prop = MyMethod.obj2int(dr["bookmark_prop"]);
                    string pro_prop = dr["pro_prop"].ToString();
                    if (bookmark_prop == 1) //直接从EntProject中寻找属性
                    {
                        if (pro.GetPropType(pro_prop) == System.Type.GetType("System.DateTime"))
                        {
                            wordDoc.Bookmarks[bookmark_name].Range.Text = MyMethod.date2str_out((DateTime)pro.GetValue(pro_prop));
                        }
                        else
                        {
                            wordDoc.Bookmarks[bookmark_name].Range.Text = pro.GetValue(pro_prop).ToString();                        
                        }
                    }
                    else if (bookmark_prop == 2) //从EntProject中寻找是否选择
                    {
                        string val = pro.GetValue(pro_prop).ToString();
                        char order = bookmark_name[bookmark_name.Length - 1];
                        if (val.IndexOf(order) == -1)//未被选中
                        {
                            wordDoc.Bookmarks[bookmark_name].Range.Text = "□";
                        }
                    }
                    else if (bookmark_prop == 3) //自定义表格
                    {
                        #region 导出委托单
                        if (pro_prop == "test_money_chinese")
                        {
                            wordDoc.Bookmarks[bookmark_name].Range.Text = MyMethod.int2chinese(MyMethod.obj2int(pro.test_money));
                        }
                        else if (pro_prop == "test_std")
                        {
                            HashSet<string> strs_ts_std = new HashSet<string>(); // 用来作为标准文档
                            foreach (DataRow dr_item_info in dt_item_infos.Rows)
                            {
                                string str_ts_std = dr_item_info["ts_std"].ToString().Trim();
                                if (str_ts_std != "")
                                {
                                    foreach (string str_ts_std_item in str_ts_std.Split('$'))
                                    {
                                        strs_ts_std.Add(str_ts_std_item.Trim());
                                    }
                                }

                            }
                            string str_std = "";
                            foreach (string str_ts_std in strs_ts_std)
                            {
                                str_std += str_ts_std + "\r\n";
                            }
                            wordDoc.Bookmarks[bookmark_name].Range.Text = str_std;
                        }
                        else if (pro_prop == "tbl_item_price")
                        {
                            DataTable dt_test_item = new DataTable();
                            dt_test_item.Columns.Add("item_name");
                            dt_test_item.Columns.Add("real_unit_price");

                            foreach (DataRow dr_item_info in dt_item_infos.Rows)
                            {
                                if (dr_item_info["test_item_type"].ToString() == "")
                                {
                                    DataRow dr_test_item = dt_test_item.NewRow();
                                    dr_test_item["item_name"] = dr_item_info["item_name"];
                                    dr_test_item["real_unit_price"] = dr_item_info["real_unit_price"];
                                    dt_test_item.Rows.Add(dr_test_item);
                                }
                            }
                            MsWord.Range tbl_test_range = wordDoc.Bookmarks[bookmark_name].Range;
                            MsWord.Table table = wordDoc.Tables.Add(tbl_test_range, dt_test_item.Rows.Count + 1, 3);

                            table.Rows.Alignment = MsWord.WdRowAlignment.wdAlignRowCenter;
                            table.Range.Cells.VerticalAlignment = MsWord.WdCellVerticalAlignment.wdCellAlignVerticalCenter;//居中
                            table.Borders.Enable = 1;//这个值可以设置得很大，例如5、13等等

                            table.Columns[1].Width = 50f;
                            table.Columns[2].Width = 330f;
                            table.Columns[3].Width = 71f;

                            table.Cell(1, 1).Range.Text = "序号";
                            table.Cell(1, 2).Range.Text = "试验项目";
                            table.Cell(1, 3).Range.Text = "单价";

                            for (int i = 0; i < dt_test_item.Rows.Count; i++)
                            {
                                    table.Cell(i + 2, 1).Range.Text = (i + 1).ToString();
                                    table.Cell(i + 2, 2).Range.Text = dt_test_item.Rows[i]["item_name"].ToString();
                                    table.Cell(i + 2, 3).Range.Text = dt_test_item.Rows[i]["real_unit_price"].ToString();   
                            }
                        }
                        #endregion
                        #region 导出报告
                        else if (pro_prop == "test_type_name")
                        {
                            string[] test_type_ids = pro.test_type.Split(',');
                            string test_type_names = "";
                            foreach (string test_type_id in test_type_ids)
                            {
                                test_type_names += CacheData.dic_group_field[(int)GlobalCache.Groups.TestType][test_type_id];
                            }
                            wordDoc.Bookmarks[bookmark_name].Range.Text = test_type_names;
                        }
                        #region 试验项目表格
                        else if (pro_prop == "tbl_item_result")
                        {
                            string[] item_result_cols = { "pro_type_item_id", "item_name", "test_item_type", "item_result", "item_eval" ,"ts_require" };
                            DataTable dt_item_type = new DataTable();
                            DataTable dt_test_item = new DataTable();
                            foreach (string col_name in item_result_cols)
                            {
                                dt_item_type.Columns.Add(col_name);
                                dt_test_item.Columns.Add(col_name);
                            }
                            foreach (DataRow dr_item in dt_item_infos.Rows)
                            {
                                string test_item_type = dr_item["test_item_type"].ToString().Trim();
                                if (test_item_type == "") //没有上级项目
                                {
                                    DataRow dr_child = dt_item_type.NewRow();
                                    foreach (string col_name in item_result_cols)
                                    {
                                        dr_child[col_name] = dr_item[col_name];
                                    }                                  
                                    dt_item_type.Rows.Add(dr_child);
                                }
                                else
                                {
                                    DataRow dr_child = dt_test_item.NewRow();
                                    foreach (string col_name in item_result_cols)
                                    {
                                        dr_child[col_name] = dr_item[col_name];
                                    }  
                                    dt_test_item.Rows.Add(dr_child);
                                }
                            }
                            Boolean flg_noType = dt_test_item.Rows.Count == 0;
                            
                            Dictionary<string, string> export_item_columns = flg_noType?CacheData.dic_test_item_columns_1:CacheData.dic_test_item_columns;

                            DataTable dt_item_result = flg_noType ? dt_item_type : dt_test_item;

                            MsWord.Range tbl_test_item_range = wordDoc.Bookmarks[bookmark_name].Range;
                            MsWord.Table ms_tbl_test_item = wordDoc.Tables.Add(tbl_test_item_range, dt_item_result.Rows.Count + 1, export_item_columns.Count + 1);
                            ms_tbl_test_item.Rows.Alignment = MsWord.WdRowAlignment.wdAlignRowCenter;
                            ms_tbl_test_item.Range.Cells.VerticalAlignment = MsWord.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                            ms_tbl_test_item.Borders.Enable = 1;
                            ms_tbl_test_item.Rows.Height = 40f;

                            if (flg_noType)
                            {
                                ms_tbl_test_item.Columns[1].Width = 39f;
                                ms_tbl_test_item.Columns[2].Width = 114f;
                                ms_tbl_test_item.Columns[3].Width = 155f;
                                ms_tbl_test_item.Columns[4].Width = 98f;
                                ms_tbl_test_item.Columns[5].Width = 94f;
                            }
                            else
                            {
                                ms_tbl_test_item.Columns[1].Width = 34f;
                                ms_tbl_test_item.Columns[2].Width = 69f;
                                ms_tbl_test_item.Columns[3].Width = 98f;
                                ms_tbl_test_item.Columns[4].Width = 133f;
                                ms_tbl_test_item.Columns[5].Width = 85f;
                                ms_tbl_test_item.Columns[6].Width = 81f;
                            }

                            ms_tbl_test_item.Cell(1, 1).Range.Font.Bold = 1;
                            ms_tbl_test_item.Cell(1, 1).Range.Font.Size = 10.5f;
                            ms_tbl_test_item.Cell(1, 1).Range.Text = "序号";

                            for (int j = 0; j < export_item_columns.Count; j++)
                            {
                                ms_tbl_test_item.Cell(1, j + 2).Range.Font.Bold = 1;
                                ms_tbl_test_item.Cell(1, j + 2).Range.Font.Size = 10.5f;
                                ms_tbl_test_item.Cell(1, j + 2).Range.Text = export_item_columns.Values.ElementAt(j);
                            }


                            if (!flg_noType) //需要merge
                            {
                                int test_type_order = 0; //类别序号
                                int test_item_order = 1; //类别下项目序号
                                string test_item_type = "-----";
                                int i = 0;
                                for (; i < dt_item_result.Rows.Count; i++)
                                {
                                    for (int j = 2; j < export_item_columns.Count; j++)
                                    {
                                        ms_tbl_test_item.Cell(i + 2, j + 2).Range.Text = dt_item_result.Rows[i][export_item_columns.Keys.ElementAt(j)].ToString();

                                    }
                                    string l_test_item_type = dt_item_result.Rows[i]["test_item_type"].ToString();
                                    string l_item_name = dt_item_result.Rows[i]["item_name"].ToString();
                                    if (test_item_type == l_test_item_type) //类别不变
                                    {
                                        test_item_order++;
                                        ms_tbl_test_item.Cell(i + 2, 1).Range.Text = String.Format("{0}.{1}", test_type_order, test_item_order);
                                        ms_tbl_test_item.Cell(i + 2, 3).Range.Text = l_item_name;
                                    }
                                    else
                                    {
                                        if (i > 0) //有数据，可以合并
                                        {
                                            if (test_item_order == 1) //上一个类型只出现一行，横向合并
                                            {
                                                ms_tbl_test_item.Cell(i + 1, 1).Range.Text = test_type_order.ToString();
                                                ms_tbl_test_item.Cell(i + 1, 2).Range.Text = "";
                                                ms_tbl_test_item.Cell(i + 1, 2).Merge(ms_tbl_test_item.Cell(i + 1, 3));
                                            }
                                            else //上一个类型只出现多行，纵向合并
                                            {
                                                ms_tbl_test_item.Cell(i + 1, 2).Merge(ms_tbl_test_item.Cell(i + 2 - test_item_order, 2));
                                            }
                                        }
                                       
                                        test_item_type = l_test_item_type;
                                        test_type_order++;
                                        test_item_order = 1;
                                        ms_tbl_test_item.Cell(i + 2, 1).Range.Text = String.Format("{0}.{1}", test_type_order, test_item_order);
                                        foreach (DataRow dr_item_type in dt_item_type.Rows)
                                        {
                                            if (dr_item_type["pro_type_item_id"].ToString() == l_test_item_type)
                                            {
                                                ms_tbl_test_item.Cell(i + 2, 2).Range.Text = dr_item_type["item_name"].ToString();
                                                break;
                                            }
                                        }                                        
                                        ms_tbl_test_item.Cell(i + 2, 3).Range.Text = l_item_name;
                                    }
                                }
                                if (i > 0)
                                {
                                    if (test_item_order == 1) //上一个类型只出现一行，横向合并
                                    {
                                        ms_tbl_test_item.Cell(i + 1, 1).Range.Text = test_type_order.ToString();
                                        ms_tbl_test_item.Cell(i + 1, 2).Range.Text = "";
                                        ms_tbl_test_item.Cell(i + 1, 2).Merge(ms_tbl_test_item.Cell(i + 1, 3));
                                    }
                                    else //上一个类型只出现多行，纵向合并
                                    {
                                        ms_tbl_test_item.Cell(i + 1, 2).Merge(ms_tbl_test_item.Cell(i + 2 - test_item_order, 2));
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < dt_item_result.Rows.Count; i++)
                                {
                                    ms_tbl_test_item.Cell(i + 2, 1).Range.Text = (i + 1).ToString();
                                    ms_tbl_test_item.Cell(i + 2, 3).Range.ParagraphFormat.Alignment = MsWord.WdParagraphAlignment.wdAlignParagraphLeft;
                                    for (int j = 0; j < export_item_columns.Count; j++)
                                    {
                                        ms_tbl_test_item.Cell(i + 2, j + 2).Range.Text = dt_item_result.Rows[i][export_item_columns.Keys.ElementAt(j)].ToString();

                                    }
                                }
                            }
                        }
                        #endregion
                        #region 仪器列表
                        else if (pro_prop == "tbl_instrument")
                        {
                            MsWord.Range tbl_instr_range = wordDoc.Bookmarks[bookmark_name].Range;
                            MsWord.Table table = wordDoc.Tables.Add(tbl_instr_range, dt_instrument.Rows.Count + 1, CacheData.dic_sel_instr_columns.Count + 1);
                        
                            table.Rows.Alignment = MsWord.WdRowAlignment.wdAlignRowCenter;
                            table.Range.Cells.VerticalAlignment = MsWord.WdCellVerticalAlignment.wdCellAlignVerticalCenter;//居中
                            table.Borders.Enable = 1;//这个值可以设置得很大，例如5、13等等

                            table.Columns[1].Width = 41f;
                            table.Columns[2].Width = 98f;
                            table.Columns[3].Width = 67f;
                            table.Columns[4].Width = 80f;
                            table.Columns[5].Width = 63f;
                            table.Columns[6].Width = 84f;
                            table.Columns[7].Width = 66f;

                            for (int j = 1; j < table.Columns.Count + 1; j++)
                            {
                               // table.Cell(1, j).LeftPadding = 0f;
                               // table.Cell(1, j).RightPadding = 0f;
                                table.Cell(1, j).Range.Font.Bold = 1;
                                table.Cell(1, j).Range.Font.Size = 10.5f;
                            }

                            table.Cell(1, 1).Range.Text = "序号";

                            for (int j = 0; j < CacheData.dic_sel_instr_columns.Count; j++)
                            {
                                table.Cell(1, j + 2).Range.Text = CacheData.dic_sel_instr_columns.Values.ElementAt(j);
                            }
                            for (int i = 0; i < dt_instrument.Rows.Count; i++)
                            {
                                table.Cell(i + 2, 1).Range.Text = (i + 1).ToString();
                                table.Cell(i + 2, 2).Range.Text = String.Format("{0} {1}", dt_instrument.Rows[i]["instrument_name"] , dt_instrument.Rows[i]["instrument_type"]);
                                for (int j = 1; j < CacheData.dic_sel_instr_columns.Count; j++)
                                {
                                    table.Cell(i + 2, j + 2).Range.Text = dt_instrument.Rows[i][CacheData.dic_sel_instr_columns.Keys.ElementAt(j)].ToString();
                                }
                            }
                        }
                        #endregion
                        #region 报告模板
                        else if (pro_prop == "report")
                        {
                            HashSet<string> strs_report_files = new HashSet<string>(); // 报告附录文档
                            MsWord.Range range = wordDoc.Bookmarks[bookmark_name].Range;                           
                            foreach (DataRow dr_info in dt_item_infos.Rows)
                            {
                                string test_item_type = dr_info["test_item_type"].ToString().Trim();
                                if (test_item_type == "") //没有上级项目
                                {
                                    string str_report_file = dr_info["report_model"].ToString().Trim();
                                    if (str_report_file != "")
                                    {
                                        range.Paragraphs.Add();
                                        string path = CacheData.base_dir + "\\templets\\report\\" + str_report_file;
                                        strs_report_files.Add(path);                                       
                                    }
                                }                               
                            }

                            MsWord.Paragraph para = wordDoc.Bookmarks[bookmark_name].Range.Paragraphs.Last;
                            foreach (string path in strs_report_files)
                            {
                                try
                                {
                                    MsWord.Document openWord = wordApp.Documents.Open(path);
                                    openWord.Select();
                                    openWord.Sections[1].Range.Copy();
                                    para.Range.PasteAndFormat(MsWord.WdRecoveryType.wdFormatOriginalFormatting);
                                    openWord.Close(false, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                        }
                        #endregion
                        #region 记录模板仪器列表
                        else if (pro_prop == "tbl_record_instrument")
                        {
                            MsWord.Range tbl_instr_range = wordDoc.Bookmarks[bookmark_name].Range;
                            MsWord.Table table = wordDoc.Tables.Add(tbl_instr_range, dt_instrument.Rows.Count + 3, 7);
                            table.Rows.Alignment = MsWord.WdRowAlignment.wdAlignRowCenter;
                            table.Range.Cells.VerticalAlignment = MsWord.WdCellVerticalAlignment.wdCellAlignVerticalCenter;//居中
                            table.Borders.Enable = 1;//这个值可以设置得很大，例如5、13等等            

                            table.Columns[1].Width = 29f;
                            table.Columns[2].Width = 100f;
                            table.Columns[3].Width = 71f;
                            table.Columns[4].Width = 78f;
                            table.Columns[5].Width = 50f;
                            table.Columns[6].Width = 86f;
                            table.Columns[7].Width = 65f;

                            for (int j = 1; j < table.Columns.Count + 1; j++)
                            {
                                table.Cell(3, j).LeftPadding = 0f;
                                table.Cell(3, j).RightPadding = 0f;
                            }

                            table.Cell(1, 1).Range.Text = "附录3 实验室主要试验设备";
                            table.Cell(1, 1).Range.Font.Size = 14;

                            table.Cell(1, 6).Range.Text = "仪器设备名称、型号";
                            table.Cell(2, 6).Range.Text = "记 录 编 号：";

                            table.Cell(3, 1).Range.Text = "序号";

                            for (int j = 0; j < CacheData.dic_sel_instr_columns.Count; j++)
                            {
                                table.Cell(3, j + 2).Range.Text = CacheData.dic_sel_instr_columns.Values.ElementAt(j);
                            }

                            for (int i = 0; i < dt_instrument.Rows.Count; i++)
                            {
                                table.Cell(i + 4, 1).Range.Text = (i + 1).ToString();
                                for (int j = 0; j < CacheData.dic_sel_instr_columns.Count; j++)
                                {
                                    table.Cell(i + 4, j + 2).Range.Text = dt_instrument.Rows[i][CacheData.dic_sel_instr_columns.Keys.ElementAt(j)].ToString();
                                }
                            }

                            table.Cell(1, 1).Merge(table.Cell(2, 5));
                            table.Cell(1, 2).Merge(table.Cell(1, 3));
                            table.Cell(2, 2).Merge(table.Cell(2, 3));

                        }
                        #endregion
                        #region 记录模板
                        else if (pro_prop == "record")
                        {
                            MsWord.Paragraph para = wordDoc.Bookmarks[bookmark_name].Range.Paragraphs.Add();
                            foreach (DataRow dr_info in dt_item_infos.Rows)
                            {
                                string test_item_type = dr_info["test_item_type"].ToString().Trim();
                                if (test_item_type == "") //没有上级项目
                                {
                                    string str_record_file = dr_info["record_model"].ToString().Trim();
                                    if (str_record_file != "")
                                    {
                                        string path = CacheData.base_dir + "\\templets\\record\\" + str_record_file;
                                        try
                                        {
                                            MsWord.Document openWord = wordApp.Documents.Open(path);
                                            openWord.Select();
                                            openWord.Sections[1].Range.Copy();

                                            para.Range.InsertBreak(MsWord.WdBreakType.wdSectionBreakNextPage);

                                            para.Range.PasteAndFormat(MsWord.WdRecoveryType.wdFormatOriginalFormatting);

                                            openWord.Close(false, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
                                        }
                                        catch
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                        else if (pro_prop == "catalog")
                        {
                            //wordDoc.TablesOfContents.Add(wordDoc.Bookmarks["catalog"].Range);
                        }
                        #endregion
                        #endregion
                    }
                }       
                string export_fileName = String.Format("{0}\\export\\{1}_{2}.docx", CacheData.base_dir, "导出文档", DateTime.Now.ToString("yyyyMMddHHmmss"));
                wordDoc.SaveAs2(export_fileName);
                return true;
            }
            catch
            {                
                return false;
            }
        }
        public static Boolean ExportPro(EntProject pro)
        {
            string modelName = CacheData.base_dir + "\\templets\\model\\project.doc";
            return ExportReport(pro, "1", modelName);          
        }

        public static Boolean ExportTestReport(EntProject pro)
        {
            string file_name = "testReport.docx";
            if (pro.report_type.IndexOf("1") != -1)
            {
                file_name = "checkReport.docx";
            }
            string modelName = CacheData.base_dir + "\\templets\\model\\" + file_name;
            return ExportReport(pro, "2", modelName);
        }

        public static bool ExportMonthReport(DateTime dt_begin, DateTime dt_end)
        {
            try
            {
                MsWord.Application wordApp = new MsWord.Application();
                wordApp.Visible = true;
                MsWord.Document wordDoc = wordApp.Documents.Add(CacheData.base_dir + "\\templets\\model\\monthPlan.doc");

                wordDoc.Bookmarks["month"].Range.Text = dt_end.Month.ToString();

                string fileName = string.Format("{0}\\export\\{1}月计划安排及执行情况统计表_{2}",
                    CacheData.base_dir , dt_end.Month , DateTime.Now.ToString("yyyyMMddHHmmss") + ".docx");

                DataTable dt = DbHelper.GetCompPro(dt_begin , dt_end);

                MsWord.Range tbl_test_range = wordDoc.Bookmarks["tbl_comp"].Range;
                MsWord.Table table = wordDoc.Tables.Add(tbl_test_range, dt.Rows.Count + 1, 7 );

                table.Rows.Alignment = MsWord.WdRowAlignment.wdAlignRowCenter;
                table.Range.Cells.VerticalAlignment = MsWord.WdCellVerticalAlignment.wdCellAlignVerticalCenter;//居中
                table.Borders.Enable = 1;//这个值可以设置得很大，例如5、13等等

                table.Columns[1].Width = 36f;
                table.Columns[2].Width = 64f;
                table.Columns[3].Width = 190f;
                table.Columns[4].Width = 169f;
                table.Columns[5].Width = 101f;
                table.Columns[6].Width = 74f;
                table.Columns[7].Width = 98f;


                table.Cell(1, 1).Range.Text = "序号";
                table.Cell(1, 2).Range.Text = "完成时间";
                table.Cell(1, 3).Range.Text = "厂家名称";
                table.Cell(1, 4).Range.Text = "试品名称";
                table.Cell(1, 5).Range.Text = "合同编号";
                table.Cell(1, 6).Range.Text = "报告完成情况";
                table.Cell(1, 7).Range.Text = "到款情况";

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    table.Cell(i + 2, 1).Range.Text = (i + 1).ToString();
                    table.Cell(i + 2, 2).Range.Text = dt.Rows[i]["comp_time"].ToString();
                    table.Cell(i + 2, 3).Range.Text = dt.Rows[i]["client"].ToString();
                    table.Cell(i + 2, 4).Range.Text = dt.Rows[i]["pro_name"].ToString();
                    table.Cell(i + 2, 5).Range.Text = dt.Rows[i]["contract_code"].ToString();
                    table.Cell(i + 2, 6).Range.Text = CacheData.dic_report_status[dt.Rows[i]["report_status"].ToString()];
                    table.Cell(i + 2, 7).Range.Text = dt.Rows[i]["in_money"].ToString();
                }

                wordDoc.SaveAs2(@fileName);
                //wordDoc.Close(true);
                // wordApp.Quit();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
