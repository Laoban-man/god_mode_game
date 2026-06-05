using System.Collections.Generic;
using UnityEngine;
using DivineDrift.Data;

namespace DivineDrift.Config
{
    /// <summary>
    /// Authoring asset for the light tech tree shared by all populations. Each
    /// population keeps its own TechProgress over this single graph definition.
    /// </summary>
    [CreateAssetMenu(menuName = "DivineDrift/Tech Tree Definition", fileName = "TechTree")]
    public class TechTreeDefinition : ScriptableObject
    {
        public List<TechNode> nodes = new List<TechNode>();

        private Dictionary<string, TechNode> _byId;

        public TechNode GetNode(string id)
        {
            if (_byId == null)
            {
                _byId = new Dictionary<string, TechNode>();
                foreach (var n in nodes) _byId[n.Id] = n;
            }
            return _byId.TryGetValue(id, out var node) ? node : null;
        }

        /// <summary>
        /// Returns nodes currently unlockable for the given progress/era.
        /// TODO: cache and invalidate on unlock for performance with many pops.
        /// </summary>
        public IEnumerable<TechNode> AvailableNodes(TechProgress progress, Era era)
        {
            foreach (var n in nodes)
                if (progress.CanUnlock(n, era))
                    yield return n;
        }
    }
}
