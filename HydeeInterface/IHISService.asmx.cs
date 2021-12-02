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
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Web.Services.Description;
using System.CodeDom;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
namespace IHISService
{
    #region 初始设置
    [WebService(Namespace = "hydee.cn")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
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
        private string OrgCode = "002";


        /*
         * / <summary>
         * / 当前用户所在的机构名称
         * / </summary>
         */
        private string OrgName = "";

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
                    mess = "Data Source=(DESCRIPTION =    (ADDRESS_LIST =      (ADDRESS = (PROTOCOL = TCP)(HOST = " + Host + ")(PORT = " + Port + "))    )    (CONNECT_DATA =      (SERVER = DEDICATED)      (SERVICE_NAME = " + Server_Name + ")    )  );Persist Security Info=True;User ID=" + UserID + ";Password=" + PassWord + ";";
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
            //Open();
            //BeginTrans();
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
                        param = cmd.Parameters.Add(new OracleParameter(inparam[i], OracleDbType.Int32, 8));
                    }
                    else if (inparamtype[i] == "varchar")
                    {
                        param = cmd.Parameters.Add(new OracleParameter(inparam[i], OracleDbType.Varchar2, 400));
                    }
                    param.Direction = ParameterDirection.Input;
                    param.Value = inparamvalue[i];
                }

                for (int i = 0; i < outparam.Length; i++)
                {
                    if (outparamtype[i] == "int")
                    {
                        param = cmd.Parameters.Add(new OracleParameter(outparam[i], OracleDbType.Int32, 4));
                    }
                    else if (outparamtype[i] == "varchar")
                    {
                        param = cmd.Parameters.Add(new OracleParameter(outparam[i], OracleDbType.Varchar2, 400));
                    }
                    else if (outparamtype[i] == "cursor")
                    {
                        param = cmd.Parameters.Add(new OracleParameter(outparam[i], OracleDbType.RefCursor, 400));
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
                    dtcur = GetDataTable("SELECT PackNum FROM tb_wms_wmsware Where WAREGUID = '" + ls_wareguid + "'");
                    l_PackNumnew = dtcur.Rows.Count > 0 ? Convert.ToInt32(dtcur.Rows[0][0]) : 0;
                    l_realsinglesum = l_realpack * l_PackNumnew + l_realsingle;
                    l_data = Convert.ToInt32(dt.Rows[i]["SingleNum"]);
                    if (l_data > l_realsinglesum)
                    {
                        return "商品【" + dt.Rows[i]["warename"].ToString() + "】库存不足";
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

        /// <summary>
        /// 解析JObject对象的一个键值
        /// </summary>
        /// <param name="jObject">JObject对象</param>
        /// <param name="key">键名（区分大小写）</param>
        /// <returns>字符串格式的键值</returns>
        public static JToken GetValue(JObject jObject, string key = "data")
        {
            if (jObject == null)
                return null;
            JToken aToken = null;
            jObject.TryGetValue(key, out aToken);
            return aToken;
        }

        public string GetRetJson(string resultcode, string errtext, List<RetKCJson> ypkc, List<RetRKJson> rkmx, List<RetCKJson> ckmx, List<RetYPJson> ypxx)
        {
            string ls_retmsg = null;
            RetBodyHJson _RetBodyHeadJson = null;
            JavaScriptSerializer serializer = null;
            _RetBodyHeadJson = new RetBodyHJson();
            _RetBodyHeadJson.errtext = errtext;
            _RetBodyHeadJson.resultcode = resultcode;
            if (ypkc != null)
            {
                _RetBodyHeadJson.ypkc = ypkc;
            }
            if (rkmx != null)
            {
                _RetBodyHeadJson.rkmx = rkmx;
            }
            if (ckmx != null)
            {
                _RetBodyHeadJson.ckmx = ckmx;
            }
            if (ypxx != null)
            {
                _RetBodyHeadJson.ypxx = ypxx;
            }
            serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new JavaScriptConverter[] { new PublicClass.QZYBJSConverter() });
            ls_retmsg = serializer.Serialize(_RetBodyHeadJson);
            return ls_retmsg;
        }

        [WebMethod(Description = "衢州医保进销存--药品订单退货")]
        public string return_drug(string requestParmJson,out string ls_retjson)
        {
            //序号 名称  代码 数据类型    长度 说明
            //1    订单编号* ddbh   string  50
            //2    药店编码* ydbm   string  10

            //返回结果：
            //序号 名称  代码 数据类型    长度 说明
            //1   执行代码* resultcode string  5   0为成功，其他为失败
            //2   错误信息* errtext    string  200 失败需要返回错误信息
            string _Getydbm = null;
            string _Getddbh = null;
            string ls_retmsg = null;
            try
            {
                JObject jo = JObject.Parse(requestParmJson);
                _Getydbm = GetValue(jo, "ydbm").ToString();
                _Getddbh = GetValue(jo, "ddbh").ToString();
                #region 反序列化请求Body
                if (_Getydbm == null)
                {
                    ls_retmsg = "请求字符串中未找到ydbm标签";
                    ls_retmsg = GetRetJson("-1", ls_retmsg, null, null, null, null);
                    ls_retjson = ls_retmsg;
                    return ls_retmsg;
                }
                if (_Getddbh == null)
                {
                    ls_retmsg = "请求字符串中未找到ddbh标签";
                    ls_retmsg = GetRetJson("-1", ls_retmsg, null, null, null, null);
                    ls_retjson = ls_retmsg;
                    return ls_retmsg;
                }
                #endregion
                #region 返回数据
                //_RetBodyHeadJson = new RetBodyHJson();
                //_RetBodyHeadJson.errtext = "无法识别的订单号";
                //_RetBodyHeadJson.resultcode = "-1";
                //_RetData_QZYBList = new ObjectList.RetData_QZYBList();
                //_RetData_QZYBList.Head = new List<RetBodyHJson>() { _RetBodyHeadJson };
                //serializer = new JavaScriptSerializer();
                //serializer.RegisterConverters(new JavaScriptConverter[] { new PublicClass.QZYBJSConverter() });
                //ls_retmsg = serializer.Serialize(_RetBodyHeadJson);
                ls_retmsg = "退货结果已由ERP主动查询出结果，此接口暂只提供预留扩展用";
                ls_retmsg = GetRetJson("-1", ls_retmsg, null, null, null, null);
                ls_retjson = ls_retmsg;
                return ls_retmsg;
                #endregion
            }
            catch (Exception ex)
            {
                ls_retmsg = ex.Message.ToString();
                ls_retmsg = GetRetJson("-2", ls_retmsg, null, null, null, null);
                ls_retjson = ls_retmsg;
                return ls_retmsg;
            }
            finally { }
        }


        [WebMethod(Description = "衢州医保进销存--库存查询")]
        public string query_drugs_store(string requestParmJson/*,out string ls_retjson*/)
        {
            //序号 名称  代码 数据类型    长度 说明
            //1    药品名称* ypmc   string  30  药品名的关键子或着助记码
            //2    药店编码* ydbm   string  10

            //返回结果：
            //序号 名称  代码 数据类型    长度 说明
            //1   执行代码* resultcode string  5   0为成功，其他为失败
            //2   错误信息* errtext    string  200 失败需要返回错误信息
            //3	  药品库存	ypkc	json		

            //1   药品名称* ypmc   String  200
            //2   药品编码* ypbm   string  20  进销存对药品的编码
            //3   药品条码 yptm    string  40
            //4   药品规格* ypgg   string  50
            //5   药品单位* ypdw   string  50
            //6   库存数量* kcsl   Double（12，2）		精确到最小包装数量，一整盒数量为1。中草药精确到克
            //7   最小可拆分单位* kcfdw  String  10  例如1盒创可贴，有10片，可以拆成10片一片一片卖，则片是最小可拆分单位

            //8   拆分数量 cfsl    Double（12，2）		
            //9   批次号 pch String  20
            //10  生产日期* scrq   string  20  yyyy - MM - dd hh24: mm: ss
            //11  生产厂商* sccs   String  200
            //12  有效期* yxq    string  20  yyyy - MM - dd hh24: mm: ss
            //13  进货单号 jhdh    String  50

            string _Getydbm = null;
            string _Getypmc = null;
            string ls_retmsg = null;
            string ls_connect = null;
            string ls_retjson = null;
            DataTable dt = null;
            PublicBll bll = new PublicBll();
            List<RetKCJson> _RetKCJson;
            try
            {
                WritingLogsToTxt(requestParmJson);
                #region 获取数据库连接
                if (!getcnParms(OrgCode, out ls_connect))
                {
                    ls_retmsg = GetRetJson("-1", ls_connect, null, null, null, null);
                    ls_retjson = ls_retmsg;
                    return ls_retmsg;
                }
                #endregion

                #region 验证身份
                bll.checkUserValid("999", null, null, null, null, ls_connect);
                #endregion
                JObject jo = JObject.Parse(requestParmJson);
                _Getydbm = GetValue(jo, "ydbm").ToString();
                _Getypmc = GetValue(jo, "ypmc").ToString();
                _Getypmc = _Getypmc.ToUpper();
                if (string.IsNullOrEmpty(_Getypmc))
                {
                    _Getypmc = "all";
                }
                #region 反序列化请求Body
                if (_Getydbm == null)
                {
                    ls_retmsg = "请求字符串中未找到ydbm标签";
                    ls_retmsg = GetRetJson("-1", ls_retmsg, null, null, null, null);
                    ls_retjson = ls_retmsg;
                    return ls_retmsg;
                }
                if (_Getypmc == null)
                {
                    ls_retmsg = "请求字符串中未找到ypmc标签";
                    ls_retmsg = GetRetJson("-1", ls_retmsg, null, null, null, null);
                    ls_retjson = ls_retmsg;
                    return ls_retmsg;
                }
                #endregion

                #region 返回数据
                //dt = bll.dao.GetDataTable("select '1' as ypmc,'2' as ypbm,'3' as yptm,'4' as ypgg,'5' as ypdw,6 as kcsl,'7' as kcfdw,8 as cfsl,'9' as pch,'10' as scrq,'11' as sccs,'12' as yxq,'13' as jhdh from dual");
                dt = bll.dao.GetDataTable(@"SELECT b.warename ypmc, b.warecode ypbm, b.barcode yptm, b.warespec ypgg,
                                               b.wareunit ypdw,
                                               CASE
                                                 WHEN b.minqty > 1 THEN
                                                  b.minqty * a.wareqty
                                                 ELSE
                                                  a.wareqty
                                               END kcsl,
                                               CASE
                                                 WHEN b.minqty > 1 THEN
                                                  b.minunit
                                                 ELSE
                                                  b.wareunit
                                               END kcfdw, b.minqty cfsl, c.makeno pch, c.makedate scrq,
                                               f_get_factoryname(b.factoryid) sccs, c.invalidate yxq, '' jhdh 
                                          FROM t_store_d a
                                          JOIN t_ware b
                                            ON a.wareid = b.wareid
                                           AND a.compid = b.compid
                                          JOIN t_store_i c
                                            ON a.wareid = c.wareid
                                           AND a.batid = c.batid
                                        where a.busno in(select busno from s_busi where ybbm = '" + _Getydbm + @"' and compid = a.compid)
                                        and (b.warename like '%" + _Getypmc + "%' or upper(b.wareabc)  like '%" + _Getypmc + "%' or 'all' = '" + _Getypmc + "') ");
                _RetKCJson = new List<RetKCJson>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    _RetKCJson.Add(new RetKCJson()
                    {
                        ypmc = dt.Rows[i]["ypmc"].ToString(),
                        ypbm = dt.Rows[i]["ypbm"].ToString(),
                        yptm = dt.Rows[i]["yptm"].ToString(),
                        ypgg = dt.Rows[i]["ypgg"].ToString(),
                        ypdw = dt.Rows[i]["ypdw"].ToString(),
                        kcsl = Convert.ToDecimal(dt.Rows[i]["kcsl"].ToString()),
                        kcfdw = dt.Rows[i]["kcfdw"].ToString(),
                        cfsl = Convert.ToDecimal(dt.Rows[i]["cfsl"].ToString()),
                        pch = dt.Rows[i]["pch"].ToString(),
                        scrq = dt.Rows[i]["scrq"].ToString(),
                        sccs = dt.Rows[i]["sccs"].ToString(),
                        yxq = dt.Rows[i]["yxq"].ToString(),
                        jhdh = dt.Rows[i]["jhdh"].ToString()
                    });
                }
                if (dt.Rows.Count == 0)
                {
                    RollbackTrans();
                    ls_retmsg = "查询结果无记录";
                    ls_retmsg = GetRetJson("-2", ls_retmsg, null, null, null, null);
                    ls_retjson = ls_retmsg;
                    return ls_retmsg;
                }
                else
                {
                    ls_retmsg = GetRetJson("0", "查询成功", _RetKCJson, null, null, null);
                    CommitTrans();
                    ls_retjson = ls_retmsg;
                    return ls_retmsg;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ls_retmsg = "查询异常：" + ex.Message.ToString();
                ls_retmsg = GetRetJson("-2", ls_retmsg, null, null, null, null);
                ls_retjson = ls_retmsg;
                return ls_retmsg;
            }
            finally { }
        }

        public static bool WritingLogsToTxt(string content)
        {
            string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, @"Log\NCR\");
            FileStream stream = null;
            StreamWriter writer = null;
            bool flag = false;
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = string.Format(@"{0}\Log_{1}{2}", path, DateTime.Now.ToString("yyyy-MM-dd"), ".txt");
                stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter(stream);
                writer.BaseStream.Seek(0L, SeekOrigin.End);
                //writer.WriteLine(content + "\r\n--" + DateTime.Now.ToString() + "--\r\n");
                content = "入参：" + content;

                writer.WriteLine(content);
                writer.Flush();
                writer.Close();
                stream.Close();
            }
            catch (Exception ce)
            {
                throw ce;
            }
            finally
            {
                writer.Close();
                stream.Close();
                flag = true;
            }
            return flag;
        }

        [WebMethod(Description = "衢州医保进销存--出入库明细查询")]
        public string query_in_out_details(string requestParmJson/*,out string ls_retjson*/)
        {
            string ls_retjson = null;
            //序号 名称  代码 数据类型    长度 说明
            //1    订单编号* ypmc   string  30  药品名的关键子或着助记码
            //2    药店编码* ydbm   string  10
            //3   起始时间* qssj   string  20  格式：
            //yyyy - MM - dd hh24: mm: ss
            //4   终止时间* zzsj   string  20  格式：
            //yyyy - MM - dd hh24: mm: ss


            //返回结果：
            //序号 名称  代码 数据类型    长度 说明
            //1   执行代码* resultcode string  5   0为成功，其他为失败
            //2   错误信息* errtext    string  200 失败需要返回错误信息
            //3   入库明细 rkmx    json
            //4   出库明细 ckmx    json


            //入库明细
            //1   入库方式* rkfs   string  20  00：采购入库
            //01：调拨入库
            //02：盘点
            //03：报溢
            //04：药品拆零
            //05：销售退货
            //06：冲正
            //07：拆零
            //2   药品名称* ypmc   String  200
            //3   药品条码 yptm    string  40
            //4   药品规格* ypgg   string  50
            //5   药品单位* ypdw   string  50
            //6   入库数量* rksl   Double（12，2）		精确到最小包装数量，一整盒数量为1。中草药精确到克
            //7   最小可拆分单位* kcfdw  String  10  例如1盒创可贴，有10片，可以拆成10片一片一片卖，则片是最小可拆分单位

            //8   拆分数量 cfsl    string Double（12，2）	
            //9   批次号 pch String  20
            //10  生产日期* scrq   string  20  yyyy - MM - dd hh24: mm: ss
            //11  生产厂商* sccs   String  200
            //12  有效期* yxq    string  20  yyyy - MM - dd hh24: mm: ss
            //13  入库时间* rksj   string  20  yyyy - MM - dd hh24: mm: ss
            //14  药品编码* ypbm   string  30
            //15  进货单号 jhdh    string  50

            //出库明细
            //1   出库方式* ckfs   String  20  00：采购退货
            //01：调拨出库
            //02：盘点
            //03：报损
            //04：药品拆零
            //05：销售
            //06：冲正
            //07：拆零
            //08：其他出库方式
            //2   药品条码 yptm    string  13  没有条形码的可以不传
            //(例如中草药)
            //3   药品名称* ypmc   String  200
            //4   药品规格* ypgg   string  50
            //5   药品单位* ypdw   string  50
            //6   出库数量* cksl   Double（12，2）		如果是拆分出库，则写拆分出库的数量
            //7   最小可拆分单位* kcfdw  String  10  例如1盒创可贴，有10片，可以拆成10片一片一片卖，则片是最小可拆分单位

            //8   出库的拆分数量 cfsl    string Double（12，2）	
            //9   批次号 pch String  20
            //10  生产日期* scrq   string  20  yyyy - MM - dd hh24: mm: ss
            //11  生产厂商* sccs   String  200
            //12  有效期* yxq    string  20  yyyy - MM - dd hh24: mm: ss
            //13  出库时间* cksj   String  20  yyyy - MM - dd hh24: mm: ss
            //14  药品编码* ypbm   String  30
            //15  销售单号 xsdh    string  50
            //16  进货单号 jhdh    String  50
            string _Getydbm = null;
            string _Getypbm = null;
            string _Getqssj = null;
            string _Getzzsj = null;
            string ls_retmsg = null;
            string ls_connect = null;
            //DateTime ld_start;
            //DateTime ld_end;
            DataTable dt = null;
            PublicBll bll = new PublicBll();
            List<RetRKJson> _RetRKJson;
            List<RetCKJson> _RetCKJson;
            try
            {
                WritingLogsToTxt(requestParmJson);
                JObject jo = JObject.Parse(requestParmJson);
                _Getydbm = GetValue(jo, "ydbm").ToString();
                _Getypbm = GetValue(jo, "ypbm").ToString();
                _Getqssj = GetValue(jo, "qssj").ToString();
                _Getzzsj = GetValue(jo, "zzsj").ToString();
                //ld_start = Convert.ToDateTime(_Getqssj);
                //ld_end = Convert.ToDateTime(_Getzzsj);
                if (string.IsNullOrEmpty(_Getypbm))
                {
                    _Getypbm = "all";
                }
                #region 反序列化请求Body
                if (_Getydbm == null)
                {
                    ls_retmsg = "请求字符串中未找到ydbm标签";
                    ls_retmsg = GetRetJson("-1", ls_retmsg, null, null, null, null);
                    ls_retjson = ls_retmsg;
                    return ls_retmsg;
                }
                if (_Getypbm == null)
                {
                    ls_retmsg = "请求字符串中未找到ypbm标签";
                    ls_retmsg = GetRetJson("-1", ls_retmsg, null, null, null, null);
                    ls_retjson = ls_retmsg;
                    return ls_retmsg;
                }
                if (_Getqssj == null)
                {
                    ls_retmsg = "请求字符串中未找到qssj标签";
                    ls_retmsg = GetRetJson("-1", ls_retmsg, null, null, null, null);
                    ls_retjson = ls_retmsg;
                    return ls_retmsg;
                }
                if (_Getzzsj == null)
                {
                    ls_retmsg = "请求字符串中未找到zzsj标签";
                    ls_retmsg = GetRetJson("-1", ls_retmsg, null, null, null, null);
                    ls_retjson = ls_retmsg;
                    return ls_retmsg;
                }
                #endregion

                #region 获取数据库连接
                if (!getcnParms(OrgCode, out ls_connect))
                {
                    ls_retmsg = GetRetJson("-1", ls_connect, null, null, null, null);
                    ls_retjson = ls_retmsg;
                    return ls_retmsg;
                }
                #endregion

                #region 验证身份
                bll.checkUserValid("999", null, null, null, null, ls_connect);
                #endregion

                #region 返回数据
                //dt = bll.dao.GetDataTable("select '1' as rkfs,'2' as ypmc,'3' as yptm,'4' as ypgg,'5' as ypdw,6 as rksl,'7' as kcfdw,8 as cfsl,'9' as pch,'10' as scrq,'11' as sccs,'12' as yxq,'13' as jhdh from dual");
                dt = bll.dao.GetDataTable(@"SELECT d.billname rkfs, b.warename ypmc, b.barcode yptm, b.warespec ypgg,
                                               b.wareunit ypdw, a.inqty rksl, '' kcfdw, 0 cfsl, c.makeno pch,
                                               c.makedate scrq, f_get_factoryname(b.factoryid) sccs,
                                               c.invalidate yxq, a.execdate rksj, b.warecode ypbm,'' jhdh
                                          FROM t_item_in_out_remain a
                                          JOIN t_ware b
                                            ON a.wareid = b.wareid
                                           AND a.compid = b.compid
                                          JOIN t_store_i c
                                            ON a.wareid = c.wareid
                                           AND a.batid = c.batid
                                          JOIN s_bill d
                                            ON a.billcode = d.billcode
                                           AND a.inqty > 0
                                        where a.busno in(select busno from s_busi where ybbm = '" + _Getydbm + @"' and compid = a.compid)
                                        and (b.warecode = '" + _Getypbm + "' or 'all' = '" + _Getypbm + @"')
                                        and a.execdate >= to_date('" + _Getqssj + "','yyyy-mm-dd hh24:mi:ss') and a.execdate < to_date('" + _Getzzsj + "','yyyy-mm-dd hh24:mi:ss') ");
                _RetRKJson = new List<RetRKJson>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    _RetRKJson.Add(new RetRKJson()
                    {
                        rkfs = dt.Rows[i]["rkfs"].ToString(),
                        ypmc = dt.Rows[i]["ypmc"].ToString(),
                        yptm = dt.Rows[i]["yptm"].ToString(),
                        ypgg = dt.Rows[i]["ypgg"].ToString(),
                        ypdw = dt.Rows[i]["ypdw"].ToString(),
                        rksl = Convert.ToDecimal(dt.Rows[i]["rksl"].ToString()),
                        kcfdw = dt.Rows[i]["kcfdw"].ToString(),
                        cfsl = Convert.ToDecimal(dt.Rows[i]["cfsl"].ToString()),
                        pch = dt.Rows[i]["pch"].ToString(),
                        scrq = dt.Rows[i]["scrq"].ToString(),
                        sccs = dt.Rows[i]["sccs"].ToString(),
                        yxq = dt.Rows[i]["yxq"].ToString(),
                        jhdh = dt.Rows[i]["jhdh"].ToString(),
                        rksj = dt.Rows[i]["rksj"].ToString(),
                        ypbm = dt.Rows[i]["ypbm"].ToString()
                    });
                }
                //dt = bll.dao.GetDataTable("select '1' as ckfs,'2' as yptm,'3' as ypmc,'4' as ypgg,'5' as ypdw,6 as cksl,'7' as kcfdw,8 as cfsl,'9' as pch,'10' as scrq,'11' as sccs,'12' as yxq,'13' as jhdh from dual");
                dt = bll.dao.GetDataTable(@"SELECT d.billname ckfs,b.warename ypmc, b.barcode yptm, b.warespec ypgg, b.wareunit ypdw,
                                                   a.outqty cksl, '' kcfdw,0 cfsl, c.makeno pch, c.makedate scrq,
                                                   f_get_factoryname(b.factoryid) sccs, c.invalidate yxq,
                                                   a.execdate cksj, b.warecode ypbm, '' xsdh, '' jhdh 
                                              FROM t_item_in_out_remain a
                                              JOIN t_ware b
                                                ON a.wareid = b.wareid
                                               AND a.compid = b.compid
                                              JOIN t_store_i c
                                                ON a.wareid = c.wareid
                                               AND a.batid = c.batid
                                              JOIN s_bill d
                                                ON a.billcode = d.billcode
                                               AND a.outqty > 0
                                            where a.busno in(select busno from s_busi where ybbm = '" + _Getydbm + @"' and compid = a.compid)
                                            and (b.warecode = '" + _Getypbm + "' or 'all' = '" + _Getypbm + @"')
                                        and a.execdate >= to_date('" + _Getqssj + "','yyyy-mm-dd hh24:mi:ss') and a.execdate < to_date('" + _Getzzsj + "','yyyy-mm-dd hh24:mi:ss') and rownum <= 100");
                _RetCKJson = new List<RetCKJson>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    _RetCKJson.Add(new RetCKJson()
                    {
                        ckfs = dt.Rows[i]["ckfs"].ToString(),
                        yptm = dt.Rows[i]["yptm"].ToString(),
                        ypmc = dt.Rows[i]["ypmc"].ToString(),
                        ypgg = dt.Rows[i]["ypgg"].ToString(),
                        ypdw = dt.Rows[i]["ypdw"].ToString(),
                        cksl = Convert.ToDecimal(dt.Rows[i]["cksl"].ToString()),
                        kcfdw = dt.Rows[i]["kcfdw"].ToString(),
                        cfsl = Convert.ToDecimal(dt.Rows[i]["cfsl"].ToString()),
                        pch = dt.Rows[i]["pch"].ToString(),
                        scrq = dt.Rows[i]["scrq"].ToString(),
                        sccs = dt.Rows[i]["sccs"].ToString(),
                        yxq = dt.Rows[i]["yxq"].ToString(),
                        jhdh = dt.Rows[i]["jhdh"].ToString(),
                        cksj = dt.Rows[i]["cksj"].ToString(),
                        ypbm = dt.Rows[i]["ypbm"].ToString(),
                        xsdh = dt.Rows[i]["xsdh"].ToString()
                    });
                }
                if (dt.Rows.Count == 0)
                {
                    RollbackTrans();
                    ls_retmsg = "查询结果无记录";
                    ls_retmsg = GetRetJson("-2", ls_retmsg, null, null, null, null);
                    ls_retjson = ls_retmsg;
                    return ls_retmsg;
                }
                else
                {
                    ls_retmsg = GetRetJson("0", "查询成功", null, _RetRKJson, _RetCKJson, null);
                    CommitTrans();
                    ls_retjson = ls_retmsg;
                    return ls_retmsg;
                }
                #endregion
            }
            catch (Exception ex)
            {
                RollbackTrans();
                ls_retmsg = ex.Message.ToString();
                //WriteLog(OperUserID, DateTime.Now, requestParmJson, ls_retmsg, requestParmJson, null, "PDA", null, requestParmJson, "0000");
                ls_retmsg = GetRetJson("-2", ls_retmsg, null, null, null, null);
                ls_retjson = ls_retmsg;
                return ls_retmsg;
            }
            finally { }
        }


        [WebMethod(Description = "衢州医保进销存--药品信息")]
        public string query_drugs(string requestParmJson,out string ls_retjson)
        {
            //序号 名称  代码 数据类型    长度 说明
            //1    药品名称* ypmc   string  30  药品名的关键子或着助记码
            //2    药店编码* ydbm   string  10

            //返回结果：
            //序号 名称  代码 数据类型    长度 说明
            //1   执行代码* resultcode string  5   0为成功，其他为失败
            //2   错误信息* errtext    string  200 失败需要返回错误信息
            //3	  药品库存	ypxx	json		

            //序号 名称  代码 数据类型    长度 说明
            //1   药品名称* ypmc   string  50
            //2   药品单位* ypdw   string  20
            //3   药品规格* ypgg   string  20
            //4   药品条码 yptm    string  20  不是必传
            //5   药品编码* ypbm   String  50
            //6   剂型 ypjx    String  50
            //7   生产厂商 sccs    String  200
            //8   成分规格 cfgg    string  50
            //9   批准文号 pzwh    String  100
            //10  通用名 tym String  200
            //11  生产地址 scdz    String  100


            string _Getydbm = null;
            string _Getypmc = null;
            string ls_retmsg = null;
            string ls_connect = null;
            DataTable dt = null;
            PublicBll bll = new PublicBll();
            List<RetYPJson> _RetYPJson;
            try
            {
                WritingLogsToTxt(requestParmJson);
                JObject jo = JObject.Parse(requestParmJson);
                _Getydbm = GetValue(jo, "ydbm").ToString();
                _Getypmc = GetValue(jo, "ypmc").ToString();
                _Getypmc = _Getypmc.ToUpper();
                if (string.IsNullOrEmpty(_Getypmc))
                {
                    _Getypmc = "all";
                }
                #region 反序列化请求Body
                if (_Getydbm == null)
                {
                    ls_retmsg = "请求字符串中未找到ydbm标签";
                    ls_retmsg = GetRetJson("-1", ls_retmsg, null, null, null, null);
                    ls_retjson = ls_retmsg;
                    return ls_retmsg;
                }
                if (_Getypmc == null)
                {
                    ls_retmsg = "请求字符串中未找到ypmc标签";
                    ls_retmsg = GetRetJson("-1", ls_retmsg, null, null, null, null);
                    ls_retjson = ls_retmsg;
                    return ls_retmsg;
                }
                #endregion

                #region 获取数据库连接
                if (!getcnParms(OrgCode, out ls_connect))
                {
                    ls_retmsg = GetRetJson("-1", ls_connect, null, null, null, null);
                    ls_retjson = ls_retmsg;
                    return ls_retmsg;
                }
                #endregion

                #region 验证身份
                bll.checkUserValid("999", null, null, null, null, ls_connect);
                #endregion

                #region 返回数据
                //dt = bll.dao.GetDataTable("select '1' as ypmc,'2' as ypdw,'3' as ypgg,'4' as yptm,'5' as ypbm,'6' as ypjx,'7' as sccs,'8' as cfgg,'9' as pzwh,'10' as tym,'11' as scdz from dual");
                dt = bll.dao.GetDataTable(@"SELECT a.warename ypmc, a.wareunit ypdw, a.warespec ypgg, a.barcode yptm,
                                                   a.warecode ypbm, f_get_classname('03', a.wareid, a.compid) ypjx,
                                                       f_get_factoryname(a.factoryid) sccs, a.warespec cfgg, a.fileno pzwh,
                                                       a.waregeneralname tym, f_get_areacode(a.areacode) scdz 
                                                  FROM t_ware a
                                                where a.compid in(select compid from s_busi where ybbm = '" + _Getydbm + @"') 
                                                and (a.warename like '%" + _Getypmc + "%' or upper(a.wareabc)  like '%" + _Getypmc + "%' or 'all' = '" + _Getypmc + "') ");
                _RetYPJson = new List<RetYPJson>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    _RetYPJson.Add(new RetYPJson()
                    {
                        ypmc = dt.Rows[i]["ypmc"].ToString(),
                        ypdw = dt.Rows[i]["ypdw"].ToString(),
                        ypgg = dt.Rows[i]["ypgg"].ToString(),
                        yptm = dt.Rows[i]["yptm"].ToString(),
                        ypbm = dt.Rows[i]["ypbm"].ToString(),
                        ypjx = dt.Rows[i]["ypjx"].ToString(),
                        sccs = dt.Rows[i]["sccs"].ToString(),
                        cfgg = dt.Rows[i]["cfgg"].ToString(),
                        pzwh = dt.Rows[i]["pzwh"].ToString(),
                        tym = dt.Rows[i]["tym"].ToString(),
                        scdz = dt.Rows[i]["scdz"].ToString()
                    });
                }
                if (dt.Rows.Count == 0)
                {
                    RollbackTrans();
                    ls_retmsg = "查询结果无记录";
                    ls_retmsg = GetRetJson("-2", ls_retmsg, null, null, null, null);
                    ls_retjson = ls_retmsg;
                    return ls_retmsg;
                }
                else
                {
                    ls_retmsg = GetRetJson("0", "查询成功", null, null, null, _RetYPJson);
                    CommitTrans();
                    ls_retjson = ls_retmsg;
                    return ls_retmsg;
                }
                #endregion
            }
            catch (Exception ex)
            {
                RollbackTrans();
                ls_retmsg = ex.Message.ToString();
                ls_retmsg = GetRetJson("-2", ls_retmsg, null, null, null, null);
                ls_retjson = ls_retmsg;
                return ls_retmsg;
            }
            finally { }
        }
    }
}

