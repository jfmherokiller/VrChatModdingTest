using ObjLoader.Loader.Data;
using ObjLoader.Loader.Data.DataStore;
using ObjLoader.Loader.TypeParsers.Interfaces;

namespace ObjLoader.Loader.TypeParsers
{
    public class ObjectParser : TypeParserBase, IObjectParser
    {
        private readonly IGroupDataStore _groupDataStore;

        public ObjectParser(IGroupDataStore groupDataStore)
        {
            _groupDataStore = groupDataStore;
        }

        protected override string Keyword
        {
            get { return "o"; }
        }

        public override void Parse(string line)
        {
            _groupDataStore.PushGroup(line);
        }
    }
}