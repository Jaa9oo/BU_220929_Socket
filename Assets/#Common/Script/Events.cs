public enum Type{
    Clinet,
    Server,
}

public class EVT_ReceveMsg{
    public Type rcvType;
    public string sMsg;
}

public class EVT_MoveForward
{
    public string name;
    public bool isForward;
}

public class EVT_MoveUp
{
    public string name;
    public bool isUp;
}

public class EVT_Rotate
{
    public string name;
    public bool isRight;
}

public class EVT_ChangeColor
{
    public string name;
}