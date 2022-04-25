import importlib
import pathlib


#
# Make a dict of {command_name: EffectClass}
_stems_to_ignore = {'__init__', 'BaseEffect'}
commands = {}
for filename in pathlib.Path(__file__).parent.glob('[A-Z]*.py'):
    stem = filename.stem
    if stem in _stems_to_ignore:
        continue
    module = importlib.import_module(f'.{stem}', 'effects')
    effect_class = module.Effect
    commands[effect_class.command_name] = effect_class
