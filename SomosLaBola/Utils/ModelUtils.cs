using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SomosLaBola.Utils
{
    public static class ModelUtils
    {
        private static Vector3[] GetVertexPositions(ModelMeshPart meshPart, Matrix transform)
        {
            if (meshPart.VertexBuffer == null)
                return null;

            Vector3[] positions = VertexElementExtractor.GetVertexElement(meshPart, VertexElementUsage.Position);
            if (positions == null)
                return null;

            Vector3[] transformedPositions = new Vector3[positions.Length];
            Vector3.Transform(positions, ref transform, transformedPositions);
            return transformedPositions;
        }

        private static TBounding GetBounding<TBounding>(ModelMeshPart meshPart, Matrix transform, Func<Vector3[], TBounding> createBounding)
        {
            var transformedPositions = GetVertexPositions(meshPart, transform) ?? throw new ArgumentNullException("GetVertexPositions(meshPart, transform)");

            return createBounding(transformedPositions);
        }

        private static BoundingBox GetBoundingBox(ModelMeshPart meshPart, Matrix transform)
        {
            return GetBounding(meshPart, transform, BoundingBox.CreateFromPoints);
        }
        private static BoundingSphere GetBoundingSphere(ModelMeshPart meshPart, Matrix transform)
        {
            return GetBounding(meshPart, transform, BoundingSphere.CreateFromPoints);
        }

        public static TBounding CreateBounding<TBounding>(Model model, 
                                                          Func<ModelMeshPart, Matrix, TBounding> getBounding,
                                                          Func<TBounding, TBounding, TBounding> mergeBounding) {
            Matrix[] boneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            return model.Meshes
                .SelectMany(mesh =>
                    mesh.MeshParts.Select(meshPart => getBounding(meshPart, boneTransforms[mesh.ParentBone.Index])))
                .Aggregate(mergeBounding);
        }

        public static BoundingBox CreateBoundingBox(Model model)
        {
            return CreateBounding(model, GetBoundingBox, BoundingBox.CreateMerged);
        }


        public static BoundingSphere CreateBoundingSphere(Model model)
        {
            return CreateBounding(model, GetBoundingSphere, BoundingSphere.CreateMerged);
        }


    }
}
