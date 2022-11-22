using CodeGolds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGolds
{
    static class Sequences
    {
        public static int[] MSequence(int[] _summators)
        {
            int power = _summators.Length;
            int[] registers = new int[power];
            int[] summators = _summators;
            int[] result = new int[(int)Math.Pow(2, power) - 1];

            summators[0] = 1;
            registers[0] = 1;

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = registers[0];
                int[] last_reg = new int[power];
                for (int j = 0; j < power; j++)
                {
                    last_reg[j] = registers[j];
                }
                for (int j = 0; j < power; j++)
                {
                    registers[power - 1] += last_reg[j] * summators[j];
                }
                registers[power - 1] %= 2;
                for (int j = power - 2; j >= 0; j--)
                {
                    registers[j] = last_reg[j + 1];

                }
            }
            return result;
        }

        //две М последовательность, в интернете есть статья как это все работает
        public static int[] MSeq(int[] _summators)
        {
            int power = _summators.Length;
            int[] register = new int[power];
            int[] summator = _summators;
            int[] result = new int[(int)Math.Pow(2, power) - 1];
            for (int i = 0; i < power; i++)
            {
                register[i] = 1;
            }
            summator[power - 1] = 1;

            for (int i = 0; i < result.Length; i++)
            {
                for (int j = power; j >= 0; j--)
                {
                    if (j == power)
                        result[i] = register[power - 1];
                    if (j != power && j != 0)
                        register[j] = register[j - 1];
                    if (j == 0)
                        register[0] = 0;
                }

                for (int j = 1; j < power; j++)
                {
                    register[0] += register[j] * summator[j];
                }
                register[0] = register[0] % 2;
            }

            return result;
        }

        public static int[] GoldSequenceFromSummators(int[] summators1, int[] summators2, int _shift)
        {
            int[] m1 = MSequence(summators1);
            int[] m2 = MSequence(summators2);
            int[] m2_shifted = new int[m2.Length];
            for (int i = 0; i < m2.Length; i++)
            {
                m2_shifted[i] = m2[(i + _shift) % m2.Length];
            }
            return XOR(m1, m2_shifted);
        }

        //4 последовательности голда полученные от двух M - последовательностей
        public static int[] GoldSequence(int[] m1, int[] m2, int _shift, bool additional_bit = false)
        {
            int[] m2_shifted = new int[m2.Length];
            for (int i = 0; i < m2.Length; i++)
            {
                m2_shifted[i] = m2[(i + _shift) % m2.Length];
            }
            int[] res = XOR(m1, m2_shifted);

            int size = res.Length;
            if (additional_bit)
                size++;

            int[] to_return = new int[size];
            for (int i = 0; i < res.Length; i++)
            {
                to_return[i] = res[i];
            }
            return to_return;
        }

        //задание каждой паре бита одно из 4 последовательностей Голда, пары: 01, 10, 11, 00
        public static int[] GoldTransform(int[] bits, int[][] gold)
        {
            List<int> to_return = new List<int>();
            for (int i = 0; i < bits.Length; i += 2)
            {
                int idx = bits[i] * 2 + bits[i + 1];
                to_return.AddRange(gold[idx]);
            }
            return to_return.ToArray<int>();
        }

        public static int[] GoldDetransform(int[] gold_seq_indexes)
        {
            List<int> recovered_bits = new List<int>();
            for (int i = 0; i < gold_seq_indexes.Length; i++)
            {
                recovered_bits.AddRange(Int.ToBits(gold_seq_indexes[i], 2));
            }
            return recovered_bits.ToArray<int>();
        }

        public static int[] XOR(int[] m1, int[] m2)
        {
            int[] to_return = new int[m1.Length];
            for (int i = 0; i < m1.Length; i++)
            {
                to_return[i] = (m1[i] + m2[i]) % 2;
            }
            return to_return;
        }
    }
}
