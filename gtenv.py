import threading
import socket
import select
import queue
import json
import time
import base64
import io

import matplotlib.pyplot    as plt
import numpy                as np
from   PIL                  import Image

GTSIM_DEFAULT_ADDRESS     = '127.0.0.1'
GTSIM_DEFAULT_PORT        = 8086
GTSIM_DEFAULT_BUFFER_SIZE = 16 * 1024 * 1024

class GTEnvironment():
	def __init__(self, address=GTSIM_DEFAULT_ADDRESS, port=GTSIM_DEFAULT_PORT, buffer_size=GTSIM_DEFAULT_BUFFER_SIZE):
		GTSIM_LOCAL_ADDRESS = '127.0.0.1'

		def send_thread(sock_comm, buffer_size, q):
			sock_quit = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			sock_quit.connect((GTSIM_LOCAL_ADDRESS, port + 1))

			sock_get = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			sock_get.connect((GTSIM_LOCAL_ADDRESS, port + 2))

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
			sock_quit.connect((GTSIM_LOCAL_ADDRESS, port + 3))

			pong = b'{"Code":"Pong", "Data":null}'

			while True:
				rs, ws, xs = select.select([sock_comm, sock_quit], [], [])

				if sock_comm in rs:
					dstr = ''
					while True:
						data  = sock_comm.recv(buffer_size)
						dstr += data.decode('utf-8')
						try:
							result = json.loads(dstr)
						except:
							continue
						break

					if result['Code'] == 'Ping':
						sock_comm.send(pong)
						continue

					q.put(result)

				if sock_quit in rs:
					sock_quit.recv(1)
					break

			sock_quit.close()

		def create_channel(port, info):
			def sock_accept(sock, info, port):
				info[port], addr = sock.accept()

			info[port] = None
			sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			sock.bind((GTSIM_LOCAL_ADDRESS, port))
			sock.listen()
			task = threading.Thread(target=sock_accept, args=(sock, info, port,))
			task.start()
			return sock

		sock_comm = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
		sock_comm.connect((address, port + 0))

		info = { }

		sock_quit_send = create_channel(port + 1, info)
		sock_get       = create_channel(port + 2, info)
		q_send         = queue.Queue()
		send           = threading.Thread(target=send_thread, args=(sock_comm, buffer_size, q_send,))
		send.start()

		sock_quit_recv = create_channel(port + 3, info)
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
		self.frameSize      = None

		self._acquire_frame_size()

	def _acquire_frame_size(self):
		self.frameSize = [ 320, 240 ]
		expl  = self.explain()
		descs = expl['Data']['StateDescriptors']
		for i in range(len(descs)):
			desc = descs[i]
			if desc['Name'] != 'frame':
				continue
			shape = desc['Shape']
			self.frameSize[0] = shape[2]
			self.frameSize[1] = shape[1]
			break

		self.image = np.zeros((self.frame_height(), self.frame_width(), self.frame_channels()))

	def _acquire_last_frame(self, result):
		frames     = result['Data']['NextState']['Values'][0]['Image']
		frames[0]  = Image.open(io.BytesIO(base64.b64decode(frames[0])))
		self.image = frames[0]

	def _send_message(self, code, data):
		message = {
			'Code': code,
			'Data': data
		}
		self.q_send.put(message)
		self.sock_get_send.send(b'0')

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

	def explain(self):
		if not self.open:
			return None
		self._send_message('Explain', None)
		message = self.q_recv.get()
		return message

	def frame_count(self):
		return 1

	def frame_channels(self):
		return 3

	def frame_width(self):
		return self.frameSize[0]

	def frame_height(self):
		return self.frameSize[1]

	def last_frame_image(self):
		return self.image

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
			self.fig = plt.figure()
			self.img = plt.imshow(np.zeros((self.frameSize[1], self.frameSize[0])))
			plt.axis('off')
			plt.show(block=False)

		self.img.set_array(self.image)
		self.fig.canvas.draw()
		plt.pause(0.000001)

		return True
