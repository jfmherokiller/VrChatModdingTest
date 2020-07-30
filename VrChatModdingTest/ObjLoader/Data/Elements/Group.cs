using System.Collections.Generic;
using ObjLoader.Loader.Data.DataStore;

namespace ObjLoader.Loader.Data.Elements
{
    public class Group : IMeshGroup
    {
        private SubMesh _current = null;
        private readonly List<IFaceGroup> _submesh = new List<IFaceGroup>();
        public List<VertexData.Vertex> Verticies = new List<VertexData.Vertex>();
        
        public Group(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
        public IList<IFaceGroup> Meshes { get { return _submesh; } }
        public IFaceGroup Mesh { get { return _current; } }

        public void AddFace(Face face)
        {
            PushMeshIfNeeded();
            _current.AddFace(face);
        }

        public void Set_Material(Material mat)
        {
            // Make sure we have a mesh at all before starting.
            PushMeshIfNeeded();
            if (_current.Material != null)// We are changing materials after we already have one, so that means we need to start a new submesh
                PushMesh();

            _current.Material = mat;
        }

        protected void PushMesh()
        {
            _current = new SubMesh();
            _submesh.Add(_current);
        }

        protected void PushMeshIfNeeded()
        {
            if (_current == null)
            {
                PushMesh();
            }
        }
    }
}