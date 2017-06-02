using JetBrains.Annotations;

namespace XyrusWorx.Media
{
	[PublicAPI]
	public class ChartCoordinateSystem : CoordinateSystem
	{
		public override double UnitToPixelRatioX => CanvasWidth;
		public override double UnitToPixelRatioY => CanvasHeight;

		public override double UnitToPixelX(double unit) => unit * UnitToPixelRatioX;
		public override double PixelToUnitX(double pixel) => pixel / UnitToPixelRatioX;
		public override double UnitToPixelY(double unit) => (1 - unit) * UnitToPixelRatioY;
		public override double PixelToUnitY(double pixel) => 1 - pixel / UnitToPixelRatioY;
	}
}