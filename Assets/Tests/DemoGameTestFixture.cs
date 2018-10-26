using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Experimental.Input.Controls;
using UnityEngine.Experimental.Input.Plugins.DualShock;
using UnityEngine.Experimental.Input.Plugins.Steam;
using UnityEngine.Experimental.Input.Plugins.XInput;
using UnityEngine.Experimental.Input.Plugins.XR;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Gyroscope = UnityEngine.Experimental.Input.Gyroscope;

/// <summary>
/// Fixture to set up tests for <see cref="DemoGame"/>.
/// </summary>
[PrebuildSetup("DemoGameTestPrebuildSetup")]
public class DemoGameTestFixture
{
    public DemoGame game { get; set; }
    public InputTestFixture input { get; set; }
    public SteamTestFixture steam { get; set; }
    public RuntimePlatform platform { get; private set; }

    public Mouse mouse { get; set; }
    public Keyboard keyboard { get; set; }
    public Touchscreen touchscreen { get; set; }
    public DualShockGamepad ps4Gamepad { get; set; }
    public XInputController xboxGamepad { get; set; }
    public Joystick joystick { get; set; }
    public Pen pen { get; set; }
    public Gyroscope gyro { get; set; }
    public XRHMD hmd { get; set; }
    public XRController leftHand { get; set; }
    public XRController rightHand { get; set; }
    public InputDevice steamController { get; set; }
    ////TODO: on-screen controls

    public DemoPlayerController player1
    {
        get { return game.players[0]; }
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Set up input.
        input = new InputTestFixture();
        input.Setup();

        // See if we have a platform set for the current test.
        var testProperties = TestContext.CurrentContext.Test.Properties;
        if (testProperties.ContainsKey("Platform"))
        {
            var value = (string)testProperties["Platform"][0];
            switch (value.ToLower())
            {
                case "windows":
                    platform = RuntimePlatform.WindowsPlayer;
                    break;

                case "osx":
                    platform = RuntimePlatform.OSXPlayer;
                    break;

                case "android":
                    platform = RuntimePlatform.Android;
                    break;

                case "ios":
                    platform = RuntimePlatform.IPhonePlayer;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
        else
        {
            platform = Application.platform;
        }
        DemoGame.platform = platform;

        // Give us a fresh scene.
        yield return SceneManager.LoadSceneAsync("Assets/Demo/Demo.unity", LoadSceneMode.Single);
        game = GameObject.Find("DemoGame").GetComponent<DemoGame>();

        // If there's a "Platform" property on the test or no specific "Device" property, add the default
        // set of devices for the current platform.
        if (testProperties.ContainsKey("Platform") || !testProperties.ContainsKey("Device"))
        {
            // Set up default device matrix for current platform.
            // NOTE: We use strings here instead of types as not all devices are available in all players.
            switch (platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    keyboard = (Keyboard)InputSystem.AddDevice("Keyboard");
                    mouse = (Mouse)InputSystem.AddDevice("Mouse");
                    pen = (Pen)InputSystem.AddDevice("Pen");
                    touchscreen = (Touchscreen)InputSystem.AddDevice("Touchscreen");
                    ps4Gamepad = (DualShockGamepad)InputSystem.AddDevice("DualShockGamepadHID");
                    xboxGamepad = (XInputController)InputSystem.AddDevice("XInputController");
                    ////TODO: joystick
                    break;

                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    keyboard = (Keyboard)InputSystem.AddDevice("Keyboard");
                    mouse = (Mouse)InputSystem.AddDevice("Mouse");
                    ps4Gamepad = (DualShockGamepad)InputSystem.AddDevice("DualShockGamepadHID");
                    xboxGamepad = (XInputController)InputSystem.AddDevice("XInputController");
                    ////TODO: joystick
                    break;

                ////TODO: other platforms
                default:
                    throw new NotImplementedException();
            }
        }

        // Add whatever devices are specified in explicit "Device" properties.
        if (testProperties.ContainsKey("Device"))
        {
            foreach (var value in testProperties["Device"])
            {
                switch (((string)value).ToLower())
                {
                    case "gamepad":
                        InputSystem.AddDevice<Gamepad>();
                        break;

                    case "keyboard":
                        InputSystem.AddDevice<Keyboard>();
                        break;

                    case "mouse":
                        InputSystem.AddDevice<Mouse>();
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        // Check if we should add VR support.
        if (testProperties.ContainsKey("VR"))
        {
            var value = (string)testProperties["VR"][0];
            switch (value.ToLower())
            {
                case "":
                case "any":
                    // Add a combination of generic XRHMD and XRController instances that don't
                    // represent any specific set of hardware out there.
                    hmd = InputSystem.AddDevice<XRHMD>();
                    leftHand = InputSystem.AddDevice<XRController>();
                    rightHand = InputSystem.AddDevice<XRController>();
                    InputSystem.SetDeviceUsage(leftHand, CommonUsages.LeftHand);
                    InputSystem.SetDeviceUsage(rightHand, CommonUsages.RightHand);
                    break;

                default:
                    throw new NotImplementedException();
            }

            DemoGame.vrSupported = true;
        }

        // Check if we should add Steam support.
        if (testProperties.ContainsKey("Steam"))
        {
            ////TODO: create steam test fixture
            steamController = InputSystem.AddDevice("SteamDemoController");
        }
    }

    [TearDown]
    public void TearDown()
    {
        input.TearDown();
    }

    public void Click(string button, int playerIndex = 0)
    {
        if (playerIndex != 0)
            throw new NotImplementedException();

        ////TODO: drive this from a mouse input event so that we cover the whole UI action setup, too
        var buttonObject = GameObject.Find(button);
        Assert.That(buttonObject != null);
        buttonObject.GetComponent<Button>().onClick.Invoke();
    }

    public void Trigger(string action, int playerIndex = 0)
    {
        // Look up action.
        var controls = game.players[playerIndex].controls;
        var actionInstance = controls.asset.FindAction(action);
        if (actionInstance == null)
            throw new ArgumentException("action");

        // And trigger it.
        input.Trigger(actionInstance);
    }

    /// <summary>
    /// Press a key on the keyboard.
    /// </summary>
    /// <param name="key"></param>
    /// <remarks>
    /// Requires the current platform to have a keyboard.
    /// </remarks>
    public void Press(Key key)
    {
        Debug.Assert(keyboard != null);
        input.Set(keyboard[key], 1);
    }

    /// <summary>
    /// Release a key on the keyboard.
    /// </summary>
    /// <param name="key"></param>
    /// <remarks>
    /// Requires the current platform to have a keyboard.
    /// </remarks>
    public void Release(Key key)
    {
        Debug.Assert(keyboard != null);
        input.Set(keyboard[key], 1);
    }

    public void Press(ButtonControl button)
    {
        throw new NotImplementedException();
    }
}
