using System.Collections.Generic;
using MES_Link.MesSimulator;
using Xunit;

namespace MES_Link.Tests
{
    public class RouteMatcherTests
    {
        // 路徑存在時,能正確找到對應的 Route
        [Fact]
        public void Match_ExistingRoute_ReturnsBlock()
        {
            var routes = new List<MesRouteBlock> { new MesRouteBlock { RouteUrl = "api/test" } };

            var result = RouteMatcher.Match(routes, "api/test");

            Assert.NotNull(result);
        }

        // 路徑大小寫不同也要能匹配(API/TEST & api/test)
        [Fact]
        public void Match_IsCaseInsensitive()
        {
            var routes = new List<MesRouteBlock> { new MesRouteBlock { RouteUrl = "api/test" } };

            var result = RouteMatcher.Match(routes, "API/TEST");

            Assert.NotNull(result);
        }

        // 路徑不存在時,回傳 null 而不是報錯
        [Fact]
        public void Match_NoMatchingRoute_ReturnsNull()
        {
            var routes = new List<MesRouteBlock> { new MesRouteBlock { RouteUrl = "api/test" } };

            var result = RouteMatcher.Match(routes, "not-exist");

            Assert.Null(result);
        }
    }
}