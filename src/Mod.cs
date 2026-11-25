using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

namespace CompendiumArcana;

public sealed class Mod : IMod
{
    private const string SupportedHash = "0F327C7669E6CAC72AAF355181B1E51EDD12FEE4D71709FC1802DBF0DA70546E";
    private readonly List<IAsmHook> hooks = new();
    public Action Disposing => () => { };
    public bool CanUnload() => false;
    public bool CanSuspend() => false;
    public void Suspend() { }
    public void Resume() { }
    public void Unload() { }
    public void StartEx(IModLoaderV1 loader, IModConfigV1 config) => Start(loader);

    public void Start(IModLoaderV1 loader)
    {
        if (hooks.Count != 0) return;
        var log = loader.GetLogger();
        try
        {
            using var process = Process.GetCurrentProcess();
            var module = process.MainModule!;
            using var exe = File.OpenRead(module.FileName);
            if (Convert.ToHexString(SHA256.HashData(exe)) != SupportedHash)
                throw new InvalidOperationException("Unsupported P4G.exe build.");
            if (!loader.GetController<IReloadedHooks>().TryGetTarget(out var api))
                throw new InvalidOperationException("Reloaded.Hooks unavailable.");

            (string name, int rva, string original)[] sites = [
                ("preview", 0x1aecfd, "488B157C7EFF00"),
                ("points", 0x1aeed2, "488B15A77CFF00"),
                // The loader can emit a 7-byte indirect jump. Replay both complete 5-byte loads.
                ("display", 0x1b0dc2, "488B5C2450488B6C2458")
            ];
            foreach (var (name, rva, original) in sites)
            {
                var address = module.BaseAddress + rva;
                var expected = Convert.FromHexString(original);
                byte[] actual = new byte[expected.Length];
                Marshal.Copy(address, actual, 0, actual.Length);
                if (!actual.SequenceEqual(expected))
                    throw new InvalidOperationException($"Unexpected instructions at {name}.");

                using var resource = typeof(Mod).Assembly.GetManifestResourceStream($"CompendiumArcana.{name}.bin")!;
                byte[] code = new byte[resource.Length];
                resource.ReadExactly(code);
                var slot = code.AsSpan().IndexOf(Convert.FromHexString("F0DEBC9A78563412"));
                BinaryPrimitives.WriteInt64LittleEndian(code.AsSpan(slot), module.BaseAddress.ToInt64());
                hooks.Add(api.CreateAsmHook(code, address.ToInt64(), AsmHookBehaviour.ExecuteFirst, expected.Length));
            }
            foreach (var hook in hooks) hook.Activate();
            log.PrintMessage("[Compendium Arcana] Enabled.", log.ColorGreen);
        }
        catch (Exception error)
        {
            foreach (var hook in hooks.Where(h => h.IsEnabled)) hook.Disable();
            log.PrintMessage($"[Compendium Arcana] Disabled. {error.Message}", log.ColorRed);
        }
    }
}
