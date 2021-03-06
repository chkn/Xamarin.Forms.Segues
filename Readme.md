# Xamarin.Forms.Segues

A library that provides support for segues between Pages.

## Segue what?

Xamarin.Forms provides navigation through the `INavigation` interface. Xamarin.Forms.Segues wraps [many navigation actions](https://github.com/chkn/Xamarin.Forms.Segues/blob/master/Xamarin.Forms.Segues/NavigationAction.cs) from `INavigation`, exposing them through the `Segue` class. Becuase `Segue` implements `ICommand`, segues can simply be assigned to anything that has a `Command` property, reducing codebehind.

## NuGet
* [Xamarin.Forms.Segues](http://www.nuget.org/packages/Xamarin.Forms.Segues) [![NuGet](https://img.shields.io/nuget/v/Xamarin.Forms.Segues.svg?label=NuGet)](https://www.nuget.org/packages/Xamarin.Forms.Segues)

## Build: 
* ![Build status](https://jamesmontemagno.visualstudio.com/_apis/public/build/definitions/6b79a378-ddd6-4e31-98ac-a12fcd68644c/21/badge)
* CI NuGet Feed: http://myget.org/F/xamarin-plugins

### Segue from XAML

Simply add this xmlns to the root element of your XAML:

```
xmlns:s="clr-namespace:Xamarin.Forms.Segues;assembly=Xamarin.Forms.Segues"
```

Then, you can add a segue to anything that has a `Command` property, such as a `Button`. The `CommandParameter` property is used to supply the type of `Page` that will be the destination of the segue. This example assumes you have a class called `NextPage` in your project:

```XML
<Button Text="Go to next page"
        Command="{s:Segue Push}"
        CommandParameter="{x:Type local:NextPage}" />
```

Note that the `{s:Segue}` markup extension creates and configures the `Segue` object. In this example, we specify a `Push` segue, but you could also specify any of the other supported [actions](https://github.com/chkn/Xamarin.Forms.Segues/blob/master/Xamarin.Forms.Segues/NavigationAction.cs).

#### Passing data

If you need to configure the page you are seguing to, you can use a `DataTemplate`. Here is the previous example, modified to set the `BindingContext` of the destination page to the `BindingContext` of the source page:

```XML
<Button Text="Go to next page with data"
        Command="{s:Segue Push}">
    <Button.CommandParameter>
        <DataTemplate>
            <local:NextPage BindingContext="{Binding}" />
        </DataTemplate>
    </Button.CommandParameter>
</Button>
```

### Segue from Code

Creating and configuring a segue from code is not much harder:

```csharp
var segue = new Segue ();

// Optionally, we can override the default segue action (Push) with the one we want
segue.Action = NavigationAction.Modal;

// Execute the segue, passing the source element that triggered it and the destination Page
await segue.ExecuteAsync (source, destination);
```

[Here is another example of creating a segue in code.](https://github.com/chkn/Xamarin.Forms.Segues/blob/master/Sample/Shared/Pages/SeguePage.xaml.cs#L16)

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

The `PlatformSegue` class (currently only implemented for iOS) integrates with the low-level platform and makes it possible to segue to and from Forms `Page`s and native screens (such as `UIViewController`s on iOS). This class is used automatically when necessary.

[Example of seguing from Page to UIViewController in XAML](https://github.com/chkn/Xamarin.Forms.Segues/blob/master/Sample/Shared/Pages/SeguePage.xaml#L14-L19)
