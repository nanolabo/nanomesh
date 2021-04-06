using System;
using System.Collections.Generic;

namespace Nanomesh
{
    public enum AttributeType
    {
        Normals,
        UVs,
        BoneWeights,
        Colors,
    }

    public static class AttributeUtils
    {
        public static MetaAttributeList CreateAttributesFromDefinitions(IList<AttributeDefinition> attributeDefinitions)
        {
            MetaAttributeList attributeList = new EmptyMetaAttributeList(0);
            for (int i = 0; i < attributeDefinitions.Count; i++)
            {
                switch (attributeDefinitions[i].type)
                {
                    case AttributeType.Normals:
                        attributeList = attributeList.AddAttributeType<Vector3F>();
                        break;
                    case AttributeType.UVs:
                        attributeList = attributeList.AddAttributeType<Vector2F>();
                        break;
                    case AttributeType.BoneWeights:
                        attributeList = attributeList.AddAttributeType<BoneWeight>();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return attributeList;
        }
    }
}