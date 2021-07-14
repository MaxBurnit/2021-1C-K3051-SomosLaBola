﻿using System.Collections.Generic;
using System.Text;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SomosLaBola.Obstaculos.Recorridos;
using SomosLaBola.Utils;

namespace SomosLaBola.Obstaculos
{
    class ObstaculoMovil : DrawableGameComponent
    {

        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderTextures = "Textures/";

        public const string ContentFolder3D = "Models/";
        public Texture2D Texture;
        public Simulation Simulation;
        public List<BodyHandle> BoxBodyHandles;
   
        public Effect Effect;
        public ObstaculoMovil(Model model, Matrix transformaciones, IRecorrido recorrido,Simulation simulation, List<BodyHandle> SphereHandles,TGCGame game) : base(game)
        {
            BoxBodyHandles= new List<BodyHandle>();
            this.model = model;
            this.recorrido = recorrido;
            this.transformaciones = transformaciones;
            this.Simulation = simulation;
            var boxBodyHandle = Simulation.Bodies.Add(BodyDescription.CreateKinematic(new RigidPose(new System.Numerics.Vector3(transformaciones.Translation.X, transformaciones.Translation.Y, transformaciones.Translation.Z)), new CollidableDescription(Simulation.Shapes.Add(new Box(100f, 100f, 100f)), 0.1f), new BodyActivityDescription(-1f)));
            BoxBodyHandles.Add(boxBodyHandle);

            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "PlatformShader");
            Texture = Game.Content.Load<Texture2D>(ContentFolderTextures + "Platform");

            /*
            Simulation.Solver.Add(boxBodyHandle, SphereHandles[0],
                    new AngularAxisMotor
                    {
                        LocalAxisA = System.Numerics.Vector3.UnitY,
                        TargetVelocity = 5f,
                        Settings = new MotorSettings(5, 0.0001f)
                    });
            foreach (ModelMesh mesh in model.Meshes)
            foreach (var part in mesh.MeshParts)
            {
                part.Effect = Effect;
            }
            */

            previusPosition = Vector3Utils.toNumeric(transformaciones.Translation);
        }

        private Matrix transformaciones;
        private Model model;
        private IRecorrido recorrido;

        private System.Numerics.Vector3 previusPosition;

        public void Draw(GameTime gameTime, Matrix vista, Matrix proyeccion)
        {
            float tiempoTranscurrido = (float)gameTime.TotalGameTime.TotalSeconds;
            float deltaTiempo = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var matrixMundoTrasladada = transformaciones * recorrido.ObtenerMovimiento(tiempoTranscurrido);
            var boxBodyHandle = Simulation.Bodies.GetBodyReference(BoxBodyHandles[0]);

            boxBodyHandle.Pose.Position = new System.Numerics.Vector3(matrixMundoTrasladada.Translation.X, matrixMundoTrasladada.Translation.Y, matrixMundoTrasladada.Translation.Z);

            var velocity = boxBodyHandle.Pose.Position - previusPosition;
            boxBodyHandle.Velocity.Linear = velocity * (1 / deltaTiempo);

            previusPosition = boxBodyHandle.Pose.Position;

            foreach (ModelMesh mesh in model.Meshes)
            {
                Effect.Parameters["World"].SetValue(matrixMundoTrasladada);
                Effect.Parameters["View"].SetValue(vista);
                Effect.Parameters["Projection"].SetValue(proyeccion);
                Effect.Parameters["ModelTexture"].SetValue(Texture);
                Effect.CurrentTechnique = Effect.Techniques["PlatformShading"];
                mesh.Draw();
            }
            
          //  Cube.Draw(matrixMundoTrasladada, vista, proyeccion);
        }

        public static ObstaculoMovil CrearObstaculoRecorridoCircular(Model model, Matrix transformaciones, Simulation simulation, List<BodyHandle> SphereHandles, TGCGame game)
        {
            var recorrido = new RecorridoCircular(13, 7);
            return new ObstaculoMovil(model, transformaciones, recorrido, simulation, SphereHandles, game);
        }
        public static ObstaculoMovil CrearObstaculoRecorridoOnda(Model model, Matrix transformaciones, Simulation simulation, List<BodyHandle> SphereHandles, TGCGame game)
        {
            var recorrido = new RecorridoOnda(5, 3, 2);
            return new ObstaculoMovil(model, transformaciones, recorrido, simulation, SphereHandles, game);
        }
        public static ObstaculoMovil CrearObstaculoRecorridoVaiven(Model model, Matrix transformaciones, Simulation simulation, List<BodyHandle> SphereHandles, TGCGame game)
        {
            var recorrido = new Vaiven(4, 0.5f, 2);
            return new ObstaculoMovil(model, transformaciones, recorrido, simulation, SphereHandles, game);
        }

    }
}