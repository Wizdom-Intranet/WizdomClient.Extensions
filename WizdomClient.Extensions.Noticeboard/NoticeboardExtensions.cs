using System;

namespace Wizdom.Client.Extensions
{


    public static class NoticeboardExtensions
    {
        private static Noticeboard _noticeboard;
        public static Noticeboard Noticeboard(this WizdomClient wizdomClient)
        {
            if (_noticeboard != null) return _noticeboard;
            return new Noticeboard(wizdomClient);
        }
    }
}
