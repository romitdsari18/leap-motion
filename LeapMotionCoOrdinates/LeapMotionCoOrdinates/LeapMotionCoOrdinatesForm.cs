using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Leap;
using System.Drawing.Imaging;

namespace LeapMotionCoOrdinates
{
    public partial class LeapMotionCoOrdinatesForm : Form
    {
        private Controller controller;
        Hand hand;
        double LeapX = 0;
        double LeapY = 0;
        double LeapZ = 0;
        double ScreenX = 0;
        double ScreenY = 0;
        double ScreenZ = 0;
        double _OriginX = 0;
        double _OriginY = 0;
        double _OriginZ = 0;
        VectorClass AxisX;
        VectorClass AxisY;
        VectorClass UnitAxisX;
        VectorClass UnitAxisY;
        VectorClass UnitAxisZ;
        double[,] _orientationScreen = new double[3, 3];
        double[,] _orientationScreenTransposed = new double[3, 3];
        double[,] _transformationInverse = new double[4, 4];

        bool SetOriginTrigger = false;
        bool SetAxisXTrigger = false;
        bool SetAxisYTrigger = false;
        bool isTransformed = false;
        bool OriginSet = true;
        bool AxisXSet = true;
        bool AxisYSet = true;

        public LeapMotionCoOrdinatesForm()
        {
            InitializeComponent();
        }

        private void cmdConnectLeapMotion_Click(object sender, EventArgs e)
        {
            controller = new Controller();

            controller.Connect += Controller_Connect;
            controller.Device += Controller_Device;
            controller.FrameReady += Controller_FrameReady;
        }

        private void Controller_FrameReady(object sender, FrameEventArgs e)
        {
            Frame frame = e.frame;

            if (frame.Hands.Count > 0)
            {
                hand = frame.Hands[0];
                if (hand.Fingers.Count > 0)
                {
                    LeapX = Math.Round(hand.Fingers[1].StabilizedTipPosition.x, 3) * 0.001;
                    LeapY = Math.Round(hand.Fingers[1].StabilizedTipPosition.z, 3) * 0.001;
                    LeapZ = Math.Round(hand.Fingers[1].StabilizedTipPosition.y, 3) * 0.001;
                    handPosX.Text = LeapX.ToString();
                    handPosY.Text = LeapY.ToString();
                    handPosZ.Text = LeapZ.ToString();

                    //confidence
                    float _confidence = hand.Confidence;
                    Confidence.Text = _confidence.ToString();

                    if (SetOriginTrigger)
                    {
                        if (OriginSet) {
                            _OriginX = LeapX;
                            _OriginY = LeapY;
                            _OriginZ = LeapZ;
                            OriginSet = false;
                            //testing

                            //_OriginX = 0;
                            //_OriginY = 0;
                            //_OriginZ = 0.275;
                            OriginX.Text = _OriginX.ToString();
                            OriginY.Text = _OriginY.ToString();
                            OriginZ.Text = _OriginZ.ToString();
                        }
                        if (SetAxisXTrigger)
                        {
                            if (AxisXSet)
                            {
                                double _Axis1X = LeapX;
                                double _Axis1Y = LeapY;
                                double _Axis1Z = LeapZ;
                                //testing

                                //double _Axis1X = 0.2;
                                //double _Axis1Y = 0;
                                //double _Axis1Z = 0.275;
                                AxisX = new VectorClass(_Axis1X - _OriginX, _Axis1Y - _OriginY, _Axis1Z - _OriginZ);
                                UnitAxisX = AxisX.Norm();
                                PAxisX.Text = UnitAxisX.X.ToString();
                                PAxisY.Text = UnitAxisX.Y.ToString();
                                PAxisZ.Text = UnitAxisX.Z.ToString();
                                AxisXSet = false;
                            }
                            if (SetAxisYTrigger)
                            {
                                if (AxisYSet)
                                {
                                    double _Axis2X = LeapX;
                                    double _Axis2Y = LeapY;
                                    double _Axis2Z = LeapZ;
                                    //testing

                                    //double _Axis2X = 0;
                                    //double _Axis2Y = 0;
                                    //double _Axis2Z = 0.175;
                                    AxisY = new VectorClass(_Axis2X - _OriginX, _Axis2Y - _OriginY, _Axis2Z - _OriginZ);
                                    UnitAxisY = AxisY.Norm();
                                    QAxisX.Text = UnitAxisY.X.ToString();
                                    QAxisY.Text = UnitAxisY.Y.ToString();
                                    QAxisZ.Text = UnitAxisY.Z.ToString();
                                    AxisYSet = false;
                                    UnitAxisZ = new VectorClass(UnitAxisX.Y * UnitAxisY.Z - UnitAxisX.Z * UnitAxisY.Y,
                                                            UnitAxisX.Z * UnitAxisY.X - UnitAxisX.X * UnitAxisY.Z,
                                                            UnitAxisX.X * UnitAxisY.Y - UnitAxisX.Y * UnitAxisY.X);

                                    _orientationScreen[0, 0] = UnitAxisX.X;
                                    _orientationScreen[1, 0] = UnitAxisY.X;
                                    _orientationScreen[2, 0] = UnitAxisZ.X;

                                    _orientationScreen[0, 1] = UnitAxisX.Y;
                                    _orientationScreen[1, 1] = UnitAxisY.Y;
                                    _orientationScreen[2, 1] = UnitAxisZ.Y;

                                    _orientationScreen[0, 2] = UnitAxisX.Z;
                                    _orientationScreen[1, 2] = UnitAxisY.Z;
                                    _orientationScreen[2, 2] = UnitAxisZ.Z;
                                    for (int i = 0; i < 3; i++)
                                    {
                                        for (int j = 0; j < 3; j++)
                                        {
                                            _orientationScreenTransposed[i, j] = _orientationScreen[j, i];
                                        }
                                    }
                                    for (int i = 0; i < 3; i++)
                                    {
                                        for (int j = 0; j < 3; j++)
                                        {
                                            _transformationInverse[i, j] = _orientationScreenTransposed[i, j];
                                        }
                                    }
                                    _transformationInverse[3, 0] = -(_orientationScreenTransposed[0, 0] * _OriginX + _orientationScreenTransposed[1, 0] * _OriginY + _orientationScreenTransposed[2, 0] * _OriginZ);
                                    _transformationInverse[3, 1] = -(_orientationScreenTransposed[0, 1] * _OriginX + _orientationScreenTransposed[1, 1] * _OriginY + _orientationScreenTransposed[2, 1] * _OriginZ);
                                    _transformationInverse[3, 2] = -(_orientationScreenTransposed[0, 2] * _OriginX + _orientationScreenTransposed[1, 2] * _OriginY + _orientationScreenTransposed[2, 2] * _OriginZ);
                                    _transformationInverse[0, 3] = _transformationInverse[1, 3] = _transformationInverse[2, 3] = 0;
                                    _transformationInverse[3, 3] = 1;
                                }
                                isTransformed = true;
                                if (isTransformed)
                                {
                                    //testing
                                    //LeapX = 0;
                                    //LeapY = 0;
                                    //LeapZ = 0;
                                    ScreenX = _transformationInverse[0, 0] * LeapX + _transformationInverse[1, 0] * LeapY + _transformationInverse[2, 0] * LeapZ + _transformationInverse[3, 0];
                                    ScreenY = _transformationInverse[0, 1] * LeapX + _transformationInverse[1, 1] * LeapY + _transformationInverse[2, 1] * LeapZ + _transformationInverse[3, 1];
                                    ScreenZ = _transformationInverse[0, 2] * LeapX + _transformationInverse[1, 2] * LeapY + _transformationInverse[2, 2] * LeapZ + _transformationInverse[3, 2];

                                    ScreenPosX.Text = ScreenX.ToString();
                                    ScreenPosY.Text = ScreenY.ToString();
                                    ScreenPosZ.Text = ScreenZ.ToString();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                handPosX.Text = "NA";
                handPosY.Text = "NA";
                handPosZ.Text = "NA";
            }
        }

        private void Controller_Device(object sender, DeviceEventArgs e)
        {
            
        }

        private void Controller_Connect(object sender, ConnectionEventArgs e)
        {
            
        }

        private void cmdDisconnectLeapMotion_Click(object sender, EventArgs e)
        {
            if (controller.IsConnected)
                controller.StopConnection();
        }

        private void cmdSetOrigin_Click(object sender, EventArgs e)
        {
            if(hand.Fingers.Count > 0)
            {
                SetOriginTrigger = true;
            }
        }

        private void cmdSetAxis1_Click(object sender, EventArgs e)
        {
            if (hand.Fingers.Count > 0)
            {
                SetAxisXTrigger = true;
            }
        }

        private void cmdSetAxis2_Click(object sender, EventArgs e)
        {
            if(hand.Fingers.Count > 0)
            {
                SetAxisYTrigger = true;
            }
        }
    }
}
