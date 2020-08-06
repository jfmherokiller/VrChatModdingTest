using UnityEngine;

namespace PulsarCRepl
{
    public class SpecialTypeFixes
    {
        
    }

    public struct SVector3
    {
        private Vector3 internalVec3;

        public SVector3(float myvecX, float myvecY, float myvecZ)
        {
            internalVec3 = new Vector3(myvecX,myvecY,myvecZ);
        }

        public SVector3(Vector3 mytransPosition)
        {
            internalVec3 = mytransPosition;
        }

        public Vector3? InternalVec3
        {
            set => internalVec3 = (Vector3) value;
            get => null;
        }

        public float Z
        {
            get => internalVec3.z;
            set => internalVec3.z = value;
        }

        public float Y
        {
            get => internalVec3.y;
            set => internalVec3.y = value;
        }

        public float X
        {
            get => internalVec3.x;
            set => internalVec3.x = value;
        }

        public Vector3 GetOrginalVec()
        {
            return internalVec3;
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
        private GameObject InternalGame;
        public SGameObject(GameObject myGameObject)
        {
            InternalGame = myGameObject;
        }

        public string Name
        {
            get => InternalGame.name;
        }

        public STransform Transform
        {
            get => new STransform(InternalGame.transform);
        }
    }
}