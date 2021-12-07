using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public class MandelbrotCS : MonoBehaviour
    {
        // GUI elements:
        public TextMeshProUGUI _minReal, _maxReal, _minImag, _maxImag, _iterationDisp;

        // Calculation variables
        private double _widthInitial, _heightInitial;
        private double _width, _height;
        private double _rStart, _iStart;
        int _maxIterations, _increment;
        double _zoomRate = 1.05, _zoomAmount = 0.0;
        double _panSpeed = 1.0;

        double _nextIterationSwitch = 0.0;
        
        // Compute shader variables
        public ComputeShader _shader;
        private ComputeBuffer _buffer;
        private RenderTexture _texture;
        public RawImage _image;

        // Data for the compute buffer
        public struct DataStruct
        {
            public double r, i, widthRatio, heightRatio;
        }

        private DataStruct[] _data;

        // Start is called before the first frame update
        void Start()
        {
            _heightInitial = _height = 4.2;
            _iStart = -(_height / 2);

            _widthInitial = _width = _height * Screen.width / Screen.height;
            _rStart = -(_width / 2);

            _maxIterations = 1024;
            _increment = 3;

            _data = new DataStruct[1];

            _data[0] = new DataStruct()
            {
                r = _rStart,
                i = _iStart,
                widthRatio = _width / Screen.width,
                heightRatio = _height / Screen.height,
            };

            _buffer = new ComputeBuffer(_data.Length, 32);  // 4 doubles @ 8 bytes each
            _texture = new RenderTexture(Screen.width, Screen.height, 0)
            {
                enableRandomWrite = true
            };
            _texture.Create();

            UpdateTexture();
        }

        // Update is called once per frame
        void Update()
        {
            var acceptedInput = false;
            double steerX = 0.0, steerY = 0.0;


            if (Input.GetKeyDown(KeyCode.Space))
            {
                ResetZoom();
                acceptedInput = true;
            }

            if (Input.GetKey(KeyCode.W))
            {
                steerY += _panSpeed;
                acceptedInput = true;
            }

            if (Input.GetKey(KeyCode.S))
            {
                steerY -= _panSpeed;
                acceptedInput = true;
            }

            if (Input.GetKey(KeyCode.A))
            {
                steerX -= _panSpeed;
                acceptedInput = true;
            }

            if (Input.GetKey(KeyCode.D))
            {
                steerX += _panSpeed;
                acceptedInput = true;
            }

            UpdatePan(steerX, steerY);

            if (Input.GetKeyDown(KeyCode.X))
            {
                if (_maxIterations < 1024)
                    _maxIterations++;
                acceptedInput = true;
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (_maxIterations > 0 )
                    _maxIterations--;
                acceptedInput = true;
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                if (_nextIterationSwitch == 0)
                {
                    _maxIterations = 0;
                    _nextIterationSwitch = 1024;
                }
                else
                {
                    _maxIterations = 1024;
                    _nextIterationSwitch = 0;
                }
                acceptedInput = true;
            }

            if (Input.GetMouseButton(0))
            {
                ZoomIn();
                acceptedInput = true;
            }

            if (Input.GetMouseButton(1))
            {
                ZoomOut();
                acceptedInput = true;
            }

            if (Input.GetMouseButtonDown(2))
            {
                CenterScreen();
                acceptedInput = true;
            }

            if (acceptedInput)
                UpdateTexture();
        }

        private void UpdatePan(double steerX, double steerY)
        {
            _rStart += steerX / System.Math.Pow(2.0, _zoomAmount) * Time.deltaTime;
            _iStart += steerY / System.Math.Pow(2.0, _zoomAmount) * Time.deltaTime;

            _data[0].r = _rStart;
            _data[0].i = _iStart;
        }

        void CenterScreen()
        {
            _rStart += (Input.mousePosition.x - (Screen.width / 2.0)) / Screen.width * _width;
            _iStart += (Input.mousePosition.y - (Screen.height / 2.0)) / Screen.height* _height;

            _data[0].r = _rStart;
            _data[0].i = _iStart;

            //UpdateTexture();
        }

        void ZoomIn()
        {
            _zoomAmount += _zoomRate * Time.deltaTime;

            UpdateZoom();
        }

        void ZoomOut()
        {
            _zoomAmount -= _zoomRate * Time.deltaTime;
            if (_zoomAmount < 0.0)
                _zoomAmount = 0.0;

            UpdateZoom();
        }

        private void ResetZoom()
        {
            _height = _heightInitial;
            _iStart = -(_height / 2);

            _width = _widthInitial;
            _rStart = -(_width / 2);

            _zoomAmount = 0.0;

            UpdateZoom();

            //UpdateTexture();
        }

        private void UpdateZoom()
        {
            var xCenter = _rStart + _width / 2.0;
            var yCenter = _iStart + _height / 2.0;

            _width = _widthInitial / System.Math.Pow(2.0, _zoomAmount);
            _height = _heightInitial / System.Math.Pow(2.0, _zoomAmount);

            _rStart = xCenter - _width / 2.0;
            _iStart = yCenter - _height / 2.0;

            //_maxIterations = Mathf.Max(100, _maxIterations + (int)(_increment * (_zoomAmount - 1.0)));

            _data[0].r = _rStart;
            _data[0].i = _iStart;
            _data[0].widthRatio = _width / Screen.width;
            _data[0].heightRatio = _height / Screen.height;

            //UpdateTexture();
        }

        void UpdateTexture()
        {
            int kernelHandle = _shader.FindKernel("CSMain");

            _buffer.SetData(_data);
            _shader.SetBuffer(kernelHandle, "buffer", _buffer);

            _shader.SetInt("iterations", _maxIterations);
            _shader.SetTexture(kernelHandle, "Result", _texture);

            _shader.Dispatch(kernelHandle, Screen.width / 32, Screen.height / 32, 1);

            RenderTexture.active = _texture;
            _image.material.mainTexture = _texture;

            _minReal.text = _rStart.ToString();
            _maxReal.text = (_rStart + _width).ToString();
            _minImag.text = _iStart.ToString();
            _maxImag.text = (_iStart + _height).ToString();
            _iterationDisp.text = _maxIterations.ToString();
        }

        private void OnDestroy()
        {
            _buffer.Dispose();
        }
    }
}