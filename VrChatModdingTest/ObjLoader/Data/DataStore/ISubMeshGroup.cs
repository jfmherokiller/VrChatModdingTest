using ObjLoader.Loader.Data.Elements;

namespace ObjLoader.Loader.Data.DataStore
{
    public interface IMeshGroup
    {
        void AddFace(Face face);
        void Set_Material(Material mat);
    }
}