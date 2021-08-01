using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Handlers
{
    public class Juggler : MonoBehaviour
    {
        public static Juggler Instance;

        private static HashSet<IUpdateHandler> _updatables;
        private static HashSet<IFixedUpdateHandler> _fixedUpdatables;

        private bool _fixedUpdateIsActive;

        public void Awake()
        {
            Instance = this;
            _updatables = new HashSet<IUpdateHandler>();
            _fixedUpdatables = new HashSet<IFixedUpdateHandler>();
        }

        private void Update()
        {
            var updatablesTemp = _updatables.ToArray();
            foreach (var updatable in updatablesTemp)
                updatable.OnUpdate();

            _fixedUpdateIsActive = false;
        }

        private void FixedUpdate()
        {
            if (_fixedUpdateIsActive)
                return;

            _fixedUpdateIsActive = true;

            var updatablesTemp = _fixedUpdatables.ToArray();
            foreach (var updatable in updatablesTemp)
                updatable.OnFixedUpdate();
        }

        public static void AddUpdateHandler(IUpdateHandler handler)
        {
            _updatables.Add(handler);
        }

        public static void RemoveUpdateHandler(IUpdateHandler handler)
        {
            _updatables.Remove(handler);
        }

        public static void AddFixedUpdateHandler(IFixedUpdateHandler handler)
        {
            _fixedUpdatables.Add(handler);
        }

        public static void RemoveFixedUpdateHandler(IFixedUpdateHandler handler)
        {
            _fixedUpdatables.Remove(handler);
        }
    }
}
