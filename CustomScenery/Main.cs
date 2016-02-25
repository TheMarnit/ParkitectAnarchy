using UnityEngine;

namespace Custom_Scenery
{
    public class Main : IMod
    {
        private GameObject _go;

        public void onEnabled()
        {
            _go = new GameObject();
        }

        public void onDisabled()
        {
            Object.Destroy(_go);
        }

        public string Name { get { return "Construction Anarchy"; } }
        public string Description { get { return "Lifts building restrictions for asset packs by Marnit."; } }
        public string Path { get; set; }
        public string Identifier { get; set; }
    }
}
