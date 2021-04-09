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

public enum CmdResult
{
    OK,
    Fail,
}

public class KeyData
{
    public string key;
    public Object value;

    public KeyData(string _key, Object _value)
    {
        key = _key;
        value = _value;
    }
}

public class CmdQueue
{
    public bool isComplete = false;
    public CmdType cmdType;
    public string table;
    public List<KeyData> tiaojian_keyObjList;
    public List<KeyData> change_keyObjList;
    public Action<CmdReturnData> onCmdCallBack;

    public CmdQueue(CmdType _cmdType, string _table, List<KeyData> _tiaojian_keyObjList, List<KeyData> _change_keyObjList, Action<CmdReturnData> _onCmdCallBack)
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
    public CmdResult result;
    public List<Object> listData;

    public CmdReturnData(CmdResult _result, List<Object> _listData = null)
    {
        result = _result;
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
            var configData = ConfigEntity.getInstance().data;
            string conn = string.Format("Data Source= {0}; Port= {1} ; User ID = {2} ; Password = {3} ; DataBase = {4} ; Charset = utf8;", configData.sql_ip, configData.sql_port, configData.user, configData.password, configData.databaseName);
            m_mySqlConnection = new MySqlConnection(conn);

            //进行数据库连接
            m_mySqlConnection.Open();

            CommonUtil.Log("数据库打开成功");

            startCmdThread();

            // 定时请求数据库，防止断开
            TimerUtil.start(configData.timerReqSql * 1000, startDingShiReq);
        }
        catch (Exception ex)
        {
            CommonUtil.Log(ex);
        }
    }

    public void addCommand(CmdType cmdType, string table, List<KeyData> tiaojian_keyObjList, List<KeyData> change_keyObjList, Action<CmdReturnData> onCmdCallBack)
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
        List<KeyData> keylist = new List<KeyData>() { new KeyData("account", "hp") };
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
                            List<Object> data = getTableData(cmdQueue.table, cmdQueue.tiaojian_keyObjList);
                            if (cmdQueue.onCmdCallBack != null)
                            {
                                doCmdComplete(cmdQueue);
                                cmdQueue.onCmdCallBack(new CmdReturnData(CmdResult.OK, data));
                            }

                            break;
                        }

                    case CmdType.insert:
                        {
                            int code = insertData(cmdQueue.table, cmdQueue.change_keyObjList);
                            CmdResult result = code == 1 ? CmdResult.OK : CmdResult.Fail;
                            if (cmdQueue.onCmdCallBack != null)
                            {
                                doCmdComplete(cmdQueue);
                                cmdQueue.onCmdCallBack(new CmdReturnData(result));
                            }

                            break;
                        }

                    case CmdType.update:
                        {
                            int code = updateData(cmdQueue.table, cmdQueue.tiaojian_keyObjList, cmdQueue.change_keyObjList);
                            CmdResult result = code == 1 ? CmdResult.OK : CmdResult.Fail;
                            if (cmdQueue.onCmdCallBack != null)
                            {
                                doCmdComplete(cmdQueue);
                                cmdQueue.onCmdCallBack(new CmdReturnData(result));
                            }

                            break;
                        }

                    case CmdType.delete:
                        {
                            int code = deleteData(cmdQueue.table, cmdQueue.tiaojian_keyObjList);
                            CmdResult result = code == 1 ? CmdResult.OK : CmdResult.Fail;
                            if (cmdQueue.onCmdCallBack != null)
                            {
                                doCmdComplete(cmdQueue);
                                cmdQueue.onCmdCallBack(new CmdReturnData(result));
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
    List<Object> queryDatabaseTable(string table)
    {
        //try
        //{
        //    if (m_mySqlConnection.State == System.Data.ConnectionState.Closed)
        //    {
        //        CommonUtil.Log("数据库连接断开，开始重新连接");
        //        openDatabase();
        //    }

        //    MySqlCommand cmd = new MySqlCommand("select * from " + table, m_mySqlConnection);
        //    MySqlDataReader dr = cmd.ExecuteReader();   //读出数据

        //    // 读出来是否有数据
        //    //CommonUtil.Log("----"+dr.HasRows);

        //    List<DBTablePreset> listData = new List<DBTablePreset>();
        //    DBTablePreset baseTablePreset = DBTableManager.getInstance().getDBTablePreset(table);

        //    while (dr.Read())
        //    {
        //        DBTablePreset tempTablePreset = new Default_Preset(baseTablePreset.tableName);

        //        for (int i = 0; i < baseTablePreset.keyList.Count; i++)
        //        {
        //            KeyData tempKeyData = new KeyData(baseTablePreset.keyList[i].m_name, baseTablePreset.keyList[i].m_valueType);
        //            tempKeyData.m_value = getObjectByValueType(dr, i, tempKeyData.m_valueType);
        //            tempTablePreset.keyList.Add(tempKeyData);
        //        }

        //        listData.Add(tempTablePreset);
        //    }

        //    dr.Close();

        //    return listData;
        //}
        //catch (Exception ex)
        //{
        //    CommonUtil.Log(ex);

        //    return null;
        //}
        return null;
    }

    // 数据库查询-按条件查询
    List<Object> getTableData(string table, List<KeyData> keyObjList)
    {
        try
        {
            string command = "select * from " + table + " where ";
            for (int i = 0; i < keyObjList.Count; i++)
            {
                switch(keyObjList[i].value.GetType().Name)
                {
                    case "String":
                        {
                            command += (keyObjList[i].key + " = '");
                            command += keyObjList[i].value;
                            command += "'";
                            break;
                        }

                    default:
                        {
                            command += (keyObjList[i].key + " = " + keyObjList[i].value);
                            break;
                        }
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

            // while每执行一次，读取一条记录，如果查询返回的数据有n条，则while执行n次
            List<Object> values = new List<object>();
            while (dr.Read())
            {
                //CommonUtil.Log("dr.FieldCount=" + dr.FieldCount);
                
                Object[] temp_values = new Object[dr.FieldCount];
                int fieldCount = dr.GetValues(temp_values);

                for(int i = 0; i< temp_values.Length; i++)
                {
                    values.Add(temp_values[i]);
                }
            }
            dr.Close();
            
            return values;
        }
        catch (Exception ex)
        {
            CommonUtil.Log(ex);

            return null;
        }
    }

    // 增加数据
    int insertData(string table, List<KeyData> keyObjList)
    {
        try
        {
            string command = "insert into " + table + " (";
            for (int i = 0; i < keyObjList.Count; i++)
            {
                command += (keyObjList[i].key);

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
                switch (keyObjList[i].value.GetType().Name)
                {
                    case "String":
                        {
                            command += "'";
                            command += keyObjList[i].value;
                            command += "'";
                            break;
                        }

                    default:
                        {
                            command += (keyObjList[i].value);
                            break;
                        }
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
    int deleteData(string table, List<KeyData> keyObjList)
    {
        try
        {
            string command = "delete from " + table + " where ";
            for (int i = 0; i < keyObjList.Count; i++)
            {
                switch (keyObjList[i].value.GetType().Name)
                {
                    case "String":
                        {
                            command += (keyObjList[i].key + " = '");
                            command += keyObjList[i].value;
                            command += "'";
                            break;
                        }

                    default:
                        {
                            command += (keyObjList[i].key + " = " + keyObjList[i].value);
                            break;
                        }
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
    int updateData(string table, List<KeyData> tiaojian_keyObjList, List<KeyData> change_keyObjList)
    {
        try
        {
            string command = "update " + table + " set ";//name = 'zsr' ,sex = 0 where id = 2";
            for (int i = 0; i < change_keyObjList.Count; i++)
            {
                command += (change_keyObjList[i].key + " = ");

                switch (change_keyObjList[i].value.GetType().Name)
                {
                    case "String":
                        {
                            command += "'";
                            command += change_keyObjList[i].value;
                            command += "'";
                            break;
                        }

                    default:
                        {
                            command += change_keyObjList[i].value;
                            break;
                        }
                }

                if (i != (change_keyObjList.Count - 1))
                {
                    command += " , ";
                }
            }

            command += " where ";

            for (int i = 0; i < tiaojian_keyObjList.Count; i++)
            {
                switch (tiaojian_keyObjList[i].value.GetType().Name)
                {
                    case "String":
                        {
                            command += (tiaojian_keyObjList[i].key + " = '");
                            command += tiaojian_keyObjList[i].value;
                            command += "'";
                            break;
                        }

                    default:
                        {
                            command += (tiaojian_keyObjList[i].key + " = " + tiaojian_keyObjList[i].value);
                            break;
                        }
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
}