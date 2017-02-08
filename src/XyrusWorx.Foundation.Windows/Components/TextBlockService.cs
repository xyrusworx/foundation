using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI]
	public static class TextBlockService
	{
		static TextBlockService()
		{
			// Register for the SizeChanged event on all TextBlocks, even if the event was handled.
			EventManager.RegisterClassHandler(
				typeof(TextBlock),
				FrameworkElement.SizeChangedEvent,
				new SizeChangedEventHandler(OnTextBlockSizeChanged),
				true);
		}

		public static readonly DependencyProperty IsTextTrimmedProperty =
			DependencyProperty.RegisterAttached("IsTextTrimmed",
				typeof(bool),
				typeof(TextBlockService),
				new PropertyMetadata(false));


		[AttachedPropertyBrowsableForType(typeof(TextBlock))]
		public static bool GetIsTextTrimmed(TextBlock target)
		{
			return (bool) target.GetValue(IsTextTrimmedProperty);
		}

		public static void SetIsTextTrimmed(TextBlock target, bool value)
		{
			target.SetValue(IsTextTrimmedProperty, value);
		}

		public static readonly DependencyProperty AutomaticToolTipEnabledProperty = DependencyProperty.RegisterAttached(
			"AutomaticToolTipEnabled",
			typeof(bool),
			typeof(TextBlockService),
			new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));

		[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
		public static bool GetAutomaticToolTipEnabled(DependencyObject element)
		{
			if (null == element)
			{
				throw new ArgumentNullException(nameof(element));
			}
			return (bool) element.GetValue(AutomaticToolTipEnabledProperty);
		}

		public static void SetAutomaticToolTipEnabled(DependencyObject element, bool value)
		{
			if (null == element)
			{
				throw new ArgumentNullException(nameof(element));
			}
			element.SetValue(AutomaticToolTipEnabledProperty, value);
		}

		private static void OnTextBlockSizeChanged(object sender, SizeChangedEventArgs e)
		{
			TriggerTextRecalculation(sender);
		}

		private static void TriggerTextRecalculation(object sender)
		{
			var textBlock = sender as TextBlock;
			if (null == textBlock)
			{
				return;
			}

			if (TextTrimming.None == textBlock.TextTrimming)
			{
				textBlock.SetCurrentValue(IsTextTrimmedProperty, false);
			}
			else
			{
				//If this function is called before databinding has finished the tooltip will never show.
				//This invoke defers the calculation of the text trimming till after all current pending databinding
				//has completed.
				var isTextTrimmed = textBlock.Dispatcher.Invoke(() => CalculateIsTextTrimmed(textBlock), DispatcherPriority.DataBind);
				textBlock.SetCurrentValue(IsTextTrimmedProperty, isTextTrimmed);
			}
		}

		private static bool CalculateIsTextTrimmed(TextBlock textBlock)
		{
			if (!textBlock.IsArrangeValid)
			{
				return GetIsTextTrimmed(textBlock);
			}

			var typeface = new Typeface(
				textBlock.FontFamily,
				textBlock.FontStyle,
				textBlock.FontWeight,
				textBlock.FontStretch);

#pragma warning disable 618
			var formattedText = new FormattedText(
#pragma warning restore 618
				textBlock.Text,
				System.Threading.Thread.CurrentThread.CurrentCulture,
				textBlock.FlowDirection,
				typeface,
				textBlock.FontSize,
				textBlock.Foreground);

			formattedText.MaxTextWidth = textBlock.ActualWidth;

			// When the maximum text width of the FormattedText instance is set to the actual
			// width of the textBlock, if the textBlock is being trimmed to fit then the formatted
			// text will report a larger height than the textBlock. Should work whether the
			// textBlock is single or multi-line.
			// The width check detects if any single line is too long to fit within the text area, 
			// this can only happen if there is a long span of text with no spaces.
			return (formattedText.Height > textBlock.ActualHeight || formattedText.MinWidth > formattedText.MaxTextWidth);
		}

		
	}
}