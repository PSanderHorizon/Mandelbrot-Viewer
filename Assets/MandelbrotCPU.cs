using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;

public class MandelbrotCPU : MonoBehaviour
{
    private double _height, _width;
    private double _rStart, _iStart;
    private int _maxIterations;

    private int _zoom;

    private Texture2D _display;

    private Assets.Gradient _colors;

    public Image _image;
    
    // Start is called before the first frame update

    void Start()
    {
        _width = 4.5;
        _height = _width * Screen.height / Screen.width;
        _rStart = -2.0;
        _iStart = -1.25;

        _maxIterations = 100;
        _zoom = 10;

        var black = new UnityEngine.Vector4(0, 0, 0, 255);
        var red= new UnityEngine.Vector4(255, 0, 0, 255);
        var green = new UnityEngine.Vector4(0, 255, 0, 255);
        var blue = new UnityEngine.Vector4(0, 0, 255, 255);
        var cyan = new UnityEngine.Vector4(0, 255, 255, 255);
        var white = new UnityEngine.Vector4(255, 255, 255, 255);
        var list = new List<UnityEngine.Vector4>
        {
            black,
            blue,
            cyan,
            red,
            green,
            white
        };
        _colors = new Assets.Gradient(list, 16);

        _display = new Texture2D(Screen.width, Screen.height);
        if (_display == null) Debug.Log("START: _display is snull somehow.");
        RunMandelbrot();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            _rStart = _rStart + (Input.mousePosition.x - (Screen.width / 2.0)) / Screen.width * _width;
            _iStart = _iStart + (Input.mousePosition.y - (Screen.height / 2.0)) / Screen.height * _height;
            RunMandelbrot();
        }

        if (Input.mouseScrollDelta.y != 0.0)
        {
            var wFactor = _width * (double)Input.mouseScrollDelta.y / _zoom;
            var hFactor = _height * (double)Input.mouseScrollDelta.y / _zoom;
            _width -= wFactor;
            _height -= hFactor;
            _rStart += wFactor / 2.0;
            _iStart += hFactor / 2.0;

            RunMandelbrot();
        }
    }

    public void RunMandelbrot()
    {
        if (_display == null) Debug.Log("DISPLAY IS NULL");
        for(var x = 0; x != _display.width; x++)
        for (var y = 0; y != _display.height; y++)
        {
            var real = _rStart + _width * x / _display.width;
            var imag = _iStart + _height * y / _display.height;
            var color = _colors[MandelbrotFunction(real, imag) % 16];
            _display.SetPixel(x, y, color);
        }

        _display.Apply();
        _image.sprite = Sprite.Create(_display, new Rect(0, 0, _display.width, _display.height),
            new UnityEngine.Vector2(0.5f, 0.5f));
    }

    private int MandelbrotFunction(double x, double y)
    {
        var z = new Complex(0, 0);
        var c = new Complex(x, y);

        for (int i = 1; i <= _maxIterations; i++)
        {
            z = Complex.Pow(z, 2) + c;
            if (Complex.Abs(z) > 2)
                return i;
        }
        
        // If we never diverge to infinity, then just return 2.
        return 0;
    }
}
