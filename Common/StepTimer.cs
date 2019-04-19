using System;
using TerraFX.Interop;

namespace UWPPlayground.Common
{
    public struct StepTimer
    {
        public static StepTimer Create()
        {
            var timer = new StepTimer { _targetElapsedTicks = TicksPerSecond / 60 };

            TryQueryPerformanceFrequency(out timer._qpcFrequency);

            TryQueryPerformanceCounter(out timer._qpcLastTime);

            timer._qpcMaxDelta = (ulong)(timer._qpcFrequency.QuadPart / 10);

            return timer;
        }

        public ulong ElapsedTicks => _elapsedTicks;

        public double ElapsedSeconds => TicksToSeconds(_elapsedTicks);

        public ulong TotalTicks => _totalTicks;

        public double TotalSeconds => TicksToSeconds(_totalTicks);

        public uint FrameCount => _frameCount;

        public uint FramesPerSeconds => _framesPerSecond;

        public void SetFixedTimeStep(bool isFixedTimestep) { _isFixedTimeStep = isFixedTimestep; }

        public void SetTargetElapsedTicks(ulong targetElapsed) { _targetElapsedTicks = targetElapsed; }

        public void SetTargetElapsedSeconds(double targetElapsed) { _targetElapsedTicks = SecondsToTicks(targetElapsed); }

        public static ulong SecondsToTicks(double seconds)
            => (ulong)(seconds * TicksPerSecond);

        public static double TicksToSeconds(ulong ticks)
            => (double)ticks / TicksPerSecond;

        public void ResetElapsedTime()
        {
            TryQueryPerformanceCounter(out _qpcLastTime);

            _leftOverTicks = 0;
            _framesPerSecond = 0;
            _framesThisSecond = 0;
            _qpcSecondCounter = 0;
        }

        public void Tick(Action update)
        {
            TryQueryPerformanceCounter(out LARGE_INTEGER currentTime);

            var timeDelta = (ulong)(currentTime.QuadPart - _qpcLastTime.QuadPart);

            _qpcLastTime = currentTime;
            _qpcSecondCounter += timeDelta;

            if (timeDelta > _qpcMaxDelta)
            {
                timeDelta = _qpcMaxDelta;
            }

            timeDelta *= TicksPerSecond;
            timeDelta /= (ulong)_qpcFrequency.QuadPart;

            uint lastFrameCount = _frameCount;

            if (_isFixedTimeStep)
            {
                if ((ulong)Math.Abs((long)(timeDelta - _targetElapsedTicks)) < TicksPerSecond / 4000)
                {
                    timeDelta = _targetElapsedTicks;
                }

                _leftOverTicks += timeDelta;

                while (_leftOverTicks >= _targetElapsedTicks)
                {
                    _elapsedTicks = _targetElapsedTicks;
                    _totalTicks += _targetElapsedTicks;
                    _leftOverTicks -= _targetElapsedTicks;
                    _frameCount++;

                    update();
                }
            }
            else
            {
                _elapsedTicks = timeDelta;
                _totalTicks += timeDelta;
                _leftOverTicks = 0;
                _frameCount++;

                update();
            }

            if (_frameCount != lastFrameCount)
            {
                _framesThisSecond++;
            }

            if (_qpcSecondCounter >= (ulong) _qpcFrequency.QuadPart)
            {
                _framesPerSecond = _framesThisSecond;
                _framesThisSecond = 0;
                _qpcSecondCounter %= (ulong)_qpcFrequency.QuadPart;
            }
        }

        private static void TryQueryPerformanceCounter(out LARGE_INTEGER lpPerformanceCounter)
        {
            if (Kernel32.QueryPerformanceCounter(out lpPerformanceCounter) == TerraFX.Interop.Windows.FALSE)
            {
                DirectXHelper.ThrowWin32Exception($"{nameof(Kernel32.QueryPerformanceCounter)} failed");
            }
        }

        private static void TryQueryPerformanceFrequency(out LARGE_INTEGER lpFrequency)
        {
            if (Kernel32.QueryPerformanceFrequency(out lpFrequency) == TerraFX.Interop.Windows.FALSE)
            {
                DirectXHelper.ThrowWin32Exception($"{nameof(Kernel32.QueryPerformanceFrequency)} failed");
            }
        }

        private const ulong TicksPerSecond = 10000000;

        private LARGE_INTEGER _qpcFrequency;
        private LARGE_INTEGER _qpcLastTime;
        private ulong _qpcMaxDelta;

        private ulong _elapsedTicks;
        private ulong _totalTicks;
        private ulong _leftOverTicks;

        private uint _frameCount;
        private uint _framesPerSecond;
        private uint _framesThisSecond;
        private ulong _qpcSecondCounter;

        private bool _isFixedTimeStep;
        private ulong _targetElapsedTicks;
    }
}