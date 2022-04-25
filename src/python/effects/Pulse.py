from enum import Enum, auto

from .BaseEffect import BaseEffect
from managers.PixelMatrix import PixelMatrix


class Effect(BaseEffect):
    description = 'Slowly ramp a color up and down'
    command_name = 'pulse'

    class Direction(Enum):
        INCREASING = auto()
        DECREASING = auto()

    def setup(self, color, speed: float = 1.0) -> None:
        self.color = self.color(color)
        if not self.is_valid_color(self.color):
            raise ValueError('Pixel values must be between 0.0 and 1.0')
        self.direction = Effect.Direction.INCREASING
        self.intensity = 0.0
        self.speed = speed

    def init(self, pixels: PixelMatrix) -> None:
        self.pixels = pixels
        for pixel in self.pixels:
            pixel.color = self.color

    def update(self, time_delta: float) -> bool:
        if self.direction is Effect.Direction.INCREASING:
            self.intensity += self.speed * time_delta
        else:
            self.intensity -= self.speed * time_delta

        if self.intensity > 1.0:
            self.direction = Effect.Direction.DECREASING
            self.intensity = 2.0 - self.intensity
        elif self.intensity < 0.0:
            self.direction = Effect.Direction.INCREASING
            self.intensity = -self.intensity

        self.color[3] = self.intensity
        # for pixel in self.pixels:
        #     pixel.color[3] = self.intensity
        self.color[3] = self.intensity
        # for pixel in self.pixels:
        #     pixel.color = (self.intensity, self.intensity, self.intensity, self.intensity)