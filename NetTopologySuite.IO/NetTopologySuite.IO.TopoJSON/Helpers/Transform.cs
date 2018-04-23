﻿using System;

namespace NetTopologySuite.IO.Helpers
{
    public class Transform : ITransform
    {
        private static readonly double[] NoScale = { 1, 1 };
        private static readonly double[] NoTranslate = { 0, 0 };

        private readonly double[] _scale;
        private readonly double[] _translate;

        public Transform() : this(NoScale, NoTranslate) { }

        public Transform(double[] scale, double[] translate)
        {
            if (scale == null)
                throw new ArgumentNullException("scale");
            if (translate == null)
                throw new ArgumentNullException("translate");

            _scale = scale;
            _translate = translate;
        }

        public bool Quantized => Scale != NoScale && Translate != NoTranslate;

        public double[] Scale => _scale;

        public double[] Translate => _translate;

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return string.Format("Scale: {0}, Translate: {1}", Scale, Translate);
        }
    }
}