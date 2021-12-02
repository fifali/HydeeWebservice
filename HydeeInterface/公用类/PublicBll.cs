using System;
using System.Text;
using System.Net;
using System.Xml;
using System.IO;
using System.Net.Cache;
using Oracle.ManagedDataAccess.Client;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Data; 
using swiftpass.utils;
using System.Collections; 

namespace IHISService
{
    public class PublicBll
    {
        #region 变量
        private ClientResponseHandler resHandler = new ClientResponseHandler();
        private Dictionary<string, string> cfg = new Dictionary<string, string>(1);
        private PayHttpClient pay = new PayHttpClient();
        private RequestHandler reqHandler = null;
        public string UserName = "";
        public string DBConnStr = "";
        public string DBServer = "";
        public string Port = "";
        public string Host = "";
        public string Server_Name = "";
        public string OrgCode = "";
        public string OrgId = "";
        public string OrgName = "";
        public string UserID = "";
        public string PassWord = "";
        public string UserPower = "";
        public string Opertime = "";
        public string Interface = "";
        public string OperUserID = "";
        public string OperPassWord = "";
        public string InterfaceUserID = "";
        public string InterfacePassWord = "";
        public bool IsValidUser = false;
        public string FunctionId = "";
        public PbulicDao dao = null;
        #endregion

        #region 结构化
        public PublicBll()
        {
            dao = new PbulicDao();
        }
        #endregion

        #region 数据库连接相关
        /*
		 * / <summary>
		 * / 获取数据库联接参数串
		 * / </summary>
		 * / <param name="OrgCode"></param>
		 * / <returns></returns>
		 */
        public bool getcnParms(string databaseini, out string mess)
        {
            XmlTextReader txtReader = new XmlTextReader(databaseini);
            try
            {
                /* 找到符合的节点获取需要的属性值 */
                while (txtReader.Read())
                {
                    txtReader.MoveToElement();
                    if (txtReader.Name == "org")
                    {
                        //if (txtReader.GetAttribute("code") == OrgCode)
                        //{
                        DBServer = txtReader.GetAttribute("DBServer");
                        Port = txtReader.GetAttribute("PORT");
                        Host = txtReader.GetAttribute("HOST");
                        Server_Name = txtReader.GetAttribute("SERVICE_NAME");
                        UserID = txtReader.GetAttribute("UserID");
                        PassWord = txtReader.GetAttribute("PassWord");
                        break;
                        //}
                        //if (txtReader.NodeType.ToString() == "EndElement")
                        //{
                        //    break;
                        //}
                    }
                }
                if (DBServer == "")
                {
                    mess = "获取机构" + OrgCode + "的数据库连接参数错误，请检查配置文件！";
                    dao.RollbackTrans();
                    return (false);
                }
                else
                {
                    mess = "Data Source=(DESCRIPTION =    (ADDRESS_LIST =      (ADDRESS = (PROTOCOL = TCP)(HOST = " + Host + ")(PORT = " + Port + "))    )    (CONNECT_DATA =      (SERVER = DEDICATED)      (SERVICE_NAME = " + Server_Name + ")    )  );Persist Security Info=True;User ID=" + UserID + ";Password=" + PassWord + ";";
                    return (true);
                }
            }
            catch (Exception e)
            {
                if (e.Message.ToString().Contains("未能找到文件"))
                {
                    mess = "服务器没有找到机构" + OrgCode + "的数据库连接配置参数！";
                }
                else
                {
                    mess = "获取机构" + OrgCode + "的数据库连接参数错误：" + e.Message.ToString();
                }
                dao.RollbackTrans();
                return (false);
            }
            finally
            {
                txtReader.Close();
            }
        }

        public bool geturlParms(string databaseini, out string url)
        {
            XmlTextReader txtReader = new XmlTextReader(databaseini);
            try
            {
                /* 找到符合的节点获取需要的属性值 */
                while (txtReader.Read())
                {
                    txtReader.MoveToElement();
                    if (txtReader.Name == "org")
                    {
                        url = txtReader.GetAttribute("url");
                        return (true);
                    }
                }
                url = "";
                return (false);
            }
            catch (Exception e)
            {
                url = e.Message.ToString();
                return (false);
            }
            finally
            {
                txtReader.Close();
            }
        }
        /*
         * / <summary>
         * / 根据给定的参数类型获得数据库联接串
         * / </summary>
         * / <param name="DataType"></param>
         * / <param name="Data"></param>
         * / <param name="mess"></param>
         * / <returns></returns>
         */
        public bool getDBConnStr(string DataType, string Data, out string mess)
        {
            try
            {
                if (DBConnStr != "")
                {
                    mess = DBConnStr;
                    return (true);
                }
                else
                {
                    switch (DataType)
                    {
                        case "OrgCode":
                            OrgCode = Data;
                            break;
                        case "hospitalcode":
                            OrgCode = Data.Substring(0, 6);
                            break;
                        case "bookcard":
                            OrgCode = Data.Substring(0, 6);
                            break;
                        default:
                            mess = "指定的区域型数据类型错误，无法去联接数据库！";
                            return (false);
                    }
                    if (getcnParms(OrgCode, out mess))
                    {
                        DBConnStr = "Provider=SQLOLEDB.1;Persist Security Info=False;" + mess;
                        return (true);
                    }
                    else
                    {
                        return (false);
                    }
                }
            }
            catch (Exception e)
            {
                DBConnStr = "";
                mess = "寻址数据库异常：" + e.Message.ToString();
                return (false);
            }
        }
        #endregion

        #region 用户状态
        /*
		 * / <summary>
		 * / 得到当前用户是否有效
		 * / </summary>
		 * / <returns></returns>
		 */
        public bool getUserState()
        {
            if (IsValidUser)
            {
                return (true);
            }
            return (false);
        }

        /*
         * / <summary>
         * / 设置当前用户是否有效
         * / </summary>
         * / <param name="IfValid"></param>
         * / <returns></returns>
         */
        public bool setUserState(bool IfValid)
        {
            IsValidUser = IfValid;
            return (IfValid);
        }
        #endregion

        #region 用户验证
        /*
		 * / <summary>
		 * / 验证用户身份，返回“TRUE”表示验证通过，InterFacePower表示允许使用的接口业务类型
		 * / </summary>
		 * / <param name="areacode"></param>
		 * / <param name="hospitalcode"></param>
		 * / <param name="userid"></param>
		 * / <param name="pwd"></param>
		 * / <returns></returns>
		 * [WebMethod(Description="Service用户身份验证，返回“TRUE”表示验证通过")]
		 */
        public string checkUserValid(string FunctionId, string userid, string pwd, string operuserid, string operuserpass, string mess)
        {
            string ls_sql;
            dao.cn = new Oracle.ManagedDataAccess.Client.OracleConnection(mess);
            if (FunctionId == "999")
            { return ""; }
            OracleDataReader myReader = null;
            try
            {
                dao.Open();
                dao.BeginTrans();
                ls_sql = "SELECT Userguid,UserName,departguid,(select Departname from Tb_Dms_Depart where DepartCode = '" + OrgCode + "') as departname FROM Tb_Dms_User WHERE loginname ='" + operuserid + "' AND UserPass='" + operuserpass + "' AND DepartGUID in (select DepartGUID from Tb_Dms_Depart where DepartCode = '" + OrgCode + "')";
                dao.cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, dao.cn);

                if (dao.inTransaction)
                {
                    dao.cmd.Transaction = dao.trans;
                }
                myReader = dao.cmd.ExecuteReader();
                if (!myReader.HasRows)
                {
                    return ("用户代码或密码错误，身份验证失败！");
                }
                else
                {
                    myReader.Read();
                    UserID = myReader.GetString(0);
                    UserName = myReader.GetString(1);
                    OrgId = myReader.GetString(2);
                    OrgName = myReader.GetString(3);
                    setUserState(true);
                    return ("TRUE");
                }
            }
            catch (Exception e)
            {
                dao.RollbackTrans();
                return ("验证异常！" + e.Message.ToString());
            }
            finally
            {

                if (myReader != null)
                {
                    if (!myReader.IsClosed)
                        myReader.Close();
                    myReader.Dispose();
                }
                dao.cmd = null;
            }
        }

        public string checkUserValid_hydee(string FunctionId, string userid, string pwd, string operuserid, string operuserpass, string mess)
        {
            string ls_sql;
            dao.cn = new Oracle.ManagedDataAccess.Client.OracleConnection(mess);
            if (FunctionId == "2001" || FunctionId == "2002" || FunctionId == "2003" || FunctionId == "2004" || FunctionId == "2005" || FunctionId == "2006" || FunctionId == "3001" || FunctionId == "3003")
            { return "TRUE"; }
            dao.cmd = null;
            OracleDataReader myReader = null;
            try
            {
                dao.Open();
                dao.BeginTrans();
                ls_sql = "SELECT Userguid,UserName FROM Tb_hydee_User WHERE loginname ='" + operuserid + "' AND UserPass='" + operuserpass + "' and status = '1'";
                dao.cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, dao.cn);

                if (dao.inTransaction)
                {
                    dao.cmd.Transaction = dao.trans;
                }
                myReader = dao.cmd.ExecuteReader();
                if (!myReader.HasRows)
                {
                    return ("用户代码或密码错误，身份验证失败！");
                }
                else
                {
                    myReader.Read();
                    UserID = myReader.GetString(0);
                    UserName = myReader.GetString(1);
                    //OrgId = myReader.GetString(2);
                    //OrgName = myReader.GetString(3);
                    setUserState(true);
                    return ("TRUE");
                }
            }
            catch (Exception e)
            {
                dao.RollbackTrans();
                return ("验证异常！" + e.Message.ToString());
            }
            finally
            {

                if (myReader != null)
                {
                    if (!myReader.IsClosed)
                        myReader.Close();
                    myReader.Dispose();
                }
                dao.cmd = null;
            }
        }
        #endregion

        #region 获取GUID
        public string getguid()
        {
            string ls_return = "";
            Oracle.ManagedDataAccess.Client.OracleCommand cmdnew;
            try
            {
                dao.Open();
                cmdnew = new Oracle.ManagedDataAccess.Client.OracleCommand("SELECT createguid() from dual", dao.cn);
                if (dao.inTransaction)
                {
                    cmdnew.Transaction = dao.trans;
                }
                OracleDataReader myReader = cmdnew.ExecuteReader();
                try
                {
                    if (!myReader.HasRows)
                    {
                        return ("获取GUID失败！");
                    }
                    else
                    {
                        myReader.Read();
                        ls_return = myReader.GetString(0);
                        return (ls_return);
                    }
                }
                finally
                {
                    if (myReader != null)
                    {
                        if (!myReader.IsClosed)
                            myReader.Close();
                        myReader.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                return ("获取GUID异常！" + e.Message.ToString());
            }
            finally
            {
                cmdnew = null;
            }
        }
        #endregion

        #region 消息体返回
        public string ReturnHead(bool ifcommit, List<RetHeadJson> _RetHeadJsont, string ReqHeadJson, string RetDataJson, string ReqBodyJson, string funccode)
        {
            string ls_retJson;
            ObjectList.RetHeadJsonList _ObjRetHeadList;
            if (ifcommit)
            {
                dao.CommitTrans();
            }
            else
            {
                dao.RollbackTrans();
            }
            _ObjRetHeadList = new ObjectList.RetHeadJsonList();
            _ObjRetHeadList.Head = _RetHeadJsont;
            ls_retJson = new JavaScriptSerializer().Serialize(_ObjRetHeadList);
            dao.WriteLog(OperUserID, DateTime.Now, ReqHeadJson, RetDataJson, ReqBodyJson, ls_retJson, "BUSINESS", null, FunctionId, funccode);
            return ls_retJson;
        }
        #endregion

        #region 处理
        public string ReturnHead_hydee(bool ifcommit, List<RetHeadJson> _RetHeadJsont, string ReqHeadJson, string RetDataJson, string ReqBodyJson, string funccode, string ls_connect)
        {
            //string ls_retJson;
            //string ls_returnCode;
            //string ls_respCode;
            //string _biz_content;
            //string ls_retmsg;
            //PublicBll bll = new PublicBll();
            //DataTable dt = null;

            //if (ifcommit)
            //{
            //    dao.CommitTrans();
            //}
            //else
            //{
            //    dao.RollbackTrans();
            //}

            if (_RetHeadJsont[0].functionid != "3001")
            {
                return _RetHeadJsont[0].returnmsg;
            }

            if (_RetHeadJsont[0].returnmsg != "TRUE")
            {
                return _RetHeadJsont[0].returnmsg;
            }
            //ObjectList.RetHeadJsonList _ObjRetHeadList;
            //_ObjRetHeadList = new ObjectList.RetHeadJsonList();
            //_ObjRetHeadList.Head = _RetHeadJsont;
            //ls_retJson = new JavaScriptSerializer().Serialize(_ObjRetHeadList);
            //dao.WriteLog_hydee(OperUserID, DateTime.Now, ReqHeadJson, RetDataJson, ReqBodyJson, ls_retJson, "BUSINESS", null, FunctionId, funccode);
            //var jObject = JObject.Parse(RetDataJson);
            //if (jObject.Property("returnCode") == null)
            //{
            //    return "返回字符串中未找到returnCode标签";
            //}
            //ls_returnCode = jObject["returnCode"].ToString();
            //if (ls_returnCode == "FAIL")
            //{
            //    return jObject["respMsg"].ToString();
            //}
            //if (jObject.Property("respCode") == null)
            //{
            //    return "返回字符串中未找到respCode标签";
            //}
            //ls_respCode = jObject["respCode"].ToString();
            //if (ls_respCode == "FAIL")
            //{
            //    return jObject["respMsg"].ToString();
            //}
            //if (jObject.Property("biz_content") == null)
            //{
            //    return "返回字符串中未找到biz_content标签";
            //}
            //List<interface_gjzh_fkmsk_ret> _interface_gjzh_fkmsk_ret;
            //_biz_content = jObject["biz_content"].ToString();
            //_biz_content = "[" + _biz_content.Replace("\\\"", "\"") + "]";
            //_interface_gjzh_fkmsk_ret = new List<interface_gjzh_fkmsk_ret>();
            //_interface_gjzh_fkmsk_ret = PublicClass.JsonStringToList<interface_gjzh_fkmsk_ret>(_biz_content);
            ////_interface_gjzh_fkmsk_ret = JsonHelper.DeserializeJsonToList<interface_gjzh_fkmsk_ret>(_biz_content);
            //if (_interface_gjzh_fkmsk_ret.Count == 0)
            //{
            //    return "biz_content字符串中未找到消息体";
            //}
            //for (int i = 0; i < _interface_gjzh_fkmsk_ret.Count; i++)
            //{
            //    dt = PublicClass.ForeachClassProperties<interface_gjzh_fkmsk_ret>(_interface_gjzh_fkmsk_ret[i], dt, "", "");
            //}
            //bll.dao.cn = new Oracle.ManagedDataAccess.Client.OracleConnection(ls_connect);
            //ls_retmsg = bll.dao.InsertDataTable(dt);
            //if (ls_retmsg != "TRUE")
            //{
            //    return ls_retmsg;
            //}
            return "SUCCESS";
        }
        #endregion

        #region 调用http
        public string HttpWeb(string sUrl, string appid, string apisign, string timestamp, string sPostData)
        {
            string sMode = "POST";
            Encoding myEncoding = Encoding.UTF8;
            //string sContentType = "application/x-www-form-urlencoded";
            string sContentType = "application/json";
            HttpWebRequest req;

            try
            {
                // init
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                req = HttpWebRequest.Create(sUrl) as HttpWebRequest;
                req.Method = sMode;
                req.Accept = "*/*";
                req.KeepAlive = false;
                req.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                req.Headers.Add("appid", appid);
                req.Headers.Add("timestamp", timestamp);
                req.Headers.Add("apisign", apisign);
                if (0 == string.Compare("POST", sMode))
                {
                    byte[] bufPost = myEncoding.GetBytes(sPostData);
                    req.ContentType = sContentType;
                    req.ContentLength = bufPost.Length;
                    Stream newStream = req.GetRequestStream();
                    newStream.Write(bufPost, 0, bufPost.Length);
                    newStream.Close();
                }

                // Response
                HttpWebResponse res = req.GetResponse() as HttpWebResponse;
                try
                {
                    // 找到合适的编码
                    Encoding encoding = null;
                    //encoding = Encoding_FromBodyName(res.CharacterSet);	// 后来发现主体部分的字符集与Response.CharacterSet不同.
                    //if (null == encoding) encoding = myEncoding;
                    encoding = myEncoding;
                    //System.Diagnostics.Debug.WriteLine(encoding);

                    // body
                    using (Stream resStream = res.GetResponseStream())
                    {
                        using (StreamReader resStreamReader = new StreamReader(resStream, encoding))
                        {
                            return resStreamReader.ReadToEnd();
                        }
                    }
                }
                finally
                {
                    res.Close();
                }
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }
        #endregion

        #region XML字符串转DS
        public DataSet ConvertXMLToDataSet(string xmlData)
        {
            StringReader stream = null;
            XmlTextReader reader = null;
            try
            {
                DataSet xmlDS = new DataSet();
                stream = new StringReader(xmlData);
                reader = new XmlTextReader(stream);
                xmlDS.ReadXml(reader);
                return xmlDS;
            }
            catch (Exception ex)
            {
                string strTest = ex.Message;
                return null;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }
        #endregion
    }
}