using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class Gradient
    {
        private readonly int _steps;
        private readonly Vector4[] _colors;

        public Gradient(Vector4 start, Vector4 end, int steps)
        {
            if (steps < 2) throw new ArgumentOutOfRangeException(nameof(steps), "Steps must be >= 2!");
            _steps = steps;

            var rStep = (end.x - start.x) / (steps - 1.0f);
            var gStep = (end.y - start.y) / (steps - 1.0f);
            var bStep = (end.z - start.z) / (steps - 1.0f);
            var aStep = (end.w - start.w) / (steps - 1.0f);

            _colors = new Vector4[steps];
            for (var i = 0; i < steps; i++)
            {
                _colors[i] = new Vector4((start.x + i * rStep)/255.0f, (start.y + i * gStep)/255.0f,
                    (start.z + i * bStep)/255.0f, (start.w + i * aStep)/255.0f);
            }
        }

        public Gradient(List<Vector4> colors, int steps)
        {
            if (colors.Count < 2) throw new ArgumentException("Colors must have more than 2 colors!");
            if (steps < 2) throw new ArgumentOutOfRangeException(nameof(steps), $"Steps must be >= 2! Got {steps}.");

            _steps = steps;
            var count = colors.Count;

            var indexes = new List<(double pos, Vector4 color)>();
            var indexPercentageStep = 1.0 / (count - 1);
            for (var i = 0; i < count; i++)
            {
                indexes.Add((i * indexPercentageStep, colors[i]));
            }
            // Now to generate the colors.
            var stepPercentage = 1.0 / (steps - 1.0);
            _colors = new Vector4[steps];
            var first = 0;
            var second = 1;
            for (var j = 0; j < steps; j++)
            {
                var p = j * stepPercentage;
                // So next would be to find the next two indices of the colors
                // with indexes surrounding p. If we land on p exactly just use
                // that color.
                while (second != indexes.Count &&
                    !(indexes[first].pos <= p && p <= indexes[second].pos))
                {
                    first++; second++;
                }
                if (indexes[first].pos == p)
                    _colors[j] = indexes[first].color;
                else if (indexes[second].pos == p)
                    _colors[j] = indexes[second].color;
                else // We need to interpolate
                {
                    var q = Math.MapRange(p, indexes[first].pos, indexes[second].pos, 0.0, 1.0);

                    var color1 = indexes[first].color;
                    var color2 = indexes[second].color;
                    var r = (color2.x - color1.x) * q + color1.x;
                    var g = (color2.y - color1.y) * q + color1.y;
                    var b = (color2.z - color1.z) * q + color1.z;
                    var a = (color2.w - color1.w) * q + color1.w;
                    _colors[j] = new Vector4((float)r / 255.0f, (float)g / 255.0f,
                        (float) b / 255.0f, (float)a / 255.0f);
                }
            }
        }
        
        public Vector4 this[int i]
        {
            get
            {
                if (i < 0 || i > _steps) throw new ArgumentOutOfRangeException(nameof(i), $"Index expected to be between 0 and {_steps}, inclusive. Got {i}.");
                return _colors[i];
            }
        }
    }
}
