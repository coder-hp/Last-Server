using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ConfigData
{
    public int server_port;
    public string sql_ip;
    public int sql_port;
    public string user;
    public string password;
    public string databaseName;
    public int timerReqSql;
}

public class ConfigEntity
{
    public static ConfigEntity s_instance = null;

    public ConfigData data;

    public static ConfigEntity getInstance()
    {
        if(s_instance == null)
        {
            s_instance = new ConfigEntity();
        }

        return s_instance;
    }
}