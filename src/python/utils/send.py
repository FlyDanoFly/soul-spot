import logging
import socket
import struct


logger = logging.getLogger(__name__)

class PixelSender:
    def __init__(self, pixel_array_size):
        self.pixel_array_size = pixel_array_size
        self.struct = struct.Struct('>' + 'BBB' * pixel_array_size)
        self.connected = False
        self.socket = None
        self.num_sends = 0

    def open_connection(self, host='127.0.0.1', port=8886):
        self.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)    
        self.socket.connect((host, port))
        self.connected = True

    def send_pixels(self, pixels):
        assert len(pixels) == self.pixel_array_size
        import itertools 

        flat_color_list = (component for pixel in pixels for component in pixel.color_as_bytes())
        payload = self.struct.pack(*flat_color_list)

        logger.debug("Sending...")
        self.socket.send(payload)
        logger.debug("Receiving...")
        data = self.socket.recv(1024)
        assert len(data) == 4, f'Recieved the wrong number of bytes, expected=4 received={len(data)}'

        it_received = struct.unpack('<I', data)[0]
        if it_received != len(payload):
            logger.error(f'Mismatch in acknowledged bytes, expected={len(payload)} received={it_received}')

        logger.debug(f"Done sending pixels")
        self.num_sends += 1

    def close_connection(self):
        if not self.connected:
            raise RuntimeError('Not connected, cannot close')
        self.socket.close()
        self.connected = False

    def __del__(self):
        if not self.socket:
            return
        self.socket.close()
        self.connected = False
