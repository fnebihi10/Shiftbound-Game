using System;
using Shiftbound;

internal static class Program
{
    private static int assertions;
    private static void Require(bool value, string label)
    {
        if (!value) throw new Exception(label);
        assertions++;
    }

    private static void Main()
    {
        var input = new JumpIntent();
        input.Advance(0.02f, true, true, false, 0.14f, 0.14f);
        Require(input.TryConsume(out bool held) && held, "held takeoff");
        Require(!input.TryConsume(out _), "one consume per press");
        input.Advance(0.02f, false, false, false, 0.14f, 0.14f);
        Require(!input.TryConsume(out _), "holding never repeats");
        input.Reset();
        input.Advance(0.02f, true, true, true, 0.14f, 0.14f);
        Require(input.TryConsume(out held) && !held, "tap takeoff");
        input.Reset();
        input.Advance(0.03125f, false, true, false, 0.125f, 0.125f);
        Require(!input.TryConsume(out _), "air press waits for support");
        input.Advance(0.03125f, false, false, true, 0.125f, 0.125f);
        input.Advance(0.03125f, true, false, false, 0.125f, 0.125f);
        Require(input.TryConsume(out held) && !held, "buffer remembers release");
        input.Reset();
        input.Advance(0.03125f, false, true, false, 0.125f, 0.125f);
        input.Advance(0.125f, false, false, true, 0.125f, 0.125f);
        input.Advance(0.03125f, true, false, false, 0.125f, 0.125f);
        Require(!input.TryConsume(out _), "buffer expires at boundary");
        input.Reset();
        input.Advance(0.03125f, true, false, false, 0.125f, 0.125f);
        input.Advance(0.0625f, false, true, false, 0.125f, 0.125f);
        Require(input.TryConsume(out _), "coyote before boundary");
        input.Reset();
        input.Advance(0.03125f, true, false, false, 0.125f, 0.125f);
        input.Advance(0.125f, false, true, false, 0.125f, 0.125f);
        Require(!input.TryConsume(out _), "coyote expires at boundary");
        input.Reset();
        input.Advance(0.02f, true, true, false, 0f, 0f);
        Require(input.TryConsume(out _), "zero forgiveness still permits direct grounded jump");
        input.Reset();
        Require(!input.TryConsume(out _) && !input.Held, "reset clears intent");
        Console.WriteLine("JUMP INTENT CHECKS PASSED: " + assertions + " assertions against production source.");
    }
}
