namespace NS.Nodes
{
    public enum ENodeCategory
    {
        //------Flow Node Start------
        Event = 0,
        FlowControl = 1,
        //Executable
        ExecInstant = 100,
        ExecDebugInstant = 101,
        ExecNonInstant = 102,
        //------Flow Node End------
        
        //------Value Node Start------
        Value = 200,
        //------Value Node End------
    }
}