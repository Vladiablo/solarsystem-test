using SixLabors.ImageSharp.Processing.Processors.Transforms;
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
            set { this._rotation = value; }
        }

        public Matrix4x4 Projection { get { return _projection; } }

        public Camera(float fovY, float aspectRatio, float near, float far)
        {
            this._fovY = fovY;
            this._fovX = fovY * aspectRatio;
            this._aspectRatio = aspectRatio;
            this._near = near;
            this._far = far;

            this.RecalculateProjection();
        }

        private void RecalculateProjection()
        {
            this._projection = Matrix4x4.CreatePerspective(this._fovX, this._fovY, this._near, this._far);//Math.Math.CreatePerspectiveMatrix(Math.Math.DegToRad(this._fovY), this._fovX, this._fovY, this._near, this._far);
        }

        public Matrix4x4 GetLookAtMatrix(in Vector3 target)
        {
            return Matrix4x4.CreateLookAt(this._position, target, Vector3.UnitZ);
        }

        public Matrix4x4 GetViewMatrix()
        {
            Vector3 radians = Math.Math.DegToRad(this._rotation);
            Matrix4x4 rotation = Matrix4x4.CreateFromYawPitchRoll(radians.Y, radians.X, radians.Z);
            Matrix4x4 translation = Matrix4x4.CreateTranslation(-this._position);
            return rotation * translation;
        }
    }
}
