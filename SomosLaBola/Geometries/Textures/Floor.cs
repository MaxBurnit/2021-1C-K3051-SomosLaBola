#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using SomosLaBola.Cameras;
using SomosLaBola.Geometries;
#endregion Using Statements

namespace SomosLaBola.Geometries.Textures
{
    class Floor : DrawableGameComponent
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderTextures = "Textures/";
        public const string ContentFolderEffects = "Effects/";
        public Effect Effect;
        public Texture2D Texture;

        public Floor(TGCGame game) : base(game)
        {
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "PlatformShader");
            Texture = Game.Content.Load<Texture2D>(ContentFolderTextures + "Platform");
            Cube = Game.Content.Load<Model>(ContentFolder3D + "geometries/cube");
            foreach (ModelMesh mesh in Cube.Meshes)
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = Effect;
                }
        }
        private Model Cube { get; set; }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        public void Draw(Matrix world, Matrix view, Matrix projection, Vector3 cameraPosition, Vector3 lightPosition)
        {
            
            foreach (ModelMesh mesh in Cube.Meshes)
            {
                Effect.Parameters["World"].SetValue(world);
                var WorldViewProjection = world * view * projection;
                Effect.Parameters["WorldViewProjection"]?.SetValue(WorldViewProjection);
                Effect.Parameters["InverseTransposeWorld"]?.SetValue(Matrix.Invert(Matrix.Transpose(world)));

                Effect.Parameters["ModelTexture"]?.SetValue(Texture);

                Effect.Parameters["KAmbient"]?.SetValue(0.8f);
                Effect.Parameters["KDiffuse"]?.SetValue(0.8f);
                Effect.Parameters["KSpecular"]?.SetValue(0.4f);

                Effect.Parameters["shininess"]?.SetValue(1f);

                Effect.Parameters["ambientColor"]?.SetValue(Color.Brown.ToVector3());
                Effect.Parameters["diffuseColor"]?.SetValue(Color.Brown.ToVector3());
                Effect.Parameters["specularColor"]?.SetValue(Color.White.ToVector3());

                Effect.Parameters["eyePosition"]?.SetValue(cameraPosition);
                Effect.Parameters["lightPosition"]?.SetValue(lightPosition);
                Effect.CurrentTechnique = Effect.Techniques["PlatformShading"];

                mesh.Draw();
            }
            //Cube.Draw(world,view,projection);
        }

    }
}
