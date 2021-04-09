using MyGameService.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketUtil
{
    class DoTaskClientReq
    {
        public static void Do(ClientInfo clientInfo, string data)
        {
            try
            {
                C2SBaseData c2sBaseData = JsonConvert.DeserializeObject<C2SBaseData>(data);
                
                switch (c2sBaseData.Tag)
                {
                    case (int)CSParam.NetTag.Login:
                        {
                            DoTask_Login.Do(clientInfo, data);
                        }
                        break;

                    case (int)CSParam.NetTag.Register:
                        {
                            DoTask_Register.Do(clientInfo, data);
                        }
                        break;

                    case (int)CSParam.NetTag.Bag:
                        {
                            DoTask_Bag.Do(clientInfo, data);
                        }
                        break;

                    default:
                        {
                            CommonUtil.Log("未知tag，不予处理：" + data);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                CommonUtil.Log("客户端传的数据有问题：" + data);
            }
        }
    }
}
