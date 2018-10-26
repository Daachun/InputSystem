using NUnit.Framework;
using UnityEngine.Experimental.Input;
using UnityEngine.Experimental.Input.LowLevel;
using UnityEngine.Experimental.Input.Plugins.Users;
using Gyroscope = UnityEngine.Experimental.Input.Gyroscope;

//much of this is so fucking arcane
//the control scheme and action stuff should be simple

//on platforms, we probably want to hook this up to system stuff; look at the Xbox API

internal class UserTests : InputTestFixture
{
    [Test]
    [Category("Users")]
    public void Users_HaveNoUsersByDefault()
    {
        Assert.That(InputUser.all, Has.Count.Zero);
    }

    [Test]
    [Category("Users")]
    public void Users_CanAddAndRemoveUsers()
    {
        var user1 = new TestUser();
        var user2 = new TestUser();

        InputUser.Add(user1);
        InputUser.Add(user2);

        Assert.That(InputUser.all, Has.Count.EqualTo(2)); // Plus default user.
        Assert.That(InputUser.all, Has.Exactly(1).SameAs(user1));
        Assert.That(InputUser.all, Has.Exactly(1).SameAs(user2));

        InputUser.Remove(user1);

        Assert.That(InputUser.all, Has.Count.EqualTo(1));
        Assert.That(InputUser.all, Has.None.SameAs(user1));
        Assert.That(InputUser.all, Has.Exactly(1).SameAs(user2));
    }

    [Test]
    [Category("Users")]
    [Ignore("TODO")]
    public void TODO_Users_CanDiscoverUsersFromDevicesReportedByRuntime()
    {
        Assert.Fail();
    }

    [Test]
    [Category("Users")]
    public void Users_HaveIndices()
    {
        var user1 = new TestUser();
        var user2 = new TestUser();

        Assert.That(user1.GetUserIndex(), Is.EqualTo(-1));

        InputUser.Add(user1);
        InputUser.Add(user2);

        Assert.That(user1.GetUserIndex(), Is.EqualTo(0));
        Assert.That(user2.GetUserIndex(), Is.EqualTo(1));

        InputUser.Remove(user1);

        Assert.That(user1.GetUserIndex(), Is.EqualTo(-1));
        Assert.That(user2.GetUserIndex(), Is.EqualTo(0));
    }

    [Test]
    [Category("Users")]
    public void Users_HaveUniqueIds()
    {
        var user1 = new TestUser();
        var user2 = new TestUser();

        Assert.That(user1.GetUserId(), Is.EqualTo(InputUser.kInvalidId));
        Assert.That(user2.GetUserId(), Is.EqualTo(InputUser.kInvalidId));

        InputUser.Add(user1);
        InputUser.Add(user2);

        Assert.That(user1.GetUserId(), Is.Not.EqualTo(InputUser.kInvalidId));
        Assert.That(user2.GetUserId(), Is.Not.EqualTo(InputUser.kInvalidId));
        Assert.That(user1.GetUserId(), Is.Not.EqualTo(user2.GetUserId()));

        InputUser.Remove(user1);

        Assert.That(user1.GetUserId(), Is.EqualTo(InputUser.kInvalidId));
    }

    [Test]
    [Category("Users")]
    public void Users_CanHaveUserNames()
    {
        var user = new TestUser();

        Assert.That(user.GetUserName(), Is.Null);

        InputUser.Add(user);

        Assert.That(user.GetUserName(), Is.Null);

        user.SetUserName("A");

        Assert.That(user.GetUserName(), Is.EqualTo("A"));

        user.SetUserName("B");

        Assert.That(user.GetUserName(), Is.EqualTo("B"));
    }

    [Test]
    [Category("Users")]
    public void Users_CanHavePlatformHandles()
    {
        var user = new TestUser();

        Assert.That(user.GetUserHandle(), Is.Null);

        InputUser.Add(user);

        Assert.That(user.GetUserHandle(), Is.Null);

        user.SetUserHandle(new InputUserHandle("test", 1));

        Assert.That(user.GetUserHandle(), Is.EqualTo(new InputUserHandle("test", 1)));

        user.SetUserHandle(null);

        Assert.That(user.GetUserHandle(), Is.Null);

        user.SetUserHandle(new InputUserHandle("test", 1));
        InputUser.Remove(user);

        Assert.That(user.GetUserHandle(), Is.Null);
    }

    ////TODO: break this apart and move the checks into the other tests
    [Test]
    [Category("Users")]
    public void Users_CanBeMonitoredForChanges()
    {
        InputUser.Add(new TestUser()); // Noise.
        InputUser.Add(new TestUser()); // Noise.
        var user = new TestUser();

        IInputUser receivedUser = null;
        InputUserChange? receivedChange = null;

        InputUser.onChange +=
            (usr, change) =>
        {
            Assert.That(receivedChange == null);
            receivedUser = usr;
            receivedChange = change;
        };

        // Added.
        InputUser.Add(user);

        Assert.That(receivedUser, Is.SameAs(user));
        Assert.That(receivedChange, Is.EqualTo(InputUserChange.Added));

        receivedUser = null;
        receivedChange = null;

        // NameChanged.
        user.SetUserName("NewName");

        Assert.That(receivedUser, Is.SameAs(user));
        Assert.That(receivedChange, Is.EqualTo(InputUserChange.NameChanged));

        receivedUser = null;
        receivedChange = null;

        // HandleChanged.
        user.SetUserHandle(new InputUserHandle("test", 1));

        Assert.That(receivedUser, Is.SameAs(user));
        Assert.That(receivedChange, Is.EqualTo(InputUserChange.HandleChanged));

        receivedUser = null;
        receivedChange = null;

        // Same name, no notification.
        user.SetUserName("NewName");

        Assert.That(receivedChange, Is.Null);

        // DevicesChanged.
        var device = InputSystem.AddDevice<Gamepad>();
        user.AssignInputDevice(device);

        Assert.That(receivedUser, Is.SameAs(user));
        Assert.That(receivedChange, Is.EqualTo(InputUserChange.DevicesChanged));

        receivedUser = null;
        receivedChange = null;

        // Same device, no notification.
        user.AssignInputDevice(device);

        Assert.That(receivedChange, Is.Null);

        // DevicesChanges, removed.
        user.ClearAssignedInputDevices();

        Assert.That(receivedUser, Is.SameAs(user));
        Assert.That(receivedChange, Is.EqualTo(InputUserChange.DevicesChanged));

        receivedUser = null;
        receivedChange = null;

        // ControlSchemeChanged.
        user.AssignControlScheme("gamepad");

        Assert.That(receivedUser, Is.SameAs(user));
        Assert.That(receivedChange, Is.EqualTo(InputUserChange.ControlSchemeChanged));

        receivedUser = null;
        receivedChange = null;

        // Same control scheme, no notification.
        user.AssignControlScheme("gamepad");

        Assert.That(receivedChange, Is.Null);

        // Removed.
        InputUser.Remove(user);

        Assert.That(receivedUser, Is.SameAs(user));
        Assert.That(receivedChange, Is.EqualTo(InputUserChange.Removed));

        ////TODO: actions
        ////TODO: activate, passivate
    }

    [Test]
    [Category("Users")]
    public void Users_CanAssignDevicesToUsers()
    {
        var user1 = new TestUser();
        var user2 = new TestUser();

        var gamepad = InputSystem.AddDevice<Gamepad>();
        var keyboard = InputSystem.AddDevice<Keyboard>();
        var mouse = InputSystem.AddDevice<Mouse>();

        Assert.That(user1.GetAssignedInputDevices(), Is.Empty);
        Assert.That(user2.GetAssignedInputDevices(), Is.Empty);

        InputUser.Add(user1);
        InputUser.Add(user2);

        user1.AssignInputDevices(new InputDevice[] {keyboard, mouse});
        user2.AssignInputDevice(gamepad);

        Assert.That(user1.GetAssignedInputDevices(), Is.EquivalentTo(new InputDevice[] { keyboard, mouse }));
        Assert.That(user2.GetAssignedInputDevices(), Is.EquivalentTo(new InputDevice[] { gamepad }));
    }

    [Test]
    [Category("Users")]
    public void Users_CannotAssignDevicesToUserThatHasNotBeenAdded()
    {
        var user = new TestUser();
        var device = InputSystem.AddDevice<Gamepad>();

        Assert.That(() => user.AssignInputDevice(device), Throws.InvalidOperationException);
    }

    [Test]
    [Category("Users")]
    public void Users_CanAssignSameDeviceToMoreThanOneUser()
    {
        var user1 = new TestUser();
        var user2 = new TestUser();

        InputUser.Add(user1);
        InputUser.Add(user2);

        var gamepad = InputSystem.AddDevice<Gamepad>();

        user1.AssignInputDevice(gamepad);
        user2.AssignInputDevice(gamepad);

        Assert.That(user1.GetAssignedInputDevices(), Is.EquivalentTo(new InputDevice[] { gamepad }));
        Assert.That(user2.GetAssignedInputDevices(), Is.EquivalentTo(new InputDevice[] { gamepad }));
    }

    [Test]
    [Category("Users")]
    public void Users_CanAssignDevicesToUserStepByStep()
    {
        var device1 = InputSystem.AddDevice<Gamepad>();
        var device2 = InputSystem.AddDevice<Gamepad>();
        var device3 = InputSystem.AddDevice<Gamepad>();

        var user1 = new TestUser();
        var user2 = new TestUser();

        InputUser.Add(user1);
        InputUser.Add(user2);

        user1.AssignInputDevice(device1);
        user2.AssignInputDevice(device2);
        user1.AssignInputDevice(device3);

        Assert.That(user1.GetAssignedInputDevices(), Is.EquivalentTo(new InputDevice[] { device1, device3}));
        Assert.That(user2.GetAssignedInputDevices(), Is.EquivalentTo(new InputDevice[] {device2}));
    }

    [Test]
    [Category("Users")]
    public void Users_AssigningSameDeviceToSameUserMoreThanOnce_IsIgnored()
    {
        var device = InputSystem.AddDevice<Gamepad>();
        var user = new TestUser();
        InputUser.Add(user);

        user.AssignInputDevice(device);
        user.AssignInputDevice(device);
        user.AssignInputDevice(device);

        Assert.That(user.GetAssignedInputDevices(), Is.EquivalentTo(new InputDevice[] {device}));
    }

    [Test]
    [Category("Users")]
    public void Users_AssignedDevices_AreLostWhenUserIsRemoved()
    {
        var device1 = InputSystem.AddDevice<Gamepad>();
        var device2 = InputSystem.AddDevice<Gamepad>();

        var user = new TestUser();
        InputUser.Add(user);

        user.AssignInputDevice(device1);
        user.AssignInputDevice(device2);

        InputUser.Remove(user);

        Assert.That(user.GetAssignedInputDevices(), Has.Count.Zero);
    }

    [Test]
    [Category("Users")]
    public void Users_CanClearAssignedDevices()
    {
        var device = InputSystem.AddDevice<Gamepad>();

        var user1 = new TestUser();
        var user2 = new TestUser();
        var user3 = new TestUser();

        InputUser.Add(user1);
        InputUser.Add(user2);

        user1.AssignInputDevice(device);
        user1.ClearAssignedInputDevices();
        user2.ClearAssignedInputDevices();
        user3.ClearAssignedInputDevices();

        Assert.That(user1.GetAssignedInputDevices(), Is.Empty);
        Assert.That(user2.GetAssignedInputDevices(), Is.Empty);
        Assert.That(user3.GetAssignedInputDevices(), Is.Empty);
    }

    [Test]
    [Category("Users")]
    public void Users_CanQueryUnassignedDevices()
    {
        var gamepad = InputSystem.AddDevice<Gamepad>();
        var keyboard = InputSystem.AddDevice<Keyboard>();
        var mouse = InputSystem.AddDevice<Mouse>();
        var touch = InputSystem.AddDevice<Touchscreen>();
        var gyro = InputSystem.AddDevice<Gyroscope>();

        var user1 = new TestUser();
        var user2 = new TestUser();
        var user3 = new TestUser();

        InputUser.Add(user1);
        InputUser.Add(user2);
        InputUser.Add(user3);

        user1.AssignInputDevice(gamepad);
        user3.AssignInputDevices(new InputDevice[] {keyboard, mouse});

        using (var unusedDevices = InputUser.GetUnassignedInputDevices())
        {
            Assert.That(unusedDevices, Has.Count.EqualTo(2));
            Assert.That(unusedDevices, Has.Exactly(1).SameAs(touch));
            Assert.That(unusedDevices, Has.Exactly(1).SameAs(gyro));
        }
    }
    
    //this is where the shit starts
    
    //what if we take enabling/disabling away from the user?
    //meaning, to enable/disable, you continue to just call Enable() and Disable(). simple.
    
    //and then.. what if we simplify associating actions to just correlating an asset with the user?
    //we won't support lose actions and maps but so what?
    
    //the current stuff not only moves enabling/disabling over into the user's domain, it also
    //blows up that domain further by involving a stack

    [Test]
    [Category("Users")]
    public void Users_CanAssignActionsToUsers()
    {
        var action = new InputAction();

        var user = new TestUser();
        InputUser.Add(user);

        Assert.That(user.GetInputActions(), Is.Empty);

        user.GetInputActions().Push(action);

        Assert.That(user.GetInputActions(), Is.EquivalentTo(new[] { action }));

        user.GetInputActions().Clear();

        Assert.That(user.GetInputActions(), Is.Empty);
    }

    [Test]
    [Category("Users")]
    public void Users_CanAssignActionMapsToUsers()
    {
        var map = new InputActionMap();

        var action1 = map.AddAction("action1");
        var action2 = map.AddAction("action2");

        var user = new TestUser();
        InputUser.Add(user);

        user.SetInputActions(map);

        Assert.That(user.GetInputActions(), Is.EquivalentTo(new[] { action1, action2 }));
    }

    [Test]
    [Category("Users")]
    public void Users_CanActivateAndPassivateInput()
    {
        var action = new InputAction();

        var user = new TestUser();
        InputUser.Add(user);

        user.GetInputActions().Push(action);

        // Make sure user is passive by default.
        Assert.That(user.IsInputActive(), Is.False);
        Assert.That(action.enabled, Is.False);

        // Activate user input.
        user.ActivateInput();

        Assert.That(action.enabled, Is.True);

        // Passivate user input again.
        user.PassivateInput();

        Assert.That(action.enabled, Is.False);
    }

    [Test]
    [Category("Users")]
    public void Users_CanAssignControlScheme()
    {
        var user = new TestUser();

        Assert.That(user.GetControlScheme(), Is.Null);

        InputUser.Add(user);

        user.AssignControlScheme("scheme");

        Assert.That(user.GetControlScheme(), Is.EqualTo(new InputControlScheme("scheme")));

        user.AssignControlScheme(null);

        Assert.That(user.GetControlScheme(), Is.Null);
    }

    [Test]
    [Category("Users")]
    public void Users_CanAssignControlScheme_AndAutomaticallyAssignMatchingUnusedDevices()
    {
        var keyboard = InputSystem.AddDevice<Keyboard>();
        InputSystem.AddDevice<Mouse>(); // Noise.
        var gamepad1 = InputSystem.AddDevice<Gamepad>();
        var gamepad2 = InputSystem.AddDevice<Gamepad>();
        var gamepad3 = InputSystem.AddDevice<Gamepad>();

        var singleGamepadScheme = new InputControlScheme("SingleGamepad")
            .WithRequiredDevice("<Gamepad>");
        var dualGamepadScheme = new InputControlScheme("DualGamepad")
            .WithRequiredDevice("<Gamepad>")
            .WithRequiredDevice("<Gamepad>");

        var user1 = new TestUser();
        var user2 = new TestUser();
        var user3 = new TestUser();

        InputUser.Add(user1);
        InputUser.Add(user2);
        InputUser.Add(user3);

        user1.AssignInputDevice(keyboard); // Should automatically be unassigned.
        user3.AssignInputDevice(keyboard); // Should not be affected by any of what we do here.

        user1.AssignControlScheme(singleGamepadScheme).AndAssignMatchingDevices();
        user2.AssignControlScheme(dualGamepadScheme).AndAssignMatchingDevices();

        Assert.That(user1.GetAssignedInputDevices(), Is.EquivalentTo(new[] { gamepad1 }));
        Assert.That(user2.GetAssignedInputDevices(), Is.EquivalentTo(new[] { gamepad2, gamepad3 }));
        Assert.That(user3.GetAssignedInputDevices(), Is.EquivalentTo(new[] { keyboard }));
    }

    // Choosing one control scheme should allow suppressing bindings from other control schemes.
    // However, this must be under user control as the setup of enabling all bindings is perfectly
    // valid and in fact desirable in a single-user scenario where the user should be allowed
    // to seamlessly transition between devices and control schemes.
    [Test]
    [Category("Users")]
    public void Users_CanAssignControlScheme_AndMaskOutBindingsFromOtherControlSchemes()
    {
        var gamepad = InputSystem.AddDevice<Gamepad>();
        var gamepadScheme = new InputControlScheme("Gamepad")
            .WithRequiredDevice("<Gamepad>");

        var action = new InputAction();
        action.AddBinding("<Gamepad>/buttonSouth", "Gamepad");
        action.AddBinding("<Mouse>/leftButton", "KeyboardMouse");

        var user = new TestUser();
        InputUser.Add(user);

        user.GetInputActions().Push(action);

        user.AssignInputDevice(gamepad);
        user.AssignControlScheme(gamepadScheme)
            .AndMaskBindingsFromOtherControlSchemes();

        Assert.That(action.controls, Is.EquivalentTo(new[] { gamepad.buttonSouth }));
        Assert.That(action.bindingMask, Is.EqualTo(new InputBinding {groups = "Gamepad"}));
    }

    [Test]
    [Category("Users")]
    public void Users_CanDetectWhenUnassignedDeviceIsUsed()
    {
        var gamepad1 = InputSystem.AddDevice<Gamepad>();
        var gamepad2 = InputSystem.AddDevice<Gamepad>();
        var gamepad3 = InputSystem.AddDevice<Gamepad>();

        var actionAssignedToUser = new InputAction(binding: "<Gamepad>/buttonSouth");

        var actionNotAssignedToUser = new InputAction(binding: "<Gamepad>/buttonNorth");
        actionNotAssignedToUser.Enable();

        var user = new TestUser();
        InputUser.Add(user);

        // Noise.
        InputUser.Add(new TestUser());
        InputUser.all[1].AssignInputDevice(gamepad3);

        IInputUser receivedUser = null;
        InputAction receivedAction = null;
        InputControl receivedControl = null;
        InputUser.onUnassignedDeviceUsed +=
            (u, a, c) =>
        {
            Assert.That(receivedUser, Is.Null);
            receivedUser = u;
            receivedAction = a;
            receivedControl = c;
        };

        user.GetInputActions().Push(actionAssignedToUser);
        user.AssignInputDevice(gamepad1);
        user.ActivateInput();

        // No callback if using gamepad1.
        InputSystem.QueueStateEvent(gamepad1, new GamepadState().WithButton(GamepadButton.South));
        InputSystem.Update();

        Assert.That(receivedUser, Is.Null);
        Assert.That(receivedAction, Is.Null);
        Assert.That(receivedControl, Is.Null);

        // Callback when using gamepad2.
        InputSystem.QueueStateEvent(gamepad2, new GamepadState().WithButton(GamepadButton.South));
        InputSystem.Update();

        Assert.That(receivedUser, Is.SameAs(user));
        Assert.That(receivedAction, Is.SameAs(actionAssignedToUser));
        Assert.That(receivedControl, Is.SameAs(gamepad2.buttonSouth));

        receivedUser = null;
        receivedControl = null;

        // No callback when triggering action not assigned to user.
        InputSystem.QueueStateEvent(gamepad1, new GamepadState().WithButton(GamepadButton.North));
        InputSystem.Update();

        Assert.That(receivedUser, Is.Null);
        Assert.That(receivedAction, Is.Null);
        Assert.That(receivedControl, Is.Null);
    }

    [Test]
    [Category("Users")]
    public void Users_CanFindUserThatIsAssignedToSpecificDevice()
    {
        var gamepad1 = InputSystem.AddDevice<Gamepad>();
        var gamepad2 = InputSystem.AddDevice<Gamepad>();
        var gamepad3 = InputSystem.AddDevice<Gamepad>();

        var user1 = new TestUser();
        var user2 = new TestUser();

        InputUser.Add(user1);
        InputUser.Add(user2);

        user1.AssignInputDevice(gamepad1);
        user2.AssignInputDevice(gamepad2);

        Assert.That(InputUser.FindUserForDevice(gamepad1), Is.SameAs(user1));
        Assert.That(InputUser.FindUserForDevice(gamepad2), Is.SameAs(user2));
        Assert.That(InputUser.FindUserForDevice(gamepad3), Is.Null);
    }

    [Test]
    [Category("Users")]
    [Ignore("TODO")]
    public void TODO_Users_CanGetAndSetCustomBindings()
    {
        Assert.Fail();
    }

    public class TestUser : IInputUser
    {
    }
}
