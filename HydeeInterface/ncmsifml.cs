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
/// 新农合目录操作相关

/// </summary>
public class ncmsifml
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

    public ncmsifml()
	{
		//
		// TODO: 在此处添加构造函数逻辑
        //
	}

    public ncmsifml(string dbServer, string dbName, string userID, string passWord, string dbConnString, bool validUser, string areaCode, int interFacePower)
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
    ~ncmsifml()
    {
        if0.Dispose();
    }
    #endregion

    /// <summary>
    /// 从指定的区县农合系统中得到本医院使用的农合药品目录

    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="startrow"></param>
    /// <param name="endrow"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    //[WebMethod(Description="得到本医院使用的农合药品目录(开始行和结束行都为0时得到药品目录总条数")]
    public string getCatalogMedi(string areacode, string hospitalcode, int startrow, int endrow, out string data)
    {
        string mess = "";
        data = "";
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
        string dbConnStr = null;
        if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
        {
            string ls_sql = null;
            DataSet myDs = null;
            ls_sql = "SELECT count(*) as medicount FROM rcmsmediarea WHERE areacode=('" + areacode + "') and chkid=2";
            myDs = if0.getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if ((startrow == endrow && startrow == 0))
                    {
                        if0.getRequestXMLStringFromDS(myDs, "得到药品目录的数据总条数", out mess, out data);
                        return mess;
                    }
                    if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) == 0)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "没有数据可以下载！");
                        return mess;
                    }
                    if (startrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行超过药品目录总条数" + myDs.Tables[0].Rows[0][0].ToString() + "。不能下载！");
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
            //得到医院级别
            SqlConnection Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
            try
            {
                Connection.Open();
                SqlCommand cmd = new SqlCommand("SELECT orglevel FROM organcode where areacode=('" + areacode + "') and orgcode=('" + hospitalcode + "')", Connection);
                
                //ls_sql = "SELECT TOP " + Convert.ToString(endrow - startrow + 1);
                //switch (Convert.ToString(cmd.ExecuteScalar()))
                //{
                //    case "1":
                //        ls_sql += " m.medicode as mcode,m.mediname as mname,m.hosplevel as level,m.price3 as price,m.rate3 as rate,m.medispec as spec,m.usescope as scope";
                //        break;
                //    case "2":
                //        ls_sql += " m.medicode as mcode,m.mediname as mname,m.hosplevel as level,m.price2 as price,m.rate2 as rate,m.medispec as spec,m.usescope as scope";
                //        break;
                //    case "3":
                //        ls_sql += " m.medicode as mcode,m.mediname as mname,m.hosplevel as level,m.price1 as price,m.rate1 as rate,m.medispec as spec,m.usescope as scope";
                //        break;
                //    default:
                //        mess = if0.getXMLErroStrFromString("2010", "查找到无效的医院级别，不能获取农合药品目录！");
                //        return mess;
                //}
                //ls_sql += " FROM (SELECT TOP " + endrow.ToString() + " medicode,mediname,hosplevel,price1,price2,price3,rate1,rate2,rate3,medispec,usescope FROM rcmsmediarea WHERE areacode=('" + areacode + "') AND chkid=2 ORDER BY medicode DESC) m";
                //ls_sql += " ORDER BY m.medicode ASC";

                ls_sql = "SELECT";
                switch (Convert.ToString(cmd.ExecuteScalar()))
                {
                    case "1":
                        ls_sql += " m.medicode as mcode,m.mediname as mname,m.hosplevel as level,m.price3 as price,m.rate3 as rate,m.medispec as spec,m.usescope as scope";
                        break;
                    case "2":
                        ls_sql += " m.medicode as mcode,m.mediname as mname,m.hosplevel as level,m.price2 as price,m.rate2 as rate,m.medispec as spec,m.usescope as scope";
                        break;
                    case "3":
                        ls_sql += " m.medicode as mcode,m.mediname as mname,m.hosplevel as level,m.price1 as price,m.rate1 as rate,m.medispec as spec,m.usescope as scope";
                        break;
                    default:
                        mess = if0.getXMLErroStrFromString("2010", "查找到无效的医院级别，不能获取农合药品目录！");
                        return mess;
                }
                ls_sql += " FROM (SELECT medicode,mediname,hosplevel,price1,price2,price3,rate1,rate2,rate3,medispec,usescope,ROW_NUMBER() OVER (ORDER BY medicode) as rownum FROM rcmsmediarea WHERE areacode=('" + areacode + "') AND chkid=2) m";
                ls_sql += " WHERE m.rownum BETWEEN " + startrow.ToString() + " AND " + endrow.ToString();

                //按照医院级别生成药品目录
                myDs = if0.getDataSet(ls_sql, out mess);
                try
                {
                    if (mess == "TRUE")
                    {
                        if0.getRequestXMLStringFromDS(myDs, "mcode:农合代码;mname:农合名称;level:1-一二三级医院用苿2-二三级医院用苿3-三级医院用药;price:最高限价负数即不限价);rate:报销比例;spec:剂型或规格scope:限制使用范围", out mess, out data);
                        return mess;
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

    /// <summary>
    /// 上传本医院的药品匹配数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="dataXML"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public string putCatalogMedi(string areacode, string hospitalcode, string dataXML, out string datas)
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
                SqlConnection Connection = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
                try
                {
                    int i, li_cnt;
                    string ls_mediid, ls_medicode, ls_mediname, ls_servercode;
                    int li_auditing;
                    string errtxt = "";
                    bool execresult = true, haverecord = false;
                    XmlNodeList nlis = data.SelectSingleNode("DATA").ChildNodes; //获取DATA节点的所有子节点
                    li_cnt = nlis.Count;
                    if (li_cnt > 999)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "上传数据超过1000行。不予处理！");
                        return mess;
                    }
                    Connection.Open();
                    SqlTransaction myTrans;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = Connection;
                    SqlDataReader myReader;
                    try
                    {
                        for (i = 0; i < li_cnt; i++)
                        {
                            haverecord = false;
                            mess = "";
                            nlis = data.GetElementsByTagName("mediid");
                            ls_mediid = Convert.ToString(nlis[i].InnerText);
                            #region //mediid是否有效，是否存在

                            if (ls_mediid == null || ls_mediid.Trim().Length == 0 || !if0.isNumber(ls_mediid))
                            {
                                mess += "mediid必须为数字型非空值";
                            }
                            else
                            {
                                try
                                {
                                    cmd.CommandText = "SELECT Auditing FROM nhppb where orgcode=('" + hospitalcode + "') and mediid=(" + ls_mediid + ")";
                                    li_auditing = Convert.ToInt32(cmd.ExecuteScalar());
                                    myReader = cmd.ExecuteReader();
                                    if (li_auditing > 0)
                                    {
                                        mess += "已审核";
                                    }
                                    else if (myReader.HasRows)
                                    {
                                        haverecord = true;
                                    }
                                    myReader.Close();
                                    myReader.Dispose();
                                }
                                catch (Exception e)
                                {
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + "数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                    return mess;
                                }
                            }
                            #endregion
                            nlis = data.GetElementsByTagName("medicode");
                            ls_medicode = Convert.ToString(nlis[i].InnerText);
                            if (ls_medicode == null || ls_medicode.Trim().Length == 0)
                            {
                                mess += "medicode不能为空";
                            }
                            nlis = data.GetElementsByTagName("mediname");
                            ls_mediname = Convert.ToString(nlis[i].InnerText);
                            if (ls_mediname == null || ls_mediname.Trim().Length == 0)
                            {
                                mess += "mediname不能为空";
                            }
                            nlis = data.GetElementsByTagName("nhdm");
                            ls_servercode = Convert.ToString(nlis[i].InnerText);
                            if (ls_servercode == null || ls_servercode.Trim().Length == 0)
                            {
                                mess += "nhdm不能为空";
                            }
                            else
                            {
                                try
                                {
                                    //cmd.CommandText = "SELECT count(*) FROM rcmsmediarea WHERE areacode=('" + areacode + "') AND medicode=('" + ls_servercode + "') AND chkid=2";
                                    cmd.CommandText = "SELECT count(*) FROM rcmssermediarea WHERE areacode=('" + areacode + "') AND nhcode=('" + ls_servercode + "')";
                                    li_auditing = Convert.ToInt32(cmd.ExecuteScalar());
                                    if (li_auditing == 0)
                                    {
                                        mess += "nhdm值(" + ls_servercode + ")不是农合药品材料目录中的代码";
                                    }
                                }
                                catch (Exception e)
                                {
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + "校验nhdm=" + ls_servercode + "的数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                    return mess;
                                }
                            }
                            if (mess != "")
                            {
                                execresult = false;
                                errtxt += "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + mess + "</errtxt></ROW>";
                            }
                            else
                            {
                                myTrans = Connection.BeginTransaction();
                                try
                                {
                                    cmd.Transaction = myTrans;
                                    if (haverecord) //已上传的做update
                                    {
                                        cmd.CommandText = "UPDATE nhppb SET medicode='" + ls_medicode + "',mediname='" + ls_mediname + "',servercode='" + ls_servercode + "' WHERE orgcode='" + hospitalcode + "' AND mediid=" + ls_mediid;
                                    }
                                    else
                                    {
                                        cmd.CommandText = "INSERT INTO nhppb(orgcode,itemtype,mediid,medicode,mediname,servercode,Auditing) VALUES('" + hospitalcode + "',0," + ls_mediid + ",'" + ls_medicode + "','" + ls_mediname + "','" + ls_servercode + "',0)";
                                    }
                                    cmd.ExecuteNonQuery();
                                    myTrans.Commit();
                                }
                                catch (Exception e)
                                {
                                    myTrans.Rollback();
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + "数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
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
                            mess = if0.getXMLStrFromString("执行成功！");
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
    /// 删除本医院上传的尚未审核的药品匹配数据"
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="dataXML"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public string delCatalogMedi(string areacode, string hospitalcode, string dataXML, out string datas)
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
                SqlConnection Connection = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
                try
                {
                    int i, li_cnt;
                    string ls_mediid;
                    int li_auditing = -1;
                    string errtxt = "";
                    bool execresult = true;
                    XmlNodeList nlis = data.SelectSingleNode("DATA").ChildNodes; //获取DATA节点的所有子节点
                    li_cnt = nlis.Count;
                    if (li_cnt > 999)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "上传数据超过1000行。不予处理！");
                        return mess;
                    }
                    Connection.Open();
                    SqlTransaction myTrans;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = Connection;
                    SqlDataReader myReader;
                    try
                    {
                        for (i = 0; i < li_cnt; i++)
                        {
                            mess = "";
                            nlis = data.GetElementsByTagName("mediid");
                            ls_mediid = Convert.ToString(nlis[i].InnerText);
                            #region
                            if (ls_mediid == null || ls_mediid.Trim().Length == 0 || !if0.isNumber(ls_mediid))
                            {
                                mess += "mediid必须为数字型非空值";
                            }
                            else
                            {
                                try
                                {
                                    cmd.CommandText = "SELECT Auditing FROM nhppb where orgcode=('" + hospitalcode + "') and mediid=(" + ls_mediid + ")";
                                    li_auditing = Convert.ToInt32(cmd.ExecuteScalar());
                                    myReader = cmd.ExecuteReader();
                                    if (li_auditing > 0)
                                    {
                                        mess += "已审核";
                                    }
                                    else if (!myReader.HasRows)
                                    {
                                        li_auditing = -1;
                                    }
                                    myReader.Close();
                                    myReader.Dispose();
                                }
                                catch (Exception e)
                                {
                                    mess = if0.getXMLErroStrFromString("9000", "数据库中校验mediid=" + ls_mediid + "的数据异常：" + e.Message.ToString());
                                    return mess;
                                }
                            }
                            #endregion
                            if (mess != "")
                            {
                                execresult = false;
                                errtxt += "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + mess + "</errtxt></ROW>";
                            }
                            else if (li_auditing == 0)
                            {
                                myTrans = Connection.BeginTransaction();
                                try
                                {
                                    cmd.Transaction = myTrans;
                                    cmd.CommandText = "DELETE FROM nhppb WHERE orgcode=('" + hospitalcode + "') and mediid=" + ls_mediid;
                                    cmd.ExecuteNonQuery();
                                    myTrans.Commit();
                                }
                                catch (Exception e)
                                {
                                    myTrans.Rollback();
                                    mess = if0.getXMLErroStrFromString("9000", "数据库中删除mediid=" + ls_mediid + "数据异常：" + e.Message.ToString());
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
                            mess = if0.getXMLStrFromString("执行成功！");
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
    /// 从指定的区县农合系统中得到本医院已审核的农合药品匹配数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="startrow"></param>
    /// <param name="endrow"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string getCatalogMediAudited(string areacode, string hospitalcode, int startrow, int endrow, out string data)
    {
        string mess = "";
        data = "";
        //参数有效性检验

        if (startrow < 0 || endrow < 0)
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行都不能小于0。不能下载！");
            return mess;
        }
        else if ((startrow == 0 && endrow != 0) || (startrow != 0 && endrow == 0))
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行必须同时为0时表示获取可下载目录条数");
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
            ls_sql = "SELECT count(*) as medicount FROM nhppb WHERE orgcode=('" + hospitalcode + "') and itemtype=0 and auditing>0";
            myDs = if0.getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if ((startrow == endrow && startrow == 0))
                    {
                        if0.getRequestXMLStringFromDS(myDs, "得到已审核的匹配药品数据总条数", out mess, out data);
                        return mess;
                    }
                    if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) == 0)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "没有数据可以下载！");
                        return mess;
                    }
                    if (startrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行超过已审核的匹配药品数据总条数" + myDs.Tables[0].Rows[0][0].ToString() + "。不能下载！");
                        return mess;
                    }
                    if (endrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        endrow = Convert.ToInt32(myDs.Tables[0].Rows[0][0]);
                    }
                }
                else
                {
                    mess = if0.getXMLErroStrFromString("9000", mess);
                    return mess;
                }
            }
            finally
            {
                myDs.Dispose();
            }

            //ls_sql = "SELECT TOP " + Convert.ToString(endrow - startrow + 1);
            //ls_sql += " p.mediid as mediid,p.servercode as nhdm,mediname as mediname";
            //ls_sql += " FROM (SELECT TOP " + endrow.ToString() + " mediid,servercode,mediname from nhppb WHERE orgcode=('" + hospitalcode + "') and itemtype=0 and auditing>0 ORDER BY mediid DESC) p";
            //ls_sql += " ORDER BY p.mediid ASC";

            ls_sql = "SELECT";
            ls_sql += " p.mediid as mediid,p.servercode as nhdm,mediname as mediname";
            ls_sql += " FROM (SELECT mediid,servercode,mediname,ROW_NUMBER() OVER (ORDER BY mediid) as rownum from nhppb WHERE orgcode=('" + hospitalcode + "') and itemtype=0 and auditing>0) p";
            ls_sql += " WHERE p.rownum BETWEEN " + startrow.ToString() + " AND " + endrow.ToString();

            //生成数据
            myDs = if0.getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if0.getRequestXMLStringFromDS(myDs, "mediid:医院项目ID;nhdm:对应的农合代码;mediname:医院项目名称", out mess, out data);
                    return mess;
                }
                else
                {
                    mess = if0.getXMLErroStrFromString("9000", mess);
                    return mess;
                }
            }
            finally
            {
                myDs.Dispose();
            }
        }
        else
        {
            mess = if0.getXMLErroStrFromString("5000", mess);
            return mess;
        }
    }

    /// <summary>
    /// 从指定的区县农合系统中得到本医院使用的门诊农合药品目录

    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="startrow"></param>
    /// <param name="endrow"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string getCatalogMediMZ(string areacode, string hospitalcode, int startrow, int endrow, out string data)
    {
        string mess = "";
        data = "";
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
        string dbConnStr = null;
        if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
        {
            string ls_sql = null;
            DataSet myDs = null;
            ls_sql = "SELECT count(*) as medicount FROM mz_rcmsmediarea WHERE areacode=('" + areacode + "') and chkid=2";
            myDs = if0.getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if ((startrow == endrow && startrow == 0))
                    {
                        if0.getRequestXMLStringFromDS(myDs, "得到门诊药品目录的数据总条数", out mess, out data);
                        return mess;
                    }
                    if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) == 0)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "没有数据可以下载！");
                        return mess;
                    }
                    if (startrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行超过有效门诊药品目录总条数" + myDs.Tables[0].Rows[0][0].ToString() + "。不能下载！");
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
            //得到医院级别
            SqlConnection Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
            try
            {
                Connection.Open();
                SqlCommand cmd = new SqlCommand("SELECT levelcode FROM organcode where areacode=('" + areacode + "') and orgcode=('" + hospitalcode + "')", Connection);
                
                //ls_sql = "SELECT TOP " + Convert.ToString(endrow - startrow + 1);
                //switch (Convert.ToString(cmd.ExecuteScalar()))
                //{
                //    case "1":
                //        ls_sql += " m.medicode as mcode,m.mediname as mname,m.hosplevel as level,m.price1 as price,m.rate1 as rate,m.medispec as spec,m.usescope as scope";
                //        break;
                //    case "2":
                //        ls_sql += " m.medicode as mcode,m.mediname as mname,m.hosplevel as level,m.price2 as price,m.rate2 as rate,m.medispec as spec,m.usescope as scope";
                //        break;
                //    case "3":
                //        ls_sql += " m.medicode as mcode,m.mediname as mname,m.hosplevel as level,m.price3 as price,m.rate3 as rate,m.medispec as spec,m.usescope as scope";
                //        break;
                //    case "4":
                //        ls_sql += " m.medicode as mcode,m.mediname as mname,m.hosplevel as level,m.price4 as price,m.rate4 as rate,m.medispec as spec,m.usescope as scope";
                //        break;
                //    case "5":
                //        ls_sql += " m.medicode as mcode,m.mediname as mname,m.hosplevel as level,m.price5 as price,m.rate5 as rate,m.medispec as spec,m.usescope as scope";
                //        break;
                //    case "9":
                //        ls_sql += " m.medicode as mcode,m.mediname as mname,m.hosplevel as level,m.price6 as price,m.rate6 as rate,m.medispec as spec,m.usescope as scope";
                //        break;
                //    default:
                //        mess = if0.getXMLErroStrFromString("2010", "查找到无效的医院级别，不能获取农合药品目录！");
                //        return mess;
                //}
                //ls_sql += " FROM (SELECT TOP " + endrow.ToString() + " medicode,mediname,hosplevel,price1,price2,price3,price4,price5,price6,rate1,rate2,rate3,rate4,rate5,rate6,medispec,usescope FROM mz_rcmsmediarea WHERE areacode=('" + areacode + "') AND chkid=2 ORDER BY medicode DESC) m";
                //ls_sql += " ORDER BY m.medicode ASC";

                ls_sql = "SELECT";
                switch (Convert.ToString(cmd.ExecuteScalar()))
                {
                    case "1":
                        ls_sql += " m.medicode as mcode,m.mediname as mname,m.hosplevel as level,m.price1 as price,m.rate1 as rate,m.medispec as spec,m.usescope as scope";
                        break;
                    case "2":
                        ls_sql += " m.medicode as mcode,m.mediname as mname,m.hosplevel as level,m.price2 as price,m.rate2 as rate,m.medispec as spec,m.usescope as scope";
                        break;
                    case "3":
                        ls_sql += " m.medicode as mcode,m.mediname as mname,m.hosplevel as level,m.price3 as price,m.rate3 as rate,m.medispec as spec,m.usescope as scope";
                        break;
                    case "4":
                        ls_sql += " m.medicode as mcode,m.mediname as mname,m.hosplevel as level,m.price4 as price,m.rate4 as rate,m.medispec as spec,m.usescope as scope";
                        break;
                    case "5":
                        ls_sql += " m.medicode as mcode,m.mediname as mname,m.hosplevel as level,m.price5 as price,m.rate5 as rate,m.medispec as spec,m.usescope as scope";
                        break;
                    case "9":
                        ls_sql += " m.medicode as mcode,m.mediname as mname,m.hosplevel as level,m.price6 as price,m.rate6 as rate,m.medispec as spec,m.usescope as scope";
                        break;
                    default:
                        mess = if0.getXMLErroStrFromString("2010", "查找到无效的医院级别，不能获取农合药品目录！");
                        return mess;
                }
                ls_sql += " FROM (SELECT medicode,mediname,hosplevel,price1,price2,price3,price4,price5,price6,rate1,rate2,rate3,rate4,rate5,rate6,medispec,usescope,ROW_NUMBER() OVER (ORDER BY medicode) as rownum FROM mz_rcmsmediarea WHERE areacode=('" + areacode + "') AND chkid=2) m";
                ls_sql += " WHERE m.rownum BETWEEN " + startrow.ToString() + " AND " + endrow.ToString();

                //按照医院级别生成药品目录
                myDs = if0.getDataSet(ls_sql, out mess);
                try
                {
                    if (mess == "TRUE")
                    {
                        if0.getRequestXMLStringFromDS(myDs, "mcode:农合代码;mname:农合名称;level:预留未使用;price:最高限价(负数即不限价);rate:报销比例;spec:剂型或规格scope:限制使用范围", out mess, out data);
                        return mess;
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

    /// <summary>
    /// 上传本医院的门诊药品匹配数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="dataXML"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public string putCatalogMediMZ(string areacode, string hospitalcode, string dataXML, out string datas)
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
                SqlConnection Connection = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
                try
                {
                    int i, li_cnt;
                    string ls_mediid, ls_medicode, ls_mediname, ls_servercode;
                    int li_auditing;
                    string errtxt = "";
                    bool execresult = true, haverecord = false;
                    XmlNodeList nlis = data.SelectSingleNode("DATA").ChildNodes; //获取DATA节点的所有子节点
                    li_cnt = nlis.Count;
                    if (li_cnt > 999)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "上传数据超过1000行。不予处理！");
                        return mess;
                    }
                    Connection.Open();
                    SqlTransaction myTrans;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = Connection;
                    SqlDataReader myReader;
                    try
                    {
                        for (i = 0; i < li_cnt; i++)
                        {
                            haverecord = false;
                            mess = "";
                            nlis = data.GetElementsByTagName("mediid");
                            ls_mediid = Convert.ToString(nlis[i].InnerText);
                            #region //mediid是否有效，是否存在

                            if (ls_mediid == null || ls_mediid.Trim().Length == 0 || !if0.isNumber(ls_mediid))
                            {
                                mess += "mediid必须为数字型非空值";
                            }
                            else
                            {
                                try
                                {
                                    cmd.CommandText = "SELECT Auditing FROM mz_nhppb where orgcode=('" + hospitalcode + "') and mediid=(" + ls_mediid + ")";
                                    li_auditing = Convert.ToInt32(cmd.ExecuteScalar());
                                    myReader = cmd.ExecuteReader();
                                    if (li_auditing > 0)
                                    {
                                        mess += "已审核";
                                    }
                                    else if (myReader.HasRows)
                                    {
                                        haverecord = true;
                                    }
                                    myReader.Close();
                                    myReader.Dispose();
                                }
                                catch (Exception e)
                                {
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + "数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                    return mess;
                                }
                            }
                            #endregion
                            nlis = data.GetElementsByTagName("medicode");
                            ls_medicode = Convert.ToString(nlis[i].InnerText);
                            if (ls_medicode == null || ls_medicode.Trim().Length == 0)
                            {
                                mess += "medicode不能为空";
                            }
                            nlis = data.GetElementsByTagName("mediname");
                            ls_mediname = Convert.ToString(nlis[i].InnerText);
                            if (ls_mediname == null || ls_mediname.Trim().Length == 0)
                            {
                                mess += "mediname不能为空";
                            }
                            nlis = data.GetElementsByTagName("nhdm");
                            ls_servercode = Convert.ToString(nlis[i].InnerText);
                            if (ls_servercode == null || ls_servercode.Trim().Length == 0)
                            {
                                mess += "nhdm不能为空";
                            }
                            else
                            {
                                try
                                {
                                    //cmd.CommandText = "SELECT count(*) FROM rcmsmediarea WHERE areacode=('" + areacode + "') AND medicode=('" + ls_servercode + "') AND chkid=2";
                                    cmd.CommandText = "SELECT count(*) FROM mz_rcmssermediarea WHERE areacode=('" + areacode + "') AND nhcode=('" + ls_servercode + "')";
                                    li_auditing = Convert.ToInt32(cmd.ExecuteScalar());
                                    if (li_auditing == 0)
                                    {
                                        mess += "nhdm值(" + ls_servercode + ")不是农合药品材料目录中的代码";
                                    }
                                }
                                catch (Exception e)
                                {
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + "校验nhdm=" + ls_servercode + "的数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                    return mess;
                                }
                            }
                            if (mess != "")
                            {
                                execresult = false;
                                errtxt += "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + mess + "</errtxt></ROW>";
                            }
                            else
                            {
                                myTrans = Connection.BeginTransaction();
                                try
                                {
                                    cmd.Transaction = myTrans;
                                    if (haverecord) //已上传的做update
                                    {
                                        cmd.CommandText = "UPDATE mz_nhppb SET medicode='" + ls_medicode + "',mediname='" + ls_mediname + "',servercode='" + ls_servercode + "' WHERE orgcode='" + hospitalcode + "' AND mediid=" + ls_mediid;
                                    }
                                    else
                                    {
                                        cmd.CommandText = "INSERT INTO mz_nhppb(orgcode,itemtype,mediid,medicode,mediname,servercode,Auditing) VALUES('" + hospitalcode + "',0," + ls_mediid + ",'" + ls_medicode + "','" + ls_mediname + "','" + ls_servercode + "',0)";
                                    }
                                    cmd.ExecuteNonQuery();
                                    myTrans.Commit();
                                }
                                catch (Exception e)
                                {
                                    myTrans.Rollback();
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + "数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
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
                            mess = if0.getXMLStrFromString("执行成功！");
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
    /// 删除本医院上传的尚未审核的门诊药品匹配数据"
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="dataXML"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public string delCatalogMediMZ(string areacode, string hospitalcode, string dataXML, out string datas)
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
                SqlConnection Connection = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
                try
                {
                    int i, li_cnt;
                    string ls_mediid;
                    int li_auditing = -1;
                    string errtxt = "";
                    bool execresult = true;
                    XmlNodeList nlis = data.SelectSingleNode("DATA").ChildNodes; //获取DATA节点的所有子节点
                    li_cnt = nlis.Count;
                    if (li_cnt > 999)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "上传数据超过1000行。不予处理！");
                        return mess;
                    }
                    Connection.Open();
                    SqlTransaction myTrans;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = Connection;
                    SqlDataReader myReader;
                    try
                    {
                        for (i = 0; i < li_cnt; i++)
                        {
                            mess = "";
                            nlis = data.GetElementsByTagName("mediid");
                            ls_mediid = Convert.ToString(nlis[i].InnerText);
                            #region
                            if (ls_mediid == null || ls_mediid.Trim().Length == 0 || !if0.isNumber(ls_mediid))
                            {
                                mess += "mediid必须为数字型非空值";
                            }
                            else
                            {
                                try
                                {
                                    cmd.CommandText = "SELECT Auditing FROM mz_nhppb where orgcode=('" + hospitalcode + "') and mediid=(" + ls_mediid + ")";
                                    li_auditing = Convert.ToInt32(cmd.ExecuteScalar());
                                    myReader = cmd.ExecuteReader();
                                    if (li_auditing > 0)
                                    {
                                        mess += "已审核";
                                    }
                                    else if (!myReader.HasRows)
                                    {
                                        li_auditing = -1;
                                    }
                                    myReader.Close();
                                    myReader.Dispose();
                                }
                                catch (Exception e)
                                {
                                    mess = if0.getXMLErroStrFromString("9000", "数据库中校验mediid=" + ls_mediid + "的数据异常：" + e.Message.ToString());
                                    return mess;
                                }
                            }
                            #endregion
                            if (mess != "")
                            {
                                execresult = false;
                                errtxt += "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + mess + "</errtxt></ROW>";
                            }
                            else if (li_auditing == 0)
                            {
                                myTrans = Connection.BeginTransaction();
                                try
                                {
                                    cmd.Transaction = myTrans;
                                    cmd.CommandText = "DELETE FROM mz_nhppb WHERE orgcode=('" + hospitalcode + "') and mediid=" + ls_mediid;
                                    cmd.ExecuteNonQuery();
                                    myTrans.Commit();
                                }
                                catch (Exception e)
                                {
                                    myTrans.Rollback();
                                    mess = if0.getXMLErroStrFromString("9000", "数据库中删除mediid=" + ls_mediid + "数据异常：" + e.Message.ToString());
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
                            mess = if0.getXMLStrFromString("执行成功！");
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
    /// 从指定的区县农合系统中得到本医院已审核的农合门诊药品匹配数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="startrow"></param>
    /// <param name="endrow"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string getCatalogMediAuditedMZ(string areacode, string hospitalcode, int startrow, int endrow, out string data)
    {
        string mess = "";
        data = "";
        //参数有效性检验

        if (startrow < 0 || endrow < 0)
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行都不能小于0。不能下载！");
            return mess;
        }
        else if ((startrow == 0 && endrow != 0) || (startrow != 0 && endrow == 0))
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行必须同时为0时表示获取可下载目录条数");
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
            ls_sql = "SELECT count(*) as medicount FROM mz_nhppb WHERE orgcode=('" + hospitalcode + "') and itemtype=0 and auditing>0";
            myDs = if0.getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if ((startrow == endrow && startrow == 0))
                    {
                        if0.getRequestXMLStringFromDS(myDs, "得到已审核的匹配药品数据总条数", out mess, out data);
                        return mess;
                    }
                    if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) == 0)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "没有数据可以下载！");
                        return mess;
                    }
                    if (startrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行超过已审核的匹配药品数据总条数" + myDs.Tables[0].Rows[0][0].ToString() + "。不能下载！");
                        return mess;
                    }
                    if (endrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        endrow = Convert.ToInt32(myDs.Tables[0].Rows[0][0]);
                    }
                }
                else
                {
                    mess = if0.getXMLErroStrFromString("9000", mess);
                    return mess;
                }
            }
            finally
            {
                myDs.Dispose();
            }

            //ls_sql = "SELECT TOP " + Convert.ToString(endrow - startrow + 1);
            //ls_sql += " p.mediid as mediid,p.servercode as nhdm,mediname as mediname";
            //ls_sql += " FROM (SELECT TOP " + endrow.ToString() + " mediid,servercode,mediname from mz_nhppb WHERE orgcode=('" + hospitalcode + "') and itemtype=0 and auditing>0 ORDER BY mediid DESC) p";
            //ls_sql += " ORDER BY p.mediid ASC";

            ls_sql = "SELECT";
            ls_sql += " p.mediid as mediid,p.servercode as nhdm,mediname as mediname";
            ls_sql += " FROM (SELECT mediid,servercode,mediname,ROW_NUMBER() OVER (ORDER BY mediid) as rownum from mz_nhppb WHERE orgcode=('" + hospitalcode + "') and itemtype=0 and auditing>0) p";
            ls_sql += " WHERE p.rownum BETWEEN " + startrow.ToString() + " AND " + endrow.ToString();

            //生成数据
            myDs = if0.getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if0.getRequestXMLStringFromDS(myDs, "mediid:医院项目ID;nhdm:对应的农合代码;mediname:医院项目名称", out mess, out data);
                    return mess;
                }
                else
                {
                    mess = if0.getXMLErroStrFromString("9000", mess);
                    return mess;
                }
            }
            finally
            {
                myDs.Dispose();
            }
        }
        else
        {
            mess = if0.getXMLErroStrFromString("5000", mess);
            return mess;
        }
    }

    /// <summary>
    /// 从指定的区县农合系统中得到本医院使用的农合医疗服务目录

    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="startrow"></param>
    /// <param name="endrow"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    //[WebMethod(Description = "得到本医院使用的农合药品目录(开始行和结束行都为0时得到医疗服务目录总条数！")]
    public string getCatalogServer(string areacode, string hospitalcode, int startrow, int endrow, out string data)
    {
        string mess = "";
        data = "";
        //参数有效性检验

        if (startrow < 0 || endrow < 0)
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行都不能小于0。不能下载！");
            return mess;
        }
        else if ((startrow == 0 && endrow != 0) || (startrow != 0 && endrow == 0))
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行必须同时为0时，表示获取可下载目录条数！");
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
        string dbConnStr = null;
        if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
        {
            string ls_sql = null;
            DataSet myDs = null;
            ls_sql = "SELECT count(*) as servercount FROM rcmsareaserver WHERE areacode=('" + areacode + "') and chkid=2";
            myDs = if0.getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if (startrow == endrow && startrow == 0)
                    {
                        if0.getRequestXMLStringFromDS(myDs, "得到医疗服务目录的数据总条数", out mess, out data);
                        return mess;
                    }
                    if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) == 0)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "没有数据可以下载！");
                        return mess;
                    }
                    if (startrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行超过医疗服务总条数" + myDs.Tables[0].Rows[0][0].ToString() + "。不能下载！");
                        return mess;
                    }
                    if (endrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        endrow = Convert.ToInt32(myDs.Tables[0].Rows[0][0]);
                    }
                }
                else
                {
                    mess = if0.getXMLErroStrFromString("9000", mess);
                    return mess;
                }
            }
            finally
            {
                myDs.Dispose();
            }
            //得到医院级别
            SqlConnection Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
            try
            {
                Connection.Open();
                SqlCommand cmd = new SqlCommand("SELECT orglevel FROM organcode where areacode=('" + areacode + "') and orgcode=('" + hospitalcode + "')", Connection);

                //ls_sql = "SELECT TOP " + Convert.ToString(endrow - startrow + 1);
                //switch (Convert.ToString(cmd.ExecuteScalar()))
                //{
                //    case "1":
                //        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price3 as price,s.rate3 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                //        break;
                //    case "2":
                //        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price2 as price,s.rate2 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                //        break;
                //    case "3":
                //        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price1 as price,s.rate1 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                //        break;
                //    default:
                //        mess = if0.getXMLErroStrFromString("2010", "查找到无效的医院级别，不能获取农合医疗服务目录！");
                //        return mess;
                //}
                //ls_sql += " FROM (SELECT TOP " + endrow.ToString() + " servercode,servername,unit,price1,price2,price3,rate1,rate2,rate3,content,excepted,remark FROM rcmsareaserver WHERE areacode=('" + areacode + "') AND chkid=2 ORDER BY servercode DESC) s";
                //ls_sql += " ORDER BY s.servercode ASC";

                ls_sql = "SELECT";
                switch (Convert.ToString(cmd.ExecuteScalar()))
                {
                    case "1":
                        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price3 as price,s.rate3 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                        break;
                    case "2":
                        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price2 as price,s.rate2 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                        break;
                    case "3":
                        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price1 as price,s.rate1 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                        break;
                    default:
                        mess = if0.getXMLErroStrFromString("2010", "查找到无效的医院级别，不能获取农合医疗服务目录！");
                        return mess;
                }
                ls_sql += " FROM (SELECT servercode,servername,unit,price1,price2,price3,rate1,rate2,rate3,content,excepted,remark,ROW_NUMBER() OVER (ORDER BY servercode) as rownum FROM rcmsareaserver WHERE areacode=('" + areacode + "') AND chkid=2) s";
                ls_sql += " WHERE s.rownum BETWEEN " + startrow.ToString() + " AND " + endrow.ToString();

                //ls_sql = "SELECT";
                //switch (Convert.ToString(cmd.ExecuteScalar()))
                //{
                //    case "1":
                //        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price3 as price,s.rate3 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                //        break;
                //    case "2":
                //        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price2 as price,s.rate2 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                //        break;
                //    case "3":
                //        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price1 as price,s.rate1 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                //        break;
                //    default:
                //        mess = if0.getXMLErroStrFromString("2010", "查找到无效的医院级别，不能获取农合医疗服务目录！");
                //        return mess;
                //}
                //ls_sql += " FROM rcmsareaserver s WHERE s.areacode=('" + areacode + "') AND chkid=2";
                //ls_sql += " AND s.rrr BETWEEN " + startrow.ToString() + " AND " + endrow.ToString();

                //按照医院级别生成药品目录
                myDs = if0.getDataSet(ls_sql, out mess);
                try
                {
                    if (mess == "TRUE")
                    {
                        if0.getRequestXMLStringFromDS(myDs, "scode:农合代码;sname:农合名称;unit:单位;price:最高限价(为负数即不限价);rate:报销比例;content:项目内涵;notin:除外内容;memo:说明", out mess, out data);
                        return mess;
                    }
                    else
                    {
                        mess = if0.getXMLErroStrFromString("9000", mess);
                        return mess;
                    }
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

    /// <summary>
    /// 上传本医院的医疗服务匹配数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="dataXML"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public string putCatalogServer(string areacode, string hospitalcode, string dataXML, out string datas)
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
                SqlConnection Connection = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
                try
                {
                    int i, li_cnt;
                    string ls_mediid, ls_medicode, ls_mediname, ls_servercode;
                    int li_auditing;
                    string errtxt = "";
                    bool execresult = true, haverecord = false;
                    XmlNodeList nlis = data.SelectSingleNode("DATA").ChildNodes; //获取DATA节点的所有子节点
                    li_cnt = nlis.Count;
                    if (li_cnt > 999)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "上传数据超过1000行。不予处理！");
                        return mess;
                    }
                    Connection.Open();
                    SqlTransaction myTrans;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = Connection;
                    SqlDataReader myReader;
                    try
                    {
                        for (i = 0; i < li_cnt; i++)
                        {
                            haverecord = false;
                            mess = "";
                            nlis = data.GetElementsByTagName("mediid");
                            ls_mediid = Convert.ToString(nlis[i].InnerText);
                            #region
                            if (ls_mediid == null || ls_mediid.Trim().Length == 0 || !if0.isNumber(ls_mediid))
                            {
                                mess += "mediid必须为数字型非空值";
                            }
                            else
                            {
                                try
                                {
                                    cmd.CommandText = "SELECT Auditing FROM nhppb where orgcode=('" + hospitalcode + "') and mediid=(" + ls_mediid + ")";
                                    li_auditing = Convert.ToInt32(cmd.ExecuteScalar());
                                    myReader = cmd.ExecuteReader();
                                    if (li_auditing > 0)
                                    {
                                        mess += "已审核";
                                    }
                                    else if (myReader.HasRows)
                                    {
                                        haverecord = true;
                                    }
                                    myReader.Close();
                                    myReader.Dispose();
                                }
                                catch (Exception e)
                                {
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + "数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                    return mess;
                                }
                            }
                            #endregion
                            nlis = data.GetElementsByTagName("medicode");
                            ls_medicode = Convert.ToString(nlis[i].InnerText);
                            if (ls_medicode == null || ls_medicode.Trim().Length == 0)
                            {
                                mess += "medicode不能为空";
                            }
                            nlis = data.GetElementsByTagName("mediname");
                            ls_mediname = Convert.ToString(nlis[i].InnerText);
                            if (ls_mediname == null || ls_mediname.Trim().Length == 0)
                            {
                                mess += "mediname不能为空";
                            }
                            nlis = data.GetElementsByTagName("nhdm");
                            ls_servercode = Convert.ToString(nlis[i].InnerText);
                            if (ls_servercode == null || ls_servercode.Trim().Length == 0)
                            {
                                mess += "nhdm不能为空";
                            }
                            else
                            {
                                try
                                {
                                    //cmd.CommandText = "SELECT count(*) FROM rcmsareaserver WHERE areacode=('" + areacode + "') AND servercode=('" + ls_servercode + "') AND chkid=2";
                                    cmd.CommandText = "SELECT count(*) FROM rcmssermediarea WHERE areacode=('" + areacode + "') AND nhcode=('" + ls_servercode + "')";
                                    li_auditing = Convert.ToInt32(cmd.ExecuteScalar());
                                    if (li_auditing == 0)
                                    {
                                        mess += "nhdm值(" + ls_servercode + ")不是农合医疗服务目录中的代码";
                                    }
                                }
                                catch (Exception e)
                                {
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + "校验nhdm=" + ls_servercode + "的数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                    return mess;
                                }
                            }
                            if (mess != "")
                            {
                                execresult = false;
                                errtxt += "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + mess + "</errtxt></ROW>";
                            }
                            else
                            {
                                myTrans = Connection.BeginTransaction();
                                try
                                {
                                    cmd.Transaction = myTrans;
                                    if (haverecord) //已上传的做update
                                    {
                                        cmd.CommandText = "UPDATE nhppb SET medicode='" + ls_medicode + "',mediname='" + ls_mediname + "',servercode='" + ls_servercode + "' WHERE orgcode='" + hospitalcode + "' AND mediid=" + ls_mediid;
                                    }
                                    else
                                    {
                                        cmd.CommandText = "INSERT INTO nhppb(orgcode,itemtype,mediid,medicode,mediname,servercode,Auditing) VALUES('" + hospitalcode + "',1," + ls_mediid + ",'" + ls_medicode + "','" + ls_mediname + "','" + ls_servercode + "',0)";
                                    }
                                    cmd.ExecuteNonQuery();
                                    myTrans.Commit();
                                }
                                catch (Exception e)
                                {
                                    myTrans.Rollback();
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + "数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
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
    /// 删除本医院上传的尚未审核的医疗服务匹配数据

    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="dataXML"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public string delCatalogServer(string areacode, string hospitalcode, string dataXML, out string datas)
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
                SqlConnection Connection = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
                try
                {
                    int i, li_cnt;
                    string ls_mediid;
                    int li_auditing = -1;
                    string errtxt = "";
                    bool execresult = true;
                    XmlNodeList nlis = data.SelectSingleNode("DATA").ChildNodes; //获取DATA节点的所有子节点
                    li_cnt = nlis.Count;
                    if (li_cnt > 999)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "上传数据超过1000行。不予处理！");
                        return mess;
                    }
                    Connection.Open();
                    SqlTransaction myTrans;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = Connection;
                    SqlDataReader myReader;
                    try
                    {
                        for (i = 0; i < li_cnt; i++)
                        {
                            mess = "";
                            nlis = data.GetElementsByTagName("mediid");
                            ls_mediid = Convert.ToString(nlis[i].InnerText);
                            #region
                            if (ls_mediid == null || ls_mediid.Trim().Length == 0 || !if0.isNumber(ls_mediid))
                            {
                                mess += "mediid必须为数字型非空值";
                            }
                            else
                            {
                                try
                                {
                                    cmd.CommandText = "SELECT Auditing FROM nhppb where orgcode=('" + hospitalcode + "') and mediid=(" + ls_mediid + ")";
                                    li_auditing = Convert.ToInt32(cmd.ExecuteScalar());
                                    myReader = cmd.ExecuteReader();
                                    if (li_auditing > 0)
                                    {
                                        mess += "已审核";
                                    }
                                    else if (!myReader.HasRows)
                                    {
                                        li_auditing = -1;
                                    }
                                    myReader.Close();
                                    myReader.Dispose();
                                }
                                catch (Exception e)
                                {
                                    mess = if0.getXMLErroStrFromString("9000", "数据库中校验mediid=" + ls_mediid + "的数据异常：" + e.Message.ToString());
                                    return mess;
                                }
                            }
                            #endregion
                            if (mess != "")
                            {
                                execresult = false;
                                errtxt += "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + mess + "</errtxt></ROW>";
                            }
                            else if (li_auditing == 0)
                            {
                                myTrans = Connection.BeginTransaction();
                                try
                                {
                                    cmd.Transaction = myTrans;
                                    cmd.CommandText = "DELETE FROM nhppb WHERE orgcode=('" + hospitalcode + "') and mediid=" + ls_mediid;
                                    cmd.ExecuteNonQuery();
                                    myTrans.Commit();
                                }
                                catch (Exception e)
                                {
                                    myTrans.Rollback();
                                    mess = if0.getXMLErroStrFromString("9000", "数据库中删除mediid=" + ls_mediid + "数据异常:" + e.Message.ToString());
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
                    mess = if0.getXMLErroStrFromString("9000", "异常:" + e.Message.ToString());
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
    /// 从指定的区县农合系统中得到本医院已审核的农合医疗服务匹配数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="startrow"></param>
    /// <param name="endrow"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string getCatalogServerAudited(string areacode, string hospitalcode, int startrow, int endrow, out string data)
    {
        string mess = "";
        data = "";
        //参数有效性检验

        if (startrow < 0 || endrow < 0)
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行都不能小于0。不能下载！");
            return mess;
        }
        else if ((startrow == 0 && endrow != 0) || (startrow != 0 && endrow == 0))
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行同时为0时，表示获取可下载目录条数");
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
            ls_sql = "SELECT count(*) as medicount FROM nhppb WHERE orgcode=('" + hospitalcode + "') and itemtype=1 and auditing>0";
            myDs = if0.getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if ((startrow == endrow && startrow == 0))
                    {
                        if0.getRequestXMLStringFromDS(myDs, "得到已审核的匹配医疗服务数据总条数", out mess, out data);
                        return mess;
                    }
                    if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) == 0)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "没有数据可以下载！");
                        return mess;
                    }
                    if (startrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行超过已审核的匹配医疗服务总条数" + myDs.Tables[0].Rows[0][0].ToString() + "。不能下载！");
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
            //ls_sql += " p.mediid as mediid,p.servercode as nhdm,mediname as mediname";
            //ls_sql += " FROM (SELECT TOP " + endrow.ToString() + " mediid,servercode,mediname from nhppb WHERE orgcode=('" + hospitalcode + "') and itemtype=1 and auditing>0 ORDER BY mediid DESC) p";
            //ls_sql += " ORDER BY p.mediid ASC";

            ls_sql = "SELECT";
            ls_sql += " p.mediid as mediid,p.servercode as nhdm,mediname as mediname";
            ls_sql += " FROM (SELECT mediid,servercode,mediname,ROW_NUMBER() OVER (ORDER BY mediid) as rownum from nhppb WHERE orgcode=('" + hospitalcode + "') and itemtype=1 and auditing>0) p";
            ls_sql += " WHERE p.rownum BETWEEN " + startrow.ToString() + " AND " + endrow.ToString();

            //生成数据
            myDs = if0.getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                if0.getRequestXMLStringFromDS(myDs, "mediid:医院项目ID;nhdm:对应的农合代码;mediname:医院项目名称", out mess, out data);
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
    /// 从指定的区县农合系统中得到本医院使用的农合门诊医疗服务目录

    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="startrow"></param>
    /// <param name="endrow"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string getCatalogServerMZ(string areacode, string hospitalcode, int startrow, int endrow, out string data)
    {
        string mess = "";
        data = "";
        //参数有效性检验

        if (startrow < 0 || endrow < 0)
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行都不能小于0。不能下载！");
            return mess;
        }
        else if ((startrow == 0 && endrow != 0) || (startrow != 0 && endrow == 0))
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行必须同时为0时，表示获取可下载目录条数！");
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
        string dbConnStr = null;
        if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
        {
            string ls_sql = null;
            DataSet myDs = null;
            ls_sql = "SELECT count(*) as servercount FROM mz_rcmsareaserver WHERE areacode=('" + areacode + "') and chkid=2";
            myDs = if0.getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if (startrow == endrow && startrow == 0)
                    {
                        if0.getRequestXMLStringFromDS(myDs, "得到医疗服务门诊目录的数据总条数", out mess, out data);
                        return mess;
                    }
                    if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) == 0)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "没有数据可以下载！");
                        return mess;
                    }
                    if (startrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行超过有效医疗服务门诊目录总条数" + myDs.Tables[0].Rows[0][0].ToString() + "。不能下载！");
                        return mess;
                    }
                    if (endrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        endrow = Convert.ToInt32(myDs.Tables[0].Rows[0][0]);
                    }
                }
                else
                {
                    mess = if0.getXMLErroStrFromString("9000", mess);
                    return mess;
                }
            }
            finally
            {
                myDs.Dispose();
            }
            //得到医院级别
            SqlConnection Connection = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
            try
            {
                Connection.Open();
                SqlCommand cmd = new SqlCommand("SELECT levelcode FROM organcode where areacode=('" + areacode + "') and orgcode=('" + hospitalcode + "')", Connection);
                
                //ls_sql = "SELECT TOP " + Convert.ToString(endrow - startrow + 1);
                //switch (Convert.ToString(cmd.ExecuteScalar()))
                //{
                //    case "1":
                //        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price1 as price,s.rate1 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                //        break;
                //    case "2":
                //        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price2 as price,s.rate2 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                //        break;
                //    case "3":
                //        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price3 as price,s.rate3 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                //        break;
                //    case "4":
                //        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price4 as price,s.rate4 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                //        break;
                //    case "5":
                //        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price5 as price,s.rate5 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                //        break;
                //    case "9":
                //        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price6 as price,s.rate6 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                //        break;
                //    default:
                //        mess = if0.getXMLErroStrFromString("2010", "查找到无效的医院级别，不能获取农合医疗服务目录！");
                //        return mess;
                //}
                //ls_sql += " FROM (SELECT TOP " + endrow.ToString() + " servercode,servername,unit,price1,price2,price3,price4,price5,price6,rate1,rate2,rate3,rate4,rate5,rate6,content,excepted,remark FROM mz_rcmsareaserver WHERE areacode=('" + areacode + "') AND chkid=2 ORDER BY servercode DESC) s";
                //ls_sql += " ORDER BY s.servercode ASC";

                ls_sql = "SELECT";
                switch (Convert.ToString(cmd.ExecuteScalar()))
                {
                    case "1":
                        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price1 as price,s.rate1 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                        break;
                    case "2":
                        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price2 as price,s.rate2 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                        break;
                    case "3":
                        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price3 as price,s.rate3 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                        break;
                    case "4":
                        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price4 as price,s.rate4 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                        break;
                    case "5":
                        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price5 as price,s.rate5 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                        break;
                    case "9":
                        ls_sql += " s.servercode as scode,s.servername as sname,s.unit as unit,s.price6 as price,s.rate6 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                        break;
                    default:
                        mess = if0.getXMLErroStrFromString("2010", "查找到无效的医院级别，不能获取农合医疗服务目录！");
                        return mess;
                }
                ls_sql += " FROM (SELECT servercode,servername,unit,price1,price2,price3,price4,price5,price6,rate1,rate2,rate3,rate4,rate5,rate6,content,excepted,remark,ROW_NUMBER() OVER (ORDER BY servercode) as rownum FROM mz_rcmsareaserver WHERE areacode=('" + areacode + "') AND chkid=2) s";
                ls_sql += " WHERE s.rownum BETWEEN " + startrow.ToString() + " AND " + endrow.ToString();

                //按照医院级别生成药品目录
                myDs = if0.getDataSet(ls_sql, out mess);
                try
                {
                    if (mess == "TRUE")
                    {
                        if0.getRequestXMLStringFromDS(myDs, "scode:农合代码;sname:农合名称;unit:单位;price:最高限价(为负数即不限价);rate:报销比例;content:项目内涵;notin:除外内容;memo:说明", out mess, out data);
                        return mess;
                    }
                    else
                    {
                        mess = if0.getXMLErroStrFromString("9000", mess);
                        return mess;
                    }
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

    /// <summary>
    /// 上传本医院的门诊医疗服务匹配数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="dataXML"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public string putCatalogServerMZ(string areacode, string hospitalcode, string dataXML, out string datas)
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
                SqlConnection Connection = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
                try
                {
                    int i, li_cnt;
                    string ls_mediid, ls_medicode, ls_mediname, ls_servercode;
                    int li_auditing;
                    string errtxt = "";
                    bool execresult = true, haverecord = false;
                    XmlNodeList nlis = data.SelectSingleNode("DATA").ChildNodes; //获取DATA节点的所有子节点
                    li_cnt = nlis.Count;
                    if (li_cnt > 999)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "上传数据超过1000行。不予处理！");
                        return mess;
                    }
                    Connection.Open();
                    SqlTransaction myTrans;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = Connection;
                    SqlDataReader myReader;
                    try
                    {
                        for (i = 0; i < li_cnt; i++)
                        {
                            haverecord = false;
                            mess = "";
                            nlis = data.GetElementsByTagName("mediid");
                            ls_mediid = Convert.ToString(nlis[i].InnerText);
                            #region
                            if (ls_mediid == null || ls_mediid.Trim().Length == 0 || !if0.isNumber(ls_mediid))
                            {
                                mess += "mediid必须为数字型非空值";
                            }
                            else
                            {
                                try
                                {
                                    cmd.CommandText = "SELECT Auditing FROM mz_nhppb where orgcode=('" + hospitalcode + "') and mediid=(" + ls_mediid + ")";
                                    li_auditing = Convert.ToInt32(cmd.ExecuteScalar());
                                    myReader = cmd.ExecuteReader();
                                    if (li_auditing > 0)
                                    {
                                        mess += "已审核";
                                    }
                                    else if (myReader.HasRows)
                                    {
                                        haverecord = true;
                                    }
                                    myReader.Close();
                                    myReader.Dispose();
                                }
                                catch (Exception e)
                                {
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + "数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                    return mess;
                                }
                            }
                            #endregion
                            nlis = data.GetElementsByTagName("medicode");
                            ls_medicode = Convert.ToString(nlis[i].InnerText);
                            if (ls_medicode == null || ls_medicode.Trim().Length == 0)
                            {
                                mess += "medicode不能为空";
                            }
                            nlis = data.GetElementsByTagName("mediname");
                            ls_mediname = Convert.ToString(nlis[i].InnerText);
                            if (ls_mediname == null || ls_mediname.Trim().Length == 0)
                            {
                                mess += "mediname不能为空";
                            }
                            nlis = data.GetElementsByTagName("nhdm");
                            ls_servercode = Convert.ToString(nlis[i].InnerText);
                            if (ls_servercode == null || ls_servercode.Trim().Length == 0)
                            {
                                mess += "nhdm不能为空";
                            }
                            else
                            {
                                try
                                {
                                    //cmd.CommandText = "SELECT count(*) FROM rcmsareaserver WHERE areacode=('" + areacode + "') AND servercode=('" + ls_servercode + "') AND chkid=2";
                                    cmd.CommandText = "SELECT count(*) FROM mz_rcmssermediarea WHERE areacode=('" + areacode + "') AND nhcode=('" + ls_servercode + "')";
                                    li_auditing = Convert.ToInt32(cmd.ExecuteScalar());
                                    if (li_auditing == 0)
                                    {
                                        mess += "nhdm值(" + ls_servercode + ")不是农合医疗服务目录中的代码";
                                    }
                                }
                                catch (Exception e)
                                {
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + "校验nhdm=" + ls_servercode + "的数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                    return mess;
                                }
                            }
                            if (mess != "")
                            {
                                execresult = false;
                                errtxt += "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + mess + "</errtxt></ROW>";
                            }
                            else
                            {
                                myTrans = Connection.BeginTransaction();
                                try
                                {
                                    cmd.Transaction = myTrans;
                                    if (haverecord) //已上传的做update
                                    {
                                        cmd.CommandText = "UPDATE mz_nhppb SET medicode='" + ls_medicode + "',mediname='" + ls_mediname + "',servercode='" + ls_servercode + "' WHERE orgcode='" + hospitalcode + "' AND mediid=" + ls_mediid;
                                    }
                                    else
                                    {
                                        cmd.CommandText = "INSERT INTO mz_nhppb(orgcode,itemtype,mediid,medicode,mediname,servercode,Auditing) VALUES('" + hospitalcode + "',1," + ls_mediid + ",'" + ls_medicode + "','" + ls_mediname + "','" + ls_servercode + "',0)";
                                    }
                                    cmd.ExecuteNonQuery();
                                    myTrans.Commit();
                                }
                                catch (Exception e)
                                {
                                    myTrans.Rollback();
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + "数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
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
    /// 删除本医院上传的尚未审核的门诊医疗服务匹配数据

    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="dataXML"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public string delCatalogServerMZ(string areacode, string hospitalcode, string dataXML, out string datas)
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
                SqlConnection Connection = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
                try
                {
                    int i, li_cnt;
                    string ls_mediid;
                    int li_auditing = -1;
                    string errtxt = "";
                    bool execresult = true;
                    XmlNodeList nlis = data.SelectSingleNode("DATA").ChildNodes; //获取DATA节点的所有子节点
                    li_cnt = nlis.Count;
                    if (li_cnt > 999)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "上传数据超过1000行。不予处理！");
                        return mess;
                    }
                    Connection.Open();
                    SqlTransaction myTrans;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = Connection;
                    SqlDataReader myReader;
                    try
                    {
                        for (i = 0; i < li_cnt; i++)
                        {
                            mess = "";
                            nlis = data.GetElementsByTagName("mediid");
                            ls_mediid = Convert.ToString(nlis[i].InnerText);
                            #region
                            if (ls_mediid == null || ls_mediid.Trim().Length == 0 || !if0.isNumber(ls_mediid))
                            {
                                mess += "mediid必须为数字型非空值";
                            }
                            else
                            {
                                try
                                {
                                    cmd.CommandText = "SELECT Auditing FROM mz_nhppb where orgcode=('" + hospitalcode + "') and mediid=(" + ls_mediid + ")";
                                    li_auditing = Convert.ToInt32(cmd.ExecuteScalar());
                                    myReader = cmd.ExecuteReader();
                                    if (li_auditing > 0)
                                    {
                                        mess += "已审核";
                                    }
                                    else if (!myReader.HasRows)
                                    {
                                        li_auditing = -1;
                                    }
                                    myReader.Close();
                                    myReader.Dispose();
                                }
                                catch (Exception e)
                                {
                                    mess = if0.getXMLErroStrFromString("9000", "数据库中校验mediid=" + ls_mediid + "的数据异常：" + e.Message.ToString());
                                    return mess;
                                }
                            }
                            #endregion
                            if (mess != "")
                            {
                                execresult = false;
                                errtxt += "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + mess + "</errtxt></ROW>";
                            }
                            else if (li_auditing == 0)
                            {
                                myTrans = Connection.BeginTransaction();
                                try
                                {
                                    cmd.Transaction = myTrans;
                                    cmd.CommandText = "DELETE FROM mz_nhppb WHERE orgcode=('" + hospitalcode + "') and mediid=" + ls_mediid;
                                    cmd.ExecuteNonQuery();
                                    myTrans.Commit();
                                }
                                catch (Exception e)
                                {
                                    myTrans.Rollback();
                                    mess = if0.getXMLErroStrFromString("9000", "数据库中删除mediid=" + ls_mediid + "数据异常:" + e.Message.ToString());
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
                    mess = if0.getXMLErroStrFromString("9000", "异常:" + e.Message.ToString());
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
    /// 从指定的区县农合系统中得到本医院已审核的农合门诊医疗服务匹配数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="startrow"></param>
    /// <param name="endrow"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string getCatalogServerAuditedMZ(string areacode, string hospitalcode, int startrow, int endrow, out string data)
    {
        string mess = "";
        data = "";
        //参数有效性检验

        if (startrow < 0 || endrow < 0)
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行都不能小于0。不能下载！");
            return mess;
        }
        else if ((startrow == 0 && endrow != 0) || (startrow != 0 && endrow == 0))
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行同时为0时，表示获取可下载目录条数");
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
            ls_sql = "SELECT count(*) as medicount FROM mz_nhppb WHERE orgcode=('" + hospitalcode + "') and itemtype=1 and auditing>0";
            myDs = if0.getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if ((startrow == endrow && startrow == 0))
                    {
                        if0.getRequestXMLStringFromDS(myDs, "得到已审核的匹配医疗服务数据总条数", out mess, out data);
                        return mess;
                    }
                    if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) == 0)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "没有数据可以下载！");
                        return mess;
                    }
                    if (startrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行超过已审核的匹配医疗服务总条数" + myDs.Tables[0].Rows[0][0].ToString() + "。不能下载！");
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
            //ls_sql += " p.mediid as mediid,p.servercode as nhdm,mediname as mediname";
            //ls_sql += " FROM (SELECT TOP " + endrow.ToString() + " mediid,servercode,mediname from mz_nhppb WHERE orgcode=('" + hospitalcode + "') and itemtype=1 and auditing>0 ORDER BY mediid DESC) p";
            //ls_sql += " ORDER BY p.mediid ASC";

            ls_sql = "SELECT";
            ls_sql += " p.mediid as mediid,p.servercode as nhdm,mediname as mediname";
            ls_sql += " FROM (SELECT mediid,servercode,mediname,ROW_NUMBER() OVER (ORDER BY mediid) as rownum from mz_nhppb WHERE orgcode=('" + hospitalcode + "') and itemtype=1 and auditing>0) p";
            ls_sql += " WHERE p.rownum BETWEEN " + startrow.ToString() + " AND " + endrow.ToString();

            //生成数据
            myDs = if0.getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                if0.getRequestXMLStringFromDS(myDs, "mediid:医院项目ID;nhdm:对应的农合代码;mediname:医院项目名称", out mess, out data);
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
    /// 从指定的区县农合系统中得到本医院使用的ICD-10目录
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="startrow"></param>
    /// <param name="endrow"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    //[WebMethod(Description = "得到本医院使用的ICD-10目录(开始行和结束行都为0时得到ICD-10目录总条数")]
    public string getCatalogICD10(string areacode, string hospitalcode, int startrow, int endrow, out string data)
    {
        string mess = "";
        data = "";
        //参数有效性检验

        if (startrow < 0 || endrow < 0)
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行都不能小于0。不能下载！");
            return mess;
        }
        else if ((startrow == 0 && endrow != 0) || (startrow != 0 && endrow == 0))
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行必须同时为0时，表示获取可下载目录条数");
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
            ls_sql = "SELECT count(*) as icdcount FROM nhicd10";
            myDs = if0.getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if (startrow == endrow && startrow == 0)
                    {
                        if0.getRequestXMLStringFromDS(myDs, "得到ICD-10目录的数据总条数", out mess, out data);
                        return mess;
                    }
                    if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) == 0)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "没有数据可以下载！");
                        return mess;
                    }
                    if (startrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行超过ICD-10目录总条数" + myDs.Tables[0].Rows[0][0].ToString() + "。不能下载！");
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

            //ls_sql = "SELECT TOP " + Convert.ToString(endrow - startrow + 1) + " i.icdcode as icode,i.icdname as iname,i.memo as memo,i.pym as py ";
            //ls_sql += " FROM (SELECT TOP " + endrow.ToString() + " icdcode,icdname,memo,pym FROM nhicd10 ORDER BY icdcode DESC) i";
            //ls_sql += " ORDER BY i.icdcode ASC";

            ls_sql = "SELECT i.icdcode as icode,i.icdname as iname,i.memo as memo,i.pym as py ";
            ls_sql += " FROM (SELECT icdcode,icdname,memo,pym,ROW_NUMBER() OVER (ORDER BY icdcode) as rownum FROM nhicd10) i";
            ls_sql += "  WHERE i.rownum BETWEEN " + startrow.ToString() + " AND " + endrow.ToString();

            //ls_sql = "SELECT i.icdcode as icode,i.icdname as iname,i.memo as memo,i.pym as py ";
            //ls_sql += " FROM nhicd10 i";
            //ls_sql += "  WHERE i.rrr >= " + startrow.ToString() + " AND i.rrr<=" + endrow.ToString();

            //生成ICD-10目录
            myDs = if0.getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                if0.getRequestXMLStringFromDS(myDs, "icode:ICD代码;iname:ICD名称;memo:说明;py:拼音码", out mess, out data);
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
    /// 上传本医院的ICD-10匹配数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="dataXML"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public string putCatalogICD10(string areacode, string hospitalcode, string dataXML, out string datas)
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
                    int i, li_cnt;
                    string ls_hicd10, ls_hicdname, ls_icdcode;
                    int li_auditing;
                    string errtxt = "";
                    bool execresult = true, haverecord = false;
                    XmlNodeList nlis = data.SelectSingleNode("DATA").ChildNodes; //获取DATA节点的所有子节点
                    li_cnt = nlis.Count;
                    if (li_cnt > 999)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "上传数据超过1000行。不予处理！");
                        return mess;
                    }
                    Connection.Open();
                    SqlTransaction myTrans;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = Connection;
                    SqlDataReader myReader;
                    try
                    {
                        for (i = 0; i < li_cnt; i++)
                        {
                            haverecord = false;
                            mess = "";
                            nlis = data.GetElementsByTagName("hicd");
                            ls_hicd10 = Convert.ToString(nlis[i].InnerText);
                            #region
                            if (ls_hicd10 == null || ls_hicd10.Trim().Length == 0)
                            {
                                mess += "hicd不能为空";
                            }
                            else
                            {
                                try
                                {
                                    cmd.CommandText = "SELECT Auditing FROM nhppb2 WHERE orgcode=('" + hospitalcode + "') AND hicd10=('" + ls_hicd10 + "')";
                                    li_auditing = Convert.ToInt32(cmd.ExecuteScalar());
                                    myReader = cmd.ExecuteReader();
                                    if (li_auditing > 0)
                                    {
                                        mess += "已审核";
                                    }
                                    else if (myReader.HasRows)
                                    {
                                        haverecord = true;
                                    }
                                    myReader.Close();
                                    myReader.Dispose();
                                }
                                catch (Exception e)
                                {
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><hicd>" + ls_hicd10 + "</hicd><errtxt>" + "校验hicd=" + ls_hicd10 + "的数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                    return mess;
                                }
                            }
                            #endregion
                            nlis = data.GetElementsByTagName("hicdname");
                            ls_hicdname = Convert.ToString(nlis[i].InnerText);
                            if (ls_hicdname == null || ls_hicdname.Trim().Length == 0)
                            {
                                mess += "icdname不能为空";
                            }
                            nlis = data.GetElementsByTagName("icdcode");
                            ls_icdcode = Convert.ToString(nlis[i].InnerText);
                            if (ls_icdcode == null || ls_icdcode.Trim().Length == 0)
                            {
                                mess += "icdcode不能为空";
                            }
                            else
                            {
                                try
                                {
                                    cmd.CommandText = "SELECT count(*) FROM nhicd10 WHERE icdcode=('" + ls_icdcode + "')";
                                    li_auditing = Convert.ToInt32(cmd.ExecuteScalar());
                                    if (li_auditing == 0)
                                    {
                                        mess += "icdcode值(" + ls_icdcode + ")不是农合ICD目录中的代码";
                                    }
                                }
                                catch (Exception e)
                                {
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><hicd>" + ls_hicd10 + "</hicd><errtxt>" + "校验hicd=" + ls_hicd10 + "的数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                    return mess;
                                }
                            }
                            if (mess != "")
                            {
                                execresult = false;
                                errtxt += "<ROW><hicd>" + ls_hicd10 + "</hicd><errtxt>" + mess + "</errtxt></ROW>";
                            }
                            else
                            {
                                myTrans = Connection.BeginTransaction();
                                try
                                {
                                    cmd.Transaction = myTrans;
                                    if (haverecord) //已上传的做update
                                    {
                                        cmd.CommandText = "UPDATE nhppb2 SET hicdname='" + ls_hicdname + "',icdcode='" + ls_icdcode + "' WHERE orgcode='" + hospitalcode + "' AND hicd10='" + ls_hicd10 + "'";
                                    }
                                    else
                                    {
                                        cmd.CommandText = "INSERT INTO nhppb2(orgcode,hicd10,hicdname,icdcode,Auditing) VALUES('" + hospitalcode + "','" + ls_hicd10 + "','" + ls_hicdname + "','" + ls_icdcode + "',0)";
                                    }
                                    cmd.ExecuteNonQuery();
                                    myTrans.Commit();
                                }
                                catch (Exception e)
                                {
                                    myTrans.Rollback();
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><hicd>" + ls_hicd10 + "</hicd><errtxt>" + "数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
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
    /// 删除本医院上传的尚未审核的ICD-10匹配数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="dataXML"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public string delCatalogICD10(string areacode, string hospitalcode, string dataXML, out string datas)
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
                SqlConnection Connection = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
                try
                {
                    int i, li_cnt;
                    string ls_hicd10;
                    int li_auditing = -1;
                    string errtxt = "";
                    bool execresult = true;
                    XmlNodeList nlis = data.SelectSingleNode("DATA").ChildNodes; //获取DATA节点的所有子节点
                    li_cnt = nlis.Count;
                    if (li_cnt > 999)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "上传数据超过1000行。不予处理！");
                        return mess;
                    }
                    Connection.Open();
                    SqlTransaction myTrans;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = Connection;
                    SqlDataReader myReader;
                    try
                    {
                        for (i = 0; i < li_cnt; i++)
                        {
                            mess = "";
                            nlis = data.GetElementsByTagName("hicd");
                            ls_hicd10 = Convert.ToString(nlis[i].InnerText);
                            #region
                            if (ls_hicd10 == null || ls_hicd10.Trim().Length == 0)
                            {
                                mess += "hicd不能为空";
                            }
                            else
                            {
                                try
                                {
                                    cmd.CommandText = "SELECT Auditing FROM nhppb2 where orgcode=('" + hospitalcode + "') and hicd10=('" + ls_hicd10 + "')";
                                    li_auditing = Convert.ToInt32(cmd.ExecuteScalar());
                                    myReader = cmd.ExecuteReader();
                                    if (li_auditing > 0)
                                    {
                                        mess += "已审核";
                                    }
                                    else if (!myReader.HasRows)
                                    {
                                        li_auditing = -1;
                                    }
                                    myReader.Close();
                                    myReader.Dispose();
                                }
                                catch (Exception e)
                                {
                                    mess = if0.getXMLErroStrFromString("9000", "数据库中校验hicd=" + ls_hicd10 + "的数据异常：" + e.Message.ToString());
                                    return mess;
                                }
                            }
                            #endregion
                            if (mess != "")
                            {
                                execresult = false;
                                errtxt += "<ROW><hicd>" + ls_hicd10 + "</hicd><errtxt>" + mess + "</errtxt></ROW>";
                            }
                            else if (li_auditing == 0)
                            {
                                myTrans = Connection.BeginTransaction();
                                try
                                {
                                    cmd.Transaction = myTrans;
                                    cmd.CommandText = "DELETE FROM nhppb2 WHERE orgcode=('" + hospitalcode + "') and hicd10='" + ls_hicd10 + "'";
                                    cmd.ExecuteNonQuery();
                                    myTrans.Commit();
                                }
                                catch (Exception e)
                                {
                                    myTrans.Rollback();
                                    mess = if0.getXMLErroStrFromString("9000", "数据库中删除hicd=" + ls_hicd10 + "数据异常：" + e.Message.ToString());
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
    /// 从指定的区县农合系统中得到本医院已审核的农合ICD-10匹配数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="startrow"></param>
    /// <param name="endrow"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string getCatalogICD10Audited(string areacode, string hospitalcode, int startrow, int endrow, out string data)
    {
        string mess = "";
        data = "";
        //参数有效性检验

        if (startrow < 0 || endrow < 0)
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行都不能小于0。不能下载！");
            return mess;
        }
        else if ((startrow == 0 && endrow != 0) || (startrow != 0 && endrow == 0))
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行必须同时为0时，表示获取可下载目录条数");
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
            ls_sql = "SELECT count(*) as medicount FROM nhppb2 WHERE orgcode=('" + hospitalcode + "') and auditing>0";
            myDs = if0.getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if ((startrow == endrow && startrow == 0))
                    {
                        if0.getRequestXMLStringFromDS(myDs, "得到已审核的匹配ICD-10数据总条数", out mess, out data);
                        return mess;
                    }
                    if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) == 0)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "没有数据可以下载！");
                        return mess;
                    }
                    if (startrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行超过已审核的匹配ICD-10总条数" + myDs.Tables[0].Rows[0][0].ToString() + "。不能下载！");
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
            //ls_sql += " p.hicd10 as hicd,p.icdcode as nhdm,hicdname hicdname";
            //ls_sql += " FROM (SELECT TOP " + endrow.ToString() + " hicd10,icdcode,hicdname from nhppb2 WHERE orgcode=('" + hospitalcode + "') and auditing>0 ORDER BY hicd10 DESC) p";
            //ls_sql += " ORDER BY p.hicd10 ASC";

            ls_sql = "SELECT";
            ls_sql += " p.hicd10 as hicd,p.icdcode as nhdm,hicdname hicdname";
            ls_sql += " FROM (SELECT hicd10,icdcode,hicdname,ROW_NUMBER() OVER (ORDER BY hicd10) as rownum from nhppb2 WHERE orgcode=('" + hospitalcode + "') and auditing>0) p";
            ls_sql += " WHERE p.rownum BETWEEN " + startrow.ToString() + " AND " + endrow.ToString();

            //生成数据
            myDs = if0.getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                if0.getRequestXMLStringFromDS(myDs, "hicd:医院疾病代码;nhdm:对应的农合代码", out mess, out data);
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
    /// 城居接口下载本院农合匹配信息
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="startrow"></param>
    /// <param name="endrow"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string getNhppb_cjjk(string areacode, string hospitalcode, int startrow, int endrow, out string data)
    {
        string mess = "";
        data = "";
        //参数有效性检验

        if (startrow < 0 || endrow < 0)
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行都不能小于0。不能下载！");
            return mess;
        }
        else if ((startrow == 0 && endrow != 0) || (startrow != 0 && endrow == 0))
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行必须同时为0时表示获取可下载目录匹配条数");
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
            ls_sql = "SELECT count(*) as medicount FROM nhppb WHERE orgcode=('" + hospitalcode + "') and auditing>0";
            myDs = if0.getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if ((startrow == endrow && startrow == 0))
                    {
                        if0.getRequestXMLStringFromDS(myDs, "得到已审核的匹配数据总条数", out mess, out data);
                        return mess;
                    }
                    if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) == 0)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "没有数据可以下载！");
                        return mess;
                    }
                    if (startrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行超过已审核的匹配数据总条数" + myDs.Tables[0].Rows[0][0].ToString() + "。不能下载！");
                        return mess;
                    }
                    if (endrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        endrow = Convert.ToInt32(myDs.Tables[0].Rows[0][0]);
                    }
                }
                else
                {
                    mess = if0.getXMLErroStrFromString("9000", mess);
                    return mess;
                }
            }
            finally
            {
                myDs.Dispose();
            }

            //ls_sql = "SELECT TOP " + Convert.ToString(endrow - startrow + 1);
            //ls_sql += " p.mediid as mediid,p.servercode as nhdm,mediname as mediname";
            //ls_sql += " FROM (SELECT TOP " + endrow.ToString() + " mediid,servercode,mediname from nhppb WHERE orgcode=('" + hospitalcode + "') and itemtype=0 and auditing>0 ORDER BY mediid DESC) p";
            //ls_sql += " ORDER BY p.mediid ASC";

            ls_sql = "SELECT";
            ls_sql += " p.itemtype as itemtype,p.mediid as mediid,p.medicode as medicode,p.servercode as nhdm,p.nhname as nhname,mediname as mediname";
            ls_sql += " FROM (SELECT itemtype,mediid,medicode,servercode,rcmssermediarea.names as nhname,mediname,ROW_NUMBER() OVER (ORDER BY mediid) as rownum from nhppb,rcmssermediarea  WHERE servercode *= nhcode And orgcode=('" + hospitalcode + "') and auditing>0) p";
            ls_sql += " WHERE p.rownum BETWEEN " + startrow.ToString() + " AND " + endrow.ToString();

            //生成数据
            myDs = if0.getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if0.getRequestXMLStringFromDS(myDs, "itemtype:费用类别;mediid:医院项目ID;medicode:项目ID;nhdm:对应的农合代码;nhname:对应的农合名称;mediname:医院项目名称", out mess, out data);
                    return mess;
                }
                else
                {
                    mess = if0.getXMLErroStrFromString("9000", mess);
                    return mess;
                }
            }
            finally
            {
                myDs.Dispose();
            }
        }
        else
        {
            mess = if0.getXMLErroStrFromString("5000", mess);
            return mess;
        }

    }

    /// <summary>
    /// 城居接口下载目录信息
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="startrow"></param>
    /// <param name="endrow"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string getCatalog_cjjk(string areacode, string hospitalcode, int startrow, int endrow, out string data)
    {
        string mess = "";
        data = "";
        //参数有效性检验

        if (startrow < 0 || endrow < 0)
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行都不能小于0。不能下载！");
            return mess;
        }
        else if ((startrow == 0 && endrow != 0) || (startrow != 0 && endrow == 0))
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行必须同时为0时表示获取可下载目录条数");
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
            ls_sql = "SELECT count(*) as medicount FROM rcmssermediarea_yb WHERE areacode=('" + areacode + "') ";
            myDs = if0.getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if ((startrow == endrow && startrow == 0))
                    {
                        if0.getRequestXMLStringFromDS(myDs, "得到目录数据总条数", out mess, out data);
                        return mess;
                    }
                    if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) == 0)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "没有数据可以下载！");
                        return mess;
                    }
                    if (startrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行超过目录数据总条数" + myDs.Tables[0].Rows[0][0].ToString() + "。不能下载！");
                        return mess;
                    }
                    if (endrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        endrow = Convert.ToInt32(myDs.Tables[0].Rows[0][0]);
                    }
                }
                else
                {
                    mess = if0.getXMLErroStrFromString("9000", mess);
                    return mess;
                }
            }
            finally
            {
                myDs.Dispose();
            }

            //ls_sql = "SELECT TOP " + Convert.ToString(endrow - startrow + 1);
            //ls_sql += " p.mediid as mediid,p.servercode as nhdm,mediname as mediname";
            //ls_sql += " FROM (SELECT TOP " + endrow.ToString() + " mediid,servercode,mediname from nhppb WHERE orgcode=('" + hospitalcode + "') and itemtype=0 and auditing>0 ORDER BY mediid DESC) p";
            //ls_sql += " ORDER BY p.mediid ASC";

            ls_sql = "SELECT";
            ls_sql += " p.areacode,p.typecode,p.nhcode,p.names,p.price1,p.rate1,p.price2,p.rate2,p.price3,p.rate3,p.content,p.excepted";
            ls_sql += " FROM (SELECT areacode,typecode,nhcode,names,price1,rate1,price2,rate2,price3,rate3,content,excepted,ROW_NUMBER() OVER (ORDER BY nhcode) as rownum from rcmssermediarea_yb  WHERE areacode=('" + areacode + "') ) p";
            ls_sql += " WHERE p.rownum BETWEEN " + startrow.ToString() + " AND " + endrow.ToString();

            //生成数据
            myDs = if0.getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if0.getRequestXMLStringFromDS(myDs, "areacode:区域代码;typecode:项目分类代码;nhcode:城居目录代码;names:目录名称;一类价格:price1;一类报销比例:rate1;二类价格:price2;二类报销比例:rate2;三类价格:price3;三类报销比例:rate3;", out mess, out data);
                    return mess;
                }
                else
                {
                    mess = if0.getXMLErroStrFromString("9000", mess);
                    return mess;
                }
            }
            finally
            {
                myDs.Dispose();
            }
        }
        else
        {
            mess = if0.getXMLErroStrFromString("5000", mess);
            return mess;
        }

    }

    /// <summary>
    /// 上传本医院的城居目录匹配数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="dataXML"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public string putCatalog_cjjk(string areacode, string hospitalcode, string dataXML, out string datas)
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
                SqlConnection Connection = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
                try
                {
                    int i, li_cnt;
                    string ls_mediid, ls_medicode, ls_mediname, ls_servercode;
                    int li_auditing;
                    string errtxt = "";
                    bool execresult = true, haverecord = false;
                    XmlNodeList nlis = data.SelectSingleNode("DATA").ChildNodes; //获取DATA节点的所有子节点
                    li_cnt = nlis.Count;
                    if (li_cnt > 999)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "上传数据超过1000行。不予处理！");
                        return mess;
                    }
                    Connection.Open();
                    SqlTransaction myTrans;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = Connection;
                    SqlDataReader myReader;
                    try
                    {
                        for (i = 0; i < li_cnt; i++)
                        {
                            haverecord = false;
                            mess = "";
                            nlis = data.GetElementsByTagName("mediid");
                            ls_mediid = Convert.ToString(nlis[i].InnerText);
                            #region
                            if (ls_mediid == null || ls_mediid.Trim().Length == 0 || !if0.isNumber(ls_mediid))
                            {
                                mess += "mediid必须为数字型非空值";
                            }
                            else
                            {
                                try
                                {
                                    cmd.CommandText = "SELECT Auditing FROM nhppb_yb where orgcode=('" + hospitalcode + "') and mediid=(" + ls_mediid + ")";
                                    li_auditing = Convert.ToInt32(cmd.ExecuteScalar());
                                    myReader = cmd.ExecuteReader();
                                    if (li_auditing > 0)
                                    {
                                        mess += "已审核";
                                    }
                                    else if (myReader.HasRows)
                                    {
                                        haverecord = true;
                                    }
                                    myReader.Close();
                                    myReader.Dispose();
                                }
                                catch (Exception e)
                                {
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + "数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                    return mess;
                                }
                            }
                            #endregion
                            nlis = data.GetElementsByTagName("medicode");
                            ls_medicode = Convert.ToString(nlis[i].InnerText);
                            if (ls_medicode == null || ls_medicode.Trim().Length == 0)
                            {
                                mess += "medicode不能为空";
                            }
                            nlis = data.GetElementsByTagName("mediname");
                            ls_mediname = Convert.ToString(nlis[i].InnerText);
                            if (ls_mediname == null || ls_mediname.Trim().Length == 0)
                            {
                                mess += "mediname不能为空";
                            }
                            nlis = data.GetElementsByTagName("nhdm");
                            ls_servercode = Convert.ToString(nlis[i].InnerText);
                            if (ls_servercode == null || ls_servercode.Trim().Length == 0)
                            {
                                mess += "nhdm不能为空";
                            }
                            else
                            {
                                try
                                {
                                    //cmd.CommandText = "SELECT count(*) FROM rcmsareaserver WHERE areacode=('" + areacode + "') AND servercode=('" + ls_servercode + "') AND chkid=2";
                                    cmd.CommandText = "SELECT count(*) FROM rcmssermediarea_yb WHERE areacode=('" + areacode + "') AND nhcode=('" + ls_servercode + "')";
                                    li_auditing = Convert.ToInt32(cmd.ExecuteScalar());
                                    if (li_auditing == 0)
                                    {
                                        mess += "nhdm值(" + ls_servercode + ")不是城居目录中的代码";
                                    }
                                }
                                catch (Exception e)
                                {
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + "校验nhdm=" + ls_servercode + "的数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
                                    return mess;
                                }
                            }
                            if (mess != "")
                            {
                                execresult = false;
                                errtxt += "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + mess + "</errtxt></ROW>";
                            }
                            else
                            {
                                myTrans = Connection.BeginTransaction();
                                try
                                {
                                    cmd.Transaction = myTrans;
                                    if (haverecord) //已上传的做update
                                    {
                                        cmd.CommandText = "UPDATE nhppb_yb SET medicode='" + ls_medicode + "',mediname='" + ls_mediname + "',servercode='" + ls_servercode + "' WHERE orgcode='" + hospitalcode + "' AND mediid=" + ls_mediid;
                                    }
                                    else
                                    {
                                        cmd.CommandText = "INSERT INTO nhppb_yb(orgcode,itemtype,mediid,medicode,mediname,servercode,Auditing) VALUES('" + hospitalcode + "',1," + ls_mediid + ",'" + ls_medicode + "','" + ls_mediname + "','" + ls_servercode + "',0)";
                                    }
                                    cmd.ExecuteNonQuery();
                                    myTrans.Commit();
                                }
                                catch (Exception e)
                                {
                                    myTrans.Rollback();
                                    mess = if0.getXMLErroStrFromStringData("9000", "存在错误", "<ROW><mediid>" + ls_mediid + "</mediid><errtxt>" + "数据异常：" + e.Message.ToString() + "</errtxt></ROW>", out datas);
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
    /// 从指定的区县农合系统中得到本医院已审核的城居匹配数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="startrow"></param>
    /// <param name="endrow"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string getCatalogSyn_cjjk(string areacode, string hospitalcode, int startrow, int endrow, out string data)
    {
        string mess = "";
        data = "";
        //参数有效性检验

        if (startrow < 0 || endrow < 0)
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行都不能小于0。不能下载！");
            return mess;
        }
        else if ((startrow == 0 && endrow != 0) || (startrow != 0 && endrow == 0))
        {
            mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行和结束行同时为0时，表示获取可下载目录条数");
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
            ls_sql = "SELECT count(*) as medicount FROM nhppb_yb WHERE orgcode=('" + hospitalcode + "')";
            myDs = if0.getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if ((startrow == endrow && startrow == 0))
                    {
                        if0.getRequestXMLStringFromDS(myDs, "得到本院的匹配医疗服务数据总条数", out mess, out data);
                        return mess;
                    }
                    if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) == 0)
                    {
                        mess = if0.getXMLErroStrFromString("2000", "没有数据可以下载！");
                        return mess;
                    }
                    if (startrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        mess = if0.getXMLErroStrFromString("2000", "调用参数的开始行超过匹配医疗服务总条数" + myDs.Tables[0].Rows[0][0].ToString() + "。不能下载！");
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
            //ls_sql += " p.mediid as mediid,p.servercode as nhdm,mediname as mediname";
            //ls_sql += " FROM (SELECT TOP " + endrow.ToString() + " mediid,servercode,mediname from nhppb WHERE orgcode=('" + hospitalcode + "') and itemtype=1 and auditing>0 ORDER BY mediid DESC) p";
            //ls_sql += " ORDER BY p.mediid ASC";

            ls_sql = "SELECT";
            ls_sql += " p.mediid as mediid,p.servercode as nhdm,p.mediname as mediname,p.auditing ";
            ls_sql += " FROM (SELECT mediid,servercode,mediname,auditing,ROW_NUMBER() OVER (ORDER BY mediid) as rownum from nhppb_yb WHERE orgcode=('" + hospitalcode + "')) p";
            ls_sql += " WHERE p.rownum BETWEEN " + startrow.ToString() + " AND " + endrow.ToString();

            //生成数据
            myDs = if0.getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                if0.getRequestXMLStringFromDS(myDs, "mediid:医院项目ID;nhdm:对应的农合代码;mediname:医院项目名称;autiting:审核标志", out mess, out data);
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
    
    #region HISonline接口调用目录相关
    /// <summary>
    /// HIS从农合下载门诊药品材料目录

    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <returns></returns>
    public string getMZCatalogMediFromHIS(string areacode, string hospitalcode, string servername, string dbname, string user, string pwd)
    {
        string hisifurl = "", retMessXML = "", dbConnStr = "", ls_sql = "";
        if (!if0.getWebServiceURL(servername, out hisifurl))
        {
            return if0.getXMLErroStrFromString("2000", hisifurl);
        }
        if (!if0.getDBConnStr("areacode", areacode, out dbConnStr))
        {
            retMessXML = if0.getXMLErroStrFromString("5000", dbConnStr);
            return retMessXML;
        }

        int li_tmp;
        DataSet lds = new DataSet();

        //得到医院级别
        SqlConnection Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
        try
        {
            Connection.Open();
            SqlCommand cmd = new SqlCommand("SELECT levelcode FROM organcode where areacode=('" + areacode + "') and orgcode=('" + hospitalcode + "')", Connection);
            li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
            ls_sql = "SELECT";
            switch (Convert.ToString(li_tmp))
            {
                case "1":
                    ls_sql += " m.medicode as mcode,m.mediname as mname," + Convert.ToString(li_tmp) + " as hosplevel,m.name_py as py," + hospitalcode + " as hospitalcode,m.hosplevel as level,m.price1 as price,m.rate1 as rate,m.medispec as spec,m.usescope as scope";
                    break;
                case "2":
                    ls_sql += " m.medicode as mcode,m.mediname as mname," + Convert.ToString(li_tmp) + " as hosplevel,m.name_py as py," + hospitalcode + " as hospitalcode,m.hosplevel as level,m.price2 as price,m.rate2 as rate,m.medispec as spec,m.usescope as scope";
                    break;
                case "3":
                    ls_sql += " m.medicode as mcode,m.mediname as mname," + Convert.ToString(li_tmp) + " as hosplevel,m.name_py as py," + hospitalcode + " as hospitalcode,m.hosplevel as level,m.price3 as price,m.rate3 as rate,m.medispec as spec,m.usescope as scope";
                    break;
                case "4":
                    ls_sql += " m.medicode as mcode,m.mediname as mname," + Convert.ToString(li_tmp) + " as hosplevel,m.name_py as py," + hospitalcode + " as hospitalcode,m.hosplevel as level,m.price4 as price,m.rate4 as rate,m.medispec as spec,m.usescope as scope";
                    break;
                case "5":
                    ls_sql += " m.medicode as mcode,m.mediname as mname," + Convert.ToString(li_tmp) + " as hosplevel,m.name_py as py," + hospitalcode + " as hospitalcode,m.hosplevel as level,m.price5 as price,m.rate5 as rate,m.medispec as spec,m.usescope as scope";
                    break;
                case "9":
                    ls_sql += " m.medicode as mcode,m.mediname as mname," + Convert.ToString(li_tmp) + " as hosplevel,m.name_py as py," + hospitalcode + " as hospitalcode,m.hosplevel as level,m.price6 as price,m.rate6 as rate,m.medispec as spec,m.usescope as scope";
                    break;
                default:
                    retMessXML = if0.getXMLErroStrFromString("2010", "查找到无效的医院级别，不能获取农合药品目录！");
                    return retMessXML;
            }
            ls_sql += " FROM mz_rcmsmediarea m WHERE areacode=('" + areacode + "') AND chkid=2";

            //按照医院级别生成药品目录
            lds = if0.getDataSet(lds, ls_sql, "t0", out retMessXML);
            try
            {
                if (retMessXML == "TRUE")
                {
                    #region 向HIS接口发送数据

                    if0.putDataToHIS(servername, dbname, user, pwd, lds, "1001", out retMessXML, out lds);
                    return retMessXML;
                    #endregion
                }
                else
                {
                    retMessXML = if0.getXMLErroStrFromString("5000", retMessXML);
                    return retMessXML;
                }
            }
            finally
            {
                cmd.Dispose();
                if (lds != null)
                    lds.Dispose();
            }
        }
        catch (Exception e)
        {
            retMessXML = if0.getXMLErroStrFromString("9000", "查询医院级别异常：" + e.Message.ToString());
            return retMessXML;
        }
        finally
        {
            if (Connection.State == ConnectionState.Open)
                Connection.Close();
        }
    }

    /// <summary>
    /// HIS从农合下载门诊医疗服务目录

    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <returns></returns>
    public string getMZCatalogServerFromHIS(string areacode, string hospitalcode, string servername, string dbname, string user, string pwd)
    {
        string hisifurl = "", retMessXML = "", dbConnStr = "", ls_sql = "";
        if (!if0.getWebServiceURL(servername, out hisifurl))
        {
            return if0.getXMLErroStrFromString("2000", hisifurl);
        }
        if (!if0.getDBConnStr("areacode", areacode, out dbConnStr))
        {
            retMessXML = if0.getXMLErroStrFromString("5000", dbConnStr);
            return retMessXML;
        }

        int li_tmp;
        DataSet lds = new DataSet();

        //得到医院级别
        SqlConnection Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
        try
        {
            Connection.Open();
            SqlCommand cmd = new SqlCommand("SELECT levelcode FROM organcode where areacode=('" + areacode + "') and orgcode=('" + hospitalcode + "')", Connection);
            li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
            ls_sql = "SELECT";
            switch (Convert.ToString(li_tmp))
            {
                case "1":
                    ls_sql += " s.servercode as scode,s.servername as sname,s.name_py as py," + Convert.ToString(li_tmp) + " as hosplevel," + hospitalcode + " as hospitalcode,s.unit as unit,s.price1 as price,s.rate1 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                    break;
                case "2":
                    ls_sql += " s.servercode as scode,s.servername as sname,s.name_py as py," + Convert.ToString(li_tmp) + " as hosplevel," + hospitalcode + " as hospitalcode,s.unit as unit,s.price2 as price,s.rate2 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                    break;
                case "3":
                    ls_sql += " s.servercode as scode,s.servername as sname,s.name_py as py," + Convert.ToString(li_tmp) + " as hosplevel," + hospitalcode + " as hospitalcode,s.unit as unit,s.price3 as price,s.rate3 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                    break;
                case "4":
                    ls_sql += " s.servercode as scode,s.servername as sname,s.name_py as py," + Convert.ToString(li_tmp) + " as hosplevel," + hospitalcode + " as hospitalcode,s.unit as unit,s.price4 as price,s.rate4 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                    break;
                case "5":
                    ls_sql += " s.servercode as scode,s.servername as sname,s.name_py as py," + Convert.ToString(li_tmp) + " as hosplevel," + hospitalcode + " as hospitalcode,s.unit as unit,s.price5 as price,s.rate5 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                    break;
                case "9":
                    ls_sql += " s.servercode as scode,s.servername as sname,s.name_py as py," + Convert.ToString(li_tmp) + " as hosplevel," + hospitalcode + " as hospitalcode,s.unit as unit,s.price6 as price,s.rate6 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                    break;
                default:
                    retMessXML = if0.getXMLErroStrFromString("2010", "查找到无效的医院级别，不能获取农合药品目录！");
                    return retMessXML;
            }
            ls_sql += " FROM mz_rcmsareaserver s WHERE areacode=('" + areacode + "') AND chkid=2";

            //按照医院级别生成药品目录
            lds = if0.getDataSet(lds, ls_sql, "t0", out retMessXML);
            try
            {
                if (retMessXML == "TRUE")
                {
                    #region 向HIS接口发送数据

                    if0.putDataToHIS(servername, dbname, user, pwd, lds, "1002", out retMessXML, out lds);
                    return retMessXML;
                    #endregion
                }
                else
                {
                    retMessXML = if0.getXMLErroStrFromString("5000", retMessXML);
                    return retMessXML;
                }
            }
            finally
            {
                cmd.Dispose();
                if (lds != null)
                    lds.Dispose();
            }
        }
        catch (Exception e)
        {
            retMessXML = if0.getXMLErroStrFromString("9000", "查询医院级别异常：" + e.Message.ToString());
            return retMessXML;
        }
        finally
        {
            if (Connection.State == ConnectionState.Open)
                Connection.Close();
        }
    }
    
    /// <summary>
    /// HIS从农合下载住院药品材料目录

    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <returns></returns>
    public string getZYCatalogMediFromHIS(string areacode, string hospitalcode, string servername, string dbname, string user, string pwd)
    {
        string hisifurl = "", retMessXML = "", dbConnStr = "", ls_sql = "";
        if (!if0.getWebServiceURL(servername, out hisifurl))
        {
            return if0.getXMLErroStrFromString("2000", hisifurl);
        }
        if (!if0.getDBConnStr("areacode", areacode, out dbConnStr))
        {
            retMessXML = if0.getXMLErroStrFromString("5000", dbConnStr);
            return retMessXML;
        }

        int li_tmp;
        DataSet lds = new DataSet();

        //得到医院级别
        SqlConnection Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
        try
        {
            Connection.Open();
            SqlCommand cmd = new SqlCommand("SELECT levelcode FROM organcode where areacode=('" + areacode + "') and orgcode=('" + hospitalcode + "')", Connection);
            li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
            ls_sql = "SELECT";
            switch (Convert.ToString(li_tmp))
            {
                case "1":
                    ls_sql += " m.medicode as mcode,m.mediname as mname,m.name_py as py," + Convert.ToString(li_tmp) + " as hosplevel," + hospitalcode + " as hospitalcode,m.hosplevel as level,m.price3 as price,m.rate3 as rate,m.medispec as spec,m.usescope as scope";
                    break;
                case "2":
                    ls_sql += " m.medicode as mcode,m.mediname as mname,m.name_py as py," + Convert.ToString(li_tmp) + " as hosplevel," + hospitalcode + " as hospitalcode,m.hosplevel as level,m.price2 as price,m.rate2 as rate,m.medispec as spec,m.usescope as scope";
                    break;
                case "3":
                    ls_sql += " m.medicode as mcode,m.mediname as mname,m.name_py as py," + Convert.ToString(li_tmp) + " as hosplevel," + hospitalcode + " as hospitalcode,m.hosplevel as level,m.price1 as price,m.rate1 as rate,m.medispec as spec,m.usescope as scope";
                    break;
                default:
                    retMessXML = if0.getXMLErroStrFromString("2010", "查找到无效的医院级别，不能获取农合药品目录！");
                    return retMessXML;
            }
            ls_sql += " FROM rcmsmediarea m WHERE areacode=('" + areacode + "') AND chkid=2";

            //按照医院级别生成药品目录
            lds = if0.getDataSet(lds, ls_sql,"t0", out retMessXML);
            try
            {
                if (retMessXML == "TRUE")
                {
                    #region 向HIS接口发送数据

                    if0.putDataToHIS(servername, dbname, user, pwd, lds, "1011", out retMessXML, out lds);
                    return retMessXML;
                    #endregion
                }
                else
                {
                    retMessXML = if0.getXMLErroStrFromString("5000", retMessXML);
                    return retMessXML;
                }
            }
            finally
            {
                cmd.Dispose();
                if (lds != null)
                    lds.Dispose();
            }
        }
        catch (Exception e)
        {
            retMessXML = if0.getXMLErroStrFromString("9000", "查询医院级别异常：" + e.Message.ToString());
            return retMessXML;
        }
        finally
        {
            if (Connection.State == ConnectionState.Open)
                Connection.Close();
        }
    }
    
    /// <summary>
    /// HIS从农合下载住院医疗服务目录

    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <returns></returns>
    public string getZYCatalogServerFromHIS(string areacode, string hospitalcode, string servername, string dbname, string user, string pwd)
    {
        string hisifurl = "", retMessXML = "", dbConnStr = "", ls_sql = "";
        if (!if0.getWebServiceURL(servername, out hisifurl))
        {
            return if0.getXMLErroStrFromString("2000", hisifurl);
        }
        if (!if0.getDBConnStr("areacode", areacode, out dbConnStr))
        {
            retMessXML = if0.getXMLErroStrFromString("5000", dbConnStr);
            return retMessXML;
        }

        int li_tmp;
        DataSet lds = new DataSet();

        //得到医院级别
        SqlConnection Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
        try
        {
            Connection.Open();
            SqlCommand cmd = new SqlCommand("SELECT levelcode FROM organcode where areacode=('" + areacode + "') and orgcode=('" + hospitalcode + "')", Connection);
            li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
            ls_sql = "SELECT";
            switch (Convert.ToString(li_tmp))
            {
                case "1":
                    ls_sql += " s.servercode as scode,s.servername as sname,s.name_py as py," + Convert.ToString(li_tmp) + " as hosplevel," + hospitalcode + " as hospitalcode,s.unit as unit,s.price3 as price,s.rate3 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                    break;
                case "2":
                    ls_sql += " s.servercode as scode,s.servername as sname,s.name_py as py," + Convert.ToString(li_tmp) + " as hosplevel," + hospitalcode + " as hospitalcode,s.unit as unit,s.price2 as price,s.rate2 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                    break;
                case "3":
                    ls_sql += " s.servercode as scode,s.servername as sname,s.name_py as py," + Convert.ToString(li_tmp) + " as hosplevel," + hospitalcode + " as hospitalcode,s.unit as unit,s.price1 as price,s.rate1 as rate,s.content as content,s.excepted as notin,s.remark as memo";
                    break;
                default:
                    retMessXML = if0.getXMLErroStrFromString("2010", "查找到无效的医院级别，不能获取农合药品目录！");
                    return retMessXML;
            }
            ls_sql += " FROM rcmsareaserver s WHERE areacode=('" + areacode + "') AND chkid=2";
            
            //按照医院级别生成药品目录
            lds = if0.getDataSet(lds, ls_sql, "t0", out retMessXML);
            try
            {
                if (retMessXML == "TRUE")
                {
                    #region 向HIS接口发送数据

                    if0.putDataToHIS(servername, dbname, user, pwd, lds, "1012", out retMessXML, out lds);
                    return retMessXML;
                    #endregion
                }
                else
                {
                    retMessXML = if0.getXMLErroStrFromString("5000", retMessXML);
                    return retMessXML;
                }
            }
            finally
            {
                cmd.Dispose();
                if (lds != null)
                    lds.Dispose();
            }
        }
        catch (Exception e)
        {
            retMessXML = if0.getXMLErroStrFromString("9000", "查询医院级别异常：" + e.Message.ToString());
            return retMessXML;
        }
        finally
        {
            if (Connection.State == ConnectionState.Open)
                Connection.Close();
        }
    }

    /// <summary>
    /// HIS从农合下载住院ICD10目录
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <returns></returns>
    public string getCatalogICD10HIS(string areacode, string hospitalcode, string servername, string dbname, string user, string pwd)
    {
        string hisifurl = "", retMessXML = "", ls_sql = "";
        if (!if0.getWebServiceURL(servername, out hisifurl))
        {
            return if0.getXMLErroStrFromString("2000", hisifurl);
        }

        DataSet lds = new DataSet();

        ls_sql = "SELECT i.icdcode as icode,i.icdname as iname,i.memo as memo,i.pym as py FROM nhicd10 i";

        //按照医院级别生成药品目录
        lds = if0.getDataSet(lds, ls_sql, "t0", out retMessXML);
        try
        {
            if (retMessXML == "TRUE")
            {
                #region 向HIS接口发送数据

               if0.putDataToHIS(servername, dbname, user, pwd, lds, "1021", out retMessXML, out lds);
                return retMessXML;
                #endregion
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

    /// <summary>
    /// HIS从农合下载住院匹配审核数据

    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <returns></returns>
    public string getZYPPHIS(string areacode, string hospitalcode, string servername, string dbname, string user, string pwd)
    {
        string hisifurl = "", retMessXML = "", ls_sql = "";
        if (!if0.getWebServiceURL(servername, out hisifurl))
        {
            return if0.getXMLErroStrFromString("2000", hisifurl);
        }

        DataSet lds = new DataSet();

        ls_sql = "SELECT p.mediid as mediid,p.servercode as nhdm,mediname as mediname";
        ls_sql += " FROM nhppb p WHERE orgcode=('" + hospitalcode + "') AND auditing>0";

        //按照医院级别生成药品目录
        lds = if0.getDataSet(lds, ls_sql, "t0", out retMessXML);
        try
        {
            if (retMessXML == "TRUE")
            {
                #region 向HIS接口发送数据

                if0.putDataToHIS(servername, dbname, user, pwd, lds, "1102", out retMessXML, out lds);
                return retMessXML;
                #endregion
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

    /// <summary>
    /// HIS从农合下载门诊匹配审核数据

    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <returns></returns>
    public string getMZPPHIS(string areacode, string hospitalcode, string servername, string dbname, string user, string pwd)
    {
        string hisifurl = "", retMessXML = "", ls_sql = "";
        if (!if0.getWebServiceURL(servername, out hisifurl))
        {
            return if0.getXMLErroStrFromString("2000", hisifurl);
        }

        DataSet lds = new DataSet();

        ls_sql = "SELECT p.mediid as mediid,p.servercode as nhdm,mediname as mediname";
        ls_sql += " FROM mz_nhppb p WHERE orgcode=('" + hospitalcode + "') AND auditing>0";

        //按照医院级别生成药品目录
        lds = if0.getDataSet(lds, ls_sql, "t0", out retMessXML);
        try
        {
            if (retMessXML == "TRUE")
            {
                #region 向HIS接口发送数据

                if0.putDataToHIS(servername, dbname, user, pwd, lds, "1104", out retMessXML, out lds);
                return retMessXML;
                #endregion
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

    /// <summary>
    /// HIS从农合下载ICD10匹配审核数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <returns></returns>
    public string getNHICD10PPHIS(string areacode, string hospitalcode, string servername, string dbname, string user, string pwd)
    {
        string hisifurl = "", retMessXML = "", ls_sql = "";
        if (!if0.getWebServiceURL(servername, out hisifurl))
        {
            return if0.getXMLErroStrFromString("2000", hisifurl);
        }

        DataSet lds = new DataSet();

        ls_sql = "SELECT p.hicd10 as hicd,p.icdcode as nhdm,hicdname hicdname";
        ls_sql += " FROM nhppb2 p WHERE orgcode=('" + hospitalcode + "') and auditing>0";

        //按照医院级别生成药品目录
        lds = if0.getDataSet(lds, ls_sql, "t0", out retMessXML);
        try
        {
            if (retMessXML == "TRUE")
            {
                #region 向HIS接口发送数据

                if0.putDataToHIS(servername, dbname, user, pwd, lds, "1103", out retMessXML, out lds);
                return retMessXML;
                #endregion
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

    /// <summary>
    /// HIS向农合上传住院匹配数据

    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <returns></returns>
    public string putZYPPHIS(string areacode, string hospitalcode, string servername, string dbname, string user, string pwd)
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
        DataSet lds = new DataSet();
        ls_sql = "SELECT p.mediid as mediid,p.medicode as medicode,p.mediname as mediname,p.nhdm as nhdm,p.itemtype as itemtype";
        ls_sql += " FROM " + dbname + "..nhppb p WHERE isnull(p.Auditing,0)=0 AND isnull(p.nhdm,'')<>''";
        if (if0.getDataFromHIS(servername, dbname, user, pwd, ls_sql, out retMessXML, out lds))
        {
            //调用成功，处理传出的DataSet
            retMessXML = "";
            #region 查询数据库Insert或Update数据
            if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
            {
                Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
                try
                {
                    Connection.Open();
                    cmd = new SqlCommand();
                    cmd.Connection = Connection;
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
                                    cmd.CommandText = "SELECT count(*) FROM nhppb WHERE orgcode=('" + hospitalcode + "') AND mediid=(" + lds.Tables[0].Rows[i]["mediid"].ToString() + ")";
                                    li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
                                    if (li_tmp > 0)
                                    {
                                        haverecord = true;
                                    }
                                }
                                catch (Exception e)
                                {
                                    return if0.getXMLErroStrFromString("9000", "农合数据库操作异常：" + e.Message.ToString());
                                }
                                #endregion
                                if (haverecord)
                                {
                                    #region 生成做UPDATE的SQL
                                    ls_sql = "UPDATE nhppb SET itemtype=" + lds.Tables[0].Rows[i]["itemtype"].ToString() + ",medicode='" + if0.escapeSQLString(lds.Tables[0].Rows[i]["medicode"].ToString()) + "'";
                                    ls_sql += ",mediname='" + if0.escapeSQLString(lds.Tables[0].Rows[i]["mediname"].ToString()) + "',servercode='" + lds.Tables[0].Rows[i]["nhdm"].ToString() + "'";
                                    ls_sql += " WHERE orgcode=('" + hospitalcode + "') AND mediid=(" + lds.Tables[0].Rows[i]["mediid"].ToString() + ") AND Auditing=0";
                                    #endregion
                                }
                                else
                                {
                                    #region 生成做INSERT的SQL
                                    ls_sql = "INSERT INTO nhppb(orgcode,itemtype,mediid,medicode,mediname,servercode,Auditing)";
                                    ls_sql += " VALUES('" + hospitalcode + "'," + lds.Tables[0].Rows[i]["itemtype"].ToString() + "," + lds.Tables[0].Rows[i]["mediid"].ToString() + ",'" + if0.escapeSQLString(lds.Tables[0].Rows[i]["medicode"].ToString()) + "'";
                                    ls_sql += ",'" + if0.escapeSQLString(lds.Tables[0].Rows[i]["mediname"].ToString()) + "','" + lds.Tables[0].Rows[i]["nhdm"].ToString() + "',0";
                                    ls_sql += ")";
                                    #endregion
                                }
                                #region //保存HIS向农合上传住院匹配数据

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
                                    return if0.getXMLErroStrFromString("9000", "HIS向农合上传住院匹配数据时异常：" + e.Message.ToString());
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
                            return if0.getXMLErroStrFromString("9000", "生成HIS向农合上传住院匹配数据的语句时异常：" + e.Message.ToString());
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
            retMessXML = if0.getXMLStrFromString("HIS向农合上传住院匹配数据传输成功！");
        }
        return retMessXML;
    }

    /// <summary>
    /// HIS向农合上传门诊匹配数据

    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <returns></returns>
    public string putMZPPHIS(string areacode, string hospitalcode, string servername, string dbname, string user, string pwd)
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
        DataSet lds = new DataSet();
        ls_sql = "SELECT p.mediid as mediid,p.medicode as medicode,p.mediname as mediname,p.nhdm as nhdm,p.itemtype as itemtype";
        ls_sql += " FROM " + dbname + "..mz_nhppb p WHERE isnull(p.Auditing,0)=0 AND isnull(p.nhdm,'')<>''";
        if (if0.getDataFromHIS(servername, dbname, user, pwd, ls_sql, out retMessXML, out lds))
        {
            //调用成功，处理传出的DataSet
            retMessXML = "";
            #region 查询数据库Insert或Update数据
            if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
            {
                Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
                try
                {
                    Connection.Open();
                    cmd = new SqlCommand();
                    cmd.Connection = Connection;
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
                                    cmd.CommandText = "SELECT count(*) FROM mz_nhppb WHERE orgcode=('" + hospitalcode + "') AND mediid=(" + lds.Tables[0].Rows[i]["mediid"].ToString() + ")";
                                    li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
                                    if (li_tmp > 0)
                                    {
                                        haverecord = true;
                                    }
                                }
                                catch (Exception e)
                                {
                                    return if0.getXMLErroStrFromString("9000", "农合数据库操作异常：" + e.Message.ToString());
                                }
                                #endregion
                                if (haverecord)
                                {
                                    #region 生成做UPDATE的SQL
                                    ls_sql = "UPDATE mz_nhppb SET itemtype=" + lds.Tables[0].Rows[i]["itemtype"].ToString() + ",medicode='" + if0.escapeSQLString(lds.Tables[0].Rows[i]["medicode"].ToString()) + "'";
                                    ls_sql += ",mediname='" + if0.escapeSQLString(lds.Tables[0].Rows[i]["mediname"].ToString()) + "',servercode='" + lds.Tables[0].Rows[i]["nhdm"].ToString() + "'";
                                    ls_sql += " WHERE orgcode=('" + hospitalcode + "') AND mediid=(" + lds.Tables[0].Rows[i]["mediid"].ToString() + ") AND Auditing=0";
                                    #endregion
                                }
                                else
                                {
                                    #region 生成做INSERT的SQL
                                    ls_sql = "INSERT INTO mz_nhppb(orgcode,itemtype,mediid,medicode,mediname,servercode,Auditing)";
                                    ls_sql += " VALUES('" + hospitalcode + "'," + lds.Tables[0].Rows[i]["itemtype"].ToString() + "," + lds.Tables[0].Rows[i]["mediid"].ToString() + ",'" + if0.escapeSQLString(lds.Tables[0].Rows[i]["medicode"].ToString()) + "'";
                                    ls_sql += ",'" + if0.escapeSQLString(lds.Tables[0].Rows[i]["mediname"].ToString()) + "','" + lds.Tables[0].Rows[i]["nhdm"].ToString() + "',0";
                                    ls_sql += ")";
                                    #endregion
                                }
                                #region //保存HIS向农合上传住院匹配数据

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
                                    return if0.getXMLErroStrFromString("9000", "HIS向农合上传门诊匹配数据时异常：" + e.Message.ToString());
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
                            return if0.getXMLErroStrFromString("9000", "生成HIS向农合上传门诊匹配数据的语句时异常：" + e.Message.ToString());
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
            retMessXML = if0.getXMLStrFromString("HIS向农合上传门诊匹配数据传输成功！");
        }
        return retMessXML;
    }

    /// <summary>
    /// HIS向农合上传ICD10匹配数据
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <returns></returns>
    public string putNHICD10PPHIS(string areacode, string hospitalcode, string servername, string dbname, string user, string pwd)
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
        DataSet lds = new DataSet();
        ls_sql = "SELECT p.hicd10 as hicd10,p.icdname as icdname,p.icdcode as icdcode";
        ls_sql += " FROM " + dbname + "..rcms_icdoperate p WHERE isnull(p.Auditing,0)=0 AND isnull(p.icdcode,'')<>''";
        if (if0.getDataFromHIS(servername, dbname, user, pwd, ls_sql, out retMessXML, out lds))
        {
            //调用成功，处理传出的DataSet
            retMessXML = "";
            #region 查询数据库Insert或Update数据
            if (if0.getDBConnStr("areacode", areacode, out dbConnStr))
            {
                Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
                try
                {
                    Connection.Open();
                    cmd = new SqlCommand();
                    cmd.Connection = Connection;
                    try
                    {
                        SqlTransaction myTrans;
                        #region 生成并执行相关SQL
                        try
                        {
                            for (i = 0; i < lds.Tables[0].Rows.Count; i++)
                            {
                                #region //费用ID号存在性验证

                                try
                                {
                                    haverecord = false;
                                    cmd.CommandText = "SELECT count(*) FROM nhppb2 WHERE orgcode=('" + hospitalcode + "') AND hicd10=('" + if0.escapeSQLString(lds.Tables[0].Rows[i]["hicd10"].ToString()) + "')";
                                    li_tmp = Convert.ToInt32(cmd.ExecuteScalar());
                                    if (li_tmp > 0)
                                    {
                                        haverecord = true;
                                    }
                                }
                                catch (Exception e)
                                {
                                    return if0.getXMLErroStrFromString("9000", "农合数据库操作异常：" + e.Message.ToString());
                                }
                                #endregion
                                if (haverecord)
                                {
                                    #region 生成做UPDATE的SQL
                                    ls_sql = "UPDATE nhppb2 SET hicdname='" + if0.escapeSQLString(lds.Tables[0].Rows[i]["icdname"].ToString()) + "',icdcode='" + if0.escapeSQLString(lds.Tables[0].Rows[i]["icdcode"].ToString()) + "'";
                                    ls_sql += " WHERE orgcode=('" + hospitalcode + "') AND hicd10=('" + if0.escapeSQLString(lds.Tables[0].Rows[i]["hicd10"].ToString()) + "') AND Auditing=0";
                                    #endregion
                                }
                                else
                                {
                                    #region 生成做INSERT的SQL
                                    ls_sql = "INSERT INTO nhppb2(orgcode,hicd10,hicdname,icdcode,Auditing)";
                                    ls_sql += " VALUES('" + hospitalcode + "','" + if0.escapeSQLString(lds.Tables[0].Rows[i]["hicd10"].ToString()) + "','" + if0.escapeSQLString(lds.Tables[0].Rows[i]["icdname"].ToString()) + "','" + if0.escapeSQLString(lds.Tables[0].Rows[i]["icdcode"].ToString()) + "',0";
                                    ls_sql += ")";
                                    #endregion
                                }
                                #region //保存HIS向农合上传住院匹配数据

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
                                    return if0.getXMLErroStrFromString("9000", "HIS向农合上传ICD10匹配数据时异常：" + e.Message.ToString());
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
                            return if0.getXMLErroStrFromString("9000", "生成HIS向农合上传ICD10匹配数据的语句时异常：" + e.Message.ToString());
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
            retMessXML = if0.getXMLStrFromString("HIS向农合上传ICD10匹配数据传输成功！");
        }
        return retMessXML;
    }
    #endregion

}
