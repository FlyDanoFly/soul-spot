from .BaseEffect import BaseEffect
from managers.PixelMatrix import PixelMatrix


class SetColor(BaseEffect):
    name = 'set_color'
    description = 'Set all the pixels to a color'

    def setup(self, color) -> None:
        self.fg = self.color(color)
        if not self.is_valid_color(self.fg):
            raise ValueError()('Foreground color is invalid')

    def init(self, pixels: PixelMatrix) -> None:
        self.pixels = pixels
        for pixel in self.pixels:
            pixel.color = self.fg

    def update(self, time_delta: float) -> bool:
        for pixel in self.pixels:
            pixel.color = self.fg
