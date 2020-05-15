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
using System.IO;

/// <summary>
/// ncmsifterrace 的摘要说明

/// </summary>
public class ncmsifterrace
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
    public ncmsifterrace()
	{
		//
		// TODO: 在此处添加构造函数逻辑
		//
    }
    
    public ncmsifterrace(string dbServer, string dbName, string userID, string passWord, string dbConnString, bool validUser, string areaCode, int interFacePower)
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
    ~ncmsifterrace()
    {
        if0.Dispose();
    }
    #endregion

    #region 基础方法
    #region 根据转诊申请单号获取本地农合的住院hospitalcode,hospid
    private bool procFunc_getHospInfo(string referral_no, out string hospitalcode, out decimal hospid,out string mess)
    {
        /*返回值
         * true  : 执行成功 如果hospitalcode="-1" And hospid = -1，则没有找到转诊单号所对应的住院信息，调用处需对hopsitalcode,hospid的值进行判断。 
         * false : 执行失败 mess中包含错误信息
        */
        string ls_sql;
        hospitalcode = "-1";
        hospid = -1;
        mess = "";
        SqlConnection sqlconn = new SqlConnection(DBConnStr);
        SqlCommand sqlcmd = new SqlCommand();
        SqlDataReader sqldr = null;
        try
        {
            sqlconn.Open();
            sqlcmd.Connection = sqlconn;
            ls_sql = "Select hospitalcode,hospid From hosppatinfo2 Where patsort = '"+referral_no+"' ";
            sqlcmd.CommandText = ls_sql;
            sqldr = sqlcmd.ExecuteReader();
            if (!sqldr.HasRows)
            {
                mess = "没有找到转诊申请单号为"+referral_no+"所对应的住院信息。";
                return true;
            }
            sqldr.Read();
            hospitalcode = sqldr.GetString(0);
            hospid = sqldr.GetDecimal(1);
            return true;
        }
        catch (Exception ee)
        {
            mess = "根据转诊申请单号查询住院信息时发生异常：" + ee.Message.ToString();
            return false;
        }
        finally
        {
            if (sqldr != null)
                sqldr.Dispose();
            sqlcmd.Dispose();
            if (sqlconn.State == ConnectionState.Open)
                sqlconn.Close();
            sqlconn.Dispose();
        }        
    }
    #endregion
    #endregion

    #region 各业务处理方法

    #region 参合验证
    public bool procFunc_F102010X(string In_gnbh, string In_jysr_XML, out string[] retstr)
    {
        retstr = new string[4];
        retstr[0] = "1";
        retstr[1] = "";
        retstr[2] = "";
        retstr[3] = "";

        string mess, ls_areacode,zyorg_code, admorg_code, medcard_id,idcard, p_sn, name;
        string ls_sql, ls_year,data,ls_admorg_name,ls_zryorg_name,ls_zyorg_level;
        DateTime ldt_date;
        DataSet myDs;

        data = "";
        XmlDocument xdoc = new XmlDocument();
        XmlNodeList xnlist = null;
        SqlConnection sqlconn= new SqlConnection(DBConnStr);
        SqlCommand sqlcmd = new SqlCommand();
        SqlDataReader sqldr = null;

        try
        {
            switch (In_gnbh.ToUpper())
            {
                #region 按医疗证+户编号/姓名
                case "F1020101":
                    #region 初始化变量
                    xdoc = if0.getXMLDocumentFromString(In_jysr_XML, out mess);
                    if (mess == "TRUE")
                    {
                        xnlist = xdoc.GetElementsByTagName("zyorg_code");
                        zyorg_code = xnlist[0].InnerText;
                        xnlist = xdoc.GetElementsByTagName("admorg_code");
                        admorg_code = xnlist[0].InnerText;
                        ls_areacode = admorg_code.Substring(0, 6);
                        xnlist = xdoc.GetElementsByTagName("medcard_id");
                        medcard_id = xnlist[0].InnerText;
                        if (medcard_id.Length == 0)
                        {
                            retstr[3] = "请填写医疗证号。";
                            return false;
                        }
                        xnlist = xdoc.GetElementsByTagName("p_sn");
                        p_sn = xnlist[0].InnerText;
                        xnlist = xdoc.GetElementsByTagName("name");
                        name = xnlist[0].InnerText;
                        if (p_sn.Length == 0 && name.Length == 0)
                        {
                            retstr[3] = "户编号和姓名不允许同时为空。";
                            return false;
                        }
                    }
                    else
                    {
                        retstr[3] = mess;
                        return false;
                    }

                    //get currentdate
                    if (if0.getCurrentDateTime(out ldt_date, out mess))
                    {
                        ls_year = string.Format("{0:yyyy}", ldt_date);
                    }
                    else
                    {
                        retstr[3] = mess;
                        return false;
                    }
                    #endregion

                    #region 查询机构信息


                    sqlconn.Open();
                    sqlcmd.Connection = sqlconn;
                    try
                    {
                        ls_sql = "Select orgname From organcode Where orgcode = '" + ls_areacode + ls_areacode + "'";
                        sqlcmd.CommandText = ls_sql;
                        ls_admorg_name = Convert.ToString(sqlcmd.ExecuteScalar());
                        if (ls_admorg_name == "" || ls_admorg_name.Length == 0)
                        {
                            retstr[3] = "未找到农合管理机构信息。";
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        retstr[2] = "查询农合管理机构名称时发生异常:" + e.Message.ToString();
                        return false;
                    }

                    try
                    {
                        ls_sql = "Select orgname,levelcode From organcode Where orgcode1 = '" + zyorg_code + "'";
                        sqlcmd.CommandText = ls_sql;
                        sqldr = sqlcmd.ExecuteReader();
                        if (!sqldr.HasRows)
                        {
                            retstr[3] = "未找到机构代码为" + zyorg_code + "的机构信息。";
                            return false;
                        }
                        sqldr.Read();
                        ls_zryorg_name = sqldr.GetString(0);
                        ls_zyorg_level = sqldr.GetString(1);
                    }
                    catch (Exception e)
                    {
                        retstr[2] = "查询医疗机构信息时发生异常:" + e.Message.ToString();
                        return false;
                    }
                    #endregion

                    #region 获取参合信息
                    ls_sql = "";
                    ls_sql = "Select b.human as person_id,'" + ls_areacode + '0' + "' as admorg_code, '" + ls_admorg_name + "' as admorg_name,'" + zyorg_code + "' as zyorg_code,'" + ls_zryorg_name + "' as zryorg_name,'" + ls_zyorg_level + "' as zyorg_level, ";
                    ls_sql += "a.bookcard as medcard_id,Right('0'+Convert(Varchar(2),b.memberno),2) as p_sn,b.patientid as idcard,b.name as name,b.sex as sex, ";
                    ls_sql += "b.birthday as birthdate,Null as age,b.phone as phone,b.mailingaddr as address,Null as town_code,Null as town_name,Null as village_code ,Null as village_name,Null as group_sn,'9' as join_attribute ";
                    ls_sql += "  From n_familydata a,n_individdata b Where a.familyno = b.familyno And a.statisticyear = b.statisticyear And a.statisticyear = '" + ls_year + "' ";
                    ls_sql += "   And a.bookcard = '" + medcard_id + "' And (memberno = '" + p_sn + "' Or b.name = '" + name + "') ";
                    myDs = if0.getDataSet(ls_sql, out mess);
                    if (mess == "TRUE")
                    {
                        if (myDs.Tables[0].Rows.Count == 0)
                        {
                            retstr[3] = "未找到该病人的参合信息。";
                            return false;
                        }
                        else if (myDs.Tables[0].Rows.Count > 1)
                        {
                            retstr[3] = "查询参合信息异常。";
                            return false;
                        }
                        else
                        {
                            if (if0.getRequestXMLStringFromDS(myDs, "参合信息", out mess, out data))
                            {
                                retstr[0] = "0";
                                retstr[2] = data;
                                return true;
                            }
                            else
                            {
                                retstr[2] = mess;
                                return false;
                            }
                        }
                    }
                    else
                    {
                        retstr[3] = mess;
                        return false;
                    }
                    break;
                    #endregion
                #endregion
                #region 按身份证+姓名/户编号
                case "F1020102":
                    #region 初始化变量
                    xdoc = if0.getXMLDocumentFromString(In_jysr_XML, out mess);
                    if (mess == "TRUE")
                    {
                        xnlist = xdoc.GetElementsByTagName("zyorg_code");
                        zyorg_code = xnlist[0].InnerText;
                        xnlist = xdoc.GetElementsByTagName("admorg_code");
                        admorg_code = xnlist[0].InnerText;
                        ls_areacode = admorg_code.Substring(0, 6);
                        xnlist = xdoc.GetElementsByTagName("idcard");
                        idcard = xnlist[0].InnerText;
                        if (idcard.Length == 0)
                        {
                            retstr[3] = "请填写身份证号。";
                            return false;
                        }
                        xnlist = xdoc.GetElementsByTagName("p_sn");
                        p_sn = xnlist[0].InnerText;
                        xnlist = xdoc.GetElementsByTagName("name");
                        name = xnlist[0].InnerText;
                        if (p_sn.Length == 0 && name.Length == 0)
                        {
                            retstr[3] = "户编号和姓名不允许同时为空。";
                            return false;
                        }
                    }
                    else
                    {
                        retstr[3] = mess;
                        return false;
                    }

                    //get currentdate
                    if (if0.getCurrentDateTime(out ldt_date, out mess))
                    {
                        ls_year = string.Format("{0:yyyy}", ldt_date);
                    }
                    else
                    {
                        retstr[3] = mess;
                        return false;
                    }
                    #endregion

                    #region 查询机构信息

                    sqlconn.Open();
                    sqlcmd.Connection = sqlconn;
                    try
                    {
                        ls_sql = "Select orgname From organcode Where orgcode = '" + ls_areacode + ls_areacode + "'";
                        sqlcmd.CommandText = ls_sql;
                        ls_admorg_name = Convert.ToString(sqlcmd.ExecuteScalar());
                        if (ls_admorg_name == "" || ls_admorg_name.Length == 0)
                        {
                            retstr[3] = "未找到农合管理机构信息。";
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        retstr[2] = "查询农合管理机构名称时发生异常:" + e.Message.ToString();
                        return false;
                    }

                    try
                    {
                        ls_sql = "Select orgname,levelcode From organcode Where orgcode1 = '" + zyorg_code + "'";
                        sqlcmd.CommandText = ls_sql;
                        sqldr = sqlcmd.ExecuteReader();
                        if (!sqldr.HasRows)
                        {
                            retstr[3] = "未找到机构代码为" + zyorg_code + "的机构信息。";
                            return false;
                        }
                        sqldr.Read();
                        ls_zryorg_name = sqldr.GetString(0);
                        ls_zyorg_level = sqldr.GetString(1);
                    }
                    catch (Exception e)
                    {
                        retstr[2] = "查询医疗机构信息时发生异常:" + e.Message.ToString();
                        return false;
                    }
                    #endregion

                    #region 获取参合信息
                    ls_sql = "";
                    ls_sql = "Select b.human as person_id,'" + ls_areacode + '0' + "' as admorg_code, '" + ls_admorg_name + "' as admorg_name,'" + zyorg_code + "' as zyorg_code,'" + ls_zryorg_name + "' as zryorg_name,'" + ls_zyorg_level + "' as zyorg_level, ";
                    ls_sql += "a.bookcard as medcard_id,Right('0'+Convert(Varchar(2),b.memberno),2) as p_sn,b.patientid as idcard,b.name as name,b.sex as sex, ";
                    ls_sql += "b.birthday as birthdate,Null as age,b.phone as phone,b.mailingaddr as address,Null as town_code,Null as town_name,Null as village_code ,Null as village_name,Null as group_sn,'9' as join_attribute ";
                    ls_sql += "  From n_familydata a,n_individdata b Where a.familyno = b.familyno And a.statisticyear = b.statisticyear And a.statisticyear = '" + ls_year + "' ";
                    ls_sql += "   And b.patientid = '" + idcard + "' And (memberno = '" + p_sn + "' Or b.name = '" + name + "') ";
                    myDs = if0.getDataSet(ls_sql, out mess);
                    if (mess == "TRUE")
                    {
                        if (myDs.Tables[0].Rows.Count == 0)
                        {
                            retstr[3] = "未找到该病人的参合信息。";
                            return false;
                        }
                        else if (myDs.Tables[0].Rows.Count > 1)
                        {
                            retstr[3] = "查询参合信息异常。";
                            return false;
                        }
                        else
                        {
                            if (if0.getRequestXMLStringFromDS(myDs, "参合信息", out mess, out data))
                            {
                                retstr[0] = "0";
                                retstr[2] = data;
                                return true;
                            }
                            else
                            {
                                retstr[2] = mess;
                                return false;
                            }
                        }
                    }
                    else
                    {
                        retstr[3] = mess;
                        return false;
                    }
                    break;
                    #endregion
                #endregion
                #region 按医疗证号返回一户的信息
                case "F1020103":
                    #region 初始化变量
                    xdoc = if0.getXMLDocumentFromString(In_jysr_XML, out mess);
                    if (mess == "TRUE")
                    {
                        xnlist = xdoc.GetElementsByTagName("zyorg_code");
                        zyorg_code = xnlist[0].InnerText;
                        xnlist = xdoc.GetElementsByTagName("admorg_code");
                        admorg_code = xnlist[0].InnerText;
                        ls_areacode = admorg_code.Substring(0, 6);
                        xnlist = xdoc.GetElementsByTagName("medcard_id");
                        medcard_id = xnlist[0].InnerText;
                        if (medcard_id.Length == 0)
                        {
                            retstr[3] = "请填写医疗证号。";
                            return false;
                        }                        
                    }
                    else
                    {
                        retstr[3] = mess;
                        return false;
                    }

                    //get currentdate
                    if (if0.getCurrentDateTime(out ldt_date, out mess))
                    {
                        ls_year = string.Format("{0:yyyy}", ldt_date);
                    }
                    else
                    {
                        retstr[3] = mess;
                        return false;
                    }
                    #endregion

                    #region 查询机构信息


                    sqlconn.Open();
                    sqlcmd.Connection = sqlconn;
                    try
                    {
                        ls_sql = "Select orgname From organcode Where orgcode = '" + ls_areacode + ls_areacode + "'";
                        sqlcmd.CommandText = ls_sql;
                        ls_admorg_name = Convert.ToString(sqlcmd.ExecuteScalar());
                        if (ls_admorg_name == "" || ls_admorg_name.Length == 0)
                        {
                            retstr[3] = "未找到农合管理机构信息。";
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        retstr[2] = "查询农合管理机构名称时发生异常:" + e.Message.ToString();
                        return false;
                    }

                    try
                    {
                        ls_sql = "Select orgname,levelcode From organcode Where orgcode1 = '" + zyorg_code + "'";
                        sqlcmd.CommandText = ls_sql;
                        sqldr = sqlcmd.ExecuteReader();
                        if (!sqldr.HasRows)
                        {
                            retstr[3] = "未找到机构代码为" + zyorg_code + "的机构信息。";
                            return false;
                        }
                        sqldr.Read();
                        ls_zryorg_name = sqldr.GetString(0);
                        ls_zyorg_level = sqldr.GetString(1);
                    }
                    catch (Exception e)
                    {
                        retstr[2] = "查询医疗机构信息时发生异常:" + e.Message.ToString();
                        return false;
                    }
                    #endregion

                    #region 获取参合信息
                    ls_sql = "";
                    ls_sql = "Select b.human as person_id,'" + ls_areacode + '0' + "' as admorg_code, '" + ls_admorg_name + "' as admorg_name,'" + zyorg_code + "' as zyorg_code,'" + ls_zryorg_name + "' as zryorg_name,'" + ls_zyorg_level + "' as zyorg_level, ";
                    ls_sql += "a.bookcard as medcard_id,Right('0'+Convert(Varchar(2),b.memberno),2) as p_sn,b.patientid as idcard,b.name as name,b.sex as sex, ";
                    ls_sql += "b.birthday as birthdate,Null as age,b.phone as phone,b.mailingaddr as address,Null as town_code,Null as town_name,Null as village_code ,Null as village_name,Null as group_sn,'9' as join_attribute ";
                    ls_sql += "  From n_familydata a,n_individdata b Where a.familyno = b.familyno And a.statisticyear = b.statisticyear And a.statisticyear = '" + ls_year + "' ";
                    ls_sql += "   And a.bookcard = '" + medcard_id + "' ";
                    myDs = if0.getDataSet(ls_sql, out mess);
                    if (mess == "TRUE")
                    {
                        if (myDs.Tables[0].Rows.Count == 0)
                        {
                            retstr[3] = "未找到该病人的参合信息。";
                            return false;
                        }                       
                        else
                        {
                            if (if0.getRequestXMLStringFromDS(myDs, "参合信息", out mess, out data))
                            {
                                retstr[0] = "0";
                                retstr[2] = data;
                                return true;
                            }
                            else
                            {
                                retstr[2] = mess;
                                return false;
                            }
                        }
                    }
                    else
                    {
                        retstr[3] = mess;
                        return false;
                    }
                    break;
                    #endregion
                #endregion
            }
        }
        finally
        {
            sqlcmd.Dispose();
            if (!sqldr.IsClosed)
                sqldr.Close();
            sqldr.Dispose();
            if (sqlconn.State == ConnectionState.Open)
            {
                sqlconn.Close();
                sqlconn.Dispose();
            }
        }        
        return true;
    }
    #endregion

    #region 转诊申请
    public bool procFunc_F1020120(string In_jysr_XML, out string[] retstr)
    {
        retstr = new string[4];
        retstr[0] = "1";
        retstr[1] = "";
        retstr[2] = "";
        retstr[3] = "";

        string mess,ls_sql;
        string referral_no,PERSON_ID,medcard_id,p_sn,name,disease_code;
        string apply_note, admorg_code, admorg_name, from_org, hospitalcode2, From_org_name, zyorg_code, hospitalcode, zyorg_name, zyorg_level, bcxe;
        string audit_state,audit_note,audit_person,send_com,opr_code,SEX;
        string IDCARD,ADDRESS,JOIN_ATTRIBUTE,TOWN_CODE,TOWN_NAME,VILLAGE_CODE,VILLAGE_NAME,GROUP_SN,GROUP_NAME;   
        string apply_date,zzyxq,audit_date,send_date,BIRTHDATE;
        int ll_count;

        SqlConnection sqlconn = new SqlConnection(DBConnStr);
        SqlCommand sqlcmd = new SqlCommand();
        SqlTransaction mytrans = null;


        try
        {
            #region 各元素有效性判断
            XmlDocument xmldoc = if0.getXMLDocumentFromString(In_jysr_XML, out mess);
            if (mess != "TRUE")
            {
                retstr[3] = mess;
                return false;
            }
            XmlNodeList nlist;
            nlist = xmldoc.GetElementsByTagName("referral_no");
            referral_no = nlist[0].InnerText;
            if (referral_no.Length == 0)
            {
                retstr[3] = "转诊申请单号不允许为空。";
                return false;
            }

            nlist = xmldoc.GetElementsByTagName("person_id");
            PERSON_ID = nlist[0].InnerText;
            if (PERSON_ID.Length == 0)
            {
                retstr[3] = "个人唯一流水号不允许为空。";
                return false;
            }

            nlist = xmldoc.GetElementsByTagName("medcard_id");
            medcard_id = nlist[0].InnerText;
            if (medcard_id.Length == 0)
            {
                retstr[3] = "医疗证号不允许为空。";
                return false;
            }

            nlist = xmldoc.GetElementsByTagName("p_sn");
            p_sn = nlist[0].InnerText;
            if (p_sn.Length == 0)
            {
                retstr[3] = "户编号不允许为空。";
                return false;
            }

            nlist = xmldoc.GetElementsByTagName("name");
            name = nlist[0].InnerText;
            if (name.Length == 0)
            {
                retstr[3] = "患者姓名不允许为空。";
                return false;
            }

            nlist = xmldoc.GetElementsByTagName("disease_code");
            disease_code = nlist[0].InnerText;

            nlist = xmldoc.GetElementsByTagName("apply_date");
            apply_date = nlist[0].InnerText;
            if (apply_date.Length == 0)
            {
                retstr[3] = "申请日期不允许为空。";
                return false;
            }
            else
            {
                if (!if0.isDate(apply_date))
                {
                    retstr[3] = "无效的申请日期。";
                    return false;
                }
            }

            nlist = xmldoc.GetElementsByTagName("apply_note");
            apply_note = nlist[0].InnerText;

            nlist = xmldoc.GetElementsByTagName("admorg_code");
            admorg_code = nlist[0].InnerText;
            if (admorg_code.Length == 0)
            {
                retstr[3] = "农合管理机构代码不允许为空。";
                return false;
            }

            nlist = xmldoc.GetElementsByTagName("admorg_name");
            admorg_name = nlist[0].InnerText;

            nlist = xmldoc.GetElementsByTagName("from_org");
            from_org = nlist[0].InnerText;
            if (from_org.Length == 0)
            {
                hospitalcode2 = "";
            }
            else
            {
                try
                {
                    sqlconn.Open();
                    ls_sql = "Select orgcode From organcode Where orgcode1 ='" + from_org + "' And areacode ='"+AreaCode+"'";
                    sqlcmd.Connection = sqlconn;
                    sqlcmd.CommandText = ls_sql;
                    hospitalcode2 = Convert.ToString(sqlcmd.ExecuteScalar());
                    if (hospitalcode2.Length == 0 || hospitalcode2 == "")
                    {
                        retstr[3] = "转出医院代码'" + from_org + "'在本地农合系统中没有找到相应的机构信息";
                        return false;
                    }
                }
                catch (Exception e)
                {
                    retstr[3] = "查询转出医院代码在本地农合系统中对应的机构代码时发生异常:" + e.Message.ToString();
                    return false;
                }
            }

            nlist = xmldoc.GetElementsByTagName("from_org_name");
            From_org_name = nlist[0].InnerText;

            nlist = xmldoc.GetElementsByTagName("zyorg_code");
            zyorg_code = nlist[0].InnerText;
            if (zyorg_code.Length == 0)
            {
                hospitalcode = "";
            }
            else
            {
                try
                {
                    if (sqlconn.State == ConnectionState.Closed)
                        sqlconn.Open();
                    ls_sql = "Select orgcode From organcode Where orgcode1 ='" + zyorg_code + "' And areacode ='" + AreaCode + "'";
                    sqlcmd.Connection = sqlconn;
                    sqlcmd.CommandText = ls_sql;
                    hospitalcode = Convert.ToString(sqlcmd.ExecuteScalar());
                    if (hospitalcode.Length == 0 || hospitalcode == "")
                    {
                        retstr[3] = "转入医院代码'"+zyorg_code+"'在本地农合系统中没有找到相应的机构信息";
                        return false;
                    }
                }
                catch (Exception e)
                {
                    retstr[3] = "查询转入医院代码在本地农合系统中对应的机构代码时发生异常:"+e.Message.ToString();
                    return false;
                }
            }

            nlist = xmldoc.GetElementsByTagName("zyorg_name");
            zyorg_name = nlist[0].InnerText;

            nlist = xmldoc.GetElementsByTagName("zyorg_level");
            zyorg_level = nlist[0].InnerText;

            nlist = xmldoc.GetElementsByTagName("bcxe");
            bcxe = nlist[0].InnerText;

            nlist = xmldoc.GetElementsByTagName("zzyxq");
            zzyxq = nlist[0].InnerText;
            if (zzyxq.Length > 0)        
            {
                if (!if0.isDate(zzyxq))
                {
                    retstr[3] = "无效的转诊日期。";
                    return false;
                }
            }

            nlist = xmldoc.GetElementsByTagName("audit_state");
            audit_state = nlist[0].InnerText;
            if (audit_state.Length == 0)
            {
                retstr[3] = "审核状态不允许为空。";
                return false;
            }


            nlist = xmldoc.GetElementsByTagName("audit_date");
            audit_date = nlist[0].InnerText;
            if (audit_date.Length > 0)
            {
                if (!if0.isDate(audit_date))
                {
                    retstr[3] = "无效的转诊日期。";
                    return false;
                }
            }

            nlist = xmldoc.GetElementsByTagName("audit_note");
            audit_note = nlist[0].InnerText;

            nlist = xmldoc.GetElementsByTagName("audit_person");
            audit_person = nlist[0].InnerText;

            nlist = xmldoc.GetElementsByTagName("send_date");
            send_date = nlist[0].InnerText;
            if (send_date.Length == 0)
            {
                retstr[3] = "报送日期不允许为空。";
                return false;
            }
            else
            {
                if (!if0.isDate(send_date))
                {
                    retstr[3] = "无效的报送日期。";
                    return false;
                }
            }

            nlist = xmldoc.GetElementsByTagName("send_com");
            send_com = nlist[0].InnerText;
            if (send_com.Length == 0)
            {
                retstr[3] = "报送单位不允许为空。";
                return false;
            }


            nlist = xmldoc.GetElementsByTagName("opr_code");
            opr_code = nlist[0].InnerText;
            if (opr_code.Length == 0)
            {
                retstr[3] = "报送人员不允许为空。";
                return false;
            }

            nlist = xmldoc.GetElementsByTagName("sex");
            SEX = nlist[0].InnerText;

            nlist = xmldoc.GetElementsByTagName("birthdate");
            BIRTHDATE = nlist[0].InnerText;
            if (BIRTHDATE.Length > 0)
            {
                if (!if0.isDate(BIRTHDATE))
                {
                    retstr[3] = "无效的出生日期。";
                    return false;
                }
            }

            nlist = xmldoc.GetElementsByTagName("idcard");
            IDCARD = nlist[0].InnerText;

            nlist = xmldoc.GetElementsByTagName("address");
            ADDRESS = nlist[0].InnerText;

            nlist = xmldoc.GetElementsByTagName("join_attribute");
            JOIN_ATTRIBUTE = nlist[0].InnerText;

            nlist = xmldoc.GetElementsByTagName("town_code");
            TOWN_CODE = nlist[0].InnerText;

            nlist = xmldoc.GetElementsByTagName("town_name");
            TOWN_NAME = nlist[0].InnerText;

            nlist = xmldoc.GetElementsByTagName("village_code");
            VILLAGE_CODE = nlist[0].InnerText;

            nlist = xmldoc.GetElementsByTagName("village_name");
            VILLAGE_NAME = nlist[0].InnerText;

            nlist = xmldoc.GetElementsByTagName("group_sn");
            GROUP_SN = nlist[0].InnerText;

            nlist = xmldoc.GetElementsByTagName("group_name");
            GROUP_NAME = nlist[0].InnerText;
            #endregion

            #region 保存转诊信息
        
            #region 判断转诊单是否已经下发过了
            try
            {
                ls_sql = "Select Count(*) From hosppatinfo2_apply Where referral_no = '" + referral_no + "'";
                if (sqlconn.State == ConnectionState.Closed)
                    sqlconn.Open();
                sqlcmd.Connection = sqlconn;
                sqlcmd.CommandText = ls_sql;
                ll_count = Convert.ToInt32(sqlcmd.ExecuteScalar());
                if (ll_count > 0)
                {
                    retstr[3] = "农合系统中已有转诊单号为'"+referral_no+"'的转诊信息，请先取消该转诊申请。";
                    return false;
                }
            }
            catch (Exception e)
            {
                retstr[3] = "查询农合系统转诊申请信息失败:"+e.Message.ToString();
                return false;
            }
            #endregion

            #region 保存转诊信息
            try
            {
                ls_sql = "Insert Into hosppatinfo2_apply ";
                ls_sql += "(referral_no,PERSON_ID,medcard_id,p_sn,name,disease_code,apply_date,apply_note, ";
                ls_sql += "admorg_code,admorg_name,from_org,hospitalcode2,From_org_name,zyorg_code,hospitalcode,zyorg_name,zyorg_level, ";
                ls_sql += "bcxe,zzyxq,audit_state,audit_date,audit_note,audit_person,send_date, ";
                ls_sql += "send_com,opr_code,sex,BIRTHDATE,IDCARD,ADDRESS,JOIN_ATTRIBUTE, ";
                ls_sql += "TOWN_CODE,TOWN_NAME,VILLAGE_CODE,VILLAGE_NAME,GROUP_SN,GROUP_NAME ) ";
                ls_sql += "Values('" + referral_no + "','" + PERSON_ID + "','" + medcard_id + "','" + p_sn + "','" + name + "','" + disease_code + "','" + apply_date + "','" + apply_note + "', ";
                ls_sql += "'" + admorg_code + "','" + admorg_name + "','" + from_org + "','" + hospitalcode2 + "','" + From_org_name + "','" + zyorg_code + "','" + hospitalcode + "','" + zyorg_name + "','" + zyorg_level + "', ";
                ls_sql += "'" + bcxe + "','" + zzyxq + "','" + audit_state + "','" + audit_date + "','" + audit_note + "','" + audit_person + "','" + send_date + "', ";
                ls_sql += "'" + send_com + "','" + opr_code + "','" + SEX + "','" + BIRTHDATE + "','" + IDCARD + "','" + ADDRESS + "','" + JOIN_ATTRIBUTE + "', ";
                ls_sql += "'" + TOWN_CODE + "','" + TOWN_NAME + "','" + VILLAGE_CODE + "','" + VILLAGE_NAME + "','" + GROUP_SN + "','" + GROUP_NAME + "' )";

                mytrans = sqlconn.BeginTransaction();
                sqlcmd.Transaction = mytrans;
                sqlcmd.CommandText = ls_sql;                
                sqlcmd.ExecuteNonQuery();
                mytrans.Commit();
                retstr[0] = "0";
            }
            catch (Exception e)
            {
                mytrans.Rollback();
                retstr[3] = "保存转诊申请信息时发生异常：" + e.Message.ToString();
            }
            #endregion
        }        
        
        #endregion
        finally
        {
            if (mytrans != null)
                mytrans.Dispose();
            sqlcmd.Dispose();
            if (sqlconn.State == ConnectionState.Open)
                sqlconn.Close();
            sqlconn.Dispose();
        }

        return true;
    }
    #endregion

    #region 取消转诊申请
    public bool procFunc_F1020122(string In_jysr_XML, out string[] retstr)
    {
        retstr = new string[4];
        retstr[0] = "1";
        retstr[1] = "";
        retstr[2] = "";
        retstr[3] = "";

        string mess,referral_no,ls_sql,hospitalcode = "";
        decimal hospid = 0;
        int li_count;

        #region 数据有效性验证
        XmlDocument xmldoc = if0.getXMLDocumentFromString(In_jysr_XML, out mess);
        if (mess != "TRUE")
        {
            retstr[3] = mess;
            return false;
        }
        XmlNodeList xlist = xmldoc.GetElementsByTagName("referral_no");
        referral_no = xlist[0].InnerText;
        if (referral_no.Length == 0)
        {
            retstr[3] = "转诊申请单号不允许为空。";
            return false;
        }
        #endregion

        #region 取消转诊信息
        SqlConnection sqlconn = new SqlConnection(DBConnStr);
        SqlCommand sqlcmd = new SqlCommand();
        SqlTransaction mytrans = null;
        sqlconn.Open();
        sqlcmd.Connection = sqlconn;
        try
        {
            #region 转诊单是否存在
            try
            {
                ls_sql = "Select Count(*) From hosppatinfo2_apply Where referral_no ='" + referral_no + "'";                
                sqlcmd.CommandText = ls_sql;
                li_count = Convert.ToInt32(sqlcmd.ExecuteScalar());
                if (li_count == 0)
                {
                    retstr[3] = "没有找到转诊申请单号为'" +referral_no+ "'的转诊信息，无法取消该转诊单。";
                    return false;
                }
            }
            catch (Exception e)
            {
                retstr[3] = "查询转诊信息时发生异常:" + e.Message.ToString();
                return false;
            }
            #endregion

            #region 转诊单所对应的住院信息是否存在
            if (procFunc_getHospInfo(referral_no,out hospitalcode,out hospid,out mess))
            {
                if (!(hospitalcode == "-1" && hospid == -1))
                {
                    retstr[3] = "农合系统中已有该转诊单所对应的住院信息，无法取消该转诊申请单。";
                    return false;
                }
            }
            else
            {
                retstr[3] = mess;
                return false;
            }
            #endregion

            #region 删除转诊单
            try
            {
                ls_sql = "Delete From hosppatinfo2_apply Where referral_no ='" + referral_no + "'";

                mytrans = sqlconn.BeginTransaction();
                sqlcmd.CommandText = ls_sql;
                sqlcmd.Transaction = mytrans;
                sqlcmd.ExecuteNonQuery();
                mytrans.Commit();
                retstr[0] = "0";
            }
            catch (Exception e)
            {
                mytrans.Rollback();
                retstr[3] = "取消转诊时发生异常:" + e.Message.ToString();
                return false;
            }
            #endregion

        }
        finally
        {
            if (mytrans != null)
                mytrans.Dispose();
            sqlcmd.Dispose();
            if (sqlconn.State == ConnectionState.Open)
                sqlconn.Close();
            sqlconn.Dispose();
        }
        #endregion

        return true;
    }
    #endregion

    #region 获取转诊信息
    public bool procFunc_F1020131(string In_jysr_XML, out string[] retstr)
    {
        retstr = new string[4];
        retstr[0] = "1";
        retstr[1] = "";
        retstr[2] = "";
        retstr[3] = "";

        string mess = "",zyorg_code,admorg_code,idcard,name,medcard_id,p_sn,referral_no,ls_sql,retdata;

        #region 数据始初化及有效性检验
        XmlDocument xdoc = if0.getXMLDocumentFromString(In_jysr_XML,out mess);
        if (mess != "TRUE")
        {
            retstr[3] = mess;
            return false;
        }
        
        XmlNodeList xnlist = xdoc.GetElementsByTagName("zyorg_code");
        zyorg_code = xnlist[0].InnerText;
        if (zyorg_code.Length == 0)
        {
            retstr[3] = "就医机构代码不允许为空。";
            return false;
        }

        xnlist = xdoc.GetElementsByTagName("admorg_code");
        admorg_code = xnlist[0].InnerText;
        if (admorg_code.Length == 0)
        {
            retstr[3] = "农合管理机构代码不允许为空。";
            return false;
        }

        xnlist = xdoc.GetElementsByTagName("idcard");
        idcard = xnlist[0].InnerText;
        xnlist = xdoc.GetElementsByTagName("name");
        name = xnlist[0].InnerText;
        xnlist = xdoc.GetElementsByTagName("medcard_id");
        medcard_id = xnlist[0].InnerText;
        xnlist = xdoc.GetElementsByTagName("p_sn");
        p_sn = xnlist[0].InnerText;
        xnlist = xdoc.GetElementsByTagName("referral_no");
        referral_no = xnlist[0].InnerText;
        #endregion

        #region 转诊单查询
        SqlConnection sqlconn = new SqlConnection(DBConnStr);
        SqlCommand sqlcmd = new SqlCommand();
        DataSet myDS =  null;
        try
        {
            ls_sql = "Select referral_no as referral_no,person_id as person_id,name as name,medcard_id as medcard_id,p_sn as p_sn, ";
            ls_sql += "idcard as idcard,sex as sex,birthdate as birthdate,null as age,null as phone, ";
            ls_sql += "address as address,town_code as town_code,town_name as town_name,village_code as village_code,village_name as village_name, ";
            ls_sql += "group_sn as group_sn,join_attribute as join_attribute,disease_code as disease_code,apply_date as apply_date,apply_note as apply_note, ";
            ls_sql += "admorg_code as admorg_code,admorg_name as admorg_name,from_org as from_org,from_org_name as  from_org_name,zyorg_code as zyorg_code, ";
            ls_sql += "zyorg_name as zyorg_name,zyorg_level as zyorg_level,bcxe as bcxe,zzyxq as zzyxq,audit_state as audit_state, ";
            ls_sql += "audit_date as audit_date,audit_note as audit_note,audit_person as audit_person ";
            ls_sql += "From hosppatinfo2_apply ";
            ls_sql += "Where zyorg_code = '" + zyorg_code + "' And admorg_code = '"+admorg_code+"' ";
            if (idcard.Length > 0)
                ls_sql += " And idcard = '" + idcard + "' ";
            if (name.Length > 0)
                ls_sql += " And name ='" + name + "' ";
            if (medcard_id.Length > 0)
                ls_sql += " And medcard_id ='" + medcard_id + "' ";
            if (p_sn.Length > 0)
                ls_sql += " And p_sn = '" + p_sn + "' ";
            if (referral_no.Length > 0)
                ls_sql += " And referral_no = '" + referral_no + "' ";
            myDS = if0.getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                if (myDS.Tables[0].Rows.Count == 0)
                {
                    retstr[3] = "在医疗机构编码为"+zyorg_code+"中，没有找到指定的转诊申请信息。";
                    return false;
                }
                else
                {
                    if (if0.getRequestXMLStringFromDS(myDS, "转诊申请", out mess, out retdata))
                    {
                        retstr[0] = "0";
                        retstr[2] = retdata;
                        return true;
                    }
                    else
                    {
                        retstr[2] = mess;
                        return false;
                    }
                }
            }
            else
            {
                retstr[3] = mess;
                return false;
            }
        }
        catch (Exception e)
        {
            retstr[3] = "查询转诊信息时发生异常：" + e.Message.ToString();
            return false;
        }
        finally
        {
            if (myDS != null)
                myDS.Dispose();
            sqlcmd.Dispose();
            if (sqlconn.State == ConnectionState.Open)
                sqlconn.Close();
            sqlconn.Dispose();
        }
        #endregion
        return true;
    }
    #endregion

    #region 获取住院信息
    public bool procFunc_F1020151(string In_jysr_XML, out string[] retstr)
    {

        retstr = new string[4];        
        retstr[0] = "1";
        retstr[1] = "";
        retstr[2] = "";
        retstr[3] = "";

        string mess, ls_sql, referral_no, zyorg_code, his_link_code, hospitalcode, hospitalcode1,patientid, patientid1, medinumb, medinumb1;
        string patientname,sex,serial,hospdate,hospresu, outdate, nopaydate, chkpayid, requoffice1, requoffice2;
        decimal hospid,hospid1;
        string  zyitem_link_code = "",nhcode, execoffice, requoffice, doctname, opername, marktype, mediname, ddate, date2, date3, num1, num2;
        string user_num, unitprice, price, operid1;
        string fee6, fee10, fee1, fee2, fee12, fee7, fee11, fee8, fee13;
        int li_count, i, detailid;

        DataSet myDS = new DataSet();
        StringReader mySR = new StringReader(In_jysr_XML);
        SqlConnection sqlconn = new SqlConnection(DBConnStr);
        SqlCommand sqlcmd = new SqlCommand();
        SqlTransaction myTrans = null;

        try
        {

            #region 加载数据及初始化数据库事务
            try
            {
                myDS.ReadXml(mySR);

            }
            catch (Exception ee)
            {
                retstr[3] = "加载住院信息时发生异常：" + ee.Message.ToString();
                return false;
            }

            sqlconn.Open();
            myTrans = sqlconn.BeginTransaction();
            sqlcmd.Connection = sqlconn;            
            sqlcmd.Transaction = myTrans;
            #endregion

            #region 数据有效性检测
            try
            {
                #region 查询本地农合中是否有该转诊申请
                try
                {
                    referral_no = Convert.ToString(myDS.Tables[0].Rows[0]["referral_no"]);
                    if (referral_no.Length == 0 || referral_no == "")
                    {
                        retstr[3] = "转诊单号不允许为空。";
                        return false;
                    }
                    ls_sql = "Select Count(*) From hosppatinfo2_apply Where referral_no ='" + referral_no + "'";
                    if (sqlconn.State != ConnectionState.Open)
                        sqlconn.Open();
                    sqlcmd.CommandText = ls_sql;
                    li_count = Convert.ToInt32(sqlcmd.ExecuteScalar());
                    if (li_count == 0)
                    {
                        retstr[3] = "农合系统中还没有转诊申请单号为" + referral_no + "的转诊申请信息，无法下载。";
                        return false;
                    }
                }
                catch (Exception ee)
                {
                    retstr[3] = "查询转诊申请信息时发生异常：" + ee.Message.ToString();
                    return false;
                }

                #endregion
                #region 查询省平台医院代码在本地农合中的对应的机构代码
                zyorg_code = Convert.ToString(myDS.Tables[0].Rows[0]["zyorg_code"]);
                if (zyorg_code.Length == 0 || zyorg_code == "")
                {
                    retstr[3] = "在诊医疗机构代码不允许为空。";
                    return false;
                }
                try
                {
                    ls_sql = "Select orgcode From organcode Where orgcode1 = '" + zyorg_code + "'";
                    if (sqlconn.State != ConnectionState.Open)                        
                        sqlconn.Open();
                    sqlcmd.CommandText = ls_sql;
                    hospitalcode = Convert.ToString(sqlcmd.ExecuteScalar());
                    if (hospitalcode.Length == 0 || hospitalcode == "")
                    {
                        retstr[3] = "在农合系统中没有找到医院代码为" + zyorg_code + "的信息。";
                        return false;
                    }
                }
                catch (Exception ee)
                {
                    retstr[3] = "查询本地医疗机构信息时发生异常：" + ee.Message.ToString();
                    return false;
                }
                #endregion
                #region 住院号hospid转换
                his_link_code = Convert.ToString(myDS.Tables[0].Rows[0]["his_link_code"]);
                if (his_link_code.Length == 0 || his_link_code == "")
                {
                    retstr[3] = "住院就诊流水号不允许为空。";
                    return false;
                }
                try
                {
                    hospid = Convert.ToDecimal(his_link_code);
                }
                catch
                {
                    try
                    {
                        ls_sql = "Select IsNull(Max(IsNull(hospid,0)),0) From hosppatinfo2 Where hospitalcode = '" + hospitalcode + "'";
                        if (sqlconn.State != ConnectionState.Open)
                            sqlconn.Open();
                        sqlcmd.CommandText = ls_sql;
                        hospid = Convert.ToDecimal(sqlcmd.ExecuteScalar());
                        hospid++;
                    }
                    catch (Exception ee)
                    {
                        retstr[3] = "生成住院号时发生异常：" + ee.Message.ToString();
                        return false;
                    }
                }
                #endregion
                #region 本地农合是否已有该住院信息
                if (procFunc_getHospInfo(referral_no, out hospitalcode1, out hospid1, out mess))
                {
                    if (!(hospitalcode1 == "-1" && hospid1 == -1))
                    {
                        retstr[3] = "农合系统中已有转诊申请单号为" + referral_no + "的住院信息，无须下载。";
                        return false;
                    }
                }
                else
                {
                    retstr[3] = mess;
                    return false;
                }              
                #endregion
            }
            catch(Exception ee)
            {
                retstr[3] = ee.Message.ToString();
                return false;
            }
            #endregion

            #region 保存住院数据
            #region 写hosppatinfo2
            #region 数据初始化及有效性检测
            patientid = Convert.ToString(myDS.Tables[2].Rows[0]["p_sn"]);
            if (patientid.Length == 0 || patientid == "")
            {
                retstr[3] = "户内编号不允许为空。";
                return false;
            }

            patientid1 = Convert.ToString(myDS.Tables[2].Rows[0]["idcard"]);              
            
            medinumb1 = Convert.ToString(myDS.Tables[2].Rows[0]["medcard_id"]);
            if (medinumb1.Length == 0 || medinumb1 == "")
            {
                retstr[3] = "医疗证/卡号不允许为空。";
                return false;
            }

            patientname = Convert.ToString(myDS.Tables[2].Rows[0]["name"]);
            if (patientname.Length == 0 || patientname == "")
            {
                retstr[3] = "患者姓名不允许为空。";
                return false;
            }

            sex = Convert.ToString(myDS.Tables[2].Rows[0]["sex"]);
            if (sex.Length == 0 || sex == "")
            {
                retstr[3] = "患者性别不允许为空。";
                return false;
            }

            serial = "0";
            hospdate = Convert.ToString(myDS.Tables[2].Rows[0]["adm_date"]);
            if (hospdate.Length > 0)
            {
                if (!if0.isDate(hospdate))
                {
                    retstr[3] = "无效的入院日期。";
                    return false;
                }
            }
            else
            {
                retstr[3] = "入院日期不允许为空。";
                return false;
            }

            try
            {
                //家庭号
                ls_sql = "Select familyno From n_familydata Where bookcard = '" + medinumb1 + "' And statisticyear = '" + hospdate.Substring(0, 4) + "'";
                if (sqlconn.State != ConnectionState.Open)
                    sqlconn.Open();
                sqlcmd.CommandText = ls_sql;
                medinumb = Convert.ToString(sqlcmd.ExecuteScalar());
                if (medinumb.Length == 0 || medinumb == "")
                {
                    retstr[3] = "家庭号为空。";
                    return false;
                }
            }
            catch (Exception ee)
            {
                retstr[3] = "查询家庭号时发生异常："+ee.Message.ToString();
                return false;
            }


            hospresu = Convert.ToString(myDS.Tables[2].Rows[0]["maindiag_name"]);
            if (hospresu.Length == 0 || hospresu == "")
            {
                retstr[3] = "疾病诊断不允许为空。";
                return false;
            }
            outdate = Convert.ToString(myDS.Tables[2].Rows[0]["dis_date"]);
            nopaydate = "";
            if (outdate.Length > 0)
            {
                if (!if0.isDate(outdate))
                {
                    retstr[3] = "无效的出院日期。";
                    return false;
                }
                nopaydate = outdate.Substring(0, 4) + outdate.Substring(5, 2) + outdate.Substring(8, 2);
            }            
            chkpayid = "0";
            requoffice1 = Convert.ToString(myDS.Tables[2].Rows[0]["adm_deptname"]);
            requoffice2 = Convert.ToString(myDS.Tables[2].Rows[0]["dis_deptname"]);
            #endregion

            #region Insert hosppatinfo2
            ls_sql = "Insert Into hosppatinfo2(hospitalcode,HospID,HospCode,patientid,patientid1,";
            ls_sql += "medinumb,medinumb1,patientname,sex,serial,";
            ls_sql += "hospdate,hospresu,outdate,nopaydate,chkpayid,";
            ls_sql += "requoffice1,requoffice2,patsort,dataresource)";
            ls_sql += "Values('"+hospitalcode+"',"+hospid+",'"+his_link_code+"','"+patientid+"','"+patientid1+"',";
            ls_sql += "'"+medinumb+"','"+medinumb1+"','"+patientname+"','"+sex+"',"+serial+",";
            ls_sql += "'"+hospdate+"','"+hospresu+"','"+outdate+"','"+nopaydate+"',"+chkpayid+",";
            ls_sql += "'"+requoffice1+"','"+requoffice2+"','"+referral_no+"',"+"2"+")";
            try
            {
                sqlcmd.CommandText = ls_sql;   
                sqlcmd.ExecuteNonQuery();
            }
            catch (Exception ee)
            {
                myTrans.Rollback();
                retstr[3] = "保存住院信息时发生异常：" + ee.Message.ToString();
                return false;
            }
            #endregion
            #endregion
            #region 写pricedetail
            for (i = 0;i < myDS.Tables[3].Rows.Count;i++)
            {
                
                #region 数据初始化及检测
                try
                {
                    zyitem_link_code = Convert.ToString(myDS.Tables[3].Rows[i]["zyitem_link_code"]);
                    if (zyitem_link_code.Length == 0 || zyitem_link_code == "")
                    {
                        myTrans.Rollback();
                        retstr[3] = "住院处方流水号不允许为空。";
                        return false;
                    }

                    detailid = i;
                    detailid++;

                    nhcode = Convert.ToString(myDS.Tables[3].Rows[i]["item_code"]);
                    if (nhcode.Length == 0 || nhcode == "")
                    {
                        myTrans.Rollback();
                        retstr[3] = "农合编码不允许为空。";
                        return false;
                    }
                    if (if0.getStrLength(nhcode) > 10)
                        nhcode = if0.getSubstring(nhcode, 10);

                    execoffice = "";
                    requoffice = "";

                    doctname = Convert.ToString(myDS.Tables[3].Rows[i]["doctor_name"]);
                    if (doctname.Length == 0 || doctname == "")
                    {
                        myTrans.Rollback();
                        retstr[3] = "医生姓名不允许为空。";
                        return false;
                    }

                    opername = Convert.ToString(myDS.Tables[3].Rows[i]["doctor_name"]);
                    if (opername.Length == 0 || opername == "")
                    {
                        myTrans.Rollback();
                        retstr[3] = "医生姓名不允许为空。";
                        return false;
                    }

                    //marktype = Convert.ToString(myDS.Tables[3].Rows[i]["item_class"]);
                    marktype = "2";
                    if (marktype.Length == 0 || marktype == "")
                    {
                        myTrans.Rollback();
                        retstr[3] = "项目类别不允许为空。";
                        return false;
                    }

                    mediname = Convert.ToString(myDS.Tables[3].Rows[i]["item_lname"]);
                    if (mediname.Length == 0 || mediname == "")
                    {
                        myTrans.Rollback();
                        retstr[3] = "药品名称不允许为空。";
                        return false;
                    }

                    ddate = Convert.ToString(myDS.Tables[3].Rows[i]["item_date"]);
                    if (ddate.Length > 0)
                    {
                        if (!if0.isDate(ddate))
                        {
                            myTrans.Rollback();
                            retstr[3] = "发生日期不是有效的日期格式(yyyy-mm-dd)。";
                            return false;
                        }
                    }
                    else
                    {
                        myTrans.Rollback();
                        retstr[3] = "发生日期不允许为空。";
                        return false;
                    }

                    date2 = Convert.ToString(myDS.Tables[3].Rows[i]["item_date"]);
                    if (date2.Length > 0)
                    {
                        date2 = date2.Substring(0, 4) + date2.Substring(5, 2) + date2.Substring(8, 2);
                    }
                    else
                    {
                        myTrans.Rollback();
                        retstr[3] = "发生日期不允许为空。";
                        return false;
                    }

                    date3 = Convert.ToString(myDS.Tables[3].Rows[i]["item_date"]);
                    if (date3.Length > 0)
                    {
                        date3 = date3.Substring(0, 4) + date3.Substring(5, 2) + date3.Substring(8, 2);
                    }
                    else
                    {
                        myTrans.Rollback();
                        retstr[3] = "发生日期不允许为空。";
                        return false;
                    }

                    num1 = Convert.ToString(myDS.Tables[3].Rows[i]["count"]);
                    if (!if0.isNumber(num1))
                    {
                        myTrans.Rollback();
                        retstr[3] = "付数不是有效的数字型。";
                        return false;
                    }

                    num2 = "0";

                    user_num = Convert.ToString(myDS.Tables[3].Rows[i]["amount"]);
                    if (!if0.isNumber(user_num))
                    {
                        myTrans.Rollback();
                        retstr[3] = "数量不是有效的数字型。";
                        return false;
                    }

                    unitprice = Convert.ToString(myDS.Tables[3].Rows[i]["price"]);
                    if (!if0.isNumber(unitprice))
                    {
                        myTrans.Rollback();
                        retstr[3] = "单价不是有效的数字型。";
                        return false;
                    }

                    price = Convert.ToString(myDS.Tables[3].Rows[i]["money"]);
                    if (!if0.isNumber(price))
                    {
                        myTrans.Rollback();
                        retstr[3] = "金额不是有效的数字型。";
                        return false;
                    }

                    operid1 = Convert.ToString(myDS.Tables[3].Rows[i]["opr_code"]);
                    if (operid1.Length == 0 || operid1 == "")
                    {
                        myTrans.Rollback();
                        retstr[3] = "报送人不允许为空。";
                        return false;
                    }
                }
                catch (Exception ee)
                {
                    myTrans.Rollback();
                    retstr[3] = "初始化住院处方流水号为" + zyitem_link_code + "的数据时发生异常：" + ee.Message.ToString();
                    return false;
                }                


                #endregion

                #region Insert pricedetail
                try
                {
                    ls_sql = "Insert Into pricedetail(hospitalcode,hospid,detailid,nhcode,execoffice,";
                    ls_sql += "requOffice,doctname,opername,markType,mediname,";
                    ls_sql += "ddate,date2,date3,num1,num2,";
                    ls_sql += "user_num,unitprice,price,operid1) ";
                    ls_sql += "Values('" + hospitalcode + "'," + hospid + "," + detailid + ",'" + nhcode + "','" + execoffice + "',";
                    ls_sql += "'" + requoffice + "','" + doctname + "','" + opername + "','" + marktype + "','" + mediname + "',";
                    ls_sql += "'" + ddate + "','" + date2 + "','" + date3 + "'," + num1 + "," + num2 + ",";
                    ls_sql += user_num + "," + unitprice + "," + price + ",'" + operid1 + "')";
                    sqlcmd.CommandText = ls_sql;
                    sqlcmd.ExecuteNonQuery();
                }
                catch (Exception ee)
                {
                    myTrans.Rollback();
                    retstr[3] = "保存住院处方流水号为" + zyitem_link_code + "的数据时发生异常：" + ee.Message.ToString();
                    return false;
                }

                #endregion
            }            
            #endregion
            #region 写pataccountdetail
            #region 数据初始化及检测
            try
            {
                fee6 = Convert.ToString(myDS.Tables[2].Rows[0]["bed_fee"]);
                if (fee6.Length > 0)
                {
                    if (!if0.isNumber(fee6))
                    {
                        myTrans.Rollback();
                        retstr[3] = "床位费不是有效的数字型。";
                        return false;
                    }
                }
                else
                {
                    fee6 = "0";
                }

                fee10 = Convert.ToString(myDS.Tables[2].Rows[0]["nurse_fee"]);
                if (fee10.Length > 0)
                {
                    if (!if0.isNumber(fee10))
                    {
                        myTrans.Rollback();
                        retstr[3] = "护理费不是有效的数字型。";
                        return false;
                    }
                }
                else
                {
                    fee10 = "0";
                }

                fee1 = Convert.ToString(myDS.Tables[2].Rows[0]["wdrug_fee"]);
                if (fee1.Length > 0)
                {
                    if (!if0.isNumber(fee1))
                    {
                        myTrans.Rollback();
                        retstr[3] = "西药费不是有效的数字型。";
                        return false;
                    }
                }
                else
                {
                    fee1 = "0";
                }

                fee2 = Convert.ToString(myDS.Tables[2].Rows[0]["cdrug_fee"]);
                if (fee2.Length > 0)
                {
                    if (!if0.isNumber(fee2))
                    {
                        myTrans.Rollback();
                        retstr[3] = "中药费不是有效的数字型。";
                        return false;
                    }
                }
                else
                {
                    fee2 = "0";
                }

                fee12 = Convert.ToString(myDS.Tables[2].Rows[0]["test_fee"]);
                if (fee12.Length > 0)
                {
                    if (!if0.isNumber(fee12))
                    {
                        myTrans.Rollback();
                        retstr[3] = "化验费不是有效的数字型。";
                        return false;
                    }
                }
                else
                {
                    fee12 = "0";
                }

                fee7 = Convert.ToString(myDS.Tables[2].Rows[0]["treat_fee"]);
                if (fee7.Length > 0)
                {
                    if (!if0.isNumber(fee7))
                    {
                        myTrans.Rollback();
                        retstr[3] = "诊疗费不是有效的数字型。";
                        return false;
                    }
                }
                else
                {
                    fee7 = "0";
                }

                fee11 = Convert.ToString(myDS.Tables[2].Rows[0]["ops_fee"]);
                if (fee11.Length > 0)
                {
                    if (!if0.isNumber(fee11))
                    {
                        myTrans.Rollback();
                        retstr[3] = "手术费不是有效的数字型。";
                        return false;
                    }
                }
                else
                {
                    fee11 = "0";
                }

                fee8 = Convert.ToString(myDS.Tables[2].Rows[0]["check_fee"]);
                if (fee8.Length > 0)
                {
                    if (!if0.isNumber(fee8))
                    {
                        myTrans.Rollback();
                        retstr[3] = "检查费不是有效的数字型。";
                        return false;
                    }
                }
                else
                {
                    fee8 = "0";
                }

                fee13 = Convert.ToString(myDS.Tables[2].Rows[0]["other_fee"]);
                if (fee13.Length > 0)
                {
                    if (!if0.isNumber(fee13))
                    {
                        myTrans.Rollback();
                        retstr[3] = "其他费不是有效的数字型。";
                        return false;
                    }
                }
                else
                {
                    fee13 = "0";
                }
            }
            catch (Exception ee)
            {
                myTrans.Rollback();
                retstr[3] = "初始化费用分类明细时发生异常：" + ee.Message.ToString();
                return false;
            }
            #endregion

            #region 写pataccountdetail
            try
            {
                ls_sql = "Insert Into pataccountdetail(hospitalcode,hospid,chkpayid,fee6,";
                ls_sql += "fee10,fee1,fee2,fee12,";
                ls_sql += "fee7,fee11,fee8,fee13)";
                ls_sql += " Values('"+hospitalcode+"',"+hospid+","+"0"+","+fee6+",";
                ls_sql += fee10+","+fee1+","+fee2+","+fee12+",";
                ls_sql += fee7+","+fee11+","+fee8+","+fee13+")";
                sqlcmd.CommandText = ls_sql;
                sqlcmd.ExecuteNonQuery();            
            }
            catch (Exception ee)
            {
                myTrans.Rollback();
                retstr[3] = "保存费用分类明细数据时发生异常：" + ee.Message.ToString();
                return false;
            }
            #endregion
            #endregion
            myTrans.Commit();
            retstr[0] = "0";
            #endregion
        }
        finally
        {
            sqlcmd.Dispose();
            sqlconn.Dispose();
            mySR.Close();
            mySR.Dispose();
            myDS.Clear();
            myDS.Dispose();
            if (myTrans != null)
            {                
                myTrans.Dispose();
            }
        } 
        return true;

    }
    #endregion

    #region 取消住院信息
    public bool procFunc_F1020153(string In_jysr_XML, out string[] retstr)
    {
        retstr = new string[4];
        retstr[0] = "1";
        retstr[1] = "";
        retstr[2] = "";
        retstr[3] = "";
        string mess, ls_sql,referral_no,hospitalcode = "";
        decimal hospid = 0;
        Int32 chkpayid = -1;

        #region 获取转诊申请单号
        XmlDocument xmldoc = if0.getXMLDocumentFromString(In_jysr_XML, out mess);
        XmlNodeList xnlist = null;
        if (mess == "TRUE")
        {
            xnlist = xmldoc.GetElementsByTagName("referral_no");
            referral_no = xnlist[0].InnerText;
            if (referral_no.Length == 0 || referral_no == "")
            {
                retstr[3] = "转诊申请单号不允许为空。";
                return false;
            }
        }
        else
        {
            retstr[3] = mess;
            return false;
        }
        #endregion

        #region 取消住院信息
        SqlConnection sqlconn = new SqlConnection(DBConnStr);
        SqlCommand sqlcmd = new SqlCommand();
        SqlTransaction sqltrans = null;
        

        try
        {
            #region 获取hospitlacode,hospid
            if (procFunc_getHospInfo(referral_no, out hospitalcode, out hospid, out mess))
            {
                if (hospitalcode == "-1" && hospid == -1)
                {
                    retstr[3] = mess;
                    return false;
                }
            }
            else
            {
                retstr[3] = mess;
                return false;
            }
            #endregion

            #region 获取chkpayid
            try
            {
                sqlconn.Open();
                sqlcmd.Connection = sqlconn;
                ls_sql = "Select chkpayid From hosppatinfo2 Where hospitalcode = '" + hospitalcode + "'And hospid ="+hospid.ToString()+" ";
                sqlcmd.CommandText = ls_sql;
                chkpayid = Convert.ToInt32(sqlcmd.ExecuteScalar());
                if (chkpayid != 0)
                {
                    retstr[3] = "转诊单号为" + referral_no + "的住院信息已经被审核，不允许取消。";
                    return false;
                }
            }
            catch (Exception ee)
            {
                retstr[3] = "根据转诊单号查询住院信息时发生异常：" + ee.Message.ToString();
                return false;
            }
            #endregion

            #region 删除住院信息 hosppatinfo2,pricedetail,pataccountdetail
            sqltrans = sqlconn.BeginTransaction();
            sqlcmd.Transaction = sqltrans;

            //pataccountdetail
            try
            {
                ls_sql = "Delete From pataccountdetail Where hospitalcode ='" + hospitalcode + "' And hospid = " + hospid.ToString();
                sqlcmd.CommandText = ls_sql;
                sqlcmd.ExecuteNonQuery();
            }
            catch (Exception ee)
            {
                sqltrans.Rollback();
                retstr[3] = "删除费用分类明细时发生异常："+ee.Message.ToString();
                return false;
            }
            //pricedetail
            try
            {
                ls_sql = "Delete From pricedetail Where hospitalcode ='" + hospitalcode + "' And hospid = " + hospid.ToString();
                sqlcmd.CommandText = ls_sql;
                sqlcmd.ExecuteNonQuery();
            }
            catch (Exception ee)
            {
                sqltrans.Rollback();
                retstr[3] = "删除费用明细时发生异常：" + ee.Message.ToString();
                return false;
            }
            //hosppatinfo2
            try
            {
                ls_sql = "Delete From hosppatinfo2 Where hospitalcode ='" + hospitalcode + "' And hospid = " + hospid.ToString();
                sqlcmd.CommandText = ls_sql;
                if (sqlcmd.ExecuteNonQuery() > 1)
                {
                    sqltrans.Rollback();
                    retstr[3] = "删除的住院信息有异常，操作失败。";
                    return false;
                }
            }
            catch (Exception ee)
            {
                sqltrans.Rollback();
                retstr[3] = "删除住院信息时发生异常：" + ee.Message.ToString();
                return false;
            }
            #endregion

            sqltrans.Commit();
            retstr[0] = "0";
        }        
        finally
        {
            
            if (sqltrans != null)
                sqltrans.Dispose();
            sqlcmd.Dispose();
            if (sqlconn.State == ConnectionState.Open)
                sqlconn.Close();
            sqlconn.Dispose();
        }
        #endregion
        
        return true;

    }
    #endregion
    
    #region 上传住院信息
    public bool procFunc_F1020154(string In_jysr_XML, out string[] retstr)
    {
        retstr = new string[4];
        retstr[0] = "1";
        retstr[1] = "";
        retstr[2] = "";
        retstr[3] = "";
        string mess,ls_sql,retdata,referral_no,zyorg_code,his_link_code,hospitalcode,orgname,maindiag_code;
        string hospcode,ls_temp;
        decimal hospid;

        XmlDocument xmldoc = if0.getXMLDocumentFromString(In_jysr_XML, out mess);
        XmlNodeList xnlist;
        SqlConnection sqlconn = new SqlConnection(DBConnStr);
        SqlCommand sqlcmd = new SqlCommand();

        #region 初始化数据
        
        if (mess == "TRUE")
        {
            xnlist = xmldoc.GetElementsByTagName("referral_no");
            referral_no = xnlist[0].InnerText;
            xnlist = xmldoc.GetElementsByTagName("zyorg_code");
            zyorg_code = xnlist[0].InnerText;
            if (zyorg_code.Length == 0 || zyorg_code == "")
            {
                retstr[3] = "医疗机构代码不允许为空。";
                return false;
            }
            xnlist = xmldoc.GetElementsByTagName("his_link_code");
            his_link_code = xnlist[0].InnerText;
            if (his_link_code.Length == 0 || his_link_code == "")
            {
                retstr[3] = "住院就诊流水号不允许为空。";
                return false;
            }
            try
            {
                if (procFunc_getHospInfo(referral_no, out hospitalcode, out hospid, out mess))
                {
                    if (hospitalcode == "-1" && hospid == -1)
                    {
                        retstr[3] = mess;
                        return false;
                    }
                }
                else
                {
                    retstr[3] = mess;
                    return false;
                }
                try
                {
                    ls_sql = "Select hospcode From hosppatinfo2 Where hospitalcode = '" + hospitalcode + "' And hospid ="+hospid.ToString()+" ";
                    sqlconn.Open();
                    sqlcmd.Connection = sqlconn;
                    sqlcmd.CommandText = ls_sql;
                    hospcode = Convert.ToString(sqlcmd.ExecuteScalar());
                }
                catch (Exception ee)
                {
                    retstr[3] = "获取农合住院流水号时发生异常：" + ee.Message.ToString();
                    return false;
                }

                try
                {
                    ls_sql ="Select orgname From organcode Where orgcode = '"+hospitalcode+"'";
                    sqlcmd.CommandText = ls_sql;
                    orgname = Convert.ToString(sqlcmd.ExecuteScalar());
                }
                catch (Exception ee)
                { 
                    retstr[3] = "获取医疗机构名称时发生异常：" + ee.Message.ToString();
                    return false;
                }

                try
                {
                    ls_sql ="Select disease_code From hosppatinfo2_apply Where referral_no = '"+referral_no+"'";
                    sqlcmd.CommandText = ls_sql;
                    maindiag_code = Convert.ToString(sqlcmd.ExecuteScalar());
                }
                catch (Exception ee)
                {
                    retstr[3] = "获取疾病代码时发生异常：" + ee.Message.ToString();
                    return false;
                }
            }
            finally
            {
                sqlcmd.Dispose();
                if (sqlconn.State == ConnectionState.Open)
                    sqlconn.Close();
                sqlconn.Dispose();
            } 
        }
        else
        {
            retstr[3] = mess;
            return false;
        }
        #endregion

        #region 获取住院信息
        DataSet ds = new DataSet();
        
        try
        {
            #region hosppatinfo2,pataccountdetail
            ls_sql = "Select a.hospcode as his_link_code,a.hospcode as inpat_no,'" + referral_no + "' as referral_no,a.medinumb as medcard_id,Right('0'+a.patientid,2) as p_sn,";
            ls_sql += "a.patientname as name,a.sex as sex,null as birthdate,null as age,a.patientid1 as idcard,";
            ls_sql += "homephon as phone,Left('" + referral_no + "',6) as dist_code,'1' as p_state,'1' as visit_type,hospdate as adm_date,";
            ls_sql += "a.outdate as dis_date, Null as inpat_days,'" + zyorg_code + "' as zyorg_code,'" + orgname + "' as zyorg_name,Null as zyorg_level,";
            ls_sql += "Null as adm_deptcode,requoffice1 as adm_deptname,Null as dis_deptcode ,requoffice2 as dis_deptname,Null as doctor,";
            ls_sql += "Null as adm_state,Null as maindiag_state,'" + maindiag_code + "' as maindiag_code,hospresu as maindiag_name,Null as complication_code,";
            ls_sql += "Null as complication_name,Null as ops_code,Null as ops_name,Null as Ward_no,Null as Ward_name,";
            ls_sql += "Null as bed_no,Null as opr_date,Null as trans_orgcode,Null as trans_orgname,Null as trans_orglevel,";
            ls_sql += "Null as canotice_no,Null as bearcard_no,Null as dis_recno,a.grandprice as total_fee,b.fee6 as bed_fee,";
            ls_sql += "b.fee10 as nurse_fee,b.fee1 as wdrug_fee,b.fee2 as cdrug_fee,b.fee12 as test_fee,b.fee7 as treat_fee,";
            ls_sql += "b.fee11 as ops_fee,b.fee8 as check_fee,b.fee13 as other_fee,hospdate as send_date,'' as send_com,";
            ls_sql += "'' as opr_code ,'0' as Valid_State,'0' as Check_State";
            ls_sql += " From hosppatinfo2 a,pataccountdetail b ";
            ls_sql += " Where a.hospitalcode = b.hospitalcode ";
            ls_sql += "  And a.hospid = b.hospid ";
            ls_sql += "  And a.hospitalcode = '" + hospitalcode + "' And a.hospid =" + hospid.ToString();
            ds = if0.getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                if (ds.Tables[0].Rows.Count == 1)
                {
                    if (if0.getXMLRowStringFromDS(ds, out ls_temp))
                    {
                        retdata = ls_temp;
                    }
                    else
                    {
                        retstr[3] = "生成住院数据时发生异常：" + ls_temp;
                        return false;
                    }
                }
                else
                {
                    retstr[3] = "获取住院数据异常：没有找到住院信息或者住院信息多于一条。";
                    return false;
                }
            }
            else
            {
                retstr[3] = "创建住院信息-"+mess;
                return false;
            }
            #endregion

            #region pricedetail
            ls_sql = "Select '"+his_link_code+"' as his_link_code,a.detailid as zyitem_link_code,'"+zyorg_code+"' as zyorg_code,'"+hospcode+"' as inpat_no,a.ddate as item_date,";
            ls_sql += "a.marktype as item_class,a.nhcode as item_code,a.mediname as item_name,a.mediid as item_lcode,a.mediname as item_lname,";
            ls_sql += "Null as specs,Null as drugform,Null as unit,a.unitprice as price,a.user_num as amount,";
            ls_sql += "Null as count,a.price as money,a.opername as doctor_name,Null as doctor_title,a.prechkfee as total_fee,";
            ls_sql += "Null as addsub_total_fee,Null as addsub_reason,a.prechktime as audit_date,Null as audit_orgcode,ddate as send_date,";
            ls_sql += "'' as send_com,'' as opr_code ";
            ls_sql += " From pricedetail a ";
            ls_sql += " Where a.hospitalcode = '" + hospitalcode + "' And a.hospid =" + hospid.ToString();
            ds = if0.getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                if (ds.Tables[0].Rows.Count > 0 )
                {
                    if (if0.getXMLRowStringFromDS(ds, out ls_temp))
                    {
                        retdata += ls_temp;
                    }
                    else
                    {
                        retstr[3] = "生成住院明细数据时发生异常：" + ls_temp;
                        return false;
                    }
                }
                else
                {
                    retstr[3] = "获取住院明细数据异常：没有找到住院明细信息。";
                    return false;
                }
            }
            else
            {
                retstr[3] = "创建住院明细信息-"+mess;
                return false;
            }

            #endregion
            retdata = "<?xml version='1.0' encoding='GB2312'?><root><businessdata>" + retdata + "</businessdata></root>";
            retstr[0] = "0";
            retstr[2] = retdata;
        }
        finally
        {
            sqlcmd.Dispose();
            sqlconn.Close();
            sqlconn.Dispose();
            ds.Dispose();
        }
        
        #endregion
        return true;
    }
    #endregion    

    #region 费用核对
    public bool procFunc_F1020161(string In_jysr_XML, out string[] retstr)
    {
        retstr = new string[4];
        retstr[0] = "1";
        retstr[1] = "";
        retstr[2] = "";
        retstr[3] = "";
        string mess, ls_sql, his_link_code, referral_no, zyorg_code,hospitalcode;
        decimal hospid;

        #region 初始化数据
        XmlDocument xmldoc = if0.getXMLDocumentFromString(In_jysr_XML, out mess);
        XmlNodeList xnlist;
        if (mess == "TRUE")
        {
            xnlist = xmldoc.GetElementsByTagName("his_link_code");
            his_link_code = xnlist[0].InnerText;
            if (his_link_code.Length == 0 || his_link_code == "")
            {
                retstr[3] = "住院就诊流水号不允许为空。";
                return false;
            }
            xnlist = xmldoc.GetElementsByTagName("referral_no");
            referral_no = xnlist[0].InnerText;
            if (referral_no.Length == 0 || referral_no == "")
            {
                retstr[3] = "转诊申请单号不允许为空。";
                return false;
            }
            xnlist =  xmldoc.GetElementsByTagName("zyorg_code");
            zyorg_code = xnlist[0].InnerText;
            if (zyorg_code.Length == 0 || zyorg_code == "")
            {
                retstr[3] = "就医机构代码不允许为空。";
                return false;
            }
        }
        else
        {
            retstr[3] = mess;
            return false;
        }
        #endregion

        #region 获取费用信息
        SqlConnection sqlconn = new SqlConnection(DBConnStr);
        SqlCommand sqlcmd = new SqlCommand();
        DataSet sqlds = new DataSet();
        try
        {
            if (procFunc_getHospInfo(referral_no, out hospitalcode, out hospid, out mess))
            {
                if (hospitalcode == "-1" && hospid == -1)
                {
                    retstr[3] = mess;
                    return false;
                }
            }
            else
            {
                retstr[3] = mess;
                return false;
            }

            try
            {
                ls_sql = "Select '"+his_link_code+"' as his_link_code,'"+referral_no+"' as referral_no,'"+zyorg_code+"' as zyorg_code,";
                ls_sql += " IsNull(Sum(IsNull(price,0)),0) as total_fee,Count(*) as item_count ";
                ls_sql += " From pricedetail ";
                ls_sql += "Where hospitalcode = '" + hospitalcode + "' And hospid = " + hospid.ToString() + " ";
                sqlds = if0.getDataSet(ls_sql, out mess);
                if (mess == "TRUE")
                {
                    if (sqlds.Tables[0].Rows.Count == 0)
                    {
                        retstr[3] = "没有可核对的住院费用。";
                        return false;
                    }
                    if (if0.getXMLRowStringFromDS(sqlds, out mess))
                    {
                        retstr[0] = "0";
                        retstr[2] = mess;
                    }
                    else
                    {
                        retstr[3] = "生成返回结果错误："+mess;
                        return false;
                    }
                }
                else
                {
                    retstr[3] = "计算住院费用错误："+mess;
                    return false;
                }
            }
            catch(Exception ee)
            {
                retstr[3] = "核对住院费用时发生异常："+ee.Message.ToString();
                return false;
            }
        }
        finally
        {            
            sqlcmd.Dispose();
            if (sqlconn.State == ConnectionState.Open)
                sqlconn.Close();
            sqlconn.Dispose();
        }
        #endregion

        return true;
    }
    #endregion

    #region 出院申请
    public bool procFunc_F1020171(string In_jysr_XML, out string[] retstr)
    {
        retstr = new string[4];
        retstr[0] = "1";
        retstr[1] = "";
        retstr[2] = "";
        retstr[3] = "";
        string mess, ls_sql, referral_no, hospitalcode;
        decimal hospid;
        Int32 chkpayid;

        #region 初始化变量
        XmlDocument xmldoc = if0.getXMLDocumentFromString(In_jysr_XML, out mess);
        XmlNodeList xnlist;
        if (mess == "TRUE")
        {
            xnlist = xmldoc.GetElementsByTagName("referral_no");
            referral_no = xnlist[0].InnerText;
            if (referral_no.Length == 0 || referral_no == "")
            {
                retstr[3] = "转诊申请单号不允许为空。";
                return false;
            }
        }
        else
        {
            retstr[3] = mess;
            return false;
        }
        #endregion

        #region 更新出院标志
        SqlConnection sqlconn = new SqlConnection(DBConnStr);
        SqlCommand sqlcmd = new SqlCommand();
        SqlTransaction sqltrans = null;
        try
        {
            if (procFunc_getHospInfo(referral_no, out hospitalcode, out hospid, out mess))
            {
                if (hospitalcode == "-1" && hospid == -1)
                {
                    retstr[3] = mess;
                    return false;
                }
            }
            else
            {
                retstr[3] = mess;
                return false;
            }

            sqlconn.Open();
            try
            {
                ls_sql = "Select chkpayid From hosppatinfo2 Where hospitalcode = '" + hospitalcode + "' And hospid = "+hospid.ToString()+" ";
                sqlcmd.Connection = sqlconn;
                sqlcmd.CommandText = ls_sql;
                chkpayid =Convert.ToInt32(sqlcmd.ExecuteScalar());
                if (chkpayid > 0)
                {
                    retstr[3] = "该住院信息已经审核过了，不允许申请出院。";
                    return false;
                }
            }
            catch (Exception ee)
            {
                retstr[3] = "查询住院信息时发生异常：" + ee.Message.ToString();
                return false;
            }

            try
            {
                sqltrans = sqlconn.BeginTransaction();
                sqlcmd.Transaction = sqltrans;
                ls_sql = "Update hosppatinfo2 Set hospperf = 1 Where hospitalcode = '" + hospitalcode + "' And hospid  = " + hospid.ToString();
                sqlcmd.CommandText = ls_sql;
                sqlcmd.ExecuteNonQuery();
                sqltrans.Commit();
                retstr[0] = "0";
            }
            catch (Exception ee)
            {
                sqltrans.Rollback();
                retstr[3] = "更新出院标志时发生异常：" + ee.Message.ToString();
                return false;
            }
        }
        finally
        {
            if (sqltrans != null)
                sqltrans.Dispose();
            sqlcmd.Dispose();
            if (sqlconn.State == ConnectionState.Open)
                sqlconn.Close();
            sqlconn.Dispose();
        }

        #endregion

        return true;
    }

    #endregion

    #region 取消出院申请
    public bool procFunc_F1020172(string In_jysr_XML, out string[] retstr)
    {
        retstr = new string[4];
        retstr[0] = "1";
        retstr[1] = "";
        retstr[2] = "";
        retstr[3] = "";
        string mess, ls_sql, referral_no, hospitalcode;
        decimal hospid;
        Int32 chkpayid;

        #region 初始化变量
        XmlDocument xmldoc = if0.getXMLDocumentFromString(In_jysr_XML, out mess);
        XmlNodeList xnlist;
        if (mess == "TRUE")
        {
            xnlist = xmldoc.GetElementsByTagName("referral_no");
            referral_no = xnlist[0].InnerText;
            if (referral_no.Length == 0 || referral_no == "")
            {
                retstr[3] = "转诊申请单号不允许为空。";
                return false;
            }
        }
        else
        {
            retstr[3] = mess;
            return false;
        }
        #endregion

        #region 更新出院标志
        SqlConnection sqlconn = new SqlConnection(DBConnStr);
        SqlCommand sqlcmd = new SqlCommand();
        SqlTransaction sqltrans = null;
        try
        {
            if (procFunc_getHospInfo(referral_no, out hospitalcode, out hospid, out mess))
            {
                if (hospitalcode == "-1" && hospid == -1)
                {
                    retstr[3] = mess;
                    return false;
                }
            }
            else
            {
                retstr[3] = mess;
                return false;
            }

            sqlconn.Open();
            try
            {
                ls_sql = "Select chkpayid From hosppatinfo2 Where hospitalcode = '" + hospitalcode + "' And hospid =" + hospid.ToString() + " ";
                sqlcmd.Connection = sqlconn;
                sqlcmd.CommandText = ls_sql;
                chkpayid = Convert.ToInt32(sqlcmd.ExecuteScalar());
                if (chkpayid > 0)
                {
                    retstr[3] = "该住院信息已经审核过了，不允许取消出院申请。";
                    return false;
                }           
            }
            catch (Exception ee)
            {
                retstr[3] = "查询住院信息时发生异常：" + ee.Message.ToString();
                return false;
            }

            try
            {
                sqltrans = sqlconn.BeginTransaction();
                sqlcmd.Transaction = sqltrans;
                ls_sql = "Update hosppatinfo2 Set hospperf = 0 Where hospitalcode = '" + hospitalcode + "' And hospid  = " + hospid.ToString();
                sqlcmd.CommandText = ls_sql;
                sqlcmd.ExecuteNonQuery();
                sqltrans.Commit();
                retstr[0] = "0";
            }
            catch (Exception ee)
            {
                sqltrans.Rollback();
                retstr[3] = "更新出院标志时发生异常：" + ee.Message.ToString();
                return false;
            }
        }
        finally
        {
            if (sqltrans != null)
                sqltrans.Dispose();
            sqlcmd.Dispose();
            if (sqlconn.State == ConnectionState.Open)
                sqlconn.Close();
            sqlconn.Dispose();
        }

        #endregion

        return true;
    }

    #endregion

    #endregion

    #region 整体业务调用入口
    public string[] findService(string In_aqyz_XML, string In_gnbh, string In_jysr_XML)
    {
        /*  入参说明:
            In_aqyz_XML :   安全认证XML 暂不判断
            In_gnbh     :   业务号
            In_jysr_XM  :   业务输入XML 不同业务号有不同的XML格式
        */
        string[] retstr = new string[4];

        retstr[0] = "1";//交易返回标志 0:成功,retstr[3]为空且retstr[2]为有效的输出XML 1:失败,retstr[2]为空,retstr[3]包含错误信息
        retstr[1] = "";//交易流水号,暂都为空
        retstr[2] = "";//交易输出XML 当交易成功则返回有效的XML，其它则为空。不同业务号有不同的XML格式
        retstr[3] = "";//返回信息 当交易失败时返回错误信息 成功则为空

        DBConnStr = DBConnStr.Replace("Provider=SQLOLEDB.1;", "");

        #region 入参有效性判断
        if (In_gnbh.Length == 0)
        {
            retstr[3] = "请填写功能号。";
            return retstr;
        }
        if (In_jysr_XML.Length == 0)
        {
            retstr[3] = "交易输入信息不允许为空。";
        }
        #endregion
        
        #region 根据业务号调用相关业务
        switch (In_gnbh.ToUpper())
        {
            #region 参合验证 F1020101-F1020103
            case "F1020101"://按医疗证+姓名/人编号
            case "F1020102"://按身份证+姓名
            case "F1020103"://按医疗证返回一户信息
                procFunc_F102010X(In_gnbh,In_jysr_XML, out retstr);
                break;
            #endregion
            #region 转诊申请 F1020120
            case "F1020120":
                procFunc_F1020120(In_jysr_XML, out retstr);
                break;
            #endregion
            #region 取消转诊申请 F1020122
            case "F1020122":
                procFunc_F1020122(In_jysr_XML, out retstr);
                break;
            #endregion
            #region 获取转诊信息 F1020131
            case "F1020131":
                procFunc_F1020131(In_jysr_XML, out retstr);
                break;
            #endregion
            #region 获取住院信息 F1020151
            case "F1020151":
                procFunc_F1020151(In_jysr_XML, out retstr);
                break;
            #endregion
            #region 取消住院信息 F1020153
            case "F1020153":
                procFunc_F1020153(In_jysr_XML, out retstr);
                break;
            #endregion
            #region 上传住院信息 F1020154
            case "F1020154":
                procFunc_F1020154(In_jysr_XML, out retstr);
                break;
            #endregion
            #region 费用核对 F1020161
            case "F1020161":
                procFunc_F1020161(In_jysr_XML, out retstr);
                break;
            #endregion
            #region 出院申请 F1020171
            case "F1020171":
                procFunc_F1020171(In_jysr_XML, out retstr);
                break;
            #endregion
            #region 取消出院申请 F1020172
            case "F1020172":
                procFunc_F1020172(In_jysr_XML, out retstr);
                break;
                #endregion
            #region 未定义的功能编号
            default:
                retstr[3] = "[" + In_gnbh + "]为未定义的功能编号。";
                break;
            #endregion
        }
        #endregion
        
        return retstr;
    }
    #endregion
}
