using UnityEngine;

namespace Shiftbound
{
    public sealed class RooftopWorldArt : MonoBehaviour
    {
        public WorldSwitcher worlds;
        public Renderer[] sharedDecks;
        public Material presentDeck;
        public Material overgrownDeck;
        public GameObject[] overgrownOnly;

        private bool? applied;

        private void Awake() { Apply(); }
        private void LateUpdate() { Apply(); }

        private void Apply()
        {
            if (worlds == null) return;
            bool overgrown = worlds.IsAltered;
            if (applied == overgrown) return;
            applied = overgrown;
            Material deck = overgrown ? overgrownDeck : presentDeck;
            if (deck != null && sharedDecks != null)
                foreach (Renderer item in sharedDecks)
                    if (item != null) item.sharedMaterial = deck;
            if (overgrownOnly != null)
                foreach (GameObject item in overgrownOnly)
                    if (item != null) item.SetActive(overgrown);
        }
    }
}
