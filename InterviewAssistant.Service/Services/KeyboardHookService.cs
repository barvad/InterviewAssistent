using InterviewAssistant.Core.Interfaces;
using InterviewAssistant.Core.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace InterviewAssistant.Service.Services
{
    public class KeyboardHookService : IKeyboardHookService, IDisposable
    {
        private readonly ILogger<KeyboardHookService> _logger;
        private readonly IConfigurationManager _configurationManager;
        private readonly List<KeyboardShortcut> _registeredShortcuts = new();
        private readonly object _lockObject = new();
        private IntPtr _hookId = IntPtr.Zero;
        private LowLevelKeyboardProc? _proc;
        private Thread? _hookThread;
        private uint _hookThreadId;
        private readonly AutoResetEvent _hookThreadStarted = new(false);
        private volatile bool _stopRequested = false;
        private bool _isInitialized = false;
        private bool _isStarted = false;

        public event Func<KeyboardShortcutEvent, Task>? ShortcutPressed;

        public KeyboardHookService(ILogger<KeyboardHookService> logger, IConfigurationManager configurationManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
        }

        public Task InitializeAsync()
        {
            lock (_lockObject)
            {
                if (_isInitialized)
                    return Task.CompletedTask;

                _proc = HookCallback;
                _isInitialized = true;
            }

            _logger.LogInformation("Keyboard hook service initialized");
            return Task.CompletedTask;
        }

        public Task StartAsync()
        {
            lock (_lockObject)
            {
                if (_isStarted)
                    return Task.CompletedTask;

                if (_proc == null)
                    throw new InvalidOperationException("Keyboard hook callback is not initialized.");

                _stopRequested = false;
                _hookThreadStarted.Reset();
                _hookThread = new Thread(HookThreadMain)
                {
                    IsBackground = true,
                    Name = "KeyboardHookThread"
                };
                _hookThread.Start();

                if (!_hookThreadStarted.WaitOne(TimeSpan.FromSeconds(5)))
                {
                    throw new InvalidOperationException("Keyboard hook thread failed to start.");
                }

                _isStarted = true;
            }

            _logger.LogInformation("Keyboard hook service started with thread id {ThreadId}", _hookThreadId);
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            lock (_lockObject)
            {
                if (!_isStarted)
                    return Task.CompletedTask;

                _stopRequested = true;
                if (_hookThreadId != 0)
                {
                    PostThreadMessage((uint)_hookThreadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
                }
            }

            if (_hookThread != null && !_hookThread.Join(2000))
            {
                _logger.LogWarning("Keyboard hook thread did not stop cleanly");
            }

            lock (_lockObject)
            {
                _hookThread = null;
                _hookThreadId = 0;
                _isStarted = false;
            }

            _logger.LogInformation("Keyboard hook service stopped");
            return Task.CompletedTask;
        }

        public void RegisterShortcut(KeyboardShortcut shortcut)
        {
            if (shortcut == null)
                throw new ArgumentNullException(nameof(shortcut));

            lock (_lockObject)
            {
                var index = _registeredShortcuts.FindIndex(s => s.Id == shortcut.Id);
                if (index >= 0)
                {
                    _registeredShortcuts[index] = shortcut;
                }
                else
                {
                    _registeredShortcuts.Add(shortcut);
                }
            }

            _logger.LogInformation("Registered shortcut: {Shortcut}", shortcut.Combination);
        }

        public void UnregisterShortcut(string shortcutId)
        {
            lock (_lockObject)
            {
                _registeredShortcuts.RemoveAll(s => s.Id == shortcutId);
            }
        }

        public IEnumerable<KeyboardShortcut> GetRegisteredShortcuts()
        {
            lock (_lockObject)
            {
                return _registeredShortcuts.ToList();
            }
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using var currentProcess = Process.GetCurrentProcess();
            using var currentModule = currentProcess.MainModule;
            var moduleHandle = IntPtr.Zero;

            if (currentModule != null)
            {
                moduleHandle = GetModuleHandle(currentModule.ModuleName);
            }

            var hook = SetWindowsHookEx(WH_KEYBOARD_LL, proc, moduleHandle, 0);
            if (hook == IntPtr.Zero)
            {
                var err = Marshal.GetLastWin32Error();
                _logger.LogError("SetWindowsHookEx failed installing keyboard hook. Win32Error={Error}", err);
            }

            return hook;
        }

        private void HookThreadMain()
        {
            _hookThreadId = GetCurrentThreadId();
            _hookId = SetHook(_proc!);
            if (_hookId == IntPtr.Zero)
            {
                _hookThreadStarted.Set();
                return;
            }

            _logger.LogInformation("Keyboard hook installed with hook id {HookId} on thread {ThreadId}", _hookId, _hookThreadId);
            _hookThreadStarted.Set();

            var msg = new MSG();
            while (!_stopRequested && GetMessage(out msg, IntPtr.Zero, 0, 0) != 0)
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }

            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }

            _logger.LogInformation("Keyboard hook thread exiting");
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0 || (wParam != WM_KEYDOWN && wParam != WM_SYSKEYDOWN))
                return CallNextHookEx(_hookId, nCode, wParam, lParam);

            var vkCode = Marshal.ReadInt32(lParam);
            var key = (VirtualKeyCode)vkCode;
            var modifiers = GetCurrentModifiers();

            if (key == VirtualKeyCode.None)
                return CallNextHookEx(_hookId, nCode, wParam, lParam);

            var registeredKeys = new HashSet<VirtualKeyCode>(_registeredShortcuts.Select(s => s.ParseCombination().key));
            if (registeredKeys.Contains(key))
            {
                _logger.LogInformation("Keyboard hook event received: vkCode={VkCode} key={Key} modifiers={Modifiers}", vkCode, key, modifiers);
            }
            else
            {
                _logger.LogDebug("Keyboard hook event ignored: vkCode={VkCode} key={Key} modifiers={Modifiers}", vkCode, key, modifiers);
            }

            List<KeyboardShortcut> shortcuts;
            lock (_lockObject)
            {
                shortcuts = _registeredShortcuts.Where(s => s.Enabled && s.Matches(modifiers, key)).ToList();
            }

            _logger.LogDebug("Registered shortcuts count={Count} matched={Matched}", _registeredShortcuts.Count, shortcuts.Count);

            if (shortcuts.Any())
            {
                var shortcut = shortcuts.OrderByDescending(s => s.Priority).First();
                var shortcutEvent = new KeyboardShortcutEvent
                {
                    Shortcut = shortcut.Combination,
                    ShortcutDefinition = shortcut,
                    Timestamp = DateTime.UtcNow,
                    Modifiers = modifiers,
                    Key = key,
                    State = KeyState.KeyDown
                };

                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (ShortcutPressed != null)
                        {
                            await ShortcutPressed.Invoke(shortcutEvent).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing shortcut event");
                    }
                });

                if (!shortcut.Passthrough)
                    return new IntPtr(1);
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private ModifierKeys GetCurrentModifiers()
        {
            var modifiers = ModifierKeys.None;

            if ((GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0)
                modifiers |= ModifierKeys.Control;
            if ((GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0)
                modifiers |= ModifierKeys.Shift;
            if ((GetAsyncKeyState(VK_MENU) & 0x8000) != 0)
                modifiers |= ModifierKeys.Alt;
            if ((GetAsyncKeyState(VK_LWIN) & 0x8000) != 0 || (GetAsyncKeyState(VK_RWIN) & 0x8000) != 0)
                modifiers |= ModifierKeys.Windows;

            return modifiers;
        }

        public void Dispose()
        {
            StopAsync().Wait();
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int VK_CONTROL = 0x11;
        private const int VK_SHIFT = 0x10;
        private const int VK_MENU = 0x12;
        private const int VK_LWIN = 0x5B;
        private const int VK_RWIN = 0x5C;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostThreadMessage(uint idThread, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        private const int WM_QUIT = 0x0012;

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hWnd;
            public uint message;
            public UIntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
            public uint lPrivate;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }
    }
}
