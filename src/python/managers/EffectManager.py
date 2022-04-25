import logging
import time

from effects.BaseEffect import BaseEffect
from managers.PixelMatrix import PixelMatrix

logger = logging.getLogger(__name__)

PIXELS_JSON_FILENAME = '../../unity/SOuL Spot/Assets/Exports/LEDs.json'


class EffectManager:
    def __init__(self):
        self.effects = []
        self.senders = []
        self.pixels = PixelMatrix()
        self.pixels.read_pixels_from_json(PIXELS_JSON_FILENAME)

    def add_effect(self, effect: BaseEffect):
        # TODO: figure out how to handle pixels for each effect\
        # Presumably they each get one and there's a blend method before sending
        self.effects.append(effect)

    def add_sender(self, sender):
        self.senders.append(sender)

    def start(self):
        time_frame_start = time.time()

        # Init all the effects
        for effect in self.effects:
            # Give each effect a pixel matrix, they will be blended later
            pixels = self.pixels.copy()
            effect.init(pixels)

        time_frame_end = time.time()
        time_delta = time_frame_end - time_frame_start

        while self.effects:
            time_frame_start = time.time()

            effects_to_remove = []
            for effect in self.effects:
                stop_updating = effect.update(time_delta)
                effects_to_remove.append(stop_updating)

            # TODO: find ways to merge the various pixel arrays
            self.pixels.blend([effect.pixels for effect in self.effects])

            for sender in self.senders:
                sender.send_pixels(self.pixels)

            # Remove effects that are done
            for idx in reversed(range(len(effects_to_remove))):
                remove = effects_to_remove[idx]
                if remove:
                    self.effects.pop(idx)

            time_frame_end = time.time()
            time_delta = time_frame_end - time_frame_start
