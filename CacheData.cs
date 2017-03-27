using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JFT.GlobalCache
{
    enum Groups
    {
        TestType = 1,
        ReportType,
        ReportFormat,
        SealType,
        CompDo,
        DevType
    };

    class CacheData
    {
        public static string base_dir = Environment.CurrentDirectory;
        public static string str_date_format = "yyyy-MM-dd";
        public static string str_date_format_out = "yyyy.MM.dd";

        public static Dictionary<string, string> dic_report_type = new Dictionary<string, string> {
         {"1","检测报告"}
        ,{"2","试验报告"} 
        };

        /*
        public static Dictionary<string, string> dic_test_type = new Dictionary<string, string> {
        {"1","型式试验"}
        ,{"2","例行检验"} 
        ,{"3","特殊试验"}
        ,{"4","性能试验(单项)"}
        ,{"5","性能试验(多项)"}
        ,{"6","定型试验"}
        ,{"7","抽样试验"}
        ,{"8","其他"}
        };



        public static Dictionary<string, string> dic_report_format = new Dictionary<string, string> {
        {"1","中文版"}
        ,{"2","英文版"}
        };

        public static Dictionary<string, string> dic_seal_type = new Dictionary<string, string> {
        {"1","CNAS L0699"}
        ,{"2","CMA 中国电力科学研究院"}
        ,{"3","CMA 电力工业电气设备质量检验测试中心"}
        ,{"4","检测报告专用章"}
        ,{"5","试验报告专用章"}
        ,{"6","型式评价专用章"}
        };

        public static Dictionary<string, string> dic_device_type = new Dictionary<string, string> {
        {"1","电子、电气器件和设备"}
        ,{"2","架空输电线路在线监测系统"} 
        ,{"3","变电设备在线监测装置"}
        ,{"4","电能质量治理设备"}
        ,{"5", "架空输电线路故障定位"}
        };
        */

        public static Dictionary<string, string> dic_choose_columns = new Dictionary<string, string> {
        {"pro_code","委托单编号"}
        , {"client","委托单位"}
        , {"pro_name","样品名称"}
        };

        public static Dictionary<string, string> dic_choose_item_columns = new Dictionary<string, string> {
         {"item_name","试验项目名称"}
        , {"ts_require","试验要求"}
        , {"ts_std","检测标准"}
        , {"unit_price","单价"}
        , {"record_model","原始记录模板"}
        , {"report_model","报告模板"}
        };

        public static Dictionary<string, string> dic_test_item_columns = new Dictionary<string, string> {
        {"test_item_type","试验类别"}
        ,{"item_name","试验项目"}
        ,{"ts_require","试验要求"}
        , {"item_result","试验结果"}
        , {"item_eval","评价"}
        };

        public static Dictionary<string, string> dic_test_item_columns_1 = new Dictionary<string, string> {
        {"item_name","试验项目"}
        ,{"ts_require","试验要求"}
        , {"item_result","试验结果"}
        , {"item_eval","评价"}
        };

        public static Dictionary<string, string> dic_sel_instr_columns = new Dictionary<string, string> {
         {"instrument_name","仪器设备名称/型号"}
        , {"device_code","设备编号"}
        , {"measure_range","测量范围"}
        , {"accuracy","不确定度/准确度/最大允许误差"}
        , {"ver_company","检定/校准机构"}
        , {"exp_date","有效日期"}
        };

        public static Dictionary<string, string> dic_detail_instr_columns = new Dictionary<string, string> {
          {"instrument_id","序号"}
       // , {"dept","部门"}
        , {"instrument_name","仪器设备名称"}
        , {"instrument_type","型号规格"}         
        , {"accuracy","不确定度/准确度"}   
        , {"manufacturer","制造厂"}   
        , {"ver_company","检定、校准机构"}
        , {"ver_date","检定日期"}
        , {"exp_date","有效日期"}
        , {"cer_num","证书号"}
        , {"pro_code","产品编号"}
        , {"device_code","设备编号"}
        , {"use_lab","使用地点"}
        , {"ver_person","送检人"}
        , {"ver_state","状态"}
        , {"measure_range","测量范围"}
        };

        public static Dictionary<string, string> dic_check_instr_columns = new Dictionary<string, string> {
          {"instrument_id","序号"}          
        , {"device_code","设备编号"}
        , {"instrument_name","仪器设备名称、型号"}
        , {"pro_code","产品编号"}
        , {"instrument_type","型号规格"}  
        , {"ver_company","检定、校准机构"} 
        , {"measure_range","测量范围"}  
        , {"ver_date","检定日期"}  
        };

        public static Dictionary<string, string> dic_detail_item_columns = new Dictionary<string, string> {
          {"item_id","序号"}
        , {"item_name","试验名称"}
        , {"ts_require","试验要求"}
        , {"ts_std","试验标准"}         
        , {"unit_price","单价"}   
        , {"report_model","报告模板"}   
        , {"record_model","记录模板"}
        , {"def_item_result","默认结果"}   
        , {"def_item_eval","默认评价"}
        };

        public static Dictionary<string, string> dic_test_status = new Dictionary<string, string> { 
         {"0","未完成"}
        ,{"1","已完成"}         
        };
        public static Dictionary<string, string> dic_report_status = new Dictionary<string, string> { 
         {"0","编写中"}
        ,{"1","已完成"}         
        };

        public static Dictionary<int, string> dic_instruments = Db.DbHelper.GetInstrumentsDic();

        public static Dictionary<string, string> dic_latestProCode = Db.DbHelper.GetLatestProCode();

        public static Dictionary<int, Dictionary<string, string>> dic_group_field = Db.DbHelper.GeGroupFiled();

    }
}
