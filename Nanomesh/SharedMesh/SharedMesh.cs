using System;
using System.Collections.Generic;
using System.Text;

namespace Nanolabo
{
    public class SharedMesh
    {
        public Vector3[] vertices;
        public Vector3F[] normals;
        public Vector2F[] uvs;
        public int[] triangles;

        public void CheckValidity()
        {
            // Throw exceptions 
            for (int i = 0; i < triangles.Length; i+=3)
            {
                
            }
        }
    }
}