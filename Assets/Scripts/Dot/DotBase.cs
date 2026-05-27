public class DotBase
{
    protected int dot_count;
    /// <summary>
    /// 当前Dot层数
    /// </summary>
    public int Dot_count => dot_count;

    protected E_Dot dot_type;

    protected IBattlable self;
    public E_Dot Dot_type => dot_type;

    /// <summary>
    /// 弱点倍率
    /// </summary>
    protected float weakMulti = 2;
    public DotBase(E_Dot _dotType,IBattlable _self,int _dotCount){
        dot_count = _dotCount;
        dot_type = _dotType;
        self = _self;
    }

    /// <summary>
    /// 调整Dot层数
    /// </summary>
    /// <param name="levelCount">变化值（可正负）</param>
    public void AdjustDotLevel(int levelCount){
        dot_count += levelCount;
        if (dot_count < 0) dot_count = 0;
    }
    public virtual void OnDotTrigger() { }

    public virtual void OnDotUpdate() { }
}
