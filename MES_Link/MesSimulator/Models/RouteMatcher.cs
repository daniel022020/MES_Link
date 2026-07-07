using System;
using System.Collections.Generic;
using System.Linq;

namespace MES_Link.MesSimulator
{
    public static class RouteMatcher
    {
        public static MesRouteBlock Match(IEnumerable<MesRouteBlock> routes, string path)
        {
            if (routes == null || string.IsNullOrEmpty(path))
                return null;

            return routes.FirstOrDefault(r =>
                r.RouteUrl.Equals(path, StringComparison.OrdinalIgnoreCase));
        }
    }
}