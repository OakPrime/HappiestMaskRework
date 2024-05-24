using System.Collections.Generic;
using RoR2;

namespace HappiestMaskRework
{
    internal class HappiestMaskBehavior : CharacterBody.ItemBehavior
    {
        private CharacterBody _ghost;

        private void OnDisable()
        {
            if ((bool) (UnityEngine.Object)_ghost && _ghost.healthComponent != null)
            {
                _ghost.healthComponent.health = 0.0f;
            }
        }

        public bool HasGhost()
        {
            return (bool)(UnityEngine.Object)_ghost;
        }

        public void SetGhost(CharacterBody newGhost)
        {
            _ghost = newGhost;
        }
    }
}
