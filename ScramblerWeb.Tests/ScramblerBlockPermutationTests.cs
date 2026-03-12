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
    public class ScramblerBlockPermutationTests
    {
        private IScrambler _permutationScrambler;

        [TestInitialize]
        public void Setup()
        {
            _permutationScrambler = new ScramblerBlockPermutation();
        }

        private byte[] StringToBytes(string str) => Encoding.UTF8.GetBytes(str);
        private string BytesToString(byte[] bytes) => Encoding.UTF8.GetString(bytes);
        private byte[] GenerateSequentialBytes(int size) => Enumerable.Range(0, size).Select(i => (byte)i).ToArray();


        [TestMethod]
        public void ScrambleDescramble_DataMatchesBlockSize_ReturnsOriginal()
        {
            byte[] data = GenerateSequentialBytes(64); 
            byte[] key = StringToBytes("permutation_key_64");

            byte[] scrambledData = _permutationScrambler.Scramble(data, key);
            byte[] descrambledData = _permutationScrambler.Descramble(scrambledData, key);

            Assert.IsNotNull(scrambledData);
            Assert.IsNotNull(descrambledData);
            CollectionAssert.AreEqual(data, descrambledData, "Descrambled data should match original data.");
            
            if (data.Length > 1 && !data.SequenceEqual(scrambledData))
            {
            }
        }

        [TestMethod]
        public void ScrambleDescramble_DataLongerThanOneBlock_ReturnsOriginal()
        {
            byte[] data = GenerateSequentialBytes(150);
            byte[] key = StringToBytes("another_key_150");

            byte[] scrambledData = _permutationScrambler.Scramble(data, key);
            byte[] descrambledData = _permutationScrambler.Descramble(scrambledData, key);

            CollectionAssert.AreEqual(data, descrambledData);
        }

        [TestMethod]
        public void ScrambleDescramble_DataSmallerThanDefaultBlockSize_ReturnsOriginal()
        {
            byte[] data = GenerateSequentialBytes(32); 
            byte[] key = StringToBytes("short_data_key_32");

            byte[] scrambledData = _permutationScrambler.Scramble(data, key);
            byte[] descrambledData = _permutationScrambler.Descramble(scrambledData, key);

            CollectionAssert.AreEqual(data, descrambledData);
        }

        [TestMethod]
        public void Scramble_EmptyKey_UsesZeroSeed_StillWorksAndReturnsOriginal()
        {
            byte[] data = StringToBytes("TestDataWithEmptyKey");
            byte[] key = new byte[0];

            byte[] scrambledData = _permutationScrambler.Scramble(data, key);
            byte[] descrambledData = _permutationScrambler.Descramble(scrambledData, key);

            CollectionAssert.AreEqual(data, descrambledData);
        }

        [TestMethod]
        public void Scramble_NullData_ReturnsNull()
        {
            byte[] data = null;
            byte[] key = StringToBytes("key");

            byte[] scrambledData = _permutationScrambler.Scramble(data, key);

            Assert.IsNull(scrambledData); 
        }

        [TestMethod]
        public void Scramble_EmptyData_ReturnsEmptyData()
        {
            byte[] data = new byte[0];
            byte[] key = StringToBytes("key");

            byte[] scrambledData = _permutationScrambler.Scramble(data, key);

            Assert.IsNotNull(scrambledData);
            Assert.AreEqual(0, scrambledData.Length);
        }

        [TestMethod]
        public void Scramble_DifferentKeys_ProduceDifferentScrambledData_ForSameInput()
        {
            byte[] data = GenerateSequentialBytes(64);
            byte[] key1 = StringToBytes("key_one");
            byte[] key2 = StringToBytes("key_two"); 

            byte[] scrambledData1 = _permutationScrambler.Scramble(data, key1);
            byte[] scrambledData2 = _permutationScrambler.Scramble(data, key2);

 
            if (key1.Sum(b => b) != key2.Sum(b => b))
            {
                Assert.IsFalse(scrambledData1.SequenceEqual(scrambledData2), "Different keys should produce different scrambled data.");
            }
            else
            {
                Assert.Inconclusive("Keys produced the same seed, cannot verify different scrambled output for this specific test case.");
            }
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

            _permutationScrambler.Scramble(data, key);
            byte[] tempScrambled = data;
            _permutationScrambler.Descramble(tempScrambled, key); 

            for (int i = 0; i < iterationsForAveraging; i++)
            {
                stopwatch.Restart();
                byte[] scrambled = _permutationScrambler.Scramble(data, key);
                stopwatch.Stop();
                scrambleTimesMs.Add(stopwatch.ElapsedMilliseconds);

                stopwatch.Restart();
                _permutationScrambler.Descramble(scrambled, key);
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
        public void Performance_Permutation_1MB_Key8B()
        {
            MeasurePerformance("Permutation: 1MB Data, 8B Key", 1 * 1024 * 1024, 8, 10); 
        }

        [TestMethod]
        public void Performance_Permutation_1MB_Key32B()
        {
            MeasurePerformance("Permutation: 1MB Data, 32B Key", 1 * 1024 * 1024, 32, 10);
        }

        [TestMethod]
        public void Performance_Permutation_1MB_Key256B()
        {
            MeasurePerformance("Permutation: 1MB Data, 256B Key", 1 * 1024 * 1024, 256, 10);
        }

        [TestMethod]
        public void Performance_Permutation_10MB_Key8B()
        {
            MeasurePerformance("Permutation: 10MB Data, 8B Key", 10 * 1024 * 1024, 8, 3);
        }

        [TestMethod]
        public void Performance_Permutation_10MB_Key32B()
        {
            MeasurePerformance("Permutation: 10MB Data, 32B Key", 10 * 1024 * 1024, 32, 3);
        }

        [TestMethod]
        public void Performance_Permutation_10MB_Key256B()
        {
            MeasurePerformance("Permutation: 10MB Data, 256B Key", 10 * 1024 * 1024, 256, 3);
        }

        [TestMethod]
        public void Performance_Permutation_100MB_Key8B()
        {
            MeasurePerformance("Permutation: 100MB Data, 8B Key", 100 * 1024 * 1024, 8, 1);
        }

        [TestMethod]
        public void Performance_Permutation_100MB_Key32B()
        {
            MeasurePerformance("Permutation: 100MB Data, 32B Key", 100 * 1024 * 1024, 32, 1);
        }

        [TestMethod]
        public void Performance_Permutation_100MB_Key256B()
        {
            MeasurePerformance("Permutation: 100MB Data, 256B Key", 100 * 1024 * 1024, 256, 1);
        }
    }
}