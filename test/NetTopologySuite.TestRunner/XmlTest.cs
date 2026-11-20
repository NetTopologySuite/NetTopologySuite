using System;
using System.Diagnostics;
using System.Globalization;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;
using Open.Topology.TestRunner.Operations;
using Open.Topology.TestRunner.Result;

namespace Open.Topology.TestRunner
{
    using NetTopologySuite.Precision;

    #region Test Event Definitions

    public class XmlTestEventArgs : EventArgs
    {
        private readonly int _nIndex      = -1;
        private readonly bool _bSuccess;
        private readonly XmlTest _objTest;

        public XmlTestEventArgs(int index, bool success, XmlTest testItem)
        {
            _nIndex   = index;
            _bSuccess = success;
            _objTest  = testItem;
        }

        public int Index => _nIndex;

        public bool Success => _bSuccess;

        public XmlTest Test => _objTest;
    }

    public delegate void XmlTextEventHandler(object sender, XmlTestEventArgs args);

    #endregion

    /// <summary>
    /// Summary description for XmlTest.
    /// </summary>
    public class XmlTest
    {
        private static NumberFormatInfo _nfi;

        protected static IFormatProvider GetNumberFormatInfo()
        {
            if (_nfi == null)
            {
                _nfi = NumberFormatInfo.InvariantInfo;
            }
            return _nfi;
        }

        #region Private Members


        private bool    _bIsDefaultTarget = true;

        private Exception _objException;

        private bool      _bSuccess;
        private IResult   _objExpectedResult;

        private Geometry  _objGeometryA;
        private Geometry  _objGeometryB;

        private object    _objArgument1;
        private object    _objArgument2;

        private string _opName = string.Empty;

        private string    _strDescription;

        private readonly double    _dTolerance     = double.Epsilon;

        private IResultMatcher _resultMatcher;
        private readonly IGeometryOperation _geometryOperation;
        #endregion

        #region Constructors and Destructor

        public XmlTest(string description, bool bIsDefaultTarget, double tolerance, IGeometryOperation geometryOperation, IResultMatcher resultMatcher)
        {
            // A case without a description stays without one, and the runner names it by its
            // position. Numbering them from a counter here made the name depend on how many
            // nameless cases had been built before, so the same case came out under different
            // names from one load to the next.
            _strDescription = description ?? string.Empty;

            _bIsDefaultTarget = bIsDefaultTarget;
            _dTolerance       = tolerance;
            _geometryOperation = geometryOperation;
            _resultMatcher = resultMatcher;
        }

        #endregion

        #region Public Properties

        public string Description
        {
            get => _strDescription;

            set => _strDescription = value;
        }

        public Exception Thrown
        {
            get => _objException;

            set => _objException = value;
        }

        public bool Success => _bSuccess;

        public Geometry A
        {
            get => _objGeometryA;

            set => _objGeometryA = value;
        }

        public Geometry B
        {
            get => _objGeometryB;

            set => _objGeometryB = value;
        }

        /// <summary>The name of the operation, as the case writes it.</summary>
        public string Operation
        {
            get => _opName;

            set => _opName = value;
        }

        /// <summary>The result the case expects, already in the shape the operation returns.</summary>
        public IResult ExpectedResult
        {
            get => _objExpectedResult;

            set => _objExpectedResult = value;
        }

        public object Argument1
        {
            get => _objArgument1;

            set => _objArgument1 = value;
        }

        public object Argument2
        {
            get => _objArgument2;

            set => _objArgument2 = value;
        }

        public bool IsDefaultTarget
        {
            get => _bIsDefaultTarget;

            set => _bIsDefaultTarget = value;
        }

        #endregion

        #region Public Methods
        public bool Run()
        {
            var c = Console.ForegroundColor;
            try
            {
                _bSuccess = RunTest();
                if (!_bSuccess)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    // DEBUG ERRORS: retry to launch the test and analyze...
                    Console.WriteLine("\n*********************************************");

                    Console.WriteLine("Encountered failing test!");
                    Console.WriteLine("Description: '{0}'", Description);
                    Console.WriteLine("A: {0}", A?.AsText() ?? "null");
                    Console.WriteLine("B: {0}", B?.AsText() ?? "null");
                    Console.WriteLine("Test method: '{0}'", Operation);
                    Console.WriteLine("arg1 {0}", Argument1);
                    Console.WriteLine("arg2 {0}", Argument2);

                    Console.WriteLine("Result expected is {0}, but was {1}", true, _bSuccess);
                    Console.WriteLine("*********************************************\n");

                }
                return _bSuccess;
            }
            catch (Exception ex)
            {
                _objException = ex;
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                XmlTestExceptionManager.Publish(ex);
                return false;
            }
            finally
            {
                Console.ForegroundColor = c;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Runs the operation this case names and compares the answer with the result the case
        /// expects, which is the whole of what the upstream runner does for every case.
        /// </summary>
        public virtual bool RunTest()
        {
            try
            {
                if (_geometryOperation == null)
                {
                    throw new InvalidOperationException(
                        "A case has no geometry operation to run. The document supplies the default one, "
                        + "so reaching here means a case was built without it.");
                }

                object[] arguments = ToArguments();
                var target = IsDefaultTarget ? _objGeometryA : _objGeometryB;

                var actualResult = _geometryOperation.Invoke(_opName, target, arguments);

                // A case that states no expected result asserts only that the operation runs, so
                // having run it there is nothing left to compare.
                if (_objExpectedResult == null)
                    return true;

                if (_resultMatcher == null)
                    _resultMatcher = CreateEqualityResultMatcher(_geometryOperation.GetReturnType(_opName));

                return _resultMatcher.IsMatch(target, _opName, arguments,
                                              actualResult, _objExpectedResult, _dTolerance);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test '{Description}' threw exception!");
                Console.WriteLine($"Test type: {Operation}");
                Console.WriteLine($"A: {A}");
                Console.WriteLine($"B: {B?.ToString() ?? new string('-',3)}");
                if (!(ReferenceEquals(Argument1, A) || ReferenceEquals(Argument1, B)) && Argument1 != null)
                    Console.WriteLine($"Argument1: {Argument1}");
                if (!(ReferenceEquals(Argument2, A) || ReferenceEquals(Argument2, B)) && Argument2 != null)
                    Console.WriteLine($"Argument2: {Argument2}");
                Console.WriteLine($"Tolerance: {_dTolerance}");
                Console.WriteLine($"Exception:\n{ex.Message}");
                Console.WriteLine($"Stack trace:\n{ex.StackTrace}");

                // Set the exception thrown
                Thrown = ex;
                //Console.WriteLine(ToString());
                //throw;
                return false;
            }
        }

        private IResultMatcher CreateEqualityResultMatcher(Type returnType)
        {
            if (returnType == typeof(int))
                return new EqualityResultMatcher<IntegerResult>();
            if (returnType == typeof(bool))
                return new EqualityResultMatcher<BooleanResult>();
            if (returnType == typeof(double))
                return new EqualityResultMatcher<DoubleResult>();
            if (typeof(Geometry).IsAssignableFrom(returnType))
                return new EqualityResultMatcher<GeometryResult>();

            Debug.Assert(false);
            return null;
        }

        private object[] ToArguments()
        {
            var ret = new System.Collections.Generic.List<object>(2);
            object o = ToGeometryOrString(Argument1);
            if (o != null) ret.Add(o);
            o = ToGeometryOrString(Argument2);
            if (o != null) ret.Add(o);

            return ret.ToArray();
        }

        private object ToGeometryOrString(object o)
        {
            if (o == null)
                return null;

            if (o is Geometry)
                return o;

            if (o is string)
            {
                string a = (string) o;
                if (a == "A" || a == "a")
                    return A;
                if (a == "B" || a == "b")
                    return B;

                return a;
            }

            return o.ToString();
        }

        #endregion

        #region Private Members

        #endregion
    }
}
