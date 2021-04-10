namespace Nanomesh
{
    public interface IInterpolable<T>
    {
        T Interpolate(T other, double ratio);
    }
}