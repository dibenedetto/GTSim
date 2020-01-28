import threading
import socket
import select
import queue
import json
import time
import base64
import io

import matplotlib.pyplot    as plt
import matplotlib.animation as animation
import numpy                as np
from   PIL                  import Image

GTSIM_DEFAULT_ADDRESS     = '127.0.0.1'
GTSIM_DEFAULT_PORT        = 8086
GTSIM_DEFAULT_BUFFER_SIZE = 256 * 1024 * 1024

class GTEnvironment():
	def __init__(self, address=GTSIM_DEFAULT_ADDRESS, port=GTSIM_DEFAULT_PORT, buffer_size=GTSIM_DEFAULT_BUFFER_SIZE):
		def send_thread(sock_comm, buffer_size, q):
			sock_quit = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			sock_quit.connect((address, port + 1))

			sock_get = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			sock_get.connect((address, port + 2))

			while True:
				rs, ws, xs = select.select([sock_get, sock_quit], [], [])

				if sock_get in rs:
					sock_get.recv(1)
					message = q.get()
					bytes   = json.dumps(message).encode()
					sock_comm.send(bytes)

				if sock_quit in rs:
					sock_quit.recv(1)
					break

			sock_quit .close()
			sock_get  .close()

		def recv_thread(sock_comm, buffer_size, q):
			sock_quit = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			sock_quit.connect((address, port + 3))

			pong = b'{"Code":"Pong", "Data":null}'

			#f = open("recv.txt", "a+")
			while True:
				rs, ws, xs = select.select([sock_comm, sock_quit], [], [])

				if sock_comm in rs:
					data   = sock_comm.recv(buffer_size)
					str    = data.decode('utf-8')
					#f.write(str + '\n\n')
					result = json.loads(str)

					if result['Code'] == 'Ping':
						sock_comm.send(pong)
						continue

					q.put(result)

				if sock_quit in rs:
					sock_quit.recv(1)
					break

			#f.close()
			sock_quit.close()

		def create_channel(address, port, info):
			def sock_accept(sock, info, port):
				info[port], addr = sock.accept()

			info[port] = None
			sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			sock.bind((address, port))
			sock.listen()
			task = threading.Thread(target=sock_accept, args=(sock, info, port,))
			task.start()
			return sock

		sock_comm = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
		sock_comm.connect((address, port + 0))

		info = { }

		sock_quit_send = create_channel(address, port + 1, info)
		sock_get       = create_channel(address, port + 2, info)
		q_send         = queue.Queue()
		send           = threading.Thread(target=send_thread, args=(sock_comm, buffer_size, q_send,))
		send.start()

		sock_quit_recv = create_channel(address, port + 3, info)
		q_recv         = queue.Queue()
		recv           = threading.Thread(target=recv_thread, args=(sock_comm, buffer_size, q_recv,))
		recv.start()

		sleep_s = 0.01
		while (info[port + 1] == None): time.sleep(sleep_s)
		while (info[port + 2] == None): time.sleep(sleep_s)
		while (info[port + 3] == None): time.sleep(sleep_s)

		self.sock_comm      = sock_comm
		self.sock_quit_send = info[port + 1]
		self.sock_get_send  = info[port + 2]
		self.q_send         = q_send
		self.send           = send
		self.sock_quit_recv = info[port + 3]
		self.q_recv         = q_recv
		self.recv           = recv
		self.open           = True
		self.image          = None
		self.fig            = None
		self.img            = None

	def _send_message(self, code, data):
		message = {
			'Code': code,
			'Data': data
		}
		self.q_send.put(message)
		self.sock_get_send.send(b'0')

	def _acquire_last_frame(self, result):
		self.image = result['Data']['NextState']['Values'][0]['Image'][0]

	def close(self):
		if not self.open:
			return False

		self._send_message('Quit', None)

		self.sock_quit_send.send(b'0')
		self.sock_quit_recv.send(b'0')

		self.send.join()
		self.recv.join()

		self.sock_comm      .close()
		self.sock_quit_send .close()
		self.sock_get_send  .close()
		self.q_send         = None
		self.send           = None
		self.sock_quit_recv .close()
		self.q_recv         = None
		self.recv           = None
		self.open           = False
		self.image          = None
		self.fig            = None
		self.img            = None

		return True

	def reset(self):
		if not self.open:
			return None
		self._send_message('Reset', None)
		message = self.q_recv.get()
		self._acquire_last_frame(message)
		return message

	def step(self, action):
		if not self.open:
			return None
		self._send_message('Step', action)
		message = self.q_recv.get()
		self._acquire_last_frame(message)
		return message

	def render(self):
		if not self.open:
			return False

		if self.fig is None:
			w = 320
			h = 240
			self.fig = plt.figure()
			self.img = plt.imshow(np.zeros((h,w)))
			plt.axis('off')
			plt.show(block=False)

		encoded = self.image
		if encoded is None:
			return True
		decoded = base64.b64decode(encoded)
		image   = Image.open(io.BytesIO(decoded))

		self.img.set_array(image)
		self.fig.canvas.draw()
		plt.pause(0.000001)

		return True
