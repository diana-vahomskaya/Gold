using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGolds
{
    static class Arrays
    {
        /// <summary>
        /// Наложение шума с заданной энергией на передаваемый массив
        /// </summary>
        /// <param name="array"> Массив, который будет зашумлен </param>
        /// <param name="SNR"> Значение SNR </param>
        /// <returns> Зашумленный массив </returns>
        public static double[] AddNoise(double[] array, double SNR)
        {
            Random rand = new Random();
            /// Начнем накладывать шум
            double[] noise = new double[array.Length];
            double[] to_return = new double[array.Length];
            /*for (int k = 0; k < array.Length; k++)
            {
                noise[k] = (rand.NextDouble() - 0.5) / 12;
            }*/
            for (int i = 0; i < 12; i++)
            {
                for (int k = 0; k < array.Length; k++)
                {
                    noise[k] += (rand.NextDouble() - 0.5) / 12;
                }
            }
            /// Отнормировали шум
            double s_energy = GetEnergy(array);
            double n_energy = GetEnergy(noise);
            double multiplier = Math.Sqrt(s_energy / n_energy * Math.Pow(10, -SNR / 10));
            for (int k = 0; k < array.Length; k++)
            {
                to_return[k] = array[k] + noise[k] * multiplier;
            }
            return to_return;
        }

        /// <summary>
        /// Получение энергии массива
        /// </summary>
        /// <param name="array"> Массив </param>
        /// <returns> Энергия массива array </returns>
        private static double GetEnergy(double[] array)
        {
            double energy = 0;
            for (int i = 0; i < array.Length; i++)
            {
                energy += array[i] * array[i];
            }
            return energy;
        }

        public static double[] Rxx(double[] _arr1, double[] _arr2)
        {
            double[] arr1, arr2, to_return;

            if (_arr1.Length >= _arr2.Length)
            {
                arr1 = _arr1;
                arr2 = _arr2;
                to_return = new double[arr1.Length];
            }
            else
            {
                arr1 = _arr2;
                arr2 = _arr1;
                to_return = new double[arr1.Length];
            }

            for (int i = 0; i < arr1.Length; i++)
            {
                for (int j = 0; j < arr2.Length; j++)
                {
                    to_return[i] += arr1[(arr1.Length + i + j) % arr1.Length] * arr2[j];
                }
            }

            return to_return;
        }
        public static double[] Rxx(int[] _arr1, int[] _arr2)
        {
            int[] arr1, arr2;
            double[] to_return;

            if (_arr1.Length >= _arr2.Length)
            {
                arr1 = _arr1;
                arr2 = _arr2;
                to_return = new double[arr1.Length];
            }
            else
            {
                arr1 = _arr2;
                arr2 = _arr1;
                to_return = new double[arr1.Length];
            }

            for (int i = 0; i < arr1.Length; i++)
            {
                for (int j = 0; j < arr2.Length; j++)
                {
                    to_return[i] += arr1[(arr1.Length + i + j) % arr1.Length] * arr2[j];
                }
            }

            return to_return;
        }
        public static double[] Sum(double[] _arr1, double[] _arr2)
        {
            int maxL = _arr1.Length > _arr2.Length ? _arr1.Length : _arr2.Length;
            int minL = _arr1.Length < _arr2.Length ? _arr1.Length : _arr2.Length;
            double[] to_return = new double[maxL];
            for (int i = 0; i < minL; i++)
            {
                to_return[i] = _arr1[i] + _arr2[i];
            }
            if (_arr1.Length > _arr2.Length)
                for (int i = minL; i < maxL; i++)
                {
                    to_return[i] = _arr1[i];
                }
            else
                for (int i = minL; i < maxL; i++)
                {
                    to_return[i] = _arr2[i];
                }
            return to_return;
        }
        public static double[] Minus(double[] _arr1, double[] _arr2)
        {
            int maxL = _arr1.Length > _arr2.Length ? _arr1.Length : _arr2.Length;
            int minL = _arr1.Length < _arr2.Length ? _arr1.Length : _arr2.Length;
            double[] to_return = new double[maxL];
            for (int i = 0; i < minL; i++)
            {
                to_return[i] = _arr1[i] - _arr2[i];
                if (Math.Abs(to_return[i]) < 1e-3) to_return[i] = 0;
            }
            if (_arr1.Length > _arr2.Length)
                for (int i = minL; i < maxL; i++)
                {
                    to_return[i] = _arr1[i];
                }
            else
                for (int i = minL; i < maxL; i++)
                {
                    to_return[i] = -_arr2[i];
                }
            return to_return;
        }
        public static double[] Cut(double[] arr, int from, int to)
        {
            double[] to_return = new double[to - from];
            for (int i = from; i < to; i++)
            {
                to_return[i - from] = arr[i];
            }
            return to_return;
        }
        public static double GetMax(double[] arr)
        {
            double max = Double.MinValue;
            for (int i = 0; i < arr.Length; i++)
            {
                if (max < arr[i]) max = arr[i];
            }
            return max;
        }
        public static int GetMaxIdx(double[] arr)
        {
            int max = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[max] < arr[i]) max = i;
            }
            return max;
        }
        public static double GetMin(double[] arr)
        {
            double min = Double.MaxValue;
            for (int i = 0; i < arr.Length; i++)
            {
                if (min > arr[i]) min = arr[i];
            }
            return min;
        }


        public static int[] Cut(int[] arr, int from, int to)
        {
            int[] to_return = new int[to - from];
            for (int i = from; i < to; i++)
            {
                to_return[i - from] = arr[i];
            }
            return to_return;
        }
    }
}
