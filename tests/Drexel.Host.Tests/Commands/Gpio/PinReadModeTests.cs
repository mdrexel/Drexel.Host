using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using Drexel.Host.Commands.Gpio;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Drexel.Host.Tests.Commands.Gpio;

[TestClass]
internal sealed class PinReadModeTests
{
    public static IEnumerable<object[]> EqualsCases { get; } =
        [
            [PinReadMode.Default, PinReadMode.Default],
            [PinReadMode.PullDown, PinReadMode.PullDown],
            [PinReadMode.PullUp, PinReadMode.PullUp],
        ];

    public static IEnumerable<object[]> EqualsObjectCases { get; } = EqualsCases;

    public static IEnumerable<object[]> NotEqualsCases { get; } =
        [
            [PinReadMode.Default, PinReadMode.PullDown],
            [PinReadMode.Default, PinReadMode.PullUp],
            [PinReadMode.PullDown, PinReadMode.Default],
            [PinReadMode.PullDown, PinReadMode.PullUp],
            [PinReadMode.PullUp, PinReadMode.Default],
            [PinReadMode.PullUp, PinReadMode.PullDown],
        ];

    public static IEnumerable<object?[]> NotEqualsObjectCases { get; } = NotEqualsCases.Concat(
        new object?[][]
        {
            [PinReadMode.Default, "wrong type"],
            [PinReadMode.Default, null],
        });

    public static IEnumerable<object[]> OperatorEqualsCases { get; } = EqualsCases;

    public static IEnumerable<object[]> OperatorNotEqualsCases { get; } = NotEqualsCases;

    public static IEnumerable<object[]> ToPinModeCases { get; } =
        [
            [PinReadMode.Default, PinMode.Input],
            [PinReadMode.PullDown, PinMode.InputPullDown],
            [PinReadMode.PullUp, PinMode.InputPullUp],
        ];

    [TestMethod]
    public void Ctor_Succeeds()
    {
        PinReadMode instance = new();

        Assert.AreEqual(PinReadMode.Default, instance);
    }

    [TestMethod]
    public void Default_Succeeds()
    {
        PinReadMode instance = default;

        Assert.AreEqual(PinReadMode.Default, instance);
    }

    [DataTestMethod]
    [DynamicData(nameof(EqualsCases))]
    public void Equals_AreEqual_ReturnsTrue(PinReadMode left, PinReadMode right)
    {
        bool actual = left.Equals(right);

        Assert.IsTrue(actual);
    }

    [DataTestMethod]
    [DynamicData(nameof(NotEqualsCases))]
    public void Equals_AreNotEqual_ReturnsFalse(PinReadMode left, PinReadMode right)
    {
        bool actual = left.Equals(right);

        Assert.IsFalse(actual);
    }

    [DataTestMethod]
    [DynamicData(nameof(EqualsObjectCases))]
    public void Equals_Object_AreEqual_ReturnsTrue(PinReadMode left, object? right)
    {
        bool actual = left.Equals(right);

        Assert.IsTrue(actual);
    }

    [DataTestMethod]
    [DynamicData(nameof(NotEqualsObjectCases))]
    public void Equals_Object_AreNotEqual_ReturnsFalse(PinReadMode left, object? right)
    {
        bool actual = left.Equals(right);

        Assert.IsFalse(actual);
    }

    [DataTestMethod]
    [DynamicData(nameof(EqualsCases))]
    public void GetHashCode_AreEqual_Succeeds(PinReadMode left, PinReadMode right)
    {
        int leftHash = left.GetHashCode();
        int rightHash = right.GetHashCode();

        Assert.AreEqual(leftHash, rightHash);
    }

    [DataTestMethod]
    [DynamicData(nameof(OperatorEqualsCases))]
    public void Operator_Equals_AreEqual_ReturnsTrue(PinReadMode left, PinReadMode right)
    {
        Assert.IsTrue(left == right);
        Assert.IsTrue(right == left);
    }

    [DataTestMethod]
    [DynamicData(nameof(OperatorNotEqualsCases))]
    public void Operator_Equals_AreNotEqual_ReturnsFalse(PinReadMode left, PinReadMode right)
    {
        Assert.IsFalse(left == right);
        Assert.IsFalse(right == left);
    }

    [DataTestMethod]
    [DynamicData(nameof(OperatorEqualsCases))]
    public void Operator_NotEquals_AreEqual_ReturnsFalse(PinReadMode left, PinReadMode right)
    {
        Assert.IsFalse(left != right);
        Assert.IsFalse(right != left);
    }

    [DataTestMethod]
    [DynamicData(nameof(OperatorNotEqualsCases))]
    public void Operator_NotEquals_AreNotEqual_ReturnsFalse(PinReadMode left, PinReadMode right)
    {
        Assert.IsTrue(left != right);
        Assert.IsTrue(right != left);
    }

    [DataTestMethod]
    [DynamicData(nameof(ToPinModeCases))]
    public void ToPinMode_Succeeds(PinReadMode instance, PinMode expected)
    {
        PinMode actual = instance.ToPinMode();

        Assert.AreEqual(expected, actual);
    }
}