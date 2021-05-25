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
            s2c.Tag = CSParam.NetTag.Register.ToString();
            s2c.Code = (int)CSParam.CodeType.Ok;
            try
            {
                C2S_Register c2s = JsonConvert.DeserializeObject<C2S_Register>(data);
                string account = c2s.Account;
                string password = c2s.Password;

                // 先查询这个账号是否存在
                List<KeyData> keylist = new List<KeyData>() { new KeyData("account", account) };
                MySqlUtil.getInstance().addCommand(CmdType.query, "user", keylist, null, (CmdReturnData cmdReturnData) =>
                {
                    if (cmdReturnData.result == CmdResult.OK)
                    {
                        List<Object> list = cmdReturnData.listData;

                        // 已存在，注册失败
                        if (list != null && list.Count > 0)
                        {
                            s2c.Code = (int)CSParam.CodeType.RegisterFail_Exist;
                            Socket_S.getInstance().Send(clientInfo, s2c);
                        }
                        // 不存在，先插入数据
                        else
                        {
                            List<KeyData> keylist2 = new List<KeyData>() {
                                new KeyData("account", account),
                                new KeyData("password", password)
                            };
                            MySqlUtil.getInstance().addCommand(CmdType.insert, "user", null, keylist2, (CmdReturnData cmdReturnData2) =>
                            {
                                if (cmdReturnData2.result == CmdResult.OK)
                                {
                                    // 插完数据后再查询该账号，返回userId
                                    MySqlUtil.getInstance().addCommand(CmdType.query, "user", keylist, null, (CmdReturnData cmdReturnData3) =>
                                    {
                                        if (cmdReturnData3.result == CmdResult.OK)
                                        {
                                            List<Object> list3 = cmdReturnData3.listData;
                                            if (list3 != null && list3.Count > 0)
                                            {
                                                Table_User table_User = Table_User.init(list3);
                                                s2c.Code = (int)CSParam.CodeType.Ok;
                                                s2c.UserId = table_User.id;
                                                Socket_S.getInstance().Send(clientInfo, s2c);
                                            }
                                            else
                                            {
                                                CommonUtil.Log("插入新用户数据后查询不到:account=" + account);
                                                s2c.Code = (int)CSParam.CodeType.ServerError;
                                                Socket_S.getInstance().Send(clientInfo, s2c);
                                            }
                                        }
                                        else
                                        {
                                            s2c.Code = (int)CSParam.CodeType.ServerError;
                                            Socket_S.getInstance().Send(clientInfo, s2c);
                                        }
                                    });
                                }
                                // 插入新用户数据失败
                                else
                                {
                                    CommonUtil.Log("插入新用户数据失败:account=" + account);
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
