using Microsoft.VisualStudio.TestTools.UnitTesting;
using Services.Scramblers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ScramblerWeb.Server.Tests.Services.Scramblers
{
    [TestClass]
    public class ScramblerXORTests
    {
        private IScrambler _xorScrambler;

        [TestInitialize]
        public void Setup()
        {
            _xorScrambler = new ScramblerXOR();
        }

        private byte[] StringToBytes(string str) => Encoding.UTF8.GetBytes(str);
        private string BytesToString(byte[] bytes) => Encoding.UTF8.GetString(bytes);

        [TestMethod]
        public void ScrambleDescramble_SimpleText_ReturnsOriginal()
        {
            byte[] data = StringToBytes("Hello World!");
            byte[] key = StringToBytes("secretkey");

            byte[] scrambledData = _xorScrambler.Scramble(data, key);
            byte[] descrambledData = _xorScrambler.Descramble(scrambledData, key);

            Assert.IsNotNull(scrambledData);
            Assert.IsNotNull(descrambledData);
            CollectionAssert.AreEqual(data, descrambledData, "Descrambled data should match original data.");
            Assert.IsFalse(data.SequenceEqual(scrambledData), "Scrambled data should be different from original if key is applied.");
        }

        [TestMethod]
        public void Scramble_NullKey_ReturnsOriginalData()
        {
            byte[] data = StringToBytes("TestData");
            byte[] key = null;

            byte[] scrambledData = _xorScrambler.Scramble(data, key);

            CollectionAssert.AreEqual(data, scrambledData, "Scrambling with null key should return original data.");
        }

        [TestMethod]
        public void Scramble_EmptyKey_ReturnsOriginalData()
        {
            byte[] data = StringToBytes("TestData");
            byte[] key = new byte[0];

            byte[] scrambledData = _xorScrambler.Scramble(data, key);

            CollectionAssert.AreEqual(data, scrambledData, "Scrambling with empty key should return original data.");
        }


        [TestMethod]
        public void Scramble_EmptyData_ReturnsEmptyData()
        {
            byte[] data = new byte[0];
            byte[] key = StringToBytes("key");

            byte[] scrambledData = _xorScrambler.Scramble(data, key);

            Assert.AreEqual(0, scrambledData.Length);
        }

        [TestMethod]
        public void Scramble_KeyLongerThanData_CorrectlyScrambles()
        {
            byte[] data = StringToBytes("Hi");
            byte[] key = StringToBytes("longsecretkey");

            byte[] scrambledData = _xorScrambler.Scramble(data, key);
            byte[] descrambledData = _xorScrambler.Descramble(scrambledData, key);

            CollectionAssert.AreEqual(data, descrambledData);
        }
        private byte[] GenerateRandomBytes(int size)
        {
            Random rnd = new Random(); 
            byte[] bytes = new byte[size];
            rnd.NextBytes(bytes);
            return bytes;
        }

        private void MeasurePerformance(string description, int dataSize, int keySize, int iterationsForAveraging)
        {
            byte[] data = GenerateRandomBytes(dataSize);
            byte[] key = GenerateRandomBytes(keySize);

            Stopwatch stopwatch = new Stopwatch();
            List<long> scrambleTimesMs = new List<long>();
            List<long> descrambleTimesMs = new List<long>();

            _xorScrambler.Scramble(data, key);
            byte[] tempScrambled = data; 
            _xorScrambler.Descramble(tempScrambled, key);

            for (int i = 0; i < iterationsForAveraging; i++)
            {
                stopwatch.Restart();
                byte[] scrambled = _xorScrambler.Scramble(data, key);
                stopwatch.Stop();
                scrambleTimesMs.Add(stopwatch.ElapsedMilliseconds);

                stopwatch.Restart();
                _xorScrambler.Descramble(scrambled, key); 
                stopwatch.Stop();
                descrambleTimesMs.Add(stopwatch.ElapsedMilliseconds);
            }

            double avgScrambleTime = scrambleTimesMs.Average();
            double avgDescrambleTime = descrambleTimesMs.Average();

            string resultMessage =
                $"--- {description} ---\n" +
                $"Data Size: {dataSize / 1024.0:F2} KB ({dataSize} bytes), Key Size: {keySize} bytes, Iterations for Avg: {iterationsForAveraging}\n" +
                $"Avg Scramble Time: {avgScrambleTime:F4} ms\n" +
                $"Avg Descramble Time: {avgDescrambleTime:F4} ms\n" +
                "-----------------------------------";

            Console.WriteLine(resultMessage);
            TestContext?.WriteLine(resultMessage); 
        }

        public TestContext TestContext { get; set; } 

        
        [TestMethod]
        public void Performance_XOR_1MB_Key8B()
        {
            MeasurePerformance("XOR: 1MB Data, 8B Key", 1 * 1024 * 1024, 8, 20);
        }

        [TestMethod]
        public void Performance_XOR_1MB_Key32B()
        {
            MeasurePerformance("XOR: 1MB Data, 32B Key", 1 * 1024 * 1024, 32, 20);
        }

        [TestMethod]
        public void Performance_XOR_1MB_Key256B()
        {
            MeasurePerformance("XOR: 1MB Data, 256B Key", 1 * 1024 * 1024, 256, 20);
        }

        [TestMethod]
        public void Performance_XOR_10MB_Key8B()
        {
            MeasurePerformance("XOR: 10MB Data, 8B Key", 10 * 1024 * 1024, 8, 5); 
        }

        [TestMethod]
        public void Performance_XOR_10MB_Key32B()
        {
            MeasurePerformance("XOR: 10MB Data, 32B Key", 10 * 1024 * 1024, 32, 5); 
        }

        [TestMethod]
        public void Performance_XOR_10MB_Key256B()
        {
            MeasurePerformance("XOR: 10MB Data, 256B Key", 10 * 1024 * 1024, 256, 5); 
        }

        [TestMethod]
        public void Performance_XOR_100MB_Key8B()
        {
            MeasurePerformance("XOR: 100MB Data, 8B Key", 100 * 1024 * 1024, 8, 5);
        }

        [TestMethod]
        public void Performance_XOR_100MB_Key32B()
        {
            MeasurePerformance("XOR: 100MB Data, 32B Key", 100 * 1024 * 1024, 32, 5);
        }

        [TestMethod]
        public void Performance_XOR_100MB_Key256B()
        {
            MeasurePerformance("XOR: 100MB Data, 256B Key", 100 * 1024 * 1024, 256, 5);
        }
    }
}
