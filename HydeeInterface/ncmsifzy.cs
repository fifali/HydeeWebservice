using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using System.EnterpriseServices;

/// <summary>
/// 住院业务相关
/// </summary>
public class ncmsifzy
{
    #region 类变量定义

    /// <summary>
    /// 服务器名
    /// </summary>
    private string DBServer = "";
    /// <summary>
    /// 数据库名
    /// </summary>
    private string DBName = "";
    /// <summary>
    /// 用户ID
    /// </summary>
    private string UserID = "";
    /// <summary>
    /// 用户密码
    /// </summary>
    private string PassWord = "";
    /// <summary>
    /// 数据库联接串
    /// </summary>
    private string DBConnStr = "";
    /// <summary>
    /// 当前用户是否有效
    /// </summary>
    private bool IsValidUser = false;
    /// <summary>
    /// 当前用户所在的区县代码
    /// </summary>
    private string AreaCode = "";
    /// <summary>
    /// 接口拥有的权限

    /// </summary>
    private int InterFacePower = -1;
    /// <summary>
    /// 主类的对象

    /// </summary>
    private ncmsif if0;

    /// <summary>
    /// 住院补助计算类

    /// </summary>
    public class HospCalcFee
    {
        #region 变量定义
        private
        string retMess;//补偿信息
        string hospitalcode,hospitalname;//医院代码
        decimal hospid;//住院号
        Int32 levelcode;//机构级别1,2村卫生室、乡镇医院；3,4县级医疗机构、地市医疗机构；5,9省级及以上医疗机构、其他医疗机构
        Int32 hospleve;//医院收费级别：1三类收费标准，2二类收费标准，3一类收费标准
        Int32 typecode;//医院定点类型
        Int32 orglevel;//医院等级1：一级医院；2：二级医院；3：三级医院 主要用来计算起付线
        string hospresu, icdname;//疾病代码及名称
        DateTime hospdate,outdate;//出入院日期
        string nopaydate;//费用截止日期
        string patientname;//病人姓名
        string partsor;//病人类别
        Int32 ifhurt;//意外伤害标志 0:不是 1:是
        decimal deratesum = 0;//医疗减免额
        Int32 hospfromlevel = 0, hosptolevel = 0;//转入本院的医院级别，本院转出到的医院级别
        string hospfrom, hospto;//转入本院的医院名称，本院转出到的医院名称
        Int32 hospperf = 1;//出入院标志 0:在院 1:出院
        Int32 localid = 0;//是否本县：0本县，1县外 2经办机构

        string familyno = "", familyno2 = "";//家庭号,跨年度的第二年的家庭号
        Int32 memberno = 0, memberno2 = 0;//成员号，跨年度的第二年的成员号
        string human = "", human2 = "";//个人编号，跨年度的第二年的个人编号
        string bookcard = "", bookcard2 = "";//医疗证，跨年度的第二年的医疗证号
        string civiltype = "";//民政补助类别

        string audit_log = ""; //补助额计算的过程日志

        #endregion

        public bool CalcFee(string hospitalcode,decimal hospid, Int32 chkpayid, string opername, Int32 settle, DataSet ds, out string retmess)
        {            
            retmess = audit_log;
            return true;
        }       

        //初始化变量及数据
        private bool InitParm1(Int32 settle, string XMLData,out string mess)
        {
            string sqltext;
            SqlCommand sqlcomm = new SqlCommand();
            SqlTransaction sqltrans = null;

            if (settle == 0)
            #region//试算
            {

            }
            #endregion
            else
            #region//正式结算
            {
                
            }
            #endregion
            mess = "";            
            return true;
        }
        
        
    }    
    #endregion 

    #region 构造函数

    public ncmsifzy()
	{
		//
		// TODO: 在此处添加构造函数逻辑
		//
    }

    public ncmsifzy(string dbServer, string dbName, string userID, string passWord, string dbConnString, bool validUser, string areaCode, int interFacePower)
    {
        if0 = new ncmsif(dbServer, dbName, userID, passWord, dbConnString, validUser, areaCode, interFacePower);
        DBServer = dbServer;
        DBName = dbName;
        UserID = userID;
        PassWord = passWord;
        DBConnStr = dbConnString;
        IsValidUser = validUser;
        AreaCode = areaCode;
        InterFacePower = interFacePower;
    }
    #endregion

    #region 析构函数
     ~ncmsifzy()
    {
        if0.Dispose();
    }
    #endregion

    #region //住院病人操作
    /// <summary>
    /// 使用hospid从农合系统中查询一个住院病人资料

    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="hospid"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string getZYPatientInfo(string areacode, string hospitalcode, string hospid, out string data)
    {
        data = "";
        if (hospid == null || hospid.Length == 0)
        {
            return if0.getXMLErroStrFromString("2000", "住院ID号(zyid)不能为空值");
        }
        if (!if0.isNumber(hospid))
        {
            return if0.getXMLErroStrFromString("2000", "住院ID号(zyid)必须是数字型");
        }
        string mess = "";
        if (if0.getDBConnStr("areacode", areacode, out mess))
        {
            string ls_sql = null;
            DataSet myDs = null;
            ls_sql = "SELECT h.HospID as zyid,h.HospCode as zyh,h.PATIENTNAME as xm,h.sex as xb,";
            ls_sql += "convert(varchar(10),BIRTH,120) as sr,h.BLOODTYPE as xx,h.COUNTRY as gj,h.NATIONALITY as mz,h.NATIPLAC as jg,";
            ls_sql += "h.MARRIAGE as hyzt,h.HOMEADDR as jtzz,h.HOMEPHON as dh,h.HOMEZIP as yb,h.Serial as zycs,";
            ls_sql += "convert(varchar(19),h.HospDate,120) as rysj,convert(varchar(19),OutDate,120) as cysj,h.retr_price as yjj,h.HospResu as zd,h.patsort as lb,";
            ls_sql += "case h.HospPerf when 0 then '在院' else '出院' end as cyzt,h.PriceType as cyjfzt,h.nopayment as wjsf,h.nopaydate as tjjz,h.nurselevel as hljb,";
            ls_sql += "h.curstate as zyqk,h.inpatientcode as fph,h.grandPrice as zyf,case h.patstate when 1 then '已死亡' else '正常' end as brzt,h.deratesum as jm,";
            ls_sql += "h.requoffice1 as ryks,h.requoffice2 as cyks,h.MEDINUMB as jth,h.MEDINUMB1 as ylzh,h.patientid as cyid,";
            ls_sql += "h.patientid1 as sfz,case h.chkpayid when 0 then '未处理' when 8 then '已预审' when 1 then '已审核' when 2 then '已确认' when 3 then '已复审' when 4 then '已审批' when 5 then '已提交' else '重算补兑' end as shzt,h.selffee as zfje,h.subsum as bcje,h.subopr as bcr,";
            ls_sql += "convert(varchar(19),h.subtime,120) as bcsj,case h.printinfo when 1 then '已打印' else '未打印' end as dybz,h.paysum as shje,case h.ifhurt when 1 then '是' else '否' end as ywsh,case h.lockstate when 1 then '冻结' else '正常' end as djbz,";
            ls_sql += "case h.ifaccount when 1 then '可结算' else '不可结算' end as kjsbz,case h.accountstate when 1 then '已结算' else '未结算' end as jsbz";
            ls_sql += " FROM hosppatinfo2 h";
            ls_sql += " WHERE h.hospitalcode='" + hospitalcode + "' AND h.HospID=" + hospid;
            //生成数据
            myDs = if0.getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                if0.getRequestXMLStringFromDS(myDs, "住院病人资料", out mess, out data);
            }
            else
            {
                mess = if0.getXMLErroStrFromString("9000", mess);
            }
            myDs.Dispose();
            return mess;
        }
        else
        {
            mess = if0.getXMLErroStrFromString("9000", mess);
            return mess;
        }
    }

    /// <summary>
    /// 上传一个住院病人资料

    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="user"></param>
    /// <param name="dataXML"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public string putZYPatientInfo(string areacode, string hospitalcode, string user, string dataXML, out string datas)
    {
        string mess = "";
        string dbConnStr = "";
        datas = "";
        if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
        {
            XmlDocument data = if0.getXMLDocumentFromString(dataXML, out mess);
            if (mess == "TRUE")
            {
                //联接串中要去掉这个关键词 Provider=SQLOLEDB.1;
                SqlConnection Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
                try
                {
                    Int32 i, li_cnt, li_tmp, li_chkpayid = 0;
                    string ls_hospid, ls_hospcode = "", ls_patientname = "", ls_sex = "", ls_birth = "", ls_xx = "", ls_gj = "", ls_mz = "", ls_jg = "", ls_hyzt = "", ls_jtzz = "";
                    string ls_dh = "", ls_yb = "", ls_zycs = "", ls_rysj = "", ls_cysj = "", ls_yjj = "", ls_zd = "", ls_lb = "", ls_cyzt = "", ls_cyjfzt = "";
                    string ls_wjsf = "", ls_tjjz = "", ls_hljb = "", ls_zyzt = "", ls_fph = "", ls_zyf = "", ls_ylzh = "", ls_sfz = "";
                    string errtxt = "";
                    bool execresult = true, haverecord = false;
                    XmlNodeList nlis = data.SelectSingleNode("DATA").ChildNodes; //获取DATA节点的所有子节点
                    li_cnt = nlis.Count;
                    Connection.Open();
                    SqlTransaction myTrans;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = Connection;
                    try
                    {
                        for (i = 0; i < li_cnt; i++)
                        {
                            li_chkpayid = 0;
                            haverecord = false;
                            mess = "";
                            nlis = data.GetElementsByTagName("zyid");
                            ls_hospid = Convert.ToString(nlis[i].InnerText);
                            #region //住院ID号有效性验证

                            if (ls_hospid == null || ls_hospid.Trim().Length == 0)
                            {
                                mess += "zyid不能为空";
                            }
                            else
                            {
                                if (!if0.isNumber(ls_hospid))
                                {
                                    mess += "zyid参数必须为数字型";
                                }
                                else
                                {
                                    //住院ID号是否已存在
                                    try
                                    {
                                        cmd.CommandText = "SELECT count(*) FROM hosppatinfo2 where hospitalcode=('" + hospitalcode + "') and HospID=(" + ls_hospid + ")";
                                        li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
                                        if (li_tmp > 0)
                                        {
                                            haverecord = true;
                                            cmd.CommandText = "SELECT chkpayid FROM hosppatinfo2 where hospitalcode=('" + hospitalcode + "') and HospID=(" + ls_hospid + ")";
                                            li_chkpayid = Convert.ToInt32(cmd.ExecuteScalar());
                                            if (li_chkpayid != 0)
                                            {
                                                mess += "资料已审核，不能再上传";
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><zyid>" + ls_hospid + "</zyid><errtxt>" + "数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                        return mess;
                                    }
                                }
                            }
                            #endregion
                            if (li_chkpayid == 0)
                            {
                                nlis = data.GetElementsByTagName("zyh");
                                ls_hospcode = Convert.ToString(nlis[i].InnerText);
                                #region //住院号验证

                                if (ls_hospcode == null || ls_hospcode.Trim().Length == 0)
                                {
                                    mess += "zyh不能为空";
                                }
                                #endregion
                                nlis = data.GetElementsByTagName("xm");
                                ls_patientname = Convert.ToString(nlis[i].InnerText);
                                #region //姓名验证
                                if (ls_patientname == null || ls_patientname.Trim().Length == 0)
                                {
                                    mess += "xm不能为空";
                                }
                                #endregion
                                nlis = data.GetElementsByTagName("xb");
                                ls_sex = Convert.ToString(nlis[i].InnerText);
                                #region //性别验证
                                if (ls_sex == null || ls_sex.Trim().Length == 0)
                                {
                                    mess += "xb不能为空";
                                }
                                else
                                {
                                    if (ls_sex != "0" && ls_sex != "1" && ls_sex != "2")
                                    {
                                        mess += "xb取值必须为2:女,1:男,0:其他";
                                    }
                                }
                                #endregion
                                nlis = data.GetElementsByTagName("sr");
                                ls_birth = Convert.ToString(nlis[i].InnerText);
                                #region //生日格式有效性验证

                                if (ls_birth == null || ls_birth.Trim().Length == 0)
                                {
                                    ls_birth = "1900-01-01 00:00:00";
                                }
                                else
                                {
                                    if (!if0.isDateTime(ls_birth))
                                    {
                                        mess += "sr格式必须为'2009-01-25 08:25:05'的形式";
                                    }
                                }
                                #endregion
                                nlis = data.GetElementsByTagName("xx"); //血型

                                ls_xx = Convert.ToString(nlis[i].InnerText);
                                nlis = data.GetElementsByTagName("gj"); //国家
                                ls_gj = Convert.ToString(nlis[i].InnerText);
                                nlis = data.GetElementsByTagName("mz"); //民族
                                ls_mz = Convert.ToString(nlis[i].InnerText);
                                nlis = data.GetElementsByTagName("jg"); //籍贯
                                ls_jg = Convert.ToString(nlis[i].InnerText);
                                nlis = data.GetElementsByTagName("hyzt"); //婚姻状态

                                ls_hyzt = Convert.ToString(nlis[i].InnerText);
                                nlis = data.GetElementsByTagName("jtzz"); //家庭住址
                                ls_jtzz = Convert.ToString(nlis[i].InnerText);
                                nlis = data.GetElementsByTagName("dh"); //电话
                                ls_dh = Convert.ToString(nlis[i].InnerText);
                                nlis = data.GetElementsByTagName("yb"); //邮编
                                ls_yb = Convert.ToString(nlis[i].InnerText);
                                nlis = data.GetElementsByTagName("zycs");
                                ls_zycs = Convert.ToString(nlis[i].InnerText);
                                #region //第几次住院

                                if (ls_zycs == null || ls_zycs.Trim().Length == 0 || !if0.isNumber(ls_zycs))
                                {
                                    mess += "zycs不能为空且必须为正整数";
                                }
                                #endregion
                                nlis = data.GetElementsByTagName("rysj");
                                ls_rysj = Convert.ToString(nlis[i].InnerText);
                                #region //入院时间有效性验证

                                if (ls_rysj == null || ls_rysj.Trim().Length == 0)
                                {
                                    mess += "rysj不能为空";
                                }
                                else
                                {
                                    if (!if0.isDateTime(ls_rysj))
                                    {
                                        mess += "rysj格式必须为'2009-01-25 08:25:05'的形式";
                                    }
                                }
                                #endregion
                                nlis = data.GetElementsByTagName("cysj");
                                ls_cysj = Convert.ToString(nlis[i].InnerText);
                                #region //出院时间有效性验证

                                if (ls_cysj != null && ls_cysj.Trim().Length != 0)
                                {
                                    if (!if0.isDateTime(ls_cysj))
                                    {
                                        mess += "cysj格式必须为'2009-01-25 08:25:05'的形式";
                                    }
                                }
                                #endregion
                                nlis = data.GetElementsByTagName("yjj");
                                ls_yjj = Convert.ToString(nlis[i].InnerText);
                                #region //预交金验证

                                if (ls_yjj == null || ls_yjj.Trim().Length == 0)
                                {
                                    mess += "yjj不能为空";
                                }
                                else
                                {
                                    if (!if0.isNumber(ls_yjj))
                                    {
                                        mess += "yjj必须是数字型";
                                    }
                                }
                                #endregion
                                nlis = data.GetElementsByTagName("zd");
                                ls_zd = Convert.ToString(nlis[i].InnerText);
                                #region //诊断验证
                                if (ls_zd == null || ls_zd.Trim().Length == 0)
                                {
                                    mess += "zd不能为空";
                                }
                                #endregion
                                nlis = data.GetElementsByTagName("lb");
                                ls_lb = Convert.ToString(nlis[i].InnerText);
                                #region //病人类别验证
                                if (ls_lb == null || ls_lb.Trim().Length == 0)
                                {
                                    mess += "lb不能为空";
                                }
                                #endregion
                                nlis = data.GetElementsByTagName("cyzt");
                                ls_cyzt = Convert.ToString(nlis[i].InnerText);
                                #region //住院状态验证

                                if (ls_cyzt == null || ls_cyzt.Trim().Length == 0)
                                {
                                    mess += "cyzt不能为空";
                                }
                                else
                                {
                                    if (ls_cyzt != "0" && ls_cyzt != "1")
                                    {
                                        mess += "cyzt取值必须为0:在院,1:出院";
                                    }
                                    else
                                    {
                                        if (ls_cyzt == "1")
                                        {
                                            if (ls_cysj == null || ls_cysj.Trim().Length == 0)
                                            {
                                                mess += "cyzt为1(出院)必须有出院时间cysj";
                                            }
                                        }
                                    }
                                }
                                #endregion
                                nlis = data.GetElementsByTagName("cyjfzt");
                                ls_cyjfzt = Convert.ToString(nlis[i].InnerText);
                                #region //出院交费情况验证
                                if (ls_cyjfzt == null || ls_cyjfzt.Trim().Length == 0 || !if0.isNumber(ls_cyjfzt))
                                {
                                    ls_cyjfzt = "1";
                                }
                                #endregion
                                nlis = data.GetElementsByTagName("wjsf");
                                ls_wjsf = Convert.ToString(nlis[i].InnerText);
                                #region //未结算费验证
                                if (ls_wjsf == null || ls_wjsf.Trim().Length == 0)
                                {
                                    mess += "wjsf不能为空";
                                }
                                else
                                {
                                    if (!if0.isNumber(ls_wjsf))
                                    {
                                        mess += "wjsf必须是数字型";
                                    }
                                }
                                #endregion
                                nlis = data.GetElementsByTagName("tjjz");
                                ls_tjjz = Convert.ToString(nlis[i].InnerText);
                                #region //费用统计截止日期验证
                                if (ls_tjjz == null || ls_tjjz.Trim().Length == 0)
                                {
                                    if (ls_cyzt == "1" && ls_cysj != "")
                                    {
                                        if (if0.isDateTime(ls_cysj))
                                        {
                                            ls_tjjz = ls_cysj.Substring(0, 4) + ls_cysj.Substring(5, 2) + ls_cysj.Substring(8, 2);
                                        }
                                        else
                                        {
                                            mess += "tjjz费用统计截止日期不能为空";
                                        }
                                    }
                                    else
                                    {
                                        if (if0.isDateTime(ls_rysj))
                                        {
                                            ls_tjjz = ls_rysj.Substring(0, 4) + ls_rysj.Substring(5, 2) + ls_rysj.Substring(8, 2);
                                        }
                                        else
                                        {
                                            mess += "tjjz费用统计截止日期不能为空";
                                        }
                                    }
                                }
                                if (ls_tjjz != null && ls_tjjz.Trim().Length != 0 && !if0.isDateTime(ls_tjjz.Substring(0, 4) + "-" + ls_tjjz.Substring(4, 2) + "-" + ls_tjjz.Substring(6, 2) + " 00:00:01"))
                                {
                                    mess += "tjjz费用统计截止日期必须为'20080129'的形式";
                                }
                                #endregion
                                nlis = data.GetElementsByTagName("hljb");
                                ls_hljb = Convert.ToString(nlis[i].InnerText);
                                #region //护理级别验证
                                if (ls_hljb == null || ls_hljb.Trim().Length == 0)
                                {
                                    mess += "hljb不能为空";
                                }
                                #endregion
                                nlis = data.GetElementsByTagName("zyqk");
                                ls_zyzt = Convert.ToString(nlis[i].InnerText);
                                #region //在院状态验证

                                if (ls_zyzt == null || ls_zyzt.Trim().Length == 0 || !if0.isNumber(ls_zyzt))
                                {
                                    mess += "zyqk不能为空且必须为整数";
                                }
                                #endregion
                                nlis = data.GetElementsByTagName("fph");
                                ls_fph = Convert.ToString(nlis[i].InnerText);
                                nlis = data.GetElementsByTagName("zyf");
                                ls_zyf = Convert.ToString(nlis[i].InnerText);
                                #region //住院总费用验证

                                if (ls_zyf == null || ls_zyf.Trim().Length == 0 || !if0.isNumber(ls_zyf))
                                {
                                    mess += "zyf不能为空且必须为数字型";
                                }
                                #endregion
                                nlis = data.GetElementsByTagName("ylzh");
                                ls_ylzh = Convert.ToString(nlis[i].InnerText);
                                #region //医疗证号验证
                                if (ls_ylzh == null || ls_ylzh.Trim().Length == 0)
                                {
                                    mess += "ylzh不能为空";
                                }
                                #endregion
                                nlis = data.GetElementsByTagName("sfz");
                                ls_sfz = Convert.ToString(nlis[i].InnerText);
                                #region //身份证号验证
                                if (ls_sfz == null || ls_sfz.Trim().Length == 0)
                                {
                                    mess += "sfz不能为空";
                                }
                                #endregion
                            }
                            if (mess != "")
                            {
                                execresult = false;
                                errtxt += "<ROW><zyid>" + ls_hospid + "</zyid><errtxt>" + mess + "</errtxt></ROW>";
                            }
                            else
                            {
                                #region //更新或插入一条住院病人记录

                                myTrans = Connection.BeginTransaction();
                                try
                                {
                                    cmd.Transaction = myTrans;
                                    if (haverecord) //已上传的做update
                                    {
                                        cmd.CommandText = "UPDATE hosppatinfo2 SET HospCode='" + ls_hospcode + "',PATIENTNAME='" + ls_patientname + "',sex=" + ls_sex;
                                        cmd.CommandText += ",BIRTH='" + ls_birth + "',BLOODTYPE='" + ls_xx + "',COUNTRY='" + ls_gj + "',NATIONALITY='" + ls_mz + "',NATIPLAC='" + ls_jg + "'";
                                        cmd.CommandText += ",MARRIAGE='" + ls_hyzt + "',HOMEADDR='" + ls_jtzz + "',HOMEPHON='" + ls_dh + "',HOMEZIP='" + ls_yb + "',Serial=" + ls_zycs;
                                        cmd.CommandText += ",HospDate='" + ls_rysj + "',OutDate='" + ls_cysj + "',retr_price=" + ls_yjj + ",HospResu='" + ls_zd + "',patsort='" + ls_lb + "'";
                                        cmd.CommandText += ",HospPerf=" + ls_cyzt + ",PriceType=" + ls_cyjfzt + ",nopayment=" + ls_wjsf + ",nopaydate='" + ls_tjjz + "',nurselevel='" + ls_hljb + "'";
                                        cmd.CommandText += ",curstate=" + ls_zyzt + ",inpatientcode='" + ls_fph + "',grandPrice=" + ls_zyf + ",MEDINUMB='" + ls_ylzh + "',medinumb1='" + ls_ylzh + "',patientid='" + ls_sfz + "'";
                                        cmd.CommandText += " WHERE hospitalcode='" + hospitalcode + "' AND HospID=" + ls_hospid;
                                    }
                                    else
                                    {
                                        //插入一条住院病人记录

                                        cmd.CommandText = "INSERT INTO hosppatinfo2(hospitalcode,HospID,HospCode,PATIENTNAME,sex,";
                                        cmd.CommandText += "BIRTH,BLOODTYPE,COUNTRY,NATIONALITY,NATIPLAC,";
                                        cmd.CommandText += "MARRIAGE,HOMEADDR,HOMEPHON,HOMEZIP,Serial,";
                                        cmd.CommandText += "HospDate,OutDate,retr_price,HospResu,patsort,";
                                        cmd.CommandText += "HospPerf,PriceType,nopayment,nopaydate,nurselevel,";
                                        cmd.CommandText += "curstate,inpatientcode,grandPrice,MEDINUMB,medinumb1,patientid)";
                                        cmd.CommandText += " VALUES('" + hospitalcode + "'," + ls_hospid + ",'" + ls_hospcode + "','" + ls_patientname + "'," + ls_sex + ",";
                                        cmd.CommandText += "'" + ls_birth + "','" + ls_xx + "','" + ls_gj + "','" + ls_mz + "','" + ls_jg + "',";
                                        cmd.CommandText += "'" + ls_hyzt + "','" + ls_jtzz + "','" + ls_dh + "','" + ls_yb + "'," + ls_zycs + ",";
                                        cmd.CommandText += "'" + ls_rysj + "','" + ls_cysj + "'," + ls_yjj + ",'" + ls_zd + "','" + ls_lb + "',";
                                        cmd.CommandText += ls_cyzt + "," + ls_cyjfzt + "," + ls_wjsf + ",'" + ls_tjjz + "','" + ls_hljb + "',";
                                        cmd.CommandText += ls_zyzt + ",'" + ls_fph + "'," + ls_zyf + ",'" + ls_ylzh + "','" + ls_ylzh + "','" + ls_sfz + "'";
                                        cmd.CommandText += ")";
                                    }
                                    cmd.ExecuteNonQuery();
                                    myTrans.Commit();
                                }
                                catch (Exception e)
                                {
                                    myTrans.Rollback();
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><zyid>" + ls_hospid + "</zyid><errtxt>" + "数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                    return mess;
                                }
                                finally
                                {
                                    myTrans.Dispose();
                                }
                                #endregion
                            }
                        }
                        if (execresult == true)
                        {
                            datas = "";
                            mess = if0.getXMLStrFromString("执行成功");
                        }
                        else
                        {
                            mess = if0.getXMLErroStrFromStringData("1000", "存在错误", errtxt, out datas);
                        }
                        return mess;
                    }
                    finally
                    {
                        //if (!myReader.IsClosed)
                        //    myReader.Close();
                        //myReader.Dispose();
                        cmd.Dispose();
                    }
                }
                catch (Exception e)
                {
                    mess = if0.getXMLErroStrFromStringData("9000", "发生异常", "<ROW><areacode>" + areacode + "</areacode><errtxt>" + e.Message.ToString() + ",可能缺少数据元</errtxt></ROW>", out datas);
                    return mess;
                }
                finally
                {
                    if (Connection.State == ConnectionState.Open)
                        Connection.Close();
                }
            }
            else
            {
                mess = if0.getXMLErroStrFromString("9000", mess);
                return mess;
            }
        }
        else
        {
            mess = if0.getXMLErroStrFromString("5000", dbConnStr);
            return mess;
        }
    }

    /// <summary>
    /// 删除一个病人资料

    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="hospid"></param>
    /// <returns></returns>
    public string delZYPatientInfo(string areacode, string hospitalcode, string hospid)
    {
        #region //住院ID号有效性验证

        if (hospid == null || hospid.Trim().Length == 0)
        {
            return if0.getXMLErroStrFromString("2000", "住院ID号(zyid)不能为空值");
        }
        else
        {
            if (!if0.isNumber(hospid))
            {
                return if0.getXMLErroStrFromString("2000", "住院ID号(zyid)必须为数字型");
            }
        }
        #endregion
        string mess = "";
        string dbConnStr = "";
        if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
        {
            //联接串中要去掉这个关键词 Provider=SQLOLEDB.1;
            SqlConnection Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
            try
            {
                Connection.Open();
                DataSet myDs = null;
                //是否存在，是否可删除
                string ls_sql = "SELECT h.chkpayid,h.printinfo FROM hosppatinfo2 h WHERE h.hospitalcode='" + hospitalcode + "' AND h.HospID=" + hospid;
                myDs = if0.getDataSet(ls_sql, out mess);
                try
                {
                    if (mess == "TRUE")
                    {
                        if (myDs.Tables[0].Rows.Count > 1)
                        {
                            mess = if0.getXMLErroStrFromString("2000", "存在多条相同的住院ID号(zyid)的病人数据！");//这是不可能发生的
                            return mess;
                        }
                        //else if (myDs.Tables[0].Rows.Count == 0)
                        //{
                        //    mess = getXMLErroStrFromString("2000", "没有这个住院ID号(zyid)的病人");//此时应该也是达到了删除的目的了

                        //    return mess;
                        //}
                        else if (myDs.Tables[0].Rows.Count == 1)
                        {
                            if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) != 0)
                            {
                                mess = if0.getXMLErroStrFromString("2200", "资料已审核，不能删除");
                                return mess;
                            }
                            if (Convert.ToInt32(myDs.Tables[0].Rows[0][1]) == 1)
                            {
                                mess = if0.getXMLErroStrFromString("2200", "病人已获补偿，不能删除");
                                return mess;
                            }
                        }
                    }
                    else
                    {
                        mess = if0.getXMLErroStrFromString("5000", mess);
                        return mess;
                    }
                }
                catch (Exception e)
                {
                    mess = if0.getXMLErroStrFromString("9000", "异常：" + e.Message.ToString());
                    return mess;
                }
                finally
                {
                    myDs.Dispose();
                }

                //删除
                SqlTransaction myTrans;
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = Connection;
                myTrans = Connection.BeginTransaction();
                try
                {
                    cmd.Transaction = myTrans;
                    cmd.CommandText = "DELETE FROM pricedetail WHERE hospitalcode='" + hospitalcode + "' and HospID=" + hospid;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM hosppatinfo2 WHERE hospitalcode='" + hospitalcode + "' and HospID=" + hospid;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM pataccountdetail WHERE hospitalcode='" + hospitalcode + "' and HospID=" + hospid;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM hosppatinfo2_set_info WHERE hospitalcode='" + hospitalcode + "' and HospID=" + hospid;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM log_pricedetail_add WHERE hospitalcode='" + hospitalcode + "' and HospID=" + hospid;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM hosp_check_info WHERE hospitalcode='" + hospitalcode + "' and HospID=" + hospid;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM pataccountprintinfo WHERE hospitalcode='" + hospitalcode + "' and HospID=" + hospid;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM calcagainresult WHERE hospitalcode='" + hospitalcode + "' and HospID=" + hospid;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM calcagainfee WHERE hospitalcode='" + hospitalcode + "' and HospID=" + hospid;
                    cmd.ExecuteNonQuery();
                    myTrans.Commit();
                    mess = if0.getXMLStrFromString("执行成功");
                    return mess;
                }
                catch (Exception e)
                {
                    myTrans.Rollback();
                    mess = if0.getXMLErroStrFromString("9000", "数据库中删除zyid=" + hospid + "数据异常：" + e.Message.ToString());
                    return mess;
                }
                finally
                {
                    myTrans.Dispose();
                }
            }
            catch (Exception e)
            {
                mess = if0.getXMLErroStrFromString("9000", "异常：" + e.Message.ToString());
                return mess;
            }
            finally
            {
                if (Connection.State == ConnectionState.Open)
                    Connection.Close();
            }
        }
        else
        {
            mess = if0.getXMLErroStrFromString("5000", dbConnStr);
            return mess;
        }
    }
    #endregion

    #region 住院病人费用操作
    /// <summary>
    /// 获取指定住院ID号的费用数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="hospcode"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string getZYPatientFee(string areacode, string hospitalcode, string hospid, out string data)
    {
        data = "";
        if (hospid == null || hospid.Length == 0)
        {
            return if0.getXMLErroStrFromString("2000", "住院ID号(zyid)不能为空值");
        }
        if (!if0.isNumber(hospid))
        {
            return if0.getXMLErroStrFromString("2000", "住院ID号(zyid)必须是数字型");
        }
        string mess = "";
        if (if0.getDBConnStr("areacode", areacode, out mess))
        {
            string ls_sql = null;
            DataSet myDs = null;
            ls_sql = "SELECT HospID as zyid,detailid as fyid,nhcode as nhdm,mediid as hismedid,Execoffice as zxks,RequOffice as bqks,";
            ls_sql += "doctname as zrys,opername as zrhs,case MarkType when 0 then '药品' when 1 then '材料' else '服务' end as lx,mediname as xmmc,";
            ls_sql += "convert(varchar(19),ddate,120) as fssj,date3 as qrrq,theoQuan as yzjl,unit as yzdw,timeid as pl,num1 as yl,num2 as jl,";
            ls_sql += "user_num as sl,unitprice as dj,price as je,operid1 as hdr,case chkpayid when 0 then 0 when 8 then 1 else chkpayid+1 end as shzt,";
            ls_sql += "selffee as zffy,prechkfee as ysje,chkinitfee as shje,case in_out when 0 then '保内' else '保外' end as bnbz";
            ls_sql += " FROM pricedetail WHERE hospitalcode='" + hospitalcode + "' AND HospID='" + hospid + "'";
            ls_sql += " ORDER BY mediid ASC";
            //生成数据
            myDs = if0.getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                if0.getRequestXMLStringFromDS(myDs, "住院费用资料", out mess, out data);
            }
            else
            {
                mess = if0.getXMLErroStrFromString("5000", mess);
            }
            myDs.Dispose();
            return mess;
        }
        else
        {
            mess = if0.getXMLErroStrFromString("5000", mess);
            return mess;
        }
    }

    /// <summary>
    /// 上传一个住院病人的费用
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="hospcode"></param>
    /// <param name="dataXML"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public string putZYPatientFee(string areacode, string hospitalcode, string hospid, string dataXML, out string datas)
    {
        string mess = "";
        string dbConnStr = "";
        datas = "";
        //string logs = "";

        //logs += "开始：" + System.DateTime.Now.Millisecond.ToString();

        if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
        {
            XmlDocument data = if0.getXMLDocumentFromString(dataXML, out mess);
            if (mess == "TRUE")
            {
                //联接串中要去掉这个关键词 Provider=SQLOLEDB.1;
                SqlConnection Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
                try
                {
                    int i, li_cnt, li_tmp, li_chkpayid = 0;
                    string ls_detailid = "", ls_nhcode = "", ls_mediid = "", ls_zxks = "", ls_bqks = "";
                    string ls_zrys = "", ls_zrhs = "", ls_lx = "", ls_xmmc = "", ls_fssj = "";
                    string ls_qrrq = "", ls_yzjl = "", ls_yzdw = "", ls_pl = "", ls_yl = "";
                    string ls_jl = "", ls_sl = "", ls_dj = "", ls_je = "", ls_hdr = "";
                    string errtxt = "";
                    bool execresult = true, haverecord = false;
                    XmlNodeList nlis = data.SelectSingleNode("DATA").ChildNodes; //获取DATA节点的所有子节点
                    li_cnt = nlis.Count;
                    Connection.Open();
                    SqlTransaction myTrans;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = Connection;
                    //SqlDataReader myReader;
                    try
                    {
                        #region 住院ID号不存在或已审核不能上传，返回

                        try
                        {
                            cmd.CommandText = "SELECT count(*) FROM hosppatinfo2 WHERE hospitalcode=('" + hospitalcode + "') and HospID=(" + hospid + ")";
                            li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
                            if (li_tmp == 0)
                            {
                                mess = if0.getXMLErroStrFromStringData("1000", "存在错误", "<ROW><hospid>" + hospid + "</hospid><errtxt>病人信息还没上传，不能上传费用</errtxt></ROW>", out datas);
                                return mess;
                            }
                            else
                            {
                                cmd.CommandText = "SELECT chkpayid FROM hosppatinfo2 where hospitalcode=('" + hospitalcode + "') and HospID=(" + hospid + ")";
                                li_chkpayid = Convert.ToInt32(cmd.ExecuteScalar());
                                if (li_chkpayid != 0)
                                {
                                    if0.getXMLErroStrFromStringData("2200", "上传失败", "<ROW><hospid>" + hospid + "</hospid><errtxt>资料已审核，不能再上传费用</errtxt></ROW>", out datas);
                                    return mess;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><zyid>" + hospid + "</zyid><errtxt>" + "数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                            return mess;
                        }
                        #endregion
                        for (i = 0; i < li_cnt; i++)
                        {
                            haverecord = false;
                            mess = "";
                            li_chkpayid = 0;
                            nlis = data.GetElementsByTagName("fyid");
                            ls_detailid = Convert.ToString(nlis[i].InnerText);
                            if (ls_detailid == null || ls_detailid.Trim().Length == 0 || !if0.isNumber(ls_detailid))
                            {
                                mess += "fyid必须为数字型";
                                if (ls_detailid == null || ls_detailid.Trim().Length == 0)
                                {
                                    ls_detailid = "";
                                }
                            }
                            else
                            {
                                #region//费用已存在未审核的只能update，已审核的不能上传

                                try
                                {
                                    cmd.CommandText = "SELECT count(*) FROM pricedetail WHERE hospitalcode=('" + hospitalcode + "') AND HospID=(" + hospid + ") AND detailid=" + ls_detailid;
                                    li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
                                    if (li_tmp > 0)
                                    {
                                        haverecord = true;
                                        cmd.CommandText = "SELECT chkpayid FROM pricedetail WHERE hospitalcode=('" + hospitalcode + "') AND HospID=(" + hospid + ") AND detailid=" + ls_detailid;
                                        li_chkpayid = Convert.ToInt32(cmd.ExecuteScalar());
                                        if (li_chkpayid != 0)
                                        {
                                            mess += "该费用已审核，不能修改";
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><zyid>" + hospid + "</zyid><errtxt>" + "数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                    return mess;
                                }
                                #endregion
                                #region 费用未审核的继续检测xml数据单元有效性

                                if (li_chkpayid == 0)
                                {

                                    //logs += "；" + ls_detailid + "取数据开始：" + System.DateTime.Now.Millisecond.ToString();

                                    nlis = data.GetElementsByTagName("nhdm");
                                    ls_nhcode = Convert.ToString(nlis[i].InnerText);
                                    nlis = data.GetElementsByTagName("hismlid");
                                    ls_mediid = Convert.ToString(nlis[i].InnerText);
                                    nlis = data.GetElementsByTagName("zxks");
                                    ls_zxks = Convert.ToString(nlis[i].InnerText);
                                    nlis = data.GetElementsByTagName("bqks");
                                    ls_bqks = Convert.ToString(nlis[i].InnerText);
                                    nlis = data.GetElementsByTagName("zrys");
                                    ls_zrys = Convert.ToString(nlis[i].InnerText);
                                    nlis = data.GetElementsByTagName("zrhs");
                                    ls_zrhs = Convert.ToString(nlis[i].InnerText);
                                    nlis = data.GetElementsByTagName("lx");
                                    ls_lx = Convert.ToString(nlis[i].InnerText);
                                    nlis = data.GetElementsByTagName("xmmc");
                                    ls_xmmc = Convert.ToString(nlis[i].InnerText);
                                    nlis = data.GetElementsByTagName("fssj");
                                    ls_fssj = Convert.ToString(nlis[i].InnerText);
                                    nlis = data.GetElementsByTagName("qrrq");
                                    ls_qrrq = Convert.ToString(nlis[i].InnerText);
                                    nlis = data.GetElementsByTagName("yzjl");
                                    ls_yzjl = Convert.ToString(nlis[i].InnerText);
                                    nlis = data.GetElementsByTagName("yzdw");
                                    ls_yzdw = Convert.ToString(nlis[i].InnerText);
                                    nlis = data.GetElementsByTagName("pl");
                                    ls_pl = Convert.ToString(nlis[i].InnerText);
                                    nlis = data.GetElementsByTagName("yl");
                                    ls_yl = Convert.ToString(nlis[i].InnerText);
                                    nlis = data.GetElementsByTagName("jl");
                                    ls_jl = Convert.ToString(nlis[i].InnerText);
                                    nlis = data.GetElementsByTagName("sl");
                                    ls_sl = Convert.ToString(nlis[i].InnerText);
                                    nlis = data.GetElementsByTagName("dj");
                                    ls_dj = Convert.ToString(nlis[i].InnerText);
                                    nlis = data.GetElementsByTagName("je");
                                    ls_je = Convert.ToString(nlis[i].InnerText);
                                    nlis = data.GetElementsByTagName("hdr");
                                    ls_hdr = Convert.ToString(nlis[i].InnerText);

                                    //logs += "，校验开始：" + System.DateTime.Now.Millisecond.ToString();

                                    //数据校验代码
                                    if (ls_nhcode == null || ls_nhcode.Trim().Length == 0)
                                    {
                                        mess += "nhdm不能为空，必须上传匹配的农合代码";
                                    }
                                    //未完成 这里还可以加入判断农合代码是否正确的判断，并计算出是否保内

                                    if (ls_mediid == null || ls_mediid.Trim().Length == 0 || !if0.isNumber(ls_mediid))
                                    {
                                        mess += "hismlid必须为数字型";
                                    }
                                    if (ls_zxks == null || ls_zxks.Trim().Length == 0)
                                    {
                                        mess += "zxks不能为空";
                                    }
                                    if (ls_bqks == null || ls_bqks.Trim().Length == 0)
                                    {
                                        mess += "bqks不能为空";
                                    }
                                    if (ls_zrys == null || ls_zrys.Trim().Length == 0)
                                    {
                                        mess += "zrys不能为空";
                                    }
                                    if (ls_zrhs == null || ls_zrhs.Trim().Length == 0)
                                    {
                                        mess += "zrhs不能为空";
                                    }
                                    if (ls_lx == null || ls_lx.Trim().Length == 0 || !if0.isNumber(ls_lx))
                                    {
                                        mess += "lx必须为数字型";
                                    }
                                    else if (ls_lx != "0" && ls_lx != "1" && ls_lx != "2")
                                    {
                                        mess += "lx必须为指定的数字0药品1材料2医疗服务";
                                    }
                                    if (ls_xmmc == null || ls_xmmc.Trim().Length == 0)
                                    {
                                        ls_xmmc = "";
                                        mess += "xmmc不能为空";
                                    }
                                    if (ls_fssj == null || ls_fssj.Trim().Length == 0)
                                    {
                                        mess += "fssj不能为空";
                                    }
                                    else
                                    {
                                        if (!if0.isDateTime(ls_fssj))
                                        {
                                            mess += "fssj格式必须为'2009-01-25 08:25:45'的格式";
                                        }
                                    }
                                    if (ls_qrrq == null || ls_qrrq.Trim().Length == 0)
                                    {
                                        mess += "qrrq不能为空";
                                    }
                                    else
                                    {
                                        if (!if0.isDateTime(ls_qrrq.Substring(0, 4) + "-" + ls_qrrq.Substring(4, 2) + "-" + ls_qrrq.Substring(6, 2) + " 00:00:00"))
                                        {
                                            mess += "qrrq格式必须为'20090125'的格式";
                                        }
                                    }
                                    if (ls_yl == null || ls_yl.Trim().Length == 0 || !if0.isNumber(ls_yl))
                                    {
                                        mess += "yl必须为数字型";
                                    }
                                    if (ls_jl == null || ls_jl.Trim().Length == 0 || !if0.isNumber(ls_jl))
                                    {
                                        mess += "jl必须为数字型";
                                    }
                                    if (ls_sl == null || ls_sl.Trim().Length == 0 || !if0.isNumber(ls_sl))
                                    {
                                        mess += "sl必须为数字型";
                                    }
                                    if (ls_dj == null || ls_dj.Trim().Length == 0 || !if0.isNumber(ls_dj))
                                    {
                                        mess += "dj必须为数字型";
                                    }
                                    if (ls_je == null || ls_je.Trim().Length == 0 || !if0.isNumber(ls_je))
                                    {
                                        mess += "je必须为数字型";
                                    }
                                    if (ls_hdr == null || ls_hdr.Trim().Length == 0)
                                    {
                                        mess += "hdr不能为空";
                                    }

                                    //logs += "，校验结束：" + System.DateTime.Now.Millisecond.ToString();

                                }
                                #endregion
                            }
                            if (mess != "")
                            {
                                execresult = false;
                                errtxt += "<ROW><fyid>" + ls_detailid + "</fyid><errtxt>住院ID" + hospid + ",fyid" + ls_detailid + "(" + ls_xmmc + "):" + mess + "</errtxt></ROW>";
                            }
                            else
                            {
                                #region 更新数据
                                myTrans = Connection.BeginTransaction();
                                try
                                {

                                    //logs += "，更新开始：" + System.DateTime.Now.Millisecond.ToString();

                                    cmd.Transaction = myTrans;
                                    if (haverecord) //已上传的做update
                                    {
                                        cmd.CommandText = "UPDATE pricedetail SET nhcode='" + ls_nhcode + "',Execoffice='" + ls_zxks + "'";
                                        cmd.CommandText += ",RequOffice='" + ls_bqks + "',doctname='" + ls_zrys + "',opername='" + ls_zrhs + "',MarkType=" + ls_lx + ",mediname='" + ls_xmmc + "'";
                                        cmd.CommandText += ",ddate='" + ls_fssj + "',date2='" + ls_fssj.Substring(0, 4) + ls_fssj.Substring(5, 2) + ls_fssj.Substring(8, 2) + "',date3='" + ls_qrrq + "',theoQuan='" + ls_yzjl + "',unit='" + ls_yzdw + "'";
                                        cmd.CommandText += ",timeid='" + ls_pl + "',num1=" + ls_yl + ",num2=" + ls_jl + ",user_num=" + ls_sl + ",unitprice=" + ls_dj;
                                        cmd.CommandText += ",price=" + ls_je + ",operid1='" + ls_hdr + "',mediid=" + ls_mediid + ",nhcodebak='" + ls_nhcode + "'";
                                        cmd.CommandText += " WHERE hospitalcode=('" + hospitalcode + "') AND HospID=(" + hospid + ") AND detailid=" + ls_detailid;
                                    }
                                    else
                                    {
                                        cmd.CommandText = "INSERT INTO pricedetail(hospitalcode,HospID,detailid,nhcode,Execoffice,";
                                        cmd.CommandText += "RequOffice,doctname,opername,MarkType,mediname,";
                                        cmd.CommandText += "ddate,date2,date3,theoQuan,unit,";
                                        cmd.CommandText += "timeid,num1,num2,user_num,unitprice,";
                                        cmd.CommandText += "price,operid1,chkpayid,mediid,nhcodebak)";
                                        cmd.CommandText += " VALUES('" + hospitalcode + "'," + hospid + "," + ls_detailid + ",'" + ls_nhcode + "','" + ls_zxks + "',";
                                        cmd.CommandText += "'" + ls_bqks + "','" + ls_zrys + "','" + ls_zrhs + "'," + ls_lx + ",'" + ls_xmmc + "',";
                                        cmd.CommandText += "'" + ls_fssj + "','" + ls_fssj.Substring(0, 4) + ls_fssj.Substring(5, 2) + ls_fssj.Substring(8, 2) + "','" + ls_qrrq + "','" + ls_yzjl + "','" + ls_yzdw + "',";
                                        cmd.CommandText += "'" + ls_pl + "'," + ls_yl + "," + ls_jl + "," + ls_sl + "," + ls_dj + ",";
                                        cmd.CommandText += ls_je + ",'" + ls_hdr + "',0," + ls_mediid + ",'" + ls_nhcode + "'";
                                        cmd.CommandText += ")";
                                    }
                                    cmd.ExecuteNonQuery();
                                    myTrans.Commit();

                                    //logs += "，更新结束：" + System.DateTime.Now.Millisecond.ToString();

                                }
                                catch (Exception e)
                                {
                                    myTrans.Rollback();
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><fyid>" + ls_detailid + "</fyid><errtxt>住院ID" + hospid + ":数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                    return mess;
                                }
                                finally
                                {
                                    myTrans.Dispose();
                                }
                                #endregion
                            }
                        }
                        if (execresult == true)
                        {
                            //mess = if0.getXMLStrFromString("执行成功" + logs);
                            mess = if0.getXMLStrFromString("执行成功");
                        }
                        else
                        {
                            mess = if0.getXMLErroStrFromStringData("1000", "存在错误", errtxt, out datas);
                        }
                        return mess;
                    }
                    finally
                    {
                        cmd.Dispose();
                    }
                }
                catch (Exception e)
                {
                    mess = if0.getXMLErroStrFromStringData("9000", "发生异常", "<ROW><areacode>" + areacode + "</areacode><errtxt>" + e.Message.ToString() + ",可能缺少数据元</errtxt></ROW>", out datas);
                    return mess;
                }
                finally
                {
                    if (Connection.State == ConnectionState.Open)
                        Connection.Close();
                }
            }
            else
            {
                mess = if0.getXMLErroStrFromString("9000", mess);
                return mess;
            }
        }
        else
        {
            mess = if0.getXMLErroStrFromString("5000", dbConnStr);
            return mess;
        }
    }

    /// <summary>
    /// 删除一个住院病人的所有费用

    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="hospcode"></param>
    /// <returns></returns>
    public string delZYPatientFee(string areacode, string hospitalcode, string hospid)
    {
        if (hospid == null || hospid.Length == 0)
        {
            return if0.getXMLErroStrFromString("2000", "住院ID号(zyid)不能为空值");
        }
        if (!if0.isNumber(hospid))
        {
            return if0.getXMLErroStrFromString("2000", "住院ID号(zyid)必须是数字型");
        }
        string mess = "";
        string dbConnStr = "";
        if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
        {
            //联接串中要去掉这个关键词 Provider=SQLOLEDB.1;
            SqlConnection Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
            try
            {
                Connection.Open();
                DataSet myDs = null;
                //是否存在，是否可删除
                #region
                string ls_sql = "SELECT h.chkpayid,h.printinfo FROM hosppatinfo2 h WHERE h.hospitalcode='" + hospitalcode + "' AND h.HospID=" + hospid;
                myDs = if0.getDataSet(ls_sql, out mess);
                try
                {
                    if (mess == "TRUE")
                    {
                        if (myDs.Tables[0].Rows.Count > 1)
                        {
                            mess = if0.getXMLErroStrFromString("2000", "存在多条相同的住院ID号(zyid)的病人数据！");//这是不可能发生的
                            return mess;
                        }
                        //else if (myDs.Tables[0].Rows.Count == 0)
                        //{
                        //    mess = getXMLErroStrFromString("2000", "没有这个住院ID号(zyid)的病人");//此时应该也是达到了删除的目的了

                        //    return mess;
                        //}
                        else if (myDs.Tables[0].Rows.Count == 1)
                        {
                            if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) != 0)
                            {
                                mess = if0.getXMLErroStrFromString("2200", "资料已审核，不能删除");
                                return mess;
                            }
                            if (Convert.ToInt32(myDs.Tables[0].Rows[0][1]) == 1)
                            {
                                mess = if0.getXMLErroStrFromString("2200", "病人已获补偿，不能删除");
                                return mess;
                            }
                        }
                    }
                    else
                    {
                        mess = if0.getXMLErroStrFromString("5000", mess);
                        return mess;
                    }
                }
                catch (Exception e)
                {
                    mess = if0.getXMLErroStrFromString("9000", "异常：" + e.Message.ToString());
                    return mess;
                }
                finally
                {
                    myDs.Dispose();
                }
                #endregion

                //删除
                SqlTransaction myTrans;
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = Connection;
                myTrans = Connection.BeginTransaction();
                try
                {
                    cmd.Transaction = myTrans;
                    cmd.CommandText = "DELETE FROM log_pricedetail_add WHERE hospitalcode='" + hospitalcode + "' and HospID=" + hospid;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM pricedetail WHERE hospitalcode='" + hospitalcode + "' and HospID='" + hospid + "'";
                    cmd.ExecuteNonQuery();
                    myTrans.Commit();
                    mess = if0.getXMLStrFromString("执行成功");
                    return mess;
                }
                catch (Exception e)
                {
                    myTrans.Rollback();
                    mess = if0.getXMLErroStrFromString("9000", "数据库中删除zyid=" + hospid + "费用数据异常：" + e.Message.ToString());
                    return mess;
                }
                finally
                {
                    myTrans.Dispose();
                }
            }
            catch (Exception e)
            {
                mess = if0.getXMLErroStrFromString("9000", "异常：" + e.Message.ToString());
                return mess;
            }
            finally
            {
                if (Connection.State == ConnectionState.Open)
                    Connection.Close();
            }
        }
        else
        {
            mess = if0.getXMLErroStrFromString("5000", dbConnStr);
            return mess;
        }
    }
    #endregion

    #region HISonline接口调用住院相关

    /// <summary>
    /// HISonline接口调用获取住院数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <param name="hospid"></param>
    /// <param name="startdate"></param>
    /// <param name="enddate"></param>
    /// <returns></returns>
    public string putZYDataFromHIS(string areacode,string hospitalcode, string servername, string dbname, string user, string pwd, string hospid, string startdate, string enddate)
    {
        string hisifurl = "", requestParmXML = "", retMessXML = "", dbConnStr = "", ls_sql = "";
        DataSet lds = new DataSet();
        if (!if0.getWebServiceURL(servername, out hisifurl))
        {
            return if0.getXMLErroStrFromString("2000", hisifurl);
        }
        requestParmXML = "<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "UTF-8" + '"' + "?>";
        requestParmXML+="<ROW><requesttype>2000</requesttype>";
        requestParmXML += "<hospid>" + hospid + "</hospid>";
        requestParmXML += "<startdate>" + startdate + "</startdate>";
        requestParmXML += "<enddate>" + enddate + "</enddate>";
        requestParmXML += "</ROW>";
        try
        {
            //创建his的Webservice接口并调用获取住院数据的方法
            hisservice his = new hisservice(hisifurl);
            if (his.rdhlhnhisif(servername, dbname, user, pwd, requestParmXML, lds, out lds, out retMessXML))
            {
                //调用成功，处理传出的DataSet
                retMessXML = "";
                #region 查询数据库Insert或Update数据
                if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
                {
                    //联接串中要去掉这个关键词 Provider=SQLOLEDB.1;
                    SqlConnection Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
                    try
                    {
                        Connection.Open();
                        SqlTransaction myTrans;
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = Connection;
                        try
                        {
                            int li_tmp,i;
                            bool haverecord = false;//农合中有没有此hospid，false没有，则需要做insert的

                            #region //住院ID号查证

                            try
                            {
                                cmd.CommandText = "SELECT count(*) FROM hosppatinfo2 where hospitalcode=('" + hospitalcode + "') and HospID=(" + hospid + ")";
                                li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
                                if (li_tmp > 0)
                                {
                                    haverecord = true;
                                    cmd.CommandText = "SELECT chkpayid FROM hosppatinfo2 where hospitalcode=('" + hospitalcode + "') and HospID=(" + hospid + ")";
                                    li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
                                    if (li_tmp != 0)
                                    {
                                        return if0.getXMLErroStrFromString("2200", "资料已审核，不能再上传");
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                return if0.getXMLErroStrFromString("9000", "农合数据库操作异常：" + e.Message.ToString());
                            }
                            #endregion
                            #region 生成并执行住院病人相关SQL
                            try
                            {
                                for (i = 0; i < lds.Tables["patient"].Rows.Count; i++)
                                {
                                    if (haverecord)
                                    {
                                        #region 生成做UPDATE的SQL
                                        //未考虑lds.Tables["patient"].Rows[i][j] == null的情况

                                        ls_sql = "UPDATE hosppatinfo2 SET ";
                                        ls_sql += "HospCode='" + lds.Tables["patient"].Rows[i]["zyh"].ToString() + "'";
                                        ls_sql += ",PATIENTNAME='" + lds.Tables["patient"].Rows[i]["xm"].ToString() + "'";
                                        ls_sql += ",sex=" + lds.Tables["patient"].Rows[i]["xb"].ToString();
                                        ls_sql += ",BIRTH='" + lds.Tables["patient"].Rows[i]["sr"].ToString() + "'";
                                        ls_sql += ",BLOODTYPE='" + lds.Tables["patient"].Rows[i]["xx"].ToString() + "'";
                                        ls_sql += ",COUNTRY='" + lds.Tables["patient"].Rows[i]["gj"].ToString() + "'";
                                        ls_sql += ",NATIONALITY='" + lds.Tables["patient"].Rows[i]["mz"].ToString() + "'";
                                        ls_sql += ",NATIPLAC='" + lds.Tables["patient"].Rows[i]["jg"].ToString() + "'";
                                        ls_sql += ",MARRIAGE='" + lds.Tables["patient"].Rows[i]["hyzt"].ToString() + "'";
                                        ls_sql += ",HOMEADDR='" + lds.Tables["patient"].Rows[i]["jtzz"].ToString() + "'";
                                        ls_sql += ",HOMEPHON='" + lds.Tables["patient"].Rows[i]["dh"].ToString() + "'";
                                        ls_sql += ",HOMEZIP='" + lds.Tables["patient"].Rows[i]["yb"].ToString() + "'";
                                        ls_sql += ",Serial=" + lds.Tables["patient"].Rows[i]["zycs"].ToString();
                                        ls_sql += ",HospDate='" + lds.Tables["patient"].Rows[i]["rysj"].ToString() + "'";
                                        ls_sql += ",OutDate='" + lds.Tables["patient"].Rows[i]["cysj"].ToString() + "'";
                                        ls_sql += ",retr_price=" + lds.Tables["patient"].Rows[i]["yjj"].ToString();
                                        ls_sql += ",HospResu='" + lds.Tables["patient"].Rows[i]["zd"].ToString() + "'";
                                        ls_sql += ",patsort='" + lds.Tables["patient"].Rows[i]["lb"].ToString() + "'";
                                        ls_sql += ",HospPerf=" + lds.Tables["patient"].Rows[i]["cyzt"].ToString();
                                        ls_sql += ",PriceType=" + lds.Tables["patient"].Rows[i]["cyjfzt"].ToString();
                                        ls_sql += ",nopayment=" + lds.Tables["patient"].Rows[i]["wjsf"].ToString();
                                        ls_sql += ",nopaydate='" + lds.Tables["patient"].Rows[i]["tjjz"].ToString() + "'";
                                        ls_sql += ",nurselevel='" + lds.Tables["patient"].Rows[i]["hljb"].ToString() + "'";
                                        ls_sql += ",curstate=" + lds.Tables["patient"].Rows[i]["zyqk"].ToString();
                                        ls_sql += ",inpatientcode='" + lds.Tables["patient"].Rows[i]["fph"].ToString() + "'";
                                        ls_sql += ",grandPrice=" + lds.Tables["patient"].Rows[i]["zyf"].ToString();
                                        ls_sql += ",MEDINUMB='" + lds.Tables["patient"].Rows[i]["ylzh"].ToString() + "'";
                                        ls_sql += ",medinumb1='" + lds.Tables["patient"].Rows[i]["ylzh"].ToString() + "'";
                                        ls_sql += ",patientid='" + lds.Tables["patient"].Rows[i]["sfz"].ToString() + "'";
                                        ls_sql += " WHERE hospitalcode='" + hospitalcode + "' AND HospID=" + hospid;
                                        #endregion
                                    }
                                    else
                                    {
                                        #region 生成做INSERT的SQL
                                        //未考虑lds.Tables["patient"].Rows[i][j] == null的情况

                                        ls_sql = "INSERT INTO hosppatinfo2(hospitalcode,HospID,HospCode,PATIENTNAME,sex,";
                                        ls_sql += "BIRTH,BLOODTYPE,COUNTRY,NATIONALITY,NATIPLAC,";
                                        ls_sql += "MARRIAGE,HOMEADDR,HOMEPHON,HOMEZIP,Serial,";
                                        ls_sql += "HospDate,OutDate,retr_price,HospResu,patsort,";
                                        ls_sql += "HospPerf,PriceType,nopayment,nopaydate,nurselevel,";
                                        ls_sql += "curstate,inpatientcode,grandPrice,MEDINUMB,medinumb1,patientid)";
                                        ls_sql += " VALUES(";
                                        ls_sql += "'" + hospitalcode + "'";
                                        ls_sql += "," + hospid;
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["zyh"].ToString() + "'";
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["xm"].ToString() + "'";
                                        ls_sql += "," + lds.Tables["patient"].Rows[i]["xb"].ToString();
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["sr"].ToString() + "'";
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["xx"].ToString() + "'";
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["gj"].ToString() + "'";
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["mz"].ToString() + "'";
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["jg"].ToString() + "'";
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["hyzt"].ToString() + "'";
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["jtzz"].ToString() + "'";
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["dh"].ToString() + "'";
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["yb"].ToString() + "'";
                                        ls_sql += "," + lds.Tables["patient"].Rows[i]["zycs"].ToString();
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["rysj"].ToString() + "'";
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["cysj"].ToString() + "'";
                                        ls_sql += "," + lds.Tables["patient"].Rows[i]["yjj"].ToString();
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["zd"].ToString() + "'";
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["lb"].ToString() + "'";
                                        ls_sql += "," + lds.Tables["patient"].Rows[i]["cyzt"].ToString();
                                        ls_sql += "," + lds.Tables["patient"].Rows[i]["cyjfzt"].ToString();
                                        ls_sql += "," + lds.Tables["patient"].Rows[i]["wjsf"].ToString();
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["tjjz"].ToString() + "'";
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["hljb"].ToString() + "'";
                                        ls_sql += "," + lds.Tables["patient"].Rows[i]["zyqk"].ToString();
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["fph"].ToString() + "'";
                                        ls_sql += "," + lds.Tables["patient"].Rows[i]["zyf"].ToString();
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["ylzh"].ToString() + "'";
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["ylzh"].ToString() + "'";
                                        ls_sql += ",'" + lds.Tables["patient"].Rows[i]["sfz"].ToString() + "'";
                                        ls_sql += ")";
                                        #endregion
                                    }
                                    #region //保存住院病人记录
                                        myTrans = Connection.BeginTransaction();
                                        try
                                        {
                                            cmd.Transaction = myTrans;
                                            cmd.CommandText = ls_sql;
                                            cmd.ExecuteNonQuery();
                                            myTrans.Commit();
                                        }
                                        catch (Exception e)
                                        {
                                            myTrans.Rollback();
                                            return if0.getXMLErroStrFromString("9000", "HIS住院数据更新到农合时异常：" + e.Message.ToString());
                                        }
                                        finally
                                        {
                                            myTrans.Dispose();
                                        }
                                        #endregion
                                }
                            }
                            catch (Exception e)
                            {
                                return if0.getXMLErroStrFromString("9000", "生成HIS住院数据更新到农合的语句时异常：" + e.Message.ToString());
                            }
                            #endregion
                            #region 生成并执行住院病人费用相关SQL
                            try
                            {
                                #region //删除住院病人此时间段内的费用记录
                                myTrans = Connection.BeginTransaction();
                                try
                                {
                                    ls_sql = "DELETE FROM pricedetail WHERE hospitalcode=('" + hospitalcode + "') AND HospID=(" + hospid + ") AND date3 BETWEEN '" + startdate + "' AND '" + enddate + "'";
                                    cmd.Transaction = myTrans;
                                    cmd.CommandText = ls_sql;
                                    cmd.ExecuteNonQuery();
                                    myTrans.Commit();
                                }
                                catch (Exception e)
                                {
                                    myTrans.Rollback();
                                    return if0.getXMLErroStrFromString("9000", "HIS住院费用数据更新时异常：" + e.Message.ToString());
                                }
                                finally
                                {
                                    myTrans.Dispose();
                                }
                                #endregion
                                for (i = 0; i < lds.Tables["fee"].Rows.Count; i++)
                                {
                                    #region 生成做INSERT的SQL
                                    //未考虑lds.Tables["patient"].Rows[i][j] == null的情况

                                    ls_sql = "INSERT INTO pricedetail(hospitalcode,HospID,detailid,nhcode,Execoffice,";
                                    ls_sql += "RequOffice,doctname,opername,MarkType,mediname,";
                                    ls_sql += "ddate,date2,date3,theoQuan,unit,";
                                    ls_sql += "timeid,num1,num2,user_num,unitprice,";
                                    ls_sql += "price,operid1,chkpayid,mediid,nhcodebak)";
                                    ls_sql += " VALUES(";
                                    ls_sql += "'" + hospitalcode + "'";
                                    ls_sql += "," + hospid;
                                    ls_sql += "," + lds.Tables["fee"].Rows[i]["fyid"].ToString();
                                    ls_sql += ",'" + lds.Tables["fee"].Rows[i]["nhdm"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables["fee"].Rows[i]["zxks"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables["fee"].Rows[i]["bqks"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables["fee"].Rows[i]["zrys"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables["fee"].Rows[i]["zrhs"].ToString() + "'";
                                    ls_sql += "," + lds.Tables["fee"].Rows[i]["lx"].ToString();
                                    ls_sql += ",'" + lds.Tables["fee"].Rows[i]["xmmc"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables["fee"].Rows[i]["fssj"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables["fee"].Rows[i]["qrrq"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables["fee"].Rows[i]["qrrq"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables["fee"].Rows[i]["yzjl"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables["fee"].Rows[i]["yzdw"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables["fee"].Rows[i]["pl"].ToString() + "'";
                                    ls_sql += "," + lds.Tables["fee"].Rows[i]["yl"].ToString();
                                    ls_sql += "," + lds.Tables["fee"].Rows[i]["jl"].ToString();
                                    ls_sql += "," + lds.Tables["fee"].Rows[i]["sl"].ToString();
                                    ls_sql += "," + lds.Tables["fee"].Rows[i]["dj"].ToString();
                                    ls_sql += "," + lds.Tables["fee"].Rows[i]["je"].ToString();
                                    ls_sql += ",'" + lds.Tables["fee"].Rows[i]["hdr"].ToString() + "'";
                                    ls_sql += ",0";
                                    ls_sql += "," + lds.Tables["fee"].Rows[i]["hismlid"].ToString();
                                    ls_sql += ",'" + lds.Tables["fee"].Rows[i]["nhdm"].ToString() + "'";
                                    ls_sql += ")";
                                    #endregion
                                    #region //插入住院病人费用记录
                                    myTrans = Connection.BeginTransaction();
                                    try
                                    {
                                        cmd.Transaction = myTrans;
                                        cmd.CommandText = ls_sql;
                                        cmd.ExecuteNonQuery();
                                        myTrans.Commit();
                                    }
                                    catch (Exception e)
                                    {
                                        myTrans.Rollback();
                                        return if0.getXMLErroStrFromString("9000", "HIS住院费用数据更新到农合时异常：" + e.Message.ToString());
                                    }
                                    finally
                                    {
                                        myTrans.Dispose();
                                    }
                                    #endregion
                                }
                            }
                            catch (Exception e)
                            {
                                return if0.getXMLErroStrFromString("9000", "生成HIS住院数据更新到农合的语句时异常：" + e.Message.ToString());
                            }
                            #endregion
                        }
                        finally
                        {
                            cmd.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                       return if0.getXMLErroStrFromString("9000", "建立新农合数据库联接异常：" + e.Message.ToString());
                    }
                    finally
                    {
                        if (Connection.State == ConnectionState.Open)
                            Connection.Close();
                    }
                }
                else
                {
                    return if0.getXMLErroStrFromString("5000", dbConnStr);
                }
                #endregion
                retMessXML = if0.getXMLStrFromString("住院ID" + hospid + "从" + startdate + "至" + enddate + "的住院数据传输成功！");
            }
            return retMessXML;
        }
        catch (Exception e)
        {
            return if0.getXMLErroStrFromString("9000", "调用HIS接口异常：" + e.Message.ToString());
        }
    }

    /// <summary>
    /// HISonline接口调用获取住院病人数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <returns></returns>
    public string putZYPatientFromHIS(string areacode, string hospitalcode, string servername, string dbname, string user, string pwd, string hospid)
    {
        string hisifurl = "", retMessXML = "", dbConnStr = "", ls_sql = "";
        DataSet lds = new DataSet();
        if (!if0.getWebServiceURL(servername, out hisifurl))
        {
            return if0.getXMLErroStrFromString("2000", hisifurl);
        }
        ls_sql = "select hospid as zyid,hospcode as zyh,";
        ls_sql += "name as xm,SEX as xb,convert(varchar(19),birth,120) as sr,xx as xx,";
        ls_sql += "country as gj,nationality as mz,natiplac as jg,";
        ls_sql += "(select marriname from " + dbname + "..GB_Marri m where m.marriid=p.marriage) hyzt,";
        ls_sql += "homeaddr as jtzz,homephon as dh,homezip as yb,serial as zycs,convert(varchar(19),hospdate,120) as rysj,";
        ls_sql += "convert(varchar(19),outdate,120) as cysj,granddeposit as yjj,";
        ls_sql += "isnull((select name from " + dbname + "..micd9 m where m.surgid = p.HospResu),'未匹配') as zd,";
        ls_sql += "(select patkindname from " + dbname + "..DictPatKind d where d.patkindid = p.patsort) as lb,";
        ls_sql += "hospperf as cyzt,0 as cyjfzt,nopayment as wjsf,nopaydate as tjjz,";
        ls_sql += "(case nurselevel when 1 then '特级' when 2 then '一级' when 3 then '二级'  when 4 then '三级' when 5 then '常规' else '常规' end ) hljb,";
        ls_sql += "curstate as zyqk,";
        ls_sql += "isnull((select h1.code from " + dbname + "..hosp_pay_history h1 where priceid =(select max(h2.priceid) from " + dbname + "..hosp_pay_history h2 where h2.hospid = p.hospid)),'无') as fph,";
        ls_sql += "grandprice as zyf,medinumb as ylzh,id as sfz";
        ls_sql += " from " + dbname + "..patinfo p where hospid=" + hospid;
        if (if0.getDataFromHIS(servername, dbname, user, pwd, ls_sql, out retMessXML, out lds))
        {
            //调用成功，处理传出的DataSet
            retMessXML = "";
            #region 查询数据库Insert或Update数据
            if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
            {
                //联接串中要去掉这个关键词 Provider=SQLOLEDB.1;
                SqlConnection Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
                try
                {
                    Connection.Open();
                    SqlTransaction myTrans;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = Connection;
                    try
                    {
                        int li_tmp, i;
                        bool haverecord = false;//农合中有没有此hospid，false没有，则需要做insert的

                        #region //住院ID号查证

                        try
                        {
                            cmd.CommandText = "SELECT count(*) FROM hosppatinfo2 where hospitalcode=('" + hospitalcode + "') and HospID=(" + hospid + ")";
                            li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
                            if (li_tmp > 0)
                            {
                                haverecord = true;
                                cmd.CommandText = "SELECT chkpayid FROM hosppatinfo2 where hospitalcode=('" + hospitalcode + "') and HospID=(" + hospid + ")";
                                li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
                                if (li_tmp != 0)
                                {
                                    return if0.getXMLErroStrFromString("2200", "资料已审核，不能再上传");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            return if0.getXMLErroStrFromString("9000", "农合数据库操作异常：" + e.Message.ToString());
                        }
                        #endregion
                        #region 生成并执行住院病人相关SQL
                        try
                        {
                            for (i = 0; i < lds.Tables[0].Rows.Count; i++)
                            {
                                if (haverecord)
                                {
                                    #region 生成做UPDATE的SQL
                                    //未考虑lds.Tables[0].Rows[i][j] == null的情况

                                    ls_sql = "UPDATE hosppatinfo2 SET ";
                                    ls_sql += "HospCode='" + lds.Tables[0].Rows[i]["zyh"].ToString() + "'";
                                    ls_sql += ",PATIENTNAME='" + lds.Tables[0].Rows[i]["xm"].ToString() + "'";
                                    ls_sql += ",sex=" + lds.Tables[0].Rows[i]["xb"].ToString();
                                    ls_sql += ",BIRTH='" + lds.Tables[0].Rows[i]["sr"].ToString() + "'";
                                    ls_sql += ",BLOODTYPE='" + lds.Tables[0].Rows[i]["xx"].ToString() + "'";
                                    ls_sql += ",COUNTRY='" + lds.Tables[0].Rows[i]["gj"].ToString() + "'";
                                    ls_sql += ",NATIONALITY='" + lds.Tables[0].Rows[i]["mz"].ToString() + "'";
                                    ls_sql += ",NATIPLAC='" + lds.Tables[0].Rows[i]["jg"].ToString() + "'";
                                    ls_sql += ",MARRIAGE='" + lds.Tables[0].Rows[i]["hyzt"].ToString() + "'";
                                    ls_sql += ",HOMEADDR='" + lds.Tables[0].Rows[i]["jtzz"].ToString() + "'";
                                    ls_sql += ",HOMEPHON='" + lds.Tables[0].Rows[i]["dh"].ToString() + "'";
                                    ls_sql += ",HOMEZIP='" + lds.Tables[0].Rows[i]["yb"].ToString() + "'";
                                    ls_sql += ",Serial=" + lds.Tables[0].Rows[i]["zycs"].ToString();
                                    ls_sql += ",HospDate='" + lds.Tables[0].Rows[i]["rysj"].ToString() + "'";
                                    ls_sql += ",OutDate='" + lds.Tables[0].Rows[i]["cysj"].ToString() + "'";
                                    ls_sql += ",retr_price=" + lds.Tables[0].Rows[i]["yjj"].ToString();
                                    ls_sql += ",HospResu='" + lds.Tables[0].Rows[i]["zd"].ToString() + "'";
                                    ls_sql += ",patsort='" + lds.Tables[0].Rows[i]["lb"].ToString() + "'";
                                    ls_sql += ",HospPerf=" + lds.Tables[0].Rows[i]["cyzt"].ToString();
                                    ls_sql += ",PriceType=" + lds.Tables[0].Rows[i]["cyjfzt"].ToString();
                                    ls_sql += ",nopayment=" + lds.Tables[0].Rows[i]["wjsf"].ToString();
                                    ls_sql += ",nopaydate='" + lds.Tables[0].Rows[i]["tjjz"].ToString() + "'";
                                    ls_sql += ",nurselevel='" + lds.Tables[0].Rows[i]["hljb"].ToString() + "'";
                                    ls_sql += ",curstate=" + lds.Tables[0].Rows[i]["zyqk"].ToString();
                                    ls_sql += ",inpatientcode='" + lds.Tables[0].Rows[i]["fph"].ToString() + "'";
                                    ls_sql += ",grandPrice=" + lds.Tables[0].Rows[i]["zyf"].ToString();
                                    ls_sql += ",MEDINUMB='" + lds.Tables[0].Rows[i]["ylzh"].ToString() + "'";
                                    ls_sql += ",medinumb1='" + lds.Tables[0].Rows[i]["ylzh"].ToString() + "'";
                                    ls_sql += ",patientid='" + lds.Tables[0].Rows[i]["sfz"].ToString() + "'";
                                    ls_sql += " WHERE hospitalcode='" + hospitalcode + "' AND HospID=" + hospid;
                                    #endregion}
                                }
                                else
                                {
                                    #region 生成做INSERT的SQL
                                    //未考虑lds.Tables[0].Rows[i][j] == null的情况

                                    ls_sql = "INSERT INTO hosppatinfo2(hospitalcode,HospID,HospCode,PATIENTNAME,sex,";
                                    ls_sql += "BIRTH,BLOODTYPE,COUNTRY,NATIONALITY,NATIPLAC,";
                                    ls_sql += "MARRIAGE,HOMEADDR,HOMEPHON,HOMEZIP,Serial,";
                                    ls_sql += "HospDate,OutDate,retr_price,HospResu,patsort,";
                                    ls_sql += "HospPerf,PriceType,nopayment,nopaydate,nurselevel,";
                                    ls_sql += "curstate,inpatientcode,grandPrice,MEDINUMB,medinumb1,patientid)";
                                    ls_sql += " VALUES(";
                                    ls_sql += "'" + hospitalcode + "'";
                                    ls_sql += "," + hospid;
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["zyh"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["xm"].ToString() + "'";
                                    ls_sql += "," + lds.Tables[0].Rows[i]["xb"].ToString();
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["sr"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["xx"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["gj"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["mz"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["jg"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["hyzt"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["jtzz"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["dh"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["yb"].ToString() + "'";
                                    ls_sql += "," + lds.Tables[0].Rows[i]["zycs"].ToString();
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["rysj"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["cysj"].ToString() + "'";
                                    ls_sql += "," + lds.Tables[0].Rows[i]["yjj"].ToString();
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["zd"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["lb"].ToString() + "'";
                                    ls_sql += "," + lds.Tables[0].Rows[i]["cyzt"].ToString();
                                    ls_sql += "," + lds.Tables[0].Rows[i]["cyjfzt"].ToString();
                                    ls_sql += "," + lds.Tables[0].Rows[i]["wjsf"].ToString();
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["tjjz"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["hljb"].ToString() + "'";
                                    ls_sql += "," + lds.Tables[0].Rows[i]["zyqk"].ToString();
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["fph"].ToString() + "'";
                                    ls_sql += "," + lds.Tables[0].Rows[i]["zyf"].ToString();
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["ylzh"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["ylzh"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["sfz"].ToString() + "'";
                                    ls_sql += ")";
                                    #endregion
                                }
                                #region //保存住院病人记录
                                myTrans = Connection.BeginTransaction();
                                try
                                {
                                    cmd.Transaction = myTrans;
                                    cmd.CommandText = ls_sql;
                                    cmd.ExecuteNonQuery();
                                    myTrans.Commit();
                                }
                                catch (Exception e)
                                {
                                    myTrans.Rollback();
                                    return if0.getXMLErroStrFromString("9000", "HIS住院数据更新到农合时异常：" + e.Message.ToString());
                                }
                                finally
                                {
                                    myTrans.Dispose();
                                }
                                #endregion
                            }
                        }
                        catch (Exception e)
                        {
                            return if0.getXMLErroStrFromString("9000", "生成HIS住院数据更新到农合的语句时异常：" + e.Message.ToString());
                        }
                        #endregion
                    }
                    finally
                    {
                        cmd.Dispose();
                    }
                }
                catch (Exception e)
                {
                    return if0.getXMLErroStrFromString("9000", "建立新农合数据库联接异常：" + e.Message.ToString());
                }
                finally
                {
                    if (Connection.State == ConnectionState.Open)
                        Connection.Close();
                }
            }
            else
            {
                return if0.getXMLErroStrFromString("5000", dbConnStr);
            }
            #endregion
            retMessXML = if0.getXMLStrFromString("住院ID" + hospid + "的住院数据传输成功！");
        }
        return retMessXML;
    }
    
    /// <summary>
    /// HISonline接口调用获取住院病人费用数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <param name="hospid"></param>
    /// <param name="startdate"></param>
    /// <param name="enddate"></param>
    /// <returns></returns>
    public string putZYPatientFeeFromHIS(string areacode, string hospitalcode, string servername, string dbname, string user, string pwd, string hospid, string startdate, string enddate)
    {
        string hisifurl = "", retMessXML = "", dbConnStr = "", ls_sql = "";
        if (!if0.getWebServiceURL(servername, out hisifurl))
        {
            return if0.getXMLErroStrFromString("2000", hisifurl);
        }

        int li_tmp, i;
        bool haverecord = false;//农合库中有没有此id，false没有，则需要做insert的

        SqlCommand cmd;
        SqlConnection Connection;
        #region 查询数据库，住院ID号查证

        if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
        {
            //联接串中要去掉这个关键词 Provider=SQLOLEDB.1;
            Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
            try
            {
                Connection.Open();
                cmd = new SqlCommand();
                cmd.Connection = Connection;
                #region //住院ID号查证

                try
                {
                    try
                    {
                        cmd.CommandText = "SELECT count(*) FROM hosppatinfo2 where hospitalcode=('" + hospitalcode + "') and HospID=(" + hospid + ")";
                        li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
                        if (li_tmp > 0)
                        {
                            haverecord = true;
                            cmd.CommandText = "SELECT chkpayid FROM hosppatinfo2 where hospitalcode=('" + hospitalcode + "') and HospID=(" + hospid + ")";
                            li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
                            if (li_tmp != 0)
                            {
                                return if0.getXMLErroStrFromString("2200", "资料已审核，不能再上传");
                            }
                        }
                        else
                        {
                            return if0.getXMLErroStrFromString("2200", "还没有上传病人资料");
                        }
                    }
                    catch (Exception e)
                    {
                        return if0.getXMLErroStrFromString("9000", "农合数据库操作异常：" + e.Message.ToString());
                    }
                }
                catch (Exception e)
                {
                    return if0.getXMLErroStrFromString("9000", "生成HIS住院数据更新到农合的语句时异常：" + e.Message.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
                #endregion
            }
            catch (Exception e)
            {
                return if0.getXMLErroStrFromString("9000", "建立新农合数据库联接异常：" + e.Message.ToString());
            }
            finally
            {
                if (Connection.State == ConnectionState.Open)
                    Connection.Close();
            }
        }
        else
        {
            return if0.getXMLErroStrFromString("5000", dbConnStr);
        }
        #endregion

        DataSet lds = new DataSet();
        ls_sql = "select p.detailid as fyid,";
        ls_sql += "isnull((select nhdm from " + dbname + "..nhppb n where n.auditing=1 and n.mediid=p.mediid),'未匹配') as nhdm,";
        ls_sql += "p.mediid as hismlid,";
        ls_sql += "isnull((select officename from " + dbname + "..dictoffice d where d.officeid=p.Execoffice),'其它') as zxks,";
        ls_sql += "isnull((select officename from " + dbname + "..dictoffice d where d.officeid=p.RequOffiID),'其它') as bqks,";
        ls_sql += "isnull((select opername from " + dbname + "..dictoper o where o.operid= p.doctid),'未知') as zrys,";
        ls_sql += "isnull((select opername from " + dbname + "..dictoper o where o.operid= p.operid1),'未知') as zrhs,";
        ls_sql += "(select (case item when 0 then '0' else '1' end) from " + dbname + "..dictmedi m where m.mediid=p.mediid) as lx,";
        ls_sql += "(select mediname from " + dbname + "..dictmedi m where m.mediid=p.mediid) as xmmc,";
        ls_sql += "convert(varchar(19),ddate,120) as fssj,date3 as qrrq,";
        ls_sql += "isnull((select (case when ISNUMERIC( theoquan )>0  then theoquan else '0' end) from " + dbname + "..doctmark k where k.autonumb = p.autonumb),'0') as yzjl,";
        ls_sql += "isnull((select unit from " + dbname + "..doctmark k where k.autonumb = p.autonumb),'')  as yzdw,";
        ls_sql += "isnull((select substring(t.hz,1,8) from " + dbname + "..doctmark k," + dbname + "..DictTimes t where k.autonumb = p.autonumb and t.timeid= k.timeid),'') as pl,";
        ls_sql += "user_num as yl,";
        ls_sql += "isnull((select (case when ISNUMERIC( theoquan )>0  then theoquan else '0' end) from " + dbname + "..doctmark m where m.autonumb = p.autonumb),'0') as jl,";
        ls_sql += " user_num as sl,unitprice as dj,price as je,";
        ls_sql += "isnull((select opername from " + dbname + "..dictoper o where o.operid= p.operid3),'未核对') as hdr";
        ls_sql += " from " + dbname + "..hosp_detail_price p where hospid=" + hospid + " and isconfirm = 1 and ischeck =1 and isprice =1 and date3 between '" + startdate + "' and '" + enddate + "'";
        if (if0.getDataFromHIS(servername, dbname, user, pwd, ls_sql, out retMessXML, out lds))
        {
            //调用成功，处理传出的DataSet
            retMessXML = "";
            #region 查询数据库Insert或Update数据
            if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
            {
                try
                {
                    Connection.Open();
                    try
                    {
                        SqlTransaction myTrans;
                        #region 生成并执行住院病人费用相关SQL
                        try
                        {
                            for (i = 0; i < lds.Tables[0].Rows.Count; i++)
                            {
                                #region //费用ID号存在性验证

                                try
                                {
                                    haverecord = false;
                                    cmd.CommandText = "SELECT count(*) FROM pricedetail where hospitalcode=('" + hospitalcode + "') and HospID=(" + hospid + ") and detailid=(" + lds.Tables[0].Rows[i]["fyid"].ToString() + ")";
                                    li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
                                    if (li_tmp > 0)
                                    {
                                        haverecord = true;
                                    }
                                }
                                catch (Exception e)
                                {
                                    return if0.getXMLErroStrFromString("9000", "农合数据库费用操作异常：" + e.Message.ToString());
                                }
                                #endregion
                                if (haverecord)
                                {
                                    #region 生成做UPDATE的SQL
                                    //未考虑lds.Tables["patient"].Rows[i][j] == null的情况

                                    ls_sql = "UPDATE pricedetail SET ";
                                    ls_sql += "nhcode='" + lds.Tables[0].Rows[i]["nhdm"].ToString() + "'";
                                    ls_sql += ",Execoffice='" + lds.Tables[0].Rows[i]["zxks"].ToString() + "'";
                                    ls_sql += ",RequOffice='" + lds.Tables[0].Rows[i]["bqks"].ToString() + "'";
                                    ls_sql += ",doctname='" + lds.Tables[0].Rows[i]["zrys"].ToString() + "'";
                                    ls_sql += ",opername='" + lds.Tables[0].Rows[i]["zrhs"].ToString() + "'";
                                    ls_sql += ",MarkType=" + lds.Tables[0].Rows[i]["lx"].ToString();
                                    ls_sql += ",mediname='" + lds.Tables[0].Rows[i]["xmmc"].ToString() + "'";
                                    ls_sql += ",ddate='" + lds.Tables[0].Rows[i]["fssj"].ToString() + "'";
                                    ls_sql += ",date2='" + lds.Tables[0].Rows[i]["qrrq"].ToString() + "'";
                                    ls_sql += ",date3='" + lds.Tables[0].Rows[i]["qrrq"].ToString() + "'";
                                    ls_sql += ",theoQuan='" + lds.Tables[0].Rows[i]["yzjl"].ToString() + "'";
                                    ls_sql += ",unit='" + lds.Tables[0].Rows[i]["yzdw"].ToString() + "'";
                                    ls_sql += ",timeid='" + lds.Tables[0].Rows[i]["pl"].ToString() + "'";
                                    ls_sql += ",num1=" + lds.Tables[0].Rows[i]["yl"].ToString();
                                    ls_sql += ",num2=" + lds.Tables[0].Rows[i]["jl"].ToString();
                                    ls_sql += ",user_num=" + lds.Tables[0].Rows[i]["sl"].ToString();
                                    ls_sql += ",unitprice=" + lds.Tables[0].Rows[i]["dj"].ToString();
                                    ls_sql += ",price=" + lds.Tables[0].Rows[i]["je"].ToString();
                                    ls_sql += ",operid1='" + lds.Tables[0].Rows[i]["hdr"].ToString() + "'";
                                    ls_sql += ",mediid=" + lds.Tables[0].Rows[i]["hismlid"].ToString();
                                    ls_sql += ",nhcodebak='" + lds.Tables[0].Rows[i]["nhdm"].ToString() + "'";
                                    ls_sql += " WHERE hospitalcode='" + hospitalcode + "' AND HospID=" + hospid + "AND detailid=" + lds.Tables[0].Rows[i]["fyid"].ToString();
                                #endregion
                                }
                                else
                                {
                                    #region 生成做INSERT的SQL
                                    //未考虑lds.Tables["patient"].Rows[i][j] == null的情况

                                    ls_sql = "INSERT INTO pricedetail(hospitalcode,HospID,detailid,nhcode,Execoffice,";
                                    ls_sql += "RequOffice,doctname,opername,MarkType,mediname,";
                                    ls_sql += "ddate,date2,date3,theoQuan,unit,";
                                    ls_sql += "timeid,num1,num2,user_num,unitprice,";
                                    ls_sql += "price,operid1,chkpayid,mediid,nhcodebak)";
                                    ls_sql += " VALUES(";
                                    ls_sql += "'" + hospitalcode + "'";
                                    ls_sql += "," + hospid;
                                    ls_sql += "," + lds.Tables[0].Rows[i]["fyid"].ToString();
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["nhdm"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["zxks"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["bqks"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["zrys"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["zrhs"].ToString() + "'";
                                    ls_sql += "," + lds.Tables[0].Rows[i]["lx"].ToString();
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["xmmc"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["fssj"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["qrrq"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["qrrq"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["yzjl"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["yzdw"].ToString() + "'";
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["pl"].ToString() + "'";
                                    ls_sql += "," + lds.Tables[0].Rows[i]["yl"].ToString();
                                    ls_sql += "," + lds.Tables[0].Rows[i]["jl"].ToString();
                                    ls_sql += "," + lds.Tables[0].Rows[i]["sl"].ToString();
                                    ls_sql += "," + lds.Tables[0].Rows[i]["dj"].ToString();
                                    ls_sql += "," + lds.Tables[0].Rows[i]["je"].ToString();
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["hdr"].ToString() + "'";
                                    ls_sql += ",0";
                                    ls_sql += "," + lds.Tables[0].Rows[i]["hismlid"].ToString();
                                    ls_sql += ",'" + lds.Tables[0].Rows[i]["nhdm"].ToString() + "'";
                                    ls_sql += ")";
                                    #endregion
                                }
                                #region //保存住院病人费用记录
                                myTrans = Connection.BeginTransaction();
                                try
                                {
                                    cmd.Transaction = myTrans;
                                    cmd.CommandText = ls_sql;
                                    cmd.ExecuteNonQuery();
                                    myTrans.Commit();
                                }
                                catch (Exception e)
                                {
                                    myTrans.Rollback();
                                    return if0.getXMLErroStrFromString("9000", "HIS住院费用数据更新到农合时异常：" + e.Message.ToString());
                                }
                                finally
                                {
                                    myTrans.Dispose();
                                }
                                #endregion
                            }
                        }
                        catch (Exception e)
                        {
                            return if0.getXMLErroStrFromString("9000", "生成HIS住院数据更新到农合的语句时异常：" + e.Message.ToString());
                        }
                        #endregion
                    }
                    finally
                    {
                        cmd.Dispose();
                    }
                }
                catch (Exception e)
                {
                    return if0.getXMLErroStrFromString("9000", "建立新农合数据库联接异常：" + e.Message.ToString());
                }
                finally
                {
                    if (Connection.State == ConnectionState.Open)
                        Connection.Close();
                }
            }
            else
            {
                return if0.getXMLErroStrFromString("5000", dbConnStr);
            }
            #endregion
            retMessXML = if0.getXMLStrFromString("住院ID" + hospid + "从" + startdate + "至" + enddate + "的住院费用数据传输成功！");
        }
        return retMessXML;
    }
    #endregion

    #region //住院审核和补偿查询操作

    /// <summary>
    /// 计算指定住院病人的每条住院费用的保内保外标志、费用分类、应补金额、预审金额

    /// </summary>
    /// <param name="hospitalcode"></param>
    /// <param name="hospid"></param>
    /// <param name="hosplevel"></param>
    /// <param name="chkpayid"></param>
    /// <param name="operer"></param>
    /// <param name="settle"></param>
    /// <param name="dataDS"></param>
    /// <param name="feeDs"></param>
    /// <param name="mess"></param>
    /// <returns></returns>
    public bool zyCalcCompensate(string hospitalcode, string hospid, string hosplevel, Int32 chkpayid, string operer, Int32 settle, DataSet dataDS, out DataSet feeDs, out string mess)
    {
        feeDs = dataDS;
        mess = "TRUE";
        #region //参数有效性检查

        if (chkpayid != 8 && chkpayid != 1 && chkpayid != 3)
        {
            mess = "请求的审核状态未定义处理方法";
            return false;
        }
        if (hosplevel != "1" && hosplevel != "2" && hosplevel != "3")
        {
            mess = "请与系统管理员联系，检查医疗机构代码中的医院等级数据！";
            return false;
        }
        if (operer.Length == 0)
        {
            operer = UserID;
        }
        #endregion
        #region //变量定义
        string ls_nhcode, ls_typecode, ls_content, ls_excepted, ls_remark, ls_mediname, ls_mediname_pp;
        Int32 li_matchs = 0, li_rcmssershad = 0, li_rcmsmedishad = 0, li_row = -1, li_class = 0;
        decimal ld_user_num, ld_unitprice, ld_price,ld_mediid;
        decimal ld_nhunitprice = 0.0M, ld_nhrate = 0.0M, ld_nhprice = 0.0M, ld_selffee = 0.0M;
        DateTime ldt_now;
        #endregion
        #region //得到病人的住院费用信息

        string ls_sql;
        if (settle == 1)
        {
            ls_sql = "SELECT hospitalcode,HospID,detailid,nhcode,Execoffice,RequOffice,doctname,opername,MarkType,mediname,";
            ls_sql +="ddate,date2,date3,theoQuan,unit,timeid,num1,num2,user_num,unitprice,";
            ls_sql +="price,operid1,selffee,autochkfee,chkinitfee,chkinitopr,chkinittime,chkagainfee,chkagainopr,chkagaintime,";
            ls_sql +="checkid,in_out,prechkfee,prechkopr,prechktime,chkpayid,nhcodebak,mediid";
            ls_sql += " FROM pricedetail";
            ls_sql += " WHERE hospitalcode='" + hospitalcode + "' AND HospID=" + hospid;
            ls_sql += " ORDER BY nhcode ASC,mediname ASC,detailid ASC";
            //DataSet feeDs = null;
            feeDs = if0.getDataSet(ls_sql, "fees", out mess);
            if (mess != "TRUE")
            {
                feeDs.Dispose();
                return false;
            }
        }
        #endregion
        SqlConnection Connection = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
        SqlCommand cmd = new SqlCommand();
        SqlDataReader myReader;
        try
        {
            Connection.Open();
            cmd.Connection = Connection;
            if (chkpayid == 8) //审核只是在平台调用，在前台界面中手工修改保内额保存

            {
                #region //智能计算把医院上传明细名称修改了的当“未匹配”处理的参数获取
                try
                {
                    cmd.CommandText = "SELECT matchs,rcmssershad,rcmsmedishad,getdate() FROM areasysparm WHERE areacode='" + AreaCode + "'";
                    myReader = cmd.ExecuteReader();
                    try
                    {
                        if (!myReader.HasRows)
                        {
                            mess = "智能计算保内金额时没有查询到系统参数";
                            return false;
                        }
                        else
                        {
                            myReader.Read();
                            li_matchs = myReader.GetInt32(0);
                            li_rcmssershad = myReader.GetInt32(1);
                            li_rcmsmedishad = myReader.GetInt32(2);
                            ldt_now = myReader.GetDateTime(3);
                        }
                    }
                    finally
                    {
                        if (!myReader.IsClosed)
                            myReader.Close();
                        myReader.Dispose();
                    }
                }
                catch (Exception e)
                {
                    mess = "查询系统参数异常：" + e.Message.ToString();
                    return false;
                }
                finally
                {
                    cmd.Dispose();
                }
                #endregion
                #region //每条费用循环计算
                ld_nhunitprice = 0.0M;
                mess = "";
                foreach (DataRow currrow in feeDs.Tables["fees"].Rows)
                {
                    li_row += 1;//记录当前行号
                    #region //已是chkpayid状态或chkpayid状态之后的不处理

                    if (chkpayid == Convert.ToDecimal(currrow["chkpayid"]) || chkpayid == 8 && Convert.ToDecimal(currrow["chkpayid"]) != 0 || chkpayid == 1 && Convert.ToDecimal(currrow["chkpayid"]) != 8 || chkpayid == 3 && Convert.ToDecimal(currrow["chkpayid"]) != 1)
                    {
                        continue;
                    }
                    #endregion
                    #region //数量、金额、并计算单价(中药的金额不等于单价×数量，故单价使用反算)
                    ld_user_num = Convert.ToDecimal(currrow["user_num"]);
                    ld_price = Convert.ToDecimal(currrow["price"]);
                    if (ld_user_num == 0.0M)
                    {
                        ld_unitprice = Convert.ToDecimal(currrow["unitprice"]);
                    }
                    else
                    {
                        ld_unitprice = ld_price / ld_user_num;
                    }
                    #endregion
                    #region //当前行与上行数据相同的，复制上行的审核结果。其中包括“未匹配”的也复制

                    if (li_row > 0 && currrow["nhcode"].ToString() == feeDs.Tables["fees"].Rows[li_row - 1]["nhcode"].ToString() && currrow["mediname"].ToString() == feeDs.Tables["fees"].Rows[li_row - 1]["mediname"].ToString() && Convert.ToDecimal(currrow["unitprice"]) == Convert.ToDecimal(feeDs.Tables["fees"].Rows[li_row - 1]["unitprice"]))
                    {
                        currrow.BeginEdit();
                        currrow["nhcode"] = feeDs.Tables["fees"].Rows[li_row - 1]["nhcode"].ToString();
                        //currrow["checkid"] = Convert.ToInt32(feeDs.Tables["fees"].Rows[li_row - 1]["checkid"]);
                        currrow["in_out"] = Convert.ToInt32(feeDs.Tables["fees"].Rows[li_row - 1]["in_out"]);
                        currrow["marktype"] = Convert.ToInt32(feeDs.Tables["fees"].Rows[li_row - 1]["marktype"]);
                        //currrow["chkpayid"] = chkpayid;//Convert.ToInt32(feeDs.Tables["fees"].Rows[li_row - 1]["chkpayid"]);
                        currrow["checkid"] = 2;
                        if (ld_nhrate == 0.0M)
                        {
                            currrow["in_out"] = 1;
                            currrow["checkid"] = 6;
                            ld_nhprice = 0.0M;
                        }
                        else
                        {
                            currrow["in_out"] = 0;
                            if (ld_nhunitprice < 0) //不限价

                            {
                                ld_nhprice = ld_unitprice * ld_user_num * ld_nhrate;
                                currrow["checkid"] = 2; //2正常
                            }
                            else if (ld_nhunitprice == 0.0M) //农合标准中限价为0的

                            {
                                ld_nhprice = 0.0M;
                                currrow["in_out"] = 1;
                                currrow["checkid"] = 6; //6不可报

                            }
                            else if (ld_nhunitprice == ld_unitprice) //恰好＝政策规定价格的
                            {
                                ld_nhprice = ld_unitprice * ld_user_num * ld_nhrate;
                                currrow["checkid"] = 2; //2正常
                            }
                            else if (ld_unitprice < ld_nhunitprice)
                            {
                                ld_nhprice = ld_unitprice * ld_user_num * ld_nhrate;
                                currrow["checkid"] = 3; //3单价低

                            }
                            else
                            {
                                ld_nhprice = ld_nhunitprice * ld_user_num * ld_nhrate;
                                currrow["checkid"] = 4; //4单价高

                            }
                        }
                        ld_selffee = Convert.ToDecimal(currrow["price"]) - ld_nhprice;
                        //设置其审核结果

                        currrow["selffee"] = ld_selffee;
                        currrow["autochkfee"] = ld_nhprice;
                        if (chkpayid == 8)
                        {
                            currrow["prechkfee"] = ld_nhprice;
                            currrow["prechkopr"] = operer;
                            currrow["prechktime"] = ldt_now;
                            currrow["chkinitfee"] = ld_nhprice;
                            currrow["chkagainfee"] = ld_nhprice;
                        }
                        else if (chkpayid == 1)
                        {
                            currrow["chkinitfee"] = ld_nhprice;
                            currrow["chkinitopr"] = operer;
                            currrow["chkinittime"] = ldt_now;
                            currrow["chkagainfee"] = ld_nhprice;
                        }
                        else if (chkpayid == 3)
                        {
                            currrow["chkagainfee"] = ld_nhprice;
                            currrow["chkagainopr"] = operer;
                            currrow["chkagaintime"] = ldt_now;
                        }
                        currrow["chkpayid"] = chkpayid;
                        currrow.EndEdit();
                        continue;
                    }
                    #endregion
                    ld_nhunitprice = 0.0M;
                    #region //修改了匹配时名称的按“未匹配”记——这个功能暂时不需要

                    if (li_matchs == 0)
                    {
                        ls_mediname = currrow["mediname"].ToString();
                        ld_mediid = Convert.ToDecimal(currrow["mediid"]);
                        cmd.Connection = Connection;
                        try
                        {
                            cmd.CommandText = "SELECT mediname FROM nhppb WHERE orgcode='" + hospitalcode + "'";
                            cmd.CommandText += " AND mediid=" + ld_mediid.ToString() + "  AND Auditing>0";
                            ls_mediname_pp = Convert.ToString(cmd.ExecuteScalar());
                            if (ls_mediname_pp == null || ls_mediname_pp.Length == 0)
                            {
                                //currrow.BeginEdit();
                                //currrow["nhcode"] = "未匹配";
                                //currrow.EndEdit();
                                ////continue;
                                mess = "上传的费用" + ls_mediname + "(fyid=" + currrow["detailid"].ToString() + ",mediid" + ld_mediid.ToString() + ")没有已审核的匹配数据，计算未完成";
                                return false;
                            }
                            else
                            {
                                if (ls_mediname_pp.IndexOf(ls_mediname) == 0)
                                {
                                    ////插入到log_hosp_mediname表中
                                    ////记录该行审核结果
                                    //currrow.BeginEdit();
                                    //currrow["nhcode"] = "未匹配";
                                    //currrow.EndEdit();
                                    ////continue;
                                    mess = "上传的费用" + ls_mediname + "(fyid=" + currrow["detailid"].ToString() + ",mediid" + ld_mediid.ToString() + ")匹配时的名称为:" + ls_mediname_pp + "，匹配关系不再成立，计算未完成";
                                    return false;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            mess = "查询匹配数据异常：" + e.Message.ToString();
                            return false;
                        }
                        finally
                        {
                            cmd.Dispose();
                        }
                    }
                    #endregion
                    ls_nhcode = currrow["nhcode"].ToString();
                    #region 农合代码是“未匹配”的直接记录审核结果
                    if (ls_nhcode == null || ls_nhcode.Length == 0 || ls_nhcode == "未匹配")
                    {
                        mess = "计算发现有未匹配的费用(fyid=" + currrow["detailid"].ToString() + ")，计算未完成";
                        return false;
                        //currrow.BeginEdit();
                        //currrow["nhcode"] = "未匹配";
                        //currrow["checkid"] = 7;
                        //currrow["in_out"] = 1;
                        ////currrow["marktype"] = 2;
                        ////currrow["chkpayid"] = 8;
                        //currrow["selffee"] = currrow["price"];
                        //currrow["autochkfee"] = 0.0;
                        //if (chkpayid == 8)
                        //{
                        //    currrow["prechkfee"] = 0.0;
                        //    currrow["prechkopr"] = operer;
                        //    currrow["prechktime"] = ldt_now;
                        //    currrow["chkinitfee"] = 0.0;
                        //}
                        //else if (chkpayid == 1)
                        //{
                        //    currrow["chkinitfee"] = 0.0;
                        //    currrow["chkinitopr"] = operer;
                        //    currrow["chkinittime"] = ldt_now;
                        //}
                        //currrow.EndEdit();
                        //continue;
                    }
                    #endregion
                    #region //农合代码不是有效目录中的代码的直接记录审核结果，否则获取补偿比率和补偿单价并计算、设置保内保外标志

                    try
                    {
                        cmd.CommandText = "SELECT typecode,codetype,rate1,price1,rate2,price2,rate3,price3,content,excepted,remark FROM rcmssermediarea WHERE areacode='" + AreaCode + "' AND nhcode='" + ls_nhcode + "'";
                        myReader = cmd.ExecuteReader();
                        try
                        {
                            if (!myReader.HasRows)
                            {
                                mess = "上传的农合代码(nhdm=" + ls_nhcode + ")不是有效的目录中的代码";
                                return false;
                                //currrow.BeginEdit();
                                //currrow["nhcode"] = "未匹配";
                                //currrow["checkid"] = 1;
                                //currrow["in_out"] = 1;
                                ////currrow["marktype"] = 2;
                                ////currrow["chkpayid"] = 8;
                                //currrow["selffee"] = currrow["price"];
                                //currrow["autochkfee"] = 0.0;
                                //if (chkpayid == 8)
                                //{
                                //    currrow["prechkfee"] = 0.0;
                                //    currrow["prechkopr"] = operer;
                                //    currrow["prechktime"] = ldt_now;
                                //    currrow["chkinitfee"] = 0.0;
                                //}
                                //else if (chkpayid == 1)
                                //{
                                //    currrow["chkinitfee"] = 0.0;
                                //    currrow["chkinitopr"] = operer;
                                //    currrow["chkinittime"] = ldt_now;
                                //}
                                //currrow.EndEdit();
                                //continue;
                            }
                            else
                            {
                                myReader.Read();
                                ls_typecode = myReader.GetString(0);
                                li_class = myReader.GetInt32(1);
                                ls_content = myReader.GetString(8);
                                ls_excepted = myReader.GetString(9);
                                ls_remark = myReader.GetString(10);
                                if (hosplevel == "1")
                                {
                                    ld_nhrate = myReader.GetDecimal(2);
                                    ld_nhunitprice = myReader.GetDecimal(3);
                                }
                                else if (hosplevel == "2")
                                {
                                    ld_nhrate = myReader.GetDecimal(4);
                                    ld_nhunitprice = myReader.GetDecimal(5);
                                }
                                else if (hosplevel == "3")
                                {
                                    ld_nhrate = myReader.GetDecimal(6);
                                    ld_nhunitprice = myReader.GetDecimal(7);
                                }
                                else
                                {
                                    mess = "医院等级错误，请联系农合系统管理员";
                                    return false;
                                }
                                currrow["checkid"] = 2;
                                if (ld_nhrate == 0.0M)
                                {
                                    currrow["in_out"] = 1;
                                    currrow["checkid"] = 6;
                                    ld_nhprice = 0.0M;
                                }
                                else
                                {
                                    if (ld_nhunitprice < 0) //不限价

                                    {
                                        ld_nhprice = ld_unitprice * ld_user_num * ld_nhrate;
                                        currrow["checkid"] = 2; //2正常
                                    }
                                    else if (ld_nhunitprice == 0.0M) //农合标准中限价为0的

                                    {
                                        ld_nhprice = 0.0M;
                                        currrow["in_out"] = 1;
                                        currrow["checkid"] = 6; //6不可报

                                    }
                                    else if (ld_nhunitprice == ld_unitprice) //恰好＝政策规定价格的
                                    {
                                        ld_nhprice = ld_unitprice * ld_user_num * ld_nhrate;
                                        currrow["checkid"] = 2; //2正常
                                    }
                                    else if (ld_unitprice < ld_nhunitprice)
                                    {
                                        ld_nhprice = ld_unitprice * ld_user_num * ld_nhrate;
                                        currrow["checkid"] = 3; //3单价低

                                    }
                                    else
                                    {
                                        ld_nhprice = ld_nhunitprice * ld_user_num * ld_nhrate;
                                        currrow["checkid"] = 4; //4单价高

                                    }
                                }
                            }
                        }
                        finally
                        {
                            if (!myReader.IsClosed)
                                myReader.Close();
                            myReader.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        mess = "查询有效农合目录异常：" + e.Message.ToString();
                        return false;
                    }
                    finally
                    {
                        cmd.Dispose();
                    }
                    #endregion
                    #region //计算费用类别
                    if (li_class == 0) //0医疗服务，1药品字典
                    {
                        currrow["marktype"] = 2;
                    }
                    else
                    {
                        switch (ls_typecode.Substring(0, 1))
                        {
                            case "1":
                                currrow["marktype"] = 0;//药品
                                break;
                            case "2":
                                currrow["marktype"] = 0;//药品
                                break;
                            case "3":
                                currrow["marktype"] = 0;//药品
                                break;
                            case "4":
                                currrow["marktype"] = 1;//材料
                                break;
                            default:
                                currrow["marktype"] = 2;//医疗服务
                                break;
                        }
                    }
                    #endregion
                    #region //设置检查结果标志的取值计算“5可疑”需要根据系统参数计算

                    if (li_class == 0) //医疗服务的

                    {
                        switch (li_rcmssershad)
                        {
                            case 0:
                                break;
                            case 1:
                                if (!(ls_content == null || ls_content.Length == 0))
                                {
                                    currrow["checkid"] = 5;
                                }
                                break;
                            case 2:
                                if (!(ls_excepted == null || ls_excepted.Length == 0))
                                {
                                    currrow["checkid"] = 5;
                                }
                                break;
                            case 3:
                                if (!(ls_remark == null || ls_remark.Length == 0))
                                {
                                    currrow["checkid"] = 5;
                                }
                                break;
                            case 4:
                                if (!(ls_content == null || ls_content.Length == 0 || ls_excepted == null || ls_excepted.Length == 0))
                                {
                                    currrow["checkid"] = 5;
                                }
                                break;
                            case 5:
                                if (!(ls_content == null || ls_content.Length == 0 || ls_remark == null || ls_remark.Length == 0))
                                {
                                    currrow["checkid"] = 5;
                                }
                                break;
                            case 6:
                                if (!(ls_excepted == null || ls_excepted.Length == 0 || ls_remark == null || ls_remark.Length == 0))
                                {
                                    currrow["checkid"] = 5;
                                }
                                break;
                            case 7:
                                if (!(ls_content == null || ls_content.Length == 0 || ls_excepted == null || ls_excepted.Length == 0 || ls_remark == null || ls_remark.Length == 0))
                                {
                                    currrow["checkid"] = 5;
                                }
                                break;
                        }
                    }
                    else //药品字典
                    {
                        switch (li_rcmsmedishad)
                        {
                            case 0:
                                break;
                            case 1:
                                if (!(ls_excepted == null || ls_excepted.Length == 0))
                                {
                                    currrow["checkid"] = 5;
                                }
                                break;
                            case 2:
                                if (!(ls_content == null || ls_content.Length == 0))
                                {
                                    currrow["checkid"] = 5;
                                }
                                break;
                            case 3:
                                if (!(ls_excepted == null || ls_excepted.Length == 0 || ls_content == null || ls_content.Length == 0))
                                {
                                    currrow["checkid"] = 5;
                                }
                                break;
                        }
                    }
                    #endregion
                    ld_selffee = Convert.ToDecimal(currrow["price"]) - ld_nhprice;
                    #region //设置审核结果到数据集
                    currrow["selffee"] = ld_selffee;
                    currrow["autochkfee"] = ld_nhprice;
                    if (chkpayid == 8)
                    {
                        currrow["prechkfee"] = ld_nhprice;
                        currrow["prechkopr"] = operer;
                        currrow["prechktime"] = ldt_now;
                        currrow["chkinitfee"] = ld_nhprice;
                        currrow["chkagainfee"] = ld_nhprice;
                    }
                    else if (chkpayid == 1)
                    {
                        currrow["chkinitfee"] = ld_nhprice;
                        currrow["chkinitopr"] = operer;
                        currrow["chkinittime"] = ldt_now;
                        currrow["chkagainfee"] = ld_nhprice;
                    }
                    else if (chkpayid == 3)
                    {
                        currrow["chkagainfee"] = ld_nhprice;
                        currrow["chkagainopr"] = operer;
                        currrow["chkagaintime"] = ldt_now;
                    }
                    currrow["chkpayid"] = chkpayid;
                    #endregion
                    //#region //存在未匹配的，则中止计算
                    //if (feeDs.Tables["fees"].Rows.Find("未匹配") != null)
                    //{
                    //    mess = "计算发现有未匹配的费用，计算未完成";
                    //    return false;
                    //}
                    //#endregion
                }
                #endregion
            }
            if (settle == 1)
            {
                #region //全部预审完成，则更新数据集到数据库

                SqlTransaction myTrans;
                myTrans = Connection.BeginTransaction();
                try
                {
                    cmd.Transaction = myTrans;
                    if (chkpayid ==8) //审核只是在平台调用，在前台界面中手工修改保内额保存

                    {
                        foreach (DataRow row in feeDs.Tables["fees"].Rows)
                        {
                            cmd.CommandText = "UPDATE pricedetail SET";
                            cmd.CommandText += " selffee=" + row["selffee"].ToString() + ",autochkfee=" + row["autochkfee"].ToString();
                            cmd.CommandText += ",prechkfee=" + row["prechkfee"].ToString() + ",prechkopr='" + row["prechkopr"] + "'";
                            cmd.CommandText += ",prechktime='" + row["prechktime"].ToString() + "',chkinitfee=" + row["chkinitfee"].ToString();
                            cmd.CommandText += ",chkinitopr='" + row["chkinitopr"] + "',chkinittime='" + row["chkinittime"].ToString() + "'";
                            cmd.CommandText += ",chkagainfee='" + row["chkagainfee"].ToString() + "',chkagainopr='" + row["chkagainopr"] + "'";
                            cmd.CommandText += ",chkagaintime='" + row["chkagaintime"].ToString() + "'";
                            cmd.CommandText += ",chkpayid=" + row["chkpayid"].ToString() + ",checkid=" + row["checkid"].ToString();
                            cmd.CommandText += ",marktype=" + row["marktype"].ToString() + ",nhcode='" + row["nhcode"] + "'";
                            cmd.CommandText += ",in_out=" + row["in_out"].ToString();
                            cmd.CommandText += " WHERE hospitalcode='" + hospitalcode + "' AND HospID=" + hospid + " AND detailid=" + row["detailid"].ToString();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    myTrans.Commit();
                }
                catch (Exception e)
                {
                    myTrans.Rollback();
                    mess = "更新费用审核数据异常:" + e.Message.ToString();
                    return false;
                }
                finally
                {
                    myTrans.Dispose();
                }
                #endregion
            }
            mess = "TRUE";
            return true;
        }
        catch (Exception e)
        {
            mess = "异常:" + e.Message.ToString();
            return false;
        }
        finally
        {
            cmd.Dispose();
            feeDs.Dispose();
            //dataDS.Dispose();
            if (Connection.State == ConnectionState.Open)
                Connection.Close();
        }
    }

    public string settleZY(string hospitalcode,decimal hospid,Int32 chkpayid, string opercode, string opername, Int32 settle, string dataXML, out string mess)
    {
        string sqltext;
        DataSet hospset = null;
        SqlCommand sqlcomm = new SqlCommand();
        SqlDataReader sqldr = null;
        XmlDocument xmldoc = new XmlDocument();
        #region //参数有效性检查

        if (chkpayid != 8)// && chkpayid != 1)//&&chkpayid!=3
        {
            mess = if0.getXMLErroStrFromString("2000", "请求的审核状态未定义处理方法");
            return mess;
        }
        if (settle != 0 && settle != 1)//0为试算，1为正式结算
        {
            mess = if0.getXMLErroStrFromString("2000", "请求的结算方式未定义");
            return mess;
        }
        //当settle=0，请求试算时，dataXML不能为""，在调用本方法时已验证过了

        if (opername.Length == 0)
        {
            opername = UserID;
        }
        #endregion

        SqlConnection Connection = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
        Connection.Open();
        try
        {
            if (settle == 1)
            #region 正式结算 判断是否可进行补偿
            {
                Int32 ifprechk, prechkdays, prechkdays1, prechkdays2, rehosp, excepmode;
                Int32 outdays, gonextstate, dataresource, curentchkpayid, hospperf, lockstate, patstate, age, hosptolevel;
                DateTime outdate, currentdate, hospdate,ddate;
                string nopaydate, hospresu, s_hospdate, familyno, memberno;
                #region 可补偿有效性判断

                #region 获取医疗信息
                sqltext = "Select outdate,nopaydate,gonextstate,dataresource,chkpayid,";
                sqltext += "hospperf,hospresu,Convert(VarChar(8),hospdate,112),medinumb,patientid, ";
                sqltext += "lockstate,patstate,age,hosptolevel,hospdate ";
                sqltext +="From hosppatinfo2 Where hospitalcode = '" + hospitalcode + "' And hospid = " + hospid;
                sqlcomm.CommandText = sqltext;
                sqlcomm.Connection = Connection;
                sqldr = sqlcomm.ExecuteReader();
                if (!sqldr.HasRows)
                {
                    mess = if0.getXMLErroStrFromString("9000", "没有查询到相应的医疗信息，请检查是否已上传了该医疗信息。");
                    return mess;
                }
                try
                {
                    sqldr.Read();
                    outdate = sqldr.GetDateTime(0);
                    nopaydate = sqldr.GetString(1);
                    gonextstate = sqldr.GetInt32(2);
                    dataresource = sqldr.GetInt32(3);
                    curentchkpayid = sqldr.GetInt32(4);
                    hospperf = sqldr.GetInt32(5);
                    hospresu = sqldr.GetString(6);
                    s_hospdate = sqldr.GetString(7);
                    familyno = sqldr.GetString(8);
                    memberno = sqldr.GetString(9);
                    lockstate = sqldr.GetInt32(10);
                    patstate = sqldr.GetInt32(11);
                    age = sqldr.GetInt32(12);
                    hosptolevel = sqldr.GetInt32(13);
                    hospdate = sqldr.GetDateTime(14);
                }
                catch (Exception ee)
                {
                    mess = if0.getXMLErroStrFromString("9000","获取医疗信息发生异常:"+ee.Message.ToString()) ;
                    return mess;
                }
                finally
                {
                    hospset.Dispose();
                    Connection.Dispose();
                    sqlcomm.Dispose();
                    sqldr.Dispose();
                }
                #endregion

                #region 获取参数信息
                sqltext = "Select ifprechk,prechkdays,prechkdays1,prechkdays2,rehosp, ";
                sqltext += " excepmode ";
                sqltext += "From areasysparm Where areacode ='" + AreaCode + "'";
                sqlcomm.CommandText = sqltext;
                sqlcomm.Connection = Connection;
                sqldr = sqlcomm.ExecuteReader();
                if (!sqldr.HasRows)
                {
                    mess = mess = if0.getXMLErroStrFromString("9000", "没有查询到系统参数，无法进行补偿。");
                    return mess;
                }
                try
                {
                    sqldr.Read();                
                    ifprechk = sqldr.GetInt32(0);               
                    prechkdays = sqldr.GetInt32(1);
                    prechkdays1 = sqldr.GetInt32(2);
                    prechkdays2 = sqldr.GetInt32(3);
                    rehosp = sqldr.GetInt32(4);
                    excepmode = sqldr.GetInt32(5);
                }
                catch(Exception ee)
                {
                    mess = if0.getXMLErroStrFromString("9000","获取补偿设置时发生异常:"+ee.Message.ToString()) ;
                    return mess;
                }
                finally
                {
                    hospset.Dispose();
                    Connection.Dispose();
                    sqlcomm.Dispose();
                    sqldr.Dispose();
                }
                #endregion

                #region 常规判断
                if (hospperf == 0)
                {
                    mess = if0.getXMLErroStrFromString("2200", "该住院信息还是在院状态，不允许再次进行补偿。");
                    return mess;
                }

                if (curentchkpayid != 0)
                {
                    mess = if0.getXMLErroStrFromString("2200", "该住院信息已补偿过了，不允许再次进行补偿。");
                    return mess;
                }
                if (ifprechk != 1)
                {
                    mess = if0.getXMLErroStrFromString("2200", "该区县农合机构不允许垫付，请联系农合机构。");
                    return mess;
                }

                if (lockstate == 1)
                {
                    mess = if0.getXMLErroStrFromString("2200", "该住院信息已被冻结,不允许审核操作。");
                    return mess;
                }

                //疾病名称有效性
                try
                {
                    sqlcomm.CommandText = "Select Count(*) From nhicd10 Where icdname = '" + hospresu + "'";
                    if (Convert.ToInt32(sqlcomm.ExecuteScalar()) == 0)
                    {
                        mess = if0.getXMLErroStrFromString("2200", "'"+hospresu+"'不是农合目录中的疾病，无法进行补偿。");
                        return mess;
                    }
                }
                catch (Exception ee)
                {
                    mess = if0.getXMLErroStrFromString("9000", "判断疾病有效性时发生异常:"+ee.Message.ToString());
                    return mess;
                }
                finally
                {
                    hospset.Dispose();
                    Connection.Dispose();
                    sqlcomm.Dispose();
                    sqldr.Dispose();
                }

                //费用截止日期有效性
                try
                {
                    sqlcomm.CommandText = "Select Max(date3) From pricedetail Where hospitalcode = '"+hospitalcode+"' And hospid = "+hospid.ToString();
                    if (Convert.ToString(sqlcomm.ExecuteScalar()) != nopaydate)
                    {
                        mess = if0.getXMLErroStrFromString("2200","费用截止日期与最后一条费用发生日期不相符，无法进行补偿。");
                        return mess;
                    }
                }
                catch (Exception ee)
                {
                    mess = if0.getXMLErroStrFromString("2200", "查询最大费用截止日期时发生异常："+ee.Message.ToString());
                    return mess;
                }
                finally
                {
                    hospset.Dispose();
                    Connection.Dispose();
                    sqlcomm.Dispose();
                    sqldr.Dispose();
                }
                #endregion

                #region 出院天数超过系统允许的出院天数
                if (!if0.getCurrentDateTime(out currentdate, out mess))
                {
                    mess = if0.getXMLErroStrFromString("9000", "获取服务器时间失败:"+mess);
                    return mess;
                }

                sqltext = "Select Top 1 DateDiff(day," + outdate.ToString() + "," + currentdate.ToString() + ") From sexcode";
                sqlcomm.CommandText = sqltext;
                try
                {
                    outdays = Convert.ToInt32(sqlcomm.ExecuteScalar());
                }
                catch (Exception ee)
                {
                    mess = if0.getXMLErroStrFromString("9000","计算出院已出院天数时发生异常:" + ee.Message.ToString());
                    return mess;
                }
                finally
                {
                    hospset.Dispose();
                    Connection.Dispose();
                    sqlcomm.Dispose();
                    sqldr.Dispose();
                }

                if (gonextstate == 1)
                    prechkdays = prechkdays1;
                else if (dataresource == 1)
                    prechkdays = prechkdays2;

                if (prechkdays > 0 && outdays > prechkdays)
                {
                    mess = if0.getXMLErroStrFromString("2200", "该病人已出院" + outdays.ToString() + "天,超过最大报销限定天数（"+prechkdays.ToString()+"）。");
                    return mess;
                }

                #endregion

                #region 重叠住院
                if (rehosp == 1) //不允许重叠住院
                {
                    sqltext = "Select Count(*) From hosppatinfo2 ";
                    sqltext += "Where ( MEDINUMB='" + familyno + "') And ( Len(medinumb) > 6) And ( patientid = '" + memberno + "') ";
                    sqltext += "( (Convert(VarChar(8),hospdate,112) < '"+s_hospdate+"' And nopaydate > '"+s_hospdate+"' ) ";
                    sqltext += "   Or (Convert(VarChar(8),hospdate,112) > '" + s_hospdate + "' And Convert(VarChar(8),hospdate,112) < '" + s_hospdate + "')";
			        sqltext +=") And (orderid > -10000 Or orderid is null)"; 
                    sqlcomm.CommandText = sqltext;
                    try
                    {
                        if (Convert.ToInt32(sqlcomm.ExecuteScalar()) > 0)
                        {
                            mess = if0.getXMLErroStrFromString("2200","该住院信息异常，同样的家庭号和成员号存在同一时段同时住院的现象.不允许审核操作!");
                            return mess;
                        }
                    }
                    catch(Exception ee)
                    {
                         mess = if0.getXMLErroStrFromString("2200","查询重叠住院信息时发生异常:"+ee.Message.ToString());
                         return mess;
                    }
                    finally
                    {
                         hospset.Dispose();
                         Connection.Dispose();
                         sqlcomm.Dispose();
                         sqldr.Dispose();
                    }
                    
                }
                #endregion

                #region 住院时长异常
                if (excepmode == 99 && (hosptolevel.Equals(null) || hosptolevel == 0)) //有转出医院的则不判断时长
                {
                    Int32 hosphour;
                    try
                    {
                        try
                        {
                            sqlcomm.CommandText = "Select Max(ddate) From pricedetail Where hospitalcode ='" + hospitalcode + "' hospid = " + hospid;
                            ddate = Convert.ToDateTime(sqlcomm.ExecuteScalar());
                        }
                        catch (Exception ee)
                        {
                            mess = if0.getXMLErroStrFromString("9000", "获取费用最大发生日期时发生异常：" + ee.Message.ToString());
                            return mess;
                        }
                        

                        try
                        {
                            sqlcomm.CommandText = "Select Top 1 DateDiff(hour," + hospdate.ToString() + "," + ddate.ToString() + ") From sexcode";
                            hosphour = Convert.ToInt32(sqlcomm.ExecuteScalar());
                        }
                        catch (Exception ee)
                        {
                            mess = if0.getXMLErroStrFromString("9000", "获取住院时长(小时)时发生异常：" + ee.Message.ToString());
                            return mess;
                        }
                               
                        if (patstate == 0)
                        {
                            sqltext = "Select Top 1 hosphour1 From areahospparm ";
                            sqltext += "Where areacode = '"+AreaCode+"' And limittype <> 4 And chkid = 2 ";
                            sqltext += "And ( ";
		                    sqltext += "    (icdname = '"+hospresu+"' ";
	                        sqltext += "     And "+age.ToString()+" >= age1 ";
		                    sqltext += "     And "+age.ToString()+" <=age2 ";
			                sqltext += "    ) ";
			                sqltext += "    Or ";
			                sqltext += "    ( ";
			                sqltext += "     (icdname ='' Or icdname is null) ";
			                sqltext += "           And ("+age.ToString()+" >= age1 ";
		                    sqltext += "         And "+age.ToString()+" <=age2 ) ";
			                sqltext += "         ) ";
                            sqltext += "        ) ";	
	                        sqltext += "Order By limittype Desc";
                        }
                        else
                        {
                            sqltext = "Select Top 1 hosphour1 From areahospparm ";
                            sqltext += " Where areacode = '" + AreaCode + "' ";
                            sqltext += " And limittype = 4 And chkid = 2";
                            sqltext += " And " + age.ToString() + " >= age1 ";
                            sqltext += " And " + age.ToString() + " <= age2";
                            sqltext += "Order By limittype Desc";
                        }
                        try
                        {
                            Int32 hosphour1;
                            sqlcomm.CommandText = sqltext;
                            hosphour1 = Convert.ToInt32(sqlcomm.ExecuteScalar());
                            if (hosphour < hosphour1 && hosphour != 0)
                            {
                                mess = if0.getXMLErroStrFromString("2200","根据农合病人住院时长限制政策,该农合病人住院时长不足"+hosphour1.ToString()+"小时,不允许报销。");
                                return mess;
                            }
                            
                        }
                        catch (Exception ee)
                        {
                            mess = if0.getXMLErroStrFromString("9000", "获取政策制定住院时长时发生异常：" + ee.Message.ToString());
                            return mess;
                        }
                        
                    }
                    catch (Exception ee)
                    {
                        mess = if0.getXMLErroStrFromString("9000", "验证住院时长(小时)时发生异常：" + ee.Message.ToString());
                        return mess;
                    }
                    finally
                    {
                        hospset.Dispose();
                        Connection.Dispose();
                        sqlcomm.Dispose();
                        sqldr.Dispose();
                    }
                }
                #endregion
                #endregion
            }
            #endregion
            else
            #region 试结算 不进行可补偿判断 从XML中构造DataSet
            {
                string ls_hospid, ls_hospcode = "", ls_patientname = "", ls_sex = "", ls_birth = "", ls_xx = "", ls_gj = "", ls_mz = "", ls_jg = "", ls_hyzt = "", ls_jtzz = "";
                string ls_dh = "", ls_yb = "", ls_zycs = "", ls_rysj = "", ls_cysj = "", ls_yjj = "", ls_zd = "", ls_lb = "", ls_cyzt = "", ls_cyjfzt = "";
                string ls_wjsf = "", ls_tjjz = "", ls_hljb = "", ls_zyzt = "", ls_fph = "", ls_zyf = "", ls_ylzh = "", ls_sfz = "";                

                XmlDocument xdoc = if0.getXMLDocumentFromString(dataXML, out mess);
                XmlNodeList nlis = null;
                if (mess == "TRUE")
                {
                    nlis = xdoc.SelectSingleNode("DATA").ChildNodes;
                    XmlNode t = null;
                    #region 住院信息有效性判断
                    nlis = xdoc.GetElementsByTagName("zyh");
                    ls_hospcode = Convert.ToString(nlis[0].InnerText);
                    #region //住院号验证

                    if (ls_hospcode == null || ls_hospcode.Trim().Length == 0)
                    {
                        mess += "zyh不能为空";
                    }
                    #endregion
                    nlis = xdoc.GetElementsByTagName("xm");
                    ls_patientname = Convert.ToString(nlis[0].InnerText);
                    #region //姓名验证
                    if (ls_patientname == null || ls_patientname.Trim().Length == 0)
                    {
                        mess += "xm不能为空";
                    }
                    #endregion
                    nlis = xdoc.GetElementsByTagName("xb");
                    ls_sex = Convert.ToString(nlis[0].InnerText);
                    #region //性别验证
                    if (ls_sex == null || ls_sex.Trim().Length == 0)
                    {
                        mess += "xb不能为空";
                    }
                    else
                    {
                        if (ls_sex != "0" && ls_sex != "1" && ls_sex != "2")
                        {
                            mess += "xb取值必须为2:女,1:男,0:其他";
                        }
                    }
                    #endregion
                    nlis = xdoc.GetElementsByTagName("sr");
                    ls_birth = Convert.ToString(nlis[0].InnerText);
                    #region //生日格式有效性验证

                    if (ls_birth == null || ls_birth.Trim().Length == 0)
                    {
                        ls_birth = "1900-01-01 00:00:00";
                    }
                    else
                    {
                        if (!if0.isDateTime(ls_birth))
                        {
                            mess += "sr格式必须为'2009-01-25 08:25:05'的形式";
                        }
                    }
                    #endregion
                    nlis = xdoc.GetElementsByTagName("xx"); //血型

                    ls_xx = Convert.ToString(nlis[0].InnerText);
                    nlis = xdoc.GetElementsByTagName("gj"); //国家
                    ls_gj = Convert.ToString(nlis[0].InnerText);
                    nlis = xdoc.GetElementsByTagName("mz"); //民族
                    ls_mz = Convert.ToString(nlis[0].InnerText);
                    nlis = xdoc.GetElementsByTagName("jg"); //籍贯
                    ls_jg = Convert.ToString(nlis[0].InnerText);
                    nlis = xdoc.GetElementsByTagName("hyzt"); //婚姻状态

                    ls_hyzt = Convert.ToString(nlis[0].InnerText);
                    nlis = xdoc.GetElementsByTagName("jtzz"); //家庭住址
                    ls_jtzz = Convert.ToString(nlis[0].InnerText);
                    nlis = xdoc.GetElementsByTagName("dh"); //电话
                    ls_dh = Convert.ToString(nlis[0].InnerText);
                    nlis = xdoc.GetElementsByTagName("yb"); //邮编
                    ls_yb = Convert.ToString(nlis[0].InnerText);
                    nlis = xdoc.GetElementsByTagName("zycs");
                    ls_zycs = Convert.ToString(nlis[0].InnerText);
                    #region //第几次住院

                    if (ls_zycs == null || ls_zycs.Trim().Length == 0 || !if0.isNumber(ls_zycs))
                    {
                        mess += "zycs不能为空且必须为正整数";
                    }
                    #endregion
                    nlis = xdoc.GetElementsByTagName("rysj");
                    ls_rysj = Convert.ToString(nlis[0].InnerText);
                    #region //入院时间有效性验证

                    if (ls_rysj == null || ls_rysj.Trim().Length == 0)
                    {
                        mess += "rysj不能为空";
                    }
                    else
                    {
                        if (!if0.isDateTime(ls_rysj))
                        {
                            mess += "rysj格式必须为'2009-01-25 08:25:05'的形式";
                        }
                    }
                    #endregion
                    nlis = xdoc.GetElementsByTagName("cysj");
                    ls_cysj = Convert.ToString(nlis[0].InnerText);
                    #region //出院时间有效性验证

                    if (ls_cysj != null && ls_cysj.Trim().Length != 0)
                    {
                        if (!if0.isDateTime(ls_cysj))
                        {
                            mess += "cysj格式必须为'2009-01-25 08:25:05'的形式";
                        }
                    }
                    #endregion
                    nlis = xdoc.GetElementsByTagName("yjj");
                    ls_yjj = Convert.ToString(nlis[0].InnerText);
                    #region //预交金验证

                    if (ls_yjj == null || ls_yjj.Trim().Length == 0)
                    {
                        mess += "yjj不能为空";
                    }
                    else
                    {
                        if (!if0.isNumber(ls_yjj))
                        {
                            mess += "yjj必须是数字型";
                        }
                    }
                    #endregion
                    nlis = xdoc.GetElementsByTagName("zd");
                    ls_zd = Convert.ToString(nlis[0].InnerText);
                    #region //诊断验证
                    if (ls_zd == null || ls_zd.Trim().Length == 0)
                    {
                        mess += "zd不能为空";
                    }
                    #endregion
                    nlis = xdoc.GetElementsByTagName("lb");
                    ls_lb = Convert.ToString(nlis[0].InnerText);
                    #region //病人类别验证
                    if (ls_lb == null || ls_lb.Trim().Length == 0)
                    {
                        mess += "lb不能为空";
                    }
                    #endregion
                    nlis = xdoc.GetElementsByTagName("cyzt");
                    ls_cyzt = Convert.ToString(nlis[0].InnerText);
                    #region //住院状态验证

                    if (ls_cyzt == null || ls_cyzt.Trim().Length == 0)
                    {
                        mess += "cyzt不能为空";
                    }
                    else
                    {
                        if (ls_cyzt != "0" && ls_cyzt != "1")
                        {
                            mess += "cyzt取值必须为0:在院,1:出院";
                        }
                        else
                        {
                            if (ls_cyzt == "1")
                            {
                                if (ls_cysj == null || ls_cysj.Trim().Length == 0)
                                {
                                    mess += "cyzt为1(出院)必须有出院时间cysj";
                                }
                            }
                        }
                    }
                    #endregion
                    nlis = xdoc.GetElementsByTagName("cyjfzt");
                    ls_cyjfzt = Convert.ToString(nlis[0].InnerText);
                    #region //出院交费情况验证
                    if (ls_cyjfzt == null || ls_cyjfzt.Trim().Length == 0 || !if0.isNumber(ls_cyjfzt))
                    {
                        ls_cyjfzt = "1";
                    }
                    #endregion
                    nlis = xdoc.GetElementsByTagName("wjsf");
                    ls_wjsf = Convert.ToString(nlis[0].InnerText);
                    #region //未结算费验证
                    if (ls_wjsf == null || ls_wjsf.Trim().Length == 0)
                    {
                        mess += "wjsf不能为空";
                    }
                    else
                    {
                        if (!if0.isNumber(ls_wjsf))
                        {
                            mess += "wjsf必须是数字型";
                        }
                    }
                    #endregion
                    nlis = xdoc.GetElementsByTagName("tjjz");
                    ls_tjjz = Convert.ToString(nlis[0].InnerText);
                    #region //费用统计截止日期验证
                    if (ls_tjjz == null || ls_tjjz.Trim().Length == 0)
                    {
                        if (ls_cyzt == "1" && ls_cysj != "")
                        {
                            if (if0.isDateTime(ls_cysj))
                            {
                                ls_tjjz = ls_cysj.Substring(0, 4) + ls_cysj.Substring(5, 2) + ls_cysj.Substring(8, 2);
                            }
                            else
                            {
                                mess += "tjjz费用统计截止日期不能为空";
                            }
                        }
                        else
                        {
                            if (if0.isDateTime(ls_rysj))
                            {
                                ls_tjjz = ls_rysj.Substring(0, 4) + ls_rysj.Substring(5, 2) + ls_rysj.Substring(8, 2);
                            }
                            else
                            {
                                mess += "tjjz费用统计截止日期不能为空";
                            }
                        }
                    }
                    if (ls_tjjz != null && ls_tjjz.Trim().Length != 0 && !if0.isDateTime(ls_tjjz.Substring(0, 4) + "-" + ls_tjjz.Substring(4, 2) + "-" + ls_tjjz.Substring(6, 2) + " 00:00:01"))
                    {
                        mess += "tjjz费用统计截止日期必须为'20080129'的形式";
                    }
                    #endregion
                    nlis = xdoc.GetElementsByTagName("hljb");
                    ls_hljb = Convert.ToString(nlis[0].InnerText);
                    #region //护理级别验证
                    if (ls_hljb == null || ls_hljb.Trim().Length == 0)
                    {
                        mess += "hljb不能为空";
                    }
                    #endregion
                    nlis = xdoc.GetElementsByTagName("zyqk");
                    ls_zyzt = Convert.ToString(nlis[0].InnerText);
                    #region //在院状态验证

                    if (ls_zyzt == null || ls_zyzt.Trim().Length == 0 || !if0.isNumber(ls_zyzt))
                    {
                        mess += "zyqk不能为空且必须为整数";
                    }
                    #endregion
                    nlis = xdoc.GetElementsByTagName("fph");
                    ls_fph = Convert.ToString(nlis[0].InnerText);
                    nlis = xdoc.GetElementsByTagName("zyf");
                    ls_zyf = Convert.ToString(nlis[0].InnerText);
                    #region //住院总费用验证

                    if (ls_zyf == null || ls_zyf.Trim().Length == 0 || !if0.isNumber(ls_zyf))
                    {
                        mess += "zyf不能为空且必须为数字型";
                    }
                    #endregion
                    nlis = xdoc.GetElementsByTagName("ylzh");
                    ls_ylzh = Convert.ToString(nlis[0].InnerText);
                    #region //医疗证号验证
                    if (ls_ylzh == null || ls_ylzh.Trim().Length == 0)
                    {
                        mess += "ylzh不能为空";
                    }
                    #endregion
                    nlis = xdoc.GetElementsByTagName("sfz");
                    ls_sfz = Convert.ToString(nlis[0].InnerText);
                    #region //身份证号验证
                    if (ls_sfz == null || ls_sfz.Trim().Length == 0)
                    {
                        mess += "sfz不能为空";
                    }
                    #endregion

                    #endregion

                    #region 明细费用有效性判断
                    Int32 i,listcount;
                    nlis = xdoc.GetElementsByTagName("fee");
                    listcount = nlis.Count;
                    for (i = 0; i < listcount; i++)
                    {

                    }
                    #endregion

                    sqltext = "Select * From hosppatinfo2 Where hospitalcode = '' And hospid = ''";
                    hospset = if0.getDataSet(sqltext,"hosppatinfo2", out mess);
                    if (mess != "TRUE")
                    {
                        return if0.getXMLErroStrFromString("9000", "试算生成虚拟住院信息时发生异常：" + mess);
                    }

                    sqltext = "Select * From pricedetail Where hospitalcode = '' And hospid = ''";
                    hospset = if0.getDataSet(sqltext, "pricedetail", out mess);
                    if (mess != "TRUE")
                    {
                        return if0.getXMLErroStrFromString("9000", "试算生成虚拟住院费用信息时发生异常：" + mess);
                    }  

                }
                else
                {
                    mess = if0.getXMLErroStrFromString("9000", "读取XML文件失败："+mess);
                    return mess;
                }
            }
            #endregion
        }
        catch (Exception ee)
        {
            mess = mess = if0.getXMLErroStrFromString("9000", "计算补偿金额时发生异常：" + ee.Message.ToString());
            return mess;
        }
        finally
        {
            hospset.Dispose();
            Connection.Dispose();
            sqlcomm.Dispose();
            sqldr.Dispose();
        }


        //HospCalcFee hospcf = new HospCalcFee();
        //DataSet feeds = null;

        //if (!hospcf.CalcFee(hospitalcode,hospid,chkpayid,opername,settle,feeds, out mess))
        //{
        //    mess = if0.getXMLErroStrFromString("9000", mess);
        //    return mess ;
        //}        
        return mess;
    }
    #endregion

}
