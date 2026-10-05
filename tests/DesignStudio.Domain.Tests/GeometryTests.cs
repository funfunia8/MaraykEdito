using DesignStudio.Geometry;

namespace DesignStudio.Domain.Tests;

public class GeometryTests
{
    [Fact]
    public void Basic_Engine_Creates_Valid_Box()
    {
        var engine = new BasicGeometryEngine();

        var box = engine.CreateBox(900, 600, 2200);

        Assert.True(engine.IsValid(box));
        Assert.Equal(900, box.Width);
        Assert.Equal(600, box.Depth);
        Assert.Equal(2200, box.Height);
    }

    [Fact]
    public void Basic_Engine_Uses_Centralized_Tolerance_Check()
    {
        var engine = new BasicGeometryEngine();

        Assert.True(engine.AreEqual(100.0, 100.0005, 0.001));
        Assert.False(engine.AreEqual(100.0, 100.01, 0.001));
    }
}
