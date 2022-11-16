using Mars.Interfaces.Environments;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitySim.Backend
{
    public record struct Position3D(float X, float Y, float Z = 0);
    public record struct RGBAColor(byte R, byte G, byte B, byte A = 255)
    {
        public static readonly RGBAColor Red =     new (255,   0,   0);
        public static readonly RGBAColor Green =   new (  0, 255,   0);
        public static readonly RGBAColor Blue =    new (  0,   0, 255);
        public static readonly RGBAColor Yellow =  new (255, 255,   0);
        public static readonly RGBAColor Cyan =    new (  0, 255, 255);
        public static readonly RGBAColor Magenta = new (255,   0, 255);
        public static readonly RGBAColor White =   new (255, 255, 255);
        public static readonly RGBAColor Black =   new (  0,   0,   0);
    }

    public interface IFrontendDebugger
    {
        public void SuspendRedraw();
        public void ResumeRedraw();
        public void AddMarkerPoint(Position3D position,
            float radius, RGBAColor color);
        public void ClearDebugMarkers();
    }

    public static class FrontendDebugger
    {
        private static Process? s_debugger;

        public static void Register(Process debugger)
        {
            s_debugger = debugger;
        }

        public static void AddMarkerPoint(Position position,
            float radius, RGBAColor color) => 
            AddMarkerPoint(new Position3D((float)position.X, (float)position.Y), radius, color);

        public static void AddMarkerPoint(Position3D position,
            float radius, RGBAColor color)
        {
            var message = $"Pnt {position.X} {position.Y} {position.Z} {radius} " +
                $"{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";

            Console.WriteLine(message);

            s_debugger!.StandardInput.WriteLine(message);
        }

        public static void AddMarkerLine(Position3D positionA, Position3D positionB,
            float thickness, RGBAColor color)
        {
            var message = $"Lin {positionA.X} {positionA.Y} {positionA.Z} {positionB.X} {positionB.Y} {positionB.Z} " +
                $"{thickness} {color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";

            Console.WriteLine(message);

            s_debugger!.StandardInput.WriteLine(message);
        }

        public static void ClearDebugMarkers() => s_debugger!.StandardInput.WriteLine("Clr");
    }
}
