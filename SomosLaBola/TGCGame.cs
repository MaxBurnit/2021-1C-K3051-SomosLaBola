using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using SomosLaBola.Cameras;
using SomosLaBola.Geometries;
using TGC.MonoGame.Samples.Collisions;
using TGC.MonoGame.Samples.Viewer;
using BepuPhysics;
using BepuUtilities.Memory;
using System.Collections.Generic;
using TGC.MonoGame.Samples.Physics.Bepu;
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

        protected new TGCViewer Game { get; }

        //Physics
        private BufferPool BufferPool { get; set; }
        public List<float> Radii { get; private set; }
        public List<BodyHandle> SphereHandles { get; private set; }
        private Simulation Simulation { get; set; }
        //private SimpleThreadDispatcher ThreadDispatcher { get; set; }


        //Camera
        private Camera Camera { get; set; }
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
        private Vector3 BallAcceleration { get; set; }
        private Vector3 BallVelocity { get; set; }

        public List<Matrix> SpheresWorld { get; private set; }
        //Booleano para saber si la bola esta en el suelo
        private bool OnGround { get; set; }



        //Colliders
        private BoundingBox Collider{ get; set; }
        public SimpleThreadDispatcher ThreadDispatcher { get; private set; }
        

        private BoundingSphere _ballSphere;

        //constants
        private const float BallSideSpeed = 100f;
        private const float BallJumpSpeed = 150f;
        private const float Gravity = 350f;
        private const float BallRotatingVelocity = 0.06f;
        private const float EPSILON = 0.00001f;
        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        /// 

        protected override void Initialize()
        {
            // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.

            // Apago el backface culling.
            // Esto se hace por un problema en el diseno del modelo del logo de la materia.
            // Una vez que empiecen su juego, esto no es mas necesario y lo pueden sacar.
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

            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                //Salgo del juego.
                Exit();

            //Camera.Update(gameTime);
            UpdatePhysics();
            // Basado en el tiempo que paso se va generando una rotacion.
            //Rotation += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            base.Update(gameTime);
        }

        private void UpdatePhysics()
        {
            //Physics
            Simulation.Timestep(1 / 60f, ThreadDispatcher);
            SpheresWorld.Clear();
            var spheresHandleCount = SphereHandles.Count;

            for (var index = 0; index < spheresHandleCount; index++)
            {
                var pose = Simulation.Bodies.GetBodyReference(SphereHandles[index]).Pose;
                var position = pose.Position;
                var quaternion = pose.Orientation;
                var world = Matrix.CreateScale(Radii[index]) *
                            Matrix.CreateFromQuaternion(new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                                quaternion.W)) *
                            Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z));
                SpheresWorld.Add(world);
            }
        }

            

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logia de renderizado del juego.
            GraphicsDevice.Clear(Color.BlueViolet);

            /*// Para dibujar le modelo necesitamos pasarle informacion que el efecto esta esperando.
            Effect.Parameters["View"].SetValue(View);
            Effect.Parameters["Projection"].SetValue(Projection);
            Effect.Parameters["DiffuseColor"].SetValue(Color.DarkBlue.ToVector3());
            var rotationMatrix = Matrix.CreateRotationY(Rotation);

            foreach (var mesh in Model.Meshes)
            {
                World = mesh.ParentBone.Transform * rotationMatrix;
                Effect.Parameters["World"].SetValue(World);
                mesh.Draw();
            }*/
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
            CameraPosition = new Vector3(15, 15, 9);
            CameraUpPosition = new Vector3(-5, -5, 50 / 3f);
            CameraUpPosition.Normalize();
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, CameraPosition, Vector3.Zero);

            Box = new CubePrimitive(GraphicsDevice);
            BoxPosition = new Vector3(0, -10, 0);
            FloorWorld = Matrix.CreateScale(30f, 0.001f, 30f) * Matrix.CreateTranslation(BoxPosition);

            // Configuramos nuestras matrices de la escena.
            SpherePosition = Vector3.Zero;
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

            // Cargo el modelo del logo.
            //Model = Content.Load<Model>(ContentFolder3D + "tgc-logo/tgc-logo");

            Sphere = Content.Load<Model>(ContentFolder3D + "geometries/Sphere");
            
            EnableDefaultLighting(Sphere);


            // Cargo un efecto basico propio declarado en el Content pipeline.
            // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
            Efecto = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
            // Asigno el efecto que cargue a cada parte del mesh.
            // Un modelo puede tener mas de 1 mesh internamente.
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
               new PoseIntegratorCallbacks(new NumericVector3(0, -1, 0)), new PositionFirstTimestepper());

            Simulation.Statics.Add(new StaticDescription(new NumericVector3(0, -10, 0),
                new CollidableDescription(Simulation.Shapes.Add(new Box(2000, 1, 2000)), 1f)));

            //Esfera
            SpheresWorld = new List<Matrix>();

            var radius = 0.02f;
            var sphereShape = new Sphere();
            sphereShape.ComputeInertia(0.4f, out var sphereInertia);
            var sphereIndex = Simulation.Shapes.Add(sphereShape);
            var position = new NumericVector3(0, 0, 0);

            var bodyDescription = BodyDescription.CreateConvexDynamic(position, radius * radius * radius, Simulation.Shapes, sphereShape);

            var bodyHandle = Simulation.Bodies.Add(bodyDescription);

            SphereHandles.Add(bodyHandle);

            Radii.Add(radius);
        }

        private void DrawContentM()
        { 

            Box.Draw(FloorWorld, Camera.View, Camera.Projection);

            SpheresWorld.ForEach(sphereWorld => Sphere.Draw(sphereWorld, Camera.View, Camera.Projection));

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