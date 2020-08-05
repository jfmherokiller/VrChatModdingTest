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
}