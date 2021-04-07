using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGameService.Net
{
    class DoTask_Login
    {
        public static void Do(ClientInfo clientInfo, string data)
        {
            S2C_Login s2c = new S2C_Login();
            s2c.Tag = (int)CSParam.NetTag.Login;
            s2c.Code = (int)CSParam.CodeType.Ok;
            try
            {
                C2S_Login c2s = JsonConvert.DeserializeObject<C2S_Login>(data);
                string account = c2s.Account;
                string password = c2s.Password;

                List<TableKeyObj> keylist = new List<TableKeyObj>() { new TableKeyObj("account", TableKeyObj.ValueType.ValueType_string, account) };

                MySqlUtil.getInstance().addCommand(CmdType.query, "user", keylist, null, (CmdReturnData cmdReturnData) =>
                {
                    if (cmdReturnData.code == 1)
                    {
                        
                        List<DBTablePreset> list = cmdReturnData.listData;
                        if (list != null && list.Count > 0)
                        {
                            string _userId = list[0].keyList[0].m_value.ToString();
                            string _account = list[0].keyList[1].m_value.ToString();
                            string _password = list[0].keyList[2].m_value.ToString();
                            string _phone = list[0].keyList[3].m_value.ToString();
                            int _zhanshiLevel = (int)list[0].keyList[4].m_value;
                            int _fashiLevel = (int)list[0].keyList[5].m_value;
                            int _huanshiLevel = (int)list[0].keyList[6].m_value;

                            if (_password == password)
                            {
                                s2c.Code = (int)CSParam.CodeType.Ok;
                                s2c.UserId = _userId;
                                s2c.zhanshiLevel = _zhanshiLevel;
                                s2c.fashiLevel = _fashiLevel;
                                s2c.huanshiLevel = _huanshiLevel;
                                Socket_S.getInstance().Send(clientInfo, s2c);
                            }
                            else
                            {
                                s2c.Code = (int)CSParam.CodeType.LoginFail;
                                Socket_S.getInstance().Send(clientInfo, s2c);
                            }
                        }
                        else
                        {
                            s2c.Code = (int)CSParam.CodeType.LoginFail;
                            Socket_S.getInstance().Send(clientInfo, s2c);
                        }
                    }
                    else if (cmdReturnData.code == -1)
                    {
                        s2c.Code = (int)CSParam.CodeType.ServerError;
                        Socket_S.getInstance().Send(clientInfo, s2c);
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                s2c.Code = (int)CSParam.CodeType.ParamError;
                Socket_S.getInstance().Send(clientInfo, s2c);
            }
        }
    }
}
