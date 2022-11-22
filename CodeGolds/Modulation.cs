using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGolds
{
    public class Modulation
    {
        public static double A, deviateA;
        public static double DFreq, MainFreq, ModulationFreq, T;
        public int CountsPerBit;

        private double[] output;

        /// <summary>
        /// Инициализация всех полей, необходимых для модуляции
        /// </summary>
        /// <param name="_DFreq"> Частота дискретизации </param>
        /// <param name="_MainFreq"> Несущая частота </param>
        /// <param name="_ModulationFreq"> Модулирующая частота </param>
        /// <param name="_T"> Время длительности сигнала </param>
        /// <param name="_A"> Амплитуда сигнала </param>
        /// <param name="_deviateA"> Изменение амплитуды сигнала </param>
        public Modulation(double _DFreq, double _MainFreq, double _ModulationFreq, int _CountsPerBit = 64, double _A = 1, double _deviateA = 0.75)
        {
            DFreq = _DFreq;
            MainFreq = _MainFreq;
            ModulationFreq = _ModulationFreq;
            CountsPerBit = _CountsPerBit;
            A = _A;
            deviateA = _deviateA;
        }


        /// <summary>
        /// Формирование I, Q, fi компонент
        /// </summary>
        /// <param name="bits"> Массив битов </param>
        /// <returns> Возвращает массив отчетов разных функций 
        /// to_return[0] - I(t)
        /// to_return[1] - Q(t)
        /// to_return[2] - fi(t) = Atan2(Q(t), I(t))
        /// </returns>
        public double[][] PM4(int[] bits, double _beta = -1)
        {
            //List<int> bits2k = new List<int>();
            //List<int> bits2k1 = new List<int>();
            output = new double[(int)(CountsPerBit * (bits.Length + bits.Length % 2))];
            double[] I = new double[output.Length];
            double[] Q = new double[output.Length];
            double[] fi = new double[output.Length];

            //List<double> I = new List<double>();
            //List<double> Q = new List<double>();
            List<double> xI = new List<double>();
            List<double> xQ = new List<double>();

            double phase = 0;

            for (int i = 0; i < bits.Length; i += 2)
            {
                for (int j = 0; j < CountsPerBit * 2; j++)
                {
                    if (i < bits.Length)
                        I[i * CountsPerBit + j] = (bits[i] * A - 0.5 * A);
                    else
                        I[i * CountsPerBit + j] = 0;
                    if (i + 1 < bits.Length)
                        Q[i * CountsPerBit + j] = (bits[i + 1] * A - 0.5 * A);
                    else
                        Q[i * CountsPerBit + j] = 0;

                    xI.Add(1.0 / DFreq * (i * CountsPerBit + j));
                    xQ.Add(xI[xI.Count - 1]);

                    var res = Math.Atan2(Q[i * CountsPerBit + j], I[i * CountsPerBit + j]);
                    fi[i * CountsPerBit + j] = res / Math.PI;

                    output[i * CountsPerBit + j] = I[i * CountsPerBit + j] * Math.Cos(phase) - Q[i * CountsPerBit + j] * Math.Sin(phase);
                    phase += MainFreq / DFreq * Math.PI * 2;
                    if (phase > Math.PI * 2) phase -= Math.PI * 2;
                }
            }

            double[][] to_return = new double[4][];
            to_return[0] = I;
            to_return[1] = Q;
            to_return[2] = fi;
            to_return[3] = output;
            return to_return;
        }


        /// <summary>
        /// Формирование временной оси для отрисовки очетов (если надо будет)
        /// </summary>
        /// <param name="_CurrentTime"> Время, которое было перед вызовом метода (можно получить из предыдущего канала) </param>
        /// <returns> Возвращает time[T * DFreq * ChannelsPerMessage] отчетов времени, каждый из которых соответствует своему отчету сигнала </returns>
        public double[] GetTime(double _CurrentTime = 0)
        {
            if (output != null)
            {
                double[] time = new double[output.Length];
                time[0] = 0;
                for (int i = 1; i < time.Length; i++)
                {
                    time[i] = time[i - 1] + 1.0 / DFreq;
                }
                return time;
            }
            else
            {
                return new double[] { -1 };
            }
        }
    }
}
