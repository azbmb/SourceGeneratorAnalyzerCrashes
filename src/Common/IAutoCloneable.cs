namespace Common
{
    public interface IAutoCloneable<out T>
    {
        T Clone();
    }
}
