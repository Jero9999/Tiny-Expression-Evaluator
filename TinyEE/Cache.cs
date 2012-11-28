using System;

namespace TinyEE
{
    internal static class Cache
    {
        internal static readonly Lazy<Parser> Parser = new Lazy<Parser>(() => new Parser(new Scanner()), true);
    }
}