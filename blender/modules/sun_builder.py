import json
import math
import random

import bpy


def _fibonacci_sphere(samples=1000):
    # https://stackoverflow.com/questions/9600801/evenly-distributing-n-points-on-a-sphere
    points = []
    phi = math.pi * (3. - math.sqrt(5.))  # golden angle in radians

    for i in range(samples):
        y = 1 - (i / float(samples - 1)) * 2  # y goes from 1 to -1
        radius = math.sqrt(1 - y * y)  # radius at y

        theta = phi * i  # golden angle increment

        x = math.cos(theta) * radius
        z = math.sin(theta) * radius

        points.append((x, y, z))

    return points

def create_pixel_data(num_pixels=600):
    pixels = []
    for point in _fibonacci_sphere(num_pixels): 
        pixels.append({
            'normal': point,
            'location': (5.0 * point[0], 5.0 * point[1], 5.0 * point[2])
        })
    return pixels

def read_pixel_data_json(filename, pixels):
    with open(filename, 'r') as inf:
        return json.load(pixels, inf)

def write_pixel_data_json(filename, pixels):
    with open(filename, 'w') as outf:
        json.dump(pixels, outf)

def _create_material(name, base_color=None, emission_color=None):
    material = bpy.data.materials.new(name)
    material.use_nodes = True

    bsdf = material.node_tree.nodes['Principled BSDF']
    if base_color is not None:
        bsdf.inputs['Base Color'].default_value = base_color

    if emission_color is not None:
        bsdf.inputs['Emission'].default_value = emission_color

    return material

def create_sun(pixels, center_radius=5.0, led_radius=0.15):
    """
    Create a SOuL Spot sun:
      - Create a UVSphere of the given radius as the central structure
      - Create an icosphere at each point
        - That has a unique material associated with it
        - And has the UVSphere associated as the parent
    """

    bpy.ops.mesh.primitive_uv_sphere_add(radius=center_radius)
    sun = bpy.context.active_object
    sun.name = 'Sun'
    color = (0,0,0,1)
    material = _create_material('Sun', base_color=color)
    sun.data.materials.append(material)

    for idx, pixel in enumerate(pixels):
        name = f'LED.{idx:03}'
        bpy.ops.mesh.primitive_ico_sphere_add(radius=led_radius, location=pixel['location'])
        ob = bpy.context.active_object
        ob.name = name
        ob.parent = sun
        
        random_color = (random.random(), random.random(), random.random(), 1.0)
        material = _create_material(name, base_color=random_color, emission_color=random_color)
        ob.data.materials.append(material)
