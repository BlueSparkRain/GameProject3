public class DotBase
{
    int dot_count;
    /// <summary>
    /// 当前Dot层数
    /// </summary>
    public int Dot_count => dot_count;

    E_Dot dot_type;
    public E_Dot Dot_type => dot_type;
    public DotBase(E_Dot _dotType)
    {
        dot_count = 0;
        dot_type = _dotType;
    }

    /// <summary>
    /// 调整Dot层数
    /// </summary>
    /// <param name="levelCount">变化值（可正负）</param>
    public void AdjustDotLevel(int levelCount){
        dot_count += levelCount;
        if (dot_count < 0) dot_count = 0;
    }
    public virtual void DotTrigger() { }
}
