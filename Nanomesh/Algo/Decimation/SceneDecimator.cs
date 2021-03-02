using System.Collections.Generic;
using System.Linq;

namespace Nanomesh
{
    public class SceneDecimator
    {
		private HashSet<ConnectedMesh> _meshes;
		private Dictionary<ConnectedMesh, DecimateModifier> _modifiers;

		public void Initialize(IEnumerable<ConnectedMesh> meshes)
		{
			_meshes = new HashSet<ConnectedMesh>(meshes);
			_modifiers = new Dictionary<ConnectedMesh, DecimateModifier>();

			foreach (var mesh in meshes)
            {
				DecimateModifier modifier = new DecimateModifier();
				modifier.Initialize(mesh);
				_modifiers.Add(mesh, modifier);

				_faceCount += mesh.FaceCount;
			}
		}

		private int _faceCount;

		public void DecimateToRatio(float targetTriangleRatio)
		{
			targetTriangleRatio = MathF.Clamp(targetTriangleRatio, 0f, 1f);
			DecimateToPolycount((int)MathF.Round(targetTriangleRatio * _faceCount));
		}

		public void DecimatePolycount(int polycount)
		{
			DecimateToPolycount((int)MathF.Round(_faceCount - polycount));
		}

		public void DecimateToPolycount(int targetTriangleCount)
		{
			while (_faceCount > targetTriangleCount)
			{
				var pair = _modifiers.OrderBy(x => x.Value.GetMinimumError()).First();
				
				int facesBefore = pair.Key.FaceCount;
				pair.Value.Iterate();

				_faceCount -= facesBefore - pair.Key.FaceCount;
			}
		}
	}
}
