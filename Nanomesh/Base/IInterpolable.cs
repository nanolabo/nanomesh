namespace Nanomesh
{
    public interface IInterpolable<T>
    {
        public T Interpolate(T other, double ratio);
    }
}