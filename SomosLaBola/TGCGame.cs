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
using TGC.MonoGame.Samples.Physics.Bepu;
using TGC.MonoGame.Samples.Viewer;
using NumericVector3 = System.Numerics.Vector3;
using BepuPhysics.Collidables;
using SomosLaBola.Content.Textures;
using SomosLaBola.Obstaculos;
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
        
        public const int ST_PRESENTACION = 0;
        public const int ST_STAGE = 1;
        public const int ST_CONTROLES = 2;
       
        public int status = ST_PRESENTACION;

        public const int M_Goma = 0;
        public const int M_Metal = 1;
        public const int M_Madera = 2;

        public int Material = M_Goma;
        public int ProxMaterial = M_Goma;
        private bool MPresionada;

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
        private Effect Efecto { get; set; }
        private Model Sphere { get; set; }
        private Model Cube { get; set; }
        private Vector3 SpherePosition { get; set; }
        public Matrix SphereWorld { get; private set; }
        private CubePrimitive Box { get; set; }
        private Vector3 BoxPosition { get; set; }

        private ObstaculoMovil ObstaculoCubo;

        //Matrix
        private Matrix World { get; set; }
        private Matrix View { get; set; }
        private Matrix Projection { get; set; }
        public Matrix FloorWorld { get; set; }
        
        //Textures
        private Texture2D GreenTexture { get; set; }
        //private Vector3 DesiredLookAt;
        private List<Matrix> MatrixWorld { get; set; }
        private Floor Floor { get; set; }
        private Texture2D SphereTexture { get; set; }

        private Vector3 Position;
        //private Vector3 ForwardDirection;
        public Boolean puedoSaltar = true;
        public float velocidadAngularYAnt;
        public float velocidadLinearYAnt;


        private SkyBox Skybox;
        public float FarPlaneDistance;
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
            InitializeContent();

            base.Initialize();
        }

        private void InitializeContent()
        {
            //Camera
            CameraPosition = Vector3.One * 100f;
            CameraUpPosition = new Vector3(-5, -5, 50 / 3f);
            CameraUpPosition.Normalize();


            //Geometry
            Box = new CubePrimitive(GraphicsDevice);
            BoxPosition = new Vector3(0, -40, 0);
            SpherePosition = Vector3.Zero;

            FarPlaneDistance = 5000;
            // Configuramos nuestras matrices de la escena.
            FloorWorld = Matrix.CreateScale(2000, 0.1f, 2000) * Matrix.CreateTranslation(BoxPosition);
            SphereWorld = Matrix.CreateScale(0.02f);
            World = Matrix.Identity;
            View = Matrix.CreateLookAt(CameraPosition, SpherePosition, CameraUpPosition);
            Projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, FarPlaneDistance*1.5f);

        }

        protected override void LoadContent()
        {

            SpriteFont = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "Arial");
            // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            MatrixWorld = new List<Matrix>();
            generateMatrixWorld();
            Floor = new Floor(this);
            Cube = Content.Load<Model>(ContentFolder3D + "geometries/cube");

            Sphere = Content.Load<Model>(ContentFolder3D + "geometries/sphere");

            GreenTexture = Content.Load<Texture2D>(ContentFolderTextures + "green");

            SphereTexture = Content.Load<Texture2D>(ContentFolderTextures + "stones");
            
            EnableDefaultLighting(Sphere);
   

            // Cargo un efecto basico propio declarado en el Content pipeline.
            // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
            Efecto = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
            var skyBox = Content.Load<Model>("3D/skybox/cube");
            var skyBoxTexture = Content.Load<TextureCube>(ContentFolderTextures + "skyboxes/skybox/skybox");
            var skyBoxEffect = Content.Load<Effect>(ContentFolderEffects + "SkyBox");
            Position = new Vector3(0, -30, 0);
            Skybox = new SkyBox(skyBox, skyBoxTexture, skyBoxEffect, FarPlaneDistance);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Asigno el efecto que cargue a cada parte del mesh.
            // Un modelo puede tener mas de 1 mesh internamente.
            //foreach (var mesh in Cube.Meshes)
            // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
              //foreach (var meshPart in mesh.MeshParts)
              //  meshPart.Effect = Efecto;

            ObstaculoCubo = ObstaculoMovil.CrearObstaculoRecorridoCircular(Sphere, Matrix.CreateScale(0.1f,0.1f,0.1f)* Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(0,13,-40)));
            LoadPhysics();

            base.LoadContent();
        }

        private void generateMatrixWorld()
        {
            float inicio = 10f;
            float decremento = 0f;
            float posZ = 0f;
            for (int i = 0; i < 20; i++)
            {
                //       Cube.Draw(Matrix.CreateRotationZ(MathHelper.Pi * 0.25f) * Matrix.CreateScale(escaladoXY, escaladoXY, escaladoZ) * Matrix.CreateTranslation(new Vector3(inicio + decremento, inicio + decremento, -escaladoZ)), View,
                // Projection);
                posZ = inicio + decremento;
                MatrixWorld.Add(Matrix.CreateScale(100f, 20f, 200f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(0, 0, posZ)));

                if (i > 10 && i <= 19)
                {
                    MatrixWorld.Add(Matrix.CreateScale(100f, 100f, 200f) * Matrix.CreateRotationX(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(0, 12, posZ)));
                }

                decremento -= 40;
            }
            posZ = inicio + decremento;
            MatrixWorld.Add(Matrix.CreateScale(100f, 20f, 500f) * Matrix.CreateRotationX(MathHelper.Pi * (-0.05f)) * Matrix.CreateTranslation(new Vector3(0, 12, posZ - 29f)));
            posZ = inicio + decremento;
            MatrixWorld.Add(Matrix.CreateScale(500f, 20f, 400f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(0, 0, posZ - 79f)));
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

            Simulation.Statics.Add(new StaticDescription(new NumericVector3(9, 28, 200),    
                new CollidableDescription(Simulation.Shapes.Add(new Sphere(2f)),1)));
            //Simulation.Statics.Add(new StaticDescription(new ))
            /*for (int i = 0; i < MatrixWorld.Count(); i++)
            {
                var traslacion = new Vector3(MatrixWorld[i].M41,MatrixWorld[i].M42,MatrixWorld[i].M43);
                var escalado = MatrixWorld[i].
                Simulation.Statics.Add(new StaticDescription(traslacion, new CollidableDescription(Simulation.Shapes.Add(new Box()))))
            }*/

            //Esfera
            var radius = 0.03f;
            var sphereShape = new Sphere(radius);
            var position = new NumericVector3(0, 40.015934f, 0);
            var bodyDescription = BodyDescription.CreateConvexDynamic(position, 1 / radius * radius * radius,
                Simulation.Shapes, sphereShape);

            var bodyHandle = Simulation.Bodies.Add(bodyDescription);

            SphereHandles.Add(bodyHandle);

            Radii.Add(radius);

            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.Forward,
                                                            new Vector3
                                                            (Simulation.Bodies.GetBodyReference(SphereHandles[0]).Pose.Position.X,
                                                            Simulation.Bodies.GetBodyReference(SphereHandles[0]).Pose.Position.Y,
                                                            Simulation.Bodies.GetBodyReference(SphereHandles[0]).Pose.Position.Z));
        }

        private void EnableDefaultLighting(Model model)
        {
            foreach (var mesh in model.Meshes)
                ((BasicEffect)mesh.Effects.FirstOrDefault())?.EnableDefaultLighting();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            switch(status){
                case ST_PRESENTACION : 
                    DrawCenterText("SOMOS LA BOLA", 3);
                    DrawCenterTextY("Presione SPACE para jugar", 300 , 1);
                    DrawCenterTextY("Presione C para ver controles", 350 , 1);
                    break;

                case ST_CONTROLES :
                    DrawCenterTextY("Las flechas del teclado se usan para moverse", 20, 1);
                    DrawCenterTextY("SPACE para saltar", 100, 1);
                    DrawCenterTextY("R para reiniciar", 180, 1);
                    DrawCenterTextY("M para cambiar el material de la bola en el proximo reinicio", 260, 1);
                    DrawCenterTextY("ESC para salir del juego", 340, 1);
                    DrawCenterTextY("P para volver a la presentacion", 420, 1);
                    break;

                default: 
                    // Aca deberiamos poner toda la logia de renderizado del juego.

                    //createStage();

                    Box.Draw(FloorWorld, Camera.View, Camera.Projection);

                    Skybox.Draw(Camera.View, Camera.Projection, Camera.Position);

                    float tiempoTranscurrido = (float)gameTime.TotalGameTime.TotalSeconds;
                    ObstaculoCubo.Draw(tiempoTranscurrido, Camera.View, Projection);

                    Sphere.Draw(SphereWorld,Camera.View,Camera.Projection);

                    /*var mesh = Sphere.Meshes.FirstOrDefault();

                    if (mesh != null)
                    {
                        foreach (var part in mesh.MeshParts)
                    {
                    part.Effect = Efecto;
                    Efecto.Parameters["World"].SetValue(SphereWorld);
                    Efecto.Parameters["View"].SetValue(Camera.View);
                    Efecto.Parameters["Projection"].SetValue(Camera.Projection);
                    Efecto.Parameters["ModelTexture"].SetValue(SphereTexture);
                    }

                    mesh.Draw();
                    }*/
            
                    SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque, 
                    SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
                    SpriteBatch.DrawString(SpriteFont, PositionE.ToString(), new Vector2(GraphicsDevice.Viewport.Width - 400, 0), Color.White);
                    SpriteBatch.DrawString(SpriteFont, "\"R\" para REINICIAR", new Vector2(10, 0), Color.White);
                    var sphereBody = Simulation.Bodies.GetBodyReference(SphereHandles[0]);
                    var stringSalto = "SALTO";
                    if(puedoSaltar) SpriteBatch.DrawString(SpriteFont, stringSalto, 
                    new Vector2(10, 30), Color.CornflowerBlue);
                    else SpriteBatch.DrawString(SpriteFont, stringSalto, new Vector2(10, 30), Color.DarkGray);
                    var StringMaterial = "PROXIMO MATERIAL: GOMA";
                    switch(ProxMaterial){
                        case M_Goma : 
                            StringMaterial = "PROXIMO MATERIAL: GOMA";
                            break;
                        
                        case M_Metal :
                            StringMaterial = "PROXIMO MATERIAL: METAL";
                            break;
                        
                        default :
                            StringMaterial = "PROXIMO MATERIAL: MADERA";
                            break;
                    }
                    SpriteBatch.DrawString(SpriteFont, StringMaterial, new Vector2(10, 60), Color.White);
                    SpriteBatch.End();
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
            // Aca deberiamos poner toda la logica de actualizacion del juego.
            switch(status){
                case ST_PRESENTACION :
                    if(Keyboard.GetState().IsKeyDown(Keys.Space)){
                        status = ST_STAGE;
                    }
                    if(Keyboard.GetState().IsKeyDown(Keys.C)){
                        status = ST_CONTROLES;
                    }
                    break;
                case ST_CONTROLES : 
                    if(Keyboard.GetState().IsKeyDown(Keys.P)){
                        status = ST_PRESENTACION;
                    }
                    break;
                default: 

                    if(Keyboard.GetState().IsKeyDown(Keys.P)){
                        status = ST_PRESENTACION;
                    }

                    var currentKeyPressed = Keyboard.GetState().IsKeyDown(Keys.M);

                    if(!currentKeyPressed && MPresionada){
                        switch(ProxMaterial){
                            case M_Goma: 
                                ProxMaterial = M_Metal;
                                break;
                            
                            case M_Metal :
                                ProxMaterial = M_Madera;
                                break;
                        
                            default :
                                ProxMaterial = M_Goma;
                                break;
                            }
                    }

                    MPresionada = currentKeyPressed;

                    var deltaTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

                    Vector3 SpherePositionM = new Vector3
                                                            (Simulation.Bodies.GetBodyReference(SphereHandles[0]).Pose.Position.X,
                                                            Simulation.Bodies.GetBodyReference(SphereHandles[0]).Pose.Position.Y,
                                                            Simulation.Bodies.GetBodyReference(SphereHandles[0]).Pose.Position.Z);

                    // Camera.Position = new Vector3(SpherePositionM.X, SpherePositionM.Y, SpherePositionM.Z - 500);

                    Camera.Update(gameTime, SpherePositionM);

                    Efecto.Parameters["ModelTexture"].SetValue(SphereTexture);
                    Efecto.Parameters["View"]?.SetValue(Camera.View);
                    Efecto.Parameters["Projection"]?.SetValue(Camera.Projection);
                    Efecto.CurrentTechnique = Efecto.Techniques["BasicColorDrawing"];

                    foreach (var mesh in Sphere.Meshes)
                    {
                    var world = mesh.ParentBone.Transform * SphereWorld;
                    Efecto.Parameters["World"]?.SetValue(world);
                    mesh.Draw();
                    }   

                    UpdatePhysics();
                    break;
            }
            // Capturar Input teclado
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                     //Salgo del juego.
                    Exit();
            base.Update(gameTime);
        }

        private void UpdatePhysics()
        {
            //Physics
            Simulation.Timestep(1 / 60f, ThreadDispatcher);
            var sphereBody = Simulation.Bodies.GetBodyReference(SphereHandles[0]);
           
            //var plataforma = Simulation.Statics.GetStaticReference(StaticHandle[0]);

            //var spheresHandleCount = SphereHandles.Count;

            var VectorMovimientoX = new NumericVector3(5,0,0);
            var VectorMovimientoZ = new NumericVector3(0,0,5);

            if(Material == M_Metal){
                VectorMovimientoX += new NumericVector3(5,0,0);
                VectorMovimientoZ += new NumericVector3(0,0,5);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                sphereBody.Awake = true;
                sphereBody.Velocity.Linear = sphereBody.Velocity.Linear - VectorMovimientoZ;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                sphereBody.Awake = true;
                sphereBody.Velocity.Linear = sphereBody.Velocity.Linear + VectorMovimientoZ;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                sphereBody.Awake = true;
                sphereBody.Velocity.Linear = sphereBody.Velocity.Linear - VectorMovimientoX;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                sphereBody.Awake = true;
                sphereBody.Velocity.Linear = sphereBody.Velocity.Linear + VectorMovimientoX;
            }

            if(sphereBody.Velocity.Linear.Y == velocidadLinearYAnt && sphereBody.Velocity.Angular.Y == velocidadAngularYAnt) puedoSaltar = true; 

            if (Keyboard.GetState().IsKeyDown(Keys.Space) && puedoSaltar)
            {
                //sphereBody.Velocity.Linear = sphereBody.Velocity.Linear + new NumericVector3(0, 100, 0);
                var VectorSalto = new NumericVector3(0,10,0);
                if(Material == M_Goma) VectorSalto += new NumericVector3(0,10,0);
                sphereBody.Awake = true;
                sphereBody.ApplyLinearImpulse(VectorSalto);
                puedoSaltar = false;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.R) || PositionE.Y < -500)
            {
                sphereBody.Awake = true;
                sphereBody.Pose.Position = NumericVector3.Zero;
                sphereBody.Velocity.Linear = NumericVector3.Zero;
                sphereBody.Velocity.Angular = NumericVector3.Zero;
                Material = ProxMaterial;
            }

            velocidadAngularYAnt = sphereBody.Velocity.Angular.Y;
            velocidadLinearYAnt = sphereBody.Velocity.Linear.Y;

            var pose = sphereBody.Pose;
            var position = pose.Position;
            var quaternion = pose.Orientation;
            SphereWorld = Matrix.CreateScale(0.3f) *
                        Matrix.CreateFromQuaternion(new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                            quaternion.W)) *
                        Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z));

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
    }

}