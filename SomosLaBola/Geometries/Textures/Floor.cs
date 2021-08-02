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
        public Texture2D PisoTexture;

        public Floor(TGCGame game) : base(game)
        {
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "PlatformShader");
            Texture = Game.Content.Load<Texture2D>(ContentFolderTextures + "Platform");
            PisoTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "piso2");
            Cube = Game.Content.Load<Model>(ContentFolder3D + "geometries/cube");
            foreach (ModelMesh mesh in Cube.Meshes)
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = Effect;
                }
        }
        public Model Cube { get; set; }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        public void Draw(Matrix world, Matrix view, Matrix projection, Vector3 cameraPosition, Vector3 lightPosition, int estadoFloor)
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
                
                switch(estadoFloor){
                    case 0 : Effect.Parameters["ambientColor"]?.SetValue(Color.DarkBlue.ToVector3());
                            Effect.Parameters["diffuseColor"]?.SetValue(Color.DarkBlue.ToVector3());
                            break;
                    case 1 : Effect.Parameters["ambientColor"]?.SetValue(Color.Yellow.ToVector3());
                            Effect.Parameters["diffuseColor"]?.SetValue(Color.Yellow.ToVector3());
                            break;
                    case 2 : Effect.Parameters["ambientColor"]?.SetValue(Color.Purple.ToVector3());
                            Effect.Parameters["diffuseColor"]?.SetValue(Color.Purple.ToVector3());
                            break;
                    case 3 : Effect.Parameters["ambientColor"]?.SetValue(Color.Green.ToVector3());
                            Effect.Parameters["diffuseColor"]?.SetValue(Color.Green.ToVector3());
                            break;
                    case 4 : Effect.Parameters["ambientColor"]?.SetValue(Color.Brown.ToVector3());
                            Effect.Parameters["diffuseColor"]?.SetValue(Color.Brown.ToVector3());
                            break;
                    case -1 : Effect.Parameters["ambientColor"]?.SetValue(Color.DarkGray.ToVector3());
                            Effect.Parameters["diffuseColor"]?.SetValue(Color.DarkGray.ToVector3());
                            Effect.Parameters["ModelTexture"]?.SetValue(PisoTexture);
                            break;
                    case -2 : Effect.Parameters["ambientColor"]?.SetValue(Color.DarkGray.ToVector3());
                            Effect.Parameters["diffuseColor"]?.SetValue(Color.DarkGray.ToVector3());
                           Effect.Parameters["ModelTexture"]?.SetValue(PisoTexture);
                            break;
                }

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
