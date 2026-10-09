using System;
using Shiftbound;
using UnityEngine;

class Program
{
    static int checks;
    static void Require(bool pass, string message) { if (!pass) throw new Exception(message); checks++; }
    static void Main()
    {
        var r = new TouchInputRouter {
            Stick = new Rect(30, 480, 192, 192), Jump = new Rect(1120, 540, 124, 124),
            Shift = new Rect(990, 564, 100, 100), Camera = new Rect(550, 100, 730, 620)
        };
        r.BeginFrame(); r.Begin(1, r.Stick.center + new Vector2(0, -64)); r.Begin(2, r.Jump.center);
        Require(r.Move.y > .9f && r.JumpHeld && r.JumpPressed, "simultaneous forward and held jump");
        r.Begin(3, r.Shift.center); Require(r.ShiftPressed && r.JumpHeld && r.Move.y > .9f, "three simultaneous production intents");
        r.BeginFrame(); r.Drag(3, r.Jump.center);
        Require(!r.ShiftPressed && !r.JumpPressed && r.JumpHeld && r.Look == Vector2.zero, "owned Shift drag cannot retrigger or rotate");
        r.Drag(2, new Vector2(650, 300)); Require(r.Look == Vector2.zero && r.JumpHeld, "jump owns contact outside button");
        r.Begin(4, new Vector2(750, 300)); r.Drag(4, new Vector2(780, 310));
        Require(r.Look == new Vector2(30, 10) && r.JumpHeld, "independent manual orbit");
        r.BeginFrame(); r.End(2); Require(r.JumpReleased && !r.JumpHeld && r.Move.y > .9f, "release preserves movement and variable-height intent");
        r.End(4); r.Begin(2, r.Jump.center); r.Begin(5, r.Jump.center); r.BeginFrame(); r.Drag(5, new Vector2(700, 300));
        Require(r.Look == Vector2.zero && r.JumpHeld, "occupied Jump contact is reserved, cannot steal orbit");
        r.End(5); Require(r.JumpHeld, "ignored pointer cannot release owner's jump");
        r.End(1); Require(r.Move == Vector2.zero && r.JumpHeld, "movement release isolated");
        r.Reset(); Require(r.ContactCount == 0 && r.Move == Vector2.zero && !r.JumpHeld && !r.JumpPressed && !r.ShiftPressed && r.Look == Vector2.zero,
            "cancel/focus reset clears all state without involuntary release action");
        r.BeginFrame(); r.Begin(8, new Vector2(500, 20)); r.Drag(8, r.Shift.center);
        Require(!r.ShiftPressed && r.Look == Vector2.zero, "ignored UI contact cannot acquire a gameplay role by dragging");
        r.Begin(9, r.Stick.center); Require(r.Move == Vector2.zero, "stick center deadzone");
        r.Drag(9, new Vector2(-900, -900)); Require(r.Move.magnitude <= 1.00001f, "stick clamps full displacement");
        r.Reset(); r.Begin(10, r.Jump.center); r.End(10);
        Require(r.JumpPressed && r.JumpReleased && !r.JumpHeld, "same-frame tap retains press and release");
        r.Reset(); r.Begin(20, r.Stick.center + new Vector2(0,-64)); r.Begin(21, r.Jump.center); r.BeginFrame();
        r.Drag(21, new Vector2(r.Shift.x+2,r.Shift.center.y));
        Require(!r.ShiftPressed && r.JumpHeld, "Shift outer edge does not accidentally trigger slide chord");
        r.Drag(21, r.Shift.center);
        Require(r.ShiftPressed && r.JumpHeld && r.Move.y>.9f && r.Look==Vector2.zero, "two-thumb Jump-to-Shift chord preserves held jump and move");
        r.BeginFrame(); r.Drag(21,r.Jump.center); r.Drag(21,r.Shift.center);
        Require(!r.ShiftPressed && r.JumpHeld, "slide chord fires once per held jump");
        r.End(21); Require(r.JumpReleased && !r.JumpHeld, "slide contact release still controls jump height");
        Console.WriteLine("TOUCH INPUT CHECKS PASSED: " + checks + " assertions against production router. No physical-touch claim.");
    }
}
