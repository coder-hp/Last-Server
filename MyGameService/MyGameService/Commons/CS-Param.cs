using System.Collections;
using System.Collections.Generic;

public class CSParam
{
    public enum NetTag
    {
        Register,
        Login,
        Bag,
        EnterGameMode2,
        CanEnterGameMode2,
        UserReady,
        GameMode2Start,
        SubmitState,
        GetUserState,
        BroadcastState,
        LeaveRoom,
    }

    public enum CodeType
    {
        Ok,
        LoginFail,
        RegisterFail_Exist,
        ParamError,
        ServerError,
    }
}

public class C2SBaseData
{
    public string Tag;
}

public class S2CBaseData
{
    public string Tag;
    public int Code;
}

public class C2S_Login : C2SBaseData
{
    public string Account;
    public string Password;
}

public class S2C_Login : S2CBaseData
{
    public int UserId = 0;
    public int zhanshiLevel = 0;
    public int fashiLevel = 0;
    public int huanshiLevel = 0;
}

public class C2S_Register : C2SBaseData
{
    public string Account = "";
    public string Password = "";
}

public class S2C_Register : S2CBaseData
{
    public int UserId = 0;
    public int zhanshiLevel = 0;
    public int fashiLevel = 0;
    public int huanshiLevel = 0;
}

public class C2S_Bag : C2SBaseData
{
    public int UserId = 0;
}

public class S2C_Bag : S2CBaseData
{
    public string data = "";
}

public class C2S_EnterGameMode2 : C2SBaseData
{
    public int UserId = 0;
}

public class S2C_EnterGameMode2 : S2CBaseData
{
}

public class C2S_SubmitState : C2SBaseData
{
    public int UserId = 0;
    public int action = 0;
    public float pos_x = 0;
    public float pos_y = 0;
    public float pos_z = 0;
    public float rotate_y = 0;
}

public class C2S_GetUserState : C2SBaseData
{
    public List<int> userId_list = new List<int>();
}

public class S2C_GetUserState : S2CBaseData
{
    public List<S2C_BroadcastState.BroadcastStateData> list = new List<S2C_BroadcastState.BroadcastStateData>();
}

public class C2S_UserReady : C2SBaseData
{
    public int UserId = 0;
}

public class C2S_LeaveRoom : C2SBaseData
{
    public int UserId = 0;
}

// ------------------------服务器通知-------------------------
public class S2C_CanEnterGameMode2 : S2CBaseData
{
    public List<int> allUserId = new List<int>();
}

public class S2C_GameMode2Start : S2CBaseData
{
}

public class S2C_BroadcastState : S2CBaseData
{
    public class BroadcastStateData
    {
        public int UserId = 0;
        public int action = 0;
        public float pos_x = 0;
        public float pos_y = 0;
        public float pos_z = 0;
        public float rotate_y = 0;
    }

    public List<BroadcastStateData> list = new List<BroadcastStateData>();
}