using System.Collections.Generic;
using RoR2;

namespace HappiestMaskRework
{
    internal class HappiestMaskBehavior : CharacterBody.ItemBehavior
    {
        public List<CharacterBody> ghosts = new List<CharacterBody>();

        private void OnDisable()
        {
            for (int i = 0; i < ghosts.Count; i++)
            {
                ghosts[i].healthComponent.health = 0.0f;
            }
        }
    }
}
