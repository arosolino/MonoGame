// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Framework.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Xna.Framework.Input
{
    static partial class GamePad
    {
        private class GamePadInfo
        {
            public IntPtr Device;
            public int PacketNumber;

            public Buttons Buttons;
            public Vector2 ThumbstickL;
            public Vector2 ThumbstickR;
            public float TriggerL;
            public float TriggerR;
        }

        private static readonly Dictionary<int, GamePadInfo> Gamepads = new Dictionary<int, GamePadInfo>();
        private static readonly Dictionary<int, int> _translationTable = new Dictionary<int, int>();

        internal static void AddDevice(int deviceId)
        {
            var gamepad = new GamePadInfo();
            gamepad.Device = Sdl.GameController.Open(deviceId);

            var id = 0;
            while (Gamepads.ContainsKey(id))
                id++;

            Gamepads.Add(id, gamepad);

            RefreshTranslationTable();
        }

        internal static void RemoveDevice(int instanceid)
        {
            foreach (KeyValuePair<int, GamePadInfo> entry in Gamepads)
            {
                if (Sdl.Joystick.InstanceID(Sdl.GameController.GetJoystick(entry.Value.Device)) == instanceid)
                {
                    Gamepads.Remove(entry.Key);
                    DisposeDevice(entry.Value);
                    break;
                }
            }

            RefreshTranslationTable();
        }

        internal static void RefreshTranslationTable()
        {
            _translationTable.Clear();
            foreach (var pair in Gamepads)
            {
                _translationTable[Sdl.Joystick.InstanceID(Sdl.GameController.GetJoystick(pair.Value.Device))] = pair.Key;
            }
        }

        internal static Buttons FromSDLButton(Sdl.GameController.Button button)
        {
            return button switch
            {
                Sdl.GameController.Button.A => Buttons.A,
                Sdl.GameController.Button.B => Buttons.B,
                Sdl.GameController.Button.X => Buttons.X,
                Sdl.GameController.Button.Y => Buttons.Y,
                Sdl.GameController.Button.Back => Buttons.Back,
                Sdl.GameController.Button.Guide => Buttons.BigButton,
                Sdl.GameController.Button.Start => Buttons.Start,
                Sdl.GameController.Button.LeftStick => Buttons.LeftStick,
                Sdl.GameController.Button.RightStick => Buttons.RightStick,
                Sdl.GameController.Button.LeftShoulder => Buttons.LeftShoulder,
                Sdl.GameController.Button.RightShoulder => Buttons.RightShoulder,
                Sdl.GameController.Button.DpadUp => Buttons.DPadUp,
                Sdl.GameController.Button.DpadDown => Buttons.DPadDown,
                Sdl.GameController.Button.DpadLeft => Buttons.DPadLeft,
                Sdl.GameController.Button.DpadRight => Buttons.DPadRight,
                _ => Buttons.None,
            };
        }

        internal static int FromSDLPacketNumber(uint packetNumber)
        {
            return packetNumber < int.MaxValue ? (int)packetNumber : (int)(packetNumber - (uint)int.MaxValue);
        }

        internal static void ChangeButton(int instanceid, uint packetNumber, byte sdlBtn, short value)
        {
            if (!TryGetGamePadInfo(instanceid, out var info))
                return;
        
            var btn = FromSDLButton((Sdl.GameController.Button)sdlBtn);

            if (value == 0)
                info.Buttons &= ~btn;
            else
                info.Buttons |= btn;

            info.PacketNumber = FromSDLPacketNumber(packetNumber);
        }

        internal static void ChangeAxis(int instanceid, uint packetNumber, byte sdlAxis, short value)
        {
            if (!TryGetGamePadInfo(instanceid, out var info))
                return;
        
            switch ((Sdl.GameController.Axis)sdlAxis)
            {
                case Sdl.GameController.Axis.LeftX:
                    info.ThumbstickL.X = GetFromSdlAxis(value);
                    break;
                case Sdl.GameController.Axis.LeftY:
                    info.ThumbstickL.Y = GetFromSdlAxis(value) * -1f;
                    break;
                case Sdl.GameController.Axis.RightX:
                    info.ThumbstickR.X = GetFromSdlAxis(value);
                    break;
                case Sdl.GameController.Axis.RightY:
                    info.ThumbstickR.Y = GetFromSdlAxis(value) * -1f;
                    break;
                case Sdl.GameController.Axis.TriggerLeft:
                    info.TriggerL = GetFromSdlAxis(value);
                    if(info.TriggerL > 0f)
                        info.Buttons |= Buttons.LeftTrigger;
                    else
                        info.Buttons &= ~Buttons.LeftTrigger;
                    break;
                case Sdl.GameController.Axis.TriggerRight:
                    info.TriggerR = GetFromSdlAxis(value);
                    if(info.TriggerR > 0f)
                        info.Buttons |= Buttons.RightTrigger;
                    else
                        info.Buttons &= ~Buttons.RightTrigger;
                    break;
            }

            info.PacketNumber = FromSDLPacketNumber(packetNumber);
        }

        private static bool TryGetGamePadInfo(int instanceid, out GamePadInfo info)
        {
            if(_translationTable.TryGetValue(instanceid, out var index) && Gamepads.TryGetValue(index, out info))
                return true;
            info = default;
            return false;
        }

        private static void DisposeDevice(GamePadInfo info)
        {
            Sdl.GameController.Close(info.Device);
        }

        private static int GetDeviceId(GamePadInfo info)
        {
            var instanceid = Sdl.Joystick.InstanceID(Sdl.GameController.GetJoystick(info.Device));
            var n = Sdl.Joystick.NumJoysticks();
            for (var i = 0; i < n; i++)
            {
                if (Sdl.Joystick.GetDeviceInstanceID(i) == instanceid)
                    return i;
            }
            return -1;
        }

        private static void ResetDevice(GamePadInfo info)
        {
            var deviceId = GetDeviceId(info);
            if(deviceId == -1)
                return;

            DisposeDevice(info);

            Joystick.ResetDevice(deviceId);

            info.Device = Sdl.GameController.Open(deviceId);
        }

        internal static void CloseDevices()
        {
            foreach (var entry in Gamepads)
                DisposeDevice(entry.Value);

            Gamepads.Clear();
        }

        private static int PlatformGetMaxNumberOfGamePads()
        {
            return 16;
        }

        private static GamePadCapabilities PlatformGetCapabilities(int index)
        {
            if (!Gamepads.TryGetValue(index, out var info))
                return new GamePadCapabilities();

            var gamecontroller = info.Device;
            var caps = new GamePadCapabilities();

            caps.IsConnected = true;
            caps.DisplayName = Sdl.GameController.GetName(gamecontroller);
            caps.Identifier = Sdl.Joystick.GetGUID(Sdl.GameController.GetJoystick(gamecontroller)).ToString();
            caps.HasLeftVibrationMotor = caps.HasRightVibrationMotor = Sdl.GameController.HasRumble(gamecontroller) != 0;
            caps.GamePadType = GamePadType.GamePad;
            caps.HasAButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.A);
            caps.HasBButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.B);
            caps.HasXButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.X);
            caps.HasYButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.Y);
            caps.HasBackButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.Back);
            caps.HasBigButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.Guide);
            caps.HasStartButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.Start);
            caps.HasDPadLeftButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.DpadLeft);
            caps.HasDPadDownButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.DpadDown);
            caps.HasDPadRightButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.DpadRight);
            caps.HasDPadUpButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.DpadUp);
            caps.HasLeftShoulderButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.LeftShoulder);
            caps.HasLeftTrigger = Sdl.GameController.HasAxis(gamecontroller, Sdl.GameController.Axis.TriggerLeft);
            caps.HasRightShoulderButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.RightShoulder);
            caps.HasRightTrigger = Sdl.GameController.HasAxis(gamecontroller, Sdl.GameController.Axis.TriggerRight);
            caps.HasLeftStickButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.LeftStick);
            caps.HasRightStickButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.RightStick);
            caps.HasLeftXThumbStick = Sdl.GameController.HasAxis(gamecontroller, Sdl.GameController.Axis.LeftX);
            caps.HasLeftYThumbStick = Sdl.GameController.HasAxis(gamecontroller, Sdl.GameController.Axis.LeftY);
            caps.HasRightXThumbStick = Sdl.GameController.HasAxis(gamecontroller, Sdl.GameController.Axis.RightX);
            caps.HasRightYThumbStick = Sdl.GameController.HasAxis(gamecontroller, Sdl.GameController.Axis.RightY);

            return caps;
        }

        private static float GetFromSdlAxis(int axis)
        {
            // SDL Axis ranges from -32768 to 32767, so we need to divide with different numbers depending on if it's positive
            if (axis < 0)
                return axis / 32768f;

            return axis / 32767f;
        }

        private static GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            if (!Gamepads.TryGetValue(index, out var info))
                return GamePadState.Default;
    
            return new GamePadState(
                new GamePadThumbSticks(info.ThumbstickL, info.ThumbstickR, leftDeadZoneMode, rightDeadZoneMode),
                new GamePadTriggers(info.TriggerL, info.TriggerR),
                new GamePadButtons(info.Buttons),
                new GamePadDPad(info.Buttons));
        }

        private static void PlatformResetState(int index)
        {
            if (!Gamepads.TryGetValue(index, out var info))
                return;

            info.Buttons = default;
            info.ThumbstickL = default;
            info.ThumbstickR = default;
            info.TriggerL = default;
            info.TriggerR = default;

            ResetDevice(info);
        }

        private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            if (!Gamepads.ContainsKey(index))
                return false;

            var gamepad = Gamepads[index];

            return Sdl.GameController.Rumble(gamepad.Device, (ushort)(65535f * leftMotor),
                       (ushort)(65535f * rightMotor), uint.MaxValue) == 0 &&
                   Sdl.GameController.RumbleTriggers(gamepad.Device, (ushort)(65535f * leftTrigger),
                       (ushort)(65535f * rightTrigger), uint.MaxValue) == 0;
        }
    }
}
