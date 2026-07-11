using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using Drexel.Host.Commands.Gpio;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Drexel.Host.Tests.Commands.Gpio;

[TestClass]
internal sealed class PinWriteValueTests
{
    public static IEnumerable<object[]> EqualsCases { get; } =
        [
            [PinWriteValue.Low, PinWriteValue.Low],
            [PinWriteValue.High, PinWriteValue.High],
        ];

    public static IEnumerable<object[]> EqualsObjectCases { get; } = EqualsCases;

    public static IEnumerable<object[]> NotEqualsCases { get; } =
        [
            [PinWriteValue.Low, PinWriteValue.High],
            [PinWriteValue.High, PinWriteValue.Low],
        ];

    public static IEnumerable<object?[]> NotEqualsObjectCases { get; } = NotEqualsCases.Concat(
        new object?[][]
        {
            [PinWriteValue.Low, "wrong type"],
            [PinWriteValue.Low, null],
        });

    public static IEnumerable<object[]> OperatorEqualsCases { get; } = EqualsCases;

    public static IEnumerable<object[]> OperatorNotEqualsCases { get; } = NotEqualsCases;

    public static IEnumerable<object[]> ToPinValueCases { get; } =
        [
            [PinWriteValue.Low, PinValue.Low],
            [PinWriteValue.High, PinValue.High],
        ];

    [TestMethod]
    public void Ctor_Succeeds()
    {
        PinWriteValue instance = new();

        Assert.AreEqual(PinWriteValue.Low, instance);
    }

    [TestMethod]
    public void Default_Succeeds()
    {
        PinWriteValue instance = default;

        Assert.AreEqual(PinWriteValue.Low, instance);
    }

    [TestMethod]
    [DynamicData(nameof(EqualsCases))]
    public void Equals_AreEqual_ReturnsTrue(PinWriteValue left, PinWriteValue right)
    {
        bool actual = left.Equals(right);

        Assert.IsTrue(actual);
    }

    [TestMethod]
    [DynamicData(nameof(NotEqualsCases))]
    public void Equals_AreNotEqual_ReturnsFalse(PinWriteValue left, PinWriteValue right)
    {
        bool actual = left.Equals(right);

        Assert.IsFalse(actual);
    }

    [TestMethod]
    [DynamicData(nameof(EqualsObjectCases))]
    public void Equals_Object_AreEqual_ReturnsTrue(PinWriteValue left, object? right)
    {
        bool actual = left.Equals(right);

        Assert.IsTrue(actual);
    }

    [TestMethod]
    [DynamicData(nameof(NotEqualsObjectCases))]
    public void Equals_Object_AreNotEqual_ReturnsFalse(PinWriteValue left, object? right)
    {
        bool actual = left.Equals(right);

        Assert.IsFalse(actual);
    }

    [TestMethod]
    [DynamicData(nameof(EqualsCases))]
    public void GetHashCode_AreEqual_Succeeds(PinWriteValue left, PinWriteValue right)
    {
        int leftHash = left.GetHashCode();
        int rightHash = right.GetHashCode();

        Assert.AreEqual(leftHash, rightHash);
    }

    [TestMethod]
    [DynamicData(nameof(OperatorEqualsCases))]
    public void Operator_Equals_AreEqual_ReturnsTrue(PinWriteValue left, PinWriteValue right)
    {
        Assert.IsTrue(left == right);
        Assert.IsTrue(right == left);
    }

    [TestMethod]
    [DynamicData(nameof(OperatorNotEqualsCases))]
    public void Operator_Equals_AreNotEqual_ReturnsFalse(PinWriteValue left, PinWriteValue right)
    {
        Assert.IsFalse(left == right);
        Assert.IsFalse(right == left);
    }

    [TestMethod]
    [DynamicData(nameof(OperatorEqualsCases))]
    public void Operator_NotEquals_AreEqual_ReturnsFalse(PinWriteValue left, PinWriteValue right)
    {
        Assert.IsFalse(left != right);
        Assert.IsFalse(right != left);
    }

    [TestMethod]
    [DynamicData(nameof(OperatorNotEqualsCases))]
    public void Operator_NotEquals_AreNotEqual_ReturnsFalse(PinWriteValue left, PinWriteValue right)
    {
        Assert.IsTrue(left != right);
        Assert.IsTrue(right != left);
    }

    [TestMethod]
    [DynamicData(nameof(ToPinValueCases))]
    public void ToPinMode_Succeeds(PinWriteValue instance, PinValue expected)
    {
        PinValue actual = instance.ToPinValue();

        Assert.AreEqual(expected, actual);
    }
}