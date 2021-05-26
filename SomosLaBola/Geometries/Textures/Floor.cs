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
        public void Draw(Matrix world, Matrix view, Matrix projection)
        {

            foreach (ModelMesh mesh in Cube.Meshes)
            {
                Effect.Parameters["World"].SetValue(world);
                Effect.Parameters["View"].SetValue(view);
                Effect.Parameters["Projection"].SetValue(projection);
                Effect.Parameters["ModelTexture"].SetValue(Texture);
                Effect.CurrentTechnique = Effect.Techniques["PlatformShading"];
                mesh.Draw();
            }
            //Cube.Draw(world,view,projection);
        }

    }
}
