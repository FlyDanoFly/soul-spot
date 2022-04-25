class Pixel:
    def __init__(self, location, normal, color=(0.0,0.0,0.0,1.0)):
        self.location = location
        self.normal = normal
        self.color = color

    def color_as_bytes(self):
        return (
            int(255 * self.color[0]),
            int(255 * self.color[1]),
            int(255 * self.color[2]),
        )