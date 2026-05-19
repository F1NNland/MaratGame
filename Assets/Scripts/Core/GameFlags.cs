using System.Collections.Generic;

namespace MaratGame.Core
{
    public sealed class GameFlags
    {
        readonly HashSet<string> _flags = new();

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

        public void Clear() => _flags.Clear();
    }
}
