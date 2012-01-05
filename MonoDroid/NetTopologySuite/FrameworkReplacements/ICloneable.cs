#if SILVERLIGHT || MONODROID
namespace System
{
    public interface ICloneable
    {
        object Clone();
    }
}
#endif
