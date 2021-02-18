using System.Collections;

namespace Nanomesh
{
    public interface IAttributeList : IEnumerable
    {
        object this[int index] { get; set; }
        int Length { get; }
        double Weight { get; set; }
        void Interpolate(int indexA, int indexB, double ratio);
        IAttributeList Clone();
        IAttributeList CreateNew(int size);
    }
}