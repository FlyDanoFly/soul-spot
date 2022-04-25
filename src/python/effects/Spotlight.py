from .BaseEffect import BaseEffect
from managers.PixelMatrix import PixelMatrix

import numpy as np


# TODO: Move this into a generic utility file
def _angle_between_vectors(v0, v1):
    angle = np.arccos(np.clip(np.dot(v0, v1), -1.0, 1.0))
    return np.degrees(angle)


class Effect(BaseEffect):
    description = 'Spin a red circle around the globe horizontal plane'
    command_name = 'spotlight'

    def setup(self, color, min_angle, max_angle, speed=1.0):
        self.fg = self.color(color)
        if not self.is_valid_color(self.fg):
            raise ValueError()('Foreground color is invalid')
        self.speed = speed
        self.degrees = 0.0
        self.min_angle = min_angle
        self.max_angle = max_angle
        
    @staticmethod
    def set_spotlight_from_normal(pixels, normal, fg, min_angle, max_angle):
        for pixel in pixels:
            pixel_normal = np.array(pixel.normal)
            angle = _angle_between_vectors(normal, pixel_normal)

            c = fg
            if angle <=  min_angle:
                intensity = 1.0
            elif angle > min_angle and angle < max_angle:
                intensity = (max_angle - angle) / (max_angle - min_angle)
                # c = x * (fg - bg) + bg
            else:
                intensity = 0.0
            pixel.color = (fg[0], fg[1], fg[2], intensity)

    def init(self, pixels: PixelMatrix) -> None:
        self.pixels = pixels
        for pixel in self.pixels:
            pixel.color = self.fg

    def update(self, time_delta: float) -> bool:
        fg = self.fg
        min_angle = self.min_angle
        max_angle = self.max_angle
        self.degrees += time_delta * self.speed
        while self.degrees > 360.0:
            self.degrees -= 360.0
        rads = np.radians(self.degrees)
        normal = np.array((np.cos(rads), 0, np.sin(rads)))
        self.set_spotlight_from_normal(self.pixels, normal, fg, min_angle, max_angle)
