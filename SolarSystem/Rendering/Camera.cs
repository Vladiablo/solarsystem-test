using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SolarSystem.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Rendering
{
    internal class Camera
    {
        private float _fovX;
        private float _fovY;
        private float _aspectRatio;
        private float _near;
        private float _far;
        private Matrix4x4 _projection;

        private Vector3 _position;
        private Vector3 _rotation;

        private float _speed;

        public float FovX
        {
            get { return _fovX; }
            set 
            {
                this._fovX = value;
                this._fovY = value / this._aspectRatio;
                this.RecalculateProjection();
            }
        }

        public float FovY
        {
            get { return _fovY; }
            set
            {
                this._fovY = value;
                this._fovX = value * this._aspectRatio;
                this.RecalculateProjection();
            }
        }

        public float AspectRatio
        {
            get { return _aspectRatio; }
            set
            {
                this._aspectRatio = value;
                this._fovX = this._fovY * value;
                this.RecalculateProjection();
            }
        }

        public float Near
        {
            get { return _near; }
            set
            {
                this._near = value;
                this.RecalculateProjection();
            }
        }

        public float Far
        {
            get { return _far; }
            set
            {
                this._far = value;
                this.RecalculateProjection();
            }
        }

        public Vector3 Position
        {
            get { return _position; }
            set { this._position = value; }
        }

        public Vector3 Rotation
        {
            get { return _rotation; }
            set 
            { 
                this._rotation = MathHelper.NormalizeRotation(value);
            }
        }

        public Matrix4x4 Projection { get { return _projection; } }

        public Camera(float fovY, float aspectRatio, float near, float far)
        {
            this._fovY = fovY;
            this._fovX = fovY * aspectRatio;
            this._aspectRatio = aspectRatio;
            this._near = near;
            this._far = far;

            this._speed = 10.0f;

            this._position = new Vector3(0.0f, 0.0f, 0.0f);
            this._rotation = new Vector3(0.0f, 0.0f, 0.0f);

            this.RecalculateProjection();
        }

        private void RecalculateProjection()
        {
            this._projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegToRad(this._fovY), this._aspectRatio, this._near, this._far);
        }

        public Matrix4x4 GetLookAtMatrix(in Vector3 target)
        {
            return Matrix4x4.CreateLookAt(this._position, target, Vector3.UnitZ);
        }

        public Matrix4x4 GetViewMatrix()
        {
            Vector3 radians = MathHelper.DegToRad(this._rotation);

            Matrix4x4 rotation =
                Matrix4x4.CreateRotationX(-MathHelper.PI / 2.0f) *
                Matrix4x4.CreateRotationZ(radians.Y) *
                Matrix4x4.CreateRotationY(radians.Z) *
                Matrix4x4.CreateRotationX(radians.X);

            Matrix4x4 translation = Matrix4x4.CreateTranslation(-this._position);

            return translation * rotation;
        }

        public void Move(in Vector3 movement)
        {
            this._position += movement;
        }

        public void MoveRelative(in Vector3 movement)
        {
            Vector3 radians = MathHelper.DegToRad(this._rotation);

            this._position += Vector3.Transform(movement, Quaternion.CreateFromYawPitchRoll(radians.Y, -radians.X, -radians.Z));
        }

        public void Rotate(in Vector3 rotation)
        {
            this._rotation = MathHelper.NormalizeRotation(this._rotation += rotation);
        }
    }
}
