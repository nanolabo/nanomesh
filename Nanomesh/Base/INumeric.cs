namespace Nanomesh
{
    public interface INumeric<T>
    {
        public T Multiply(double factor);
        public T Sum(T other);
    }
}
