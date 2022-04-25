from pprint import pprint
import bge
from collections import OrderedDict
import bpy

import select
import socket

import struct 


class FakeSocketListener:
    def __init__(self, fake_data=None):
        self.fake_data = fake_data

    def get_data(self):
        return self.fake_data

    def close(self):
        pass


class PixelListener:
    def __init__(self, num_pixels, port):
        self.num_pixels = num_pixels

        server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        server_socket.setblocking(0)
        server_socket.bind(('', port))
        server_socket.setblocking(0)
        server_socket.listen(5)
        server_socket.setblocking(0)
        print(f"Listening on port {port}")

        read_list = [server_socket]

        self.server_socket = server_socket
        self.read_list = read_list

    def get_data(self):
        readable, writable, errored = select.select(self.read_list, [], [], 0.0)
        # print('done with readable')
        data = b''
        for s in readable:
            # print('got a readable', s)
            if s is self.server_socket:
                # print('if s is server_socket')
                client_socket, address = self.server_socket.accept()
                self.read_list.append(client_socket)
                # print("Connection from", address)
            else:
                # print('else (i is not server_socket)')
                data = s.recv(3 * self.num_pixels)
                if data:
                    # print(f'data len={len(data)}')
                    #s.send(data)
                    s.send(struct.pack('>I', len(data)))
                else:
                    # print('not data, closing')
                    s.close()
                    self.read_list.remove(s)
        return data

    def close(self):
        self.server_socket.close()


class SunComponent(bge.types.KX_PythonComponent):
    args = OrderedDict([
        ("Listen Port", 8888),
    ])

    def start(self, args):
        print('SunComponent.start', args)

        # Get number of LEDs by looking at children
        self.num_pixels = len(self.object.children)
        
        self.port = args["Listen Port"]
        self.listener = PixelListener(self.num_pixels, self.port)
        self.struct = struct.Struct(f'>{3 * self.num_pixels}B')

    def update(self):
        data = self.listener.get_data()
        if data is None or len(data) == 0:
            return
        if len(data) != 3 * self.num_pixels:
            print(f'Bad length: expected {3*self.num_pixels} got {len(data)}')
        payload = self.struct.unpack(data)
        colors = [(payload[i] / 255, payload[i+1] / 255, payload[i+2] / 255, 1.0) for i in range(0, len(payload), 3)]
        for idx, color in enumerate(colors):
            material = bpy.data.materials[f'LED.{idx:03}']
            bsdf = material.node_tree.nodes['Principled BSDF']
            base_color = bsdf.inputs['Base Color']
            base_color.default_value = color
            base_color = bsdf.inputs['Emission']
            base_color.default_value = color

    def update_old(self):
        # print('Movement.update')
        keyboard = bge.logic.keyboard
        inputs = keyboard.inputs
        
        move = 0
        rotate = 0
    
        if inputs[bge.events.WKEY].values[-1]:
            move += self.move_speed
        if inputs[bge.events.SKEY].values[-1]:
            move -= self.move_speed
            
        if inputs[bge.events.AKEY].values[-1]:
            rotate += self.turn_speed
        if inputs[bge.events.DKEY].values[-1]:
            rotate -= self.turn_speed

        data = self.listener.get_data()
        if data:
            # print('data:', data)
            # print('data[0]:', data[0], data[0] == ord(b'r'))
            if data == b'w':
                move += self.move_speed            
            if data == b's':
                move -= self.move_speed
            if data[0] == ord(b'r'):
                val = int(data[1:]) / 255.0
                # print(val)
                self.base_color.default_value[0] = val
            if data[0] == ord(b'g'):
                val = int(data[1:]) / 255.0
                # print(val)
                self.base_color.default_value[1] = val
            if data[0] == ord(b'b'):
                val = int(data[1:]) / 255.0
                # print(val)
                self.base_color.default_value[2] = val
        self.object.applyMovement((0.0, move, 0.0), True)
        
    def dispose(self):
        print('Movement.dispose')
        self.listener.close()