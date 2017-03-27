using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NPOI.SS.UserModel;
using System.IO;
using System.Data;
using NPOI.HSSF.UserModel;

namespace JFT
{
    class ExcelNPIOHelper:IDisposable
    {
        private string fileName = null; //文件名
        private IWorkbook workbook = null;
        private FileStream fs = null;
        private bool disposed;

        public ExcelNPIOHelper(string fileName)
        {
            this.fileName = fileName;
            disposed = false;
        }

        /// <summary>
        /// 将DataTable数据导入到excel中
        /// </summary>
        /// <param name="data">要导入的数据</param>
        /// <param name="isColumnWritten">DataTable的列名是否要导入</param>
        /// <param name="sheetName">要导入的excel的sheet的名称</param>
        /// <returns>导入数据行数(包含列名那一行)</returns>
        public int DataTableToExcel(DataTable data, string sheetName, bool isColumnWritten)
        {
            int i = 0;
            int j = 0;
            int count = 0;
            ISheet sheet = null;

            fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            workbook = new HSSFWorkbook();

            try
            {
                if (workbook != null)
                {
                    sheet = workbook.CreateSheet(sheetName);
                }
                else
                {
                    return -1;
                }

                if (isColumnWritten == true) //写入DataTable的列名
                {
                    IRow row = sheet.CreateRow(0);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Columns[j].ColumnName);
                    }
                    count = 1;
                }
                else
                {
                    count = 0;
                }

                for (i = 0; i < data.Rows.Count; ++i)
                {
                    IRow row = sheet.CreateRow(count);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Rows[i][j].ToString());
                    }
                    ++count;
                }
                workbook.Write(fs); //写入到excel
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return -1;
            }
        }


        /// <summary>
        /// 将excel中的数据导入到DataTable中
        /// </summary>
        /// <param name="sheetName">excel工作薄sheet的名称</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <returns>返回的DataTable</returns>
        public DataTable ExcelToDataTable(string sheetName, bool isFirstRowColumn)
        {
            ISheet sheet = null;
            DataTable data = new DataTable();
            int startRow = 0;
            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                workbook = new HSSFWorkbook(fs);

                if (sheetName != null)
                {
                    sheet = workbook.GetSheet(sheetName);
                    if (sheet == null) //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                    {
                        sheet = workbook.GetSheetAt(0);
                    }
                }
                else
                {
                    sheet = workbook.GetSheetAt(0);
                }
                if (sheet != null)
                {
                    IRow firstRow = sheet.GetRow(0);
                    int cellCount = firstRow.LastCellNum; //一行最后一个cell的编号 即总的列数

                    if (isFirstRowColumn)
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            ICell cell = firstRow.GetCell(i);
                            if (cell != null)
                            {
                                string cellValue = cell.StringCellValue;
                                if (cellValue != null)
                                {
                                    DataColumn column = new DataColumn(cellValue);
                                    data.Columns.Add(column);
                                }
                            }
                        }
                        startRow = sheet.FirstRowNum + 1;
                    }
                    else
                    {
                        startRow = sheet.FirstRowNum;
                    }

                    //最后一列的标号
                    int rowCount = sheet.LastRowNum;
                    for (int i = startRow; i <= rowCount; ++i)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue; //没有数据的行默认是null　　　　　　　

                        DataRow dataRow = data.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            if (row.GetCell(j) != null) //同理，没有数据的单元格都默认是null
                                dataRow[j] = row.GetCell(j).ToString();
                        }
                        data.Rows.Add(dataRow);
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return null;
            }
        }

        public bool ExcelUpdate(Stream stream , DataTable dt , string year )
        {
            workbook = new HSSFWorkbook(stream);
            ISheet sheet_test = workbook.GetSheetAt(0);
            ISheet sheet_check = workbook.GetSheetAt(1);

            sheet_test.GetRow(0).GetCell(0).SetCellValue(String.Format("{0}年度电磁兼容试验报告", year));
            sheet_check.GetRow(0).GetCell(0).SetCellValue(String.Format("{0}年度电磁兼容检测报告", year));

            int test_last_row_num = sheet_test.LastRowNum;
            int check_last_row_num = sheet_check.LastRowNum;
            IRow rowTemplate_test = sheet_test.GetRow(test_last_row_num);
            IRow rowTemplate_check = sheet_check.GetRow(check_last_row_num);
            int test_num = 0;
            int check_num = 0;
            foreach(DataRow dr in dt.Rows)
            {
                IRow row = null;
                if (dr["report_type"].ToString() == "1") // 检测报告
                {
                    check_num++;
                    row = sheet_check.CreateRow(check_last_row_num + check_num);
                    /*
                    rowTemplate_check.CopyRowTo(check_last_row_num + check_num);
                    row = sheet_check.GetRow(check_last_row_num + check_num);
                    row.Height = rowTemplate_check.Height;
                    row.GetCell(0).SetCellValue(check_num);
                    */
                    row.HeightInPoints = 30f;
                    row.CreateCell(0).SetCellValue(check_num);
                }
                else 
                {
                    test_num++;
                    row = sheet_test.CreateRow(test_last_row_num + test_num);
                    row.HeightInPoints = 20f;
                    row.CreateCell(0).SetCellValue(test_num);
                    /*
                    rowTemplate_test.CopyRowTo(test_last_row_num + test_num);
                    row = sheet_test.GetRow(test_last_row_num + test_num);
                    row.Height = rowTemplate_test.Height;
                    row.GetCell(0).SetCellValue(test_num);
                     */
                }

                row.CreateCell(1).SetCellValue(MyMethod.date2str_out(MyMethod.str2Date(dr["create_time"].ToString())));
                row.CreateCell(2).SetCellValue(dr["client"].ToString());
                row.CreateCell(3).SetCellValue(dr["pro_name"].ToString());
                row.CreateCell(4).SetCellValue(dr["pro_type"].ToString());

                string[] test_type_ids = dr["test_type"].ToString().Split(',');
                string test_type_names = "";
                foreach (string test_type_id in test_type_ids)
                {
                    test_type_names += JFT.GlobalCache.CacheData.dic_group_field[(int)GlobalCache.Groups.TestType][test_type_id];
                }

                row.CreateCell(5).SetCellValue(test_type_names);
                row.CreateCell(6).SetCellValue(MyMethod.obj2double(dr["test_money"]) / 10000);
                row.CreateCell(7).SetCellValue(dr["pro_code"].ToString());
                row.CreateCell(8).SetCellValue(dr["contract_code"].ToString());
                row.CreateCell(9).SetCellValue(dr["linkman"].ToString() + dr["tel"].ToString());
            }        

            try
            {
                fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                workbook.Write(fs); //写入到excel
                return true;
            }
            catch
            {
                return false;
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (fs != null)
                        fs.Close();
                }

                fs = null;
                disposed = true;
            }
        }
    }
}
