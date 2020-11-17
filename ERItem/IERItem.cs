using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace SPERCommonLib
{
    public interface IERItem
    {
        SPListItem listItem { get; }
        SPItemEventProperties eventProperties { get; }
        string itemTitle { get; }
        string eventType { get; }
    }
}

