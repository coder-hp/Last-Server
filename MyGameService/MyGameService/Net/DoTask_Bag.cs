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
    class DoTask_Bag
    {
        public static void Do(ClientInfo clientInfo, string data)
        {
            S2C_Bag s2c = new S2C_Bag();
            s2c.Tag = CSParam.NetTag.Bag.ToString();
            s2c.Code = (int)CSParam.CodeType.Ok;
            try
            {
                C2S_Bag c2s = JsonConvert.DeserializeObject<C2S_Bag>(data);
                int UserId = c2s.UserId;

                List<KeyData> keylist = new List<KeyData>() { new KeyData("id", UserId) };

                MySqlUtil.getInstance().addCommand(CmdType.query, "bag", keylist, null, (CmdReturnData cmdReturnData) =>
                {
                    if (cmdReturnData.result == CmdResult.OK)
                    {
                        s2c.Code = (int)CSParam.CodeType.Ok;

                        List<Object> list = cmdReturnData.listData;
                        List<Table_Bag> dataList = new List<Table_Bag>();
                        if (list != null && list.Count > 0)
                        {
                            dataList = Table_Bag.init(list);
                        }
                        s2c.data = JsonConvert.SerializeObject(dataList);
                        Socket_S.getInstance().Send(clientInfo, s2c);
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
