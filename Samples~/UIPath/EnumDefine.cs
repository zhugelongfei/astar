namespace Lonfee.AStar.Sample
{
    public enum EUserCtrlStatus
    {
        None,
        SetStart,
        SetTarget,
        SetBlock,
    }

    public enum ENodeTag
    {
        Normal,
        Open,
        Close,
        Block,
        Path,
        Start,
        Target,
        Folyd,
    }
}