using System;
using Microsoft.Xna.Framework;

namespace Torq2.Graphics.Cameras
{
	public class Curve3D
	{
		public Curve curveX = new Curve();
		public Curve curveY = new Curve();
		public Curve curveZ = new Curve();

		public Curve3D()
		{
			curveX.PostLoop = CurveLoopType.CycleOffset;
			curveY.PostLoop = CurveLoopType.Oscillate;
			curveZ.PostLoop = CurveLoopType.Oscillate;

			curveX.PreLoop = CurveLoopType.Oscillate;
			curveY.PreLoop = CurveLoopType.Oscillate;
			curveZ.PreLoop = CurveLoopType.Oscillate;
		}

		public void AddPoint(Vector3 tPoint, float fTime)
		{
			curveX.Keys.Add(new CurveKey(fTime, tPoint.X));
			curveY.Keys.Add(new CurveKey(fTime, tPoint.Y));
			curveZ.Keys.Add(new CurveKey(fTime, tPoint.Z));
		}

		public void SetTangents()
		{
			CurveKey prev;
			CurveKey current;
			CurveKey next;
			int prevIndex;
			int nextIndex;
			for (int i = 0; i < curveX.Keys.Count; i++)
			{
				prevIndex = i - 1;
				if (prevIndex < 0) prevIndex = i;

				nextIndex = i + 1;
				if (nextIndex == curveX.Keys.Count) nextIndex = i;

				prev = curveX.Keys[prevIndex];
				next = curveX.Keys[nextIndex];
				current = curveX.Keys[i];
				SetCurveKeyTangent(ref prev, ref current, ref next);
				curveX.Keys[i] = current;
			}
		}

		private void SetCurveKeyTangent(ref CurveKey prev, ref CurveKey cur, ref CurveKey next)
		{
			float dt = next.Position - prev.Position;
			float dv = next.Value - prev.Value;
			if (Math.Abs(dv) < float.Epsilon)
			{
				cur.TangentIn = 0;
				cur.TangentOut = 0;
			}
			else
			{
				// The in and out tangents should be equal to the slope between the adjacent keys
				cur.TangentIn = dv * (cur.Position - prev.Position) / dt;
				cur.TangentOut = dv * (next.Position - cur.Position) / dt;
			}
		}

		public Vector3 GetPointOnCurve(float time)
		{
			Vector3 point = new Vector3();
			point.X = curveX.Evaluate(time);
			point.Y = curveY.Evaluate(time);
			point.Z = curveZ.Evaluate(time);
			return point;
		}
	}

	public class ScriptedCamera : Camera
	{
		private Curve3D m_pCurvePosition;
		private Curve3D m_pCurveLookAt;

		private Vector3 m_tPosition;
		private Vector3 m_tLookAt;
		private readonly Vector3 m_tUp;

		public ScriptedCamera(Game pGame)
			: base()
		{
			m_pCurvePosition = new Curve3D();
			m_pCurveLookAt = new Curve3D();

			float time = 0;
			m_pCurvePosition.AddPoint(new Vector3(10, 10, 140), time);
			m_pCurveLookAt.AddPoint(new Vector3(1000, 1000, 100), time);
			time += 2000;
			m_pCurvePosition.AddPoint(new Vector3(120, 0, 130), time);
			time += 2000;
			m_pCurvePosition.AddPoint(new Vector3(130, 300, 120), time);
			time += 2000;
			m_pCurvePosition.AddPoint(new Vector3(1000, 1000, 200), time);
			time += 2000;
			m_pCurvePosition.AddPoint(new Vector3(12f, 0, 90), time);
			time += 2000;

			m_pCurvePosition.SetTangents();
			m_pCurveLookAt.SetTangents();

			m_tUp = new Vector3(0, 0, 1);
		}

		public override void Update(GameTime gameTime)
		{
			m_tPosition = m_pCurvePosition.GetPointOnCurve((float) gameTime.TotalRealTime.TotalMilliseconds);
			m_tLookAt = m_pCurveLookAt.GetPointOnCurve((float) gameTime.TotalRealTime.TotalMilliseconds);

			m_tView = Matrix.CreateLookAt(m_tPosition, m_tLookAt, m_tUp);
		}
	}
}
