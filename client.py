import threading
import socket
import select
import queue
import json

GTSIM_ADDRESS     = '127.0.0.1'
GTSIM_PORT        = 8086
GTSIM_BUFFER_SIZE = 8 * 1024 * 1024

class GTEnvironment():
	def __init__(self):
		def send_thread(sock_comm, buffer_size, q):
			sock_quit = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			sock_quit.connect((GTSIM_ADDRESS, GTSIM_PORT + 1))

			sock_get = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			sock_get.connect((GTSIM_ADDRESS, GTSIM_PORT + 2))

			while True:
				rs, ws, xs = select.select([sock_get, sock_quit], [], [])

				if sock_get in rs:
					sock_get.recv(1)
					message = q.get()
					bytes   = json.dumps(message).encode()
					sock_comm.send(bytes)

				if sock_quit in rs:
					sock_quit.recv(1)
					break;

			sock_quit .close()
			sock_get  .close()

		def recv_thread(sock_comm, buffer_size, q):
			sock_quit = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			sock_quit.connect((GTSIM_ADDRESS, GTSIM_PORT + 3))

			pong = b'{"Code":"Pong", "Data":null}'

			while True:
				rs, ws, xs = select.select([sock_comm, sock_quit], [], [])

				if sock_comm in rs:
					data   = sock_comm.recv(buffer_size)
					str    = data.decode('utf-8')
					result = json.loads(str)

					if result['Code'] == 'Ping':
						sock_comm.send(pong)
						continue

					q.put(result)

				if sock_quit in rs:
					sock_quit.recv(1)
					break;

			sock_quit.close()

		def create_channel(address, port, s):
			def sock_accept(sock, s):
				s['s'] = None
				s['s'], addr = sock.accept()

			sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			sock.bind((address, port))
			sock.listen()
			task = threading.Thread(target=sock_accept, args=(sock, s,))
			task.start()
			return sock;

		sock_comm = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
		sock_comm.connect((GTSIM_ADDRESS, GTSIM_PORT + 0))

		s1 = { 's': None }
		sock_quit_send = create_channel(GTSIM_ADDRESS, GTSIM_PORT + 1, s1)
		s2 = { 's': None }
		sock_get       = create_channel(GTSIM_ADDRESS, GTSIM_PORT + 2, s2)
		q_send         = queue.Queue()
		send           = threading.Thread(target=send_thread, args=(sock_comm, GTSIM_BUFFER_SIZE, q_send,))
		send.start()
		while (s1['s'] == None): pass
		while (s2['s'] == None): pass

		s3 = { 's': None }
		sock_quit_recv = create_channel(GTSIM_ADDRESS, GTSIM_PORT + 3, s3)
		q_recv         = queue.Queue()
		recv           = threading.Thread(target=recv_thread, args=(sock_comm, GTSIM_BUFFER_SIZE, q_recv,))
		recv.start()
		while (s3['s'] == None): pass

		self.sock_comm      = sock_comm
		self.sock_quit_send = s1['s']
		self.sock_get_send  = s2['s']
		self.q_send         = q_send
		self.send           = send
		self.sock_quit_recv = s3['s']
		self.q_recv         = q_recv
		self.recv           = recv
		self.open           = True

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

		return True

	def reset(self):
		if not self.open:
			return None
		self._send_message('Reset', None)
		message = self.q_recv.get()
		return message

	def step(self, action):
		if not self.open:
			return None
		self._send_message('Step', action)
		message = self.q_recv.get()
		return message


env    = GTEnvironment()
result = env.reset()
print(result)
done   = result['Data']['Terminated']
while not done:
	action = { 'Values': [ { 'Data': [ 40.0 ] }, None, None ] }
	result = env.step(action)
	print(result)
	done   = result['Data']['Terminated']
env.close()
