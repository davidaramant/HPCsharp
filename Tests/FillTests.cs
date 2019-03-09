using System;
using HPCsharp;
using NUnit.Framework;

namespace Tests
{
    public class FillTests
    {
        [TestCase(32, 5)]
        [TestCase(33, 5)]
        [TestCase(63, 5)]
        [TestCase(64, 6)]
        [TestCase(10_000, 13)]
        public void ShouldFindLeftOverElements(int length, int expectedMsb)
        {
            Assert.That(FindMostSignificantBit(length), Is.EqualTo(expectedMsb));
        }

        [Test]
        public void ShouldFillWithArrayCopy()
        {
            int[] array = new int[100];
            array.FillUsingArrayCopy2(5);
            Assert.That(array, Has.All.EqualTo(5));
        }

        [Test]
        public void ShouldFillStructsWithArrayCopy()
        {
            TimeSpan[] array = new TimeSpan[100];
            array.FillUsingArrayCopy2(TimeSpan.FromDays(5));
            Assert.That(array, Has.All.EqualTo(TimeSpan.FromDays(5)));
        }

        [Test]
        public void ShouldFillStructsWithBlockCopy()
        {
            TimeSpan[] array = new TimeSpan[100];
            array.FillUsingBlockCopy(TimeSpan.FromDays(5));
            Assert.That(array, Has.All.EqualTo(TimeSpan.FromDays(5)));
        }

        private static int FindMostSignificantBit(int length)
        {
            int power = 0;
            while ((length >>= 1) > 0)
            {
                power++;
            }

            return power;
        }
    }
}