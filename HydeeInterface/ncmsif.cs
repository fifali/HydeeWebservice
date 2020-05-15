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
    #region ���������
    /// <summary>
    /// ��������
    /// </summary>
    private string DBServer = "";
    /// <summary>
    /// ���ݿ���
    /// </summary>
    private string DBName = "";
    /// <summary>
    /// �û�ID
    /// </summary>
    private string UserID = "";
    /// <summary>
    /// �û�����
    /// </summary>
    private string PassWord = "";
    /// <summary>
    /// ���ݿ����Ӵ�
    /// </summary>
    private string DBConnStr = "";
    /// <summary>
    /// ��ǰ�û��Ƿ���Ч
    /// </summary>
    private bool IsValidUser = false;
    /// <summary>
    /// ��ǰ�û����ڵ����ش���
    /// </summary>
    private string AreaCode = "";
    /// <summary>
    /// �ӿ�ӵ�е�Ȩ��
    /// </summary>
    private int InterFacePower = -1;
    #endregion 
    
    #region ���캯��
    /// <summary>
    /// ���캯��
    /// </summary>
    public ncmsif()
    {
        //���ʹ����Ƶ��������ȡ��ע�������� 
        //InitializeComponent(); 
    }

    /// <summary>
    /// ���캯������ʼ�������
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

    #region ��������
    ~ncmsif()
    {
        Dispose();
    }
    #endregion

    #region ���ݿ����ӵ���������
    /// <summary>
    /// ��ȡ���ݿ����Ӳ�����
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
            //����һ��XmlTextReader��Ķ��󲢵���Read��������ȡXML�ļ�
            XmlTextReader txtReader = new XmlTextReader(Server.MapPath("./DBConn/" + areacode + ".xml"));
            txtReader.Read();
            txtReader.Read();
            //�ҵ����ϵĽڵ��ȡ��Ҫ������ֵ
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
                mess = "��ȡ����" + areacode + "�����ݿ����Ӳ����������������ļ���";
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
            if (e.Message.ToString().Contains("δ���ҵ��ļ�"))
            {
                mess = "������û���ҵ�����" + areacode + "�����ݿ��������ò�����";
            }
            else
            {
                mess = "��ȡ����" + areacode + "�����ݿ����Ӳ�������" + e.Message.ToString();
            }
            return false;
        }
    }

    /// <summary>
    /// ���ݸ����Ĳ������ͻ�����ݿ����Ӵ�
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
                        mess = "ָ�����������������ʹ����޷�ȥ�������ݿ⣡";
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
            mess = "Ѱַ���ݿ��쳣��"+e.Message.ToString();
            return false;
        }
    }

    /// <summary>
    /// �õ���ǰ�û��Ƿ���Ч
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
    /// ���õ�ǰ�û��Ƿ���Ч
    /// </summary>
    /// <param name="IfValid"></param>
    /// <returns></returns>
    public bool setUserState(bool IfValid)
    {
        IsValidUser = IfValid;
        return IfValid;
    }
    #endregion

    #region ��������

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
    /// �ж��ַ����Ƿ�������
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
    /// �ж��ַ����Ƿ�ָ����datetime�͸�ʽ
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
    /// ��֤����ֵ�Ƿ���Ч
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
    /// SQL��ʹ������ֶ�ֵ�к��е����ţ���ʹ��ת���
    /// �磺ֱ�ӿ����򵰰�����(Coombs') ������SQL��ҪתΪ   ֱ�ӿ����򵰰�����(Coombs'')
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
    /// �ڹ���XML֮ǰ�����ַ����а�����XML�ǹ淶�ַ�ʹ��ת���ַ��滻���ݲ�ʹ��
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
    /// ��ָ����DataSet�����DataTable
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
            mess = "���ݼ�����ӱ�Ĳ�������";
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
                //ds.Tables[tabName].BeginLoadData();//����BeginLoadData�������Ż�����
                //myOda.Fill(ds, tabName); //���DataSet
                //ds.Tables[tabName].EndLoadData();
                ////�رմ�OleDbConnection
                //myConn.Close(); //ע�⼰ʱ�ر�����
                ////feeDs.Tables["fees"].PrimaryKey = new DataColumn[] { feeDs.Tables["fees"].Columns["hospitalcode"], feeDs.Tables["fees"].Columns["HospCode"], feeDs.Tables["fees"].Columns["detailid"] };//����һ������
                //myOda.Dispose();
                //mess = "TRUE";
                //return ds;

                SqlConnection myConn = new SqlConnection(ls_mess.Replace("Provider=SQLOLEDB.1;", ""));
                myConn.Open();
                SqlDataAdapter myDA = new SqlDataAdapter(sqlStr, myConn);
                myDA.Fill(ds, tabName);
                myConn.Close(); //ע�⼰ʱ�ر�����
                myDA.Dispose();
                mess = "TRUE";
                return ds;
            }
            catch (Exception e)
            {
                mess = "�������ݼ��쳣��" + e.Message.ToString();
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
    /// ����ָ����SQL��ָ���ı�������DataSet
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
                mess = "û��SQL���޷��������ݼ�!";
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
    /// ����ָ����SQL����DataSet
    /// </summary>
    /// <param name="sqlStr"></param>
    /// <param name="mess"></param>
    /// <returns></returns>
    public DataSet getDataSet(string sqlStr, out string mess)
    {
        return getDataSet(sqlStr, "Table0", out mess);
    }

    /// <summary>
    /// ����DataSet�õ�XML��<ROW></ROW>��ʽ��
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
            rowxml = "���ݼ�����XML�쳣��" + e.Message.ToString();
            return false;
        }
    }

    /// <summary>
    /// ����DataSet�õ�XML��ʽ�ַ���
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
            execresulterrtxt = "���ݼ�����XML�쳣��" + xmltmp;
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
            mess += "<MEMO><![CDATA[" + as_memo + "(û������)]]></MEMO>";
        }
        else
        {
            mess += "<MEMO><![CDATA[" + as_memo + "]]></MEMO>";
        }
        mess += "</ROW></DESC>";
        return true;
    }

    /// <summary>
    /// �Ӵ��������ַ������ɷ��ظ��û��Ĵ�������XML�ַ���
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
    /// �Ӵ��������ַ����ʹ��������ַ������ɷ��ظ��û��Ĵ�������XML�ַ���
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
            data += errodata;//errodata������<ROW>...</ROW><ROW>...</ROW>����ʽ
            data += "</DATA>";
        }
        else
        {
            data = "";
        }
        return xmlstr;
    }

    /// <summary>
    /// ���ַ������ɷ��ظ��û���XML�����ַ���
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
    /// �Ӹ�������������ַ������ɷ��ظ��û���XML�ַ���������XML
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
    /// ͨ��XML��ʽ�ַ������XMLDocument����
    /// </summary>
    /// <param name="as_str"></param>
    /// <param name="mess"></param>
    /// <returns></returns>
    public XmlDocument getXMLDocumentFromString(string as_str, out string mess)
    {
        if (as_str.Length == 0)
        {
            mess = "XML��ʽ�ַ�������Ϊ��";
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
            mess = "����ȷ��XML��ʽ�ַ�����" + e.Message.ToString();
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
            mess = "��ѯϵͳ��ǰʱ���쳣:" + e.Message.ToString();
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

    #region ����ҵ�񷽷�

    #region //����ũ�Ͻӿڲ�Ʒ��ʶ��
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
                mess = getXMLErroStrFromString("9000", "�쳣��" + e.Message.ToString() + ",����ȱ������Ԫ");
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
                    mess = getXMLErroStrFromString("5001","���²�Ʒ��ʶʧ��" + e.Message);
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
                mess = getXMLErroStrFromString("9000", "�����쳣"+ e.Message.ToString());
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

    #region //�û���֤
    /// <summary>
    /// ��֤�û���ݣ����ء�TRUE����ʾ��֤ͨ����InterFacePower��ʾ����ʹ�õĽӿ�ҵ������
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="hospitalcode"></param>
    /// <param name="userid"></param>
    /// <param name="pwd"></param>
    /// <returns></returns>
    //[WebMethod(Description="Service�û������֤�����ء�TRUE����ʾ��֤ͨ��")]
    public string checkUserValid(string areacode, string hospitalcode, string userid, string pwd)
    {
        if (areacode.Length !=6)
        {
            return "�Ƿ���������롣��ʹ�ù���6λ������룡";
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
                if (userid == "RDHL_HIS_Temped" && pwd == "TempUsedPassword") //HISonline����ʱʹ�õ��û�������
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
                        return "��������ҽԺ������û������������������֤ʧ�ܣ�";
                    }
                    else
                    {
                        myReader.Read();
                        InterFacePower = myReader.GetInt32(1);
                        if (userid == "RDHL_HIS_Temped" && pwd == "TempUsedPassword") //HISonline����ʱʹ�õ��û�������
                        {
                            InterFacePower = 0;//סԺ�������ﲻ����	0
                        }
                        else
                        {
                            switch (InterFacePower)
                            {
                                case 100: //������ʹ��	100
                                case 10: //סԺ��������������	10
                                case 1: //סԺ������������	1
                                    break;
                                case 0: //סԺ�������ﲻ����	0
                                case 11: //סԺ�������ﲻ����	11
                                    break;
                                //case -1: //��������ʹ��	-1
                                //    break;
                                default:
                                    setUserState(false);
                                    return "�ӿ�δȡ�����:" + myReader.GetString(0);
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
                return "��֤�쳣��" + e.Message.ToString();
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

    #region //�Ǿӽӿڹ��߰汾�Ż�ȡ
    public string getCJJKVer()
    {
        return getXMLStrFromString("1.0");
    }
    #endregion

    #region //ҽԺ��Ϣ��ȡ
    /// <summary>
    /// ����ҽԺ����õ�����������Ϣ
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="HospitalCode"></param>
    /// <returns></returns>
    //[WebMethod(Description = "����ҽԺ����õ�����������Ϣ")]
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
                    getRequestXMLStringFromDS(myDs, "areaname:������;hospitalname:ҽԺ��;hospitallevel:1-I��ҽԺ2-II��ҽԺ3-III��ҽԺ", out mess,out data);
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
    /// ����ҽԺ����ģ����ѯ������Ϣ
    /// </summary>
    /// <param name="areacode"></param>
    /// <param name="HospitalName"></param>
    /// <returns></returns>
    //[WebMethod(Description = "����ҽԺ����ģ����ѯ������Ϣ")]
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
                    getRequestXMLStringFromDS(myDs, "areaname:������;hospitalname:ҽԺ��;hospitallevel:1-I��ҽԺ2-II��ҽԺ3-III��ҽԺ", out mess, out data);
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

    #region //���ݲ�����Ⱥ;��Ƚ�ȡ�㷨���㲹����
    /// <summary>
    /// ���ݲ�����Ⱥ;��Ƚ�ȡ�㷨���㲹����
    /// </summary>
    /// <param name="adec_values"></param> //������
    /// <param name="ai_chkprecision"></param> //�������  -2 ��Ԫ -1 ʰԪ 0 Ԫ 1 �� 2 ��
    /// <param name="ai_precisionmode"></param> //���Ƚ�ȡ�㷨	Ĭ��ֵ��1���������뷨 2:��β�� 3:��β��
    /// <returns></returns> //�����Ĳ�����
    public decimal calcChkValue(decimal adec_values, Int32 ai_chkprecision, Int32 ai_precisionmode)
    {
        decimal ldec_values = adec_values;
        #region //������֤
        switch (ai_precisionmode)
        {
            case 1:
            case 2:
            case 3:
            case 4:
                break;
            default:
                ai_precisionmode = 1; //Ĭ��ֵ��1���������뷨
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
                ai_chkprecision = 0; //Ĭ��ֵ��0����ȷ��Ԫ
                break;
        }
        #endregion
        switch (ai_precisionmode)
        {
            #region //��������
            case 1: //��������
                switch (ai_chkprecision)
                {
                    case 0: //Ԫ
                    case 1: //��
                    case 2: //��
                        ldec_values = Math.Round(ldec_values + 0.000001M, ai_chkprecision);
                        break;
                    case -1: //ʰԪ
                        ldec_values = Math.Round(ldec_values + 0.000001M / 10.0M, 0) * 10.0M;
                        break;
                    case -2: //��Ԫ
                        ldec_values = Math.Round(ldec_values + 0.000001M / 100.0M, 0) * 100.0M;
                        break;
                }
                break;
            #endregion
            #region //��β��
            case 2: //��β��
                switch (ai_chkprecision)
                {
                    case 0: //Ԫ
                        ldec_values = Convert.ToDecimal(Convert.ToInt32(ldec_values - 0.49999M));
                        break;
                    case 1: //��
                        ldec_values = Convert.ToInt32(ldec_values * 10.0M - 0.49999M) / 10.0M;
                        break;
                    case 2: //��
                        ldec_values = Convert.ToInt32(ldec_values * 100.0M - 0.49999M) / 100.0M;
                        break;
                    case -1: //ʰԪ
                        ldec_values = Convert.ToInt32(ldec_values / 10.0M - 0.49999M) * 10.0M;
                        break;
                    case -2: //��Ԫ
                        ldec_values = Convert.ToInt32(ldec_values / 100.0M - 0.49999M) * 100.0M;
                        break;
                }
                break;
            #endregion
            #region //��β��
            case 3: //��β��
                switch (ai_chkprecision)
                {
                    case 0: //Ԫ
                        ldec_values = Convert.ToDecimal(Convert.ToInt32(ldec_values + 0.50001M));
                        break;
                    case 1: //��
                        ldec_values = Convert.ToInt32(ldec_values * 10 + 0.50001M) / 10.0M;
                        break;
                    case 2: //��
                        ldec_values = Convert.ToInt32(ldec_values * 100.0M + 0.50001M) / 100.0M;
                        break;
                    case -1: //ʰԪ
                        ldec_values = Convert.ToInt32(ldec_values / 10.0M + 0.50001M) * 10.0M;
                        break;
                    case -2: //��Ԫ
                        ldec_values = Convert.ToInt32(ldec_values / 100.0M + 0.50001M) * 100.0M;
                        break;
                }
                break;
            #endregion
        }
        return ldec_values;
    }
    #endregion

    #region //�κ���֤�ͻ�ȡ��Ч�κ�������
    /// <summary>
    /// ���ſ������
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
            mess = getXMLErroStrFromString("2000", "����year��Ч");
            return mess;
        }
        Int32 li_checkpatinfo; //���ϴ���Ǽ�ͥ�Ż���֤����
        #region //���ϴ���Ǽ�ͥ�Ż���֤����
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
                            return getXMLErroStrFromString("2000", "��Ч��ϵͳ����");
                        }
                    }
                    else
                    {
                        return getXMLErroStrFromString("2000", "ϵͳ����������");
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
                return getXMLErroStrFromString("9000", "��ѯϵͳ�����쳣��" + e.Message.ToString());
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
            mess = getXMLErroStrFromString("2000", "����medicard��Ч");
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
                    #region //����
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
                case 0: //���ϴ����ҽ��֤��
                    ls_tmp += " WHERE statisticyear='"+year+"' AND bookcard='"+medicard+"'";
                    break;
                case 1: //���ϴ���Ǽ�ͥ��
                    ls_tmp += " WHERE familyno='" + medicard + "' AND statisticyear='" + year + "'";
                    break;
            }
            DataSet myDs = getDataSet(ls_tmp, out mess);
            if (mess == "TRUE")
            {
                getRequestXMLStringFromDS(myDs, "famiyno:��ͥ��;bookcard:֤����;validno:֤���汾��", out mess, out data);
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
    /// ����ҽ��֤�Ż����֤�ŵõ���Ч�κ�������"
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
            mess = getXMLErroStrFromString("2000", "����year��Ч");
            return mess;
        }
        if (id == "0")
        {
            id = "";
        }
        if (id.Length != 0 && id.Length != 15 && id.Length != 18)
        {
            mess = getXMLErroStrFromString("2000", "����id��Ч");
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
                mess = getXMLErroStrFromString("2000", "�ṩ����medicard����ͬʱ�ṩauthid");
                return mess;
            }
            else
            {
                if (!isNumber(authid))
                {
                    mess = getXMLErroStrFromString("2000", "����authid����Ϊ������");
                    return mess;
                }
            }
        }
        if (medicard.Length == 0 && id.Length == 0 && bankcard.Length == 0)
        {
            mess = getXMLErroStrFromString("2000", "����medicard��id��bankcard����ͬʱΪ��");
            return mess;
        }
        else
            if (getDBConnStr("areacode", areacode, out mess))
            {
                if (medicard.Length > 0) //��ʱ��Ҫͨ����ʶ��������֤�Ƿ���Ч��
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
                                    return getXMLErroStrFromString("2000", "��Ч��֤��");
                                }
                            }
                            else
                            {
                                return getXMLErroStrFromString("2000", "֤��������");
                            }
                        }
                        finally
                        {
                            cmd.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        return getXMLErroStrFromString("9000", "��֤����Ч���쳣��" + e.Message.ToString());
                    }
                    finally
                    {
                        if (Connection.State == ConnectionState.Open)
                            Connection.Close();
                    }
                }
                #endregion
                string ls_sql = "SELECT f.bookcard as zkh,f.name+'(��'+convert(varchar,fc.familyaccbalance)+')' as hz,i.human as grh,r.name as gx,i.name as xm,s.sexname as xb,i.patientid as sfz";
                ls_sql += ",isnull(convert(varchar(10),i.birthday,120),'') as csrq,n.name as xz,isnull(i.ifoutpatient,0) as mb,isnull(convert(varchar(10),i.outpatienttime,120),'') as mbsj";
                ls_sql += ",a.cityname as dz,f.familyno+convert(varchar,f.validno) as jth";
                ls_sql += " FROM n_familydata f,n_individdata i,n_individcwuchkkp c,sysarea a,n_familycwuchkkp fc,";
                ls_sql += "(select code,name from n_standdata where standtab='S101-06') r,";
                ls_sql += "sexcode s,";
                ls_sql += "(select code,name from n_standdata where standtab='nature') n";
                ls_sql += " WHERE f.familyno=i.familyno AND f.statisticyear=i.statisticyear AND i.human=c.human AND i.statisticyear=c.statisticyear AND f.areacode=a.areacode AND f.areacode2=a.areacode2";
                ls_sql += " AND fc.familyno=f.familyno AND fc.statisticyear=f.statisticyear";
                if (medicard.Length > 16) //������ũ��ϵͳ��д����������ͥ��Ϊ16λ�����Ϊҽ��֤ʶ����
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
                    getRequestXMLStringFromDS(myDs, "zkh:֤����;hz:����;grh:���˱��;gx:��ͥ��ϵ;xm:����;xb:�Ա�;sfz:���֤��;csrq:��������;xz:��������;mb:�Ƿ�����(0��1��;mbsj:�������￪ʼʱ��;dz:��ͥ��ַ", out mess, out data);
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
    /// ͨ��ҽ��֤��+���֤��+������ҽ��֤��+���֤�ţ�ҽ��֤��+���������֤��+�������ַ�ʽ��֤�κ���Ч��
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
    /// ��������ֵfalse��mess!=""�����������쳣��false��mess==""����Ч�κ��ߣ�true����Ч�κ���;
    /// ��Ч�κ�ʱ��humanΪΪ���˱��롣areacode2Ϊ������롣natureΪ�������ʡ�orderid��������������š�ifoutpatient�Ƿ��������outpatienttime�������￪ʼʱ��
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
            mess = "����year��Ч";
            return false;
        }
        id = id.Trim();
        if (id == "0"||id == "��")
        {
            id = "";
        }
        if (id.Length != 0 && id.Length != 15 && id.Length != 18)
        {
            mess = "����sfz��Ч�����ȱ���Ϊ0��15��18λ��";
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
                mess = "�ṩ����zkh����ͬʱ�ṩzksbm";
                return false;
            }
            else
            {
                if (!isNumber(authid))
                {
                    mess = "����zksbm����Ϊ������";
                    return false;
                }
            }
        }
        if (humanin.Length == 0 && (namein.Length == 0 && (id.Length == 0 || medicard.Length == 0)) && (id.Length == 0 && (medicard.Length == 0 || namein.Length == 0)) && (medicard.Length == 0 && (id.Length == 0 || namein.Length == 0)))
        {
            mess = "����zkh��sfz��xm��grh�ṩ��ֵ�����";
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
                            mess = "��֤�κ���Ч��ʱ��ѯ�������";
                            return false;
                        }
                        else if (myDs.Tables[0].Rows.Count == 0)
                        {
                            mess = "�κ���Ϣ���в����ڴ���";//û�ҵ���
                            return false;
                        }
                        else
                        {   
                            if (medicard.Length > 0)
                            {
                                if (myDs.Tables[0].Rows[0][7].ToString() != authid)
                                {
                                    mess = "ҽ��֤�������ƣ���ʹ�����Ƶ��¿����˿���ͣ�û��ʧ�����Ͻ��Ϲܰ죡";
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
    /// ��֤�κ���Ч��
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

    #region //���¼�ͥ�˻��������ʻ���Ϣ
    /// <summary>
    /// ���¼�ͥ�������ʻ���Ϣ
    /// </summary>
    /// <param name="hospitalcode"></param> //ҽԺ����
    /// <param name="hospid"></param> //����Ż�סԺID��
    /// <param name="ptype"></param> //�˻�����ҵ������1:����סԺ�ʻ���Ϣ 2����������סԺ�ʻ���Ϣ
    /// <param name="optype"></param> //ҵ��������1:Ԥ��/��� 2��ȡ��Ԥ��/ȡ�����
    /// <param name="opfee"></param> //���²������0:���������ʻ���Ϣ 1��ֻ���²������
    /// <param name="chkfee"></param> //�������
    /// <param name="grandprice"></param> //סԺ����
    /// <param name="human"></param> //ũ�ϸ��˱���
    /// <param name="statisticyear"></param> //�κ����
    /// <param name="nopaydate"></param> //�������������
    /// <param name="mess"></param> //��������ʱ�Ĵ�����Ϣ
    /// <returns></returns>
    public bool setPersonAccount(string hospitalcode, string hospid, Int16 ptype, Int16 optype, Int16 opfee, decimal chkfee, decimal grandprice, string human, string statisticyear, string nopaydate, out string mess)
    {
        mess = "";
        #region //������Ч����֤
        if (!getUserState())
        {
            mess = "�����˻�ʱ���ֵ��ñ��ӿڵ��û�û��ͨ����֤";
            return false;
        }
        if (chkfee == 0)
        {
            return true;
        }
        if (!isDateTime(statisticyear + "-01-01 00.00.01"))
        {
            mess = "�����˻�ʱʹ���˴���Ĳκ���Ȳ���";
            return false;
        }
        if (ptype != 1 && ptype != 2)
        {
            mess = "�����˻�ʱʹ����û�ж�����˻�����ҵ������";
            return false;
        }
        if (optype != 1 && optype != 2)
        {
            mess = "�����˻�ʱʹ����û�ж����ҵ��������";
            return false;
        }
        if (opfee != 0 && opfee != 0)
        {
            mess = "�����˻�ʱʹ����û�ж���ĸ��²������";
            return false;
        }
        if (ptype == 1 && !isDateTime(nopaydate.Substring(0, 4) + nopaydate.Substring(4, 2) + nopaydate.Substring(6, 2) + " 00.00.01"))
        {
            mess = "�����˻�ʱʹ���˴�������������ڲ���";
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
            #region //�κ�������ȡ
            try
            {
                if (mess == "TRUE")
                {
                    if (myDs.Tables[0].Rows.Count == 0)
                    {
                        mess = "û�в�ѯ����زκ�����";
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
                case 1: //סԺ��Ϣ
                    #region//��ѯ��ͥ�ʻ�
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
                            if (ldec_temp2 == -1) //����ֹ��޸��˼�ͥ�ʻ�֧������ô��TAΪ׼
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
                    #region //��ѯסԺ����
                    ls_sql = "SELECT hosps FROM uv_hosppatinfo2 WHERE hospitalcode='" + hospitalcode + "' AND hospid=" + hospid;
                    myDs = getDataSet(ls_sql, out mess);
                    if (mess == "TRUE")
                    {
                        if (myDs.Tables[0].Rows.Count == 0)
                        {
                            mess = "û�в�ѯ��סԺ������Ϣ";
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
                    #region //��ѯ���ڷ���
                    ls_sql = "SELECT Sum(IsNull(chkagainfee,0)) FROM pricedetail WHERE hospitalcode='" + hospitalcode + "' AND hospid=" + hospid;
                    myDs = getDataSet(ls_sql, out mess);
                    if (mess == "TRUE")
                    {
                        if (myDs.Tables[0].Rows.Count == 0)
                        {
                            mess = "û�в�ѯ�����ڷ���";
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
                    #region //��ʼ����
                    cmd.Transaction = myTrans;
                    if (optype == 1) //Ԥ��/���
                    #region
                    {
                        Connection.Open();
                        if (opfee == 0) //���������ʻ���Ϣ
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
                                mess = "�����˻��쳣��" + e.Message.ToString();
                                return false;
                            }
                        }
                        else //ֻ���²������
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
                                mess = "�����˻������쳣��" + e.Message.ToString();
                                return false;
                            }
                        }
                        ldec_familyaccbalance *= -1;
                    }
                    #endregion
                    else //ȡ��Ԥ��/ȡ�����
                    #region
                    {
                        Connection.Open();
                        if (opfee == 0) //���������ʻ���Ϣ
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
                                mess = "�����˻��쳣��" + e.Message.ToString();
                                return false;
                            }
                        }
                        else //ֻ���²������
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
                                mess = "�����˻������쳣��" + e.Message.ToString();
                                return false;
                            }
                        }
                    }
                    #endregion
                    #region //�����¼�ͥ�ʻ���Ϣ
                    if (opfee == 0) //���������ʻ���Ϣ
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
                            mess = "���¼�ͥ�˻��쳣��" + e.Message.ToString();
                            return false;
                        }

                    }
                    else //ֻ���²������
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
                            mess = "���¼�ͥ�˻������쳣��" + e.Message.ToString();
                            return false;
                        }
                    }
                    #endregion
                    #endregion
                    break;
                case 2: //������Ϣ
                    #region //���ﲡ����Ϣ
                    ls_sql = "SELECT Convert(VarChar(8),hospdate,112),IsNull(grandprice,0),IsNull(familyfound,0),";
                    ls_sql += "IsNull(grandprice,0) - (IsNull(mediselffee,0) + IsNull(materselffee,0) + IsNull(serverselffee,0)),isnull(chkpayid,0)";
                    ls_sql += " FROM mz_hosppatinfo2 WHERE hospitalcode='" + hospitalcode + "' AND hospcode='" + hospid + "'";
                    myDs = getDataSet(ls_sql, out mess);
                    if (mess == "TRUE")
                    {
                        if (myDs.Tables[0].Rows.Count == 0)
                        {
                            mess = "û�в�ѯ����������";
                            myDs.Dispose();
                            return false;
                        }
                        else
                        {
                            nopaydate = myDs.Tables[0].Rows[0][0].ToString();
                            grandprice = Convert.ToDecimal(myDs.Tables[0].Rows[0][1]);
                            ldec_familyaccbalance = Convert.ToDecimal(myDs.Tables[0].Rows[0][2]);
                            ldec_inprice = Convert.ToDecimal(myDs.Tables[0].Rows[0][3]);
                            li_mzchkpayid = Convert.ToInt32(myDs.Tables[0].Rows[0][4]); //�����ͥ�˻�֧����־
                        }
                    }
                    else
                    {
                        myDs.Dispose();
                        return false;
                    }
                    myDs.Dispose();
                    #endregion
                    #region //��ʼ����
                    cmd.Transaction = myTrans;
                    if (optype == 1)//Ԥ��/���
                    #region //Ԥ����˸���n_individcwuchkkp
                    {
                        if (opfee == 0) //���������ʻ���Ϣ
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
                                mess = "���¸����˻��쳣��" + e.Message.ToString();
                                return false;
                            }
                        }
                        else //ֻ���²������
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
                                mess = "���¸����˻������쳣��" + e.Message.ToString();
                                return false;
                            }
                        }
                        ldec_familyaccbalance *= -1;
                    }
                    #endregion
                    else //ȡ��Ԥ��/ȡ�����
                    #region //ȡ��Ԥ����˸���n_individcwuchkkp
                    {
                        if (opfee == 0) //���������ʻ���Ϣ
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
                                mess = "���¼�ͥ�˻��쳣��" + e.Message.ToString();
                                return false;
                            }
                        }
                        else //ֻ���²������
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
                                mess = "���¼�ͥ�˻������쳣��" + e.Message.ToString();
                                return false;
                            }
                        }
                    }
                    #endregion
                    if (opfee == 0) //���������ʻ���Ϣ
                    #region //�����¼�ͥ�ʻ���Ϣ
                    {
                        #region //����mz_hosppatinfo2��ͥ�˻�֧����־
                        if ((li_mzchkpayid == 1 && optype == 1) || (li_mzchkpayid == 0 && optype == 2)) //Ԥ��/���ʱ��ͥ�˻���֧����ȡ��Ԥ��/���ʱ��ͥ�˻���δ֧����
                        {
                            ldec_familyaccbalance = 0.0M; //��ͥ������仯
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
                                mess = "���¸����˻��쳣��" + e.Message.ToString();
                                return false;
                            }
                        }
                        #endregion
                        #region //���¼�ͥ�˻���Ϣ
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
                            mess = "���¼�ͥ�˻������쳣��" + e.Message.ToString();
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
                            mess = "���¼�ͥ�˻������쳣��" + e.Message.ToString();
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
            mess = "�쳣��" + e.Message.ToString();
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

    #region ũ�ϻ���������
    /// <summary>
    /// ָ�����µĻ�����㵥��ѯ
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
        #region //�·ݸ�ʽ��Ч����֤
        if (month == null || month.Trim().Length == 0)
        {
            mess += "month����Ϊ��";
        }
        else
        {
            if (!isDateTime(month.Substring(0, 4) + "-" + month.Substring(4, 2) + "-01 00:00:01"))
            {
                mess += "month��ʽ����Ϊ'200901'����ʽ";
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
            ls_sql = "SELECT b.bill as bill,case when b.state=1 then '�ѽ���' else 'ȡ������' end as state,b.payfee as hjsj,b.mustpayfee as zydf,b.deductfee1 as zyshmxkc,b.deductfee2 as zyjsmxkc,";
            ls_sql += "b.punishfee as zycf,b.prereservefee as zyscyl,b.reservefee as zybcyl,b.noreservefee as zywfhscyl,b.payfee_hosp as zysj,b.startdate as zyksrq,b.enddate as zyjsrq";
            ls_sql += ",b.mustpayfee_mz as mzdf,b.deductfee1_mz as mzshmxkc,b.deductfee2_mz as mzjsmxkc,b.punishfee_mz as mzcf,b.prereservefee_mz as mzscyl,b.reservefee_mz as mzbcyl";
            ls_sql += ",b.noreservefee_mz as mzwfhscyl,b.payfee_mz as mzsj,b.startdate_mz as mzksrq,b.enddate_mz as mzjsrq,b.accounter as jsr,convert(varchar,b.accountdate,120) as jssj,b.markinfo as jssm";
            ls_sql += " FROM accountinfo_hosp b";
            ls_sql += " WHERE b.hospitalcode='" + orgcode + "' AND convert(varchar(6),b.accountdate,112)='" + month + "'";
            //��������
            myDs = getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                getRequestXMLStringFromDS(myDs, "ʵ�ʽ�����=(ҽԺ�渶���+�ϴ�Ԥ�����)-(�����ϸ�۳����+������ϸ�۳����+�������+����Ԥ�����+δ�����ϴ�Ԥ�����)", out mess, out data);
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
    /// ָ��������㵥���ݲ�ѯ
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
            //����סԺ�����״̬�����������
            ls_sql = "SELECT 'ҽ�Ʒ���' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
            ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
            ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'M' as jzlx";
            ls_sql += " FROM accountinfo_mz a,mz_pataccountdetail p";
            ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospcode=p.HospCode) AND (p.chkpayid=0)";
            ls_sql += " UNION ";
            ls_sql += "SELECT 'Ԥ����' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
            ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
            ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'M' as jzlx";
            ls_sql += " FROM accountinfo_mz a,mz_pataccountdetail p";
            ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospcode=p.HospCode)";
            ls_sql += " AND (p.chkpayid=(SELECT max(chkpayid) FROM mz_pataccountdetail WHERE (hospitalcode=a.hospitalcode AND HospCode=a.hospcode) AND (chkpayid=8 OR chkpayid=1)))";
            ls_sql += " UNION ";
            ls_sql += "SELECT TOP 1 '��˱���' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
            ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
            ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'M' as jzlx";
            ls_sql += " FROM accountinfo_mz a,mz_pataccountdetail p";
            ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospcode=p.HospCode)";
            ls_sql += " AND (p.chkpayid=(SELECT min(chkpayid) FROM mz_pataccountdetail WHERE (hospitalcode=a.hospitalcode AND HospCode=a.hospcode) AND (chkpayid=8 OR chkpayid=1)))";
            ls_sql += " UNION ";
            ls_sql += "SELECT 'ҽ�Ʒ���' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
            ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
            ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'Z' as jzlx";
            ls_sql += " FROM accountinfo a,pataccountdetail p";
            ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospid=p.hospid) AND (p.chkpayid=0)";
            ls_sql += " UNION ";
            ls_sql += "SELECT 'Ԥ����' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
            ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
            ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'Z' as jzlx";
            ls_sql += " FROM accountinfo a,pataccountdetail p";
            ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospid=p.hospid)";
            ls_sql += " AND (p.chkpayid=(SELECT max(chkpayid) FROM pataccountdetail WHERE (hospitalcode=a.hospitalcode AND hospid=a.hospid) AND (chkpayid=8 OR chkpayid=1)))";
            ls_sql += " UNION ";
            ls_sql += "SELECT TOP 1 '��˱���' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
            ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
            ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'Z' as jzlx";
            ls_sql += " FROM accountinfo a,pataccountdetail p";
            ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospid=p.hospid)";
            ls_sql += " AND (p.chkpayid=(SELECT min(chkpayid) FROM pataccountdetail WHERE (hospitalcode=a.hospitalcode AND hospid=a.hospid) AND (chkpayid=8 OR chkpayid=1)))";
            //��������סԺ�����״̬�����������
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

            //����סԺ������ϸ����
            ls_sql = "SELECT i.name as xm,s.cityname as dz,f.familyno as jth,f.bookcard as zkh,f.validno as authid,";
            ls_sql += "m.hospcode as jzh,convert(varchar(10),m.hospdate,120) as jzksrq,convert(varchar(10),m.outdate,120) as jzjsrq,h.HospResu as zd,";
            ls_sql += "m.grandPrice as zje,c.mediprice -c.mediselffee +c.materprice -c.materselffee +c.serverprice -c.serverselffee as bnje,";
            ls_sql += "c.payorder as bch,c.subsum as bcje,convert(varchar(19),c.paytime,120) as bczfsj,0.0 as qzxzje,0.0 as qzewbcje,0.0 as qzecbcje,";
            ls_sql += "0.0 as qzczbc,0.0 as qzmzbc,m.familyfund as qzjtzhzc,m.medifee as ypje,c.mediselffee as ypzfje,c.subsum as ysje,m.payfee as shje,m.accountfee as jsje,m.detailfee as kkje,";
            ls_sql += "'M' as jzlx,'ũ��' as brlx,";
            ls_sql += "case zc.lx when 10 then '��ͨ' when 20 then '����' when 30 then '���' when 40 then '����' else '����' end as bclx";
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
            ls_sql += "case when h.gonextstate=1 then 'ת������' when h.ifhurt=1 then '�����˺�' when h.orderid>10000 then '������' when h.orderid< -10000 then '����' else 'һ��סԺ' end as bclx";
            ls_sql += " FROM accountinfo_hosp a,accountinfo z,hosppatinfo2 h,hosp_check_info i,n_familydata f,sysarea s";
            ls_sql += " WHERE a.bill='" + bill + "' AND a.hospitalcode='" + orgcode + "' AND a.bill=z.bill AND h.hospitalcode=z.hospitalcode AND h.hospid=z.hospid AND h.hospitalcode=i.hospitalcode";
            ls_sql += " AND h.hospid=i.hospid AND f.familyno=h.MEDINUMB AND f.statisticyear=convert(varchar(4),h.HospDate,112) AND s.areacode=z.patarea1 AND s.areacode2=z.patarea2";
            //��������סԺ������ϸ����
            myDs = getDataSet(ls_sql, out mess);
            if (mess == "TRUE")
            {
                //getRequestXMLStringFromDS(myDs, bill + "��������", out mess, out data);
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

            //����������data
            data = "<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "UTF-8" + '"' + "?><DATA>" + data + "</DATA>";
            mess = getXMLStrFromString("���㵥�������");

            return mess;
        }
        else
        {
            mess = getXMLErroStrFromString("5000", mess);
            return mess;
        }
    }

    /// <summary>
    /// ָ��������㵥���ݲ�ѯ--������Ϣ
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
                //סԺ�����״̬�����������
                ls_sql = "SELECT 'ҽ�Ʒ���' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
                ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
                ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'Z' as jzlx";
                ls_sql += " FROM accountinfo a,pataccountdetail p";
                ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospid=p.hospid) AND (p.chkpayid=0)";
                ls_sql += " UNION ";
                ls_sql += "SELECT 'Ԥ����' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
                ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
                ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'Z' as jzlx";
                ls_sql += " FROM accountinfo a,pataccountdetail p";
                ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospid=p.hospid)";
                ls_sql += " AND (p.chkpayid=(SELECT max(chkpayid) FROM pataccountdetail WHERE (hospitalcode=a.hospitalcode AND hospid=a.hospid) AND (chkpayid=8 OR chkpayid=1)))";
                ls_sql += " UNION ";
                ls_sql += "SELECT TOP 1 '��˱���' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
                ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
                ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'Z' as jzlx";
                ls_sql += " FROM accountinfo a,pataccountdetail p";
                ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospid=p.hospid)";
                ls_sql += " AND (p.chkpayid=(SELECT min(chkpayid) FROM pataccountdetail WHERE (hospitalcode=a.hospitalcode AND hospid=a.hospid) AND (chkpayid=8 OR chkpayid=1)))";
            }
            else if (settletype.ToUpper() == "MZ")
            {
                //��������״̬�����������
                ls_sql = "SELECT 'ҽ�Ʒ���' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
                ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
                ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'M' as jzlx";
                ls_sql += " FROM accountinfo_mz a,mz_pataccountdetail p";
                ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospcode=p.HospCode) AND (p.chkpayid=0)";
                ls_sql += " UNION ";
                ls_sql += "SELECT 'Ԥ����' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
                ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
                ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'M' as jzlx";
                ls_sql += " FROM accountinfo_mz a,mz_pataccountdetail p";
                ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospcode=p.HospCode)";
                ls_sql += " AND (p.chkpayid=(SELECT max(chkpayid) FROM mz_pataccountdetail WHERE (hospitalcode=a.hospitalcode AND HospCode=a.hospcode) AND (chkpayid=8 OR chkpayid=1)))";
                ls_sql += " UNION ";
                ls_sql += "SELECT TOP 1 '��˱���' as fylb,isnull(sum(p.fee1),0.0) as fee1,isnull(sum(p.fee2),0.0) as fee2,isnull(sum(p.fee3),0.0) as fee3,isnull(sum(p.fee4),0.0) as fee4,isnull(sum(p.fee5),0.0) as fee5";
                ls_sql += ",isnull(sum(p.fee6),0.0) as fee6,isnull(sum(p.fee7),0.0) as fee7,isnull(sum(p.fee8),0.0) as fee8,isnull(sum(p.fee9),0.0) as fee9,isnull(sum(p.fee10),0.0) as fee10,isnull(sum(p.fee11),0.0) as fee11";
                ls_sql += ",isnull(sum(p.fee12),0.0) as fee12,isnull(sum(p.fee13),0.0) as fee13,isnull(sum(p.fee14),0.0) as fee14,isnull(sum(p.fee15),0.0) as fee15,'M' as jzlx";
                ls_sql += " FROM accountinfo_mz a,mz_pataccountdetail p";
                ls_sql += " WHERE (a.bill='" + bill + "' AND a.hospitalcode=p.hospitalcode AND a.hospcode=p.HospCode)";
                ls_sql += " AND (p.chkpayid=(SELECT min(chkpayid) FROM mz_pataccountdetail WHERE (hospitalcode=a.hospitalcode AND HospCode=a.hospcode) AND (chkpayid=8 OR chkpayid=1)))";
            }
            else
            {
                data = "";
                mess = getXMLErroStrFromString("5000", "��Ч�Ľ��㵥���͡�");
                return mess;
            }           

           
            //��������סԺ�����״̬�����������
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

            //����������data
            data = "<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "UTF-8" + '"' + "?><DATA>" + data + "</DATA>";
            mess = getXMLStrFromString("���㵥�������");

            return mess;
        }
        else
        {
            mess = getXMLErroStrFromString("5000", mess);
            return mess;
        }
    }

    /// <summary>
    /// ָ��������㵥��ϸ���ݲ�ѯ
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
                mess = getXMLErroStrFromString("2000", "���ò����Ŀ�ʼ�кͽ����ж�����С��0���������أ�");
                return mess;
            }
            else if ((startrow == 0 && endrow != 0) || (startrow != 0 && endrow == 0))
            {
                data = "";
                mess = getXMLErroStrFromString("2000", "���ò����Ŀ�ʼ�кͽ����б���ͬʱΪ0����ʾ��ȡ�����ؽ�����ϸ������");
                return mess;
            }
            else if (startrow > endrow)
            {
                data = "";
                mess = getXMLErroStrFromString("2000", "���ò����Ŀ�ʼ�д��ڽ����С��������أ�");
                return mess;
            }
            else if (endrow - startrow > 199)
            {
                data = "";
                mess = getXMLErroStrFromString("2000", "�������󳬹�200�С��������أ�");
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
                        getRequestXMLStringFromDS(myDs, "�õ�������ϸ������������", out mess, out data);
                        return mess;
                    }
                    if (Convert.ToInt32(myDs.Tables[0].Rows[0][0]) == 0)
                    {
                        data = "";
                        mess = getXMLErroStrFromString("2000", "û�����ݿ������أ�");
                        return mess;
                    }
                    if (startrow > Convert.ToInt32(myDs.Tables[0].Rows[0][0]))
                    {
                        data = "";
                        mess = getXMLErroStrFromString("2000", "���ò����Ŀ�ʼ�г���ҩƷĿ¼������" + myDs.Tables[0].Rows[0][0].ToString() + "���������أ�");
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
                ls_sql += "case when h.gonextstate=1 then 'ת������' when h.ifhurt=1 then '�����˺�' when h.orderid>10000 then '������' when h.orderid< -10000 then '����' else 'һ��סԺ' end as bclx";
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
                ls_sql += "'M' as jzlx,'ũ��' as brlx,";
                ls_sql += "case zc.lx when 10 then '��ͨ' when 20 then '����' when 30 then '���' when 40 then '����' else '����' end as bclx";
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
                //getRequestXMLStringFromDS(myDs, bill + "��������", out mess, out data);
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

            //����������data
            data = "<?xml version=" + '"' + "1.0" + '"' + " encoding=" + '"' + "UTF-8" + '"' + "?><DATA>" + data + "</DATA>";
            mess = getXMLStrFromString("���㵥�������");

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

    #region HISonline WebService �ӿ�ҵ���������

    /// <summary>
    /// HISonline WebService URL��ַ��ȡ
    /// </summary>
    /// <param name="servername"></param>
    /// <param name="mess"></param>
    /// <returns></returns>
    public bool getWebServiceURL(string servername, out string mess)
    {
        if (servername == null || servername.Length == 0 || !isValidParm(servername))
        {
            mess = "�ṩ��HIS������������ȷ";
            return false;
        }
        try
        {
            mess = "";
            //����һ��XmlTextReader��Ķ��󲢵���Read��������ȡXML�ļ�
            XmlTextReader txtReader = new XmlTextReader(Server.MapPath("./Services/" + servername + ".xml"));
            txtReader.Read();
            txtReader.Read();
            //�ҵ����ϵĽڵ��ȡ��Ҫ������ֵ
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
                mess = "��ȡ" + servername + "�Ľӿڵ�ַ�������������ļ���";
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            if (e.Message.ToString().Contains("δ���ҵ��ļ�"))
            {
                mess = "û���ҵ�" + servername + "�Ľӿڵ�ַ���ò�����";
            }
            else
            {
                mess = "��ȡ" + servername + "�Ľӿڵ�ַ���ò�������" + e.Message.ToString();
            }
            return false;
        }
    }

    /// <summary>
    /// ͨ�ô���SQL��HISonline�ӿڻ�ȡHIS���ݣ���DataSet����
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
            retMessXML = getXMLErroStrFromString("2000", "û��ָ��SQL���");
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
            //����his��Webservice�ӿڲ����û�ȡסԺ���ݵķ���
            hisservice his = new hisservice(hisifurl);
            if (his.rdhlhnhisif(servername, dbname, user, pwd, requestParmXML, lds, out lds, out retMessXML))
            {
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            retMessXML = getXMLErroStrFromString("9000", "����HIS�ӿ��쳣��" + e.Message.ToString());
            return false;
        }
    }

    /// <summary>
    /// ͨ�ô���SQL��HISonline�ӿڻ�ȡHIS���ݣ���XML����
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
    /// ʹ��ָ����ҵ��ŵ���HIS�ӿ����䷢��dataset
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
    /// ������һ��putDataToHIS������������������
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
            retMessXML = getXMLErroStrFromString("2000", "û��ָ������HIS�ӿڵ�ҵ���");
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
            //����his��Webservice�ӿڲ����û�ȡסԺ���ݵķ���
            hisservice his = new hisservice(hisifurl);
            if (his.rdhlhnhisif(servername, dbname, user, pwd, requestParmXML, data, out lds, out retMessXML))
            {
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            retMessXML = getXMLErroStrFromString("9000", "����HIS�ӿ��쳣��" + e.Message.ToString());
            return false;
        }
    }
    #endregion

    #region WebService����

    /// <summary>
    /// ���Ե�WebService�������Ƿ�����
    /// </summary>
    /// <returns></returns>
    [WebMethod(Description = "��������ũ��ϵͳ�Ƿ�����")]
    public string testConnection(string areacode)
    {
        if (areacode == null || areacode.Length == 0 || !isValidParm(areacode))
        {
            return getXMLErroStrFromString("1000", "�ͻ��˳�������ũ�Ͻӿ��������ṩ��������벻��ȷ�����ܲ������ݿ����ӣ�");
        }
        string mess = "";
        if (getDBConnStr("areacode", areacode, out mess))
        {
            //���Ӵ���Ҫȥ������ؼ��� Provider=SQLOLEDB.1;
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
                        return getXMLStrFromString("�ͻ��˵��ýӿ�����ũ��ϵͳ������");
                    }
                    else
                    {
                        return getXMLErroStrFromString("1000", "�ͻ��˳�������ũ�Ͻӿ��������������ݿⲻ�ɹ���");
                    }
                }
                catch (Exception e)
                {
                    return getXMLErroStrFromString("9000", "�ͻ��˳�������ũ�Ͻӿ��������������ݿ��쳣��" + e.Message.ToString());
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
            return getXMLErroStrFromString("5000", "�ͻ��˳�������ũ�Ͻӿ�������" + mess + "������������Ƿ���ȷ");
        }
    }

    /// <summary>
    /// �ڴﻥ������ũ��WebService�������(���������ݺϲ�������ֵ�еķ���)
    /// </summary>
    /// <param name="requestParmXML"></param>
    /// <param name="uploadDataXML"></param>
    /// <returns></returns>
    [WebMethod(Description = "���ýӿڴ���ҵ��ķ���2")]
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
    /// �ڴﻥ������ũ��WebService�������(ע��ҵ������������"0XX��ʾ����Ҫ��֤��ݵ�ҵ�����)
    /// </summary>
    /// <param name="requestParmXML"></param>
    /// <param name="uploadDataXML"></param>
    /// <param name="retDataXML"></param>
    /// <returns></returns>
    [WebMethod( Description = "���ýӿڴ���ҵ��ķ���")]
    public string rdhlhnncmsif(string requestParmXML, string uploadDataXML, out string retDataXML)
    {
        //��ע�⣬����retDataXML�ͱ������ķ���ֵ��PB�»�ȡ֤�������retDataXML���ڱ�������
        //�洢�������ݣ�return����״̬����PB�µ���ʱ�պû�ȡ�����ģ���returnֵ��data������
        //��ȡ����ȷ��״̬����ˣ������з����У���������ֵ�����������ص�ǰ�ˣ���ʹPB�е���
        //return����״̬�������л�ȡ����data
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
                    if (requesttype.Substring(0, 2) == "00") //����Ҫ�����֤�ġ�ע�����������
                    #region
                    {
                        switch (requesttype)
                        {
                            case "0001"://����ҽԺ����õ���ҽԺ����Ϣ
                                #region
                                lis = xmldoc.GetElementsByTagName("hospitalcode");
                                hospitalcode = lis[0].InnerText;
                                if (!isValidParm(hospitalcode))
                                {
                                    return getXMLErroStrFromString("2000", "����hospitalcode����");
                                }
                                retDataXML = getHospitalInfo(areacode, hospitalcode, out mess);
                                return mess;
                            //break;
                                #endregion
                            case "0002"://����ҽԺ����ģ����ѯ������Ϣ
                                #region
                                string hospitalname;
                                lis = xmldoc.GetElementsByTagName("hospitalname");
                                hospitalname = lis[0].InnerText;
                                if (!isValidParm(hospitalname))
                                {
                                    return getXMLErroStrFromString("2000", "����hospitalname����");
                                }
                                retDataXML = getHospitalsInfo(areacode, hospitalname, out mess);
                                return mess;
                            //break;
                                #endregion
                            case "0003"://�Ǿӽӿڹ��߰汾�Ż�ȡ
                                retDataXML = getCJJKVer();
                                return mess;
                            default:
                                #region
                                retDataXML = getXMLErroStrFromString("2000", "û�ж����ҵ��������룺" + requesttype + "");
                                return "";
                            //break;
                                #endregion
                        }
                    }
                    #endregion
                    else //���¶�����Ҫ�����֤��ҵ��
                    #region
                    {
                        string userid, pwd;
                        lis = xmldoc.GetElementsByTagName("hospitalcode");
                        hospitalcode = lis[0].InnerText;
                        if (!isValidParm(hospitalcode))
                        {
                            retDataXML = getXMLErroStrFromString("2000", "����hospitalcode����");
                            return "";
                        }
                        lis = xmldoc.GetElementsByTagName("userid");
                        userid = lis[0].InnerText;
                        if (!isValidParm(userid))
                        {
                            retDataXML = getXMLErroStrFromString("2000", "����userid����");
                            return "";
                        }
                        lis = xmldoc.GetElementsByTagName("password");
                        pwd = lis[0].InnerText;
                        if (!isValidParm(pwd))
                        {
                            retDataXML = getXMLErroStrFromString("2000", "����password����");
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
                                #region //�û������֤
                                case "1000"://�û������֤
                                    #region
                                    retDataXML = getXMLStrFromString("�û������֤ͨ��");
                                    return "";
                                //break;
                                    #endregion
                                #endregion
                                #region //�ſ���������
                                case "1005":                                   
                                    #region
                                    string statisticyear, bookno;
                                    lis = xmldoc.GetElementsByTagName("year");
                                    statisticyear = lis[0].InnerText;
                                    if (!isValidParm(statisticyear))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����year����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("medicard");
                                    bookno = lis[0].InnerText;
                                    if (!isValidParm(bookno))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����medicard����");
                                        return "";
                                    }
                                    retDataXML = getCardInfo(areacode, statisticyear, bookno, out mess);
                                    return mess;
                                    #endregion
                                #endregion
                                #region //��ȡ�κ�����
                                case "1010"://��ȡ�κ�����
                                    #region
                                    string year, medicard, authid, id, bankcard;
                                    lis = xmldoc.GetElementsByTagName("year");
                                    year = lis[0].InnerText;
                                    if (!isValidParm(year))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����year����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("medicard");
                                    medicard = lis[0].InnerText;
                                    if (!isValidParm(medicard))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����medicard����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("authid");
                                    authid = lis[0].InnerText;
                                    if (!isValidParm(authid))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����authid����");
                                        return "";
                                    }
                                    //���medicard>16λ������16λ����ǿ�ʶ����
                                    if (medicard.Length > 16)
                                    {
                                        authid = medicard.Substring(16);
                                    }
                                    lis = xmldoc.GetElementsByTagName("id");
                                    id = lis[0].InnerText;
                                    if (!isValidParm(id))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����id����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("bankcard");
                                    bankcard = lis[0].InnerText;
                                    if (!isValidParm(bankcard))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����bankcard����");
                                        return "";
                                    }
                                    retDataXML = getPersonsInfo(areacode, year, medicard, authid, id, bankcard, out mess);
                                    return mess;
                                    #endregion
                                #endregion
                                #region //Ŀ¼����
                                case "1101"://�õ���ҽԺʹ�õ�ũ��סԺҩƷĿ¼
                                    #region
                                    if (InterFacePower == 10)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startmedi1, endmedi1;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "��ʼ�в���";
                                    startmedi1 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "�����в�����";
                                    endmedi1 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogMedi(areacode, hospitalcode, startmedi1, endmedi1, out mess);
                                    return mess;
                                    //break;
                                    #endregion
                                case "1102"://�ϴ���ҽԺ��סԺҩƷƥ������
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "Ӧ�������ݵ�XML�ַ�������Ϊ��");
                                        return "";
                                    }
                                    retDataXML = ml.putCatalogMedi(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                    //break;
                                    #endregion
                                case "1103"://�õ���ҽԺ�����ͨ����סԺҩƷƥ������
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startmedi3, endmedi3;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "��ʼ�в���";
                                    startmedi3 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "�����в�����";
                                    endmedi3 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogMediAudited(areacode, hospitalcode, startmedi3, endmedi3, out mess);
                                    return mess;
                                    //break;
                                    #endregion
                                case "1104"://ɾ����ҽԺ�ϴ��Ļ�δ���ͨ����סԺҩƷƥ������
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "Ӧ�������ݵ�XML�ַ�������Ϊ��");
                                        return "";
                                    }
                                    retDataXML = ml.delCatalogMedi(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1105"://�õ���ҽԺʹ�õ�ũ������ҩƷĿ¼
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startmedi5, endmedi5;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "��ʼ�в���";
                                    startmedi5 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "�����в�����";
                                    endmedi5 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogMediMZ(areacode, hospitalcode, startmedi5, endmedi5, out mess);
                                    return mess;
                                //break;
                                    #endregion
                                case "1106"://�ϴ���ҽԺ������ҩƷƥ������
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "Ӧ�������ݵ�XML�ַ�������Ϊ��");
                                        return "";
                                    }
                                    retDataXML = ml.putCatalogMediMZ(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                //break;
                                    #endregion
                                case "1107"://�õ���ҽԺ�����ͨ��������ҩƷƥ������
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startmedi7, endmedi7;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "��ʼ�в���";
                                    startmedi7 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "�����в�����";
                                    endmedi7 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogMediAuditedMZ(areacode, hospitalcode, startmedi7, endmedi7, out mess);
                                    return mess;
                                //break;
                                    #endregion
                                case "1108"://ɾ����ҽԺ�ϴ��Ļ�δ���ͨ��������ҩƷƥ������
                                    #region
                                    if (InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "Ӧ�������ݵ�XML�ַ�������Ϊ��");
                                        return "";
                                    }
                                    retDataXML = ml.delCatalogMediMZ(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1111"://�õ���ҽԺʹ�õ�ũ��סԺҽ�Ʒ���Ŀ¼
                                    #region
                                    if (InterFacePower == 10)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startsvr1, endsvr1;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "��ʼ�в���";
                                    startsvr1 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "�����в�����";
                                    endsvr1 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogServer(areacode, hospitalcode, startsvr1, endsvr1, out mess);
                                    return mess;
                                    //break;
                                    #endregion
                                case "1112"://�ϴ���ҽԺ��סԺҽ�Ʒ���ƥ������
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "Ӧ�������ݵ�XML�ַ�������Ϊ��");
                                        return "";
                                    }
                                    retDataXML = ml.putCatalogServer(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1113"://�õ���ҽԺ�����ͨ����סԺҽ�Ʒ���ƥ������
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startsvr3, endsvr3;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "��ʼ�в���";
                                    startsvr3 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "�����в�����";
                                    endsvr3 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogServerAudited(areacode, hospitalcode, startsvr3, endsvr3, out mess);
                                    return mess;
                                    //break;
                                    #endregion
                                case "1114"://ɾ����ҽԺ�ϴ��Ļ�δ���ͨ����סԺҽ�Ʒ���ƥ������
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "Ӧ�������ݵ�XML�ַ�������Ϊ��");
                                        return "";
                                    }
                                    retDataXML = ml.delCatalogServer(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1115"://�õ���ҽԺʹ�õ�ũ������ҽ�Ʒ���Ŀ¼
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startsvr15, endsvr15;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "��ʼ�в���";
                                    startsvr15 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "�����в�����";
                                    endsvr15 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogServerMZ(areacode, hospitalcode, startsvr15, endsvr15, out mess);
                                    return mess;
                                //break;
                                    #endregion
                                case "1116"://�ϴ���ҽԺ������ҽ�Ʒ���ƥ������
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "Ӧ�������ݵ�XML�ַ�������Ϊ��");
                                        return "";
                                    }
                                    retDataXML = ml.putCatalogServerMZ(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1117"://�õ���ҽԺ�����ͨ��������ҽ�Ʒ���ƥ������
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startsvr7, endsvr7;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "��ʼ�в���";
                                    startsvr7 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "�����в�����";
                                    endsvr7 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogServerAuditedMZ(areacode, hospitalcode, startsvr7, endsvr7, out mess);
                                    return mess;
                                //break;
                                    #endregion
                                case "1118"://ɾ����ҽԺ�ϴ��Ļ�δ���ͨ��������ҽ�Ʒ���ƥ������
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "Ӧ�������ݵ�XML�ַ�������Ϊ��");
                                        return "";
                                    }
                                    retDataXML = ml.delCatalogServerMZ(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1121"://�õ���ҽԺʹ�õ�ICD-10Ŀ¼
                                    #region
                                    errtxt = "";
                                    int starticd1, endicd1;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "��ʼ�в���";
                                    starticd1 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "�����в�����";
                                    endicd1 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogICD10(areacode, hospitalcode, starticd1, endicd1, out mess);
                                    return mess;
                                    //break;
                                    #endregion
                                case "1122"://�ϴ���ҽԺ��ICD-10ƥ������
                                    #region
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "Ӧ�������ݵ�XML�ַ�������Ϊ��");
                                        return "";
                                    }
                                    retDataXML = ml.putCatalogICD10(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1123"://�õ���ҽԺ����˵�ICD-10����
                                    #region
                                    errtxt = "";
                                    int starticd3, endicd3;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "��ʼ�в���";
                                    starticd3 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "�����в�����";
                                    endicd3 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogICD10Audited(areacode, hospitalcode, starticd3, endicd3, out mess);
                                    return mess;
                                //break;
                                    #endregion
                                case "1124"://ɾ����ҽԺ�ϴ��Ļ�δ���ͨ����ICD-10ƥ������
                                    #region
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "Ӧ�������ݵ�XML�ַ�������Ϊ��");
                                        return "";
                                    }
                                    retDataXML = ml.delCatalogICD10(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1151"://�Ǿӽӿ����ر�Ժũ��ƥ����Ϣ
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startmedi51, endmedi51;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "��ʼ�в���";
                                    startmedi51 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "�����в�����";
                                    endmedi51 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getNhppb_cjjk(areacode, hospitalcode, startmedi51, endmedi51, out mess);
                                    return mess;
                                    #endregion
                                case "1152"://�Ǿӽӿ����سǾ���Ϣ
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startmedi52, endmedi52;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "��ʼ�в���";
                                    startmedi52 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "�����в�����";
                                    endmedi52 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalog_cjjk(areacode, hospitalcode, startmedi52, endmedi52, out mess);
                                    return mess;
                                    #endregion
                                case "1153"://�ϴ��Ǿ�Ŀ¼ƥ����Ϣ(ҩƷ����+ҽ�Ʒ���)
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "Ӧ�������ݵ�XML�ַ�������Ϊ��");
                                        return "";
                                    }
                                    retDataXML = ml.putCatalog_cjjk(areacode, hospitalcode, uploadDataXML, out mess);
                                    return mess;
                                //break;
                                    #endregion
                                case "1154"://ͬ����ҽԺ�ĳǾ�Ŀ¼ƥ������
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    errtxt = "";
                                    int startsvr1154, endsvr1154;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "��ʼ�в���";
                                    startsvr1154 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "�����в�����";
                                    endsvr1154 = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = ml.getCatalogSyn_cjjk(areacode, hospitalcode, startsvr1154, endsvr1154, out mess);
                                    return mess;
                                //break;
                                    #endregion
                                #endregion
                                #region //���ﲡ�˲���
                                case "1201"://��ũ��ϵͳ�в�ѯһ�����ﲡ������
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("mzh");//�����
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����mzh����");
                                        return "";
                                    }
                                    retDataXML = mz.getMZPatientInfo(areacode, hospitalcode, ls_tmp, out mess);
                                    return mess;
                                    #endregion
                                case "1202"://�ϴ�����ũ�ϲ�����Ϣ(֧�ֶ��������Ϣ�ϴ�)
                                    #region

                                    if (!setPDCert(requestParmXML, out retDataXML))
                                        return "";
                                    

                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "Ӧ�������ݵ�XML�ַ�������Ϊ��");
                                        return "";
                                    }
                                    retDataXML = mz.putMZPatientInfo(areacode, hospitalcode, userid, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1203"://ɾ��һ�����ﲡ�˵�����
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("mzh");
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����mzh����");
                                        return "";
                                    }
                                    retDataXML = mz.delMZPatientInfo(areacode, hospitalcode, ls_tmp);
                                    return "";
                                    #endregion
                                case "1204"://�޸�һ�����ﲡ�˵�����
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("mzh");
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����mzh����");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "Ӧ�������ݵ�XML�ַ�������Ϊ��");
                                        return "";
                                    }
                                    retDataXML = mz.editMZPatientInfo(areacode, hospitalcode, ls_tmp, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                #endregion
                                #region //���ﲡ�˷��ò���
                                case "1211"://��ȡָ������ŵķ�������
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("mzh");
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����mzh����");
                                        return "";
                                    }
                                    retDataXML = mz.getMZPatientFee(areacode, hospitalcode, ls_tmp, out mess);
                                    return mess;
                                    #endregion
                                case "1212"://�ϴ�һ�����ﲡ�˵ķ���
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("mzh");
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����mzh����");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "Ӧ�������ݵ�XML�ַ�������Ϊ��");
                                        return "";
                                    }
                                    retDataXML = mz.putMZPatientFee(areacode, hospitalcode, ls_tmp, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1213"://ɾ��һ�����ﲡ�˵����з���
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("mzh");
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����mzh����");
                                        return "";
                                    }
                                    retDataXML = mz.delMZPatientFee(areacode, hospitalcode, ls_tmp);
                                    return "";
                                    #endregion
                                #endregion
                                #region //���ﲡ�˽������
                                case "1221"://�õ�һ�����ﲡ�˵Ľ�������
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("bch");//������(bch)
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����bch����");
                                        return "";
                                    }
                                    retDataXML = mz.settleMZQuery(areacode, ls_tmp, out mess);
                                    return mess;
                                    #endregion
                                case "1222": //���������������Խ���
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "Ӧ�������ݵ�XML�ַ�������Ϊ��");
                                        return "";
                                    }
                                    retDataXML = mz.settleMZ(hospitalcode, "TestOrder", 8, "", "", 0, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "1223"://����������������ʽ����
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    string ls_opername1223;
                                    lis = xmldoc.GetElementsByTagName("bch");//������(bch)
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����bch����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("czyxm");//����Ա����(czyxm)
                                    ls_opername1223 = Convert.ToString(lis[0].InnerText);
                                    retDataXML = mz.settleMZ(hospitalcode, ls_tmp, 8, "", ls_opername1223, 1, "", out mess);
                                    return mess;
                                    #endregion
                                case "1224": //��������������ȡ������
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    string ls_opername1224;
                                    lis = xmldoc.GetElementsByTagName("bch");//������(bch)
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����bch����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("czyxm");//����Ա����(czyxm)
                                    ls_opername1224 = Convert.ToString(lis[0].InnerText);
                                    retDataXML = mz.settleMZCancel(areacode, hospitalcode, "", ls_opername1224, ls_tmp);
                                    return "";
                                    #endregion
                                case "1225": //������������������֧��
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    string ls_opername1225, ls_pay1225;
                                    lis = xmldoc.GetElementsByTagName("bch");//������(bch)
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����bch����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("pay");//ʵ�ʲ�����(pay)
                                    ls_pay1225 = Convert.ToString(lis[0].InnerText);
                                    if (ls_pay1225 == null || ls_pay1225.Trim().Length == 0 || !isNumber(ls_pay1225))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����pay����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("czyxm");//����Ա����(czyxm)
                                    ls_opername1225 = Convert.ToString(lis[0].InnerText);
                                    retDataXML = mz.settleMZSetPay(hospitalcode, ls_tmp, ls_pay1225, ls_opername1225);
                                    return "";
                                    #endregion
                                case "1226": //����֧�����Ƿ����ȡ������
                                    #region 
                                    retDataXML = mz.settleMZQueryCancelabled(areacode, out mess);
                                    return mess;
                                    #endregion
                                case "991225": //�����������������
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    string ls_opercode991225, ls_opername991225;
                                    lis = xmldoc.GetElementsByTagName("bch");//������(bch)
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����bch����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("czydm");//����Ա����(czydm)
                                    ls_opercode991225 = Convert.ToString(lis[0].InnerText);
                                    lis = xmldoc.GetElementsByTagName("czyxm");//����Ա����(czyxm)
                                    ls_opername991225 = Convert.ToString(lis[0].InnerText);
                                    retDataXML = mz.settleMZ(hospitalcode, ls_tmp, 1, ls_opercode991225, ls_opername991225, 1, "", out mess);
                                    return mess;
                                    #endregion
                                case "991226": //��������������ȡ�����
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    return "";
                                    #endregion
                                #endregion
                                #region //���ҵ�����
                                case "1301"://����ָ��������ڵ���Ч��������
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    retDataXML = mz.getTJHealthyPacket(areacode, hospitalcode, out mess);
                                    return mess;
                                    #endregion
                                case "1302"://���������δ�����걨������
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    errtxt = "";
                                    int starttj, endtj;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    errtxt = "��ʼ�в���";
                                    starttj = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    errtxt = "�����в�����";
                                    endtj = Convert.ToInt32(lis[0].InnerText);
                                    errtxt = "";
                                    retDataXML = mz.getTJDeclareRoster(areacode, hospitalcode, starttj, endtj, out mess);
                                    return mess;
                                    #endregion
                                #endregion
                                #region //סԺ���˲���
                                case "2201"://��ũ��ϵͳ�в�ѯһ��סԺ��������
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("zyid");//סԺID��
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����zyid����");
                                        return "";
                                    }
                                    retDataXML = zy.getZYPatientInfo(areacode, hospitalcode, ls_tmp, out mess);
                                    return mess;
                                    #endregion
                                case "2202"://�ϴ�סԺũ�ϲ�����Ϣ(֧�ֶ��������Ϣ�ϴ�)
                                    #region
                                    
                                    if (!setPDCert(requestParmXML, out retDataXML))
                                        return "";

                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "Ӧ�������ݵ�XML�ַ�������Ϊ��");
                                        return "";
                                    }
                                    retDataXML = zy.putZYPatientInfo(areacode, hospitalcode, userid, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "2203"://ɾ��һ��סԺ���˵�����
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("zyid");//סԺID��
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����zyid����");
                                        return "";
                                    }
                                    retDataXML = zy.delZYPatientInfo(areacode, hospitalcode, ls_tmp);
                                    return "";
                                    #endregion
                                #endregion
                                #region //סԺ���˷��ò���
                                case "2211"://��ȡָ��סԺ�ŵķ�������
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("zyid");//סԺID��
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����zyid����");
                                        return "";
                                    }
                                    retDataXML = zy.getZYPatientFee(areacode, hospitalcode, ls_tmp, out mess);
                                    return mess;
                                    #endregion
                                case "2212"://�ϴ�һ��סԺ���˵ķ���
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("zyid");//סԺID��
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����zyid����");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "Ӧ�������ݵ�XML�ַ�������Ϊ��");
                                        return "";
                                    }
                                    retDataXML = zy.putZYPatientFee(areacode, hospitalcode, ls_tmp, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "2213"://ɾ��һ��סԺ���˵����з���
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("zyid");//סԺID��
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����zyid����");
                                        return "";
                                    }
                                    retDataXML = zy.delZYPatientFee(areacode, hospitalcode, ls_tmp);
                                    return "";
                                    #endregion
                                #endregion
                                #region//סԺ���˽������
                                case "2222"://�Խ���
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    if (uploadDataXML.Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "Ӧ�������ݵ�XML�ַ�������Ϊ��");
                                        return "";
                                    }
                                    retDataXML = zy.settleZY(hospitalcode,0, 8, "", "", 0, uploadDataXML, out mess);
                                    return mess;
                                    #endregion
                                case "2223"://��ʽ����
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }

                                    string opername_2223 = "";
                                    decimal hospid;

                                    try
                                    {
                                        lis = xmldoc.GetElementsByTagName("zyid");//סԺID��
                                        ls_tmp = Convert.ToString(lis[0].InnerText);
                                        if (!isNumber(ls_tmp))
                                        {
                                            retDataXML = "סԺ�ű����������ͣ�����סԺ�š�";
                                            return mess;
                                        }
                                        hospid = Convert.ToDecimal(ls_tmp);
                                    }
                                    catch (Exception ee)
                                    {
                                        retDataXML = "��ȡסԺ��ʧ��:" + ee.Message.ToString();
                                        return mess;
                                    }

                                    lis = xmldoc.GetElementsByTagName("czyxm");//����Ա����(czyxm)
                                    opername_2223 = Convert.ToString(lis[0].InnerText);
                                    retDataXML = zy.settleZY(hospitalcode, hospid, 8, "", opername_2223, 1, "", out mess);
                                    return mess;
                                    #endregion

                                #endregion
                                #region HISOnline��HISWebservice��ȡ����
                                case "4101":
                                    #region ����סԺҩƷ����Ŀ¼
                                    string servername4101, dbname4101;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4101 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4101))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����server����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4101 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4101))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����db����");
                                        return "";
                                    } 
                                    retDataXML = ml.getZYCatalogMediFromHIS(areacode, hospitalcode, servername4101, dbname4101, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4102":
                                    #region �ϴ�סԺҩƷ����ҽ�Ʒ���ƥ������
                                    string servername4102, dbname4102;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4102 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4102))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����server����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4102 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4102))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����db����");
                                        return "";
                                    }
                                    retDataXML = ml.putZYPPHIS(areacode, hospitalcode, servername4102, dbname4102, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4103":
                                    #region ����סԺƥ���������
                                    string servername4103, dbname4103;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4103 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4103))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����server����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4103 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4103))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����db����");
                                        return "";
                                    }
                                    retDataXML = ml.getZYPPHIS(areacode, hospitalcode, servername4103, dbname4103, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4105":
                                    #region ��ȡ����ҩƷ����Ŀ¼
                                    string servername4105, dbname4105;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4105 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4105))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����server����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4105 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4105))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����db����");
                                        return "";
                                    }
                                    retDataXML = ml.getMZCatalogMediFromHIS(areacode, hospitalcode, servername4105, dbname4105, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4106":
                                    #region �ϴ�����ҩƷ����ҽ�Ʒ���ƥ������
                                    string servername4106, dbname4106;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4106 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4106))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����server����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4106 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4106))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����db����");
                                        return "";
                                    }
                                    retDataXML = ml.putMZPPHIS(areacode, hospitalcode, servername4106, dbname4106, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4107":
                                    #region ��������ƥ���������
                                    string servername4107, dbname4107;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4107 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4107))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����server����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4107 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4107))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����db����");
                                        return "";
                                    }
                                    retDataXML = ml.getMZPPHIS(areacode, hospitalcode, servername4107, dbname4107, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4111":
                                    #region ��ȡסԺҽ�Ʒ���Ŀ¼
                                    string servername4111, dbname4111;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4111 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4111))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����server����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4111 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4111))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����db����");
                                        return "";
                                    }
                                    retDataXML = ml.getZYCatalogServerFromHIS(areacode, hospitalcode, servername4111, dbname4111, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4115":
                                    #region ��ȡ����ҽ�Ʒ���Ŀ¼
                                    string servername4115, dbname4115;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4115 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4115))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����server����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4115 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4115))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����db����");
                                        return "";
                                    }
                                    retDataXML = ml.getMZCatalogServerFromHIS(areacode, hospitalcode, servername4115, dbname4115, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4121":
                                    #region ��ȡICD10Ŀ¼
                                    string servername4121, dbname4121;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4121 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4121))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����server����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4121 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4121))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����db����");
                                        return "";
                                    }
                                    retDataXML = ml.getCatalogICD10HIS(areacode, hospitalcode, servername4121, dbname4121, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4122":
                                    #region �ϴ�ICD-10ƥ������
                                    string servername4122, dbname4122;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4122 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4122))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����server����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4122 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4122))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����db����");
                                        return "";
                                    }
                                    retDataXML = ml.putNHICD10PPHIS(areacode, hospitalcode, servername4122, dbname4122, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4123":
                                    #region ��������˵�ICD-10ƥ������
                                    string servername4123, dbname4123;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4123 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4123))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����server����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4123 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4123))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����db����");
                                        return "";
                                    }
                                    retDataXML = ml.getNHICD10PPHIS(areacode, hospitalcode, servername4123, dbname4123, "RDHNHISWebService", "RDHN_NCMS1.0Used");
                                    return "";
                                    #endregion
                                case "4201"://�ϴ�һ����������
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    string servername4201, dbname4201, hospid4201;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4201 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4201))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����server����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4201 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4201))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����db����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("zyid");//סԺID��
                                    hospid4201 = Convert.ToString(lis[0].InnerText);
                                    if (hospid4201 == null || hospid4201.Trim().Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����zyid����Ϊ��");
                                        return "";
                                    }
                                    else
                                    {
                                        if (!isNumber(hospid4201))
                                        {
                                            retDataXML = getXMLErroStrFromString("2000", "����zyid����Ϊ������");
                                            return "";
                                        }
                                    }
                                    retDataXML = zy.putZYPatientFromHIS(areacode, hospitalcode, servername4201, dbname4201, "RDHNHISWebService", "RDHN_NCMS1.0Used", hospid4201);
                                    return "";
                                    #endregion
                                case "4202"://�ϴ�סԺ��������
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    }
                                    string servername4202, dbname4202, hospid4202, startdate4202, enddate4202;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4202 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4202))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����server����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4202 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4202))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����db����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("zyid");//סԺID��
                                    hospid4202 = Convert.ToString(lis[0].InnerText);
                                    if (hospid4202 == null || hospid4202.Trim().Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����zyid����Ϊ��");
                                        return "";
                                    }
                                    else
                                    {
                                        if (!isNumber(hospid4202))
                                        {
                                            retDataXML = getXMLErroStrFromString("2000", "����zyid����Ϊ������");
                                            return "";
                                        }
                                    }
                                    lis = xmldoc.GetElementsByTagName("ksrq");
                                    startdate4202 = Convert.ToString(lis[0].InnerText);
                                    if (!isDateTime(startdate4202.Substring(0, 4) + "-" + startdate4202.Substring(4, 2) + "-" + startdate4202.Substring(6, 2) + " 00:00:00"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "ksrq��ʽ����Ϊ'20090125'�ĸ�ʽ");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("jsrq");
                                    enddate4202 = Convert.ToString(lis[0].InnerText);
                                    if (!isDateTime(enddate4202.Substring(0, 4) + "-" + enddate4202.Substring(4, 2) + "-" + enddate4202.Substring(6, 2) + " 00:00:00"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "jsrq��ʽ����Ϊ'20090125'�ĸ�ʽ");
                                        return "";
                                    }
                                    retDataXML = zy.putZYPatientFeeFromHIS(areacode, hospitalcode, servername4202, dbname4202, "RDHNHISWebService", "RDHN_NCMS1.0Used", hospid4202, startdate4202, enddate4202);
                                    return "";
                                    #endregion
                                case "4203"://�ϴ�סԺ���˺ͷ�������
                                    #region
                                    if (InterFacePower == 10 || InterFacePower == 1 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "סԺҵ������ӿ�δ���");
                                        return "";
                                    } 
                                    string servername4203, dbname4203, hospid4203, startdate4203, enddate4203;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4203 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4203))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����server����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4203 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4203))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����db����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("zyid");//סԺID��
                                    hospid4203 = Convert.ToString(lis[0].InnerText);
                                    if (hospid4203 == null || hospid4203.Trim().Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����zyid����Ϊ��");
                                        return "";
                                    }
                                    else
                                    {
                                        if (!isNumber(hospid4203))
                                        {
                                            retDataXML = getXMLErroStrFromString("2000", "����zyid����Ϊ������");
                                            return "";
                                        }
                                    }
                                    lis = xmldoc.GetElementsByTagName("ksrq");
                                    startdate4203 = Convert.ToString(lis[0].InnerText);
                                    if (!isDateTime(startdate4203.Substring(0, 4) + "-" + startdate4203.Substring(4, 2) + "-" + startdate4203.Substring(6, 2) + " 00:00:00"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "ksrq��ʽ����Ϊ'20090125'�ĸ�ʽ");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("jsrq");
                                    enddate4203 = Convert.ToString(lis[0].InnerText);
                                    if (!isDateTime(enddate4203.Substring(0, 4) + "-" + enddate4203.Substring(4, 2) + "-" + enddate4203.Substring(6, 2) + " 00:00:00"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "jsrq��ʽ����Ϊ'20090125'�ĸ�ʽ");
                                        return "";
                                    }
                                    retDataXML = zy.putZYDataFromHIS(areacode, hospitalcode, servername4203, dbname4203, "RDHNHISWebService", "RDHN_NCMS1.0Used", hospid4203, startdate4203, enddate4203);
                                    return "";
                                    #endregion
                                case "4212"://�ϴ������������
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    string servername4212, dbname4212, hospcode4212;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4212 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4212))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����server����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4212 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4212))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����db����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("mzh");//�����
                                    hospcode4212 = Convert.ToString(lis[0].InnerText);
                                    if (hospcode4212 == null || hospcode4212.Trim().Length == 0)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����mzh����Ϊ��");
                                        return "";
                                    }
                                    retDataXML = mz.putMZPatientFeeFromHIS(areacode, hospitalcode, servername4212, dbname4212, "RDHNHISWebService", "RDHN_NCMS1.0Used", hospcode4212);
                                    return "";
                                    #endregion
                                case "4501"://����ָ�����ڶε���ͨ��������
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    string servername4501, dbname4501, system4501, startdate4501, enddate4501;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4501 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4501))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����server����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4501 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4501))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����db����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("system");
                                    system4501 = Convert.ToString(lis[0].InnerText);
                                    lis = xmldoc.GetElementsByTagName("startdate");//��ʼ����
                                    startdate4501 = Convert.ToString(lis[0].InnerText);
                                    if (!isDateTime(startdate4501.Substring(0, 4) + "-" + startdate4501.Substring(4, 2) + "-" + startdate4501.Substring(6, 2) + " 00:00:00"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "startdate��ʽ����Ϊ'20090125'�ĸ�ʽ");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("enddate");//��������
                                    enddate4501 = Convert.ToString(lis[0].InnerText);
                                    if (!isDateTime(enddate4501.Substring(0, 4) + "-" + enddate4501.Substring(4, 2) + "-" + enddate4501.Substring(6, 2) + " 00:00:00"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "enddate��ʽ����Ϊ'20090125'�ĸ�ʽ");
                                        return "";
                                    }
                                    retDataXML = mz.getMZCommData(hospitalcode, servername4501, dbname4501, system4501, "RDHNHISWebService", "RDHN_NCMS1.0Used", "0", startdate4501, enddate4501, out mess);
                                    return mess;
                                    #endregion
                                case "4502"://����ָ�����ڶε��������
                                    #region
                                    if (InterFacePower == 0 || InterFacePower == 11)
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����ҵ������ӿ�δ���");
                                        return "";
                                    }
                                    string servername4502, dbname4502, system4502, startdate4502, enddate4502;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servername4502 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servername4502))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����server����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbname4502 = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbname4502))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����db����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("system");
                                    system4502 = Convert.ToString(lis[0].InnerText);
                                    lis = xmldoc.GetElementsByTagName("startdate");//��ʼ����
                                    startdate4502 = Convert.ToString(lis[0].InnerText);
                                    if (!isDateTime(startdate4502.Substring(0, 4) + "-" + startdate4502.Substring(4, 2) + "-" + startdate4502.Substring(6, 2) + " 00:00:00"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "startdate��ʽ����Ϊ'20090125'�ĸ�ʽ");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("enddate");//��������
                                    enddate4502 = Convert.ToString(lis[0].InnerText);
                                    if (!isDateTime(enddate4502.Substring(0, 4) + "-" + enddate4502.Substring(4, 2) + "-" + enddate4502.Substring(6, 2) + " 00:00:00"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "enddate��ʽ����Ϊ'20090125'�ĸ�ʽ");
                                        return "";
                                    }
                                    retDataXML = mz.getMZCommData(hospitalcode, servername4502, dbname4502, system4502, "RDHNHISWebService", "RDHN_NCMS1.0Used", "1", startdate4502, enddate4502, out mess);
                                    return mess;
                                    #endregion
                                case "HISSQL": //��ѯHIS����
                                    #region
                                    string servernamehissql, dbnamehissql, sqlhissql;
                                    lis = xmldoc.GetElementsByTagName("server");
                                    servernamehissql = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(servernamehissql))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����server����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("db");
                                    dbnamehissql = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(dbnamehissql))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����db����");
                                        return "";
                                    }
                                    lis = xmldoc.GetElementsByTagName("sql");
                                    sqlhissql = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(sqlhissql))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����sql����");
                                        return "";
                                    }
                                    getDataFromHIS(servernamehissql, dbnamehissql, "RDHNHISWebService", "RDHN_NCMS1.0Used", sqlhissql, out retDataXML, out mess);
                                    return mess;
                                    #endregion
                                #endregion
                                #region �ؼ��������
                                case "5001":
                                    #region ָ�����µĻ�����㵥��ѯ
                                    lis = xmldoc.GetElementsByTagName("month");//����
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����month����");
                                        return "";
                                    }
                                    retDataXML = foundationSettleBillQuery(areacode, hospitalcode, ls_tmp, out mess);
                                    return mess;
                                    #endregion
                                case "5002":
                                    #region ָ��������㵥���ݲ�ѯ
                                    lis = xmldoc.GetElementsByTagName("bill");//����
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����bill����");
                                        return "";
                                    }
                                    retDataXML = foundationSettleBillData(areacode, hospitalcode, ls_tmp, out mess);
                                    return mess;
                                    #endregion
                                case "5003":
                                    #region ָ��������㵥�������ݲ�ѯ
                                    lis = xmldoc.GetElementsByTagName("bill");//���㵥��
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����bill����");
                                        return "";
                                    }
                                    string settletype;//���㵥���� MZ:���� ZY:סԺ
                                    lis = xmldoc.GetElementsByTagName("settletype");
                                    settletype =  Convert.ToString(lis[0].InnerText).ToUpper();
                                    if (!(settletype == "MZ" || settletype == "ZY"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����settletype����");
                                        return "";
                                    }
                                    retDataXML = foundationSettleBillData_collect(areacode, hospitalcode,settletype, ls_tmp, out mess);
                                    return mess;
                                    #endregion
                                case "5004":
                                    #region ָ��������㵥��ϸ���ݲ�ѯ
                                    lis = xmldoc.GetElementsByTagName("bill");//���㵥��
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isValidParm(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����bill����");
                                        return "";
                                    }
                                    string bill;
                                    bill = ls_tmp;
                                    
                                    string settletype5004;//���㵥���� MZ:���� ZY:סԺ
                                    lis = xmldoc.GetElementsByTagName("settletype");
                                    settletype5004 = Convert.ToString(lis[0].InnerText).ToUpper();
                                    if (!(settletype5004 == "MZ" || settletype5004 == "ZY"))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����settletype����");
                                        return "";
                                    }

                                    int startrow5004, endrow5004;
                                    lis = xmldoc.GetElementsByTagName("startrownum");
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isNumber(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����startrow����");
                                        return "";
                                    }
                                    startrow5004 = Convert.ToInt32(ls_tmp);

                                    lis = xmldoc.GetElementsByTagName("endrownum");
                                    ls_tmp = Convert.ToString(lis[0].InnerText);
                                    if (!isNumber(ls_tmp))
                                    {
                                        retDataXML = getXMLErroStrFromString("2000", "����endrow����");
                                        return "";
                                    }
                                    endrow5004 = Convert.ToInt32(ls_tmp);
                                    retDataXML = foundationSettleBillData_detail(areacode, hospitalcode, settletype5004, bill,startrow5004,endrow5004,out mess);
                                    return mess;                                    
                                    #endregion
                                #endregion
                                default:
                                    #region
                                    retDataXML = getXMLErroStrFromString("2000", "û�ж����ҵ���������" + requesttype + ")");
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
                        retDataXML = getXMLErroStrFromString("9000", "�쳣��" + e.Message.ToString() + ",����ȱ������Ԫ");
                    }
                    else
                    {
                        retDataXML = getXMLErroStrFromString("9000", errtxt + "�쳣��" + e.Message.ToString() + ",����ȱ������Ԫ");
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

    [WebMethod( Description = "ʡƽ̨���ýӿڴ���ҵ��ķ���")]
    public string[]  rdhlrcmsterrace(string In_aqyz_XML,string In_gnbh,string In_jysr_XML)
    {
        string[] retstr = new string[4];
        string areacode = null,mess = null;

        retstr[0] = "1";//���׷��ر�־ 0:�ɹ�,retstr[3]Ϊ����retstr[2]Ϊ��Ч�����XML 1:ʧ��,retstr[2]Ϊ��,retstr[3]����������Ϣ
        retstr[1] = "";//������ˮ��,�ݶ�Ϊ��
        retstr[2] = "";//�������XML �����׳ɹ��򷵻���Ч��XML��������Ϊ��
        retstr[3] = "";//������Ϣ ������ʧ��ʱ���ش�����Ϣ �ɹ���Ϊ��

        XmlDocument xdoc = getXMLDocumentFromString(In_jysr_XML, out mess);
        if (mess != "TRUE")
        {
            retstr[3] = mess;
            return retstr;
        }
        XmlNodeList xnlist;

        #region ��������XML��û��������룬����ֻ�ܾ������ҵ������ʼ����ʵ��
        switch (In_gnbh.ToUpper())
        {
            #region �κ���֤
            case "F1020101"://��ҽ��֤+����/�˱��
            case "F1020102"://�����֤+����
            case "F1020103"://��ҽ��֤����һ����Ϣ           
                xnlist = xdoc.GetElementsByTagName("admorg_code");
                areacode = xnlist[0].InnerText;
                if (areacode.Length == 0)
                {
                    retstr[3] = "��ʼ��ʧ��:����дũ�ϻ������롣";
                    return retstr;
                }
                areacode = areacode.Substring(0,6);
                break;
            #endregion
            #region ת�����뼰ȡ������
            case "F1020120":                            
            case "F1020122":
                xnlist = xdoc.GetElementsByTagName("referral_no");
                areacode = xnlist[0].InnerText;
                if (areacode.Length == 0)
                {
                    retstr[3] = "��ʼ��ʧ��:����дת�����뵥�š�";
                    return retstr;
                }
                areacode = areacode.Substring(0, 6);
                break;
            #endregion
            #region ��ȡת����Ϣ
            case "F1020131":
                xnlist = xdoc.GetElementsByTagName("admorg_code");
                areacode = xnlist[0].InnerText;
                if (areacode.Length == 0)
                {
                    retstr[3] = "��ʼ��ʧ��:����дũ�ϻ������롣";
                    return retstr;
                }
                areacode = areacode.Substring(0, 6);
                break;
            #endregion
            #region ��ȡסԺ��Ϣ
            case "F1020151":
                xnlist = xdoc.GetElementsByTagName("referral_no");
                areacode = xnlist[0].InnerText;
                if (areacode.Length == 0)
                {
                    retstr[3] = "��ʼ��ʧ��:����дת�����뵥�š�";
                    return retstr;
                }
                areacode = areacode.Substring(0, 6);
                break;
            #endregion
            #region ȡ��סԺ��Ϣ
            case "F1020153":
                xnlist = xdoc.GetElementsByTagName("referral_no");
                areacode = xnlist[0].InnerText;
                if (areacode.Length == 0)
                {
                    retstr[3] = "��ʼ��ʧ��:����дת�����뵥�š�";
                    return retstr;
                }
                areacode = areacode.Substring(0, 6);
                break;
            #endregion
            #region �ϴ�סԺ��Ϣ
            case "F1020154":
                xnlist = xdoc.GetElementsByTagName("referral_no");
                areacode = xnlist[0].InnerText;
                if (areacode.Length == 0)
                {
                    retstr[3] = "��ʼ��ʧ��:����дת�����뵥�š�";
                    return retstr;
                }
                areacode = areacode.Substring(0, 6);
                break;
                #endregion
            #region ���ú˶�
            case "F1020161":
                xnlist = xdoc.GetElementsByTagName("referral_no");
                areacode = xnlist[0].InnerText;
                if (areacode.Length == 0)
                {
                    retstr[3] = "��ʼ��ʧ��:����дת�����뵥�š�";
                    return retstr;
                }
                areacode = areacode.Substring(0, 6);
                break;
            #endregion
            #region ��Ժ����
            case "F1020171":
                xnlist = xdoc.GetElementsByTagName("referral_no");
                areacode = xnlist[0].InnerText;
                if (areacode.Length == 0)
                {
                    retstr[3] = "��ʼ��ʧ��:����дת�����뵥�š�";
                    return retstr;
                }
                areacode = areacode.Substring(0, 6);
                break;
            #endregion
            #region ȡ����Ժ����
            case "F1020172":
                xnlist = xdoc.GetElementsByTagName("referral_no");
                areacode = xnlist[0].InnerText;
                if (areacode.Length == 0)
                {
                    retstr[3] = "��ʼ��ʧ��:����дת�����뵥�š�";
                    return retstr;
                }
                areacode = areacode.Substring(0, 6);
                break;
            #endregion
            #region δ�����ҵ����
            default:
                retstr[3] = "δ�����ҵ���š�";
                return retstr;
            #endregion
        }
        //��ʼ���������ӱ���
        if (!getDBConnStr("areacode", areacode, out mess))
        {
            retstr[3] = mess;
            return retstr;
        }
        #endregion ����ҵ��ŵ������ҵ��

        ncmsifterrace terraceif;
        try
        {
            terraceif = new ncmsifterrace(DBServer, DBName, UserID, PassWord, DBConnStr, IsValidUser, AreaCode, InterFacePower);
            retstr = terraceif.findService(In_aqyz_XML, In_gnbh, In_jysr_XML);
        }
        catch (Exception e)
        {
            retstr[3] = "����ʧ��:" + e.Message.ToString();
        }
        finally
        {
            terraceif = null;
        }
        
        return retstr;
    }
    #endregion
}