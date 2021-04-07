using System.Collections;
using System.Collections.Generic;

public class CSParam
{
    public enum NetTag
    {
        Register,
        Login,
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
    public string UserId = "";
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
    public string UserId = "";
}

public class C2S_Register : C2SBaseData
{
    public string Account = "";
    public string Password = "";
}

public class S2C_Register : S2CBaseData
{
    public string UserId = "";
}