using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Various
{
    [TestFixture, Explicit("missing input files in folder 'VAM\\VAM_ikk'")]
    public class AffineTransformBuilderUsage
    {
        /// <summary>
        /// Calculates Affine and Helmert transformation using Least-Squares Regression of input and output points
        /// </summary>
        public class LeastSquaresTransform
        {
            private readonly List<Coordinate> _inputs;
            private readonly List<Coordinate> _outputs;

            /// <summary>
            /// Initialize Least Squares transformations
            /// </summary>
            public LeastSquaresTransform()
            {
                _inputs = new List<Coordinate>();
                _outputs = new List<Coordinate>();
            }

            /// <summary>
            /// Adds an input and output value pair to the collection
            /// </summary>
            /// <param name="input">The input coordinate</param>
            /// <param name="output">The output coordinate</param>
            public void AddInputOutputPoint(Coordinate input, Coordinate output)
            {
                _inputs.Add(input);
                _outputs.Add(output);
            }

            /// <summary>
            /// Removes input and output value pair at the specified index
            /// </summary>
            /// <param name="i"></param>
            public void RemoveInputOutputPointAt(int i)
            {
                _inputs.RemoveAt(i);
                _outputs.RemoveAt(i);
            }

            /// <summary>
            /// Gets the input point value at the specified index
            /// </summary>
            /// <param name="i">index</param>
            /// <returns>Input point value a index 'i'</returns>
            public Coordinate GetInputPoint(int i)
            {
                return _inputs[i];
            }

            /// <summary>
            /// Sets the input point value at the specified index
            /// </summary>
            /// <param name="p">Point value</param>
            /// <param name="i">index</param>
            public void SetInputPointAt(Coordinate p, int i)
            {
                _inputs[i] = p;
            }

            /// <summary>
            /// Gets the output point value at the specified index
            /// </summary>
            /// <param name="i">index</param>
            /// <returns>Output point value a index 'i'</returns>
            public Coordinate GetOutputPoint(int i)
            {
                return _outputs[i];
            }

            /// <summary>
            /// Sets the output point value at the specified index
            /// </summary>
            /// <param name="p">Point value</param>
            /// <param name="i">index</param>
            public void SetOutputPointAt(Coordinate p, int i)
            {
                _outputs[i] = p;
            }

            private static Coordinate MeanCoordinate(IEnumerable<Coordinate> coordinates)
            {
                Coordinate mean = new Coordinate(0, 0);
                int count = 0;

                foreach (Coordinate coordinate in coordinates)
                {
                    mean.X += coordinate.X;
                    mean.Y += coordinate.Y;
                    count++;
                }

                mean.X = Math.Round(mean.X/count, MidpointRounding.AwayFromZero);
                mean.Y = Math.Round(mean.Y/count, MidpointRounding.AwayFromZero);
                
                return mean;
            }

            /// <summary>
            /// Return an array with the six affine transformation parameters {a,b,c,d,e,f} and the sum of the squares of the residuals (s0)
            /// </summary>
            /// <remarks>
            /// a,b defines scale vector 1 of coordinate system, d,e scale vector 2. c,f defines offset.
            /// <para>
            /// Converting from input (X,Y) to output coordinate system (X',Y') is done by:
            /// X' = a*X + b*Y + c, Y' = d*X + e*Y + f
            /// </para>
            /// <para>
            /// Transformation based on Mikhail "Introduction to Modern Photogrammetry" p. 399-300.
            /// Extended to arbitrary number of measurements by M. Nielsen
            /// </para>
            /// </remarks>
            /// <returns>Array with the six transformation parameters and sum of squared residuals:  a,b,c,d,e,f,s0</returns>
            public double[] GetAffineTransformation()
            {
                if (_inputs.Count < 3)
                    throw (new Exception("At least 3 measurements required to calculate affine transformation"));

                //double precision isn't always enough when transforming large numbers.
                //Lets subtract some mean values and add them later again:
                //Find approximate center values:
                Coordinate meanInput = MeanCoordinate(_inputs);
                Coordinate[] inputs = ToMeanArray(_inputs, meanInput);
                Coordinate meanOutput = MeanCoordinate(_outputs);
                Coordinate[] outputs = ToMeanArray(_outputs, meanOutput);

                double[][] matrix = CreateMatrix(3, 3);
                //Create normal equation: transpose(B)*B
                //B: matrix of calibrated values. Example of row in B: [x , y , -1]
                for (int i = 0; i < inputs.Length; i++)
                {
                    //Calculate summed values
                    matrix[0][0] += Math.Pow(inputs[i].X, 2);
                    matrix[0][1] += inputs[i].X * inputs[i].Y;
                    matrix[0][2] += -inputs[i].X;
                    matrix[1][1] += Math.Pow(inputs[i].Y, 2);
                    matrix[1][2] += -inputs[i].Y;
                }
                matrix[2][2] = inputs.Length;

                double[] t1 = new double[3];
                double[] t2 = new double[3];

                for (int i = 0; i < _inputs.Count; i++)
                {
                    t1[0] += inputs[i].X * outputs[i].X;
                    t1[1] += inputs[i].Y * outputs[i].X;
                    t1[2] += -outputs[i].X;

                    t2[0] += inputs[i].X * outputs[i].Y;
                    t2[1] += inputs[i].Y * outputs[i].Y;
                    t2[2] += -outputs[i].Y;
                }
                double[] trans = new double[7];
                // Solve equation N = transpose(B)*t1
                double frac = 1d /
                              (-matrix[0][0] * matrix[1][1] * matrix[2][2] + matrix[0][0] * Math.Pow(matrix[1][2], 2) + Math.Pow(matrix[0][1], 2) * matrix[2][2] -
                               2 * matrix[1][2] * matrix[0][1] * matrix[0][2] + matrix[1][1] * Math.Pow(matrix[0][2], 2));
                trans[0] = (-matrix[0][1] * matrix[1][2] * t1[2] + matrix[0][1] * t1[1] * matrix[2][2] - matrix[0][2] * matrix[1][2] * t1[1] + matrix[0][2] * matrix[1][1] * t1[2] -
                            t1[0] * matrix[1][1] * matrix[2][2] + t1[0] * Math.Pow(matrix[1][2], 2)) * frac;
                trans[1] = (-matrix[0][1] * matrix[0][2] * t1[2] + matrix[0][1] * t1[0] * matrix[2][2] + matrix[0][0] * matrix[1][2] * t1[2] - matrix[0][0] * t1[1] * matrix[2][2] -
                            matrix[0][2] * matrix[1][2] * t1[0] + Math.Pow(matrix[0][2], 2) * t1[1]) * frac;
                trans[2] =
                    -(-matrix[1][2] * matrix[0][1] * t1[0] + Math.Pow(matrix[0][1], 2) * t1[2] + matrix[0][0] * matrix[1][2] * t1[1] - matrix[0][0] * matrix[1][1] * t1[2] -
                      matrix[0][2] * matrix[0][1] * t1[1] + matrix[1][1] * matrix[0][2] * t1[0]) * frac;
                trans[2] += -meanOutput.X + meanInput.X;
                // Solve equation N = transpose(B)*t2
                trans[3] = (-matrix[0][1] * matrix[1][2] * t2[2] + matrix[0][1] * t2[1] * matrix[2][2] - matrix[0][2] * matrix[1][2] * t2[1] + matrix[0][2] * matrix[1][1] * t2[2] -
                            t2[0] * matrix[1][1] * matrix[2][2] + t2[0] * Math.Pow(matrix[1][2], 2)) * frac;
                trans[4] = (-matrix[0][1] * matrix[0][2] * t2[2] + matrix[0][1] * t2[0] * matrix[2][2] + matrix[0][0] * matrix[1][2] * t2[2] - matrix[0][0] * t2[1] * matrix[2][2] -
                            matrix[0][2] * matrix[1][2] * t2[0] + Math.Pow(matrix[0][2], 2) * t2[1]) * frac;
                trans[5] =
                    -(-matrix[1][2] * matrix[0][1] * t2[0] + Math.Pow(matrix[0][1], 2) * t2[2] + matrix[0][0] * matrix[1][2] * t2[1] - matrix[0][0] * matrix[1][1] * t2[2] -
                      matrix[0][2] * matrix[0][1] * t2[1] + matrix[1][1] * matrix[0][2] * t2[0]) * frac;
                trans[5] += -meanOutput.Y + meanInput.Y;



                //Calculate s0
                double s0 = 0;
                for (int i = 0; i < _inputs.Count; i++)
                {
                    double x = inputs[i].X * trans[0] + inputs[i].Y * trans[1] + trans[2];
                    double y = inputs[i].X * trans[3] + inputs[i].Y * trans[4] + trans[5];
                    s0 += Math.Pow(x - outputs[i].X, 2) + Math.Pow(y - outputs[i].Y, 2);
                }
                trans[6] = Math.Sqrt(s0) / (_inputs.Count);
                return trans;
            }

            /// <summary>
            /// Calculates the four helmert transformation parameters {a,b,c,d} and the sum of the squares of the residuals (s0)
            /// </summary>
            /// <remarks>
            /// <para>
            /// a,b defines scale vector 1 of coordinate system, d,e scale vector 2.
            /// c,f defines offset.
            /// </para>
            /// <para>
            /// Converting from input (X,Y) to output coordinate system (X',Y') is done by:
            /// X' = a*X + b*Y + c, Y' = -b*X + a*Y + d
            /// </para>
            /// <para>This is a transformation initially based on the affine transformation but slightly simpler.</para>
            /// </remarks>
            /// <returns>Array with the four transformation parameters, and sum of squared residuals: a,b,c,d,s0</returns>
            public double[] GetHelmertTransformation()
            {
                if (_inputs.Count < 2)
                    throw (new Exception("At least 2 measurements required to calculate helmert transformation"));

                //double precision isn't always enough. Lets subtract some mean values and add them later again:
                //Find approximate center values:
                Coordinate meanInput = MeanCoordinate(_inputs);
                Coordinate[] inputs = ToMeanArray(_inputs, meanInput);
                Coordinate meanOutput = MeanCoordinate(_outputs);
                Coordinate[] outputs = ToMeanArray(_outputs, meanOutput);

                double b00 = 0d;
                double b02 = 0d;
                double b03 = 0d;
                double[] t = new double[4];
                for (int i = 0; i < inputs.Length; i++)
                {
                    //Calculate summed values
                    b00 += Math.Pow(inputs[i].X, 2) + Math.Pow(inputs[i].Y, 2);
                    b02 -= inputs[i].X;
                    b03 -= inputs[i].Y;
                    t[0] += -(inputs[i].X * outputs[i].X) - (inputs[i].Y * outputs[i].Y);
                    t[1] += -(inputs[i].Y * outputs[i].X) + (inputs[i].X * outputs[i].Y);
                    t[2] += outputs[i].X;
                    t[3] += outputs[i].Y;
                }
                double frac = 1d / (-inputs.Length * b00 + Math.Pow(b02, 2) + Math.Pow(b03, 2));
                double[] result = new double[5];
                result[0] = (-inputs.Length * t[0] + b02 * t[2] + b03 * t[3]) * frac;
                result[1] = (-inputs.Length * t[1] + b03 * t[2] - b02 * t[3]) * frac;
                result[2] = (b02 * t[0] + b03 * t[1] - t[2] * b00) * frac + meanOutput.X;
                result[3] = (b03 * t[0] - b02 * t[1] - t[3] * b00) * frac + meanOutput.Y;

                //Calculate s0
                double s0 = 0d;
                for (int i = 0; i < inputs.Length; i++)
                {
                    double x = inputs[i].X * result[0] + inputs[i].Y * result[1] + result[2];
                    double y = -inputs[i].X * result[1] + inputs[i].Y * result[0] + result[3];
                    s0 += Math.Pow(x - outputs[i].X, 2) + Math.Pow(y - outputs[i].Y, 2);
                }
                result[4] = Math.Sqrt(s0) / (_inputs.Count);
                return result;
            }

            private static Coordinate[] ToMeanArray(List<Coordinate> coordinates, Coordinate mean)
            {
                Coordinate[] res = new Coordinate[coordinates.Count];
                for (int i = 0; i < coordinates.Count; i++)
                {
                    res[i] = new Coordinate(coordinates[i].X - mean.X,
                                            coordinates[i].Y - mean.Y);
                }
                return res;
            }

            /// <summary>
            /// Creates an n x m matrix of doubles
            /// </summary>
            /// <param name="n">width of matrix</param>
            /// <param name="m">height of matrix</param>
            /// <returns>n*m matrix</returns>
            private static double[][] CreateMatrix(int n, int m)
            {
                double[][] matrix = new double[n][];
                for (int i = 0; i < n; i++)
                {
                    matrix[i] = new double[m];
                }
                return matrix;
            }
        }

        
        private string _currentDirectory;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _currentDirectory = Environment.CurrentDirectory; 
            Environment.CurrentDirectory = @"D:\temp\VAM\VAM_ikk";
        }

        [Test]
        public void Test()
        {
            AffineTransformation at = ReadKnotenListe("Knoten_ikk.liste");
            ApplyTransform("VAM_IVA0_ikk.csv", at);
        }

        [Test]
        public void TestLsq()
        {
            AffineTransformation at = ReadKnotenListeLsq("Knoten_ikk.liste");
            ApplyTransform("VAM_IVA0_ikk.csv", at, "_neu_lsq");
        }

        private static AffineTransformation ReadKnotenListe(string file)
        {
            int index = 0;
            Coordinate[] src = new Coordinate[3];
            Coordinate[] dst = new Coordinate[3];
            using (StreamReader sr = new StreamReader(File.OpenRead(file)))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    if (line.StartsWith("#")) continue;

                    src[index] = new Coordinate(double.Parse(line.Substring(15, 11), NumberFormatInfo.InvariantInfo),
                                                double.Parse(line.Substring(26, 11), NumberFormatInfo.InvariantInfo));
                    dst[index] = new Coordinate(double.Parse(line.Substring(37, 11), NumberFormatInfo.InvariantInfo),
                                                double.Parse(line.Substring(48, 11), NumberFormatInfo.InvariantInfo));
                    index++;
                    if (index == 3) break;
                }
            }
            AffineTransformationBuilder atb = new AffineTransformationBuilder(src[0], src[1], src[2], dst[0], dst[1], dst[2]);
            return atb.GetTransformation();
        }

        private static AffineTransformation ReadKnotenListeLsq(string file)
        {
            LeastSquaresTransform lsq = new LeastSquaresTransform();

            using (StreamReader sr = new StreamReader(File.OpenRead(file)))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    if (line.StartsWith("#")) continue;

                    lsq.AddInputOutputPoint(new Coordinate(double.Parse(line.Substring(15, 11), NumberFormatInfo.InvariantInfo),
                                                           double.Parse(line.Substring(26, 11), NumberFormatInfo.InvariantInfo)),
                                            new Coordinate(double.Parse(line.Substring(37, 11), NumberFormatInfo.InvariantInfo),
                                                           double.Parse(line.Substring(48, 11), NumberFormatInfo.InvariantInfo)));
                }
            }

            return new AffineTransformation(lsq.GetAffineTransformation());
        }

        private static void ApplyTransform(string file, AffineTransformation at, string suffix = null)
        {
            if (string.IsNullOrEmpty(suffix)) suffix = "_neu";
            string outFile = Path.GetFileName(file) + suffix + Path.GetExtension(file);
            if (File.Exists(outFile)) File.Delete(outFile);

            Console.WriteLine("Performing transformation using \n{0}", at);
            using (StreamWriter sw = new StreamWriter(File.OpenWrite(outFile)))
            {
                using (StreamReader sr = new StreamReader(File.OpenRead(file)))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            if (!line.StartsWith("#"))
                            {
                                string[] parts = line.Split(',');
                                Coordinate src = new Coordinate(double.Parse(parts[2], NumberFormatInfo.InvariantInfo),
                                                         double.Parse(parts[3], NumberFormatInfo.InvariantInfo));

                                Coordinate dst = new Coordinate();
                                dst = at.Transform(src, dst);
                                parts[2] = dst.X.ToString(NumberFormatInfo.InvariantInfo);
                                parts[3] = dst.Y.ToString(NumberFormatInfo.InvariantInfo);
                                line = string.Join(",", parts);
                            }
                        }
                        sw.WriteLine(line);
                    }
                }
            }

        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}