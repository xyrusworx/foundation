using System;
using JetBrains.Annotations;

namespace XyrusWorx.Media
{
	[PublicAPI]
	public class GridCoordinateSystem : CoordinateSystem
	{
		private const double mBaseUnitToPixelRatio = 150.0;

		private double mBaseOffsetX;
		private double mBaseOffsetY;

		public double OffsetX { get; private set; }
		public double OffsetY { get; private set; }

		public double ZoomRatio { get; private set; } = 1.0;
		public override double LogarithmicScale => Math.Pow(10, -Math.Round(Math.Log10(ZoomRatio)));

		public override double UnitToPixelRatioX => mBaseUnitToPixelRatio * ZoomRatio;
		public override double UnitToPixelRatioY => mBaseUnitToPixelRatio * ZoomRatio;

		public override double UnitToPixelX(double unit) => (unit - OffsetX) * UnitToPixelRatioX + mBaseOffsetX;
		public override double PixelToUnitX(double pixel) => (pixel - mBaseOffsetX) / UnitToPixelRatioX + OffsetX;
		public override double UnitToPixelY(double unit) => (unit + OffsetY) * -UnitToPixelRatioY + mBaseOffsetY;
		public override double PixelToUnitY(double pixel) => (pixel - mBaseOffsetY) / -UnitToPixelRatioY - OffsetY;

		protected override void ResizeOverride(double pixelWidth, double pixelHeight)
		{
			mBaseOffsetX = pixelWidth / 2.0;
			mBaseOffsetY = pixelHeight / 2.0;
		}

		public void Zoom(double delta, double cursorX, double cursorY)
		{
			if ((Math.Round(Math.Log10(ZoomRatio * 1.25)) > 12 && delta > 0) ||
				(Math.Round(Math.Log10(ZoomRatio * 0.8)) < -12) && delta < 0)
			{
				return;
			}

			ZoomRatio *= delta > 0 ? 1.25 : 0.8;
		}
		public void Pan(double offsetX, double offsetY)
		{
			OffsetX = offsetX;
			OffsetY = offsetY;
		}
	}
}
