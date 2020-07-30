using System.Collections.Generic;
using ObjLoader.Loader.Data.DataStore;

namespace ObjLoader.Loader.Data.Elements
{
    public class SubMesh : IFaceGroup
    {
        private readonly List<Face> _faces = new List<Face>();

        public SubMesh() { }
        
        public Material Material { get; set; }

        public List<Face> Faces { get { return _faces; } }

        public void AddFace(Face face)
        {
            _faces.Add(face);
        }
    }
}