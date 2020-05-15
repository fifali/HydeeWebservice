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
using System.Text;

[WebService(Namespace = "rdhl.hn.ncms")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class ncmsif : System.Web.Services.WebService
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
    #endregion 
    
    #region 构造函数
    /// <summary>
    /// 构造函数
    /// </summary>
    public ncmsif()
    {
        //如果使用设计的组件，请取消注释以下行 
        //InitializeComponent(); 
    }

    /// <summary>
    /// 构造函数，初始化类变量
    /// </summary>
    public ncmsif(string dbServer, string dbName, string userID, string passWord, string dbConnString, bool validUser, string areaCode, int interFacePower)
    {
        //DBServer,DBName,UserID,PassWord,DBConnStr,IsValidUser,AreaCode,InterFacePower
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
    ~ncmsif()
    {
        Dispose();
    }
    #endregion

    #region 数据库联接等类变量相关
    /// <summary>
    /// 获取数据库联接参数串
    /// </summary>
    /// <param name="areacode"></param>
    /// <returns></returns>
    public bool getConnParms(string areacode, out string mess)
    {
        if (AreaCode==areacode && DBServer != "")
        {
            mess = DBConnStr+";Password=" + PassWord;
            return true;
        }
        try
        {
            //创建一个XmlTextReader类的对象并调用Read方法来读取XML文件
            XmlTextReader txtReader = new XmlTextReader(Server.MapPath("./DBConn/" + areacode + ".xml"));
            txtReader.Read();
            txtReader.Read();
            //找到符合的节点获取需要的属性值
            while (txtReader.Read())
            {
                txtReader.MoveToElement();
                if (txtReader.Name == "area")
                {
                    if (txtReader.GetAttribute("code") == areacode)
                    {
                        DBServer = txtReader.GetAttribute("DBServer");
                        DBName = txtReader.GetAttribute("DBName");
                        UserID = txtReader.GetAttribute("UserID");
                        PassWord = txtReader.GetAttribute("PassWord");
                        break;
                    }
                    if (txtReader.NodeType.ToString() == "EndElement")
                    {
                        break;
                    }
                }
            }
            if (DBServer == "")
            {
                mess = "获取区域" + areacode + "的数据库联接参数错误，请检查配置文件！";
                return false;
            }
            else
            {
                mess = "Data Source=" + DBServer + ";Initial Catalog=" + DBName + ";User ID=" + UserID + ";Password=" + PassWord + ";Application Name=HNNCMS WebService Connection Pooling;Pooling=true;Min Pool Size=1;Max Pool Size=100;Connect timeout = 30";
                return true;
            }
        }
        catch (Exception e)
        {
            if (e.Message.ToString().Contains("未能找到文件"))
            {
                mess = "服务器没有找到区域" + areacode + "的数据库联接配置参数！";
            }
            else
            {
                mess = "获取区域" + areacode + "的数据库联接参数错误：" + e.Message.ToString();
            }
            return false;
        }
    }

    /// <summary>
    /// 根据给定的参数类型获得数据库联接串
    /// </summary>
    /// <param name="DataType"></param>
    /// <param name="Data"></param>
    /// <param name="mess"></param>
    /// <returns></returns>
    public bool getDBConnStr(string DataType, string Data, out string mess)
    {
        try
        {
            //string ConnStr = "Provider=SQLOLEDB.1;Persist Security Info=True;User ID=sa;Password=peds2006;Initial Catalog=cd;Data Source=rdhl";
            if (DBConnStr != "")
            {
                mess = DBConnStr;
                return true;
            }
            else
            {
                switch (DataType)
                {
                    case "areacode":
                        AreaCode = Data;
                        break;
                    case "hospitalcode":
                        AreaCode = Data.Substring(0, 6);
                        break;
                    case "bookcard":
                        AreaCode = Data.Substring(0, 6);
                        break;
                    default:
                        mess = "指定的区域型数据类型错误，无法去联接数据库！";
                        return false;
                }
                if (getConnParms(AreaCode, out mess))
                {
                    DBConnStr = "Provider=SQLOLEDB.1;Persist Security Info=False;" + mess;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            DBConnStr = "";
            mess = "寻址数据库异常："+e.Message.ToString();
            return false;
        }
    }

    /// <summary>
    /// 得到当前用户是否有效
    /// </summary>
    /// <returns></returns>
    public bool getUserState()
    {
        if (IsValidUser)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 设置当前用户是否有效
    /// </summary>
    /// <param name="IfValid"></param>
    /// <returns></returns>
    public bool setUserState(bool IfValid)
    {
        IsValidUser = IfValid;
        return IfValid;
    }
    #endregion

    #region 基础方法

    public Int32 getStrLength(string str)
    {
        return Encoding.GetEncoding("utf-8").GetBytes(str).Length;

    }
    public string getSubstring(string str, int strLength)
    {
        byte[] bwrite = Encoding.GetEncoding("utf-8").GetBytes(str.ToCharArray()); 
       if(bwrite.Length >= strLength)
           return Encoding.Default.GetString(bwrite, 0, strLength);
       else return  Encoding.Default.GetString(bwrite);
    }

    /// <summary>
    /// 判断字符串是否数字型
    /// </summary>
    /// <param name="strNumber"></param>
    /// <returns></returns>
    public bool isNumber(string strNumber)
    {
        //string strValidRealPattern="^([-]|[.]|[-.]|[0-9])[0-9]*[.]*[0-9]+$"; 
        //Regex objNumberPattern = new Regex(strValidRealPattern); 
        //return objNumberPattern.IsMatch(strNumber); 
        try
        {
            double iRet = Convert.ToDouble(strNumber);
        }
        catch//(Exception e)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 判断字符串是否指定的datetime型格式
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public bool isDateTime(string str)
    {        
        try
        {
            //"2009-12-07 13:36:49"
            DateTime.Parse(str.Substring(0, 4) + '-' + str.Substring(5, 2) + '-' + str.Substring(8, 2) + ' ' + str.Substring(11, 2) + ':' + str.Substring(14, 2) + ':' + str.Substring(17, 2));
        }
        catch
        {
            return false;
        }
        return true;
    }

    public bool isDate(string str)
    {
        try
        {
            switch (str.Length)
            {
                case 0:
                    return false;
                case 8:
                    DateTime.Parse(str.Substring(0, 4) + '-' + str.Substring(5, 2) + '-' + str.Substring(8, 2));
                    return true;
                case 10:
                    DateTime.Parse(str);
                    return true;
                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
                

    }

    /// <summary>
    /// 验证参数值是否有效
    /// </summary>
    /// <param name="parmvalue"></param>
    /// <returns></returns>
    public bool isValidParm(string parmvalue)
    {
        if (parmvalue.Length > 0)
        {
            if (parmvalue.ToLower().IndexOf(" or ") > 0 || parmvalue.ToLower().IndexOf(")or") > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return true;
        }
    }
    
    /// <summary>
    /// SQL中使用如果字段值中含有单引号，则使用转义符
    /// 如：直接抗人球蛋白试验(Coombs') ——在SQL中要转为   直接抗人球蛋白试验(Coombs'')
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public string escapeSQLString(string column)
    {
        if (column.Length == 0 || column.IndexOf("'") == -1)
        {
            return column;
        }
        string ls_tmp = column, ls_tmp2;
        while (ls_tmp.IndexOf("'") != -1)
        {
            if (ls_tmp.IndexOf("'") == 0)
            {
                ls_tmp2 = "";
            }
            else
            {
                ls_tmp2 = ls_tmp.Substring(0, ls_tmp.IndexOf("'") - 1);
            }
            ls_tmp = ls_tmp.Substring(ls_tmp.IndexOf("'") + 1);
            column = ls_tmp2 + "''" + ls_tmp;
        }
        return column;
    }

    /// <summary>
    /// 在构造XML之前，将字符串中包含的XML非规范字符使用转义字符替换。暂不使用
    /// </summary>
    /// <param name="xml"></param>
    /// <returns></returns>
    public string ConvertXmlString(string str)
    {
        for (int intNext = 0; true; )
        {
            int intIndexOf = str.IndexOf("&", intNext);
            if (intIndexOf <= 0)
            {
                break;
            }
            else
            {
                str = str.Substring(0, intIndexOf) + "&amp;" + str.Substring(intIndexOf + 1);
            }
        }
        for (; true; )
        {
            int intIndexOf = str.IndexOf("<");
            if (intIndexOf <= 0)
            {
                break;
            }
            else
            {
                str = str.Substring(0, intIndexOf) + "&lt;" + str.Substring(intIndexOf + 1);
            }
        }
        for (; true; )
        {
            int intIndexOf = str.IndexOf(">");
            if (intIndexOf <= 0)
            {
                break;
            }
            else
            {
                str = str.Substring(0, intIndexOf) + "&gt;" + str.Substring(intIndexOf + 1);
            }
        }
        for (; true; )
        {
            int intIndexOf = str.IndexOf("\"");
            if (intIndexOf <= 0)
            {
                break;
            }
            else
            {
                str = str.Substring(0, intIndexOf) + "&quot;" + str.Substring(intIndexOf + 1);
            }
        }
        return str;
    }

    /// <summary>
    /// 在指定的DataSet中添加DataTable
    /// </summary>
    /// <param name="ds"></param>
    /// <param name="sqlStr"></param>
    /// <param name="tabName"></param>
    /// <param name="mess"></param>
    /// <returns></returns>
    public DataSet getDataSet(DataSet ds, string sqlStr, string tabName, out string mess)
    {
        if (ds == null || sqlStr.Length == 0 || tabName.Length == 0)
        {
            mess = "数据集中添加表的参数错误！";
            return ds;
        }
        string ls_mess = "";
        if (getDBConnStr("areacode", AreaCode, out ls_mess))
        {
            try
            {
                //OleDbConnection myConn = new OleDbConnection(ls_mess);
                //myConn.Open();
                //OleDbDataAdapter myOda = new OleDbDataAdapter(sqlStr, myConn);
                //ds.EnforceConstraints = false;
                //ds.Tables.Add(new DataTable(tabName));
                //ds.Tables[tabName].BeginLoadData();//调用BeginLoadData方法来优化性能
                //myOda.Fill(ds, tabName); //填充DataSet
                //ds.Tables[tabName].EndLoadData();
                ////关闭此OleDbConnection
                //myConn.Close(); //注意及时关闭连接
                ////feeDs.Tables["fees"].PrimaryKey = new DataColumn[] { feeDs.Tables["fees"].Columns["hospitalcode"], feeDs.Tables["fees"].Columns["HospCode"], feeDs.Tables["fees"].Columns["detailid"] };//建立一个主键
                //myOda.Dispose();
                //mess = "TRUE";
                //return ds;

                SqlConnection myConn = new SqlConnection(ls_mess.Replace("Provider=SQLOLEDB.1;", ""));
                myConn.Open();
                SqlDataAdapter myDA = new SqlDataAdapter(sqlStr, myConn);
                myDA.Fill(ds, tabName);
                myConn.Close(); //注意及时关闭连接
                myDA.Dispose();
                mess = "TRUE";
                return ds;
            }
            catch (Exception e)
            {
                mess = "生成数据集异常：" + e.Message.ToString();
                return ds;
            }
        }
        else
        {
            mess = ls_mess;
            return ds;
        }
    }

    /// <summary>
    /// 根据指定的SQL和指定的表名返回DataSet
    /// </summary>
    /// <param name="sqlStr"></param>
    /// <param name="tabName"></param>
    /// <param name="mess"></param>
    /// <returns></returns>
    public DataSet getDataSet(string sqlStr, string tabName, out string mess)
    {
        mess = "";
        DataSet myDataSet = new DataSet();
        try
        {
            //string ls_mess = "";
            if (sqlStr.Length == 0)
            {
                mess = "没有SQL，无法生成数据集!";
                return null;
            }
            if (tabName == "")
            {
                tabName = "Table0";
            }
            myDataSet = getDataSet(myDataSet, sqlStr, tabName, out mess);
            if (mess != "TRUE")
            {
                return null;
            }
            else
            {
                return myDataSet;
            }
        }
        finally
        {
            myDataSet.Dispose();
        }
    }

    /// <summary>
    /// 根据指定的SQL返回DataSet
    /// </summary>
    /// <param name="sqlStr"></param>
    /// <param name="mess"></param>
    /// <returns></returns>
    public DataSet getDataSet(string sqlStr, out string mess)
    {
        return getDataSet(sqlStr, "Table0", out mess);
    }

    /// <summary>
    /// 根据DataSet得到XML中<ROW></ROW>格式串
    /// </summary>
    /// <param name="a_ds"></param>
    /// <param name="rowxml"></param>
    /// <returns></returns>
    public bool getXMLRowStringFromDS(DataSet a_ds, out string rowxml)
    {
        rowxml = "";
        int i, j; 
        try
        {
            for (i = 0; i < a_ds.Tables[0].Rows.Count; i++)
            {
                rowxml += "<ROW>";
                for (j = 0; j < a_ds.Tables[0].Columns.Count; j++)
                {
                    if (a_ds.Tables[0].Rows[i][j] == null)
                    {
                        rowxml += "<" + a_ds.Tables[0].Columns[j].ColumnName + "></" + a_ds.Tables[0].Columns[j].ColumnName + ">";
                    }
                    else
                    {
                        rowxml += "<" + a_ds.Tables[0].Columns[j].ColumnName + "><![CDATA[" + a_ds.Tables[0].Rows[i][j].ToString() + "]]></" + a_ds.Tables[0].Columns[j].ColumnName + ">";
                    }
                }
                rowxml += "</ROW>";
            }
            return true;
        }
        catch (Exception e)
        {
            rowxml = "数据集生成XML异常：" + e.Message.ToString();
            return false;
        }
    }

    /// <summary>
    /// 根据DataSet得到XML格式字符串
    /// </summary>
    /// <param name="a_ds"></param>
    /// <param name="as_memo"></param>
    /// <param name="mess"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool getRequestXMLStringFromDS(DataSet a_ds, string as_memo, out string mess, out string data)
    {
        mess = "<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "UTF-8" + '"' + "?>";
        string xmltmp = "";
        string execresult = "";
        string execresulterrtxt = "";
        if (getXMLRowStringFromDS(a_ds, out xmltmp))
        {
            if (xmltmp != "")
            {
                data = "<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "UTF-8" + '"' + "?>";
                data += "<DATA>";
                data += xmltmp;
                data += "</DATA>";
            }
            else
            {
                data = "";
            }
        }
        else
        {
            execresult = "FALSE";
            execresulterrtxt = "数据集生成XML异常：" + xmltmp;
            mess += "<DESC><ROW>";
            mess += "<RESULT>" + execresult + "</RESULT>";
            mess += "<ERRNO>9000</ERRNO>";
            mess += "<ERRO>" + execresulterrtxt + "</ERRO>";
            mess += "<MEMO>" + as_memo + "</MEMO>";
            mess += "</ROW></DESC>";
            data = "";
            return false;
        }
        execresult = "TRUE";
        mess += "<DESC><ROW>";
        mess += "<RESULT>" + execresult + "</RESULT>";
        mess += "<ERRNO>0000</ERRNO>";
        mess += "<ERRO><![CDATA[" + execresulterrtxt + "]]></ERRO>";
        if (data == "")
        {
            mess += "<MEMO><![CDATA[" + as_memo + "(没有数据)]]></MEMO>";
        }
        else
        {
            mess += "<MEMO><![CDATA[" + as_memo + "]]></MEMO>";
        }
        mess += "</ROW></DESC>";
        return true;
    }

    /// <summary>
    /// 从错误描述字符串生成返回给用户的错误描述XML字符串
    /// </summary>
    /// <param name="errotxt"></param>
    /// <returns></returns>
    public string getXMLErroStrFromString(string errno, string errotxt)
    {
        string xmlstr = "<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "UTF-8" + '"' + "?>";
        xmlstr += "<DESC><ROW>";
        xmlstr += "<RESULT>FALSE</RESULT>";
        xmlstr += "<ERRNO>" + errno + "</ERRNO>";
        xmlstr += "<ERRO><![CDATA[" + errotxt + "]]></ERRO>";
        xmlstr += "<MEMO></MEMO>";
        xmlstr += "</ROW></DESC>";
        return xmlstr;
    }

    /// <summary>
    /// 从错误描述字符串和错误数据字符串生成返回给用户的错误描述XML字符串
    /// </summary>
    /// <param name="errotxt"></param>
    /// <param name="errodata"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string getXMLErroStrFromStringData(string errno, string errotxt, string errodata, out string data)
    {
        string xmlstr = "<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "UTF-8" + '"' + "?>";
        xmlstr += "<DESC><ROW>";
        xmlstr += "<RESULT>FALSE</RESULT>";
        xmlstr += "<ERRNO>" + errno + "</ERRNO>";
        xmlstr += "<ERRO><![CDATA[" + errotxt + "]]></ERRO>";
        xmlstr += "<MEMO></MEMO>";
        xmlstr += "</ROW></DESC>";
        if (errodata != "")
        {
            data = "<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "UTF-8" + '"' + "?>";
            data += "<DATA>";
            data += errodata;//errodata必须是<ROW>...</ROW><ROW>...</ROW>的形式
            data += "</DATA>";
        }
        else
        {
            data = "";
        }
        return xmlstr;
    }

    /// <summary>
    /// 从字符串生成返回给用户的XML描述字符串
    /// </summary>
    /// <param name="memo"></param>
    /// <returns></returns>
    public string getXMLStrFromString(string memo)
    {
        string xmlstr = "<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "UTF-8" + '"' + "?>";
        xmlstr += "<DESC><ROW>";
        xmlstr += "<RESULT>TRUE</RESULT>";
        xmlstr += "<ERRNO>0000</ERRNO>";
        xmlstr += "<ERRO></ERRO>";
        xmlstr += "<MEMO><![CDATA[" + memo + "]]></MEMO>";
        xmlstr += "</ROW></DESC>";
        return xmlstr;
    }

    /// <summary>
    /// 从根据输入的数据字符串生成返回给用户的XML字符串和数据XML
    /// </summary>
    /// <param name="indata"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string getXMLStrFromStringData(string indata, out string data)
    {
        string xmlstr = "<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "UTF-8" + '"' + "?>";
        xmlstr += "<DESC><ROW>";
        xmlstr += "<RESULT>TRUE</RESULT>";
        xmlstr += "<ERRNO>0000</ERRNO>";
        xmlstr += "<ERRO></ERRO>";
        xmlstr += "<MEMO></MEMO>";
        xmlstr += "</ROW></DESC>";
        if (indata != "")
        {
            data = "<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "UTF-8" + '"' + "?>";
            data += "<DATA>";
            data += indata;
            data += "</DATA>";
        }
        else
        {
            data = "";
        }
        return xmlstr;
    }

    /// <summary>
    /// 通过XML格式字符串获得XMLDocument对象
    /// </summary>
    /// <param name="as_str"></param>
    /// <param name="mess"></param>
    /// <returns></returns>
    public XmlDocument getXMLDocumentFromString(string as_str, out string mess)
    {
        if (as_str.Length == 0)
        {
            mess = "XML格式字符串不能为空";
            return null;
        }
        XmlDocument doc = new XmlDocument();
        try
        {
            doc.LoadXml(as_str);
            mess = "TRUE";
            return doc;
        }
        catch (Exception e)
        {
            mess = "不正确的XML格式字符串：" + e.Message.ToString();
            return null;
        }
    }

    public bool getCurrentDateTime(out DateTime ldatetime, out string mess)
    {
        mess = "";
        ldatetime = Convert.ToDateTime("1900-01-01");

        SqlConnection sconn = new SqlConnection((DBConnStr.Replace("Provider=SQLOLEDB.1;", "")));
        cmdand cmd = new cmdand();

        sconn.Open();
        cmd.Connection = sconn;
        cmd.CommandText = " Select Top 1 GetDate() From sysarea Where areacode = '" + AreaCode + "'";
        try
        {
            ldatetime = Convert.ToDateTime(cmd.ExecuteScalar());
            return true;
        }
        catch (Exception e)
        {
            mess = "查询系统当前时间异常:" + e.Message.ToString();
            return false;
        }
        finally
        {
            cmd.Dispose();
            if (sconn.State == ConnectionState.Open)
            {
                sconn.Close();
                sconn.Dispose();
            }
        }
    }
    
    #endregion

    #region 基础业务方法

    #region //设置农合接口产品标识符
    private bool setPDCert(string requestParmXML, out string mess)
    {
        
        string dbConnStr = "",areacode,hospitalcode,pdcert;

        mess = "";

        return true;

        XmlDocument xmldoc = getXMLDocumentFromString(requestParmXML, out mess);
        if (mess == "TRUE")
        {
            try
            {
                XmlNodeList list;
                list = xmldoc.GetElementsByTagName("areacode");
                areacode = list[0].InnerText;
                list = xmldoc.GetElementsByTagName("hospitalcode");
                hospitalcode = list[0].InnerText;

                if (requestParmXML.IndexOf("<pdcert>") > 0)
                {
                    list = xmldoc.GetElementsByTagName("pdcert");
                    pdcert = list[0].InnerText;
                }
                else 
                {
                    pdcert = "Not regist";
                }
            }
            catch (Exception e)
            {
                mess = getXMLErroStrFromString("9000", "异常：" + e.Message.ToString() + ",可能缺少数据元");
                return false;
            }
        }
        else
        {
            mess = getXMLErroStrFromString("9000", mess);
            return false;
        }
       
        
        if (pdcert.Length == 0)
            return true;

        if (getDBConnStr("areacode",areacode,out dbConnStr))
        {
            SqlConnection Connection = new SqlConnection(dbConnStr.Replace("Provider=SQLOLEDB.1;", ""));
            try
            {
                Connection.Open();
                cmdand sqlcmd = new cmdand();
                sqlcmd.Connection = Connection;
                SqlTransaction mytrans;

                mytrans = Connection.BeginTransaction();
                try
                {
                    sqlcmd.Transaction = mytrans;
                    sqlcmd.CommandText = "Update organcode Set pdcert = '" + pdcert + "' Where orgcode = '" + hospitalcode + "'";
                    sqlcmd.ExecuteNonQuery();
                    mytrans.Commit();
                }
                catch (Exception e)
                {
                    mytrans.Rollback();
                    mess = getXMLErroStrFromString("5001","更新产品标识失败" + e.Message);
                    return false;
                }
                finally
                {
                    mytrans.Dispose();
                    sqlcmd.Dispose();
                }
            }
            catch (Exception e)
            {
                mess = getXMLErroStrFromString("9000", "发生异常"+ e.Message.ToString());
                return false;
            }
            finally
            {
                if (Connection.State == ConnectionState.Open)
                    Connection.Close();                 
            }
            return true;
        }
        else
        {
            mess = getXMLErroStrFromString("50000",dbConnStr);
            return false;
        }

    }

    #endregion

    #region //用户验证
    /// <summary>
    /// 验证用户身份，返回“TRUE”表示验证通过，InterFacePower表示允许使用的接口业务类型
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="userid"></param>
    /// <param name="pwd"></param>
    /// <returns></returns>
    //[WebMethod(Description="Service用户身份验证，返回“TRUE”表示验证通过")]
    public string checkUserValid(string areacode, string hospitalcode, string userid, string pwd)
    {
        if (areacode.Length !=6)
        {
            return "非法的区域代码。请使用国标6位区域代码！";
        }
        string mess="";
        if (getDBConnStr("areacode", areacode, out mess))
        {
            SqlConnection Connection = new SqlConnection(mess);
            try
            {
                Connection.Open();
                cmdand cmd;
                //cmdand cmd = new cmdand("SELECT count(*) FROM sysoperator where areacode=('" + areacode + "') and opercode=('" + userid + "')", Connection);
                if (userid == "RDHL_HIS_Temped" && pwd == "TempUsedPassword") //HISonline中临时使用的用户名密码
                {
                    cmd = new cmdand("SELECT interface1,interface2 FROM organcode WHERE orgcode='" + hospitalcode + "' AND areacode=('" + areacode + "')", Connection);
                }
                else
                {
                    cmd = new cmdand("SELECT interface1,interface2 FROM organcode WHERE orgcode='" + hospitalcode + "' AND areacode=('" + areacode + "') AND interface_ncmsid=('" + userid + "') AND interface_ncmspass=('" + pwd + "')", Connection);
                }
                SqlDataReader myReader = cmd.ExecuteReader();
                try
                {
                    if (!myReader.HasRows)
                    {
                        return "区域代码或医院代码或用户代码或密码错误，身份验证失败！";
                    }
                    else
                    {
                        myReader.Read();
                        InterFacePower = myReader.GetInt32(1);
                        if (userid == "RDHL_HIS_Temped" && pwd == "TempUsedPassword") //HISonline中临时使用的用户名密码
                        {
                            InterFacePower = 0;//住院允许门诊不允许	0
                        }
                        else
                        {
                            switch (InterFacePower)
                            {
                                case 100: //都允许使用	100
                                case 10: //住院不允许门诊允许	10
                                case 1: //住院限制门诊允许	1
                                    break;
                                case 0: //住院允许门诊不允许	0
                                case 11: //住院限制门诊不允许	11
                                    break;
                                //case -1: //都不允许使用	-1
                                //    break;
                                default:
                                    setUserState(false);
                                    return "接口未取得许可:" + myReader.GetString(0);
                            }
                        }
                        setUserState(true);
                        return "TRUE";
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
                return "验证异常！" + e.Message.ToString();
            }
            finally
            {
                if (Connection.State == ConnectionState.Open)
                    Connection.Close();
            }
        }
        else
        {
            return mess;
        }
    }
    #endregion

    #region //城居接口工具版本号获取
    public string getCJJKVer()
    {
        return getXMLStrFromString("1.0");
    }
    #endregion

    #region //医院信息获取
    /// <summary>
    /// 根据医院代码得到本机构的信息
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="HospitalCode"></param>
    /// <returns></returns>
    //[WebMethod(Description = "根据医院代码得到本机构的信息")]
    public string getHospitalInfo(string areacode, string HospitalCode, out string data)
    { 
        string mess = "";
        data = "";
        if (getDBConnStr("areacode", areacode, out mess))
        {
            string ls_sql = "SELECT a.cityname as areaname,o.orgname as hospitalname,o.orglevel as hospitallevel FROM organcode o,sysarea a ";
            ls_sql += "where o.areacode=a.areacode and o.areacode=('" + areacode + "') and o.areacode2=a.areacode2 and o.orgcode=('" + HospitalCode + "')";
            DataSet myDs = getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    getRequestXMLStringFromDS(myDs, "areaname:区域名;hospitalname:医院名;hospitallevel:1-I级医院2-II级医院3-III级医院", out mess,out data);
                    return mess;
                }
                else
                {
                    mess = getXMLErroStrFromString("5000",mess);
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
            mess = getXMLErroStrFromString("5000",mess);
            return mess;
        }
    }

    /// <summary>
    /// 根据医院名称模糊查询机构信息
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="HospitalName"></param>
    /// <returns></returns>
    //[WebMethod(Description = "根据医院名称模糊查询机构信息")]
    public string getHospitalsInfo(string areacode, string HospitalName, out string data)
    {
        string mess = "";
        data = "";
        if (getDBConnStr("areacode", areacode, out mess))
        {
            string ls_sql = "SELECT a.cityname as areaname,o.orgname as hospitalname,o.levelcode as hospitallevel FROM organcode o,sysarea a ";
            ls_sql += "where o.areacode=a.areacode and o.areacode=('" + areacode + "') and o.areacode2=a.areacode2 and o.orgname like ('%" + HospitalName + "%')";
            DataSet myDs = getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    getRequestXMLStringFromDS(myDs, "areaname:区域名;hospitalname:医院名;hospitallevel:1-I级医院2-II级医院3-III级医院", out mess, out data);
                    return mess;
                }
                else
                {
                    mess = getXMLErroStrFromString("5000", mess);
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
            mess = getXMLErroStrFromString("5000", mess);
            return mess;
        }
    }
    #endregion

    #region //根据补助额精度和精度截取算法计算补助额
    /// <summary>
    /// 根据补助额精度和精度截取算法计算补助额
    /// </summary>
    /// <param name="adec_values"></param> //补助额
    /// <param name="ai_chkprecision"></param> //补助额精度  -2 百元 -1 拾元 0 元 1 角 2 分
    /// <param name="ai_precisionmode"></param> //精度截取算法	默认值是1：四舍五入法 2:舍尾法 3:收尾法
    /// <returns></returns> //计算后的补助额
    public decimal calcChkValue(decimal adec_values, Int32 ai_chkprecision, Int32 ai_precisionmode)
    {
        decimal ldec_values = adec_values;
        #region //参数验证
        switch (ai_precisionmode)
        {
            case 1:
            case 2:
            case 3:
            case 4:
                break;
            default:
                ai_precisionmode = 1; //默认值是1：四舍五入法
                break;
        }
        switch (ai_chkprecision)
        {
            case 0:
            case 1:
            case 2:
            case -1:
            case -2:
                break;
            default:
                ai_chkprecision = 0; //默认值是0：精确到元
                break;
        }
        #endregion
        switch (ai_precisionmode)
        {
            #region //四舍五入
            case 1: //四舍五入
                switch (ai_chkprecision)
                {
                    case 0: //元
                    case 1: //角
                    case 2: //分
                        ldec_values = Math.Round(ldec_values + 0.000001M, ai_chkprecision);
                        break;
                    case -1: //拾元
                        ldec_values = Math.Round(ldec_values + 0.000001M / 10.0M, 0) * 10.0M;
                        break;
                    case -2: //百元
                        ldec_values = Math.Round(ldec_values + 0.000001M / 100.0M, 0) * 100.0M;
                        break;
                }
                break;
            #endregion
            #region //舍尾法
            case 2: //舍尾法
                switch (ai_chkprecision)
                {
                    case 0: //元
                        ldec_values = Convert.ToDecimal(Convert.ToInt32(ldec_values - 0.49999M));
                        break;
                    case 1: //角
                        ldec_values = Convert.ToInt32(ldec_values * 10.0M - 0.49999M) / 10.0M;
                        break;
                    case 2: //分
                        ldec_values = Convert.ToInt32(ldec_values * 100.0M - 0.49999M) / 100.0M;
                        break;
                    case -1: //拾元
                        ldec_values = Convert.ToInt32(ldec_values / 10.0M - 0.49999M) * 10.0M;
                        break;
                    case -2: //百元
                        ldec_values = Convert.ToInt32(ldec_values / 100.0M - 0.49999M) * 100.0M;
                        break;
                }
                break;
            #endregion
            #region //收尾法
            case 3: //收尾法
                switch (ai_chkprecision)
                {
                    case 0: //元
                        ldec_values = Convert.ToDecimal(Convert.ToInt32(ldec_values + 0.50001M));
                        break;
                    case 1: //角
                        ldec_values = Convert.ToInt32(ldec_values * 10 + 0.50001M) / 10.0M;
                        break;
                    case 2: //分
                        ldec_values = Convert.ToInt32(ldec_values * 100.0M + 0.50001M) / 100.0M;
                        break;
                    case -1: //拾元
                        ldec_values = Convert.ToInt32(ldec_values / 10.0M + 0.50001M) * 10.0M;
                        break;
                    case -2: //百元
                        ldec_values = Convert.ToInt32(ldec_values / 100.0M + 0.50001M) * 100.0M;
                        break;
                }
                break;
            #endregion
        }
        return ldec_values;
    }
    #endregion

    #region //参合验证和获取有效参合者数据
    /// <summary>
    /// 读磁卡后解密
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="year"></param>
    /// <param name="medicard"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string getCardInfo(string areacode, string year, string medicard, out string data)
    {
        string mess = "", authid = "";
        data = "";
        if (!(isNumber(year) && Convert.ToInt16(year) > 2008))
        {
            mess = getXMLErroStrFromString("2000", "参数year无效");
            return mess;
        }
        Int32 li_checkpatinfo; //卡上存的是家庭号还是证卡号
        #region //卡上存的是家庭号还是证卡号
        if (getDBConnStr("areacode", areacode, out mess))
        {
            SqlConnection Connection = new SqlConnection(mess.Replace("Provider=SQLOLEDB.1;", ""));
            try
            {
                string ls_authsql = "SELECT case when checkpatinfo=0 then 99999 else checkpatinfo end FROM areasysparm WHERE areacode='"+areacode+"'";
                Connection.Open();
                cmdand cmd = new cmdand(ls_authsql, Connection);
                try
                {
                    if (Convert.ToInt32(cmd.ExecuteScalar()) != 0)
                    {
                        if (Convert.ToInt32(cmd.ExecuteScalar()) != 99999 && Convert.ToInt32(cmd.ExecuteScalar()) != 1)
                        {
                            return getXMLErroStrFromString("2000", "无效的系统参数");
                        }
                    }
                    else
                    {
                        return getXMLErroStrFromString("2000", "系统参数不存在");
                    }
                    li_checkpatinfo = Convert.ToInt32(cmd.ExecuteScalar());
                    if (li_checkpatinfo == 99999)
                    {
                        li_checkpatinfo = 0;
                    }
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                return getXMLErroStrFromString("9000", "查询系统参数异常：" + e.Message.ToString());
            }
            finally
            {
                if (Connection.State == ConnectionState.Open)
                    Connection.Close();
            }
        }
        else
        {
            mess = getXMLErroStrFromString("5000", mess);
            return mess;
        }
        #endregion
        if (medicard.Length == 0)
        {
            mess = getXMLErroStrFromString("2000", "参数medicard无效");
            return mess;
        }
        else
        {
            Int32 li_loop=0;
            String ls_tmp="";
            switch (areacode)
            {
                case "430181":
                    authid = medicard.Substring(medicard.Length -4);
                    medicard = medicard.Substring(0,medicard.Length -4);
                    #region //解密
                    if (authid.Length > 0)
                    {
                        for (li_loop = authid.Length - 1; li_loop >= 0; li_loop--)
                        {
                            switch (authid.Substring(li_loop, 1))
                            {
                                case "7":
                                    ls_tmp += " ";
                                    break;
                                case "E":
                                    ls_tmp += "0";
                                    break;
                                case "C":
                                    ls_tmp += "1";
                                    break;
                                case "3":
                                    ls_tmp += "2";
                                    break;
                                case "0":
                                    ls_tmp += "3";
                                    break;
                                case "2":
                                    ls_tmp += "4";
                                    break;
                                case "F":
                                    ls_tmp += "5";
                                    break;
                                case "B":
                                    ls_tmp += "6";
                                    break;
                                case "9":
                                    ls_tmp += "7";
                                    break;
                                case "A":
                                    ls_tmp += "8";
                                    break;
                                case "D":
                                    ls_tmp += "9";
                                    break;
                            }
                        }
                        ls_tmp = ls_tmp.Trim();
                        ls_tmp = Convert.ToString((Convert.ToInt32(ls_tmp) - Convert.ToInt32('S'))/7);
                        authid = ls_tmp;
                    }
                    else
                    {
                        authid = "0";
                    }
                    #endregion
                    break;
                default:
                    authid = "0";
                    break;
            }
            ls_tmp = "SELECT familyno+convert(varchar,validno) as famiyno,bookcard,validno FROM n_familydata";
            switch (li_checkpatinfo)
            {
                case 0: //卡上存的是医疗证号
                    ls_tmp += " WHERE statisticyear='"+year+"' AND bookcard='"+medicard+"'";
                    break;
                case 1: //卡上存的是家庭号
                    ls_tmp += " WHERE familyno='" + medicard + "' AND statisticyear='" + year + "'";
                    break;
            }
            DataSet myDs = getDataSet(ls_tmp, out mess);
            if (mess == "TRUE")
            {
                getRequestXMLStringFromDS(myDs, "famiyno:家庭号;bookcard:证卡号;validno:证卡版本号", out mess, out data);
            }
            else
            {
                mess = getXMLErroStrFromString("5000", mess);
            }
            myDs.Dispose();
            return mess;

        }
    }

    /// <summary>
    /// 根据医疗证号或身份证号得到有效参合者数据"
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="year"></param>
    /// <param name="medicard"></param>
    /// <param name="authid"></param>
    /// <param name="ID"></param>
    /// <param name="bankcard"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string getPersonsInfo(string areacode, string year, string medicard, string authid, string id, string bankcard, out string data)
    {
        string mess = "";
        data = "";
        if (!(isNumber(year) && Convert.ToInt16(year) > 2008))
        {
            mess = getXMLErroStrFromString("2000", "参数year无效");
            return mess;
        }
        if (id == "0")
        {
            id = "";
        }
        if (id.Length != 0 && id.Length != 15 && id.Length != 18)
        {
            mess = getXMLErroStrFromString("2000", "参数id无效");
            return mess;
        }
        if (medicard.Length > 0)
        {
            if (medicard.Length > 16)
            {
                authid = medicard.Substring(16);
            }
            if (authid.Length == 0)
            {
                mess = getXMLErroStrFromString("2000", "提供参数medicard必须同时提供authid");
                return mess;
            }
            else
            {
                if (!isNumber(authid))
                {
                    mess = getXMLErroStrFromString("2000", "参数authid必须为数字型");
                    return mess;
                }
            }
        }
        if (medicard.Length == 0 && id.Length == 0 && bankcard.Length == 0)
        {
            mess = getXMLErroStrFromString("2000", "参数medicard、id和bankcard不能同时为空");
            return mess;
        }
        else
            if (getDBConnStr("areacode", areacode, out mess))
            {
                if (medicard.Length > 0) //此时需要通过卡识别码先验证是否有效卡
                #region
                {
                    SqlConnection Connection = new SqlConnection(mess.Replace("Provider=SQLOLEDB.1;", ""));
                    try
                    {
                        string ls_authsql = "SELECT case when validno=0 then 99999 else validno end FROM n_familydata WHERE";
                        if (medicard.Length > 16)
                        {
                            ls_authsql += " familyno='" + medicard.Substring(0, 16) + "' AND statisticyear='" + year + "'";
                        }
                        else if (medicard.Length > 0)
                        {
                            ls_authsql += " statisticyear='" + year + "' AND bookcard='" + medicard + "'";
                        }
                        Connection.Open();
                        cmdand cmd = new cmdand(ls_authsql, Connection);
                        try
                        {
                            if (Convert.ToInt32(cmd.ExecuteScalar()) != 0)
                            {
                                if (Convert.ToInt32(cmd.ExecuteScalar()) != 99999 && Convert.ToString(cmd.ExecuteScalar()) != authid)
                                {
                                    return getXMLErroStrFromString("2000", "无效的证卡");
                                }
                            }
                            else
                            {
                                return getXMLErroStrFromString("2000", "证卡不存在");
                            }
                        }
                        finally
                        {
                            cmd.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        return getXMLErroStrFromString("9000", "验证卡有效性异常：" + e.Message.ToString());
                    }
                    finally
                    {
                        if (Connection.State == ConnectionState.Open)
                            Connection.Close();
                    }
                }
                #endregion
                string ls_sql = "SELECT f.bookcard as zkh,f.name+'(余'+convert(varchar,fc.familyaccbalance)+')' as hz,i.human as grh,r.name as gx,i.name as xm,s.sexname as xb,i.patientid as sfz";
                ls_sql += ",isnull(convert(varchar(10),i.birthday,120),'') as csrq,n.name as xz,isnull(i.ifoutpatient,0) as mb,isnull(convert(varchar(10),i.outpatienttime,120),'') as mbsj";
                ls_sql += ",a.cityname as dz,f.familyno+convert(varchar,f.validno) as jth";
                ls_sql += " FROM n_familydata f,n_individdata i,n_individcwuchkkp c,sysarea a,n_familycwuchkkp fc,";
                ls_sql += "(select code,name from n_standdata where standtab='S101-06') r,";
                ls_sql += "sexcode s,";
                ls_sql += "(select code,name from n_standdata where standtab='nature') n";
                ls_sql += " WHERE f.familyno=i.familyno AND f.statisticyear=i.statisticyear AND i.human=c.human AND i.statisticyear=c.statisticyear AND f.areacode=a.areacode AND f.areacode2=a.areacode2";
                ls_sql += " AND fc.familyno=f.familyno AND fc.statisticyear=f.statisticyear";
                if (medicard.Length > 16) //按现在农合系统的写卡方法，家庭号为16位，其后为医疗证识别码
                {
                    ls_sql += " AND f.familyno='" + medicard.Substring(0, 16) + "'";
                    ls_sql += " AND f.statisticyear='" + year + "'";
                    ls_sql += " AND f.validno=" + authid;
                }
                else if (medicard.Length > 0)
                {
                    ls_sql += " AND f.statisticyear='" + year + "'";
                    ls_sql += " AND f.bookcard='" + medicard + "'";
                    ls_sql += " AND f.validno=" + authid;
                }
                else
                {
                    ls_sql += " AND f.statisticyear='" + year + "'";
                }
                ls_sql += " AND f.areacode='" + areacode + "'";
                ls_sql += " AND i.relation=r.code AND i.sex=s.sex AND i.nature=n.code ";
                ls_sql += " AND f.familystate='00' AND f.bookcdstate='01' AND c.accstate='1'";
                if (id.Length > 0)
                {
                    ls_sql += " AND i.patientid='" + id + "'";
                }
                if (bankcard.Length > 0)
                {
                    ls_sql += " AND f.doorplate='" + bankcard + "'";
                }
                ls_sql += " ORDER BY f.familyno,i.memberno";
                DataSet myDs = getDataSet(ls_sql, out mess);
                if (mess == "TRUE")
                {
                    getRequestXMLStringFromDS(myDs, "zkh:证卡号;hz:户主;grh:个人编号;gx:家庭关系;xm:姓名;xb:性别;sfz:身份证号;csrq:出生日期;xz:个人性质;mb:是否慢病(0否1是;mbsj:慢病门诊开始时间;dz:家庭地址", out mess, out data);
                }
                else
                {
                    mess = getXMLErroStrFromString("5000", mess);
                }
                myDs.Dispose();
                return mess;
            }
            else
            {
                mess = getXMLErroStrFromString("5000", mess);
                return mess;
            }
    }

    /// <summary>
    /// 通过医疗证号+身份证号+姓名；医疗证号+身份证号；医疗证号+姓名；身份证号+姓名四种方式验证参合有效性
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="year"></param>
    /// <param name="medicard"></param>
    /// <param name="authid"></param>
    /// <param name="namein"></param>
    /// <param name="humanin"></param>
    /// <param name="name"></param>
    /// <param name="id"></param>
    /// <param name="human"></param>
    /// <param name="areacode2"></param>
    /// <param name="nature"></param>
    /// <param name="orderid"></param>
    /// <param name="ifoutpatient"></param>
    /// <param name="outpatienttime"></param>
    /// <param name="mess"></param>
    /// <returns></returns>
    /// 方法返回值false且mess!=""则参数错误或异常；false且mess==""则无效参合者；true则有效参合者;
    /// 有效参合时：human为为个人编码。areacode2为村组编码。nature为个人性质。orderid特殊门诊政策序号。ifoutpatient是否特殊门诊。outpatienttime特殊门诊开始时间
    public bool checkPersonsValid(string areacode, string year, string medicard, string authid, string namein, string humanin, out string name, string id, out string human, out string areacode2, out string nature, out Int32 orderid, out Int32 ifoutpatient, out string outpatienttime, out string mess)
    {
        mess = "";
        human = "";
        areacode2 = "";
        nature = "";
        name = "";
        orderid = -1;
        ifoutpatient = 0;
        outpatienttime = "";
        if (!(isNumber(year) && Convert.ToInt16(year) > 2008))
        {
            mess = "参数year无效";
            return false;
        }
        id = id.Trim();
        if (id == "0"||id == "无")
        {
            id = "";
        }
        if (id.Length != 0 && id.Length != 15 && id.Length != 18)
        {
            mess = "参数sfz无效，长度必须为0或15或18位长";
            return false;
        }
        if (medicard.Length > 0)
        {
            if (medicard.Length > 16)
            {
                authid = medicard.Substring(16);
            }
            if (authid.Length == 0)
            {
                mess = "提供参数zkh必须同时提供zksbm";
                return false;
            }
            else
            {
                if (!isNumber(authid))
                {
                    mess = "参数zksbm必须为数字型";
                    return false;
                }
            }
        }
        if (humanin.Length == 0 && (namein.Length == 0 && (id.Length == 0 || medicard.Length == 0)) && (id.Length == 0 && (medicard.Length == 0 || namein.Length == 0)) && (medicard.Length == 0 && (id.Length == 0 || namein.Length == 0)))
        {
            mess = "参数zkh、sfz、xm和grh提供的值不充分";
            return false;
        }
        else
        {
            if (getDBConnStr("areacode", areacode, out mess))
            {
                DataSet myDs = null;
                string ls_sql = "SELECT i.human,f.areacode2,i.name,n.name,i.orderid,i.ifoutpatient,isnull(convert(varchar,i.outpatienttime,120),''),f.validno FROM n_familydata f,n_individdata i,n_individcwuchkkp c,(select code,name from n_standdata where standtab='nature') n";
                ls_sql += " WHERE f.familyno=i.familyno AND f.statisticyear=i.statisticyear AND i.human=c.human AND i.statisticyear=c.statisticyear";
                ls_sql += " AND n.code=i.nature";
                if (humanin.Length > 0)
                {
                    ls_sql += " AND i.human='" + humanin + "' AND i.statisticyear='" + year + "'";
                }
                if (medicard.Length > 16)
                {
                    //ls_sql += " AND f.statisticyear='" + year + "' AND f.familyno='" + medicard.Substring(0, 16) + "' AND f.validno=" + authid;
                    ls_sql += " AND f.statisticyear='" + year + "' AND f.familyno='" + medicard.Substring(0, 16) +"'";
                }
                else if (medicard.Length > 0)
                {
                    //ls_sql += " AND f.statisticyear='" + year + "' AND f.bookcard='" + medicard + "' AND f.validno=" + authid;
                    ls_sql += " AND f.statisticyear='" + year + "' AND f.bookcard='" + medicard +"'";
                }
                if (id.Length > 0)
                {
                    ls_sql += " AND i.statisticyear='" + year + "' AND i.patientid='" + id + "'";
                }
                if (namein.Length > 0)
                {
                    ls_sql += " AND i.name='" + namein + "'";
                }
                ls_sql += " AND f.familystate='00' AND f.bookcdstate='01' AND c.accstate='1'";
                myDs = getDataSet(ls_sql, out mess);
                try
                {
                    if (mess == "TRUE")
                    {
                        if (myDs.Tables[0].Rows.Count > 1)
                        {
                            mess = "验证参合有效性时查询到多个人";
                            return false;
                        }
                        else if (myDs.Tables[0].Rows.Count == 0)
                        {
                            mess = "参合信息库中不存在此人";//没找到人
                            return false;
                        }
                        else
                        {   
                            if (medicard.Length > 0)
                            {
                                if (myDs.Tables[0].Rows[0][7].ToString() != authid)
                                {
                                    mess = "医疗证卡已重制，请使用重制的新卡。此卡已停用或挂失，请上交合管办！";
                                    return false;
                                }
                            }
                            human = myDs.Tables[0].Rows[0][0].ToString();
                            areacode2 = myDs.Tables[0].Rows[0][1].ToString();
                            name = myDs.Tables[0].Rows[0][2].ToString();
                            nature = myDs.Tables[0].Rows[0][3].ToString();
                            orderid = Convert.ToInt32(myDs.Tables[0].Rows[0][4]);
                            ifoutpatient = Convert.ToInt32(myDs.Tables[0].Rows[0][5]);
                            outpatienttime = myDs.Tables[0].Rows[0][6].ToString();
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
    }

    /// <summary>
    /// 验证参合有效性
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="year"></param>
    /// <param name="medicard"></param>
    /// <param name="authid"></param>
    /// <param name="name"></param>
    /// <param name="id"></param>
    /// <param name="human"></param>
    /// <param name="areacode2"></param>
    /// <param name="nature"></param>
    /// <param name="mess"></param>
    /// <returns></returns>
    public bool checkPersonsValid(string areacode, string year, string medicard, string authid, string namein, string humanin, out string name, string id, out string human, out string areacode2, out string nature, out string mess)
    {
        mess = "";
        human = "";
        areacode2 = "";
        nature = "";
        name = "";
        Int32 orderid, ifoutpatient;
        string outpatienttime;
        return checkPersonsValid(areacode, year, medicard, authid, namein, humanin, out name, id, out human, out areacode2, out nature, out orderid, out ifoutpatient, out outpatienttime, out mess);
    }
    #endregion

    #region //更新家庭账户及个人帐户信息
    /// <summary>
    /// 更新家庭及个人帐户信息
    /// </summary>
    /// <param name="hospitalcode"></param> //医院代码
    /// <param name="hospid"></param> //就诊号或住院ID号
    /// <param name="ptype"></param> //账户更新业务类型1:更新住院帐户信息 2：更新门诊住院帐户信息
    /// <param name="optype"></param> //业务操作类别1:预审/审核 2：取消预审/取消审核
    /// <param name="opfee"></param> //更新操作类别0:更新所有帐户信息 1：只更新补助金额
    /// <param name="chkfee"></param> //补助金额
    /// <param name="grandprice"></param> //住院费用
    /// <param name="human"></param> //农合个人编码
    /// <param name="statisticyear"></param> //参合年度
    /// <param name="nopaydate"></param> //费用最后发生日期
    /// <param name="mess"></param> //发生错误时的错误信息
    /// <returns></returns>
    public bool setPersonAccount(string hospitalcode, string hospid, Int16 ptype, Int16 optype, Int16 opfee, decimal chkfee, decimal grandprice, string human, string statisticyear, string nopaydate, out string mess)
    {
        mess = "";
        #region //参数有效性验证
        if (!getUserState())
        {
            mess = "更新账户时发现调用本接口的用户没有通过验证";
            return false;
        }
        if (chkfee == 0)
        {
            return true;
        }
        if (!isDateTime(statisticyear + "-01-01 00.00.01"))
        {
            mess = "更新账户时使用了错误的参合年度参数";
            return false;
        }
        if (ptype != 1 && ptype != 2)
        {
            mess = "更新账户时使用了没有定义的账户更新业务类型";
            return false;
        }
        if (optype != 1 && optype != 2)
        {
            mess = "更新账户时使用了没有定义的业务操作类别";
            return false;
        }
        if (opfee != 0 && opfee != 0)
        {
            mess = "更新账户时使用了没有定义的更新操作类别";
            return false;
        }
        if (ptype == 1 && !isDateTime(nopaydate.Substring(0, 4) + nopaydate.Substring(4, 2) + nopaydate.Substring(6, 2) + " 00.00.01"))
        {
            mess = "更新账户时使用了错误的最后费用日期参数";
            return false;
        }
        #endregion
        string familyno = "";
        Int32 memberno = 0;
        string ls_sql = "";
        decimal ldec_familyaccbalance = 0;
        decimal ldec_temp1, ldec_temp2, ldec_inprice;
        Int32 li_hosps, li_mzchkpayid;
        SqlConnection Connection = new SqlConnection(DBConnStr.Replace("Provider=SQLOLEDB.1;", ""));
        cmdand cmd = new cmdand();
        cmd.Connection = Connection;
        SqlTransaction myTrans;
        Connection.Open();
        myTrans = Connection.BeginTransaction();
        try
        {
            ls_sql = "SELECT familyno,memberno FROM n_individdata WHERE human='" + human + "' AND statisticyear='" + statisticyear + "'";
            DataSet myDs = getDataSet(ls_sql, out mess);
            #region //参合资料提取
            try
            {
                if (mess == "TRUE")
                {
                    if (myDs.Tables[0].Rows.Count == 0)
                    {
                        mess = "没有查询到相关参合资料";
                        return false;
                    }
                    else
                    {
                        familyno = myDs.Tables[0].Rows[0][0].ToString();
                        memberno = Convert.ToInt32(myDs.Tables[0].Rows[0][1]);
                    }
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                myDs.Dispose();
            }
            #endregion
            switch (ptype)
            {
                case 1: //住院信息
                    #region//查询家庭帐户
                    ls_sql = "SELECT IsNull(familyfound,0),IsNull(familyfundedit,-1) FROM hosp_check_info WHERE hospitalcode='" + hospitalcode + "' AND hospid=" + hospid;
                    myDs = getDataSet(ls_sql, out mess);
                    if (mess == "TRUE")
                    {
                        if (myDs.Tables[0].Rows.Count == 0)
                        {
                            ldec_temp1 = 0;
                            ldec_temp2 = 0;
                        }
                        else
                        {
                            ldec_temp1 = Convert.ToDecimal(myDs.Tables[0].Rows[0][0]);
                            ldec_temp2 = Convert.ToDecimal(myDs.Tables[0].Rows[0][1]);
                            if (ldec_temp2 == -1) //如果手工修改了家庭帐户支付，那么以TA为准
                            {
                                ldec_familyaccbalance = ldec_temp1;
                            }
                            else
                            {
                                ldec_familyaccbalance = ldec_temp2;
                            }
                        }
                    }
                    else
                    {
                        myDs.Dispose();
                        return false;
                    }
                    myDs.Dispose();
                    #endregion
                    #region //查询住院天数
                    ls_sql = "SELECT hosps FROM uv_hosppatinfo2 WHERE hospitalcode='" + hospitalcode + "' AND hospid=" + hospid;
                    myDs = getDataSet(ls_sql, out mess);
                    if (mess == "TRUE")
                    {
                        if (myDs.Tables[0].Rows.Count == 0)
                        {
                            mess = "没有查询到住院天数信息";
                            return false;
                        }
                        else
                        {
                            li_hosps = Convert.ToInt32(myDs.Tables[0].Rows[0][0]);
                        }
                    }
                    else
                    {
                        myDs.Dispose();
                        return false;
                    }
                    myDs.Dispose();
                    #endregion
                    #region //查询保内费用
                    ls_sql = "SELECT Sum(IsNull(chkagainfee,0)) FROM pricedetail WHERE hospitalcode='" + hospitalcode + "' AND hospid=" + hospid;
                    myDs = getDataSet(ls_sql, out mess);
                    if (mess == "TRUE")
                    {
                        if (myDs.Tables[0].Rows.Count == 0)
                        {
                            mess = "没有查询到保内费用";
                            return false;
                        }
                        else
                        {
                            ldec_inprice = Convert.ToDecimal(myDs.Tables[0].Rows[0][0]);
                        }
                    }
                    else
                    {
                        myDs.Dispose();
                        return false;
                    }
                    myDs.Dispose();
                    #endregion
                    #region //开始更新
                    cmd.Transaction = myTrans;
                    if (optype == 1) //预审/审核
                    #region
                    {
                        Connection.Open();
                        if (opfee == 0) //更新所有帐户信息
                        {
                            try
                            {
                                //cmd.Transaction = myTrans;
                                cmd.CommandText = "UPDATE n_individcwuchkkp SET hospcount = IsNull(hospcount,0) + 1,";
                                cmd.CommandText += "hospdates = IsNull(hospdates,0)+" + li_hosps.ToString() + ",";
                                cmd.CommandText += "hospfee = IsNull(hospfee,0)+" + grandprice.ToString() + ",";
                                cmd.CommandText += "inhospfee = IsNull(inhospfee,0)+" + ldec_inprice.ToString() + ",";
                                cmd.CommandText += "hospredeem = IsNull(hospredeem,0)+" + chkfee.ToString();
                                cmd.CommandText += " WHERE human ='" + human + "' AND statisticyear='" + statisticyear + "'";
                                cmd.ExecuteNonQuery();
                                //myTrans.Commit();
                            }
                            catch (Exception e)
                            {
                                myTrans.Rollback();
                                mess = "更新账户异常：" + e.Message.ToString();
                                return false;
                            }
                        }
                        else //只更新补助金额
                        {
                            try
                            {
                                //cmd.Transaction = myTrans;
                                cmd.CommandText = "UPDATE n_individcwuchkkp SET hospredeem = IsNull(hospredeem,0)+" + chkfee.ToString();
                                cmd.CommandText += " WHERE human ='" + human + "' AND statisticyear='" + statisticyear + "'";
                                cmd.ExecuteNonQuery();
                                //myTrans.Commit();
                            }
                            catch (Exception e)
                            {
                                myTrans.Rollback();
                                mess = "更新账户补助异常：" + e.Message.ToString();
                                return false;
                            }
                        }
                        ldec_familyaccbalance *= -1;
                    }
                    #endregion
                    else //取消预审/取消审核
                    #region
                    {
                        Connection.Open();
                        if (opfee == 0) //更新所有帐户信息
                        {
                            try
                            {
                                //cmd.Transaction = myTrans;
                                cmd.CommandText = "UPDATE n_individcwuchkkp SET hospcount = Case When IsNull(hospcount,0) -1 < 0 Then 0 Else IsNull(hospcount,0) - 1 End,";
                                cmd.CommandText += "hospdates = Case When IsNull(hospdates,0) - " + li_hosps.ToString() + " < 0 Then 0 Else IsNull(hospdates,0) - " + li_hosps.ToString() + " End,";
                                cmd.CommandText += "hospfee = Case When IsNull(hospfee,0) - " + grandprice.ToString() + " < 0 Then 0 Else IsNull(hospfee,0) - " + grandprice.ToString() + " End,";
                                cmd.CommandText += "inhospfee = Case When IsNull(inhospfee,0) - " + ldec_inprice.ToString() + " < 0 Then 0 Else IsNull(inhospfee,0) - " + ldec_inprice.ToString() + " End,";
                                cmd.CommandText += "hospredeem = Case When IsNull(hospredeem,0) - " + chkfee.ToString() + " < 0 Then 0 Else IsNull(hospredeem,0) - " + chkfee.ToString() + " End";
                                cmd.CommandText += " WHERE human ='" + human + "' AND statisticyear='" + statisticyear + "'";
                                cmd.ExecuteNonQuery();
                                //myTrans.Commit();
                            }
                            catch (Exception e)
                            {
                                myTrans.Rollback();
                                mess = "更新账户异常：" + e.Message.ToString();
                                return false;
                            }
                        }
                        else //只更新补助金额
                        {
                            try
                            {
                                //cmd.Transaction = myTrans;
                                cmd.CommandText = "UPDATE n_individcwuchkkp SET hospredeem = Case When IsNull(hospredeem,0) - " + chkfee.ToString() + " < 0 Then 0 Else IsNull(hospredeem,0) - " + chkfee.ToString() + " End";
                                cmd.CommandText += " WHERE human ='" + human + "' AND statisticyear='" + statisticyear + "'";
                                cmd.ExecuteNonQuery();
                                //myTrans.Commit();
                            }
                            catch (Exception e)
                            {
                                myTrans.Rollback();
                                mess = "更新账户补助异常：" + e.Message.ToString();
                                return false;
                            }
                        }
                    }
                    #endregion
                    #region //最后更新家庭帐户信息
                    if (opfee == 0) //更新所有帐户信息
                    {
                        try
                        {
                            //cmd.Transaction = myTrans;
                            cmd.CommandText = "UPDATE n_familycwuchkkp SET hospcount = a.a1,hospdates = a.a2,hospfee = a.a3,";
                            cmd.CommandText += "inhospfee = a.a4,hospredeem = a.a5,familyaccbalance = familyaccbalance + " + ldec_familyaccbalance.ToString();
                            cmd.CommandText += " FROM (Select Sum(IsNull(hospcount,0)) a1,Sum(IsNull(hospdates,0)) a2,Sum(IsNull(hospfee,0)) a3,";
                            cmd.CommandText += "Sum(IsNull(inhospfee,0)) a4,Sum(IsNull(hospredeem,0)) a5";
                            cmd.CommandText += " From n_individcwuchkkp a ,n_individdata b";
                            cmd.CommandText += " Where a.human = b.human And a.statisticyear = '" + statisticyear + "'";
                            cmd.CommandText += " And a.statisticyear = b.statisticyear And b.familyno = '" + familyno + "') a";
                            cmd.CommandText += " WHERE n_familycwuchkkp.familyno = '" + familyno + "' AND n_familycwuchkkp.statisticyear = '" + statisticyear + "'";
                            cmd.ExecuteNonQuery();
                            myTrans.Commit();
                        }
                        catch (Exception e)
                        {
                            myTrans.Rollback();
                            mess = "更新家庭账户异常：" + e.Message.ToString();
                            return false;
                        }

                    }
                    else //只更新补助金额
                    {
                        try
                        {
                            //cmd.Transaction = myTrans;
                            cmd.CommandText = "UPDATE n_familycwuchkkp SET hospredeem = a.a5";
                            cmd.CommandText += " FROM (Select Sum(IsNull(hospredeem,0)) a5 From n_individcwuchkkp a ,n_individdata b";
                            cmd.CommandText += " Where a.human = b.human And a.statisticyear = '" + statisticyear + "'";
                            cmd.CommandText += " And a.statisticyear = b.statisticyear And b.familyno = '" + familyno + "') a";
                            cmd.CommandText += " WHERE n_familycwuchkkp.familyno = '" + familyno + "' AND n_familycwuchkkp.statisticyear = '" + statisticyear + "'";
                            cmd.ExecuteNonQuery();
                            myTrans.Commit();
                        }
                        catch (Exception e)
                        {
                            myTrans.Rollback();
                            mess = "更新家庭账户补助异常：" + e.Message.ToString();
                            return false;
                        }
                    }
                    #endregion
                    #endregion
                    break;
                case 2: //门诊信息
                    #region //门诊病人信息
                    ls_sql = "SELECT Convert(VarChar(8),hospdate,112),IsNull(grandprice,0),IsNull(familyfound,0),";
                    ls_sql += "IsNull(grandprice,0) - (IsNull(mediselffee,0) + IsNull(materselffee,0) + IsNull(serverselffee,0)),isnull(chkpayid,0)";
                    ls_sql += " FROM mz_hosppatinfo2 WHERE hospitalcode='" + hospitalcode + "' AND hospcode='" + hospid + "'";
                    myDs = getDataSet(ls_sql, out mess);
                    if (mess == "TRUE")
                    {
                        if (myDs.Tables[0].Rows.Count == 0)
                        {
                            mess = "没有查询到门诊资料";
                            myDs.Dispose();
                            return false;
                        }
                        else
                        {
                            nopaydate = myDs.Tables[0].Rows[0][0].ToString();
                            grandprice = Convert.ToDecimal(myDs.Tables[0].Rows[0][1]);
                            ldec_familyaccbalance = Convert.ToDecimal(myDs.Tables[0].Rows[0][2]);
                            ldec_inprice = Convert.ToDecimal(myDs.Tables[0].Rows[0][3]);
                            li_mzchkpayid = Convert.ToInt32(myDs.Tables[0].Rows[0][4]); //门诊家庭账户支付标志
                        }
                    }
                    else
                    {
                        myDs.Dispose();
                        return false;
                    }
                    myDs.Dispose();
                    #endregion
                    #region //开始更新
                    cmd.Transaction = myTrans;
                    if (optype == 1)//预审/审核
                    #region //预审审核更新n_individcwuchkkp
                    {
                        if (opfee == 0) //更新所有帐户信息
                        {
                            try
                            {
                                //cmd.Transaction = myTrans;
                                cmd.CommandText = "UPDATE n_individcwuchkkp SET otpatcount = IsNull(otpatcount,0) + 1,";
                                cmd.CommandText += "otpatientfee = IsNull(otpatientfee,0) + " + grandprice.ToString() + ",";
                                cmd.CommandText += "inotpatientfee = IsNull(inotpatientfee,0) + " + ldec_inprice.ToString() + ",";
                                cmd.CommandText += "otpatredeem = IsNull(otpatredeem,0) + " + chkfee.ToString();
                                cmd.CommandText += " WHERE human = '" + human + "' AND statisticyear = '" + statisticyear + "'";
                                cmd.ExecuteNonQuery();
                                //myTrans.Commit();
                            }
                            catch (Exception e)
                            {
                                myTrans.Rollback();
                                mess = "更新个人账户异常：" + e.Message.ToString();
                                return false;
                            }
                        }
                        else //只更新补助金额
                        {
                            try
                            {
                                //cmd.Transaction = myTrans;
                                cmd.CommandText = "UPDATE n_individcwuchkkp SET otpatredeem = IsNull(otpatredeem,0) + " + chkfee.ToString();
                                cmd.CommandText += " WHERE human = '" + human + "' AND statisticyear = '" + statisticyear + "'";
                                cmd.ExecuteNonQuery();
                                //myTrans.Commit();
                            }
                            catch (Exception e)
                            {
                                myTrans.Rollback();
                                mess = "更新个人账户补助异常：" + e.Message.ToString();
                                return false;
                            }
                        }
                        ldec_familyaccbalance *= -1;
                    }
                    #endregion
                    else //取消预审/取消审核
                    #region //取消预审审核更新n_individcwuchkkp
                    {
                        if (opfee == 0) //更新所有帐户信息
                        {
                            try
                            {
                                //cmd.Transaction = myTrans;
                                cmd.CommandText = "UPDATE n_individcwuchkkp SET otpatcount = Case When IsNull(otpatcount,0) - 1 < 0 Then 0 Else IsNull(otpatcount,0) - 1 End,";
                                cmd.CommandText += "otpatientfee = Case When IsNull(otpatientfee,0) - " + grandprice.ToString() + " < 0 Then 0 Else IsNull(otpatientfee,0) - " + grandprice.ToString() + " End,";
                                cmd.CommandText += "inotpatientfee = Case When IsNull(inotpatientfee,0) - " + ldec_inprice.ToString() + " < 0 Then 0 Else IsNull(inotpatientfee,0) - " + ldec_inprice.ToString() + " End,";
                                cmd.CommandText += "otpatredeem = Case When IsNull(otpatredeem,0) - " + chkfee.ToString() + " < 0 Then 0 Else IsNull(otpatredeem,0) - " + chkfee.ToString() + " End";
                                cmd.CommandText += " WHERE human = '" + human + "' AND statisticyear = '" + statisticyear + "'";
                                cmd.ExecuteNonQuery();
                                //myTrans.Commit();
                            }
                            catch (Exception e)
                            {
                                myTrans.Rollback();
                                mess = "更新家庭账户异常：" + e.Message.ToString();
                                return false;
                            }
                        }
                        else //只更新补助金额
                        {
                            try
                            {
                                //cmd.Transaction = myTrans;
                                cmd.CommandText = "UPDATE n_individcwuchkkp SET otpatredeem = Case When IsNull(otpatredeem,0) - " + chkfee.ToString() + " < 0 Then 0 Else IsNull(otpatredeem,0) - " + chkfee.ToString() + " End";
                                cmd.CommandText += " WHERE human = '" + human + "' AND statisticyear = '" + statisticyear + "'";
                                cmd.ExecuteNonQuery();
                                //myTrans.Commit();
                            }
                            catch (Exception e)
                            {
                                myTrans.Rollback();
                                mess = "更新家庭账户补助异常：" + e.Message.ToString();
                                return false;
                            }
                        }
                    }
                    #endregion
                    if (opfee == 0) //更新所有帐户信息
                    #region //最后更新家庭帐户信息
                    {
                        #region //更新mz_hosppatinfo2家庭账户支付标志
                        if ((li_mzchkpayid == 1 && optype == 1) || (li_mzchkpayid == 0 && optype == 2)) //预审/审核时家庭账户已支付；取消预审/审核时家庭账户还未支付的
                        {
                            ldec_familyaccbalance = 0.0M; //家庭基金不需变化
                        }
                        else
                        {
                            try
                            {
                                cmd.CommandText = "UPDATE mz_hosppatinfo2 SET chkpayid = CASE " + optype.ToString() + " WHEN 1 THEN 1 ELSE 0 END FROM mz_hosppatinfo2 WHERE hospitalcode='" + hospitalcode + "' AND hospcode='" + hospid + "'";
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                myTrans.Rollback();
                                mess = "更新个人账户异常：" + e.Message.ToString();
                                return false;
                            }
                        }
                        #endregion
                        #region //更新家庭账户信息
                        try
                        {
                            //cmd.Transaction = myTrans;
                            cmd.CommandText = "UPDATE n_familycwuchkkp SET otpatcount = a.a1,otpatientfee = a.a2,inotpatientfee = a.a3,otpatredeem = a.a4,";
                            cmd.CommandText += "familyaccbalance = familyaccbalance + " + ldec_familyaccbalance.ToString();
                            cmd.CommandText += " FROM (Select Sum(IsNull(otpatcount,0)) a1,Sum(IsNull(otpatientfee,0)) a2,";
                            cmd.CommandText += "Sum(IsNull(inotpatientfee,0)) a3,Sum(IsNull(otpatredeem,0)) a4";
                            cmd.CommandText += " From n_individcwuchkkp a ,n_individdata b";
                            cmd.CommandText += " Where a.human = b.human And a.statisticyear = '" + statisticyear + "'";
                            cmd.CommandText += " And a.statisticyear = b.statisticyear And b.familyno = '" + familyno + "') a";
                            cmd.CommandText += " WHERE n_familycwuchkkp.familyno = '" + familyno + "' AND n_familycwuchkkp.statisticyear = '" + statisticyear + "'";
                            cmd.ExecuteNonQuery();
                            myTrans.Commit();
                        }
                        catch (Exception e)
                        {
                            myTrans.Rollback();
                            mess = "更新家庭账户补助异常：" + e.Message.ToString();
                            return false;
                        }
                        #endregion
                    }
                    #endregion
                    else
                    #region
                    {
                        try
                        {
                            //cmd.Transaction = myTrans;
                            cmd.CommandText = "UPDATE n_familycwuchkkp SET otpatredeem = a.a4";
                            cmd.CommandText += " From (Select Sum(IsNull(otpatredeem,0)) a4 From n_individcwuchkkp a ,n_individdata b";
                            cmd.CommandText += " Where a.human = b.human And a.statisticyear = '" + statisticyear + "'";
                            cmd.CommandText += " And a.statisticyear = b.statisticyear And b.familyno = '" + familyno + "') a";
                            cmd.CommandText += " WHERE n_familycwuchkkp.familyno = '" + familyno + "' AND n_familycwuchkkp.statisticyear = '" + statisticyear + "'";
                            cmd.ExecuteNonQuery();
                            myTrans.Commit();
                        }
                        catch (Exception e)
                        {
                            myTrans.Rollback();
                            mess = "更新家庭账户补助异常：" + e.Message.ToString();
                            return false;
                        }
                    }
                    #endregion
                    #endregion
                    break;
            }
            return true;
        }
        catch (Exception e)
        {
            mess = "异常：" + e.Message.ToString();
            return false;
        }
        finally
        {
            if (Connection.State == ConnectionState.Open)
                Connection.Close();
            cmd.Dispose();
            myTrans.Dispose();
        }

    }
    #endregion

    #region 农合基金结算操作
    /// <summary>
    /// 指定年月的基金结算单查询
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="orgcode"></param>
    /// <param name="month"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string foundationSettleBillQuery(string areacode, string orgcode, string month, out string data)
    {
        data = "";
        string mess = "";
        #region //月份格式有效性验证
        if (month == null || month.Trim().Length == 0)
        {
            mess += "month不能为空";
        }
        else
        {
            if (!isDateTime(month.Substring(0, 4) + "-" + month.Substring(4, 2) + "-01 00:00:01"))
            {
                mess += "month格式必须为'200901'的形式";
            }
        }
        if (mess != "")
        {
            mess = getXMLErroStrFromString("2000", mess);
            return mess;
        }
        #endregion
        if (getDBConnStr("areacode", areacode, out mess))
        {
            string ls_sql = null;
            DataSet myDs = null;
            ls_sql = "SELECT b.bill as bill,case when b.state=1 then '已结算' else '取消结算' end as state,b.payfee as hjsj,b.mustpayfee as zydf,b.deductfee1 as zyshmxkc,b.deductfee2 as zyjsmxkc,";
            ls_sql += "b.punishfee as zycf,b.prereservefee as zyscyl,b.reservefee as zybcyl,b.noreservefee as zywfhscyl,b.payfee_hosp as zysj,b.startdate as zyksrq,b.enddate as zyjsrq";
            ls_sql += ",b.mustpayfee_mz as mzdf,b.deductfee1_mz as mzshmxkc,b.deductfee2_mz as mzjsmxkc,b.punishfee_mz as mzcf,b.prereservefee_mz as mzscyl,b.reservefee_mz as mzbcyl";
            ls_sql += ",b.noreservefee_mz as mzwfhscyl,b.payfee_mz as mzsj,b.startdate_mz as mzksrq,b.enddate_mz as mzjsrq,b.accounter as jsr,convert(varchar,b.accountdate,120) as jssj,b.markinfo as jssm";
            ls_sql += " FROM accountinfo_hosp b";
            ls_sql += " WHERE b.hospitalcode='" + orgcode + "' AND convert(varchar(6),b.accountdate,112)='" + month + "'";
            //生成数据
            myDs = getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                getRequestXMLStringFromDS(myDs, "实际结算金额=(医院垫付金额+上次预留金额)-(审核明细扣除金额+结算明细扣除金额+处罚金额+本次预留金额+未返还上次预留金额)", out mess, out data);
            }
            else
            {
                mess = getXMLErroStrFromString("5000", mess);
            }
            myDs.Dispose();
            return mess;
        }
        else
        {
            mess = getXMLErroStrFromString("5000", mess);
            return mess;
        }
    }

    /// <summary>
    /// 指定基金结算单数据查询
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="bill"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string foundationSettleBillData(string areacode, string orgcode, string bill, out string data)
    {
        data = "";
        string mess = "";
        if (getDBConnStr("areacode", areacode, out mess))
        {
            string ls_sql = null;
            DataSet myDs = null;
            //门诊住院各审核状态分类汇总数据
            ls_sql = "SELECT '医疗费用' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
            ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
            ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'M' as jzlx";
            ls_sql += " FROM accountinfo_mz a,mz_pataccountdetail p";
            ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospcode=p.HospCode) AND (p.chkpayid=0)";
            ls_sql += " UNION ";
            ls_sql += "SELECT '预审保内' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
            ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
            ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'M' as jzlx";
            ls_sql += " FROM accountinfo_mz a,mz_pataccountdetail p";
            ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospcode=p.HospCode)";
            ls_sql += " AND (p.chkpayid=(SELECT max(chkpayid) FROM mz_pataccountdetail WHERE (hospitalcode=a.hospitalcode AND HospCode=a.hospcode) AND (chkpayid=8 OR chkpayid=1)))";
            ls_sql += " UNION ";
            ls_sql += "SELECT TOP 1 '审核保内' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
            ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
            ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'M' as jzlx";
            ls_sql += " FROM accountinfo_mz a,mz_pataccountdetail p";
            ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospcode=p.HospCode)";
            ls_sql += " AND (p.chkpayid=(SELECT min(chkpayid) FROM mz_pataccountdetail WHERE (hospitalcode=a.hospitalcode AND HospCode=a.hospcode) AND (chkpayid=8 OR chkpayid=1)))";
            ls_sql += " UNION ";
            ls_sql += "SELECT '医疗费用' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
            ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
            ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'Z' as jzlx";
            ls_sql += " FROM accountinfo a,pataccountdetail p";
            ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospid=p.hospid) AND (p.chkpayid=0)";
            ls_sql += " UNION ";
            ls_sql += "SELECT '预审保内' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
            ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
            ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'Z' as jzlx";
            ls_sql += " FROM accountinfo a,pataccountdetail p";
            ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospid=p.hospid)";
            ls_sql += " AND (p.chkpayid=(SELECT max(chkpayid) FROM pataccountdetail WHERE (hospitalcode=a.hospitalcode AND hospid=a.hospid) AND (chkpayid=8 OR chkpayid=1)))";
            ls_sql += " UNION ";
            ls_sql += "SELECT TOP 1 '审核保内' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
            ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
            ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'Z' as jzlx";
            ls_sql += " FROM accountinfo a,pataccountdetail p";
            ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospid=p.hospid)";
            ls_sql += " AND (p.chkpayid=(SELECT min(chkpayid) FROM pataccountdetail WHERE (hospitalcode=a.hospitalcode AND hospid=a.hospid) AND (chkpayid=8 OR chkpayid=1)))";
            //生成门诊住院各审核状态分类汇总数据
            myDs = getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                if (getXMLRowStringFromDS(myDs, out mess))
                {
                    data = "<FLHZ>" + mess + "</FLHZ>";
                }
                else
                {
                    data = "";
                    mess = getXMLErroStrFromString("5000", mess);
                    return mess;
                }
            }
            else
            {
                data = "";
                mess = getXMLErroStrFromString("5000", mess);
                return mess;
            }
            myDs.Dispose();

            //门诊住院结算明细数据
            ls_sql = "SELECT i.name as xm,s.cityname as dz,f.familyno as jth,f.bookcard as zkh,f.validno as authid,";
            ls_sql += "m.hospcode as jzh,convert(varchar(10),m.hospdate,120) as jzksrq,convert(varchar(10),m.outdate,120) as jzjsrq,h.HospResu as zd,";
            ls_sql += "m.grandPrice as zje,c.mediprice -c.mediselffee +c.materprice -c.materselffee +c.serverprice -c.serverselffee as bnje,";
            ls_sql += "c.payorder as bch,c.subsum as bcje,convert(varchar(19),c.paytime,120) as bczfsj,0.0 as qzxzje,0.0 as qzewbcje,0.0 as qzecbcje,";
            ls_sql += "0.0 as qzczbc,0.0 as qzmzbc,m.familyfund as qzjtzhzc,m.medifee as ypje,c.mediselffee as ypzfje,c.subsum as ysje,m.payfee as shje,m.accountfee as jsje,m.detailfee as kkje,";
            ls_sql += "'M' as jzlx,'农合' as brlx,";
            ls_sql += "case zc.lx when 10 then '普通' when 20 then '慢病' when 30 then '体检' when 40 then '留观' else '其他' end as bclx";
            ls_sql += " FROM accountinfo_hosp a,accountinfo_mz m,mz_checkpay c,n_individdata i,n_familydata f,sysarea s,mz_hosppatinfo2 h,";
            ls_sql += "(select hosptype as lx,areacode as area,orderid as id from mz_arearcmsparm union select 20 as lx,areacode as area,orderid as id from mz_arearcmsparm2) zc";
            ls_sql += " WHERE a.bill='" + bill + "' AND a.hospitalcode='" + orgcode + "' AND a.bill=m.bill AND c.payorder=m.payorder AND h.hospitalcode=m.hospitalcode";
            ls_sql += " AND h.HospCode=m.hospcode AND i.human=c.human AND i.statisticyear=c.statisticyear AND f.statisticyear=i.statisticyear AND f.familyno=i.familyno";
            ls_sql += " AND s.areacode=m.patarea1 AND s.areacode2=m.patarea2 AND zc.area=m.patarea1 AND zc.id=c.orderid";
            ls_sql += " UNION ";
            ls_sql += "SELECT h.PATIENTNAME as xm,s.cityname as dz,h.MEDINUMB as jth,h.medinumb1 as zkh,f.validno as authid,";
            ls_sql += "convert(varchar,h.hospid) as jzh,convert(varchar(10),h.hospdate,120) as jzksrq,convert(varchar(10),h.OutDate,120) as jzjsrq,h.HospResu as zd,";
            ls_sql += "z.grandPrice as zje,z.grandprice -z.selffee as bnje,";
            ls_sql += "z.hospitalcode+convert(varchar,z.hospid) as bch,h.subsum as bcje,convert(varchar(19),h.subtime,120) as bczfsj,i.addfee2 as qzxzje,i.addfee3 as qzewbcje,i.addfee4 as qzecbcje,";
            ls_sql += "i.pay_cz as qzczbc,i.civilfee as qzmzbc,z.familyfund as qzjtzhzc,z.medifee as ypje,z.medifee -z.medipayfee as ypzfje,h.subsum as ysje,z.payfee as shje,z.accountfee as jsje,z.detailfee as kkje,";
            ls_sql += "'Z' as jzlx,h.patsort as brlx,";
            ls_sql += "case when h.gonextstate=1 then '转下流程' when h.ifhurt=1 then '意外伤害' when h.orderid>10000 then '单病种' when h.orderid< -10000 then '大额补偿' else '一般住院' end as bclx";
            ls_sql += " FROM accountinfo_hosp a,accountinfo z,hosppatinfo2 h,hosp_check_info i,n_familydata f,sysarea s";
            ls_sql += " WHERE a.bill='" + bill + "' AND a.hospitalcode='" + orgcode + "' AND a.bill=z.bill AND h.hospitalcode=z.hospitalcode AND h.hospid=z.hospid AND h.hospitalcode=i.hospitalcode";
            ls_sql += " AND h.hospid=i.hospid AND f.familyno=h.MEDINUMB AND f.statisticyear=convert(varchar(4),h.HospDate,112) AND s.areacode=z.patarea1 AND s.areacode2=z.patarea2";
            //生成门诊住院结算明细数据
            myDs = getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                //getRequestXMLStringFromDS(myDs, bill + "结算数据", out mess, out data);
                //public bool getXMLRowStringFromDS(DataSet a_ds, out string rowxml)
                if (getXMLRowStringFromDS(myDs, out mess))
                {
                    data += "<JSMX>" + mess + "</JSMX>";
                }
                else
                {
                    data = "";
                    mess = getXMLErroStrFromString("5000", mess);
                    return mess;
                }
            }
            else
            {
                data = "";
                mess = getXMLErroStrFromString("5000", mess);
                return mess;
            }
            myDs.Dispose();

            //构造完整的data
            data = "<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "UTF-8" + '"' + "?><DATA>" + data + "</DATA>";
            mess = getXMLStrFromString("结算单相关数据");

            return mess;
        }
        else
        {
            mess = getXMLErroStrFromString("5000", mess);
            return mess;
        }
    }

    /// <summary>
    /// 指定基金结算单数据查询--汇总信息
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="bill"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string foundationSettleBillData_collect(string areacode, string orgcode, string settletype,string bill, out string data)
    {
        data = "";
        string mess = "";
        if (getDBConnStr("areacode", areacode, out mess))
        {
            string ls_sql = null;
            DataSet myDs = null;

            if (settletype.ToUpper() == "ZY")
            {
                //住院各审核状态分类汇总数据
                ls_sql = "SELECT '医疗费用' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
                ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
                ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'Z' as jzlx";
                ls_sql += " FROM accountinfo a,pataccountdetail p";
                ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospid=p.hospid) AND (p.chkpayid=0)";
                ls_sql += " UNION ";
                ls_sql += "SELECT '预审保内' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
                ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
                ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'Z' as jzlx";
                ls_sql += " FROM accountinfo a,pataccountdetail p";
                ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospid=p.hospid)";
                ls_sql += " AND (p.chkpayid=(SELECT max(chkpayid) FROM pataccountdetail WHERE (hospitalcode=a.hospitalcode AND hospid=a.hospid) AND (chkpayid=8 OR chkpayid=1)))";
                ls_sql += " UNION ";
                ls_sql += "SELECT TOP 1 '审核保内' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
                ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
                ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'Z' as jzlx";
                ls_sql += " FROM accountinfo a,pataccountdetail p";
                ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospid=p.hospid)";
                ls_sql += " AND (p.chkpayid=(SELECT min(chkpayid) FROM pataccountdetail WHERE (hospitalcode=a.hospitalcode AND hospid=a.hospid) AND (chkpayid=8 OR chkpayid=1)))";
            }
            else if (settletype.ToUpper() == "MZ")
            {
                //门诊各审核状态分类汇总数据
                ls_sql = "SELECT '医疗费用' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
                ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
                ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'M' as jzlx";
                ls_sql += " FROM accountinfo_mz a,mz_pataccountdetail p";
                ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospcode=p.HospCode) AND (p.chkpayid=0)";
                ls_sql += " UNION ";
                ls_sql += "SELECT '预审保内' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
                ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
                ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'M' as jzlx";
                ls_sql += " FROM accountinfo_mz a,mz_pataccountdetail p";
                ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospcode=p.HospCode)";
                ls_sql += " AND (p.chkpayid=(SELECT max(chkpayid) FROM mz_pataccountdetail WHERE (hospitalcode=a.hospitalcode AND HospCode=a.hospcode) AND (chkpayid=8 OR chkpayid=1)))";
                ls_sql += " UNION ";
                ls_sql += "SELECT TOP 1 '审核保内' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
                ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
                ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'M' as jzlx";
                ls_sql += " FROM accountinfo_mz a,mz_pataccountdetail p";
                ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospcode=p.HospCode)";
                ls_sql += " AND (p.chkpayid=(SELECT min(chkpayid) FROM mz_pataccountdetail WHERE (hospitalcode=a.hospitalcode AND HospCode=a.hospcode) AND (chkpayid=8 OR chkpayid=1)))";
            }
            else
            {
                data = "";
                mess = getXMLErroStrFromString("5000", "无效的结算单类型。");
                return mess;
            }           

           
            //生成门诊住院各审核状态分类汇总数据
            myDs = getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                if (getXMLRowStringFromDS(myDs, out mess))
                {
                    data = "<FLHZ>" + mess + "</FLHZ>";
                }
                else
                {
                    data = "";
                    mess = getXMLErroStrFromString("5000", mess);
                    return mess;
                }
            }
            else
            {
                data = "";
                mess = getXMLErroStrFromString("5000", mess);
                return mess;
            }
            myDs.Dispose();

            //构造完整的data
            data = "<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "UTF-8" + '"' + "?><DATA>" + data + "</DATA>";
            mess = getXMLStrFromString("结算单相关数据");

            return mess;
        }
        else
        {
            mess = getXMLErroStrFromString("5000", mess);
            return mess;
        }
    }

    /// <summary>
    /// 指定基金结算单明细数据查询
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="bill"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string foundationSettleBillData_detail(string areacode, string orgcode, string settletype, string bill, int startrow, int endrow, out string data)
    {
        data = "";
        string mess = "";
        if (getDBConnStr("areacode", areacode, out mess))
        {
            string ls_sql = null;
            DataSet myDs = null;

            if (startrow < 0 || endrow < 0)
            {
                data = "";
                mess = getXMLErroStrFromString("2000", "调用参数的开始行和结束行都不能小于0。不能下载！");
                return mess;
            }
            else if ((startrow == 0 && endrow != 0) || (startrow != 0 && endrow == 0))
            {
                data = "";
                mess = getXMLErroStrFromString("2000", "调用参数的开始行和结束行必须同时为0，表示获取可下载结算明细条数。");
                return mess;
            }
            else if (startrow > endrow)
            {
                data = "";
                mess = getXMLErroStrFromString("2000", "调用参数的开始行大于结束行。不能下载！");
                return mess;
            }
            else if (endrow - startrow > 199)
            {
                data = "";
                mess = getXMLErroStrFromString("2000", "调用请求超过200行。不能下载！");
                return mess;
            }

            if (settletype == "MZ")
            {
                ls_sql = "SELECT Count(*) as detailcount ";
                ls_sql += " FROM accountinfo_hosp a,accountinfo_mz m,mz_checkpay c,n_individdata i,n_familydata f,sysarea s,mz_hosppatinfo2 h,";
                ls_sql += "(select hosptype as lx,areacode as area,orderid as id from mz_arearcmsparm union select 20 as lx,areacode as area,orderid as id from mz_arearcmsparm2) zc";
                ls_sql += " WHERE a.bill='" + bill + "' AND a.bill=m.bill AND c.payorder=m.payorder AND h.hospitalcode=m.hospitalcode";
                ls_sql += " AND h.HospCode=m.hospcode AND i.human=c.human AND i.statisticyear=c.statisticyear AND f.statisticyear=i.statisticyear AND f.familyno=i.familyno";
                ls_sql += " AND s.areacode=m.patarea1 AND s.areacode2=m.patarea2 AND zc.area=m.patarea1 AND zc.id=c.orderid";
            }
            else
            {
                ls_sql = "SELECT Count(*) as detailcount ";
                ls_sql += " FROM accountinfo_hosp a,accountinfo z,hosppatinfo2 h,hosp_check_info i,n_familydata f,sysarea s";
                ls_sql += " WHERE a.bill='" + bill + "' AND a.bill=z.bill AND h.hospitalcode=z.hospitalcode AND h.hospid=z.hospid AND h.hospitalcode=i.hospitalcode";
                ls_sql += " AND h.hospid=i.hospid AND f.familyno=h.MEDINUMB AND f.statisticyear=convert(varchar(4),h.HospDate,112) AND s.areacode=z.patarea1 AND s.areacode2=z.patarea2";
            }
            myDs = getDataSet(ls_sql, out mess);
            try
            {
                if (mess == "TRUE")
                {
                    if ((startrow == endrow && startrow == 0))
                    {
                        getRequestXMLStringFromDS(myDs, "得到结算明细的数据总条数", out mess, out data);
                        return mess;
                    }
                    if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) == 0)
                    {
                        data = "";
                        mess = getXMLErroStrFromString("2000", "没有数据可以下载！");
                        return mess;
                    }
                    if (startrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        data = "";
                        mess = getXMLErroStrFromString("2000", "调用参数的开始行超过药品目录总条数" + myDs.Tables[0].Rows[0][0].ToString() + "。不能下载！");
                        return mess;
                    }
                    if (endrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        endrow = Convert.ToInt32(myDs.Tables[0].Rows[0][0]);
                    }
                }
                else
                {
                    data = "";
                    mess = getXMLErroStrFromString("5000", mess);
                    return mess;
                }
            }
            finally
            {
                myDs.Dispose();
            }

            if (settletype == "ZY")
            {
                ls_sql = "Select zz.xm,zz.dz,zz.jth,zz.zkh,zz.authid,";
                ls_sql += "zz.jzh,zz.jzksrq,zz.jzjsrq,zz.zd,";
                ls_sql += "zz.bnje,zz.bch,zz.bcje,zz.bczfsj,zz.qzxzje,zz.qzewbcje,zz.qzecbcje,";
                ls_sql += "zz.qzczbc,zz.qzmzbc,zz.qzjtzhzc,zz.ypje,zz.ypzfje,zz.ysje,zz.shje,zz.jsje,zz.kkje,";
                ls_sql += "zz.jzlx,zz.brlx,zz.bclx ";
                ls_sql += "From ( ";
                ls_sql += "SELECT ROW_NUMBER() OVER (ORDER BY h.hospid) as rownum,h.PATIENTNAME as xm,s.cityname as dz,h.MEDINUMB as jth,h.medinumb1 as zkh,f.validno as authid,";
                ls_sql += "convert(varchar,h.hospid) as jzh,convert(varchar(10),h.hospdate,120) as jzksrq,convert(varchar(10),h.OutDate,120) as jzjsrq,h.HospResu as zd,";
                ls_sql += "z.grandPrice as zje,z.grandprice -z.selffee as bnje,";
                ls_sql += "z.hospitalcode+convert(varchar,z.hospid) as bch,h.subsum as bcje,convert(varchar(19),h.subtime,120) as bczfsj,i.addfee2 as qzxzje,i.addfee3 as qzewbcje,i.addfee4 as qzecbcje,";
                ls_sql += "i.pay_cz as qzczbc,i.civilfee as qzmzbc,z.familyfund as qzjtzhzc,z.medifee as ypje,z.medifee -z.medipayfee as ypzfje,h.subsum as ysje,z.payfee as shje,z.accountfee as jsje,z.detailfee as kkje,";
                ls_sql += "'Z' as jzlx,h.patsort as brlx,";
                ls_sql += "case when h.gonextstate=1 then '转下流程' when h.ifhurt=1 then '意外伤害' when h.orderid>10000 then '单病种' when h.orderid< -10000 then '大额补偿' else '一般住院' end as bclx";
                ls_sql += " FROM accountinfo_hosp a,accountinfo z,hosppatinfo2 h,hosp_check_info i,n_familydata f,sysarea s";
                ls_sql += " WHERE a.bill='" + bill + "' AND a.bill=z.bill AND h.hospitalcode=z.hospitalcode AND h.hospid=z.hospid AND h.hospitalcode=i.hospitalcode";
                ls_sql += " AND h.hospid=i.hospid AND f.familyno=h.MEDINUMB AND f.statisticyear=convert(varchar(4),h.HospDate,112) AND s.areacode=z.patarea1 AND s.areacode2=z.patarea2 ";
                ls_sql += ") zz";
                ls_sql += " WHERE zz.rownum BETWEEN " + startrow.ToString() + " AND " + endrow.ToString(); 
            }
            else
            {
                ls_sql = "SELECT zz.xm,zz.dz,zz.jth,zz.zkh,zz.authid,";
                ls_sql += "zz.jzh,zz.jzksrq,zz.jzjsrq,zz.zd,";
                ls_sql += "zz.zje,zz.bnje,";
                ls_sql += "zz.bch,zz.bcje,zz.bczfsj,zz.qzxzje,zz.qzewbcje,zz.qzecbcje,";
                ls_sql += "zz.qzczbc,zz.qzmzbc,zz.qzjtzhzc,zz.ypje,zz.ypzfje,zz.ysje,zz.shje,zz.jsje,zz.kkje,";
                ls_sql += "zz.jzlx,zz.brlx,zz.bclx ";
                ls_sql += "From ( ";
                ls_sql += "SELECT ROW_NUMBER() OVER (ORDER BY m.hospcode) as rownum, i.name as xm,s.cityname as dz,f.familyno as jth,f.bookcard as zkh,f.validno as authid,";
                ls_sql += "m.hospcode as jzh,convert(varchar(10),m.hospdate,120) as jzksrq,convert(varchar(10),m.outdate,120) as jzjsrq,h.HospResu as zd,";
                ls_sql += "m.grandPrice as zje,c.mediprice -c.mediselffee +c.materprice -c.materselffee +c.serverprice -c.serverselffee as bnje,";
                ls_sql += "c.payorder as bch,c.subsum as bcje,convert(varchar(19),c.paytime,120) as bczfsj,0.0 as qzxzje,0.0 as qzewbcje,0.0 as qzecbcje,";
                ls_sql += "0.0 as qzczbc,0.0 as qzmzbc,m.familyfund as qzjtzhzc,m.medifee as ypje,c.mediselffee as ypzfje,c.subsum as ysje,m.payfee as shje,m.accountfee as jsje,m.detailfee as kkje,";
                ls_sql += "'M' as jzlx,'农合' as brlx,";
                ls_sql += "case zc.lx when 10 then '普通' when 20 then '慢病' when 30 then '体检' when 40 then '留观' else '其他' end as bclx";
                ls_sql += " FROM accountinfo_hosp a,accountinfo_mz m,mz_checkpay c,n_individdata i,n_familydata f,sysarea s,mz_hosppatinfo2 h,";
                ls_sql += "(select hosptype as lx,areacode as area,orderid as id from mz_arearcmsparm union select 20 as lx,areacode as area,orderid as id from mz_arearcmsparm2) zc";
                ls_sql += " WHERE a.bill='" + bill + "' AND a.bill=m.bill AND c.payorder=m.payorder AND h.hospitalcode=m.hospitalcode";
                ls_sql += " AND h.HospCode=m.hospcode AND i.human=c.human AND i.statisticyear=c.statisticyear AND f.statisticyear=i.statisticyear AND f.familyno=i.familyno";
                ls_sql += " AND s.areacode=m.patarea1 AND s.areacode2=m.patarea2 AND zc.area=m.patarea1 AND zc.id=c.orderid ";
                ls_sql += " ) zz";
                ls_sql += " WHERE zz.rownum BETWEEN " + startrow.ToString() + " AND " + endrow.ToString(); 
            }

            myDs = getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                //getRequestXMLStringFromDS(myDs, bill + "结算数据", out mess, out data);
                //public bool getXMLRowStringFromDS(DataSet a_ds, out string rowxml)
                if (getXMLRowStringFromDS(myDs, out mess))
                {
                    data += "<JSMX>" + mess + "</JSMX>";
                }
                else
                {
                    data = "";
                    mess = getXMLErroStrFromString("5000", mess);
                    return mess;
                }
            }
            else
            {
                data = "";
                mess = getXMLErroStrFromString("5000", mess);
                return mess;
            }
            myDs.Dispose();

            //构造完整的data
            data = "<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "UTF-8" + '"' + "?><DATA>" + data + "</DATA>";
            mess = getXMLStrFromString("结算单相关数据");

            return mess;

        }
        else
        {
            data = "";
            mess = getXMLErroStrFromString("5000", mess);
            return mess;
        }
    }

    #endregion

    #endregion

    #region HISonline WebService 接口业务基础方法

    /// <summary>
    /// HISonline WebService URL地址获取
    /// </summary>
    /// <param name="servername"></param>
    /// <param name="mess"></param>
    /// <returns></returns>
    public bool getWebServiceURL(string servername, out string mess)
    {
        if (servername == null || servername.Length == 0 || !isValidParm(servername))
        {
            mess = "提供的HIS服务器名不正确";
            return false;
        }
        try
        {
            mess = "";
            //创建一个XmlTextReader类的对象并调用Read方法来读取XML文件
            XmlTextReader txtReader = new XmlTextReader(Server.MapPath("./Services/" + servername + ".xml"));
            txtReader.Read();
            txtReader.Read();
            //找到符合的节点获取需要的属性值
            while (txtReader.Read())
            {
                txtReader.MoveToElement();
                if (txtReader.Name == "server")
                {
                    mess = txtReader.GetAttribute("URL");
                    break;
                }
            }
            if (mess == "")
            {
                mess = "获取" + servername + "的接口地址错误，请检查配置文件！";
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            if (e.Message.ToString().Contains("未能找到文件"))
            {
                mess = "没有找到" + servername + "的接口地址配置参数！";
            }
            else
            {
                mess = "获取" + servername + "的接口地址配置参数错误：" + e.Message.ToString();
            }
            return false;
        }
    }

    /// <summary>
    /// 通用传入SQL从HISonline接口获取HIS数据，用DataSet返回
    /// </summary>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <param name="sql"></param>
    /// <param name="retMessXML"></param>
    /// <param name="lds"></param>
    /// <returns></returns>
    public bool getDataFromHIS(string servername, string dbname, string user, string pwd, string sql, out string retMessXML, out DataSet lds)
    {
        retMessXML = "";
        lds = null;
        if (sql == null || sql.Trim().Length == 0)
        {
            retMessXML = getXMLErroStrFromString("2000", "没有指定SQL语句");
            return false;
        }
        string hisifurl = "", requestParmXML = "";
        if (!getWebServiceURL(servername, out hisifurl))
        {
            retMessXML = getXMLErroStrFromString("2000", hisifurl);
            return false;
        }
        requestParmXML = "<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "UTF-8" + '"' + "?>";
        requestParmXML += "<ROW><requesttype>0000</requesttype>";
        requestParmXML += "<sql><![CDATA[" + sql + "]]></sql>";        
        requestParmXML += "</ROW>";
        try
        {
            //创建his的Webservice接口并调用获取住院数据的方法
            hisservice his = new hisservice(hisifurl);
            if (his.rdhlhnhisif(servername, dbname, user, pwd, requestParmXML, lds, out lds, out retMessXML))
            {
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            retMessXML = getXMLErroStrFromString("9000", "调用HIS接口异常：" + e.Message.ToString());
            return false;
        }
    }

    /// <summary>
    /// 通用传入SQL从HISonline接口获取HIS数据，用XML返回
    /// </summary>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <param name="sql"></param>
    /// <param name="retMessXML"></param>
    /// <param name="retDataXML"></param>
    /// <returns></returns>
    public bool getDataFromHIS(string servername, string dbname, string user, string pwd, string sql, out string retMessXML, out string retDataXML)
    {
        retMessXML = "";
        retDataXML = "";
        DataSet lds = new DataSet();
        if (getDataFromHIS(servername, dbname, user, pwd, sql, out retMessXML, out lds))
        {
            return getRequestXMLStringFromDS(lds, "", out retMessXML, out retDataXML);
        }
        return false;
    }
    
    /// <summary>
    /// 使用指定的业务号调用HIS接口向其发送dataset
    /// </summary>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <param name="data"></param>
    /// <param name="ywh"></param>
    /// <param name="retMessXML"></param>
    /// <param name="lds"></param>
    /// <returns></returns>
    public bool putDataToHIS(string servername, string dbname, string user, string pwd, DataSet data, string ywh, out string retMessXML, out DataSet lds)
    {    
        return putDataToHIS(servername, dbname, user, pwd, data, ywh,"", out retMessXML, out lds);
    }

    /// <summary>
    /// 调用上一个putDataToHIS方法，传入其他参数
    /// </summary>
    /// <param name="servername"></param>
    /// <param name="dbname"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <param name="data"></param>
    /// <param name="ywh"></param>
    /// <param name="otherparm"></param>
    /// <returns></returns>
    public bool putDataToHIS(string servername, string dbname, string user, string pwd, DataSet data, string ywh, string otherparm, out string retMessXML, out DataSet lds)
    {        
        retMessXML = "";
        lds = null;
        if (ywh == null || ywh.Trim().Length == 0)
        {
            retMessXML = getXMLErroStrFromString("2000", "没有指定调用HIS接口的业务号");
            return false;
        }
        string hisifurl = "", requestParmXML = "";
        if (!getWebServiceURL(servername, out hisifurl))
        {
            retMessXML = getXMLErroStrFromString("2000", hisifurl);
            return false;
        }
        requestParmXML = "<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "UTF-8" + '"' + "?>";
        requestParmXML += "<ROW><requesttype>"+ywh+"</requesttype>";
        if (!(otherparm == null || otherparm.Trim().Length == 0))
        {
            requestParmXML += otherparm;
        }
        requestParmXML += "</ROW>";
        try
        {
            //创建his的Webservice接口并调用获取住院数据的方法
            hisservice his = new hisservice(hisifurl);
            if (his.rdhlhnhisif(servername, dbname, user, pwd, requestParmXML, data, out lds, out retMessXML))
            {
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            retMessXML = getXMLErroStrFromString("9000", "调用HIS接口异常：" + e.Message.ToString());
            return false;
        }
    }
    #endregion

    #region WebService方法

    /// <summary>
    /// 测试到WebService的联接是否正常
    /// </summary>
    /// <returns></returns>
    [WebMethod(Description = "测试联接农合系统是否正常")]
    public string testConnection(string areacode)
    {
        if (areacode == null || areacode.Length == 0 || !isValidParm(areacode))
        {
            return getXMLErroStrFromString("1000", "客户端程序联接农合接口正常，提供的区域代码不正确，不能测试数据库联接！");
        }
        string mess = "";
        if (getDBConnStr("areacode", areacode, out mess))
        {
            //联接串中要去掉这个关键词 Provider=SQLOLEDB.1;
            SqlConnection Connection = new SqlConnection(mess.Replace("Provider=SQLOLEDB.1;", ""));
            try
            {
                Connection.Open();
                cmdand cmd = new cmdand();
                cmd.Connection = Connection;
                try
                {
                    cmd.CommandText = "SELECT count(*) FROM sexcode";
                    if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                    {
                        return getXMLStrFromString("客户端调用接口联接农合系统正常！");
                    }
                    else
                    {
                        return getXMLErroStrFromString("1000", "客户端程序联接农合接口正常，联接数据库不成功！");
                    }
                }
                catch (Exception e)
                {
                    return getXMLErroStrFromString("9000", "客户端程序联接农合接口正常，联接数据库异常：" + e.Message.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            finally
            {
                Connection.Close();
                Connection.Dispose();
            }
        }
        else
        {
            return getXMLErroStrFromString("5000", "客户端程序联接农合接口正常，" + mess + "请检查区域代码是否正确");
        }
    }

    /// <summary>
    /// 融达互联湖南农合WebService调用入口(将出口数据合并到返回值中的方法)
    /// </summary>
    /// <param name="requestParmXML"></param>
    /// <param name="uploadDataXML"></param>
    /// <returns></returns>
    [WebMethod(Description = "调用接口处理业务的方法2")]
    public string rdhlhnncmsif2(string requestParmXML, string uploadDataXML)
    {
        string retstring="", retXML="";
        retstring = rdhlhnncmsif(requestParmXML, uploadDataXML,out retXML);
        if (retXML.IndexOf("</DESC>") > 0)
        {
            retXML = retXML.Replace("<DESC>", "<DATA>");
            retXML = retXML.Replace("</DESC>", "");
            if (retstring.IndexOf("</DATA>") > 0)
            {
                retstring = retstring.Replace("<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "UTF-8" + '"' + "?><DATA>", "");
            }
            else
            {
                retstring += "</DATA>";
            }
            retstring = retXML + retstring;
            return retstring;
        }
        return retXML;
    }

    /// <summary>
    /// 融达互联湖南农合WebService调用入口(注意业务请求编码规则"0XX表示不需要验证身份的业务编码)
    /// </summary>
    /// <param name="requestParmXML"></param>
    /// <param name="uploadDataXML"></param>
    /// <param name="retDataXML"></param>
    /// <returns></returns>
    [WebMethod( Description = "调用接口处理业务的方法")]
    public string rdhlhnncmsif(string requestParmXML, string uploadDataXML, out string retDataXML)
    {
        //请注意，以下retDataXML和本方法的返回值在PB下获取证明，如果retDataXML中在本方法中
        //存储的是数据，return的是状态，则PB下调用时刚好获取到反的，即return值是data，出参
        //获取到的确是状态。因此，在下列方法中，将这两个值交换过来返回到前端，以使PB中调用
        //return的是状态，出参中获取的是data
        string mess = "";
        string errtxt = "";
        retDataXML = "";
        try
        {
            XmlDocument xmldoc = getXMLDocumentFromString(requestParmXML, out mess);
            if (mess == "TRUE")
            {
                try
                {
                    string areacode = null;
                    string requesttype = null;
                    string hospitalcode = null;
                    string ls_tmp = null;
                    XmlNodeList lis = xmldoc.GetElementsByTagName("areacode");
                    areacode = lis[0].InnerText;
                    lis = xmldoc.GetElementsByTagName("requesttype");
                    requesttype = lis[0].InnerText;
                    if (requesttype.Substring(0, 2) == "00") //不需要身份验证的。注意这个编码规冿
                    #region
                    {
                        switch (requesttype)
                        {
                            case "0001"://根据医院代码得到本医院的信息
                                #region
                                lis = xmldoc.GetElementsByTagName("hospitalcode");
                                hospitalcode = lis[0].InnerText;
                                if (!isValidParm(hospitalcode))
                                {
                                    return getXMLErroStrFromString("2000", "参数hospitalcode错误");
                                }
                                retDataXML = getHospitalInfo(areacode, hospitalcode, out mess);
                                return mess;
                            //break;
                                #endregion
                            case "0002"://根据医院名称模糊查询机构信息
                                #region
                                string hospitalname;
                                lis = xmldoc.GetElementsByTagName("hospitalname");
                                hospitalname = lis[0].InnerText;
                                if (!isValidParm(hospitalname))
                                {
                                    return getXMLErroStrFromString("2000", "参数hospitalname错误");
                                }
                                retDataXML = getHospitalsInfo(areacode, hospitalname, out mess);
                                return mess;
                            //break;
                                #endregion
                            case "0003"://城居接口工具版本号获取
                                retDataXML = getCJJKVer();
                                return mess;
                            default:
                                #region
                                retDataXML = getXMLErroStrFromString("2000", "没有定义的业务请求编码：" + requesttype + "");
                                return "";
                            //break;
                                #endregion
                        }
                    }
                    #endregion
                    else //以下都是需要身份验证的业务
                    #region
                    {
                        string userid, pwd;
                        lis = xmldoc.GetElementsByTagName("hospitalcode");
                        hospitalcode = lis[0].InnerText;
                        if (!isValidParm(hospitalcode))
                        {
                            retDataXML = getXMLErroStrFromString("2000", "参数hospitalcode错误");
                            return "";
                        }
                        lis = xmldoc.GetElementsByTagName("userid");
                        userid = lis[0].InnerText;
                        if (!isValidParm(userid))
                        {
                            retDataXML = getXMLErroStrFromString("2000", "参数userid错误");
                            return "";
                        }
                        lis = xmldoc.GetElementsByTagName("password");
                        pwd = lis[0].InnerText;
                        if (!isValidParm(pwd))
                        {
                            retDataXML = getXMLErroStrFromString("2000", "参数password错误");
                            return "";
                        }
                        mess = checkUserValid(areacode, hospitalcode, userid, pwd);
                        if (mess != "TRUE")
                        {
                            retDataXML = getXMLErroStrFromString("2001", mess);
                            return "";
                        }
                        else
                        {
                            ncmsifml ml = new ncmsifml(DBServer, DBName, UserID, PassWord, DBConnStr, IsValidUser, AreaCode, InterFacePower);
                            ncmsifmz mz = new ncmsifmz(DBServer, DBName, UserID, PassWord, DBConnStr, IsValidUser, AreaCode, InterFacePower);
                            ncmsifzy zy = new ncmsifzy(DBServer, DBName, UserID, PassWord, DBConnStr, IsValidUser, AreaCode, InterFacePower);
                            switch (requesttype)
                            {
                                #region //用户身份验证
                                case "1000"://用户身份验证
                                    #region
                                    retDataXML = getXMLStrFromString("用户身份验证通过");
                                    return "";
                                //break;
                                    #endregion
                                #endregion
                                #region //磁卡读卡解密
                                case "1005":                                   
                                    #region
                                    string statisticyear, bookno;
                                    lis = xmldoc.GetElementsByTagName("year");
                                    statisticyear = lis[0].InnerText;
                                    if (!isValidParm(statisticyear))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数year错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("medicard");
                                    bookno = lis[0].InnerText;
                                    if (!isValidParm(bookno))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数medicard错误");
                                        return "";
                                    }
                                    retDataXML = getCardInfo(areacode, statisticyear, bookno, out mess);
                                    return mess;
                                    #endregion
                                #endregion
                                #region //获取参合数据
                                case "1010"://获取参合数据
                                    #region
                                    string year, medicard, authid, id, bankcard;
                                    lis = xmldoc.GetElementsByTagName("year");
                                    year = lis[0].InnerText;
                                    if (!isValidParm(year))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数year错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("medicard");
                                    medicard = lis[0].InnerText;
                                    if (!isValidParm(medicard))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数medicard错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("authid");
                                    authid = lis[0].InnerText;
                                    if (!isValidParm(authid))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数authid错误");
                                        return "";
                                    }
                                    //如果medicard>16位长，则16位后的是卡识别码
                                    if (medicard.Length > 16)
                                    {
                                        authid = medicard.Substring(16);
                                    }
                                    lis = xmldoc.GetElementsByTagName("id");
                                    id = lis[0].InnerText;
                                    if (!isValidParm(id))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数id错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("bankcard");
                                    bankcard = lis[0].InnerText;
                                    if (!isValidParm(bankcard))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数bankcard错误");
                                        return "";
                                    }
                                    retDataXML = getPersonsInfo(areacode, year, medicard, authid, id, bankcard, out mess);
                                    return mess;
                                    #endregion
                                #endregion
                                #region //目录操作
                                case "1101"://得到本医院使用的农合住院药品目录
                                    #region
                                    if (InterFacePower == 10)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startmedi1, endmedi1;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "开始行参数";
                                    startmedi1 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "结束行参数：";
                                    endmedi1 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogMedi(areacode, hospitalcode, startmedi1, endmedi1, out mess);
                                    return mess;
                                    //break;
                                    #endregion
                                case "1102"://上传本医院的住院药品匹配数据
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "应包含数据的XML字符串不能为空");
                                        return "";
                                    }
                                    retDataXML = ml.putCatalogMedi(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                    //break;
                                    #endregion
                                case "1103"://得到本医院已审核通过的住院药品匹配数据
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startmedi3, endmedi3;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "开始行参数";
                                    startmedi3 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "结束行参数：";
                                    endmedi3 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogMediAudited(areacode, hospitalcode, startmedi3, endmedi3, out mess);
                                    return mess;
                                    //break;
                                    #endregion
                                case "1104"://删除本医院上传的还未审核通过的住院药品匹配数据
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "应包含数据的XML字符串不能为空");
                                        return "";
                                    }
                                    retDataXML = ml.delCatalogMedi(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1105"://得到本医院使用的农合门诊药品目录
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startmedi5, endmedi5;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "开始行参数";
                                    startmedi5 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "结束行参数：";
                                    endmedi5 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogMediMZ(areacode, hospitalcode, startmedi5, endmedi5, out mess);
                                    return mess;
                                //break;
                                    #endregion
                                case "1106"://上传本医院的门诊药品匹配数据
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "应包含数据的XML字符串不能为空");
                                        return "";
                                    }
                                    retDataXML = ml.putCatalogMediMZ(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                //break;
                                    #endregion
                                case "1107"://得到本医院已审核通过的门诊药品匹配数据
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startmedi7, endmedi7;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "开始行参数";
                                    startmedi7 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "结束行参数：";
                                    endmedi7 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogMediAuditedMZ(areacode, hospitalcode, startmedi7, endmedi7, out mess);
                                    return mess;
                                //break;
                                    #endregion
                                case "1108"://删除本医院上传的还未审核通过的门诊药品匹配数据
                                    #region
                                    if (InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "应包含数据的XML字符串不能为空");
                                        return "";
                                    }
                                    retDataXML = ml.delCatalogMediMZ(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1111"://得到本医院使用的农合住院医疗服务目录
                                    #region
                                    if (InterFacePower == 10)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startsvr1, endsvr1;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "开始行参数";
                                    startsvr1 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "结束行参数：";
                                    endsvr1 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogServer(areacode, hospitalcode, startsvr1, endsvr1, out mess);
                                    return mess;
                                    //break;
                                    #endregion
                                case "1112"://上传本医院的住院医疗服务匹配数据
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "应包含数据的XML字符串不能为空");
                                        return "";
                                    }
                                    retDataXML = ml.putCatalogServer(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1113"://得到本医院已审核通过的住院医疗服务匹配数据
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startsvr3, endsvr3;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "开始行参数";
                                    startsvr3 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "结束行参数：";
                                    endsvr3 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogServerAudited(areacode, hospitalcode, startsvr3, endsvr3, out mess);
                                    return mess;
                                    //break;
                                    #endregion
                                case "1114"://删除本医院上传的还未审核通过的住院医疗服务匹配数据
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "应包含数据的XML字符串不能为空");
                                        return "";
                                    }
                                    retDataXML = ml.delCatalogServer(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1115"://得到本医院使用的农合门诊医疗服务目录
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startsvr15, endsvr15;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "开始行参数";
                                    startsvr15 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "结束行参数：";
                                    endsvr15 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogServerMZ(areacode, hospitalcode, startsvr15, endsvr15, out mess);
                                    return mess;
                                //break;
                                    #endregion
                                case "1116"://上传本医院的门诊医疗服务匹配数据
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "应包含数据的XML字符串不能为空");
                                        return "";
                                    }
                                    retDataXML = ml.putCatalogServerMZ(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1117"://得到本医院已审核通过的门诊医疗服务匹配数据
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startsvr7, endsvr7;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "开始行参数";
                                    startsvr7 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "结束行参数：";
                                    endsvr7 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogServerAuditedMZ(areacode, hospitalcode, startsvr7, endsvr7, out mess);
                                    return mess;
                                //break;
                                    #endregion
                                case "1118"://删除本医院上传的还未审核通过的门诊医疗服务匹配数据
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "应包含数据的XML字符串不能为空");
                                        return "";
                                    }
                                    retDataXML = ml.delCatalogServerMZ(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1121"://得到本医院使用的ICD-10目录
                                    #region
                                    errtxt = "";
                                    int starticd1, endicd1;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "开始行参数";
                                    starticd1 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "结束行参数：";
                                    endicd1 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogICD10(areacode, hospitalcode, starticd1, endicd1, out mess);
                                    return mess;
                                    //break;
                                    #endregion
                                case "1122"://上传本医院的ICD-10匹配数据
                                    #region
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "应包含数据的XML字符串不能为空");
                                        return "";
                                    }
                                    retDataXML = ml.putCatalogICD10(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1123"://得到本医院已审核的ICD-10数据
                                    #region
                                    errtxt = "";
                                    int starticd3, endicd3;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "开始行参数";
                                    starticd3 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "结束行参数：";
                                    endicd3 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogICD10Audited(areacode, hospitalcode, starticd3, endicd3, out mess);
                                    return mess;
                                //break;
                                    #endregion
                                case "1124"://删除本医院上传的还未审核通过的ICD-10匹配数据
                                    #region
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "应包含数据的XML字符串不能为空");
                                        return "";
                                    }
                                    retDataXML = ml.delCatalogICD10(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1151"://城居接口下载本院农合匹配信息
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startmedi51, endmedi51;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "开始行参数";
                                    startmedi51 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "结束行参数：";
                                    endmedi51 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getNhppb_cjjk(areacode, hospitalcode, startmedi51, endmedi51, out mess);
                                    return mess;
                                    #endregion
                                case "1152"://城居接口下载城居信息
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startmedi52, endmedi52;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "开始行参数";
                                    startmedi52 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "结束行参数：";
                                    endmedi52 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalog_cjjk(areacode, hospitalcode, startmedi52, endmedi52, out mess);
                                    return mess;
                                    #endregion
                                case "1153"://上传城居目录匹配信息(药品材料+医疗服务)
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "应包含数据的XML字符串不能为空");
                                        return "";
                                    }
                                    retDataXML = ml.putCatalog_cjjk(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                //break;
                                    #endregion
                                case "1154"://同步本医院的城居目录匹配数据
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startsvr1154, endsvr1154;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "开始行参数";
                                    startsvr1154 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "结束行参数：";
                                    endsvr1154 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogSyn_cjjk(areacode, hospitalcode, startsvr1154, endsvr1154, out mess);
                                    return mess;
                                //break;
                                    #endregion
                                #endregion
                                #region //门诊病人操作
                                case "1201"://从农合系统中查询一个门诊病人资料
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("mzh");//门诊号
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数mzh错误");
                                        return "";
                                    }
                                    retDataXML = mz.getMZPatientInfo(areacode, hospitalcode, ls_tmp, out mess);
                                    return mess;
                                    #endregion
                                case "1202"://上传门诊农合病人信息(支持多个病人信息上传)
                                    #region

                                    if (!setPDCert(requestParmXML, out retDataXML))
                                        return "";
                                    

                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "应包含数据的XML字符串不能为空");
                                        return "";
                                    }
                                    retDataXML = mz.putMZPatientInfo(areacode, hospitalcode, userid, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1203"://删除一个门诊病人的资料
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("mzh");
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数mzh错误");
                                        return "";
                                    }
                                    retDataXML = mz.delMZPatientInfo(areacode, hospitalcode, ls_tmp);
                                    return "";
                                    #endregion
                                case "1204"://修改一个门诊病人的资料
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("mzh");
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数mzh错误");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "应包含数据的XML字符串不能为空");
                                        return "";
                                    }
                                    retDataXML = mz.editMZPatientInfo(areacode, hospitalcode, ls_tmp, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                #endregion
                                #region //门诊病人费用操作
                                case "1211"://获取指定门诊号的费用数据
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("mzh");
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数mzh错误");
                                        return "";
                                    }
                                    retDataXML = mz.getMZPatientFee(areacode, hospitalcode, ls_tmp, out mess);
                                    return mess;
                                    #endregion
                                case "1212"://上传一个门诊病人的费用
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("mzh");
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数mzh错误");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "应包含数据的XML字符串不能为空");
                                        return "";
                                    }
                                    retDataXML = mz.putMZPatientFee(areacode, hospitalcode, ls_tmp, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1213"://删除一个门诊病人的所有费用
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("mzh");
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数mzh错误");
                                        return "";
                                    }
                                    retDataXML = mz.delMZPatientFee(areacode, hospitalcode, ls_tmp);
                                    return "";
                                    #endregion
                                #endregion
                                #region //门诊病人结算操作
                                case "1221"://得到一个门诊病人的结算资料
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("bch");//补偿号(bch)
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数bch错误");
                                        return "";
                                    }
                                    retDataXML = mz.settleMZQuery(areacode, ls_tmp, out mess);
                                    return mess;
                                    #endregion
                                case "1222": //非特殊慢病门诊试结算
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "应包含数据的XML字符串不能为空");
                                        return "";
                                    }
                                    retDataXML = mz.settleMZ(hospitalcode, "TestOrder", 8, "", "", 0, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1223"://非特殊慢病门诊正式结算
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    string ls_opername1223;
                                    lis = xmldoc.GetElementsByTagName("bch");//补偿号(bch)
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数bch错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("czyxm");//操作员姓名(czyxm)
                                    ls_opername1223 = Convert.ToString(lis[0].InnerText);
                                    retDataXML = mz.settleMZ(hospitalcode, ls_tmp, 8, "", ls_opername1223, 1, "", out mess);
                                    return mess;
                                    #endregion
                                case "1224": //非特殊慢病门诊取消结算
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    string ls_opername1224;
                                    lis = xmldoc.GetElementsByTagName("bch");//补偿号(bch)
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数bch错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("czyxm");//操作员姓名(czyxm)
                                    ls_opername1224 = Convert.ToString(lis[0].InnerText);
                                    retDataXML = mz.settleMZCancel(areacode, hospitalcode, "", ls_opername1224, ls_tmp);
                                    return "";
                                    #endregion
                                case "1225": //非特殊慢病门诊申请支付
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    string ls_opername1225, ls_pay1225;
                                    lis = xmldoc.GetElementsByTagName("bch");//补偿号(bch)
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数bch错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("pay");//实际补偿额(pay)
                                    ls_pay1225 = Convert.ToString(lis[0].InnerText);
                                    if (ls_pay1225 == null || ls_pay1225.Trim().Length == 0 || !isNumber(ls_pay1225))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数pay错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("czyxm");//操作员姓名(czyxm)
                                    ls_opername1225 = Convert.ToString(lis[0].InnerText);
                                    retDataXML = mz.settleMZSetPay(hospitalcode, ls_tmp, ls_pay1225, ls_opername1225);
                                    return "";
                                    #endregion
                                case "1226": //申请支付后是否可以取消结算
                                    #region 
                                    retDataXML = mz.settleMZQueryCancelabled(areacode, out mess);
                                    return mess;
                                    #endregion
                                case "991225": //非特殊慢病门诊审核
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    string ls_opercode991225, ls_opername991225;
                                    lis = xmldoc.GetElementsByTagName("bch");//补偿号(bch)
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数bch错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("czydm");//操作员代码(czydm)
                                    ls_opercode991225 = Convert.ToString(lis[0].InnerText);
                                    lis = xmldoc.GetElementsByTagName("czyxm");//操作员姓名(czyxm)
                                    ls_opername991225 = Convert.ToString(lis[0].InnerText);
                                    retDataXML = mz.settleMZ(hospitalcode, ls_tmp, 1, ls_opercode991225, ls_opername991225, 1, "", out mess);
                                    return mess;
                                    #endregion
                                case "991226": //非特殊慢病门诊取消审核
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    return "";
                                    #endregion
                                #endregion
                                #region //体检业务操作
                                case "1301"://下载指定体检日期的有效健康体检包
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    retDataXML = mz.getTJHealthyPacket(areacode, hospitalcode, out mess);
                                    return mess;
                                    #endregion
                                case "1302"://下载已审核未体检的申报花名册
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    errtxt = "";
                                    int starttj, endtj;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "开始行参数";
                                    starttj = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "结束行参数：";
                                    endtj = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = mz.getTJDeclareRoster(areacode, hospitalcode, starttj, endtj, out mess);
                                    return mess;
                                    #endregion
                                #endregion
                                #region //住院病人操作
                                case "2201"://从农合系统中查询一个住院病人资料
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("zyid");//住院ID号
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数zyid错误");
                                        return "";
                                    }
                                    retDataXML = zy.getZYPatientInfo(areacode, hospitalcode, ls_tmp, out mess);
                                    return mess;
                                    #endregion
                                case "2202"://上传住院农合病人信息(支持多个病人信息上传)
                                    #region
                                    
                                    if (!setPDCert(requestParmXML, out retDataXML))
                                        return "";

                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "应包含数据的XML字符串不能为空");
                                        return "";
                                    }
                                    retDataXML = zy.putZYPatientInfo(areacode, hospitalcode, userid, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "2203"://删除一个住院病人的资料
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("zyid");//住院ID号
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数zyid错误");
                                        return "";
                                    }
                                    retDataXML = zy.delZYPatientInfo(areacode, hospitalcode, ls_tmp);
                                    return "";
                                    #endregion
                                #endregion
                                #region //住院病人费用操作
                                case "2211"://获取指定住院号的费用数据
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("zyid");//住院ID号
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数zyid错误");
                                        return "";
                                    }
                                    retDataXML = zy.getZYPatientFee(areacode, hospitalcode, ls_tmp, out mess);
                                    return mess;
                                    #endregion
                                case "2212"://上传一个住院病人的费用
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("zyid");//住院ID号
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数zyid错误");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "应包含数据的XML字符串不能为空");
                                        return "";
                                    }
                                    retDataXML = zy.putZYPatientFee(areacode, hospitalcode, ls_tmp, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "2213"://删除一个住院病人的所有费用
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("zyid");//住院ID号
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数zyid错误");
                                        return "";
                                    }
                                    retDataXML = zy.delZYPatientFee(areacode, hospitalcode, ls_tmp);
                                    return "";
                                    #endregion
                                #endregion
                                #region//住院病人结算操作
                                case "2222"://试结算
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "应包含数据的XML字符串不能为空");
                                        return "";
                                    }
                                    retDataXML = zy.settleZY(hospitalcode,0, 8, "", "", 0, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "2223"://正式结算
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }

                                    string opername_2223 = "";
                                    decimal hospid;

                                    try
                                    {
                                        lis = xmldoc.GetElementsByTagName("zyid");//住院ID号
                                        ls_tmp = Convert.ToString(lis[0].InnerText);
                                        if (!isNumber(ls_tmp))
                                        {
                                            retDataXML = "住院号必须是数字型，请检查住院号。";
                                            return mess;
                                        }
                                        hospid = Convert.ToDecimal(ls_tmp);
                                    }
                                    catch (Exception ee)
                                    {
                                        retDataXML = "获取住院号失败:" + ee.Message.ToString();
                                        return mess;
                                    }

                                    lis = xmldoc.GetElementsByTagName("czyxm");//操作员姓名(czyxm)
                                    opername_2223 = Convert.ToString(lis[0].InnerText);
                                    retDataXML = zy.settleZY(hospitalcode, hospid, 8, "", opername_2223, 1, "", out mess);
                                    return mess;
                                    #endregion

                                #endregion
                                #region HISOnline从HISWebservice获取数据
                                case "4101":
                                    #region 下载住院药品材料目录
                                    string servername4101, dbname4101;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4101 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4101))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数server错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4101 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4101))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数db错误");
                                        return "";
                                    } 
                                    retDataXML = ml.getZYCatalogMediFromHIS(areacode, hospitalcode, servername4101, dbname4101, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4102":
                                    #region 上传住院药品材料医疗服务匹配数据
                                    string servername4102, dbname4102;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4102 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4102))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数server错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4102 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4102))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数db错误");
                                        return "";
                                    }
                                    retDataXML = ml.putZYPPHIS(areacode, hospitalcode, servername4102, dbname4102, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4103":
                                    #region 下载住院匹配审核数据
                                    string servername4103, dbname4103;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4103 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4103))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数server错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4103 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4103))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数db错误");
                                        return "";
                                    }
                                    retDataXML = ml.getZYPPHIS(areacode, hospitalcode, servername4103, dbname4103, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4105":
                                    #region 获取门诊药品材料目录
                                    string servername4105, dbname4105;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4105 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4105))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数server错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4105 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4105))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数db错误");
                                        return "";
                                    }
                                    retDataXML = ml.getMZCatalogMediFromHIS(areacode, hospitalcode, servername4105, dbname4105, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4106":
                                    #region 上传门诊药品材料医疗服务匹配数据
                                    string servername4106, dbname4106;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4106 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4106))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数server错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4106 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4106))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数db错误");
                                        return "";
                                    }
                                    retDataXML = ml.putMZPPHIS(areacode, hospitalcode, servername4106, dbname4106, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4107":
                                    #region 下载门诊匹配审核数据
                                    string servername4107, dbname4107;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4107 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4107))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数server错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4107 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4107))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数db错误");
                                        return "";
                                    }
                                    retDataXML = ml.getMZPPHIS(areacode, hospitalcode, servername4107, dbname4107, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4111":
                                    #region 获取住院医疗服务目录
                                    string servername4111, dbname4111;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4111 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4111))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数server错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4111 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4111))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数db错误");
                                        return "";
                                    }
                                    retDataXML = ml.getZYCatalogServerFromHIS(areacode, hospitalcode, servername4111, dbname4111, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4115":
                                    #region 获取门诊医疗服务目录
                                    string servername4115, dbname4115;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4115 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4115))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数server错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4115 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4115))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数db错误");
                                        return "";
                                    }
                                    retDataXML = ml.getMZCatalogServerFromHIS(areacode, hospitalcode, servername4115, dbname4115, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4121":
                                    #region 获取ICD10目录
                                    string servername4121, dbname4121;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4121 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4121))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数server错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4121 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4121))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数db错误");
                                        return "";
                                    }
                                    retDataXML = ml.getCatalogICD10HIS(areacode, hospitalcode, servername4121, dbname4121, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4122":
                                    #region 上传ICD-10匹配数据
                                    string servername4122, dbname4122;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4122 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4122))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数server错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4122 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4122))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数db错误");
                                        return "";
                                    }
                                    retDataXML = ml.putNHICD10PPHIS(areacode, hospitalcode, servername4122, dbname4122, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4123":
                                    #region 下载已审核的ICD-10匹配数据
                                    string servername4123, dbname4123;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4123 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4123))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数server错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4123 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4123))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数db错误");
                                        return "";
                                    }
                                    retDataXML = ml.getNHICD10PPHIS(areacode, hospitalcode, servername4123, dbname4123, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4201"://上传一个病人资料
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    string servername4201, dbname4201, hospid4201;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4201 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4201))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数server错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4201 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4201))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数db错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("zyid");//住院ID号
                                    hospid4201 = Convert.ToString(lis[0].InnerText);
                                    if (hospid4201 == null || hospid4201.Trim().Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数zyid不能为空");
                                        return "";
                                    }
                                    else
                                    {
                                        if (!isNumber(hospid4201))
                                        {
                                            retDataXML = getXMLErroStrFromString("2000", "参数zyid必须为数字型");
                                            return "";
                                        }
                                    }
                                    retDataXML = zy.putZYPatientFromHIS(areacode, hospitalcode, servername4201, dbname4201, "RDHNHISWebService", "RDHN_NCMS1.0Used", hospid4201);
                                    return "";
                                    #endregion
                                case "4202"://上传住院费用数据
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    }
                                    string servername4202, dbname4202, hospid4202, startdate4202, enddate4202;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4202 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4202))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数server错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4202 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4202))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数db错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("zyid");//住院ID号
                                    hospid4202 = Convert.ToString(lis[0].InnerText);
                                    if (hospid4202 == null || hospid4202.Trim().Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数zyid不能为空");
                                        return "";
                                    }
                                    else
                                    {
                                        if (!isNumber(hospid4202))
                                        {
                                            retDataXML = getXMLErroStrFromString("2000", "参数zyid必须为数字型");
                                            return "";
                                        }
                                    }
                                    lis = xmldoc.GetElementsByTagName("ksrq");
                                    startdate4202 = Convert.ToString(lis[0].InnerText);
                                    if (!isDateTime(startdate4202.Substring(0, 4) + "-" + startdate4202.Substring(4, 2) + "-" + startdate4202.Substring(6, 2) + " 00:00:00"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "ksrq格式必须为'20090125'的格式");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("jsrq");
                                    enddate4202 = Convert.ToString(lis[0].InnerText);
                                    if (!isDateTime(enddate4202.Substring(0, 4) + "-" + enddate4202.Substring(4, 2) + "-" + enddate4202.Substring(6, 2) + " 00:00:00"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "jsrq格式必须为'20090125'的格式");
                                        return "";
                                    }
                                    retDataXML = zy.putZYPatientFeeFromHIS(areacode, hospitalcode, servername4202, dbname4202, "RDHNHISWebService", "RDHN_NCMS1.0Used", hospid4202, startdate4202, enddate4202);
                                    return "";
                                    #endregion
                                case "4203"://上传住院病人和费用数据
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "住院业务操作接口未许可");
                                        return "";
                                    } 
                                    string servername4203, dbname4203, hospid4203, startdate4203, enddate4203;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4203 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4203))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数server错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4203 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4203))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数db错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("zyid");//住院ID号
                                    hospid4203 = Convert.ToString(lis[0].InnerText);
                                    if (hospid4203 == null || hospid4203.Trim().Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数zyid不能为空");
                                        return "";
                                    }
                                    else
                                    {
                                        if (!isNumber(hospid4203))
                                        {
                                            retDataXML = getXMLErroStrFromString("2000", "参数zyid必须为数字型");
                                            return "";
                                        }
                                    }
                                    lis = xmldoc.GetElementsByTagName("ksrq");
                                    startdate4203 = Convert.ToString(lis[0].InnerText);
                                    if (!isDateTime(startdate4203.Substring(0, 4) + "-" + startdate4203.Substring(4, 2) + "-" + startdate4203.Substring(6, 2) + " 00:00:00"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "ksrq格式必须为'20090125'的格式");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("jsrq");
                                    enddate4203 = Convert.ToString(lis[0].InnerText);
                                    if (!isDateTime(enddate4203.Substring(0, 4) + "-" + enddate4203.Substring(4, 2) + "-" + enddate4203.Substring(6, 2) + " 00:00:00"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "jsrq格式必须为'20090125'的格式");
                                        return "";
                                    }
                                    retDataXML = zy.putZYDataFromHIS(areacode, hospitalcode, servername4203, dbname4203, "RDHNHISWebService", "RDHN_NCMS1.0Used", hospid4203, startdate4203, enddate4203);
                                    return "";
                                    #endregion
                                case "4212"://上传门诊费用数据
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    string servername4212, dbname4212, hospcode4212;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4212 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4212))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数server错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4212 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4212))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数db错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("mzh");//门诊号
                                    hospcode4212 = Convert.ToString(lis[0].InnerText);
                                    if (hospcode4212 == null || hospcode4212.Trim().Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数mzh不能为空");
                                        return "";
                                    }
                                    retDataXML = mz.putMZPatientFeeFromHIS(areacode, hospitalcode, servername4212, dbname4212, "RDHNHISWebService", "RDHN_NCMS1.0Used", hospcode4212);
                                    return "";
                                    #endregion
                                case "4501"://下载指定日期段的普通门诊数据
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    string servername4501, dbname4501, system4501, startdate4501, enddate4501;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4501 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4501))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数server错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4501 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4501))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数db错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("system");
                                    system4501 = Convert.ToString(lis[0].InnerText);
                                    lis = xmldoc.GetElementsByTagName("startdate");//开始日期
                                    startdate4501 = Convert.ToString(lis[0].InnerText);
                                    if (!isDateTime(startdate4501.Substring(0, 4) + "-" + startdate4501.Substring(4, 2) + "-" + startdate4501.Substring(6, 2) + " 00:00:00"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "startdate格式必须为'20090125'的格式");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("enddate");//结束日期
                                    enddate4501 = Convert.ToString(lis[0].InnerText);
                                    if (!isDateTime(enddate4501.Substring(0, 4) + "-" + enddate4501.Substring(4, 2) + "-" + enddate4501.Substring(6, 2) + " 00:00:00"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "enddate格式必须为'20090125'的格式");
                                        return "";
                                    }
                                    retDataXML = mz.getMZCommData(hospitalcode, servername4501, dbname4501, system4501, "RDHNHISWebService", "RDHN_NCMS1.0Used", "0", startdate4501, enddate4501, out mess);
                                    return mess;
                                    #endregion
                                case "4502"://下载指定日期段的体检数据
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "门诊业务操作接口未许可");
                                        return "";
                                    }
                                    string servername4502, dbname4502, system4502, startdate4502, enddate4502;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4502 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4502))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数server错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4502 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4502))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数db错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("system");
                                    system4502 = Convert.ToString(lis[0].InnerText);
                                    lis = xmldoc.GetElementsByTagName("startdate");//开始日期
                                    startdate4502 = Convert.ToString(lis[0].InnerText);
                                    if (!isDateTime(startdate4502.Substring(0, 4) + "-" + startdate4502.Substring(4, 2) + "-" + startdate4502.Substring(6, 2) + " 00:00:00"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "startdate格式必须为'20090125'的格式");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("enddate");//结束日期
                                    enddate4502 = Convert.ToString(lis[0].InnerText);
                                    if (!isDateTime(enddate4502.Substring(0, 4) + "-" + enddate4502.Substring(4, 2) + "-" + enddate4502.Substring(6, 2) + " 00:00:00"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "enddate格式必须为'20090125'的格式");
                                        return "";
                                    }
                                    retDataXML = mz.getMZCommData(hospitalcode, servername4502, dbname4502, system4502, "RDHNHISWebService", "RDHN_NCMS1.0Used", "1", startdate4502, enddate4502, out mess);
                                    return mess;
                                    #endregion
                                case "HISSQL": //查询HIS数据
                                    #region
                                    string servernamehissql, dbnamehissql, sqlhissql;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servernamehissql = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servernamehissql))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数server错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbnamehissql = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbnamehissql))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数db错误");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("sql");
                                    sqlhissql = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(sqlhissql))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数sql错误");
                                        return "";
                                    }
                                    getDataFromHIS(servernamehissql, dbnamehissql, "RDHNHISWebService", "RDHN_NCMS1.0Used", sqlhissql, out retDataXML, out mess);
                                    return mess;
                                    #endregion
                                #endregion
                                #region 县级结算操作
                                case "5001":
                                    #region 指定年月的基金结算单查询
                                    lis = xmldoc.GetElementsByTagName("month");//年月
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数month错误");
                                        return "";
                                    }
                                    retDataXML = foundationSettleBillQuery(areacode, hospitalcode, ls_tmp, out mess);
                                    return mess;
                                    #endregion
                                case "5002":
                                    #region 指定基金结算单数据查询
                                    lis = xmldoc.GetElementsByTagName("bill");//年月
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数bill错误");
                                        return "";
                                    }
                                    retDataXML = foundationSettleBillData(areacode, hospitalcode, ls_tmp, out mess);
                                    return mess;
                                    #endregion
                                case "5003":
                                    #region 指定基金结算单汇总数据查询
                                    lis = xmldoc.GetElementsByTagName("bill");//结算单号
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数bill错误");
                                        return "";
                                    }
                                    string settletype;//结算单类型 MZ:门诊 ZY:住院
                                    lis = xmldoc.GetElementsByTagName("settletype");
                                    settletype =  Convert.ToString(lis[0].InnerText).ToUpper();
                                    if (!(settletype == "MZ" || settletype == "ZY"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数settletype错误");
                                        return "";
                                    }
                                    retDataXML = foundationSettleBillData_collect(areacode, hospitalcode,settletype, ls_tmp, out mess);
                                    return mess;
                                    #endregion
                                case "5004":
                                    #region 指定基金结算单明细数据查询
                                    lis = xmldoc.GetElementsByTagName("bill");//结算单号
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数bill错误");
                                        return "";
                                    }
                                    string bill;
                                    bill = ls_tmp;
                                    
                                    string settletype5004;//结算单类型 MZ:门诊 ZY:住院
                                    lis = xmldoc.GetElementsByTagName("settletype");
                                    settletype5004 = Convert.ToString(lis[0].InnerText).ToUpper();
                                    if (!(settletype5004 == "MZ" || settletype5004 == "ZY"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数settletype错误");
                                        return "";
                                    }

                                    int startrow5004, endrow5004;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isNumber(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数startrow错误");
                                        return "";
                                    }
                                    startrow5004 = Convert.ToInt32(ls_tmp);

                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isNumber(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "参数endrow错误");
                                        return "";
                                    }
                                    endrow5004 = Convert.ToInt32(ls_tmp);
                                    retDataXML = foundationSettleBillData_detail(areacode, hospitalcode, settletype5004, bill,startrow5004,endrow5004,out mess);
                                    return mess;                                    
                                    #endregion
                                #endregion
                                default:
                                    #region
                                    retDataXML = getXMLErroStrFromString("2000", "没有定义的业务请求编码" + requesttype + ")");
                                    return "";
                                    //break;
                                    #endregion
                            }
                        }
                    }
                    #endregion
                }
                catch (Exception e)
                {
                    if (errtxt == "")
                    {
                        retDataXML = getXMLErroStrFromString("9000", "异常：" + e.Message.ToString() + ",可能缺少数据元");
                    }
                    else
                    {
                        retDataXML = getXMLErroStrFromString("9000", errtxt + "异常：" + e.Message.ToString() + ",可能缺少数据元");
                    }
                    return "";
                }
            }
            else
            {
                retDataXML = getXMLErroStrFromString("9000", mess);
                return "";
            }
        }
        catch (Exception e)
        {
            retDataXML = getXMLErroStrFromString("9000", e.Message.ToString());
            return "";
        }
    }

    [WebMethod( Description = "省平台调用接口处理业务的方法")]
    public string[]  rdhlrcmsterrace(string In_aqyz_XML,string In_gnbh,string In_jysr_XML)
    {
        string[] retstr = new string[4];
        string areacode = null,mess = null;

        retstr[0] = "1";//交易返回标志 0:成功,retstr[3]为空且retstr[2]为有效的输出XML 1:失败,retstr[2]为空,retstr[3]包含错误信息
        retstr[1] = "";//交易流水号,暂都为空
        retstr[2] = "";//交易输出XML 当交易成功则返回有效的XML，其它则为空
        retstr[3] = "";//返回信息 当交易失败时返回错误信息 成功则为空

        XmlDocument xdoc = getXMLDocumentFromString(In_jysr_XML, out mess);
        if (mess != "TRUE")
        {
            retstr[3] = mess;
            return retstr;
        }
        XmlNodeList xnlist;

        #region 由于输入XML中没有区域代码，所以只能具体根据业务来初始化类实例
        switch (In_gnbh.ToUpper())
        {
            #region 参合验证
            case "F1020101"://按医疗证+姓名/人编号
            case "F1020102"://按身份证+姓名
            case "F1020103"://按医疗证返回一户信息           
                xnlist = xdoc.GetElementsByTagName("admorg_code");
                areacode = xnlist[0].InnerText;
                if (areacode.Length == 0)
                {
                    retstr[3] = "初始化失败:请填写农合机构代码。";
                    return retstr;
                }
                areacode = areacode.Substring(0,6);
                break;
            #endregion
            #region 转诊申请及取消申请
            case "F1020120":                            
            case "F1020122":
                xnlist = xdoc.GetElementsByTagName("referral_no");
                areacode = xnlist[0].InnerText;
                if (areacode.Length == 0)
                {
                    retstr[3] = "初始化失败:请填写转诊申请单号。";
                    return retstr;
                }
                areacode = areacode.Substring(0, 6);
                break;
            #endregion
            #region 获取转诊信息
            case "F1020131":
                xnlist = xdoc.GetElementsByTagName("admorg_code");
                areacode = xnlist[0].InnerText;
                if (areacode.Length == 0)
                {
                    retstr[3] = "初始化失败:请填写农合机构代码。";
                    return retstr;
                }
                areacode = areacode.Substring(0, 6);
                break;
            #endregion
            #region 获取住院信息
            case "F1020151":
                xnlist = xdoc.GetElementsByTagName("referral_no");
                areacode = xnlist[0].InnerText;
                if (areacode.Length == 0)
                {
                    retstr[3] = "初始化失败:请填写转诊申请单号。";
                    return retstr;
                }
                areacode = areacode.Substring(0, 6);
                break;
            #endregion
            #region 取消住院信息
            case "F1020153":
                xnlist = xdoc.GetElementsByTagName("referral_no");
                areacode = xnlist[0].InnerText;
                if (areacode.Length == 0)
                {
                    retstr[3] = "初始化失败:请填写转诊申请单号。";
                    return retstr;
                }
                areacode = areacode.Substring(0, 6);
                break;
            #endregion
            #region 上传住院信息
            case "F1020154":
                xnlist = xdoc.GetElementsByTagName("referral_no");
                areacode = xnlist[0].InnerText;
                if (areacode.Length == 0)
                {
                    retstr[3] = "初始化失败:请填写转诊申请单号。";
                    return retstr;
                }
                areacode = areacode.Substring(0, 6);
                break;
                #endregion
            #region 费用核对
            case "F1020161":
                xnlist = xdoc.GetElementsByTagName("referral_no");
                areacode = xnlist[0].InnerText;
                if (areacode.Length == 0)
                {
                    retstr[3] = "初始化失败:请填写转诊申请单号。";
                    return retstr;
                }
                areacode = areacode.Substring(0, 6);
                break;
            #endregion
            #region 出院申请
            case "F1020171":
                xnlist = xdoc.GetElementsByTagName("referral_no");
                areacode = xnlist[0].InnerText;
                if (areacode.Length == 0)
                {
                    retstr[3] = "初始化失败:请填写转诊申请单号。";
                    return retstr;
                }
                areacode = areacode.Substring(0, 6);
                break;
            #endregion
            #region 取消出院申请
            case "F1020172":
                xnlist = xdoc.GetElementsByTagName("referral_no");
                areacode = xnlist[0].InnerText;
                if (areacode.Length == 0)
                {
                    retstr[3] = "初始化失败:请填写转诊申请单号。";
                    return retstr;
                }
                areacode = areacode.Substring(0, 6);
                break;
            #endregion
            #region 未定义的业务编号
            default:
                retstr[3] = "未定义的业务编号。";
                return retstr;
            #endregion
        }
        //初始化数据连接变量
        if (!getDBConnStr("areacode", areacode, out mess))
        {
            retstr[3] = mess;
            return retstr;
        }
        #endregion 根据业务号调用相关业务

        ncmsifterrace terraceif;
        try
        {
            terraceif = new ncmsifterrace(DBServer, DBName, UserID, PassWord, DBConnStr, IsValidUser, AreaCode, InterFacePower);
            retstr = terraceif.findService(In_aqyz_XML, In_gnbh, In_jysr_XML);
        }
        catch (Exception e)
        {
            retstr[3] = "交易失败:" + e.Message.ToString();
        }
        finally
        {
            terraceif = null;
        }
        
        return retstr;
    }
    #endregion
}