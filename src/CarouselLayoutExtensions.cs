
namespace Carousel;

public static class CarouselLayoutExtensions
{
    public static Task SetPosition(
        this VisualElement visual, double x, double y, uint duration, Easing easing)
    {
        var centeredX = x - visual.Width * 0.5;
        var centeredY = y - visual.Height * 0.5;

        return visual.TranslateTo(centeredX, centeredY, duration, easing);
    }
}
