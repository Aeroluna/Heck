namespace Heck
{
    public interface ICustomEventCustomData
    {
    }

    public interface IEventCustomData
    {
    }

    public interface IObjectCustomData
    {
    }

    public interface ICopyable<out T>
    {
        public T Copy();
    }
}
