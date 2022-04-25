import logging

import numpy as np


class BaseEffect:
    # Base classes need to define these members:
    # description = 'A description of the effect'
    # command_name = 'The name of the command'

    def __init__(self):
        # Ensure the minimum has been defined
        assert hasattr(self, 'description'), 'Effects must define "descrption"'
        assert hasattr(self, 'command_name'), 'Effects must define "command_name"'

    #
    # Helper methods
    # TODO: Move these into a util function
    #

    @staticmethod
    def color(r_or_tuple=0.0, g=0.0, b=0.0, a=1.0):
        if isinstance(r_or_tuple, (tuple, list)):
            r, g, b, a = r_or_tuple
        elif isinstance(r_or_tuple, (int, float)) \
            and isinstance(g, (int, float)) \
            and isinstance(b, (int, float)) \
            and isinstance(a, (int, float)):
            r = r_or_tuple
        else:
            raise TypeError('BaseEffect.color must be called with RGB values or an indexable of len 3')

        assert 0.0 <= r <= 1, 'Red compoment must be between [0.0....1.0]'
        assert 0.0 <= g <= 1, 'Green compoment must be between [0.0...1.0]'
        assert 0.0 <= b <= 1, 'Blue compoment must be between [0.0...1.0]'
        assert 0.0 <= a <= 1, 'Alpha compoment must be between [0.0...1.0]'

        return np.array((r, g, b, a))
            
    @staticmethod
    def is_valid_color(color):
        return 0.0 <= color[0] <= 1.0 \
            or 0.0 <= color[1] <= 1.0 \
            or  0.0 <= color[2] <= 1.0

    #
    # Methods that can be overridden in child classes
    #

    def setup(self) -> None:
        pass

    def init(self, pixels: 'PixelMatrix') -> None:
        pass

    def update(self, time_delta: float) -> bool:
        return True
