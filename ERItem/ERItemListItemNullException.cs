using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SPSCommon.ERItem
{
    public class ERItemListItemNullException : Exception
    {
        public ERItemListItemNullException()
        {
        }

        public ERItemListItemNullException(string message)
            : base(message)
        {
        }

        public ERItemListItemNullException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
