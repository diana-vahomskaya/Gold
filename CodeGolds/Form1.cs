using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace CodeGolds
{
    public partial class Form1 : Form
    {
        Modulation md;
        Random rnd;
        Thread exp_thread;

        bool[] maximize = new bool[3] { false, false, false };

        int[] bits, changed_bits;
        int[][] gold;
        double[][] gold_qpsk, h;
        double[][] decs;
        Complex[] z;

        double[] x, I, Q, fi, s;
        double[] nI, nQ, nfi, ns;
        double[] success, SNR_values;

        int CountsPerBit;
        int mode = 0,
            N, //число усреднений
            M; //число значений SNR

        double Beta;
        double SNR, minSNR, maxSNR;

        public Form1()
        {
            InitializeComponent();
            UpdateValues();

            StartInit();
        }

        public void StartInit()
        {
            rnd = new Random();

            gold = new int[4][];
            gold_qpsk = new double[4][];

            /* //задание двух последовательностей
             int[] m1 = Sequences.MSeq(new int[] { 0, 1, 0, 0, 0 });
             int[] m2 = Sequences.MSeq(new int[] { 0, 1, 1, 1, 0 });

             Bitrate_param.Text = ((int)(ToDouble(DFreq_param) / ToInt(CountsPerBit_param))).ToString();

             //19, 14, 12, 17 - для кодов Голда длиной 31
             gold[0] = Sequences.GoldSequence(m1, m2, 19);
             gold[1] = Sequences.GoldSequence(m1, m2, 14);
             gold[2] = Sequences.GoldSequence(m1, m2, 12);
             gold[3] = Sequences.GoldSequence(m1, m2, 17);
            */
            int[] m1 = Sequences.MSeq(new int[] { 0, 1, 0, 0, 0, 0 });
            int[] m2 = Sequences.MSeq(new int[] { 0, 1, 1, 1, 0, 0 });
            // 5, 8, 17, 33 - для кодов Голда длины 63
            gold[0] = Sequences.GoldSequence(m1, m2, 5, true);
            gold[1] = Sequences.GoldSequence(m1, m2, 8, true);
            gold[2] = Sequences.GoldSequence(m1, m2, 17, true);
            gold[3] = Sequences.GoldSequence(m1, m2, 33, true);


            h = new double[4][];
            decs = new double[4][];
        }

     
        public void UpdateValues()
        {
            Beta = ToDouble(Beta_param);

            CountsPerBit = ToInt(CountsPerBit_param);
            SNR = ToDouble(SNR_param);

            minSNR = ToDouble(SNRmin_param);
            maxSNR = ToDouble(SNRmax_param);
            M = (int)((1 / ToDouble(CountOfSNRSteps_param)) * (maxSNR + Math.Abs(minSNR)));
            N = ToInt(CountOfExps_param);
        }

        private void Modulation_action_Click(object sender, EventArgs e)
        {
            UpdateValues();

            bits = Bits.FromString(Bits_param.Text);
            changed_bits = Sequences.GoldTransform(bits, gold);

            GetSignals();
            NoiseSignals(SNR);

            DrawIQ();
            DrawI();
            DrawQ();
            DrawS();
            Experiment();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void NoiseSignals(double SNR)
        {
            nI = Arrays.AddNoise(I, SNR);
            nQ = Arrays.AddNoise(Q, SNR);
            nfi = Arrays.AddNoise(fi, SNR);
            ns = Arrays.AddNoise(s, SNR);
        }

        private void GetSignals()
        {
            md = new Modulation(ToDouble(DFreq_param), ToDouble(MainFreq_param), 6000, CountsPerBit);
            var outputs = md.PM4(changed_bits);
            I = outputs[0];
            Q = outputs[1];
            fi = outputs[2];
            s = outputs[3];
            x = md.GetTime(0);
            /// Модуляция каждой последовательности Голда ФМ-4 модуляцией
            for (int i = 0; i < 4; i++)
            {
                gold_qpsk[i] = md.PM4(gold[i])[2];
            }
        }

        private  int Experiment(bool is_threaded = false)
        {
            for (int i = 0; i < decs.Length; i++)
            {
                decs[i] = Arrays.Rxx(nfi, gold_qpsk[i]);
            }

            if (!is_threaded)
                DrawCorrelation();
            int[] max_idx = new int[bits.Length / 2];
            for (int i = 0; i < max_idx.Length; i++)
            {
                double[][] cut_of_rxx = new double[4][];
                double max = double.MinValue;
                int idx = -1;
                for (int k = 0; k < 4; k++)
                {
                    cut_of_rxx[k] = Arrays.Cut(decs[k], i * gold[0].Length * CountsPerBit, (i + 1) * gold[0].Length * CountsPerBit);
                    double potentional_max = Arrays.GetMax(cut_of_rxx[k]);
                    if (potentional_max > max)
                    {
                        max = potentional_max;
                        idx = k;
                    }
                }
                max_idx[i] = idx;
            }

            int right_bits = 0;
            int[] decoded_bits = Sequences.GoldDetransform(max_idx);
            for (int i = 0; i < bits.Length; i++)
            {
                right_bits += bits[i] == decoded_bits[i] ? 1 : 0;
            }
            double success = (double)(right_bits) / bits.Length;
            if (!is_threaded)
                success_param.Text = (success * 100).ToString("F2");

            string decoded_bits_str = Bits.ToString(decoded_bits, 16);
            if (!is_threaded)
                DecodedBits_param.Text = decoded_bits_str;

            return right_bits;

        }
        private void Experiment_action_Click(object sender, EventArgs e)
        {
            if (exp_thread != null)
            {
                if (exp_thread.IsAlive)
                {
                    exp_thread.Abort();
                }
            }

            UpdateValues();

            bits = Bits.FromString(Bits_param.Text);
            changed_bits = Sequences.GoldTransform(bits, gold);

            GetSignals();

            success = new double[M + 1];
            SNR_values = new double[M + 1];
            for (int i = 0; i < SNR_values.Length; i++)
            {
                SNR_values[i] = minSNR + (maxSNR - minSNR) * i / (double)M;
            }

            ExpThread();
            //exp_thread.Start();

            chart_Experiment.Series[0].Points.Clear();
            for (int i = 0; i < M + 1; i++)
            {
                chart_Experiment.Series[0].Points.AddXY(SNR_values[i], success[i]);
            }
        }
        public void ExpThread()
        {
            int right_bits = 0;
            int total_bits = bits.Length * N;
            double local_success;
            double progress = 0;

            /// Эксперимент
            for (int i = 0; i < N; i++)
            {
                for (int snr = 0; snr < M + 1; snr++)
                {
                    NoiseSignals(SNR_values[snr]);
                    right_bits = Experiment(true);
                    local_success = (double)right_bits / total_bits;
                    success[snr] += local_success;
                    progress += 1.0 / N / (M + 1);
                }
            }
        }
        private void CountsPerBit_param_TextChanged(object sender, EventArgs e)
        {
            try
            {
                CountsPerBit = Convert.ToInt32(CountsPerBit_param.Text);
                Bitrate_param.Text = ((int)(Convert.ToDouble(DFreq_param.Text) / CountsPerBit)).ToString();
            }
            catch (FormatException)
            {
                ;
            }
        }

        private void DFreq_param_TextChanged(object sender, EventArgs e)
        {
            try
            {
                CountsPerBit = Convert.ToInt32(CountsPerBit_param.Text);
                Bitrate_param.Text = ((int)(Convert.ToDouble(DFreq_param.Text) / CountsPerBit)).ToString();
            }
            catch (FormatException)
            {
                ;
            }
        }

        public void DrawI()
        {
            chart_ComponentI.Series[0].Points.Clear();
            if (x == null || I == null)
                chart_ComponentI.Series[0].Points.Clear();
            else
            {
                for(int i = 0; i < I.Length; i++)
                {
                    chart_ComponentI.Series[0].Points.AddXY(x[i], I[i]);
                }
            }
        }

        public void DrawQ()
        {
            chart_componentQ.Series[0].Points.Clear();
            if (x == null || Q == null)
                chart_componentQ.Series[0].Points.Clear();
            else
            {
                for (int i = 0; i < Q.Length; i++)
                {
                    chart_componentQ.Series[0].Points.AddXY(x[i], Q[i]);
                }
            }
        }

       


        public void DrawIQ()
        {
            chart_IQ.Series[0].Points.Clear();
            if (I == null || Q == null)
                chart_IQ.Series[0].Points.Clear();
            else
            {
                for(int i = 0; i < Q.Length; i++)
                {
                    chart_IQ.Series[0].Points.AddXY(Q[i], I[i]);
                }
            }
        }
        public void DrawS()
        {
            chart_Complex.Series[0].Points.Clear();
            if (x == null || s == null)
                chart_Complex.Series[0].Points.Clear();
            else
            {
                for (int i = 0; i < s.Length; i++)
                {
                    chart_Complex.Series[0].Points.AddXY(x[i], s[i]);
                }
            }
        }
        public void DrawCorrelation()
        {
            char_Corr.Series[0].Points.Clear();
            char_Corr.Series[1].Points.Clear();
            char_Corr.Series[2].Points.Clear();
            char_Corr.Series[3].Points.Clear();

            if (bits != null)
            {
                for(int i = 0; i< x.Length; i++)
                {
                    for(int l = 0; l < decs.Length; l++)
                    {
                        char_Corr.Series[l].Points.AddXY(x[i], decs[l][i]);
                    }
                }

            }
            else
            {
                char_Corr.Series[0].Points.Clear();
                char_Corr.Series[1].Points.Clear();
                char_Corr.Series[2].Points.Clear();
                char_Corr.Series[3].Points.Clear();
            }
        }
        public double ToDouble(TextBox TB)
        {
            return Convert.ToDouble(TB.Text);
        }

        public int ToInt(TextBox TB)
        {
            return Convert.ToInt32(TB.Text);
        }
    }
}
