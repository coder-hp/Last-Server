using SocketUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGameService.Game
{
    public enum RoomUserState
    {
        Wait,
        Ready,
    }
    public class RoomUserInfo
    {
        public ClientInfo clientInfo;
        public int userId;
        public RoomUserState roomUserState = RoomUserState.Wait;
        public S2C_BroadcastState.BroadcastStateData cmd = null;

        public RoomUserInfo(ClientInfo _clientInfo, int _userId)
        {
            clientInfo = _clientInfo;
            userId = _userId;
        }
    }

    public class RoomLogic
    {
        public List<RoomUserInfo> list_user = new List<RoomUserInfo>();

        public RoomLogic(List<WaitMatchUserInfo> userList)
        {
            for(int i = 0; i < userList.Count; i++)
            {
                list_user.Add(new RoomUserInfo(userList[i].clientInfo, userList[i].userId));
            }

            RoomManager.addRoom(this);
        }

        public bool checkIsExistUser(int userId)
        {
            for(int i = 0; i < list_user.Count; i++)
            {
                if(list_user[i].userId == userId)
                {
                    return true;
                }
            }

            return false;
        }

        public bool checkIsExistUser(ClientInfo clientInfo)
        {
            for (int i = 0; i < list_user.Count; i++)
            {
                if (list_user[i].clientInfo == clientInfo)
                {
                    return true;
                }
            }

            return false;
        }

        public void setUserState(int userId, RoomUserState roomUserState)
        {
            for (int i = 0; i < list_user.Count; i++)
            {
                if (list_user[i].userId == userId)
                {
                    list_user[i].roomUserState = roomUserState;

                    if(roomUserState == RoomUserState.Ready)
                    {
                        // 所有人准备好，开始游戏
                        if(checkIsAllReady())
                        {
                            S2C_GameMode2Start s2c = new S2C_GameMode2Start();
                            s2c.Tag = CSParam.NetTag.GameMode2Start.ToString();
                            s2c.Code = (int)CSParam.CodeType.Ok;

                            for (int j = 0; j < list_user.Count; j++)
                            {
                                Socket_S.getInstance().Send(list_user[j].clientInfo, s2c);
                            }
                        }
                    }
                    break;
                }
            }
        }

        public void getUserState(ClientInfo clientInfo, C2S_GetUserState c2s)
        {
            S2C_GetUserState s2c = new S2C_GetUserState();
            s2c.Tag = CSParam.NetTag.GetUserState.ToString();
            s2c.Code = (int)CSParam.CodeType.Ok;

            List<S2C_BroadcastState.BroadcastStateData> cmdList = new List<S2C_BroadcastState.BroadcastStateData>();
            for (int i = 0; i < c2s.userId_list.Count; i++)
            {
                RoomUserInfo roomUserInfo = getUser(c2s.userId_list[i]);
                if (roomUserInfo != null && roomUserInfo.cmd != null)
                {
                    cmdList.Add(roomUserInfo.cmd);
                }
            }
            s2c.list = cmdList;

            if (cmdList.Count > 0)
            {
                Socket_S.getInstance().Send(clientInfo, s2c);
            }
        }

        bool checkIsAllReady()
        {
            for (int i = 0; i < list_user.Count; i++)
            {
                if(list_user[i].roomUserState != RoomUserState.Ready)
                {
                    return false;
                }
            }

            return true;
        }

        public RoomUserInfo getUser(int userId)
        {
            for (int i = 0; i < list_user.Count; i++)
            {
                if (list_user[i].userId == userId)
                {
                    return list_user[i];
                }
            }

            return null;
        }

        public void deleteUser(int userId)
        {
            for (int i = 0; i < list_user.Count; i++)
            {
                if (list_user[i].userId == userId)
                {
                    list_user.RemoveAt(i);
                    break;
                }
            }
        }

        public void deleteUser(ClientInfo clientInfo)
        {
            for (int i = 0; i < list_user.Count; i++)
            {
                if (list_user[i].clientInfo == clientInfo)
                {
                    list_user.RemoveAt(i);
                    break;
                }
            }
        }

        public void broadcastState(C2S_SubmitState c2s)
        {
            S2C_BroadcastState.BroadcastStateData s2c = new S2C_BroadcastState.BroadcastStateData();
            s2c.UserId = c2s.UserId;
            s2c.action = c2s.action;
            s2c.pos_x = c2s.pos_x;
            s2c.pos_y = c2s.pos_y;
            s2c.pos_z = c2s.pos_z;
            s2c.rotate_y = c2s.rotate_y;

            RoomUserInfo roomUserInfo = getUser(c2s.UserId);
            if(roomUserInfo != null)
            {
                roomUserInfo.cmd = s2c;
            }
        }
    }
}
