using Microsoft.VisualStudio.TestTools.UnitTesting;
using Services.Scramblers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace ScramblerWeb.Server.Tests.Services.Scramblers
{
    [TestClass]
    public class ScramblerCaesarTests
    {
        private IScrambler _caesarScrambler;

        [TestInitialize]
        public void Setup()
        {
            _caesarScrambler = new ScramblerCaesar();
        }

        private byte[] StringToBytes(string str) => Encoding.UTF8.GetBytes(str);
        private string BytesToString(byte[] bytes) => Encoding.UTF8.GetString(bytes);
        private byte[] GenerateRandomBytes(int size)
        {
            Random rnd = new Random();
            byte[] bytes = new byte[size];
            rnd.NextBytes(bytes);
            return bytes;
        }

        [TestMethod]
        public void ScrambleDescramble_SimpleText_KeyShorterThanData_ReturnsOriginal()
        {
            byte[] data = StringToBytes("Hello Caesar World!");
            byte[] key = StringToBytes("shift");

            byte[] scrambledData = _caesarScrambler.Scramble(data, key);
            byte[] descrambledData = _caesarScrambler.Descramble(scrambledData, key);

            Assert.IsNotNull(scrambledData);
            Assert.IsNotNull(descrambledData);
            Assert.AreNotEqual(0, scrambledData.Length);
            CollectionAssert.AreEqual(data, descrambledData, "Descrambled data should match original data.");
            if (key.Any(b => b != 0) && data.Length > 0)
            {
                Assert.IsFalse(data.SequenceEqual(scrambledData), "Scrambled data should be different from original if key is non-trivial.");
            }
        }

        [TestMethod]
        public void ScrambleDescramble_KeySameLengthAsData_ReturnsOriginal()
        {
            byte[] data = StringToBytes("Match Key!");
            byte[] key = StringToBytes("SecretKey!"); 

            byte[] scrambledData = _caesarScrambler.Scramble(data, key);
            byte[] descrambledData = _caesarScrambler.Descramble(scrambledData, key);

            CollectionAssert.AreEqual(data, descrambledData);
        }

        [TestMethod]
        public void ScrambleDescramble_KeyLongerThanData_ReturnsOriginal()
        {
            byte[] data = StringToBytes("Tiny");
            byte[] key = StringToBytes("VeryLongKeyForCaesar");

            byte[] scrambledData = _caesarScrambler.Scramble(data, key);
            byte[] descrambledData = _caesarScrambler.Descramble(scrambledData, key);

            CollectionAssert.AreEqual(data, descrambledData);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void Scramble_NullKey_ThrowsNullReferenceException()
        {
            byte[] data = StringToBytes("TestData");
            byte[] key = null;

            _caesarScrambler.Scramble(data, key);
        }

        [TestMethod]
        [ExpectedException(typeof(DivideByZeroException))]
        public void Scramble_EmptyKey_ThrowsDivideByZeroException()
        {
            byte[] data = StringToBytes("TestData");
            byte[] key = new byte[0];

            _caesarScrambler.Scramble(data, key);
        }


        [TestMethod]
        public void Scramble_NullData_ReturnsNull()
        {
            byte[] data = null;
            byte[] key = StringToBytes("key");

            byte[] scrambledData = _caesarScrambler.Scramble(data, key);

            Assert.IsNull(scrambledData);
        }

        [TestMethod]
        public void Scramble_EmptyData_ReturnsEmptyData()
        {
            byte[] data = new byte[0];
            byte[] key = StringToBytes("key");

            byte[] scrambledData = _caesarScrambler.Scramble(data, key);

            Assert.IsNotNull(scrambledData);
            Assert.AreEqual(0, scrambledData.Length);
        }

        [TestMethod]
        public void Scramble_AllZeroKey_ReturnsOriginalData()
        {
            byte[] data = StringToBytes("TestWithZeroKey");
            byte[] key = new byte[] { 0, 0, 0 };

            byte[] scrambledData = _caesarScrambler.Scramble(data, key);

            CollectionAssert.AreEqual(data, scrambledData, "Scrambling with all-zero key should return original data.");
        }

        private void MeasurePerformance(string description, int dataSize, int keySize, int iterationsForAveraging)
        {
            byte[] data = GenerateRandomBytes(dataSize);
            byte[] key = GenerateRandomBytes(keySize).Select(b => (byte)(b == 0 ? 1 : b)).ToArray();
            if (key.Length == 0 && keySize > 0)
            {
                key = new byte[keySize];
                for (int k = 0; k < keySize; ++k) key[k] = (byte)(k + 1);
            }


            Stopwatch stopwatch = new Stopwatch();
            List<long> scrambleTimesMs = new List<long>();
            List<long> descrambleTimesMs = new List<long>();

            _caesarScrambler.Scramble(data, key);
            byte[] tempScrambled = data;
            _caesarScrambler.Descramble(tempScrambled, key);

            for (int i = 0; i < iterationsForAveraging; i++)
            {
                stopwatch.Restart();
                byte[] scrambled = _caesarScrambler.Scramble(data, key);
                stopwatch.Stop();
                scrambleTimesMs.Add(stopwatch.ElapsedMilliseconds);

                stopwatch.Restart();
                _caesarScrambler.Descramble(scrambled, key);
                stopwatch.Stop();
                descrambleTimesMs.Add(stopwatch.ElapsedMilliseconds);
            }

            double avgScrambleTime = scrambleTimesMs.Average();
            double avgDescrambleTime = descrambleTimesMs.Average();

            string resultMessage =
                $"--- {description} ---\n" +
                $"Data Size: {dataSize / (1024.0 * 1024.0):F2} MB ({dataSize} bytes), Key Size: {keySize} bytes, Iterations for Avg: {iterationsForAveraging}\n" +
                $"Avg Scramble Time: {avgScrambleTime:F4} ms\n" +
                $"Avg Descramble Time: {avgDescrambleTime:F4} ms\n" +
                "-----------------------------------";

            Console.WriteLine(resultMessage);
        }

        [TestMethod]
        public void Performance_Caesar_1MB_Key8B()
        {
            MeasurePerformance("Caesar: 1MB Data, 8B Key", 1 * 1024 * 1024, 8, 20);
        }

        [TestMethod]
        public void Performance_Caesar_1MB_Key32B()
        {
            MeasurePerformance("Caesar: 1MB Data, 32B Key", 1 * 1024 * 1024, 32, 20);
        }

        [TestMethod]
        public void Performance_Caesar_1MB_Key256B()
        {
            MeasurePerformance("Caesar: 1MB Data, 256B Key", 1 * 1024 * 1024, 256, 20);
        }

        [TestMethod]
        public void Performance_Caesar_10MB_Key8B()
        {
            MeasurePerformance("Caesar: 10MB Data, 8B Key", 10 * 1024 * 1024, 8, 5);
        }

        [TestMethod]
        public void Performance_Caesar_10MB_Key32B()
        {
            MeasurePerformance("Caesar: 10MB Data, 32B Key", 10 * 1024 * 1024, 32, 5);
        }

        [TestMethod]
        public void Performance_Caesar_10MB_Key256B()
        {
            MeasurePerformance("Caesar: 10MB Data, 256B Key", 10 * 1024 * 1024, 256, 5);
        }

        [TestMethod]
        public void Performance_Caesar_100MB_Key8B()
        {
            MeasurePerformance("Caesar: 100MB Data, 8B Key", 100 * 1024 * 1024, 8, 2);
        }

        [TestMethod]
        public void Performance_Caesar_100MB_Key32B()
        {
            MeasurePerformance("Caesar: 100MB Data, 32B Key", 100 * 1024 * 1024, 32, 2);
        }

        [TestMethod]
        public void Performance_Caesar_100MB_Key256B()
        {
            MeasurePerformance("Caesar: 100MB Data, 256B Key", 100 * 1024 * 1024, 256, 2);
        }
    }
}