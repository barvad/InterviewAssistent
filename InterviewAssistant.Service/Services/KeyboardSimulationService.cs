using System.Runtime.InteropServices;
using InterviewAssistant.Core.Interfaces;
using InterviewAssistant.Core.Models;
using Microsoft.Extensions.Logging;

namespace InterviewAssistant.Service.Services;

public class KeyboardSimulationService : IKeyboardSimulationService
{
    private const int INPUT_KEYBOARD = 1;
    const uint KEYEVENTF_KEYDOWN = 0x0000;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint KEYEVENTF_UNICODE = 0x0004;
    private const uint KEYEVENTF_SCANCODE = 0x0008;
    const ushort VK_RETURN = 0x0D;
    private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
    private readonly object _bufferLock = new();
    private readonly Dictionary<string, (string buffer, int index)> _buffers = new();
    private readonly ILogger<KeyboardSimulationService> _logger;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isSimulationActive;

    public KeyboardSimulationService(ILogger<KeyboardSimulationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task InitializeAsync()
    {
        _logger.LogInformation("Keyboard simulation service initialized");
        return Task.CompletedTask;
    }

    public Task StartAsync()
    {
        _logger.LogInformation("Keyboard simulation service started");
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        CancelSimulation();
        _logger.LogInformation("Keyboard simulation service stopped");
        return Task.CompletedTask;
    }

    public async Task ProcessApiResponseAsync(GroqApiResponse apiResponse)
    {
        if (apiResponse == null)
            throw new ArgumentNullException(nameof(apiResponse));

        if (apiResponse.HasError())
        {
            _logger.LogWarning("API response contains error: {Error}", apiResponse.GetErrorMessage());
            return;
        }

        var commands = apiResponse.ExtractCommands();
        if (commands.Any())
        {
            foreach (var command in commands)
            {
                if (_cancellationTokenSource?.IsCancellationRequested ?? false)
                    break;

                if (!string.IsNullOrWhiteSpace(command.Shortcut))
                {
                    await SimulateKeyCombinationAsync(command.Shortcut);
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(command.Text)) await SimulateTextInputAsync(command.Text);
            }
        }
        else
        {
            var content = apiResponse.GetContent();
            if (!string.IsNullOrWhiteSpace(content))
                // Default behavior: simulate full text input when called directly
                await SimulateTextInputAsync(content.Trim());
        }
    }

    public Task StoreBufferAsync(string shortcutId, string content)
    {
        if (string.IsNullOrEmpty(shortcutId) || content == null)
            return Task.CompletedTask;

        lock (_bufferLock)
        {
            _buffers[shortcutId] = (content, 0);
        }

        return Task.CompletedTask;
    }

    public bool HasPendingBuffer(string shortcutId)
    {
        if (string.IsNullOrEmpty(shortcutId))
            return false;

        lock (_bufferLock)
        {
            if (!_buffers.TryGetValue(shortcutId, out var entry))
                return false;

            return entry.index < entry.buffer.Length;
        }
    }

    public Task InsertNextCharacterAsync(string shortcutId)
    {
        if (string.IsNullOrEmpty(shortcutId))
            return Task.CompletedTask;

        char? next = null;
        lock (_bufferLock)
        {
            if (!_buffers.TryGetValue(shortcutId, out var entry))
                return Task.CompletedTask;

            if (entry.index >= entry.buffer.Length)
            {
                _buffers.Remove(shortcutId);
                return Task.CompletedTask;
            }

            next = entry.buffer[entry.index];
            entry.index++;

            if (entry.index >= entry.buffer.Length)
                _buffers.Remove(shortcutId);
            else
                _buffers[shortcutId] = entry;
        }

        if (next.HasValue) return SimulateUnicodeCharacterAsync(next.Value);

        return Task.CompletedTask;
    }

    public async Task SimulateTextInputAsync(string text, int delayBetweenChars = 50)
    {
        if (string.IsNullOrEmpty(text))
            return;

        _isSimulationActive = true;
        _cancellationTokenSource = new CancellationTokenSource();

        foreach (var character in text)
        {
            if (_cancellationTokenSource.IsCancellationRequested)
                break;

            await SimulateUnicodeCharacterAsync(character).ConfigureAwait(false);
            await Task.Delay(delayBetweenChars, _cancellationTokenSource.Token).ConfigureAwait(false);
        }

        _isSimulationActive = false;
    }

    public Task SimulateKeyPressAsync(VirtualKeyCode keyCode, bool shift = false, bool ctrl = false, bool alt = false)
    {
        var inputs = new List<INPUT>();

        if (shift)
            inputs.Add(CreateKeyInput(VirtualKeyCode.Shift, 0));
        if (ctrl)
            inputs.Add(CreateKeyInput(VirtualKeyCode.Control, 0));
        if (alt)
            inputs.Add(CreateKeyInput(VirtualKeyCode.Alt, 0));

        inputs.Add(CreateKeyInput(keyCode, 0));
        inputs.Add(CreateKeyInput(keyCode, KEYEVENTF_KEYUP));

        if (alt)
            inputs.Add(CreateKeyInput(VirtualKeyCode.Alt, KEYEVENTF_KEYUP));
        if (ctrl)
            inputs.Add(CreateKeyInput(VirtualKeyCode.Control, KEYEVENTF_KEYUP));
        if (shift)
            inputs.Add(CreateKeyInput(VirtualKeyCode.Shift, KEYEVENTF_KEYUP));

        SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf<INPUT>());
        return Task.CompletedTask;
    }

    public Task SimulateKeyCombinationAsync(string combination)
    {
        if (string.IsNullOrWhiteSpace(combination))
            return Task.CompletedTask;

        var temp = new KeyboardShortcut(combination, string.Empty, ShortcutAction.CustomAction);
        var (modifiers, key) = temp.ParseCombination();
        var inputs = new List<INPUT>();

        if (modifiers.HasFlag(ModifierKeys.Control))
            inputs.Add(CreateKeyInput(VirtualKeyCode.Control, 0));
        if (modifiers.HasFlag(ModifierKeys.Shift))
            inputs.Add(CreateKeyInput(VirtualKeyCode.Shift, 0));
        if (modifiers.HasFlag(ModifierKeys.Alt))
            inputs.Add(CreateKeyInput(VirtualKeyCode.Alt, 0));
        if (modifiers.HasFlag(ModifierKeys.Windows))
            inputs.Add(CreateKeyInput(VirtualKeyCode.LWin, 0));

        inputs.Add(CreateKeyInput(key, 0));
        inputs.Add(CreateKeyInput(key, KEYEVENTF_KEYUP));

        if (modifiers.HasFlag(ModifierKeys.Windows))
            inputs.Add(CreateKeyInput(VirtualKeyCode.LWin, KEYEVENTF_KEYUP));
        if (modifiers.HasFlag(ModifierKeys.Alt))
            inputs.Add(CreateKeyInput(VirtualKeyCode.Alt, KEYEVENTF_KEYUP));
        if (modifiers.HasFlag(ModifierKeys.Shift))
            inputs.Add(CreateKeyInput(VirtualKeyCode.Shift, KEYEVENTF_KEYUP));
        if (modifiers.HasFlag(ModifierKeys.Control))
            inputs.Add(CreateKeyInput(VirtualKeyCode.Control, KEYEVENTF_KEYUP));

        SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf<INPUT>());
        return Task.CompletedTask;
    }

    public Task SimulateTypingAsync(string text, int wordsPerMinute = 120)
    {
        if (string.IsNullOrEmpty(text))
            return Task.CompletedTask;

        var delay = Math.Max(10, 60000 / (wordsPerMinute * 5));
        return SimulateTextInputAsync(text, delay);
    }

    public bool IsSimulationActive()
    {
        return _isSimulationActive;
    }

    public void CancelSimulation()
    {
        if (_isSimulationActive)
        {
            _cancellationTokenSource?.Cancel();
            _isSimulationActive = false;
        }
    }

    private static INPUT CreateKeyInput(VirtualKeyCode keyCode, uint flags)
    {
        return new INPUT
        {
            type = INPUT_KEYBOARD,
            U = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = (ushort)keyCode,
                    wScan = 0,
                    dwFlags = flags,
                    dwExtraInfo = UIntPtr.Zero,
                    time = 0
                }
            }
        };
    }

    private Task SimulateUnicodeCharacterAsync(char character)
    {
        if (character=='\n' )
        {
            SendPhysicalKey(VK_RETURN);
            return Task.CompletedTask;
        }
        var inputs = new[]
        {
            new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = character,
                        dwFlags = KEYEVENTF_UNICODE,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            },
            new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = character,
                        dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
        return Task.CompletedTask;
    }

    private static void SendPhysicalKey(ushort vKey)
    {
        ushort scanCode = (ushort)MapVirtualKey(vKey, 0);
        ushort shiftScan = (ushort)MapVirtualKey(0x10, 0); // Скан-код клавиши Shift (0x10 = VK_SHIFT)

        INPUT[] inputs = new INPUT[4];

        // 1. Зажимаем Shift
        inputs[0] = new INPUT { type = INPUT_KEYBOARD, U = new InputUnion { ki = new KEYBDINPUT { wVk = 0, wScan = shiftScan, dwFlags = KEYEVENTF_SCANCODE } } };

        // 2. Нажимаем Enter
        inputs[1] = new INPUT { type = INPUT_KEYBOARD, U = new InputUnion { ki = new KEYBDINPUT { wVk = 0, wScan = scanCode, dwFlags = KEYEVENTF_SCANCODE } } };

        // 3. Отпускаем Enter
        inputs[2] = new INPUT { type = INPUT_KEYBOARD, U = new InputUnion { ki = new KEYBDINPUT { wVk = 0, wScan = scanCode, dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP } } };

        // 4. Отпускаем Shift
        inputs[3] = new INPUT { type = INPUT_KEYBOARD, U = new InputUnion { ki = new KEYBDINPUT { wVk = 0, wScan = shiftScan, dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP } } };

        // Отправляем всю комбинацию в один квант времени, чтобы браузер не успел отклонить её
        SendInput(4, inputs, Marshal.SizeOf(typeof(INPUT)));
    }

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public int type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
        [FieldOffset(0)] public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
}