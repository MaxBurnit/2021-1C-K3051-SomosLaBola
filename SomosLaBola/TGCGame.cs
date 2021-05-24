using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using SomosLaBola.Cameras;
using SomosLaBola.Geometries;
using BepuPhysics;
using BepuUtilities.Memory;
using System.Collections.Generic;
using TGC.MonoGame.Samples.Physics.Bepu;
using TGC.MonoGame.Samples.Viewer;
using NumericVector3 = System.Numerics.Vector3;
using BepuPhysics.Collidables;

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
        private Vector3 CameraPosition { get; set; }
        private Vector3 CameraUpPosition { get; set; }

        //Graphics
        private GraphicsDeviceManager Graphics { get; }
        private SpriteBatch SpriteBatch { get; set; }

        //Models
        private Model Model { get; set; }
        private Effect Efecto { get; set; }
        private float Rotation { get; set; }
        private Model Cube { get; set; }
        private Model Sphere { get; set; }

        private Vector3 SpherePosition { get; set; }
        public Matrix SphereWorld { get; private set; }
        private TorusPrimitive Torus { get; set; }
        private Vector3 TorusPosition { get; set; }
        private CylinderPrimitive Cylinder { get; set; }
        private TeapotPrimitive Teapot { get; set; }
        private Vector3 TeapotPosition { get; set; }
        private CubePrimitive Box { get; set; }
        private Vector3 BoxPosition { get; set; }

        //Matrix
        private Matrix World { get; set; }
        private Matrix View { get; set; }
        private Matrix Projection { get; set; }
        public Matrix FloorWorld { get; set; }
        public List<Matrix> SpheresWorld { get; private set; }

        public Boolean puedoSaltar = true;
        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        /// 

        protected override void Initialize()
        {
            // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.

            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;
            // Seria hasta aca.
            InitializeContentM();

            base.Initialize();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        ///     que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void LoadContent()
        {

            SpriteFont = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "Arial");
            LoadContentM();
           
           

            base.LoadContent();
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

            SphereWorld = Matrix.CreateTranslation(SpherePosition) * Matrix.CreateScale(0.02f);

            //Update Camera
            Camera.TargetPosition = PositionE;

            Camera.Position = new Vector3(PositionE.X, PositionE.Y, PositionE.Z -500);

            Camera.BuildView();

            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                //Salgo del juego.
                Exit();

            UpdatePhysics();
            base.Update(gameTime);
        }

        private void UpdatePhysics()
        {
            //Physics
            Simulation.Timestep(1 / 60f, ThreadDispatcher);
            SpheresWorld.Clear();
            var sphereBody = Simulation.Bodies.GetBodyReference(SphereHandles[0]);

            //var spheresHandleCount = SphereHandles.Count;

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                
                sphereBody.Velocity.Linear = sphereBody.Velocity.Linear + new NumericVector3(0, 0, 5);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                sphereBody.Velocity.Linear = sphereBody.Velocity.Linear + new NumericVector3(0, 0, -5);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                sphereBody.Velocity.Linear = sphereBody.Velocity.Linear + new NumericVector3(5, 0, 0);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                sphereBody.Velocity.Linear = sphereBody.Velocity.Linear + new NumericVector3(-5, 0, 0);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                if (puedoSaltar)
                {
                    sphereBody.Velocity.Linear = sphereBody.Velocity.Linear + new NumericVector3(0, 100, 0);
                    puedoSaltar = false;
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.R) || PositionE.Y < -500)
            {
                sphereBody.Pose.Position = NumericVector3.Zero;
                sphereBody.Velocity.Linear = NumericVector3.Zero;
                sphereBody.Velocity.Angular = NumericVector3.Zero;
            }



            var pose = sphereBody.Pose;
                var position = pose.Position;
                var quaternion = pose.Orientation;
                var world = Matrix.CreateScale(0.3f) *
                            Matrix.CreateFromQuaternion(new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                                quaternion.W)) *
                            Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z));

                SpheresWorld.Add(world);
                PositionE = new Vector3(position.X, position.Y, position.Z);
                
            

            



        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logia de renderizado del juego.
            GraphicsDevice.Clear(Color.BlueViolet);
            DrawContentM();
           
        }

        private void DrawGeometry(GeometricPrimitive geometry, Vector3 position, float yaw = 0f, float pitch = 0f, float roll = 0f)
        {
            var effect = geometry.Effect;

            effect.World = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll) * Matrix.CreateTranslation(position) * Matrix.CreateScale(0.3f);
            effect.View = View;
            effect.Projection = Projection;

            geometry.Draw(effect);
        }

        private void InitializeContentM()
        {
            //Camera
            CameraPosition = Vector3.One * 100f;
            CameraUpPosition = new Vector3(-5, -5, 50 / 3f);
            CameraUpPosition.Normalize();
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, CameraPosition, Vector3.Zero);
            
            //Geometry
            Box = new CubePrimitive(GraphicsDevice);
            BoxPosition = new Vector3(0, -40, 0);
            SpherePosition = Vector3.Zero;


            // Configuramos nuestras matrices de la escena.
            FloorWorld = Matrix.CreateScale(2000, 0.1f, 2000) * Matrix.CreateTranslation(BoxPosition);
            SphereWorld = Matrix.CreateScale(0.02f);
            World = Matrix.Identity;
            View = Matrix.CreateLookAt(CameraPosition, SpherePosition, CameraUpPosition);
            Projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 250);

        }

        private void LoadContentM()
        {
            // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            Sphere = Content.Load<Model>(ContentFolder3D + "geometries/Sphere");
            
            EnableDefaultLighting(Sphere);

            // Cargo un efecto basico propio declarado en el Content pipeline.
            // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
            Efecto = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            LoadPhysics();

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

            Simulation.Statics.Add(new StaticDescription(new NumericVector3(0, -20, 0),
                new CollidableDescription(Simulation.Shapes.Add(new Box(2000, 100, 2000)), 1)));

            //Esfera
            SpheresWorld = new List<Matrix>();

            var radius = 0.03f;
            var sphereShape = new Sphere(radius);
            var position = new NumericVector3(0, 30.015934f, 0);
            var bodyDescription = BodyDescription.CreateConvexDynamic(position,1/radius*radius*radius, 
                Simulation.Shapes,sphereShape);

            var bodyHandle = Simulation.Bodies.Add(bodyDescription);

            SphereHandles.Add(bodyHandle);

            Radii.Add(radius);
        }

        private void DrawContentM()
        { 
            Box.Draw(FloorWorld, Camera.View, Camera.Projection);

            SpheresWorld.ForEach(sphereWorld => Sphere.Draw(sphereWorld, Camera.View, Camera.Projection));

            SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            SpriteBatch.DrawString(SpriteFont , PositionE.ToString() , new Vector2(GraphicsDevice.Viewport.Width - 400, 0), Color.White);
            SpriteBatch.End();

        }

        /// <summary>
        ///     Libero los recursos que se cargaron en el juego.
        /// </summary>
        private void EnableDefaultLighting(Model model)
        {
            foreach (var mesh in model.Meshes)
                ((BasicEffect)mesh.Effects.FirstOrDefault())?.EnableDefaultLighting();
        }
        protected override void UnloadContent()
        {

            // Libero los recursos.
            Content.Unload();

            base.UnloadContent();
        }
    }
}