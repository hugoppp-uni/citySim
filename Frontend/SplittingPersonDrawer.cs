using CitySim.Frontend.Helpers;
using Raylib_CsLo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Raylib_CsLo.Raylib;

namespace CitySim.Frontend
{
    internal static class SplittingPersonDrawer
    {
        //Shape Parameters
        static Vector2 s_headPosition = new Vector2(0, -40);

        const float HeadRadius = 10;

        const float BodyLeft = -10;
        const float BodyRight = 10;
        const float BodyTop = -25;
        const float BodyBottom = 0;

        const float BodyCornerRadius = 5;


        //Resolution Parameters
        const int Circle_VertexCount = 128;
        const float RowHeight = 0.5f;


        //VertexBuffers
        static Vector2[] headVerts;
        static Vector2[] headVertsDynamic;

        static Vector2[] bodyVerts;
        static Vector2[] bodyVertsDynamic;

        static SplittingPersonDrawer()
        {
            headVerts = new Vector2[Circle_VertexCount + 2];
            headVertsDynamic = new Vector2[Circle_VertexCount + 2];

            headVerts[0] = s_headPosition;

            for (int i = 0; i < Circle_VertexCount + 1; i++)
            {
                float a = MathF.PI * 2 / Circle_VertexCount * i;
                var (sin, cos) = MathF.SinCos(a);
                headVerts[i + 1] = s_headPosition + new Vector2(sin, cos) * HeadRadius;
            }


            float RowHeight = 0.5f;

            int Rows = (int)((BodyBottom - BodyTop) / RowHeight);

            bodyVerts = new Vector2[Rows * 2 + 2];
            bodyVertsDynamic = new Vector2[Rows * 2 + 2];

            for (int i = 0; i < Rows + 1; i++)
            {
                float t;

                float y = BodyTop + RowHeight * i;
                if (y < BodyTop + BodyCornerRadius)
                    t = 1 - (y - BodyTop) / BodyCornerRadius;

                else if (y > BodyBottom - BodyCornerRadius)
                    t = 1 - (BodyBottom - y) / BodyCornerRadius;
                else
                    t = 0;

                float d = 1 - MathF.Sqrt(1 - t * t);

                bodyVerts[i * 2] = new Vector2(BodyRight - d * BodyCornerRadius, y);
                bodyVerts[i * 2 + 1] = new Vector2(BodyLeft + d * BodyCornerRadius, y);

            }
        }

        public static void Draw(float progress, Vector2 posAStart, Vector2 posAEnd, Color headColorA, Color bodyColorA,
                                                Vector2 posBStart, Vector2 posBEnd, Color headColorB, Color bodyColorB)
        {
            float pealProgress = MathF.Min(progress, 0.7f) / 0.7f;

            float splitProgress = Math.Clamp(progress - 0.7f, 0, 0.3f) / 0.3f;

            float splitAnim = Util.EaseOutElastic(splitProgress);

            Vector2 posA = Vector2.Lerp(
                posAStart,
                posAEnd,
                splitAnim);

            Vector2 posB = Vector2.Lerp(
                posBStart,
                posBEnd,
                splitAnim);

            unsafe
            {
                Color _bodyColorB = splitProgress > 0 ? bodyColorB : bodyColorA;


                float time = (float)GetTime();

                Vector2 VertexTransformation(Vector2 pos)
                {
                    float headTop = -150;

                    float t_y = (pos.Y - BodyBottom) / (BodyBottom - headTop);

                    float t1 = pealProgress;
                    float t2 = splitAnim;


                    float a = t_y * t_y * t1 * (50 + MathF.Sin(time * 50) * 4);
                    float b = 0;

                    return pos + new Vector2(
                        a * (1 - t2) + b * t2,
                        0);
                }

                Vector2 mirrorX = new Vector2(-1, 1);


                for (int i = 0; i < headVertsDynamic.Length; i++)
                {
                    headVertsDynamic[i] =
                        VertexTransformation(headVerts[i] * mirrorX) * mirrorX + posA;
                }

                fixed (Vector2* hvp = headVertsDynamic)
                {
                    DrawTriangleFan(hvp, headVerts.Length, headColorA);
                }


                for (int i = 0; i < headVertsDynamic.Length; i++)
                {
                    headVertsDynamic[i] =
                        VertexTransformation(headVerts[i]) + posB;
                }

                fixed (Vector2* hvp = headVertsDynamic)
                {
                    DrawTriangleFan(hvp, headVerts.Length, headColorB);
                }



                for (int i = 0; i < bodyVertsDynamic.Length; i++)
                {
                    bodyVertsDynamic[i] =
                        VertexTransformation(bodyVerts[i] * mirrorX) * mirrorX + posA;
                }

                fixed (Vector2* bvp = bodyVertsDynamic)
                {
                    DrawTriangleStrip(bvp, bodyVerts.Length, bodyColorA);
                }



                for (int i = 0; i < bodyVertsDynamic.Length; i++)
                {
                    bodyVertsDynamic[i] =
                        VertexTransformation(bodyVerts[i]) + posB;
                }

                fixed (Vector2* bvp = bodyVertsDynamic)
                {
                    DrawTriangleStrip(bvp, bodyVerts.Length, _bodyColorB);
                }
            }
        }
    }
}
