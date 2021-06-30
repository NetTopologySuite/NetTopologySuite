using System;
using NetTopologySuite.Operation.Union;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Union
{
    public class DisjointSetsTest
    {
        [Test]
        public void TestEmpty()
        {
            int[] nums = Array.Empty<int>();
            CheckIntsModulo(nums, 3, new string[] { });
        }

        [Test]
        public void TestSingleItem()
        {
            int[] nums = new int[] { 11 };
            CheckIntsModulo(nums, 3, new string[] { "11",});
        }

        [Test]
        public void TestIntsModulo3()
        {
            int[] nums = new int[]
            {
                11, 22, 3, 45, 5, 62, 7
            };
            CheckIntsModulo(nums, 3, new string[]
            {
                "3,45",
                "11,5,62",
                "22,7"
            });
        }

        [Test]
        public void TestIntsModulo2()
        {
            int[] nums = new int[]
            {
                11, 22, 3, 45, 5, 62, 7
            };
            CheckIntsModulo(nums, 2, new string[]
            {
                "22,62",
                "11,3,45,5,7"
            });
        }

        private void CheckIntsModulo(int[] nums, int modulus, string[] setsExpected)
        {
            var dset = new DisjointSets(nums.Length);
            for (int i = 1; i < nums.Length; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (nums[j] % modulus == nums[i] % modulus)
                    {
                        dset.Merge(i, j);
                    }
                }
            }

            string[] sets = DumpSets(nums, dset);
            Assert.That(sets.Length, Is.EqualTo(setsExpected.Length));
            for (int i = 0; i < sets.Length; i++)
            {
                Assert.That(sets[i], Is.EqualTo(setsExpected[i]));
            }
        }

        private string[] DumpSets(int[] nums, DisjointSets dset)
        {
            var subsets = dset.GetSubsets();
            int nSet = subsets.Count;
            //System.out.println("# Sets = " + nSet);
            string[] sets = new string[nSet];
            for (int s = 0; s < nSet; s++)
            {
                //System.out.println("---- Set " + s);
                int size = subsets.GetSize(s);
                string str = "";
                for (int si = 0; si < size; si++)
                {
                    int itemIndex = subsets.GetItem(s, si);
                    if (si > 0) str += ",";
                    str += nums[itemIndex];
                }

                sets[s] = str;

                //TestContext.WriteLine(str);
            }

            return sets;
        }
    }

}
