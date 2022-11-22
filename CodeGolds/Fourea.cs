using System;
using System.Numerics;

namespace CodeGolds
{
    static class Fourea
    {
        public static Complex[] FFT(Complex[] spectr, int n, int direct)
        {
            Complex buf;
            int i, j, istep;
            int m, mmax;
            double r, r1, theta, w_r, w_i, temp_r, temp_i;
            double pi = 3.1415926f;

            r = pi * direct;
            j = 0;
            for (i = 0; i < n; i++)
            {
                if (i < j)
                {
                    temp_r = spectr[j].Real;
                    temp_i = spectr[j].Imaginary;
                    buf = new Complex(spectr[i].Real, spectr[i].Imaginary);
                    spectr[j] = buf;
                    buf = new Complex(temp_r, temp_i);
                    spectr[i] = buf;
                }
                m = n >> 1;
                while (j >= m) { j -= m; m = (m + 1) / 2; }
                j += m;
            }

            mmax = 1;
            while (mmax < n)
            {
                istep = mmax << 1;
                r1 = r / (float)mmax;
                for (m = 0; m < mmax; m++)
                {
                    theta = r1 * m;
                    w_r = (float)Math.Cos((double)theta);
                    w_i = (float)Math.Sin((double)theta);
                    for (i = m; i < n; i += istep)
                    {
                        j = i + mmax;
                        temp_r = w_r * spectr[j].Real - w_i * spectr[j].Imaginary;
                        temp_i = w_r * spectr[j].Imaginary + w_i * spectr[j].Real;
                        buf = new Complex(spectr[i].Real - temp_r, spectr[i].Imaginary - temp_i);
                        spectr[j] = buf;
                        buf = new Complex(spectr[i].Real + temp_r, spectr[i].Imaginary + temp_i);
                        spectr[i] = buf;
                    }
                }
                mmax = istep;
            }

            if (direct > 0)
            {
                for (i = 0; i < n; i++)
                {
                    buf = new Complex(spectr[i].Real / (float)n, spectr[i].Imaginary / (float)n);
                    spectr[i] = buf;
                }
            }

            return spectr;
        }
    }
}
