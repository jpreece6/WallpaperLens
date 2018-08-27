using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallpaperLens.Events
{
    public interface IEventAggregator
    {
        TEvent GetEvent<TEvent>() where TEvent : EventBase, new();
    }
}
