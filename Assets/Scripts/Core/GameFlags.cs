using System.Collections.Generic;
using System.Linq;

namespace MaratGame.Core
{
    public sealed class GameFlags
    {
        readonly HashSet<string> _flags = new();
        public int Count => _flags.Count;

        public void SetFlag(string flagId)
        {
            if (string.IsNullOrWhiteSpace(flagId))
                return;

            _flags.Add(flagId);
        }

        public bool HasFlag(string flagId)
        {
            if (string.IsNullOrWhiteSpace(flagId))
                return false;

            return _flags.Contains(flagId);
        }

        public string[] Snapshot()
        {
            if (_flags.Count == 0)
                return System.Array.Empty<string>();

            return _flags.OrderBy(flag => flag).ToArray();
        }

        public void Clear() => _flags.Clear();
    }
}
