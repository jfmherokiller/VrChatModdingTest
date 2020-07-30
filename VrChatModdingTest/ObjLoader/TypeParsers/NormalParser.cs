using ObjLoader.Loader.Common;
using ObjLoader.Loader.Data;
using ObjLoader.Loader.Data.DataStore;
using ObjLoader.Loader.Data.VertexData;
using ObjLoader.Loader.TypeParsers.Interfaces;

namespace ObjLoader.Loader.TypeParsers
{
    public class NormalParser : TypeParserBase, INormalParser
    {
        private readonly INormalDataStore _normalDataStore;

        public NormalParser(INormalDataStore normalDataStore)
        {
            _normalDataStore = normalDataStore;
        }

        protected override string Keyword
        {
            get { return "vn"; }
        }

        public override void Parse(string line)
        {
            var parts = line.Split(' ');

            var x = parts[0].ParseInvariantFloat();
            var y = parts[1].ParseInvariantFloat();
            var z = parts[2].ParseInvariantFloat();

            var normal = new Normal(x, y, z);
            _normalDataStore.AddNormal(normal);
        }
    }
}