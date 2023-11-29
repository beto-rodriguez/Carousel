
namespace Carousel;

public static class CarouselLayoutExtensions
{
    public static Task[] CarouselTransform(
        this VisualElement visual,
        double x,
        double y,
        double rotation,
        double scale,
        uint duration,
        Easing easing)
    {
        var centeredX = x - visual.Width * 0.5;
        var centeredY = y - visual.Height * 0.5;

        return
        [
            visual.TranslateTo(centeredX, centeredY, duration, easing),
            visual.ScaleTo(scale, duration, easing),
            visual.RotateTo(rotation, duration, easing),
        ];
    }
}
