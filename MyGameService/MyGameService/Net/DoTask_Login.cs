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
            s2c.Tag = CSParam.NetTag.Login.ToString();
            s2c.Code = (int)CSParam.CodeType.Ok;
            try
            {
                C2S_Login c2s = JsonConvert.DeserializeObject<C2S_Login>(data);
                string account = c2s.Account;
                string password = c2s.Password;

                List<KeyData> keylist = new List<KeyData>() { new KeyData("account", account) };

                MySqlUtil.getInstance().addCommand(CmdType.query, "user", keylist, null, (CmdReturnData cmdReturnData) =>
                {
                    if (cmdReturnData.result == CmdResult.OK)
                    {
                        List<Object> list = cmdReturnData.listData;
                        if (list != null && list.Count > 0)
                        {
                            Table_User table_User = Table_User.init(list);
                            if (table_User.password == password)
                            {
                                s2c.Code = (int)CSParam.CodeType.Ok;
                                s2c.UserId = table_User.id;
                                s2c.zhanshiLevel = table_User.zhanshilevel;
                                s2c.fashiLevel = table_User.fashilevel;
                                s2c.huanshiLevel = table_User.huanshilevel;
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
                    else
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
