import importlib
import pathlib

#
# Import BaseEffect, then populate `effects` with all the effects in this directory
#

# Get the BaseEffect class
BaseEffect = importlib.import_module('.BaseEffect', 'effects').BaseEffect

# Base names of things to ignore, either filename or classes, e.g. __init__.py or BaseEffect
_stems_to_ignore = {'__init__', 'BaseEffect'}

#
# Make a dict of {effect_name: EffectClass}
effects = {}

# Loop through the files in this directory
for filename in pathlib.Path(__file__).parent.glob('*.py'):
    stem = filename.stem
    if stem in _stems_to_ignore:
        continue

    # Import the file and look for classes
    module = importlib.import_module(f'.{stem}', 'effects')
    for attrname in vars(module):
        if attrname in _stems_to_ignore:
            continue
        attr = getattr(module, attrname)
        if isinstance(attr, type) and issubclass(attr, BaseEffect):
            # This is a class and derived from BaseEffect, keep it
            effects[attr.name] = attr
