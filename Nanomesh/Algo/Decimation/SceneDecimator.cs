using System.Collections.Generic;
using System.Linq;

namespace Nanomesh
{
    public class SceneDecimator
    {
        private class ModifierAndOccurrences
        {
            public int occurrences = 1;
            public DecimateModifier modifier = new DecimateModifier();
        }

        private Dictionary<ConnectedMesh, ModifierAndOccurrences> _modifiers;

        public void Initialize(IEnumerable<ConnectedMesh> meshes)
        {
            _modifiers = new Dictionary<ConnectedMesh, ModifierAndOccurrences>();

            foreach (ConnectedMesh mesh in meshes)
            {
                ModifierAndOccurrences modifier;
                if (_modifiers.ContainsKey(mesh))
                {
                    modifier = _modifiers[mesh];
                    modifier.occurrences++;
                }
                else
                {
                    _modifiers.Add(mesh, modifier = new ModifierAndOccurrences());
                    //System.Console.WriteLine($"Faces:{mesh.FaceCount}");
                    modifier.modifier.Initialize(mesh);
                }

                _faceCount += mesh.FaceCount;
            }

            _initalFaceCount = _faceCount;
        }

        private int _faceCount;
        private int _initalFaceCount;

        public void DecimateToRatio(float targetTriangleRatio)
        {
            targetTriangleRatio = MathF.Clamp(targetTriangleRatio, 0f, 1f);
            DecimateToPolycount((int)MathF.Round(targetTriangleRatio * _initalFaceCount));
        }

        public void DecimatePolycount(int polycount)
        {
            DecimateToPolycount((int)MathF.Round(_initalFaceCount - polycount));
        }

        public void DecimateToPolycount(int targetTriangleCount)
        {
            //System.Console.WriteLine($"Faces:{_faceCount} Target:{targetTriangleCount}");
            while (_faceCount > targetTriangleCount)
            {
                KeyValuePair<ConnectedMesh, ModifierAndOccurrences> pair = _modifiers.OrderBy(x => x.Value.modifier.GetMinimumError()).First();

                int facesBefore = pair.Key.FaceCount;
                pair.Value.modifier.Iterate();

                if (facesBefore == pair.Key.FaceCount)
                    break; // Exit !

                _faceCount -= (facesBefore - pair.Key.FaceCount) * pair.Value.occurrences;
            }
        }
    }
}