using Xunit;

namespace Vouch.Tests;

public class FloatingPointEdgeTests
{
    private static void Fails(Action act) => Assert.Throws<VouchException>(act);

    private static readonly double[] Doubles =
    {
        double.NaN, double.PositiveInfinity, double.NegativeInfinity, double.MaxValue, double.MinValue,
    };

    private static readonly float[] Floats =
    {
        float.NaN, float.PositiveInfinity, float.NegativeInfinity, float.MaxValue, float.MinValue,
    };

    [Fact]
    public void Double_BePositive_and_BeNegative()
    {
        double.PositiveInfinity.Should().BePositive();
        double.MaxValue.Should().BePositive();
        double.NegativeInfinity.Should().BeNegative();
        double.MinValue.Should().BeNegative();
        Fails(() => double.NaN.Should().BePositive());
        Fails(() => double.NegativeInfinity.Should().BePositive());
        Fails(() => double.MinValue.Should().BePositive());
        Fails(() => double.NaN.Should().BeNegative());
        Fails(() => double.PositiveInfinity.Should().BeNegative());
        Fails(() => double.MaxValue.Should().BeNegative());
    }

    [Fact]
    public void Float_BePositive_and_BeNegative()
    {
        float.PositiveInfinity.Should().BePositive();
        float.MaxValue.Should().BePositive();
        float.NegativeInfinity.Should().BeNegative();
        float.MinValue.Should().BeNegative();
        Fails(() => float.NaN.Should().BePositive());
        Fails(() => float.NegativeInfinity.Should().BePositive());
        Fails(() => float.MinValue.Should().BePositive());
        Fails(() => float.NaN.Should().BeNegative());
        Fails(() => float.PositiveInfinity.Should().BeNegative());
        Fails(() => float.MaxValue.Should().BeNegative());
    }

    [Fact]
    public void Double_BeNaN_and_BeFinite_across_edge_values()
    {
        foreach (var value in Doubles)
        {
            var v = value;
            if (double.IsNaN(v)) { v.Should().BeNaN(); Fails(() => v.Should().NotBeNaN()); } else { v.Should().NotBeNaN(); Fails(() => v.Should().BeNaN()); }
            if (double.IsFinite(v)) { v.Should().BeFinite(); Fails(() => v.Should().NotBeFinite()); } else { v.Should().NotBeFinite(); Fails(() => v.Should().BeFinite()); }
        }
    }

    [Fact]
    public void Float_BeNaN_and_BeFinite_across_edge_values()
    {
        foreach (var value in Floats)
        {
            var v = value;
            if (float.IsNaN(v)) { v.Should().BeNaN(); Fails(() => v.Should().NotBeNaN()); } else { v.Should().NotBeNaN(); Fails(() => v.Should().BeNaN()); }
            if (float.IsFinite(v)) { v.Should().BeFinite(); Fails(() => v.Should().NotBeFinite()); } else { v.Should().NotBeFinite(); Fails(() => v.Should().BeFinite()); }
        }
    }

    [Fact]
    public void Double_BeCloseTo_edge_values()
    {
        // NaN is close to nothing, not even itself.
        Fails(() => double.NaN.Should().BeCloseTo(double.NaN, 1));
        Fails(() => double.NaN.Should().BeCloseTo(1.0, double.MaxValue));
        Fails(() => 1.0.Should().BeCloseTo(double.NaN, double.MaxValue));
        Fails(() => 1.0.Should().BeCloseTo(1.0, double.NaN));
        double.NaN.Should().NotBeCloseTo(double.NaN, 1);
        double.NaN.Should().NotBeCloseTo(1.0, double.MaxValue);

        // Equal infinities are close, opposite ones and finite ones are not (no overflow exception).
        double.PositiveInfinity.Should().BeCloseTo(double.PositiveInfinity, 0);
        double.NegativeInfinity.Should().BeCloseTo(double.NegativeInfinity, 0);
        Fails(() => double.PositiveInfinity.Should().BeCloseTo(double.NegativeInfinity, double.MaxValue));
        Fails(() => double.PositiveInfinity.Should().BeCloseTo(double.MaxValue, double.MaxValue));
        double.PositiveInfinity.Should().NotBeCloseTo(double.NegativeInfinity, double.MaxValue);
        Fails(() => double.PositiveInfinity.Should().NotBeCloseTo(double.PositiveInfinity, 0));

        // MaxValue and MinValue: the difference overflows to infinity, which is beyond any finite delta.
        double.MaxValue.Should().BeCloseTo(double.MaxValue, 0);
        double.MinValue.Should().BeCloseTo(double.MinValue, 0);
        Fails(() => double.MaxValue.Should().BeCloseTo(double.MinValue, double.MaxValue));
        double.MaxValue.Should().NotBeCloseTo(double.MinValue, double.MaxValue);
        double.MaxValue.Should().BeCloseTo(double.MaxValue / 2, double.MaxValue);
    }

    [Fact]
    public void Float_BeCloseTo_edge_values()
    {
        Fails(() => float.NaN.Should().BeCloseTo(float.NaN, 1));
        Fails(() => float.NaN.Should().BeCloseTo(1f, float.MaxValue));
        float.NaN.Should().NotBeCloseTo(1f, float.MaxValue);
        float.PositiveInfinity.Should().BeCloseTo(float.PositiveInfinity, 0);
        float.NegativeInfinity.Should().BeCloseTo(float.NegativeInfinity, 0);
        Fails(() => float.PositiveInfinity.Should().BeCloseTo(float.NegativeInfinity, float.MaxValue));
        Fails(() => float.PositiveInfinity.Should().BeCloseTo(float.MaxValue, float.MaxValue));
        float.PositiveInfinity.Should().NotBeCloseTo(float.NegativeInfinity, float.MaxValue);
        float.MaxValue.Should().BeCloseTo(float.MaxValue, 0);
        float.MinValue.Should().BeCloseTo(float.MinValue, 0);
        Fails(() => float.MaxValue.Should().BeCloseTo(float.MinValue, float.MaxValue));
        float.MaxValue.Should().NotBeCloseTo(float.MinValue, float.MaxValue);
    }
}
