﻿namespace Carousel;

/// <summary>
/// A layout that arranges children in a carousel.
/// </summary>
[ContentProperty(nameof(Children))]
public class CarouselLayout : AbsoluteLayout
{
    private bool _isLoaded = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="CarouselLayout"/> class.
    /// </summary>
    public CarouselLayout()
    {
        // is there a better place to call this?
        Loaded += (_, _) =>
        {
            _isLoaded = true;
            MeasureCarousel();
        };
        SizeChanged += (_, _) =>
        {
            MeasureCarousel();
        };

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnTapped;
        GestureRecognizers.Add(tapGesture);
    }

    /// <summary>
    /// The active item property.
    /// </summary>
    public static readonly BindableProperty ActiveItemProperty =
        BindableProperty.Create(nameof(ActiveItem), typeof(int), typeof(CarouselLayout), 
            0, BindingMode.TwoWay, propertyChanged: OnPropertyChanged);

    /// <summary>
    /// The wing start property.
    /// </summary>
    public static readonly BindableProperty WingStartProperty = 
        BindableProperty.Create(nameof(WingStart), typeof(double?), typeof(CarouselLayout),
            null, propertyChanged: OnPropertyChanged);

    /// <summary>
    /// The wing length property.
    /// </summary>
    public static readonly BindableProperty WingLengthProperty =
        BindableProperty.Create(nameof(WingLength), typeof(double), typeof(CarouselLayout),
            50d, propertyChanged: OnPropertyChanged);

    /// <summary>
    /// The take children property.
    /// </summary>
    public static readonly BindableProperty TakeChildrenProperty =
        BindableProperty.Create(nameof(TakeChildren), typeof(int), typeof(CarouselLayout), 
            2, propertyChanged: OnPropertyChanged);

    /// <summary>
    /// The inactive scale property.
    /// </summary>
    public static readonly BindableProperty InactiveScaleProperty = 
        BindableProperty.Create(nameof(InactiveScale), typeof(double), typeof(CarouselLayout), 
            0.80, propertyChanged: OnPropertyChanged);

    /// <summary>
    /// The wing scale step property.
    /// </summary>
    public static readonly BindableProperty WingScaleStepProperty =
        BindableProperty.Create(nameof(WingScaleStep), typeof(double), typeof(CarouselLayout),
            0.05, propertyChanged: OnPropertyChanged);

    /// <summary>
    /// The inactive rotation property.
    /// </summary>
    public static readonly BindableProperty InactiveRotationProperty =
        BindableProperty.Create(nameof(InactiveRotation), typeof(double), typeof(CarouselLayout),
            0d, propertyChanged: OnPropertyChanged);

    /// <summary>
    /// Gets or sets the active item in the carousel.
    /// </summary>
    public int ActiveItem
    {
        get => (int)GetValue(ActiveItemProperty);
        set => SetValue(ActiveItemProperty, value);
    }

    /// <summary>
    /// Gets or sets the distance from the center of the carousel to the start of the wing, in pixels,
    /// default is null, which means the 25% of the active element width.
    /// </summary>
    public double? WingStart
    {
        get => (double?)GetValue(WingStartProperty);
        set => SetValue(WingStartProperty, value);
    }

    /// <summary>
    /// Gets or sets the length of the wing, in pixels, default is 50.
    /// </summary>
    public double WingLength
    {
        get => (double)GetValue(WingLengthProperty);
        set => SetValue(WingLengthProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of children to take on each side of the active element, default is 2.
    /// </summary>
    public int TakeChildren
    {
        get => (int)GetValue(TakeChildrenProperty);
        set => SetValue(TakeChildrenProperty, value);
    }

    /// <summary>
    /// Gets or sets the scale of the inactive elements, default is 0.80.
    /// </summary>
    public double InactiveScale
    {
        get => (double)GetValue(InactiveScaleProperty);
        set => SetValue(InactiveScaleProperty, value);
    }

    /// <summary>
    /// Gets or sets the scale step between each element inside the wing, default is 0.05.
    /// </summary>
    public double WingScaleStep
    {
        get => (double)GetValue(WingScaleStepProperty);
        set => SetValue(WingScaleStepProperty, value);
    }

    /// <summary>
    /// Gets or sets the rotation of the inactive elements, default is 0.
    /// </summary>
    public double InactiveRotation
    {
        get => (double)GetValue(InactiveRotationProperty);
        set => SetValue(InactiveRotationProperty, value);
    }

    private void MeasureCarousel()
    {
        // prevents a crash with a message like:
        // Unable to find IAnimationManager....
        if (!_isLoaded) return;

        var activeIndex = ActiveItem;
        var takeChildren = TakeChildren;

        var activeWidth = Children[activeIndex].Width;
        if (double.IsNaN(activeWidth)) 
            throw new Exception("CarouselLayout requires all children to have a Width");

        var wingStart = WingStart ?? 0.25 * activeWidth;
        var wingLength = WingLength;

        var cx = Width * 0.5;
        var cy = Height * 0.5;

        uint duration = 800;
        var easing = Easing.CubicOut;
        var scale = InactiveScale;
        var scaleStep = WingScaleStep;
        var rotation = InactiveRotation;

        for (int i = 0; i < Children.Count; i++)
        {
            var child = (VisualElement)Children[i];

            var distance = Math.Abs(activeIndex - i);
            var wingOffset = wingLength * (distance - 1) / takeChildren;
            var wingScale = scale - (distance - 1) * scaleStep;

            if (double.IsInfinity(wingOffset) || double.IsNaN(wingOffset)) wingOffset = 0;

            if (i < activeIndex)
            {
                // we are on the left wing
                if (i < activeIndex - takeChildren)
                {
                    // the element is ignored by the TakeChildren property
                    _ = child.CarouselTransform(-cx, cy, rotation, scale, duration, easing);
                    continue;
                }

                child.ZIndex = Children.Count - distance;
                _ = child.CarouselTransform(
                    cx - wingStart - wingOffset, cy, rotation, wingScale, duration, easing);
            }

            if (i > activeIndex)
            {
                // we are on the right wing
                if (i > activeIndex + takeChildren)
                {
                    // the element is ignored by the TakeChildren property
                    _ = child.CarouselTransform(Width + cx, cy, -rotation, scale, duration, easing);
                    continue;
                }

                child.ZIndex = Children.Count - distance;
                _ = child.CarouselTransform(
                    cx + wingStart + wingOffset, cy, -rotation, wingScale, duration, easing);
            }

            if (i == activeIndex)
            {
                // this is the active element
                _ = child.CarouselTransform(cx, cy, 0, 1, duration, easing);
                child.ZIndex = Children.Count;
            }
        }
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        var p = e.GetPosition(this);
        if (p is null) return;

        var cx = Width * 0.5;
        
        var activeItem = ActiveItem;

        if (p.Value.X < cx)
        {
            // then we are on the left wing
            activeItem--;
            if (activeItem < 0) activeItem = Children.Count - 1;
        }
        else
        {
            // then we are on the right wing
            activeItem++;
            if (activeItem >= Children.Count ) activeItem = 0;
        }

        ActiveItem = activeItem;
    }

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((CarouselLayout)bindable).MeasureCarousel();
    }
}
