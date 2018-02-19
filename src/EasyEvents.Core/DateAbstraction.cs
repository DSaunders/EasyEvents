namespace EasyEvents.Core
{
    using System;

    public static class DateAbstraction
    {
        private static DateTime? _pausedAt;
        public static DateTime UtcNow
        {
            get
            {
                return (_pausedAt.HasValue)
                ? _pausedAt.Value
                : DateTime.UtcNow;
            }
        }

        public static void Pause() {
            _pausedAt = DateTime.UtcNow;
        }

        public static void Resume() {
            _pausedAt = null;
        }
    }
}