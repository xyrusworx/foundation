using System;
using JetBrains.Annotations;

namespace XyrusWorx.Media
{
	[PublicAPI]
	public abstract class CoordinateSystem
	{
		public abstract double UnitToPixelRatioX { get; }
		public abstract double UnitToPixelRatioY { get; }

		public double CanvasWidth { get; private set; }
		public double CanvasHeight { get; private set; }

		public void Resize(double pixelWidth, double pixelHeight)
		{
			CanvasWidth = pixelWidth;
			CanvasHeight = pixelHeight;

			ResizeOverride(pixelWidth, pixelHeight);
		}
		protected virtual void ResizeOverride(double pixelWidth, double pixelHeight)
		{
		}

		public abstract double UnitToPixelX(double unit);
		public abstract double PixelToUnitX(double pixel);
		public abstract double UnitToPixelY(double unit);
		public abstract double PixelToUnitY(double pixel);

		public virtual double LogarithmicScale => 1.0;

		public double Snap(double u, double l, SnapBehavior snapBehavior = SnapBehavior.Round)
		{
			Func<double, double> transform;
			switch (snapBehavior)
			{
				case SnapBehavior.Round:
					transform = Math.Round;
					break;
				case SnapBehavior.Floor:
					transform = Math.Floor;
					break;
				case SnapBehavior.Ceil:
					transform = Math.Ceiling;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(snapBehavior));
			}

			return transform(u / l) * l;
		}
		public bool IsPointVisible(double unitX, double unitY)
		{
			var pixelX = UnitToPixelX(unitX);
			var pixelY = UnitToPixelY(unitY);

			return pixelX >= 0 && pixelY >= 0 &&
			       pixelX < CanvasWidth &&
			       pixelY < CanvasHeight;
		}
	}
}