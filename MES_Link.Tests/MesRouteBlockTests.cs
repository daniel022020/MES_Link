using MES_Link.MesSimulator;
using Xunit;

namespace MES_Link.Tests
{
    public class MesRouteBlockTests
    {
        // 選了 Format2 之後,Format1 要自動變成沒選(互斥邏輯)
        [Fact]
        public void SelectingFormat2_ShouldAutomaticallyUnselectFormat1()
        {
            var route = new MesRouteBlock();
            route.IsSelected_Format2 = true;

            Assert.False(route.IsSelected_Format1);
            Assert.True(route.IsSelected_Format2);
        }

        // 物件剛建立時,預設狀態要是 Format1 被選中
        [Fact]
        public void DefaultState_Format1IsSelected()
        {
            var route = new MesRouteBlock();

            Assert.True(route.IsSelected_Format1);
        }
    }
}