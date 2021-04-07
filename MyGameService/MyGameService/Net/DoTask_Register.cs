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
    class DoTask_Register
    {
        public static void Do(ClientInfo clientInfo, string data)
        {
            S2C_Register s2c = new S2C_Register();
            s2c.Tag = (int)CSParam.NetTag.Register;
            s2c.Code = (int)CSParam.CodeType.Ok;
            try
            {
                C2S_Register c2s = JsonConvert.DeserializeObject<C2S_Register>(data);
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
                            s2c.Code = (int)CSParam.CodeType.RegisterFail_Exist;
                            Socket_S.getInstance().Send(clientInfo, s2c);
                        }
                        else
                        {
                            string userId = account + password;
                            List<TableKeyObj> keylist2 = new List<TableKeyObj>() {
                                new TableKeyObj("id", TableKeyObj.ValueType.ValueType_string, userId),
                                new TableKeyObj("account", TableKeyObj.ValueType.ValueType_string, account),
                                new TableKeyObj("password", TableKeyObj.ValueType.ValueType_int, password)
                            };

                            MySqlUtil.getInstance().addCommand(CmdType.insert, "user", null, keylist2, (CmdReturnData cmdReturnData2) =>
                            {
                                if (cmdReturnData2.code == 1)
                                {
                                    s2c.Code = (int)CSParam.CodeType.Ok;
                                    s2c.UserId = userId;
                                    Socket_S.getInstance().Send(clientInfo, s2c);
                                }
                                // 插入新用户数据失败
                                else
                                {
                                    s2c.Code = (int)CSParam.CodeType.ServerError;
                                    Socket_S.getInstance().Send(clientInfo, s2c);
                                }
                            });
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
