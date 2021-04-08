using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

public enum CmdType
{
    query,
    insert,
    update,
    delete,
}

public delegate void OnCmdCallBack(CmdReturnData cmdReturnData);

public class CmdQueue
{
    public bool isComplete = false;
    public CmdType cmdType;
    public string table;
    public List<TableKeyObj> tiaojian_keyObjList;
    public List<TableKeyObj> change_keyObjList;
    public OnCmdCallBack onCmdCallBack;

    public CmdQueue(CmdType _cmdType, string _table, List<TableKeyObj> _tiaojian_keyObjList, List<TableKeyObj> _change_keyObjList, OnCmdCallBack _onCmdCallBack)
    {
        cmdType = _cmdType;
        table = _table;
        tiaojian_keyObjList = _tiaojian_keyObjList;
        change_keyObjList = _change_keyObjList;
        onCmdCallBack = _onCmdCallBack;
    }
}

public class CmdReturnData
{
    public int code;
    public List<DBTablePreset> listData;

    public CmdReturnData(int _code, List<DBTablePreset> _listData = null)
    {
        code = _code;
        listData = _listData;
    }
}

class MySqlUtil
{
    static MySqlUtil s_mySqlUtil = null;

    MySqlConnection m_mySqlConnection;
    public List<CmdQueue> cmdQueue_list = new List<CmdQueue>();
    public int cmdQueueListMaxCount = 0;

    bool isCmding = false;
    MySqlDataReader curMySqlDataReader = null;

    public static MySqlUtil getInstance()
    {
        if (s_mySqlUtil == null)
        {
            s_mySqlUtil = new MySqlUtil();
        }

        return s_mySqlUtil;
    }

    // 打开数据库
    public void openDatabase()
    {
        try
        {
            DBTableManager.getInstance().init();

            var configData = ConfigEntity.getInstance().data;
            string conn = string.Format("Data Source= {0}; Port= {1} ; User ID = {2} ; Password = {3} ; DataBase = {4} ; Charset = utf8;", configData.sql_ip, configData.sql_port, configData.user, configData.password, configData.databaseName);
            m_mySqlConnection = new MySqlConnection(conn);

            //进行数据库连接
            m_mySqlConnection.Open();

            CommonUtil.Log("数据库打开成功");

            DBTableManager.getInstance().init();

            startCmdThread();

            // 定时请求数据库，防止断开
            TimerUtil.start(configData.timerReqSql * 1000, startDingShiReq);
        }
        catch (Exception ex)
        {
            CommonUtil.Log(ex);
        }
    }

    public void addCommand(CmdType cmdType, string table, List<TableKeyObj> tiaojian_keyObjList, List<TableKeyObj> change_keyObjList, OnCmdCallBack onCmdCallBack)
    {
        cmdQueue_list.Add(new CmdQueue(cmdType, table, tiaojian_keyObjList, change_keyObjList, onCmdCallBack));

        if (cmdQueue_list.Count > cmdQueueListMaxCount)
        {
            addCmdQueueListMaxCount();
        }

        //CommonUtil.Log("addCommand:" + cmdQueue_list.Count);
    }

    void startCmdThread()
    {
        TimerUtil.start(10, newThread);
    }

    void startDingShiReq(object source, System.Timers.ElapsedEventArgs e)
    {
        List<TableKeyObj> keylist = new List<TableKeyObj>() { new TableKeyObj("account", TableKeyObj.ValueType.ValueType_string, "hp") };
        addCommand(CmdType.query, "user", keylist, null, (CmdReturnData cmdReturnData) =>
        {
            CommonUtil.Log("定时请求数据库，防止断开");
        });
    }

    void newThread(object source, System.Timers.ElapsedEventArgs e)
    {
        {
            try
            {
                if (isCmding)
                {
                    return;
                }

                bool isReady = true;
                if (curMySqlDataReader != null)
                {
                    if (!curMySqlDataReader.IsClosed)
                    {
                        isReady = false;
                        CommonUtil.Log("curMySqlDataReader没有Close");
                    }
                }

                if (isReady && cmdQueue_list.Count > 0)
                {
                    CmdQueue cmdQueue = cmdQueue_list[0];
                    if (cmdQueue != null)
                    {
                        doCmd(cmdQueue);
                    }
                }
            }
            catch (Exception ex)
            {
                CommonUtil.Log(ex.ToString());
            }
        }
    }

    void doCmd(CmdQueue cmdQueue)
    {
        try
        {
            isCmding = true;
            if (cmdQueue != null)
            {
                switch (cmdQueue.cmdType)
                {
                    case CmdType.query:
                        {
                            List<DBTablePreset> listData = getTableData(cmdQueue.table, cmdQueue.tiaojian_keyObjList);
                            if (cmdQueue.onCmdCallBack != null)
                            {
                                doCmdComplete(cmdQueue);
                                cmdQueue.onCmdCallBack(new CmdReturnData(listData == null ? -1 : 1, listData));
                            }

                            break;
                        }

                    case CmdType.insert:
                        {
                            int code = insertData(cmdQueue.table, cmdQueue.change_keyObjList);
                            if (cmdQueue.onCmdCallBack != null)
                            {
                                doCmdComplete(cmdQueue);
                                cmdQueue.onCmdCallBack(new CmdReturnData(code));
                            }

                            break;
                        }

                    case CmdType.update:
                        {
                            int code = updateData(cmdQueue.table, cmdQueue.tiaojian_keyObjList, cmdQueue.change_keyObjList);
                            if (cmdQueue.onCmdCallBack != null)
                            {
                                doCmdComplete(cmdQueue);
                                cmdQueue.onCmdCallBack(new CmdReturnData(code));
                            }

                            break;
                        }

                    case CmdType.delete:
                        {
                            int code = deleteData(cmdQueue.table, cmdQueue.tiaojian_keyObjList);
                            if (cmdQueue.onCmdCallBack != null)
                            {
                                doCmdComplete(cmdQueue);
                                cmdQueue.onCmdCallBack(new CmdReturnData(code));
                            }

                            break;
                        }
                }
            }
            else
            {
                isCmding = false;
                CommonUtil.Log("cmdQueue == null");
            }
        }
        catch (Exception ex)
        {
            CommonUtil.Log(ex);
        }
    }

    void doCmdComplete(CmdQueue cmdQueue)
    {
        if (cmdQueue_list.Count > 0)
        {
            cmdQueue.isComplete = true;
            cmdQueue_list.Remove(cmdQueue);
        }
        isCmding = false;
    }

    CmdQueue getNextCmd()
    {
        for (int i = 0; i < cmdQueue_list.Count; i++)
        {
            if (cmdQueue_list[i] != null)
            {
                if (!cmdQueue_list[i].isComplete)
                {
                    return cmdQueue_list[i];
                }
            }
        }

        return null;
    }

    void cleanCmdQuene()
    {
        for (int i = cmdQueue_list.Count - 1; i >= 0; i--)
        {
            CmdQueue temp = cmdQueue_list[i];
            if (temp == null)
            {
                //cmdQueue_list.Remove(temp);
            }
            else if (temp.isComplete)
            {
                //cmdQueue_list.Remove(temp);
            }
        }
    }

    // 数据库查询-遍历整个表
    List<DBTablePreset> queryDatabaseTable(string table)
    {
        try
        {
            if (m_mySqlConnection.State == System.Data.ConnectionState.Closed)
            {
                CommonUtil.Log("数据库连接断开，开始重新连接");
                openDatabase();
            }

            MySqlCommand cmd = new MySqlCommand("select * from " + table, m_mySqlConnection);
            MySqlDataReader dr = cmd.ExecuteReader();   //读出数据

            // 读出来是否有数据
            //CommonUtil.Log("----"+dr.HasRows);

            List<DBTablePreset> listData = new List<DBTablePreset>();
            DBTablePreset baseTablePreset = DBTableManager.getInstance().getDBTablePreset(table);

            while (dr.Read())
            {
                DBTablePreset tempTablePreset = new Default_Preset(baseTablePreset.tableName);

                for (int i = 0; i < baseTablePreset.keyList.Count; i++)
                {
                    TableKeyObj tempTableKeyObj = new TableKeyObj(baseTablePreset.keyList[i].m_name, baseTablePreset.keyList[i].m_valueType);
                    tempTableKeyObj.m_value = getObjectByValueType(dr, i, tempTableKeyObj.m_valueType);
                    tempTablePreset.keyList.Add(tempTableKeyObj);
                }

                listData.Add(tempTablePreset);
            }

            dr.Close();

            return listData;
        }
        catch (Exception ex)
        {
            CommonUtil.Log(ex);

            return null;
        }
    }

    // 数据库查询-按条件查询
    List<DBTablePreset> getTableData(string table, List<TableKeyObj> keyObjList)
    {
        try
        {
            List<DBTablePreset> dbTablePresetList = new List<DBTablePreset>();

            string command = "select * from " + table + " where ";
            for (int i = 0; i < keyObjList.Count; i++)
            {
                if (keyObjList[i].m_valueType == TableKeyObj.ValueType.ValueType_string)
                {
                    command += (keyObjList[i].m_name + " = '");
                    command += keyObjList[i].m_value;
                    command += "'";
                }
                else
                {
                    command += (keyObjList[i].m_name + " = " + keyObjList[i].m_value);
                }

                if (i != (keyObjList.Count - 1))
                {
                    command += " and ";
                }
            }
            //CommonUtil.Log(command);

            MySqlDataReader dr = ExecuteCommandHasReturn(command);
            curMySqlDataReader = dr;
            if (dr == null)
            {
                return null;
            }

            List<DBTablePreset> listData = new List<DBTablePreset>();
            DBTablePreset baseTablePreset = DBTableManager.getInstance().getDBTablePreset(table);

            while (dr.Read())
            {
                DBTablePreset tempTablePreset = new Default_Preset(baseTablePreset.tableName);

                for (int i = 0; i < baseTablePreset.keyList.Count; i++)
                {
                    TableKeyObj tempTableKeyObj = new TableKeyObj(baseTablePreset.keyList[i].m_name, baseTablePreset.keyList[i].m_valueType);
                    tempTableKeyObj.m_value = getObjectByValueType(dr, i, tempTableKeyObj.m_valueType);
                    tempTablePreset.keyList.Add(tempTableKeyObj);
                }

                listData.Add(tempTablePreset);
            }

            dr.Close();

            return listData;
        }
        catch (Exception ex)
        {
            CommonUtil.Log(ex);

            return null;
        }
    }

    // 增加数据
    int insertData(string table, List<TableKeyObj> keyObjList)
    {
        try
        {
            string command = "insert into " + table + " (";
            for (int i = 0; i < keyObjList.Count; i++)
            {
                command += (keyObjList[i].m_name);

                if (i != (keyObjList.Count - 1))
                {
                    command += " , ";
                }
                else
                {
                    command += " ) values (";
                }
            }

            for (int i = 0; i < keyObjList.Count; i++)
            {
                if (keyObjList[i].m_valueType == TableKeyObj.ValueType.ValueType_string)
                {
                    command += "'";
                    command += keyObjList[i].m_value;
                    command += "'";
                }
                else
                {
                    command += (keyObjList[i].m_value);
                }

                if (i != (keyObjList.Count - 1))
                {
                    command += " , ";
                }
                else
                {
                    command += " )";
                }
            }

            //CommonUtil.Log(command);

            if (m_mySqlConnection.State == System.Data.ConnectionState.Closed)
            {
                CommonUtil.Log("数据库连接断开，开始重新连接");
                openDatabase();
            }

            MySqlCommand cmd = new MySqlCommand(command, m_mySqlConnection);
            return cmd.ExecuteNonQuery();
            //return Task<int>cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            CommonUtil.Log(ex);

            return -1;
        }
    }

    // 删除数据
    int deleteData(string table, List<TableKeyObj> keyObjList)
    {
        try
        {
            string command = "delete from " + table + " where ";
            for (int i = 0; i < keyObjList.Count; i++)
            {
                if (keyObjList[i].m_valueType == TableKeyObj.ValueType.ValueType_string)
                {
                    command += (keyObjList[i].m_name + " = '");
                    command += keyObjList[i].m_value;
                    command += "'";
                }
                else
                {
                    command += (keyObjList[i].m_name + " = " + keyObjList[i].m_value);
                }

                if (i != (keyObjList.Count - 1))
                {
                    command += " and ";
                }
            }

            //CommonUtil.Log(command);

            if (m_mySqlConnection.State == System.Data.ConnectionState.Closed)
            {
                CommonUtil.Log("数据库连接断开，开始重新连接");
                openDatabase();
            }

            MySqlCommand cmd = new MySqlCommand(command, m_mySqlConnection);
            return cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            CommonUtil.Log(ex);

            return -1;
        }
    }

    // 修改数据
    int updateData(string table, List<TableKeyObj> tiaojian_keyObjList, List<TableKeyObj> change_keyObjList)
    {
        try
        {
            string command = "update " + table + " set ";//name = 'zsr' ,sex = 0 where id = 2";
            for (int i = 0; i < change_keyObjList.Count; i++)
            {
                command += (change_keyObjList[i].m_name + " = ");

                if (change_keyObjList[i].m_valueType == TableKeyObj.ValueType.ValueType_string)
                {
                    command += "'";
                    command += change_keyObjList[i].m_value;
                    command += "'";
                }
                else
                {
                    command += change_keyObjList[i].m_value;
                }

                if (i != (change_keyObjList.Count - 1))
                {
                    command += " , ";
                }
            }

            command += " where ";

            for (int i = 0; i < tiaojian_keyObjList.Count; i++)
            {
                if (tiaojian_keyObjList[i].m_valueType == TableKeyObj.ValueType.ValueType_string)
                {
                    command += (tiaojian_keyObjList[i].m_name + " = '");
                    command += tiaojian_keyObjList[i].m_value;
                    command += "'";
                }
                else
                {
                    command += (tiaojian_keyObjList[i].m_name + " = " + tiaojian_keyObjList[i].m_value);
                }

                if (i != (tiaojian_keyObjList.Count - 1))
                {
                    command += " and ";
                }
            }

            //CommonUtil.Log(command);

            if (m_mySqlConnection.State == System.Data.ConnectionState.Closed)
            {
                CommonUtil.Log("数据库连接断开，开始重新连接");
                openDatabase();

                return -1;
            }

            MySqlCommand cmd = new MySqlCommand(command, m_mySqlConnection);
            return cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            CommonUtil.Log(ex);

            return -1;
        }
    }

    int ExecuteCommand(string command)
    {
        try
        {
            if (m_mySqlConnection.State == System.Data.ConnectionState.Closed)
            {
                CommonUtil.Log("数据库连接断开，开始重新连接");
                openDatabase();

                return -1;
            }

            MySqlCommand cmd = new MySqlCommand(command, m_mySqlConnection);
            return cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            //LogUtil.getInstance().writeErrorLog("错误信息：" + ex.Message.ToString() + "\r\n");
            CommonUtil.Log("错误信息：" + ex);

            throw ex;
        }
    }

    MySqlDataReader ExecuteCommandHasReturn(string command)
    {
        try
        {
            if (m_mySqlConnection.State == System.Data.ConnectionState.Closed)
            {
                CommonUtil.Log("数据库连接断开，开始重新连接");
                openDatabase();

                return null;
            }

            MySqlCommand cmd = new MySqlCommand(command, m_mySqlConnection);
            MySqlDataReader dr = cmd.ExecuteReader();
            return dr;
        }
        catch (Exception ex)
        {
            CommonUtil.Log("错误信息：" + ex);

            throw ex;
        }
    }

    // 关闭数据库
    public void closeDatabase()
    {
        m_mySqlConnection.Close();
    }

    void addCmdQueueListMaxCount()
    {
        ++cmdQueueListMaxCount;
        //CommonUtil.Log("addCmdQueueListMaxCount:" + cmdQueueListMaxCount);
    }

    //-----------------------------------------------------
    public object getObjectByValueType(MySqlDataReader dr, int index, TableKeyObj.ValueType valueType)
    {
        object obj = null;

        switch (valueType)
        {
            case TableKeyObj.ValueType.ValueType_int:
                {
                    obj = dr.GetInt32(index);
                }
                break;

            case TableKeyObj.ValueType.ValueType_float:
                {
                    obj = dr.GetFloat(index);
                }
                break;

            case TableKeyObj.ValueType.ValueType_string:
                {
                    obj = dr.GetString(index);
                }
                break;
        }

        return obj;
    }
}