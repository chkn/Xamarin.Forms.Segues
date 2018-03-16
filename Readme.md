# Xamarin.Forms.Segues

A library that provides support for segues between Pages.

## Segue what?

Xamarin.Forms provides navigation through the `INavigation` interface. Xamarin.Forms.Segues wraps [many navigation actions](https://github.com/chkn/Xamarin.Forms.Segues/blob/master/Xamarin.Forms.Segues/SegueAction.cs) from `INavigation`, exposing them as `ISegue`s through the `Segue` class. Becuase `ISegue` derives from `ICommand`, segues can simply be assigned to anything that has a `Command` property, reducing codebehind.

### Segue from XAML

Simply add this xmlns to the root element of your XAML:

```
xmlns:s="clr-namespace:Xamarin.Forms.Segues;assembly=Xamarin.Forms.Segues"
```

Then, you can add a segue to anything that has a `Command` property, such as a `Button`. The `CommandParameter` property is used to supply the `Page` that will be the destination of the segue. This example assumes you have a class called `NextPage` in your project:

```XML
<Button Text="Go to next page"
        Command="{s:Segue Push}">
    <Button.CommandParameter>
		<local:NextPage />
    </Button.CommandParameter>
</Button>
```

Note that the `{s:Segue}` markup extension creates and configures the `Segue` object. In this example, we specify a `Push` segue, but you could also specify any of the other supported [actions](https://github.com/chkn/Xamarin.Forms.Segues/blob/master/Xamarin.Forms.Segues/SegueAction.cs).

### Segue from Code

Creating and configuring a segue from code is not much harder. [Here is an example.](https://github.com/chkn/Xamarin.Forms.Segues/blob/master/Sample/Shared/Pages/SeguePage.xaml.cs#L16)

## Custom Segues

The default animations are good in many cases, but sometimes you will want to create a custom transition between screens. You can do this with a custom segue.

### Cross-Platform Segues

Xamarin.Forms provides many powerful APIs for animation. Using these, you can create a cross-platform custom segue by simply subclassing the `Segue` class. Here's a basic template:

```csharp
public class MyCustomSegue : Segue {
    protected override async Task ExecuteAsync (Page destination)
    {
        // 1. Set the desired properties on the destination page

        // 2. Animate the source page (accessible from the SourcePage property)

        // 3. Update the navigation stack, making the destination visible
        await base.ExecuteAsync (destination);

        // 4. Animate the destination page

        // 5. Reset the source page properties if necessary
    }
}
```

[Here are some concrete examples of cross-platform custom segues.](https://github.com/chkn/Xamarin.Forms.Segues/tree/master/Sample/Shared/Custom%20Segues)

### Platform Segues

The `PlatformSegue` class (currently only implemented for iOS) integrates with the low-level platform and makes it possible to segue to and from Forms `Page`s and native screens (such as `UIViewController`s on iOS). This class is used automatically when necessary for built-in segues, but you can also subclass it to create a platform-specific custom segue.

[Example of custom platform segue for iOS](https://github.com/chkn/Xamarin.Forms.Segues/blob/master/Sample/iOS/GateSegue.cs)

[Example of seguing from Page to UIViewController in XAML](https://github.com/chkn/Xamarin.Forms.Segues/blob/master/Sample/Shared/Pages/SeguePage.xaml#L14-L35)

[Example of seguing back from UIViewController in code](https://github.com/chkn/Xamarin.Forms.Segues/blob/master/Sample/iOS/NativeSegueDest.cs#L15)