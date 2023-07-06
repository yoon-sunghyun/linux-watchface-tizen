using System;
using Tizen.Security;
using Tizen.Sensor;
using Tizen.System;
using Xamarin.Forms;

namespace linux_watchface
{
    internal class ViewModelMain
    {
        private readonly double _FontSize;
        private HeartRateMonitor _HeartSensor;
        private Pedometer _StepsSensor;

        public ViewModel<bool> vmAmbientModeDisabled { get; set; }
        public ViewModel<Color> vmBackgroundColor { get; set; }
        public ViewModel<DateTime> vmDateTime { get; set; }
        public FStringVM vmHeader { get; set; }
        public FStringVM vmFooter { get; set; }
        public FStringVM vmBattery { get; set; }
        public FStringVM vmDate { get; set; }
        public FStringVM vmTime { get; set; }
        public FStringVM vmHeart { get; set; }
        public FStringVM vmSteps { get; set; }

        public ViewModelMain()
        {
            _FontSize = Device.GetNamedSize(
                NamedSize.Micro,
                typeof(Label),
                false);
            _FontSize *= 0.75;

            vmAmbientModeDisabled = new ViewModel<bool>()
            {
                Value = true
            };
            vmBackgroundColor = new ViewModel<Color>()
            {
                Value = CustomColor.Red
            };
            vmDateTime = new ViewModel<DateTime>()
            {
                Value = DateTime.UtcNow
            };

            vmHeader = new FStringVM(_FontSize, "user@watch:~$ now", CustomColor.Gray);
            vmFooter = new FStringVM(_FontSize, "user@watch:~$ ", CustomColor.Gray);
            vmBattery = new FStringVM(_FontSize, "[BAT.] ", CustomColor.White);
            vmDate = new FStringVM(_FontSize, "[DATE] ", CustomColor.White);
            vmTime = new FStringVM(_FontSize, "[TIME] ", CustomColor.White);
            vmHeart = new FStringVM(_FontSize, "[H.R.] ", CustomColor.White);
            vmSteps = new FStringVM(_FontSize, "[STEP] ", CustomColor.White);

            _InitSensors();
            _InitEventHandlers();

            _OnAmbientChanged();
            _OnBatteryChanged();
            _OnDateTimeChanged();
        }

        public void OnTerminate()
        {
            _HeartSensor.Stop();
            _HeartSensor.DataUpdated -= _OnHeartChanged;
            _HeartSensor.Dispose();
            _HeartSensor = null;

            _StepsSensor.Stop();
            _StepsSensor.DataUpdated -= _OnStepsChanged;
            _StepsSensor.Dispose();
            _StepsSensor = null;
        }

        private void _InitSensors()
        {
            try
            {
                const string privilege = "http://tizen.org/privilege/healthinfo";
                CheckResult checkResult = PrivacyPrivilegeManager.CheckPermission(privilege);

                switch (checkResult)
                {
                    case CheckResult.Ask:
                    {
                        PrivacyPrivilegeManager.ResponseContext responseContext = null;

                        if (PrivacyPrivilegeManager
                            .GetResponseContext(privilege)
                            .TryGetTarget(out responseContext))
                        {
                            responseContext.ResponseFetched += (sender, args) => {
                                switch (args.result)
                                {
                                    case RequestResult.AllowForever:
                                    {
                                        _OnInitSensorsAllowed();
                                        break;
                                    }
                                    case RequestResult.DenyForever:
                                    case RequestResult.DenyOnce:
                                    {
                                        _OnInitSensorsDenied();
                                        break;
                                    }
                                }
                            };
                        }
                        PrivacyPrivilegeManager.RequestPermission(privilege);
                        break;
                    }
                    case CheckResult.Allow:
                    {
                        _OnInitSensorsAllowed();
                        break;
                    }
                    case CheckResult.Deny:
                    {
                        _OnInitSensorsDenied();
                        break;
                    }
                }
            }
            catch (Exception)
            {
                _OnInitSensorsDenied();
            }
        }

        private void _OnInitSensorsAllowed()
        {
            try
            {
                _HeartSensor = new HeartRateMonitor();
                _HeartSensor.DataUpdated += _OnHeartChanged;
                _HeartSensor.PausePolicy = SensorPausePolicy.All;
                _HeartSensor.Interval = 500;
                _HeartSensor.Start();
                vmHeart.Edit($"{_HeartSensor.HeartRate} bpm", CustomColor.Red);

                _StepsSensor = new Pedometer();
                _StepsSensor.DataUpdated += _OnStepsChanged;
                _StepsSensor.PausePolicy = SensorPausePolicy.PowerSaveMode;
                _StepsSensor.Interval = 500;
                _StepsSensor.Start();
                vmSteps.Edit($"{_StepsSensor.StepCount} steps", CustomColor.Orange);
            }
            catch (Exception)
            {
                _OnInitSensorsDenied();
            }
        }

        private void _OnInitSensorsDenied()
        {
            _HeartSensor = null;
            _StepsSensor = null;
            vmHeart.Edit("access was denied", CustomColor.Gray);
            vmSteps.Edit("access was denied", CustomColor.Gray);
        }

        private void _InitEventHandlers()
        {
            vmAmbientModeDisabled.PropertyChanged
                += (sender, args) => { _OnAmbientChanged(); };
            Battery.ChargingStateChanged
                += (sender, args) => { _OnBatteryChanged(); };
            Battery.PercentChanged
                += (sender, args) => { _OnBatteryChanged(); };
            Battery.LevelChanged
                += (sender, args) => { _OnBatteryChanged(); };
            vmDateTime.PropertyChanged
                += (sender, args) => { _OnDateTimeChanged(); };
        }

        private void _OnAmbientChanged()
        {
            if (vmAmbientModeDisabled.Value)
            {
                vmBackgroundColor.Value = CustomColor.DarkGray;
            }
            else
            {
                vmBackgroundColor.Value = Color.Transparent;
            }
            _OnDateTimeChanged();
        }

        private void _OnBatteryChanged()
        {
            if (vmAmbientModeDisabled.Value)
            {
                var val = (10 - (Battery.Percent / 10));
                var textBar = $"{"■■■■■■■■■■□□□□□□□□□□".Substring(val, 10)}";
                var textNum = $"{((Battery.IsCharging ? "⚡" : "") + Battery.Percent), 3}%";

                var text = $"[{textBar}|{textNum}]";
                var textColor = CustomColor.GetBatteryColor();
                vmBattery.Edit(text, textColor);

                if (Battery.IsCharging)
                {
                    if (_HeartSensor != null)
                    {
                        _HeartSensor.Stop();
                        vmHeart.Edit("⚡", CustomColor.Gray);
                    }
                    if (_StepsSensor != null)
                    {
                        _StepsSensor.Stop();
                        vmSteps.Edit("⚡", CustomColor.Gray);
                    }
                }
                else
                {
                    if (_HeartSensor != null)
                    {
                        _HeartSensor.Start();
                        vmHeart.Edit($"{_HeartSensor.HeartRate} bpm", CustomColor.Red);
                    }
                    if (_StepsSensor != null)
                    {
                        _StepsSensor.Start();
                        vmSteps.Edit($"{_StepsSensor.StepCount} steps", CustomColor.Orange);
                    }
                }
            }
        }

        private void _OnDateTimeChanged()
        {
            string text;
            Color textColor;

            if (vmAmbientModeDisabled.Value)
            {
                // formatting date
                text = $"{$"{vmDateTime.Value:yyyy/MM/dd ddd.}",-17}";
                textColor = CustomColor.GetDateColor(vmDateTime.Value);
                vmDate.Edit(text, textColor);

                // formatting time
                text = $"{$"{vmDateTime.Value:HH:mm:ss [K]}",-17}";
                textColor = CustomColor.GetTimeColor(vmDateTime.Value);
                vmTime.Edit(text, textColor);

                // changing cursor color
                vmFooter.Edit("_", CustomColor.GetTickColor(vmDateTime.Value));
            }
            else
            {
                // formatting time for ambient mode
                // only showing hour and minute
                text = $"{$"{vmDateTime.Value:HH:mm}",-17}";
                textColor = CustomColor.GetTimeColor(vmDateTime.Value);
                vmTime.Edit(text, textColor);
            }
        }

        private void _OnHeartChanged(object sender, HeartRateMonitorDataUpdatedEventArgs args)
        {
            vmHeart.Edit($"{args.HeartRate} bpm", CustomColor.Red);
        }

        private void _OnStepsChanged(object sender, PedometerDataUpdatedEventArgs args)
        {
            vmSteps.Edit($"{args.StepCount} steps", CustomColor.Orange);
        }
    }
}
