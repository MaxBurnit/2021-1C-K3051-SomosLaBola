﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using SomosLaBola.Cameras;
using SomosLaBola.Geometries;
using SomosLaBola.Geometries.Textures;
using BepuPhysics;
using BepuUtilities.Memory;
using System.Collections.Generic;
using System.Numerics;
using TGC.MonoGame.Samples.Physics.Bepu;
using NumericVector3 = System.Numerics.Vector3;
using BepuPhysics.Collidables;
using SomosLaBola.Content.Textures;
using SomosLaBola.Obstaculos;
using SomosLaBola.Utils;
using Quaternion = Microsoft.Xna.Framework.Quaternion;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Microsoft.Xna.Framework.Media;

namespace SomosLaBola
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new TGCGame())
                game.Run();
        }
    }

    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);
            // Descomentar para que el juego sea pantalla completa.
            // Graphics.IsFullScreen = true;
            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
            // Hace que el mouse sea visible.
            IsMouseVisible = true;
        }
        private SpriteFont SpriteFont { get; set; }
        //Physics
        private BufferPool BufferPool { get; set; }
        public List<float> Radii { get; private set; }
        public List<BodyHandle> SphereHandles { get; private set; }
        private Simulation Simulation { get; set; }
        public SimpleThreadDispatcher ThreadDispatcher { get; private set; }
        public Vector3 PositionE { get; private set; }

        //Camera
        private TargetCamera Camera { get; set; }

        //Graphics
        private GraphicsDeviceManager Graphics { get; }
        private SpriteBatch SpriteBatch { get; set; }

        //Models
        private Effect Efecto { get; set; }
        private Model Sphere { get; set; }
        private Vector3 SpherePosition { get; set; }
        public Matrix SphereWorld { get; private set; }
        private CubePrimitive Box { get; set; }
        private Vector3 BoxPosition { get; set; }

        private ObstaculoMovil obstaculoEsfera;

        //Matrix
        public Matrix FloorWorld { get; set; }
        public List<Matrix> SpheresWorld { get; private set; }
        
        //Textures
        private Texture2D GreenTexture { get; set; }
        //private Vector3 DesiredLookAt;
        private List<Matrix> MatrixWorld { get; set; }
        private Floor Floor { get; set; }

        //private Vector3 ForwardDirection;
        public Boolean puedoSaltar = true;

        private Vector3 PlayerInitialPosition = new Vector3(0, 40, 0);

        private SkyBox Skybox;

        public const int ST_PRESENTACION = 0;
        public const int ST_STAGE = 1;
        public const int ST_CONTROLES = 2;

        public int status = ST_PRESENTACION;

        //Song
        private Song Song { get; set; }
        private string SongName { get; set; }
        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        /// 

        protected override void Initialize()
        {
            // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.Forward, PlayerInitialPosition);

            var rasterizerState = new RasterizerState {CullMode = CullMode.None};
            GraphicsDevice.RasterizerState = rasterizerState;

            // Seria hasta aca.
            InitializeContent();

            base.Initialize();
        }

        private void InitializeContent()
        {
            //Geometry
            Box = new CubePrimitive(GraphicsDevice);
            BoxPosition = new Vector3(0, 0, 0);
            SpherePosition = Vector3.Zero;

            // Configuramos nuestras matrices de la escena.
            FloorWorld = Matrix.CreateScale(2000, 0.1f, 2000) * Matrix.CreateTranslation(BoxPosition);
            SphereWorld = Matrix.CreateScale(0.02f);

        }

        protected override void LoadContent()
        {
            SpriteFont = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "Arial");
            // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            MatrixWorld = new List<Matrix>();
           // ObstaculoCubo = ObstaculoMovil.CrearObstaculoRecorridoCircular(Sphere, Matrix.CreateScale(0.1f, 0.1f, 0.1f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(0, 13, -40)));
            LoadPhysics();
            generateMatrixWorld();
            Floor = new Floor(this);

            Sphere = Content.Load<Model>(ContentFolder3D + "geometries/sphere");

            GreenTexture = Content.Load<Texture2D>(ContentFolderTextures + "green");
            
            EnableDefaultLighting(Sphere);
   

            // Cargo un efecto basico propio declarado en el Content pipeline.
            // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
            Efecto = Content.Load<Effect>(ContentFolderEffects + "BasicShader");


            var skyBox = Content.Load<Model>("3D/skybox/cube");
            var skyBoxTexture = Content.Load<TextureCube>(ContentFolderTextures + "skyboxes/skybox/skybox");
            var skyBoxEffect = Content.Load<Effect>(ContentFolderEffects + "SkyBox");
            Skybox = new SkyBox(skyBox, skyBoxTexture, skyBoxEffect, Camera.FarPlane/2);
            //Skybox = new SkyBox(skyBox, skyBoxTexture, skyBoxEffect, 1000);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            SongName = "funkorama";
            Song = Content.Load<Song>(ContentFolderMusic + SongName);
            MediaPlayer.Play(Song);
            // Asigno el efecto que cargue a cada parte del mesh.
            // Un modelo puede tener mas de 1 mesh internamente.
            //foreach (var mesh in Cube.Meshes)
            // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
            //foreach (var meshPart in mesh.MeshParts)
            //  meshPart.Effect = Efecto;



            base.LoadContent();
        }

        private void generateMatrixWorld()
        {
            float inicio = Simulation.Bodies.GetBodyReference(SphereHandles[0]).Pose.Position.Z;
            float incremento = 0f;
            float posZ = 0f;
            Vector3 pos;
            for (int i = 0; i < 90; i++)
            {
                //       Cube.Draw(Matrix.CreateRotationZ(MathHelper.Pi * 0.25f) * Matrix.CreateScale(escaladoXY, escaladoXY, escaladoZ) * Matrix.CreateTranslation(new Vector3(inicio + decremento, inicio + decremento, -escaladoZ)), View,
                // Projection);
                posZ = inicio + incremento;
                pos = Vector3.Forward * posZ;
                MatrixWorld.Add(Matrix.CreateScale(60f, 2f, 20f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(pos));
                Simulation.Statics.Add(new StaticDescription( Vector3Utils.toNumeric(pos), new CollidableDescription(Simulation.Shapes.Add(new Box(120f, 8f, 40f)),1)));
                if (i > 10 && i <= 29)
                {
                    //Agrego una collision vertical para que choque con la pared
                    //    if (i == 11)
                    //        Simulation.Statics.Add(new StaticDescription(new NumericVector3(0f, 0f, posZ), new CollidableDescription(Simulation.Shapes.Add(new Box(120f, 60f, 40f)), 1)));
                    pos += Vector3.Up * 12; 
                    MatrixWorld.Add(Matrix.CreateScale(60f, 30f, 20f) * Matrix.CreateRotationX(MathHelper.Pi) * Matrix.CreateTranslation(pos));
                    Simulation.Statics.Add(new StaticDescription(Vector3Utils.toNumeric(pos), new CollidableDescription(Simulation.Shapes.Add(new Box(120f, 64f, 40f)), 1)));

                }

                incremento += 40;
            }
            //posZ = inicio + incremento;
            //   MatrixWorld.Add(Matrix.CreateScale(70f, 2f, 100f) * Matrix.CreateRotationX(MathHelper.Pi * (0.08f)) * Matrix.CreateTranslation(new Vector3(0, 12, posZ + 80f)));

            posZ = inicio + incremento + 490;
            pos = Vector3.Forward * posZ;
            
            Simulation.Statics.Add(new StaticDescription(Vector3Utils.toNumeric(pos), new CollidableDescription(Simulation.Shapes.Add(new Box(640f, 8f, 1000f)), 1)));
            MatrixWorld.Add(Matrix.CreateScale(300f, 2f, 500f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(pos));
        }

        private void LoadPhysics()
        {
            //Physics
            BufferPool = new BufferPool();

            Radii = new List<float>();

            SphereHandles = new List<BodyHandle>();

            var targetThreadCount = Math.Max(1,
               Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            ThreadDispatcher = new SimpleThreadDispatcher(targetThreadCount);

            Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(),
               new PoseIntegratorCallbacks(new NumericVector3(0, -500, 0)), new PositionFirstTimestepper());

       //     Simulation.Statics.Add(new StaticDescription(new NumericVector3(0, 0, 0),
    //new CollidableDescription(Simulation.Shapes.Add(new Box(2000, 100, 5000)), 1)));
            //  for (int i=0;i<MatrixWorld.Count;i++)
            //  {
            //      Matrix world = MatrixWorld[i];
            //      Simulation.Statics.Add(new StaticDescription(new NumericVector3(world.Translation.X, world.Translation.Y, world.Translation.Z),
            //     new CollidableDescription(Simulation.Shapes.Add(new Box(200, 100, 500)), 1)));
            //  }
            //Simulation.Statics.Add(new StaticDescription(new NumericVector3(0, -20, 0),
              //  new CollidableDescription(Simulation.Shapes.Add(new Box(200, 100, 500)), 1)));

            //Simulation.Statics.Add(new StaticDescription(new NumericVector3(9, 28, 200), new CollidableDescription(Simulation.Shapes.Add(new Sphere(2f)),1)));
            //Simulation.Statics.Add(new StaticDescription(new ))
           /* for (int i = 0; i < MatrixWorld.Count(); i++)
            {
                Simulation.Statics.Add(new StaticDescription(MatrixWorld[i].))
                Floor.Draw(MatrixWorld[i], Camera.View, Camera.Projection);
            }*/


            //Simulation.Statics.Add(new StaticDescription(new NumericVector3(0, -20, 0), new CollidableDescription(Simulation.Shapes.Add(
              //  new Box(2000, 100, 2000)), 1)));
            //Esfera
            SpheresWorld = new List<Matrix>();


            var radius = 5f;
            var sphereShape = new Sphere(radius);
            var position = Vector3Utils.toNumeric(PlayerInitialPosition);
            var bodyDescription = BodyDescription.CreateConvexDynamic(position, 1 / radius * radius * radius,
                Simulation.Shapes, sphereShape);

            var bodyHandle = Simulation.Bodies.Add(bodyDescription);

            SphereHandles.Add(bodyHandle);

            Radii.Add(radius);

        }

        private void EnableDefaultLighting(Model model)
        {
            foreach (var mesh in model.Meshes)
                ((BasicEffect)mesh.Effects.FirstOrDefault())?.EnableDefaultLighting();
        }

        protected override void Draw(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logia de renderizado del juego.
            GraphicsDevice.Clear(Color.Black);
            //Box.Draw(FloorWorld, Camera.View, Camera.Projection);
            createStage();
            Efecto.Parameters["View"].SetValue(Camera.View);
            Efecto.Parameters["Projection"].SetValue(Camera.Projection);

            GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.None };

            Skybox.Draw(Camera.View, Camera.Projection, Camera.Position);


            float tiempoTranscurrido = (float)gameTime.TotalGameTime.TotalSeconds;
            //ObstaculoCubo.Draw(tiempoTranscurrido, Camera.View, Projection);
            SpheresWorld.ForEach(sphereWorld => Sphere.Draw(sphereWorld, Camera.View, Camera.Projection));

            SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque,
                SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            SpriteBatch.DrawString(SpriteFont, PositionE.ToString(), new Vector2(GraphicsDevice.Viewport.Width - 400, 0), Color.White);
            SpriteBatch.DrawString(SpriteFont, "\"R\" para REINICIAR", new Vector2(10, 0), Color.White);
            var sphereBody = Simulation.Bodies.GetBodyReference(SphereHandles[0]);
            var stringSalto = "SALTO";
            if (puedoSaltar) SpriteBatch.DrawString(SpriteFont, stringSalto,
                new Vector2(0, 30), Color.CornflowerBlue);
            else SpriteBatch.DrawString(SpriteFont, stringSalto, new Vector2(0, 30), Color.DarkGray);
            SpriteBatch.End();
            

        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logica de actualizacion del juego.
            var deltaTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
          
            SphereWorld = Matrix.CreateTranslation(SpherePosition) * Matrix.CreateScale(0.3f);
            Vector3 SpherePositionM = new Vector3
                                                            (Simulation.Bodies.GetBodyReference(SphereHandles[0]).Pose.Position.X,
                                                            Simulation.Bodies.GetBodyReference(SphereHandles[0]).Pose.Position.Y,
                                                            Simulation.Bodies.GetBodyReference(SphereHandles[0]).Pose.Position.Z);

            // Camera.Position = new Vector3(SpherePositionM.X, SpherePositionM.Y, SpherePositionM.Z - 500);

            Camera.Update(gameTime, SpherePositionM);

            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                //Salgo del juego.
                Exit();
            
            if(MediaPlayer.State == MediaState.Stopped)
            {
                MediaPlayer.Play(Song);
            }
            UpdatePhysics();
            base.Update(gameTime);
        }

        private void UpdatePhysics()
        {
            //Physics
            Simulation.Timestep(1/ 60f, ThreadDispatcher);
            SpheresWorld.Clear();
            var sphereBody = Simulation.Bodies.GetBodyReference(SphereHandles[0]);

            var playerAceleration = 5;
           
            //var plataforma = Simulation.Statics.GetStaticReference(StaticHandle[0]);

            //var spheresHandleCount = SphereHandles.Count;

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                sphereBody.Awake = true;
                sphereBody.Velocity.Linear = sphereBody.Velocity.Linear + NumericVector3Utils.Forward * playerAceleration;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                sphereBody.Awake = true;
                sphereBody.Velocity.Linear = sphereBody.Velocity.Linear + NumericVector3Utils.Backward * playerAceleration;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                sphereBody.Awake = true;
                sphereBody.Velocity.Linear = sphereBody.Velocity.Linear + NumericVector3Utils.Left * playerAceleration;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                sphereBody.Awake = true;
                sphereBody.Velocity.Linear = sphereBody.Velocity.Linear + NumericVector3Utils.Right * playerAceleration;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                //sphereBody.Velocity.Linear = sphereBody.Velocity.Linear + new NumericVector3(0, 100, 0);
                if (puedoSaltar)
                {
                    var jumpImpulseForce = 1000;
                    sphereBody.Awake = true;
                    sphereBody.ApplyLinearImpulse(NumericVector3Utils.Up * jumpImpulseForce);
                    puedoSaltar = false;

                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.R) || PositionE.Y < -500)
            {
                sphereBody.Awake = true;
                sphereBody.Pose.Position = new NumericVector3(0f,10f,0f);
                sphereBody.Velocity.Linear = NumericVector3.Zero;
                sphereBody.Velocity.Angular = NumericVector3.Zero;
                puedoSaltar = true;
            }

            var pose = sphereBody.Pose;
            var position = pose.Position;
            var quaternion = pose.Orientation;
            var world = Matrix.CreateScale(0.05f) *
                        Matrix.CreateFromQuaternion(new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                            quaternion.W)) *
                        Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z));

            SpheresWorld.Add(world);
            
            PositionE = new Vector3(position.X, position.Y, position.Z);

        }

        private void createStage()
        {
            for (int i = 0; i < MatrixWorld.Count(); i++)
            {
                Floor.Draw(MatrixWorld[i], Camera.View, Camera.Projection);
            }
            // worldMatrix=generarCubosRectos(4);
            // Floor.draw();
        }

        /// <summary>
        ///     Libero los recursos que se cargaron en el juego.
        /// </summary>
        protected override void UnloadContent()
        {

            // Libero los recursos.
            Content.Unload();

            base.UnloadContent();
        }

    }
}