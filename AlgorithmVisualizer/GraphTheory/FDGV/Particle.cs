﻿using System;
using System.Collections.Generic;
using System.Drawing;

using AlgorithmVisualizer.GraphTheory.Utils;
using AlgorithmVisualizer.MathUtils;

namespace AlgorithmVisualizer.GraphTheory.FDGV
{
	public class Particle : GNode
	{
		public Color InnerColor { get; set; }
		public Color BorderColor { get; set; }
		public Color TextColor { get; set; }

		private int textSize = 10;
		private float borderWidth = 1.7f;
		public bool Pinned { get; set; } = false;
		public void TogglePin() => Pinned = !Pinned;

		// Position, velocity and acceleration (2D vectors)
		private Vector pos, vel, acc;
		// Note that vector getter/setters use a copy constructor!
		public Vector Pos { get { return new Vector(pos.X, pos.Y); } set { pos = new Vector(value.X, value.Y); } }
		public Vector Vel { get { return new Vector(vel.X, vel.Y); } set { vel = new Vector(value.X, value.Y); } }
		public Vector Acc { get { return new Vector(acc.X, acc.Y); } set { acc = new Vector(value.X, value.Y); } }

		// Physics related params and their defaults, can be changed from "FDGVConfigForm.cs".
		public const float DefaultSize = 30, DefaultG = 1000f, DefaultMaxSpeed = 10f,
			DefaultMaxCenterPullMag = 0.1f, DefaultVelDecay = 0.99f;
		public static float G, MaxSpeed, MaxCenterPullMag, VelDecay, Size;

		public Particle(int id, int data, Vector _pos) : base(id, data)
		{
			pos = _pos;
			vel = new Vector(0, 0);
			acc = new Vector(0, 0);
			// Use default color scheme
			SetDefaultColors();
		}

		public static void SetDefaultPhysicsParams()
		{
			G = DefaultG;
			MaxSpeed = DefaultMaxSpeed;
			MaxCenterPullMag = DefaultMaxCenterPullMag;
			VelDecay = DefaultVelDecay;
			Size = DefaultSize;
		}
		public void SetDefaultColors()
		{
			// Set particle's colors to defaults
			InnerColor = Colors.ParticleInnerColor;
			BorderColor = Colors.ParticleBorderColor;
			TextColor = Colors.ParticleTextColor;
		}

		public void Draw(Graphics g, int canvasHeight, int canvasWidth)
		{
			// Needed in case of window resize where forces are disabled and particle
			// clips outside of canvas
			BoundWithinCanvas(canvasHeight, canvasWidth);
			using (var innerBrush = new SolidBrush(InnerColor)) Draw(g, innerBrush);
		}
		public void Undraw(Graphics g)
		{
			using (var undrawBrush = new SolidBrush(Colors.Undraw)) Draw(g, undrawBrush);
		}
		private void Draw(Graphics g, SolidBrush brush)
		{
			float radius = Size / 2;
			// Create rectagle centered around x, y
			RectangleF rect = new RectangleF(pos.X - radius, pos.Y - radius, Size, Size);
			// Draw/Undraw particle
			g.FillEllipse(brush, rect);
			// If not undrawing the particle
			if (brush.Color != Colors.Undraw)
			{
				// Draw border
				using (var pen = new Pen(BorderColor, borderWidth)) g.DrawEllipse(pen, rect);
				// Draw node id centered within particle
				using (var textBrush = new SolidBrush(TextColor))
				using (var sf = new StringFormat())
				{
					// Used to center string with sf
					sf.LineAlignment = StringAlignment.Center;
					sf.Alignment = StringAlignment.Center;
					using (Font font = new Font("Arial", textSize))
						g.DrawString(Id.ToString(), font, textBrush, rect, sf);
				}
			}
		}
 
		public void Accelerate(Vector F) => acc += F;
		public void UpdatePos(int canvasHeight, int canvasWidth)
		{
			// Ignore pinned particles
			if (!Pinned)
			{
				// Update velocity using current acceleration and apply force decay
				vel += acc;
				vel *= VelDecay;
				if (vel.Magnitude() > MaxSpeed) vel.SetMagnitude(MaxSpeed);
				// Update the position by adding the velocity to the current position
				pos += vel;
				BoundWithinCanvas(canvasHeight, canvasWidth);
			}
			// Avoid propagation of acceleration between invocations to this method
			acc.Set(0, 0);
		}
		public void BoundWithinCanvas(int canvasHeight, int canvasWidth)
		{
			// Bounds the position vector of this particle within the canvas.
			// The bounding is with respect to the middle point where x, y are
			// always offset by radius from all 4 directions

			float radius = Size / 2;
			// Bound X within canvasWidth
			pos.X = Math.Max(radius, Math.Min(canvasWidth - radius, pos.X));
			// Bound Y within canvasHeight
			pos.Y = Math.Max(radius, Math.Min(canvasHeight - radius, pos.Y));
		}
		public void PullToCenter(Vector centerPos)
		{
			// Vector of full length from pos to centerPos
			Vector F = centerPos - pos;
			// Compute a magnitude and adjust F's mag accordingly
			float mag = Math.Min(F.Magnitude() / G, MaxCenterPullMag);
			F.SetMagnitude(mag);
			// Add force into acc
			acc += F;
		}
		public void ApplyRepulsiveForces(List<Particle> paricleList)
		{
			// Gravitational force formula: F = G * (m1 + m2) / d^2

			// foreach particle in the particle list
			foreach (Particle particle in paricleList)
			{
				// If not this particle
				if (this != particle)
				{
					// Get vector of full magnitude from this.pos to particle.pos
					Vector F = particle.pos - this.pos;
					// If the magnitude is 0 (particle overlap) - randomize F
					if (F.Magnitude() == 0) F = Vector.GetRandom();
					// set F's mag and add into acc
					F.SetMagnitude(G / (F.Magnitude() * F.Magnitude()));
					particle.acc += F;
				}
			}
		}

		public bool PointIsWithin(float x, float y)
		{
			// Check if the given point's coordinates are within the particle(circle)
			// Note: given the center point of a circle and its radius, a given point is
			// within the circle if: (x1-x2)^2 + (y1-y2)^2 <= r^2

			// Compute particle's radius
			float r = Size / 2;
			// Find particle's center point
			float xc = pos.X, yc = pos.Y;
			// Use formula to check if given point (x, y) is within the particle
			float dx = xc - x, dy = yc - y;
			return dx * dx + dy * dy <= r * r;
		}
	}
}

