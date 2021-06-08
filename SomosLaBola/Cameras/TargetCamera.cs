using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SomosLaBola.Cameras
{
    /// <summary>
    ///     Camera looking at a particular point, assumes the up vector is in y.
    /// </summary>
    public class TargetCamera : Camera
    {
        /// <summary>
        ///     The direction that is "up" from the camera's point of view.
        /// </summary>
        public readonly Vector3 DefaultWorldUpVector = Vector3.Up;

        private Vector2 pastMousePosition;
        public float MouseSensitivity { get; set; } = 1f;

        private bool changed;

        private float cameraDistance;

        private float maxPitch { get; set; } = MathHelper.ToRadians(20);
        private float minPitch { get; set; } = MathHelper.ToRadians(-20);
        private float pitch = 0;
        private float yaw = 0;


        /// <summary>
        ///     Camera looking at a particular direction, which has the up vector (0,1,0).
        /// </summary>
        /// <param name="aspectRatio">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="position">The position of the camera.</param>
        /// <param name="targetPosition">The target towards which the camera is pointing.</param>
        public TargetCamera(float aspectRatio, Vector3 FrontDirection, Vector3 targetPosition) : base(aspectRatio)
        {
            this.FrontDirection = FrontDirection;
            cameraDistance = 250;
            var position = targetPosition - FrontDirection * cameraDistance; 
            pastMousePosition = Mouse.GetState().Position.ToVector2();
            BuildView(position, targetPosition);
        }

        /// <summary>
        ///     Camera looking at a particular direction, which has the up vector (0,1,0).
        /// </summary>
        /// <param name="aspectRatio">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="position">The position of the camera.</param>
        /// <param name="targetPosition">The target towards which the camera is pointing.</param>
        /// <param name="nearPlaneDistance">Distance to the near view plane.</param>
        /// <param name="farPlaneDistance">Distance to the far view plane.</param>
        public TargetCamera(float aspectRatio, Vector3 position, Vector3 targetPosition, float nearPlaneDistance,
            float farPlaneDistance) : base(aspectRatio, nearPlaneDistance, farPlaneDistance)
        {
            pastMousePosition = Mouse.GetState().Position.ToVector2();
            BuildView(position, targetPosition);
        }

        /// <summary>
        ///     The target towards which the camera is pointing.
        /// </summary>
        public Vector3 TargetPosition { get; set; }

        /// <summary>
        ///     Build view matrix and update the internal directions.
        /// </summary>
        /// <param name="position">The position of the camera.</param>
        /// <param name="targetPosition">The target towards which the camera is pointing.</param>
        private void BuildView(Vector3 position, Vector3 targetPosition)
        {
            Position = position;
            TargetPosition = targetPosition;
            BuildView();
        }

        /// <summary>
        ///     Build view matrix and update the internal directions.
        /// </summary>
        public void BuildView()
        {
            View = Matrix.CreateLookAt(Position, Position + FrontDirection, UpDirection);
        }

        private void UpdateInternalDirections(Vector3 frontDirection)
        {
            FrontDirection = Vector3.Normalize(frontDirection);
            RightDirection = Vector3.Normalize(Vector3.Cross(DefaultWorldUpVector, FrontDirection));
            UpDirection = Vector3.Cross(FrontDirection, RightDirection);
        }


        public void Update(GameTime gameTime, Vector3 targetPosition)
        {
            var elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            changed = false;

            if (this.TargetPosition != targetPosition)
            {
                this.TargetPosition = targetPosition;
                changed = true;
            }

            ProcessMouseMovement(elapsedTime);
            Position = TargetPosition - FrontDirection * cameraDistance;

            if (changed)
                BuildView();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            
        }

        private void ProcessMouseMovement(float elapsedTime)
        {
            var mouseState = Mouse.GetState();

            if (mouseState.RightButton.Equals(ButtonState.Pressed))
            {
                var mouseDelta = mouseState.Position.ToVector2() - pastMousePosition;
                mouseDelta *= MouseSensitivity * elapsedTime;

                yaw += mouseDelta.X;
                yaw = MathHelper.WrapAngle(yaw);

                pitch += mouseDelta.Y;
                pitch = MathHelper.Clamp(pitch, minPitch, maxPitch);


                //var frontRotation = Matrix.CreateRotationY(yaw) * Matrix.CreateRotationZ(pitch);
                var frontRotation = Matrix.CreateRotationZ(pitch);

                //var frontRotation = Matrix.CreateFromYawPitchRoll(yaw, pitch, 0);

                Vector3 tempFront;
                tempFront.X = MathF.Cos(yaw) * MathF.Cos(pitch);
                tempFront.Y = MathF.Sin(pitch);
                tempFront.Z = MathF.Sin(yaw) * MathF.Cos(pitch);

                if (mouseDelta != Vector2.Zero)
                    UpdateInternalDirections(tempFront);

                changed = true;

            }

            pastMousePosition = Mouse.GetState().Position.ToVector2();
        }

    }
}