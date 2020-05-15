//============================================================================
// Webservice接口程序:
//----------------------------------------------------------------------------
// 描述: 
//----------------------------------------------------------------------------
// 参数:(无)
//----------------------------------------------------------------------------
// 返回值:  (none)
//----------------------------------------------------------------------------
// 作者:	lwb		日期: 2015.09.22
//----------------------------------------------------------------------------
// 修改历史: 
//	
//============================================================================
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using Oracle.ManagedDataAccess.Client;
namespace HydeeInterface
{
    #region 初始设置
    [WebService(Namespace = "mia.hn.cn")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    /*
     * 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。
     * [System.Web.Script.Services.ScriptService]
     */
    #endregion
    public class IHISService : System.Web.Services.WebService
    {
        #region Timer触发亮灯
        //public HydeeInterface()
        //{
        //    //如果使用设计的组件，请取消注释以下行 
        //    InitializeComponent();
        //}
        //private System.Timers.Timer timer1;
        //private System.ComponentModel.IContainer components;
        //private void InitializeComponent()
        //{
        //    this.components = new System.ComponentModel.Container();
        //    this.timer1 = new System.Timers.Timer();
        //    // 
        //    // timer1
        //    // 
        //    this.timer1.Enabled = true;
        //    this.timer1.Interval = 10000;
        //    this.timer1.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Elapsed);
        //}
        //        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //        {
        //            string ls_controlguid = "";
        //            string ls_order;
        //            string ls_return = "FALSE";
        //            DataTable dt = null;
        //            dt = new DataTable();
        //            dt = GetDataTable(@"select controlguid from tb_wms_control where ip = '" + as_controlip + "'");
        //            if (dt.Rows.Count > 0)
        //            {
        //                ls_controlguid = dt.Rows[0][0].ToString();
        //                dt = new DataTable();
        //                dt = GetDataTable(@"select count(1) from tb_wms_control where controlguid = '" + ls_controlguid + "' and status = '0'");
        //                if (Convert.ToInt32(dt.Rows[0][0].ToString()) > 0)
        //                {
        //                    dt = new DataTable();
        //                    dt = GetDataTable(@"select controlguid
        //                          from Tb_Wms_Pick t
        //                         where (Status = getpickstatus('已通知拣货') or
        //                               status = getpickstatus('已通知拣货【PDA作业】')
        //                                or Status = getpickstatus('已领取拣货'))
        //                           and controlguid = '" + ls_controlguid + @"'
        //                           and NatureGUID in(select NatureGUID from Tb_Wms_StoreAreaNature where naturecode = '002')
        //                        union all
        //                        select controlguid
        //                          from Tb_Wms_Invent t
        //                         where (status = getinventstatus('已通知盘点') or status = getinventstatus('已领取盘点')) 
        //                           and controlguid  = '" + ls_controlguid + @"'
        //                           and StoreAreaGUID in(select storeareaguid from tb_wms_storearea where storeareaguid = t.storeareaguid and natureguid in(select NatureGUID from Tb_Wms_StoreAreaNature where naturecode = '002'))");
        //                    if (dt.Rows.Count > 0)
        //                    {
        //                        #region 下发预处理命令3001
        //                        ls_return = "TRUE";
        //                        ls_order = "7EAA017D5E807E";
        //                        //下发预处理命令                       
        //                        //Send(handler, ls_order);
        //                        SqlDataTableCommit(@"UPDATE Tb_Wms_Control
        //				        SET status = '4'
        //				        Where controlguid = '" + ls_controlguid + "'");
        //                        WriteLog("SendControl", DateTime.Now, ls_order, null, as_controlip);                        
        //                        CommitTrans();
        //                        #endregion
        //                    }
        //                }
        //            }      
        //}
        #endregion

        #region 业务变量定义
        public string address = "";
        public string ib_iflabel = "";
        public int host;
        public int webservicehost;
        public int timers;
        public TcpClient _client = null;
        public MemoryStream memory = new MemoryStream();
        public XmlDocument xmlDoc = new XmlDocument();
        public XmlDocument doc = new XmlDocument();
        public Oracle.ManagedDataAccess.Client.OracleTransaction trans;/*事务处理类*/
        Oracle.ManagedDataAccess.Client.OracleCommand cmd = null;
        public bool inTransaction = false;/*指示当前是否正处于事务中*/
        public Oracle.ManagedDataAccess.Client.OracleConnection cn; /*数据库连接*/
        private Int32 returncode = -1;//存储过程返回编码
        private string returnmsg = "";//存储过程返回信息
        #endregion

        #region 类变量定义
        /*
		 * / <summary>
		 * / 操作时间
		 * / </summary>
		 */
        private string Opertime = "";

        /*
         * / <summary>
         * / 通讯密匙
         * / </summary>
         */
        private string Interface = "";

        /*
         * / <summary>
         * / 服务器名
         * / </summary>
         */
        private string DBServer = "";

        /*
         * / <summary>
         * / 服务器名
         * / </summary>
         */
        private string Port = "";

        /*
         * / <summary>
         * / 服务器名
         * / </summary>
         */
        private string Host = "";

        /*
         * / <summary>
         * / 服务器名
         * / </summary>
         */
        private string Server_Name = "";

        /*
         * / <summary>
         * / 数据库连接用户ID
         * / </summary>
         */
        private string UserID = "";

        /*
         * / <summary>
         * / 数据库连接用户密码
         * / </summary>
         */
        private string PassWord = "";

        /*
         * / <summary>
         * / 操作用户ID
         * / </summary>
         */
        private string OperUserID = "";

        /*
         * / <summary>
         * / 操作用户密码
         * / </summary>
         */
        private string OperPassWord = "";

        /*
         * / <summary>
         * / 接口连接用户ID
         * / </summary>
         */
        private string InterfaceUserID = "";

        /*
         * / <summary>
         * / 接口连接用户密码
         * / </summary>
         */
        private string InterfacePassWord = "";

        /*
         * / <summary>
         * / 数据库联接串
         * / </summary>
         */
        private string DBConnStr = "";

        /*
         * / <summary>
         * / 当前用户是否有效
         * / </summary>
         */
        private bool IsValidUser = false;

        /*
         * / <summary>
         * / 当前用户所在的机构代码
         * / </summary>
         */
        private string OrgCode = "";

        /*
         * / <summary>
         * / 功能编码
         * / </summary>
         */
        private string FunctionId = "";

        /*
         * / <summary>
         * / 当前用户所在的机构名称
         * / </summary>
         */
        private string OrgName = "";

        /*
         * / <summary>
         * / 当前用户所在的WMS库代码
         * / </summary>
         */
        private string WmsCode = "";

        /*
         * / <summary>
         * / 当前DB处理类型
         * / </summary>
         */
        private string controlip = "";

        /*
         * / <summary>
         * / 接口拥有的权限
         * / </summary>
         * private int InterFacePower = -1;
         */
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
            //if (OrgCode == orgcode && DBServer != "")
            //{
            //    mess = DBConnStr + ";Password=" + PassWord;
            //    return (true);
            //}

            /* 创建一个XmlTextReader类的对象并调用Read方法来读取XML文件 */
            //databaseini = "002";
            XmlTextReader txtReader = new XmlTextReader(Server.MapPath("./DBConn/" + databaseini + ".xml"));
            try
            {
                /* 找到符合的节点获取需要的属性值 */
                while (txtReader.Read())
                {
                    txtReader.MoveToElement();
                    if (txtReader.Name == "org")
                    {
                        if (txtReader.GetAttribute("code") == OrgCode)
                        {
                            DBServer = txtReader.GetAttribute("DBServer");
                            Port = txtReader.GetAttribute("PORT");
                            Host = txtReader.GetAttribute("HOST");
                            Server_Name = txtReader.GetAttribute("SERVICE_NAME");
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
                    mess = "获取机构" + OrgCode + "的数据库联接参数错误，请检查配置文件！";
                    RollbackTrans();
                    return (false);
                }
                else
                {
                    /* mess = "Data Source=" + DBServer + ";Initial Catalog=" + DBName + ";User ID=" + UserID + ";Password=" + PassWord + ";Application Name=HNNCMS WebService cnection Pooling;Pooling=true;Min Pool Size=1;Max Pool Size=100;cnect timeout = 30"; */
                    //mess = "Server=" + DBServer + ";User ID=" + UserID + ";Password=" + PassWord + ";integrated security=no";
                    mess = "Data Source=(DESCRIPTION =    (ADDRESS_LIST =      (ADDRESS = (PROTOCOL = TCP)(HOST = "+Host+")(PORT = "+Port+"))    )    (CONNECT_DATA =      (SERVER = DEDICATED)      (SERVICE_NAME = "+Server_Name+")    )  );Persist Security Info=True;User ID="+UserID+";Password="+PassWord+";";
                    return (true);
                }
            }
            catch (Exception e)
            {
                if (e.Message.ToString().Contains("未能找到文件"))
                {
                    mess = "服务器没有找到机构" + OrgCode + "的数据库联接配置参数！";
                }
                else
                {
                    mess = "获取机构" + OrgCode + "的数据库联接参数错误：" + e.Message.ToString();
                }
                RollbackTrans();
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
        public string checkUserValid(string FunctionId, string wmscode, string userid, string pwd, string operuserid, string operuserpass, string mess, int type, int checkoperuer, out string ls_xml)
        {
            cn = new Oracle.ManagedDataAccess.Client.OracleConnection(mess);
            cmd = null;
            OracleDataReader myReader = null;
            ls_xml = null;
            try
            {
                Open();
                BeginTrans();
                if (address == "")
                {
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("SELECT serviceIP,Host,IfLabel,webservicehost,timers FROM Tb_Wms_Store WHERE StoreCode='" + wmscode + "'", cn);
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }
                    myReader = cmd.ExecuteReader();
                    if (!myReader.HasRows)
                    {
                        return ("WMS库未配置Webservice地址！");
                    }
                    else
                    {
                        myReader.Read();
                        address = myReader[0].ToString();
                        ib_iflabel = myReader[2].ToString();
                        host = Convert.ToInt32(myReader[1].ToString());
                        webservicehost = Convert.ToInt32(myReader[3].ToString());
                        timers = Convert.ToInt32(myReader[4].ToString());
                        timers = timers * 1000;
                    }
                }
                if (type == 0)
                {
                    return ("TRUE");
                }
                if (type == 1)
                {
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("SELECT StoreName FROM Tb_Wms_Store WHERE StoreCode='" + wmscode + "' AND pdaUser=('" + userid + "') AND pdaPass=('" + pwd + "')", cn);
                }
                else if (type == 2)
                {
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("SELECT StoreName FROM Tb_Wms_Store WHERE StoreCode='" + wmscode + "' AND BusinessUser=('" + userid + "') AND BusinessPass=('" + pwd + "')", cn);
                }
                else if (type == 3)
                {
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("SELECT StoreName FROM Tb_Wms_Store WHERE StoreCode='" + wmscode + "' AND BusinessUser=('" + userid + "') AND BusinessPass=('" + pwd + "') union all SELECT StoreName FROM Tb_Wms_Store WHERE StoreCode='" + wmscode + "' AND pdaUser=('" + userid + "') AND pdaPass=('" + pwd + "')", cn);
                }
                if (inTransaction)
                {
                    cmd.Transaction = trans;
                }
                myReader = cmd.ExecuteReader();
                if (!myReader.HasRows)
                {
                    return ("WMS库代码或接口用户代码或密码错误，身份验证失败！");
                }
                else
                {
                    myReader.Read();
                    OrgName = myReader.GetString(0);
                    if (checkoperuer == 1) /* 需要用户身份验证 */
                    {
                        if (FunctionId == "1040")//工号牌登陆
                        {
                            cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("SELECT UserCode,userpass,(select PdaUser from tb_wms_store where storecode = '001') as PdaUser,(select Pdapass from tb_wms_store where storecode = '001') as Pdapass,GetPower(usercode) as power,username FROM Tb_Wms_User WHERE  usercardno=('" + operuserid + "') AND IfPDA = '1'", cn);
                            if (inTransaction)
                            {
                                cmd.Transaction = trans;
                            }
                            myReader = cmd.ExecuteReader();
                            if (!myReader.HasRows)
                            {
                                return ("操作用户工号牌错误，身份验证失败！");
                            }
                            else
                            {
                                setUserState(true);
                                return "TRUE";
                            }
                        }
                        else
                        {
                            cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("SELECT UserName FROM Tb_Wms_User WHERE  UserCode=('" + operuserid + "') AND UserPass=('" + operuserpass + "') AND IfPDA = '1'", cn);
                            if (inTransaction)
                            {
                                cmd.Transaction = trans;
                            }
                            myReader = cmd.ExecuteReader();
                            if (!myReader.HasRows)
                            {
                                return ("操作用户代码或密码错误，身份验证失败！");
                            }
                            else
                            {
                                setUserState(true);
                                return ("TRUE");
                            }
                        }
                    }
                    else
                    { /*不需要用户身份验证 */
                        setUserState(true);
                        return ("TRUE");
                    }
                }
            }
            catch (Exception e)
            {
                RollbackTrans();
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
                cmd = null;
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
                Open();
                cmdnew = new Oracle.ManagedDataAccess.Client.OracleCommand("SELECT createguid() from dual", cn);
                if (inTransaction)
                {
                    cmdnew.Transaction = trans;
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

        #region 非存储过程处理业务
        public string boxbar(string boxbar, string orderguid, string checkguid, string inguid, DataTable ddt, string flag)
        {
            string ls_TransportBoxGUID;
            string ls_sql;
            string ls_wareguid;
            string ls_returnmsg;
            string ls_batchs;
            string ls_orgguid;
            string ls_curdata;
            string ls_prodata;
            string ls_expdata;
            cmd = null;
            int l_num1, l_num2, l_num3;
            string[] ls_inparamtype, ls_inparamvalue, ls_outparam, ls_outparamtype, ls_inparam;
            string ls_xml;
            string ld_num;
            OracleDataReader myReader = null;
            DataTable dt = null;
            dt = new DataTable();
            dt = GetDataTable("select orgguid from Tb_Wms_BugInstoreOrder where orderguid  = '" + orderguid + "' union all select orgguid from Tb_Wms_SaleReturnInstoreOrder where orderguid = '" + orderguid + "' union all select orgguid from Tb_Wms_PeriodInstore where inguid = '" + inguid + "' union all select orgguid from Tb_Wms_moveInstore where inguid = '" + inguid + "'");
            ls_orgguid = dt.Rows[0][0].ToString();
            try
            {
                Open();
                BeginTrans();
                ls_curdata = DateTime.Now.ToString("yyyyMMdd");
                cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("select TransportBoxGUID from Tb_Wms_TransportBox where Bar = '" + boxbar + "'", cn);
                if (inTransaction)
                {
                    cmd.Transaction = trans;
                }
                myReader = cmd.ExecuteReader();
                if (!myReader.HasRows)
                {
                    return ("周转箱条码不存在，请检查！");
                }
                else
                {
                    myReader.Read();
                    ls_TransportBoxGUID = myReader.GetString(0);

                    dt = new DataTable();
                    dt = GetDataTable("select wareguid,batchs,WareNum from TB_WMS_TRANSPORTBOXWARE where billno  = '" + inguid + "' and transportboxguid = '" + ls_TransportBoxGUID + "' and status = '0'");
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)//删除周转箱商品信息前，扣减数量
                        {
                            ls_wareguid = dt.Rows[i][0].ToString();
                            ls_batchs = dt.Rows[i][1].ToString();
                            ld_num = dt.Rows[i][2].ToString();
                            ls_returnmsg = SqlDataTable("update Tb_Wms_BugInstoreCheckDetail set QUALsingleNUM = QUALsingleNUM - " + ld_num + ",NOQUALsingleNUM = NOQUALsingleNUM + " + ld_num + " where checkguid = '" + checkguid + "' and  wareguid = '" + ls_wareguid + "'  and Batch = '" + ls_batchs + "'");
                            if (ls_returnmsg != "TRUE")
                            {
                                return ls_returnmsg;
                            }
                            ls_returnmsg = SqlDataTable("update Tb_Wms_BugInstoreCheck set QualsingleSum = QualsingleSum - " + ld_num + ",NoqualsingleSum = NoqualsingleSum + " + ld_num + " where checkguid = '" + checkguid + "'");
                            if (ls_returnmsg != "TRUE")
                            {
                                return ls_returnmsg;
                            }

                            ls_returnmsg = SqlDataTable("update Tb_Wms_SaleReturnInstoreCheckD set QUALsingleNUM = QUALsingleNUM - " + ld_num + ",NOQUALsingleNUM = NOQUALsingleNUM + " + ld_num + " where checkguid = '" + checkguid + "' and  wareguid = '" + ls_wareguid + "'  and Batch = '" + ls_batchs + "'");
                            if (ls_returnmsg != "TRUE")
                            {
                                return ls_returnmsg;
                            }
                            ls_returnmsg = SqlDataTable("update Tb_Wms_SaleReturnInstoreCheck  set QualsingleSum = QualsingleSum - " + ld_num + ",NoqualsingleSum = NoqualsingleSum + " + ld_num + " where checkguid = '" + checkguid + "'");
                            if (ls_returnmsg != "TRUE")
                            {
                                return ls_returnmsg;
                            }

                            ls_returnmsg = SqlDataTable("update TB_WMS_BUGINSTOREDETAIL set singleNUM = singleNUM - " + ld_num + " where inguid = '" + inguid + "' and  wareguid = '" + ls_wareguid + "' and batch = '" + ls_batchs + "'");
                            if (ls_returnmsg != "TRUE")
                            {
                                return ls_returnmsg;
                            }
                            ls_returnmsg = SqlDataTable("update Tb_Wms_SaleReturnInstoreDetail set singleNUM = singleNUM - " + ld_num + " where inguid = '" + inguid + "' and  wareguid = '" + ls_wareguid + "' and batch = '" + ls_batchs + "'");
                            if (ls_returnmsg != "TRUE")
                            {
                                return ls_returnmsg;
                            }
                        }
                    }

                    ls_sql = "delete from TB_WMS_TRANSPORTBOXWARE where billno = '" + inguid + "' and TRANSPORTBOXGUID = '" + ls_TransportBoxGUID + "'";
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }
                    cmd.ExecuteNonQuery();

                    for (int i = 0; i < ddt.Rows.Count; i++)
                    {
                        cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("select wareguid from tb_wms_wmsware where bar = '" + ddt.Rows[i]["warebar"].ToString() + "'", cn);
                        if (inTransaction)
                        {
                            cmd.Transaction = trans;
                        }
                        myReader = cmd.ExecuteReader();
                        if (!myReader.HasRows)
                        {
                            return ("商品条码不存在，请检查！");
                        }
                        myReader.Read();
                        ls_wareguid = myReader.GetString(0);


                        dt = new DataTable();
                        dt = GetDataTable("select sum(nvl(warenum,0)) from tb_wms_transportboxware where billno = '" + inguid + "' and wareguid = '" + ls_wareguid + "'");
                        if (dt.Rows.Count == 0)
                        {
                            l_num1 = 0;
                        }
                        else
                        {
                            if (dt.Rows[0][0].ToString() == "")
                            {
                                l_num1 = 0;
                            }
                            else
                            {
                                l_num1 = Convert.ToInt32(dt.Rows[0][0].ToString());
                            }
                        }
                        dt = new DataTable();
                        dt = GetDataTable("select sum(nvl(singlenum,0)) from (select singlenum from tb_wms_buginstoreorderdetail where orderguid = '" + orderguid + "' and wareguid = '" + ls_wareguid + "' union all select singlenum from tb_wms_salereturninstoreorderd where orderguid = '" + orderguid + "' and wareguid = '" + ls_wareguid + "' union all select singlenum from tb_wms_periodinstoredetail where inguid = '" + orderguid + "' and wareguid = '" + ls_wareguid + "' union all select MoveNum from tb_wms_moveinstoredetail where inguid = '" + orderguid + "' and wareguid = '" + ls_wareguid + "')");
                        if (dt.Rows.Count == 0)
                        {
                            l_num2 = 0;
                        }
                        else
                        {
                            if (dt.Rows[0][0].ToString() == "")
                            {
                                l_num2 = 0;
                            }
                            else
                            {
                                l_num2 = Convert.ToInt32(dt.Rows[0][0].ToString());
                            }
                        }
                        l_num3 = Convert.ToInt32(ddt.Rows[i]["warenum"].ToString());
                        if (l_num1 + l_num3 > l_num2)
                        {
                            dt = new DataTable();
                            dt = GetDataTable("select warename from tb_wms_wmsware where  wareguid = '" + ls_wareguid + "'");
                            return "商品【" + dt.Rows[0][0].ToString() + "】验收数量不能大于订单数量【" + l_num2.ToString() + "】，请检查！";
                        }

                        cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("select wareguid from Tb_Wms_BugInstoreOrderDetail where WareGUID = '" + ls_wareguid + "' and orderguid = '" + orderguid + "' union all select wareguid from Tb_Wms_SaleReturnInstoreOrderD where WareGUID = '" + ls_wareguid + "' and orderguid = '" + orderguid + "' union all select WareGUID from Tb_Wms_PeriodInstoreDetail where inguid = '" + inguid + "' and wareguid = '" + ls_wareguid + "' union all select WareGUID from Tb_Wms_MoveInstoreDetail where inguid = '" + inguid + "' and wareguid = '" + ls_wareguid + "' ", cn);
                        if (inTransaction)
                        {
                            cmd.Transaction = trans;
                        }
                        myReader = cmd.ExecuteReader();
                        if (!myReader.HasRows)
                        {
                            return ("该验收单不存在此商品，请检查！");
                        }


                        ls_expdata = ddt.Rows[i]["wareexpdate"].ToString();
                        ls_prodata = ddt.Rows[i]["wareproductdate"].ToString();

                        if (Convert.ToInt32(ls_prodata) > Convert.ToInt32(ls_curdata))
                        {
                            return ("生产日期不能大于当前日期！");
                        }
                        //System.TimeSpan ts = Convert.ToDateTime(DateTime.ParseExact(ls_expdata, "yyyyMMdd", null).ToString("yyyy-MM-dd")) - Convert.ToDateTime(DateTime.Now);
                        //if (ts.Days < 90)
                        //{
                        //    return ("有效期在90天内！");
                        //}
                        if (flag == "1")//需要输入批号效期
                        {
                            ls_sql = @"insert into TB_WMS_TRANSPORTBOXWARE( 
                               TRANSPORTBOXWAREGUID,
                               TRANSPORTBOXGUID,
                               WAREGUID,
                               WARENUM,
                               BATCHS,
                               EXPDATE,
                               BILLTYPE,
                               BILLNO,
                               BOXDATE,
                               OPERUSER,
                               STATUS,
                               productdate)
                               values
                               (
                                createguid(),
                                '" + ls_TransportBoxGUID + @"',
                                '" + ls_wareguid + @"',                                
                                 '" + ddt.Rows[i]["warenum"].ToString() + @"',
                                 '" + ddt.Rows[i]["warebatch"].ToString() + @"',
                                 to_date('" + ddt.Rows[i]["wareexpdate"].ToString() + @"','yyyymmdd'),
                                '入库单',
                                 '" + inguid + @"',
                                 sysdate,
                                 '" + OperUserID + @"',
                                '0',
                                 to_date('" + ddt.Rows[i]["wareproductdate"].ToString() + @"','yyyymmdd')
                               )";
                            cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                            if (inTransaction)
                            {
                                cmd.Transaction = trans;
                            }
                            cmd.ExecuteNonQuery();
                            //批号效期处理                        
                            ls_sql = @"select to_char(ExpDate,'yyyymmdd'),RelationGUID from Tb_Wms_WareBatchExp where wareguid = '" + ls_wareguid + "' and batchs = '" + ddt.Rows[i]["warebatch"].ToString() + "' and orgguid = '" + ls_orgguid + "'";
                            cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                            if (inTransaction)
                            {
                                cmd.Transaction = trans;
                            }
                            myReader = cmd.ExecuteReader();
                            if (myReader.HasRows)//存在该商品批号，进行效期更新
                            {
                                myReader.Read();
                                ls_sql = @"update Tb_Wms_WareBatchExp set 
                                            ExpDate = to_date('" + ddt.Rows[i]["wareexpdate"].ToString() + @"','yyyymmdd') ,
                                            productdate = to_date('" + ddt.Rows[i]["wareproductdate"].ToString() + @"','yyyymmdd') 
                                            where RelationGUID = '" + myReader.GetString(1) + "'";
                                cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                                if (inTransaction)
                                {
                                    cmd.Transaction = trans;
                                }
                                cmd.ExecuteNonQuery();
                            }
                            else
                            {
                                ls_sql = @"insert into Tb_Wms_WareBatchExp( 
                               RelationGUID,
                               WareGUID,
                               Batchs,
                               ExpDate,
                               productdate,
                               orgguid)
                               values
                               (
                                createguid(),
                                '" + ls_wareguid + @"',
                                 '" + ddt.Rows[i]["warebatch"].ToString() + @"', 
                                 to_date('" + ddt.Rows[i]["wareexpdate"].ToString() + @"','yyyymmdd'),
                                 to_date('" + ddt.Rows[i]["wareproductdate"].ToString() + @"','yyyymmdd'),
                                '" + ls_orgguid + @"'
                               )";
                                cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                                if (inTransaction)
                                {
                                    cmd.Transaction = trans;
                                }
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else//可能存在退货验收不输入批号、效期、生产日期的需求，暂时未考虑此情况
                        {

                        }

                        ls_sql = @"update Tb_Wms_TransportBox
                                   set Status = '1'
                                 where TransportBoxGUID = '" + ls_TransportBoxGUID + "'";
                        cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                        if (inTransaction)
                        {
                            cmd.Transaction = trans;
                        }
                        cmd.ExecuteNonQuery();
                    }
                }
                ls_inparam = new string[4];
                ls_inparam[0] = "as_orderguid";
                ls_inparam[1] = "as_checkguid";
                ls_inparam[2] = "as_inguid";
                ls_inparam[3] = "as_guid";

                ls_inparamtype = new string[4];
                ls_inparamtype[0] = "varchar";
                ls_inparamtype[1] = "varchar";
                ls_inparamtype[2] = "varchar";
                ls_inparamtype[3] = "varchar";
                ls_inparamvalue = new string[4];
                ls_inparamvalue[0] = orderguid;
                ls_inparamvalue[1] = checkguid;
                ls_inparamvalue[2] = inguid;
                ls_inparamvalue[3] = ls_TransportBoxGUID;

                ls_outparam = new string[2];
                ls_outparam[0] = "as_returncode";
                ls_outparam[1] = "as_returnmsg";
                ls_outparamtype = new string[2];
                ls_outparamtype[0] = "varchar";
                ls_outparamtype[1] = "varchar";
                ls_returnmsg = Doprocedure("miawms.sp_ofcheckbill2", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1004, true);
                if (ls_returnmsg != "TRUE")
                {
                    return ls_returnmsg;
                }
                CommitTrans();
                return "TRUE";
            }
            catch (Exception e)
            {
                RollbackTrans();
                return ("异常！" + e.Message.ToString());
            }
            finally
            {
                if (myReader != null)
                {
                    if (!myReader.IsClosed)
                        myReader.Close();
                    myReader.Dispose();
                }
                cmd = null;
            }
        }

        public string traybar(string traybar, string orderguid, string checkguid, string inguid, DataTable ddt, string flag)
        {
            string ls_TrayGUID;
            string ls_sql;
            string ls_wareguid;
            string ls_warepackguid;
            string ls_returnmsg;
            string ls_orgguid;
            string ls_curdata;
            string ls_prodata;
            string ls_expdata;
            string ls_savekind;
            string ls_savekindold;
            cmd = null;
            int l_num1, l_num2, l_num3;
            OracleDataReader myReader = null;
            DataTable dt = null;
            string[] ls_inparamtype, ls_inparamvalue, ls_outparam, ls_outparamtype, ls_inparam;
            string ls_xml;
            dt = new DataTable();
            dt = GetDataTable("select orgguid from Tb_Wms_BugInstoreOrder where orderguid  = '" + orderguid + "' union all select orgguid from Tb_Wms_SaleReturnInstoreOrder where orderguid = '" + orderguid + "' union all select orgguid from Tb_Wms_PeriodInstore where inguid = '" + inguid + "'");
            ls_orgguid = dt.Rows[0][0].ToString();
            ls_curdata = DateTime.Now.ToString("yyyyMMdd");
            try
            {
                Open();
                BeginTrans();
                if (ddt.Rows.Count == 1 && ddt.Rows[0]["packwarebar"].ToString() == "")//最后一个托盘商品件删除，单独处理
                {
                    ls_inparam = new string[4];
                    ls_inparam[0] = "as_orderguid";
                    ls_inparam[1] = "as_checkguid";
                    ls_inparam[2] = "as_inguid";
                    ls_inparam[3] = "as_boxbar";
                    ls_inparamvalue = new string[4];
                    ls_inparamvalue[0] = orderguid;
                    ls_inparamvalue[1] = checkguid;
                    ls_inparamvalue[2] = inguid;
                    ls_inparamvalue[3] = traybar;
                    ls_inparamtype = new string[4];
                    ls_inparamtype[0] = "varchar";
                    ls_inparamtype[1] = "varchar";
                    ls_inparamtype[2] = "varchar";
                    ls_inparamtype[3] = "varchar";
                    ls_outparam = new string[2];
                    ls_outparam[0] = "as_returncode";
                    ls_outparam[1] = "as_returnmsg";
                    ls_outparamtype = new string[2];
                    ls_outparamtype[0] = "varchar";
                    ls_outparamtype[1] = "varchar";
                    ls_returnmsg = Doprocedure("miawms.sp_get1058", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1058, true);
                    return ls_returnmsg;
                }
                cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("select TrayGUID from Tb_Wms_Tray where Bar = '" + traybar + "'", cn);
                if (inTransaction)
                {
                    cmd.Transaction = trans;
                }
                myReader = cmd.ExecuteReader();
                if (!myReader.HasRows)
                {
                    return ("托盘条码不存在，请检查！");
                }
                else
                {
                    myReader.Read();
                    ls_TrayGUID = myReader.GetString(0);

                    dt = new DataTable();
                    dt = GetDataTable("select PackWareGUID from Tb_Wms_TrayWarePack where inguid  = '" + inguid + "' and TRAYGUID = '" + ls_TrayGUID + "' and status = '0'");
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)//删除托盘商品件信息前，扣减数量
                        {
                            ls_warepackguid = dt.Rows[i][0].ToString();
                            ls_returnmsg = SqlDataTable("update Tb_Wms_BugInstoreCheckDetail set QUALPACKNUM = QUALPACKNUM - 1,NOQUALPACKNUM = NOQUALPACKNUM + 1 where checkguid = '" + checkguid + "' and  wareguid in (select wareguid from Tb_Wms_WarePack where WarePackGUID = '" + ls_warepackguid + "') and Batch in (select batchs from Tb_Wms_WarePack where WarePackGUID = '" + ls_warepackguid + "')");
                            if (ls_returnmsg != "TRUE")
                            {
                                return ls_returnmsg;
                            }
                            ls_returnmsg = SqlDataTable("update Tb_Wms_BugInstoreCheck set QualPackSum = QualPackSum - 1,NoqualPackSum = NoqualPackSum + 1 where checkguid = '" + checkguid + "'");
                            if (ls_returnmsg != "TRUE")
                            {
                                return ls_returnmsg;
                            }

                            ls_warepackguid = dt.Rows[i][0].ToString();
                            ls_returnmsg = SqlDataTable("update Tb_Wms_SaleReturnInstoreCheckD set QUALPACKNUM = QUALPACKNUM - 1,NOQUALPACKNUM = NOQUALPACKNUM + 1 where checkguid = '" + checkguid + "' and  wareguid in (select wareguid from Tb_Wms_WarePack where WarePackGUID = '" + ls_warepackguid + "') and Batch in (select batchs from Tb_Wms_WarePack where WarePackGUID = '" + ls_warepackguid + "')");
                            if (ls_returnmsg != "TRUE")
                            {
                                return ls_returnmsg;
                            }
                            ls_returnmsg = SqlDataTable("update Tb_Wms_SaleReturnInstoreCheck set QualPackSum = QualPackSum - 1,NoqualPackSum = NoqualPackSum + 1 where checkguid = '" + checkguid + "'");
                            if (ls_returnmsg != "TRUE")
                            {
                                return ls_returnmsg;
                            }

                            ls_returnmsg = SqlDataTable("update TB_WMS_BUGINSTOREDETAIL set PACKNUM = PACKNUM - 1 where inguid = '" + inguid + "' and  wareguid in (select wareguid from Tb_Wms_WarePack where WarePackGUID = '" + ls_warepackguid + "') and Batch in (select batchs from Tb_Wms_WarePack where WarePackGUID = '" + ls_warepackguid + "') ");
                            if (ls_returnmsg != "TRUE")
                            {
                                return ls_returnmsg;
                            }
                            ls_returnmsg = SqlDataTable("update Tb_Wms_SaleReturnInstoreDetail set PACKNUM = PACKNUM - 1 where inguid = '" + inguid + "' and  wareguid in (select wareguid from Tb_Wms_WarePack where WarePackGUID = '" + ls_warepackguid + "') and Batch in (select batchs from Tb_Wms_WarePack where WarePackGUID = '" + ls_warepackguid + "') ");
                            if (ls_returnmsg != "TRUE")
                            {
                                return ls_returnmsg;
                            }
                        }
                    }

                    ls_sql = "delete from Tb_Wms_TrayWarePack where inguid = '" + inguid + "' and TRAYGUID = '" + ls_TrayGUID + "'";
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }
                    cmd.ExecuteNonQuery();


                    ls_sql = "update tb_wms_tray set Status = '1' where trayguid = '" + ls_TrayGUID + "'";
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }
                    cmd.ExecuteNonQuery();
                    ls_savekindold = "";
                    for (int i = 0; i < ddt.Rows.Count; i++)
                    {
                        ls_expdata = ddt.Rows[i]["packwareexpdate"].ToString();
                        ls_prodata = ddt.Rows[i]["packwareproductdate"].ToString();

                        if (Convert.ToInt32(ls_prodata) > Convert.ToInt32(ls_curdata))
                        {
                            return ("生产日期不能大于当前日期！");
                        }
                        //System.TimeSpan ts = Convert.ToDateTime(DateTime.ParseExact(ls_expdata, "yyyyMMdd", null).ToString("yyyy-MM-dd")) - Convert.ToDateTime(DateTime.Now);
                        //if (ts.Days < 90)
                        //{
                        //    return ("有效期在90天内！");
                        //}

                        if (flag == "1")
                        {
                            cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("select wareguid from Tb_Wms_wmsware where Bar = '" + ddt.Rows[i]["warebar"].ToString() + "'", cn);
                            if (inTransaction)
                            {
                                cmd.Transaction = trans;
                            }
                            myReader = cmd.ExecuteReader();
                            if (!myReader.HasRows)
                            {
                                return ("商品条码不存在，请检查！");
                            }
                            else
                            {
                                myReader.Read();
                                ls_wareguid = myReader.GetString(0);
                                dt = new DataTable();
                                dt = GetDataTable("select savekindguid from tb_wms_wmsware where wareguid = '" + ls_wareguid + "'");
                                if (dt.Rows.Count < 1)
                                {
                                    return ("商品件存储类别未维护，请检查！");
                                }
                                ls_savekind = dt.Rows[0][0].ToString();
                                if (i == 0)
                                {
                                    ls_savekindold = ls_savekind;
                                }
                                if (ls_savekindold != ls_savekind)
                                {
                                    return ("托盘商品件存储类别不一致，请检查！");
                                }
                            }

                            dt = new DataTable();
                            dt = GetDataTable("select count(1) from tb_wms_traywarepack where InGUID = '" + inguid + "' and PackWareGUID in(select warepackguid from tb_wms_warepack where wareguid = '" + ls_wareguid + "' and status = '0')");
                            if (dt.Rows.Count == 0)
                            {
                                l_num1 = 0;
                            }
                            else
                            {
                                if (dt.Rows[0][0].ToString() == "")
                                {
                                    l_num1 = 0;
                                }
                                else
                                {
                                    l_num1 = Convert.ToInt32(dt.Rows[0][0].ToString());
                                }
                            }
                            dt = new DataTable();
                            dt = GetDataTable("select sum(nvl(packnum,0)) from (select packnum from tb_wms_buginstoreorderdetail where orderguid = '" + orderguid + "' and wareguid = '" + ls_wareguid + "' union all select packnum from tb_wms_salereturninstoreorderd where orderguid = '" + orderguid + "' and wareguid = '" + ls_wareguid + "' union all select packnum from tb_wms_periodinstoredetail where inguid = '" + orderguid + "' and wareguid = '" + ls_wareguid + "')");
                            if (dt.Rows.Count == 0)
                            {
                                l_num2 = 0;
                            }
                            else
                            {
                                if (dt.Rows[0][0].ToString() == "")
                                {
                                    l_num2 = 0;
                                }
                                else
                                {
                                    l_num2 = Convert.ToInt32(dt.Rows[0][0].ToString());
                                }
                            }
                            l_num3 = 1;
                            if (l_num1 + l_num3 > l_num2)
                            {
                                dt = new DataTable();
                                dt = GetDataTable("select warename from tb_wms_wmsware where  wareguid = '" + ls_wareguid + "'");
                                return "商品【" + dt.Rows[0][0].ToString() + "】验收件数不能大于订单件数【" + l_num2.ToString() + "】，请检查！";
                            }

                            cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("select wareguid from Tb_Wms_BugInstoreOrderDetail where WareGUID = '" + ls_wareguid + "' and orderguid = '" + orderguid + "' union all select wareguid from Tb_Wms_SaleReturnInstoreOrderD where WareGUID = '" + ls_wareguid + "' and orderguid = '" + orderguid + "' union all select WareGUID from Tb_Wms_PeriodInstoreDetail where inguid = '" + inguid + "' and wareguid = '" + ls_wareguid + "'", cn);
                            if (inTransaction)
                            {
                                cmd.Transaction = trans;
                            }
                            myReader = cmd.ExecuteReader();
                            if (!myReader.HasRows)
                            {
                                return ("该验收单不存在此商品件，请检查！");
                            }

                            ls_sql = "delete from TB_WMS_WAREPACK where ORDERGUID = '" + orderguid + "' and BAR = '" + ddt.Rows[i]["packwarebar"].ToString() + "'";
                            cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                            if (inTransaction)
                            {
                                cmd.Transaction = trans;
                            }
                            cmd.ExecuteNonQuery();

                            ls_sql = @"insert into TB_WMS_WAREPACK(
                           WAREPACKGUID,
                           ORDERGUID,
                           WAREGUID,
                           BAR,
                           SINGLENUM,
                           BATCHS,
                           EXPDATE,
                           PRODUCTDATE,
                           status)
                           values
                           (
                            createguid(),
                            '" + orderguid + @"',
                            '" + ls_wareguid + @"',
                            '" + ddt.Rows[i]["packwarebar"].ToString() + @"',
                           (select PackNum from Tb_Wms_WmsWare where wareguid = '" + ls_wareguid + @"'),
                           '" + ddt.Rows[i]["packwarebatch"].ToString() + @"',
                           to_date('" + ddt.Rows[i]["packwareexpdate"].ToString() + @"','yyyymmdd'),
                           to_date('" + ddt.Rows[i]["packwareproductdate"].ToString() + @"','yyyymmdd'),
                           '0'
                           )";
                            cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                            if (inTransaction)
                            {
                                cmd.Transaction = trans;
                            }
                            cmd.ExecuteNonQuery();
                        }


                        //商品件条码是否存在
                        ls_sql = "select bar from Tb_Wms_WarePack where Bar =  '" + ddt.Rows[i]["packwarebar"].ToString() + "' and status = '0'";
                        cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                        if (inTransaction)
                        {
                            cmd.Transaction = trans;
                        }
                        myReader = cmd.ExecuteReader();
                        if (!myReader.HasRows)//不存在记录
                        {
                            RollbackTrans();
                            return "商品件条码【" + ddt.Rows[i]["packwarebar"].ToString() + "】不存在，请核查！";
                        }
                        //生成托盘商品件信息
                        ls_sql = @"insert into Tb_Wms_TrayWarePack( 
                               TRAYPACKWAREGUID,
                               TRAYGUID,
                               PACKWAREGUID,
                               TRAYDATE,
                               OPERUSER,
                                inguid,
                                status,
                                orderbyno)
                               values
                               (
                                createguid(),
                                '" + ls_TrayGUID + @"',
                                (select distinct WarePackGUID from Tb_Wms_WarePack where bar = '" + ddt.Rows[i]["packwarebar"].ToString() + @"' and orderguid = '" + orderguid + @"'), 
                                 sysdate,
                                 '" + OperUserID + @"',
                                '" + inguid + @"',
                                '0',
                                (select case when (select max(orderbyno) from Tb_Wms_TrayWarePack where trayguid = '" + ls_TrayGUID + @"' and 
inguid =  '" + inguid + @"') is null then 1 else (select max(nvl(orderbyno,0)) + 1 from Tb_Wms_TrayWarePack where trayguid = '" + ls_TrayGUID + @"' and 
inguid =  '" + inguid + @"') end from dual)             
                               )";
                        cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                        if (inTransaction)
                        {
                            cmd.Transaction = trans;
                        }
                        cmd.ExecuteNonQuery();

                        if (flag == "1")
                        {
                            ls_sql = @"update Tb_Wms_WarePack
                                   set Batchs = '" + ddt.Rows[i]["packwarebatch"].ToString() + @"',
                                       ExpDate = to_date('" + ddt.Rows[i]["packwareexpdate"].ToString() + @"','yyyymmdd'),
                                       productDate = to_date('" + ddt.Rows[i]["packwareproductdate"].ToString() + @"','yyyymmdd')
                                 where Bar = '" + ddt.Rows[i]["packwarebar"].ToString() + @"'
                                       and OrderGUID = '" + orderguid + "'";
                            cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                            if (inTransaction)
                            {
                                cmd.Transaction = trans;
                            }
                            cmd.ExecuteNonQuery();

                            //批号效期处理
                            ls_sql = @"select to_char(ExpDate,'yyyymmdd'),RelationGUID from Tb_Wms_WareBatchExp where wareguid in (select wareguid from Tb_Wms_WarePack where Bar = '" + ddt.Rows[i]["packwarebar"].ToString() + @"'  and status = '0') and batchs = '" + ddt.Rows[i]["packwarebatch"].ToString() + @"' and orgguid = '" + ls_orgguid + "'";
                            cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                            if (inTransaction)
                            {
                                cmd.Transaction = trans;
                            }
                            myReader = cmd.ExecuteReader();
                            if (myReader.HasRows)//存在记录
                            {
                                myReader.Read();
                                if (myReader.GetString(0) != ddt.Rows[i]["packwareexpdate"].ToString())
                                {
                                    //RollbackTrans();
                                    //return "存在同一批号商品，但效期与此次不同，请核查！";
                                    ls_sql = @"update Tb_Wms_WareBatchExp set 
                                            ExpDate = to_date('" + ddt.Rows[i]["packwareexpdate"].ToString() + @"','yyyymmdd') ,
                                            productDate = to_date('" + ddt.Rows[i]["packwareproductdate"].ToString() + @"','yyyymmdd') 
                                            where RelationGUID = '" + myReader.GetString(1) + "'";
                                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                                    if (inTransaction)
                                    {
                                        cmd.Transaction = trans;
                                    }
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                ls_sql = @"insert into Tb_Wms_WareBatchExp( 
                               RelationGUID,
                               WareGUID,
                               Batchs,
                               ExpDate,
                                productdate,
                                orgguid)
                               values
                               (
                                createguid(),
                                (select wareguid from Tb_Wms_WarePack where Bar = '" + ddt.Rows[i]["packwarebar"].ToString() + @"' and orderguid = '" + orderguid + @"'),
                                '" + ddt.Rows[i]["packwarebatch"].ToString() + @"', 
                                 to_date('" + ddt.Rows[i]["packwareexpdate"].ToString() + @"','yyyymmdd'),
                                 to_date('" + ddt.Rows[i]["packwareproductdate"].ToString() + @"','yyyymmdd'),
                                '" + ls_orgguid + @"'
                               )";
                                cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                                if (inTransaction)
                                {
                                    cmd.Transaction = trans;
                                }
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else//使用贴标机，PDA不输入批号效期等
                        {

                        }
                    }


                    ls_inparam = new string[4];
                    ls_inparam[0] = "as_orderguid";
                    ls_inparam[1] = "as_checkguid";
                    ls_inparam[2] = "as_inguid";
                    ls_inparam[3] = "as_guid";

                    ls_inparamtype = new string[4];
                    ls_inparamtype[0] = "varchar";
                    ls_inparamtype[1] = "varchar";
                    ls_inparamtype[2] = "varchar";
                    ls_inparamtype[3] = "varchar";
                    ls_inparamvalue = new string[4];
                    ls_inparamvalue[0] = orderguid;
                    ls_inparamvalue[1] = checkguid;
                    ls_inparamvalue[2] = inguid;
                    ls_inparamvalue[3] = ls_TrayGUID;

                    ls_outparam = new string[2];
                    ls_outparam[0] = "as_returncode";
                    ls_outparam[1] = "as_returnmsg";
                    ls_outparamtype = new string[2];
                    ls_outparamtype[0] = "varchar";
                    ls_outparamtype[1] = "varchar";
                    ls_returnmsg = Doprocedure("miawms.sp_ofcheckbill2", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1005, true);
                    if (ls_returnmsg != "TRUE")
                    {
                        return ls_returnmsg;
                    }
                }
                CommitTrans();
                return "TRUE";
            }
            catch (Exception e)
            {
                RollbackTrans();
                return ("异常！" + e.Message.ToString());
            }
            finally
            {
                if (myReader != null)
                {
                    if (!myReader.IsClosed)
                        myReader.Close();
                    myReader.Dispose();
                }
                cmd = null;
            }
        }

        public string boxwork(string boxbar, string pickguid)
        {
            string ls_sql;
            cmd = null;
            OracleDataReader myReader = null;
            try
            {
                Open();
                BeginTrans();
                cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("select TransportBoxGUID from Tb_Wms_TransportBox where Bar = '" + boxbar + "'", cn);
                if (inTransaction)
                {
                    cmd.Transaction = trans;
                }
                myReader = cmd.ExecuteReader();
                if (!myReader.HasRows)
                {
                    return ("周转箱条码不存在，请检查！");
                }
                else
                {
                    myReader.Read();
                    ls_sql = @"insert into Tb_Wms_PickBox( 
                               PICKBOXGUID,
                               PICKGUID,
                               TRANSPORTBOXGUID,
                               WAREGUID,
                               UNITS,
                               WARENUM,
                               STATES,
                               WARENAME,
                               SPECS,
                               FACTAREA,
                               BATCH,
                               EXPDATE)
                               values
                               (
                                createguid(),
                                '" + pickguid + @"',
                                '" + myReader.GetString(0) + @"', 
                                null,
                                null,
                                null,
                                '0',
                                null,
                                null,
                                null,
                                null,
                                null                               
                               )";
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }
                    cmd.ExecuteNonQuery();

                    ls_sql = @"update Tb_Wms_TransportBox
                                   set Status = '1'
                                 where Bar = '" + boxbar + "'";
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }
                    cmd.ExecuteNonQuery();
                }
                CommitTrans();
                return "TRUE";
            }
            catch (Exception e)
            {
                RollbackTrans();
                return ("异常！" + e.Message.ToString());
            }
            finally
            {
                if (myReader != null)
                {
                    if (!myReader.IsClosed)
                        myReader.Close();
                    myReader.Dispose();
                }
                cmd = null;
            }
        }

        public string warepackwork(string warepackbar, string pickguid, string ls_locationguid)
        {
            string ls_sql;
            cmd = null;
            string[] ls_inparamtype, ls_inparamvalue, ls_outparam, ls_outparamtype, ls_inparam;
            string ls_xml;
            DataTable dt = null;
            DataTable dt2 = null;
            OracleDataReader myReader = null;
            string ls_returnmsg;
            int l_packnum = 0;
            int l_realnum = 0;
            try
            {
                Open();
                BeginTrans();
                cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("select WarePackGUID from Tb_Wms_WarePack where Bar = '" + warepackbar + "' and status = '0'", cn);
                if (inTransaction)
                {
                    cmd.Transaction = trans;
                }
                myReader = cmd.ExecuteReader();
                if (!myReader.HasRows)
                {
                    return ("商品件条码不存在，请检查！");
                }
                else
                {
                    myReader.Read();
                    ls_sql = @"insert into Tb_Wms_PickWarePackDetail( 
                                           PACKDETAILGUID,
                                           DETAILGUID,
                                           PICKGUID,
                                           ORGGUID,
                                           STOREAREAGUID,
                                           CONTROLGUID,
                                           NATUREGUID,
                                           SHELVEGUID,
                                           LABELGUID,
                                           LOCATIONGUID,
                                           WAREGUID,
                                           BATCHS,
                                           WAREPACKBAR,
                                           REMARK,
                                           status)
                                           values
                                           (
                                            createguid(),
                                            (select detailguid from tb_wms_pickdetail where pickguid = '" + pickguid + @"' and locationguid = '" + ls_locationguid + @"'
                                            and wareguid in(select wareguid from tb_wms_warepack where bar = '" + warepackbar + @"' and status = '0') and batchs = (select batchs from tb_wms_warepack where bar = '" + warepackbar + @"' and status = '0')),
                                            '" + pickguid + @"',
                                            null, 
                                            null,
                                            null,
                                            null,
                                            null,
                                            null,
                                '" + ls_locationguid + @"',
                                (select wareguid from tb_wms_warepack where bar = '" + warepackbar + @"' and status = '0'),
                                (select batchs from tb_wms_warepack where bar = '" + warepackbar + @"' and status = '0'),
                                '" + warepackbar + @"', 
                                null,
                                '0'                             
                               )";
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }
                    cmd.ExecuteNonQuery();
                    dt = new DataTable();
                    dt = GetDataTable("select sum(nvl(PackNum,0)) from Tb_Wms_PickDetail where PickGUID = '" + pickguid + "' and locationguid = '" + ls_locationguid + "'");
                    l_packnum = Convert.ToInt32(dt.Rows[0][0].ToString());
                    dt2 = new DataTable();
                    dt2 = GetDataTable("select count(1) from Tb_Wms_PickWarePackDetail where PickGUID = '" + pickguid + "' and locationguid = '" + ls_locationguid + "'");
                    l_realnum = Convert.ToInt32(dt2.Rows[0][0].ToString());
                    if (l_packnum == l_realnum)//托盘应拣件数=实际拣货件数，则进行原来的托盘拣货完成操作
                    {
                        ls_inparam = new string[4];
                        ls_inparam[0] = "as_pickguid";
                        ls_inparam[1] = "as_locationguid";
                        ls_inparam[2] = "as_operuser";
                        ls_inparam[3] = "as_picknum";
                        ls_inparamtype = new string[4];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_inparamtype[3] = "int";
                        ls_inparamvalue = new string[4];
                        ls_inparamvalue[0] = pickguid;
                        ls_inparamvalue[1] = ls_locationguid;
                        ls_inparamvalue[2] = OperUserID;
                        ls_inparamvalue[3] = l_packnum.ToString();

                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";

                        ls_returnmsg = Doprocedure("miawms.sp_updatepickinfo", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1031, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            RollbackTrans();
                            return ls_returnmsg;
                        }
                        else
                        {
                            dt = new DataTable();
                            dt = GetDataTable("select sum(nvl(PackNum,0)) from Tb_Wms_PickDetail where PickGUID = '" + pickguid + "'");
                            l_packnum = Convert.ToInt32(dt.Rows[0][0].ToString());
                            dt2 = new DataTable();
                            dt2 = GetDataTable("select count(1) from Tb_Wms_PickWarePackDetail where PickGUID = '" + pickguid + "'");
                            l_realnum = Convert.ToInt32(dt2.Rows[0][0].ToString());
                            if (l_packnum == l_realnum)//任务应拣件数=实际拣货件数，则进行原来的任务拣货完成操作
                            {
                                ls_returnmsg = SqlDataTable("update Tb_Wms_Pick set Status = '3' where pickguid = '" + pickguid + "'");
                                if (ls_returnmsg != "TRUE")
                                {
                                    RollbackTrans();
                                    return ls_returnmsg;
                                }
                                ls_returnmsg = SqlDataTable("update Tb_Wms_Pickconfirm set Status = '1' where pickguid = '" + pickguid + "' and status <> '8'");
                                if (ls_returnmsg != "TRUE")
                                {
                                    RollbackTrans();
                                    return ls_returnmsg;
                                }
                            }
                        }
                    }
                }
                CommitTrans();
                return "TRUE";
            }
            catch (Exception e)
            {
                RollbackTrans();
                return ("异常！" + e.Message.ToString());
            }
            finally
            {
                if (myReader != null)
                {
                    if (!myReader.IsClosed)
                        myReader.Close();
                    myReader.Dispose();
                }
                cmd = null;
            }
        }

        public string deliverbox(string confirmguid, string boxbar, DataTable ddt)
        {
            string ls_sql;
            string ls_LOGISTICBOXGUID;
            cmd = null;
            OracleDataReader myReader = null;
            try
            {
                Open();
                BeginTrans();
                cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("select LogisticBoxGUID from Tb_Wms_LogisticBox where Bar = '" + boxbar + "'", cn);
                if (inTransaction)
                {
                    cmd.Transaction = trans;
                }
                myReader = cmd.ExecuteReader();
                if (!myReader.HasRows)
                {
                    return ("配送箱条码不存在，请检查！");
                }
                else
                {
                    myReader.Read();
                    ls_LOGISTICBOXGUID = myReader[0].ToString();
                    for (int i = 0; i < ddt.Rows.Count; i++)
                    {
                        cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("select wareguid from tb_wms_wmsware where bar = '" + ddt.Rows[i]["warebar"].ToString() + "'", cn);
                        if (inTransaction)
                        {
                            cmd.Transaction = trans;
                        }
                        myReader = cmd.ExecuteReader();
                        if (!myReader.HasRows)
                        {
                            return ("商品条码不存在，请检查！");
                        }

                        ls_sql = @"insert into Tb_Wms_DeliverBox( 
                               DELIVERBOXGUID,
                               pickguid,
                               LOGISTICBOXGUID,
                               WAREGUID,
                               warenum)
                               values
                               (
                                createguid(),
                               (select pickguid from Tb_Wms_CheckConfirm where ConfirmGUID = '" + confirmguid + @"'),
                                '" + ls_LOGISTICBOXGUID + @"',                               
                                '" + myReader[0].ToString() + @"',
                                " + ddt.Rows[i]["warenum"].ToString() + @"          
                               )";
                        cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                        if (inTransaction)
                        {
                            cmd.Transaction = trans;
                        }
                        cmd.ExecuteNonQuery();
                    }
                }
                CommitTrans();
                return "TRUE";
            }
            catch (Exception e)
            {
                RollbackTrans();
                return ("异常！" + e.Message.ToString());
            }
            finally
            {
                if (myReader != null)
                {
                    if (!myReader.IsClosed)
                        myReader.Close();
                    myReader.Dispose();
                }
                cmd = null;
            }
        }

        public string cancelwarepackwork(string warepackbar, string pickguid, string locationguid)
        {
            string ls_sql;
            cmd = null;
            OracleDataReader myReader = null;
            try
            {
                Open();
                BeginTrans();
                cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("select WarePackGUID from Tb_Wms_WarePack where Bar = '" + warepackbar + "' and status = '0'", cn);
                if (inTransaction)
                {
                    cmd.Transaction = trans;
                }
                myReader = cmd.ExecuteReader();
                if (!myReader.HasRows)
                {
                    return ("商品件条码不存在，请检查！");
                }
                else
                {
                    myReader.Read();
                    ls_sql = @"delete Tb_Wms_PickWarePackDetail where pickguid = '" + pickguid + "' and WAREPACKBAR = '" + warepackbar + "'";
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }
                    cmd.ExecuteNonQuery();

                    //                    ls_sql = @"update TB_WMS_PICKCONFIRM set PICKPACKSUM = PICKPACKSUM - 1 where PickGUID = '" + pickguid + "'";
                    //                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                    //                    if (inTransaction)
                    //                    {
                    //                        cmd.Transaction = trans;
                    //                    }
                    //                    cmd.ExecuteNonQuery();

                    //                    ls_sql = @"update Tb_Wms_PickDetail
                    //                               set ConfirmSingleNum = 0,
                    //                                   ConfirmPackNum   = 0,
                    //                                   ConfirmFlag      = getconfirmstatus('未确认')
                    //                             where PickGUID = '" + pickguid + @"'
                    //                               and locationguid = '"+locationguid+"'";
                    //                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                    //                    if (inTransaction)
                    //                    {
                    //                        cmd.Transaction = trans;
                    //                    }
                    //                    cmd.ExecuteNonQuery();

                    //                    ls_sql = @"update Tb_Wms_LabelpickDetail t
                    //                                   set comfirmnum  = 0,
                    //                                       confirmflag = GetConfirmstatus('未确认'),
                    //                                       confirmuser = null,
                    //                                       confirmtime = null
                    //                                 where LocationGUID = '"+locationguid+@"'
                    //                                   and LabelpickGUID in
                    //                                       (select LabelpickGUID
                    //                                          from tb_wms_labelpick
                    //                                         where pickguid = '" + pickguid + @"')";
                    //                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                    //                    if (inTransaction)
                    //                    {
                    //                        cmd.Transaction = trans;
                    //                    }
                    //                    cmd.ExecuteNonQuery();

                }
                CommitTrans();
                return "TRUE";
            }
            catch (Exception e)
            {
                RollbackTrans();
                return ("异常！" + e.Message.ToString());
            }
            finally
            {
                if (myReader != null)
                {
                    if (!myReader.IsClosed)
                        myReader.Close();
                    myReader.Dispose();
                }
                cmd = null;
            }
        }

        public string cancelboxwork(string boxbar, string pickguid)
        {
            string ls_sql;
            cmd = null;
            OracleDataReader myReader = null;
            try
            {
                Open();
                BeginTrans();
                cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("select TransportBoxGUID from Tb_Wms_TransportBox where Bar = '" + boxbar + "'", cn);
                if (inTransaction)
                {
                    cmd.Transaction = trans;
                }
                myReader = cmd.ExecuteReader();
                if (!myReader.HasRows)
                {
                    return ("周转箱条码不存在，请检查！");
                }
                else
                {
                    myReader.Read();
                    ls_sql = "delete from Tb_Wms_PickBox where pickguid = '" + pickguid + "' and TRANSPORTBOXGUID = '" + myReader.GetString(0) + "'";
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }
                    cmd.ExecuteNonQuery();

                    ls_sql = @"update Tb_Wms_TransportBox
                                   set Status = '0'
                                 where Bar = '" + boxbar + "'";
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }
                    cmd.ExecuteNonQuery();
                }
                CommitTrans();
                return "TRUE";
            }
            catch (Exception e)
            {
                RollbackTrans();
                return ("异常！" + e.Message.ToString());
            }
            finally
            {
                if (myReader != null)
                {
                    if (!myReader.IsClosed)
                        myReader.Close();
                    myReader.Dispose();
                }
                cmd = null;
            }
        }

        public string checkware(string confirmguid, DataTable ddt)
        {
            string ls_sql;
            cmd = null;
            OracleDataReader myReader = null;
            try
            {
                Open();
                BeginTrans();

                for (int i = 0; i < ddt.Rows.Count; i++)
                {
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("select wareguid from tb_wms_wmsware where bar = '" + ddt.Rows[i]["warebar"].ToString() + "'", cn);
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }
                    myReader = cmd.ExecuteReader();
                    if (!myReader.HasRows)
                    {
                        return ("商品条码不存在，请检查！");
                    }

                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("select TEMPGUID from Tb_Wms_TempCheck where CONFIRMGUID = '" + confirmguid + "' and WAREBAR = '" + ddt.Rows[i]["warebar"].ToString() + "' ", cn);
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }
                    myReader = cmd.ExecuteReader();
                    if (!myReader.HasRows)
                    {
                        return ("该商品条码已扫描，不能重复扫描！");
                    }

                    ls_sql = @"insert into Tb_Wms_TempCheck( 
                               TEMPGUID,
                               CONFIRMGUID,
                               TYPES,
                               WAREBAR,
                               WAREPACKBAR,
                               WARENUM,
                               STATES)
                               values
                               (
                                createguid(),
                                '" + confirmguid + @"',
                                '0',                                
                                 '" + ddt.Rows[i]["warebar"].ToString() + @"',
                                 null,
                                 '" + ddt.Rows[i]["warenum"].ToString() + @"',
                                '0'
                               )";
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }
                    cmd.ExecuteNonQuery();
                }

                CommitTrans();
                return "TRUE";
            }
            catch (Exception e)
            {
                RollbackTrans();
                return ("异常！" + e.Message.ToString());
            }
            finally
            {
                if (myReader != null)
                {
                    if (!myReader.IsClosed)
                        myReader.Close();
                    myReader.Dispose();
                }
                cmd = null;
            }
        }

        public string checkwarepack(string confirmguid, DataTable ddt)
        {
            string ls_sql;
            cmd = null;
            string ls_pickguid;
            DataTable dt = null;
            OracleDataReader myReader = null;
            try
            {
                Open();
                BeginTrans();
                dt = new DataTable();
                dt = GetDataTable("select PickGUID from Tb_Wms_PickConfirm where ConfirmGUID  = '" + confirmguid + "'");
                ls_pickguid = dt.Rows[0][0].ToString();
                for (int i = 0; i < ddt.Rows.Count; i++)
                {
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("select wareguid from tb_wms_warepack where bar = '" + ddt.Rows[i]["warebar"].ToString() + "' and status = '0' and bar in(select WarePackBar from Tb_Wms_PickWarePackDetail where PickGUID = '" + ls_pickguid + "')", cn);
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }
                    myReader = cmd.ExecuteReader();
                    if (!myReader.HasRows)
                    {
                        return ("商品件条码不存在，请检查！");
                    }



                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("select TEMPGUID from Tb_Wms_TempCheck where CONFIRMGUID = '" + confirmguid + "' and WAREPACKBAR = '" + ddt.Rows[i]["warebar"].ToString() + "' ", cn);
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }
                    myReader = cmd.ExecuteReader();
                    if (myReader.HasRows)
                    {
                        return ("该商品件条码已扫描，不能重复扫描！");
                    }


                    ls_sql = @"insert into Tb_Wms_TempCheck( 
                               TEMPGUID,
                               CONFIRMGUID,
                               TYPES,
                               WAREBAR,
                               WAREPACKBAR,
                               WARENUM,
                               STATES)
                               values
                               (
                                createguid(),
                                '" + confirmguid + @"',
                                '1', 
                                 null,                               
                                 '" + ddt.Rows[i]["warebar"].ToString() + @"',
                                 null,
                                '0'
                               )";
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }
                    cmd.ExecuteNonQuery();
                }

                CommitTrans();
                return "TRUE";
            }
            catch (Exception e)
            {
                RollbackTrans();
                return ("异常！" + e.Message.ToString());
            }
            finally
            {
                if (myReader != null)
                {
                    if (!myReader.IsClosed)
                        myReader.Close();
                    myReader.Dispose();
                }
                cmd = null;
            }
        }

        public string companywork(string inguid)
        {
            string ls_sql;
            cmd = null;
            OracleDataReader myReader = null;
            try
            {
                Open();
                BeginTrans();
                cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("select type from(select '1' as type from tb_wms_periodinstore where inguid = '" + inguid + "')", cn);
                if (inTransaction)
                {
                    cmd.Transaction = trans;
                }
                myReader = cmd.ExecuteReader();
                if (!myReader.HasRows)
                {
                    return ("入库单号不存在，请检查！");
                }
                else
                {
                    myReader.Read();
                    if (myReader.GetString(0) == "1")
                    {
                        ls_sql = @"Update Tb_Wms_PeriodInstore
		                            Set status = '1'
		                            Where inguid = '" + inguid + "'";
                        cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                    }
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }
                    cmd.ExecuteNonQuery();

                }
                CommitTrans();
                return "TRUE";
            }
            catch (Exception e)
            {
                RollbackTrans();
                return ("异常！" + e.Message.ToString());
            }
            finally
            {
                if (myReader != null)
                {
                    if (!myReader.IsClosed)
                        myReader.Close();
                    myReader.Dispose();
                }
                cmd = null;
            }
        }

        public string companywork2(string inguid)
        {
            string ls_sql;
            cmd = null;
            OracleDataReader myReader = null;
            try
            {
                Open();
                BeginTrans();
                cmd = new Oracle.ManagedDataAccess.Client.OracleCommand("select type from(select '1' as type from tb_wms_moveinstore where inguid = '" + inguid + "')", cn);
                if (inTransaction)
                {
                    cmd.Transaction = trans;
                }
                myReader = cmd.ExecuteReader();
                if (!myReader.HasRows)
                {
                    return ("入库单号不存在，请检查！");
                }
                else
                {
                    myReader.Read();
                    if (myReader.GetString(0) == "1")
                    {
                        ls_sql = @"Update Tb_Wms_moveInstore
		                            Set status = '1'
		                            Where inguid = '" + inguid + "'";
                        cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
                    }
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }
                    cmd.ExecuteNonQuery();
                }
                CommitTrans();
                return "TRUE";
            }
            catch (Exception e)
            {
                RollbackTrans();
                return ("异常！" + e.Message.ToString());
            }
            finally
            {
                if (myReader != null)
                {
                    if (!myReader.IsClosed)
                        myReader.Close();
                    myReader.Dispose();
                }
                cmd = null;
            }
        }
        #endregion

        #region 封装函数
        //连接服务器
        public void ConnServer()
        {
            try
            {
                _client = new TcpClient(address, host);
            }
            //处理参数为空引用异常 
            catch (ArgumentNullException ae)
            {
                Console.WriteLine("ArgumentNullException : {0}", ae.Message.ToString());
                throw new ArgumentNullException("参数异常" + ae.Message.ToString());
            }
            //处理操作系统异常 
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.Message.ToString());
                throw new Exception("连接异常：" + se.Message.ToString());
            }
            catch (Exception ew)
            {
                Console.WriteLine("Unexpected exception : {0}", ew.Message.ToString());
                throw new Exception("其它异常：" + ew.Message.ToString());
            }
        }

        public byte[] StringToByte(string InString)
        {
            string[] ByteStrings;
            ByteStrings = InString.Split(" ".ToCharArray());
            byte[] ByteOut;
            ByteOut = new byte[ByteStrings.Length - 1];
            for (int i = 0; i == ByteStrings.Length - 1; i++)
            {
                ByteOut[i] = Convert.ToByte(("0x" + ByteStrings[i]));
            }
            return ByteOut;
        }

        public byte[] strToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        public string ToHexString(byte[] bytes)
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString("X2"));
                }
                hexString = strB.ToString();
            }
            return hexString;
        }

        #region crc
        private static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        public long GetModBusCRC(string DATA)
        {
            long functionReturnValue = 0;

            long i = 0;
            long J = 0;
            byte[] v = null;
            v = strToToHexByte(DATA);

            //1.预置1个16位的寄存器为十六进制FFFF（即全为1）：称此寄存器为CRC寄存器；
            long CRC = 0;
            CRC = 0xffffL;
            for (i = 0; i <= (v).Length - 1; i++)
            {
                //2.把第一个8位二进制数据（既通讯信息帧的第一个字节）与16位的CRC寄存器的低8位相异或，把结果放于CRC寄存器；
                CRC = (CRC / 256) * 256L + (CRC % 256L) ^ v[i];
                for (J = 0; J <= 7; J++)
                {
                    //3.把CRC寄存器的内容右移一位（朝低位）用0填补最高位，并检查最低位；
                    //4.如果最低位为0：重复第3步（再次右移一位）；
                    // 如果最低位为1：CRC寄存器与多项式A001（1010 0000 0000 0001）进行异或；
                    //5.重复步骤3和4，直到右移8次，这样整个8位数据全部进行了处理；
                    long d0 = 0;
                    d0 = CRC & 1L;
                    CRC = CRC / 2;
                    if (d0 == 1)
                        CRC = CRC ^ 0xa001L;

                }

                //6.重复步骤2到步骤5，进行通讯信息帧下一字节的处理；
            }

            //7.最后得到的CRC寄存器内容即为：CRC码。
            CRC = CRC % 65536;
            functionReturnValue = CRC;

            return functionReturnValue;
        }
        #endregion

        public string ChangeOrder(string head, string businessid, string lableaddress, string waresum, string ls_string)
        {
            string ls_order = "";
            ls_order = head + businessid + lableaddress + waresum + ls_string;
            //CRC计算开始
            long lon = GetModBusCRC(ls_order);
            long h1, l0;
            h1 = lon % 256;
            l0 = lon / 256;

            string s = "";
            if (Convert.ToString(h1, 16).Length < 2)
            {
                s = "0" + Convert.ToString(h1, 16);
            }
            else
            {
                s = Convert.ToString(h1, 16);
            }

            if (Convert.ToString(l0, 16).Length < 2)
            {
                s = s + "0" + Convert.ToString(l0, 16);
            }
            else
            {
                s = s + Convert.ToString(l0, 16);
            }
            //CRC结束
            ls_order += s.ToUpper();
            ls_order = ChangeCrc(ls_order);
            return ls_order;
        }

        public string ChangeCrc(string ls_order)
        {
            string ls_neworder = "";
            for (int i = 0; i < ls_order.Length / 2; i++)
            {
                if (ls_order.Substring(i * 2, 2) == "7D")
                {
                    ls_neworder += "7D5D";
                }
                else if (ls_order.Substring(i * 2, 2) == "7E")
                {
                    ls_neworder += "7D5E";
                }
                else
                {
                    ls_neworder += ls_order.Substring(i * 2, 2);
                }
            }
            return ls_neworder;
        }

        public string ChangeCrc2(string ls_order)
        {
            string ls_neworder = "";
            //if (ls_order.Length <= 6)
            //{
            //    ls_neworder = ls_order;
            //    return ls_neworder;
            //}
            ////ls_order = ls_order.Substring(4, ls_order.Length - 6);
            ls_order = ls_order.Replace("7D", "7D5D");
            ls_order = ls_order.Replace("7E", "7D5E");
            ls_neworder = ls_order;
            return ls_neworder;
        }

        public string SendFile(string message)
        {
            TcpClient client = new TcpClient(address, host);
            NetworkStream stream = client.GetStream();
            try
            {
                //1.发送数据   
                //byte[] messages = strToHexByte(message);
                byte[] messages = Encoding.ASCII.GetBytes(message);//ASC发送
                stream.WriteTimeout = timers;//发送时间
                stream.Write(messages, 0, messages.Length);
                //2.接收状态,长度<1024字节
                byte[] bytes = new Byte[1024];
                string data = string.Empty;
                stream.ReadTimeout = timers;//接收返回信息
                int length = stream.Read(bytes, 0, bytes.Length);
                if (length > 0)
                {
                    data = ToHexString(bytes);//十六进制接收
                    if (data.Substring(0, 8) == "53535353" || data.Substring(0, 8) == "35353535")//字符串接收
                    {
                        data = System.Text.Encoding.UTF8.GetString(bytes, 0, length);
                        data = data.Substring(8, data.Length - 8);
                        return data;
                    }
                }
                //3.关闭对象
                return data;
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
            finally
            {
                stream.Close();
                client.Close();
            }
        }

        public string bYtesToString(byte[] bytes)
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString());
                }
                hexString = strB.ToString();
            }
            return hexString;
        }

        public string TransAction(string str)
        {
            string ls_msg;

            try
            {
                ls_msg = str;
                return SendFile(ls_msg);//发送文件
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
            finally
            {
                System.GC.Collect();
            }
        }

        #region 公用函数
        public void WriteLog(string ls_operuser, DateTime ls_opertime, string ls_text, string ls_text2, string ls_text3, string ls_text4, string ls_readtext, string ls_sendtext, string funcid, string ls_status)
        {
            string ls_sql;
            Open();
            BeginTrans();
            if (ls_text == String.Empty || ls_text == null)
            {
                ls_text = " ";
            }
            if (ls_text2 == String.Empty || ls_text2 == null)
            {
                ls_text2 = " ";
            }
            if (ls_text3 == String.Empty || ls_text3 == null)
            {
                ls_text3 = " ";
            }
            if (ls_text4 == String.Empty || ls_text4 == null)
            {
                ls_text4 = " ";
            }
            if (ls_readtext == String.Empty || ls_readtext == null)
            {
                ls_readtext = " ";
            }
            if (ls_sendtext == String.Empty || ls_sendtext == null)
            {
                ls_sendtext = " ";
            }
            ls_text = ls_text.Replace("'", "''");
            ls_text2 = ls_text2.Replace("'", "''");
            ls_text3 = ls_text3.Replace("'", "''");
            ls_text4 = ls_text4.Replace("'", "''");
            ls_readtext = ls_readtext.Replace("'", "''");
            ls_sendtext = ls_sendtext.Replace("'", "''");


            ls_sql = @"insert into Tb_Wms_webserviceInterfaceLog (LOGGUID,
                                           LOGID,
                                           LOGTEXT,
                                           LOGTEXTD,
                                           LOGTEXT2,
                                           LOGTEXT3,
                                           OPERDATE,
                                           OPERUSER,
                                           REMARK,
                                           SENDTO,
                                           READTO,
                                            FuncID,
                                            status)
                                           values
                                           (
                                           createguid(),
                                           (select nvl(max(logid),0) + 1 from Tb_Wms_webserviceInterfaceLog),
                                           :exper,               
                                           :exper3,                
                                           :exper2,              
                                           :exper4,              
                                           sysdate,               
                                           '" + ls_operuser + @"',               
                                           null,               
                                           '" + ls_sendtext + @"',               
                                           '" + ls_readtext + @"',              
                                           '" + funcid + @"',
                                           '" + ls_status + @"'                                           
                                           )";
            //ls_returnmsg = SqlDataTable(ls_sql);
            cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(ls_sql, cn);
            OracleParameter op = new OracleParameter("exper", OracleDbType.Clob);
            op.Value = ls_text;
            OracleParameter op3 = new OracleParameter("exper3", OracleDbType.Clob);
            op3.Value = ls_text3;
            OracleParameter op2 = new OracleParameter("exper2", OracleDbType.Clob);
            op2.Value = ls_text2;
            OracleParameter op4 = new OracleParameter("exper4", OracleDbType.Clob);
            op4.Value = ls_text4;
            cmd.Parameters.Add(op);
            cmd.Parameters.Add(op3);
            cmd.Parameters.Add(op2);
            cmd.Parameters.Add(op4);
            cmd.ExecuteNonQuery();
            CommitTrans();
            Close();
        }

        public string SqlDataTableCommit(string strSql)
        {
            string ls_return = "";
            Oracle.ManagedDataAccess.Client.OracleTransaction ston = null;
            Oracle.ManagedDataAccess.Client.OracleCommand cmdd = null;
            try
            {
                Open();
                BeginTrans();
                if (!inTransaction)
                {
                    ston = cn.BeginTransaction();
                    inTransaction = true;
                }
                cmdd = new Oracle.ManagedDataAccess.Client.OracleCommand();
                cmdd.Connection = this.cn;

                if (inTransaction)
                {
                    cmdd.Transaction = trans;
                }
                else
                {
                    cmdd.Transaction = ston;
                }

                cmdd.CommandText = strSql;
                cmdd.ExecuteNonQuery();
                cmdd.Transaction.Commit();

            }
            catch (Exception ex)
            {
                ls_return = ex.Message.ToString();
                if (!inTransaction && cn.State.ToString().ToUpper() == "OPEN")
                {
                    ston.Rollback();
                }

                cmdd = null;
                return (ls_return);
            }
            finally
            {
                cmdd = null;
            }
            return ("TRUE");
        }

        public string Doprocedure(string proname, string[] inparam, string[] inparamvalue, string[] inparamtype, string[] outparam, string[] outparamtype, out string ls_returnxml, int func, bool ib_commit)
        {
            cmd = cn.CreateCommand();
            OracleParameter param = null;
            ls_returnxml = "";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            OracleDataAdapter da = null;
            ib_commit = false;
            string ls_cursorname = "";
            string ls_text = "";
            try
            {
                Open();
                BeginTrans();
                if (inTransaction)
                {
                    cmd.Transaction = trans;
                }
                cmd.CommandText = proname;
                cmd.CommandType = CommandType.StoredProcedure;
                for (int i = 0; i < inparam.Length; i++)
                {
                    if (inparamtype[i] == "int")
                    {
                        param = cmd.Parameters.Add(new OracleParameter(inparam[i], OracleDbType.Int32,8));
                    }
                    else if (inparamtype[i] == "varchar")
                    {
                        param = cmd.Parameters.Add(new OracleParameter(inparam[i], OracleDbType.Varchar2,400));
                    }
                    param.Direction = ParameterDirection.Input;
                    param.Value = inparamvalue[i];
                }

                for (int i = 0; i < outparam.Length; i++)
                {
                    if (outparamtype[i] == "int")
                    {
                        param = cmd.Parameters.Add(new OracleParameter(outparam[i], OracleDbType.Int32,4));
                    }
                    else if (outparamtype[i] == "varchar")
                    {
                        param = cmd.Parameters.Add(new OracleParameter(outparam[i], OracleDbType.Varchar2,400));
                    }
                    else if (outparamtype[i] == "cursor")
                    {
                        param = cmd.Parameters.Add(new OracleParameter(outparam[i], OracleDbType.RefCursor,400));
                        ls_cursorname = outparam[i];
                    }
                    param.Direction = ParameterDirection.Output;
                    param.Value = ls_text.PadRight(400, ' ');
                }
                cmd.ExecuteNonQuery();
                returncode = Convert.ToInt32(cmd.Parameters["as_returncode"].Value.ToString());
                returnmsg = Convert.ToString(cmd.Parameters["as_returnmsg"].Value.ToString());
                if (returncode != 0)
                {
                    RollbackTrans();
                    return (returnmsg);
                }
                if (ls_cursorname != "")
                {
                    if (func == 1001 || func == 1002 || func == 1060 || func == 1061 || func == 1009 || func == 1010 || func == 1013 || func == 1014 || func == 1015 || func == 1019 || func == 1022 || func == 1023 || func == 1033 || func == 1024 || func == 1026 || func == 1027 || func == 1036 || func == 1037 || func == 1046 || func == 1048 || func == 1049 || func == 1050 || func == 1052 || func == 1053 || func == 1054 || func == 1056 || func == 1057 || func == 1101 || func == 1103 || func == 1188 || func == 1039 || func == 1040 || func == 1062 || func == 1063)
                    {
                        da = new OracleDataAdapter(cmd);
                        da.TableMappings.Add("Table", ls_cursorname);
                        da.Fill(ds);
                        dt = ds.Tables[0];
                        if (dt.Rows.Count == 0)
                        {
                            if (func == 1002)
                            {
                                return ("服务器找不到相关任务");
                            }
                            else
                            {
                                WriteXml(dt);
                                ls_returnxml = doc.InnerXml;
                            }
                        }
                        else
                        {
                            WriteXml(dt);
                            ls_returnxml = doc.InnerXml;
                        }
                    }
                }
                else
                {
                    if (func == 1003)
                    {
                        ls_returnxml += "<?xml version='1.0' encoding='gb2312'?>";
                        ls_returnxml += "<function>";
                        ls_returnxml += "<data rowcount='1' columns='3'>";
                        ls_returnxml += "<row rownum='0'>";
                        ls_returnxml += "<column colnum='0' colname='checkguid'>" + Convert.ToString(cmd.Parameters["as_checkguid"].Value.ToString()) + "</column>";
                        ls_returnxml += "<column colnum='1' colname='inguid'>" + Convert.ToString(cmd.Parameters["as_inguid"].Value.ToString()) + "</column>";
                        ls_returnxml += "<column colnum='2' colname='flag'>" + Convert.ToString(cmd.Parameters["as_flag"].Value.ToString()) + "</column>";
                        ls_returnxml += "</row>";
                        ls_returnxml += "</data>";
                        ls_returnxml += "</function>";
                    }
                    else
                    {
                        ls_returnxml = "";
                    }
                }
                if (ib_commit)
                {
                    CommitTrans();
                }
                return ("TRUE");
            }
            catch (Exception ex)
            {
                RollbackTrans();
                returnmsg = ex.Message.ToString();
                return (returnmsg);
            }
            finally
            {
                cmd = null;
                if (da != null)
                {
                    da.Dispose();
                }
                if (ds != null)
                {
                    ds.Dispose();
                }
                if (dt != null)
                {
                    dt.Dispose();
                }
            }
        }

        public DataSet GetDataSet(string QueryString)
        {
            DataSet ds = null;
            Oracle.ManagedDataAccess.Client.OracleCommand cmdcur = null;
            OracleDataAdapter ad = null;
            try
            {
                Open();
                BeginTrans();
                cmdcur = new Oracle.ManagedDataAccess.Client.OracleCommand();
                cmdcur.Connection = this.cn;
                if (inTransaction)
                    cmdcur.Transaction = trans;
                ds = new DataSet();
                ad = new OracleDataAdapter();
                cmdcur.CommandText = QueryString;
                ad.SelectCommand = cmdcur;
                ad.Fill(ds);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cmdcur = null;
                ad = null;
            }
            return (ds);
        }

        public DataTable GetDataTable(string QueryString)
        {
            DataSet ds = GetDataSet(QueryString);
            if (ds != null)
            {
                if (ds.Tables.Count > 0)
                {
                    return (ds.Tables[0]);
                }
                else
                {
                    return (new DataTable());
                }
            }
            else
            {
                return (new DataTable());
            }
        }

        public string convertstring(string str/*, DataColumn dtcol*/)
        {
            str = "'" + str + "'";
            return str;
        }

        /*public string InsertAdapter(DataTable dt, string tablename, int row, string columnname, string guid)
        {
            string sql = "insert into " + tablename + "  (" + columnname + ",";
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i < dt.Columns.Count - 1)
                    sql += dt.Columns[i].ColumnName + ",";
                else
                    sql += dt.Columns[i].ColumnName;
            }
            sql += " ) values ('" + guid + "',";

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i < dt.Columns.Count - 1)
                    sql += convertstring(dt.Rows[row][i].ToString().Trim()) + ",";
                else
                    sql += convertstring(dt.Rows[row][i].ToString().Trim());
            }
            sql += " )";

            return (sql);
        }*/

        public string InsertAdapter(DataTable dt, string tablename, int row)
        {
            DataTable dtcur = new DataTable();
            dtcur = GetDataTable("select * from " + tablename + " where 1 = 2");
            string sql = "insert into " + tablename + "  (";
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i < dt.Columns.Count - 1)
                    sql += dt.Columns[i].ColumnName + ",";
                else
                    sql += dt.Columns[i].ColumnName;
            }
            sql += " ) values (";

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (dtcur.Columns[dt.Columns[i].ColumnName].DataType == typeof(string))
                {
                    if (i < dt.Columns.Count - 1)
                        sql += convertstring(dt.Rows[row][i].ToString().Trim()) + ",";
                    else
                        sql += convertstring(dt.Rows[row][i].ToString().Trim());
                }
                else if (dtcur.Columns[dt.Columns[i].ColumnName].DataType == typeof(DateTime))
                {
                    if (i < dt.Columns.Count - 1)
                        if (string.IsNullOrEmpty(dt.Rows[row][i].ToString().Trim()))
                        {
                            sql += "null,";
                        }
                        else
                        {
                            sql += "to_date(" + convertstring(dt.Rows[row][i].ToString().Trim()) + ",'yyyymmddhh24miss')" + ",";
                        }
                    else
                        if (string.IsNullOrEmpty(dt.Rows[row][i].ToString().Trim()))
                        {
                            sql += "null";
                        }
                        else
                        {
                            sql += "to_date(" + convertstring(dt.Rows[row][i].ToString().Trim()) + ",'yyyymmddhh24miss')";
                        }
                }
                else
                {
                    if (i < dt.Columns.Count - 1)
                        if (string.IsNullOrEmpty(dt.Rows[row][i].ToString().Trim()))
                        {
                            sql += "null,";
                        }
                        else
                        {
                            sql += dt.Rows[row][i].ToString().Trim() + ",";
                        }
                    else
                        if (string.IsNullOrEmpty(dt.Rows[row][i].ToString().Trim()))
                        {
                            sql += "null";
                        }
                        else
                        {
                            sql += dt.Rows[row][i].ToString().Trim();
                        }
                }
            }
            sql += " )";

            return (sql);
        }

        public string DeleteAdapter(DataTable dt, string tablename, int row)
        {
            string sql = "delete " + tablename + " where ";
            sql += dt.Columns[0].ColumnName + " = ";
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i < dt.Columns.Count - 1)
                    sql += convertstring(dt.Rows[row][i].ToString().Trim()) + ",";
                else
                    sql += convertstring(dt.Rows[row][i].ToString().Trim());
            }
            sql += " ";

            return (sql);
        }

        public string FindAdapter(DataTable dt, int row, string sql)
        {
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                sql += convertstring(dt.Rows[row][i].ToString().Trim()) + ",";
            }

            return (sql);
        }

        public string UpdateAdapter(DataTable dt, string tablename, int row)
        {
            DataTable dtcur = new DataTable();
            dtcur = GetDataTable("select * from " + tablename + " where 1 = 2");
            string sql = "update " + tablename + " set ";
            DataColumn[] dtcols = dt.PrimaryKey;
            if (dtcols.Length == 0)
            {
                return "该表没有主键,无法生成修改语句";
            }
            else
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    bool iskey = false;
                    for (int k = 0; k < dtcols.Length; k++)
                    {
                        if (dt.Columns[i].ColumnName.Trim() == dtcols[k].ColumnName.Trim())
                            iskey = true;
                    }

                    if (!iskey)
                    {
                        if (i < dt.Columns.Count - 1)
                        {
                            if (dt.Rows[row][i].ToString().Trim() != "noupdate")
                            {
                                if (dtcur.Columns[dt.Columns[i].ColumnName].DataType == typeof(string))
                                {
                                    sql += dt.Columns[i].ColumnName + "=" + convertstring(dt.Rows[row][i].ToString().Trim()) + ", ";
                                }
                                else if (dtcur.Columns[dt.Columns[i].ColumnName].DataType == typeof(DateTime))
                                {
                                    if (string.IsNullOrEmpty(dt.Rows[row][i].ToString().Trim()))
                                    {
                                        sql += dt.Columns[i].ColumnName + "=" + "null,";
                                    }
                                    else
                                    {
                                        sql += dt.Columns[i].ColumnName + "= to_date(" + convertstring(dt.Rows[row][i].ToString().Trim()) + ",'yyyymmddhh24miss'),";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(dt.Rows[row][i].ToString().Trim()))
                                    {
                                        sql += dt.Columns[i].ColumnName + "=" + "null,";
                                    }
                                    else
                                    {
                                        sql += dt.Columns[i].ColumnName + "=" + dt.Rows[row][i].ToString().Trim() + ", ";
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (dt.Rows[row][i].ToString().Trim() != "noupdate")
                            {
                                if (dtcur.Columns[dt.Columns[i].ColumnName].DataType == typeof(string))
                                {
                                    sql += dt.Columns[i].ColumnName + "=" + convertstring(dt.Rows[row][i].ToString().Trim());
                                }
                                else if (dtcur.Columns[dt.Columns[i].ColumnName].DataType == typeof(DateTime))
                                {
                                    if (string.IsNullOrEmpty(dt.Rows[row][i].ToString().Trim()))
                                    {
                                        sql += dt.Columns[i].ColumnName + "=" + "null";
                                    }
                                    else
                                    {
                                        sql += dt.Columns[i].ColumnName + "=to_date(" + convertstring(dt.Rows[row][i].ToString().Trim()) + ",'yyyymmddhh24miss')";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(dt.Rows[row][i].ToString().Trim()))
                                    {
                                        sql += dt.Columns[i].ColumnName + "=" + "null";
                                    }
                                    else
                                    {
                                        sql += dt.Columns[i].ColumnName + "=" + dt.Rows[row][i].ToString().Trim();
                                    }
                                }
                            }
                            else
                            {
                                sql = sql.Substring(0, sql.Trim().Length - 1);
                            }
                        }
                    }
                }

                sql += " where ";

                for (int j = 0; j < dtcols.Length; j++)
                {
                    if (j == 0)
                        sql += dtcols[j].ColumnName.Trim() + "=" + convertstring(dt.Rows[row][dtcols[j].ColumnName.Trim()].ToString().Trim()) + "  ";
                    else
                        sql += " and " + dtcols[j].ColumnName.Trim() + "=" + convertstring(dt.Rows[row][dtcols[j].ColumnName.Trim()].ToString().Trim()) + " ";
                }
            }

            return (sql);
        }

        /// <summary> 
        /// 中文转化为GUID（先插入基础字典后转化）
        /// </summary> 
        /// <param name="dt">需转化的数据</param>
        /// <param name="functionid">函数功能ID</param> 
        /// <param name="dbtype">数据操作类型</param> 
        /// <param name="dtnew">转化后的数据</param> 
        /// <returns>返回值</returns>
        public string data_base_do(DataTable dt, string functionid, string dbtype, out DataTable dtnew)
        {
            string ls_name;
            string ls_msg;
            string ls_productareaguid;
            string ls_unitguid;
            string ls_guid;
            long ll_maxproductareacode;
            long ll_maxunitcode;
            string ls_maxproductareacode;
            string ls_maxunitcode;
            DataTable dtcur = new DataTable();
            dtnew = dt;
            if (functionid == "2002")//机构商品目录
            {
                dtcur = GetDataTable("select max(productareacode) from tb_wms_productarea");
                ls_maxproductareacode = dtcur.Rows[0][0].ToString();
                if (string.IsNullOrEmpty(ls_maxproductareacode))
                {
                    ll_maxproductareacode = 0;
                }
                else
                {
                    ll_maxproductareacode = Convert.ToInt32(ls_maxproductareacode);
                }
                dtcur = GetDataTable("select max(unitcode) from Tb_Wms_WareUnit");
                ls_maxunitcode = dtcur.Rows[0][0].ToString();
                if (string.IsNullOrEmpty(ls_maxunitcode))
                {
                    ll_maxunitcode = 0;
                }
                else
                {
                    ll_maxunitcode = Convert.ToInt32(ls_maxunitcode);
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (dt.Columns[j].ColumnName.ToLower() == "productareaguid")
                        {
                            ls_name = dt.Rows[i]["productareaguid"].ToString();
                            if (ls_name == string.Empty && ls_name.Trim() == "")
                            {
                                dtnew.Rows[i][j] = null;
                                continue;
                            }
                            dtcur = GetDataTable("select productareaguid from tb_wms_productarea where ProductAreaName = '" + ls_name + "'");
                            if (dtcur.Rows.Count == 0)
                            {
                                dtcur = GetDataTable("select createguid() from dual");
                                ls_productareaguid = dtcur.Rows[0][0].ToString();
                                ll_maxproductareacode++;
                                ls_maxproductareacode = ll_maxproductareacode.ToString().PadLeft(5, '0');
                                ls_msg = SqlDataTable("insert into tb_wms_productarea(productareaguid,ProductAreaCode,ProductAreaName) values('" + ls_productareaguid + "','" + ls_maxproductareacode + "','" + ls_name + "')");
                                if (ls_msg != "TRUE")
                                {
                                    return ls_msg;
                                }
                                ls_guid = ls_productareaguid;
                            }
                            else
                            {
                                ls_productareaguid = dtcur.Rows[0][0].ToString();
                                ls_guid = ls_productareaguid;
                            }
                            dtnew.Rows[i][j] = ls_guid;
                        }
                        else if (dt.Columns[j].ColumnName.ToLower() == "unitguid")
                        {
                            ls_name = dt.Rows[i]["unitguid"].ToString();
                            if (ls_name == string.Empty && ls_name.Trim() == "")
                            {
                                dtnew.Rows[i][j] = null;
                                continue;
                            }
                            dtcur = GetDataTable("select UnitGUID from Tb_Wms_WareUnit where UnitName = '" + ls_name + "'");
                            if (dtcur.Rows.Count == 0)
                            {
                                dtcur = GetDataTable("select createguid() from dual");
                                ls_unitguid = dtcur.Rows[0][0].ToString();
                                ll_maxunitcode++;
                                ls_maxunitcode = ll_maxunitcode.ToString().PadLeft(5, '0');
                                ls_msg = SqlDataTable("insert into Tb_Wms_WareUnit(UnitGUID,UnitCode ,UnitName) values('" + ls_unitguid + "','" + ls_maxunitcode + "','" + ls_name + "')");
                                if (ls_msg != "TRUE")
                                {
                                    return ls_msg;
                                }
                                ls_guid = ls_unitguid;
                            }
                            else
                            {
                                ls_unitguid = dtcur.Rows[0][0].ToString();
                                ls_guid = ls_unitguid;
                            }
                            dtnew.Rows[i][j] = ls_guid;
                        }
                    }
                }
            }
            else if (functionid == "2011")//销售出库订单明细
            {
                string ls_orgguid;
                string ls_wareguid;
                string ls_batchs;
                long l_singnum;
                long l_packnum;
                long l_realsingle;
                long l_realpack;
                long l_PackNumnew;
                long l_singnum2;
                long l_packnum2;
                long l_KeepsingleNum;
                long l_keeppacknum;
                long l_orderKeepsingleNum;
                long l_orderkeeppacknum;
                long l_realsinglesum;
                long l_data;
                long l_ceilnum;
                long l_setpack;
                long l_setsingle;
                dtcur = GetDataTable("select orgguid from Tb_Wms_SaleOutstoreOrder where OrderGUID = '" + dt.Rows[0]["OrderGUID"] + "'");
                ls_orgguid = dtcur.Rows[0][0].ToString();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ls_wareguid = dt.Rows[i]["wareguid"].ToString();
                    ls_batchs = dt.Rows[i]["batchs"].ToString();
                    dtcur = GetDataTable("SELECT COALESCE(sum(nvl(WareStoreSingleNum,0)),0),COALESCE(sum(nvl(WareStorePackNum,0)),0) FROM Tb_Wms_OrgStore WHERE orgguid = '" + ls_orgguid + "' AND wareguid = '" + ls_wareguid + "' And BATCHS = '" + ls_batchs + "'");
                    l_singnum = dtcur.Rows.Count > 0 ? Convert.ToInt32(dtcur.Rows[0][0]) : 0;
                    l_packnum = dtcur.Rows.Count > 0 ? Convert.ToInt32(dtcur.Rows[0][1]) : 0;
                    dtcur = GetDataTable("SELECT COALESCE(sum(nvl(KeepsingleNum,0)),0),COALESCE(sum(nvl(KeepNum,0)),0) FROM Tb_Wms_PickKeepStore WHERE OrgGUID = '" + ls_orgguid + "' AND wareguid = '" + ls_wareguid + "'And BATCHS = '" + ls_batchs + "'");
                    l_KeepsingleNum = dtcur.Rows.Count > 0 ? Convert.ToInt32(dtcur.Rows[0][0]) : 0;
                    l_keeppacknum = dtcur.Rows.Count > 0 ? Convert.ToInt32(dtcur.Rows[0][1]) : 0;
                    dtcur = GetDataTable("SELECT COALESCE(sum(nvl(KeepsingleNum,0)),0),COALESCE(sum(nvl(KeepNum,0)),0) FROM Tb_Wms_orderKeepStore WHERE OrgGUID = '" + ls_orgguid + "' AND wareguid = '" + ls_wareguid + "' And BATCHS = '" + ls_batchs + "'");
                    l_orderKeepsingleNum = dtcur.Rows.Count > 0 ? Convert.ToInt32(dtcur.Rows[0][0]) : 0;
                    l_orderkeeppacknum = dtcur.Rows.Count > 0 ? Convert.ToInt32(dtcur.Rows[0][1]) : 0;
			        l_singnum2 = l_singnum - l_KeepsingleNum - l_orderKeepsingleNum;
			        l_packnum2 = l_packnum - l_keeppacknum - l_orderkeeppacknum;

                    l_realsingle = l_singnum2;
                    l_realpack = l_packnum2;
                    dtcur = GetDataTable("SELECT PackNum FROM tb_wms_wmsware Where WAREGUID = '"+ls_wareguid+"'");
                    l_PackNumnew = dtcur.Rows.Count > 0 ? Convert.ToInt32(dtcur.Rows[0][0]) : 0;
                    l_realsinglesum = l_realpack * l_PackNumnew + l_realsingle;
                    l_data = Convert.ToInt32(dt.Rows[i]["SingleNum"]);
                    if (l_data > l_realsinglesum)
                    {
                        return "商品【"+dt.Rows[i]["warename"].ToString()+"】库存不足";
                    }

                    if (l_packnum == 0)
                    {
                        dtnew.Rows[i]["singlenum"] = l_data;
                        dtnew.Rows[i]["packnum"] = 0;
                    }
                    else
                    {
                        l_ceilnum = Convert.ToInt32(l_data / l_packnum);
                        if (l_ceilnum > l_realpack)
                        {
                            l_setpack = l_realpack;
                            l_setsingle = l_data - l_setpack * l_packnum;
                        }
                        else
                        {
                            l_setpack = l_ceilnum;
                            l_setsingle = l_data - l_setpack * l_packnum;
                        }
                        dtnew.Rows[i]["singlenum"] = l_setsingle;
                        dtnew.Rows[i]["packnum"] = l_setpack;
                    }
                }
            }
            else if (functionid == "2013")//购进退货出库订单明细
            {
                string ls_orgguid;
                string ls_wareguid;
                string ls_batchs;
                long l_singnum;
                long l_packnum;
                long l_realsingle;
                long l_realpack;
                long l_PackNumnew;
                long l_singnum2;
                long l_packnum2;
                long l_KeepsingleNum;
                long l_keeppacknum;
                long l_orderKeepsingleNum;
                long l_orderkeeppacknum;
                long l_realsinglesum;
                long l_data;
                long l_ceilnum;
                long l_setpack;
                long l_setsingle;
                dtcur = GetDataTable("select orgguid from Tb_Wms_SaleOutstoreOrder where OrderGUID = '" + dt.Rows[0]["OrderGUID"] + "'");
                ls_orgguid = dtcur.Rows[0][0].ToString();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ls_wareguid = dt.Rows[i]["wareguid"].ToString();
                    ls_batchs = dt.Rows[i]["batchs"].ToString();
                    dtcur = GetDataTable("SELECT COALESCE(sum(nvl(WareStoreSingleNum,0)),0),COALESCE(sum(nvl(WareStorePackNum,0)),0) FROM Tb_Wms_OrgStore WHERE orgguid = '" + ls_orgguid + "' AND wareguid = '" + ls_wareguid + "' And BATCHS = '" + ls_batchs + "'");
                    l_singnum = dtcur.Rows.Count > 0 ? Convert.ToInt32(dtcur.Rows[0][0]) : 0;
                    l_packnum = dtcur.Rows.Count > 0 ? Convert.ToInt32(dtcur.Rows[0][1]) : 0;
                    dtcur = GetDataTable("SELECT COALESCE(sum(nvl(KeepsingleNum,0)),0),COALESCE(sum(nvl(KeepNum,0)),0) FROM Tb_Wms_PickKeepStore WHERE OrgGUID = '" + ls_orgguid + "' AND wareguid = '" + ls_wareguid + "'And BATCHS = '" + ls_batchs + "'");
                    l_KeepsingleNum = dtcur.Rows.Count > 0 ? Convert.ToInt32(dtcur.Rows[0][0]) : 0;
                    l_keeppacknum = dtcur.Rows.Count > 0 ? Convert.ToInt32(dtcur.Rows[0][1]) : 0;
                    dtcur = GetDataTable("SELECT COALESCE(sum(nvl(KeepsingleNum,0)),0),COALESCE(sum(nvl(KeepNum,0)),0) FROM Tb_Wms_orderKeepStore WHERE OrgGUID = '" + ls_orgguid + "' AND wareguid = '" + ls_wareguid + "' And BATCHS = '" + ls_batchs + "'");
                    l_orderKeepsingleNum = dtcur.Rows.Count > 0 ? Convert.ToInt32(dtcur.Rows[0][0]) : 0;
                    l_orderkeeppacknum = dtcur.Rows.Count > 0 ? Convert.ToInt32(dtcur.Rows[0][1]) : 0;
                    l_singnum2 = l_singnum - l_KeepsingleNum - l_orderKeepsingleNum;
                    l_packnum2 = l_packnum - l_keeppacknum - l_orderkeeppacknum;

                    l_realsingle = l_singnum2;
                    l_realpack = l_packnum2;
                    dtcur = GetDataTable("SELECT PackNum FROM tb_wms_wmsware Where WAREGUID = '" + ls_wareguid + "'");
                    l_PackNumnew = dtcur.Rows.Count > 0 ? Convert.ToInt32(dtcur.Rows[0][0]) : 0;
                    l_realsinglesum = l_realpack * l_PackNumnew + l_realsingle;
                    l_data = Convert.ToInt32(dt.Rows[i]["SingleNum"]);

                    if (l_packnum == 0)
                    {
                        dtnew.Rows[i]["singlenum"] = l_data;
                        dtnew.Rows[i]["packnum"] = 0;
                    }
                    else
                    {
                        l_ceilnum = Convert.ToInt32(l_data / l_packnum);
                        if (l_ceilnum > l_realpack)
                        {
                            l_setpack = l_realpack;
                            l_setsingle = l_data - l_setpack * l_packnum;
                        }
                        else
                        {
                            l_setpack = l_ceilnum;
                            l_setsingle = l_data - l_setpack * l_packnum;
                        }
                        dtnew.Rows[i]["singlenum"] = l_setsingle;
                        dtnew.Rows[i]["packnum"] = l_setpack;
                    }
                }
            }
            return "TRUE";
        }

        public string data_validation(DataTable dt, string functionid, string dbtype)
        {
            if (dt.Rows.Count == 0)
            {
                return "没有传入明细数据，请检查！";
            }
            if (functionid == "2002")//商品目录
            {
                if (dbtype == "1" || dbtype == "2")//新增、更新
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(dt.Rows[i]["warename"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行商品名称不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["bar"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行商品条码不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["WareCode"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行商品编码不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["PackNum"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行件装数量不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["PackVolumn"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行件装体积不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["SingleVolumn"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行单品体积不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["UnitGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行单位不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["KindGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行商品类别不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["SaveKindGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行存储类别不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["ProductAreaGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行产地不能为空！";
                        }
                    }
                }
            }
            else if (functionid == "2006")//采购入库订单
            {
                if (dbtype == "1" || dbtype == "2")//新增、更新
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(dt.Rows[i]["OrgGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行机构不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["WmsCompanyGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行往来单位不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["OrderType"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行订单类型不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["OrderNo"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行订单编号不能为空！";
                        }
                    }
                }
            }
            else if (functionid == "2007")//采购入库订单明细
            {
                if (dbtype == "1" || dbtype == "2")//新增、更新
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(dt.Rows[i]["warebar"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行商品条码不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["OrderGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行订单号不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["WareGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行商品ID不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["WareCode"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行商品编码不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["WareName"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行商品名称不能为空！";
                        }
                    }
                }
            }
            else if (functionid == "2008")//销售退货入库订单
            {
                if (dbtype == "1" || dbtype == "2")//新增、更新
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(dt.Rows[i]["OrgGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行机构不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["WmsCompanyGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行往来单位不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["OrderType"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行订单类型不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["OrderNo"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行订单编号不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["SaleOutstoreOrderGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行销售出库订单不能为空！";
                        }
                    }
                }
            }
            else if (functionid == "2009")//销售退货入库订单明细
            {
                if (dbtype == "1" || dbtype == "2")//新增、更新
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(dt.Rows[i]["OrderGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行订单号不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["WareGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行商品ID不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["WareCode"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行商品编码不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["WareName"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行商品名称不能为空！";
                        }
                    }
                }
            }
            else if (functionid == "2010")//销售出库订单
            {
                if (dbtype == "1" || dbtype == "2")//新增、更新
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(dt.Rows[i]["OrgGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行机构不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["WmsCompanyGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行往来单位不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["OrderType"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行订单类型不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["OrderCode"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行订单编号不能为空！";
                        }
                    }
                }
            }
            else if (functionid == "2011")//销售出库订单明细
            {
                if (dbtype == "1" || dbtype == "2")//新增、更新
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(dt.Rows[i]["OrderGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行订单号不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["WareGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行商品ID不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["WareCode"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行商品编码不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["WareName"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行商品名称不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["Batchs"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行批号不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["ExpDate"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行截至有效期不能为空！";
                        }
                    }
                }
            }
            else if (functionid == "2012")//购进退货出库订单
            {
                if (dbtype == "1" || dbtype == "2")//新增、更新
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(dt.Rows[i]["OrgGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行机构不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["WmsCompanyGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行往来单位不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["OrderCode"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行订单编号不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["OrderGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行采购入库订单不能为空！";
                        }
                    }
                }
            }
            else if (functionid == "2013")//购进退货出库订单明细
            {
                if (dbtype == "1" || dbtype == "2")//新增、更新
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(dt.Rows[i]["OrderGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行订单号不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["WareGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行商品ID不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["WareNo"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行商品编码不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["WareName"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行商品名称不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["Batchs"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行批号不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["ExpDate"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行截至有效期不能为空！";
                        }
                    }
                }
            }
            else if (functionid == "2014")//机构目录
            {
                if (dbtype == "1" || dbtype == "2")//新增、更新
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(dt.Rows[i]["OrgCode"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行机构编码不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["OrgName"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行机构名称不能为空！";
                        }
                    }
                }
            }
            else if (functionid == "2015")//往来单位目录
            {
                if (dbtype == "1" || dbtype == "2")//新增、更新
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(dt.Rows[i]["NatureGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行单位性质不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["KindGUID"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行单位类别不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["CompanyCode"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行单位编号不能为空！";
                        }
                        else if (string.IsNullOrEmpty(dt.Rows[i]["CompanyName"].ToString()))
                        {
                            return "第【" + (i + 1).ToString() + "】行单位名称不能为空！";
                        }
                    }
                }
            }
            return "TRUE";
        }

        public string InsertDataTable(DataTable dt, string TableName, string guidname)
        {
            string strSql = "";
            string ls_return = "";
            //string ls_guid = "";
            cmd = null;
            if (dt.Rows.Count > 0)
            {
                try
                {
                    Open();
                    BeginTrans();
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand();
                    cmd.Connection = this.cn;
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (guidname != "")//未传入GUID，自动生成
                        {
                            //ls_guid = getguid();
                            //if (ls_guid.Length != 38)
                            //{
                            //    ls_return = ls_guid;
                            //    RollbackTrans();
                            //    cmd = null;
                            //    return (ls_return);
                            //}
                            //strSql = InsertAdapter(dt, TableName, i, guidname, ls_guid);
                            ls_return = "未传入GUID";
                        }
                        else
                        {
                            strSql = InsertAdapter(dt, TableName, i);//传入了GUID
                        }
                        cmd.CommandText = strSql;
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    ls_return = ex.Message.ToString();
                    RollbackTrans();
                    cmd = null;
                    return (ls_return);
                }
                finally
                {
                    cmd = null;
                }
            }
            ls_return = "TRUE";
            return (ls_return);
        }

        public string InsertOrUpdateDataTable(DataTable dt, string TableName, string guidname)
        {
            string strSql = "";
            string ls_return = "";
            string ls_guidvalue = "";
            DataTable dtcur = new DataTable();
            cmd = null;
            if (dt.Rows.Count > 0)
            {
                try
                {
                    Open();
                    BeginTrans();
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand();
                    cmd.Connection = this.cn;
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ls_guidvalue = dt.Rows[i][guidname].ToString();
                        dtcur = GetDataTable("select " + guidname + " from " + TableName + " where " + guidname + " = '" + ls_guidvalue + "'");
                        if (dtcur.Rows.Count == 0)
                        {
                            strSql = InsertAdapter(dt, TableName, i);
                            cmd.CommandText = strSql;
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            strSql = UpdateAdapter(dt, TableName, i);
                            cmd.CommandText = strSql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ls_return = ex.Message.ToString();
                    RollbackTrans();
                    cmd = null;
                    return (ls_return);
                }
                finally
                {
                    cmd = null;
                }
            }
            ls_return = "TRUE";
            return (ls_return);
        }

        public string SqlDataTable(DataTable dt)
        {
            string strSql = "";
            string ls_return = "";
            cmd = null;
            if (dt.Rows.Count > 0)
            {
                try
                {
                    Open();
                    BeginTrans();
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand();
                    cmd.Connection = this.cn;
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        strSql = dt.Rows[i][0].ToString();
                        cmd.CommandText = "begin\r\n" + strSql + "\r\nend;";
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    ls_return = ex.Message.ToString();
                    RollbackTrans();
                    cmd = null;
                    return (ls_return);
                }
                finally
                {
                    cmd = null;
                }
            }
            ls_return = "TRUE";
            return (ls_return);
        }

        public string SqlDataTable(string strSql)
        {
            string ls_return = "";
            cmd = null;
            try
            {
                Open();
                BeginTrans();
                cmd = new Oracle.ManagedDataAccess.Client.OracleCommand();
                cmd.Connection = this.cn;
                if (inTransaction)
                {
                    cmd.Transaction = trans;
                }
                cmd.CommandText = strSql;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ls_return = ex.Message.ToString();
                RollbackTrans();
                cmd = null;
                return (ls_return);
            }
            finally
            {
                cmd = null;
            }
            return ("TRUE");
        }

        public string DeleteDataTable(DataTable dt, string TableName)
        {
            string strSql = "";
            string ls_return = "";
            cmd = null;
            if (dt.Rows.Count > 0)
            {
                try
                {
                    Open();
                    BeginTrans();
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand();
                    cmd.Connection = this.cn;
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }


                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        strSql = DeleteAdapter(dt, TableName, i);
                        cmd.CommandText = strSql;
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    ls_return = ex.Message.ToString();
                    RollbackTrans();
                    cmd = null;
                    return (ls_return);
                }
                finally
                {
                    cmd = null;
                }
            }
            return (ls_return);
        }

        public string FindDataTable(DataTable dt, string functionid, string dbtype, out string ls_xml)
        {
            string strSql = "";
            string sql = "";
            string ls_return = "";
            DataTable ddt = null;
            ls_xml = "";
            if (dt.Rows.Count > 0)
            {
                try
                {
                    switch (functionid)
                    {
                        case "2005":
                            sql = "select wareguid,warecode,warename ";
                            sql += " from tb_wms_wmsware where wareguid in( ";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                strSql = FindAdapter(dt, i, strSql);
                            }
                            break;
                        case "2017":
                            if (dbtype == "1")
                            {
                                sql = "select orderguid,CheckNo,CheckName,WareSum,NoWareSum,QualSingleSum,NoqualSingleSum,QualPackSum,NoqualPackSum,OperUser,OperDate,Remark ";
                                sql += " from TB_WMS_BUGINSTORECHECK where ORDERGUID in( ";
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    strSql = FindAdapter(dt, i, strSql);
                                }
                            }
                            else if (dbtype == "2")
                            {
                                sql = "select orderguid,CheckNo,CheckName,WareSum,NoWareSum,QualSingleSum,NoqualSingleSum,QualPackSum,NoqualPackSum,OperUser,OperDate,Remark ";
                                sql += " from Tb_Wms_SaleReturnInstoreCheck where ORDERGUID in( ";
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    strSql = FindAdapter(dt, i, strSql);
                                }
                            }
                            break;
                        case "2018":
                            if (dbtype == "1")
                            {
                                sql = "select (select orderguid from TB_WMS_BUGINSTORECHECK where checkguid = t.checkguid) as orderguid,CheckGUID,WareGUID,WareName,Units,FactArea,Specs,Batch,ExpDate,ProductDate,QualSingleNum,QualPackNum,NoqualSingleNum,NoqualPackNum,Remark ";
                                sql += " from TB_WMS_BUGINSTORECHECKdetail t where CheckGUID in(select checkguid from TB_WMS_BUGINSTORECHECK where orderguid in(";
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    strSql = FindAdapter(dt, i, strSql);
                                }
                            }
                            else if (dbtype == "2")
                            {
                                sql = "select (select orderguid from Tb_Wms_SaleReturnInstoreCheck where checkguid = t.checkguid) as orderguid,CheckGUID,WareGUID,WareName,Units,FactArea,Specs,Batch,ExpDate,ProductDate,QualSingleNum,QualPackNum,NoqualSingleNum,NoqualPackNum,Remark ";
                                sql += " from Tb_Wms_SaleReturnInstoreCheckd t where CheckGUID in(select checkguid from Tb_Wms_SaleReturnInstoreCheck where orderguid in(";
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    strSql = FindAdapter(dt, i, strSql);
                                }
                            }
                            break;
                        case "2019":
                            if (dbtype == "1")
                            {
                                sql = "select INVENTCONFIRMGUID,ORGGUID,STOREAREAGUID,FROMTYPE,OPERUSER,OPERDATE,CHECKUSER,CHECKDATA,REMARK ";
                                sql += " from TB_WMS_INVENTADD where InventConfirmGUID in( select InventConfirmGUID from Tb_Wms_InventConfirm where InventGUID in( select InventGUID from Tb_Wms_Invent where StartInventGUID in(";
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    strSql = FindAdapter(dt, i, strSql);
                                }
                            }
                            else if (dbtype == "2")
                            {
                                sql = "select INVENTCONFIRMGUID,ORGGUID,STOREAREAGUID,FROMTYPE,OPERUSER,OPERDATE,CHECKUSER,CHECKDATA,REMARK ";
                                sql += " from TB_WMS_INVENTdel where InventConfirmGUID in( select InventConfirmGUID from Tb_Wms_InventConfirm where InventGUID in( select InventGUID from Tb_Wms_Invent where StartInventGUID in(";
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    strSql = FindAdapter(dt, i, strSql);
                                }
                            }
                            break;
                        case "2020":
                            if (dbtype == "1")
                            {
                                sql = "select SheLveGUID,LocationGUID,WareGUID,batchs,InventaddPackNum,InventaddSingleNum,Remark ";
                                sql += " from Tb_Wms_InventAddDetail t where InventAddGUID in(select InventAddGUID from TB_WMS_INVENTADD where InventConfirmGUID in( select InventConfirmGUID from Tb_Wms_InventConfirm where InventGUID in( select InventGUID from Tb_Wms_Invent where StartInventGUID in(";
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    strSql = FindAdapter(dt, i, strSql);
                                }
                            }
                            else if (dbtype == "2")
                            {
                                sql = "select SheLveGUID,LocationGUID,WareGUID,batchs,InventdelPackNum,InventdelSingleNum,Remark ";
                                sql += " from Tb_Wms_InventdelDetail t where InventdelGUID in(select InventdelGUID from TB_WMS_INVENTdel where InventConfirmGUID in( select InventConfirmGUID from Tb_Wms_InventConfirm where InventGUID in( select InventGUID from Tb_Wms_Invent where StartInventGUID in(";
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    strSql = FindAdapter(dt, i, strSql);
                                }
                            }
                            break;
                    }
                    strSql = strSql.Substring(0, strSql.Length - 1) + " ";
                    switch (functionid)
                    {
                        case "2018":
                            sql += strSql + " )) and batch is not null";
                            break;
                        case "2019":
                            sql += strSql + " ))) ";
                            break;
                        case "2020":
                            sql += strSql + " )))) ";
                            break;
                        default:
                            sql += strSql + " ) ";
                            break;
                    }
                    ddt = GetDataTable(sql);
                    WriteXml(ddt);
                    ls_xml = doc.InnerXml;
                }
                catch (Exception ex)
                {
                    ls_return = ex.Message.ToString();
                    return (ls_return);
                }
                finally
                {

                }
            }
            return ("TRUE");
        }

        public string FindDataTable(DataTable dt, out string ls_xml)
        {
            string strSql = "";
            string ls_return = "";
            ls_xml = "";
            DataTable ddt = null;
            try
            {
                strSql = dt.Rows[0][0].ToString();
                ddt = GetDataTable(strSql);
                WriteXml(ddt);
                ls_xml = doc.InnerXml;
            }
            catch (Exception ex)
            {
                ls_return = ex.Message.ToString();
                return (ls_return);
            }
            ls_return = "TRUE";
            return (ls_return);
        }

        public string UpdateDataTable(DataTable dt, string TableName)
        {
            string strSql = "";
            string ls_return = "";
            cmd = null;
            if (dt.Rows.Count > 0)
            {
                try
                {
                    Open();
                    BeginTrans();
                    cmd = new Oracle.ManagedDataAccess.Client.OracleCommand();
                    cmd.Connection = this.cn;
                    if (inTransaction)
                    {
                        cmd.Transaction = trans;
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        strSql = UpdateAdapter(dt, TableName, i);
                        cmd.CommandText = strSql;
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    ls_return = ex.Message.ToString();
                    RollbackTrans();
                    cmd = null;
                    return (ls_return);
                }
                finally
                {
                    cmd = null;
                }
            }
            ls_return = "TRUE";
            return (ls_return);
        }

        public void Open()
        {
            if (cn.State.ToString().ToUpper() != "OPEN")
                this.cn.Open();
        }

        public void Close()
        {
            if (cn.State.ToString().ToUpper() == "OPEN")
            {
                this.cn.Close();
            }
        }

        public void DisPose()
        {
            if (cn.State.ToString().ToUpper() == "OPEN")
            {
                this.cn.Close();
            }

            this.cn.Dispose();
            this.cn = null;
        }

        protected void BeginTrans()
        {
            if (trans == null)
            {
                trans = null;
                trans = cn.BeginTransaction();
                inTransaction = true;
            }
        }

        protected void CommitTrans()
        {
            if (trans != null)
            {
                try
                {
                    trans.Commit();
                    inTransaction = false;
                    Close();
                }
                catch { }
            }
            else
            {
                if (cn != null)
                {
                    Close();
                }
            }
        }

        protected void RollbackTrans()
        {
            if (trans != null)
            {
                try
                {
                    trans.Rollback();
                    inTransaction = false;
                    Close();
                }
                catch { }
            }
            else
            {
                if (cn != null)
                {
                    Close();
                }
            }
        }
        #endregion

        #region 解析通讯密匙
        /*
		 * / <summary>
		 * / MD5 32位加密
		 * / </summary>
		 * / <param name="str"></param>
		 * / <returns></returns>
		 */
        public string UserMd5(string str)
        {
            string cl = str;
            string pwd = "";
            MD5 md5 = MD5.Create(); /* 实例化一个md5对像 */
            /* 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　 */
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            /* 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得 */
            for (int i = 0; i < s.Length; i++)
            {
                /* 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符 */
                pwd = pwd + s[i].ToString("X2");
            }
            return (pwd.ToUpper());
        }


        /*
         * / <summary>
         * / 获取消息序列号
         * / </summary>
         * / <returns>消息序列号</returns>
         */
        public string GetSequenceId()
        {
            return (DateTime.Now.ToString("YYYYMMDDHHmmssSSS"));
        }


        #region 字符串转Base64
        /*
		 * / <summary>
		 * / 字符串转Base64
		 * / </summary>
		 * / <param name="str">字符串</param>
		 * / <returns>Base64字符串</returns>
		 */
        public string String2Base64(string str)
        {
            byte[] byteBody = Encoding.UTF8.GetBytes(str);
            return (Convert.ToBase64String(byteBody));
        }


        #endregion

        /*
		 * / <summary>
		 * / Base64转字符串
		 * / </summary>
		 * / <param name="str">Base64字符串</param>
		 * / <returns>字符串</returns>
		 */
        public string Base642String(string str)
        {
            byte[] byteBody = Convert.FromBase64String(str);
            return (Encoding.UTF8.GetString(byteBody, 0, byteBody.Length));
        }


        #endregion

        #region readxml
        /* 构造函数，将内存流的数据存为xmlDocument */
        public void ReadXml(MemoryStream ms)
        {
            memory = ms;
            memory.Seek(0, SeekOrigin.Begin);
            xmlDoc.Load(memory);
        }


        /* 构造函数，将内存流的数据存为xmlDocument */
        public void ReadXml(string ls_xml)
        {
            //ls_xml = ls_xml.Replace("<![CDATA[", "").Replace("]]>", "");
            try
            {
                xmlDoc.LoadXml(ls_xml);
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            finally
            {

            }
        }


        #region 解析编码
        public string function_id()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "functionid")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }

            return (Getinfo);
        }

        public string org_no()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "orgcode")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_returncode()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "returncode")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_returnmsg()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "returnmsg")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_no()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "operusername")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_orderguid()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "orderguid")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_bartype()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "bartype")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_picktype()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "picktype")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_wareguid()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "wareguid")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_warepackbar()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "warepackbar")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_shelveguid()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "shelveguid")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_flag()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "flag")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_groundguid()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "groundguid")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_guid()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "guid")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_locationguid()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "locationguid")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_pickguid()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "pickguid")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_locationbar()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "locationbar")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_trayguid()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "trayguid")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_codebar()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "codebar")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_codetype()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "codetype")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_status()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "status")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_batchs()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "batchs")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_num()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "num")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_Transportguid()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "transportguid")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_boxguid()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "boxguid")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_checkguid()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "checkguid")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_boxbar()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "boxbar")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_bar()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "bar")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_type()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "type")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_usercardbar()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "usercardbar")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_inventguid()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "inventguid")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_confirmguid()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "confirmguid")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_order()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "order")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_traybar()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "traybar")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_warebar()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "warebar")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_inguid()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "inguid")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_interface()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "interface")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_model()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "model")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_labeladdress()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "labeladdress")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_businessid()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "businessid")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_time()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "opertime")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string oper_pass()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "operuserpass")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string user_no()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "interfaceusername")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string user_pass()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "interfaceuserpass")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string wms_no()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "wmscode")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        public string wms_dbtype()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "dbtype")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }
        #endregion

        /* 解析返回信息 */
        public void GetOutCode(ref string out_code, ref string outtext)
        {
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "returncode")
                {
                    out_code = xmlelem.InnerText;
                }
                else if (xmlelem.Name == "returnmsg")
                {
                    outtext = xmlelem.InnerText;
                }
            }
        }

        /* 解析单个SQL及参数 */
        public string GetSql()
        {
            string Getinfo = "";
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "SQL")
                {
                    Getinfo = xmlelem.InnerText;
                }
            }
            return (Getinfo);
        }

        /* 解析多个SQL及参数 */
        public ArrayList GetAllSQL()
        {
            int rowcount = 0;
            int rownum = 0;
            int i = 0;
            string SQLvalue = "";
            ArrayList arraySQL = new ArrayList();

            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "SQL")
                {
                    rowcount = int.Parse(xmlelem.GetAttribute("rowcount").ToString());
                    i = 0;
                    XmlNodeList SQLlist = (XmlNodeList)xmlnode.ChildNodes;
                    foreach (XmlNode rownode in SQLlist)
                    {
                        XmlElement xmlrow = (XmlElement)rownode;
                        if (xmlrow.Name == "row")
                        {
                            i++;
                            rownum = int.Parse(xmlrow.GetAttribute("rownum").ToString());
                            if (i == rownum)
                            {
                                SQLvalue = xmlrow.InnerText;
                                arraySQL.Add(SQLvalue);
                            }
                            else
                            {
                                throw new Exception("在第" + rownum.ToString() + "行出现错误！");
                            }
                        }
                    }

                    if (rowcount != arraySQL.Count)
                    {
                        throw new Exception("在读取数据时出现错误，行数与实际不符！");
                    }
                }
            }
            return (arraySQL);
        }

        public DataTable GetAllColumnsData(DataTable resourceDt)
        {
            if (resourceDt != null)
            {
                int rowcount = 0;
                int columns = 0;
                int rownum = 0;
                int colnum = 0;
                int i = 0;
                string columnvalue = "";
                int dtrownum = 0;
                DataTable dt = resourceDt;

                XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

                foreach (XmlNode xmlnode in nodeList)
                {
                    XmlElement xmlelem = (XmlElement)xmlnode;
                    if (xmlelem.Name == "data")
                    {
                        rowcount = int.Parse(xmlelem.GetAttribute("rowcount").ToString());
                        columns = int.Parse(xmlelem.GetAttribute("columns").ToString());

                        XmlNodeList datalist = (XmlNodeList)xmlnode.ChildNodes;

                        foreach (XmlNode rownode in datalist)
                        {
                            XmlElement xmlrow = (XmlElement)rownode;
                            if (xmlrow.Name == "row")
                            {
                                rownum = int.Parse(xmlrow.GetAttribute("rownum").ToString());

                                if (dtrownum == rownum)
                                {
                                    DataRow dtrow = dt.NewRow(); ;

                                    i = 0;
                                    XmlNodeList rowlist = (XmlNodeList)rownode.ChildNodes;
                                    foreach (XmlNode colnode in rowlist)
                                    {
                                        XmlElement xmlcolumn = (XmlElement)colnode;
                                        if (xmlcolumn.Name == "column")
                                        {
                                            colnum = int.Parse(xmlcolumn.GetAttribute("colnum").ToString());

                                            if (i == colnum)
                                            {
                                                columnvalue = xmlcolumn.InnerText;
                                                dtrow[colnum] = columnvalue.Replace("@@@@", " ");
                                                i++;
                                            }
                                            else
                                            {
                                                throw new Exception("在第" + rownum.ToString() + "行第" + i.ToString() + "列出现错误！");
                                            }
                                        }
                                    }
                                    dt.Rows.Add(dtrow);
                                    dtrownum++;
                                }
                                else
                                {
                                    throw new Exception("在第" + rownum.ToString() + "行出现错误！");
                                }
                            }
                        }
                    }
                }
                return (dt);
            }
            else
            {
                /* return GetAllColumnsData(); */
                return (null);
            }
        }

        public DataTable GetAllColumnsData()
        {
            int rowcount = 0;
            int columns = 0;
            int rownum = 0;
            int colnum = 0;
            int i = 0;
            string columnvalue = "";
            string colname = "";
            int dtrownum = 0;
            DataTable dt = new DataTable();

            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "data")
                {
                    rowcount = int.Parse(xmlelem.GetAttribute("rowcount").ToString());
                    columns = int.Parse(xmlelem.GetAttribute("columns").ToString());

                    for (i = 1; i <= columns; i++)
                    {
                        dt.Columns.Add("column" + i.ToString(), typeof(string));
                    }


                    XmlNodeList datalist = (XmlNodeList)xmlnode.ChildNodes;

                    foreach (XmlNode rownode in datalist)
                    {
                        XmlElement xmlrow = (XmlElement)rownode;
                        if (xmlrow.Name == "row")
                        {
                            rownum = int.Parse(xmlrow.GetAttribute("rownum").ToString());

                            if (dtrownum == rownum)
                            {
                                DataRow dtrow = dt.NewRow(); ;

                                i = 0;
                                XmlNodeList rowlist = (XmlNodeList)rownode.ChildNodes;
                                foreach (XmlNode colnode in rowlist)
                                {
                                    XmlElement xmlcolumn = (XmlElement)colnode;
                                    if (xmlcolumn.Name == "column")
                                    {
                                        colnum = int.Parse(xmlcolumn.GetAttribute("colnum").ToString());
                                        colname = xmlcolumn.GetAttribute("colname").ToString();
                                        if (i == colnum)
                                        {
                                            columnvalue = xmlcolumn.InnerText;
                                            dt.Columns[i].ColumnName = colname;
                                            dtrow[colnum] = columnvalue.Replace("@@@@", " ");
                                            i++;
                                        }
                                        else
                                        {
                                            throw new Exception("在第" + rownum.ToString() + "行第" + i.ToString() + "列出现错误！");
                                        }
                                    }
                                }
                                dt.Rows.Add(dtrow);
                                dtrownum++;
                            }
                            else
                            {
                                throw new Exception("在第" + rownum.ToString() + "行出现错误！");
                            }
                        }
                    }
                }
            }
            return dt;
        }

        public DataTable GetAllColumnsData(string[] ls_columnname, int primarykey)
        {
            int rowcount = 0;
            int columns = 0;
            int rownum = 0;
            int colnum = 0;
            int i = 0;
            string columnvalue = "";
            int dtrownum = 0;
            DataTable dt = new DataTable();

            XmlNodeList nodeList = xmlDoc.SelectSingleNode("function").ChildNodes;

            foreach (XmlNode xmlnode in nodeList)
            {
                XmlElement xmlelem = (XmlElement)xmlnode;
                if (xmlelem.Name == "data")
                {
                    rowcount = int.Parse(xmlelem.GetAttribute("rowcount").ToString());
                    columns = int.Parse(xmlelem.GetAttribute("columns").ToString());

                    for (i = 0; i <= ls_columnname.Length - 1; i++)
                    {
                        /* dt.Columns.Add("column" + i.ToString(), typeof(string)); */
                        dt.Columns.Add(ls_columnname[i], typeof(string));
                    }

                    dt.PrimaryKey = new DataColumn[] { dt.Columns[ls_columnname[primarykey]] };
                    XmlNodeList datalist = (XmlNodeList)xmlnode.ChildNodes;

                    foreach (XmlNode rownode in datalist)
                    {
                        XmlElement xmlrow = (XmlElement)rownode;
                        if (xmlrow.Name == "row")
                        {
                            rownum = int.Parse(xmlrow.GetAttribute("rownum").ToString());

                            if (dtrownum == rownum)
                            {
                                DataRow dtrow = dt.NewRow(); ;

                                i = 0;
                                XmlNodeList rowlist = (XmlNodeList)rownode.ChildNodes;
                                foreach (XmlNode colnode in rowlist)
                                {
                                    XmlElement xmlcolumn = (XmlElement)colnode;
                                    if (xmlcolumn.Name == "column")
                                    {
                                        colnum = int.Parse(xmlcolumn.GetAttribute("colnum").ToString());

                                        if (i == colnum)
                                        {
                                            columnvalue = xmlcolumn.InnerText;
                                            dtrow[colnum] = columnvalue;
                                            i++;
                                        }
                                        else
                                        {
                                            throw new Exception("在第" + rownum.ToString() + "行第" + i.ToString() + "列出现错误！");
                                        }
                                    }
                                }
                                dt.Rows.Add(dtrow);
                                dtrownum++;
                            }
                            else
                            {
                                throw new Exception("在第" + rownum.ToString() + "行出现错误！");
                            }
                        }
                    }
                }
            }
            return (dt);
        }
        #endregion

        #region writexml
        #region  将XmlDocument转化为string


        /*
		 * / <summary>
		 * / 将XmlDocument转化为string
		 * / </summary>
		 * / <param name="xmlDoc"></param>
		 * / <returns></returns>
		 */
        public string ConvertXmlToString(XmlDocument xml_Doc)
        {
            MemoryStream stream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(stream, null);
            writer.Formatting = Formatting.Indented;
            xml_Doc.Save(writer);
            StreamReader sr = new StreamReader(stream, System.Text.Encoding.UTF8);
            stream.Position = 0;
            string xmlString = sr.ReadToEnd();
            sr.Close();
            stream.Close();
            return (xmlString);
        }


        #endregion

        #region 输入请求
        /* 单个SQL或参数 */
        public void WriteXml(string function_id, string center_no, string hospital_sysno,
                      string SQL)
        {
            doc = new XmlDocument();
            /* 添加功能 */
            XmlElement rootNode = doc.CreateElement("function");
            doc.AppendChild(rootNode);

            XmlElement function_id_element = doc.CreateElement("function_id");
            function_id_element.InnerText = function_id;

            XmlElement center_no_element = doc.CreateElement("center_no");
            center_no_element.InnerText = center_no;

            XmlElement hospital_sysno_element = doc.CreateElement("hospital_sysno");
            hospital_sysno_element.InnerText = hospital_sysno;

            XmlElement SQL_element = doc.CreateElement("SQL");
            SQL_element.InnerText = SQL;

            doc.DocumentElement.AppendChild(function_id_element); /* 添加function_id元素到根节点 */
            doc.DocumentElement.AppendChild(center_no_element);
            doc.DocumentElement.AppendChild(hospital_sysno_element);
            doc.DocumentElement.AppendChild(SQL_element);

            XmlProcessingInstruction xmlPI = doc.CreateProcessingInstruction("xml", "version='1.0' encoding='gb2312'");
            doc.InsertBefore(xmlPI, doc.ChildNodes[0]);
        }


        /* 返回值 */
        public void WriteTestXml(string functionid, string returncode, string returnmsg, string orgname)
        {
            doc = new XmlDocument();
            /* 添加功能 */
            XmlElement rootNode = doc.CreateElement("function");
            doc.AppendChild(rootNode);

            XmlElement functionid_element = doc.CreateElement("functionid");
            functionid_element.InnerText = functionid;

            XmlElement returncode_element = doc.CreateElement("returncode");
            returncode_element.InnerText = returncode;

            XmlElement returnmsg_element = doc.CreateElement("returnmsg");
            returnmsg_element.InnerText = returnmsg;

            XmlElement orgname_element = doc.CreateElement("orgname");
            orgname_element.InnerText = orgname;

            doc.DocumentElement.AppendChild(functionid_element); /* 添加functionid元素到根节点 */
            doc.DocumentElement.AppendChild(returncode_element);
            doc.DocumentElement.AppendChild(returnmsg_element);
            doc.DocumentElement.AppendChild(orgname_element);

            XmlProcessingInstruction xmlPI = doc.CreateProcessingInstruction("xml", "version='1.0' encoding='gb2312'");
            doc.InsertBefore(xmlPI, doc.ChildNodes[0]);
        }


        /* 返回值 */
        public void WriteTestXml(string functionid, string returncode, string returnmsg)
        {
            doc = new XmlDocument();
            /* 添加功能 */
            XmlElement rootNode = doc.CreateElement("function");
            doc.AppendChild(rootNode);

            XmlElement functionid_element = doc.CreateElement("functionid");
            functionid_element.InnerText = functionid;

            XmlElement returncode_element = doc.CreateElement("returncode");
            returncode_element.InnerText = returncode;

            XmlElement returnmsg_element = doc.CreateElement("returnmsg");
            returnmsg_element.InnerText = returnmsg;

            doc.DocumentElement.AppendChild(functionid_element); /* 添加functionid元素到根节点 */
            doc.DocumentElement.AppendChild(returncode_element);
            doc.DocumentElement.AppendChild(returnmsg_element);

            XmlProcessingInstruction xmlPI = doc.CreateProcessingInstruction("xml", "version='1.0' encoding='gb2312'");
            doc.InsertBefore(xmlPI, doc.ChildNodes[0]);
        }


        /* 单个SQL+datatable */
        public void WriteXml(string function_id, string center_no, string hospital_sysno,
                      string SQL, DataTable dt)
        {
            doc = new XmlDocument();
            /* 添加功能 */
            XmlElement rootNode = doc.CreateElement("function");
            doc.AppendChild(rootNode);

            XmlElement function_id_element = doc.CreateElement("function_id");
            function_id_element.InnerText = function_id;

            XmlElement center_no_element = doc.CreateElement("center_no");
            center_no_element.InnerText = center_no;

            XmlElement hospital_sysno_element = doc.CreateElement("hospital_sysno");
            hospital_sysno_element.InnerText = hospital_sysno;

            XmlElement SQL_element = doc.CreateElement("SQL");
            SQL_element.InnerText = SQL;

            doc.DocumentElement.AppendChild(function_id_element); /* 添加function_id元素到根节点 */
            doc.DocumentElement.AppendChild(center_no_element);
            doc.DocumentElement.AppendChild(hospital_sysno_element);
            doc.DocumentElement.AppendChild(SQL_element);

            XmlElement dataelement = doc.CreateElement("data");
            /* 添加总行数 */
            dataelement.SetAttribute("rowcount", dt.Rows.Count.ToString());
            /* 添加总列数 */
            dataelement.SetAttribute("columns", dt.Columns.Count.ToString());

            /* 循环添加行和列 */
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                /* 添加行 */
                XmlElement addrow = doc.CreateElement("row");
                addrow.SetAttribute("rownum", i.ToString());
                dataelement.AppendChild(addrow);
                /* 添加列 */
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    XmlElement addcolume = doc.CreateElement("column");
                    addcolume.SetAttribute("colnum", j.ToString());
                    addcolume.SetAttribute("colname", dt.Columns[j].ColumnName);
                    addcolume.InnerText = dt.Rows[i][j].ToString();
                    addrow.AppendChild(addcolume);
                }
            }

            /* 添加所有元素到根节点 */
            try
            {
                doc.DocumentElement.AppendChild(dataelement);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            XmlProcessingInstruction xmlPI = doc.CreateProcessingInstruction("xml", "version='1.0' encoding='gb2312'");
            doc.InsertBefore(xmlPI, doc.ChildNodes[0]);
        }

        public string WriteXml2(DataTable dt)
        {
            doc = new XmlDocument();

            XmlElement dataelement = doc.CreateElement("data");
            /* 添加总行数 */
            dataelement.SetAttribute("rowcount", dt.Rows.Count.ToString());
            /* 添加总列数 */
            dataelement.SetAttribute("columns", dt.Columns.Count.ToString());

            /* 循环添加行和列 */
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                /* 添加行 */
                XmlElement addrow = doc.CreateElement("row");
                addrow.SetAttribute("rownum", i.ToString());
                dataelement.AppendChild(addrow);
                /* 添加列 */
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    XmlElement addcolume = doc.CreateElement("column");
                    addcolume.SetAttribute("colnum", j.ToString());
                    addcolume.SetAttribute("colname", dt.Columns[j].ColumnName);
                    addcolume.InnerText = dt.Rows[i][j].ToString();
                    addrow.AppendChild(addcolume);
                }
            }

            return dataelement.InnerXml.ToString();
        }


        /* 多个字段或参数 */
        public void WriteXml(string function_id, string center_no, string hospital_sysno,
                      ArrayList arraySQL)
        {
            doc = new XmlDocument();
            /* 添加根元素 */
            XmlElement rootNode = doc.CreateElement("function");
            doc.AppendChild(rootNode);

            XmlElement function_id_element = doc.CreateElement("function_id");
            function_id_element.InnerText = function_id;


            XmlElement center_no_element = doc.CreateElement("center_no");
            center_no_element.InnerText = center_no;

            XmlElement hospital_sysno_element = doc.CreateElement("hospital_sysno");
            hospital_sysno_element.InnerText = hospital_sysno;

            XmlElement SQL_element = doc.CreateElement("SQL");
            /* 添加总数 */
            SQL_element.SetAttribute("rowcount", arraySQL.Count.ToString());

            /* 循环添加行和列 */
            for (int i = 1; i <= arraySQL.Count; i++)
            {
                /* 添加行 */
                XmlElement addrow = doc.CreateElement("row");
                addrow.SetAttribute("rownum", i.ToString());
                addrow.InnerText = (string)arraySQL[i - 1];
                SQL_element.AppendChild(addrow);
            }

            doc.DocumentElement.AppendChild(function_id_element); /* 添加function_id元素到根节点 */
            doc.DocumentElement.AppendChild(center_no_element);
            doc.DocumentElement.AppendChild(hospital_sysno_element);
            doc.DocumentElement.AppendChild(SQL_element);

            XmlProcessingInstruction xmlPI = doc.CreateProcessingInstruction("xml", "version='1.0' encoding='gb2312'");


            doc.InsertBefore(xmlPI, doc.ChildNodes[0]);

        }


        #endregion

        #region 输出结果
        public void WriteXml(DataTable dt)
        {
            doc = new XmlDocument();
            /* 添加根元素 */
            XmlElement rootNode = doc.CreateElement("function");
            doc.AppendChild(rootNode);

            XmlElement dataelement = doc.CreateElement("data");
            /* 添加总行数 */
            dataelement.SetAttribute("rowcount", dt.Rows.Count.ToString());
            /* 添加总列数 */
            dataelement.SetAttribute("columns", dt.Columns.Count.ToString());

            /* 循环添加行和列 */
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                /* 添加行 */
                XmlElement addrow = doc.CreateElement("row");
                addrow.SetAttribute("rownum", i.ToString());
                dataelement.AppendChild(addrow);
                /* 添加列 */
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    XmlElement addcolume = doc.CreateElement("column");
                    addcolume.SetAttribute("colnum", j.ToString());
                    addcolume.SetAttribute("colname", dt.Columns[j].ColumnName);
                    addcolume.InnerText = dt.Rows[i][j].ToString();
                    addrow.AppendChild(addcolume);
                }
            }

            /* 添加所有元素到根节点 */
            try
            {
                /*
                 * doc.DocumentElement.AppendChild(idelement); / * 添加function_id元素到根节点 * /
                 * doc.DocumentElement.AppendChild(codeelement);
                 * doc.DocumentElement.AppendChild(outelement);
                 */
                doc.DocumentElement.AppendChild(dataelement);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            XmlProcessingInstruction xmlPI = doc.CreateProcessingInstruction("xml", "version='1.0' encoding='gb2312'");
            doc.InsertBefore(xmlPI, doc.ChildNodes[0]);
        }


        public void WriteXml(string fun_id, string out_code, string outtext)
        {
            doc = new XmlDocument();
            /* 添加根元素 */
            XmlElement rootNode = doc.CreateElement("function");
            doc.AppendChild(rootNode);

            XmlElement idelement = doc.CreateElement("functionid");
            idelement.InnerText = fun_id;

            /* 添加out_code元素 */
            XmlElement codeelement = doc.CreateElement("returncode");
            codeelement.InnerText = out_code;

            XmlElement outelement = doc.CreateElement("returnmsg");
            outelement.InnerText = outtext;

            doc.DocumentElement.AppendChild(idelement); /*添加functionid元素到根节点 */
            doc.DocumentElement.AppendChild(codeelement);
            doc.DocumentElement.AppendChild(outelement);


            XmlProcessingInstruction xmlPI = doc.CreateProcessingInstruction("xml", "version='1.0' encoding='gb2312'");
            doc.InsertBefore(xmlPI, doc.ChildNodes[0]);
        }


        #endregion

        public string GetStreamString(MemoryStream ms)
        {
            byte[] cache = new BinaryReader(ms).ReadBytes(Convert.ToInt32(ms.Length));
            return (Convert.ToBase64String(cache, 0, cache.Length));
        }



        #endregion
        #endregion

        #region 主接口
        [WebMethod(Description = "HTTP调用POST")]
        public string PostStr(string requeststr, out string retDatastr)
        {
            try
            {
                string strPost = requeststr;
                byte[] buffer = Encoding.UTF8.GetBytes(strPost);
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://192.168.1.234:8089/MySericeDemo/postMostStr/strlen=" + buffer.Length);
                request.Method = "POST";
                request.ContentType = "text/plain";

                request.ContentLength = buffer.Length;
                Stream requestStram = request.GetRequestStream();
                requestStram.Write(buffer, 0, buffer.Length);
                requestStram.Close();

                Stream getStream = request.GetResponse().GetResponseStream();

                byte[] resultByte = new byte[200];
                getStream.Read(resultByte, 0, resultByte.Length);
            }
            catch (Exception ex)
            {
                retDatastr = ex.ToString();
                return ex.ToString();
            }
            retDatastr = "POST提交成功！";
            return "POST提交成功！";
        }

        [WebMethod(Description = "HTTP调用GET")]
        public string GetStr(out string retDatastr)
        {
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create("http://192.168.1.234:8089/MySericeDemo/postText");
            httpRequest.Timeout = 10000;
            httpRequest.Method = "GET";
            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            StreamReader sr = new StreamReader(httpResponse.GetResponseStream(), System.Text.Encoding.GetEncoding("utf-8"));

            string result = sr.ReadToEnd();
            try
            {
                result = result.Replace("\r", "").Replace("\n", "").Replace("\t", "");
                int status = (int)httpResponse.StatusCode;
                sr.Close();
            }
            catch (Exception ex)
            {
                retDatastr = ex.ToString();
                return result;
            }
            retDatastr = result;
            return result;
        }

        [WebMethod(Description = "连接测试")]
        public string TestConnect(string requestParmXML, string uploadDataXML, out string retDataXML)
        {
            string ls_xml;
            string ls_msg;
            string ls_returnmsg;
            retDataXML = "";
            ReadXml(requestParmXML);
            OrgCode = org_no();
            WmsCode = wms_no();
            controlip = wms_dbtype();
            InterfaceUserID = user_no();
            InterfacePassWord = user_pass();
            OperUserID = oper_no();
            OperPassWord = oper_pass();
            Opertime = oper_time();
            Interface = oper_interface();
            FunctionId = function_id();
            //FunctionId = "1002";
            //Opertime = "20150822134925";
            if (Interface != "MIAWMS")
            {
                if (String2Base64(UserMd5(FunctionId + Opertime)) != Interface)
                {
                    WriteXml("0001", "2000", "通讯密匙验证失败");
                    return (doc.InnerXml);
                }
            }
            if (getcnParms(OrgCode, out ls_msg))
            {
                ls_returnmsg = checkUserValid(FunctionId, WmsCode, InterfaceUserID, InterfacePassWord, OperUserID, OperPassWord, ls_msg, 3 /*业务系统传2;PDA传1;测试传3*/, 0, out ls_xml);
                if (ls_returnmsg == "TRUE")
                {
                    //                    retDataXML = @"<?xml version='1.0' encoding='GB2312'?>
                    //                                    <function>
                    //                                    <data rowcount='1' columns='1'>
                    //                                        <row rownum='0'>
                    //                                            <column colnum='0'>" + OrgName + @"</column>
                    //                                        </row>
                    //                                    </data>
                    //                                    </function>";
                    WriteXml("0001", "0000", "测试成功");
                }
                else
                {
                    WriteXml("0001", "2001", ls_returnmsg);
                }
            }
            else
            {
                WriteXml("0001", "9100", ls_msg);
            }
            return (doc.InnerXml);
        }

        [WebMethod(Description = "PDA调用接口处理业务的方法")]
        //public string PdaInterface(string requestParmXML, string uploadDataXML/*, out string retDataXML*/)
        public string PdaInterface(string requestParmXML, string uploadDataXML, out string retDataXML)
        {
            #region 变量
            //string retDataXML = "";
            string ls_msg = "";
            string ls_returnmsg = "";
            string ls_pickguid = "";
            string ls_naturecode = "";
            string ls_returndt = "";
            string[] ls_inparam = null;
            string[] ls_inparamvalue = null;
            string[] ls_inparamtype = null;
            string[] ls_outparam = null;
            string[] ls_outparamtype = null;
            string ls_xml = "";
            string ls_bar = "";
            string ls_type = "";
            string ls_boxbar = "";
            string ls_picktype = "";
            string ls_traybar = "";
            string ls_inguid = "";
            string ls_orderguid = "";
            string ls_checkguid = "";
            string ls_groundguid = "";
            string ls_confirmguid = "";
            string ls_guid = "";
            string ls_flag = "";
            long ll_cnt = 0;
            string ls_inventnum = "";
            string ls_locationguid = "";
            string ls_getxml = "";
            string ls_controlip = "";
            string ls_opermsg = "";
            string ls_returncode = "";
            string ls_wareguid = "";
            string ls_warepackbar = "";
            string ls_inventguid = "";
            string ls_trayguid = "";
            string ls_groundnum = "";
            string ls_picknum = "";
            string ls_bartype = "";
            string ls_orgguid = "";
            string ls_usercardbar = "";
            DataTable dt = new DataTable();
            DataTable ddt = null;
            retDataXML = "";
            try
            {
                ReadXml(requestParmXML);
                OrgCode = org_no();
                WmsCode = wms_no();
                controlip = wms_dbtype();
                InterfaceUserID = user_no();
                InterfacePassWord = user_pass();
                OperUserID = oper_no();
                OperPassWord = oper_pass();
                Opertime = oper_time();
                Interface = oper_interface();
                FunctionId = function_id();
            #endregion

                #region 密匙验证
                if (Interface != "MIAWMS")
                {
                    if (String2Base64(UserMd5(FunctionId + Opertime)) != Interface)
                    {
                        WriteXml("0001", "2000", "通讯密匙验证失败");
                        return (doc.InnerXml);
                    }
                }
                #endregion

                #region 获取数据库连接
                if (!getcnParms(OrgCode, out ls_msg))
                {
                    WriteXml(FunctionId, "9100", ls_msg);
                    return (doc.InnerXml);
                }
                #endregion

                #region 工号牌登陆1040
                if (FunctionId == "1040")
                {
                    ReadXml(uploadDataXML);
                    OperUserID = oper_usercardbar();
                    ls_usercardbar = oper_usercardbar();
                }
                #endregion

                #region 验证身份
                if (FunctionId == "1001" || FunctionId == "1002" || FunctionId == "1040") /* 需要身份验证的业务 */
                {
                    ls_returnmsg = checkUserValid(FunctionId, WmsCode, InterfaceUserID, InterfacePassWord, OperUserID, OperPassWord, ls_msg, 1 /*业务系统传2;PDA传1*/, 1, out ls_xml);
                }
                else
                {
                    ls_returnmsg = checkUserValid(FunctionId, WmsCode, InterfaceUserID, InterfacePassWord, OperUserID, OperPassWord, ls_msg, 1 /*业务系统传2;PDA传1*/, 0, out ls_xml);
                }
                if (ls_returnmsg != "TRUE")
                {
                    WriteXml(FunctionId, "2001", ls_returnmsg);
                    return (doc.InnerXml);
                }
                #endregion

                switch (FunctionId)
                {
                    #region 用户登录1001
                    case "1001":
                        #region 效验
                        ls_inparam = new string[2];
                        ls_inparam[0] = "operusername";
                        ls_inparam[1] = "operuserpass";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = OperUserID;
                        ls_inparamvalue[1] = OperPassWord;
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1001", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[1];
                        ls_inparam[0] = "as_operuser";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = OperUserID;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_getpdainterface2", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1001, true);

                        //ls_inparam = new string[1];
                        //ls_inparam[0] = "as_usercard";
                        //ls_inparamvalue = new string[1];
                        //ls_inparamvalue[0] = ls_usercardbar;
                        //ls_inparamtype = new string[1];
                        //ls_inparamtype[0] = "varchar";
                        //ls_outparam = new string[3];
                        //ls_outparam[0] = "cur_Jobs";
                        //ls_outparam[1] = "as_returncode";
                        //ls_outparam[2] = "as_returnmsg";
                        //ls_outparamtype = new string[3];
                        //ls_outparamtype[0] = "cursor";
                        //ls_outparamtype[1] = "varchar";
                        //ls_outparamtype[2] = "varchar";
                        //ls_returnmsg = Doprocedure("miawms.sp_getpdainterface", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1001, true);
                        break;
                    #endregion


                    #region 工号牌登陆1040
                    case "1040":
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "usercardbar";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_usercardbar;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1040", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[1];
                        ls_inparam[0] = "as_usercard";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_usercardbar;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_getpdainterface", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1040, true);
                        break;
                    #endregion


                    #region 下载任务列表1002
                    case "1002":
                        ReadXml(uploadDataXML);
                        ls_type = oper_type();
                        ls_bar = oper_bar();
                        #region 效验
                        ls_inparam = new string[3];
                        ls_inparam[0] = "as_usercode";
                        ls_inparam[1] = "as_type";
                        ls_inparam[2] = "as_bar";
                        ls_inparamvalue = new string[3];
                        ls_inparamvalue[0] = OperUserID;
                        ls_inparamvalue[1] = ls_type;
                        ls_inparamvalue[2] = ls_bar;
                        ls_inparamtype = new string[3];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.checknew1002", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        //#region 效验
                        //ls_inparam = new string[1];
                        //ls_inparam[0] = "as_usercode";
                        //ls_inparamvalue = new string[1];
                        //ls_inparamvalue[0] = OperUserID;
                        //ls_inparamtype = new string[1];
                        //ls_inparamtype[0] = "varchar";
                        //ls_outparam = new string[2];
                        //ls_outparam[0] = "as_returncode";
                        //ls_outparam[1] = "as_returnmsg";
                        //ls_outparamtype = new string[2];
                        //ls_outparamtype[0] = "varchar";
                        //ls_outparamtype[1] = "varchar";
                        //ls_returnmsg = Doprocedure("miawms.check1002", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        //if (ls_returnmsg != "TRUE")
                        //{
                        //    break;
                        //}
                        //#endregion
                        ls_inparam = new string[3];
                        ls_inparam[0] = "as_usercode";
                        ls_inparam[1] = "as_type";
                        ls_inparam[2] = "as_bar";
                        ls_inparamvalue = new string[3];
                        ls_inparamvalue[0] = OperUserID;
                        ls_inparamvalue[1] = ls_type;
                        ls_inparamvalue[2] = ls_bar;
                        ls_inparamtype = new string[3];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_getwork2", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1002, true);

                        //ls_inparam = new string[1];
                        //ls_inparam[0] = "as_usercode";
                        //ls_inparamvalue = new string[1];
                        //ls_inparamvalue[0] = OperUserID;
                        //ls_inparamtype = new string[1];
                        //ls_inparamtype[0] = "varchar";
                        //ls_outparam = new string[3];
                        //ls_outparam[0] = "cur_Jobs";
                        //ls_outparam[1] = "as_returncode";
                        //ls_outparam[2] = "as_returnmsg";
                        //ls_outparamtype = new string[3];
                        //ls_outparamtype[0] = "cursor";
                        //ls_outparamtype[1] = "varchar";
                        //ls_outparamtype[2] = "varchar";
                        //ls_returnmsg = Doprocedure("miawms.sp_getwork", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1002, true);
                        break;
                    #endregion


                    #region 获取剩余任务数
                    case "1060":
                        ReadXml(uploadDataXML);
                        ls_type = oper_type();
                        #region 效验
                        ls_inparam = new string[2];
                        ls_inparam[0] = "as_operuser";
                        ls_inparam[1] = "as_type";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = OperUserID;
                        ls_inparamvalue[1] = ls_type;
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1060", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[2];
                        ls_inparam[0] = "as_usercode";
                        ls_inparam[1] = "as_type";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = OperUserID;
                        ls_inparamvalue[1] = ls_type;
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_getworknum", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1060, true);
                        break;
                    #endregion


                    #region 根据商品条码获取商品信息
                    case "1061":
                        ReadXml(uploadDataXML);
                        ls_bar = oper_warebar();
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "as_bar";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_bar;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1061", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[1];
                        ls_inparam[0] = "as_bar";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_bar;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_getwaremaininfo", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1061, true);
                        break;
                    #endregion


                    #region 下载条码列表1046
                    case "1046":
                        ReadXml(uploadDataXML);
                        ls_bartype = oper_bartype();
                        ls_inparam = new string[1];
                        ls_inparam[0] = "as_bartype";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_bartype;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_getbar", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1046, true);
                        break;
                    #endregion


                    #region 根据条码领取任务1088
                    case "1088":
                        ReadXml(uploadDataXML);
                        ls_bar = oper_bar();
                        #region 效验
                        #endregion
                        ls_inparam = new string[1];
                        ls_inparam[0] = "as_bar";
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_bar;
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";

                        ls_returnmsg = Doprocedure("miawms.sp_workbar", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1088, true);
                        break;
                    #endregion


                    #region 领取验收任务1003
                    case "1003":
                        ReadXml(uploadDataXML);
                        ls_orderguid = oper_orderguid();
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "orderguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_orderguid;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1003", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[2];
                        ls_inparam[0] = "as_ORDERGUID";
                        ls_inparam[1] = "as_usercode";
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = ls_orderguid;
                        ls_inparamvalue[1] = OperUserID;

                        ls_outparam = new string[5];
                        ls_outparam[0] = "as_checkguid";
                        ls_outparam[1] = "as_inguid";
                        ls_outparam[2] = "as_flag";
                        ls_outparam[3] = "as_returncode";
                        ls_outparam[4] = "as_returnmsg";
                        ls_outparamtype = new string[5];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_outparamtype[3] = "varchar";
                        ls_outparamtype[4] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_checkstart", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1003, true);
                        break;
                    #endregion


                    #region 取消验收任务1008
                    case "1008":
                        ReadXml(uploadDataXML);
                        ls_orderguid = oper_orderguid();
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "orderguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_orderguid;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1008", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[1];
                        ls_inparam[0] = "as_ORDERGUID";
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_orderguid;

                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_checkcancel", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1008, true);
                        break;
                    #endregion


                    #region 取消上架任务1018
                    case "1018":
                        ReadXml(uploadDataXML);
                        ls_groundguid = oper_groundguid();
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "groundguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_groundguid;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1018", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        //更新状态
                        //ls_returnmsg = SqlDataTable("delete from tb_wms_labelgrounddetail where LabelGroundGUID = (select LabelGroundGUID from tb_wms_labelground where groundguid = '" + ls_groundguid + "')");
                        //ls_returnmsg = SqlDataTable("delete from tb_wms_labelground where groundguid = '" + ls_groundguid + "'");
                        ls_returnmsg = SqlDataTable("update Tb_Wms_Ground set States = '3' where groundguid = '" + ls_groundguid + "'");
                        //                    ls_returnmsg = SqlDataTable(@"update Tb_Wms_BugInstoreOrder set Status = '5' where Orderguid = 
                        //                                                                                 (select orderguid
                        //                                                                                    from tb_wms_buginstorecheck
                        //                                                                                   where checkguid =
                        //                                                                                         (select checkguid
                        //                                                                                            from tb_wms_buginstore
                        //                                                                                           where inguid =
                        //                                                                                                 (select inguid
                        //                                                                                                    from tb_wms_ground
                        //                                                                                                   where groundguid = '" + ls_groundguid + "')))");
                        //                    ls_returnmsg = SqlDataTable(@"update Tb_Wms_salereturnInstoreOrder set Status = '5' where Orderguid = 
                        //                                                                                 (select orderguid
                        //                                                                                    from tb_wms_salereturninstorecheck
                        //                                                                                   where checkguid =
                        //                                                                                         (select checkguid
                        //                                                                                            from tb_wms_salereturninstore
                        //                                                                                           where inguid =
                        //                                                                                                 (select inguid
                        //                                                                                                    from tb_wms_ground
                        //                                                                                                   where groundguid = '" + ls_groundguid + "')))");

                        ls_returnmsg = "TRUE";
                        dt = new DataTable();
                        dt = GetDataTable(@"select naturecode from tb_wms_storeareanature where natureguid in(select natureguid from tb_wms_ground where groundguid = '" + ls_groundguid + "')");
                        ls_naturecode = dt.Rows[0][0].ToString();

                        //控制器下发复位指令
                        if (ls_returnmsg == "TRUE" && ((ib_iflabel == "2" && ls_naturecode == "002") || ib_iflabel == "3"))
                        {
                            dt = new DataTable();
                            dt = GetDataTable("select ip from Tb_Wms_Control where ControlGUID in (select ControlGUID from Tb_Wms_Ground where groundguid = '" + ls_groundguid + "')");
                            if (dt.Rows.Count == 0)
                            {
                                ls_returnmsg = "没有指定控制器IP，不能下发，请检查！";
                                break;
                            }
                            ls_controlip = dt.Rows[0][0].ToString();
                            ls_returnmsg = LabelInterface(
                                                        @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                        <function>
                                                            <functionid>3009</functionid>
                                                            <wmscode></wmscode>
                                                            <orgcode>" + OrgCode + @"</orgcode>
                                                            <interfaceusername></interfaceusername>
                                                            <interfaceuserpass></interfaceuserpass>
                                                            <operusername></operusername>
                                                            <operuserpass></operuserpass>
                                                            <opertime></opertime>
                                                            <dbtype>" + ls_controlip + @"</dbtype>
                                                            <interface>MIAWMS</interface>
                                                        </function>",
                                                            "",
                                                            out ls_xml);
                            ReadXml(ls_returnmsg);
                            ls_returncode = oper_returncode();
                            ls_opermsg = oper_returnmsg();
                            if (ls_returncode != "0000")
                            {
                                ls_returnmsg = ls_opermsg;
                                break;
                            }
                            else
                            {
                                ls_returnmsg = "TRUE";
                            }
                        }
                        break;
                    #endregion


                    #region 取消拣货任务1021
                    case "1021":
                        ReadXml(uploadDataXML);
                        ls_pickguid = oper_pickguid();
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "pickguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_pickguid;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1021", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        //更新状态
                        ls_returnmsg = SqlDataTable("update Tb_Wms_pick set Status = '1' where pickguid = '" + ls_pickguid + "'");
                        ls_returnmsg = SqlDataTable("update Tb_Wms_control set Status = '0' where controlguid = (select controlguid from tb_wm_pick where pickguid = '" + ls_pickguid + "')");
                        ddt = new DataTable();
                        ddt = GetDataTable("select NatureCode from tb_wms_storeareanature where NatureGUID in (select NatureGUID from Tb_Wms_pick where pickguid = '" + ls_pickguid + "')");
                        ls_naturecode = ddt.Rows[0][0].ToString();
                        ////生成电子标签上架单及明细（已经挪到分配货位中实现）
                        //if (ib_iflabel)
                        //{
                        //ls_returnmsg = Doprocedure("miawms.sp_buginstoreissued", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1007, true);
                        //}
                        ls_returnmsg = "TRUE";
                        //电子标签下发复位指令
                        if (ls_returnmsg == "TRUE" && (((ib_iflabel == "1" || ib_iflabel == "2") && ls_naturecode == "002") || ib_iflabel == "3"))
                        {
                            dt = new DataTable();
                            dt = GetDataTable("select ip from Tb_Wms_Control where ControlGUID in (select ControlGUID from Tb_Wms_pick where pickguid = '" + ls_pickguid + "')");
                            if (dt.Rows.Count == 0)
                            {
                                ls_returnmsg = "没有指定控制器IP，不能下发，请检查！";
                                break;
                            }
                            ls_controlip = dt.Rows[0][0].ToString();

                            ls_returnmsg = LabelInterface(
                                                        @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                        <function>
                                                            <functionid>3009</functionid>
                                                            <wmscode></wmscode>
                                                            <orgcode>" + OrgCode + @"</orgcode>
                                                            <interfaceusername></interfaceusername>
                                                            <interfaceuserpass></interfaceuserpass>
                                                            <operusername></operusername>
                                                            <operuserpass></operuserpass>
                                                            <opertime></opertime>
                                                            <dbtype>" + ls_controlip + @"</dbtype>
                                                            <interface>MIAWMS</interface>
                                                        </function>",
                                                            "",
                                                            out ls_xml);
                            ReadXml(ls_returnmsg);
                            ls_returncode = oper_returncode();
                            ls_opermsg = oper_returnmsg();
                            ls_returnmsg = ls_opermsg;
                            if (ls_returncode != "0000")
                            {
                                ls_returnmsg = ls_opermsg;
                                break;
                            }
                            else
                            {
                                ls_returnmsg = "TRUE";
                            }
                        }
                        break;
                    #endregion


                    #region 取消盘点任务1029
                    case "1029":
                        ReadXml(uploadDataXML);
                        ls_inventguid = oper_inventguid();
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "inventguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_inventguid;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1029", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        //更新状态
                        ls_returnmsg = SqlDataTable(@"update Tb_Wms_Invent set Status = '1' where inventguid = '" + ls_inventguid + "'");
                        ls_returnmsg = SqlDataTable("update Tb_Wms_control set Status = '0' where controlguid = (select ControlGUID from Tb_Wms_LabelPick where pickguid = '" + ls_inventguid + "')");

                        ddt = new DataTable();
                        ddt = GetDataTable("select NatureCode from tb_wms_storeareanature where natureguid in(select natureguid from tb_wms_storearea where storeareaguid in (select storeareaguid from Tb_Wms_Invent where inventguid = '" + ls_inventguid + "'))");
                        ls_naturecode = ddt.Rows[0][0].ToString();
                        ////生成电子标签上架单及明细（已经挪到分配货位中实现）
                        //if (ib_iflabel)
                        //{
                        //ls_returnmsg = Doprocedure("miawms.sp_buginstoreissued", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1007, true);
                        //}
                        ls_returnmsg = "TRUE";
                        //电子标签下发复位指令
                        if (ls_returnmsg == "TRUE" && (((ib_iflabel == "1" || ib_iflabel == "2") && ls_naturecode == "002") || ib_iflabel == "3"))
                        {
                            dt = new DataTable();
                            dt = GetDataTable("select ip from Tb_Wms_Control where ControlGUID in (select ControlGUID from Tb_Wms_labelpick where pickguid = '" + ls_inventguid + "')");
                            if (dt.Rows.Count == 0)
                            {
                                ls_returnmsg = "没有指定控制器IP，不能下发，请检查！";
                                break;
                            }
                            ls_controlip = dt.Rows[0][0].ToString();
                            ls_returnmsg = LabelInterface(
                                                        @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                        <function>
                                                            <functionid>3009</functionid>
                                                            <wmscode></wmscode>
                                                            <orgcode>" + OrgCode + @"</orgcode>
                                                            <interfaceusername></interfaceusername>
                                                            <interfaceuserpass></interfaceuserpass>
                                                            <operusername></operusername>
                                                            <operuserpass></operuserpass>
                                                            <opertime></opertime>
                                                            <dbtype>" + ls_controlip + @"</dbtype>
                                                            <interface>MIAWMS</interface>
                                                        </function>",
                                                            "",
                                                            out ls_xml);
                            ReadXml(ls_returnmsg);
                            ls_returncode = oper_returncode();
                            ls_opermsg = oper_returnmsg();
                            if (ls_returncode != "0000")
                            {
                                ls_returnmsg = ls_opermsg;
                                break;
                            }
                            else
                            {
                                ls_returnmsg = "TRUE";
                            }
                        }
                        break;
                    #endregion


                    #region 取消复核任务1035
                    case "1035":
                        ReadXml(uploadDataXML);
                        ls_confirmguid = oper_confirmguid();
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "confirmguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_confirmguid;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1035", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        //更新状态
                        ls_returnmsg = SqlDataTable(@"update tb_wms_pickconfirm set Status = '1' where status <> '8' pickguid in(select pickguid
                                                      from tb_wms_pick
                                                     where outguid = (select outguid
                                                                        from tb_wms_pick
                                                                       where pickguid = (Select pickguid
                                                                                           from tb_wms_pickconfirm
                                                                                          where confirmguid = '" + ls_confirmguid + @"'))
                                                       and storeareaguid =
                                                           (select storeareaguid
                                                              from tb_wms_pick
                                                             where pickguid = (Select pickguid
                                                                                 from tb_wms_pickconfirm
                                                                                where confirmguid = '" + ls_confirmguid + "')))");
                        break;
                    #endregion


                    #region 查询已领取任务1019
                    case "1019":
                        ReadXml(uploadDataXML);
                        ls_type = oper_type();
                        ls_inparam = new string[2];
                        ls_inparam[0] = "as_usercode";
                        ls_inparam[1] = "as_type";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = OperUserID;
                        ls_inparamvalue[1] = ls_type;
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_haveget", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1019, true);
                        break;
                    #endregion


                    #region 查询服务器时间
                    case "1062":
                        ReadXml(uploadDataXML);
                        ls_inparam = new string[0];
                        ls_inparamvalue = new string[0];
                        ls_inparamtype = new string[0];
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_getserverdate", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1062, true);
                        break;
                    #endregion


                    #region 单品周转箱绑定1004
                    case "1004":
                        ReadXml(uploadDataXML);
                        ls_boxbar = oper_boxbar();
                        ls_orderguid = oper_orderguid();
                        ls_checkguid = oper_checkguid();
                        ls_flag = oper_flag();
                        ls_inguid = oper_inguid();
                        #region 效验
                        ls_inparam = new string[5];
                        ls_inparam[0] = "as_orderguid";
                        ls_inparam[1] = "as_checkguid";
                        ls_inparam[2] = "as_inguid";
                        ls_inparam[3] = "as_boxbar";
                        ls_inparam[4] = "as_flag";
                        ls_inparamvalue = new string[5];
                        ls_inparamvalue[0] = ls_orderguid;
                        ls_inparamvalue[1] = ls_checkguid;
                        ls_inparamvalue[2] = ls_inguid;
                        ls_inparamvalue[3] = ls_boxbar;
                        ls_inparamvalue[4] = ls_flag;
                        ls_inparamtype = new string[5];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_inparamtype[3] = "varchar";
                        ls_inparamtype[4] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1004", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        dt = GetAllColumnsData();
                        ls_returnmsg = boxbar(ls_boxbar, ls_orderguid, ls_checkguid, ls_inguid, dt, ls_flag);
                        break;
                    #endregion


                    #region 商品件托盘绑定1005
                    case "1005":
                        ReadXml(uploadDataXML);
                        ls_traybar = oper_traybar();
                        ls_orderguid = oper_orderguid();
                        ls_checkguid = oper_checkguid();
                        ls_inguid = oper_inguid();
                        ls_flag = oper_flag();
                        #region 效验
                        ls_inparam = new string[5];
                        ls_inparam[0] = "as_orderguid";
                        ls_inparam[1] = "as_checkguid";
                        ls_inparam[2] = "as_inguid";
                        ls_inparam[3] = "as_traybar";
                        ls_inparam[4] = "as_flag";
                        ls_inparamvalue = new string[5];
                        ls_inparamvalue[0] = ls_orderguid;
                        ls_inparamvalue[1] = ls_checkguid;
                        ls_inparamvalue[2] = ls_inguid;
                        ls_inparamvalue[3] = ls_traybar;
                        ls_inparamvalue[4] = ls_flag;
                        ls_inparamtype = new string[5];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_inparamtype[3] = "varchar";
                        ls_inparamtype[4] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1005", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        dt = GetAllColumnsData();
                        ls_returnmsg = traybar(ls_traybar, ls_orderguid, ls_checkguid, ls_inguid, dt, ls_flag);
                        break;
                    #endregion


                    #region 完成验收任务1006
                    case "1006":
                        ReadXml(uploadDataXML);
                        ls_orderguid = oper_orderguid();
                        ls_checkguid = oper_checkguid();
                        ls_inguid = oper_inguid();

                        #region 效验
                        ls_inparam = new string[3];
                        ls_inparam[0] = "orderguid";
                        ls_inparam[1] = "checkguid";
                        ls_inparam[2] = "inguid";
                        ls_inparamvalue = new string[3];
                        ls_inparamvalue[0] = ls_orderguid;
                        ls_inparamvalue[1] = ls_checkguid;
                        ls_inparamvalue[2] = ls_inguid;
                        ls_inparamtype = new string[3];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1006", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion

                        ddt = new DataTable();
                        ddt = GetDataTable(@"select count(1) from tb_wms_buginstoreorder where orderguid = '" + ls_orderguid + "' and status = getorderstatus('订单验收完成')");
                        ll_cnt = Convert.ToInt32(ddt.Rows[0][0].ToString());
                        ddt = new DataTable();
                        ddt = GetDataTable(@"select count(1) from tb_wms_salereturninstoreorder where orderguid = '" + ls_orderguid + "' and status = getorderstatus('订单验收完成')");
                        ll_cnt += Convert.ToInt32(ddt.Rows[0][0].ToString());
                        ddt = new DataTable();
                        ddt = GetDataTable(@"select count(1) from tb_wms_periodinstore where InGUID = '" + ls_inguid + "' and status = '1'");
                        ll_cnt += Convert.ToInt32(ddt.Rows[0][0].ToString());
                        ddt = new DataTable();
                        ddt = GetDataTable(@"select count(1) from Tb_Wms_moveInstore where InGUID = '" + ls_inguid + "' and status = '1'");
                        ll_cnt += Convert.ToInt32(ddt.Rows[0][0].ToString());

                        if (ll_cnt > 0)
                        {
                            ls_returnmsg = "已完成任务验收，不能重复操作！";
                            break;
                        }
                        else
                        {

                        }

                        ll_cnt = 0;
                        ddt = new DataTable();
                        ddt = GetDataTable(@"select count(1) from TB_WMS_TRANSPORTBOXWARE where billno = '" + ls_inguid + "'");
                        ll_cnt = Convert.ToInt32(ddt.Rows[0][0].ToString());

                        ddt = new DataTable();
                        ddt = GetDataTable(@"select count(1) from Tb_Wms_TrayWarePack where InGUID = '" + ls_inguid + "'");
                        ll_cnt += Convert.ToInt32(ddt.Rows[0][0].ToString());

                        if (ll_cnt == 0)
                        {
                            ls_returnmsg = "没有进行过验收绑定，不能提交完成操作！";
                            break;
                        }
                        else
                        {

                        }


                        if (string.IsNullOrEmpty(ls_orderguid))
                        {
                            dt = GetDataTable(@"select count(1) from Tb_Wms_PeriodInstore where InGUID ='" + ls_inguid + "'");
                            if (Convert.ToInt32(dt.Rows[0][0].ToString()) > 0)
                            {
                                ls_returnmsg = companywork(ls_inguid);//期初绑定完成
                                break;
                            }
                            else
                            {
                                dt = GetDataTable(@"select count(1) from Tb_Wms_moveInstore where InGUID ='" + ls_inguid + "'");
                                if (Convert.ToInt32(dt.Rows[0][0].ToString()) > 0)
                                {
                                    ls_returnmsg = companywork2(ls_inguid);//零货移库绑定完成
                                    break;
                                }
                                else
                                {
                                    ls_returnmsg = "无法识别的入库单号";
                                    break;
                                }
                            }
                        }
                        ls_inparam = new string[3];
                        ls_inparam[0] = "as_orderguid";
                        ls_inparam[1] = "as_checkguid";
                        ls_inparam[2] = "as_inguid";
                        ls_inparamtype = new string[3];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_inparamvalue = new string[3];
                        ls_inparamvalue[0] = ls_orderguid;
                        ls_inparamvalue[1] = ls_checkguid;
                        ls_inparamvalue[2] = ls_inguid;

                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_ofcheckbill", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1006, true);
                        break;
                    #endregion


                    #region 领取上架任务1007
                    case "1007":
                        ReadXml(uploadDataXML);
                        ls_groundguid = oper_groundguid();
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "groundguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_groundguid;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1007", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion

                        ls_inparam = new string[1];
                        ls_inparam[0] = "as_groundguid";
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_groundguid;

                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = "TRUE";
                        dt = new DataTable();
                        dt = GetDataTable(@"select naturecode from tb_wms_storeareanature where natureguid in(select natureguid from tb_wms_ground where groundguid = '" + ls_groundguid + "')");
                        ls_naturecode = dt.Rows[0][0].ToString();
                        //电子标签亮灯
                        if (ls_returnmsg == "TRUE" && ((ib_iflabel == "2" && ls_naturecode == "002") || ib_iflabel == "3"))
                        {
                            dt = GetDataTable(@"select BusinessID,LabelAddress from tb_wms_labelgrounddetail where labelgroundguid in( select labelgroundguid from Tb_Wms_LabelGround where groundguid = '" + ls_groundguid + "') group by BusinessID,LabelAddress");
                            ddt = new DataTable();
                            ddt = GetDataTable("select ip from Tb_Wms_Control where ControlGUID in (select ControlGUID from Tb_Wms_Ground where groundguid = '" + ls_groundguid + "')");
                            if (ddt.Rows.Count == 0)
                            {
                                ls_returnmsg = "没有指定控制器IP，不能下发，请检查！";
                                break;
                            }
                            ls_controlip = ddt.Rows[0][0].ToString();
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                ddt = new DataTable();
                                ddt = GetDataTable(@"select WarePost,sum(WareNum) as WareNum from tb_wms_labelgrounddetail where LabelAddress = '" + dt.Rows[i][1].ToString() + "' and LabelGroundGUID in(select LabelGroundGUID from Tb_Wms_LabelGround where GroundGUID = '" + ls_groundguid + "') group by warepost,warepostrow,warepostcolumn");
                                ls_getxml = WriteXml2(ddt);
                                //下发补货指令，电子标签亮灯
                                ls_returnmsg = LabelInterface(
                                                            @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                        <function>
                                                            <functionid>3004</functionid>
                                                            <wmscode></wmscode>
                                                            <orgcode>" + OrgCode + @"</orgcode>
                                                            <interfaceusername></interfaceusername>
                                                            <interfaceuserpass></interfaceuserpass>
                                                            <operusername></operusername>
                                                            <operuserpass></operuserpass>
                                                            <opertime></opertime>
                                                            <dbtype>" + ls_controlip + @"</dbtype>
                                                            <interface>MIAWMS</interface>
                                                        </function>",
                                                            @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                        <function>
                                                            <model>1</model>
                                                            <businessid>" + dt.Rows[i][0].ToString() + @"</businessid>
                                                            <labeladdress>" + dt.Rows[i][1].ToString() + @"</labeladdress>
                                                            <waresum>" + ddt.Rows.Count.ToString() + @"</waresum>
                                                            <data rowcount='" + ddt.Rows.Count.ToString() + @"' columns=""4"">
                                                                " + ls_getxml + @"
                                                            </data>
                                                        </function>",
                                                                out ls_xml);
                                ReadXml(ls_returnmsg);
                                ls_returncode = oper_returncode();
                                ls_opermsg = oper_returnmsg();
                                ls_returnmsg = ls_opermsg;
                                if (ls_returncode != "0000")
                                {
                                    //复位？
                                    break;
                                }
                            }
                            if (ls_returncode != "0000")
                            {
                                break;
                            }
                            //发送完成下发指令
                            ls_returnmsg = LabelInterface(
                                                        @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                            <function>
                                                                <functionid>3005</functionid>
                                                                <wmscode></wmscode>
                                                                <orgcode>" + OrgCode + @"</orgcode>
                                                                <interfaceusername></interfaceusername>
                                                                <interfaceuserpass></interfaceuserpass>
                                                                <operusername></operusername>
                                                                <operuserpass></operuserpass>
                                                                <opertime></opertime>
                                                                <dbtype>" + ls_controlip + @"</dbtype>
                                                                <interface>MIAWMS</interface>
                                                            </function>", "", out ls_xml);
                            ReadXml(ls_returnmsg);
                            ls_returncode = oper_returncode();
                            ls_opermsg = oper_returnmsg();
                            ls_returnmsg = ls_opermsg;
                            if (ls_returncode != "0000")
                            {
                                //复位？
                                break;
                            }
                            else
                            {
                                ls_returnmsg = "TRUE";
                            }
                        }
                        else
                        {
                            ls_returnmsg = "TRUE";
                        }
                        if (ls_returnmsg == "TRUE")
                        {
                            ls_returnmsg = SqlDataTable("update Tb_Wms_Ground set States = '4' where groundguid = '" + ls_groundguid + "'");
                        }
                        break;
                    #endregion


                    #region 领取拣货任务1020
                    case "1020":
                        ReadXml(uploadDataXML);
                        ls_pickguid = oper_pickguid();
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "as_pickguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_pickguid;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1020", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ddt = new DataTable();
                        ddt = GetDataTable("select NatureCode from tb_wms_storeareanature where NatureGUID in (select NatureGUID from Tb_Wms_pick where pickguid = '" + ls_pickguid + "')");
                        ls_naturecode = ddt.Rows[0][0].ToString();
                        ddt = new DataTable();
                        ddt = GetDataTable("select count(1) from tb_wms_pick where pickguid = '" + ls_pickguid + "' and status = '5'");
                        if (Convert.ToInt32(ddt.Rows[0][0].ToString()) > 0)//非电子标签作业的情况（通知拣货就已经失败，没有亮巷道灯，后续全按PDA进行作业)
                        {
                            ls_flag = "1";
                        }
                        else
                        {
                            ls_flag = "0";
                        }
                        //自动分配复核台
                        ls_inparam = new string[1];
                        ls_inparam[0] = "as_pickguid";
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_pickguid;

                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_pickchecktable", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1020, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        else
                        {
                            //电子标签亮灯
                            if (ls_flag == "0" && ls_returnmsg == "TRUE" && (((ib_iflabel == "1" || ib_iflabel == "2") && ls_naturecode == "002") || ib_iflabel == "3"))
                            {
                                dt = GetDataTable(@"select BusinessID,LabelAddress from tb_wms_labelpickdetail where LabelpickGUID in( select LabelpickGUID from Tb_Wms_Labelpick where pickguid = '" + ls_pickguid + "') group by BusinessID,LabelAddress");
                                ddt = new DataTable();
                                ddt = GetDataTable("select ip from Tb_Wms_Control where ControlGUID in (select ControlGUID from Tb_Wms_pick where pickguid = '" + ls_pickguid + "')");
                                if (ddt.Rows.Count == 0)
                                {
                                    ls_returnmsg = "没有指定控制器IP，请检查！";
                                    break;
                                }
                                ls_controlip = ddt.Rows[0][0].ToString();
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    ddt = new DataTable();
                                    ddt = GetDataTable(@"select WarePost,WareNum from tb_wms_labelpickdetail where LabelAddress = '" + dt.Rows[i][1].ToString() + "' and LabelpickGUID in (select LabelpickGUID from tb_wms_labelpick where pickguid = '" + ls_pickguid + "')");
                                    ls_getxml = WriteXml2(ddt);
                                    //下发拣货指令，电子标签亮灯
                                    ls_returnmsg = LabelInterface(
                                                                @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                        <function>
                                                            <functionid>3002</functionid>
                                                            <wmscode></wmscode>
                                                            <orgcode>" + OrgCode + @"</orgcode>
                                                            <interfaceusername></interfaceusername>
                                                            <interfaceuserpass></interfaceuserpass>
                                                            <operusername></operusername>
                                                            <operuserpass></operuserpass>
                                                            <opertime></opertime>
                                                            <dbtype>" + ls_controlip + @"</dbtype>
                                                            <interface>MIAWMS</interface>
                                                        </function>",
                                                                @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                        <function>
                                                            <model>1</model>
                                                            <businessid>" + dt.Rows[i][0].ToString() + @"</businessid>
                                                            <labeladdress>" + dt.Rows[i][1].ToString() + @"</labeladdress>
                                                            <waresum>" + ddt.Rows.Count.ToString() + @"</waresum>
                                                            <data rowcount='" + ddt.Rows.Count.ToString() + @"' columns=""2"">
                                                                " + ls_getxml + @"
                                                            </data>
                                                        </function>",
                                                                    out ls_xml);
                                    ReadXml(ls_returnmsg);
                                    ls_returncode = oper_returncode();
                                    ls_opermsg = oper_returnmsg();
                                    ls_returnmsg = ls_opermsg;
                                    if (ls_returncode != "0000")
                                    {
                                        //复位？
                                        LabelInterface(
                                                        @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                                                    <function>
                                                                                        <functionid>3009</functionid>
                                                                                        <wmscode></wmscode>
                                                                                        <orgcode>" + OrgCode + @"</orgcode>
                                                                                        <interfaceusername></interfaceusername>
                                                                                        <interfaceuserpass></interfaceuserpass>
                                                                                        <operusername></operusername>
                                                                                        <operuserpass></operuserpass>
                                                                                        <opertime></opertime>
                                                                                        <dbtype>" + ls_controlip + @"</dbtype>
                                                                                        <interface>MIAWMS</interface>
                                                                                    </function>",
                                                            "",
                                                            out ls_xml);

                                        //下发预处理
                                        ls_returnmsg = LabelInterface(
                                                                    @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                            <function>
                                                                <functionid>3001</functionid>
                                                                <wmscode></wmscode>
                                                                <orgcode>" + OrgCode + @"</orgcode>
                                                                <interfaceusername></interfaceusername>
                                                                <interfaceuserpass></interfaceuserpass>
                                                                <operusername></operusername>
                                                                <operuserpass></operuserpass>
                                                                <opertime></opertime>
                                                                <dbtype>" + ls_controlip + @"</dbtype>
                                                                <interface>MIAWMS</interface>
                                                            </function>", "", out ls_xml);
                                        ls_returnmsg = SqlDataTable("update Tb_Wms_control set Status = '4' where controlguid = (select ControlGUID from Tb_Wms_pick where pickguid = '" + ls_pickguid + "')");
                                        break;
                                    }
                                }
                                if (ls_returncode != "0000")
                                {
                                    break;
                                }
                                //发送完成下发指令
                                ls_returnmsg = LabelInterface(
                                                            @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                            <function>
                                                                <functionid>3005</functionid>
                                                                <wmscode></wmscode>
                                                                <orgcode>" + OrgCode + @"</orgcode>
                                                                <interfaceusername></interfaceusername>
                                                                <interfaceuserpass></interfaceuserpass>
                                                                <operusername></operusername>
                                                                <operuserpass></operuserpass>
                                                                <opertime></opertime>
                                                                <dbtype>" + ls_controlip + @"</dbtype>
                                                                <interface>MIAWMS</interface>
                                                            </function>", "", out ls_xml);
                                ls_returnmsg = "TRUE";
                            }
                            else
                            {
                                if (ls_naturecode == "001")
                                {
                                    ls_returnmsg = "TRUE";
                                }
                                else
                                {
                                    ls_returnmsg = "未满足亮灯条件";
                                    break;
                                }
                            }

                            if (ls_returnmsg == "TRUE")
                            {
                                ls_returnmsg = SqlDataTable("update Tb_Wms_pick set Status = '2',getuser = '" + OperUserID + "',getdate = sysdate where pickguid = '" + ls_pickguid + "'");
                                if (ls_naturecode == "002")
                                {
                                    ls_returnmsg = SqlDataTable("update Tb_Wms_control set Status = '1' where controlguid = (select controlguid from tb_wms_pick where pickguid = '" + ls_pickguid + "')");
                                }
                            }
                            break;
                        }
                    #endregion


                    #region 领取盘点任务1028
                    case "1028":
                        ReadXml(uploadDataXML);
                        ls_inventguid = oper_inventguid();
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "as_inventguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_inventguid;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1028", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion

                        ddt = new DataTable();
                        ddt = GetDataTable("select NatureCode from tb_wms_storeareanature where natureguid in(select natureguid from tb_wms_storearea where storeareaguid in (select storeareaguid from Tb_Wms_Invent where inventguid = '" + ls_inventguid + "'))");
                        ls_naturecode = ddt.Rows[0][0].ToString();
                        ddt = new DataTable();
                        ddt = GetDataTable("select count(1) from Tb_Wms_Invent where InventGUID = '" + ls_inventguid + "' and status = '5'");
                        if (Convert.ToInt32(ddt.Rows[0][0].ToString()) > 0)//非电子标签作业的情况（通知拣货就已经失败，没有亮巷道灯，后续全按PDA进行作业)
                        {
                            ls_flag = "1";
                        }
                        else
                        {
                            ls_flag = "0";
                        }
                        ls_returnmsg = "TRUE";
                        //电子标签亮灯
                        if (ls_flag == "0" && ls_returnmsg == "TRUE" && (((ib_iflabel == "1" || ib_iflabel == "2") && ls_naturecode == "002") || ib_iflabel == "3"))
                        {
                            dt = GetDataTable(@"select BusinessID,LabelAddress from tb_wms_labelpickdetail where LabelpickGUID in( select LabelpickGUID from Tb_Wms_Labelpick where pickguid = '" + ls_inventguid + "') group by BusinessID,LabelAddress");
                            ddt = new DataTable();
                            ddt = GetDataTable("select ip from Tb_Wms_Control where ControlGUID in (select ControlGUID from Tb_Wms_invent where inventguid = '" + ls_inventguid + "')");
                            if (ddt.Rows.Count == 0)
                            {
                                ls_returnmsg = "没有指定控制器IP，请检查！";
                                break;
                            }
                            ls_controlip = ddt.Rows[0][0].ToString();
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                ddt = new DataTable();
                                ddt = GetDataTable(@"select WarePost,WareNum from tb_wms_labelpickdetail where LabelAddress = '" + dt.Rows[i][1].ToString() + "' and LabelpickGUID in (select LabelpickGUID from tb_wms_labelpick where pickguid = '" + ls_inventguid + "')");
                                ls_getxml = WriteXml2(ddt);
                                //下发盘点指令，电子标签亮灯
                                ls_returnmsg = LabelInterface(
                                                            @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                        <function>
                                                            <functionid>3003</functionid>
                                                            <wmscode></wmscode>
                                                            <orgcode>" + OrgCode + @"</orgcode>
                                                            <interfaceusername></interfaceusername>
                                                            <interfaceuserpass></interfaceuserpass>
                                                            <operusername></operusername>
                                                            <operuserpass></operuserpass>
                                                            <opertime></opertime>
                                                            <dbtype>" + ls_controlip + @"</dbtype>
                                                            <interface>MIAWMS</interface>
                                                        </function>",
                                                            @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                        <function>
                                                            <model>1</model>
                                                            <businessid>" + dt.Rows[i][0].ToString() + @"</businessid>
                                                            <labeladdress>" + dt.Rows[i][1].ToString() + @"</labeladdress>
                                                            <waresum>" + ddt.Rows.Count.ToString() + @"</waresum>
                                                            <data rowcount='" + ddt.Rows.Count.ToString() + @"' columns=""2"">
                                                                " + ls_getxml + @"
                                                            </data>
                                                        </function>",
                                                                out ls_xml);
                                ReadXml(ls_returnmsg);
                                ls_returncode = oper_returncode();
                                ls_opermsg = oper_returnmsg();
                                ls_returnmsg = ls_opermsg;
                                if (ls_returncode != "0000")
                                {
                                    //复位？
                                    LabelInterface(
                                                    @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                                                    <function>
                                                                                        <functionid>3009</functionid>
                                                                                        <wmscode></wmscode>
                                                                                        <orgcode>" + OrgCode + @"</orgcode>
                                                                                        <interfaceusername></interfaceusername>
                                                                                        <interfaceuserpass></interfaceuserpass>
                                                                                        <operusername></operusername>
                                                                                        <operuserpass></operuserpass>
                                                                                        <opertime></opertime>
                                                                                        <dbtype>" + ls_controlip + @"</dbtype>
                                                                                        <interface>MIAWMS</interface>
                                                                                    </function>",
                                                        "",
                                                        out ls_xml);
                                    //下发预处理
                                    ls_returnmsg = LabelInterface(
                                                                @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                            <function>
                                                                <functionid>3001</functionid>
                                                                <wmscode></wmscode>
                                                                <orgcode>" + OrgCode + @"</orgcode>
                                                                <interfaceusername></interfaceusername>
                                                                <interfaceuserpass></interfaceuserpass>
                                                                <operusername></operusername>
                                                                <operuserpass></operuserpass>
                                                                <opertime></opertime>
                                                                <dbtype>" + ls_controlip + @"</dbtype>
                                                                <interface>MIAWMS</interface>
                                                            </function>", "", out ls_xml);
                                    ls_returnmsg = SqlDataTable("update Tb_Wms_control set Status = '4' where controlguid = (select ControlGUID from Tb_Wms_invent where inventguid = '" + ls_inventguid + "')");

                                    break;
                                }
                                else
                                {
                                    ls_returnmsg = "TRUE";
                                }
                            }

                            if (ls_returncode != "0000")
                            {
                                break;
                            }
                            //发送完成下发指令
                            ls_returnmsg = LabelInterface(
                                                        @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                            <function>
                                                                <functionid>3005</functionid>
                                                                <wmscode></wmscode>
                                                                <orgcode>" + OrgCode + @"</orgcode>
                                                                <interfaceusername></interfaceusername>
                                                                <interfaceuserpass></interfaceuserpass>
                                                                <operusername></operusername>
                                                                <operuserpass></operuserpass>
                                                                <opertime></opertime>
                                                                <dbtype>" + ls_controlip + @"</dbtype>
                                                                <interface>MIAWMS</interface>
                                                            </function>", "", out ls_xml);
                            ls_returnmsg = "TRUE";
                        }
                        else
                        {
                            if (ls_naturecode == "001")
                            {
                                ls_returnmsg = "TRUE";
                            }
                            else
                            {
                                ls_returnmsg = "未满足亮灯条件";
                                break;
                            }
                        }
                        if (ls_returnmsg == "TRUE")
                        {
                            ls_inparam = new string[2];
                            ls_inparam[0] = "as_inventguid";
                            ls_inparam[1] = "as_operuser";
                            ls_inparamvalue = new string[2];
                            ls_inparamvalue[0] = ls_inventguid;
                            ls_inparamvalue[1] = OperUserID;
                            ls_inparamtype = new string[2];
                            ls_inparamtype[0] = "varchar";
                            ls_inparamtype[1] = "varchar";
                            ls_outparam = new string[2];
                            ls_outparam[0] = "as_returncode";
                            ls_outparam[1] = "as_returnmsg";
                            ls_outparamtype = new string[2];
                            ls_outparamtype[0] = "varchar";
                            ls_outparamtype[1] = "varchar";
                            ls_returnmsg = Doprocedure("miawms.sp_inventconfirm", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1028, true);
                            if (ls_returnmsg != "TRUE")
                            {
                                break;
                            }
                            if (ls_naturecode == "002")
                            {
                                ls_returnmsg = SqlDataTable("update Tb_Wms_control set Status = '2' where controlguid = (select distinct ControlGUID from Tb_Wms_LabelPick where pickguid = '" + ls_inventguid + "')");
                            }
                        }
                        break;
                    #endregion


                    #region 领取复核任务1034
                    case "1034":
                        ReadXml(uploadDataXML);
                        ls_confirmguid = oper_confirmguid();
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "confirmguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_confirmguid;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1034", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_returnmsg = SqlDataTable(@"update tb_wms_pickconfirm set Status = '2',getuser = '" + OperUserID + @"',getdate = sysdate where status <> '8' and                                                   pickguid in(select pickguid
                                                      from tb_wms_pick
                                                     where outguid = (select outguid
                                                                        from tb_wms_pick
                                                                       where pickguid = (Select pickguid
                                                                                           from tb_wms_pickconfirm
                                                                                          where confirmguid = '" + ls_confirmguid + @"'))
                                                       and storeareaguid =
                                                           (select storeareaguid
                                                              from tb_wms_pick
                                                             where pickguid = (Select pickguid
                                                                                 from tb_wms_pickconfirm
                                                                                where confirmguid = '" + ls_confirmguid + "')))");
                        break;
                    #endregion


                    #region 零货零散上架查询1009
                    case "1009":
                        string ls_transportboxguid = "";
                        ReadXml(uploadDataXML);
                        ls_boxbar = oper_boxbar();
                        #region 效验
                        ls_inparam = new string[2];
                        ls_inparam[0] = "as_boxbar";
                        ls_inparam[1] = "as_usercode";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = ls_boxbar;
                        ls_inparamvalue[1] = OperUserID;
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1009", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[1];
                        ls_inparam[0] = "as_boxbar";
                        //ls_inparam[1] = "as_usercode";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_boxbar;
                        //ls_inparamvalue[1] = OperUserID;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        //ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";

                        dt = new DataTable();
                        dt = GetDataTable(@"select count(1) from Tb_Wms_TransportBoxWare where transportboxguid in(select transportboxguid from tb_wms_transportbox where bar = '" + ls_boxbar + "' union all select WarePackGUID from Tb_Wms_Warepack where Bar = '" + ls_boxbar + "' and status = '0') and status = '1'");
                        if (Convert.ToInt32(dt.Rows[0][0].ToString()) > 0)
                        {
                            ls_returnmsg = "不需要上架的周转箱！";
                            break;
                        }

                        dt = new DataTable();
                        dt = GetDataTable(@"select transportboxguid from tb_wms_transportbox where bar = '" + ls_boxbar + "' union all select WarePackGUID from Tb_Wms_Warepack where bar = '" + ls_boxbar + "' and status = '0'");
                        ls_transportboxguid = dt.Rows[0][0].ToString();

                        dt = new DataTable();
                        dt = GetDataTable(@"select groundguid,locationguid from Tb_Wms_BoxGroundDetail where TransportBoxGUID = '" + ls_transportboxguid + "' and ConfirmFlag = '0'");
                        if (dt.Rows[0][0].ToString() == "")
                        {
                            ls_returnmsg = "无法识别的周转箱，请检查！";
                            break;
                        }
                        ls_groundguid = dt.Rows[0][0].ToString();
                        ls_locationguid = dt.Rows[0][1].ToString();
                        ls_returnmsg = "TRUE";
                        //判断前一个箱是否上架确认
                        //如果任务第一个箱上架，进行处理
                        dt = new DataTable();
                        dt = GetDataTable(@"select count(1) from Tb_Wms_Ground where groundguid = '" + ls_groundguid + "' and States = '4'");
                        if (Convert.ToInt32(dt.Rows[0][0].ToString()) == 0)
                        {
                            ls_returnmsg = SqlDataTable("update Tb_Wms_Ground set States = '4' where groundguid = '" + ls_groundguid + "'");
                            //生成上架确认单
                            ls_confirmguid = getguid();
                            ls_returnmsg = SqlDataTable(@"insert into TB_WMS_GROUNDCONFIRM
                                                        (CONFIRMGUID,
                                                         gROUNDGUID,
                                                         STOREAREAGUID,
                                                         COMPANYGUID,
                                                         OPERUSER,
                                                         OPERDATE,
                                                         REMARK,
                                                         status)
                                                      values
                                                        ('" + ls_confirmguid + @"',
                                                         '" + ls_groundguid + @"',
                                                         (select storeareaguid
                                                            from Tb_Wms_Ground
                                                           where groundguid = '" + ls_groundguid + @"'),
                                                         (select companyguid
                                                            from tb_wms_ground
                                                           where groundguid = '" + ls_groundguid + @"'),
                                                         '" + OperUserID + @"',
                                                         sysdate,
                                                         null,
                                                         '0')");
                            ls_returnmsg = SqlDataTable(@"update Tb_Wms_LabelGroundDetail
                                                     set ConfirmGUID = '" + ls_confirmguid + @"'
                                                   where labelgroundguid in
                                                         (select labelgroundguid
                                                            from tb_wms_labelground
                                                           where groundguid =  '" + ls_groundguid + @"')");
                            ls_returnmsg = SqlDataTable(@"update Tb_Wms_BoxGroundDetail
                                                     set ConfirmGUID = '" + ls_confirmguid + @"'
                                                   where GroundGUID =  '" + ls_groundguid + @"'");
                        }
                        //巷道灯亮
                        //电子标签亮灯
                        //ls_returnmsg = "FALSE";
                        if (ls_returnmsg == "TRUE" && ((ib_iflabel == "2") || ib_iflabel == "3"))
                        {
                            ddt = new DataTable();
                            ddt = GetDataTable("select ip from Tb_Wms_Control where ControlGUID in (select ControlGUID from Tb_Wms_Ground where groundguid = '" + ls_groundguid + "')");
                            if (ddt.Rows.Count == 0)
                            {
                                ls_returnmsg = "没有指定控制器IP，不能下发，请检查！";
                                break;
                            }
                            ls_controlip = ddt.Rows[0][0].ToString();

                            dt = GetDataTable(@"select BusinessID2,LabelAddress as LabelAddress from tb_wms_labelgrounddetail where labelgroundguid in( select labelgroundguid from Tb_Wms_LabelGround where groundguid = '" + ls_groundguid + "') and locationguid = '" + ls_locationguid + "' and transportboxguid = '" + ls_transportboxguid + "'");
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                ddt = GetDataTable(@"select WarePost,WareNum as WareNum from tb_wms_labelgrounddetail where LabelAddress = '" + dt.Rows[i][1].ToString() + "' and LabelGroundGUID in(select LabelGroundGUID from Tb_Wms_LabelGround where GroundGUID = '" + ls_groundguid + "')  and locationguid = '" + ls_locationguid + "' and transportboxguid = '" + ls_transportboxguid + "'");
                                ls_getxml = WriteXml2(ddt);
                                //下发补货指令，电子标签亮灯
                                ls_returnmsg = LabelInterface(
                                                            @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                        <function>
                                                            <functionid>3004</functionid>
                                                            <wmscode></wmscode>
                                                            <orgcode>" + OrgCode + @"</orgcode>
                                                            <interfaceusername></interfaceusername>
                                                            <interfaceuserpass></interfaceuserpass>
                                                            <operusername></operusername>
                                                            <operuserpass></operuserpass>
                                                            <opertime></opertime>
                                                            <dbtype>" + ls_controlip + @"</dbtype>
                                                            <interface>MIAWMS</interface>
                                                        </function>",
                                                            @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                        <function>
                                                            <model>1</model>
                                                            <businessid>" + dt.Rows[i][0].ToString() + @"</businessid>
                                                            <labeladdress>" + dt.Rows[i][1].ToString() + @"</labeladdress>
                                                            <waresum>" + ddt.Rows.Count.ToString() + @"</waresum>
                                                            <data rowcount='" + ddt.Rows.Count.ToString() + @"' columns=""2"">
                                                                " + ls_getxml + @"
                                                            </data>
                                                        </function>",
                                                                out ls_xml);
                                ReadXml(ls_returnmsg);
                                ls_returncode = oper_returncode();
                                ls_opermsg = oper_returnmsg();
                                ls_returnmsg = ls_opermsg;
                                if (ls_returncode != "0000")
                                {
                                    //复位？
                                    break;
                                }
                            }
                            if (ls_returncode != "0000")
                            {
                                break;
                            }
                            //发送完成下发指令
                            ls_returnmsg = LabelInterface(
                                                        @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                            <function>
                                                                <functionid>3005</functionid>
                                                                <wmscode></wmscode>
                                                                <orgcode>" + OrgCode + @"</orgcode>
                                                                <interfaceusername></interfaceusername>
                                                                <interfaceuserpass></interfaceuserpass>
                                                                <operusername></operusername>
                                                                <operuserpass></operuserpass>
                                                                <opertime></opertime>
                                                                <dbtype>" + ls_controlip + @"</dbtype>
                                                                <interface>MIAWMS</interface>
                                                            </function>", "", out ls_xml);
                            ReadXml(ls_returnmsg);
                            ls_returncode = oper_returncode();
                            ls_opermsg = oper_returnmsg();
                            ls_returnmsg = ls_opermsg;
                            if (ls_returncode != "0000")
                            {
                                //复位？
                                break;
                            }
                            else
                            {
                                ls_returnmsg = "TRUE";
                            }
                        }
                        else
                        {
                            ls_returnmsg = "TRUE";
                        }
                        //电子标签上报后，在service端依次执行存储过程sp_groundconfirm3、sp_completeground
                        ls_returnmsg = Doprocedure("miawms.sp_transportboxground", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1009, true);
                        if (ls_returnmsg == "TRUE")
                        {
                            ls_returnmsg = SqlDataTable("update Tb_Wms_control set Status = '3' where controlguid = (select ControlGUID from Tb_Wms_Ground where groundguid = '" + ls_groundguid + "')");
                            ls_returnmsg = SqlDataTable("update Tb_Wms_label set Status = '1' where labelguid = (select labelguid from tb_wms_location where locationguid =(select LocationGUID from Tb_Wms_BoxGroundDetail where TransportBoxGUID = (select TransportBoxGUID from Tb_Wms_transportbox where bar = '" + ls_boxbar + "') and groundguid = '" + ls_groundguid + "'))");
                        }
                        break;
                    #endregion


                    #region 整件零散上架查询1010
                    case "1010":
                        ReadXml(uploadDataXML);
                        ls_traybar = oper_traybar();
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "as_traybar";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_traybar;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1010", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[2];
                        ls_inparam[0] = "as_traybar";
                        ls_inparam[1] = "as_usercode";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = ls_traybar;
                        ls_inparamvalue[1] = OperUserID;
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";

                        dt = new DataTable();
                        dt = GetDataTable(@"select count(1) from Tb_Wms_Tray where status = '0' and TrayGUID =  (select TrayGUID from Tb_Wms_Tray where bar = '" + ls_traybar + "')");
                        if (Convert.ToInt32(dt.Rows[0][0].ToString()) > 0)
                        {
                            ls_returnmsg = "不需要上架的托盘！";
                            break;
                        }

                        dt = new DataTable();
                        dt = GetDataTable(@"select groundguid from Tb_Wms_PackGroundDetail where LocationGroundTrayGUID = (select LocationGroundTrayGUID from Tb_Wms_LocationGroundTray where TrayGUID =  (select TrayGUID from Tb_Wms_Tray where bar = '" + ls_traybar + "'))");
                        if (dt.Rows.Count == 0)
                        {
                            ls_returnmsg = "未分配货位，不能上架！";
                            break;
                        }
                        ls_groundguid = dt.Rows[0][0].ToString();

                        dt = new DataTable();
                        dt = GetDataTable(@"select count(1) from Tb_Wms_LocationGroundTray where States = '1' and LocationGroundTrayGUID in(select LocationGroundTrayGUID from Tb_Wms_PackGroundDetail where GroundGUID = '" + ls_groundguid + "')");
                        if (Convert.ToInt32(dt.Rows[0][0]) == 0)//任务第一个托盘上架
                        {
                            //领取任务
                            //亮灯可能存在
                            ls_returnmsg = SqlDataTable("update Tb_Wms_Ground set States = '4' where groundguid = '" + ls_groundguid + "' and states = '3'");
                        }
                        ls_returnmsg = Doprocedure("miawms.sp_trayground", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1010, true);
                        break;
                    #endregion


                    #region 完成零货零散上架1011
                    case "1011":
                        //上架任务已经领取的情况下，不能进行该操作，提示
                        string ls_boxguid = "";
                        ReadXml(uploadDataXML);
                        ls_boxbar = oper_boxbar();
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "boxbar";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_boxbar;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1011", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        dt = new DataTable();
                        dt = GetDataTable("select transportboxguid from tb_wms_transportbox where bar = '" + ls_boxbar + "'");
                        ls_boxguid = dt.Rows[0][0].ToString();

                        dt = new DataTable();
                        dt = GetDataTable("select count(1) from Tb_Wms_TransportBoxWare where TransportBoxGUID = '" + ls_boxguid + "' and  Status = gettransboxstatus('未上架')");
                        if (Convert.ToInt32(dt.Rows[0][0].ToString()) > 0)
                        {

                        }
                        else
                        {
                            ls_returnmsg = "该箱已为上架状态，不能重复上架！";
                            break;
                        }

                        dt = new DataTable();
                        dt = GetDataTable("select groundguid from Tb_Wms_BoxGroundDetail where TransportBoxGUID = '" + ls_boxguid + "' and ConfirmFlag = '0'");
                        ls_groundguid = dt.Rows[0][0].ToString();
                        ls_inparam = new string[3];
                        ls_inparam[0] = "as_groundguid";
                        ls_inparam[1] = "as_boxguid";
                        ls_inparam[2] = "as_operuser";
                        ls_inparamtype = new string[3];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_inparamvalue = new string[3];
                        ls_inparamvalue[0] = ls_groundguid;
                        ls_inparamvalue[1] = ls_boxguid;
                        ls_inparamvalue[2] = OperUserID;

                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = "TRUE";
                        ls_returnmsg = Doprocedure("miawms.sp_boxconfirm", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1011, true);
                        break;
                    #endregion


                    #region 完成整件零散上架1012
                    case "1012":
                        ReadXml(uploadDataXML);
                        ls_traybar = oper_traybar();
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "traybar";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_traybar;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1012", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion

                        dt = new DataTable();
                        dt = GetDataTable("select groundguid from Tb_Wms_PackGroundDetail where LocationGroundTrayGUID =(select LocationGroundTrayGUID from Tb_Wms_LocationGroundTray where TrayGUID in (select TrayGUID from tb_wms_tray where bar = '" + ls_traybar + "') and states = '0')");
                        ls_groundguid = dt.Rows[0][0].ToString();
                        dt = new DataTable();
                        dt = GetDataTable("select trayguid from Tb_Wms_tray where bar = '" + ls_traybar + "'");
                        ls_trayguid = dt.Rows[0][0].ToString();

                        dt = new DataTable();
                        dt = GetDataTable("select count(1) from Tb_Wms_LocationGroundTray where trayguid = '" + ls_trayguid + "' and  States = gettraystatus('待上架')");
                        if (Convert.ToInt32(dt.Rows[0][0].ToString()) > 0)
                        {

                        }
                        else
                        {
                            ls_returnmsg = "该托盘已为上架状态，不能重复上架！";
                            break;
                        }

                        ls_inparam = new string[3];
                        ls_inparam[0] = "as_groundguid";
                        ls_inparam[1] = "as_trayguid";
                        ls_inparam[2] = "as_operuser";
                        ls_inparamtype = new string[3];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_inparamvalue = new string[3];
                        ls_inparamvalue[0] = ls_groundguid;
                        ls_inparamvalue[1] = ls_trayguid;
                        ls_inparamvalue[2] = OperUserID;

                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = "TRUE";
                        ls_returnmsg = Doprocedure("miawms.sp_trayconfirm", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1012, true);
                        break;
                    #endregion


                    #region 根据任务获取上架货架列表1013
                    case "1013":
                        ReadXml(uploadDataXML);
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "groundguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = oper_groundguid();
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1013", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[1];
                        ls_inparam[0] = "as_groundguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = oper_groundguid();
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_getshelve", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1013, true);
                        break;
                    #endregion


                    #region 根据任务获取拣货货架列表1022
                    case "1022":
                        ReadXml(uploadDataXML);
                        ls_picktype = oper_picktype();
                        #region 效验
                        ls_inparam = new string[2];
                        ls_inparam[0] = "as_pickguid";
                        ls_inparam[1] = "as_picktype";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = oper_pickguid();
                        ls_inparamvalue[1] = ls_picktype;
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1022", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        if (ls_picktype == "0")
                        {
                            ls_inparam = new string[1];
                            ls_inparam[0] = "as_pickguid";
                            ls_inparamvalue = new string[1];
                            ls_inparamvalue[0] = oper_pickguid();
                            ls_inparamtype = new string[1];
                            ls_inparamtype[0] = "varchar";
                            ls_outparam = new string[3];
                            ls_outparam[0] = "cur_Jobs";
                            ls_outparam[1] = "as_returncode";
                            ls_outparam[2] = "as_returnmsg";
                            ls_outparamtype = new string[3];
                            ls_outparamtype[0] = "cursor";
                            ls_outparamtype[1] = "varchar";
                            ls_outparamtype[2] = "varchar";
                            ls_returnmsg = Doprocedure("miawms.sp_getpickshelve", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1022, true);
                        }
                        else if (ls_picktype == "1")
                        {
                            ls_inparam = new string[1];
                            ls_inparam[0] = "as_inventguid";
                            ls_inparamvalue = new string[1];
                            ls_inparamvalue[0] = oper_pickguid();
                            ls_inparamtype = new string[1];
                            ls_inparamtype[0] = "varchar";
                            ls_outparam = new string[3];
                            ls_outparam[0] = "cur_Jobs";
                            ls_outparam[1] = "as_returncode";
                            ls_outparam[2] = "as_returnmsg";
                            ls_outparamtype = new string[3];
                            ls_outparamtype[0] = "cursor";
                            ls_outparamtype[1] = "varchar";
                            ls_outparamtype[2] = "varchar";
                            ls_returnmsg = Doprocedure("miawms.sp_getinventshelve", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1022, true);
                        }
                        break;
                    #endregion


                    #region 根据货架获取单品信息列表1023
                    case "1023":
                        ReadXml(uploadDataXML);
                        ls_picktype = oper_picktype();
                        #region 效验
                        ls_inparam = new string[3];
                        ls_inparam[0] = "pickguid";
                        ls_inparam[1] = "shelveguid";
                        ls_inparam[2] = "picktype";
                        ls_inparamvalue = new string[3];
                        ls_inparamvalue[0] = oper_pickguid();
                        ls_inparamvalue[1] = oper_shelveguid();
                        ls_inparamvalue[2] = ls_picktype;
                        ls_inparamtype = new string[3];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1023", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        if (ls_picktype == "0")
                        {
                            ls_inparam = new string[2];
                            ls_inparam[0] = "as_pickguid";
                            ls_inparam[1] = "as_shelveguid";
                            ls_inparamvalue = new string[2];
                            ls_inparamvalue[0] = oper_pickguid();
                            ls_inparamvalue[1] = oper_shelveguid();
                            ls_inparamtype = new string[2];
                            ls_inparamtype[0] = "varchar";
                            ls_inparamtype[1] = "varchar";
                            ls_outparam = new string[3];
                            ls_outparam[0] = "cur_Jobs";
                            ls_outparam[1] = "as_returncode";
                            ls_outparam[2] = "as_returnmsg";
                            ls_outparamtype = new string[3];
                            ls_outparamtype[0] = "cursor";
                            ls_outparamtype[1] = "varchar";
                            ls_outparamtype[2] = "varchar";
                            ls_returnmsg = Doprocedure("miawms.sp_getwareinfo", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1023, true);
                        }
                        else if (ls_picktype == "1")
                        {
                            ls_inparam = new string[2];
                            ls_inparam[0] = "as_inventguid";
                            ls_inparam[1] = "as_shelveguid";
                            ls_inparamvalue = new string[2];
                            ls_inparamvalue[0] = oper_pickguid();
                            ls_inparamvalue[1] = oper_shelveguid();
                            ls_inparamtype = new string[2];
                            ls_inparamtype[0] = "varchar";
                            ls_inparamtype[1] = "varchar";
                            ls_outparam = new string[3];
                            ls_outparam[0] = "cur_Jobs";
                            ls_outparam[1] = "as_returncode";
                            ls_outparam[2] = "as_returnmsg";
                            ls_outparamtype = new string[3];
                            ls_outparamtype[0] = "cursor";
                            ls_outparamtype[1] = "varchar";
                            ls_outparamtype[2] = "varchar";
                            ls_returnmsg = Doprocedure("miawms.sp_getwareinfo2", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1023, true);
                        }
                        break;
                    #endregion


                    #region 根据货架获取托盘信息列表1024
                    case "1024":
                        ReadXml(uploadDataXML);
                        ls_picktype = oper_picktype();
                        #region 效验
                        ls_inparam = new string[3];
                        ls_inparam[0] = "pickguid";
                        ls_inparam[1] = "shelveguid";
                        ls_inparam[2] = "picktype";
                        ls_inparamvalue = new string[3];
                        ls_inparamvalue[0] = oper_pickguid();
                        ls_inparamvalue[1] = oper_shelveguid();
                        ls_inparamvalue[2] = ls_picktype;
                        ls_inparamtype = new string[3];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1024", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        if (ls_picktype == "0")
                        {
                            ls_inparam = new string[2];
                            ls_inparam[0] = "as_pickguid";
                            ls_inparam[1] = "as_shelveguid";
                            ls_inparamvalue = new string[2];
                            ls_inparamvalue[0] = oper_pickguid();
                            ls_inparamvalue[1] = oper_shelveguid();
                            ls_inparamtype = new string[2];
                            ls_inparamtype[0] = "varchar";
                            ls_inparamtype[1] = "varchar";
                            ls_outparam = new string[3];
                            ls_outparam[0] = "cur_Jobs";
                            ls_outparam[1] = "as_returncode";
                            ls_outparam[2] = "as_returnmsg";
                            ls_outparamtype = new string[3];
                            ls_outparamtype[0] = "cursor";
                            ls_outparamtype[1] = "varchar";
                            ls_outparamtype[2] = "varchar";
                            ls_returnmsg = Doprocedure("miawms.sp_getwareinfo3", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1024, true);
                        }
                        else if (ls_picktype == "1")
                        {
                            ls_inparam = new string[2];
                            ls_inparam[0] = "as_inventguid";
                            ls_inparam[1] = "as_shelveguid";
                            ls_inparamvalue = new string[2];
                            ls_inparamvalue[0] = oper_pickguid();
                            ls_inparamvalue[1] = oper_shelveguid();
                            ls_inparamtype = new string[2];
                            ls_inparamtype[0] = "varchar";
                            ls_inparamtype[1] = "varchar";
                            ls_outparam = new string[3];
                            ls_outparam[0] = "cur_Jobs";
                            ls_outparam[1] = "as_returncode";
                            ls_outparam[2] = "as_returnmsg";
                            ls_outparamtype = new string[3];
                            ls_outparamtype[0] = "cursor";
                            ls_outparamtype[1] = "varchar";
                            ls_outparamtype[2] = "varchar";
                            ls_returnmsg = Doprocedure("miawms.sp_getwareinfo6", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1024, true);
                        }
                        break;
                    #endregion


                    #region 根据托盘获取商品件信息列表1033
                    case "1033":
                        ReadXml(uploadDataXML);
                        ls_picktype = oper_picktype();
                        #region 效验
                        ls_inparam = new string[3];
                        ls_inparam[0] = "pickguid";
                        ls_inparam[1] = "trayguid";
                        ls_inparam[2] = "picktype";
                        ls_inparamvalue = new string[3];
                        ls_inparamvalue[0] = oper_pickguid();
                        ls_inparamvalue[1] = oper_trayguid();
                        ls_inparamvalue[2] = ls_picktype;
                        ls_inparamtype = new string[3];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1033", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        if (ls_picktype == "0")
                        {
                            ls_inparam = new string[2];
                            ls_inparam[0] = "as_pickguid";
                            ls_inparam[1] = "as_trayguid";
                            ls_inparamvalue = new string[2];
                            ls_inparamvalue[0] = oper_pickguid();
                            ls_inparamvalue[1] = oper_trayguid();
                            ls_inparamtype = new string[2];
                            ls_inparamtype[0] = "varchar";
                            ls_inparamtype[1] = "varchar";
                            ls_outparam = new string[3];
                            ls_outparam[0] = "cur_Jobs";
                            ls_outparam[1] = "as_returncode";
                            ls_outparam[2] = "as_returnmsg";
                            ls_outparamtype = new string[3];
                            ls_outparamtype[0] = "cursor";
                            ls_outparamtype[1] = "varchar";
                            ls_outparamtype[2] = "varchar";
                            ls_returnmsg = Doprocedure("miawms.sp_getwareinfo4", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1033, true);
                        }
                        else if (ls_picktype == "1")
                        {
                            ls_inparam = new string[2];
                            ls_inparam[0] = "as_inventguid";
                            ls_inparam[1] = "as_trayguid";
                            ls_inparamvalue = new string[2];
                            ls_inparamvalue[0] = oper_pickguid();
                            ls_inparamvalue[1] = oper_trayguid();
                            ls_inparamtype = new string[2];
                            ls_inparamtype[0] = "varchar";
                            ls_inparamtype[1] = "varchar";
                            ls_outparam = new string[3];
                            ls_outparam[0] = "cur_Jobs";
                            ls_outparam[1] = "as_returncode";
                            ls_outparam[2] = "as_returnmsg";
                            ls_outparamtype = new string[3];
                            ls_outparamtype[0] = "cursor";
                            ls_outparamtype[1] = "varchar";
                            ls_outparamtype[2] = "varchar";
                            ls_returnmsg = Doprocedure("miawms.sp_getwareinfo7", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1033, true);
                        }
                        break;
                    #endregion


                    #region 周转箱任务绑定1025
                    case "1025":
                        ReadXml(uploadDataXML);
                        ls_boxbar = oper_boxbar();
                        ls_pickguid = oper_pickguid();
                        #region 效验
                        ls_inparam = new string[2];
                        ls_inparam[0] = "pickguid";
                        ls_inparam[1] = "boxbar";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = ls_pickguid;
                        ls_inparamvalue[1] = ls_boxbar;
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1025", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion

                        dt = new DataTable();
                        dt = GetDataTable(@"select count(1) from Tb_Wms_PickBox where PickGUID = '" + ls_pickguid + "' and TransportBoxGUID = (select TransportBoxGUID from Tb_Wms_TransportBox where bar = '" + ls_boxbar + "')");
                        ll_cnt = Convert.ToInt32(dt.Rows[0][0].ToString());

                        if (ll_cnt > 0)
                        {
                            ls_returnmsg = "已绑定拣货箱，不能重复操作！";
                            break;
                        }
                        else
                        {

                        }

                        ls_returnmsg = boxwork(ls_boxbar, ls_pickguid);
                        break;
                    #endregion


                    #region 商品件任务绑定1031
                    case "1031":
                        ReadXml(uploadDataXML);
                        ls_boxbar = oper_warepackbar();
                        ls_pickguid = oper_pickguid();
                        ls_locationguid = oper_locationguid();
                        #region 效验
                        ls_inparam = new string[3];
                        ls_inparam[0] = "as_pickguid";
                        ls_inparam[1] = "as_warepackbar";
                        ls_inparam[2] = "as_locationguid";
                        ls_inparamvalue = new string[3];
                        ls_inparamvalue[0] = ls_pickguid;
                        ls_inparamvalue[1] = ls_boxbar;
                        ls_inparamvalue[2] = ls_locationguid;
                        ls_inparamtype = new string[3];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1031", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        dt = new DataTable();
                        dt = GetDataTable(@"select count(1) from Tb_Wms_PickWarePackDetail where PickGUID = '" + ls_pickguid + "' and WarePackBar =  '" + ls_boxbar + "'");
                        ll_cnt = Convert.ToInt32(dt.Rows[0][0].ToString());

                        if (ll_cnt > 0)
                        {
                            ls_returnmsg = "已绑定此商品件，不能重复操作！";
                            break;
                        }
                        else
                        {

                        }
                        ls_returnmsg = warepackwork(ls_boxbar, ls_pickguid, ls_locationguid);
                        break;
                    #endregion


                    #region 配送箱单品绑定1041
                    case "1041":
                        ReadXml(uploadDataXML);
                        ls_confirmguid = oper_confirmguid();
                        ls_boxbar = oper_boxbar();
                        #region 效验
                        ls_inparam = new string[2];
                        ls_inparam[0] = "confirmguid";
                        ls_inparam[1] = "boxbar";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = ls_confirmguid;
                        ls_inparamvalue[1] = ls_boxbar;
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1041", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        dt = GetAllColumnsData();
                        ls_returnmsg = deliverbox(ls_confirmguid, ls_boxbar, dt);
                        break;
                    #endregion


                    #region 取消商品件任务绑定1032
                    case "1032":
                        ReadXml(uploadDataXML);
                        ls_boxbar = oper_warepackbar();
                        ls_pickguid = oper_pickguid();
                        #region 效验
                        ls_inparam = new string[3];
                        ls_inparam[0] = "as_pickguid";
                        ls_inparam[1] = "as_warepackbar";
                        ls_inparam[2] = "as_locationguid";
                        ls_inparamvalue = new string[3];
                        ls_inparamvalue[0] = ls_pickguid;
                        ls_inparamvalue[1] = ls_boxbar;
                        ls_inparamvalue[2] = oper_locationguid();
                        ls_inparamtype = new string[3];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1032", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_returnmsg = cancelwarepackwork(ls_boxbar, ls_pickguid, oper_locationguid());
                        break;
                    #endregion


                    #region 取消周转箱任务绑定1030
                    case "1030":
                        ReadXml(uploadDataXML);
                        ls_boxbar = oper_boxbar();
                        ls_pickguid = oper_pickguid();
                        #region 效验
                        ls_inparam = new string[2];
                        ls_inparam[0] = "pickguid";
                        ls_inparam[1] = "boxbar";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = ls_pickguid;
                        ls_inparamvalue[1] = ls_boxbar;
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1030", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_returnmsg = cancelboxwork(ls_boxbar, ls_pickguid);
                        break;
                    #endregion


                    #region 根据任务获取盘点货架列表1026
                    case "1026":
                        ReadXml(uploadDataXML);
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "inventguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = oper_inventguid();
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1026", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[1];
                        ls_inparam[0] = "as_inventguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = oper_inventguid();
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_getinventshelve", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1026, true);
                        break;
                    #endregion


                    #region 根据货架获取物品信息列表1027
                    case "1027":
                        ReadXml(uploadDataXML);
                        #region 效验
                        ls_inparam = new string[2];
                        ls_inparam[0] = "inventguid";
                        ls_inparam[1] = "shelveguid";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = oper_inventguid();
                        ls_inparamvalue[1] = oper_shelveguid();
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1027", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[2];
                        ls_inparam[0] = "as_inventguid";
                        ls_inparam[1] = "as_shelveguid";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = oper_inventguid();
                        ls_inparamvalue[1] = oper_shelveguid();
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_getwareinfo2", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1027, true);
                        break;
                    #endregion


                    #region 根据货架获取周转箱、托盘列表1014
                    case "1014":
                        ReadXml(uploadDataXML);
                        #region 效验
                        ls_inparam = new string[2];
                        ls_inparam[0] = "groundguid";
                        ls_inparam[1] = "shelveguid";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = oper_groundguid();
                        ls_inparamvalue[1] = oper_shelveguid();
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1014", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[2];
                        ls_inparam[0] = "as_groundguid";
                        ls_inparam[1] = "as_shelveguid";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = oper_groundguid();
                        ls_inparamvalue[1] = oper_shelveguid();
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_getboxtray", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1014, true);
                        break;
                    #endregion


                    #region 根据周转箱获取物品列表1015
                    case "1015":
                        ReadXml(uploadDataXML);
                        #region 效验
                        ls_inparam = new string[2];
                        ls_inparam[0] = "groundguid";
                        ls_inparam[1] = "boxguid";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = oper_groundguid();
                        ls_inparamvalue[1] = oper_boxguid();
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1015", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[2];
                        ls_inparam[0] = "as_groundguid";
                        ls_inparam[1] = "as_boxguid";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = oper_groundguid();
                        ls_inparamvalue[1] = oper_boxguid();
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_getboxware", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1015, true);
                        break;
                    #endregion


                    #region 单箱、托盘完成上架1016
                    case "1016":
                        ReadXml(uploadDataXML);
                        ls_groundguid = oper_groundguid();
                        ls_guid = oper_guid();
                        #region 效验
                        ls_inparam = new string[2];
                        ls_inparam[0] = "as_groundguid";
                        ls_inparam[1] = "as_guid";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = ls_groundguid;
                        ls_inparamvalue[1] = ls_guid;
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1016", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        dt = GetAllColumnsData();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            ls_locationguid = dt.Rows[i]["locationguid"].ToString();
                            ls_groundnum = dt.Rows[i]["realnum"].ToString();

                            ddt = new DataTable();
                            ddt = GetDataTable(@"select count(1) from Tb_Wms_LocationGroundTray where trayguid = '" + ls_guid + "' and States = gettraystatus('待上架')");
                            ll_cnt = Convert.ToInt32(ddt.Rows[0][0].ToString());


                            ddt = new DataTable();
                            ddt = GetDataTable(@"select count(1) from (select 1 from Tb_Wms_TransportBoxWare where TransportBoxGUID = '" + ls_guid + "' and Status = gettransboxstatus('未上架') union all select 1 from tb_wms_warepack where warepackguid = '" + ls_guid + "')");
                            ll_cnt += Convert.ToInt32(ddt.Rows[0][0].ToString());

                            if (ll_cnt > 0)
                            {

                            }
                            else
                            {
                                ls_returnmsg = "不存在待上架记录，不能进行上架操作！";
                                break;
                            }

                            ls_inparam = new string[2];
                            ls_inparam[0] = "as_groundguid";
                            ls_inparam[1] = "as_operuser";
                            ls_inparamtype = new string[2];
                            ls_inparamtype[0] = "varchar";
                            ls_inparamtype[1] = "varchar";
                            ls_inparamvalue = new string[4];
                            ls_inparamvalue[0] = ls_groundguid;
                            ls_inparamvalue[1] = OperUserID;
                            ls_outparam = new string[2];
                            ls_outparam[0] = "as_returncode";
                            ls_outparam[1] = "as_returnmsg";
                            ls_outparamtype = new string[2];
                            ls_outparamtype[0] = "varchar";
                            ls_outparamtype[1] = "varchar";
                            ls_returnmsg = Doprocedure("miawms.sp_updategroundinfo", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1016, true);//零货库不执行
                            if (ls_returnmsg != "TRUE")
                            {
                                break;
                            }

                            //执行存储过程（增加库存、设置状态等）
                            ls_inparam = new string[5];
                            ls_inparam[0] = "as_groundguid";
                            ls_inparam[1] = "as_locationguid";
                            ls_inparam[2] = "as_guid";
                            ls_inparam[3] = "as_confirmnum";
                            ls_inparam[4] = "as_operuser";
                            ls_inparamtype = new string[5];
                            ls_inparamtype[0] = "varchar";
                            ls_inparamtype[1] = "varchar";
                            ls_inparamtype[2] = "varchar";
                            ls_inparamtype[3] = "int";
                            ls_inparamtype[4] = "varchar";
                            ls_inparamvalue = new string[5];
                            ls_inparamvalue[0] = ls_groundguid;
                            ls_inparamvalue[1] = ls_locationguid;
                            ls_inparamvalue[2] = ls_guid;
                            ls_inparamvalue[3] = ls_groundnum;
                            ls_inparamvalue[4] = OperUserID;

                            ls_outparam = new string[2];
                            ls_outparam[0] = "as_returncode";
                            ls_outparam[1] = "as_returnmsg";
                            ls_outparamtype = new string[2];
                            ls_outparamtype[0] = "varchar";
                            ls_outparamtype[1] = "varchar";
                            ls_returnmsg = Doprocedure("miawms.sp_allgroundconfirm2 ", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1016, true);
                        }
                        break;
                    #endregion


                    #region 任务完成上架1042
                    case "1042":
                        ReadXml(uploadDataXML);
                        ls_groundguid = oper_groundguid();

                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "groundguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_groundguid;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1042", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion

                        ddt = new DataTable();
                        ddt = GetDataTable(@"select count(1) from Tb_Wms_Ground where groundguid = '" + ls_groundguid + "' and States = getgroundstatus('已完成上架')");
                        ll_cnt = Convert.ToInt32(ddt.Rows[0][0].ToString());

                        if (ll_cnt > 0)
                        {
                            ls_returnmsg = "已完成任务上架，不能重复操作！";
                            break;
                        }

                        ddt = new DataTable();
                        ddt = GetDataTable(@"select count(1) from Tb_Wms_LabelGroundDetail where labelgroundguid in
                                       (select labelgroundguid
                                          from tb_wms_labelground
                                         where groundguid = '" + ls_groundguid + "') and confirmflag = GetConfirmstatus('已确认')");
                        ll_cnt = Convert.ToInt32(ddt.Rows[0][0].ToString());
                        if (ll_cnt == 0)
                        {
                            ls_returnmsg = "还未进行过上架操作，不能提交完成任务！";
                            break;
                        }

                        dt = new DataTable();
                        dt = GetDataTable(@"select count(1)
                                            from TB_WMS_GROUNDCONFIRM
                                           where gROUNDGUID = '" + ls_groundguid + "'");
                        ll_cnt = Convert.ToInt32(dt.Rows[0][0].ToString());
                        if (ll_cnt > 0)
                        {
                            dt = new DataTable();
                            dt = GetDataTable(@"select confirmguid
                                            from TB_WMS_GROUNDCONFIRM
                                           where gROUNDGUID =  '" + ls_groundguid + "'");
                            ls_confirmguid = dt.Rows[0][0].ToString();
                        }
                        else
                        {
                            dt = new DataTable();
                            dt = GetDataTable(@"select createguid()
                                            from dual");
                            ls_confirmguid = dt.Rows[0][0].ToString();
                        }
                        ls_returnmsg = SqlDataTable(@"update Tb_Wms_LabelGroundDetail
                                                       set ConfirmGUID = '" + ls_confirmguid + @"'
                                                     where LabelGroundGUID in
                                                           (select LabelGroundGUID
                                                              from Tb_Wms_LabelGround
                                                             where GroundGUID = '" + ls_groundguid + "')");
                        if (ll_cnt == 0)
                        {
                            ls_returnmsg = SqlDataTable(@"insert into TB_WMS_GROUNDCONFIRM
                                                        (CONFIRMGUID,
                                                         gROUNDGUID,
                                                         STOREAREAGUID,
                                                         COMPANYGUID,
                                                         OPERUSER,
                                                         OPERDATE,
                                                         REMARK)
                                                      values
                                                        ('" + ls_confirmguid + @"',
                                                          '" + ls_groundguid + @"',
                                                         (  select storeareaguid
                                                              from Tb_Wms_Ground
                                                             where groundguid = '" + ls_groundguid + @"'),
                                                         (select companyguid
                                                            from tb_wms_ground
                                                           where groundguid =  '" + ls_groundguid + @"'),
                                                         '" + OperUserID + @"',
                                                         sysdate,
                                                         null)");
                        }
                        ls_returnmsg = SqlDataTable(@"update Tb_Wms_Ground
                                                     set States = getgroundstatus('已完成上架')
                                                   where groundguid = '" + ls_groundguid + "'");
                        ls_returnmsg = SqlDataTable(@"delete from Tb_Wms_GroundKeepLocation
                                                   where groundguid = '" + ls_groundguid + "'");
                        ls_returnmsg = "TRUE";
                        dt = new DataTable();
                        dt = GetDataTable(@"select naturecode from tb_wms_storeareanature where natureguid in(select natureguid from tb_wms_ground where groundguid = '" + ls_groundguid + "')");
                        ls_naturecode = dt.Rows[0][0].ToString();
                        //电子标签下发复位指令
                        if (ls_returnmsg == "TRUE" && ((ib_iflabel == "2" && ls_naturecode == "002") || ib_iflabel == "3"))
                        {
                            dt = new DataTable();
                            dt = GetDataTable("select ip from Tb_Wms_Control where ControlGUID in (select ControlGUID from Tb_Wms_Ground where groundguid = '" + ls_groundguid + "')");
                            if (dt.Rows.Count == 0)
                            {
                                ls_returnmsg = "没有指定控制器IP，不能下发，请检查！";
                                break;
                            }
                            ls_controlip = dt.Rows[0][0].ToString();
                            ls_returnmsg = LabelInterface(
                                                        @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                <function>
                                                    <functionid>3009</functionid>
                                                    <wmscode></wmscode>
                                                    <orgcode>" + OrgCode + @"</orgcode>
                                                    <interfaceusername></interfaceusername>
                                                    <interfaceuserpass></interfaceuserpass>
                                                    <operusername></operusername>
                                                    <operuserpass></operuserpass>
                                                    <opertime></opertime>
                                                    <dbtype>" + ls_controlip + @"</dbtype>
                                                    <interface>MIAWMS</interface>
                                                </function>",
                                                            "",
                                                            out ls_xml);
                            ReadXml(ls_returnmsg);
                            ls_returncode = oper_returncode();
                            ls_opermsg = oper_returnmsg();
                            if (ls_returncode != "0000")
                            {
                                ls_returnmsg = ls_opermsg;
                                break;
                            }
                            else
                            {
                                ls_returnmsg = "TRUE";
                            }
                        }
                        break;
                    #endregion


                    #region 托盘盘点完成1043
                    case "1043":
                        ReadXml(uploadDataXML);
                        ls_pickguid = oper_pickguid();
                        ls_locationguid = oper_locationguid();
                        ls_trayguid = oper_trayguid();
                        #region 效验
                        ls_inparam = new string[3];
                        ls_inparam[0] = "as_pickguid";
                        ls_inparam[1] = "as_trayguid";
                        ls_inparam[2] = "as_locationguid";
                        ls_inparamvalue = new string[3];
                        ls_inparamvalue[0] = ls_pickguid;
                        ls_inparamvalue[1] = ls_trayguid;
                        ls_inparamvalue[2] = ls_locationguid;
                        ls_inparamtype = new string[3];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1043", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[4];
                        ls_inparam[0] = "as_inventguid";
                        ls_inparam[1] = "as_trayguid";
                        ls_inparam[2] = "as_locationguid";
                        ls_inparam[3] = "as_operuser";
                        ls_inparamvalue = new string[4];
                        ls_inparamvalue[0] = ls_pickguid;
                        ls_inparamvalue[1] = ls_trayguid;
                        ls_inparamvalue[2] = ls_locationguid;
                        ls_inparamvalue[3] = OperUserID;
                        ls_inparamtype = new string[4];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_inparamtype[3] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_updatepickinfo2", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1043, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        break;
                    #endregion


                    #region 整件拣货任务完成1044
                    case "1044":
                        ReadXml(uploadDataXML);
                        ls_pickguid = oper_pickguid();
                        ls_picktype = oper_picktype();

                        #region 效验
                        ls_inparam = new string[2];
                        ls_inparam[0] = "pickguid";
                        ls_inparam[1] = "picktype";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = ls_pickguid;
                        ls_inparamvalue[1] = ls_picktype;
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1044", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion

                        ddt = new DataTable();
                        ddt = GetDataTable(@"select count(1) from Tb_Wms_PickDetail where PickGUID = '" + ls_pickguid + "' and ConfirmFlag = getconfirmstatus('已确认')");
                        ll_cnt = Convert.ToInt32(ddt.Rows[0][0].ToString());
                        if (ll_cnt == 0)
                        {
                            ls_returnmsg = "还未进行过拣货操作，不能提交完成任务！";
                            break;
                        }
                        if (ls_picktype == "0")
                        {
                            //执行存储过程（生成确认单）
                            ls_inparam = new string[2];
                            ls_inparam[0] = "as_pickguid";
                            ls_inparam[1] = "as_operuser";
                            ls_inparamtype = new string[2];
                            ls_inparamtype[0] = "varchar";
                            ls_inparamtype[1] = "varchar";
                            ls_inparamvalue = new string[2];
                            ls_inparamvalue[0] = ls_pickguid;
                            ls_inparamvalue[1] = OperUserID;

                            ls_outparam = new string[2];
                            ls_outparam[0] = "as_returncode";
                            ls_outparam[1] = "as_returnmsg";
                            ls_outparamtype = new string[2];
                            ls_outparamtype[0] = "varchar";
                            ls_outparamtype[1] = "varchar";

                            ddt = new DataTable();
                            ddt = GetDataTable(@"select count(1) from TB_WMS_PICKCONFIRM where PICKGUID = '" + ls_pickguid + "' and status <> '8'");
                            ll_cnt = Convert.ToInt32(ddt.Rows[0][0].ToString());

                            if (ll_cnt > 0)
                            {
                                ls_returnmsg = "已完成任务，不能重复操作！";
                                break;
                            }
                            else
                            {

                            }

                            ls_returnmsg = Doprocedure("miawms.sp_pickconfirm", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1044, true);
                            if (ls_returnmsg != "TRUE")
                            {
                                break;
                            }
                            else
                            {
                                ls_returnmsg = SqlDataTable("update Tb_Wms_Pick set Status = '3' where pickguid = '" + ls_pickguid + "'");
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                            }
                        }
                        else if (ls_picktype == "1")
                        {
                            ls_inparam = new string[2];
                            ls_inparam[0] = "as_inventguid";
                            ls_inparam[1] = "as_operuser";
                            ls_inparamtype = new string[2];
                            ls_inparamtype[0] = "varchar";
                            ls_inparamtype[1] = "varchar";
                            ls_inparamvalue = new string[2];
                            ls_inparamvalue[0] = ls_pickguid;
                            ls_inparamvalue[1] = OperUserID;

                            ls_outparam = new string[2];
                            ls_outparam[0] = "as_returncode";
                            ls_outparam[1] = "as_returnmsg";
                            ls_outparamtype = new string[2];
                            ls_outparamtype[0] = "varchar";
                            ls_outparamtype[1] = "varchar";

                            ddt = new DataTable();
                            ddt = GetDataTable(@"select count(1) from TB_WMS_inventCONFIRM where inventGUID = '" + ls_pickguid + "'");
                            ll_cnt = Convert.ToInt32(ddt.Rows[0][0].ToString());

                            if (ll_cnt > 0)
                            {
                                ls_returnmsg = "已完成任务，不能重复操作！";
                                break;
                            }
                            else
                            {

                            }

                            ls_returnmsg = Doprocedure("miawms.sp_inventconfirm", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1044, true);
                            if (ls_returnmsg != "TRUE")
                            {
                                break;
                            }
                            else
                            {
                                ls_returnmsg = SqlDataTable("update Tb_Wms_invent set Status = '3' where inventguid = '" + ls_pickguid + "'");
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                                else
                                {
                                    dt = new DataTable();
                                    dt = GetDataTable("select count(1) from Tb_Wms_Invent where StartInventGUID = (select StartInventGUID from Tb_Wms_Invent where inventguid = '" + ls_pickguid + "') and status in ('0','1','2')");//没有一条未确认的记录了
                                    ll_cnt = Convert.ToInt32(ddt.Rows[0][0].ToString());
                                    if (ll_cnt == 0)
                                    {
                                        ls_returnmsg = SqlDataTable("update Tb_Wms_StartInvent set Status = '2' where StartInventGUID = (select StartInventGUID from Tb_Wms_Invent where inventguid = '" + ls_pickguid + "')");
                                    }
                                }
                            }
                        }

                        //电子标签亮灯
                        if (ls_returnmsg == "TRUE" && ib_iflabel == "3")
                        {
                            //电子标签下发复位指令
                            dt = new DataTable();
                            dt = GetDataTable("select ip from Tb_Wms_Control where ControlGUID in (select ControlGUID from Tb_Wms_Pick where pickguid = '" + ls_pickguid + "')");
                            if (dt.Rows.Count == 0)
                            {
                                ls_returnmsg = "没有指定控制器IP，不能下发，请检查！";
                                break;
                            }
                            ls_controlip = dt.Rows[0][0].ToString();
                            ls_returnmsg = LabelInterface(
                                                        @"<?xml version=""1.0"" encoding=""GB2312""?>
                                                <function>
                                                    <functionid>3009</functionid>
                                                    <wmscode></wmscode>
                                                    <orgcode>" + OrgCode + @"</orgcode>
                                                    <interfaceusername></interfaceusername>
                                                    <interfaceuserpass></interfaceuserpass>
                                                    <operusername></operusername>
                                                    <operuserpass></operuserpass>
                                                    <opertime></opertime>
                                                    <dbtype>" + ls_controlip + @"</dbtype>
                                                    <interface>MIAWMS</interface>
                                                </function>",
                                                            "",
                                                            out ls_xml);
                            ReadXml(ls_returnmsg);
                            ls_returncode = oper_returncode();
                            ls_opermsg = oper_returnmsg();
                            if (ls_returncode != "0000")
                            {
                                ls_returnmsg = ls_opermsg;
                                break;
                            }
                            else
                            {
                                ls_returnmsg = "TRUE";
                            }
                        }


                        break;
                    #endregion


                    #region 整件盘点确认1047
                    case "1047":
                        ReadXml(uploadDataXML);
                        ls_inventguid = oper_inventguid();
                        #region 效验
                        ls_inparam = new string[2];
                        ls_inparam[0] = "inventguid";
                        ls_inparam[1] = "shelveguid";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = ls_inventguid;
                        ls_inparamvalue[1] = oper_shelveguid();
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1047", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        dt = GetAllColumnsData();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            ls_locationguid = dt.Rows[i][2].ToString();
                            ls_inventnum = dt.Rows[i][6].ToString();
                            ls_wareguid = dt.Rows[i][0].ToString();
                            ls_returnmsg = SqlDataTable("update Tb_Wms_LabelpickDetail set comfirmnum = " + ls_inventnum + ",confirmflag = '1',confirmuser = '" + OperUserID + "',confirmtime = sysdate where LocationGUID = '" + ls_locationguid + "' and LabelpickGUID in (select LabelpickGUID from Tb_Wms_LabelPick  where PickGUID = '" + ls_inventguid + "') and wareguid = '" + ls_wareguid + "'");
                            if (ls_returnmsg != "TRUE")
                            {
                                break;
                            }

                            //执行存储过程（生成确认单）
                            ls_inparam = new string[2];
                            ls_inparam[0] = "as_inventguid";
                            ls_inparam[1] = "as_operuser";
                            ls_inparamtype = new string[2];
                            ls_inparamtype[0] = "varchar";
                            ls_inparamtype[1] = "varchar";
                            ls_inparamvalue = new string[2];
                            ls_inparamvalue[0] = ls_inventguid;
                            ls_inparamvalue[1] = OperUserID;

                            ls_outparam = new string[2];
                            ls_outparam[0] = "as_returncode";
                            ls_outparam[1] = "as_returnmsg";
                            ls_outparamtype = new string[2];
                            ls_outparamtype[0] = "varchar";
                            ls_outparamtype[1] = "varchar";
                            ls_returnmsg = "TRUE";

                            ls_returnmsg = Doprocedure("miawms.sp_inventconfirm", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1047, true);
                            if (ls_returnmsg != "TRUE")
                            {
                                break;
                            }
                            ls_returnmsg = SqlDataTable("update Tb_Wms_Invent set Status = '3' where inventguid = '" + ls_inventguid + "'");
                            if (ls_returnmsg != "TRUE")
                            {
                                break;
                            }
                        }
                        break;
                    #endregion


                    #region 生成商品件信息1017
                    case "1017":
                        ReadXml(uploadDataXML);
                        ls_orderguid = oper_orderguid();
                        ls_wareguid = oper_wareguid();
                        ls_warepackbar = oper_warepackbar();
                        #region 效验
                        ls_inparam = new string[3];
                        ls_inparam[0] = "orderguid";
                        ls_inparam[1] = "wareguid";
                        ls_inparam[2] = "warepackbar";
                        ls_inparamvalue = new string[3];
                        ls_inparamvalue[0] = ls_orderguid;
                        ls_inparamvalue[1] = ls_wareguid;
                        ls_inparamvalue[2] = ls_warepackbar;
                        ls_inparamtype = new string[3];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1017", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[3];
                        ls_inparam[0] = "as_orderguid";
                        ls_inparam[1] = "as_wareguid";
                        ls_inparam[2] = "as_warepackbar";
                        ls_inparamtype = new string[3];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_inparamvalue = new string[3];
                        ls_inparamvalue[0] = ls_orderguid;
                        ls_inparamvalue[1] = ls_wareguid;
                        ls_inparamvalue[2] = ls_warepackbar;

                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_setwarepack", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1017, true);
                        break;
                    #endregion


                    #region 根据任务获取周转箱列表1036
                    case "1036":
                        ReadXml(uploadDataXML);
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "confirmguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = oper_confirmguid();
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1036", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[1];
                        ls_inparam[0] = "as_confirmguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = oper_confirmguid();
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_getpickbox", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1036, true);
                        break;
                    #endregion


                    #region 根据任务获取商品件列表1037
                    case "1037":
                        ReadXml(uploadDataXML);
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "confirmguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = oper_confirmguid();
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1037", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[1];
                        ls_inparam[0] = "as_confirmguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = oper_confirmguid();
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_getpickwarepack", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1037, true);
                        break;
                    #endregion


                    #region 零货复核单品扫描1038
                    case "1038":
                        ReadXml(uploadDataXML);
                        ls_confirmguid = oper_confirmguid();
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "confirmguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_confirmguid;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1038", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        dt = GetAllColumnsData();
                        ls_returnmsg = checkware(ls_confirmguid, dt);
                        break;
                    #endregion


                    #region 整件复核商品件扫描1039
                    case "1039":
                        ReadXml(uploadDataXML);
                        dt = GetAllColumnsData();
                        ls_bar = dt.Rows[0][0].ToString();
                        ls_confirmguid = oper_confirmguid();
                        #region 效验
                        ls_inparam = new string[2];
                        ls_inparam[0] = "as_bar";
                        ls_inparam[1] = "as_confirmguid";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = ls_bar;
                        ls_inparamvalue[1] = ls_confirmguid;
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1039", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[3];
                        ls_inparam[0] = "as_bar";
                        ls_inparam[1] = "as_operuser";
                        ls_inparam[2] = "as_confirmguid";
                        ls_inparamvalue = new string[3];
                        ls_inparamvalue[0] = ls_bar;
                        ls_inparamvalue[1] = OperUserID;
                        ls_inparamvalue[2] = ls_confirmguid;
                        ls_inparamtype = new string[3];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_packcheck", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1039, true);
                        break;
                    #endregion


                    #region 托盘商品件查询1048
                    case "1048":
                        ReadXml(uploadDataXML);
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "trayguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = oper_trayguid();
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1048", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[1];
                        ls_inparam[0] = "as_trayguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = oper_trayguid();
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_gettraywarepack", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1048, true);
                        break;
                    #endregion


                    #region 根据托盘商品条码获取已绑定商品件信息1049
                    case "1049":
                        ReadXml(uploadDataXML);
                        ls_warepackbar = oper_warepackbar();
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "warepackbar";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_warepackbar;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1049", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[4];
                        ls_inparam[0] = "as_pickguid";
                        ls_inparam[1] = "as_trayguid";
                        ls_inparam[2] = "as_wareguid";
                        ls_inparam[3] = "as_batchs";
                        ls_inparamvalue = new string[4];
                        ls_inparamvalue[0] = oper_pickguid();
                        ls_inparamvalue[1] = oper_trayguid();
                        ls_inparamvalue[2] = oper_wareguid();
                        ls_inparamvalue[3] = oper_batchs();
                        ls_inparamtype = new string[4];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_inparamtype[3] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";

                        ls_returnmsg = Doprocedure("miawms.sp_getwareinfo5", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1049, true);
                        break;
                    #endregion


                    #region 获取拣货单所需周转箱数量1050
                    case "1050":
                        ReadXml(uploadDataXML);
                        ls_pickguid = oper_pickguid();
                        #region 效验
                        ls_inparam = new string[1];
                        ls_inparam[0] = "pickguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_pickguid;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1050", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[1];
                        ls_inparam[0] = "as_pickguid";
                        ls_inparamvalue = new string[1];
                        ls_inparamvalue[0] = ls_pickguid;
                        ls_inparamtype = new string[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_calcbox", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1050, true);
                        break;
                    #endregion


                    #region 零货货位拣货完成1051
                    case "1051":
                        ReadXml(uploadDataXML);
                        ls_pickguid = oper_pickguid();
                        ls_locationguid = oper_locationguid();
                        ls_picktype = oper_picktype();
                        #region 效验
                        ls_inparam = new string[3];
                        ls_inparam[0] = "as_pickguid";
                        ls_inparam[1] = "as_locationguid";
                        ls_inparam[2] = "as_picktype";
                        ls_inparamvalue = new string[3];
                        ls_inparamvalue[0] = ls_pickguid;
                        ls_inparamvalue[1] = ls_locationguid;
                        ls_inparamvalue[2] = ls_picktype;
                        ls_inparamtype = new string[3];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1051", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        dt = GetAllColumnsData();
                        ls_picknum = dt.Rows[0]["realnum"].ToString();
                        if (ls_picktype == "0")
                        {
                            ls_inparam = new string[4];
                            ls_inparam[0] = "as_pickguid";
                            ls_inparam[1] = "as_locationguid";
                            ls_inparam[2] = "as_picknum";
                            ls_inparam[3] = "as_operuser";
                            ls_inparamvalue = new string[4];
                            ls_inparamvalue[0] = ls_pickguid;
                            ls_inparamvalue[1] = ls_locationguid;
                            ls_inparamvalue[2] = ls_picknum;
                            ls_inparamvalue[3] = OperUserID;
                            ls_inparamtype = new string[4];
                            ls_inparamtype[0] = "varchar";
                            ls_inparamtype[1] = "varchar";
                            ls_inparamtype[2] = "int";
                            ls_inparamtype[3] = "varchar";
                            ls_outparam = new string[2];
                            ls_outparam[0] = "as_returncode";
                            ls_outparam[1] = "as_returnmsg";
                            ls_outparamtype = new string[2];
                            ls_outparamtype[0] = "varchar";
                            ls_outparamtype[1] = "varchar";
                            ls_returnmsg = Doprocedure("miawms.sp_singlepick", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1051, true);
                        }
                        else if (ls_picktype == "1")//拣货电子标签标记时，就要更新Tb_Wms_InventDetail的确认数量
                        {
                            ls_inparam = new string[4];
                            ls_inparam[0] = "as_pickguid";
                            ls_inparam[1] = "as_locationguid";
                            ls_inparam[2] = "as_picknum";
                            ls_inparam[3] = "as_operuser";
                            ls_inparamvalue = new string[4];
                            ls_inparamvalue[0] = ls_pickguid;
                            ls_inparamvalue[1] = ls_locationguid;
                            ls_inparamvalue[2] = ls_picknum;
                            ls_inparamvalue[3] = OperUserID;
                            ls_inparamtype = new string[4];
                            ls_inparamtype[0] = "varchar";
                            ls_inparamtype[1] = "varchar";
                            ls_inparamtype[2] = "int";
                            ls_inparamtype[3] = "varchar";
                            ls_outparam = new string[2];
                            ls_outparam[0] = "as_returncode";
                            ls_outparam[1] = "as_returnmsg";
                            ls_outparamtype = new string[2];
                            ls_outparamtype[0] = "varchar";
                            ls_outparamtype[1] = "varchar";
                            ls_returnmsg = Doprocedure("miawms.sp_singlepick2", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1051, true);
                        }
                        break;
                    #endregion


                    #region 扫描货位条码获取零货货位拣货信息1052
                    case "1052":
                        ReadXml(uploadDataXML);
                        ls_bar = oper_locationbar();
                        ls_pickguid = oper_pickguid();
                        #region 效验
                        ls_inparam = new string[2];
                        ls_inparam[0] = "pickguid";
                        ls_inparam[1] = "locationbar";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = ls_pickguid;
                        ls_inparamvalue[1] = ls_bar;
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1052", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        dt = new DataTable();
                        dt = GetDataTable(@"select locationguid from Tb_Wms_location where bar = '" + ls_bar + "'");
                        ls_locationguid = dt.Rows[0][0].ToString();
                        ls_inparam = new string[2];
                        ls_inparam[0] = "as_locationguid";
                        ls_inparam[1] = "as_pickguid";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = ls_locationguid;
                        ls_inparamvalue[1] = ls_pickguid;
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_getwareinfo8", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1052, true);
                        break;
                    #endregion


                    #region 托盘商品件单件盘点确认1059
                    case "1059":
                        ReadXml(uploadDataXML);
                        ls_locationguid = oper_locationguid();
                        ls_pickguid = oper_pickguid();
                        ls_trayguid = oper_trayguid();
                        ls_warepackbar = oper_warepackbar();
                        #region 效验
                        ls_inparam = new string[4];
                        ls_inparam[0] = "as_pickguid";
                        ls_inparam[1] = "as_trayguid";
                        ls_inparam[2] = "as_locationguid";
                        ls_inparam[3] = "as_warepackbar";
                        ls_inparamvalue = new string[4];
                        ls_inparamvalue[0] = ls_pickguid;
                        ls_inparamvalue[1] = ls_trayguid;
                        ls_inparamvalue[2] = ls_locationguid;
                        ls_inparamvalue[3] = ls_warepackbar;
                        ls_inparamtype = new string[4];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_inparamtype[3] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1059", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[5];
                        ls_inparam[0] = "as_pickguid";
                        ls_inparam[1] = "as_trayguid";
                        ls_inparam[2] = "as_locationguid";
                        ls_inparam[3] = "as_warepackbar";
                        ls_inparam[4] = "as_operuser";
                        ls_inparamvalue = new string[5];
                        ls_inparamvalue[0] = ls_pickguid;
                        ls_inparamvalue[1] = ls_trayguid;
                        ls_inparamvalue[2] = ls_locationguid;
                        ls_inparamvalue[3] = ls_warepackbar;
                        ls_inparamvalue[4] = OperUserID;
                        ls_inparamtype = new string[5];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_inparamtype[3] = "varchar";
                        ls_inparamtype[4] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_pickpackconfirm", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1059, true);
                        break;
                    #endregion


                    #region PDA验证信息（通用方法）1101
                    case "1101":
                        ReadXml(uploadDataXML);
                        ls_inparam = new string[4];
                        ls_inparam[0] = "as_guid";
                        ls_inparam[1] = "as_codebar";
                        ls_inparam[2] = "as_typeid";
                        ls_inparam[3] = "as_status";
                        ls_inparamvalue = new string[4];
                        ls_inparamvalue[0] = oper_guid();
                        ls_inparamvalue[1] = oper_codebar();
                        ls_inparamvalue[2] = oper_codetype();
                        ls_inparamvalue[3] = oper_status();
                        ls_inparamtype = new string[4];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "int";
                        ls_inparamtype[3] = "int";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_getCodeInfo", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1101, true);
                        break;
                    #endregion


                    #region 条码查询1063
                    case "1063":
                        ReadXml(uploadDataXML);
                        ls_inparam = new string[2];
                        ls_inparam[0] = "as_codebar";
                        ls_inparam[1] = "as_operuser";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = oper_codebar();
                        ls_inparamvalue[1] = OperUserID;
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_getbarInfo", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1063, true);
                        break;
                    #endregion


                    #region 入库判断订单是否合法1102(作废)
                    case "1102":
                        ReadXml(uploadDataXML);
                        ls_inparam = new String[3];
                        ls_inparam[0] = "as_Orderid";
                        ls_inparam[1] = "as_Wareguid";
                        ls_inparam[2] = "as_Num";
                        ls_inparamvalue = new String[3];
                        ls_inparamvalue[0] = oper_orderguid();
                        ls_inparamvalue[1] = oper_wareguid();
                        ls_inparamvalue[2] = oper_num();
                        ls_inparamtype = new String[3];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.getOrderInfo", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1102, true);
                        break;
                    #endregion


                    #region 根据入库品种、批号信息 获取有效期、生产批号1103
                    case "1103":
                        ReadXml(uploadDataXML);
                        ls_orderguid = oper_orderguid();
                        ls_inparam = new String[3];
                        ls_inparam[0] = "as_warebar";
                        ls_inparam[1] = "as_Batch";
                        ls_inparam[2] = "as_orgguid";
                        dt = new DataTable();
                        dt = GetDataTable("select orgguid from tb_wms_buginstoreorder where orderguid = '" + ls_orderguid + "' union all select orgguid from tb_wms_salereturninstoreorder where orderguid = '" + ls_orderguid + "' union all select orgguid from tb_wms_periodinstore where inguid = '" + ls_orderguid + "' union all select orgguid from tb_wms_moveinstore where inguid = '" + ls_orderguid + "'");
                        ls_orgguid = dt.Rows[0][0].ToString();
                        ls_inparamvalue = new String[3];
                        ls_inparamvalue[0] = oper_warebar();
                        ls_inparamvalue[1] = oper_batchs();
                        ls_inparamvalue[2] = ls_orgguid;
                        ls_inparamtype = new String[3];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.getWareBatch", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1103, true);
                        break;
                    #endregion


                    #region 一个周转箱只能存放一个批号的品种1104(作废)
                    case "1104":
                        ReadXml(uploadDataXML);
                        ls_inparam = new String[1];
                        ls_inparam[0] = "as_Transportguid";
                        ls_inparamvalue[0] = oper_Transportguid();
                        ls_inparamtype = new String[1];
                        ls_inparamtype[0] = "varchar";
                        ls_outparam = new String[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.getTransPort", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1104, true);
                        break;
                    #endregion


                    #region 根据验收任务获取已绑定周转箱列表1053
                    case "1053":
                        ReadXml(uploadDataXML);
                        ls_inparam = new string[2];
                        ls_inparam[0] = "as_inguid";
                        ls_inparam[1] = "as_operuser";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = oper_inguid();
                        ls_inparamvalue[1] = OperUserID;
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_get1053", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1053, true);
                        break;
                    #endregion


                    #region 根据验收任务获取已绑定托盘列表1056
                    case "1056":
                        ReadXml(uploadDataXML);
                        ls_inparam = new string[2];
                        ls_inparam[0] = "as_inguid";
                        ls_inparam[1] = "as_operuser";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = oper_inguid();
                        ls_inparamvalue[1] = OperUserID;
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_get1056", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1056, true);
                        break;
                    #endregion


                    #region 根据验收已绑定周转箱获取绑定商品信息1054
                    case "1054":
                        ReadXml(uploadDataXML);
                        ls_inparam = new string[2];
                        ls_inparam[0] = "as_inguid";
                        ls_inparam[1] = "as_boxbar";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = oper_inguid();
                        ls_inparamvalue[1] = oper_boxbar();
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_get1054", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1054, true);
                        break;
                    #endregion


                    #region 根据验收已绑定托盘获取绑定商品件1057
                    case "1057":
                        ReadXml(uploadDataXML);
                        ls_inparam = new string[2];
                        ls_inparam[0] = "as_inguid";
                        ls_inparam[1] = "as_boxbar";
                        ls_inparamvalue = new string[2];
                        ls_inparamvalue[0] = oper_inguid();
                        ls_inparamvalue[1] = oper_boxbar();
                        ls_inparamtype = new string[2];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_outparam = new string[3];
                        ls_outparam[0] = "cur_Jobs";
                        ls_outparam[1] = "as_returncode";
                        ls_outparam[2] = "as_returnmsg";
                        ls_outparamtype = new string[3];
                        ls_outparamtype[0] = "cursor";
                        ls_outparamtype[1] = "varchar";
                        ls_outparamtype[2] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_get1057", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1057, true);
                        break;
                    #endregion


                    #region 删除验收已绑定周转箱1055
                    case "1055":
                        ReadXml(uploadDataXML);
                        #region 效验
                        ls_inparam = new string[4];
                        ls_inparam[0] = "as_orderguid";
                        ls_inparam[1] = "as_checkguid";
                        ls_inparam[2] = "as_inguid";
                        ls_inparam[3] = "as_boxbar";
                        ls_inparamvalue = new string[4];
                        ls_inparamvalue[0] = oper_orderguid();
                        ls_inparamvalue[1] = oper_checkguid();
                        ls_inparamvalue[2] = oper_inguid();
                        ls_inparamvalue[3] = oper_boxbar();
                        ls_inparamtype = new string[4];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_inparamtype[3] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1055", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[4];
                        ls_inparam[0] = "as_orderguid";
                        ls_inparam[1] = "as_checkguid";
                        ls_inparam[2] = "as_inguid";
                        ls_inparam[3] = "as_boxbar";
                        ls_inparamvalue = new string[4];
                        ls_inparamvalue[0] = oper_orderguid();
                        ls_inparamvalue[1] = oper_checkguid();
                        ls_inparamvalue[2] = oper_inguid();
                        ls_inparamvalue[3] = oper_boxbar();
                        ls_inparamtype = new string[4];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_inparamtype[3] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_get1055", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1055, true);
                        break;
                    #endregion


                    #region 删除验收已绑定托盘1058
                    case "1058":
                        ReadXml(uploadDataXML);
                        #region 效验
                        ls_inparam = new string[4];
                        ls_inparam[0] = "as_orderguid";
                        ls_inparam[1] = "as_checkguid";
                        ls_inparam[2] = "as_inguid";
                        ls_inparam[3] = "as_boxbar";
                        ls_inparamvalue = new string[4];
                        ls_inparamvalue[0] = oper_orderguid();
                        ls_inparamvalue[1] = oper_checkguid();
                        ls_inparamvalue[2] = oper_inguid();
                        ls_inparamvalue[3] = oper_boxbar();
                        ls_inparamtype = new string[4];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_inparamtype[3] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.check1058", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 8888, true);
                        if (ls_returnmsg != "TRUE")
                        {
                            break;
                        }
                        #endregion
                        ls_inparam = new string[4];
                        ls_inparam[0] = "as_orderguid";
                        ls_inparam[1] = "as_checkguid";
                        ls_inparam[2] = "as_inguid";
                        ls_inparam[3] = "as_boxbar";
                        ls_inparamvalue = new string[4];
                        ls_inparamvalue[0] = oper_orderguid();
                        ls_inparamvalue[1] = oper_checkguid();
                        ls_inparamvalue[2] = oper_inguid();
                        ls_inparamvalue[3] = oper_boxbar();
                        ls_inparamtype = new string[4];
                        ls_inparamtype[0] = "varchar";
                        ls_inparamtype[1] = "varchar";
                        ls_inparamtype[2] = "varchar";
                        ls_inparamtype[3] = "varchar";
                        ls_outparam = new string[2];
                        ls_outparam[0] = "as_returncode";
                        ls_outparam[1] = "as_returnmsg";
                        ls_outparamtype = new string[2];
                        ls_outparamtype[0] = "varchar";
                        ls_outparamtype[1] = "varchar";
                        ls_returnmsg = Doprocedure("miawms.sp_get1058", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1058, true);
                        break;
                    #endregion

                }
                #region 返回值处理
                if (ls_returnmsg != "TRUE")
                {
                    RollbackTrans();
                    WriteXml(FunctionId, "1000", ls_returnmsg);
                    WriteLog(OperUserID, DateTime.Now, requestParmXML, retDataXML, uploadDataXML, doc.InnerXml, "PDA", null, FunctionId, "1000");
                }
                else
                {
                    CommitTrans();
                    if (ls_returndt != "")
                    {
                        WriteXml(FunctionId, "0000", "处理成功");
                        retDataXML = ls_returndt;
                    }
                    else if (ls_xml != "" && ls_xml != null)
                    {
                        WriteXml(FunctionId, "0000", "处理成功");
                        retDataXML = ls_xml;
                    }
                    else
                    {
                        WriteXml(FunctionId, "0000", "处理成功");
                    }
                    WriteLog(OperUserID, DateTime.Now, requestParmXML, retDataXML, uploadDataXML, doc.InnerXml, "PDA", null, FunctionId, "0000");
                }
                return (doc.InnerXml);
                #endregion
            }
            catch (Exception ex)
            {
                RollbackTrans();
                ls_returnmsg = ex.Message.ToString();
                WriteXml(FunctionId, "1000", ls_returnmsg);
                WriteLog(OperUserID, DateTime.Now, requestParmXML, ls_returnmsg, uploadDataXML, doc.InnerXml, "PDA", null, FunctionId, "1000");
                return (doc.InnerXml);
            }
        }

        [WebMethod(Description = "业务系统调用接口处理业务的方法")]
        public string BusinessInterface(string requestParmXML, string uploadDataXML, out string retDataXML)
        //public string BusinessInterface(string requestParmXML, string uploadDataXML/*, out string retDataXML*/)
        {
            #region 变量
            string ls_msg = "";
            string ls_returnmsg = "";
            string ls_returndt = "";
            string[] ls_columnname = null;
            string ls_xml;
            string ls_dbtype;
            string ls_orderguid = "";
            string ls_orgguid = "";
            //string retDataXML = "";
            ReadXml(requestParmXML);
            OrgCode = org_no();
            WmsCode = wms_no();
            ls_dbtype = wms_dbtype();
            InterfaceUserID = user_no();
            InterfacePassWord = user_pass();
            OperUserID = oper_no();
            OperPassWord = oper_pass();
            Opertime = oper_time();
            Interface = oper_interface();
            FunctionId = function_id();
            retDataXML = "";
            #endregion
            try
            {
                #region 初始化
                if (Interface != "MIAWMS")
                {
                    if (String2Base64(UserMd5(FunctionId + Opertime)) != Interface)
                    {
                        WriteXml("0001", "2000", "通讯密匙验证失败");
                        return (doc.InnerXml);
                    }
                }
                #endregion
                if (getcnParms(OrgCode, out ls_msg))
                {
                    ls_returnmsg = checkUserValid(FunctionId, WmsCode, InterfaceUserID, InterfacePassWord, OperUserID, OperPassWord, ls_msg, 2 /*业务系统传2;PDA传1*/, 0/*不需要用户身份验证*/, out ls_xml);
                    if (ls_returnmsg == "TRUE")
                    {
                        DataTable dt = new DataTable();
                        DataTable dtnew = new DataTable();
                        DataTable dtt = null;
                        string ls_guid = null;
                        switch (FunctionId)
                        {
                            #region 执行动态语句     0002
                            case "0002":
                                ls_columnname = new string[1];
                                ReadXml(uploadDataXML);
                                ls_columnname[0] = "sql";
                                dt = GetAllColumnsData(ls_columnname, 0);
                                ls_returnmsg = data_validation(dt, FunctionId, ls_dbtype);
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                                switch (ls_dbtype)
                                {
                                    case "0":       /*查询、当前只支持单SQL查询*/
                                        ls_returnmsg = FindDataTable(dt, out ls_returndt);
                                        break;
                                    case "1":       /*插入*/
                                        ls_returnmsg = SqlDataTable(dt);
                                        break;
                                    case "2":       /*更新*/
                                        ls_returnmsg = SqlDataTable(dt);
                                        break;
                                    case "3":       /*删除*/
                                        ls_returnmsg = SqlDataTable(dt);
                                        break;
                                    default:
                                        ls_returnmsg = "无法识别的操作类型";
                                        break;
                                }
                                break;
                            #endregion
                            #region 上传机构商品目录    2002
                            case "2002": /*上传机构商品目录 */
                                ls_columnname = new string[33];
                                ReadXml(uploadDataXML);
                                ls_columnname[0] = "WAREGUID".ToLower();
                                ls_columnname[1] = "SPECS".ToLower();
                                ls_columnname[2] = "DRUGGUID".ToLower();
                                ls_columnname[3] = "UNITGUID".ToLower();
                                ls_columnname[4] = "PRODUCTFACTGUID".ToLower();
                                ls_columnname[5] = "PRODUCTAREAGUID".ToLower();
                                ls_columnname[6] = "TYPEGUID".ToLower();
                                ls_columnname[7] = "KINDGUID".ToLower();
                                ls_columnname[8] = "FORMGUID".ToLower();
                                ls_columnname[9] = "SAVEKINDGUID".ToLower();
                                ls_columnname[10] = "WARECODE".ToLower();
                                ls_columnname[11] = "WARENAME".ToLower();
                                ls_columnname[12] = "ONAME".ToLower();
                                ls_columnname[13] = "BAR".ToLower();
                                ls_columnname[14] = "SAVECONDIT".ToLower();
                                ls_columnname[15] = "PACKNUM".ToLower();
                                ls_columnname[16] = "PACKWEIGHT".ToLower();
                                ls_columnname[17] = "PACKDEPTH".ToLower();
                                ls_columnname[18] = "PACKHEIGHT".ToLower();
                                ls_columnname[19] = "PACKWIDTH".ToLower();
                                ls_columnname[20] = "PACKVOLUMN".ToLower();
                                ls_columnname[21] = "SINGLEWEIGHT".ToLower();
                                ls_columnname[22] = "SINGLEWIDTH".ToLower();
                                ls_columnname[23] = "SINGLEHEIGHT".ToLower();
                                ls_columnname[24] = "SINGLEDEPTH".ToLower();
                                ls_columnname[25] = "SINGLEVOLUMN".ToLower();
                                ls_columnname[26] = "MIDPACK".ToLower();
                                ls_columnname[27] = "QUALSTAND".ToLower();
                                ls_columnname[28] = "APPROVALNUM".ToLower();
                                ls_columnname[29] = "QUALDATE".ToLower();
                                ls_columnname[30] = "REMARK".ToLower();
                                ls_columnname[31] = "PYM".ToLower();
                                ls_columnname[32] = "WBM".ToLower();
                                dt = GetAllColumnsData(ls_columnname, 0);
                                if (ls_dbtype == "1")
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["wareguid"].ToString()))
                                        {
                                            dr["wareguid"] = getguid();
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["wareguid"].ToString()))
                                        {
                                            dtt = new DataTable();
                                            dtt = GetDataTable(@"select wareguid from tb_wms_wmsware 
                                    where warecode = '" + dr["warecode"].ToString() + "'");
                                            ls_guid = dtt.Rows[0][0].ToString();
                                            dr["wareguid"] = ls_guid;
                                        }
                                    }
                                }
                                ls_returnmsg = data_validation(dt, FunctionId, ls_dbtype);
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                                switch (ls_dbtype)
                                {
                                    case "1":       /*插入机构商品目录 */
                                        ls_returnmsg = data_base_do(dt, FunctionId, ls_dbtype, out dtnew);
                                        if (ls_returnmsg != "TRUE")
                                        {
                                            break;
                                        }
                                        dt = dtnew;
                                        ls_returnmsg = InsertDataTable(dt, "tb_wms_wmsware", "");
                                        break;
                                    case "2":       /*更新机构商品目录 */
                                        ls_returnmsg = data_base_do(dt, FunctionId, ls_dbtype, out dtnew);
                                        if (ls_returnmsg != "TRUE")
                                        {
                                            break;
                                        }
                                        dt = dtnew;
                                        ls_returnmsg = UpdateDataTable(dt, "tb_wms_wmsware");
                                        break;
                                    default:
                                        ls_returnmsg = "无法识别的操作类型";
                                        break;
                                }
                                break;
                            #endregion
                            #region 删除机构商品目录    2004
                            case "2004":            /*删除机构商品目录 */
                                ls_columnname = new string[1];
                                ReadXml(uploadDataXML);
                                ls_columnname[0] = "wareguid";
                                dt = GetAllColumnsData(ls_columnname, 0);
                                ls_returnmsg = data_validation(dt, FunctionId, ls_dbtype);
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                                ls_returnmsg = DeleteDataTable(dt, "tb_wms_wmsware");
                                break;
                            #endregion
                            #region 查询机构商品目录    2005
                            case "2005":            /*查询机构商品目录 */
                                ReadXml(uploadDataXML);
                                dt = GetAllColumnsData();
                                ls_returnmsg = data_validation(dt, FunctionId, ls_dbtype);
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                                ls_returnmsg = FindDataTable(dt, FunctionId, ls_dbtype, out ls_returndt);
                                break;
                            #endregion
                            #region 上传采购入库订单    2006
                            case "2006": /*上传采购入库订单*/
                                ls_columnname = new string[20];
                                ReadXml(uploadDataXML);
                                ls_columnname[0] = "ORDERGUID".ToLower();
                                ls_columnname[1] = "ORGGUID".ToLower();
                                ls_columnname[2] = "COMPANYGUID".ToLower();
                                ls_columnname[3] = "WMSCOMPANYGUID".ToLower();
                                ls_columnname[4] = "OLDORDERGUID".ToLower();
                                ls_columnname[5] = "ORDERTYPE".ToLower();
                                ls_columnname[6] = "ORDERNO".ToLower();
                                ls_columnname[7] = "WARESUM".ToLower();
                                ls_columnname[8] = "SINGLESUM".ToLower();
                                ls_columnname[9] = "PACKSUM".ToLower();
                                ls_columnname[10] = "OPERUSER".ToLower();
                                ls_columnname[11] = "OPERDATE".ToLower();
                                ls_columnname[12] = "REMARK".ToLower();
                                ls_columnname[13] = "STATUS".ToLower();
                                ls_columnname[14] = "GOUSER".ToLower();
                                ls_columnname[15] = "GODATE".ToLower();
                                ls_columnname[16] = "GETUSER".ToLower();
                                ls_columnname[17] = "GETDATE".ToLower();
                                ls_columnname[18] = "CHECKUSER".ToLower();
                                ls_columnname[19] = "CHECKDATE".ToLower();
                                dt = GetAllColumnsData(ls_columnname, 0);
                                if (ls_dbtype == "1")
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["orderguid"].ToString()))
                                        {
                                            dr["orderguid"] = getguid();
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["orderguid"].ToString()))
                                        {
                                            dtt = new DataTable();
                                            dtt = GetDataTable(@"select orderguid from Tb_Wms_BugInstoreOrder 
                                    where orderno = '" + dr["orderno"].ToString() + "'");
                                            ls_guid = dtt.Rows[0][0].ToString();
                                            dr["orderguid"] = ls_guid;
                                        }
                                    }
                                }
                                ls_returnmsg = data_validation(dt, FunctionId, ls_dbtype);
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                                switch (ls_dbtype)
                                {
                                    case "1":       /*插入*/
                                        ls_returnmsg = InsertDataTable(dt, "Tb_Wms_BugInstoreOrder", "");
                                        break;
                                    case "2":       /*更新*/
                                        ls_returnmsg = UpdateDataTable(dt, "Tb_Wms_BugInstoreOrder");
                                        break;
                                    case "3":       /*插入或更新*/
                                        ls_returnmsg = InsertOrUpdateDataTable(dt, "Tb_Wms_BugInstoreOrder", "orderguid");
                                        break;
                                    default:
                                        ls_returnmsg = "无法识别的操作类型";
                                        break;
                                }
                                break;
                            #endregion
                            #region 上传采购入库订单明细  2007
                            case "2007": /*上传采购入库订单明细*/
                                ls_columnname = new string[13];
                                ReadXml(uploadDataXML);
                                ls_columnname[0] = "ORDERDETAILGUID".ToLower();
                                ls_columnname[1] = "ORDERGUID".ToLower();
                                ls_columnname[2] = "WAREGUID".ToLower();
                                ls_columnname[3] = "BUSINESSWAREGUID".ToLower();
                                ls_columnname[4] = "WARECODE".ToLower();
                                ls_columnname[5] = "WARENAME".ToLower();
                                ls_columnname[6] = "WAREBAR".ToLower();
                                ls_columnname[7] = "SPECS".ToLower();
                                ls_columnname[8] = "FACTAREA".ToLower();
                                ls_columnname[9] = "UNITS".ToLower();
                                ls_columnname[10] = "SINGLENUM".ToLower();
                                ls_columnname[11] = "PACKNUM".ToLower();
                                ls_columnname[12] = "REMARK".ToLower();
                                dt = GetAllColumnsData(ls_columnname, 0);
                                if (ls_dbtype == "1")
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["ORDERDETAILGUID"].ToString()))
                                        {
                                            dr["ORDERDETAILGUID"] = getguid();
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["ORDERDETAILGUID"].ToString()))
                                        {
                                            ls_returnmsg = "未传输订单明细GUID，请检查";
                                            break;
                                        }
                                    }
                                }
                                ls_returnmsg = data_validation(dt, FunctionId, ls_dbtype);
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                                switch (ls_dbtype)
                                {
                                    case "1":       /*插入*/
                                        ls_returnmsg = InsertDataTable(dt, "Tb_Wms_BugInstoreOrderDetail", "");
                                        break;
                                    case "2":       /*更新*/
                                        ls_returnmsg = UpdateDataTable(dt, "Tb_Wms_BugInstoreOrderDetail");
                                        break;
                                    case "3":       /*插入或更新*/
                                        ls_returnmsg = InsertOrUpdateDataTable(dt, "Tb_Wms_BugInstoreOrderDetail", "orderdetailguid");
                                        break;
                                    default:
                                        ls_returnmsg = "无法识别的操作类型";
                                        break;
                                }
                                break;
                            #endregion
                            #region 上传销售退货入库订单  2008
                            case "2008": /*上传销售退货入库订单*/
                                ls_columnname = new string[21];
                                ReadXml(uploadDataXML);
                                ls_columnname[0] = "ORDERGUID".ToLower();
                                ls_columnname[1] = "SALEOUTSTOREORDERGUID".ToLower();
                                ls_columnname[2] = "ORGGUID".ToLower();
                                ls_columnname[3] = "BUSINESSCOMPANYGUID".ToLower();
                                ls_columnname[4] = "WMSCOMPANYGUID".ToLower();
                                ls_columnname[5] = "ORDERNO".ToLower();
                                ls_columnname[6] = "WARESUM".ToLower();
                                ls_columnname[7] = "PACKSUM".ToLower();
                                ls_columnname[8] = "SINGLESUM".ToLower();
                                ls_columnname[9] = "OPERUSER".ToLower();
                                ls_columnname[10] = "OPERDATE".ToLower();
                                ls_columnname[11] = "STATUS".ToLower();
                                ls_columnname[12] = "REMARK".ToLower();
                                ls_columnname[13] = "GOUSER".ToLower();
                                ls_columnname[14] = "GODATE".ToLower();
                                ls_columnname[15] = "GETUSER".ToLower();
                                ls_columnname[16] = "GETDATE".ToLower();
                                ls_columnname[17] = "CHECKUSER".ToLower();
                                ls_columnname[18] = "CHECKDATE".ToLower();
                                ls_columnname[19] = "ORDERTYPE".ToLower();
                                ls_columnname[20] = "OLDORDERGUID".ToLower();
                                dt = GetAllColumnsData(ls_columnname, 0);
                                if (ls_dbtype == "1")
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["orderguid"].ToString()))
                                        {
                                            dr["orderguid"] = getguid();
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["orderguid"].ToString()))
                                        {
                                            dtt = new DataTable();
                                            dtt = GetDataTable(@"select orderguid from Tb_Wms_SaleReturnInstoreOrder 
                                    where orderno = '" + dr["orderno"].ToString() + "'");
                                            ls_guid = dtt.Rows[0][0].ToString();
                                            dr["orderguid"] = ls_guid;
                                        }
                                    }
                                }
                                ls_returnmsg = data_validation(dt, FunctionId, ls_dbtype);
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                                switch (ls_dbtype)
                                {
                                    case "1":       /*插入*/
                                        ls_returnmsg = InsertDataTable(dt, "Tb_Wms_SaleReturnInstoreOrder", "");
                                        break;
                                    case "2":       /*更新*/
                                        ls_returnmsg = UpdateDataTable(dt, "Tb_Wms_SaleReturnInstoreOrder");
                                        break;
                                    case "3":       /*插入或更新*/
                                        ls_returnmsg = InsertOrUpdateDataTable(dt, "Tb_Wms_SaleReturnInstoreOrder", "orderguid");
                                        break;
                                    default:
                                        ls_returnmsg = "无法识别的操作类型";
                                        break;
                                }
                                break;
                            #endregion
                            #region 上传销售退货入库订单明细    2009
                            case "2009": /*上传销售退货入库订单明细*/
                                ls_columnname = new string[15];
                                ReadXml(uploadDataXML);
                                ls_columnname[0] = "ORDERDGUID".ToLower();
                                ls_columnname[1] = "ORDERGUID".ToLower();
                                ls_columnname[2] = "BUSINESSWAREGUID".ToLower();
                                ls_columnname[3] = "WAREGUID".ToLower();
                                ls_columnname[4] = "WARECODE".ToLower();
                                ls_columnname[5] = "WARENAME".ToLower();
                                ls_columnname[6] = "SPECS".ToLower();
                                ls_columnname[7] = "FACTAREA".ToLower();
                                ls_columnname[8] = "UNITS".ToLower();
                                ls_columnname[9] = "SINGLENUM".ToLower();
                                ls_columnname[10] = "PACKNUM".ToLower();
                                ls_columnname[11] = "REMARK".ToLower();
                                ls_columnname[12] = "BATCHS".ToLower();
                                ls_columnname[13] = "EXPDATE".ToLower();
                                ls_columnname[14] = "PRODUCTDATE".ToLower();
                                dt = GetAllColumnsData(ls_columnname, 0);
                                if (ls_dbtype == "1")
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["ORDERDGUID"].ToString()))
                                        {
                                            dr["ORDERDGUID"] = getguid();
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["ORDERDGUID"].ToString()))
                                        {
                                            ls_returnmsg = "未传输订单明细GUID，请检查";
                                            break;
                                        }
                                    }
                                }
                                ls_returnmsg = data_validation(dt, FunctionId, ls_dbtype);
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                                switch (ls_dbtype)
                                {
                                    case "1":       /*插入*/
                                        ls_returnmsg = InsertDataTable(dt, "Tb_Wms_SaleReturnInstoreOrderD", "");
                                        break;
                                    case "2":       /*更新*/
                                        ls_returnmsg = UpdateDataTable(dt, "Tb_Wms_SaleReturnInstoreOrderD");
                                        break;
                                    case "3":       /*插入或更新*/
                                        ls_returnmsg = InsertOrUpdateDataTable(dt, "Tb_Wms_SaleReturnInstoreOrderD", "orderdguid");
                                        break;
                                    default:
                                        ls_returnmsg = "无法识别的操作类型";
                                        break;
                                }
                                break;
                            #endregion
                            #region 上传销售出库订单    2010
                            case "2010": /*上传销售出库订单*/
                                ls_columnname = new string[13];
                                ReadXml(uploadDataXML);
                                ls_columnname[0] = "ORDERGUID".ToLower();
                                ls_columnname[1] = "ORGGUID".ToLower();
                                ls_columnname[2] = "COMPANYGUID".ToLower();
                                ls_columnname[3] = "WMSCOMPANYGUID".ToLower();
                                ls_columnname[4] = "ORDERTYPE".ToLower();
                                ls_columnname[5] = "ORDERCODE".ToLower();
                                ls_columnname[6] = "ORDERNAME".ToLower();
                                ls_columnname[7] = "SINGLESUM".ToLower();
                                ls_columnname[8] = "PACKSUM".ToLower();
                                ls_columnname[9] = "OPERUSER".ToLower();
                                ls_columnname[10] = "OPERDATE".ToLower();
                                ls_columnname[11] = "REMARK".ToLower();
                                ls_columnname[12] = "STATUS".ToLower();
                                dt = GetAllColumnsData(ls_columnname, 0);
                                if (ls_dbtype == "1")
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["orderguid"].ToString()))
                                        {
                                            dr["orderguid"] = getguid();
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["orderguid"].ToString()))
                                        {
                                            dtt = new DataTable();
                                            dtt = GetDataTable(@"select orderguid from Tb_Wms_SaleOutstoreOrder 
                                    where ORDERCODE = '" + dr["ORDERCODE"].ToString() + "'");
                                            ls_guid = dtt.Rows[0][0].ToString();
                                            dr["orderguid"] = ls_guid;
                                        }
                                    }
                                }
                                ls_returnmsg = data_validation(dt, FunctionId, ls_dbtype);
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                                switch (ls_dbtype)
                                {
                                    case "1":       /*插入*/
                                        ls_returnmsg = InsertDataTable(dt, "Tb_Wms_SaleOutstoreOrder", "");
                                        break;
                                    case "2":       /*更新*/
                                        ls_returnmsg = UpdateDataTable(dt, "Tb_Wms_SaleOutstoreOrder");
                                        break;
                                    case "3":       /*插入或更新*/
                                        ls_returnmsg = InsertOrUpdateDataTable(dt, "Tb_Wms_SaleOutstoreOrder", "orderguid");
                                        break;
                                    default:
                                        ls_returnmsg = "无法识别的操作类型";
                                        break;
                                }
                                break;
                            #endregion
                            #region 上传销售出库订单明细  2011
                            case "2011": /*上传销售出库订单明细*/
                                ls_columnname = new string[15];
                                ReadXml(uploadDataXML);
                                ls_columnname[0] = "DETAILGUID".ToLower();
                                ls_columnname[1] = "ORDERGUID".ToLower();
                                ls_columnname[2] = "BUSINESSWAREGUID".ToLower();
                                ls_columnname[3] = "WAREGUID".ToLower();
                                ls_columnname[4] = "WARECODE".ToLower();
                                ls_columnname[5] = "WARENAME".ToLower();
                                ls_columnname[6] = "SPECS".ToLower();
                                ls_columnname[7] = "FACTAREA".ToLower();
                                ls_columnname[8] = "UNITS".ToLower();
                                ls_columnname[9] = "BATCHS".ToLower();
                                ls_columnname[10] = "PRODUCTDATE".ToLower();
                                ls_columnname[11] = "EXPDATE".ToLower();
                                ls_columnname[12] = "SINGLENUM".ToLower();
                                ls_columnname[13] = "PACKNUM".ToLower();
                                ls_columnname[14] = "REMARK".ToLower();
                                dt = GetAllColumnsData(ls_columnname, 0);
                                if (ls_dbtype == "1")
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["DETAILGUID"].ToString()))
                                        {
                                            dr["DETAILGUID"] = getguid();
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["DETAILGUID"].ToString()))
                                        {
                                            ls_returnmsg = "未传输订单明细GUID，请检查";
                                            break;
                                        }
                                    }
                                }
                                ls_returnmsg = data_validation(dt, FunctionId, ls_dbtype);
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                                switch (ls_dbtype)
                                {
                                    case "1":       /*插入*/
                                        ls_returnmsg = data_base_do(dt, FunctionId, ls_dbtype, out dtnew);
                                        if (ls_returnmsg != "TRUE")
                                        {
                                            break;
                                        }
                                        dt = dtnew;
                                        ls_returnmsg = InsertDataTable(dt, "Tb_Wms_SaleOutstoreOrderDetail", "");
                                        if (ls_returnmsg != "TRUE")
                                        {
                                            break;
                                        }
                                        ls_orderguid = dt.Rows[0]["ORDERGUID"].ToString();
                                        ls_orgguid = GetDataTable(@"select orgguid from Tb_Wms_SaleOutstoreOrder 
                                    where orderguid = '" + ls_orderguid + "'").Rows[0][0].ToString();
                                        for (int i = 0; i < dt.Rows.Count; i++)
                                        {
                                            ls_guid = GetDataTable("select createguid() from dual").Rows[0][0].ToString();
                                            ls_returnmsg = SqlDataTable(@"INSERT INTO TB_WMS_ORDERKEEPSTORE(KEEPGUID,ORDERGUID,ORGGUID,WAREGUID,BATCHS,KEEPSINGLENUM,KEEPNUM,REMARK) VALUES('" + ls_guid + "','" + ls_orderguid + "','" + ls_orgguid + "','" + dt.Rows[i]["WAREGUID"].ToString() + "','" + dt.Rows[i]["BATCHS"].ToString() + "'," + dt.Rows[i]["SINGLENUM"].ToString() + "," + dt.Rows[i]["PACKNUM"].ToString() + ",NULL)");
                                            if (ls_returnmsg != "TRUE")
                                            {
                                                break;
                                            }
                                        }
                                        break;
                                    case "2":       /*更新*/
                                        ls_returnmsg = UpdateDataTable(dt, "Tb_Wms_SaleOutstoreOrderDetail");
                                        break;
                                    case "3":       /*插入或更新*/
                                        ls_returnmsg = InsertOrUpdateDataTable(dt, "Tb_Wms_SaleOutstoreOrderDetail", "detailguid");
                                        break;
                                    default:
                                        ls_returnmsg = "无法识别的操作类型";
                                        break;
                                }
                                break;
                            #endregion
                            #region 上传购进退货出库订单    2012
                            case "2012": /*购进退货出库订单*/
                                ls_columnname = new string[13];
                                ReadXml(uploadDataXML);
                                ls_columnname[0] = "DETAILGUID".ToLower();
                                ls_columnname[1] = "ORDERGUID".ToLower();
                                ls_columnname[2] = "ORGGUID".ToLower();
                                ls_columnname[3] = "COMPANYGUID".ToLower();
                                ls_columnname[4] = "WMSCOMPANYGUID".ToLower();
                                ls_columnname[5] = "ORDERCODE".ToLower();
                                ls_columnname[6] = "ORDERNAME".ToLower();
                                ls_columnname[7] = "SINGLESUM".ToLower();
                                ls_columnname[8] = "PACKSUM".ToLower();
                                ls_columnname[9] = "OPERUSER".ToLower();
                                ls_columnname[10] = "OPERDATE".ToLower();
                                ls_columnname[11] = "REMARK".ToLower();
                                ls_columnname[12] = "STATUS".ToLower();
                                dt = GetAllColumnsData(ls_columnname, 0);
                                if (ls_dbtype == "1")
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["DETAILGUID"].ToString()))
                                        {
                                            dr["DETAILGUID"] = getguid();
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["DETAILGUID"].ToString()))
                                        {
                                            dtt = new DataTable();
                                            dtt = GetDataTable(@"select DETAILGUID from Tb_Wms_BugReturnOutstoreOrder 
                                    where ORDERCODE = '" + dr["ORDERCODE"].ToString() + "'");
                                            ls_guid = dtt.Rows[0][0].ToString();
                                            dr["orderguid"] = ls_guid;
                                        }
                                    }
                                }
                                ls_returnmsg = data_validation(dt, FunctionId, ls_dbtype);
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                                switch (ls_dbtype)
                                {
                                    case "1":       /*插入*/
                                        ls_returnmsg = InsertDataTable(dt, "Tb_Wms_BugReturnOutstoreOrder", "");
                                        break;
                                    case "2":       /*更新*/
                                        ls_returnmsg = UpdateDataTable(dt, "Tb_Wms_BugReturnOutstoreOrder");
                                        break;
                                    case "3":       /*插入或更新*/
                                        ls_returnmsg = InsertOrUpdateDataTable(dt, "Tb_Wms_BugReturnOutstoreOrder", "detailguid");
                                        break;
                                    default:
                                        ls_returnmsg = "无法识别的操作类型";
                                        break;
                                }
                                break;
                            #endregion
                            #region 上传购进退货出库订单明细  2013
                            case "2013": /*购进退货出库订单明细*/
                                ls_columnname = new string[15];
                                ReadXml(uploadDataXML);
                                ls_columnname[0] = "DETAILGUID".ToLower();
                                ls_columnname[1] = "ORDERGUID".ToLower();
                                ls_columnname[2] = "BUSINESSWAREGUID".ToLower();
                                ls_columnname[3] = "WAREGUID".ToLower();
                                ls_columnname[4] = "WARENO".ToLower();
                                ls_columnname[5] = "WARENAME".ToLower();
                                ls_columnname[6] = "SPECS".ToLower();
                                ls_columnname[7] = "FACTAREA".ToLower();
                                ls_columnname[8] = "UNITS".ToLower();
                                ls_columnname[9] = "BATCHS".ToLower();
                                ls_columnname[10] = "PRODUCTDATE".ToLower();
                                ls_columnname[11] = "EXPDATE".ToLower();
                                ls_columnname[12] = "SINGLENUM".ToLower();
                                ls_columnname[13] = "PACKNUM".ToLower();
                                ls_columnname[14] = "REMARK".ToLower();
                                dt = GetAllColumnsData(ls_columnname, 0);
                                if (ls_dbtype == "1")
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["DETAILGUID"].ToString()))
                                        {
                                            dr["DETAILGUID"] = getguid();
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["DETAILGUID"].ToString()))
                                        {
                                            ls_returnmsg = "未传输订单明细GUID，请检查";
                                            break;
                                        }
                                    }
                                }
                                ls_returnmsg = data_validation(dt, FunctionId, ls_dbtype);
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                                switch (ls_dbtype)
                                {
                                    case "1":       /*插入*/
                                        ls_returnmsg = data_base_do(dt, FunctionId, ls_dbtype, out dtnew);
                                        if (ls_returnmsg != "TRUE")
                                        {
                                            break;
                                        }
                                        dt = dtnew;
                                        ls_returnmsg = InsertDataTable(dt, "Tb_Wms_BugReturnOutstoreOrderD", "");
                                        if (ls_returnmsg != "TRUE")
                                        {
                                            break;
                                        }
                                        ls_orderguid = dt.Rows[0]["ORDERGUID"].ToString();
                                        ls_orgguid = GetDataTable(@"select orgguid from Tb_Wms_BugReturnOutstoreOrder 
                                    where orderguid = '" + ls_orderguid + "'").Rows[0][0].ToString();
                                        for (int i = 0; i < dt.Rows.Count; i++)
                                        {
                                            ls_guid = GetDataTable("select createguid() from dual").Rows[0][0].ToString();
                                            ls_returnmsg = SqlDataTable(@"INSERT INTO TB_WMS_ORDERKEEPSTORE(KEEPGUID,ORDERGUID,ORGGUID,WAREGUID,BATCHS,KEEPSINGLENUM,KEEPNUM,REMARK) VALUES('" + ls_guid + "','" + ls_orderguid + "','" + ls_orgguid + "','" + dt.Rows[i]["WAREGUID"].ToString() + "','" + dt.Rows[i]["BATCHS"].ToString() + "'," + dt.Rows[i]["SINGLENUM"].ToString() + "," + dt.Rows[i]["PACKNUM"].ToString() + ",NULL)");
                                            if (ls_returnmsg != "TRUE")
                                            {
                                                break;
                                            }
                                        }
                                        break;
                                    case "2":       /*更新*/
                                        ls_returnmsg = UpdateDataTable(dt, "Tb_Wms_BugReturnOutstoreOrderD");
                                        break;
                                    case "3":       /*插入或更新*/
                                        ls_returnmsg = InsertOrUpdateDataTable(dt, "Tb_Wms_BugReturnOutstoreOrderD", "detailguid");
                                        break;
                                    default:
                                        ls_returnmsg = "无法识别的操作类型";
                                        break;
                                }
                                break;
                            #endregion
                            #region 上传机构目录  2014
                            case "2014": /*上传机构目录 */
                                ls_columnname = new string[12];
                                ReadXml(uploadDataXML);
                                ls_columnname[0] = "ORGGUID".ToLower();
                                ls_columnname[1] = "NATUREGUID".ToLower();
                                ls_columnname[2] = "LEVELGUID".ToLower();
                                ls_columnname[3] = "ORGCODE".ToLower();
                                ls_columnname[4] = "ORGNAME".ToLower();
                                ls_columnname[5] = "ADDRESS".ToLower();
                                ls_columnname[6] = "CONTACT".ToLower();
                                ls_columnname[7] = "TEL".ToLower();
                                ls_columnname[8] = "MAIL".ToLower();
                                ls_columnname[9] = "QQ".ToLower();
                                ls_columnname[10] = "FAX".ToLower();
                                ls_columnname[11] = "REMARK".ToLower();
                                dt = GetAllColumnsData(ls_columnname, 0);
                                if (ls_dbtype == "1")
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["orgguid"].ToString()))
                                        {
                                            dr["orgguid"] = getguid();
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["orgguid"].ToString()))
                                        {
                                            dtt = new DataTable();
                                            dtt = GetDataTable(@"select orgguid from tb_wms_org
                                    where orgcode = '" + dr["orgcode"].ToString() + "'");
                                            ls_guid = dtt.Rows[0][0].ToString();
                                            dr["orgguid"] = ls_guid;
                                        }
                                    }
                                }
                                ls_returnmsg = data_validation(dt, FunctionId, ls_dbtype);
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                                switch (ls_dbtype)
                                {
                                    case "1":       /*插入机构目录 */
                                        ls_returnmsg = InsertDataTable(dt, "tb_wms_org", "");
                                        break;
                                    case "2":       /*更新机构目录 */
                                        ls_returnmsg = UpdateDataTable(dt, "tb_wms_org");
                                        break;
                                    case "3":       /*插入或更新*/
                                        ls_returnmsg = InsertOrUpdateDataTable(dt, "tb_wms_org", "orgguid");
                                        break;
                                    default:
                                        ls_returnmsg = "无法识别的操作类型";
                                        break;
                                }
                                break;
                            #endregion
                            #region 上传往来单位目录    2015
                            case "2015": /*上传往来单位目录 */
                                ls_columnname = new string[15];
                                ReadXml(uploadDataXML);
                                ls_columnname[0] = "COMPANYGUID".ToLower();
                                ls_columnname[1] = "AREAGUID".ToLower();
                                ls_columnname[2] = "NATUREGUID".ToLower();
                                ls_columnname[3] = "KINDGUID".ToLower();
                                ls_columnname[4] = "COMPANYCODE".ToLower();
                                ls_columnname[5] = "COMPANYNAME".ToLower();
                                ls_columnname[6] = "SNAME".ToLower();
                                ls_columnname[7] = "CONTACT".ToLower();
                                ls_columnname[8] = "TEL".ToLower();
                                ls_columnname[9] = "ADDRESS".ToLower();
                                ls_columnname[10] = "REGNO".ToLower();
                                ls_columnname[11] = "TAXNO".ToLower();
                                ls_columnname[12] = "OPERDATE".ToLower();
                                ls_columnname[13] = "PYM".ToLower();
                                ls_columnname[14] = "WBM".ToLower();
                                dt = GetAllColumnsData(ls_columnname, 0);
                                if (ls_dbtype == "1")
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["companyguid"].ToString()))
                                        {
                                            dr["companyguid"] = getguid();
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (string.IsNullOrEmpty(dr["companyguid"].ToString()))
                                        {
                                            dtt = new DataTable();
                                            dtt = GetDataTable(@"select companyguid from tb_wms_company 
                                    where companycode = '" + dr["companycode"].ToString() + "'");
                                            ls_guid = dtt.Rows[0][0].ToString();
                                            dr["companyguid"] = ls_guid;
                                        }
                                    }
                                }
                                ls_returnmsg = data_validation(dt, FunctionId, ls_dbtype);
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                                switch (ls_dbtype)
                                {
                                    case "1":       /*插入往来单位目录 */
                                        ls_returnmsg = InsertDataTable(dt, "tb_wms_company", "");
                                        break;
                                    case "2":       /*更新往来单位目录 */
                                        ls_returnmsg = UpdateDataTable(dt, "tb_wms_company");
                                        break;
                                    case "3":       /*插入或更新*/
                                        ls_returnmsg = InsertOrUpdateDataTable(dt, "tb_wms_company", "companyguid");
                                        break;
                                    default:
                                        ls_returnmsg = "无法识别的操作类型";
                                        break;
                                }
                                break;
                            #endregion
                            #region 查询已上架单据目录    2017
                            case "2017":            /*查询已上架单据目录 */
                                ReadXml(uploadDataXML);
                                dt = GetAllColumnsData();
                                ls_returnmsg = data_validation(dt, FunctionId, ls_dbtype);
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                                ls_returnmsg = FindDataTable(dt, FunctionId, ls_dbtype, out ls_returndt);
                                break;
                            #endregion
                            #region 查询已上架单据明细目录    2018
                            case "2018":            /*查询已上架单据明细目录 */
                                ReadXml(uploadDataXML);
                                dt = GetAllColumnsData();
                                ls_returnmsg = data_validation(dt, FunctionId, ls_dbtype);
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                                ls_returnmsg = FindDataTable(dt, FunctionId, ls_dbtype, out ls_returndt);
                                break;
                            #endregion
                            #region 查询盘赢亏单据目录    2019
                            case "2019":            /*查询盘赢亏单据目录 */
                                ReadXml(uploadDataXML);
                                dt = GetAllColumnsData();
                                ls_returnmsg = data_validation(dt, FunctionId, ls_dbtype);
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                                ls_returnmsg = FindDataTable(dt, FunctionId, ls_dbtype, out ls_returndt);
                                break;
                            #endregion
                            #region 查询盘赢亏单据明细目录   2020
                            case "2020":            /*查询盘赢亏单据明细目录 */
                                ReadXml(uploadDataXML);
                                dt = GetAllColumnsData();
                                ls_returnmsg = data_validation(dt, FunctionId, ls_dbtype);
                                if (ls_returnmsg != "TRUE")
                                {
                                    break;
                                }
                                ls_returnmsg = FindDataTable(dt, FunctionId, ls_dbtype, out ls_returndt);
                                break;
                            #endregion
                            default:
                                ls_returnmsg = "无法识别的操作类型";
                                break;
                        }
                        #region 返回
                        if (ls_returnmsg != "" && ls_returnmsg != "TRUE")
                        {
                            RollbackTrans();
                            WriteXml(FunctionId, "1000", ls_returnmsg);
                            WriteLog(OperUserID, DateTime.Now, requestParmXML, retDataXML, uploadDataXML, doc.InnerXml, "BUSINESS", null, FunctionId, "1000");
                        }
                        else
                        {
                            CommitTrans();
                            if (ls_returndt != "")
                            {
                                retDataXML = ls_returndt;
                                WriteXml(FunctionId, "0000", "处理成功");
                                WriteLog(OperUserID, DateTime.Now, requestParmXML, retDataXML, uploadDataXML, doc.InnerXml, "BUSINESS", null, FunctionId, "0000");
                            }
                            else
                            {
                                WriteXml(FunctionId, "0000", "处理成功");
                                WriteLog(OperUserID, DateTime.Now, requestParmXML, retDataXML, uploadDataXML, doc.InnerXml, "BUSINESS", null, FunctionId, "0000");
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        RollbackTrans();
                        WriteXml(FunctionId, "2001", ls_returnmsg);
                        WriteLog(OperUserID, DateTime.Now, requestParmXML, retDataXML, uploadDataXML, doc.InnerXml, "BUSINESS", null, FunctionId, "2001");
                    }
                }
                else
                {
                    RollbackTrans();
                    WriteXml(FunctionId, "9100", ls_msg);
                    WriteLog(OperUserID, DateTime.Now, requestParmXML, retDataXML, uploadDataXML, doc.InnerXml, "BUSINESS", null, FunctionId, "9100");
                }
                return (doc.InnerXml);
            }
            catch (Exception ex)
            {
                RollbackTrans();
                ls_returnmsg = ex.Message.ToString();
                WriteXml(FunctionId, "1000", ls_returnmsg);
                WriteLog(OperUserID, DateTime.Now, requestParmXML, ls_returnmsg, uploadDataXML, doc.InnerXml, "BUSINESS", null, FunctionId, "1000");
                return (doc.InnerXml);
            }
        }

        [WebMethod(Description = "电子标签调用接口处理的方法")]
        public string LabelInterface(string requestParmXML, string uploadDataXML, out string retDataXML)
        {
            #region 变量
            string ls_msg = "";
            string ls_returnmsg = "";
            string ls_returndt = "";
            string ls_order = "";
            string ls_model = "";
            string ls_businessid = "";
            string ls_labeladdress = "";
            string ls_waresum = "";
            string ls_warelocation = "";
            string ls_string = "";
            string ls_xml;
            string ls_warenum = "";
            DataTable dt = new DataTable();
            retDataXML = "";
            try
            {
                ReadXml(requestParmXML);
                OrgCode = org_no();
                WmsCode = wms_no();
                controlip = wms_dbtype();
                InterfaceUserID = user_no();
                InterfacePassWord = user_pass();
                OperPassWord = oper_pass();
                Opertime = oper_time();
                Interface = oper_interface();
                FunctionId = function_id();
            #endregion

                #region 电子标签内部处理，不需要验证、数据库连接等
                if (Interface != "MIAWMS")
                {
                    if (String2Base64(UserMd5(FunctionId + Opertime)) != Interface)
                    {
                        WriteXml("0001", "2000", "通讯密匙验证失败");
                        return (doc.InnerXml);
                    }
                }
                //if (Interface != "MIAWMS")
                //{
                if (!getcnParms(OrgCode, out ls_msg))
                {
                    WriteXml(FunctionId, "9100", ls_msg);
                    return (doc.InnerXml);
                }
                //}
                //if (Interface != "MIAWMS")
                //{
                ls_returnmsg = checkUserValid(FunctionId, WmsCode, InterfaceUserID, InterfacePassWord, OperUserID, OperPassWord, ls_msg, 0 /*业务系统传2;PDA传1*/, 0, out ls_xml);
                //}
                //else
                //{
                //    ls_returnmsg = "TRUE";
                //}
                if (ls_returnmsg != "TRUE")
                {
                    WriteXml(FunctionId, "2001", ls_returnmsg);
                    //return doc.InnerText;
                    return (doc.InnerXml);
                }
                #endregion

                switch (FunctionId)
                {
                    #region 上传预处理命令3001
                    case "3001":
                        ls_returnmsg = TransAction("3001@@" + controlip);
                        if (ls_returnmsg.Substring(0, 8) == "7EAA0101")
                        {
                            ls_returnmsg = "TRUE";
                        }
                        else
                        {
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0102")
                            {
                                ls_returnmsg = "执行失败";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0103")
                            {
                                ls_returnmsg = "任务已存在";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0104")
                            {
                                ls_returnmsg = "任务不匹配";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0105")
                            {
                                ls_returnmsg = "执行失败，控制正在执行其他命令";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0106")
                            {
                                ls_returnmsg = "发送业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0107")
                            {
                                ls_returnmsg = "轮询业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0108")
                            {
                                ls_returnmsg = "当前标签任务执行成功";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0109")
                            {
                                ls_returnmsg = "执行成功,业务已改动 （当前物品数量不等于下发业务数量）";
                                break;
                            }
                        }
                        break;
                    #endregion

                    #region 上传拣货命令3002
                    case "3002":
                        ReadXml(uploadDataXML);
                        ls_model = oper_model();
                        ls_businessid = oper_businessid().PadLeft(8, '0');
                        //ls_labeladdress = oper_labeladdress().PadLeft(2, '0');
                        ls_labeladdress = (Convert.ToInt32(oper_labeladdress()).ToString("X2")).ToUpper();//十进制转十六进制
                        dt = GetAllColumnsData();
                        ls_waresum = dt.Rows.Count.ToString("X2").ToUpper();
                        if (ls_model == "0")//直接转发指令
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                ls_order = dt.Rows[i][0].ToString();
                                ls_returnmsg = TransAction("3002@@" + controlip + "@@" + ls_order);
                                if (ls_returnmsg.Substring(0, 18) == "7EAA02" + ls_businessid + ls_labeladdress + "01")
                                {
                                    ls_returnmsg = "TRUE";
                                }
                                else
                                {
                                    //这里可能要加复位指令上传
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA02" + ls_businessid + ls_labeladdress + "02")
                                    {
                                        ls_returnmsg = "执行失败";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA02" + ls_businessid + ls_labeladdress + "03")
                                    {
                                        ls_returnmsg = "任务已存在";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA02" + ls_businessid + ls_labeladdress + "04")
                                    {
                                        ls_returnmsg = "任务不匹配";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA02" + ls_businessid + ls_labeladdress + "05")
                                    {
                                        ls_returnmsg = "执行失败，控制正在执行其他命令";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA02" + ls_businessid + ls_labeladdress + "06")
                                    {
                                        ls_returnmsg = "发送业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA02" + ls_businessid + ls_labeladdress + "07")
                                    {
                                        ls_returnmsg = "轮询业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA02" + ls_businessid + ls_labeladdress + "08")
                                    {
                                        ls_returnmsg = "当前标签任务执行成功";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA02" + ls_businessid + ls_labeladdress + "09")
                                    {
                                        ls_returnmsg = "执行成功,业务已改动 （当前物品数量不等于下发业务数量）";
                                        break;
                                    }
                                }
                            }
                        }
                        else if (ls_model == "1")//生成指令发送
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                ls_warelocation = Convert.ToInt32(dt.Rows[i]["warepost"]).ToString("X2").ToUpper();
                                ls_warenum = Convert.ToInt32(dt.Rows[i]["warenum"]).ToString("X4").ToUpper();
                                ls_string += ls_warelocation + ls_warenum;
                            }
                            ls_order = ChangeOrder("02", ls_businessid, ls_labeladdress, ls_waresum, ls_string);
                            ls_returnmsg = TransAction("3002@@" + controlip + "@@7EAA" + ls_order + "7E");
                            if (ls_returnmsg.Substring(0, 18) == "7EAA02" + ls_businessid + ls_labeladdress + "01")
                            {
                                ls_returnmsg = "TRUE";
                            }
                            else
                            {
                                //这里可能要加复位指令上传
                                if (ls_returnmsg.Substring(0, 18) == "7EAA02" + ls_businessid + ls_labeladdress + "02")
                                {
                                    ls_returnmsg = "执行失败";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA02" + ls_businessid + ls_labeladdress + "03")
                                {
                                    ls_returnmsg = "任务已存在";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA02" + ls_businessid + ls_labeladdress + "04")
                                {
                                    ls_returnmsg = "任务不匹配";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA02" + ls_businessid + ls_labeladdress + "05")
                                {
                                    ls_returnmsg = "执行失败，控制正在执行其他命令";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA02" + ls_businessid + ls_labeladdress + "06")
                                {
                                    ls_returnmsg = "发送业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA02" + ls_businessid + ls_labeladdress + "07")
                                {
                                    ls_returnmsg = "轮询业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA02" + ls_businessid + ls_labeladdress + "08")
                                {
                                    ls_returnmsg = "当前标签任务执行成功";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA02" + ls_businessid + ls_labeladdress + "09")
                                {
                                    ls_returnmsg = "执行成功,业务已改动 （当前物品数量不等于下发业务数量）";
                                    break;
                                }
                            }
                        }
                        else
                        {
                            ls_returnmsg = "模式错误！";
                        }
                        break;
                    #endregion

                    #region 上传盘点命令3003
                    case "3003":
                        ReadXml(uploadDataXML);
                        ls_model = oper_model();
                        ls_businessid = oper_businessid().PadLeft(8, '0');
                        //ls_labeladdress = oper_labeladdress().PadLeft(2, '0');
                        ls_labeladdress = (Convert.ToInt32(oper_labeladdress()).ToString("X2")).ToUpper();//十进制转十六进制
                        dt = GetAllColumnsData();
                        ls_waresum = dt.Rows.Count.ToString("X2").ToUpper();
                        if (ls_model == "0")
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                ls_order = dt.Rows[i][0].ToString();
                                ls_returnmsg = TransAction("3003@@" + controlip + "@@" + ls_order);
                                if (ls_returnmsg.Substring(0, 18) == "7EAA03" + ls_businessid + ls_labeladdress + "01")
                                {
                                    ls_returnmsg = "TRUE";
                                }
                                else
                                {
                                    //这里可能要加复位指令上传
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA03" + ls_businessid + ls_labeladdress + "02")
                                    {
                                        ls_returnmsg = "执行失败";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA03" + ls_businessid + ls_labeladdress + "03")
                                    {
                                        ls_returnmsg = "任务已存在";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA03" + ls_businessid + ls_labeladdress + "04")
                                    {
                                        ls_returnmsg = "任务不匹配";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA03" + ls_businessid + ls_labeladdress + "05")
                                    {
                                        ls_returnmsg = "执行失败，控制正在执行其他命令";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA03" + ls_businessid + ls_labeladdress + "06")
                                    {
                                        ls_returnmsg = "发送业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA03" + ls_businessid + ls_labeladdress + "07")
                                    {
                                        ls_returnmsg = "轮询业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA03" + ls_businessid + ls_labeladdress + "08")
                                    {
                                        ls_returnmsg = "当前标签任务执行成功";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA03" + ls_businessid + ls_labeladdress + "09")
                                    {
                                        ls_returnmsg = "执行成功,业务已改动 （当前物品数量不等于下发业务数量）";
                                        break;
                                    }
                                }
                            }
                        }
                        else if (ls_model == "1")
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                ls_warelocation = Convert.ToInt32(dt.Rows[i]["warepost"]).ToString("X2").ToUpper();
                                ls_warenum = Convert.ToInt32(dt.Rows[i]["warenum"]).ToString("X4").ToUpper();
                                ls_string += ls_warelocation + ls_warenum;
                            }
                            ls_order = ChangeOrder("03", ls_businessid, ls_labeladdress, ls_waresum, ls_string);
                            ls_returnmsg = TransAction("3003@@" + controlip + "@@7EAA" + ls_order + "7E");
                            if (ls_returnmsg.Substring(0, 18) == "7EAA03" + ls_businessid + ls_labeladdress + "01")
                            {
                                ls_returnmsg = "TRUE";
                            }
                            else
                            {
                                //这里可能要加复位指令上传
                                if (ls_returnmsg.Substring(0, 18) == "7EAA03" + ls_businessid + ls_labeladdress + "02")
                                {
                                    ls_returnmsg = "执行失败";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA03" + ls_businessid + ls_labeladdress + "03")
                                {
                                    ls_returnmsg = "任务已存在";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA03" + ls_businessid + ls_labeladdress + "04")
                                {
                                    ls_returnmsg = "任务不匹配";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA03" + ls_businessid + ls_labeladdress + "05")
                                {
                                    ls_returnmsg = "执行失败，控制正在执行其他命令";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA03" + ls_businessid + ls_labeladdress + "06")
                                {
                                    ls_returnmsg = "发送业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA03" + ls_businessid + ls_labeladdress + "07")
                                {
                                    ls_returnmsg = "轮询业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA03" + ls_businessid + ls_labeladdress + "08")
                                {
                                    ls_returnmsg = "当前标签任务执行成功";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA03" + ls_businessid + ls_labeladdress + "09")
                                {
                                    ls_returnmsg = "执行成功,业务已改动 （当前物品数量不等于下发业务数量）";
                                    break;
                                }
                            }
                        }
                        else
                        {
                            ls_returnmsg = "模式错误！";
                        }
                        break;
                    #endregion

                    #region 上传补货命令3004
                    case "3004":
                        ReadXml(uploadDataXML);
                        ls_model = oper_model();
                        ls_businessid = oper_businessid().PadLeft(8, '0');
                        ls_labeladdress = (Convert.ToInt32(oper_labeladdress()).ToString("X2")).ToUpper();//十进制转十六进制
                        dt = GetAllColumnsData();
                        ls_waresum = dt.Rows.Count.ToString("X2").ToUpper();
                        if (ls_model == "0")
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                ls_order = dt.Rows[i][0].ToString();
                                ls_returnmsg = TransAction("3004@@" + controlip + "@@" + ls_order);
                                if (ls_returnmsg.Substring(0, 18) == "7EAA04" + ls_businessid + ls_labeladdress + "01")
                                {
                                    ls_returnmsg = "TRUE";
                                }
                                else
                                {
                                    //这里可能要加复位指令上传
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA04" + ls_businessid + ls_labeladdress + "02")
                                    {
                                        ls_returnmsg = "执行失败";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA04" + ls_businessid + ls_labeladdress + "03")
                                    {
                                        ls_returnmsg = "任务已存在";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA04" + ls_businessid + ls_labeladdress + "04")
                                    {
                                        ls_returnmsg = "任务不匹配";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA04" + ls_businessid + ls_labeladdress + "05")
                                    {
                                        ls_returnmsg = "执行失败，控制正在执行其他命令";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA04" + ls_businessid + ls_labeladdress + "06")
                                    {
                                        ls_returnmsg = "发送业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA04" + ls_businessid + ls_labeladdress + "07")
                                    {
                                        ls_returnmsg = "轮询业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA04" + ls_businessid + ls_labeladdress + "08")
                                    {
                                        ls_returnmsg = "当前标签任务执行成功";
                                        break;
                                    }
                                    if (ls_returnmsg.Substring(0, 18) == "7EAA04" + ls_businessid + ls_labeladdress + "09")
                                    {
                                        ls_returnmsg = "执行成功,业务已改动 （当前物品数量不等于下发业务数量）";
                                        break;
                                    }
                                }
                            }
                        }
                        else if (ls_model == "1")
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                ls_warelocation = Convert.ToInt32(dt.Rows[i]["warepost"]).ToString("X2").ToUpper();
                                ls_warenum = Convert.ToInt32(dt.Rows[i]["warenum"]).ToString("X4").ToUpper();
                                ls_string += ls_warelocation + ls_warenum;
                            }
                            ls_order = ChangeOrder("04", ls_businessid, ls_labeladdress, ls_waresum, ls_string);
                            ls_returnmsg = TransAction("3004@@" + controlip + "@@7EAA" + ls_order + "7E");
                            if (ls_returnmsg.Substring(0, 18) == "7EAA04" + ls_businessid + ls_labeladdress + "01")
                            {
                                ls_returnmsg = "TRUE";
                            }
                            else
                            {
                                //这里可能要加复位指令上传
                                if (ls_returnmsg.Substring(0, 18) == "7EAA04" + ls_businessid + ls_labeladdress + "02")
                                {
                                    ls_returnmsg = "执行失败";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA04" + ls_businessid + ls_labeladdress + "03")
                                {
                                    ls_returnmsg = "任务已存在";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA04" + ls_businessid + ls_labeladdress + "04")
                                {
                                    ls_returnmsg = "任务不匹配";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA04" + ls_businessid + ls_labeladdress + "05")
                                {
                                    ls_returnmsg = "执行失败，控制正在执行其他命令";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA04" + ls_businessid + ls_labeladdress + "06")
                                {
                                    ls_returnmsg = "发送业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA04" + ls_businessid + ls_labeladdress + "07")
                                {
                                    ls_returnmsg = "轮询业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA04" + ls_businessid + ls_labeladdress + "08")
                                {
                                    ls_returnmsg = "当前标签任务执行成功";
                                    break;
                                }
                                if (ls_returnmsg.Substring(0, 18) == "7EAA04" + ls_businessid + ls_labeladdress + "09")
                                {
                                    ls_returnmsg = "执行成功,业务已改动 （当前物品数量不等于下发业务数量）";
                                    break;
                                }
                            }
                        }
                        else
                        {
                            ls_returnmsg = "模式错误！";
                        }
                        break;
                    #endregion

                    #region 上传完成下发命令3005
                    case "3005":
                        ls_returnmsg = TransAction("3005@@" + controlip);
                        if (ls_returnmsg.Substring(0, 8) == "7EAA0501")
                        {
                            ls_returnmsg = "TRUE";
                        }
                        else
                        {
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0502")
                            {
                                ls_returnmsg = "执行失败";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0503")
                            {
                                ls_returnmsg = "任务已存在";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0504")
                            {
                                ls_returnmsg = "任务不匹配";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0505")
                            {
                                ls_returnmsg = "执行失败，控制正在执行其他命令";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0506")
                            {
                                ls_returnmsg = "发送业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0507")
                            {
                                ls_returnmsg = "轮询业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0508")
                            {
                                ls_returnmsg = "当前标签任务执行成功";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0509")
                            {
                                ls_returnmsg = "执行成功,业务已改动 （当前物品数量不等于下发业务数量）";
                                break;
                            }
                        }
                        break;
                    #endregion

                    #region 上传电子标签自检命令3006
                    case "3006":
                        ls_returnmsg = TransAction("3006@@" + controlip);
                        if (ls_returnmsg.Substring(0, 8) == "7EAA0601")
                        {
                            ls_returnmsg = "TRUE";
                        }
                        else
                        {
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0602")
                            {
                                ls_returnmsg = "执行失败";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0603")
                            {
                                ls_returnmsg = "任务已存在";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0604")
                            {
                                ls_returnmsg = "任务不匹配";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0605")
                            {
                                ls_returnmsg = "执行失败，控制正在执行其他命令";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0606")
                            {
                                ls_returnmsg = "发送业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0607")
                            {
                                ls_returnmsg = "轮询业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0608")
                            {
                                ls_returnmsg = "当前标签任务执行成功";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0609")
                            {
                                ls_returnmsg = "执行成功,业务已改动 （当前物品数量不等于下发业务数量）";
                                break;
                            }
                        }
                        break;
                    #endregion

                    #region 上传显示电子标签地址命令3007
                    case "3007":
                        ls_returnmsg = TransAction("3007@@" + controlip);
                        if (ls_returnmsg.Substring(0, 8) == "7EAA0701")
                        {
                            ls_returnmsg = "TRUE";
                        }
                        else
                        {
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0702")
                            {
                                ls_returnmsg = "执行失败";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0703")
                            {
                                ls_returnmsg = "任务已存在";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0704")
                            {
                                ls_returnmsg = "任务不匹配";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0705")
                            {
                                ls_returnmsg = "执行失败，控制正在执行其他命令";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0706")
                            {
                                ls_returnmsg = "发送业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0707")
                            {
                                ls_returnmsg = "轮询业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0708")
                            {
                                ls_returnmsg = "当前标签任务执行成功";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0709")
                            {
                                ls_returnmsg = "执行成功,业务已改动 （当前物品数量不等于下发业务数量）";
                                break;
                            }
                        }
                        break;
                    #endregion

                    #region 上传关闭电子标签地址显示命令3008
                    case "3008":
                        ls_returnmsg = TransAction("3008@@" + controlip);
                        if (ls_returnmsg.Substring(0, 8) == "7EAA0801")
                        {
                            ls_returnmsg = "TRUE";
                        }
                        else
                        {
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0802")
                            {
                                ls_returnmsg = "执行失败";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0803")
                            {
                                ls_returnmsg = "任务已存在";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0804")
                            {
                                ls_returnmsg = "任务不匹配";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0805")
                            {
                                ls_returnmsg = "执行失败，控制正在执行其他命令";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0806")
                            {
                                ls_returnmsg = "发送业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0807")
                            {
                                ls_returnmsg = "轮询业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0808")
                            {
                                ls_returnmsg = "当前标签任务执行成功";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0809")
                            {
                                ls_returnmsg = "执行成功,业务已改动 （当前物品数量不等于下发业务数量）";
                                break;
                            }
                        }
                        break;
                    #endregion

                    #region 上传系统复位命令3009
                    case "3009":
                        ls_returnmsg = TransAction("3009@@" + controlip);
                        if (ls_returnmsg.Substring(0, 8) == "7EAA0901")
                        {
                            ls_returnmsg = "TRUE";
                        }
                        else
                        {
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0902")
                            {
                                ls_returnmsg = "执行失败";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0903")
                            {
                                ls_returnmsg = "任务已存在";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0904")
                            {
                                ls_returnmsg = "任务不匹配";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0905")
                            {
                                ls_returnmsg = "执行失败，控制正在执行其他命令";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0906")
                            {
                                ls_returnmsg = "发送业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0907")
                            {
                                ls_returnmsg = "轮询业务电子标签未响应  （电子标签故障 或者 线路故障）";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0908")
                            {
                                ls_returnmsg = "当前标签任务执行成功";
                                break;
                            }
                            if (ls_returnmsg.Substring(0, 8) == "7EAA0909")
                            {
                                ls_returnmsg = "执行成功,业务已改动 （当前物品数量不等于下发业务数量）";
                                break;
                            }
                        }
                        break;
                    #endregion
                    default:
                        break;
                }
                #region 返回结果处理
                if (ls_returnmsg != "TRUE")
                {
                    RollbackTrans();
                    WriteXml(FunctionId, "1000", ls_returnmsg);
                    WriteLog(OperUserID, DateTime.Now, requestParmXML, retDataXML, uploadDataXML, doc.InnerXml, null, "LABEL", FunctionId, "1000");
                }
                else
                {
                    CommitTrans();
                    if (ls_returndt != "")
                    {
                        WriteXml(FunctionId, "0000", "处理成功");
                        retDataXML = ls_returndt;
                    }
                    else
                    {
                        WriteXml(FunctionId, "0000", "处理成功");
                    }
                    WriteLog(OperUserID, DateTime.Now, requestParmXML, retDataXML, uploadDataXML, doc.InnerXml, null, "LABEL", FunctionId, "0000");
                }
                return (doc.InnerXml);
                #endregion
            }
            catch (Exception ex)
            {
                RollbackTrans();
                ls_returnmsg = ex.Message.ToString();
                WriteXml(FunctionId, "1000", ls_returnmsg);
                WriteLog(OperUserID, DateTime.Now, requestParmXML, ls_returnmsg, uploadDataXML, doc.InnerXml, null, "LABEL", FunctionId, "1000");
                return (doc.InnerXml);
            }
        }
        #endregion
    }
}


