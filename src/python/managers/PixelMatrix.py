from __future__ import annotations

from operator import itemgetter
import json

from primitives.Pixel import Pixel


class PixelMatrix:
    pixels: list[Pixel]

    def __init__(self, pixels: PixelMatrix = None):
        if pixels is None:
            self.pixels = []
        else:
            self.pixels = pixels

    def __len__(self):
        return len(self.pixels)

    def __getitem__(self, idx):
        return self.pixels[idx]

    def read_pixels_from_json(self, filename: str) -> None:
        pixels = []
        in_json = json.load(open(filename, 'r'))
        for normal, position in zip(in_json['normals'], in_json['positions']):
            pixels.append(Pixel(location=tuple(position), normal=tuple(normal)))
        self.pixels = pixels

    def copy(self) -> PixelMatrix:
        return PixelMatrix([
            Pixel(location=pixel.location, normal=pixel.normal, color=pixel.color)
            for pixel in self.pixels
        ])

    def blend(self, pixel_matricies: list[PixelMatrix()]) -> None:
        for idx in range(len(self.pixels)):
            colors = []
            # colors.append(self.pixels[idx].color)
            colors.append((0.0,0.0,0.0,1.0))
            for pixel_matrix in pixel_matricies:
                colors.append(pixel_matrix.pixels[idx].color)
            max_alpha = sum(map(itemgetter(3), colors))
            r = 0
            g = 0
            b = 0
            for color in colors:
                r += color[0] * color[3] / max_alpha
                g += color[1] * color[3] / max_alpha
                b += color[2] * color[3] / max_alpha
            color = (r, g, b, 1.0)
            self.pixels[idx].color = color