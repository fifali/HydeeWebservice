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
/// 门诊业务操作
/// </summary>
public class ncmsifmz
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
    #endregion 

    #region 构造函数


    public ncmsifmz()
	{
		//
		// TODO: 在此处添加构造函数逻辑
		//
    }

    public ncmsifmz(string dbServer, string dbName, string userID, string passWord, string dbConnString, bool validUser, string areaCode, int interFacePower)
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
    ~ncmsifmz()
    {
        if0.Dispose();
    }
    #endregion

    #region //门诊病人操作
    /// <summary>
    /// 生成病人的补偿号
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="human"></param>
    /// <param name="year"></param>
    /// <param name="payorder"></param>
    /// <returns></returns>
    public bool getMZPatientPayorder(string areacode, string human, string year, out string payorder)
    {
        payorder = "";
        if (human == null || human.Length == 0)
        {
            payorder = "没有个人编码";
            return false;
        }
        if (year == null || year.Length != 4)
        {
            payorder = "没有年度";
            return false;
        }
        string dbConnStr = "";
        if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
        {
            SqlCommand cmd = new SqlCommand();
            SqlConnection Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
            try
            {
                Connection.Open();
                cmd.Connection = Connection;
                ////cmd.CommandText = "SELECT MAX(payorder) FROM mz_checkpay WHERE human='" + human + "'";
                //cmd.CommandText = "SELECT MAX(payorder) FROM mz_checkpay WHERE statisticyear='" + year + "'";
                ////cmd.CommandText += " AND areacode2='" + human.Substring(6, 6) + "' AND right(human,4)='" + human.Substring(14, 4) + "'";
                //cmd.CommandText += " AND left(payorder,12)='" + human.Substring(6, 6) + year.Substring(2, 2) + human.Substring(14, 4) + "'";
                //屏蔽上面的方法，因为宁乡出现了2010年的而补偿号中标识为11年的数据，导致11年该人的门诊生成补偿号报告说重复的问题。是因为上述查询中使用了statisticyear条件限制。

                cmd.CommandText = "SELECT MAX(payorder) FROM mz_checkpay WHERE payorder like '" + human.Substring(6, 6) + year.Substring(2, 2) + human.Substring(14, 4) + "%'";
                payorder = Convert.ToString(cmd.ExecuteScalar());
                if (payorder == null || payorder.Length == 0)
                {
                    payorder = "000";
                }
                else
                {
                    payorder = payorder.Substring(payorder.Length - 3, 3);
                }
                //补偿号规则:个人编码中的乡村组编码+2位年+后2位家庭号+成员序号+补偿序号
                payorder = human.Substring(6, 6) + year.Substring(2, 2) + human.Substring(14, 4) + string.Format("{0:000}", Convert.ToInt32(payorder) + 1);
                return true;
            }
            catch (Exception e)
            {
                payorder = "数据库发生异常：" + e.Message.ToString();
                return false;
            }
            finally
            {
                if (Connection.State == ConnectionState.Open)
                    Connection.Close();
                cmd.Dispose();
            }
        }
        else
        {
            payorder = dbConnStr;
            return false;
        }
    }

    /// <summary>
    /// 从农合系统中查询一个门诊病人资料


    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="hospcode"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string getMZPatientInfo(string areacode, string hospitalcode, string hospcode, out string data)
    {
        data = "";
        if (hospcode == null || hospcode.Length == 0)
        {
            return if0.getXMLErroStrFromString("2000", "就诊号(mzh)不能为空值");
        }
        string mess = "";
        if (if0.getDBConnStr("areacode", areacode, out mess))
        {
            string ls_sql = null;
            DataSet myDs = null;
            ls_sql = "SELECT h.hospitalcode as yydm,h.HospCode as mzh,h.requoffice1 as ks,h.doctor as ys,h.PATIENTNAME as xm,";
            ls_sql += "h.HospDate as rq,h.patstate as zt,h.hosptype as lx,h.HospResu as icdname,h.grandPrice as zje,";
            ls_sql += "h.deratesum as jm,h.inpatientcode as fph,h.ifchina as zyzl,h.chkpayid as bcbz,h.payorder as bch,";
            ls_sql += "c.accountstate as jsbz,c.payid as zfbj,c.lockstate as djbz,c.ifaccount as kjsbz,";
            ls_sql += "f.bookcard as medicard,f.validno as zksbm,i.patientid as sfz,h.autoprekey";
            ls_sql += " FROM mz_checkpay c,mz_hosppatinfo2 h,n_individdata i,n_familydata f";
            ls_sql += " WHERE c.payorder=h.payorder AND i.human=h.human AND i.statisticyear=h.statisticyear";
            ls_sql += " AND i.statisticyear=f.statisticyear AND i.familyno=f.familyno";
            ls_sql += " AND h.hospitalcode='" + hospitalcode + "' AND h.HospCode='" + hospcode + "'";
            //生成数据
            myDs = if0.getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                if0.getRequestXMLStringFromDS(myDs, "门诊病人资料", out mess, out data);
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
    /// 上传门诊农合病人信息(支持多个病人信息上传)
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="user"></param>
    /// <param name="dataXML"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public string putMZPatientInfo(string areacode, string hospitalcode, string user, string dataXML, out string datas)
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
                    Int32 i, li_cnt, li_tmp, li_orderid = -1, li_ifoutpatient = 0;//...特殊门诊政策序号，是否特殊门诊


                    string ls_hospcode, ls_requoffice1, ls_doctor, ls_patientname, ls_hospdate, ls_patstate, ls_hosptype;
                    string ls_hospresuid, ls_hospresu, ls_grandprice, ls_deratesum, ls_inpatientcode, ls_ifchina;
                    string ls_year = "", ls_medicard, ls_authid, ls_id,ls_year2 = "";
                    string ls_year30 = "";//体检申报年度
                    string ls_human = "", ls_areacode2 = "", ls_patsort = "", ls_payorder = "", ls_tmp = "", errtxt = "", ls_autoprekey = "-1";
                    string ls_outpatienttime = ""; //特殊门诊开始时间


                    Int32 li_paymode = -1, li_approval = -1;//特殊门诊补助模式，需预先申报批准
                    bool execresult = true, haverecord = false;
                    XmlNodeList nlis = data.SelectSingleNode("DATA").ChildNodes; //获取DATA节点的所有子节点
                    li_cnt = nlis.Count;
                    Connection.Open();
                    SqlTransaction myTrans;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = Connection;
                    SqlDataReader myReader = null;
                    try
                    {
                        for (i = 0; i < li_cnt; i++)
                        {
                            haverecord = false;
                            mess = "";
                            nlis = data.GetElementsByTagName("mzh");
                            ls_hospcode = Convert.ToString(nlis[i].InnerText);
                            #region //门诊号有效性验证


                            if (ls_hospcode == null || ls_hospcode.Trim().Length == 0)
                            {
                                mess += "mzh不能为空";
                            }
                            else
                            {
                                if (!if0.isValidParm(ls_hospcode))
                                {
                                    mess += "mzh参数无效";
                                }
                                else
                                {
                                    ////门诊号是否已存在
                                    //try
                                    //{
                                    //    cmd.CommandText = "SELECT count(*) FROM mz_hosppatinfo2 where hospitalcode=('" + hospitalcode + "') and HospCode=('" + ls_hospcode + "')";
                                    //    li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
                                    //    if (li_tmp > 0)
                                    //    {
                                    //        mess += "mzh已存在，不能上传";
                                    //    }
                                    //}
                                    //catch (Exception e)
                                    //{
                                    //    mess = getXMLErroStrFromString("9000", "数据库校验mzh=" + ls_hospcode + "的数据异常：" + e.Message.ToString());
                                    //    return mess;
                                    //}

                                    DataSet myDs = null;
                                    ls_payorder = "";
                                    string ls_sql = "SELECT h.chkpayid,c.chkpayid,c.payid,c.payorder FROM mz_checkpay c,mz_hosppatinfo2 h WHERE c.payorder = h.payorder";
                                    ls_sql += " AND h.hospitalcode=('" + hospitalcode + "') and HospCode=('" + ls_hospcode + "')";
                                    myDs = if0.getDataSet(ls_sql, out ls_tmp);
                                    try
                                    {
                                        if (ls_tmp == "TRUE")
                                        {
                                            if (myDs.Tables[0].Rows.Count > 1)
                                            {
                                                mess += "存在多条相同的门诊号的数据！";//这是不可能发生的
                                            }
                                            else if (myDs.Tables[0].Rows.Count == 1)
                                            {
                                                if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) != 0 || Convert.ToInt32(myDs.Tables[0].Rows[0][1]) != 0)
                                                {
                                                    mess += "资料已审核，不能再上传";
                                                }
                                                if (Convert.ToInt32(myDs.Tables[0].Rows[0][2]) == 1)
                                                {
                                                    mess += ";病人已获补偿，不能再上传";
                                                }
                                                ls_payorder = Convert.ToString(myDs.Tables[0].Rows[0][3]);
                                                haverecord = true;
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
                                        mess = if0.getXMLErroStrFromStringData("9000", "发生异常", "<ROW><mzh>" + ls_hospcode + "</mzh><errtxt>" + "发生异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                        return mess;
                                    }
                                    finally
                                    {
                                        myDs.Dispose();
                                    }
                                }
                            }
                            #endregion
                            nlis = data.GetElementsByTagName("rq");
                            ls_hospdate = Convert.ToString(nlis[i].InnerText);
                            #region //日期格式有效性验证


                            if (ls_hospdate == null || ls_hospdate.Trim().Length == 0)
                            {
                                mess += "rq不能为空";
                            }
                            else
                            {
                                if (!if0.isDateTime(ls_hospdate))
                                {
                                    mess += "rq格式必须为'2009-01-25 08:25:05'的形式";
                                }
                            }
                            #endregion
                            nlis = data.GetElementsByTagName("zkh");
                            ls_medicard = Convert.ToString(nlis[i].InnerText);
                            nlis = data.GetElementsByTagName("zksbm");
                            ls_authid = Convert.ToString(nlis[i].InnerText);
                            nlis = data.GetElementsByTagName("xm");
                            ls_patientname = Convert.ToString(nlis[i].InnerText);
                            if (ls_patientname == null || ls_patientname.Trim().Length == 0)
                            {
                                mess += "xm不能为空";
                            }
                            nlis = data.GetElementsByTagName("sfz");
                            ls_id = Convert.ToString(nlis[i].InnerText);
                            nlis = data.GetElementsByTagName("ks");
                            ls_requoffice1 = Convert.ToString(nlis[i].InnerText);
                            if (ls_requoffice1 == null || ls_requoffice1.Trim().Length == 0)
                            {
                                mess += "ks不能为空";
                            }
                            nlis = data.GetElementsByTagName("ys");
                            ls_doctor = Convert.ToString(nlis[i].InnerText);
                            if (ls_doctor == null || ls_doctor.Trim().Length == 0)
                            {
                                mess += "ys不能为空";
                            }
                            nlis = data.GetElementsByTagName("zt");
                            ls_patstate = Convert.ToString(nlis[i].InnerText);
                            if (ls_patstate == null || ls_patstate.Trim().Length == 0 || !if0.isNumber(ls_patstate))
                            {
                                mess += "zt必须为数字型";
                            }
                            else if (ls_patstate != "0" && ls_patstate != "1")
                            {
                                mess += "zt值必须为0或1(0正常1已死亡)";
                            }
                            nlis = data.GetElementsByTagName("lx");
                            ls_hosptype = Convert.ToString(nlis[i].InnerText);
                            if (ls_hosptype == null || ls_hosptype.Trim().Length == 0 || !if0.isNumber(ls_hosptype))
                            {
                                mess += "lx必须为数字型";
                            }
                            else if (ls_hosptype != "10" && ls_hosptype != "20" && ls_hosptype != "30" && ls_hosptype != "40")
                            {
                                mess += "lx值必须为10,20,30,40(10普通门诊，20特殊慢病门诊，30体检，40门诊留观)";
                            }
                            if (ls_hosptype == "30")
                            {
                                nlis = data.GetElementsByTagName("sbid");
                                ls_autoprekey = Convert.ToString(nlis[0].InnerText);
                                if (ls_autoprekey == null || ls_autoprekey.Trim().Length == 0 || !if0.isNumber(ls_autoprekey))
                                {
                                    mess += "sbid必须为数字型";
                                }
                                try
                                {
                                    cmd.CommandText = "Select statisticyear From Tb_declare_roster Where autoprekey = " + ls_autoprekey;
                                    ls_year30 = Convert.ToString(cmd.ExecuteScalar());
                                    if (ls_year30.Equals(""))
                                    {
                                        mess += "未找到体检申请申报ID号(" + ls_autoprekey + ")。";
                                    }
                                }
                                catch (Exception ee)
                                {
                                    mess += "查询体检申请年度时发生异常:"+ee.Message.ToString();
                                }
                            }
                            else
                            {
                                ls_autoprekey = "-1";
                            }
                            nlis = data.GetElementsByTagName("icdcode");
                            ls_hospresuid = Convert.ToString(nlis[i].InnerText);
                            nlis = data.GetElementsByTagName("icdname");
                            ls_hospresu = Convert.ToString(nlis[i].InnerText);
                            #region //ICD有效性判断


                            if ((ls_hospresu == null || ls_hospresu.Trim().Length == 0) && (ls_hospresuid == null || ls_hospresuid.Trim().Length == 0))
                            {
                                mess += "icdcode和icdname不能同时为空";
                            }
                            else
                            {
                                if (mess == "") //已有错误信息，不再劳神去数据库中校验了，节约资源
                                {
                                    //诊断名是否有效


                                    try
                                    {
                                        cmd.CommandText = "SELECT icdname FROM nhicd10";
                                        if (!(ls_hospresuid == null || ls_hospresuid.Trim().Length == 0))
                                        {
                                            cmd.CommandText += " WHERE icdcode='" + ls_hospresuid + "'";
                                        }
                                        else if (!(ls_hospresu == null || ls_hospresu.Trim().Length == 0))
                                        {
                                            cmd.CommandText += " WHERE icdcode=icdcode AND icdname='" + ls_hospresu + "'";
                                        }
                                        ls_hospresu = Convert.ToString(cmd.ExecuteScalar());
                                        if (ls_hospresu == "")
                                        {
                                            mess += "icdcode或icdname在农合ICD目录中不存在";
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mzh>" + ls_hospcode + "</mzh><errtxt>" + "校验mzh=" + ls_hospcode + "的数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                        return mess;
                                    }
                                    //传上来的是特殊慢病门诊的类型的有效性判断


                                    if (ls_hosptype == "20" && ls_hospresu != "")
                                    {
                                        try
                                        {
                                            //上传疾病是否是申请的疾病，政策是否有效


                                            ls_tmp = ls_hospdate.Substring(0, 4) + ls_hospdate.Substring(5, 2) + ls_hospdate.Substring(8, 2);
                                            if (li_ifoutpatient == 1)
                                            {
                                                cmd.CommandText = "SELECT icdname,paymode,approval FROM mz_arearcmsparm2 WHERE (areacode='" + areacode + "' AND '" + ls_tmp + "' BETWEEN validbegin AND validend";
                                                cmd.CommandText += " AND '" + ls_outpatienttime.Substring(0, 4) + ls_outpatienttime.Substring(5, 2) + ls_outpatienttime.Substring(8, 2) + "' BETWEEN validbegin AND validend";
                                                cmd.CommandText += " AND icdname='" + ls_hospresu + "' AND orderid=" + li_orderid.ToString() + ")";
                                                cmd.CommandText += " OR (areacode='" + areacode + "' AND '" + ls_tmp + "' BETWEEN validbegin AND validend AND icdname='" + ls_hospresu + "' AND approval=0) ORDER BY approval DESC";
                                            }
                                            else
                                            {
                                                cmd.CommandText = "SELECT icdname,paymode,approval FROM mz_arearcmsparm2 WHERE areacode='" + areacode + "' AND '" + ls_tmp + "' BETWEEN validbegin AND validend AND icdname='" + ls_hospresu + "' AND approval=0";
                                            }
                                            myReader = cmd.ExecuteReader();
                                            try
                                            {
                                                if (!myReader.HasRows)
                                                {
                                                    mess += "没有查到" + ls_hospresu + "的有效特殊慢病政策";//HIS传过来的是慢病，因此要返回指定这个错误


                                                }
                                                else
                                                {
                                                    myReader.Read();
                                                    ls_tmp = myReader.GetString(0);//疾病
                                                    li_paymode = myReader.GetInt32(1);//补助模式
                                                    li_approval = myReader.GetInt32(2);//需预先申报批准
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
                                            mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mzh>" + ls_hospcode + "</mzh><errtxt>" + "校验mzh=" + ls_hospcode + "的特殊慢病审批数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                            return mess;
                                        }
                                    }
                                    //虽未指定慢病门诊类型，但是特殊门诊有效申报者的
                                    else if (li_ifoutpatient == 1 && ls_hospresu != "")
                                    {
                                        try
                                        {
                                            //上传疾病是否是特殊慢病疾病，政策是否有效，是则补偿类别为20
                                            ls_tmp = ls_hospdate.Substring(0, 4) + ls_hospdate.Substring(5, 2) + ls_hospdate.Substring(8, 2);
                                            cmd.CommandText = "SELECT icdname,paymode,approval FROM mz_arearcmsparm2 WHERE (areacode='" + areacode + "' AND '" + ls_tmp + "' BETWEEN validbegin AND validend";
                                            cmd.CommandText += " AND '" + ls_outpatienttime.Substring(0, 4) + ls_outpatienttime.Substring(5, 2) + ls_outpatienttime.Substring(8, 2) + "' BETWEEN validbegin AND validend";
                                            cmd.CommandText += " AND icdname='" + ls_hospresu + "' AND orderid=" + li_orderid.ToString() + ")";
                                            cmd.CommandText += " OR (areacode='" + areacode + "' AND '" + ls_tmp + "' BETWEEN validbegin AND validend AND icdname='" + ls_hospresu + "' AND approval=0) ORDER BY approval DESC";
                                            myReader = cmd.ExecuteReader();
                                            try
                                            {
                                                if (myReader.HasRows)
                                                {
                                                    ls_hosptype = "20";
                                                    myReader.Read();
                                                    ls_tmp = myReader.GetString(0);//疾病
                                                    li_paymode = myReader.GetInt32(1);//补助模式
                                                    li_approval = myReader.GetInt32(2);//需预先申报批准
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
                                            mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mzh>" + ls_hospcode + "</mzh><errtxt>" + "校验mzh=" + ls_hospcode + "的特殊慢病审批数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                            return mess;
                                        }
                                    }
                                }

                            }
                            #endregion
                            nlis = data.GetElementsByTagName("zje");
                            ls_grandprice = Convert.ToString(nlis[i].InnerText);
                            if (ls_grandprice == null || ls_grandprice.Trim().Length == 0 || !if0.isNumber(ls_grandprice))
                            {
                                mess += "zje必须为数字型";
                            }
                            nlis = data.GetElementsByTagName("jm");
                            ls_deratesum = Convert.ToString(nlis[i].InnerText);
                            if (ls_deratesum == null || ls_deratesum.Trim().Length == 0 || !if0.isNumber(ls_deratesum))
                            {
                                mess += "jm必须为数字型";
                            }
                            nlis = data.GetElementsByTagName("fph");
                            ls_inpatientcode = Convert.ToString(nlis[i].InnerText);
                            if (ls_inpatientcode == null || ls_inpatientcode.Trim().Length == 0)
                            {
                                mess += "fph不能为空";
                            }
                            nlis = data.GetElementsByTagName("zyzl");
                            ls_ifchina = Convert.ToString(nlis[i].InnerText);
                            if (ls_ifchina == null || ls_ifchina.Trim().Length == 0 || !if0.isNumber(ls_ifchina))
                            {
                                mess += "zyzl必须为数字型";
                            }
                            else if (ls_ifchina != "0" && ls_ifchina != "1")
                            {
                                mess += "zyzl值必须为0或1(0否1是)";
                            }
                            if (mess == "") //如果已有错误信息，不再劳神去数据库中校验了，节约资源
                            {
                                #region //参合有效性验证


                                ls_tmp = "";
                                ls_year = Convert.ToString(DateTime.Parse(ls_hospdate).Year);
                                if (ls_hosptype == "30")
                                {
                                    ls_year2 = ls_year30;
                                }
                                else
                                {
                                    ls_year2 = ls_year;
                                }
                                if (!if0.checkPersonsValid(areacode, ls_year2, ls_medicard, ls_authid, ls_patientname, "", out ls_patientname, ls_id, out ls_human, out ls_areacode2, out ls_patsort, out li_orderid, out li_ifoutpatient, out ls_outpatienttime, out mess))
                                {
                                    mess += ls_tmp;
                                }
                                #endregion
                                #region 验证申报ID号是否有效，验证体检的费用与申报体检包的费用是否一致


                                if (ls_hosptype == "30")
                                {
                                    if (mess == "") //如果已有错误信息，不再劳神去数据库中校验了，节约资源
                                    {
                                        if (checkTJAskedid(areacode, ls_autoprekey, hospitalcode, ls_year2, ls_human, 0, out mess))
                                        {
                                            checkTJPacketFee(Connection, hospitalcode, Convert.ToDecimal(ls_grandprice), ls_autoprekey, out mess);
                                        }
                                    }
                                }
                                #endregion
                            }
                            if (mess != "")
                            {
                                execresult = false;
                                errtxt += "<ROW><mzh>" + ls_hospcode + "</mzh><errtxt>" + mess + "</errtxt></ROW>";
                            }
                            else
                            {
                                #region //获取补偿号


                                if (ls_payorder != "")
                                {
                                    li_tmp = 1;
                                }
                                else
                                {
                                    li_tmp = 0;//当设置为1时，表示已存在了补偿记录的


                                    if (li_paymode != -1) //特殊慢病门诊的


                                    {
                                        switch (li_paymode)
                                        {
                                            case 10://每次封顶
                                                ls_tmp = ls_hospdate.Substring(0, 4) + ls_hospdate.Substring(5, 2) + ls_hospdate.Substring(8, 2);
                                                break;
                                            case 20://每月封顶
                                            case 21://每月封顶(当月无住院时)
                                                ls_tmp = ls_hospdate.Substring(0, 4) + ls_hospdate.Substring(5, 2);
                                                cmd.CommandText = "SELECT payorder FROM mz_checkpay WHERE human='" + ls_human + "' AND statisticyear='" + ls_year + "' AND paydate='" + ls_tmp + "'";
                                                ls_payorder = Convert.ToString(cmd.ExecuteScalar());
                                                if (ls_payorder == null || ls_payorder == "")
                                                {
                                                    ls_payorder = "";
                                                }
                                                else
                                                {
                                                    li_tmp = 1;
                                                }
                                                break;
                                            case 30://每年封顶
                                            case 31://每年起付线外定额
                                            case 32://每年封顶(封顶额含同诊断住院补偿额)
                                            case 33://每年封顶(封顶额含同年住院补偿额)
                                                ls_tmp = ls_hospdate.Substring(0, 4);
                                                cmd.CommandText = "SELECT payorder FROM mz_checkpay WHERE human='" + ls_human + "' AND statisticyear='" + ls_year + "' AND paydate='" + ls_tmp + "'";
                                                ls_payorder = Convert.ToString(cmd.ExecuteScalar());
                                                if (ls_payorder == null || ls_payorder == "")
                                                {
                                                    ls_payorder = "";
                                                }
                                                else
                                                {
                                                    li_tmp = 1;
                                                }
                                                break;
                                            default:
                                                mess = if0.getXMLErroStrFromString("9000", "不支持的特殊慢病补助模式，请联系开发人员");
                                                return mess;
                                        }
                                    }
                                    else
                                    {
                                        ls_tmp = ls_hospdate.Substring(0, 4) + ls_hospdate.Substring(5, 2) + ls_hospdate.Substring(8, 2);
                                    }
                                    if (ls_payorder == "")
                                    {
                                        if (!getMZPatientPayorder(areacode, ls_human, ls_year2, out ls_payorder))
                                        {
                                            mess = if0.getXMLErroStrFromString("9000", ls_payorder);
                                            return mess;
                                        }
                                    }
                                }
                                #endregion
                                myTrans = Connection.BeginTransaction();
                                try
                                {
                                    cmd.Transaction = myTrans;
                                    if (li_tmp == 0)
                                    {
                                        //先插入一条门诊补偿空记录
                                        cmd.CommandText = "INSERT INTO mz_checkpay(human,statisticyear,areacode2,payorder,grandPrice,";
                                        cmd.CommandText += "assfee,familyfound,paydate,nhpay,mediprice,";
                                        cmd.CommandText += "mediselffee,materprice,materselffee,serverprice,serverselffee,";
                                        cmd.CommandText += "chkpayid,calcid,accountstate,orderid,payid,";
                                        cmd.CommandText += "organpay,lockstate,ifaccount,inputor,inputtime,inputorg) VALUES(";
                                        cmd.CommandText += "'" + ls_human + "','" + ls_year2 + "','" + ls_areacode2 + "','" + ls_payorder + "'," + ls_grandprice + ",";
                                        cmd.CommandText += "0.0,0.0,'" + ls_tmp + "',0.0,0.0,";
                                        cmd.CommandText += "0.0,0.0,0.0,0.0,0.0,";
                                        cmd.CommandText += "0,0,0,-1,0,";
                                        cmd.CommandText += "'" + hospitalcode + "',0,1,'" + user + "',getdate(),'" + hospitalcode + "')";
                                        cmd.ExecuteNonQuery();
                                    }
                                    if (haverecord)
                                    {
                                        cmd.CommandText = "UPDATE mz_hosppatinfo2 SET requoffice1='" + ls_requoffice1 + "',doctor='" + ls_doctor + "',PATIENTNAME='" + ls_patientname + "'";
                                        cmd.CommandText += ",HospDate='" + ls_hospdate + "',patstate='" + ls_patstate + "',hosptype='" + ls_hosptype + "',HospResu='" + ls_hospresu + "',grandPrice=" + ls_grandprice;
                                        cmd.CommandText += ",deratesum=" + ls_deratesum + ",inpatientcode='" + ls_inpatientcode + "',human='" + ls_human + "',statisticyear='" + ls_year2 + "',areacode2='" + ls_areacode2 + "'";
                                        cmd.CommandText += ",patsort='" + ls_patsort + "',ifchina=" + ls_ifchina + ",dataresource=0,areacode='" + areacode + "',autoprekey=" + ls_autoprekey;
                                        cmd.CommandText += " WHERE hospitalcode='" + hospitalcode + "' AND HospCode='" + ls_hospcode + "'";
                                    }
                                    else
                                    {
                                        //再插入一条门诊病人记录


                                        cmd.CommandText = "INSERT INTO mz_hosppatinfo2(hospitalcode,HospCode,requoffice1,doctor,PATIENTNAME,";
                                        cmd.CommandText += "HospDate,patstate,hosptype,HospResu,grandPrice,";
                                        cmd.CommandText += "deratesum,inpatientcode,human,statisticyear,areacode2,";
                                        cmd.CommandText += "patsort,ifchina,assfee,familyfound,nhpay,";
                                        cmd.CommandText += "mediprice,mediselffee,materprice,materselffee,serverprice,";
                                        cmd.CommandText += "serverselffee,chkpayid,payorder,dataresource,areacode,autoprekey)";
                                        cmd.CommandText += " VALUES('" + hospitalcode + "','" + ls_hospcode + "','" + ls_requoffice1 + "','" + ls_doctor + "','" + ls_patientname + "',";
                                        cmd.CommandText += "'" + ls_hospdate + "'," + ls_patstate + "," + ls_hosptype + ",'" + ls_hospresu + "'," + ls_grandprice + ",";
                                        cmd.CommandText += ls_deratesum + ",'" + ls_inpatientcode + "','" + ls_human + "','" + ls_year2 + "','" + ls_areacode2 + "',";
                                        cmd.CommandText += "'" + ls_patsort + "'," + ls_ifchina + ",0.0,0.0,0.0,";
                                        cmd.CommandText += "0.0,0.0,0.0,0.0,0.0,";
                                        cmd.CommandText += "0.0,0,'" + ls_payorder + "',0,'" + areacode + "'," + ls_autoprekey;
                                        cmd.CommandText += ")";
                                    }
                                    cmd.ExecuteNonQuery();
                                    myTrans.Commit();
                                }
                                catch (Exception e)
                                {
                                    myTrans.Rollback();
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mzh>" + ls_hospcode + "</mzh><errtxt>" + "数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                    return mess;
                                }
                                finally
                                {
                                    myTrans.Dispose();
                                }
                            }
                        }
                        if (execresult == true)
                        {
                            datas = "<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "UTF-8" + '"' + "?><DATA><ROW><bch>" + ls_payorder + "</bch></ROW></DATA>";
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
    /// 删除一个已上传的还没有处理且没有补偿的
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="hospcode"></param>
    /// <returns></returns>
    public string delMZPatientInfo(string areacode, string hospitalcode, string hospcode)
    {
        if (hospcode == null || hospcode.Length == 0)
        {
            return if0.getXMLErroStrFromString("2000", "就诊号(mzh)不能为空值");
        }
        string mess = "";
        string dbConnStr = "";
        Int32 li_tmp;
        if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
        {
            //联接串中要去掉这个关键词 Provider=SQLOLEDB.1;
            SqlConnection Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
            try
            {
                string ls_payorder = null;
                Connection.Open();
                DataSet myDs = null;

                //是否存在，是否可删除
                string ls_sql = "SELECT h.chkpayid,c.chkpayid,c.payid,c.payorder FROM mz_checkpay c,mz_hosppatinfo2 h WHERE c.payorder = h.payorder";
                ls_sql += " AND h.hospitalcode='" + hospitalcode + "' AND h.HospCode='" + hospcode + "'";
                myDs = if0.getDataSet(ls_sql, out mess);
                try
                {
                    if (mess == "TRUE")
                    {
                        if (myDs.Tables[0].Rows.Count > 1)
                        {
                            mess = if0.getXMLErroStrFromString("2000", "存在多条相同的门诊号的数据！");//这是不可能发生的
                            return mess;
                        }
                        //else if (myDs.Tables[0].Rows.Count == 0)
                        //{
                        //    mess = getXMLErroStrFromString("2000", "没有这个门诊号的病人");//此时应该也是达到了删除的目的了


                        //    return mess;
                        //}
                        else if (myDs.Tables[0].Rows.Count == 1)
                        {
                            if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) != 0 || Convert.ToInt32(myDs.Tables[0].Rows[0][1]) != 0)
                            {
                                mess = if0.getXMLErroStrFromString("2200", "资料已审核，不能删除");
                                return mess;
                            }
                            if (Convert.ToInt32(myDs.Tables[0].Rows[0][2]) == 1)
                            {
                                mess = if0.getXMLErroStrFromString("2200", "病人已获补偿，不能删除");
                                return mess;
                            }
                            ls_payorder = Convert.ToString(myDs.Tables[0].Rows[0][3]);
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
                //补偿号下有几个门诊病人信息(特殊慢病门诊)
                if (ls_payorder != null && ls_payorder.Length != 0)
                {
                    try
                    {
                        cmd.CommandText = "SELECT count(*) FROM mz_hosppatinfo2 where payorder=('" + ls_payorder + "')";
                        li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    catch (Exception e)
                    {
                        mess = if0.getXMLErroStrFromString("9000", "验证病人资料异常mzh=" + hospcode + "：" + e.Message.ToString());
                        return mess;
                    }
                }
                else
                {
                    li_tmp = 0;
                }
                myTrans = Connection.BeginTransaction();
                try
                {
                    cmd.Transaction = myTrans;
                    cmd.CommandText = "DELETE FROM mz_hosppatinfo2 WHERE hospitalcode='" + hospitalcode + "' and HospCode='" + hospcode + "'";
                    cmd.ExecuteNonQuery();
                    if (li_tmp != 1)
                    {
                        cmd.CommandText = "DELETE FROM mz_checkpay WHERE payorder='" + ls_payorder + "'";
                        cmd.ExecuteNonQuery();
                    }
                    myTrans.Commit();
                    mess = if0.getXMLStrFromString("执行成功");
                    return mess;
                }
                catch (Exception e)
                {
                    myTrans.Rollback();
                    mess = if0.getXMLErroStrFromString("9000", "数据库中删除mzh=" + hospcode + "数据异常：" + e.Message.ToString());
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

    /// <summary>
    /// 修改一个门诊病人的资料
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="hospcode"></param>
    /// <param name="dataXML"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public string editMZPatientInfo(string areacode, string hospitalcode, string hospcode, string dataXML, out string datas)
    {
        datas = "";
        if (hospcode == null || hospcode.Trim().Length == 0)
        {
            return if0.getXMLErroStrFromString("2000", "mzh不能为空");
        }
        string mess = "";
        string dbConnStr = "";
        if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
        {
            XmlDocument data = if0.getXMLDocumentFromString(dataXML, out mess);
            if (mess == "TRUE")
            {
                //联接串中要去掉这个关键词 Provider=SQLOLEDB.1;
                SqlConnection Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
                try
                {
                    int li_tmp, li_chkpayid;
                    string ls_requoffice1, ls_doctor, ls_patstate, ls_hosptype, ls_hospresuid, ls_hospresu;
                    string ls_grandprice, ls_deratesum, ls_inpatientcode, ls_ifchina, ls_autoprekey;
                    string errtxt = "";
                    bool execresult = true;
                    XmlNodeList nlis;
                    Connection.Open();
                    SqlTransaction myTrans;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = Connection;
                    //SqlDataReader myReader;
                    try
                    {
                        mess = "";
                        //门诊号是否已存在
                        #region
                        try
                        {
                            cmd.CommandText = "SELECT count(*) FROM mz_hosppatinfo2 where hospitalcode=('" + hospitalcode + "') and HospCode=('" + hospcode + "')";
                            li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
                            //myReader = cmd.ExecuteReader();
                            if (li_tmp == 0)
                            {
                                mess = if0.getXMLErroStrFromString("2000", "mzh不存在，不能修改病人资料");
                                return mess;
                            }
                            else
                            {
                                cmd.CommandText = "SELECT c.chkpayid FROM mz_checkpay c,mz_hosppatinfo2 h WHERE c.payorder = h.payorder";
                                cmd.CommandText += " AND h.hospitalcode=('" + hospitalcode + "') AND h.HospCode=('" + hospcode + "')";
                                li_chkpayid = Convert.ToInt32(cmd.ExecuteScalar());
                                if (li_chkpayid != 0)
                                {
                                    mess = if0.getXMLErroStrFromString("2200", "资料已审核，不能删除");
                                    return mess;
                                }
                            }
                            //myReader.Close();
                            //myReader.Dispose();
                        }
                        catch (Exception e)
                        {
                            mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mzh>" + hospcode + "</mzh><errtxt>" + "校验mzh=" + hospcode + "的数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                            return mess;
                        }
                        #endregion
                        nlis = data.GetElementsByTagName("ks");
                        ls_requoffice1 = Convert.ToString(nlis[0].InnerText);
                        if (ls_requoffice1 == null || ls_requoffice1.Trim().Length == 0)
                        {
                            mess += "ks不能为空";
                        }
                        nlis = data.GetElementsByTagName("ys");
                        ls_doctor = Convert.ToString(nlis[0].InnerText);
                        if (ls_doctor == null || ls_doctor.Trim().Length == 0)
                        {
                            mess += "ys不能为空";
                        }
                        nlis = data.GetElementsByTagName("zt");
                        ls_patstate = Convert.ToString(nlis[0].InnerText);
                        if (ls_patstate == null || ls_patstate.Trim().Length == 0 || !if0.isNumber(ls_patstate))
                        {
                            mess += "zt必须为数字型";
                        }
                        else if (ls_patstate != "0" && ls_patstate != "1")
                        {
                            mess += "zt值必须为0或1(0正常1已死亡)";
                        }
                        nlis = data.GetElementsByTagName("lx");
                        ls_hosptype = Convert.ToString(nlis[0].InnerText);
                        if (ls_hosptype == null || ls_hosptype.Trim().Length == 0 || !if0.isNumber(ls_hosptype))
                        {
                            mess += "lx必须为数字型";
                        }
                        else if (ls_hosptype != "10" && ls_hosptype != "20" && ls_hosptype != "30" && ls_hosptype != "40")
                        {
                            mess += "lx值必须为10,20,30,40(10普通门诊，20特殊慢病门诊，30体检，40门诊留观)";
                        }
                        if (ls_hosptype == "30")
                        {
                            nlis = data.GetElementsByTagName("sbid");
                            ls_autoprekey = Convert.ToString(nlis[0].InnerText);
                            if (ls_autoprekey == null || ls_autoprekey.Trim().Length == 0 || !if0.isNumber(ls_autoprekey))
                            {
                                mess += "sbid必须为数字型";
                            }
                            //if (mess == "") //至此还没有错误信息的，则验证申报ID号的有效性


                            //{
                            //    checkTJAskedid(areacode, ls_autoprekey, hospitalcode, "", "", 0, out mess);
                            //}
                        }
                        else
                        {
                            ls_autoprekey = "-1";
                        }
                        nlis = data.GetElementsByTagName("icdcode");
                        ls_hospresuid = Convert.ToString(nlis[0].InnerText);
                        nlis = data.GetElementsByTagName("icdname");
                        ls_hospresu = Convert.ToString(nlis[0].InnerText);
                        if ((ls_hospresu == null || ls_hospresu.Trim().Length == 0) && (ls_hospresuid == null || ls_hospresuid.Trim().Length == 0))
                        {
                            mess += "icdcode和icdname不能同时为空";
                        }
                        else
                        {
                            //诊断名是否有效


                            try
                            {
                                cmd.CommandText = "SELECT icdname FROM nhicd10";
                                if (!(ls_hospresuid == null || ls_hospresuid.Trim().Length == 0))
                                {
                                    cmd.CommandText += " WHERE icdcode='" + ls_hospresuid + "'";
                                }
                                else if (!(ls_hospresu == null || ls_hospresu.Trim().Length == 0))
                                {
                                    cmd.CommandText += " WHERE icdname='" + ls_hospresu + "'";
                                }
                                ls_hospresu = Convert.ToString(cmd.ExecuteScalar());
                                //myReader = cmd.ExecuteReader();
                                if (ls_hospresu == "")
                                {
                                    mess += "icdcode或icdname在农合ICD目录中不存在";
                                }
                                //myReader.Close();
                                //myReader.Dispose();
                            }
                            catch (Exception e)
                            {
                                mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mzh>" + hospcode + "</mzh><errtxt>" + "校验mzh=" + hospcode + "的数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                return mess;
                            }

                        }
                        nlis = data.GetElementsByTagName("zje");
                        ls_grandprice = Convert.ToString(nlis[0].InnerText);
                        if (ls_grandprice == null || ls_grandprice.Trim().Length == 0 || !if0.isNumber(ls_grandprice))
                        {
                            mess += "zje必须为数字型";
                        }
                        nlis = data.GetElementsByTagName("jm");
                        ls_deratesum = Convert.ToString(nlis[0].InnerText);
                        if (ls_deratesum == null || ls_deratesum.Trim().Length == 0 || !if0.isNumber(ls_deratesum))
                        {
                            mess += "jm必须为数字型";
                        }
                        nlis = data.GetElementsByTagName("fph");
                        ls_inpatientcode = Convert.ToString(nlis[0].InnerText);
                        if (ls_inpatientcode == null || ls_inpatientcode.Trim().Length == 0)
                        {
                            mess += "fph不能为空";
                        }
                        nlis = data.GetElementsByTagName("zyzl");
                        ls_ifchina = Convert.ToString(nlis[0].InnerText);
                        if (ls_ifchina == null || ls_ifchina.Trim().Length == 0 || !if0.isNumber(ls_ifchina))
                        {
                            mess += "zyzl必须为数字型";
                        }
                        else if (ls_ifchina != "0" && ls_ifchina != "1")
                        {
                            mess += "zyzl值必须为0或1(0否1是)";
                        }
                        if (mess == "") //如果已有错误信息，不再劳神去数据库中校验了，节约资源
                        {
                            #region 验证申报ID号是否有效，验证体检的费用与申报体检包的费用是否一致


                            if (ls_hosptype == "30")
                            {
                                if (mess == "") //如果已有错误信息，不再劳神去数据库中校验了，节约资源
                                {
                                    if (checkTJAskedid(areacode, ls_autoprekey, hospitalcode, "", "", 0, out mess))
                                    {
                                        checkTJPacketFee(Connection, hospitalcode, Convert.ToDecimal(ls_grandprice), ls_autoprekey, out mess);
                                    }
                                }
                            }
                            #endregion
                        }
                        if (mess != "")
                        {
                            execresult = false;
                            errtxt += "<ROW><mzh>" + hospcode + "</mzh><errtxt>" + mess + "</errtxt></ROW>";
                        }
                        else
                        {
                            myTrans = Connection.BeginTransaction();
                            try
                            {
                                cmd.Transaction = myTrans;
                                cmd.CommandText = "UPDATE mz_hosppatinfo2 SET requoffice1='" + ls_requoffice1 + "',doctor='" + ls_doctor + "',";
                                cmd.CommandText += "patstate=" + ls_patstate + ",hosptype='" + ls_hosptype + "',HospResu='" + ls_hospresu + "',grandPrice=" + ls_grandprice + ",";
                                cmd.CommandText += "deratesum=" + ls_deratesum + ",inpatientcode='" + ls_inpatientcode + "',";
                                cmd.CommandText += "ifchina=" + ls_ifchina + ",dataresource=0,autoprekey=" + ls_autoprekey + " WHERE hospitalcode='" + hospitalcode + "' AND HospCode='" + hospcode + "'";
                                cmd.ExecuteNonQuery();
                                myTrans.Commit();
                            }
                            catch (Exception e)
                            {
                                myTrans.Rollback();
                                mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mzh>" + hospcode + "</mzh><errtxt>" + "数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                return mess;
                            }
                            finally
                            {
                                myTrans.Dispose();
                            }
                        }
                        if (execresult == true)
                        {
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
    #endregion

    #region //门诊病人费用操作
    /// <summary>
    /// 获取指定门诊号的费用数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="hospcode"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string getMZPatientFee(string areacode, string hospitalcode, string hospcode, out string data)
    {
        data = "";
        if (hospcode == null || hospcode.Length == 0)
        {
            return if0.getXMLErroStrFromString("2000", "就诊号(mzh)不能为空值");
        }
        string mess = "";
        if (if0.getDBConnStr("areacode", areacode, out mess))
        {
            string ls_sql = null;
            DataSet myDs = null;
            ls_sql = "SELECT hospitalcode as yydm,HospCode as mzh,detailid as fyid,nhcode as nhdm,mediid as hismlid,MarkType as fylx,";
            ls_sql += "mediname as hisname,convert(varchar,ddate,120) as fysj,user_num as sl,unitprice as dj,";
            ls_sql += "price as je,selffee as zf,in_out as bn,";
            ls_sql += "case chkpayid when 0 then 0 when 8 then 1 else chkpayid+1 end as shzt,prechkfee as ysje,chkinitfee as shje";
            ls_sql += " FROM mz_pricedetail";
            ls_sql += " WHERE hospitalcode='" + hospitalcode + "' AND HospCode='" + hospcode + "'";
            ls_sql += " ORDER BY hospitalcode ASC,HospCode ASC,mediid ASC";
            //生成数据
            myDs = if0.getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                if0.getRequestXMLStringFromDS(myDs, "门诊费用资料", out mess, out data);
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
    /// 上传一个门诊病人的费用
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="hospcode"></param>
    /// <param name="dataXML"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public string putMZPatientFee(string areacode, string hospitalcode, string hospcode, string dataXML, out string datas)
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
                    int i, li_cnt, li_tmp, li_chkpayid;
                    string ls_detailid = "", ls_nhcode = "", ls_marktype = "", ls_mediname = "", ls_ddate = "", ls_user_num = "";
                    string ls_unitprice = "", ls_price = "", ls_mediid = "";
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
                        mess = "";
                        #region 门诊号是不存在或已审核的不能上传
                        try
                        {
                            cmd.CommandText = "SELECT count(*) FROM mz_hosppatinfo2 where hospitalcode=('" + hospitalcode + "') and HospCode=('" + hospcode + "')";
                            li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
                            if (li_tmp == 0)
                            {
                                mess = "病人信息还没上传，不能上传费用";
                            }
                            else
                            {
                                cmd.CommandText = "SELECT c.chkpayid FROM mz_checkpay c,mz_hosppatinfo2 h WHERE c.payorder = h.payorder";
                                cmd.CommandText += " AND h.hospitalcode=('" + hospitalcode + "') AND h.HospCode=('" + hospcode + "')";
                                li_chkpayid = Convert.ToInt32(cmd.ExecuteScalar());
                                if (li_chkpayid != 0)
                                {
                                    mess = "资料已审核，不能上传费用";
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mzh>" + hospcode + "</mzh><errtxt>" + "数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                            return mess;
                        }
                        #endregion
                        if (mess != "")
                        {
                            mess = if0.getXMLErroStrFromStringData("2000", "数据校验未通过", "<ROW><mzh>" + hospcode + "</mzh><errtxt>" + mess + "</errtxt></ROW>", out datas);
                            return mess;
                        }
                        for (i = 0; i < li_cnt; i++)
                        {
                            haverecord = false;
                            mess = "";
                            nlis = data.GetElementsByTagName("fyid");
                            ls_detailid = Convert.ToString(nlis[i].InnerText);
                            if (ls_detailid == null || ls_detailid.Trim().Length == 0 || !if0.isNumber(ls_detailid))
                            {
                                mess += "fyid必须为数字型";
                            }
                            else
                            {
                                #region 费用已存在的只能update
                                try
                                {
                                    cmd.CommandText = "SELECT count(*) FROM mz_pricedetail WHERE hospitalcode=('" + hospitalcode + "') AND HospCode=('" + hospcode + "') AND detailid=" + ls_detailid;
                                    li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
                                    if (li_tmp > 0)
                                    {
                                        haverecord = true;
                                    }
                                }
                                catch (Exception e)
                                {
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mzh>" + hospcode + "</mzh><errtxt>" + "数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                    return mess;
                                }
                                #endregion
                                #region 其他xml数据单元有效性验证


                                nlis = data.GetElementsByTagName("nhdm");
                                ls_nhcode = Convert.ToString(nlis[i].InnerText);
                                if (ls_nhcode == null || ls_nhcode.Trim().Length == 0)
                                {
                                    mess += "nhdm不能为空，必须上传匹配的农合代码";
                                }
                                //未完成 这里还可以加入判断农合代码是否正确的判断，并计算出是否保内


                                nlis = data.GetElementsByTagName("fylx");
                                ls_marktype = Convert.ToString(nlis[i].InnerText);
                                if (ls_marktype == null || ls_marktype.Trim().Length == 0 || !if0.isNumber(ls_marktype))
                                {
                                    mess += "fylx必须为数字型";
                                }
                                else if (ls_marktype != "0" && ls_marktype != "1" && ls_marktype != "2")
                                {
                                    mess += "fylx必须为指定的数字0药品1材料2医疗服务";
                                }
                                nlis = data.GetElementsByTagName("hisname");
                                ls_mediname = Convert.ToString(nlis[i].InnerText);
                                if (ls_mediname == null || ls_mediname.Trim().Length == 0)
                                {
                                    ls_mediname = "";
                                    mess += "hisname不能为空";
                                }
                                nlis = data.GetElementsByTagName("fysj");
                                ls_ddate = Convert.ToString(nlis[i].InnerText);
                                if (ls_ddate == null || ls_ddate.Trim().Length == 0)
                                {
                                    mess += "fysj不能为空";
                                }
                                else
                                {
                                    if (!if0.isDateTime(ls_ddate))
                                    {
                                        mess += "fysj格式必须为'2009-01-25 08:25:45'的形式";
                                    }
                                }
                                nlis = data.GetElementsByTagName("sl");
                                ls_user_num = Convert.ToString(nlis[i].InnerText);
                                if (ls_user_num == null || ls_user_num.Trim().Length == 0 || !if0.isNumber(ls_user_num))
                                {
                                    mess += "sl必须为数字型";
                                }
                                nlis = data.GetElementsByTagName("dj");
                                ls_unitprice = Convert.ToString(nlis[i].InnerText);
                                if (ls_unitprice == null || ls_unitprice.Trim().Length == 0 || !if0.isNumber(ls_unitprice))
                                {
                                    mess += "dj必须为数字型";
                                }
                                nlis = data.GetElementsByTagName("je");
                                ls_price = Convert.ToString(nlis[i].InnerText);
                                if (ls_price == null || ls_price.Trim().Length == 0 || !if0.isNumber(ls_price))
                                {
                                    mess += "je必须为数字型";
                                }
                                nlis = data.GetElementsByTagName("hismlid");
                                ls_mediid = Convert.ToString(nlis[i].InnerText);
                                if (ls_mediid == null || ls_mediid.Trim().Length == 0 || !if0.isNumber(ls_mediid))
                                {
                                    mess += "hismlid必须为数字型";
                                }
                                #endregion
                            }
                            if (mess != "")
                            {
                                execresult = false;
                                errtxt += "<ROW><fyid>" + ls_detailid + "</fyid><errtxt>门诊号" + hospcode + ":" + ls_mediname + "的" + mess + "</errtxt></ROW>";
                            }
                            else
                            {
                                #region 更新数据
                                myTrans = Connection.BeginTransaction();
                                try
                                {
                                    cmd.Transaction = myTrans;
                                    if (haverecord) //已上传的做update
                                    {
                                        cmd.CommandText = "UPDATE mz_pricedetail SET nhcode='" + ls_nhcode + "',MarkType=" + ls_marktype;
                                        cmd.CommandText += ",mediname='" + ls_mediname + "',ddate='" + ls_ddate + "',user_num=" + ls_user_num + ",unitprice=" + ls_unitprice + ",price=" + ls_price + ",mediid=" + ls_mediid + ",chkpayid=0";
                                        cmd.CommandText += " WHERE hospitalcode=('" + hospitalcode + "') AND HospCode=('" + hospcode + "') AND detailid=" + ls_detailid;
                                    }
                                    else
                                    {
                                        cmd.CommandText = "INSERT INTO mz_pricedetail(hospitalcode,HospCode,detailid,nhcode,MarkType,";
                                        cmd.CommandText += "mediname,ddate,user_num,unitprice,price,selffee,autochkfee,checkid,in_out,chkpayid,";
                                        cmd.CommandText += "mediid)";
                                        cmd.CommandText += " VALUES('" + hospitalcode + "','" + hospcode + "'," + ls_detailid + ",'" + ls_nhcode + "'," + ls_marktype + ",";
                                        cmd.CommandText += "'" + ls_mediname + "','" + ls_ddate + "'," + ls_user_num + "," + ls_unitprice + "," + ls_price + ",0.0,0.0,0,1,0," + ls_mediid;
                                        cmd.CommandText += ")";
                                    }
                                    cmd.ExecuteNonQuery();
                                    myTrans.Commit();
                                }
                                catch (Exception e)
                                {
                                    myTrans.Rollback();
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><fyid>" + ls_detailid + "</fyid><errtxt>门诊号" + hospcode + ":数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
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
                    mess = if0.getXMLErroStrFromStringData("9000", "发生异常", "<ROW><areacode>" + areacode + "</areacode><errtxt>" + mess + e.Message.ToString() + ",可能缺少数据元</errtxt></ROW>", out datas);
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
    /// 删除一个门诊病人的所有费用


    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="hospcode"></param>
    /// <returns></returns>
    public string delMZPatientFee(string areacode, string hospitalcode, string hospcode)
    {
        if (hospcode == null || hospcode.Length == 0)
        {
            return if0.getXMLErroStrFromString("2000", "就诊号(mzh)不能为空值");
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
                string ls_sql = "SELECT h.chkpayid,c.chkpayid,c.payid FROM mz_checkpay c,mz_hosppatinfo2 h WHERE c.payorder = h.payorder";
                ls_sql += " AND h.hospitalcode='" + hospitalcode + "' AND h.HospCode='" + hospcode + "'";
                myDs = if0.getDataSet(ls_sql, out mess);
                try
                {
                    if (mess == "TRUE")
                    {
                        if (myDs.Tables[0].Rows.Count > 1)
                        {
                            mess = if0.getXMLErroStrFromString("2000", "存在多条相同的门诊号的数据！");//这是不可能发生的
                            return mess;
                        }
                        //else if (myDs.Tables[0].Rows.Count == 0)
                        //{
                        //    mess = getXMLErroStrFromString("2000", "没有这个门诊号的病人");//此时应该也是达到了删除的目的了


                        //    return mess;
                        //}
                        else if (myDs.Tables[0].Rows.Count == 1)
                        {
                            if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) != 0 || Convert.ToInt32(myDs.Tables[0].Rows[0][1]) != 0)
                            {
                                mess = if0.getXMLErroStrFromString("2200", "资料已审核，不能删除");
                                return mess;
                            }
                            if (Convert.ToInt32(myDs.Tables[0].Rows[0][2]) == 1)
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
                    cmd.CommandText = "DELETE FROM mz_pricedetail WHERE hospitalcode='" + hospitalcode + "' and HospCode='" + hospcode + "'";
                    cmd.ExecuteNonQuery();
                    myTrans.Commit();
                    mess = if0.getXMLStrFromString("执行成功");
                    return mess;
                }
                catch (Exception e)
                {
                    myTrans.Rollback();
                    mess = if0.getXMLErroStrFromString("9000", "数据库中删除mzh=" + hospcode + "费用数据异常：" + e.Message.ToString());
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

    #region HISonline接口调用门诊相关
    /// <summary>
    /// 从HIS里查询指定门诊号的费用上传到农合里


    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <param name="hospcode"></param>
    /// <returns></returns>
    public string putMZPatientFeeFromHIS(string areacode, string hospitalcode, string servername, string dbname, string user, string pwd, string hospcode)
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
        #region 查询数据库，门诊号查证


        if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
        {
            //联接串中要去掉这个关键词 Provider=SQLOLEDB.1;
            Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
            try
            {
                Connection.Open();
                cmd = new SqlCommand();
                cmd.Connection = Connection;
                try
                {
                    try
                    {
                        cmd.CommandText = "SELECT count(*) FROM mz_hosppatinfo2 WHERE hospitalcode=('" + hospitalcode + "') AND HospCode=('" + hospcode + "')";
                        li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
                        if (li_tmp > 0)
                        {
                            haverecord = true;
                            cmd.CommandText = "SELECT chkpayid FROM mz_hosppatinfo2 WHERE hospitalcode=('" + hospitalcode + "') AND HospCode=('" + hospcode + "')";
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
                    return if0.getXMLErroStrFromString("9000", "生成HIS门诊数据更新到农合的语句时异常：" + e.Message.ToString());
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
        DataSet lds = new DataSet();
        ls_sql = "select r.autonumb * 1000 + r.serial as fyid,case when len(p.nhdm)=0 then '未匹配' else isnull(p.nhdm,'未匹配') end as nhdm,fylx = (case when d.item =1 then 2 else 0 end),";
        ls_sql += "d.mediname as hisname,i.CreaDate as fysj,r.quantity as sl,r.unitpric as dj,r.itemamou as je,r.mediid as hismlid";
        ls_sql += " from " + dbname + "..nClinReciInfo r, " + dbname + "..dictmedi d LEFT JOIN " + dbname + "..mz_nhppb p ON d.mediid = p.mediid, " + dbname + "..nClinInvoInfo i";
        ls_sql += " where i.autonumb = r.autonumb and d.MediId = r.MediID and i.main_AUTONUMB =" + hospcode;
        ls_sql += " and d.medicode not in(select jdecode from " + dbname + "..Clin_SysSet union all select discode from " + dbname + "..Clin_SysSet)";
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
                        #region 生成并执行门诊病人费用相关SQL
                        try
                        {
                            for (i = 0; i < lds.Tables[0].Rows.Count; i++)
                            {
                                #region //费用ID号存在性验证


                                try
                                {
                                    haverecord = false;
                                    cmd.CommandText = "SELECT count(*) FROM mz_pricedetail where hospitalcode=('" + hospitalcode + "') and HospCode=('" + hospcode + "') and detailid=(" + lds.Tables[0].Rows[i]["fyid"].ToString() + ")";
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
                                    ls_sql = "UPDATE mz_pricedetail SET nhcode='" + lds.Tables[0].Rows[i]["nhdm"].ToString() + "',MarkType=" + lds.Tables[0].Rows[i]["fylx"].ToString();
                                    ls_sql += ",mediname='" + lds.Tables[0].Rows[i]["hisname"].ToString() + "',ddate='" + lds.Tables[0].Rows[i]["fysj"].ToString() + "',user_num=" + lds.Tables[0].Rows[i]["sl"].ToString() + ",unitprice=" + lds.Tables[0].Rows[i]["dj"].ToString() + ",price=" + lds.Tables[0].Rows[i]["je"].ToString() + ",mediid=" + lds.Tables[0].Rows[i]["hismlid"].ToString() + ",chkpayid=0";
                                    ls_sql += " WHERE hospitalcode=('" + hospitalcode + "') AND HospCode=('" + hospcode + "') AND detailid=" + lds.Tables[0].Rows[i]["fyid"].ToString();
                                    #endregion
                                }
                                else
                                {
                                    #region 生成做INSERT的SQL
                                    ls_sql = "INSERT INTO mz_pricedetail(hospitalcode,HospCode,detailid,nhcode,MarkType,";
                                    ls_sql += "mediname,ddate,user_num,unitprice,price,selffee,autochkfee,checkid,in_out,chkpayid,";
                                    ls_sql += "mediid)";
                                    ls_sql += " VALUES('" + hospitalcode + "','" + hospcode + "'," + lds.Tables[0].Rows[i]["fyid"].ToString() + ",'" + lds.Tables[0].Rows[i]["nhdm"].ToString() + "'," + lds.Tables[0].Rows[i]["fylx"].ToString() + ",";
                                    ls_sql += "'" + lds.Tables[0].Rows[i]["hisname"].ToString() + "','" + lds.Tables[0].Rows[i]["fysj"].ToString() + "'," + lds.Tables[0].Rows[i]["sl"].ToString() + "," + lds.Tables[0].Rows[i]["dj"].ToString() + "," + lds.Tables[0].Rows[i]["je"].ToString() + ",0.0,0.0,0,1,0," + lds.Tables[0].Rows[i]["hismlid"].ToString();
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
                                    return if0.getXMLErroStrFromString("9000", "HIS门诊费用数据更新到农合时异常：" + e.Message.ToString());
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
                            return if0.getXMLErroStrFromString("9000", "生成HIS门诊数据更新到农合的语句时异常：" + e.Message.ToString());
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
            retMessXML = if0.getXMLStrFromString("门诊号" + hospcode + "的门诊费用数据传输成功！");
        }
        return retMessXML;
    }

    /// <summary>
    /// 从农合系统里下载指定日期段的普通门诊(体检、其他)补偿数据
    /// </summary>
    /// <param name="hospitalcode"></param>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <param name="mztype"></param>0普通门诊（包括普通、门诊留观），1体检
    /// <param name="begindate"></param>
    /// <param name="enddate"></param>
    /// <param name="mess"></param>
    /// <returns></returns>
    public string getMZCommData(string hospitalcode, string servername, string dbname, string system, string user, string pwd, string mztype, string begindate, string enddate, out string mess)
    {
        string hisifurl = "", retMessXML = "", ls_tmp = "";
        mess="";
        if (system == "BS" && !if0.getWebServiceURL(servername, out hisifurl))
        {
            return if0.getXMLErroStrFromString("2000", hisifurl);
        }

        DataSet lds = new DataSet();
        
        ls_tmp = "SELECT h.PATIENTNAME as xm,h.hospcode as mzh,c.payorder as bch,c.grandPrice as je,c.nhpay as sjbj,c.subsum as dfje,h.deratesum as jm,case when c.chkpayid>0 then '已审核' else '未处理' end as shzt,";
        ls_tmp += "case when c.payid=0 then '未支付' else '已支付' end as dfbj,case when c.accountstate=0 then '未结算' else '已结算' end as jsbz,case when h.ifchina=0 then '否' else '是' end as zybj,";
        ls_tmp += "c.paytime as dfsj,h.autoprekey as sbid,c.inputtime as scsj FROM mz_checkpay c,mz_hosppatinfo2 h";
        ls_tmp += " WHERE c.payorder=h.payorder AND c.inputorg='" + hospitalcode + "' AND c.orderid<6000 AND h.dataresource=0";
        ls_tmp += " AND h.HospDate BETWEEN '" + begindate.Substring(0, 4) + "-" + begindate.Substring(4, 2) + "-" + begindate.Substring(6, 2) + " 00:00:00.000' AND '" + enddate.Substring(0, 4) + "-" + enddate.Substring(4, 2) + "-" + enddate.Substring(6, 2) + " 23:59:59.999'";
        if (mztype == "0") //非体检
        {
            ls_tmp += " AND h.hosptype<>30";
        }
        else //体检
        {
            ls_tmp += " AND h.hosptype=30";
        }

        //按照医院级别生成药品目录
        lds = if0.getDataSet(lds, ls_tmp, "t0", out retMessXML);
        try
        {
            if (retMessXML == "TRUE")
            {
                if (system == "BS")
                {
                    #region 向HIS接口发送数据


                    ls_tmp = "<begindate>" + begindate + "</begindate><enddate>" + enddate + "</enddate>";
                    if (mztype == "0") //非体检
                    {
                        ls_tmp += "<hosptype>0</hosptype>";
                    }
                    else
                    {
                        ls_tmp += "<hosptype>1</hosptype>";
                    }
                    if0.putDataToHIS(servername, dbname, user, pwd, lds, "4001", ls_tmp, out retMessXML, out lds);
                    return retMessXML;
                    #endregion
                }
                else
                {
                    #region 返回XML
                    if0.getRequestXMLStringFromDS(lds, "农合中普通门诊数据", out retMessXML, out mess);
                    return retMessXML;
                    #endregion
                }
            }
            else
            {
                retMessXML = if0.getXMLErroStrFromString("5000", retMessXML);
                return retMessXML;
            }
        }
        finally
        {
            if (lds != null)
                lds.Dispose();
        }
    }
    #endregion

    #region //普通门诊审核和补偿查询操作

    /// <summary>
    /// 计算指定门诊病人的每条费用的保内保外标志、费用分类、应补金额、预审金额


    /// </summary>
    /// <param name="payorder"></param>
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
    public bool mzCalcCompensate(string payorder, string hospitalcode, string hospid, string hosplevel, Int32 chkpayid, string operer, Int32 settle, DataSet dataDS, out DataSet feeDs, out string mess)
    {
        feeDs = dataDS;
        mess = "TRUE";
        #region //参数有效性检查


        if (chkpayid != 8 && chkpayid != 1)//&&chkpayid!=3
        {
            mess = "请求的审核状态未定义处理方法";
            return false;
        }
        if (hosplevel != "1" && hosplevel != "2" && hosplevel != "3" && hosplevel != "4" && hosplevel != "5" && hosplevel != "9")
        {
            mess = "请与系统管理员联系，检查医疗机构代码中的机构级别数据！";
            return false;
        }
        if (operer.Length == 0)
        {
            operer = UserID;
        }
        #endregion
        #region //变量定义
        string ls_nhcode, ls_typecode, ls_content, ls_excepted, ls_remark, ls_temp;//, ls_mediname, ls_mediname_pp;
        Int32 li_matchs = 0, li_rcmssershad = 0, li_rcmsmedishad = 0, li_row = -1, li_class = 0, li_mz_nhppb,li_count = 0;
        decimal ld_user_num, ld_unitprice, ld_price;//,ld_mediid;
        decimal ld_nhunitprice = 0.0M, ld_nhrate = 0.0M, ld_nhprice = 0.0M, ld_selffee = 0.0M;
        DateTime ldt_now;
        #endregion
        #region //得到病人的门诊费用信息


        string ls_sql;
        if (settle == 1)
        {
            ls_sql = "SELECT hospitalcode,HospCode,detailid,nhcode,MarkType,mediname,ddate,";
            ls_sql += "user_num,unitprice,price,selffee,autochkfee,";
            ls_sql += "checkid,in_out,chkpayid,prechkfee,prechkopr,";
            ls_sql += "prechktime,chkinitfee,chkinitopr,chkinittime,mediid";
            ls_sql += " FROM mz_pricedetail";
            ls_sql += " WHERE hospitalcode='" + hospitalcode + "' AND HospCode='" + hospid + "'";
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
            if (chkpayid != 1) //审核只是在平台调用，在前台界面中手工修改保内额保存


            {
                #region //智能计算把医院上传明细名称修改了的当“未匹配”处理的参数获取
                try
                {
                    cmd.CommandText = "SELECT matchs,rcmssershad,rcmsmedishad,getdate(),mz_nhppb FROM areasysparm WHERE areacode='" + AreaCode + "'";
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
                            li_mz_nhppb = myReader.GetInt32(4);
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
                        currrow["chkpayid"] = chkpayid;//Convert.ToInt32(feeDs.Tables["fees"].Rows[li_row - 1]["chkpayid"]);
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
                        if (Convert.ToDecimal(currrow["price"]) - ld_nhprice < 0)
                            ld_nhprice = Convert.ToDecimal(currrow["price"]);
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
                        }
                        else if (chkpayid == 1)
                        {
                            currrow["chkinitfee"] = ld_nhprice;
                            currrow["chkinitopr"] = operer;
                            currrow["chkinittime"] = ldt_now;
                        }
                        currrow["chkpayid"] = chkpayid;
                        currrow.EndEdit();
                        continue;
                    }
                    #endregion
                    ld_nhunitprice = 0.0M;
                    #region //修改了匹配时名称的按“未匹配”记——这个功能暂时不需要


                    //if (li_matchs == 0)
                    //{
                    //    ls_mediname = currrow["mediname"].ToString();
                    //    ld_mediid = Convert.ToDecimal(currrow["mediid"]);
                    //    cmd.Connection = Connection;
                    //    try
                    //    {
                    //        cmd.CommandText = "SELECT mediname FROM mz_nhppb WHERE orgcode='" + hospitalcode + "'";
                    //        cmd.CommandText += " AND mediid=" + ld_mediid.ToString() + "  AND Auditing>0";
                    //        ls_mediname_pp = Convert.ToString(cmd.ExecuteScalar());
                    //        if (ls_mediname_pp == null || ls_mediname_pp.Length == 0)
                    //        {
                    //            currrow.BeginEdit();
                    //            currrow["nhcode"] = "未匹配";
                    //            currrow.EndEdit();
                    //            //continue;
                    //        }
                    //        else
                    //        {
                    //            if (ls_mediname_pp.IndexOf(ls_mediname) > 0)
                    //            {
                    //                //插入到log_hosp_mediname表中
                    //                //记录该行审核结果
                    //                currrow.BeginEdit();
                    //                currrow["nhcode"] = "未匹配";
                    //                currrow.EndEdit();
                    //                //continue;
                    //            }
                    //        }
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        mess = "查询匹配表异常：" + e.Message.ToString();
                    //        return false;
                    //    }
                    //    finally
                    //    {
                    //        cmd.Dispose();
                    //    }
                    //}
                    #endregion
                    ls_nhcode = currrow["nhcode"].ToString();
                    #region //农合代码是“未匹配”的直接记录审核结果
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
                        cmd.CommandText = "SELECT typecode,codetype,rate1,price1,rate2,price2,rate3,price3,price4,rate4,price5,rate5,price6,rate6,content,excepted,remark FROM mz_rcmssermediarea WHERE areacode='" + AreaCode + "' AND nhcode='" + ls_nhcode + "'";
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
                                ls_content = myReader.GetString(14);
                                ls_excepted = myReader.GetString(15);
                                ls_remark = myReader.GetString(16);
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
                                else if (hosplevel == "4")
                                {
                                    ld_nhrate = myReader.GetDecimal(8);
                                    ld_nhunitprice = myReader.GetDecimal(9);
                                }
                                else if (hosplevel == "5")
                                {
                                    ld_nhrate = myReader.GetDecimal(10);
                                    ld_nhunitprice = myReader.GetDecimal(11);
                                }
                                else if (hosplevel == "9")
                                {
                                    ld_nhrate = myReader.GetDecimal(12);
                                    ld_nhunitprice = myReader.GetDecimal(13);
                                }
                                else
                                {
                                    mess = "医疗机构级别错误，请联系农合系统管理员";
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
                    #region //门诊匹配信息是否要审核才生效
                    if (li_mz_nhppb == 1) //需要匹配审核通过才生效

                    {
                        cmd.CommandText = "Select Count(*) From mz_nhppb Where orgcode = '" + hospitalcode + "' And mediid = " + currrow["mediid"].ToString()+ " And servercode = '" + ls_nhcode + "' And auditing > 0 ";
                        li_count = Convert.ToInt32(cmd.ExecuteScalar());
                        if (li_count == 0)
                        {
                            mess = "在本医院的门诊农合目录匹配信息中没有找到HIS项目代码(hismlid=" + currrow["mediid"] + ")的匹配信息或者匹配信息还没有审核通过，请先维护本医院的门诊农合目录匹配信息。";
                            return false;
                        }
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
                    }
                    else if (chkpayid == 1)
                    {
                        currrow["chkinitfee"] = ld_nhprice;
                        currrow["chkinitopr"] = operer;
                        currrow["chkinittime"] = ldt_now;
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
                    if (chkpayid != 1) //审核只是在平台调用，在前台界面中手工修改保内额保存


                    {
                        foreach (DataRow row in feeDs.Tables["fees"].Rows)
                        {
                            cmd.CommandText = "UPDATE mz_pricedetail SET";
                            cmd.CommandText += " selffee=" + row["selffee"].ToString() + ",autochkfee=" + row["autochkfee"].ToString();
                            cmd.CommandText += ",prechkfee=" + row["prechkfee"].ToString() + ",prechkopr='" + row["prechkopr"] + "'";
                            cmd.CommandText += ",prechktime='" + row["prechktime"].ToString() + "',chkinitfee=" + row["chkinitfee"].ToString();
                            cmd.CommandText += ",chkinitopr='" + row["chkinitopr"] + "',chkinittime='" + row["chkinittime"].ToString() + "'";
                            cmd.CommandText += ",chkpayid=" + row["chkpayid"].ToString() + ",checkid=" + row["checkid"].ToString();
                            //cmd.CommandText += ",checkid=" + row["checkid"].ToString();
                            cmd.CommandText += ",marktype=" + row["marktype"].ToString() + ",nhcode='" + row["nhcode"] + "'";
                            cmd.CommandText += ",in_out=" + row["in_out"].ToString();
                            cmd.CommandText += " WHERE hospitalcode='" + hospitalcode + "' AND HospCode='" + hospid + "' AND detailid=" + row["detailid"].ToString();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    cmd.CommandText = "UPDATE mz_hosppatinfo2 SET grandprice=p.price,mediprice=p.mprice,mediselffee=p.mself,materprice=p.maprice,materselffee=p.maself,serverprice=p.sprice,serverselffee=p.sself";
                    cmd.CommandText += " FROM (SELECT hospitalcode,HospCode,sum(price) as price,sum(case marktype when 0 then price else 0.0 end) as mprice,sum(case marktype when 0 then selffee else 0.0 end) as mself,";
                    cmd.CommandText += "sum(case marktype when 1 then price else 0.0 end) as maprice,sum(case marktype when 1 then selffee else 0.0 end) as maself,";
                    cmd.CommandText += "sum(case marktype when 2 then price else 0.0 end) as sprice,sum(case marktype when 2 then selffee else 0.0 end) as sself";
                    cmd.CommandText += " FROM mz_pricedetail WHERE hospitalcode='" + hospitalcode + "' AND HospCode='" + hospid + "' GROUP BY hospitalcode,HospCode) p";
                    cmd.CommandText += " WHERE p.hospitalcode=mz_hosppatinfo2.hospitalcode AND p.HospCode=mz_hosppatinfo2.HospCode AND mz_hosppatinfo2.hospitalcode='" + hospitalcode + "' AND mz_hosppatinfo2.HospCode='" + hospid + "'";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "UPDATE mz_checkpay SET grandprice=h.price,mediprice=h.mprice,mediselffee=h.mself,materprice=h.maprice,materselffee=h.maself,serverprice=h.sprice,serverselffee=h.sself";
                    cmd.CommandText += " FROM (SELECT payorder,hospitalcode,HospCode,sum(grandprice) as price,sum(mediprice) as mprice,sum(mediselffee) as mself,";
                    cmd.CommandText += "sum(materprice) as maprice,sum(materselffee) as maself,";
                    cmd.CommandText += "sum(serverprice) as sprice,sum(serverselffee) as sself";
                    cmd.CommandText += " FROM mz_hosppatinfo2 WHERE hospitalcode='" + hospitalcode + "' AND HospCode='" + hospid + "' GROUP BY payorder,hospitalcode,HospCode) h";
                    cmd.CommandText += " WHERE h.payorder=mz_checkpay.payorder AND h.hospitalcode='" + hospitalcode + "' AND h.HospCode='" + hospid + "'";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM mz_pataccountdetail WHERE (hospitalcode='" + hospitalcode + "' AND HospCode='" + hospid + "') AND (chkpayid=0 OR chkpayid=" + chkpayid.ToString() + ")";
                    cmd.ExecuteNonQuery();
                    //请注意，以下SQL是假定没有“未匹配”的情况下的，因此，未匹配费都为被设置为0.0
                    cmd.CommandText = "INSERT INTO mz_pataccountdetail(hospitalcode,HospCode,chkpayid,payorder,fee1,fee2,fee3,fee4,fee5,fee6,";
                    cmd.CommandText += "fee7,fee8,fee9,fee10,fee11,fee12,fee13,fee14,fee15,fee16,calcinfo)";
                    cmd.CommandText += " SELECT '" + hospitalcode + "','" + hospid + "',0,'" + payorder + "',isnull(sum(case when r.typesname='西药部分' or r.typesname='抗微生物药' then price else 0.0 end),0.0),";
                    cmd.CommandText += "isnull(sum(case when r.typesname='中成药' or r.typesname='中成药部分' then price else 0.0 end),0.0),isnull(sum(case when r.typesname='中药饮片' or r.typesname='中药饮片部分' then price else 0.0 end),0.0),";
                    cmd.CommandText += "isnull(sum(case when r.typesname='材料' or r.typesname='材料类' then price else 0.0 end),0.0),isnull(sum(case when r.typesname='挂号费' then price else 0.0 end),0.0),";
                    cmd.CommandText += "isnull(sum(case when r.typesname='床位费' then price else 0.0 end),0.0),isnull(sum(case when r.typesname='诊查费' then price else 0.0 end),0.0),";
                    cmd.CommandText += "isnull(sum(case when r.typesname='检查费' then price else 0.0 end),0.0),isnull(sum(case when r.typesname='治疗费' then price else 0.0 end),0.0),";
                    cmd.CommandText += "isnull(sum(case when r.typesname='护理费' then price else 0.0 end),0.0),isnull(sum(case when r.typesname='手术费' then price else 0.0 end),0.0),";
                    cmd.CommandText += "isnull(sum(case when r.typesname='化验费' then price else 0.0 end),0.0),isnull(sum(case when r.typesname not in ('西药部分','抗微生物药','中成药','中成药部分','中药饮片','中药饮片部分','材料','材料类','挂号费','床位费','诊查费','检查费','治疗费','护理费','手术费','化验费','特殊材料费') then price else 0.0 end),0.0),";
                    cmd.CommandText += "isnull(sum(case when r.typesname='特殊材料费' then price else 0.0 end),0.0),0.0,isnull(sum(case when r.typesname='抗微生物药' then price else 0.0 end),0.0),''";
                    cmd.CommandText += " FROM mz_pricedetail p,mz_rcmssermediarea r WHERE r.areacode='" + AreaCode + "' AND p.nhcode=r.nhcode AND p.hospitalcode='" + hospitalcode + "' AND p.HospCode='" + hospid + "'";
                    cmd.ExecuteNonQuery();
                    if (chkpayid == 1)
                    {
                        ls_temp = "chkinitfee";
                    }
                    else
                    {
                        ls_temp = "prechkfee";
                    }
                    cmd.CommandText = "INSERT INTO mz_pataccountdetail(hospitalcode,HospCode,chkpayid,payorder,fee1,fee2,fee3,fee4,fee5,fee6,";
                    cmd.CommandText += "fee7,fee8,fee9,fee10,fee11,fee12,fee13,fee14,fee15,fee16,calcinfo)";
                    cmd.CommandText += " SELECT '" + hospitalcode + "','" + hospid + "'," + chkpayid.ToString() + ",'" + payorder + "',isnull(sum(case when r.typesname='西药部分' or r.typesname='抗微生物药' then p." + ls_temp + " else 0.0 end),0.0),";
                    cmd.CommandText += "isnull(sum(case when r.typesname='中成药' or r.typesname='中成药部分' then p." + ls_temp + " else 0.0 end),0.0),isnull(sum(case when r.typesname='中药饮片' or r.typesname='中药饮片部分' then p." + ls_temp + " else 0.0 end),0.0),";
                    cmd.CommandText += "isnull(sum(case when r.typesname='材料' or r.typesname='材料类' then p." + ls_temp + " else 0.0 end),0.0),isnull(sum(case when r.typesname='挂号费' then p." + ls_temp + " else 0.0 end),0.0),";
                    cmd.CommandText += "isnull(sum(case when r.typesname='床位费' then p." + ls_temp + " else 0.0 end),0.0),isnull(sum(case when r.typesname='诊查费' then p." + ls_temp + " else 0.0 end),0.0),";
                    cmd.CommandText += "isnull(sum(case when r.typesname='检查费' then p." + ls_temp + " else 0.0 end),0.0),isnull(sum(case when r.typesname='治疗费' then p." + ls_temp + " else 0.0 end),0.0),";
                    cmd.CommandText += "isnull(sum(case when r.typesname='护理费' then p." + ls_temp + " else 0.0 end),0.0),isnull(sum(case when r.typesname='手术费' then p." + ls_temp + " else 0.0 end),0.0),";
                    cmd.CommandText += "isnull(sum(case when r.typesname='化验费' then p." + ls_temp + " else 0.0 end),0.0),isnull(sum(case when r.typesname not in ('西药部分','抗微生物药','中成药','中成药部分','中药饮片','中药饮片部分','材料','材料类','挂号费','床位费','诊查费','检查费','治疗费','护理费','手术费','化验费','特殊材料费') then p." + ls_temp + " else 0.0 end),0.0),";
                    cmd.CommandText += "isnull(sum(case when r.typesname='特殊材料费' then p." + ls_temp + " else 0.0 end),0.0),0.0,isnull(sum(case when r.typesname='抗微生物药' then p." + ls_temp + " else 0.0 end),0.0),''";
                    cmd.CommandText += " FROM mz_pricedetail p,mz_rcmssermediarea r WHERE r.areacode='" + AreaCode + "' AND p.nhcode=r.nhcode AND p.hospitalcode='" + hospitalcode + "' AND p.HospCode='" + hospid + "'";
                    cmd.ExecuteNonQuery();
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
            else
            {
                #region //更新数据集上的字段


                decimal ld_mediprice = 0.0M, ld_mediselffee = 0.0M, ld_materprice = 0.0M, ld_materselffee = 0.0M, ld_serverprice = 0.0M, ld_serverselffee = 0.0M;
                foreach (DataRow feerow in feeDs.Tables["fees"].Rows)
                {
                    switch (Convert.ToInt32(feerow["marktype"]))
                    {
                        case 0://药品
                            ld_mediprice += Convert.ToDecimal(feerow["price"]);
                            ld_mediselffee += Convert.ToDecimal(feerow["selffee"]);
                            break;
                        case 1://材料
                            ld_materprice += Convert.ToDecimal(feerow["price"]);
                            ld_materselffee += Convert.ToDecimal(feerow["selffee"]);
                            break;
                        case 2://医疗服务
                            ld_serverprice += Convert.ToDecimal(feerow["price"]);
                            ld_serverselffee += Convert.ToDecimal(feerow["selffee"]);
                            break;
                    }
                }
                feeDs.Tables["mzhosppatinfo2"].Rows[0]["grandPrice"] = ld_mediprice + ld_materprice + ld_serverprice;
                feeDs.Tables["mzhosppatinfo2"].Rows[0]["mediprice"] = ld_mediprice;
                feeDs.Tables["mzhosppatinfo2"].Rows[0]["mediselffee"] = ld_mediselffee;
                feeDs.Tables["mzhosppatinfo2"].Rows[0]["materprice"] = ld_materprice;
                feeDs.Tables["mzhosppatinfo2"].Rows[0]["materselffee"] = ld_materselffee;
                feeDs.Tables["mzhosppatinfo2"].Rows[0]["serverprice"] = ld_serverprice;
                feeDs.Tables["mzhosppatinfo2"].Rows[0]["serverselffee"] = ld_serverselffee;
                feeDs.Tables["mzcheckpay"].Rows[0]["grandPrice"] = ld_mediprice + ld_materprice + ld_serverprice;
                feeDs.Tables["mzcheckpay"].Rows[0]["mediprice"] = ld_mediprice;
                feeDs.Tables["mzcheckpay"].Rows[0]["mediselffee"] = ld_mediselffee;
                feeDs.Tables["mzcheckpay"].Rows[0]["materprice"] = ld_materprice;
                feeDs.Tables["mzcheckpay"].Rows[0]["materselffee"] = ld_materselffee;
                feeDs.Tables["mzcheckpay"].Rows[0]["serverprice"] = ld_serverprice;
                feeDs.Tables["mzcheckpay"].Rows[0]["serverselffee"] = ld_serverselffee;
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

    /// <summary>
    /// 计算指定门诊病人的补偿金额


    /// </summary>
    /// <param name="payorder"></param>
    /// <param name="patareacode2"></param>
    /// <param name="hosplevel"></param>
    /// <param name="hospareacode"></param>
    /// <param name="hospareacode2"></param>
    /// <param name="chkpayid"></param> //请求进行的业务，8请求预审，1请求审核
    /// <param name="operer"></param>
    /// <param name="settle"></param>
    /// <param name="dataDS"></param>
    /// <param name="feeDs"></param>
    /// <param name="ad_assfee"></param>
    /// <param name="ad_familyfound"></param>
    /// <param name="ad_nhpay"></param>
    /// <param name="ad_familyaccbalance"></param>
    /// <param name="mess"></param>
    /// <returns></returns> //返回true时，mess是补偿计算描述文本；返回false时mess是发生的错误描述
    public bool mzCalcPayFee(string payorder, string patareacode2, string hosplevel, string hospareacode, string hospareacode2, Int32 chkpayid, string operer, Int32 settle, DataSet dataDS, out DataSet feeDs, out decimal ad_assfee, out decimal ad_familyfound, out decimal ad_nhpay, out decimal ad_familyaccbalance, out string mess)
    {
        feeDs = dataDS;
        ad_assfee = 0.0M;//应补金额
        ad_familyfound = 0.0M;//家庭帐户补偿金额
        ad_nhpay = 0.0M;//实补金额
        ad_familyaccbalance = 0.0M;//家庭帐户余额
        mess = "";//审核过程描述
        #region //参数有效性检查


        if (patareacode2.Length != 6)
        {
            mess = "缺少病人所在村组参数";
            return false;
        }
        if (hospareacode.Length != 6 || hospareacode2.Length != 6)
        {
            mess = "缺少医院所在村组参数";
            return false;
        }
        if (chkpayid != 8 && chkpayid != 1)//&&chkpayid!=3
        {
            mess = "请求的审核状态未定义处理方法";
            return false;
        }
        if (hosplevel != "1" && hosplevel != "2" && hosplevel != "3" && hosplevel != "4" && hosplevel != "5" && hosplevel != "9")
        {
            mess = "请与系统管理员联系，检查医疗机构代码中的机构级别数据！";
            return false;
        }
        #endregion
        string ls_areacode = "", ls_hospitalcode = "", ls_hospdate = "";
        DateTime ldt_hospdate,ldt_date;
        Int32 li_hosptype, li_temp;
        decimal ld_familyfee, ld_fee;
        Int32 ll_chkprecision, ll_precisionmode, ll_retfamilyfund;
        string ls_retfamilyfunddate, ls_sql, ls_familyno, ls_statisticyear, ls_human;
        string ls_days3, ls_days2, ls_days1,ls_days0;
        Int32 li_days3, li_days2, li_days1,li_days0;
        decimal ldec_days3, ldec_days2, ldec_days1,ldec_days0, ldec_fee_all;
        decimal ldec_nhpay3,ldec_nhpay2,ldec_nhpay1,ldec_nhpay0,ldec_nhpay_all;


        if (operer.Length == 0)
        {
            operer = UserID;
        }
        //DataSet feeDs = null; //建立DataSet对象
        #region //得到病人的补偿记录，就诊记录，补偿政策。非特殊慢病门诊病人记录只能一条。至少得有一条补偿政策(计算按排序的第一条政策来计算)
        try
        {
            if (settle == 1)
            {
                //------------------------------查询补偿记录
                ls_sql = "SELECT human,statisticyear,areacode2,payorder,grandPrice,assfee,familyfound,paydate,nhpay,mediprice,";
                ls_sql += "mediselffee,materprice,materselffee,serverprice,serverselffee,chkpayid,prechkfee,prechkopr,prechktime,chkinitsum,";
                ls_sql += "chkinitopr,chkinittime,calcid,accountstate,orderid,payid,organpay,paytime,calcinfo,inputor,";
                ls_sql += "inputtime,lockstate,ifaccount,subsum,subopr,subtime,chkpayid";
                ls_sql += " FROM mz_checkpay";
                ls_sql += " WHERE payorder='" + payorder + "'";
                //补偿记录只能一条            
                feeDs = if0.getDataSet(ls_sql, "mzcheckpay", out mess);
            }
            if (feeDs.Tables["mzcheckpay"].Rows.Count != 1)
            {
                if (feeDs.Tables["mzcheckpay"].Rows.Count == 0)
                {
                    mess = "没有查询到补偿记录";
                }
                else if (feeDs.Tables["mzcheckpay"].Rows.Count > 1)
                {
                    mess = "补偿号异常，同一个补偿号查询到多条补偿记录，请联系农合系统管理员处理";
                }
                feeDs.Dispose();
                return false;
            }
            if (settle == 1)
            {
                //------------------------------查询门诊病人记录
                ls_sql = "SELECT hospitalcode,HospCode,requoffice1,doctor,PATIENTNAME,HospDate,patstate,hosptype,HospResu,grandPrice,";
                ls_sql += "deratesum,inpatientcode,human,statisticyear,areacode2,patsort,ifchina,assfee,familyfound,nhpay,";
                ls_sql += "mediprice,mediselffee,materprice,materselffee,serverprice,serverselffee,chkpayid,payorder,dataresource,areacode,autoprekey";
                ls_sql += " FROM mz_hosppatinfo2";
                ls_sql += " WHERE payorder='" + payorder + "' AND hosptype<>20";
                ls_sql += " ORDER BY hospitalcode ASC,HospCode ASC";
                feeDs = if0.getDataSet(feeDs, ls_sql, "mzhosppatinfo2", out mess);
            }
            //非特殊慢病门诊病人记录只能一条


            if (feeDs.Tables["mzhosppatinfo2"].Rows.Count != 1)
            {
                if (feeDs.Tables["mzhosppatinfo2"].Rows.Count == 0)
                {
                    mess = "没有查询到门诊病人数据";
                }
                else if (feeDs.Tables["mzhosppatinfo2"].Rows.Count > 1)
                {
                    mess = "补偿号异常，同一个补偿号查询到多条普通门诊病人数据，请联系农合系统管理员处理";
                }
                feeDs.Dispose();
                return false;
            }
            //变量赋值


            ls_areacode = Convert.ToString(feeDs.Tables["mzhosppatinfo2"].Rows[0]["areacode"]);
            ls_hospitalcode = Convert.ToString(feeDs.Tables["mzhosppatinfo2"].Rows[0]["hospitalcode"]);
            ldt_hospdate = Convert.ToDateTime(feeDs.Tables["mzhosppatinfo2"].Rows[0]["HospDate"]);
            ls_hospdate = string.Format("{0:yyyyMMdd}", ldt_hospdate);
            li_hosptype = Convert.ToInt32(feeDs.Tables["mzhosppatinfo2"].Rows[0]["hosptype"]);
            //------------------------------查询相应门诊补偿政策记录
            if (ls_areacode != "430481") //耒阳的怪政策


            {
                ls_sql = "SELECT orderid,areacode,levelcode,organcode,validbegin,validend,feetype,startline,subsidyrate,toptype,";
                ls_sql += "a1,a2,a3,zystartline,zysubsidyrate,zytoptype,b1,b2,b3,inputor,inputtime,chkid,chkinit,chkinittime,";
                ls_sql += "chkopr,chktime,hadused,hosptype,payarea,ifbalance";
                ls_sql += " FROM mz_arearcmsparm";
                ls_sql += " WHERE (areacode='" + ls_areacode + "' AND levelcode='" + hosplevel + "') AND (organcode='' OR organcode='" + ls_hospitalcode + "') AND hosptype = " + li_hosptype.ToString() + " AND (validbegin<='" + ls_hospdate + "' AND validend>='" + ls_hospdate + "' AND chkid=2)";
                ls_sql += " ORDER BY case isnull(rtrim(organcode),'') when '' then 'zzzzzzzzzzzzzz' else organcode end ASC";
                feeDs = if0.getDataSet(feeDs, ls_sql, "mzarearcmsparm", out mess);
                //至少得有一条补偿政策(计算按排序的第一条政策来计算)
                if (feeDs.Tables["mzarearcmsparm"].Rows.Count == 0)
                {
                    mess = "没有查询到对应的补偿政策数据，请联系合作医疗主管部门配置";
                    feeDs.Dispose();
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            mess = "查询门诊资料和补偿政策异常：" + e.Message.ToString();
            return false;
        }
        finally
        {
            feeDs.EnforceConstraints = true;
        }
        #endregion
        #region //根据该病人所在区域看是否政策允许该病人补助


        if (ls_areacode != "430481") //耒阳的怪政策


        {
            switch (Convert.ToInt32(feeDs.Tables["mzarearcmsparm"].Rows[0]["payarea"])) //可补偿区域0不限制10本县参合者20本乡镇参合者30本村参合者


            {
                case 0:
                    break;
                case 10:
                    if (hospareacode != AreaCode)
                    {
                        mess = "政策规定门诊只能补偿医院所在县参合者";
                        return false;
                    }
                    break;
                case 20:
                    if (hospareacode != AreaCode || patareacode2.Substring(0, 2) != hospareacode2.Substring(0, 2))
                    {
                        mess = "政策规定门诊只能补偿医院所在乡镇参合者";
                        return false;
                    }
                    break;
                case 30:
                    if (hospareacode != AreaCode || patareacode2.Substring(0, 4) != hospareacode2.Substring(0, 4))
                    {
                        mess = "政策规定门诊只能补偿医院所在村参合者";
                        return false;
                    }
                    break;
                default:
                    mess = "政策中包含未定义的可补偿区域参数值，请联系开发人员!";
                    return false;
            }
        }
        #endregion
        mess = "";
        SqlConnection Connection = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
        SqlCommand cmd = new SqlCommand();
        SqlDataReader myReader;
        try
        {
            Connection.Open();
            cmd.Connection = Connection;
            #region 验证申报ID号是否有效，验证体检的费用与申报体检包的费用是否一致


            if (li_hosptype == 30)
            {
                #region 申报ID号有效性判断


                ls_human = Convert.ToString(feeDs.Tables["mzhosppatinfo2"].Rows[0]["human"]);
                ls_statisticyear = Convert.ToString(feeDs.Tables["mzhosppatinfo2"].Rows[0]["statisticyear"]);
                if (settle == 1)
                {
                    li_temp = Convert.ToInt32(feeDs.Tables["mzcheckpay"].Rows[0]["chkpayid"]);
                }
                else
                {
                    li_temp = 0;
                }
                if (!checkTJAskedid(Convert.ToString(feeDs.Tables["mzhosppatinfo2"].Rows[0]["areacode"]), Convert.ToString(feeDs.Tables["mzhosppatinfo2"].Rows[0]["autoprekey"]), ls_hospitalcode, ls_statisticyear, ls_human, li_temp, out mess)) //验证申报ID号的有效性


                {
                    return false;
                }
                #endregion
                ld_fee = Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["mediprice"]);
                ld_fee += Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["materprice"]);
                ld_fee += Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["serverprice"]);
                if (!checkTJPacketFee(Connection, ls_hospitalcode, ld_fee, Convert.ToString(feeDs.Tables["mzhosppatinfo2"].Rows[0]["autoprekey"]), out mess))
                {
                    return false;
                }
            }
            #endregion
            #region //查参合启用参数、每人缴款额、金额小数取舍方法等系统参数
            try
            {
                cmd.CommandText = "SELECT familyfee,chkprecision,precisionmode,retfamilyfund,retfamilyfunddate FROM areasysparm WHERE areacode='" + AreaCode + "'";
                myReader = cmd.ExecuteReader();
                try
                {
                    if (!myReader.HasRows)
                    {
                        mess = "审核计算时查询系统参数数据库出错，不能继续！";
                        return false;
                    }
                    else
                    {
                        myReader.Read();
                        //ld_familyfee = myReader.GetDecimal(0);
                        ll_chkprecision = myReader.GetInt32(1);
                        ll_precisionmode = myReader.GetInt32(2);
                        ll_retfamilyfund = myReader.GetInt32(3);
                        ls_retfamilyfunddate = myReader.GetString(4);
                        if (!(ll_retfamilyfund == 1 && int.Parse(ls_hospdate) >= int.Parse(ls_retfamilyfunddate)))
                        {
                            ld_familyfee = 0.0M; //判断是否使用家庭帐户 设置成零后,下面的程序将不进行家庭帐户的使用
                            //在普通门诊政策中增加是否使用家庭账户的设置，此标志作废


                        }
                        else
                        {
                            ld_familyfee = 1.0M; //参数设置为启用了家庭账户的


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
                mess = "审核计算时查询系统参数异常：" + e.Message.ToString();
                return false;
            }
            finally
            {
                cmd.Dispose();
            }
            #endregion
            #region //设置使用的政策号和政策使用标志


            if (ls_areacode != "430481") //耒阳的怪政策


            {
                feeDs.Tables["mzcheckpay"].Rows[0]["orderid"] = feeDs.Tables["mzarearcmsparm"].Rows[0]["orderid"];
            }
            else
            {
                feeDs.Tables["mzcheckpay"].Rows[0]["orderid"] = 0;
            }
            #endregion
            decimal ld_startline, ld_subsidyrate, ld_a1, ld_a2, ld_a3;
            Int32 ll_toptype, ll_ifbalance = -1;
            #region //获取政策参数变量值


            if (ls_areacode != "430481") //耒阳的怪政策


            {
                ll_ifbalance = Convert.ToInt32(feeDs.Tables["mzarearcmsparm"].Rows[0]["ifbalance"]);
                switch (Convert.ToInt32(feeDs.Tables["mzhosppatinfo2"].Rows[0]["ifchina"])) //是否中医药治疗0:否 1:是


                {
                    case 0:
                        ld_startline = Convert.ToDecimal(feeDs.Tables["mzarearcmsparm"].Rows[0]["startline"]);
                        ld_subsidyrate = Convert.ToDecimal(feeDs.Tables["mzarearcmsparm"].Rows[0]["subsidyrate"]);
                        ll_toptype = Convert.ToInt32(feeDs.Tables["mzarearcmsparm"].Rows[0]["toptype"]);
                        ld_a1 = Convert.ToDecimal(feeDs.Tables["mzarearcmsparm"].Rows[0]["a1"]);
                        ld_a2 = Convert.ToDecimal(feeDs.Tables["mzarearcmsparm"].Rows[0]["a2"]);
                        ld_a3 = Convert.ToDecimal(feeDs.Tables["mzarearcmsparm"].Rows[0]["a3"]);
                        break;
                    case 1:
                        ld_startline = Convert.ToDecimal(feeDs.Tables["mzarearcmsparm"].Rows[0]["zystartline"]);
                        ld_subsidyrate = Convert.ToDecimal(feeDs.Tables["mzarearcmsparm"].Rows[0]["zysubsidyrate"]);
                        ll_toptype = Convert.ToInt32(feeDs.Tables["mzarearcmsparm"].Rows[0]["zytoptype"]);
                        ld_a1 = Convert.ToDecimal(feeDs.Tables["mzarearcmsparm"].Rows[0]["b1"]);
                        ld_a2 = Convert.ToDecimal(feeDs.Tables["mzarearcmsparm"].Rows[0]["b2"]);
                        ld_a3 = Convert.ToDecimal(feeDs.Tables["mzarearcmsparm"].Rows[0]["b3"]);
                        break;
                    default:
                        mess = "没有定义的治疗类型，请联系开发人员!";
                        return false;
                }
            }
            else
            {
                ld_startline = 0.0M;
                ld_subsidyrate = 0.0M;
                ll_toptype = 0;
                ld_a1 = 0.0M;
                ld_a2 = 0.0M;
                ld_a3 = 0.0M;
            }
            #endregion
            #region //查询家庭帐户余额ad_familyaccbalance
            ls_human = Convert.ToString(feeDs.Tables["mzhosppatinfo2"].Rows[0]["human"]);
            ls_statisticyear = Convert.ToString(feeDs.Tables["mzhosppatinfo2"].Rows[0]["statisticyear"]);
            if (ld_familyfee != 0.0M) //启用了家庭账户的，则查家庭账户余额


            {
                ls_sql = "SELECT f.familyaccbalance FROM n_familycwuchkkp f,n_individdata i WHERE f.familyno=i.familyno AND f.statisticyear=i.statisticyear AND i.human='" + ls_human + "' AND i.statisticyear='" + ls_statisticyear + "'";
                cmd.CommandText = ls_sql;
                try
                {
                    ad_familyaccbalance = Convert.ToDecimal(cmd.ExecuteScalar());
                }
                catch (Exception e)
                {
                    mess = "查询家庭帐户余额异常:" + e.Message.ToString();
                    return false;
                }
            }
            else
            {
                ad_familyaccbalance = 0.0M;
            }
            #endregion
            feeDs.Tables["mzcheckpay"].Rows[0]["calcid"] = 4; //正常
            if (ls_areacode == "430481") //耒阳的怪政策


            {
                #region //耒阳的怪政策


                ld_fee = Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["mediprice"]) - Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["mediselffee"]);
                ld_fee += Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["materprice"]) - Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["materselffee"]);
                ld_fee += Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["serverprice"]) - Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["serverselffee"]);
                if (mess != "")
                {
                    mess += ";";
                }
                mess += "保内额:" + ld_fee.ToString("0.##");  
              
                li_days3 = 0;
                li_days2 = 0;
                li_days1 = 0;
                li_days0 = 0;
                ldec_days3 = 0.0M;
                ldec_days2 = 0.0M;
                ldec_days1 = 0.0M;
                ldec_days0 = 0.0M;
                ldec_fee_all = 0.0M;
                ldec_nhpay3 = 0.0M;
                ldec_nhpay2 = 0.0M;
                ldec_nhpay1 = 0.0M;
                ldec_nhpay0 = 0.0M;
                ldec_nhpay_all = 0.0M;

                #region //得到系统当前时间
                //cmd.CommandText = " Select Top 1 GetDate() From sysarea Where areacode = '" + ls_areacode + "'";
                //try
                //{
                //    ldt_date = Convert.ToDateTime(cmd.ExecuteScalar());
                //}
                //catch (Exception e)
                //{
                //    mess = "查询系统当前时间异常:" + e.Message.ToString();
                //    return false;
                //}
                #endregion

                #region //查询近三天的普通门诊的累计补偿金额,以就诊时间为准。


                ldt_date = ldt_hospdate;
                ls_days3 = string.Format("{0:yyyy-MM-dd}", ldt_date.AddDays(-3));
                ls_days2 = string.Format("{0:yyyy-MM-dd}", ldt_date.AddDays(-2));
                ls_days1 = string.Format("{0:yyyy-MM-dd}", ldt_date.AddDays(-1));
                ls_days0 = string.Format("{0:yyyy-MM-dd}", ldt_date);                

                ls_sql = "Select IsNull(Sum(Case When Convert(VarChar(10),b.hospdate,120) = '" + ls_days3 + "' Then 1 Else 0 End),0), ";
                ls_sql += "IsNull(Sum(Case When Convert(VarChar(10),b.hospdate,120) = '" + ls_days2 + "' Then 1 Else 0 End),0), ";
                ls_sql += "IsNull(Sum(Case When Convert(VarChar(10),b.hospdate,120) = '" + ls_days1 + "' Then 1 Else 0 End),0), ";
                ls_sql += "IsNull(Sum(Case When Convert(VarChar(10),b.hospdate,120) = '" + ls_days0 + "' Then 1 Else 0 End),0), ";
                ls_sql += "IsNull(Sum(Case When Convert(VarChar(10),b.hospdate,120) = '" + ls_days3 + "' Then IsNull(b.grandprice,0) - (IsNull(b.mediselffee,0)+ IsNull(b.materselffee,0) + IsNull(b.serverselffee,0)) Else 0 End),0), ";
                ls_sql += "IsNull(Sum(Case When Convert(VarChar(10),b.hospdate,120) = '" + ls_days2 + "' Then IsNull(b.grandprice,0) - (IsNull(b.mediselffee,0)+ IsNull(b.materselffee,0) + IsNull(b.serverselffee,0)) Else 0 End),0), ";
                ls_sql += "IsNull(Sum(Case When Convert(VarChar(10),b.hospdate,120) = '" + ls_days1 + "' Then IsNull(b.grandprice,0) - (IsNull(b.mediselffee,0)+ IsNull(b.materselffee,0) + IsNull(b.serverselffee,0)) Else 0 End),0),  ";
                ls_sql += "IsNull(Sum(Case When Convert(VarChar(10),b.hospdate,120) = '" + ls_days0 + "' Then IsNull(b.grandprice,0) - (IsNull(b.mediselffee,0)+ IsNull(b.materselffee,0) + IsNull(b.serverselffee,0)) Else 0 End),0),  ";
                ls_sql += "IsNull(Sum(Case When Convert(VarChar(10),b.hospdate,120) = '" + ls_days3 + "' Then IsNull(a.subsum,0) Else 0 End),0), ";
                ls_sql += "IsNull(Sum(Case When Convert(VarChar(10),b.hospdate,120) = '" + ls_days2 + "' Then IsNull(a.subsum,0) Else 0 End),0), ";
                ls_sql += "IsNull(Sum(Case When Convert(VarChar(10),b.hospdate,120) = '" + ls_days1 + "' Then IsNull(a.subsum,0) Else 0 End),0), ";
                ls_sql += "IsNull(Sum(Case When Convert(VarChar(10),b.hospdate,120) = '" + ls_days0 + "' Then IsNull(a.subsum,0) Else 0 End),0) ";
                ls_sql += "From mz_checkpay a,mz_hosppatinfo2 b ";
                ls_sql += "Where a.human = '" + ls_human + "' And a.statisticyear ='" + ls_statisticyear + "' ";
                ls_sql += "And a.payorder = b.payorder And b.hospitalcode = '"+ls_hospitalcode+"' And a.chkpayid > 0 And a.payid = 1 ";
                ls_sql += "And b.hospdate Between '" + ls_days3 + " 00:00:00' And '" + ls_days0 + " 23:59:59' ";
                //ls_sql += "And Convert(VarChar(10),b.hospdate,120) >= '" + ls_days3 + "' ";
                //ls_sql += "And Convert(VarChar(10),b.hospdate,120) <= '" + ls_days0 + "' ";

                try
                {
                    cmd.CommandText = ls_sql;
                    myReader = cmd.ExecuteReader();
                    try
                    {
                        if (!myReader.HasRows)
                        {
                            mess = "查询该病人最近三天补偿情况信息失败，不能继续！";
                            return false;
                        }
                        else
                        {
                            myReader.Read();
                            li_days3 = myReader.GetInt32(0);
                            li_days2 = myReader.GetInt32(1);
                            li_days1 = myReader.GetInt32(2);
                            li_days0 = myReader.GetInt32(3);
                            ldec_days3 = myReader.GetDecimal(4);
                            ldec_days2 = myReader.GetDecimal(5);
                            ldec_days1 = myReader.GetDecimal(6);
                            ldec_days0 = myReader.GetDecimal(7);
                            ldec_nhpay3 = myReader.GetDecimal(8);
                            ldec_nhpay2 = myReader.GetDecimal(9);
                            ldec_nhpay1 = myReader.GetDecimal(10);
                            ldec_nhpay0 = myReader.GetDecimal(11);                            
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
                    mess = "查询该病人最近三天补偿情况信息出现异常：" + e.Message.ToString();
                    return false;
                }
                finally
                {
                    cmd.Dispose();
                }

                ldec_fee_all = 0.0M;
                ldec_nhpay_all = 0.0M;
                //前三天都有报销,则当天为第一天。三天为一个循环



                if (li_days3 > 0 && li_days2 > 0 && li_days1 > 0) 
		        {
                    ldec_fee_all = 0.0M;
                    ldec_nhpay_all = 0.0M;
                }
	            
                //前两天有报销
                if (li_days3 == 0 && li_days2 > 0 && li_days1 > 0)
		        {    
		            ldec_fee_all = ldec_days2 + ldec_days1;
                    ldec_nhpay_all = ldec_nhpay2 + ldec_nhpay1;
                }
	            
                //前一天有报销
                if (li_days2 == 0 && li_days1 > 0)
                {
                    ldec_fee_all = ldec_days1;
                    ldec_nhpay_all = ldec_nhpay1;
	            }
                
                //加上今天的


	            ldec_fee_all += ldec_days0;
                ldec_nhpay_all += ldec_nhpay0;

                if (mess != "")
                {
                    mess += ";";
                }
                mess += "三天累计保内额:" + ldec_fee_all.ToString("0.##")+";三天累计补助额:"+ldec_nhpay_all.ToString("0.##")+";";
                ldec_fee_all += ld_fee;
                #endregion

                if (ldec_fee_all < 0.0M)
                {
                    ad_assfee = 0.0M;
                    if (mess != "")
                    {
                        mess += ";";
                    }
                    mess += "保内额小于0，补助额=0"+";";
                    //+ ad_nhpay.ToString("0.##");
                }
                else if (ldec_fee_all <= 100.0M)
                {
                    ad_assfee = ldec_fee_all * 0.3M;
                    if (mess != "")
                    {
                        mess += ";";
                    }
                    mess += "补助额" + ad_assfee.ToString("0.##") + "= 保内额(" + ldec_fee_all.ToString("0.##") + ") * 0.3"+";";
                }
                else if (ldec_fee_all <= 200.0M)
                {
                    ad_assfee = 30.0M + (ldec_fee_all - 100.0M) * 0.45M;
                    if (mess != "")
                    {
                        mess += ";";
                    }
                    mess += "补助额" + ad_assfee.ToString("0.##") + "= 30+保内额(" + ldec_fee_all.ToString("0.##") + "-100) * 0.45"+";";
                }
                else
                {
                    ad_assfee = 75.0M + (ldec_fee_all - 200.0M) * 0.65M;
                    if (mess != "")
                    {
                        mess += ";";
                    }
                    mess += "补助额" + ad_assfee.ToString("0.##") + "= 75+保内额(" + ldec_fee_all.ToString("0.##") + "-200) * 0.65"+";";
                }

                if (ad_assfee - ldec_nhpay_all > 0)
		        {    
                    mess += "最终补助额:"+(ad_assfee - ldec_nhpay_all).ToString("0.##")+"=本次补助额"+ad_assfee.ToString("0.##")+" - 三天累计补助额"+ldec_nhpay_all.ToString("0.##")+".";
                    ad_assfee = ad_assfee - ldec_nhpay_all;
	            }
                else
		        {    
                    mess += "本次补助额<三天累计补助额,故最终补助额:0.";
		            ad_assfee = 0;
                }
	            


                if (Convert.ToDecimal(feeDs.Tables["mzhosppatinfo2"].Rows[0]["deratesum"]) + ad_assfee > Convert.ToDecimal(feeDs.Tables["mzhosppatinfo2"].Rows[0]["grandPrice"]))
                {
                    ad_assfee = Convert.ToDecimal(feeDs.Tables["mzhosppatinfo2"].Rows[0]["grandPrice"]) - Convert.ToDecimal(feeDs.Tables["mzhosppatinfo2"].Rows[0]["deratesum"]);
                    mess += ";应补额不超过门诊费用" + feeDs.Tables["mzhosppatinfo2"].Rows[0]["grandPrice"].ToString() + "—医院减免" + feeDs.Tables["mzhosppatinfo2"].Rows[0]["deratesum"].ToString() + "＝" + ad_assfee.ToString("0.##");
                }
                ad_nhpay = ad_assfee;
                ad_nhpay = if0.calcChkValue(ad_nhpay, ll_chkprecision, ll_precisionmode);
                #endregion
            }
            else
            {
                #region //获取参与计算的费用总额
                switch (Convert.ToInt32(feeDs.Tables["mzarearcmsparm"].Rows[0]["feetype"]))
                {
                    case 0: //按保内额计算补偿
                        ld_fee = Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["mediprice"]) - Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["mediselffee"]);
                        ld_fee += Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["materprice"]) - Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["materselffee"]);
                        ld_fee += Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["serverprice"]) - Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["serverselffee"]);
                        if (mess != "")
                        {
                            mess += ";";
                        }
                        mess += "保内额:" + ld_fee.ToString("0.##");
                        break;
                    case 1: //按医疗费用计算补偿


                        ld_fee = Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["mediprice"]);
                        ld_fee += Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["materprice"]);
                        ld_fee += Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["serverprice"]);
                        //----------Added By Sunyan 20100726 Below For 宁乡HIS中四舍五入入上来了几分钱，则补偿后，可能HIS还要病人出0.1元


		                if (Convert.ToDecimal(feeDs.Tables["mzhosppatinfo2"].Rows[0]["deratesum"]) > -0.1M && Convert.ToDecimal(feeDs.Tables["mzhosppatinfo2"].Rows[0]["deratesum"]) <0.0M)
			            {
                            ld_fee -= Convert.ToDecimal(feeDs.Tables["mzhosppatinfo2"].Rows[0]["deratesum"]);
                        }
		                //----------Added By Sunyan 20100726 Above----------
                        if (mess != "")
                        {
                            mess += ";";
                        }
                        mess += "医疗费:" + ld_fee.ToString("0.##");
                        break;
                    default:
                        mess = "没有定义的计算费用类型，请联系开发人员!";
                        return false;
                }
                #endregion
                #region //应补金额计算ad_assfee
                if (ld_fee > ld_startline)
                {
                    ad_assfee = (ld_fee - ld_startline) * ld_subsidyrate; //应补金额
                    ad_assfee = if0.calcChkValue(ad_assfee, ll_chkprecision, ll_precisionmode);
                    if (mess != "")
                    {
                        mess += ";";
                    }
                    mess += "应补金额:" + ad_assfee.ToString("0.##") + "=(" + ld_fee.ToString("0.###") + "-" + ld_startline.ToString("0.###") + ")×" + ld_subsidyrate.ToString("0.###");
                }
                else
                {
                    ad_assfee = 0.0M;
                    feeDs.Tables["mzcheckpay"].Rows[0]["calcid"] = 1; //小于起付线


                    if (mess != "")
                    {
                        mess += ";";
                    }
                    mess += "小于起付线" + ld_startline.ToString("0.###") + "应补=" + ad_assfee.ToString("0.##");
                }
                if (Convert.ToDecimal(feeDs.Tables["mzhosppatinfo2"].Rows[0]["deratesum"]) + ad_assfee > Convert.ToDecimal(feeDs.Tables["mzhosppatinfo2"].Rows[0]["grandPrice"]))
                {
                    if (mess != "")
                    {
                        mess += ";";
                    }
                    ad_assfee = Convert.ToDecimal(feeDs.Tables["mzhosppatinfo2"].Rows[0]["grandPrice"]) - Convert.ToDecimal(feeDs.Tables["mzhosppatinfo2"].Rows[0]["deratesum"]);
                    ad_assfee = if0.calcChkValue(ad_assfee, ll_chkprecision, ll_precisionmode);
                    mess += "应补额不超过门诊费用" + feeDs.Tables["mzhosppatinfo2"].Rows[0]["grandPrice"].ToString() + "—医院减免" + feeDs.Tables["mzhosppatinfo2"].Rows[0]["deratesum"].ToString() + "＝" + ad_assfee.ToString("0.##");
                }
                #endregion
                #region //开始按政策计算
                decimal ld_topline, ld_havepay, ld_tmp1, ld_tmp2;
                switch (ll_toptype)
                {
                    #region //10:20:每天补偿额<=α   //每天门诊额<=α
                    case 10:
                    case 20://每天补偿额<=α   //每天门诊额<=α
                        ld_topline = ld_a1;
                        if (mess != "")
                        {
                            mess += ";";
                        }
                        mess += "每天限额:" + ld_topline.ToString("0.##");
                        ls_sql = "SELECT isnull(sum(nhpay),0.0) FROM mz_checkpay WHERE human='" + ls_human + "' AND statisticyear='" + ls_statisticyear + "'";
                        ls_sql += " AND chkpayid>0 AND payorder<>'" + payorder + "' AND orderid<6000 AND paydate='" + ls_hospdate + "'";
                        cmd.CommandText = ls_sql;
                        try
                        {
                            ld_havepay = Convert.ToDecimal(cmd.ExecuteScalar());
                        }
                        catch (Exception e)
                        {
                            mess = "查询当天已补偿额时异常:" + e.Message.ToString();
                            return false;
                        }
                        if (mess != "")
                        {
                            mess += ";";
                        }
                        mess += ls_hospdate + "已补偿:" + ld_havepay.ToString("0.##");
                        if (ld_havepay + ad_assfee > ld_topline)
                        {
                            ad_nhpay = ld_topline - ld_havepay; //实补金额
                            feeDs.Tables["mzcheckpay"].Rows[0]["calcid"] = 2;
                            if (mess != "")
                            {
                                mess += ";";
                            }
                            mess += "当天已补＋应补金额＞每天限额";
                        }
                        else
                        {
                            ad_nhpay = ad_assfee;
                        }
                        //if(ld_familyfee != 0.0M)
                        if (ll_ifbalance == 1)
                        {
                            if (Convert.ToInt32(feeDs.Tables["mzcheckpay"].Rows[0]["payid"]) != 1) //未支付标记的，则家庭账户才可能支付


                            {
                                if (ad_nhpay >= ad_familyaccbalance)
                                {
                                    ad_familyfound = ad_familyaccbalance; //其中家庭帐户补助额


                                    ad_familyaccbalance = 0.0M;
                                }
                                else
                                {
                                    ad_familyfound = ad_nhpay;	//其中家庭帐户补助额


                                    ad_familyaccbalance -= ad_nhpay;
                                }
                                if (mess != "")
                                {
                                    mess += ";";
                                }
                            }
                            else //否则，家庭账户支付按支付标记时的支付额度
                            {
                                ad_familyfound = Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["familyfound"]);
                            }
                            mess += "其中家庭帐户补助:" + ad_familyfound.ToString("0.##");
                        }
                        else
                        {
                            ad_familyfound = 0.0M;	//其中家庭帐户补助额


                        }
                        if (mess != "")
                        {
                            mess += ";";
                        }
                        mess += "本次补助:" + ad_nhpay.ToString("0.##");
                        break;
                    #endregion
                    #region //11:21:每次补偿额<=α    //每次门诊额<=α
                    case 11:
                    case 21://每次补偿额<=α    //每次门诊额<=α
                        ld_topline = ld_a1;
                        if (mess != "")
                        {
                            mess += ";";
                        }
                        mess += "每次限额:" + ld_topline.ToString("0.##");
                        if (ad_assfee > ld_topline)
                        {
                            ad_nhpay = ld_topline; //实补金额
                            feeDs.Tables["mzcheckpay"].Rows[0]["calcid"] = 2;
                            if (mess != "")
                            {
                                mess += ";";
                            }
                            mess += "应补金额＞每次限额";
                        }
                        else
                        {
                            ad_nhpay = ad_assfee;
                        }
                        //if (ld_familyfee != 0.0M)
                        if (ll_ifbalance == 1)
                        {
                            if (Convert.ToInt32(feeDs.Tables["mzcheckpay"].Rows[0]["payid"]) != 1) //未支付标记的，则家庭账户才可能支付


                            {
                                if (ad_nhpay >= ad_familyaccbalance)
                                {
                                    ad_familyfound = ad_familyaccbalance; //其中家庭帐户补助额


                                    ad_familyaccbalance = 0.0M;
                                }
                                else
                                {
                                    ad_familyfound = ad_nhpay;	//其中家庭帐户补助额


                                    ad_familyaccbalance -= ad_nhpay;
                                }
                                if (mess != "")
                                {
                                    mess += ";";
                                }
                            }
                            else //否则，家庭账户支付按支付标记时的支付额度
                            {
                                ad_familyfound = Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["familyfound"]);
                            }
                            mess += "其中家庭帐户补助:" + ad_familyfound.ToString("0.##");
                        }
                        else
                        {
                            ad_familyfound = 0;	//其中家庭帐户补助额


                        }
                        if (mess != "")
                        {
                            mess += ";";
                        }
                        mess += "本次补助:" + ad_nhpay.ToString("0.##");
                        break;
                    #endregion
                    #region //30:年补偿额<=家庭参合人数×α
                    case 30://年补偿额<=家庭参合人数×α
                        ls_sql = "select familyno from n_individdata where human='" + ls_human + "' AND statisticyear='" + ls_statisticyear + "'";
                        cmd.CommandText = ls_sql;
                        try
                        {
                            ls_familyno = Convert.ToString(cmd.ExecuteScalar());
                        }
                        catch (Exception e)
                        {
                            mess = "查询家庭编号时异常:" + e.Message.ToString();
                            return false;
                        }
                        ls_sql = "SELECT count(*) FROM n_individdata i,n_individcwuchkkp c WHERE i.human=c.human AND i.statisticyear=c.statisticyear";
                        ls_sql += " AND i.statisticyear='" + ls_statisticyear + "' AND i.familyno='" + ls_familyno + "' AND c.accstate=1";
                        cmd.CommandText = ls_sql;
                        try
                        {
                            ld_topline = Convert.ToInt32(cmd.ExecuteScalar()) * ld_a1;
                        }
                        catch (Exception e)
                        {
                            mess = "查询家庭参合人数时异常:" + e.Message.ToString();
                            return false;
                        }
                        if (mess != "")
                        {
                            mess += ";";
                        }
                        mess += "每年限额" + ld_topline.ToString("0.##") + "＝家庭参合人数" + Convert.ToInt32(cmd.ExecuteScalar()).ToString("0") + "×" + ld_a1.ToString("0.##");
                        ls_sql = "SELECT isnull(sum(nhpay),0.0) FROM mz_checkpay WHERE human='" + ls_human + "' AND statisticyear='" + ls_statisticyear + "'";
                        ls_sql += " AND payorder<>'" + payorder + "' AND chkpayid>0 AND orderid<6000";
                        cmd.CommandText = ls_sql;
                        try
                        {
                            ld_havepay = Convert.ToDecimal(cmd.ExecuteScalar());
                        }
                        catch (Exception e)
                        {
                            mess = "查询" + ls_statisticyear + "年已补偿额时异常:" + e.Message.ToString();
                            return false;
                        }
                        if (ld_havepay + ad_assfee > ld_topline)
                        {
                            ad_nhpay = ld_topline - ld_havepay; //实补金额
                            feeDs.Tables["mzcheckpay"].Rows[0]["calcid"] = 3;
                            if (mess != "")
                            {
                                mess += ";";
                            }
                            mess += "当年已补" + ld_havepay.ToString("0.##") + "＋应补金额" + ad_assfee.ToString("0.##") + "＞每年限额" + ld_topline.ToString("0.##");
                        }
                        else
                        {
                            ad_nhpay = ad_assfee;
                        }
                        if (mess != "")
                        {
                            mess += ";";
                        }
                        ad_nhpay = if0.calcChkValue(ad_nhpay, ll_chkprecision, ll_precisionmode);
                        //if (ld_familyfee != 0.0M)
                        if (ll_ifbalance == 1)
                        {
                            if (Convert.ToInt32(feeDs.Tables["mzcheckpay"].Rows[0]["payid"]) != 1) //未支付标记的，则家庭账户才可能支付


                            {
                                if (ad_nhpay >= ad_familyaccbalance)
                                {
                                    ad_familyfound = ad_familyaccbalance;	//其中家庭帐户补助额


                                    ad_familyaccbalance = 0.0M;
                                }
                                else
                                {
                                    ad_familyfound = ad_nhpay;	//其中家庭帐户补助额


                                    ad_familyaccbalance -= ad_nhpay;
                                }
                            }
                            else //否则，家庭账户支付按支付标记时的支付额度
                            {
                                ad_familyfound = Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["familyfound"]);
                            }
                            mess += "其中家庭帐户补助:" + ad_familyfound.ToString("0.##");
                            mess += ";";
                        }
                        else
                        {
                            ad_familyfound = 0.0M;	//其中家庭帐户补助额


                        }
                        mess += "本次补助:" + ad_nhpay.ToString("0.##");
                        break;
                    #endregion
                    #region //31:年补偿额每户<=α
                    case 31://年补偿额每户<=α
                        ld_topline = ld_a1;
                        if (mess != "")
                        {
                            mess += ";";
                        }
                        mess += "每年限额:" + ld_topline.ToString("0.##");
                        ls_sql = "select familyno from n_individdata where human='" + ls_human + "' AND statisticyear='" + ls_statisticyear + "'";
                        cmd.CommandText = ls_sql;
                        try
                        {
                            ls_familyno = Convert.ToString(cmd.ExecuteScalar());
                        }
                        catch (Exception e)
                        {
                            mess = "查询家庭编号时异常:" + e.Message.ToString();
                            return false;
                        }
                        ls_sql = "SELECT isnull(sum(c.nhpay),0.0) FROM mz_checkpay c,n_individdata i WHERE i.statisticyear='" + ls_statisticyear + "' and i.familyno='" + ls_familyno + "'";
                        ls_sql += " And i.human=c.human and i.statisticyear=c.statisticyear And c.payorder<>'" + payorder + "' And c.chkpayid>0 AND c.orderid<6000";
                        cmd.CommandText = ls_sql;
                        try
                        {
                            ld_havepay = Convert.ToDecimal(cmd.ExecuteScalar());
                        }
                        catch (Exception e)
                        {
                            mess = "查询" + ls_statisticyear + "年户已补偿额时异常:" + e.Message.ToString();
                            return false;
                        }
                        if (ld_havepay + ad_assfee > ld_topline)
                        {
                            ad_nhpay = ld_topline - ld_havepay; //实补金额
                            feeDs.Tables["mzcheckpay"].Rows[0]["calcid"] = 3;
                            if (mess != "")
                            {
                                mess += ";";
                            }
                            mess += "当年户已补" + ld_havepay.ToString("0.##") + "＋应补金额" + ad_assfee.ToString("0.##") + "＞每年限额" + ld_topline.ToString("0.##");
                        }
                        else
                        {
                            ad_nhpay = ad_assfee;
                        }
                        if (mess != "")
                        {
                            mess += ";";
                        }
                        ad_nhpay = if0.calcChkValue(ad_nhpay, ll_chkprecision, ll_precisionmode);
                        //if (ld_familyfee != 0.0M)
                        if (ll_ifbalance == 1)
                        {
                            if (Convert.ToInt32(feeDs.Tables["mzcheckpay"].Rows[0]["payid"]) != 1) //未支付标记的，则家庭账户才可能支付


                            {
                                if (ad_nhpay >= ad_familyaccbalance)
                                {
                                    ad_familyfound = ad_familyaccbalance;	//其中家庭帐户补助额


                                    ad_familyaccbalance = 0.0M;
                                    mess += ";";
                                }
                                else
                                {
                                    ad_familyfound = ad_nhpay;	//其中家庭帐户补助额


                                    ad_familyaccbalance -= ad_nhpay;
                                }
                            }
                            else //否则，家庭账户支付按支付标记时的支付额度
                            {
                                ad_familyfound = Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["familyfound"]);
                            }
                            mess += "其中家庭帐户补助:" + ad_familyfound.ToString("0.##") + ";";
                        }
                        else
                        {
                            ad_familyfound = 0.0M;	//其中家庭帐户补助额


                        }
                        mess += "本次补助:" + ad_nhpay.ToString("0.##");
                        break;
                    #endregion
                    #region //32:33:每天限补α1次,每次封顶α2,每户年封顶<=家庭参合人数×α3 或 <=当前家庭帐户余额
                    case 32://每天限补α1次,每次封顶α2,每户年封顶<=家庭参合人数×α3
                    case 33://每天限补α1次,每次封顶α2,每户年封顶<=当前家庭帐户余额
                        if (mess != "")
                        {
                            mess += ";";
                        }
                        mess += "每天限补:" + ld_a1.ToString("0") + "次,每次封顶:" + ld_a2.ToString();
                        ls_sql = "select count(human) from mz_checkpay where human='" + ls_human + "' AND statisticyear='" + ls_statisticyear + "'";
                        ls_sql += " AND payorder<>'" + payorder + "' AND chkpayid>0  And paydate='" + ls_hospdate + "' AND orderid<6000";
                        cmd.CommandText = ls_sql;
                        try
                        {
                            li_temp = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        catch (Exception e)
                        {
                            mess = "查询当天已补次数时数据库异常:" + e.Message.ToString();
                            return false;
                        }
                        if (li_temp >= ld_a1)
                        {
                            if (mess != "")
                            {
                                mess += ";";
                            }
                            mess += ls_hospdate + "已补偿" + li_temp + "次";
                            ad_nhpay = 0.0M;
                        }
                        else
                        {
                            if (ll_toptype == 32) //每天限补α1次,每次封顶α2,每户年封顶<=家庭参合人数×α3
                            {
                                ls_sql = "select familyno from n_individdata where human='" + ls_human + "' AND statisticyear='" + ls_statisticyear + "'";
                                cmd.CommandText = ls_sql;
                                try
                                {
                                    ls_familyno = Convert.ToString(cmd.ExecuteScalar());
                                }
                                catch (Exception e)
                                {
                                    mess = "查询家庭编号时异常:" + e.Message.ToString();
                                    return false;
                                }
                                ls_sql = "SELECT count(*) FROM n_individdata i,n_individcwuchkkp c WHERE i.human=c.human AND i.statisticyear=c.statisticyear";
                                ls_sql += " AND i.statisticyear='" + ls_statisticyear + "' AND i.familyno='" + ls_familyno + "' AND c.accstate=1";
                                cmd.CommandText = ls_sql;
                                try
                                {
                                    ld_topline = Convert.ToInt32(cmd.ExecuteScalar()) * ld_a3;
                                }
                                catch (Exception e)
                                {
                                    mess = "查询家庭参合人数时异常:" + e.Message.ToString();
                                    return false;
                                }
                                if (mess != "")
                                {
                                    mess += ";";
                                }
                                mess += "每年户限额" + ld_topline.ToString("0.##") + "＝家庭参合人数" + Convert.ToInt32(cmd.ExecuteScalar()).ToString("0") + "×" + ld_a3.ToString("0.##");
                                ls_sql = "SELECT isnull(sum(c.nhpay),0.0) FROM mz_checkpay c,n_individdata i WHERE i.statisticyear='" + ls_statisticyear + "' and i.familyno='" + ls_familyno + "'";
                                ls_sql += " And i.human=c.human and i.statisticyear=c.statisticyear And c.payorder<>'" + payorder + "' And c.chkpayid>0 AND c.orderid<6000";
                                cmd.CommandText = ls_sql;
                                try
                                {
                                    ld_havepay = Convert.ToDecimal(cmd.ExecuteScalar());
                                }
                                catch (Exception e)
                                {
                                    mess = "查询" + ls_statisticyear + "年户已补偿额时异常:" + e.Message.ToString();
                                    return false;
                                }
                                if (mess != "")
                                {
                                    mess += ";";
                                }
                                mess += ls_statisticyear + "年户已补" + ld_havepay.ToString();
                                if (ld_topline - ld_havepay > ld_a2) //下面计算结果以这里最小值为封顶
                                {
                                    ld_topline = ld_a2;
                                }
                                else
                                {
                                    if (ld_topline < ld_havepay)
                                    {
                                        ld_topline = 0.0M;
                                    }
                                    else
                                    {
                                        ld_topline = ld_topline - ld_havepay;
                                    }
                                }
                                if (mess != "")
                                {
                                    mess += ";";
                                }
                                mess += "本次补偿封顶线" + ld_topline.ToString();
                            }
                            else //每天限补α1次,每次封顶α2,每户年封顶<=当前家庭帐户余额
                            {
                                ld_topline = ad_familyaccbalance;
                                if (ld_topline > ld_a2)
                                {
                                    ld_topline = ld_a2;
                                }
                                else
                                {
                                    if (mess != "")
                                    {
                                        mess += ";";
                                    }
                                    mess += "本次封顶(家庭帐户余额)" + ld_topline.ToString();
                                }
                            }
                            if (ld_topline > ad_assfee)
                            {
                                ad_nhpay = ad_assfee; //实补金额
                            }
                            else
                            {
                                ad_nhpay = ld_topline;
                            }
                            ad_nhpay = if0.calcChkValue(ad_nhpay, ll_chkprecision, ll_precisionmode);
                        }
                        if (mess != "")
                        {
                            mess += ";";
                        }
                        //if (ld_familyfee != 0.0M)
                        if (ll_ifbalance == 1)
                        {
                            if (Convert.ToInt32(feeDs.Tables["mzcheckpay"].Rows[0]["payid"]) != 1) //未支付标记的，则家庭账户才可能支付


                            {
                                if (ad_nhpay >= ad_familyaccbalance)
                                {
                                    ad_familyfound = ad_familyaccbalance;	//其中家庭帐户补助额


                                    ad_familyaccbalance = 0.0M;
                                }
                                else
                                {
                                    ad_familyfound = ad_nhpay;	//其中家庭帐户补助额


                                    ad_familyaccbalance -= ad_nhpay;
                                }
                            }
                            else //否则，家庭账户支付按支付标记时的支付额度
                            {
                                ad_familyfound = Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["familyfound"]);
                            }
                            mess += "其中家庭帐户补助:" + ad_familyfound.ToString("0.##");
                            mess += ";";
                        }
                        else
                        {
                            ad_familyfound = 0.0M;	//其中家庭帐户补助额


                        }
                        mess += "本次补助:" + ad_nhpay.ToString("0.##");
                        break;
                    #endregion
                    #region //40:每次补偿额<=当前家庭帐户余额
                    case 40://每次补偿额<=当前家庭帐户余额
                        ld_topline = ad_familyaccbalance;
                        if (mess != "")
                        {
                            mess += ";";
                        }
                        mess += "每次家庭帐户支出限额:" + ld_topline.ToString("0.##");
                        mess += ";";
                        //if (ld_familyfee != 0.0M)
                        if (ll_ifbalance == 1)
                        {
                            if (Convert.ToInt32(feeDs.Tables["mzcheckpay"].Rows[0]["payid"]) != 1) //未支付标记的，则家庭账户才可能支付


                            {
                                if (ad_assfee >= ad_familyaccbalance)
                                {
                                    if (ad_familyaccbalance > ld_topline) //家庭帐户支出封顶
                                    {
                                        ad_familyfound = ld_topline;	//其中家庭帐户补助额


                                        mess += "补助额大于每次家庭帐户支出限额:" + ld_topline.ToString("0.##") + ";";
                                        ad_familyaccbalance = ad_familyaccbalance - ad_familyfound;
                                        ad_nhpay = ad_familyfound;
                                    }
                                    else
                                    {
                                        ad_familyfound = ad_familyaccbalance;	//其中家庭帐户补助额


                                        ad_nhpay = ad_familyfound;
                                        ad_familyaccbalance = 0.0M;
                                    }
                                }
                                else
                                {
                                    if (ad_assfee > ld_topline)
                                    {
                                        ad_familyfound = ld_topline;	//其中家庭帐户补助额


                                        ad_familyaccbalance -= ad_familyfound;
                                        ad_nhpay = ad_familyfound;
                                    }
                                    else
                                    {
                                        ad_familyfound = ad_assfee;	//其中家庭帐户补助额


                                        ad_familyaccbalance -= ad_familyfound;
                                        ad_nhpay = ad_familyfound;
                                    }
                                }
                            }
                            else //否则，家庭账户支付按支付标记时的支付额度
                            {
                                ad_familyfound = Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["familyfound"]);
                            }
                            mess += "其中家庭帐户补助:" + ad_familyfound.ToString("0.##") + ";";
                            ad_nhpay = if0.calcChkValue(ad_nhpay, ll_chkprecision, ll_precisionmode);
                        }
                        else
                        {
                            ad_familyfound = 0.0M;	//其中家庭帐户补助额


                            ad_nhpay = 0.0M;
                            mess += "系统还未启用门诊家庭帐户报销或当年缴费没有划入家庭帐户;";
                        }
                        mess += "本次补助:" + ad_nhpay.ToString("0.##");
                        break;
                    #endregion
                    #region //41:每次封顶α1,每户年补偿<=当前家庭帐户余额
                    case 41:
                        ld_topline = ld_a1;
                        if (mess != "")
                        {
                            mess += ";";
                        }
                        mess += "每次封顶:" + ld_topline.ToString("0.##");
                        mess += ";";
                        //if (ld_familyfee != 0.0M)
                        if (ll_ifbalance == 1)
                        {
                            if (Convert.ToInt32(feeDs.Tables["mzcheckpay"].Rows[0]["payid"]) != 1) //未支付标记的，则家庭账户才可能支付


                            {
                                if (ad_assfee >= ad_familyaccbalance)
                                {
                                    if (ad_familyaccbalance > ld_topline) //家庭帐户支出封顶
                                    {
                                        ad_familyfound = ld_topline;	//其中家庭帐户补助额


                                        mess += "补助额大于每次封顶;";
                                        ad_familyaccbalance = ad_familyaccbalance - ad_familyfound;
                                        ad_nhpay = ad_familyfound;
                                    }
                                    else
                                    {
                                        ad_familyfound = ad_familyaccbalance;	//其中家庭帐户补助额


                                        ad_nhpay = ad_familyfound;
                                        ad_familyaccbalance = 0.0M;
                                    }
                                }
                                else
                                {
                                    if (ad_assfee > ld_topline)
                                    {
                                        ad_familyfound = ld_topline;	//其中家庭帐户补助额


                                        ad_familyaccbalance -= ad_familyfound;
                                        ad_nhpay = ad_familyfound;
                                    }
                                    else
                                    {
                                        ad_familyfound = ad_assfee;	//其中家庭帐户补助额


                                        ad_familyaccbalance -= ad_familyfound;
                                        ad_nhpay = ad_familyfound;
                                    }
                                }
                            }
                            else //否则，家庭账户支付按支付标记时的支付额度
                            {
                                ad_familyfound = Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["familyfound"]);
                            }
                            mess += "其中家庭帐户补助:" + ad_familyfound.ToString("0.##") + ";";
                            ad_nhpay = if0.calcChkValue(ad_nhpay, ll_chkprecision, ll_precisionmode);
                        }
                        else
                        {
                            ad_familyfound = 0.0M;	//其中家庭帐户补助额


                            ad_nhpay = 0.0M;
                            mess += "系统还未启用门诊家庭帐户报销或当年缴费没有划入家庭帐户;";
                        }
                        mess += "本次补助:" + ad_nhpay.ToString("0.##");
                        break;
                    #endregion
                    #region //50:51:就诊月每人补偿<=α1 //就诊月每户补偿<=α1
                    case 50:
                    case 51:
                        ld_topline = ld_a1;
                        if (mess != "")
                        {
                            mess += ";";
                        }
                        //每月已补偿额
                        if (ll_toptype == 50)
                        {
                            mess += "当月每人补偿限额:" + ld_topline.ToString("0.##");
                            ls_sql = "SELECT isnull(sum(nhpay),0.0) FROM mz_checkpay WHERE human='" + ls_human + "' AND statisticyear='" + ls_statisticyear + "'";
                            ls_sql += " AND chkpayid>0 AND payorder<>'" + payorder + "' AND orderid<6000 AND left(paydate,6)='" + ls_hospdate.Substring(1, 6) + "'";
                            cmd.CommandText = ls_sql;
                            try
                            {
                                ld_havepay = Convert.ToDecimal(cmd.ExecuteScalar());
                            }
                            catch (Exception e)
                            {
                                mess = "查询" + ls_hospdate.Substring(1, 6) + "月已补偿额时异常:" + e.Message.ToString();
                                return false;
                            }
                        }
                        else
                        {
                            mess += "当月每户补偿限额:" + ld_topline.ToString("0.##");
                            ls_sql = "select familyno from n_individdata where human='" + ls_human + "' AND statisticyear='" + ls_statisticyear + "'";
                            cmd.CommandText = ls_sql;
                            try
                            {
                                ls_familyno = Convert.ToString(cmd.ExecuteScalar());
                            }
                            catch (Exception e)
                            {
                                mess = "查询家庭编号时异常:" + e.Message.ToString();
                                return false;
                            }
                            ls_sql = "SELECT isnull(sum(c.nhpay),0.0) FROM mz_checkpay c,n_individdata i WHERE i.statisticyear='" + ls_statisticyear + "' and i.familyno='" + ls_familyno + "'";
                            ls_sql += " And i.human=c.human and i.statisticyear=c.statisticyear And c.payorder<>'" + payorder + "' And c.chkpayid>0 AND c.orderid<6000 And left(c.paydate,6)='" + ls_hospdate.Substring(1, 6) + "'";
                            cmd.CommandText = ls_sql;
                            try
                            {
                                ld_havepay = Convert.ToDecimal(cmd.ExecuteScalar());
                            }
                            catch (Exception e)
                            {
                                mess = "查询" + ls_hospdate.Substring(1, 6) + "月户已补偿额时异常:" + e.Message.ToString();
                                return false;
                            }
                        }
                        //计算实补金额
                        if (ld_havepay + ad_assfee > ld_topline)
                        {
                            ad_nhpay = ld_topline - ld_havepay; //实补金额
                            feeDs.Tables["mzcheckpay"].Rows[0]["calcid"] = 2;
                            if (mess != "")
                            {
                                mess += ";";
                            }
                            mess += "当月已补" + ld_havepay.ToString("0.##") + "＋应补金额" + ad_assfee.ToString("0.##") + "＞当月限额" + ld_topline.ToString("0.##");
                        }
                        else
                        {
                            ad_nhpay = ad_assfee;
                        }
                        if (mess != "")
                        {
                            mess += ";";
                        }
                        ad_nhpay = if0.calcChkValue(ad_nhpay, ll_chkprecision, ll_precisionmode);
                        //if (ld_familyfee != 0.0M)
                        if (ll_ifbalance == 1)
                        {
                            if (Convert.ToInt32(feeDs.Tables["mzcheckpay"].Rows[0]["payid"]) != 1) //未支付标记的，则家庭账户才可能支付


                            {
                                if (ad_nhpay >= ad_familyaccbalance)
                                {
                                    ad_familyfound = ad_familyaccbalance;	//其中家庭帐户补助额


                                    ad_familyaccbalance = 0.0M;
                                    mess += ";";
                                }
                                else
                                {
                                    ad_familyfound = ad_nhpay;	//其中家庭帐户补助额


                                    ad_familyaccbalance -= ad_nhpay;
                                }
                            }
                            else //否则，家庭账户支付按支付标记时的支付额度
                            {
                                ad_familyfound = Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["familyfound"]);
                            }
                            mess += "其中家庭帐户补助:" + ad_familyfound.ToString("0.##") + ";";
                        }
                        else
                        {
                            ad_familyfound = 0.0M;	//其中家庭帐户补助额


                        }
                        mess += "本次补助:" + ad_nhpay.ToString("0.##");
                        break;
                    #endregion
                    #region //52:53:每人每天补偿<=α1,每月<=α2,每年<=α3 //每户每天补偿<=α1,每月<=α2,每年<=α3
                    case 52:
                    case 53:
                        if (mess != "")
                        {
                            mess += ";";
                        }
                        //每月已补偿额
                        if (ll_toptype == 52)
                        {
                            mess += "日限补:" + ld_a1.ToString("0.##");
                            mess += ";月限补:" + ld_a2.ToString("0.##");
                            mess += ";年限补:" + ld_a3.ToString("0.##");
                            //日已补


                            ls_sql = "SELECT isnull(sum(nhpay),0.0) FROM mz_checkpay WHERE human='" + ls_human + "' AND statisticyear='" + ls_statisticyear + "'";
                            ls_sql += " AND chkpayid>0 AND payorder<>'" + payorder + "' AND orderid<6000 AND paydate='" + ls_hospdate + "'";
                            cmd.CommandText = ls_sql;
                            try
                            {
                                ld_havepay = Convert.ToDecimal(cmd.ExecuteScalar());
                                mess += ";" + ls_hospdate + "已补" + ld_havepay.ToString("0.##");
                            }
                            catch (Exception e)
                            {
                                mess = "查询" + ls_hospdate + "已补偿额时异常:" + e.Message.ToString();
                                return false;
                            }
                            //月已补


                            ls_sql = "SELECT isnull(sum(nhpay),0.0) FROM mz_checkpay WHERE human='" + ls_human + "' AND statisticyear='" + ls_statisticyear + "'";
                            ls_sql += " AND chkpayid>0 AND payorder<>'" + payorder + "' AND orderid<6000 AND left(paydate,6)='" + ls_hospdate.Substring(1, 6) + "'";
                            cmd.CommandText = ls_sql;
                            try
                            {
                                ld_tmp1 = Convert.ToDecimal(cmd.ExecuteScalar());
                                mess += ";" + ls_hospdate.Substring(1, 6) + "已补" + ld_havepay.ToString("0.##");
                            }
                            catch (Exception e)
                            {
                                mess = "查询" + ls_hospdate.Substring(1, 6) + "已补偿额时异常:" + e.Message.ToString();
                                return false;
                            }
                            //年已补


                            ls_sql = "SELECT isnull(sum(nhpay),0.0) FROM mz_checkpay WHERE human='" + ls_human + "' AND statisticyear='" + ls_statisticyear + "'";
                            ls_sql += " AND chkpayid>0 AND payorder<>'" + payorder + "' AND orderid<6000 AND left(paydate,6)='" + ls_hospdate.Substring(1, 4) + "'";
                            cmd.CommandText = ls_sql;
                            try
                            {
                                ld_tmp2 = Convert.ToDecimal(cmd.ExecuteScalar());
                                mess += ";" + ls_hospdate.Substring(1, 4) + "已补" + ld_havepay.ToString("0.##");
                            }
                            catch (Exception e)
                            {
                                mess = "查询" + ls_hospdate.Substring(1, 4) + "已补偿额时异常:" + e.Message.ToString();
                                return false;
                            }
                        }
                        else
                        {
                            ls_sql = "select familyno from n_individdata where human='" + ls_human + "' AND statisticyear='" + ls_statisticyear + "'";
                            cmd.CommandText = ls_sql;
                            try
                            {
                                ls_familyno = Convert.ToString(cmd.ExecuteScalar());
                            }
                            catch (Exception e)
                            {
                                mess = "查询家庭编号时异常:" + e.Message.ToString();
                                return false;
                            }
                            mess += "日户限补:" + ld_a1.ToString("0.##");
                            mess += ";月户限补:" + ld_a2.ToString("0.##");
                            mess += ";年户限补:" + ld_a3.ToString("0.##");
                            //日户限补
                            ls_sql = "SELECT isnull(sum(c.nhpay),0.0) FROM mz_checkpay c,n_individdata i WHERE i.statisticyear='" + ls_statisticyear + "' and i.familyno='" + ls_familyno + "'";
                            ls_sql += " And i.human=c.human and i.statisticyear=c.statisticyear And c.payorder<>'" + payorder + "' And c.chkpayid>0 AND c.orderid<6000 And c.paydate='" + ls_hospdate + "'";
                            cmd.CommandText = ls_sql;
                            try
                            {
                                ld_havepay = Convert.ToDecimal(cmd.ExecuteScalar());
                                mess += ";" + ls_hospdate + "户已补" + ld_havepay.ToString("0.##");
                            }
                            catch (Exception e)
                            {
                                mess = "查询" + ls_hospdate.Substring(1, 6) + "月户已补偿额时异常:" + e.Message.ToString();
                                return false;
                            }
                            //月户限补
                            ls_sql = "SELECT isnull(sum(c.nhpay),0.0) FROM mz_checkpay c,n_individdata i WHERE i.statisticyear='" + ls_statisticyear + "' and i.familyno='" + ls_familyno + "'";
                            ls_sql += " And i.human=c.human and i.statisticyear=c.statisticyear And c.payorder<>'" + payorder + "' And c.chkpayid>0 AND c.orderid<6000 And left(c.paydate,6)='" + ls_hospdate.Substring(1, 6) + "'";
                            cmd.CommandText = ls_sql;
                            try
                            {
                                ld_tmp1 = Convert.ToDecimal(cmd.ExecuteScalar());
                                mess += ";" + ls_hospdate.Substring(1, 6) + "户已补" + ld_havepay.ToString("0.##");
                            }
                            catch (Exception e)
                            {
                                mess = "查询" + ls_hospdate.Substring(1, 6) + "月户已补偿额时异常:" + e.Message.ToString();
                                return false;
                            }
                            //月年限补
                            ls_sql = "SELECT isnull(sum(c.nhpay),0.0) FROM mz_checkpay c,n_individdata i WHERE i.statisticyear='" + ls_statisticyear + "' and i.familyno='" + ls_familyno + "'";
                            ls_sql += " And i.human=c.human and i.statisticyear=c.statisticyear And c.payorder<>'" + payorder + "' And c.chkpayid>0 AND c.orderid<6000 And left(c.paydate,6)='" + ls_hospdate.Substring(1, 4) + "'";
                            cmd.CommandText = ls_sql;
                            try
                            {
                                ld_tmp2 = Convert.ToDecimal(cmd.ExecuteScalar());
                                mess += ";" + ls_hospdate.Substring(1, 4) + "户已补" + ld_havepay.ToString("0.##");
                            }
                            catch (Exception e)
                            {
                                mess = "查询" + ls_hospdate.Substring(1, 4) + "月户已补偿额时异常:" + e.Message.ToString();
                                return false;
                            }
                        }
                        //取应补金额、各限额最小值计算实补金额


                        ad_nhpay = Math.Min(Math.Min(Math.Min(ad_assfee, ld_a1 - ld_havepay), ld_a2 - ld_tmp1), ld_a3 - ld_tmp2);
                        if (ad_nhpay < 0.0M)
                        {
                            ad_nhpay = 0.0M;
                        }
                        ad_nhpay = if0.calcChkValue(ad_nhpay, ll_chkprecision, ll_precisionmode);
                        //if (ld_familyfee != 0.0M)
                        if (ll_ifbalance == 1)
                        {
                            if (Convert.ToInt32(feeDs.Tables["mzcheckpay"].Rows[0]["payid"]) != 1) //未支付标记的，则家庭账户才可能支付


                            {
                                if (ad_nhpay >= ad_familyaccbalance)
                                {
                                    ad_familyfound = ad_familyaccbalance;	//其中家庭帐户补助额


                                    ad_familyaccbalance = 0.0M;
                                    mess += ";";
                                }
                                else
                                {
                                    ad_familyfound = ad_nhpay;	//其中家庭帐户补助额


                                    ad_familyaccbalance -= ad_nhpay;
                                }
                            }
                            else //否则，家庭账户支付按支付标记时的支付额度
                            {
                                ad_familyfound = Convert.ToDecimal(feeDs.Tables["mzcheckpay"].Rows[0]["familyfound"]);
                            }
                            mess += "其中家庭帐户补助:" + ad_familyfound.ToString("0.##") + ";";
                        }
                        else
                        {
                            ad_familyfound = 0.0M;	//其中家庭帐户补助额


                        }
                        mess += "本次补助:" + ad_nhpay.ToString("0.##");
                        break;
                    #endregion
                    default:
                        mess = "没有定义的封顶方式，请联系开发人员!";
                        return false;
                }
                #endregion
            }
            #region //计算结果更新到数据集
            if (ad_familyfound < 0.0M)
            {
                ad_familyfound = 0.0M;
            }
            if (ad_nhpay < 0.0M)
            {
                ad_nhpay = 0.0M;
            }
            feeDs.Tables["mzcheckpay"].Rows[0]["assfee"] = ad_assfee; //总应补助额


            feeDs.Tables["mzhosppatinfo2"].Rows[0]["assfee"] = ad_assfee;
            feeDs.Tables["mzcheckpay"].Rows[0]["familyfound"] = ad_familyfound; //家庭帐户补助额


            feeDs.Tables["mzhosppatinfo2"].Rows[0]["familyfound"] = ad_familyfound;
            feeDs.Tables["mzcheckpay"].Rows[0]["nhpay"] = ad_nhpay; //实际补助额


            feeDs.Tables["mzhosppatinfo2"].Rows[0]["nhpay"] = ad_nhpay;
            feeDs.Tables["mzcheckpay"].Rows[0]["calcinfo"] = mess; //计算过程
            #endregion
            if (settle == 1)
            {
                #region //保存DataSet中的修改数据
                SqlTransaction myTrans;
                myTrans = Connection.BeginTransaction();
                try
                {
                    cmd.Transaction = myTrans;
                    if (ls_areacode != "430481") //耒阳的怪政策


                    {
                        cmd.CommandText = "UPDATE mz_arearcmsparm SET hadused=1";
                        cmd.CommandText += " WHERE orderid=" + feeDs.Tables["mzarearcmsparm"].Rows[0]["orderid"].ToString() + " AND areacode='" + AreaCode + "'";
                        cmd.ExecuteNonQuery();
                    }
                    cmd.CommandText = "UPDATE mz_hosppatinfo2 SET assfee=" + feeDs.Tables["mzhosppatinfo2"].Rows[0]["assfee"].ToString();
                    cmd.CommandText += ",familyfound=" + feeDs.Tables["mzhosppatinfo2"].Rows[0]["familyfound"].ToString();
                    cmd.CommandText += ",nhpay=" + feeDs.Tables["mzhosppatinfo2"].Rows[0]["nhpay"].ToString();
                    cmd.CommandText += " WHERE hospitalcode='" + feeDs.Tables["mzhosppatinfo2"].Rows[0]["hospitalcode"].ToString() + "' AND HospCode='" + feeDs.Tables["mzhosppatinfo2"].Rows[0]["HospCode"].ToString() + "'";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "UPDATE mz_checkpay SET assfee=" + feeDs.Tables["mzcheckpay"].Rows[0]["assfee"].ToString();
                    cmd.CommandText += ",familyfound=" + feeDs.Tables["mzcheckpay"].Rows[0]["familyfound"].ToString();
                    cmd.CommandText += ",nhpay=" + feeDs.Tables["mzcheckpay"].Rows[0]["nhpay"].ToString();
                    cmd.CommandText += ",calcid=" + feeDs.Tables["mzcheckpay"].Rows[0]["calcid"].ToString();
                    cmd.CommandText += ",orderid=" + feeDs.Tables["mzcheckpay"].Rows[0]["orderid"].ToString();
                    cmd.CommandText += ",calcinfo='" + feeDs.Tables["mzcheckpay"].Rows[0]["calcinfo"].ToString() + "'";
                    cmd.CommandText += " WHERE payorder='" + feeDs.Tables["mzcheckpay"].Rows[0]["payorder"].ToString() + "'";
                    cmd.ExecuteNonQuery();
                    myTrans.Commit();
                }
                catch (Exception e)
                {
                    myTrans.Rollback();
                    mess = "更新审核数据异常:" + e.Message.ToString();
                    return false;
                }
                finally
                {
                    myTrans.Dispose();
                }
                #endregion
            }
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

    /// <summary>
    /// 计算(包括试算和正式结算)指定门诊病人的预审/审核补偿额


    /// </summary>
    /// <param name="hospitalcode"></param> //业务请求单位
    /// <param name="payorder"></param>
    /// <param name="chkpayid"></param>//8预审1审核
    /// <param name="operer"></param>
    /// <param name="settle"></param>//0为试算，1为正式结算


    /// <param name="data"></param>
    /// <returns></returns>
    public string settleMZ(string hospitalcode, string payorder, Int32 chkpayid, string opercode, string operer, Int32 settle, string dataXML, out string data)
    {
        //如果chkpayid=1，则要验证农合系统中的权限分配


        data = "";
        string mess, ls_human, ls_statisticyear, ls_patientname, ls_areacode2, ls_patsort, ls_jsgc;
        string ls_hospcode, ls_nopaydate;
        string ls_levelcode, ls_orglevel, ls_hospleve, ls_paydate; //机构级别,医院等级,收费标准,就诊日期
        Int32 li_chkpayidnow, li_accountstate, li_lockstate, li_hosptype, li_payid, li_tmp;
        decimal ld_grandprice, ld_havepay;
        string ls_zkh, ls_zksbm, ls_sfz, ls_sql, ls_hospresu, ls_hospresuid, ls_tmp;
        Int32 li_paymode = -1, li_approval = -1, li_orderid = -1, li_ifoutpatient = 0;//特殊门诊补助模式，需预先申报批准特殊门诊政策序号，是否特殊门诊


        string ls_outpatienttime = "", ls_orgareacode, ls_orgareacode2; //特殊门诊开始时间


        Int32 i, li_cnt, j;
        mess = "";
        DataSet setDS = null;
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


        if (settle == 1)
        {
            if (payorder == null || payorder.Length < 15)
            {
                mess = if0.getXMLErroStrFromString("2000", "补偿号不正确");
                return mess;
            }
        }
        else
        {
            payorder = "TestOrder";
        }
        if (operer.Length == 0)
        {
            operer = UserID;
        }
        #endregion
        SqlConnection Connection = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
        SqlCommand cmd = new SqlCommand();
        SqlDataReader myReader = null;
        try
        {
            Connection.Open();
            cmd.Connection = Connection;
            #region //补偿号是否存在并得到个人编码、参合年度、就诊日期、医院代码、就诊号等


            if (settle == 1) //正式结算的从数据库中取值


            #region
            {
                try
                {
                    cmd.CommandText = "SELECT count(*),c.human,c.statisticyear,c.paydate,c.chkpayid,c.payid,h.grandPrice,h.HospCode,convert(varchar(8),h.HospDate,112),h.hosptype,c.accountstate,c.lockstate FROM mz_checkpay c,mz_hosppatinfo2 h WHERE c.payorder=h.payorder AND c.payorder='" + payorder + "'";
                    cmd.CommandText += " GROUP BY c.human,c.statisticyear,c.paydate,c.chkpayid,c.payid,h.grandPrice,h.HospCode,h.HospDate,h.hosptype,c.accountstate,c.lockstate";
                    myReader = cmd.ExecuteReader();
                    try
                    {
                        if (!myReader.HasRows)
                        {
                            mess = if0.getXMLErroStrFromString("2000", "补偿号不存在");
                            return mess;
                        }
                        else
                        {
                            myReader.Read();
                            if (myReader.GetInt32(0) > 1)
                            {
                                mess = if0.getXMLErroStrFromString("2200", "本补偿号存在多个就诊信息，不允许审核");
                                return mess;
                            }
                            else
                            {
                                ls_human = myReader.GetString(1);
                                ls_statisticyear = myReader.GetString(2);
                                ls_paydate = myReader.GetString(3);
                                li_chkpayidnow = myReader.GetInt32(4);
                                li_payid = myReader.GetInt32(5);
                                ld_grandprice = myReader.GetDecimal(6);
                                ls_hospcode = myReader.GetString(7);
                                ls_nopaydate = myReader.GetString(8);
                                li_hosptype = myReader.GetInt32(9);
                                li_accountstate = myReader.GetInt32(10);
                                li_lockstate = myReader.GetInt32(11);
                            }
                        }
                    }
                    finally
                    {
                        if (!myReader.IsClosed)
                            myReader.Close();
                        myReader.Dispose();
                    }
                    #region //一些标志判断的有效性判断


                    if (li_accountstate == 1)
                    {
                        mess = if0.getXMLErroStrFromString("2200", "本门诊数据'已结算'，不能再处理");
                        return mess;
                    }
                    if (li_lockstate == 1)
                    {
                        mess = if0.getXMLErroStrFromString("2200", "本门诊数据'已冻结'，不能再处理");
                        return mess;
                    }
                    switch (chkpayid)
                    {
                        case 8:
                            if (li_chkpayidnow == 1)
                            {
                                mess = if0.getXMLErroStrFromString("2200", "本门诊数据审核状态是'已审核'，不能再预审");
                                return mess;
                            }
                            else if (li_chkpayidnow == 8)
                            {
                                mess = if0.getXMLErroStrFromString("2200", "本门诊数据审核状态是'已预审'，不能再预审");
                                return mess;
                            }
                            else if (li_chkpayidnow != 0)
                            {
                                mess = if0.getXMLErroStrFromString("2000", "未定义的审核状态，不能继续操作!请联系开发人员");
                                return mess;
                            }
                            break;
                        case 1:
                            if (li_chkpayidnow == 0)
                            {
                                mess = if0.getXMLErroStrFromString("2200", "本门诊数据审核状态是'未处理'，不能审核");
                                return mess;
                            }
                            else if (li_chkpayidnow == 1)
                            {
                                mess = if0.getXMLErroStrFromString("2200", "本门诊数据审核状态是'已审核'，不能再审核");
                                return mess;
                            }
                            else if (li_chkpayidnow != 8)
                            {
                                mess = if0.getXMLErroStrFromString("2000", "未定义的审核状态，不能继续操作!请联系开发人员");
                                return mess;
                            }
                            break;
                        //case 3:
                    }
                    #endregion
                }
                catch (Exception e)
                {
                    mess = if0.getXMLErroStrFromString("9000", "验证补偿号异常：" + e.Message.ToString());
                    return mess;
                }
                finally
                {
                    cmd.Dispose();
                }
                #region //参合有效性验证


                if (!if0.checkPersonsValid(AreaCode, ls_statisticyear, "", "", "", ls_human, out ls_patientname, "", out ls_human, out ls_areacode2, out ls_patsort, out mess))
                {
                    mess = if0.getXMLErroStrFromString("2000", "参合验证未通过，请咨询其参合资料是否变动！");
                    return mess;
                }
                #endregion
            }
            #endregion
            else //试算的从XML中取值生成DataSet
            #region
            {
                XmlDocument xmldoc = if0.getXMLDocumentFromString(dataXML, out mess);
                if (mess == "TRUE")
                {
                    try
                    {
                        XmlNodeList lis = xmldoc.GetElementsByTagName("mzh");
                        #region //基本变量获取
                        ls_hospcode = lis[0].InnerText;
                        if (!if0.isValidParm(ls_hospcode))
                        {
                            return if0.getXMLErroStrFromString("2000", "参数mzh错误");
                        }
                        lis = xmldoc.GetElementsByTagName("rq");
                        ls_nopaydate = lis[0].InnerText;
                        if (ls_nopaydate == null || ls_nopaydate.Trim().Length == 0)
                        {
                            return if0.getXMLErroStrFromString("2000", "rq不能为空");
                        }
                        else
                        {
                            if (!if0.isDateTime(ls_nopaydate))
                            {
                                return if0.getXMLErroStrFromString("2000", "rq格式必须为'2009-01-25 08:25:05'的形式");
                            }
                        }
                        lis = xmldoc.GetElementsByTagName("zkh");
                        ls_zkh = lis[0].InnerText;
                        if (!if0.isValidParm(ls_zkh))
                        {
                            return if0.getXMLErroStrFromString("2000", "参数zkh错误");
                        }
                        lis = xmldoc.GetElementsByTagName("zksbm");
                        ls_zksbm = lis[0].InnerText;
                        if (!if0.isNumber(ls_zksbm))
                        {
                            return if0.getXMLErroStrFromString("2000", "参数zksbm必须为数字型");
                        }
                        lis = xmldoc.GetElementsByTagName("xm");
                        ls_patientname = lis[0].InnerText;
                        if (!if0.isValidParm(ls_patientname))
                        {
                            return if0.getXMLErroStrFromString("2000", "参数xm错误");
                        }
                        lis = xmldoc.GetElementsByTagName("sfz");
                        ls_sfz = lis[0].InnerText;
                        if (!if0.isValidParm(ls_sfz))
                        {
                            return if0.getXMLErroStrFromString("2000", "参数sfz错误");
                        }
                        ls_statisticyear = ls_nopaydate.Substring(0, 4);
                        #region //参合有效性验证


                        if (!if0.checkPersonsValid(AreaCode, ls_statisticyear, ls_zkh, ls_zksbm, ls_patientname, "", out ls_patientname, ls_sfz, out ls_human, out ls_areacode2, out ls_patsort, out li_orderid, out li_ifoutpatient, out ls_outpatienttime, out mess))
                        {
                            mess = if0.getXMLErroStrFromString("2000", "参合验证未通过，请检查相关参数或咨询其参合资料是否变动！");
                            return mess;
                        }
                        #endregion
                        ls_paydate = ls_statisticyear + ls_nopaydate.Substring(5, 2) + ls_nopaydate.Substring(8, 2);
                        li_chkpayidnow = 0;
                        li_payid = 0;
                        lis = xmldoc.GetElementsByTagName("zje");
                        ld_grandprice = Convert.ToDecimal(lis[0].InnerText);
                        if (!if0.isNumber(lis[0].InnerText))
                        {
                            return if0.getXMLErroStrFromString("2000", "参数zje必须为数字型");
                        }
                        ls_nopaydate = ls_paydate;
                        lis = xmldoc.GetElementsByTagName("lx");
                        li_hosptype = Convert.ToInt32(lis[0].InnerText);
                        if (lis[0].InnerText == null || lis[0].InnerText.Trim().Length == 0 || !if0.isNumber(lis[0].InnerText))
                        {
                            mess += "lx必须为数字型";
                        }
                        else if (li_hosptype != 10 && li_hosptype != 20 && li_hosptype != 30 && li_hosptype != 40)
                        {
                            mess += "lx值必须为10,20,30,40(10普通门诊，20特殊慢病门诊，30体检，40门诊留观)";
                        }
                        li_accountstate = 0;
                        li_lockstate = 0;
                        #endregion
                        #region //生成DataSet
                        ls_sql = "SELECT * FROM mz_checkpay WHERE payorder='试算'";
                        setDS = if0.getDataSet(ls_sql, "mzcheckpay", out mess);
                        DataRow row = null;
                        #region //插入一行补偿表数据
                        if (mess == "TRUE")
                        {
                            row = setDS.Tables["mzcheckpay"].NewRow();
                            row["human"] = ls_human;
                            row["statisticyear"] = ls_statisticyear;
                            row["areacode2"] = ls_areacode2;
                            row["payorder"] = payorder;
                            row["grandPrice"] = ld_grandprice;
                            row["assfee"] = 0.0;
                            row["familyfound"] = 0.0;
                            row["paydate"] = ls_paydate;
                            row["nhpay"] = 0.0;
                            row["mediprice"] = 0.0;
                            row["mediselffee"] = 0.0;
                            row["materprice"] = 0.0;
                            row["materselffee"] = 0.0;
                            row["serverprice"] = 0.0;
                            row["serverselffee"] = 0.0;
                            row["chkpayid"] = 0;
                            row["prechkfee"] = 0.0;
                            row["chkinitsum"] = 0.0;
                            row["subsum"] = 0.0;
                            row["payid"] = 0;
                            setDS.Tables["mzcheckpay"].Rows.Add(row);
                        }
                        else
                        {
                            mess = if0.getXMLErroStrFromString("5000", mess);
                            return mess;
                        }
                        #endregion
                        ls_sql = "SELECT * FROM mz_hosppatinfo2 WHERE hospitalcode='' AND HospCode=''";
                        setDS = if0.getDataSet(setDS, ls_sql, "mzhosppatinfo2", out mess);
                        ls_sql = "SELECT * FROM mz_pricedetail WHERE hospitalcode='' AND HospCode=''";
                        setDS = if0.getDataSet(setDS, ls_sql, "fees", out mess);
                        XmlNodeList nlis = xmldoc.SelectSingleNode("DATA").ChildNodes; //获取DATA节点的所有子节点
                        li_cnt = nlis.Count;
                        if (li_cnt > 1)
                        {
                            mess = "不能包含两条病人资料数据";
                            mess = if0.getXMLErroStrFromString("2000", mess);
                            return mess;
                        }
                        for (i = 0; i < li_cnt; i++) //只允许一次审核一个门诊资料的
                        {
                            #region //插入病人资料
                            if (mess == "TRUE")
                            {
                                row = setDS.Tables["mzhosppatinfo2"].NewRow();
                                row["hospitalcode"] = hospitalcode;
                                lis = xmldoc.GetElementsByTagName("mzh");
                                ls_hospcode = lis[i].InnerText;
                                row["HospCode"] = ls_hospcode;
                                row["PATIENTNAME"] = ls_patientname;
                                lis = xmldoc.GetElementsByTagName("rq");
                                row["HospDate"] = DateTime.Parse(lis[i].InnerText);
                                lis = xmldoc.GetElementsByTagName("zt");
                                row["patstate"] = Convert.ToInt32(lis[i].InnerText);
                                lis = xmldoc.GetElementsByTagName("icdcode");
                                ls_hospresuid = lis[i].InnerText;
                                lis = xmldoc.GetElementsByTagName("icdname");
                                ls_hospresu = lis[i].InnerText;
                                #region //ICD有效性判断


                                if ((ls_hospresu == null || ls_hospresu.Trim().Length == 0) && (ls_hospresuid == null || ls_hospresuid.Trim().Length == 0))
                                {
                                    mess = "icdcode和icdname不能同时为空";
                                    mess = if0.getXMLErroStrFromString("5000", mess);
                                    return mess;
                                }
                                else
                                {
                                    //诊断名是否有效


                                    try
                                    {
                                        cmd.CommandText = "SELECT icdname FROM nhicd10";
                                        if (!(ls_hospresuid == null || ls_hospresuid.Trim().Length == 0))
                                        {
                                            cmd.CommandText += " WHERE icdcode='" + ls_hospresuid + "'";
                                        }
                                        else if (!(ls_hospresu == null || ls_hospresu.Trim().Length == 0))
                                        {
                                            cmd.CommandText += " WHERE icdcode=icdcode AND icdname='" + ls_hospresu + "'";
                                        }
                                        ls_hospresu = Convert.ToString(cmd.ExecuteScalar());
                                        if (ls_hospresu == "")
                                        {
                                            mess = "icdcode或icdname在农合ICD目录中不存在";
                                            mess = if0.getXMLErroStrFromString("5000", mess);
                                            return mess;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        mess = if0.getXMLErroStrFromString("9000", "数据库校验mzh=" + ls_hospcode + "的数据异常：" + e.Message.ToString());
                                        return mess;
                                    }
                                    //传上来的是特殊慢病门诊的类型的有效性判断


                                    if (li_hosptype == 20 && ls_hospresu != "")
                                    {
                                        try
                                        {
                                            //上传疾病是否是申请的疾病，政策是否有效


                                            ls_tmp = ls_nopaydate;
                                            if (li_ifoutpatient == 1)
                                            {
                                                cmd.CommandText = "SELECT icdname,paymode,approval FROM mz_arearcmsparm2 WHERE (areacode='" + AreaCode + "' AND '" + ls_tmp + "' BETWEEN validbegin AND validend";
                                                cmd.CommandText += " AND '" + ls_outpatienttime.Substring(0, 4) + ls_outpatienttime.Substring(5, 2) + ls_outpatienttime.Substring(8, 2) + "' BETWEEN validbegin AND validend";
                                                cmd.CommandText += " AND icdname='" + ls_hospresu + "' AND orderid=" + li_orderid.ToString() + ")";
                                                cmd.CommandText += " OR (areacode='" + AreaCode + "' AND '" + ls_tmp + "' BETWEEN validbegin AND validend AND icdname='" + ls_hospresu + "' AND approval=0) ORDER BY approval DESC";
                                            }
                                            else
                                            {
                                                cmd.CommandText = "SELECT icdname,paymode,approval FROM mz_arearcmsparm2 WHERE areacode='" + AreaCode + "' AND '" + ls_tmp + "' BETWEEN validbegin AND validend AND icdname='" + ls_hospresu + "' AND approval=0";
                                            }
                                            myReader = cmd.ExecuteReader();
                                            if (!myReader.HasRows)
                                            {
                                                mess = "没有查到" + ls_hospresu + "的有效特殊慢病政策";//HIS传过来的是慢病，因此要返回指定这个错误


                                                mess = if0.getXMLErroStrFromString("5000", mess);
                                                return mess;
                                            }
                                            else
                                            {
                                                myReader.Read();
                                                ls_tmp = myReader.GetString(0);//疾病
                                                li_paymode = myReader.GetInt32(1);//补助模式
                                                li_approval = myReader.GetInt32(2);//需预先申报批准
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            mess = if0.getXMLErroStrFromString("9000", "数据库校验mzh=" + ls_hospcode + "的特殊慢病审批数据异常：" + e.Message.ToString());
                                            return mess;
                                        }
                                        finally
                                        {
                                            if (!myReader.IsClosed)
                                                myReader.Close();
                                            myReader.Dispose();
                                        }
                                    }
                                    //虽未指定慢病门诊类型，但是特殊门诊有效申报者的
                                    else if (li_ifoutpatient == 1 && ls_hospresu != "")
                                    {
                                        try
                                        {
                                            //上传疾病是否是特殊慢病疾病，政策是否有效，是则补偿类别为20
                                            ls_tmp = ls_nopaydate;
                                            cmd.CommandText = "SELECT icdname,paymode,approval FROM mz_arearcmsparm2 WHERE (areacode='" + AreaCode + "' AND '" + ls_tmp + "' BETWEEN validbegin AND validend";
                                            cmd.CommandText += " AND '" + ls_outpatienttime.Substring(0, 4) + ls_outpatienttime.Substring(5, 2) + ls_outpatienttime.Substring(8, 2) + "' BETWEEN validbegin AND validend";
                                            cmd.CommandText += " AND icdname='" + ls_hospresu + "' AND orderid=" + li_orderid.ToString() + ")";
                                            cmd.CommandText += " OR (areacode='" + AreaCode + "' AND '" + ls_tmp + "' BETWEEN validbegin AND validend AND icdname='" + ls_hospresu + "' AND approval=0) ORDER BY approval DESC";
                                            myReader = cmd.ExecuteReader();
                                            if (myReader.HasRows)
                                            {
                                                li_hosptype = 20;
                                                myReader.Read();
                                                ls_tmp = myReader.GetString(0);//疾病
                                                li_paymode = myReader.GetInt32(1);//补助模式
                                                li_approval = myReader.GetInt32(2);//需预先申报批准
                                                //if (ls_tmp != ls_hospresu) //根据查询条件，这句应该总是不会执行
                                                //{
                                                //    mess = "没有查到" + ls_hospresu + "的有效特殊慢病政策";
                                                //    mess = getXMLErroStrFromString("5000", mess);
                                                //    return mess;
                                                //}
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            mess = if0.getXMLErroStrFromString("9000", "数据库校验mzh=" + ls_hospcode + "的特殊慢病审批数据异常：" + e.Message.ToString());
                                            return mess;
                                        }
                                        finally
                                        {
                                            if (!myReader.IsClosed)
                                                myReader.Close();
                                            myReader.Dispose();
                                        }
                                    }
                                }
                                #endregion
                                row["hosptype"] = li_hosptype;
                                row["HospResu"] = ls_hospresu;
                                lis = xmldoc.GetElementsByTagName("zje");
                                row["grandPrice"] = Convert.ToDecimal(lis[i].InnerText);
                                lis = xmldoc.GetElementsByTagName("jm");
                                row["deratesum"] = Convert.ToDecimal(lis[i].InnerText);
                                row["human"] = ls_human;
                                row["statisticyear"] = ls_statisticyear;
                                row["areacode"] = AreaCode;
                                row["areacode2"] = ls_areacode2;
                                lis = xmldoc.GetElementsByTagName("zyzl");
                                row["ifchina"] = Convert.ToInt32(lis[i].InnerText);
                                row["chkpayid"] = 0;
                                row["payorder"] = payorder;
                                setDS.Tables["mzhosppatinfo2"].Rows.Add(row);
                            }
                            else
                            {
                                mess = if0.getXMLErroStrFromString("5000", mess);
                                return mess;
                            }
                            #endregion
                            lis = xmldoc.GetElementsByTagName("fee");
                            nlis = lis[i].ChildNodes; //获取fee节点的所有子节点
                            li_tmp = nlis.Count;
                            for (j = 0; j < li_tmp; j++)
                            {
                                #region //插入费用数据
                                if (mess == "TRUE")
                                {
                                    row = setDS.Tables["fees"].NewRow();
                                    row["hospitalcode"] = hospitalcode;
                                    row["HospCode"] = ls_hospcode;
                                    lis = xmldoc.GetElementsByTagName("fyid");
                                    if (Convert.ToString(lis[j].InnerText) == null || Convert.ToString(lis[j].InnerText).Trim().Length == 0 || !if0.isNumber(Convert.ToString(lis[j].InnerText)))
                                    {
                                        mess = "fyid必须为数字型";
                                        mess = if0.getXMLErroStrFromString("5000", mess);
                                        return mess;
                                    }
                                    row["detailid"] = Convert.ToString(lis[j].InnerText);
                                    lis = xmldoc.GetElementsByTagName("hisname");
                                    if (Convert.ToString(lis[j].InnerText) == null || Convert.ToString(lis[j].InnerText).Trim().Length == 0)
                                    {
                                        mess = "hisname不能为空";
                                        mess = if0.getXMLErroStrFromString("5000", mess);
                                        return mess;
                                    }
                                    row["mediname"] = Convert.ToString(lis[j].InnerText);
                                    lis = xmldoc.GetElementsByTagName("nhdm");
                                    if (lis[j].InnerText == null || lis[j].InnerText.Trim().Length == 0)
                                    {
                                        mess = "nhdm不能为空，" + row["mediname"] + "没匹配";
                                        mess = if0.getXMLErroStrFromString("5000", mess);
                                        return mess;
                                    }
                                    //未完成 这里还可以加入判断农合代码是否正确的判断，并计算出是否保内


                                    row["nhcode"] = lis[j].InnerText;
                                    lis = xmldoc.GetElementsByTagName("fylx");
                                    if (Convert.ToString(lis[j].InnerText) == null || Convert.ToString(lis[j].InnerText).Trim().Length == 0 || !if0.isNumber(Convert.ToString(lis[j].InnerText)))
                                    {
                                        mess = "fylx必须为数字型";
                                        mess = if0.getXMLErroStrFromString("5000", mess);
                                        return mess;
                                    }
                                    else if (Convert.ToString(lis[j].InnerText) != "0" && Convert.ToString(lis[j].InnerText) != "1" && Convert.ToString(lis[j].InnerText) != "2")
                                    {
                                        mess = "fylx必须为指定的数字0药品1材料2医疗服务";
                                        mess = if0.getXMLErroStrFromString("5000", mess);
                                        return mess;
                                    }
                                    row["MarkType"] = Convert.ToString(lis[j].InnerText);
                                    lis = xmldoc.GetElementsByTagName("fysj");
                                    if (Convert.ToString(lis[j].InnerText) == null || Convert.ToString(lis[j].InnerText).Trim().Length == 0)
                                    {
                                        mess = "fysj不能为空";
                                        mess = if0.getXMLErroStrFromString("5000", mess);
                                        return mess;
                                    }
                                    else
                                    {
                                        if (!if0.isDateTime(Convert.ToString(lis[j].InnerText)))
                                        {
                                            mess = "fysj格式必须为'2009-01-25 08:25:45'的形式";
                                            mess = if0.getXMLErroStrFromString("5000", mess);
                                            return mess;
                                        }
                                    }
                                    row["ddate"] = DateTime.Parse(lis[j].InnerText);
                                    lis = xmldoc.GetElementsByTagName("sl");
                                    if (Convert.ToString(lis[j].InnerText) == null || Convert.ToString(lis[j].InnerText).Trim().Length == 0 || !if0.isNumber(Convert.ToString(lis[j].InnerText)))
                                    {
                                        mess = "sl必须为数字型";
                                        mess = if0.getXMLErroStrFromString("5000", mess);
                                        return mess;
                                    }
                                    row["user_num"] = Convert.ToDecimal(lis[j].InnerText);
                                    lis = xmldoc.GetElementsByTagName("dj");
                                    if (lis[j].InnerText == null || lis[j].InnerText.Trim().Length == 0 || !if0.isNumber(lis[j].InnerText))
                                    {
                                        mess = "dj必须为数字型";
                                        mess = if0.getXMLErroStrFromString("5000", mess);
                                        return mess;
                                    }
                                    row["unitprice"] = Convert.ToDecimal(lis[j].InnerText);
                                    lis = xmldoc.GetElementsByTagName("je");
                                    if (lis[j].InnerText == null || lis[j].InnerText.Trim().Length == 0 || !if0.isNumber(lis[j].InnerText))
                                    {
                                        mess = "je必须为数字型";
                                        mess = if0.getXMLErroStrFromString("5000", mess);
                                        return mess;
                                    }
                                    row["price"] = Convert.ToDecimal(lis[j].InnerText);
                                    lis = xmldoc.GetElementsByTagName("hismlid");
                                    if (lis[j].InnerText == null || lis[j].InnerText.Trim().Length == 0 || !if0.isNumber(lis[j].InnerText))
                                    {
                                        mess = "hismlid必须为数字型";
                                        mess = if0.getXMLErroStrFromString("5000", mess);
                                        return mess;
                                    }
                                    row["mediid"] = Convert.ToDecimal(lis[j].InnerText);
                                    row["selffee"] = 0.0;
                                    row["autochkfee"] = 0.0;
                                    row["checkid"] = 0;
                                    row["in_out"] = 0;
                                    row["chkpayid"] = 0;
                                    row["prechkfee"] = 0.0;
                                    row["chkinitfee"] = 0.0;
                                    setDS.Tables["fees"].Rows.Add(row);
                                }
                                else
                                {
                                    mess = if0.getXMLErroStrFromString("5000", mess);
                                    return mess;
                                }
                                #endregion
                            }
                        }
                        #endregion
                    }
                    catch (Exception e)
                    {
                        return if0.getXMLErroStrFromString("9000", "异常：" + e.Message.ToString());
                    }
                }
                else
                {
                    return if0.getXMLErroStrFromString("9000", mess);
                }
            }
            #endregion
            #endregion
            #region 门诊医疗机构兑付区域检验

            cmd.CommandText = "Select areacode2,arealevel From mz_org_area Where areacode = '" + AreaCode + "' And orgcode = '" +hospitalcode+ "'";
            myReader = cmd.ExecuteReader();
            try
            {
                if (myReader.HasRows)
                {
                    bool lb_pay = false;
                    string ls_areacode22;
                    Int32 li_arealevel;
                    while (myReader.Read())
                    {
                        ls_areacode22 = myReader.GetString(0);
                        li_arealevel = myReader.GetInt32(1);
                        switch (li_arealevel)
                        {
                            case 3:
                                lb_pay = true;
                                break;
                            case 4:
                                if (ls_areacode2.Substring(1, 2) == ls_areacode22.Substring(1, 2))
                                {
                                    lb_pay = true;
                                }
                                break;
                            case 5:
                                if (ls_areacode2.Substring(1, 4) == ls_areacode22.Substring(1, 4))
                                {
                                    lb_pay = true;
                                }
                                break;
                            case 6:
                                if (ls_areacode2 == ls_areacode22)
                                {
                                    lb_pay = true;
                                }
                                break;
                        }
                    }
                    if (!lb_pay)
                    {
                        mess = if0.getXMLErroStrFromString("2200", "该医疗机构没有得到对参合区域编号:" + ls_areacode2 + "进行门诊兑付的赋权，无法进行门诊兑付。");
                        return mess;
                    }
                }
            }
            finally
            {
                if (!myReader.IsClosed)
                    myReader.Close();
                myReader.Dispose();
            }
            #endregion
            #region //查询机构级别,医院等级,收费标准
            try
            {
                cmd.CommandText = "SELECT o.levelcode,o.orglevel,a.hospleve,o.areacode,o.areacode2 FROM organcode o,areaorgancode a WHERE o.orgcode=a.orgcode AND o.orgcode='" + hospitalcode + "' AND ifvalid='1'";
                if (ls_paydate.Length == 4) //按年补偿的只有年份


                {
                    cmd.CommandText += " AND validbegin<='" + ls_paydate + "0101' AND validend>='" + ls_paydate + "0101'";
                }
                else if (ls_paydate.Length == 6) //按月补偿的只有年月


                {
                    cmd.CommandText += " AND validbegin<='" + ls_paydate + "01' AND validend>='" + ls_paydate + "01'";
                }
                else
                {
                    cmd.CommandText += " AND validbegin<='" + ls_paydate + "' AND validend>='" + ls_paydate + "'";
                }
                myReader = cmd.ExecuteReader();
                if (!myReader.HasRows)
                {
                    mess = if0.getXMLErroStrFromString("2000", "获取机构信息失败，请检查是否定点机构");
                    return mess;
                }
                else
                {
                    myReader.Read();
                    ls_levelcode = myReader.GetString(0);//机构级别
                    ls_orglevel = myReader.GetString(1);//医院等级
                    ls_hospleve = myReader.GetString(2);//收费标准
                    ls_orgareacode = myReader.GetString(3);
                    ls_orgareacode2 = myReader.GetString(4);
                }
            }
            catch (Exception e)
            {
                mess = if0.getXMLErroStrFromString("9000", "获取机构信息异常：" + e.Message.ToString());
                return mess;
            }
            finally
            {
                if (!myReader.IsClosed)
                    myReader.Close();
                myReader.Dispose();
                cmd.Dispose();
            }
            #endregion
            #region //费用审核
            if (!mzCalcCompensate(payorder, hospitalcode, ls_hospcode, ls_levelcode, chkpayid, operer, settle, setDS, out setDS, out mess))
            {
                mess = if0.getXMLErroStrFromString("9000", mess);
                return mess;
            }
            #endregion
            decimal ld_assfee, ld_familyfound, ld_nhpay, ld_familyaccbalance;
            #region //计算补偿金额。


            if (!mzCalcPayFee(payorder, ls_areacode2, ls_levelcode, ls_orgareacode, ls_orgareacode2, chkpayid, operer, settle, setDS, out setDS, out ld_assfee, out ld_familyfound, out ld_nhpay, out ld_familyaccbalance, out ls_jsgc))
            {
                mess = if0.getXMLErroStrFromString("9000", ls_jsgc);
                return mess;
            }
            #endregion
            #region //正式结算要更新标志和扣减家庭账户
            //更新结算标志和金额


            SqlTransaction myTrans;
            myTrans = Connection.BeginTransaction();
            try
            {
                cmd.Transaction = myTrans;
                if (settle == 1)
                {
                    switch (chkpayid)
                    {
                        case 8:
                            //if (li_payid == 0) //未打印支付的，则将打印支付标记和支付金额记录下


                            //{
                            //    cmd.CommandText = "UPDATE mz_checkpay SET chkpayid='" + chkpayid.ToString() + "',prechkfee=" + ld_nhpay.ToString() + ",";
                            //    cmd.CommandText += "prechkopr='" + operer + "',prechktime=getdate(),chkinitsum=" + ld_nhpay.ToString() + ",payid=1,organpay='" + hospitalcode + "',paytime=getdate(),";
                            //    cmd.CommandText += "subsum=" + ld_nhpay.ToString() + ",subopr='" + operer + "',subtime=getdate()";
                            //    cmd.CommandText += " WHERE payorder='" + payorder + "'";
                            //    li_payid = 1;//本次设置了打印支付标志了
                            //}
                            //else
                            //{
                            cmd.CommandText = "UPDATE mz_checkpay SET chkpayid='" + chkpayid.ToString() + "',prechkfee=" + ld_nhpay.ToString() + ",";
                            cmd.CommandText += "prechkopr='" + operer + "',prechktime=getdate(),chkinitsum=" + ld_nhpay.ToString();
                            cmd.CommandText += " WHERE payorder='" + payorder + "'";
                            cmd.ExecuteNonQuery();
                            li_payid = 0;//本次没有设置打印支付标志
                            #region 体检申报的置申报资料完成体检状态


                            if (li_hosptype == 30)
                            {
                                cmd.CommandText = "UPDATE Tb_declare_roster SET state=40,payorder='" + payorder + "'";
                                cmd.CommandText += " WHERE autoprekey=" + Convert.ToString(setDS.Tables["mzhosppatinfo2"].Rows[0]["autoprekey"]) + " AND state=20";
                                cmd.ExecuteNonQuery();
                            }
                            #endregion
                            //}
                            break;
                        case 1:
                            //if (li_payid == 0) //未打印支付的
                            //{
                            //    cmd.CommandText = "UPDATE mz_checkpay SET chkpayid='" + chkpayid.ToString() + "',prechkfee=case when isnull(prechkopr,'')='' then " + ld_nhpay.ToString() + " else prechkfee end,";
                            //    cmd.CommandText += "chkinitsum=" + ld_nhpay.ToString() + ",chkinitopr='" + operer + "',chkinittime=getdate(),payid=1,organpay='" + hospitalcode + "',paytime=getdate(),";
                            //    cmd.CommandText += "subsum=" + ld_nhpay.ToString() + ",subopr='" + operer + "',subtime=getdate()";
                            //    cmd.CommandText += " WHERE payorder='" + payorder + "'";
                            //    li_payid = 1;//本次设置了打印支付标志了
                            //}
                            //else
                            //{
                            cmd.CommandText = "UPDATE mz_checkpay SET chkpayid='" + chkpayid.ToString() + "',prechkfee=case when isnull(prechkopr,'')='' then " + ld_nhpay.ToString() + " else prechkfee end,";
                            cmd.CommandText += "chkinitsum=" + ld_nhpay.ToString() + ",chkinitopr='" + operer + "',chkinittime=getdate()";
                            cmd.CommandText += " WHERE payorder='" + payorder + "'";
                            cmd.ExecuteNonQuery();
                            li_payid = 0;//本次没有设置打印支付标志
                            //}
                            break;
                    }
                    //下列语句因为在费用明细审核方法中已赋值，因此这里不需要再更新了


                    //cmd.CommandText = "UPDATE mz_pricedetail SET chkpayid=" + chkpayid.ToString() + " WHERE hospitalcode='" + hospitalcode + "' AND HospCode='" + ls_hospcode + "'";
                    //cmd.ExecuteNonQuery();
                    //下列语句因为chkpayid用来作为家庭基金扣减标志，因此这里屏蔽


                    //cmd.CommandText = "UPDATE mz_hosppatinfo2 SET chkpayid=1 WHERE payorder='" + payorder + "'";
                    //cmd.ExecuteNonQuery();
                }
                cmd.CommandText = "UPDATE mz_pataccountdetail SET calcinfo='" + ls_jsgc + "' WHERE payorder='" + payorder + "' AND chkpayid=" + chkpayid.ToString();
                cmd.ExecuteNonQuery();
                myTrans.Commit();
            }
            catch (Exception e)
            {
                myTrans.Rollback();
                mess = if0.getXMLErroStrFromString("9000", "数据库保存结算数据异常：" + e.Message.ToString());
                return mess;
            }
            finally
            {
                myTrans.Dispose();
            }
            #endregion
            //#region //未打印支付的，扣家庭账户金额
            //if (settle == 1)
            //{
            //    if (li_payid == 0) //未打印支付的，扣家庭账户金额
            //    {
            //        if (!setPersonAccount(hospitalcode, ls_hospcode, 2, 1, 0, ld_nhpay, ld_grandprice, ls_human, ls_statisticyear, ls_nopaydate, out mess))
            //        {
            //            mess = getXMLErroStrFromString("9000", mess);
            //            return mess;
            //        }
            //    }
            //}
            //#endregion
            #region //构造返回数据


            #region //补偿号


            if (settle == 1)
            {
                data = "<bch>" + payorder + "</bch>"; //补偿号


            }
            else
            {
                data = "<bch></bch>"; //补偿号


            }
            #endregion
            #region // 补偿类别
            switch (li_hosptype)
            {
                case 10:
                    data += "<bclb>普通门诊</bclb>";
                    break;
                case 20:
                    data += "<bclb>特殊慢病门诊</bclb>";
                    break;
                case 30:
                    data += "<bclb>体检</bclb>";
                    break;
                case 40:
                    data += "<bclb>门诊留观</bclb>";
                    break;
                default:
                    data += "<bclb>未定义</bclb>";
                    break;
            }
            #endregion
            #region //补偿日期，已获补助


            try
            {
                cmd.CommandText = "SELECT isnull(sum(p.subsum),0.0) FROM mz_checkpay p WHERE p.human='" + ls_human + "' AND p.statisticyear='" + ls_statisticyear + "'";
                cmd.CommandText += "  AND p.payorder<>'" + payorder + "' AND p.payid=1 AND p.orderid<1000";
                cmd.CommandText += " AND left(p.paydate,4)='" + ls_statisticyear + "'";
                myReader = cmd.ExecuteReader();
                if (!myReader.HasRows)
                {
                    //mess = getXMLErroStrFromString("2000", "获取当年已获补助失败");
                    //return mess;
                    if (settle == 1)
                    {
                        if (!myReader.IsClosed)
                            myReader.Close();
                        cmd.CommandText = "SELECT convert(varchar,getdate(),120)";
                        data += "<bcrq>" + Convert.ToString(cmd.ExecuteScalar()) + "</bcrq>";
                    }
                    else
                    {
                        data += "<bcrq></bcrq>";
                    }
                    data += "<havepay></havepay>";
                }
                else
                {
                    myReader.Read();
                    ld_havepay = myReader.GetDecimal(0);
                    myReader.Close();
                    if (settle == 1)
                    {
                        cmd.CommandText = "SELECT convert(varchar,prechktime,120) FROM mz_checkpay WHERE payorder='" + payorder + "'";
                        ls_tmp = Convert.ToString(cmd.ExecuteScalar());
                        if (ls_tmp == null || ls_tmp.Length == 0)
                        {
                            data += "<bcrq></bcrq>";
                        }
                        else
                        {
                            data += "<bcrq>" + ls_tmp + "</bcrq>";
                        }
                    }
                    else
                    {
                        data += "<bcrq></bcrq>";
                    }
                    data += "<havepay>" + Convert.ToString(ld_havepay) + "</havepay>";
                }
            }
            catch (Exception e)
            {
                data = "";
                mess = if0.getXMLErroStrFromString("9000", "获取当年已获补助异常：" + e.Message.ToString());
                return mess;
            }
            finally
            {
                if (!myReader.IsClosed)
                    myReader.Close();
                myReader.Dispose();
                cmd.Dispose();
            }
            #endregion
            #region //机构级别
            switch (ls_levelcode)
            {
                case "1":
                    data += "<orglev>村卫生室</orglev>";
                    break;
                case "2":
                    data += "<orglev>乡镇卫生院</orglev>";
                    break;
                case "3":
                    data += "<orglev>县级医疗机构</orglev>";
                    break;
                case "4":
                    data += "<orglev>地市医疗机构</orglev>";
                    break;
                case "5":
                    data += "<orglev>省级及以上医疗机构</orglev>";
                    break;
                default:
                    data += "<orglev>其他医疗机构</orglev>";
                    break;
            }
            #endregion
            //应补金额
            data += "<assfee>" + ld_assfee.ToString() + "</assfee>";
            //家庭帐户补助额


            data += "<familypay>" + ld_familyfound.ToString() + "</familypay>";
            //实际补偿额


            data += "<pay>" + ld_nhpay.ToString() + "</pay>";
            //家庭账户余额
            data += "<fbalance>" + ld_familyaccbalance.ToString() + "</fbalance>";
            //计算过程描述
            data += "<jsgc>" + ls_jsgc + "</jsgc>";
            if (settle == 1)
            {
                #region //医疗费用和可报费用


                try
                {
                    cmd.CommandText = "SELECT isnull(sum(fee1),0.0),isnull(sum(fee2),0.0),isnull(sum(fee3),0.0),isnull(sum(fee4),0.0),isnull(sum(fee5),0.0),isnull(sum(fee6),0.0),isnull(sum(fee7),0.0),isnull(sum(fee8),0.0),isnull(sum(fee9),0.0),isnull(sum(fee10),0.0),isnull(sum(fee11),0.0),";
                    cmd.CommandText += "isnull(sum(fee12),0.0),isnull(sum(fee13),0.0),isnull(sum(fee14),0.0),isnull(sum(fee15),0.0),isnull(sum(fee16),0.0) FROM mz_pataccountdetail WHERE (payorder='" + payorder + "')";
                    cmd.CommandText += " AND (chkpayid=0 OR chkpayid=" + chkpayid.ToString() + ") GROUP BY chkpayid ORDER BY chkpayid";
                    myReader = cmd.ExecuteReader();
                    if (!myReader.HasRows)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "没能获取到分类医疗费用和可报费用");
                        return mess;
                    }
                    else
                    {
                        myReader.Read();
                        for (li_tmp = 0; li_tmp < 16; li_tmp++)
                        {
                            data += "<fee" + Convert.ToString(li_tmp + 1) + ">" + Convert.ToString(myReader.GetDecimal(li_tmp)) + "</fee" + Convert.ToString(li_tmp + 1) + ">";
                        }
                        myReader.Read();
                        for (li_tmp = 0; li_tmp < 16; li_tmp++)
                        {
                            data += "<kbfee" + Convert.ToString(li_tmp + 1) + ">" + Convert.ToString(myReader.GetDecimal(li_tmp)) + "</kbfee" + Convert.ToString(li_tmp + 1) + ">";
                        }
                    }
                }
                catch (Exception e)
                {
                    data = "";
                    mess = if0.getXMLErroStrFromString("9000", "获取当年已获补助异常：" + e.Message.ToString());
                    return mess;
                }
                finally
                {
                    if (!myReader.IsClosed)
                        myReader.Close();
                    myReader.Dispose();
                    cmd.Dispose();
                }
                #endregion
            }
            else
            {
                #region //分类费用和自费费用


                data += "<mfee>" + Convert.ToString(setDS.Tables["mzcheckpay"].Rows[0]["mediprice"]) + "</mfee>";
                data += "<mself>" + Convert.ToString(setDS.Tables["mzcheckpay"].Rows[0]["mediselffee"]) + "</mself>";
                data += "<mafee>" + Convert.ToString(setDS.Tables["mzcheckpay"].Rows[0]["materprice"]) + "</mafee>";
                data += "<maself>" + Convert.ToString(setDS.Tables["mzcheckpay"].Rows[0]["materselffee"]) + "</maself>";
                data += "<sfee>" + Convert.ToString(setDS.Tables["mzcheckpay"].Rows[0]["serverprice"]) + "</sfee>";
                data += "<sself>" + Convert.ToString(setDS.Tables["mzcheckpay"].Rows[0]["serverselffee"]) + "</sself>";
                #endregion
            }
            #endregion
            mess = if0.getXMLStrFromString("计算成功");
            if0.getXMLStrFromStringData("<ROW>" + data + "</ROW>", out data);
            return mess;
        }
        catch (Exception e)
        {
            data = "";
            mess = if0.getXMLErroStrFromString("9000", "异常:" + e.Message.ToString());
            return mess;
        }
        finally
        {
            cmd.Dispose();
            if (Connection.State == ConnectionState.Open)
                Connection.Close();
            if (setDS != null)
            {
                setDS.Dispose();
            }
        }
    }

    /// <summary>
    /// 取消指定补偿号的门诊结算
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="opercode"></param>
    /// <param name="operer"></param>
    /// <param name="payorder"></param>
    /// <returns></returns>
    public string settleMZCancel(string areacode, string hospitalcode, string opercode, string operer, string payorder)
    {
        if (operer.Length == 0)
        {
            operer = UserID;
        }
        string mess = "", ls_dtnow = "", ls_hospcode = "", ls_human = "", ls_statisticyear = "", ls_hospdate = "";
        Int32 li_chkpayid, li_payid, li_accountstate, li_lockstate, li_hosptype;
        decimal ld_grandprice, ld_nhpay, ld_autoprekey;
        SqlConnection Connection = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
        SqlCommand cmd = new SqlCommand();
        SqlDataReader myReader;
        try
        {
            Connection.Open();
            cmd.Connection = Connection;
            #region //验证审核状态是否可以取消结算


            try
            {
                cmd.CommandText = "SELECT count(*),c.chkpayid,c.payid,convert(varchar,getdate(),120),h.HospCode,h.nhpay,h.human,h.statisticyear,h.grandPrice,convert(varchar(8),h.HospDate,112),c.accountstate,c.lockstate,h.hosptype,h.autoprekey";
                cmd.CommandText += " FROM mz_checkpay c,mz_hosppatinfo2 h WHERE c.payorder=h.payorder AND c.payorder='" + payorder + "'";
                cmd.CommandText += " GROUP BY c.chkpayid,c.payid,h.HospCode,h.nhpay,h.human,h.statisticyear,h.grandPrice,h.HospDate,c.accountstate,c.lockstate,h.hosptype,h.autoprekey";
                myReader = cmd.ExecuteReader();
                try
                {
                    if (!myReader.HasRows)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "补偿号不存在");
                        return mess;
                    }
                    else
                    {
                        myReader.Read();
                        if (myReader.GetInt32(0) > 1)
                        {
                            mess = if0.getXMLErroStrFromString("2200", "本补偿号存在多个就诊信息，不允许取消结算");
                            return mess;
                        }
                        else
                        {
                            li_chkpayid = myReader.GetInt32(1);
                            li_payid = myReader.GetInt32(2);
                            ls_dtnow = myReader.GetString(3);
                            ls_hospcode = myReader.GetString(4);
                            ld_nhpay = myReader.GetDecimal(5);
                            ls_human = myReader.GetString(6);
                            ls_statisticyear = myReader.GetString(7);
                            ld_grandprice = myReader.GetDecimal(8);
                            ls_hospdate = myReader.GetString(9);
                            li_accountstate = myReader.GetInt32(10);
                            li_lockstate = myReader.GetInt32(11);
                            li_hosptype = myReader.GetInt32(12);
                            ld_autoprekey = myReader.GetDecimal(13);
                        }
                    }
                    myReader.Close();
                    //已结算的不允许取消


                    if (li_accountstate == 1)
                    {
                        mess = if0.getXMLErroStrFromString("2200", "本门诊补偿已与合管基金月结，不能取消结算。");
                        return mess;
                    }
                    //已冻结的不允许取消


                    if (li_lockstate == 1)
                    {
                        mess = if0.getXMLErroStrFromString("2200", "本门诊补偿数据已被冻结，不能取消结算。");
                        return mess;
                    }
                    //个人结算单打印后是否允许取消预审
                    if (li_payid == 1)
                    {
                        cmd.CommandText = "SELECT mzunchk FROM areasysparm WHERE areacode='" + AreaCode + "'";
                        myReader = cmd.ExecuteReader();
                        if (!myReader.HasRows)
                        {
                            mess = if0.getXMLErroStrFromString("2000", "农合系统中还没有设置系统参数");
                            return mess;
                        }
                        else
                        {
                            myReader.Read();
                            if (myReader.GetInt32(0) == 0)
                            {
                                mess = if0.getXMLErroStrFromString("2200", "本门诊补偿'已支付'，农合系统中配置成了不能取消结算。取消农合结算失败");
                                return mess;
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
                switch (li_chkpayid)
                {
                    case 0:
                        mess = if0.getXMLErroStrFromString("2200", "本门诊数据为'未处理'状态，不需要取消结算");
                        return mess;
                    case 1:
                        mess = if0.getXMLErroStrFromString("2200", "本门诊数据为'已审核'状态，不能取消结算");
                        return mess;
                    case 2:
                        mess = if0.getXMLErroStrFromString("2200", "本门诊数据为'已确认'状态，不能取消结算");
                        return mess;
                    case 3:
                        mess = if0.getXMLErroStrFromString("2200", "本门诊数据为'已复审'状态，不能取消结算");
                        return mess;
                    case 4:
                        mess = if0.getXMLErroStrFromString("2200", "本门诊数据为'已审批'状态，不能取消结算");
                        return mess;
                    case 5:
                        mess = if0.getXMLErroStrFromString("2200", "本门诊数据为'已提交'状态，不能取消结算");
                        return mess;
                    case 6:
                        mess = if0.getXMLErroStrFromString("2200", "本门诊数据为'已重算'状态，不能取消结算");
                        return mess;
                    case 7:
                        mess = if0.getXMLErroStrFromString("2200", "本门诊数据为'已补兑'状态，不能取消结算");
                        return mess;
                    case 8:
                        break;
                    default:
                        mess = if0.getXMLErroStrFromString("2200", "未定义的审核状态，不能继续操作!请联系开发人员");
                        return mess;
                }
            }
            catch (Exception e)
            {
                mess = if0.getXMLErroStrFromString("9000", "验证补偿号异常：" + e.Message.ToString());
                return mess;
            }
            finally
            {
                cmd.Dispose();
            }
            #endregion
            #region //只有打印了的，才更新家庭帐户
            if (li_payid == 1)
            {
                if (!if0.setPersonAccount(hospitalcode, ls_hospcode, 2, 2, 0, ld_nhpay, ld_grandprice, ls_human, ls_statisticyear, ls_hospdate, out mess))
                {
                    mess = if0.getXMLErroStrFromString("9000", mess);
                    return mess;
                }
            }
            #endregion
            #region //更新标志信息、删除就诊相关资料


            SqlTransaction myTrans;
            myTrans = Connection.BeginTransaction();
            try
            {
                cmd.Transaction = myTrans;
                cmd.CommandText = "UPDATE mz_checkpay SET chkpayid=0,prechkopr='" + operer + "',prechktime='" + ls_dtnow + "',payid=0,paytime='" + ls_dtnow + "',subsum=0.0,subopr='" + operer + "' WHERE payorder='" + payorder + "'";
                //cmd.CommandText = "UPDATE mz_checkpay SET chkpayid=0,prechkopr='" + operer + "',prechktime='" + ls_dtnow + "' WHERE payorder='" + payorder + "'";
                cmd.ExecuteNonQuery();
                //cmd.CommandText = "UPDATE mz_hosppatinfo2 SET chkpayid=0 WHERE payorder='" + payorder + "'";
                //cmd.ExecuteNonQuery();
                #region 体检申报的置申报资料已审核状态


                if (li_hosptype == 30)
                {
                    cmd.CommandText = "UPDATE Tb_declare_roster SET state=20,payorder=''";
                    cmd.CommandText += " WHERE autoprekey=" + ld_autoprekey.ToString() + " AND state=40";
                    cmd.ExecuteNonQuery();
                }
                #endregion
                #region 删除掉此门诊相关资料,以免保存垃圾数据
                cmd.CommandText = "DELETE FROM mz_pataccountdetail  WHERE EXISTS (SELECT * FROM mz_hosppatinfo2 WHERE mz_hosppatinfo2.payorder='" + payorder + "'";
                cmd.CommandText += " AND mz_hosppatinfo2.hospitalcode=mz_pataccountdetail.hospitalcode AND mz_hosppatinfo2.hospcode=mz_pataccountdetail.hospcode)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "DELETE FROM mz_pricedetail WHERE EXISTS (SELECT * FROM mz_hosppatinfo2 WHERE mz_hosppatinfo2.payorder='" + payorder + "'";
                cmd.CommandText += " AND mz_hosppatinfo2.hospitalcode=mz_pricedetail.hospitalcode AND mz_hosppatinfo2.hospcode=mz_pricedetail.hospcode)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "DELETE FROM mz_hosppatinfo2 WHERE payorder='" + payorder + "'";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "DELETE FROM mz_checkpay WHERE payorder='" + payorder + "'";
                cmd.ExecuteNonQuery();
                #endregion
                myTrans.Commit();
            }
            catch (Exception e)
            {
                myTrans.Rollback();
                mess = if0.getXMLErroStrFromString("9000", "保存取消结算数据异常：" + e.Message.ToString());
                return mess;
            }
            finally
            {
                myTrans.Dispose();
            }
            #endregion
            mess = if0.getXMLStrFromString("取消结算成功");
            return mess;
        }
        catch (Exception e)
        {
            mess = if0.getXMLErroStrFromString("9000", "异常:" + e.Message.ToString());
            return mess;
        }
        finally
        {
            cmd.Dispose();
            if (Connection.State == ConnectionState.Open)
                Connection.Close();
        }
    }

    /// <summary>
    /// 非特殊慢病门诊申请支付


    /// </summary>
    /// <param name="hospitalcode"></param>
    /// <param name="payorder"></param>
    /// <param name="pay"></param>
    /// <param name="operer"></param>
    /// <returns></returns>
    public string settleMZSetPay(string hospitalcode, string payorder, string pay, string operer)
    {
        string mess = "";
        #region //参数有效性检查


        if (payorder == null || payorder.Length < 15)
        {
            mess = if0.getXMLErroStrFromString("2000", "补偿号不正确");
            return mess;
        }
        if (operer.Length == 0)
        {
            operer = UserID;
        }
        #endregion
        string ls_sql, ls_hospitalcode, ls_hospcode, ls_nopaydate, ls_human, ls_statisticyear;
        Decimal ld_nhpay, ld_grandprice;
        DataSet setDS = null;
        Int32 i;
        try
        {
            #region //是否可以支付的检查


            ls_sql = "SELECT chkpayid,prechkfee,accountstate,payid,subopr,convert(varchar(19),subtime,120) as paytime,lockstate,ifaccount FROM mz_checkpay";
            ls_sql += " WHERE payorder='" + payorder + "'";
            try
            {
                setDS = if0.getDataSet(ls_sql, "mzcheckpay", out mess);
                if (setDS.Tables["mzcheckpay"].Rows.Count == 0)
                {
                    mess = if0.getXMLErroStrFromString("2000", "补偿号" + payorder + "不存在！");
                    return mess;
                }
                else
                {
                    if (Convert.ToInt32(setDS.Tables["mzcheckpay"].Rows[0]["chkpayid"]) == 0)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "还没有成功结算，请结算后再支付！");
                        return mess;
                    }
                    if (Convert.ToDecimal(setDS.Tables["mzcheckpay"].Rows[0]["prechkfee"]) != Convert.ToDecimal(pay))
                    {
                        mess = if0.getXMLErroStrFromString("2000", "申请支付金额" + pay + "与农合系统中的补偿额不一致，请取消结算并重新结算再支付！");
                        return mess;
                    }
                    if (Convert.ToInt32(setDS.Tables["mzcheckpay"].Rows[0]["payid"]) == 1)
                    {
                        mess = if0.getXMLStrFromString(Convert.ToString(setDS.Tables["mzcheckpay"].Rows[0]["subopr"]) + "在" + Convert.ToString(setDS.Tables["mzcheckpay"].Rows[0]["paytime"]) + "已经支付！");
                        return mess;
                    }
                    if (Convert.ToInt32(setDS.Tables["mzcheckpay"].Rows[0]["lockstate"]) == 1)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "农合系统中该补偿资料是冻结状态，不允许支付！");
                        return mess;
                    }
                }
            }
            catch (Exception e)
            {
                mess = if0.getXMLErroStrFromString("9000", "查询门诊补偿资料：" + e.Message.ToString());
                return mess;
            }
            #endregion
            #region //扣家庭账户金额


            ls_sql = "SELECT hospitalcode,HospCode,convert(varchar(8),HospDate,112) as nopaydate,grandPrice,nhpay,human,statisticyear";
            ls_sql += " FROM mz_hosppatinfo2 WHERE payorder='" + payorder + "'";
            try
            {
                setDS = if0.getDataSet(setDS, ls_sql, "mzhosppatinfo2", out mess);
                for (i = 0; i < setDS.Tables["mzhosppatinfo2"].Rows.Count; i++)
                {
                    ls_hospitalcode = Convert.ToString(setDS.Tables["mzhosppatinfo2"].Rows[i]["hospitalcode"]);
                    ls_hospcode = Convert.ToString(setDS.Tables["mzhosppatinfo2"].Rows[i]["HospCode"]);
                    ls_nopaydate = Convert.ToString(setDS.Tables["mzhosppatinfo2"].Rows[i]["nopaydate"]);
                    ls_human = Convert.ToString(setDS.Tables["mzhosppatinfo2"].Rows[i]["human"]);
                    ls_statisticyear = Convert.ToString(setDS.Tables["mzhosppatinfo2"].Rows[i]["statisticyear"]);
                    ld_grandprice = Convert.ToDecimal(setDS.Tables["mzhosppatinfo2"].Rows[i]["grandPrice"]);
                    ld_nhpay = Convert.ToDecimal(setDS.Tables["mzhosppatinfo2"].Rows[i]["nhpay"]);
                    if (!if0.setPersonAccount(ls_hospitalcode, ls_hospcode, 2, 1, 0, ld_nhpay, ld_grandprice, ls_human, ls_statisticyear, ls_nopaydate, out mess))
                    {
                        mess = if0.getXMLErroStrFromString("9000", mess);
                        return mess;
                    }
                }
            }
            catch (Exception e)
            {
                mess = if0.getXMLErroStrFromString("9000", "查询门诊病人资料：" + e.Message.ToString());
                return mess;
            }
        }
            #endregion
        finally
        {
            setDS.Dispose();
        }
        SqlConnection Connection = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
        SqlCommand cmd = new SqlCommand();
        #region //设置支付标志
        try
        {
            Connection.Open();
            cmd.Connection = Connection;
            SqlTransaction myTrans;
            myTrans = Connection.BeginTransaction();
            try
            {
                cmd.Transaction = myTrans;
                cmd.CommandText = "UPDATE mz_checkpay SET payid=1,organpay='" + hospitalcode + "',paytime=getdate(),subsum=nhpay,subopr='" + operer + "',subtime=getdate()";
                cmd.CommandText += " WHERE payorder='" + payorder + "'";
                cmd.ExecuteNonQuery();
                myTrans.Commit();
            }
            catch (Exception e)
            {
                myTrans.Rollback();
                mess = if0.getXMLErroStrFromString("9000", "数据库保存支付数据异常：" + e.Message.ToString());
                return mess;
            }
            finally
            {
                myTrans.Dispose();
            }
            mess = if0.getXMLStrFromString("支付成功");
            return mess;
        }
        catch (Exception e)
        {
            mess = if0.getXMLErroStrFromString("9000", "异常:" + e.Message.ToString());
            return mess;
        }
        finally
        {
            cmd.Dispose();
            if (Connection.State == ConnectionState.Open)
                Connection.Close();
        }
        #endregion
    }

    /// <summary>
    /// 申请支付后是否可以取消结算


    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="mess"></param>
    /// <returns></returns>
    public string settleMZQueryCancelabled(string areacode, out string mess)
    {
        SqlConnection Connection = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
        SqlCommand cmd = new SqlCommand();
        SqlDataReader myReader;
        try
        {
            Connection.Open();
            cmd.Connection = Connection;
            try
            {
                //个人结算单打印后是否允许取消预审
                cmd.CommandText = "SELECT mzunchk FROM areasysparm WHERE areacode='" + areacode + "'";
                myReader = cmd.ExecuteReader();
                try
                {
                    if (!myReader.HasRows)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "农合系统中还没有设置系统参数");
                        return mess;
                    }
                    else
                    {
                        myReader.Read();
                        if (myReader.GetInt32(0) == 1) //允许
                        {
                            if0.getXMLStrFromStringData("<ROW>允许</ROW>", out mess);
                            return if0.getXMLStrFromString("允许");
                        }
                        else
                        {
                            if0.getXMLStrFromStringData("<ROW>不允许</ROW>", out mess);
                            return if0.getXMLStrFromString("不允许");
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
                mess = if0.getXMLErroStrFromString("9000", "验证补偿号异常：" + e.Message.ToString());
                return mess;
            }
            finally
            {
                cmd.Dispose();
            }
        }
        catch (Exception e)
        {
            mess = if0.getXMLErroStrFromString("9000", "异常:" + e.Message.ToString());
            return mess;
        }
        finally
        {
            cmd.Dispose();
            if (Connection.State == ConnectionState.Open)
                Connection.Close();
        }
    }

    /// <summary>
    /// 查询一个门诊病人的结算资料
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="payorder"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string settleMZQuery(string areacode, string payorder, out string data)
    {
        data = "";
        if (payorder == null || payorder.Length == 0)
        {
            return if0.getXMLErroStrFromString("2000", "补偿号(bch)不能为空值");
        }
        string mess = "";
        if (if0.getDBConnStr("areacode", areacode, out mess))
        {
            string ls_sql = null;
            DataSet myDs = null;
            ls_sql = "SELECT c.payorder as bch,c.grandPrice as je,c.assfee as ybje,c.familyfound as jtzhbc,c.nhpay as sjbj,c.mediprice as ypf,";
            ls_sql += "c.mediselffee as ypzf,c.materprice as clf,c.materselffee as clzf,c.serverprice as ylfwf,c.serverselffee as ylfwzf,";
            ls_sql += "case c.chkpayid when 0 then 0 when 8 then 1 else c.chkpayid+1 end as shzt,c.prechkfee as yse,c.chkinitsum as she,";
            ls_sql += "c.accountstate as jsbz,c.payid as dfbj,convert(varchar,c.paytime,120) as dfsj,c.lockstate as djbz,c.ifaccount as kjsbz";
            ls_sql += " FROM mz_checkpay c";
            ls_sql += " WHERE c.payorder='" + payorder + "'";
            //生成数据
            myDs = if0.getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                if0.getRequestXMLStringFromDS(myDs, "门诊病人补偿结算数据", out mess, out data);
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
    #endregion

    #region //门诊健康体检操作
    /// <summary>
    /// 下载指定体检日期的有效健康体检包


    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="tjrq"></param> //体检日期，用于限定有效期
    /// <param name="data"></param>
    /// <returns></returns>
    public string getTJHealthyPacket(string areacode, string hospitalcode, out string data)
    {
        string mess = "", ls_levelcode = "";
        data = "";
        if (if0.getDBConnStr("areacode", areacode, out mess))
        {

            string dbConnStr = null;
            if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
            {
                //得到医院级别
                SqlConnection Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
                try
                {
                    Connection.Open();
                    SqlCommand cmd = new SqlCommand("SELECT levelcode FROM organcode where orgcode=('" + hospitalcode + "') and areacode=('" + areacode + "')", Connection);
                    ls_levelcode = Convert.ToString(cmd.ExecuteScalar());
                    string ls_sql = null;
                    DataSet myDs = null;
                    ls_sql = "SELECT p.bcode as bcode,p.bname as bname";
                    switch (ls_levelcode)
                    {
                        case "1":
                            ls_sql += ",p.bprice as bprice";
                            break;
                        case "2":
                            ls_sql += ",p.bprice2 as bprice";
                            break;
                        case "3":
                            ls_sql += ",p.bprice3 as bprice";
                            break;
                        case "4":
                            ls_sql += ",p.bprice4 as bprice";
                            break;
                        case "5":
                            ls_sql += ",p.bprice5 as bprice";
                            break;
                        case "9":
                            ls_sql += ",p.bprice6 as bprice";
                            break;
                        default:
                            mess = if0.getXMLErroStrFromString("2010", "查找到无效的医院级别，不能获取农合药品目录！");
                            return mess;
                    }
                    ls_sql += ",p.remark as bremark,convert(varchar(10),p.startdate,120) as startrq,convert(varchar(10),p.enddate,120) as endrq,d.detailid as nhdm,d.remark as remark";
                    switch (ls_levelcode)
                    {
                        case "1":
                            ls_sql += ",r.price1 as price,r.rate1 as rate";
                            break;
                        case "2":
                            ls_sql += ",r.price2 as price,r.rate2 as rate";
                            break;
                        case "3":
                            ls_sql += ",r.price3 as price,r.rate3 as rate";
                            break;
                        case "4":
                            ls_sql += ",r.price4 as price,r.rate4 as rate";
                            break;
                        case "5":
                            ls_sql += ",r.price5 as price,r.rate5 as rate";
                            break;
                        case "9":
                            ls_sql += ",r.price6 as price,r.rate6 as rate";
                            break;
                        default:
                            mess = if0.getXMLErroStrFromString("2010", "查找到无效的医院级别，不能获取农合药品目录！");
                            return mess;
                    }
                    ls_sql += " FROM Tb_project_pack p,Tb_project_pack_detail d,Tb_project_packlimit l,mz_rcmssermediarea r";
                    ls_sql += " WHERE p.areacode='" + areacode + "' AND p.areacode=l.areacode AND p.bcode=l.bcode AND p.flag=1";
                    ls_sql += " AND l.orgcode='" + hospitalcode + "' AND l.state=2";
                    ls_sql += " AND p.areacode=d.areacode AND p.bcode=d.bcode AND d.areacode=r.areacode AND d.detailid=r.nhcode";
                    ls_sql += " ORDER BY p.bcode,d.detailid";
                    //生成数据
                    myDs = if0.getDataSet(ls_sql, out mess);
                    try
                    {
                        if (mess == "TRUE")
                        {
                            if0.getRequestXMLStringFromDS(myDs, "有效体检包内容", out mess, out data);
                        }
                        else
                        {
                            mess = if0.getXMLErroStrFromString("9000", mess);
                        }
                        myDs.Dispose();
                        return mess;
                    }
                    finally
                    {
                        myDs.Dispose();
                        cmd.Dispose();
                    }
                }
                catch (Exception e)
                {
                    mess = if0.getXMLErroStrFromString("9000", "查询医院级别异常：" + e.Message.ToString());
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
                mess = if0.getXMLErroStrFromString("5000", mess);
                return mess;
            }
        }
        else
        {
            mess = if0.getXMLErroStrFromString("9000", mess);
            return mess;
        }
    }

    /// <summary>
    /// 下载已审核未体检的申报花名册
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string getTJDeclareRoster(string areacode, string hospitalcode, int startrow, int endrow, out string data)
    {
        data = "";
        string mess = "";
        //参数有效性检验


        if (startrow < 0 || endrow < 0)
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行都不能小于0。不能下载！");
            return mess;
        }
        else if ((startrow == 0 && endrow != 0) || (startrow != 0 && endrow == 0))
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行必须同时为0，表示获取可下载目录条数");
            return mess;
        }
        else if (startrow > endrow)
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行大于结束行。不能下载！");
            return mess;
        }
        else if (endrow - startrow > 999)
        {
            mess = if0.getXMLErroStrFromString("2000", "调用请求超过1000行。不能下载！");
            return mess;
        }
        if (if0.getDBConnStr("areacode", areacode, out mess))
        {

            string ls_sql = null;
            DataSet myDs = null;
            ls_sql = "SELECT count(*) as sbcount FROM Tb_declare_roster WHERE orgcode='" + hospitalcode + "' AND state=20";
            myDs = if0.getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if ((startrow == endrow && startrow == 0))
                    {
                        if0.getRequestXMLStringFromDS(myDs, "得到已审核的申报数据总条数", out mess, out data);
                        return mess;
                    }
                    if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) == 0)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "没有已审核未体检的申报花名册数据可下载！");
                        return mess;
                    }
                    if (startrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行超过已审核的申报总条数" + myDs.Tables[0].Rows[0][0].ToString() + "。不能下载！");
                        return mess;
                    }
                    if (endrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        endrow = Convert.ToInt32(myDs.Tables[0].Rows[0][0]);
                    }
                }
                else
                {
                    mess = if0.getXMLErroStrFromString("5000", mess);
                    return mess;
                }
            }
            finally
            {
                myDs.Dispose();
            }

            //ls_sql = "SELECT TOP " + Convert.ToString(endrow - startrow + 1);
            //ls_sql += " a.sbid,a.lbid,a.lbmc,a.bcode,a.xm,a.xb,a.nl,a.chnd,a.zkh,a.zksbm,a.sfz,a.jtzz,a.lxdh,a.jhtjrq";
            //ls_sql += " FROM (SELECT TOP " + endrow.ToString() + " r.autoprekey as sbid,c.classid as lbid,c.classname as lbmc,r.bcode as bcode,r.patientname as xm,s.sexname as xb,r.patientage as nl";
            //ls_sql += ",r.statisticyear as chnd,r.patientcard as zkh,r.validno as zksbm,r.patientid as sfz,a.cityname as jtzz,r.patienttel as lxdh,convert(varchar(10),r.agreetime,120) as jhtjrq";
            //ls_sql += " FROM Tb_declare_roster r,Tb_healthy_class c,sexcode s,sysarea a";
            //ls_sql += " WHERE r.orgcode='" + hospitalcode + "' AND r.state=20 AND r.areacode=c.areacode AND r.classid=c.classid AND r.patientsex=s.sex AND r.areacode=a.areacode";
            //ls_sql += " AND r.areacode2=a.areacode2 ORDER BY r.autoprekey) a";
            //ls_sql += " ORDER BY sbid DESC";

            ls_sql = "SELECT";
            ls_sql += " a.sbid,a.lbid,a.lbmc,a.bcode,a.xm,a.xb,a.nl,a.chnd,a.zkh,a.zksbm,a.sfz,a.jtzz,a.lxdh,a.jhtjrq";
            ls_sql += " FROM (SELECT r.autoprekey as sbid,c.classid as lbid,c.classname as lbmc,r.bcode as bcode,r.patientname as xm,s.sexname as xb,r.patientage as nl";
            ls_sql += ",r.statisticyear as chnd,r.patientcard as zkh,r.validno as zksbm,r.patientid as sfz,a.cityname as jtzz,r.patienttel as lxdh,convert(varchar(10),r.agreetime,120) as jhtjrq";
            ls_sql += ",ROW_NUMBER() OVER (ORDER BY r.autoprekey) as rownum FROM Tb_declare_roster r,Tb_healthy_class c,sexcode s,sysarea a";
            ls_sql += " WHERE r.orgcode='" + hospitalcode + "' AND r.state=20 AND r.areacode=c.areacode AND r.classid=c.classid AND r.patientsex=s.sex AND r.areacode=a.areacode";
            ls_sql += " AND r.areacode2=a.areacode2) a";
            ls_sql += " WHERE a.rownum BETWEEN " + startrow.ToString() + " AND " + endrow.ToString();

            //生成数据
            myDs = if0.getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                if0.getRequestXMLStringFromDS(myDs, "未体检的申报花名册", out mess, out data);
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
    /// 验证体检申报ID是否有效，状态是否与当前补偿数据状态相符


    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="key"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="year"></param> 允许为空进行弱判断


    /// <param name="human"></param> 允许为空进行弱判断


    /// <param name="chkpayid"></param>
    /// <param name="mess"></param>
    /// <returns></returns>
    public bool checkTJAskedid(string areacode, string key, string hospitalcode, string year, string human, int chkpayid, out string mess)
    {
        mess = "";
        if (!if0.isNumber(key))
        {
            mess = "参数key必须为数字型";
            return false;
        }
        if (year.Length > 0 && !(if0.isNumber(year) && Convert.ToInt16(year) > 2008)) //参数year允许为空进行弱判断


        {
            mess = "参数year无效";
            return false;
        }
        if (human.Length != 0 && human.Length != 18) //参数human允许为空进行弱判断


        {
            mess = "参数human无效";
            return false;
        }
        if (if0.getDBConnStr("areacode", areacode, out mess))
        {

            SqlConnection sqlcon = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
            SqlCommand sqlcomm = new SqlCommand();
            SqlDataReader sqldr = null;
            Int32 mz_tj_passyear;


            try
            {
                sqlcomm.CommandText = "Select mz_tj_passyear From areasysparm Where areacode = " + areacode;
                sqlcomm.Connection = sqlcon;
                sqlcon.Open();
                sqldr = sqlcomm.ExecuteReader();
                if (!sqldr.HasRows)
                {
                    mess = "没有找到体检跨年度系统参数。";
                    return false;
                }
                sqldr.Read();
                mz_tj_passyear = sqldr.GetInt32(0);                
            }
            catch (Exception ee)
            {
                mess = "查询门诊体检跨年度系统参数时发生异常:"+ee.Message.ToString();
                return false;
            }
            finally
            {
                sqlcon.Dispose();
                sqlcomm.Dispose();
                sqldr.Dispose();
            }
            



            DataSet myDs = null;
            string ls_sql = "SELECT count(*) FROM Tb_declare_roster WHERE autoprekey=" + key.ToString();
            //ls_sql += " AND orgcode='" + hospitalcode + "' AND statisticyear='" + year + "' AND human='" + human + "'";
            ls_sql += " AND orgcode='" + hospitalcode + "'";
            
            if (mz_tj_passyear == 0)
            {
                if (year.Length > 0)
                {
                    ls_sql += " AND statisticyear='" + year + "'";
                }
            }
            if (human.Length > 0)
            {
                ls_sql += " AND human='" + human + "'";
            }
            if (chkpayid == 0) //还未任何审核过的，则看申报资料为已审核状态的是否存在
            {
                ls_sql += " AND state=20";
            }
            else //已走过审核流程的，则看申报资料为已完成体检状态的是否存在
            {
                ls_sql += " AND state=40";
            }
            myDs = if0.getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) == 0)
                    {
                        mess = "体检申报库中资料(申报ID号:" + key.ToString() + ")不存在!";
                        return false;
                    }
                    else
                    {
                        mess = "";
                        return true;
                    }
                }
                else
                {
                    //mess = mess;
                    return false;
                }
            }
            finally
            {
                myDs.Dispose();
            }
        }
        else
        {
            //mess = mess;
            return false;
        }
    }

    /// <summary>
    /// 验证门诊总费用与申报体检包的费用是否一致


    /// </summary>
    /// <param name="sConn"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="mzFee"></param>
    /// <param name="autoprekey"></param>
    /// <param name="mess"></param>
    /// <returns></returns>
    public bool checkTJPacketFee(SqlConnection sConn, string hospitalcode, decimal mzFee, string autoprekey, out string mess)
    {
        mess = "";
        string ls_levelcode;
        decimal ld_tjpackfee;
        bool lb_openconn = false;
        Int32 ll_cnt;
        SqlCommand cmd = new SqlCommand();
        SqlDataReader myReader;
        try
        {
            if (sConn.State == ConnectionState.Closed)
            {
                lb_openconn = true;
                sConn.Open();
            }
            cmd.Connection = sConn;
            #region 查询医疗机构级别
            try
            {
                cmd.CommandText = "SELECT levelcode FROM organcode WHERE orgcode='" + hospitalcode + "'";
                myReader = cmd.ExecuteReader();
                try
                {
                    if (!myReader.HasRows)
                    {
                        mess = "没有设置机构代码" + hospitalcode + "，不能继续！";
                        return false;
                    }
                    else
                    {
                        myReader.Read();
                        ls_levelcode = myReader.GetString(0);
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
                mess = "查询医院机构级别异常：" + e.Message.ToString();
                return false;
            }
            finally
            {
                cmd.Dispose();
            }
            #endregion
            #region 验证体检费用一致


            try
            {
                cmd.CommandText = "SELECT ";
                switch (ls_levelcode)
                {
                    case "1":
                        cmd.CommandText += "SUM(CASE WHEN n.price1 < 0.0 THEN 0.0 ELSE n.price1 END),SUM(CASE WHEN n.price1 < 0.0 THEN 1 ELSE 0 END)";
                        break;
                    case "2":
                        cmd.CommandText += "SUM(CASE WHEN n.price2 < 0.0 THEN 0.0 ELSE n.price2 END),SUM(CASE WHEN n.price2 < 0.0 THEN 1 ELSE 0 END)";
                        break;
                    case "3":
                        cmd.CommandText += "SUM(CASE WHEN n.price3 < 0.0 THEN 0.0 ELSE n.price3 END),SUM(CASE WHEN n.price3 < 0.0 THEN 1 ELSE 0 END)";
                        break;
                    case "4":
                        cmd.CommandText += "SUM(CASE WHEN n.price4 < 0.0 THEN 0.0 ELSE n.price4 END),SUM(CASE WHEN n.price4 < 0.0 THEN 1 ELSE 0 END)";
                        break;
                    case "5":
                        cmd.CommandText += "SUM(CASE WHEN n.price5 < 0.0 THEN 0.0 ELSE n.price5 END),SUM(CASE WHEN n.price5 < 0.0 THEN 1 ELSE 0 END)";
                        break;
                    case "9":
                        cmd.CommandText += "SUM(CASE WHEN n.price6 < 0.0 THEN 0.0 ELSE n.price6 END),SUM(CASE WHEN n.price6 < 0.0 THEN 1 ELSE 0 END)";
                        break;
                    default:
                        mess = "没有定义的机构级别代码(" + ls_levelcode + ")!";
                        return false;
                }
                cmd.CommandText += " FROM Tb_declare_roster r,Tb_project_pack_detail d,mz_rcmssermediarea n";
                cmd.CommandText += " WHERE r.autoprekey=" + autoprekey;
                cmd.CommandText += " AND r.areacode=d.areacode AND r.bcode=d.bcode AND r.areacode=n.areacode AND d.detailid=n.nhcode";
                myReader = cmd.ExecuteReader();
                try
                {
                    if (!myReader.HasRows)
                    {
                        mess = "农合系统中体检包内容不是有效的门诊农合目录，不能继续！";
                        return false;
                    }
                    else
                    {
                        myReader.Read();
                        ld_tjpackfee = myReader.GetDecimal(0);
                        ll_cnt = myReader.GetInt32(1);
                        if (ll_cnt == 0) //体检包中没有不限价的费用条目，则比较体检总费用


                        {
                            if (ld_tjpackfee != mzFee)
                            {
                                mess = "体检申报(申报ID" + autoprekey + ")资料中的体检包费用为" + ld_tjpackfee.ToString() + "，与本次体检总费用" + mzFee.ToString() + "不符!";
                                return false;
                            }
                        }
                        else
                        {
                            if (ld_tjpackfee < mzFee)
                            {
                                mess = "体检申报(申报ID" + autoprekey + ")资料中的体检包费用至少为" + ld_tjpackfee.ToString() + "，大于本次体检总费用!";
                                return false;
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
                mess = "查询医院机构级别异常：" + e.Message.ToString();
                return false;
            }
            finally
            {
                cmd.Dispose();
            }
            #endregion
            mess = "";
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
            if (lb_openconn && sConn.State == ConnectionState.Open)
                sConn.Close();
        }
    }
    #endregion

}
