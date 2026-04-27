using System;
using System.Reflection;
using Verse;

namespace NoNeedAbandonedSettlement
{
    public static class LogCompat
    {
        private static readonly MethodInfo MessageOneArg = typeof(Log).GetMethod("Message", new[] { typeof(string) });
        private static readonly MethodInfo MessageTwoArg = typeof(Log).GetMethod("Message", new[] { typeof(string), typeof(bool) });

        public static void Message(string text)
        {
            if (MessageOneArg != null)
            {
                MessageOneArg.Invoke(null, new object[] { text });
                return;
            }

            if (MessageTwoArg != null)
            {
                MessageTwoArg.Invoke(null, new object[] { text, false });
            }
        }
    }
}