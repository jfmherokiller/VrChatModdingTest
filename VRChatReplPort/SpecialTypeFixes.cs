using UnityEngine;

namespace PulsarCRepl
{
    public class SpecialTypeFixes
    {
        
    }

    public struct SVector3
    {
        private float x;
        private float y;
        private float z;

        public SVector3(float myvecX, float myvecY, float myvecZ)
        {
            x = myvecX;
            y = myvecY;
            z = myvecZ;
        }

        public SVector3(Vector3 mytransPosition)
        {
            x = mytransPosition.x;
            y = mytransPosition.y;
            z = mytransPosition.z;
        }

        public float Z
        {
            get => z;
            set => z = value;
        }

        public float Y
        {
            get => y;
            set => y = value;
        }

        public float X
        {
            get => x;
            set => x = value;
        }

        public Vector3 GetOrginalVec()
        {
            return new Vector3(x,y,z);
        }

        public override string ToString()
        {
            return $"{x},{y},{z}";
        }
    }

    public struct STransform
    {
        private SVector3 Position;
        private SQuaderton Rotation;
        
        public STransform(Transform mytrans)
        {
            Position = new SVector3(mytrans.position);
            Rotation = new SQuaderton(mytrans.rotation);
        }
        
        public SVector3 position
        {
            get => Position;
            set => Position = value;
        }
        public SQuaderton rotation
        {
            get => Rotation;
            set => Rotation = value;
        }
    }

    public struct SQuaderton
    {
        private float w;
        private float x;
        private float y;
        private float z;

        public float W
        {
            get => w;
            set => w = value;
        }

        public float X
        {
            get => x;
            set => x = value;
        }

        public float Y
        {
            get => y;
            set => y = value;
        }

        public float Z
        {
            get => z;
            set => z = value;
        }

        public SQuaderton(Quaternion mytransRotation)
        {
            w = mytransRotation.w;
            x = mytransRotation.x;
            y = mytransRotation.y;
            z = mytransRotation.z;
        }
    }

    public struct SGameObject
    {
        private STransform _transform;
        private string _name;
        public SGameObject(GameObject myGameObject)
        {
            _transform = new STransform(myGameObject.transform);
            _name = myGameObject.name;
        }

        public string Name
        {
            get => _name;
        }

        public STransform Transform
        {
            get => _transform;
        }
    }
}