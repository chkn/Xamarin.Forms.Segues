using System;
using System.Threading.Tasks;

using UIKit;
using CoreGraphics;
using CoreAnimation;

using Xamarin.Forms.Segues;

namespace Sample {

	// Port of "Gate Open Inside" and "Gate Close Inside" from https://github.com/mathebox/MBSegueCollection

	public class GateSegue : PlatformSegue {

		protected override Task ExecuteAsync (UIViewController destination)
		{
			if (Action != SegueAction.Pop) {
				// Gate open
				UIView destinationViewSnapshot = GetDestinationViewSnapshot (destination);
				UIView leftSide = GetSourceViewSnapshot ();
				UIView rightSide = GetSourceViewSnapshot ();

				MaskLeftSideOfView (rightSide);
				MaskRightSideOfView (leftSide);

				SetAnchorPoint (new CGPoint (0, 0.5), leftSide);
				SetAnchorPoint (new CGPoint (1, 0.5), rightSide);

				NativeSource.View.AddSubview (destinationViewSnapshot);
				NativeSource.View.AddSubview (leftSide);
				NativeSource.View.AddSubview (rightSide);

				var tcs = new TaskCompletionSource<object> ();
				UIView.Animate (1d, 0.25d, UIViewAnimationOptions.CurveEaseIn, () => {
					CATransform3D t = CATransform3D.Identity;
					t.m34 = (nfloat)(1.0 / 500); // set perspective
					leftSide.Layer.Transform = t.Rotate ((nfloat)(-Math.PI/2), 0.0f, 1.0f, 0.0f);
					rightSide.Layer.Transform = t.Rotate ((nfloat)(Math.PI/2), 0.0f, 1.0f, 0.0f);
				}, async () => {
					// Actually effect the navigation
					await base.ExecuteAsync (destination);

					rightSide.RemoveFromSuperview ();
					leftSide.RemoveFromSuperview ();
					destinationViewSnapshot.RemoveFromSuperview ();

					tcs.SetResult (null);
				});
				return tcs.Task;
			} else {
				// Gate close
				UIView leftSide = GetDestinationViewSnapshot (destination);
				UIView rightSide = GetDestinationViewSnapshot (destination);

				MaskLeftSideOfView (rightSide);
				MaskRightSideOfView (leftSide);

				SetAnchorPoint (new CGPoint (0, 0.5), leftSide);
				SetAnchorPoint (new CGPoint (1, 0.5), rightSide);

				CATransform3D t = CATransform3D.Identity;
				t.m34 = (nfloat)(1.0 / 500); // set perspective
				leftSide.Layer.Transform = t.Rotate ((nfloat)(-Math.PI/2), 0.0f, 1.0f, 0.0f);
				rightSide.Layer.Transform = t.Rotate ((nfloat)(Math.PI/2), 0.0f, 1.0f, 0.0f);

				NativeSource.View.AddSubview (leftSide);
				NativeSource.View.AddSubview (rightSide);

				var tcs = new TaskCompletionSource<object> ();
				UIView.Animate (1d, 0.25d, UIViewAnimationOptions.CurveEaseOut, () => {
					leftSide.Layer.Transform = CATransform3D.Identity;
					rightSide.Layer.Transform = CATransform3D.Identity;
				}, async () => {
					// Actually effect the navigation
					await base.ExecuteAsync (destination);

					rightSide.RemoveFromSuperview ();
					leftSide.RemoveFromSuperview ();

					tcs.SetResult (null);
				});
				return tcs.Task;
			}
		}

		UIView GetSourceViewSnapshot ()
			=> NativeSource.View.SnapshotView (false);

		static UIView GetDestinationViewSnapshot (UIViewController destination)
		{
			UIGraphics.BeginImageContextWithOptions (destination.View.Bounds.Size, false, 0);
			destination.View.DrawViewHierarchy (destination.View.Bounds, true);
			var destinationViewImage = UIGraphics.GetImageFromCurrentImageContext ();
			UIGraphics.EndImageContext ();
			return new UIImageView (destinationViewImage);
		}

		static void MaskView (UIView view, CGRect rect)
		{
			var mask = new CAShapeLayer ();
			var path = CGPath.FromRect (rect);
			mask.Path = path;
			view.Layer.Mask = mask;
		}

		static void MaskLeftSideOfView (UIView view)
		{
		    CGRect rect = new CGRect (view.Bounds.Size.Width/2, view.Bounds.Location.Y,
		                             view.Bounds.Size.Width/2, view.Bounds.Size.Height);
		    MaskView (view, rect);
		}

		static void MaskRightSideOfView (UIView view)
		{
		    CGRect rect = new CGRect (view.Bounds.Location.X, view.Bounds.Location.Y,
		                             view.Bounds.Size.Width/2, view.Bounds.Size.Height);
		    MaskView (view, rect);
		}

		static void SetAnchorPoint (CGPoint anchorPoint, UIView view)
		{
		    CGPoint newPoint = new CGPoint (view.Bounds.Size.Width * anchorPoint.X,
		                                   view.Bounds.Size.Height * anchorPoint.Y);
		    CGPoint oldPoint = new CGPoint (view.Bounds.Size.Width * view.Layer.AnchorPoint.X,
		                                   view.Bounds.Size.Height * view.Layer.AnchorPoint.Y);

		    newPoint = view.Transform.TransformPoint (newPoint);
		    oldPoint = view.Transform.TransformPoint (oldPoint);

		    CGPoint position = view.Layer.Position;

		    position.X -= oldPoint.X;
		    position.X += newPoint.X;

		    position.Y -= oldPoint.Y;
		    position.Y += newPoint.Y;

		    view.Layer.Position = position;
		    view.Layer.AnchorPoint = anchorPoint;
		}
	}
}
