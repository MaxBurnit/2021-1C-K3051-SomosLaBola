﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using SomosLaBola.Cameras;
using SomosLaBola.Geometries;
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
        private Camera Camera { get; set; }
        private GraphicsDeviceManager Graphics { get; }
        private SpriteBatch SpriteBatch { get; set; }
        private Model Model { get; set; }
        private Effect Efecto { get; set; }
        private float Rotation { get; set; }
        private Matrix World { get; set; }
        private Matrix View { get; set; }
        private Matrix Projection { get; set; }
        private SpherePrimitive Sphere { get; set; }
        private Vector3 SpherePosition { get; set; }
        private TorusPrimitive Torus { get; set; }
        private Vector3 TorusPosition { get; set; }
        private CylinderPrimitive Cylinder { get; set; }
        private TeapotPrimitive Teapot { get; set; }
        private Vector3 TeapotPosition { get; set; }

        private CubePrimitive Box { get; set; }
        private Vector3 BoxPosition { get; set; }

        private Vector3 CameraPosition { get; set; }

        private Vector3 CameraUpPosition { get; set; }

        private float Time = 0;

        private Model Cube { get; set; }

        private Model Esfera { get; set; }

        private ObstaculoMovil ObstaculoEsfera;

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
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
            CameraPosition = new Vector3(15, 15, 9);
            CameraUpPosition = new Vector3(-5, -5, 50 / 3f);
            CameraUpPosition.Normalize();
            // Camera = new SimpleCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.UnitZ * 55, 15, 0.5f);

            Sphere = new SpherePrimitive(GraphicsDevice, 10);
            //SpherePosition = new Vector3(0, 0, 0);

            Torus = new TorusPrimitive(GraphicsDevice, 20, 1);
            TorusPosition = new Vector3(0, -2, -10);

            Time = 0;

            Cylinder = new CylinderPrimitive(GraphicsDevice, 20, 5);

            Teapot = new TeapotPrimitive(GraphicsDevice, 10);
            TeapotPosition = new Vector3(-15, 10, 0);


            // Configuramos nuestras matrices de la escena.
            SpherePosition = Vector3.Zero;
            World = Matrix.Identity;
            View = Matrix.CreateLookAt(CameraPosition, SpherePosition, CameraUpPosition);
            Projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 250);

            base.Initialize();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        ///     que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void LoadContent()
        {


            Cube = Content.Load<Model>(ContentFolder3D + "geometries/cube");
            ((BasicEffect)Cube.Meshes.FirstOrDefault()?.Effects.FirstOrDefault())?.EnableDefaultLighting();

            Esfera = Content.Load<Model>(ContentFolder3D + "geometries/sphere");
            // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Cargo el modelo del logo.
            //Model = Content.Load<Model>(ContentFolder3D + "tgc-logo/tgc-logo");

            // Cargo un efecto basico propio declarado en el Content pipeline.
            // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
            Efecto = Content.Load<Effect>(ContentFolderEffects + "BasicShader");

            // Asigno el efecto que cargue a cada parte del mesh.
            // Un modelo puede tener mas de 1 mesh internamente.
            foreach (var mesh in Esfera.Meshes)
                // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
                foreach (var meshPart in mesh.MeshParts)
                    meshPart.Effect = Efecto;


            ObstaculoEsfera = ObstaculoMovil.CrearObstaculoRecorridoVaiven(Cube, Matrix.Identity);

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

            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                //Salgo del juego.
                Exit();

            //Camera.Update(gameTime);


            // Basado en el tiempo que paso se va generando una rotacion.
            //Rotation += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            base.Update(gameTime);
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {

            float tiempoTranscurrido = (float) gameTime.TotalGameTime.TotalSeconds;

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

            Time += 0.5f;
            var rotationMatrix = Matrix.CreateRotationZ(Time * 0.01f);
            DrawGeometry(Sphere, SpherePosition, 0, 0, 0);
            DrawGeometry(Torus, TorusPosition, 0, MathHelper.Pi / 2, 0);
            DrawGeometry(Torus, TorusPosition, 0, MathHelper.Pi / 2, 0);
            DrawGeometry(Cylinder, new Vector3(10, 5, -5), 0, MathHelper.Pi / 2, (float)gameTime.TotalGameTime.TotalSeconds);
            DrawGeometry(Cylinder, new Vector3(10, 5, -5), (float)gameTime.TotalGameTime.TotalSeconds, 0, 0);

            DrawGeometry(Teapot, TeapotPosition, yaw: MathHelper.Pi * -0.5f, pitch: MathHelper.Pi * -0.25f);
            var escalaPiso = 10f;
            Cube.Draw(Matrix.CreateRotationZ(MathHelper.Pi * 0.25f) * Matrix.CreateScale(escalaPiso) * Matrix.CreateTranslation(new Vector3(-7f, -7f, escalaPiso * -1f - 2f)), View,
            Projection);
            Cube.Draw(rotationMatrix * Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(new Vector3(1, -1, 3)), View,
            Projection);
            Cube.Draw(rotationMatrix * Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(new Vector3(-1, 2, 3)), View,
            Projection);
            Cube.Draw(Matrix.CreateRotationZ(MathHelper.Pi * 0.25f) * Matrix.CreateScale(escalaPiso) * Matrix.CreateTranslation(-25f, -25f, -escalaPiso), View,
            Projection);
            Efecto.Parameters["View"].SetValue(View);
            Efecto.Parameters["Projection"].SetValue(Projection);
            Color rojo = Color.Red;
            Vector3 color = rojo.ToVector3();
            //Efecto.Parameters["DiffuseColor"].SetValue(color);


            foreach (var mesh in Esfera.Meshes)
            {
                World = mesh.ParentBone.Transform * Matrix.CreateTranslation(SpherePosition) * rotationMatrix * Matrix.CreateScale(0.008f);
                Efecto.Parameters["World"].SetValue(World);
                mesh.Draw();
            }
            SpherePosition = new Vector3(SpherePosition.X - 1, SpherePosition.Y - 1, SpherePosition.Z);


            ObstaculoEsfera.Draw(tiempoTranscurrido, View, Projection);

        }

        private void DrawGeometry(GeometricPrimitive geometry, Vector3 position, float yaw = 0f, float pitch = 0f, float roll = 0f)
        {
            var effect = geometry.Effect;

            effect.World = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll) * Matrix.CreateTranslation(position) * Matrix.CreateScale(0.3f);
            effect.View = View;
            effect.Projection = Projection;

            geometry.Draw(effect);
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