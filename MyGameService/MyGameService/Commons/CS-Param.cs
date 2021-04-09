using System.Collections;
using System.Collections.Generic;

public class CSParam
{
    public enum NetTag
    {
        Register,
        Login,
        Bag,
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
    public int Tag;
    public int UserId = 0;
}

public class S2CBaseData
{
    public int Tag;
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
    //public int UserId = 0;
}

public class S2C_Bag : S2CBaseData
{
    public string data = "";
}