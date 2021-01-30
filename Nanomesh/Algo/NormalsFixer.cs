using System;

namespace Nanomesh
{
	public interface IAlgo
	{
		void Start(ConnectedMesh mesh);
		void Abort();
		event Action<int> Progressed;
		event Action Finished;
		event Action<Exception> Failed;
	}


	public class NormalsFixer : IAlgo
    {
        public event Action<int> Progressed;
        public event Action Finished;
        public event Action<Exception> Failed;

        public void Abort()
        {
            
        }

        public void Start(ConnectedMesh mesh)
        {
            for (int i = 0; i < mesh.attributes.Length; i++)
            {
                var attribute = mesh.attributes[i];
                attribute.normal = attribute.normal.Normalized;
                mesh.attributes[i] = attribute;
            }
        }
    }
}