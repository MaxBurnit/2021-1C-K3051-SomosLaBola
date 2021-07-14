using Microsoft.Xna.Framework;
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
using System.Diagnostics;
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
using SomosLaBola.Obstaculos.Recorridos;
using SomosLaBola.PlayerInfo;
using SomosLaBola.Powerups;
using Vector4 = Microsoft.Xna.Framework.Vector4;

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
        public const string ContentFolder3D = "3D/";
        public const string ContentFolderModels = "Models/";
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
            //Graphics.IsFullScreen = true;
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
        private Effect EfectoBasico { get; set; }
        private Effect EfectoEM { get; set; }
        private Model Sphere { get; set; }

        private Model Cube { get; set; }
        private Vector3 SpherePosition { get; set; }
        public Matrix SphereWorld { get; private set; }
        private CubePrimitive Box { get; set; }
        private Vector3 BoxPosition { get; set; }

        private Model TankModel { get; set; }

        private ObstaculoMovil obstaculoEsfera;

        private ObstaculoMovil obstMovil { get; set; }
        //Matrix
        public Matrix FloorWorld { get; set; }
        public List<Matrix> SpheresWorld { get; private set; }
        private Matrix QuadWorld;

        //Textures
        private Texture2D GomaTexture { get; set; }
        private Texture2D MetalTexture { get; set; }
        private Texture2D MaderaTexture { get; set; }

        private const int EnvironmentmapSize = 2048;
        private RenderTargetCube EnvironmentMapRenderTarget { get; set; }
        private StaticCamera CubeMapCamera { get; set; }
        private Effect DebugTextureEffect { get; set; }
        private FullScreenQuad FullScreenQuad { get; set; }

        //private Vector3 DesiredLookAt;
        private List<Matrix> MatrixWorld { get; set; }

        private List<Matrix> MatrixWorldObs { get; set; }
        
        private Floor Floor { get; set; }

        //private Vector3 ForwardDirection;
        public Boolean puedoSaltar = false;
        public float velocidadAngularYAnt;
        public float velocidadLinearYAnt;

        private Vector3 PlayerInitialPosition = Checkpoint.CurrentCheckpoint;

        private SkyBox Skybox;

        public const int ST_PRESENTACION = 0;
        public const int ST_STAGE = 1;
        public const int ST_CONTROLES = 2;

        public int status = ST_PRESENTACION;

        //Materiales
        public const int M_Goma = 0;
        public const int M_Metal = 1;
        public const int M_Madera = 2;

        public int Material = M_Goma;
        public int ProxMaterial = M_Goma;

        public float KAmbientGoma = 0.8f;
        public float KDiffuseGoma = 0.6f;
        public float KSpecularGoma = 0.8f;

        public float KAmbientMetal = 0.9f;
        public float KDiffuseMetal = 0.6f;
        public float KSpecularMetal = 0.8f;

        public float KAmbientMadera = 0.5f;
        public float KDiffuseMadera = 0.5f;
        public float KSpecularMadera = 0.4f;
        

        private bool MPresionada;

        //Song
        private Song Song { get; set; }
        private string SongName { get; set; }

        private float Timer { get; set; }



        private List<Trigger> powerUps = new List<Trigger>();
        private List<Checkpoint> checkpoints = new List<Checkpoint>();
        private List<Trigger> initialPowerUps;

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

            CubeMapCamera = new StaticCamera(1f, SpherePosition, Vector3.UnitX, Vector3.Up);
            CubeMapCamera.BuildProjection(1f, 1f, 3000f, MathHelper.PiOver2);


        }

        protected override void LoadContent()
        {
            SpriteFont = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "Arial");
            Cube = Content.Load<Model>(ContentFolder3D + "geometries/cube");
            // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            MatrixWorld = new List<Matrix>();
            MatrixWorldObs = new List<Matrix>();
           // ObstaculoCubo = ObstaculoMovil.CrearObstaculoRecorridoCircular(Sphere, Matrix.CreateScale(0.1f, 0.1f, 0.1f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(0, 13, -40)));
            LoadPhysics();
            generateMatrixWorld();
            Floor = new Floor(this);

            Sphere = Content.Load<Model>(ContentFolderModels + "geometries/sphere");
            TankModel = Content.Load<Model>(ContentFolder3D + "tank/tank");

            GomaTexture = Content.Load<Texture2D>(ContentFolderTextures + "rubber");
            MetalTexture = Content.Load<Texture2D>(ContentFolderTextures + "metal-bola");
            MaderaTexture = Content.Load<Texture2D>(ContentFolderTextures + "wood/caja-madera-4");

            //EnableDefaultLighting(Sphere);

            checkpoints.Add(new Checkpoint(new Vector3(0, 65, -1000)));
            checkpoints.Add(new Checkpoint(new Vector3(0, 65, -2700)));
            checkpoints.Add(new Checkpoint(new Vector3(0, 20, -8500)));
            checkpoints.Add(new Checkpoint(new Vector3(2500, -650, -9300)));
            checkpoints.Add(new Checkpoint(new Vector3(8850, -650, -9150)));

            powerUps.Add(new Planear(new Vector3(0, 20, -100)));
            initialPowerUps = new List<Trigger>(powerUps);


            // Cargo un efecto basico propio declarado en el Content pipeline.
            // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
            Efecto = Content.Load<Effect>(ContentFolderEffects + "BlinnPhong");
            Efecto.Parameters["ModelTexture"]?.SetValue(GomaTexture);

            EfectoBasico = Content.Load<Effect>(ContentFolderEffects + "BlinnPhong");
            EfectoEM = Content.Load<Effect>(ContentFolderEffects + "EnvironmentMap");

            var skyBox = Content.Load<Model>("3D/skybox/cube");
            var skyBoxTexture = Content.Load<TextureCube>(ContentFolderTextures + "skyboxes/skybox/skybox");
            var skyBoxEffect = Content.Load<Effect>(ContentFolderEffects + "SkyBox");
            Skybox = new SkyBox(skyBox, skyBoxTexture, skyBoxEffect, Camera.FarPlane/2);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            EnvironmentMapRenderTarget = new RenderTargetCube(GraphicsDevice, EnvironmentmapSize, false,
                SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            SongName = "funkorama";
            Song = Content.Load<Song>(ContentFolderMusic + SongName);
            MediaPlayer.Play(Song);
            // Asigno el efecto que cargue a cada parte del mesh.
            // Un modelo puede tener mas de 1 mesh internamente.
            //foreach (var mesh in Cube.Meshes)
            // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
            //foreach (var meshPart in mesh.MeshParts)
            //  meshPart.Effect = Efecto;

            foreach (var meshPart in Sphere.Meshes.SelectMany(mesh => mesh.MeshParts))
                meshPart.Effect = Efecto;

            DebugTextureEffect = Content.Load<Effect>(ContentFolderEffects + "DebugTexture");
            DebugTextureEffect.CurrentTechnique = DebugTextureEffect.Techniques["DebugCubeMap"];

            FullScreenQuad = new FullScreenQuad(GraphicsDevice);

            QuadWorld = Matrix.CreateScale(new Vector3(0.9f, 0.2f, 0f)) * Matrix.CreateTranslation(Vector3.Down * 0.7f);

            base.LoadContent();
        }

        private void generateMatrixWorld()
        {
            float posZ = 0f;
            System.Numerics.Quaternion rot=System.Numerics.Quaternion.CreateFromRotationMatrix(Matrix4x4.CreateRotationZ(-MathHelper.Pi / 10));
            Vector3 pos = Vector3.Forward * posZ;

            MatrixWorld.Add(Matrix.CreateScale(60f, 2f, 400f)  * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(pos.X, pos.Y, pos.Z - 400f)));
            MatrixWorld.Add(Matrix.CreateScale(60f, 30f, 1000f)* Matrix.CreateRotationX(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(pos.X, pos.Y + 28, pos.Z - 1800f)));
            MatrixWorld.Add(Matrix.CreateScale(60f, 2f, 3000f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(pos.X, pos.Y, pos.Z - 5800)));
            MatrixWorld.Add(Matrix.CreateScale(300f, 2f, 500f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(pos.X, pos.Y, pos.Z - 9300)));
            MatrixWorld.Add(Matrix.CreateScale(300f, 2f, 500f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(pos.X, pos.Y + 200f, pos.Z - 9300)));
            MatrixWorld.Add(Matrix.CreateScale(10f, 100f, 500f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(pos.X - 310f, pos.Y + 100f, pos.Z - 9300)));
            MatrixWorld.Add(Matrix.CreateScale(1000f, 2f, 150f) * Matrix.CreateRotationZ(-MathHelper.Pi / 10) * Matrix.CreateTranslation(new Vector3(pos.X + 1250f, pos.Y - 310f, pos.Z - 9300)));
            MatrixWorld.Add(Matrix.CreateScale(10f, 100f, 200) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(pos.X+ 290f, pos.Y + 100f, pos.Z - 9600)));
            MatrixWorld.Add(Matrix.CreateScale(10f, 100f, 200) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(pos.X + 290f, pos.Y + 100f, pos.Z - 9000)));
            MatrixWorld.Add(Matrix.CreateScale(300f, 100f, 10f)* Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(pos.X, pos.Y + 100f, pos.Z - 9810)));
            MatrixWorld.Add(Matrix.CreateScale(300f, 2f, 600f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(pos.X + 2590f, pos.Y - 700f, pos.Z - 9300)));
            MatrixWorld.Add(Matrix.CreateScale(3000f, 2f, 300f)* Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(pos.X + 5890f, pos.Y - 700f, pos.Z - 9300)));
            MatrixWorld.Add(Matrix.CreateScale(600f, 2f, 900f)* Matrix.CreateRotationZ(-MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(pos.X + 9490f, pos.Y - 700f, pos.Z - 9300)));
            float lastPosition=generateIndividualPlatforms(pos);
            MatrixWorld.Add(Matrix.CreateScale(500f, 10f, 500f) * Matrix.CreateRotationZ(-MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(pos.X + lastPosition + 500f+300f +300f, pos.Y - 650f, pos.Z - 9300)));
            MatrixWorldObs.Add(Matrix.CreateScale(50f, 50f, 50f) * Matrix.CreateRotationZ(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(pos.X + 5890f, pos.Y-650f, pos.Z - 9600)));
            createObstaculoMovil();
            //Es la misma traslación del bloque                                                    Es el doble del escalado del bloque.
            Simulation.Statics.Add(new StaticDescription(Vector3Utils.toNumeric(new Vector3(pos.X, pos.Y, pos.Z - 400f)), new CollidableDescription(Simulation.Shapes.Add(new Box(120f, (pos.Y + 2) * 2, 800f)), 0.1f)));
            Simulation.Statics.Add(new StaticDescription(Vector3Utils.toNumeric(new Vector3(pos.X, pos.Y + 28, pos.Z - 1800f)), new CollidableDescription(Simulation.Shapes.Add(new Box(120f, (pos.Y + 30) * 2, 1000f * 2)), 0.1f)));
            Simulation.Statics.Add(new StaticDescription(Vector3Utils.toNumeric(new Vector3(pos.X, pos.Y, pos.Z - 5800)), new CollidableDescription(Simulation.Shapes.Add(new Box(120f, (pos.Y + 2) * 2, 3000f * 2)), 0.1f)));
            Simulation.Statics.Add(new StaticDescription(Vector3Utils.toNumeric(new Vector3(pos.X, pos.Y, pos.Z - 9300)), new CollidableDescription(Simulation.Shapes.Add(new Box(300f * 2, (pos.Y + 2) * 2, 500f * 2)), 0.1f)));
            Simulation.Statics.Add(new StaticDescription(Vector3Utils.toNumeric(new Vector3(pos.X, pos.Y+200, pos.Z - 9300)), new CollidableDescription(Simulation.Shapes.Add(new Box(300f * 2, (pos.Y + 2) * 2, 500f * 2)), 0.1f)));
            Simulation.Statics.Add(new StaticDescription(Vector3Utils.toNumeric(new Vector3(pos.X - 310f, pos.Y + 100f, pos.Z - 9300)), new CollidableDescription(Simulation.Shapes.Add(new Box(10f * 2, (pos.Y + 100f) * 2, 500f * 2)), 0.1f)));
            Simulation.Statics.Add(new StaticDescription(Vector3Utils.toNumeric(new Vector3(pos.X + 1250f, pos.Y - 310f, pos.Z - 9300)), rot, new CollidableDescription(Simulation.Shapes.Add(new Box(1000f * 2, (pos.Y + 2) * 2, 150f * 2)), 0.1f)));
            Simulation.Statics.Add(new StaticDescription(Vector3Utils.toNumeric(new Vector3(pos.X + 290f, pos.Y + 100f, pos.Z - 9600)), new CollidableDescription(Simulation.Shapes.Add(new Box(10f * 2, (pos.Y + 100) * 2, 200f * 2)), 0.1f)));
            Simulation.Statics.Add(new StaticDescription(Vector3Utils.toNumeric(new Vector3(pos.X + 290f, pos.Y + 100f, pos.Z - 9000)), new CollidableDescription(Simulation.Shapes.Add(new Box(10f * 2, (pos.Y + 100) * 2, 200f * 2)), 0.1f)));
            Simulation.Statics.Add(new StaticDescription(Vector3Utils.toNumeric(new Vector3(pos.X, pos.Y + 100f, pos.Z - 9810)), new CollidableDescription(Simulation.Shapes.Add(new Box(300f * 2, (pos.Y + 100f) * 2, 10f * 2)), 0.1f)));
            Simulation.Statics.Add(new StaticDescription(Vector3Utils.toNumeric(new Vector3(pos.X + 2590f, pos.Y - 700f, pos.Z - 9300)), new CollidableDescription(Simulation.Shapes.Add(new Box(300f * 2, (pos.Y + 2) * 2, 600f * 2)), 0.1f)));
            Simulation.Statics.Add(new StaticDescription(Vector3Utils.toNumeric(new Vector3(pos.X + 5890f, pos.Y - 700f, pos.Z - 9300)), new CollidableDescription(Simulation.Shapes.Add(new Box(3000f * 2, (pos.Y + 2) * 2, 500f * 2)), 0.1f)));
            Simulation.Statics.Add(new StaticDescription(Vector3Utils.toNumeric(new Vector3(pos.X + 9490f, pos.Y - 700f, pos.Z - 9300)), new CollidableDescription(Simulation.Shapes.Add(new Box(600f * 2, (pos.Y + 2) * 2, 1000f * 2)), 0.1f)));
            Simulation.Statics.Add(new StaticDescription(Vector3Utils.toNumeric(new Vector3(pos.X + lastPosition + 500f + 300f+ 300f, pos.Y - 700f, pos.Z - 9300)), new CollidableDescription(Simulation.Shapes.Add(new Box(500f * 2, (pos.Y + 10f) * 2, 500 * 2)), 0.1f)));

        }

        private float generateIndividualPlatforms(Vector3 pos)
        {
            float posX = 9490f;
            float posAcum = posX;
            float displacement = 1000f;
            for (int i = 0; i <= 10; i++)
            {
                MatrixWorld.Add(Matrix.CreateScale(300f, 10f, 300f) * Matrix.CreateRotationZ(-MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(pos.X + posX + displacement, pos.Y - 700f, pos.Z - 9300)));
                Simulation.Statics.Add(new StaticDescription(Vector3Utils.toNumeric(new Vector3(pos.X + posX + displacement, pos.Y - 700f, pos.Z - 9300)), new CollidableDescription(Simulation.Shapes.Add(new Box(300f * 2, (pos.Y + 10f) * 2, 300f * 2)), 0.1f)));
                displacement += 1000f;
                posAcum += 1000f;
            }
            return posAcum;
        }
        private void createObstaculoMovil()
        {       
           IRecorrido recorrido = new Vaiven(600f, 0.5f, 2);
           obstMovil = new ObstaculoMovil(Cube, MatrixWorldObs[0], recorrido, Simulation, SphereHandles,this);
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

          //var bodyDescription= BodyDescription.CreateDynamic(position, new BodyInertia { InverseMass =radius * radius * radius }, new CollidableDescription(Simulation.Shapes.Add(new Sphere(5f)), 0.1f, ContinuousDetectionSettings.Continuous(1e-3f, 1e-2f)), new BodyActivityDescription(0.01f));
            var bodyDescription = BodyDescription.CreateConvexDynamic(position, 1 / radius * radius * radius,
               Simulation.Shapes, sphereShape);

            bodyDescription.Collidable.Continuity.Mode = ContinuousDetectionMode.Continuous;

            var bodyHandle = Simulation.Bodies.Add(bodyDescription);

            SphereHandles.Add(bodyHandle);

            Radii.Add(radius);

        }

        private void EnableDefaultLighting(Model model)
        {
            foreach (var mesh in model.Meshes)
                ((BasicEffect)mesh.Effects.FirstOrDefault())?.EnableDefaultLighting();
        }
        float time;
        protected override void Draw(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logia de renderizado del juego.
            GraphicsDevice.Clear(Color.Black);
           

            if (status == ST_PRESENTACION)
            {
                DrawCenterText("SOMOS LA BOLA", 3);
                DrawCenterTextY("Presione SPACE para jugar", GraphicsDevice.Viewport.Height * 7/10, 1);
                DrawCenterTextY("Presione C para ver controles", GraphicsDevice.Viewport.Height * 8/10, 1);
                return;
            }
            else if (status == ST_CONTROLES)
            {
                DrawCenterTextY("Las flechas del teclado se usan para moverse", GraphicsDevice.Viewport.Height * 1/12, 1);
                DrawCenterTextY("SPACE para saltar", GraphicsDevice.Viewport.Height * 3/12, 1);
                DrawCenterTextY("R para reiniciar", GraphicsDevice.Viewport.Height * 5/12, 1);
                DrawCenterTextY("M para cambiar el material de la bola en el proximo reinicio", GraphicsDevice.Viewport.Height * 7/12, 1);
                DrawCenterTextY("ESC para salir del juego", GraphicsDevice.Viewport.Height * 9/12, 1);
                DrawCenterTextY("P para volver a la presentacion", GraphicsDevice.Viewport.Height * 11/12, 1);
                return;
            }

            //Box.Draw(FloorWorld, Camera.View, Camera.Projection);
            float tiempoTranscurrido = (float)gameTime.TotalGameTime.TotalSeconds;

            var playerTexture = Material switch
                {
                    M_Goma => GomaTexture,
                    M_Metal => MetalTexture,
                    M_Madera => MaderaTexture,
                    _ => MaderaTexture
                };

            switch(Material){
                case M_Metal :
                    Efecto.Parameters["ModelTexture"]?.SetValue(playerTexture);
                    Efecto.Parameters["reflectionLevel"]?.SetValue(0.2f);

                    Efecto.Parameters["KAmbient"]?.SetValue(KAmbientMetal);
                    Efecto.Parameters["KDiffuse"]?.SetValue(KDiffuseMetal);
                    Efecto.Parameters["KSpecular"]?.SetValue(KSpecularMetal);

                    Efecto.Parameters["shininess"]?.SetValue(1f);

                    Efecto.Parameters["ambientColor"]?.SetValue(Color.LightGray.ToVector3());
                    Efecto.Parameters["diffuseColor"]?.SetValue(Color.Gray.ToVector3());
                    Efecto.Parameters["specularColor"]?.SetValue(Color.White.ToVector3());

                    DrawEnvironmentMap();
                    break;

                case M_Goma :
                    Efecto.CurrentTechnique = Efecto.Techniques["BasicColorDrawing"];

                    Efecto.Parameters["ModelTexture"]?.SetValue(playerTexture);

                    Efecto.Parameters["KAmbient"]?.SetValue(KAmbientGoma);
                    Efecto.Parameters["KDiffuse"]?.SetValue(KDiffuseGoma);
                    Efecto.Parameters["KSpecular"]?.SetValue(KSpecularGoma);

                    Efecto.Parameters["shininess"]?.SetValue(1f);

                    Efecto.Parameters["ambientColor"]?.SetValue(Color.MonoGameOrange.ToVector3());
                    Efecto.Parameters["diffuseColor"]?.SetValue(Color.MonoGameOrange.ToVector3());
                    Efecto.Parameters["specularColor"]?.SetValue(Color.White.ToVector3());

                    SpheresWorld.ForEach(sphereWorld => {
                        var mesh = Sphere.Meshes.FirstOrDefault();
                        if (mesh != null)
                        {
                            foreach (var part in mesh.MeshParts)
                            {
                                part.Effect = Efecto;
                                var worldM = mesh.ParentBone.Transform * sphereWorld;
                                Efecto.Parameters["World"]?.SetValue(mesh.ParentBone.Transform * sphereWorld);
                                var WorldViewProjection = worldM * Camera.View * Camera.Projection;
                                Efecto.Parameters["WorldViewProjection"]?.SetValue(WorldViewProjection);
                                Efecto.Parameters["InverseTransposeWorld"]
                                    ?.SetValue(Matrix.Invert(Matrix.Transpose(worldM)));
                            }

                            mesh.Draw();
                        }
                    });


                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.None };
                    createStage(Camera);
                    time += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    obstMovil.Draw(time, Camera.View, Camera.Projection);
                    
                    break;

                default :  
                    Efecto.Parameters["ModelTexture"]?.SetValue(playerTexture);
                    Efecto.Parameters["reflectionLevel"]?.SetValue(0.06f);

                    Efecto.Parameters["KAmbient"]?.SetValue(KAmbientMadera);
                    Efecto.Parameters["KDiffuse"]?.SetValue(KDiffuseMadera);
                    Efecto.Parameters["KSpecular"]?.SetValue(KSpecularMadera);

                    Efecto.Parameters["shininess"]?.SetValue(1f);

                    Efecto.Parameters["ambientColor"]?.SetValue(Color.Brown.ToVector3());
                    Efecto.Parameters["diffuseColor"]?.SetValue(Color.Brown.ToVector3());
                    Efecto.Parameters["specularColor"]?.SetValue(Color.White.ToVector3());

                    DrawEnvironmentMap();
                    break;
                    
            }
            
            GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.None };
            Vector3 roundPosition = new Vector3(MathF.Round(PositionE.X,2), MathF.Round(PositionE.Y,2), MathF.Round(PositionE.Z,2));
            Skybox.Draw(Camera.View, Camera.Projection, Camera.Position);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque,
                SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            SpriteBatch.DrawString(SpriteFont, roundPosition.ToString(), new Vector2(GraphicsDevice.Viewport.Width /2, 0), Color.White);
            SpriteBatch.DrawString(SpriteFont, "\"R\" para REINICIAR", new Vector2(GraphicsDevice.Viewport.Width / 30, 0), Color.White);
            var sphereBody = Simulation.Bodies.GetBodyReference(SphereHandles[0]);
            var stringSalto = "SALTO";

            if (puedoSaltar) 
                SpriteBatch.DrawString(SpriteFont, stringSalto,new Vector2(GraphicsDevice.Viewport.Width / 30, GraphicsDevice.Viewport.Height / 10),
                 Color.CornflowerBlue);
            else 
                SpriteBatch.DrawString(SpriteFont, stringSalto, new Vector2(GraphicsDevice.Viewport.Width / 30, GraphicsDevice.Viewport.Height / 10),
                 Color.DarkGray);

            string stringMaterial = ProxMaterial switch
            {
                M_Goma => "PROXIMO MATERIAL: GOMA",
                M_Metal => "PROXIMO MATERIAL: METAL",
                _ => "PROXIMO MATERIAL: MADERA"
            };

            SpriteBatch.DrawString(SpriteFont, stringMaterial, new Vector2(GraphicsDevice.Viewport.Width / 30, GraphicsDevice.Viewport.Height / 5), Color.White);

            SpriteBatch.End();

            checkpoints.ForEach(x => DrawTrigger(x, tiempoTranscurrido));
            powerUps.ForEach(x => DrawTrigger(x, tiempoTranscurrido));

        }

         private void DrawEnvironmentMap()
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.None };

            for (var face = CubeMapFace.PositiveX; face <= CubeMapFace.NegativeZ; face++)
            {
                GraphicsDevice.SetRenderTarget(EnvironmentMapRenderTarget, face);
                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

                SetCubemapCameraForOrientation(face);
                CubeMapCamera.BuildView();

                createStage(CubeMapCamera);
            }

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            createStage(Camera);

            Efecto.CurrentTechnique = Efecto.Techniques["EnvironmentMapSphere"];
            Efecto.Parameters["environmentMap"]?.SetValue(EnvironmentMapRenderTarget);

            SpheresWorld.ForEach(sphereWorld => {
                var mesh = Sphere.Meshes.FirstOrDefault();
                if (mesh != null)
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = Efecto;
                    var worldM = mesh.ParentBone.Transform * sphereWorld;
                    Efecto.Parameters["World"]?.SetValue(worldM);
                    var WorldViewProjection = worldM * Camera.View * Camera.Projection;
                    Efecto.Parameters["WorldViewProjection"]?.SetValue(WorldViewProjection);
                    Efecto.Parameters["InverseTransposeWorld"]?.SetValue(Matrix.Invert(Matrix.Transpose(worldM)));
                }
                mesh.Draw();
            });

            //DebugTextureEffect.Parameters["World"].SetValue(QuadWorld);
            //DebugTextureEffect.Parameters["cubeMapTexture"]?.SetValue(EnvironmentMapRenderTarget);
            FullScreenQuad.Draw(DebugTextureEffect);
        }


         private void DrawTrigger(Trigger trigger, float tiempoTranscurrido)
         {
             GraphicsDevice.BlendState = BlendState.AlphaBlend;

             var worldMatrix = Matrix.CreateRotationY(tiempoTranscurrido % (MathF.PI * 2)) *
                               Matrix.CreateTranslation(trigger.BoundingSphere.Center);

             var mesh = Sphere.Meshes.FirstOrDefault();



             if (mesh != null)
             {
                 var meshScale = Vector3.Zero;
                 mesh.ParentBone.Transform.Decompose(out meshScale, out _, out _);

                 var scale = 1 / (meshScale.X / trigger.BoundingSphere.Radius);

                 EfectoBasico.CurrentTechnique = EfectoBasico.Techniques["SetColorDrawing"];


                 var color = trigger.Color().ToVector4();
                 color.W = 0.2f;

                 EfectoBasico.Parameters["Color"]?.SetValue(color);
                 EfectoBasico.Parameters["ambientColor"]?.SetValue(Vector3.One);

                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = EfectoBasico;
                    var worldM = mesh.ParentBone.Transform * Matrix.CreateScale(scale)  * worldMatrix;
                    EfectoBasico.Parameters["World"]?.SetValue(worldM);
                    var worldViewProjection = worldM * Camera.View * Camera.Projection;
                    EfectoBasico.Parameters["WorldViewProjection"]?.SetValue(worldViewProjection);
                    EfectoBasico.Parameters["InverseTransposeWorld"]
                        ?.SetValue(Matrix.Invert(Matrix.Transpose(worldM)));
                }

                mesh.Draw();
             }

             /*
            var a = new SpherePrimitive(GraphicsDevice,trigger.BoundingSphere.Radius * 2);
            a.Effect = Efecto;
            a.Effect.Alpha = 0.2f;


            a.Draw(worldMatrix, Camera.View, Camera.Projection);
             */
         }

        private void SetCubemapCameraForOrientation(CubeMapFace face)
        {
            switch (face)
            {
                default:
                case CubeMapFace.PositiveX:
                    CubeMapCamera.FrontDirection = -Vector3.UnitX;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.NegativeX:
                    CubeMapCamera.FrontDirection = Vector3.UnitX;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.PositiveY:
                    CubeMapCamera.FrontDirection = Vector3.Down;
                    CubeMapCamera.UpDirection = Vector3.UnitZ;
                    break;

                case CubeMapFace.NegativeY:
                    CubeMapCamera.FrontDirection = Vector3.Up;
                    CubeMapCamera.UpDirection = -Vector3.UnitZ;
                    break;

                case CubeMapFace.PositiveZ:
                    CubeMapCamera.FrontDirection = -Vector3.UnitZ;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.NegativeZ:
                    CubeMapCamera.FrontDirection = Vector3.UnitZ;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;
            }
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            if (status == ST_PRESENTACION)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    status = ST_STAGE;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.C))
                {
                    status = ST_CONTROLES;
                }
                return;
            }
            else if (status == ST_CONTROLES)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.P))
                {
                    status = ST_PRESENTACION;
                }
                return;
            }

            var currentKeyPressed = Keyboard.GetState().IsKeyDown(Keys.M);

            if (!currentKeyPressed && MPresionada)
            {
                switch (ProxMaterial)
                {
                    case M_Goma:
                        ProxMaterial = M_Metal;
                        break;

                    case M_Metal:
                        ProxMaterial = M_Madera;
                        break;

                    default:
                        ProxMaterial = M_Goma;
                        break;
                }
            }

            Player.Update(gameTime);

            MPresionada = currentKeyPressed;

            // Aca deberiamos poner toda la logica de actualizacion del juego.
            var deltaTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
          
            SphereWorld = Matrix.CreateTranslation(SpherePosition) * Matrix.CreateScale(0.3f);
            Vector3 SpherePositionM = new Vector3
                                                            (Simulation.Bodies.GetBodyReference(SphereHandles[0]).Pose.Position.X,
                                                            Simulation.Bodies.GetBodyReference(SphereHandles[0]).Pose.Position.Y,
                                                            Simulation.Bodies.GetBodyReference(SphereHandles[0]).Pose.Position.Z);

            // Camera.Position = new Vector3(SpherePositionM.X, SpherePositionM.Y, SpherePositionM.Z - 500);

           

            Camera.Update(gameTime, SpherePositionM);
            CubeMapCamera.Position = SpherePositionM;

            Timer += (float) gameTime.ElapsedGameTime.TotalSeconds;

            var lightPosition = new Vector3((float) Math.Cos(Timer) * 700f, 800f, (float) Math.Sin(Timer) * 700f);
            Efecto.Parameters["eyePosition"]?.SetValue(Camera.Position);
            Efecto.Parameters["lightPosition"]?.SetValue(lightPosition);

            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                //Salgo del juego.
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                status = ST_PRESENTACION;
            }

            if (MediaPlayer.State == MediaState.Stopped)
            {
                MediaPlayer.Play(Song);
            }
            UpdatePhysics();

            //var playerBounding = Sphere.Meshes.First().BoundingSphere.Transform(SphereWorld);

            var playerBounding = new BoundingSphere(SpherePositionM, 5);

            var acquiredPowerUp = powerUps.Find(powerUp => powerUp.BoundingSphere.Intersects(playerBounding));

            if (acquiredPowerUp != null)
            {
                powerUps.Remove(acquiredPowerUp);
                acquiredPowerUp.AplicarEfecto();
            }

            var acquiredCheckpoint = checkpoints.Find(checkpoint => checkpoint.BoundingSphere.Intersects(playerBounding));

            if (acquiredCheckpoint != null)
            {
                checkpoints.Remove(acquiredCheckpoint);
                acquiredCheckpoint.AplicarEfecto();
            }


            base.Update(gameTime);
        }

        private void UpdatePhysics()
        {
            //Physics
            Simulation.Timestep(1/ 60f, ThreadDispatcher);
            SpheresWorld.Clear();
           
            var sphereBody = Simulation.Bodies.GetBodyReference(SphereHandles[0]);
            var playerAceleration = 10;
            //var plataforma = Simulation.Statics.GetStaticReference(StaticHandle[0]);

            //var spheresHandleCount = SphereHandles.Count;

            var VectorMovimientoX = new NumericVector3(5, 0, 0);
            var VectorMovimientoZ = new NumericVector3(0, 0, 5);

            if (Material == M_Metal)
            {
                VectorMovimientoX += new NumericVector3(5, 0, 0);
                VectorMovimientoZ += new NumericVector3(0, 0, 5);
            }

            var cameraFront = Camera.FrontDirection;
            Vector3 forwardDirectionMovement = new Vector3(cameraFront.X, 0, cameraFront.Z);
            forwardDirectionMovement.Normalize();

            Vector3 rightDirectionMovement = Vector3.Cross(forwardDirectionMovement, Vector3.Up);
            rightDirectionMovement.Normalize();

            var forward = Vector3Utils.toNumeric(forwardDirectionMovement);
            var backward = Vector3Utils.toNumeric(forwardDirectionMovement * -1);
            var left = Vector3Utils.toNumeric(rightDirectionMovement * -1);
            var right = Vector3Utils.toNumeric(rightDirectionMovement);

            var materialSpeedBoost = Material switch
            {
                M_Goma => 0.9f,
                M_Metal => 1.3f,
                _ => 1
            };

            var materialJumpBoost = Material switch
            {
                M_Goma => 1.75f,
                M_Metal => 0.9f,
                _ => 1
            };

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                sphereBody.Awake = true;
                sphereBody.Velocity.Linear = sphereBody.Velocity.Linear + forward * playerAceleration * materialSpeedBoost;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                sphereBody.Awake = true;
                sphereBody.Velocity.Linear = sphereBody.Velocity.Linear + backward * playerAceleration * materialSpeedBoost;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                sphereBody.Awake = true;
                sphereBody.Velocity.Linear = sphereBody.Velocity.Linear + left * playerAceleration * materialSpeedBoost;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                sphereBody.Awake = true;
                sphereBody.Velocity.Linear = sphereBody.Velocity.Linear + right * playerAceleration * materialSpeedBoost;
            }

            if(MathHelper.Distance(sphereBody.Velocity.Linear.Y, velocidadLinearYAnt) < 0.5 
                && MathHelper.Distance(sphereBody.Velocity.Angular.Y, velocidadAngularYAnt) < 0.5) puedoSaltar = true;

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                //sphereBody.Velocity.Linear = sphereBody.Velocity.Linear + new NumericVector3(0, 100, 0);
                if (puedoSaltar)
                {
                    var jumpImpulseForce = 1000;
                    sphereBody.Awake = true;
                    sphereBody.ApplyLinearImpulse(NumericVector3Utils.Up * jumpImpulseForce * materialJumpBoost);
                    puedoSaltar = false;

                }
            }

            velocidadAngularYAnt = sphereBody.Velocity.Angular.Y;
            velocidadLinearYAnt = sphereBody.Velocity.Linear.Y;

            var maxSpeed = 150f;

            var horizontalVelocity = sphereBody.Velocity.Linear;
            horizontalVelocity.Y = 0;

            var speed = MathHelper.Min(horizontalVelocity.Length(), maxSpeed);

            horizontalVelocity = horizontalVelocity != NumericVector3.Zero ? 
                NumericVector3.Normalize(horizontalVelocity) * speed : NumericVector3.Zero;

            var finalVelocity = horizontalVelocity;

            var yVelocity = sphereBody.Velocity.Linear.Y;

            finalVelocity.Y = MathHelper.Max(yVelocity, -Player.PlayerStatus.MaxFallingSpeed);

            sphereBody.Velocity.Linear = finalVelocity;

            if (Keyboard.GetState().IsKeyDown(Keys.R) || PositionE.Y < -2000)
            {
                powerUps = new List<Trigger>(initialPowerUps);
                Player.resetStatus();
                sphereBody.Awake = true;
                sphereBody.Pose.Position = Vector3Utils.toNumeric(Checkpoint.CurrentCheckpoint);
                sphereBody.Velocity.Linear = NumericVector3.Zero;
                sphereBody.Velocity.Angular = NumericVector3.Zero;
                puedoSaltar = true;
                Material = ProxMaterial;
                if(Material == M_Goma) Efecto = EfectoBasico;
                else Efecto = EfectoEM;
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

        public void DrawCenterText(string msg, float escala)
        {
            var W = GraphicsDevice.Viewport.Width;
            var H = GraphicsDevice.Viewport.Height;
            var size = SpriteFont.MeasureString(msg) * escala;
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation((W - size.X) / 2, (H - size.Y) / 2, 0));
            SpriteBatch.DrawString(SpriteFont, msg, new Vector2(0, 0), Color.CornflowerBlue);
            SpriteBatch.End();
        }

        public void DrawCenterTextY(string msg, float Y, float escala)
        {
            var W = GraphicsDevice.Viewport.Width;
            var H = GraphicsDevice.Viewport.Height;
            var size = SpriteFont.MeasureString(msg) * escala;
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation((W - size.X) / 2, Y, 0));
            SpriteBatch.DrawString(SpriteFont, msg, new Vector2(0, 0), Color.White);
            SpriteBatch.End();
        }


        private void createStage(Camera Camera)
        {
            for (int i = 0; i < MatrixWorld.Count(); i++)
            {
                Floor.Draw(MatrixWorld[i], Camera.View, Camera.Projection);
            }
            
            Skybox.Draw(Camera.View, Camera.Projection, Camera.Position);
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