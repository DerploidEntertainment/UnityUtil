using Sirenix.OdinInspector;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace UnityEngine.Inventory {

    [RequireComponent(typeof(Collector))]
    public class AmmoCollector : MonoBehaviour
    {
        [Required]
        public Inventory? Inventory;
        public float Radius = 1f;

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void Awake() => GetComponent<Collector>().Collected.AddListener(collect);

        private void collect(Collector collector, Collectible collectible) {
            // If no Ammo Collectible was found then just return
            AmmoCollectible ac = collectible.GetComponent<AmmoCollectible>();
            if (ac is null)
                return;

            // Try to find a Weapon with a matching name in the Inventory and adjust its ammo
            AmmoTool tool = Inventory.GetComponentsInChildren<AmmoTool>(true)
                                     .SingleOrDefault(t => t.Info.AmmoTypeName == ac.AmmoTypeName);
            if (tool is not null) {
                int leftover = tool.Load((int)collectible.Amount);
                collectible.Collect(collector, leftover);
            }
        }
    }

}
